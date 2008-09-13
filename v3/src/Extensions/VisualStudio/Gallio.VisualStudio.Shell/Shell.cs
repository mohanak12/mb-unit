// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Gallio.VisualStudio.Shell.Actions;
using Gallio.VisualStudio.Shell.Resources;
using Microsoft.VisualStudio.Shell.Interop;

namespace Gallio.VisualStudio.Shell
{
    /// <summary>
    /// Default implementation of the shell.
    /// </summary>
    public class Shell : IShell
    {
        private readonly DefaultActionManager actionManager;

        private ShellPackage package;
        private DTE2 dte;
        private AddIn addIn;
        private ShellAddInHandler addInHandler;

        public Shell()
        {
            actionManager = new DefaultActionManager(this);
        }

        /// <inheritdoc />
        public DTE2 DTE
        {
            get { return dte; }
        }

        /// <inheritdoc />
        public ShellPackage Package
        {
            get { return package; }
        }

        /// <inheritdoc />
        public AddIn AddIn
        {
            get { return addIn; }
        }

        /// <inheritdoc />
        public IActionManager ActionManager
        {
            get { return actionManager; }
        }

        /// <inheritdoc />
        public object GetVsService(Type serviceType)
        {
            IServiceProvider serviceProvider = package;
            return serviceProvider.GetService(serviceType);
        }

        /// <inheritdoc />
        public T GetVsService<T>(Type serviceInterface)
        {
            return (T)GetVsService(serviceInterface);
        }

        /// <inheritdoc />
        public void ProfferVsService(Type serviceType, Func<object> factory)
        {
            IServiceContainer container = GetVsService<IServiceContainer>(typeof(IServiceContainer));
            if (container != null)
            {
                ServiceCreatorCallback callback = delegate { return factory(); };
                container.AddService(serviceType, callback, true);
            }
        }

        internal void OnPackageInitialized(ShellPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            ConfigureShellServices(package, addInHandler);
        }

        internal void OnPackageDisposed()
        {
            ConfigureShellServices(null, addInHandler);
        }

        internal void OnAddInConnected(ShellAddInHandler addInHandler)
        {
            if (addInHandler == null)
                throw new ArgumentNullException("addInHandler");

            ConfigureShellServices(package, addInHandler);
        }

        internal void OnAddInDisconnected()
        {
            ConfigureShellServices(package, null);
        }

        internal void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus statusOption, ref object commandText)
        {
            actionManager.QueryStatus(commandName, neededText, ref statusOption, ref commandText);
        }

        internal void Exec(string commandName, vsCommandExecOption executeOption, ref object variantIn, ref object variantOut, ref bool handled)
        {
            actionManager.Exec(commandName, executeOption, ref variantIn, ref variantOut, ref handled);
        }

        private void ConfigureShellServices(ShellPackage package, ShellAddInHandler addInHandler)
        {
            if (package == null || addInHandler == null)
                DisposeShellServices();

            this.package = package;
            this.addInHandler = addInHandler;

            addIn = addInHandler != null ? addInHandler.AddIn : null;
            dte = package != null ? GetVsService<DTE2>(typeof(SDTE)) :
                addInHandler != null ? addInHandler.DTE : null;

            if (package != null && addInHandler != null)
                InitializeShellServices();
        }

        private void InitializeShellServices()
        {
            actionManager.Initialize();
        }

        private void DisposeShellServices()
        {
            actionManager.Shutdown();
        }
    }
}
