// MbUnit Test Framework
// 
// Copyright (c) 2004 Jonathan de Halleux
//
// This software is provided 'as-is', without any express or implied warranty. 
// 
// In no event will the authors be held liable for any damages arising from 
// the use of this software.
// Permission is granted to anyone to use this software for any purpose, 
// including commercial applications, and to alter it and redistribute it 
// freely, subject to the following restrictions:
//
//		1. The origin of this software must not be misrepresented; 
//		you must not claim that you wrote the original software. 
//		If you use this software in a product, an acknowledgment in the product 
//		documentation would be appreciated but is not required.
//
//		2. Altered source versions must be plainly marked as such, and must 
//		not be misrepresented as being the original software.
//
//		3. This notice may not be removed or altered from any source 
//		distribution.
//		
//		MbUnit HomePage: http://www.mbunit.com
//		Author: Jonathan de Halleux

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Xsl;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

using MbUnit.Core.Reports.Serialization;

namespace MbUnit.Core.Reports
{
	/// <summary>
	/// </summary>
	public abstract class XslTransformReport : ReportBase
	{
        private static XmlSerializer serializer = new XmlSerializer(typeof(ReportResult));
		private XslTransform transform = null;

		public XslTransform Transform 
		{
			get 
			{
				return this.transform;
			}
			set 
			{
				this.transform = value;
			}
		}

		protected XslTransformReport()
		{ }

        public override void Render(ReportResult result, TextWriter writer)
        {
            // create xml report and render
            XmlTextWriter output = new XmlTextWriter(writer);
            if (transform != null)
            {
                StringWriter xmlWriter = new StringWriter();
                serializer.Serialize(xmlWriter, result);
                
                // load xslt
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlWriter.ToString());
                transform.Transform(doc, new XsltArgumentList(), output);
            }
            else
            {
                serializer.Serialize(output, result);
            }
            output.Flush();
            output.Close();
        }
	}
}
