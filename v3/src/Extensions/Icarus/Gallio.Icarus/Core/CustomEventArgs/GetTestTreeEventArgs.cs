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
// Copyright 2007 MbUnit Project - http://www.mbunit.com/
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
using Gallio.Model;

namespace Gallio.Icarus.Core.CustomEventArgs 
{
    public class GetTestTreeEventArgs : EventArgs
    {
        private readonly string mode;
        private readonly bool reloadTestModelData, shadowCopyEnabled;
        private readonly TestPackageConfig testPackageConfig;

        public GetTestTreeEventArgs(string mode, bool reloadTestModelData)
        {
            this.mode = mode;
            this.reloadTestModelData = reloadTestModelData;
        }

        public GetTestTreeEventArgs(bool shadowCopyEnabled, TestPackageConfig testPackageConfig)
        {
            this.shadowCopyEnabled = shadowCopyEnabled;
            this.testPackageConfig = testPackageConfig;
        }

        public string Mode
        {
            get { return mode; }
        }

        public bool ReloadTestModelData
        {
            get { return reloadTestModelData; }
        }

        public bool ShadowCopyEnabled
        {
            get { return shadowCopyEnabled; }
        }

        public TestPackageConfig TestPackageConfig
        {
            get { return testPackageConfig; }
        }
    }
}
