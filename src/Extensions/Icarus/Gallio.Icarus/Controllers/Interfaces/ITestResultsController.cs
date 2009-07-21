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

using System.Drawing;
using System.Windows.Forms;

namespace Gallio.Icarus.Controllers.Interfaces
{
    public interface ITestResultsController
    {
        int ResultsCount { get; }
        string TestStatusBarStyle { get; }
        Color PassedColor { get; }
        Color FailedColor { get; }
        Color InconclusiveColor { get; }
        Color SkippedColor { get; }
        int PassedTestCount { get; }
        int FailedTestCount { get; }
        int SkippedTestCount { get; }
        int InconclusiveTestCount { get; }

        void CacheVirtualItems(int startIndex, int endIndex);
        ListViewItem RetrieveVirtualItem(int itemIndex);
        void SetSortColumn(int column);
    }
}
