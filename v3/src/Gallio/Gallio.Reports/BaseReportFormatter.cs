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
using System.Collections.Specialized;
using System.Text;
using Gallio.Model.Logging;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Runner.Reports;

namespace Gallio.Reports
{
    /// <summary>
    /// Abstract base class for report formatters.
    /// </summary>
    public abstract class BaseReportFormatter : IReportFormatter
    {
        private readonly string name;
        private readonly string description;
        private AttachmentContentDisposition defaultAttachmentContentDisposition;

        /// <summary>
        /// Gets the name of the option that how attachments are saved.
        /// </summary>
        public const string AttachmentContentDispositionOption = @"AttachmentContentDisposition";

        /// <summary>
        /// Creates a report formatter.
        /// </summary>
        /// <param name="name">The formatter name</param>
        /// <param name="description">The formatter description</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> 
        /// or <paramref name="description"/> is null</exception>
        protected BaseReportFormatter(string name, string description)
        {
            if (name == null)
                throw new ArgumentNullException(@"name");
            if (description == null)
                throw new ArgumentNullException("description");

            this.name = name;
            this.description = description;
        }

        /// <summary>
        /// Gets or sets the default attachment content disposition.
        /// Defaults to <see cref="AttachmentContentDisposition.Absent" />.
        /// </summary>
        public AttachmentContentDisposition DefaultAttachmentContentDisposition
        {
            get { return defaultAttachmentContentDisposition; }
            set { defaultAttachmentContentDisposition = value; }
        }

        /// <summary>
        /// Gets the attachment content disposition.
        /// </summary>
        /// <param name="options">The formatter options</param>
        /// <returns>The attachment content disposition</returns>
        protected AttachmentContentDisposition GetAttachmentContentDisposition(ReportFormatterOptions options)
        {
            string contentDisposition;
            if (options.Properties.TryGetValue(AttachmentContentDispositionOption, out contentDisposition))
            {
                try
                {
                    return (AttachmentContentDisposition)Enum.Parse(typeof(AttachmentContentDisposition), contentDisposition, true);
                }
                catch (ArgumentException)
                {
                    // Ignore parse error.
                }
            }

            return defaultAttachmentContentDisposition;
        }

        /// <inheritdoc />
        public string Name
        {
            get { return name; }
        }

        /// <inheritdoc />
        public string Description
        {
            get { return description; }
        }

        /// <inheritdoc />
        public abstract void Format(IReportWriter reportWriter, ReportFormatterOptions formatterOptions,
            IProgressMonitor progressMonitor);
    }
}
