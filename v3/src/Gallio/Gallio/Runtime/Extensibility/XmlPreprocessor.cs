﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Gallio.Collections;

namespace Gallio.Runtime.Extensibility
{
    /// <summary>
    /// Preprocesses XML based on the presence of processing instructions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Recognizes the following directives:
    /// </para>
    /// <list type="bullet">
    /// <item>&lt;?ifdef CONSTANT?&gt;: Begins conditional block whose contents are output only if CONSTANT is defined.</item>
    /// <item>&lt;?ifndef CONSTANT?&gt;: Begins conditional block whose contents are output only if CONSTANT is not defined.</item>
    /// <item>&lt;?else?&gt;: Begins alternative conditional block whose contents are output only if the previous ifdef/ifndef condition was not met.</item>
    /// <item>&lt;?endif?&gt;: Ends conditional block.</item>
    /// </list>
    /// </remarks>
    public class XmlPreprocessor
    {
        private readonly HashSet<string> constants;

        /// <summary>
        /// Creates an Xml preprocessor.
        /// </summary>
        public XmlPreprocessor()
        {
            constants = new HashSet<string>();
        }

        /// <summary>
        /// Defines a preprocessor constant.
        /// </summary>
        /// <param name="constant">The constant</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="constant"/>
        /// is null</exception>
        public void Define(string constant)
        {
            if (constant == null)
                throw new ArgumentNullException("constant");

            constants.Add(constant); 
        }

        /// <summary>
        /// Returns true if the specified preprocessor constant is defined.
        /// </summary>
        /// <param name="constant">The constant</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="constant"/>
        /// is null</exception>
        /// <returns>True if the constant is defined</returns>
        public bool IsDefined(string constant)
        {
            if (constant == null)
                throw new ArgumentNullException("constant");

            return constants.Contains(constant);
        }

        /// <summary>
        /// Preprocesses and copies an Xml document from a reader into a writer.
        /// </summary>
        /// <param name="xmlReader">The Xml reader</param>
        /// <param name="xmlWriter">The Xml writer</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="xmlReader"/>
        /// or <paramref name="xmlWriter"/> is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if the input Xml is malformed
        /// such as if it contains unbalanced ifdef/endif pairs.</exception>
        public void Preprocess(XmlReader xmlReader, XmlWriter xmlWriter)
        {
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");
            if (xmlWriter == null)
                throw new ArgumentNullException("xmlWriter");

            // Tracks whether a given block has been included or excluded.
            Stack<bool> blockStack = new Stack<bool>();
            blockStack.Push(true);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.ProcessingInstruction)
                {
                    switch (xmlReader.Name)
                    {
                        case "define":
                            if (blockStack.Peek())
                                Define(xmlReader.Value);
                            continue;

                        case "ifdef":
                            blockStack.Push(blockStack.Peek() && IsDefined(xmlReader.Value));
                            continue;

                        case "ifndef":
                            blockStack.Push(blockStack.Peek() && !IsDefined(xmlReader.Value));
                            continue;

                        case "else":
                            if (blockStack.Count == 1)
                                throw new InvalidOperationException(
                                    "Found <?else?> instruction without enclosing <?ifdef?> or <?ifndef?> block.");
                            blockStack.Push(!blockStack.Pop() && blockStack.Peek()); // order matters
                            continue;

                        case "endif":
                            if (blockStack.Count == 1)
                                throw new InvalidOperationException(
                                    "Found <?endif?> instruction without matching <?ifdef?> or <?ifndef?>.");
                            blockStack.Pop();
                            continue;
                    }
                }

                if (!blockStack.Peek())
                    continue;

                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        xmlWriter.WriteStartElement(xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
                        xmlWriter.WriteAttributes(xmlReader, true);
                        if (xmlReader.IsEmptyElement)
                            xmlWriter.WriteEndElement();
                        break;

                    case XmlNodeType.Text:
                        xmlWriter.WriteValue(xmlReader.Value);
                        break;

                    case XmlNodeType.CDATA:
                        xmlWriter.WriteCData(xmlReader.Value);
                        break;

                    case XmlNodeType.EntityReference:
                        xmlWriter.WriteEntityRef(xmlReader.Name);
                        break;

                    case XmlNodeType.Comment:
                        xmlWriter.WriteComment(xmlReader.Value);
                        break;

                    case XmlNodeType.DocumentType:
                        xmlWriter.WriteDocType(xmlReader.Name, xmlReader.GetAttribute("PUBLIC"),
                            xmlReader.GetAttribute("SYSTEM"), xmlReader.Value);
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        xmlWriter.WriteWhitespace(xmlReader.Value);
                        break;

                    case XmlNodeType.EndElement:
                        xmlWriter.WriteFullEndElement();
                        break;

                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        xmlWriter.WriteProcessingInstruction(xmlReader.Name, xmlReader.Value);
                        break;
                }
            }

            if (blockStack.Count != 1)
                throw new InvalidOperationException("Missing <?endif?> instruction.");

            xmlWriter.Flush();
        }
    }
}
