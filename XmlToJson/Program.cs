using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace XmlToJson {
    class Converter {
        private string BracketAndIndent (string value) {
            string indent = "    ";
            value = indent + Regex.Replace(value, System.Environment.NewLine, System.Environment.NewLine + indent);
            value = "{" + System.Environment.NewLine + value + System.Environment.NewLine + "}";
            return value;
        }

        private string EnumerateAttributes (XmlNode node) {
            string value = "";
            XmlAttributeCollection attributes = node.Attributes;
            if (attributes.Count > 0) {
                int valueCount = 0;
                foreach (XmlAttribute attribute in attributes) {
                    string attributeValue = "\"" + attribute.Name + "\" : \"" + attribute.Value + "\"";
                    if (++valueCount > 1) {
                        value += "," + System.Environment.NewLine;
                    }
                    value += attributeValue;
                }
            }
            if (value.Length > 0) {
                value = BracketAndIndent(value);
            }
            return value;
        }

        private string XmlNodeAsJsonText (XmlNode node) {
            string value = "";
            int valueCount = 0;

            // enumerate attributes, if any
            string attributeValue = EnumerateAttributes(node);
            if (attributeValue.Length > 0) {
                ++valueCount;
                value += "\"_attributes\" : " + attributeValue;
            }

            // enumerate children, or extract the inner value accordingly
            bool hasChildren = node.HasChildNodes && (node.ChildNodes[0].NodeType != XmlNodeType.Text);
            if (hasChildren) {
                for (int i = 0; i < node.ChildNodes.Count; i++) {
                    string childValue = XmlNodeAsJsonText(node.ChildNodes[i]);
                    if (childValue.Length > 0) {
                        if (++valueCount > 1) {
                            value += "," + System.Environment.NewLine;
                        }
                        value += childValue;
                    }
                }
            } else if (node.NodeType != XmlNodeType.Comment) {
                string innerText = node.InnerText;
                innerText = Regex.Replace(innerText, @"\n|\r", "");
                innerText = Regex.Replace(innerText, @"\t| +", " ");
                innerText = Regex.Replace(innerText, "\"", "&quot;");
                if (innerText.Length > 0) {
                    if (valueCount > 0) {
                        value += "," + System.Environment.NewLine + "\"value\" : ";
                    }
                    value += "\"" + innerText + "\"";
                }
            }

            // finish with a bracket
            if (valueCount > 0) {
                value = BracketAndIndent(value);
            }
            
            return (value.Length > 0) ? ("\"" + node.Name + "\" : " + value) : "";
        }

        public string XmlToJson (XmlDocument xmlDocument) {
            XmlNode root = xmlDocument.DocumentElement;

            // convert the xml to a json string
            string value = XmlNodeAsJsonText(root);
            value = BracketAndIndent(value);
            return value;
        }
    }

    class Program {
        static void Main (string[] args) {
            if (args.Length > 0) {
                for (int i = 0; i < args.Length; ++i) {
                    // load the xml input document
                    string inputXmlFilename = args[i];
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(inputXmlFilename);

                    // convert it to JSON
                    Converter converter = new Converter();
                    string jsonText = converter.XmlToJson(xmlDocument);

                    // save the jsonText to the output file
                    string outputJsonFilename = Path.ChangeExtension(inputXmlFilename, "json");
                    using (var outputFileStream = File.Open(outputJsonFilename, FileMode.Create, FileAccess.Write)) {
                        using (var outputStreamWriter = new StreamWriter(outputFileStream)) {
                            outputStreamWriter.Write(jsonText);
                            outputStreamWriter.Flush();
                            outputStreamWriter.Close();
                        }
                    }
                }
            } else {
                Console.WriteLine("USAGE: xmlToJson <filenames>");
            }
        }
    }
}
