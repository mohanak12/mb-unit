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
using System.Runtime.InteropServices;
using Gallio.Loader;
using Gallio.VisualStudio.Shell.Resources;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Gallio.VisualStudio.Shell.UI;

namespace Gallio.VisualStudio.Shell
{
    /// <summary>
    /// The shell package is a meta-package for all Gallio-related extensions to Visual Studio.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By itself it does nothing much except to display Gallio product information in the
    /// Visual Studio About Box.  Actual functionality is contributed by other plugins
    /// that implement <see cref="IShellExtension" />.
    /// </para>
    /// <para>
    /// The shell package exposes Visual Studio services by way of its associated <see cref="IShell" />.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, RegisterUsing = RegistrationMethod.Assembly)]
    // Note: Can't register by CodeBase because the Tip loader assumes the assembly can be resolved by name
    //       which means it has to be present in the VS PrivateAssemblies.
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0")]
    [InstalledProductRegistration(true, null, null, null)]
    [ProvideLoadKey("Standard", "3.0", "Gallio", "Gallio Project", VSPackageResourceIds.ProductLoadKeyId)]
    [Guid(Guids.ShellPkgGuidString)]
    [ComVisible(true)]
    [ProvideToolWindow(typeof(ShellToolWindowPane))]
    public sealed class ShellPackage : Package, IShellPackage
    {
        private Shell shell;

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            // Do not initialize the shell package fully when running devenv /setup.
            if (IsRunningPackageSetup)
                return;

            shell = ShellAccessor.GetShellInternal(true);
            shell.OnPackageInitialized(this);
        }

        private bool IsRunningPackageSetup
        {
            get { return GetService(typeof(SDTE)) == null; }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (shell != null)
                {
                    shell.OnPackageDisposed();
                    shell = null;
                }
            }

            base.Dispose(disposing);
        }

        int IVsInstalledProduct.IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 0;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.OfficialName(out string pbstrName)
        {
            pbstrName = VSPackage.PackageName;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductID(out string pbstrPID)
        {
            string versionLabel = Gallio.Loader.VersionPolicy.GetVersionLabel(Assembly.GetExecutingAssembly());

            pbstrPID = String.Format(VSPackage.PackageVersionFormat, versionLabel);
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = VSPackage.PackageDescription;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = VSPackageResourceIds.ProductIconId;
            return VSConstants.S_OK;
        }
    }
}
