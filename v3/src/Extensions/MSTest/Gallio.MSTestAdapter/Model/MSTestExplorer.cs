// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Reflection;
using System.Collections.Generic;
using Gallio.Common.Reflection;
using Gallio.Model;
using Gallio.Model.Helpers;
using Gallio.Model.Tree;
using Gallio.MSTestAdapter.Properties;

namespace Gallio.MSTestAdapter.Model
{
    /// <summary>
    /// Explores tests in MSTest assemblies.
    /// </summary>
    internal class MSTestExplorer : TestExplorer
    {
        private const string FrameworkName = "MSTest";
        private const string MSTestAssemblyDisplayName = @"Microsoft.VisualStudio.QualityTools.UnitTestFramework";

        private readonly Dictionary<Version, Test> frameworkTests;
        private readonly Dictionary<IAssemblyInfo, Test> assemblyTests;
        private readonly Dictionary<ITypeInfo, Test> typeTests;

        public MSTestExplorer()
        {
            frameworkTests = new Dictionary<Version, Test>();
            assemblyTests = new Dictionary<IAssemblyInfo, Test>();
            typeTests = new Dictionary<ITypeInfo, Test>();
        }

        protected override void ExploreImpl(IReflectionPolicy reflectionPolicy, ICodeElementInfo codeElement)
        {
            IAssemblyInfo assembly = ReflectionUtils.GetAssembly(codeElement);
            if (assembly != null)
            {
                Version frameworkVersion = GetFrameworkVersion(assembly);
                if (frameworkVersion != null)
                {
                    Test frameworkTest = GetFrameworkTest(frameworkVersion, TestModel.RootTest);
                    Test assemblyTest = GetAssemblyTest(assembly, frameworkTest, frameworkVersion, true);

                    ITypeInfo type = ReflectionUtils.GetType(codeElement);
                    if (type != null)
                    {
                        TryGetTypeTest(type, assemblyTest);
                    }
                }
            }
        }

        private static Version GetFrameworkVersion(IAssemblyInfo assembly)
        {
            AssemblyName frameworkAssemblyName = ReflectionUtils.FindAssemblyReference(assembly, MSTestAssemblyDisplayName);
            return frameworkAssemblyName != null ? frameworkAssemblyName.Version : null;
        }

        private Test GetFrameworkTest(Version frameworkVersion, Test rootTest)
        {
            Test frameworkTest;
            if (!frameworkTests.TryGetValue(frameworkVersion, out frameworkTest))
            {
                frameworkTest = CreateFrameworkTest(frameworkVersion);
                rootTest.AddChild(frameworkTest);

                frameworkTests.Add(frameworkVersion, frameworkTest);
            }

            return frameworkTest;
        }

        private static Test CreateFrameworkTest(Version frameworkVersion)
        {
            //TODO: Use resource strings
            Test frameworkTest = new Test(String.Format("MSTest v{0}", frameworkVersion), null);
            frameworkTest.LocalIdHint = FrameworkName;
            frameworkTest.Kind = TestKinds.Framework;
            frameworkTest.Metadata.Add(MetadataKeys.Framework, FrameworkName);

            return frameworkTest;
        }

        private Test GetAssemblyTest(IAssemblyInfo assembly, Test frameworkTest, Version frameworkVersion, bool populateRecursively)
        {
            Test assemblyTest;
            if (!assemblyTests.TryGetValue(assembly, out assemblyTest))
            {
                assemblyTest = CreateAssemblyTest(assembly, frameworkVersion);
                frameworkTest.AddChild(assemblyTest);
                assemblyTests.Add(assembly, assemblyTest);
            }

            if (populateRecursively)
            {
                foreach (ITypeInfo type in assembly.GetExportedTypes())
                    TryGetTypeTest(type, assemblyTest);
            }

            return assemblyTest;
        }

        private static Test CreateAssemblyTest(IAssemblyInfo assembly, Version frameworkVersion)
        {
            MSTestAssembly assemblyTest = new MSTestAssembly(assembly.Name, assembly, frameworkVersion);
            assemblyTest.Kind = TestKinds.Assembly;

            ModelUtils.PopulateMetadataFromAssembly(assembly, assemblyTest.Metadata);

            return assemblyTest;
        }

        private Test TryGetTypeTest(ITypeInfo type, Test assemblyTest)
        {
            Test typeTest;
            if (!typeTests.TryGetValue(type, out typeTest))
            {
                try
                {
                    if (IsFixture(type))
                    {
                        typeTest = CreateTypeTest(type);
                    }
                }
                catch (Exception ex)
                {
                    TestModel.AddAnnotation(new Annotation(AnnotationType.Error, type, "An exception was thrown while exploring an MSTest test type.", ex));
                }

                if (typeTest != null)
                {
                    assemblyTest.AddChild(typeTest);
                    typeTests.Add(type, typeTest);
                }
            }

            return typeTest;
        }

        private MSTest CreateTypeTest(ITypeInfo typeInfo)
        {
            MSTest typeTest = new MSTest(typeInfo.Name, typeInfo);
            typeTest.Kind = TestKinds.Fixture;

            foreach (IMethodInfo method in typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                IEnumerable<IAttributeInfo> methodAttributes = method.GetAttributeInfos(null, true);
                foreach (IAttributeInfo methodAttribute in methodAttributes)
                {
                    if (methodAttribute.Type.FullName.CompareTo(MSTestAttributes.TestMethodAttribute) == 0)
                    {
                        try
                        {
                            MSTest testMethod = CreateMethodTest(typeInfo, method);
                            typeTest.AddChild(testMethod);
                        }
                        catch (Exception ex)
                        {
                            TestModel.AddAnnotation(new Annotation(AnnotationType.Error, method, "An exception was thrown while exploring an MSTest test method.", ex));
                        }
                        break;
                    }
                }
            }

            PopulateTestClassMetadata(typeInfo, typeTest);

            // Add XML documentation.
            string xmlDocumentation = typeInfo.GetXmlDocumentation();
            if (xmlDocumentation != null)
                typeTest.Metadata.SetValue(MetadataKeys.XmlDocumentation, xmlDocumentation);

            return typeTest;
        }

        private static void PopulateTestClassMetadata(ITypeInfo typeInfo, MSTest typeTest)
        {
            IEnumerable<IAttributeInfo> attributes = typeInfo.GetAttributeInfos(null, true);
            foreach (IAttributeInfo attribute in attributes)
            {
                switch (attribute.Type.FullName)
                {
                    case MSTestAttributes.DeploymentItemAttribute:
                        typeTest.Metadata.SetValue(MSTestMetadataKeys.DeploymentItem, GetDeploymentItem(attribute));
                        break;
                    case MSTestAttributes.IgnoreAttribute:
                        typeTest.Metadata.SetValue(MetadataKeys.IgnoreReason, Resources.MSTestExplorer_IgnoreAttributeWasAppliedToClass);
                        break;
                    default:
                        break;
                }
            }
        }

        private static MSTest CreateMethodTest(ITypeInfo typeInfo, IMethodInfo methodInfo)
        {
            MSTest methodTest = new MSTest(methodInfo.Name, methodInfo);
            methodTest.Kind = TestKinds.Test;
            methodTest.IsTestCase = true;

            PopulateTestMethodMetadata(methodInfo, methodTest);

            // Add XML documentation.
            string xmlDocumentation = methodInfo.GetXmlDocumentation();
            if (xmlDocumentation != null)
                methodTest.Metadata.SetValue(MetadataKeys.XmlDocumentation, xmlDocumentation);

            return methodTest;
        }

        private static void PopulateTestMethodMetadata(IMethodInfo methodInfo, MSTest methodTest)
        {
            IEnumerable<IAttributeInfo> attributes = methodInfo.GetAttributeInfos(null, true);
            foreach (IAttributeInfo attribute in attributes)
            {
                switch (attribute.Type.FullName)
                {
                    case MSTestAttributes.AspNetDevelopmentServerAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.AspNetDevelopmentServer, GetAspNetDevelopmentServer(attribute));
                        break;
                    case MSTestAttributes.AspNetDevelopmentServerHostAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.AspNetDevelopmentServerHost, GetAspNetDevelopmentServerHost(attribute));
                        break;
                    case MSTestAttributes.CredentialAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.Credential, GetCredential(attribute));
                        break;
                    case MSTestAttributes.CssIterationAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.CssIteration, GetAttributePropertyValue(attribute, MSTestMetadataKeys.CssIteration));
                        break;
                    case MSTestAttributes.CssProjectStructureAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.CssProjectStructure, GetAttributePropertyValue(attribute, MSTestMetadataKeys.CssProjectStructure));
                        break;
                    case MSTestAttributes.DeploymentItemAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.DeploymentItem, GetDeploymentItem(attribute));
                        break;
                    case MSTestAttributes.DataSourceAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.DataSource, GetDatasource(attribute));
                        break;
                    case MSTestAttributes.DescriptionAttribute:
                        methodTest.Metadata.SetValue(MetadataKeys.Description, GetAttributePropertyValue(attribute, MetadataKeys.Description));
                        break;
                    case MSTestAttributes.HostTypeAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.HostType, GetHostType(attribute));
                        break;
                    case MSTestAttributes.IgnoreAttribute:
                        methodTest.Metadata.SetValue(MetadataKeys.IgnoreReason, Resources.MSTestExplorer_IgnoreAttributeWasAppliedToTest);
                        break;
                    case MSTestAttributes.OwnerAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.Owner, GetAttributePropertyValue(attribute, MSTestMetadataKeys.Owner));
                        break;
                    case MSTestAttributes.PriorityAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.Priority, GetAttributePropertyValue(attribute, MSTestMetadataKeys.Priority));
                        break;
                    case MSTestAttributes.TestPropertyAttribute:
                        AddTestProperty(attribute, methodTest);
                        break;
                    case MSTestAttributes.TimeoutAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.Timeout, GetAttributePropertyValue(attribute, MSTestMetadataKeys.Timeout));
                        break;
                    case MSTestAttributes.UrlToTestAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.UrlToTest, GetAttributePropertyValue(attribute, MSTestMetadataKeys.UrlToTest));
                        break;
                    case MSTestAttributes.WorkItemAttribute:
                        methodTest.Metadata.SetValue(MSTestMetadataKeys.WorkItem, GetAttributePropertyValue(attribute, "Id"));
                        break;
                    default:
                        break;
                }
            }
        }

        private static string GetAttributePropertyValue(IAttributeInfo attributeInfo, string propertyName)
        {
            Attribute attribute = (Attribute)attributeInfo.Resolve(false);
            PropertyInfo property = attributeInfo.Type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance).Resolve(false);
            object value = property.GetValue(attribute, null);
            if (value != null)
                return value.ToString();
            return null;
        }

        private static string GetAspNetDevelopmentServer(IAttributeInfo attributeInfo)
        {
            string name = GetAttributePropertyValue(attributeInfo, "Name");

            return "Name=" + name + ", " + GetAspNetDevelopmentServerHost(attributeInfo);
        }

        private static string GetAspNetDevelopmentServerHost(IAttributeInfo attributeInfo)
        {
            string pathToWebApp = GetAttributePropertyValue(attributeInfo, "PathToWebApp");
            string webAppRoot = GetAttributePropertyValue(attributeInfo, "WebAppRoot");
            string aspNetDevelopmentServerHost = "PathToWebApp=" + pathToWebApp +
                ", WebAppRoot=" + webAppRoot;

            return aspNetDevelopmentServerHost;
        }

        private static string GetCredential(IAttributeInfo attributeInfo)
        {
            string domain = GetAttributePropertyValue(attributeInfo, "Domain");
            string password = GetAttributePropertyValue(attributeInfo, "Password");
            string userName = GetAttributePropertyValue(attributeInfo, "UserName");
            string credential = "UserName=" + userName +
                ", Password=" + password;
            if (domain != null)
                credential += ", Domain=" + domain;

            return credential;
        }

        private static string GetDatasource(IAttributeInfo attributeInfo)
        {
            string connectionString = GetAttributePropertyValue(attributeInfo, "ConnectionString");
            string dataAccessMethod = GetAttributePropertyValue(attributeInfo, "DataAccessMethod");
            string dataSourceSettingName = GetAttributePropertyValue(attributeInfo, "DataSourceSettingName");
            string providerInvariantName = GetAttributePropertyValue(attributeInfo, "ProviderInvariantName");
            string tableName = GetAttributePropertyValue(attributeInfo, "TableName");

            string datasource = "ConnectionString=" + connectionString;
            if (dataAccessMethod != null)
                datasource += ", DataAccessMethod=" + dataAccessMethod;
            if (dataSourceSettingName != null)
                datasource += ", DataSourceSettingName=" + dataSourceSettingName;
            if (providerInvariantName != null)
                datasource += ", ProviderInvariantName=" + providerInvariantName;
            if (providerInvariantName != null)
                datasource += ", TableName=" + tableName;

            return datasource;
        }

        private static string GetDeploymentItem(IAttributeInfo attributeInfo)
        {
            string path = GetAttributePropertyValue(attributeInfo, "Path");
            string outputDirectory = GetAttributePropertyValue(attributeInfo, "OutputDirectory");
            return "Path=" + path + ", OutputDirectory=" + outputDirectory;
        }

        private static string GetHostType(IAttributeInfo attributeInfo)
        {
            string hostType = GetAttributePropertyValue(attributeInfo, "HostType");
            string hostData = GetAttributePropertyValue(attributeInfo, "HostData");
            return "HostType=" + hostType + ", HostData=" + hostData;
        }

        private static void AddTestProperty(IAttributeInfo attributeInfo, MSTest methodTest)
        {
            string name = GetAttributePropertyValue(attributeInfo, "Name");
            string value = GetAttributePropertyValue(attributeInfo, "Value");
            methodTest.Metadata.Add(name, value);
        }

        private static bool IsFixture(ITypeInfo type)
        {
            IEnumerable<IAttributeInfo> attributes = type.GetAttributeInfos(null, true);
            foreach (IAttributeInfo attribute in attributes)
            {
                if (attribute.Type.FullName.CompareTo(MSTestAttributes.TestClassAttribute) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}