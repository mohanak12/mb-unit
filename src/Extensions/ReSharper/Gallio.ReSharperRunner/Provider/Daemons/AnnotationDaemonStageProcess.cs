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
using System.Collections.Generic;
using Gallio.Common.Collections;
using Gallio.Model;
using Gallio.ReSharperRunner.Provider.Daemons;
using Gallio.ReSharperRunner.Reflection;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;

#if RESHARPER_31
using JetBrains.ReSharper.Editor;
using ReSharperDocumentRange = JetBrains.ReSharper.Editor.DocumentRange;
#else
using JetBrains.DocumentModel;
using ReSharperDocumentRange = JetBrains.DocumentModel.DocumentRange;
#endif

namespace Gallio.ReSharperRunner.Provider.Daemons
{
    internal class AnnotationDaemonStageProcess : IDaemonStageProcess
    {
        private readonly IDaemonProcess process;

        public AnnotationDaemonStageProcess(IDaemonProcess process)
        {
            this.process = process;
        }

#if RESHARPER_31 || RESHARPER_40 || RESHARPER_41
        public DaemonStageProcessResult Execute()
        {
            DaemonStageProcessResult result = new DaemonStageProcessResult();
            result.FullyRehighlighted = true;
            result.Highlightings = GetHighlightings();

            return result;
        }
#else
        public void Execute(Action<DaemonStageResult> committer)
        {
            committer(new DaemonStageResult(GetHighlightings()));
        }
#endif

        private HighlightingInfo[] GetHighlightings()
        {
            IProjectFile projectFile = process.ProjectFile;
            if (! projectFile.IsValid)
                return EmptyArray<HighlightingInfo>.Instance;

            ProjectFileState state = ProjectFileState.GetFileState(projectFile);
            if (state == null)
                return EmptyArray<HighlightingInfo>.Instance;

            List<HighlightingInfo> highlightings = new List<HighlightingInfo>();

            foreach (AnnotationState annotation in state.Annotations)
            {
                IDeclaredElement declaredElement = annotation.GetDeclaredElement();
                if (declaredElement != null && declaredElement.IsValid())
                {
                    foreach (IDeclaration declaration in declaredElement.GetDeclarationsIn(projectFile))
                    {
                        if (declaration.IsValid())
                        {
                            ReSharperDocumentRange range = declaration.GetNameDocumentRange();
#if RESHARPER_31 || RESHARPER_40 || RESHARPER_41
                            if (range.IsValid)
#else
                            if (range.IsValid())
#endif
                            {
                                highlightings.Add(new HighlightingInfo(range,
                                    AnnotationHighlighting.CreateHighlighting(annotation)));
                            }
                        }
                    }
                }
            }

            return highlightings.ToArray();
        }
    }
}