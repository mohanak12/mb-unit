﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gallio.Collections;
using Gallio.Runtime.Extensibility;
using Gallio.Schema.Plugins;
using Gallio.Utilities;
using MbUnit.Framework;
using Rhino.Mocks;

namespace Gallio.Tests.Runtime.Extensibility
{
    [TestsOn(typeof(CachingPluginLoader))]
    public class CachingPluginLoaderTest
    {
        [Test]
        public void GetCurrentUserPluginCacheDir_Always_IsInLocalApplicationData()
        {
            Assert.StartsWith(CachingPluginLoader.GetCurrentUserPluginCacheDir(),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        [Test]
        public void ClearCurrentUserCache_IfCacheExists_RemovesCacheDirectory()
        {
            var cacheDir = CachingPluginLoader.GetCurrentUserPluginCacheDir();

            CachingPluginLoader.ClearCurrentUserPluginCache();
            Assert.IsFalse(Directory.Exists(cacheDir));
        }

        [Test]
        public void PopulateCatalog_WhenPluginXmlContainsPreprocessorInstructions_AppliesThem()
        {
            string pluginContents = "<plugin pluginId=\"pluginId\" xmlns=\"http://www.gallio.org/\"><traits><?ifdef A?><name>A</name><?endif?><?ifdef B?><property>B</property><?endif?></traits></plugin>";

            PluginLoaderTest.RunWithTemporaryPluginFile((pluginDir, pluginFile) =>
            {
                var loader = new CachingPluginLoader();
                loader.AddPluginPath(pluginFile);
                loader.DefinePreprocessorConstant("A");

                Hash64 hash = new Hash64().Add(pluginFile).Add("A");
                var cacheDir = CachingPluginLoader.GetCurrentUserPluginCacheDir();
                string cacheFilePath = Path.Combine(cacheDir, hash + ".xml");

                if (File.Exists(cacheFilePath))
                    File.Delete(cacheFilePath);

                // First pass.
                {
                    Plugin plugin = null;
                    var catalog = MockRepository.GenerateMock<IPluginCatalog>();
                    catalog.Expect(x => x.AddPlugin(null, null)).IgnoreArguments()
                        .Do((Action<Plugin, DirectoryInfo>)delegate(Plugin pluginArg, DirectoryInfo baseDirectoryArg)
                        {
                            plugin = pluginArg;
                        });

                    loader.PopulateCatalog(catalog);

                    catalog.VerifyAllExpectations(); // added one plugin

                    Assert.AreEqual(new PropertySet() { { "name", "A" } }, plugin.Traits.PropertySet);
                }

                // Check cache file.
                {
                    Assert.IsTrue(File.Exists(cacheFilePath));

                    Cache cache = Assert.XmlDeserialize<Cache>(File.ReadAllText(cacheFilePath));

                    Assert.AreEqual(1, cache.PluginInfos.Count);
                    Assert.AreEqual(pluginDir, cache.PluginInfos[0].BaseDirectory);
                    Assert.AreEqual("pluginId", cache.PluginInfos[0].Plugin.PluginId);
                    Assert.AreEqual(pluginFile, cache.PluginInfos[0].PluginFile);
                    Assert.AreEqual(File.GetLastWriteTimeUtc(pluginFile), cache.PluginInfos[0].PluginFileModificationTime);
                }

                // Second pass should restore from cache.
                {
                    Plugin plugin = null;
                    var catalog = MockRepository.GenerateMock<IPluginCatalog>();
                    catalog.Expect(x => x.AddPlugin(null, null)).IgnoreArguments()
                        .Do((Action<Plugin, DirectoryInfo>)delegate(Plugin pluginArg, DirectoryInfo baseDirectoryArg)
                        {
                            plugin = pluginArg;
                        });

                    loader.PopulateCatalog(catalog);

                    catalog.VerifyAllExpectations(); // added one plugin

                    Assert.AreEqual(new PropertySet() { { "name", "A" } }, plugin.Traits.PropertySet);
                }
            }, pluginContents);
        }
    }
}
