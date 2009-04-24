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
using System.ComponentModel;
using System.IO;
using Gallio.Icarus.Mediator.Interfaces;

namespace Gallio.Icarus.Controllers
{
    public class ApplicationController : IApplicationController
    {
        private readonly IcarusArguments arguments;
        private string projectFileName = string.Empty;
        
        public IMediator Mediator { get; private set; }
        
        public string ProjectFileName
        {
            get
            {
                return string.IsNullOrEmpty(projectFileName) ? "Gallio Icarus" :
                    string.Format("{0} - Gallio Icarus", Path.GetFileNameWithoutExtension(projectFileName));
            }
            set
            {
                projectFileName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ProjectFileName"));
            }
        }

        public ApplicationController(IcarusArguments args, IMediator mediator)
        {
            arguments = args;
            Mediator = mediator;
        }

        public void Load()
        {
            var assemblyFiles = new List<string>();
            if (arguments != null && arguments.Assemblies.Length > 0)
            {
                foreach (var assembly in arguments.Assemblies)
                {
                    if (!File.Exists(assembly))
                        continue;

                    if (Path.GetExtension(assembly) == ".gallio")
                    {
                        Mediator.OpenProject(assembly);
                        break;
                    }
                    assemblyFiles.Add(assembly);
                }
                Mediator.AddAssemblies(assemblyFiles);
            }
            else if (Mediator.OptionsController.RestorePreviousSettings && Mediator.OptionsController.RecentProjects.Count > 0)
            {
                string projectName = Mediator.OptionsController.RecentProjects.Items[0];
                ProjectFileName = projectName;
                Mediator.OpenProject(projectName);
            }
        }

        public void OpenProject(string projectName)
        {
            ProjectFileName = projectName;
            Mediator.OpenProject(projectName);
        }

        public void SaveProject()
        {
            Mediator.SaveProject(projectFileName);
        }

        public void NewProject()
        {
            ProjectFileName = string.Empty;
            Mediator.NewProject();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
