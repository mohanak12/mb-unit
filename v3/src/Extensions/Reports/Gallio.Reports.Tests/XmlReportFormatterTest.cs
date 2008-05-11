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

using System.Collections.Specialized;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Runner.Reports;
using Gallio.Tests;
using MbUnit.Framework;

namespace Gallio.Reports.Tests
{
    [TestFixture]
    [TestsOn(typeof(XmlReportFormatter))]
    public class XmlReportFormatterTest : BaseUnitTest
    {
        [Test, ExpectedArgumentNullException]
        public void NameCannotBeNull()
        {
            new XmlReportFormatter(null, "description");
        }

        [Test, ExpectedArgumentNullException]
        public void DescriptionCannotBeNull()
        {
            new XmlReportFormatter("name", null);
        }

        [Test]
        public void NameIsTheSameAsWasSpecifiedInTheConstructor()
        {
            XmlReportFormatter formatter = new XmlReportFormatter("SomeName", "description");
            Assert.AreEqual("SomeName", formatter.Name);
        }

        [Test]
        public void DescriptionIsTheSameAsWasSpecifiedInTheConstructor()
        {
            XmlReportFormatter formatter = new XmlReportFormatter("SomeName", "description");
            Assert.AreEqual("description", formatter.Description);
        }

        [Test]
        public void TheDefaultAttachmentContentDispositionIsAbsent()
        {
            XmlReportFormatter formatter = new XmlReportFormatter("Xml", "description");
            Assert.AreEqual(ExecutionLogAttachmentContentDisposition.Absent, formatter.DefaultAttachmentContentDisposition);
        }

        [Test]
        public void TheDefaultAttachmentContentDispositionCanBeChanged()
        {
            XmlReportFormatter formatter = new XmlReportFormatter("Xml", "description");

            formatter.DefaultAttachmentContentDisposition = ExecutionLogAttachmentContentDisposition.Inline;
            Assert.AreEqual(ExecutionLogAttachmentContentDisposition.Inline, formatter.DefaultAttachmentContentDisposition);
        }

        [Test]
        public void FormatWritesTheReportWithTheDefaultAttachmentContentDispositionIfNoneSpecified()
        {
            IProgressMonitor progressMonitor = Mocks.Stub<IProgressMonitor>();
            IReportWriter writer = Mocks.CreateMock<IReportWriter>();

            using (Mocks.Record())
            {
                writer.SaveReport(ExecutionLogAttachmentContentDisposition.Absent, progressMonitor);
            }

            using (Mocks.Playback())
            {
                XmlReportFormatter formatter = new XmlReportFormatter("Xml", "description");
                NameValueCollection options = new NameValueCollection();

                formatter.Format(writer, options, progressMonitor);
            }
        }

        [Test]
        public void FormatWritesTheReportWithTheSpecifiedAttachmentContentDisposition()
        {
            IProgressMonitor progressMonitor = Mocks.Stub<IProgressMonitor>();
            IReportWriter writer = Mocks.CreateMock<IReportWriter>();

            using (Mocks.Record())
            {
                writer.SaveReport(ExecutionLogAttachmentContentDisposition.Link, progressMonitor);
            }

            using (Mocks.Playback())
            {
                XmlReportFormatter formatter = new XmlReportFormatter("Xml", "description");
                NameValueCollection options = new NameValueCollection();
                options.Add(XmlReportFormatter.AttachmentContentDispositionOption, ExecutionLogAttachmentContentDisposition.Link.ToString());

                formatter.Format(writer, options, progressMonitor);
            }
        }
    }
}
