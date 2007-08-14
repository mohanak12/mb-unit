// Copyright 2007 MbUnit Project - http://www.mbunit.com/
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
using System.Text;
using MbUnit.Core.Harness;
using MbUnit.Framework.Kernel.Serialization;
using MbUnit.Core.Utilities;
using MbUnit.Framework.Kernel.Events;
using MbUnit.Framework.Kernel.Model;

namespace MbUnit.Core.Runner
{
    /// <summary>
    /// Base implementation of a test domain.
    /// </summary>
    /// <remarks>
    /// The base implementation inherits from <see cref="MarshalByRefObject" />
    /// so that test domain's services can be accessed remotely if needed.
    /// </remarks>
    public abstract class BaseTestDomain : LongLivingMarshalByRefObject, ITestDomain
    {
        private bool disposed;
        private IEventListener listener;
        private TestPackage package;
        private TemplateModel templateModel;
        private TestModel testModel;

        /// <inheritdoc />
        public void Dispose()
        {
            if (!disposed)
            {
                InternalDispose();

                listener = null;
                package = null;
                templateModel = null;
                testModel = null;
                disposed = true;
            }
        }

        /// <inheritdoc />
        public TestPackage Package
        {
            get
            {
                ThrowIfDisposed();
                return package;
            }
            protected set
            {
                package = value;
            }
        }

        /// <inheritdoc />
        public TemplateModel TemplateModel
        {
            get
            {
                ThrowIfDisposed();
                return templateModel;
            }
            protected set
            {
                templateModel = value;
            }
        }

        /// <inheritdoc />
        public TestModel TestModel
        {
            get
            {
                ThrowIfDisposed();
                return testModel;
            }
            protected set
            {
                testModel = value;
            }
        }

        /// <summary>
        /// Gets the event listener, or null if none was set.
        /// </summary>
        protected IEventListener Listener
        {
            get { return listener; }
        }

        /// <inheritdoc />
        public void SetEventListener(IEventListener listener)
        {
            this.listener = listener;
        }

        /// <inheritdoc />
        public void LoadPackage(IProgressMonitor progressMonitor, TestPackage package)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");
            if (package == null)
                throw new ArgumentNullException("package");

            ThrowIfDisposed();

            using (progressMonitor)
            {
                progressMonitor.BeginTask("Loading test package.", 1.05);

                using (SubProgressMonitor unloadProgressMonitor = new SubProgressMonitor(progressMonitor, 0.05))
                {
                    unloadProgressMonitor.BeginTask("Unloading previous test package.", 1);
                    InternalUnloadPackage(progressMonitor);
                }

                this.package = package;
                InternalLoadPackage(progressMonitor, package);
            }
        }

        /// <inheritdoc />
        public void BuildTemplates(IProgressMonitor progressMonitor, TemplateEnumerationOptions options)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");
            if (options == null)
                throw new ArgumentNullException("options");

            ThrowIfDisposed();

            using (progressMonitor)
            {
                progressMonitor.BeginTask("Building test templates.", 1);
                InternalBuildTemplates(progressMonitor, options);
            }
        }

        /// <inheritdoc />
        public void BuildTests(IProgressMonitor progressMonitor, TestEnumerationOptions options)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");
            if (options == null)
                throw new ArgumentNullException("options");

            ThrowIfDisposed();

            using (progressMonitor)
            {
                progressMonitor.BeginTask("Building tests.", 1);
                InternalBuildTests(progressMonitor, options);
            }
        }

        /// <inheritdoc />
        public void UnloadPackage(IProgressMonitor progressMonitor)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");

            ThrowIfDisposed();

            using (progressMonitor)
            {
                progressMonitor.BeginTask("Unloading test package.", 1);
                InternalUnloadPackage(progressMonitor);

                package = null;
                testModel = null;
                templateModel = null;
            }
        }

        /// <inheritdoc />
        public void RunTests(IProgressMonitor progressMonitor, TestExecutionOptions options)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");
            if (options == null)
                throw new ArgumentNullException("options");

            ThrowIfDisposed();

            using (progressMonitor)
            {
                progressMonitor.BeginTask("Running tests.", 1);
                InternalRunTests(progressMonitor, options);
            }
        }

        /// <summary>
        /// Internal implementation of <see cref="Dispose" />.
        /// </summary>
        protected abstract void InternalDispose();

        /// <summary>
        /// Internal implementation of <see cref="LoadPackage" />.
        /// </summary>
        /// <param name="progressMonitor">The progress monitor with 1 work unit to do</param>
        /// <param name="package">The test package</param>
        protected abstract void InternalLoadPackage(IProgressMonitor progressMonitor, TestPackage package);

        /// <summary>
        /// Internal implementation of <see cref="BuildTemplates" />.
        /// </summary>
        /// <param name="progressMonitor">The progress monitor with 1 work unit to do</param>
        /// <param name="options">The template enumeration options</param>
        protected abstract void InternalBuildTemplates(IProgressMonitor progressMonitor, TemplateEnumerationOptions options);

        /// <summary>
        /// Internal implementation of <see cref="BuildTests" />.
        /// </summary>
        /// <param name="progressMonitor">The progress monitor with 1 work unit to do</param>
        /// <param name="options">The test enumeration options</param>
        protected abstract void InternalBuildTests(IProgressMonitor progressMonitor, TestEnumerationOptions options);

        /// <summary>
        /// Internal implementation of <see cref="RunTests" />.
        /// </summary>
        /// <param name="progressMonitor">The progress monitor with 1 work unit to do</param>
        /// <param name="options">The test execution options</param>
        protected abstract void InternalRunTests(IProgressMonitor progressMonitor, TestExecutionOptions options);

        /// <summary>
        /// Internal implementation of <see cref="UnloadPackage" />.
        /// </summary>
        /// <param name="progressMonitor">The progress monitor with 1 work unit to do</param>
        protected abstract void InternalUnloadPackage(IProgressMonitor progressMonitor);

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if the domain has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("Isolated test domain");
        }
    }
}
