﻿// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Gallio.Common.Reflection;
using Gallio.Model;
using Gallio.Common.Collections;

namespace Gallio.MbUnitCppAdapter.Model.PE
{
    /// <summary>
    /// Information about a PE file.
    /// </summary>
    public class PEImageInfo
    {
        private readonly ProcessorArchitecture architecture;
        private readonly IList<string> imports;
        private readonly IList<string> exports;

        /// <summary>
        /// Gets the CPU that the executable file is intended for.
        /// </summary>
        public ProcessorArchitecture Architecture
        {
            get
            {
                return architecture;
            }
        }

        public IList<string> Imports
        {
            get
            {
                return imports;
            }
        }

        public IList<string> Exports
        {
            get
            {
                return exports;
            }
        }

        public PEImageInfo(ProcessorArchitecture architecture, IEnumerable<string> imports, IEnumerable<string> exports)
        {
            this.architecture = architecture;
            this.imports = new List<string>(imports ?? EmptyArray<string>.Instance).AsReadOnly();
            this.exports = new List<string>(exports ?? EmptyArray<string>.Instance).AsReadOnly();
        }
    }
}