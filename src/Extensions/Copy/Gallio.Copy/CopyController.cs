// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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

using System.IO;
using System.Reflection;
using Gallio.Common.IO;
using Gallio.Common.Reflection;
using Gallio.Copy.Commands;
using Gallio.UI.DataBinding;
using Gallio.UI.ProgressMonitoring;

namespace Gallio.Copy
{
    public class CopyController : ICopyController
    {
        private readonly ITaskManager taskManager;

        public Observable<string> SourcePluginFolder { get; private set; }
        public Observable<string> TargetPluginFolder { get; private set; }

        public PluginTreeModel SourcePlugins { get; private set; }
        public PluginTreeModel TargetPlugins { get; private set; }

        public CopyController(ITaskManager taskManager, IFileSystem fileSystem)
        {
            this.taskManager = taskManager;

            SourcePluginFolder = new Observable<string>();
            TargetPluginFolder = new Observable<string>();

            SourcePlugins = new PluginTreeModel(fileSystem);
            TargetPlugins = new PluginTreeModel(fileSystem);
        }

        public void CopyPlugins()
        {
        }

        public void Load()
        {
            UpdateSourcePluginFolder(GetSourcePluginFolder());
        }

        public void Shutdown()
        {
            taskManager.ClearQueue();
        }

        public void UpdateSourcePluginFolder(string sourcePluginFolder)
        {
            SourcePluginFolder.Value = sourcePluginFolder;
            UpdatePluginFolder(SourcePlugins, sourcePluginFolder);
        }

        public void UpdateTargetPluginFolder(string targetPluginFolder)
        {
            TargetPluginFolder.Value = targetPluginFolder;
            UpdatePluginFolder(TargetPlugins, targetPluginFolder);
        }

        private void UpdatePluginFolder(PluginTreeModel pluginTreeModel, 
            string targetPluginFolder)
        {
            taskManager.QueueTask(new UpdatePluginFolderCommand(pluginTreeModel, 
                targetPluginFolder));
            taskManager.QueueTask(new ComparePluginsCommand(SourcePlugins,
                TargetPlugins));
        }

        // modified version of debug method in DefaultRuntime.
        // TODO: replace with install location
        private static string GetSourcePluginFolder()
        {
            // Find the root "src" dir.
            string initPath = AssemblyUtils.GetAssemblyLocalPath(Assembly.GetExecutingAssembly());

            string srcDir = initPath;
            while (srcDir != null && Path.GetFileName(srcDir) != @"src")
                srcDir = Path.GetDirectoryName(srcDir);

            return srcDir;
        }
    }
}
