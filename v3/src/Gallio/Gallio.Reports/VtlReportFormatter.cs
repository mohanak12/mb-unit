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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Gallio.Common.Xml;
using Gallio.Common.Markup;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Runtime;
using Gallio.Runner.Reports;
using Path = System.IO.Path;
using NVelocity;
using NVelocity.App;
using System.Collections;
using NVelocity.Runtime;
using Gallio.Runner.Reports.Schema;

namespace Gallio.Reports
{
    /// <summary>
    /// <para>
    /// Generic report formatter based on the NVelocity template engine.
    /// </para>
    /// <para>
    /// Recognizes the following options:
    /// <list type="bullet">
    /// <listheader>
    /// <term>Option</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>AttachmentContentDisposition</term>
    /// <description>Overrides the default attachment content disposition for the format.
    /// The content disposition may be "Absent" to exclude attachments, "Link" to
    /// include attachments by reference to external files, or "Inline" to include attachments as
    /// inline content within the formatted document.  Different formats use different
    /// default content dispositions.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public class VtlReportFormatter : BaseReportFormatter
    {
        private readonly string extension;
        private readonly string contentType;
        private readonly DirectoryInfo resourceDirectory;
        private readonly string templatePath;
        private readonly string[] resourcePaths;
        private string templateFilePath;

        /// <summary>
        /// Creates an XSLT report formatter.
        /// </summary>
        /// <param name="extension">The preferred extension without a '.'</param>
        /// <param name="contentType">The content type of the main report document.</param>
        /// <param name="resourceDirectory">The resource directory.</param>
        /// <param name="templatePath">The path of the NVelocity template relative to the resource directory.</param>
        /// <param name="resourcePaths">The paths of the resources (such as images or CSS) to copy
        /// to the report directory relative to the resource directory.</param>
        /// <exception cref="ArgumentNullException">Thrown if any arguments are null.</exception>
        public VtlReportFormatter(string extension, string contentType, DirectoryInfo resourceDirectory, string templatePath, string[] resourcePaths)
        {
            if (extension == null)
                throw new ArgumentNullException(@"extension");
            if (contentType == null)
                throw new ArgumentNullException(@"contentType");
            if (resourceDirectory == null)
                throw new ArgumentNullException(@"resourceDirectory");
            if (templatePath == null)
                throw new ArgumentNullException(@"templatePath");
            if (resourcePaths == null || Array.IndexOf(resourcePaths, null) >= 0)
                throw new ArgumentNullException(@"resourcePaths");

            this.extension = extension;
            this.contentType = contentType;
            this.resourceDirectory = resourceDirectory;
            this.templatePath = templatePath;
            this.resourcePaths = resourcePaths;
        }

        /// <inheritdoc />
        public override void Format(IReportWriter reportWriter, ReportFormatterOptions options, IProgressMonitor progressMonitor)
        {
            AttachmentContentDisposition attachmentContentDisposition = GetAttachmentContentDisposition(options);

            using (progressMonitor.BeginTask("Formatting report.", 10))
            {
                progressMonitor.SetStatus("Applying template.");
                ApplyTemplate(reportWriter, attachmentContentDisposition, options);
                progressMonitor.Worked(3);

                progressMonitor.SetStatus("Copying resources.");
                CopyResources(reportWriter);
                progressMonitor.Worked(2);

                progressMonitor.SetStatus(@"");

                if (attachmentContentDisposition == AttachmentContentDisposition.Link)
                {
                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(5))
                        reportWriter.SaveReportAttachments(subProgressMonitor);
                }
            }
        }

        private VelocityEngine CreateVelocityEngine()
        {
            var engine = new VelocityEngine();
            engine.SetProperty(RuntimeConstants_Fields.FILE_RESOURCE_LOADER_PATH, Path.GetDirectoryName(templateFilePath));
            engine.Init();
            return engine;
        }

        private VelocityContext CreateVelocityContext(Report report)
        {
            var context = new VelocityContext();
            context.Put("report", report);
            context.Put("cr", "\n");
            context.Put("newLine", Environment.NewLine);
            return context;
        }

        /// <summary>
        /// Applies the template to produce a report.
        /// </summary>
        protected virtual void ApplyTemplate(IReportWriter reportWriter, AttachmentContentDisposition attachmentContentDisposition, ReportFormatterOptions options)
        {
            templateFilePath = Path.Combine(resourceDirectory.FullName, templatePath);
            VelocityEngine engine = CreateVelocityEngine();
            VelocityContext context = CreateVelocityContext(reportWriter.Report);
            string reportPath = reportWriter.ReportContainer.ReportName + @"." + extension;
            Encoding encoding = new UTF8Encoding(false);

            using (var writer =  new StreamWriter(reportWriter.ReportContainer.OpenWrite(reportPath, contentType, encoding)))
            using (Stream template = new FileStream(templateFilePath, FileMode.Open))
            {
                engine.Evaluate(context, writer, String.Empty, template);
            }

            reportWriter.AddReportDocumentPath(reportPath);
        }

        /// <summary>
        /// Copies additional resources to the content path within the report.
        /// </summary>
        protected virtual void CopyResources(IReportWriter reportWriter)
        {
            foreach (string resourcePath in resourcePaths)
            {
                if (resourcePath.Length > 0)
                {
                    string sourceContentPath = Path.Combine(resourceDirectory.FullName, resourcePath);
                    string destContentPath = Path.Combine(reportWriter.ReportContainer.ReportName, resourcePath);
                    ReportContainerUtils.CopyToReport(reportWriter.ReportContainer, sourceContentPath, destContentPath);
                }
            }
        }
    }
}