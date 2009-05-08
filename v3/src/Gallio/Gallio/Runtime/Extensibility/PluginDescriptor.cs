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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using Gallio.Common;
using Gallio.Common.Collections;
using Gallio.Common.Reflection;

namespace Gallio.Runtime.Extensibility
{
    internal sealed class PluginDescriptor : IPluginDescriptor
    {
        private static IHandlerFactory traitsHandlerFactory = new SingletonHandlerFactory();

        private readonly Registry registry;
        private readonly string pluginId;
        private readonly TypeName pluginTypeName;
        private readonly DirectoryInfo baseDirectory;
        private readonly PropertySet pluginProperties;
        private readonly PropertySet traitsProperties;
        private readonly IHandlerFactory pluginHandlerFactory;
        private readonly IResourceLocator resourceLocator;
        private readonly ReadOnlyCollection<AssemblyReference> assemblyReferences;
        private readonly ReadOnlyCollection<IPluginDescriptor> pluginDependencies;
        private readonly ReadOnlyCollection<string> probingPaths;

        private Type pluginType;
        private IHandler pluginHandler;
        private IHandler traitsHandler;
        private string disabledReason;

        public PluginDescriptor(Registry registry, PluginRegistration pluginRegistration, IList<IPluginDescriptor> completePluginDependenciesCopy)
        {
            this.registry = registry;
            pluginId = pluginRegistration.PluginId;
            pluginTypeName = pluginRegistration.PluginTypeName;
            baseDirectory = pluginRegistration.BaseDirectory;
            pluginProperties = pluginRegistration.PluginProperties.Copy().AsReadOnly();
            traitsProperties = pluginRegistration.TraitsProperties.Copy().AsReadOnly();
            pluginHandlerFactory = pluginRegistration.PluginHandlerFactory;
            resourceLocator = new FileSystemResourceLocator(baseDirectory);
            assemblyReferences = new ReadOnlyCollection<AssemblyReference>(GenericCollectionUtils.ToArray(pluginRegistration.AssemblyReferences));
            pluginDependencies = new ReadOnlyCollection<IPluginDescriptor>(completePluginDependenciesCopy);
            probingPaths = new ReadOnlyCollection<string>(GenericCollectionUtils.ToArray(pluginRegistration.ProbingPaths));
        }

        // Used by unit tests.
        internal static void RunWithInjectedTraitsHandlerFactoryMock(IHandlerFactory traitsHandlerFactory, Action action)
        {
            IHandlerFactory oldTraitsHandlerFactory = traitsHandlerFactory;
            try
            {
                PluginDescriptor.traitsHandlerFactory = traitsHandlerFactory;
                action();
            }
            finally
            {
                PluginDescriptor.traitsHandlerFactory = oldTraitsHandlerFactory;
            }
        }

        public string PluginId
        {
            get { return pluginId; }
        }

        public TypeName PluginTypeName
        {
            get { return pluginTypeName; }
        }

        public IHandlerFactory PluginHandlerFactory
        {
            get { return pluginHandlerFactory; }
        }

        public DirectoryInfo BaseDirectory
        {
            get { return baseDirectory; }
        }

        public IList<AssemblyReference> AssemblyReferences
        {
            get { return assemblyReferences; }
        }

        public PropertySet PluginProperties
        {
            get { return pluginProperties; }
        }

        public PropertySet TraitsProperties
        {
            get { return traitsProperties; }
        }

        public IResourceLocator ResourceLocator
        {
            get { return resourceLocator; }
        }

        public bool IsDisabled
        {
            get
            {
                return FirstDisabledPluginDependency != null || disabledReason != null;
            }
        }

        public string DisabledReason
        {
            get
            {
                var firstDisabledPluginDependency = FirstDisabledPluginDependency;
                if (firstDisabledPluginDependency != null)
                    return string.Format(string.Format("The plugin depends on another disabled plugin.  Reason: {0}", firstDisabledPluginDependency.DisabledReason));

                if (disabledReason == null)
                    throw new InvalidOperationException("The plugin has not been disabled.");

                return disabledReason;
            }
        }

        private IPluginDescriptor FirstDisabledPluginDependency
        {
            get { return GenericCollectionUtils.Find(pluginDependencies, p => p.IsDisabled); }
        }

        public IList<IPluginDescriptor> PluginDependencies
        {
            get { return pluginDependencies; }
        }

        public IList<string> ProbingPaths
        {
            get { return probingPaths; }
        }

        public Type ResolvePluginType()
        {
            if (pluginType == null)
            {
                try
                {
                    pluginType = pluginTypeName.Resolve();
                }
                catch (Exception ex)
                {
                    throw new RuntimeException(string.Format("Could not resolve the plugin type of plugin '{0}'.", pluginId), ex);
                }
            }

            return pluginType;
        }

        public IHandler ResolvePluginHandler()
        {
            if (pluginHandler == null)
            {
                try
                {
                    Type contractType = typeof (IPlugin);
                    Type objectType = ResolvePluginType();
                    registry.DataBox.Write(data =>
                    {
                        if (pluginHandler == null)
                            pluginHandler = pluginHandlerFactory.CreateHandler(registry.ServiceLocator, resourceLocator,
                                contractType, objectType, pluginProperties);
                    });
                }
                catch (Exception ex)
                {
                    throw new RuntimeException(string.Format("Could not resolve the plugin handler of plugin '{0}'.", pluginId), ex);
                }
            }

            return pluginHandler;
        }

        public IPlugin ResolvePlugin()
        {
            try
            {
                return (IPlugin) ResolvePluginHandler().Activate();
            }
            catch (Exception ex)
            {
                throw new RuntimeException(string.Format("Could not resolve instance of plugin '{0}'.", pluginId), ex);
            }
        }

        public IHandler ResolveTraitsHandler()
        {
            if (traitsHandler == null)
            {
                try
                {
                    Type contractType = typeof(PluginTraits);
                    Type objectType = typeof(PluginTraits);
                    registry.DataBox.Write(data =>
                    {
                        if (traitsHandler == null)
                            traitsHandler = traitsHandlerFactory.CreateHandler(registry.ServiceLocator, resourceLocator,
                                contractType, objectType, traitsProperties);
                    });
                }
                catch (Exception ex)
                {
                    throw new RuntimeException(string.Format("Could not resolve the traits handler of plugin '{0}'.", pluginId), ex);
                }
            }

            return traitsHandler;
        }

        public PluginTraits ResolveTraits()
        {
            try
            {
                return (PluginTraits)ResolveTraitsHandler().Activate();
            }
            catch (Exception ex)
            {
                throw new RuntimeException(string.Format("Could not resolve traits of plugin '{0}'.", pluginId), ex);
            }
        }

        public void Disable(string reason)
        {
            if (reason == null)
                throw new ArgumentNullException("reason");

            disabledReason = reason;
        }
    }
}
