// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan De Halleux, Jamie Cansdale
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
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Castle.Core;
using Castle.Core.Logging;
using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Configuration.Interpreters.XmlProcessor;
using Gallio.Utilities;

namespace Gallio.Hosting
{
    /// <summary>
    /// Default implementation of <see cref="IRuntime" /> based on the
    /// Castle inversion of control microkernel.
    /// </summary>
    /// <remarks>
    /// Loads plugins automatically when initialized by searching for configuration
    /// files named "*.plugin" in the plugin load directories and installing
    /// any Windsor configuration sections into the container.
    /// </remarks>
    [Singleton]
    public class WindsorRuntime : IRuntime, IContainerAccessor
    {
        private readonly IAssemblyResolverManager assemblyResolverManager;
        private readonly RuntimeSetup runtimeSetup;
        private readonly List<string> pluginDirectories;
        private readonly Dictionary<string, string> pluginPaths;

        private WindsorContainer container;

        /// <summary>
        /// Initializes the runtime.
        /// </summary>
        /// <param name="assemblyResolverManager">The assembly resolver to use</param>
        /// <param name="runtimeSetup">The runtime setup options</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyResolverManager"/> or
        /// <paramref name="runtimeSetup" /> is null</exception>
        public WindsorRuntime(IAssemblyResolverManager assemblyResolverManager, RuntimeSetup runtimeSetup)
        {
            if (assemblyResolverManager == null)
                throw new ArgumentNullException(@"assemblyResolverManager");
            if (runtimeSetup == null)
                throw new ArgumentNullException(@"runtimeSetup");

            this.assemblyResolverManager = assemblyResolverManager;
            this.runtimeSetup = runtimeSetup.Copy();

            container = new WindsorContainer();
            pluginDirectories = new List<string>();
            pluginPaths = new Dictionary<string, string>();

            ConfigureForDebugging();

            SetInstallationPath();
            SetInstallationConfiguration();

            ConfigureDefaultPluginDirectories();
            ConfigurePluginDirectoriesFromSetup();
            ConfigurePluginDirectoriesFromInstallationConfiguration();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (container != null)
            {
                container.Dispose();
                container = null;
            }
        }

        /// <inheritdoc />
        public IWindsorContainer Container
        {
            get { return container; }
        }

        /// <summary>
        /// Gets a mutable list of directories to be searched recursively for plugins configuration files.
        /// </summary>
        /// <value>
        /// Initially the list contains the directory where the runtime library is located.
        /// </value>
        public IList<string> PluginDirectories
        {
            get { return pluginDirectories; }
        }

        /// <summary>
        /// Adds a plugin directory to be searched recursively.
        /// </summary>
        /// <param name="pluginDirectory">The plugin directory to add</param>
        public void AddPluginDirectory(string pluginDirectory)
        {
            if (!pluginDirectories.Contains(pluginDirectory))
                pluginDirectories.Add(pluginDirectory);
        }

        /// <inheritdoc />
        public void Initialize(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            ThrowIfDisposed();

            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));
            container.Kernel.AddComponentInstance(@"Core.Logger", typeof(ILogger), logger);
            container.Kernel.AddComponentInstance(@"Core.Runtime", typeof(IRuntime), this);
            container.Kernel.AddComponentInstance(@"Core.AssemblyResolverManager", typeof(IAssemblyResolverManager), assemblyResolverManager);

            if (runtimeSetup.ConfigurationFilePath != null)
            {
                if (File.Exists(runtimeSetup.ConfigurationFilePath))
                    LoadConfigurationFromFile(runtimeSetup.ConfigurationFilePath);
            }
            else
            {
                if (ConfigurationManager.GetSection("castle") != null)
                    LoadConfigurationFromResource(new ConfigResource());
            }

            LoadAllPluginConfiguration();
            RunContainerInstaller();
        }

        /// <inheritdoc />
        public object Resolve(Type service)
        {
            return container.Resolve(service);
        }

        /// <inheritdoc />
        public T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        /// <inheritdoc />
        public T[] ResolveAll<T>()
        {
            return container.Kernel.ResolveServices<T>();
        }

        /// <inheritdoc />
        public RuntimeSetup GetRuntimeSetup()
        {
            return runtimeSetup.Copy();
        }

        /// <inheritdoc />
        public string MapUriToLocalPath(Uri uri)
        {
            if (uri.IsFile)
            {
                return uri.LocalPath;
            }
            else if (uri.Scheme == @"plugin")
            {
                string pluginName = uri.Host.ToLowerInvariant();
                string relativePath = uri.PathAndQuery;
                if (pluginName.Length == 0)
                    throw new InvalidOperationException(String.Format("Malformed plugin relative Uri: '{0}'.", uri));

                string pluginPath;
                if (! pluginPaths.TryGetValue(pluginName, out pluginPath))
                    throw new InvalidOperationException(String.Format("Unrecognized plugin name in Uri: '{0}'.", uri));

                if (relativePath.Length == 0 || relativePath == @"/")
                    return pluginPath;

                return Path.Combine(pluginPath, relativePath.Substring(1));
            }
            else
            {
                throw new InvalidOperationException(String.Format("Unsupported Uri scheme in Uri: '{0}'.", uri));
            }
        }

        private void ThrowIfDisposed()
        {
            if (container == null)
                throw new ObjectDisposedException("The runtime has been disposed.");
        }

        private void SetInstallationPath()
        {
            if (runtimeSetup.InstallationPath == null)
                runtimeSetup.InstallationPath = Path.GetDirectoryName(Loader.GetFriendlyAssemblyLocation(typeof(IRuntime).Assembly));
            else
                runtimeSetup.InstallationPath = Path.GetFullPath(runtimeSetup.InstallationPath);
        }

        private void SetInstallationConfiguration()
        {
            if (runtimeSetup.InstallationConfiguration == null)
                runtimeSetup.InstallationConfiguration = InstallationConfiguration.LoadFromRegistry();
        }

        private void ConfigureDefaultPluginDirectories()
        {
            AddPluginDirectory(runtimeSetup.InstallationPath);
        }

        private void ConfigurePluginDirectoriesFromSetup()
        {
            foreach (string pluginDirectory in runtimeSetup.PluginDirectories)
                AddPluginDirectory(pluginDirectory);
        }

        private void ConfigurePluginDirectoriesFromInstallationConfiguration()
        {
            if (runtimeSetup.InstallationConfiguration != null)
            {
                foreach (string pluginDirectory in runtimeSetup.InstallationConfiguration.AdditionalPluginDirectories)
                    AddPluginDirectory(pluginDirectory);
            }
        }

        private void LoadAllPluginConfiguration()
        {
            List<string> loadedPluginFilenames = new List<string>();
            
            foreach (string pluginDirectory in pluginDirectories)
            {
                DirectoryInfo pluginDirectoryInfo = new DirectoryInfo(pluginDirectory);
                if (pluginDirectoryInfo.Exists)
                {
                    foreach (FileInfo pluginConfigFile in pluginDirectoryInfo.GetFiles(@"*.plugin", SearchOption.AllDirectories))
                    {
                        // It can happen that we find two copies of the same plugin file
                        // in different directories such as during debugging when we ask
                        // the runtime to load plugins from multiple directories.  So we
                        // enforce a uniqueness constraint on plugin file names since they
                        // uniquely identify plugins.  The first plugin file found wins.
                        // Thus an ambiguity may arise if duplicate plugin files differ in content.
                        if (!loadedPluginFilenames.Contains(pluginConfigFile.Name))
                        {
                            loadedPluginFilenames.Add(pluginConfigFile.Name);

                            LoadConfigurationFromFile(pluginConfigFile.FullName);
                        }
                    }
                }
            }
        }

        private void LoadConfigurationFromFile(string configFile)
        {
            // Ensure that we can resolve any types referenced by the config file.
            string pluginPath = FileUtils.GetFullPathOfParentDirectory(configFile);
            assemblyResolverManager.AddHintDirectory(pluginPath);
            assemblyResolverManager.AddHintDirectory(Path.Combine(pluginPath, @"bin"));

            // Load the configuration.
            XmlElement rootElement;
            try
            {
                FileResource pluginManifestResource = new FileResource(configFile);
                XmlProcessor xmlProcessor = new XmlProcessor();
                rootElement = xmlProcessor.Process(pluginManifestResource) as XmlElement;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture,
                    "Unable to load or process config file '{0}'.", configFile), ex);
            }
            if (rootElement == null)
                throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture,
                    "Plugin manifest Xml file '{0}' yielded unexpected Xml node after pre-processing.", configFile));

            XmlElement castleElement = rootElement.SelectSingleNode("//castle") as XmlElement;
            if (castleElement != null)
                LoadConfigurationFromResource(new StaticContentResource(castleElement.OuterXml));

            // Register the plugin path.
            string pluginName = Path.GetFileNameWithoutExtension(configFile);
            string contentPath = Path.GetDirectoryName(Path.GetFullPath(configFile));
            pluginPaths.Add(pluginName.ToLowerInvariant(), contentPath);
        }

        private void LoadConfigurationFromResource(IResource resource)
        {
            XmlInterpreter configInterpreter = new XmlInterpreter(resource);
            configInterpreter.ProcessResource(resource, container.Kernel.ConfigurationStore);
        }

        private void RunContainerInstaller()
        {
            container.Installer.SetUp(container, container.Kernel.ConfigurationStore);
        }

        /// <summary>
        /// Configure the runtime for debugging purposes within Visual Studio.
        /// This code makes assumptions about the layout of the projects on disk that
        /// help to make debugging work "magically".  Unless a specific installation
        /// path has been set, it is overridden with the location of the Gallio project
        /// "bin" folder and the root directory of the source tree is added
        /// the list of plugin directories to ensure that plugins can be resolved.
        /// </summary>
        [Conditional("DEBUG")]
        private void ConfigureForDebugging()
        {
            // Find the root "src" dir.
            string gallioBinDir = Path.GetDirectoryName(Loader.GetAssemblyLocalPath(typeof(WindsorRuntime).Assembly));

            string srcDir = gallioBinDir;
            while (srcDir != null && Path.GetFileName(srcDir) != @"src")
                srcDir = Path.GetDirectoryName(srcDir);

            if (srcDir == null)
                return; // not found!

            // Force the installation path to be set to where the primary Gallio assemblies and Gallio.Host.exe
            // are located.
            runtimeSetup.InstallationPath = Path.Combine(srcDir, @"Gallio\Gallio\bin");

            // Add the solution folder to the list of plugin directories so that we can resolve
            // all plugins that have been compiled within the solution. 
            AddPluginDirectory(srcDir);
        }
    }
}
