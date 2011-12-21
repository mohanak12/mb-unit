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

using System;
using Gallio.Loader;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;

namespace Gallio.ReSharperRunner.Provider.Daemons
{
    /// <summary>
    /// This daemon stage adds support for displaying annotations produced by
    /// the test exploration process.
    /// </summary>
    [DaemonStage(StagesBefore=new [] { typeof(UnitTestDaemonStage)})]
    public class AnnotationDaemonStage : IDaemonStage
    {
        static AnnotationDaemonStage()
        {
            LoaderManager.InitializeAndSetupRuntimeIfNeeded();
        }

#if RESHARPER_31 || RESHARPER_40 || RESHARPER_41
        public IDaemonStageProcess CreateProcess(IDaemonProcess process)
#else
        public IDaemonStageProcess CreateProcess(IDaemonProcess process, DaemonProcessKind processKind)
#endif
        {
            return new AnnotationDaemonStageProcess(process);
        }

#if RESHARPER_60
    	public ErrorStripeRequest NeedsErrorStripe(IPsiSourceFile sourceFile)
    	{
    		return GetErrorStripe();
    	}
#endif

    	public ErrorStripeRequest NeedsErrorStripe(IProjectFile projectFile)
        {
			return GetErrorStripe();
        }

    	private static ErrorStripeRequest GetErrorStripe()
    	{
    		return ErrorStripeRequest.STRIPE_AND_ERRORS;
    	}
    }
}