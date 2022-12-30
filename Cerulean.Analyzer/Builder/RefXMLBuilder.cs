using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cerulean.Analyzer
{
    public class RefXMLBuilder
    {
        public string? XmlPath { get; set; }
        private XElement? _root;

        private void EnsureLoaded()
        {
            if (_root != null || XmlPath == null)
                return;
            if (!File.Exists(XmlPath))
                File.Create(XmlPath).Close();
            var xmlRoot = XDocument.Load(XmlPath).Root;
            if (!xmlRoot.HasElements)
                xmlRoot.Add(new XElement("RefXML"));
            _root = xmlRoot.Element("RefXML");
        }

        public void AddComponent(string component, string namespacePart, IDictionary<string, string> propertyTypes)
        {
            EnsureLoaded();

            if (_root is null || Exists(component, namespacePart))
                return;

            var element = new XElement("Component");
            element.SetAttributeValue("Name", component);
            element.SetAttributeValue("Namespace", namespacePart);
            foreach (var propertyPair in propertyTypes)
            {
                var property = new XElement("Property");
                property.SetAttributeValue("Name", propertyPair.Key);
                property.SetAttributeValue("Type", propertyPair.Value);
                element.Add(property);
            }
            _root.Add(element);
            _root.Save(XmlPath!);
        }

        public bool Exists(string component, string namespacePart)
        {
            var elements = _root?.Elements("Component")
                .Where(e => e.Attribute("Name")?.Value == component &&
                            e.Attribute("Namespace")?.Value == namespacePart);
            return elements?.Any() ?? false;
        }
    }
}
