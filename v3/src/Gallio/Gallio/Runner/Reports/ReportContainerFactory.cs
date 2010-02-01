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
using System.Text;

namespace Gallio.Runner.Reports
{
    /// <summary>
    /// Abstract factory that provides a <see cref="IReportContainer"/> for reading or saving
    /// a report, according to the specified settings.
    /// </summary>
    public class ReportContainerFactory
    {
        private readonly string reportDirectory;
        private readonly string reportName;

        /// <summary>
        /// Constructs a factory.
        /// </summary>
        /// <param name="reportDirectory">The report directory path.</param>
        /// <param name="reportName">The report name.</param>
        public ReportContainerFactory(string reportDirectory, string reportName)
        {
            this.reportDirectory = reportDirectory;
            this.reportName = reportName;
        }

        /// <summary>
        /// Makes a report container for a saving operation.
        /// </summary>
        /// <param name="reportArchive">Indicates if the report must be packed in a compressed archive.</param>
        /// <returns>A new instance of report container.</returns>
        public IReportContainer MakeForSaving(bool reportArchive)
        {
            if (reportArchive)
            {
                return new ArchiveReportContainer(reportDirectory, reportName);
            }

            return new FileSystemReportContainer(reportDirectory, reportName);
        }

        /// <summary>
        /// Makes a report container for a reading operation.
        /// </summary>
        /// <returns>A new instance of report container.</returns>
        public IReportContainer MakeForReading()
        {
            throw new NotImplementedException();
        }
    }
}
