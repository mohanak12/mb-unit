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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Gallio.MSTestAdapter.Model;
using Gallio.Runner.Caching;

namespace Gallio.MSTestAdapter.Wrapper
{
    internal class MSTestRunner2008 : MSTestRunner
    {
        public MSTestRunner2008(IDiskCache diskCache) : base(diskCache)
        {
        }

        protected override string GetVisualStudioVersion()
        {
            return "9.0";
        }

        protected override void WriteTestMetadata(XmlWriter writer, IEnumerable<MSTest> tests, string assemblyFilePath)
        {
            writer.WriteStartDocument();

            writer.WriteStartElement("TestLists", @"http://microsoft.com/schemas/VisualStudio/TeamTest/2006");

            writer.WriteStartElement("TestList");
            writer.WriteAttributeString("name", "Lists of Tests");
            writer.WriteAttributeString("id", RootTestListGuid.ToString());

            writer.WriteEndElement();

            writer.WriteStartElement("TestList");
            writer.WriteAttributeString("id", SelectedTestListGuid.ToString());
            writer.WriteAttributeString("name", SelectedTestListName);
            writer.WriteAttributeString("parentListId", RootTestListGuid.ToString());
            writer.WriteStartElement("TestLinks");

            foreach (MSTest test in tests)
            {
                if (test.IsTestCase)
                {
                    writer.WriteStartElement("TestLink");
                    writer.WriteAttributeString("id", test.Guid);
                    writer.WriteAttributeString("name", test.TestName);
                    writer.WriteAttributeString("storage", assemblyFilePath);
                    writer.WriteAttributeString("type",
                        "Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestElement, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.ObjectModel, PublicKeyToken=b03f5f7f11d50a3a");
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndDocument();
        }

        protected override void WriteRunConfig(XmlWriter writer)
        {
            /*
                <?xml version="1.0" encoding="UTF-8"?>
                <TestRunConfiguration name="Gallio Test Run" id="94d309d9-02ec-4f2a-978b-bb07dab7ab0f" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2006">
                  <Deployment enabled="false" />
                  <Description>This is a test run configuration used by Gallio to launch MSTest tests locally.</Description>
                  <TestTypeSpecific />
                </TestRunConfiguration>
             */

            writer.WriteStartDocument();

            writer.WriteStartElement("TestRunConfiguration", @"http://microsoft.com/schemas/VisualStudio/TeamTest/2006");
            writer.WriteAttributeString("name", "Gallio Test Run");
            writer.WriteAttributeString("id", "94d309d9-02ec-4f2a-978b-bb07dab7ab0f");

            writer.WriteElementString("Description", "This is a test run configuration used by Gallio to launch MSTest tests locally.");

            writer.WriteStartElement("Deployment");
            writer.WriteAttributeString("enabled", "false");
            writer.WriteEndElement();

            writer.WriteStartElement("TestTypeSpecific");
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndDocument();
        }
    }
}
