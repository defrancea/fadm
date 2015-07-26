/*
 * Copyright (c) 2015, Fadm. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fadm.Model;
using Fadm.Utilities;
using Model.Fadm.Model.Schema;

namespace Fadm.Core.Loader
{
    /// <summary>
    /// Loads project descriptor and create a model representation.
    /// </summary>
    public class DescriptorLoader
    {
        /// <summary>
        /// The namespace prefix.
        /// </summary>
        private const string NS_PREFIX = "ns";

        /// <summary>
        /// The namespace.
        /// </summary>
        private const string NS_NAME = "urn:project-schema";

        /// <summary>
        /// The dependency xpath query.
        /// </summary>
        private const string DEPENDENCY_QUERY = "/ns:Project/ns:Dependencies/ns:Dependency";

        /// <summary>
        /// The name node.
        /// </summary>
        private const string NODE_NAME = "ns:Name";

        /// <summary>
        /// The version node.
        /// </summary>
        private const string NODE_VERSION = "ns:Version";

        /// <summary>
        /// The culture node.
        /// </summary>
        private const string NODE_CULTURE = "ns:Culture";

        /// <summary>
        /// The architecture node.
        /// </summary>
        private const string NODE_ARCHITECTURE = "ns:Architecture";

        /// <summary>
        /// Parses a file and create a new insteance of <see cref="Project"/>.
        /// </summary>
        /// <param name="fileName">The project descriptor to load.</param>
        /// <returns>The loaded descriptor.</returns>
        public Project Load(string fileName)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(fileName, "The filename must be set.");
            Validate.IsTrue(File.Exists(fileName), string.Format(CultureInfo.InvariantCulture, "Descriptor {0} not found.", fileName));

            // Initialize ressources
            XmlSchema projectSchema = LoadProjectSchema();
            XmlReaderSettings settings = BuildSettings(projectSchema);

            // Load the document
            List<Dependency> dependencyList = new List<Dependency>();
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader xml = XmlReader.Create(stream, settings))
                {
                    // Load xml document
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(xml);

                    // Initialize namespace manager
                    XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                    namespaceManager.AddNamespace(NS_PREFIX, NS_NAME);

                    // Iterate over all dependencies
                    XmlNodeList dependencyNodes = xmlDocument.SelectNodes(DEPENDENCY_QUERY, namespaceManager);
                    foreach (XmlNode dependencyNode in dependencyNodes)
                    {
                        // Retrieve xml nodes
                        XmlNode nameNode = dependencyNode.SelectSingleNode(NODE_NAME, namespaceManager);
                        XmlNode versionNode = dependencyNode.SelectSingleNode(NODE_VERSION, namespaceManager);
                        XmlNode cultureNode = dependencyNode.SelectSingleNode(NODE_CULTURE, namespaceManager);
                        XmlNode architectureNode = dependencyNode.SelectSingleNode(NODE_ARCHITECTURE, namespaceManager);

                        // Load values from xml node
                        string name = LoadString(nameNode);
                        Version version = LoadVesion(versionNode);

                        // Add to known dependencies as it if only name and version are specified
                        if (null == cultureNode && null == architectureNode)
                        {
                            dependencyList.Add(new Dependency(name, version));
                        }

                        // Otherwise build remaining data
                        else
                        {
                            // Load extra information
                            CultureInfo culture = LoadCulture(cultureNode);
                            ProcessorArchitecture architecture = LoadArchitecture(architectureNode);

                            // Add to known dependencies
                            dependencyList.Add(new Dependency(name, version, culture, architecture));
                        }
                    }
                }
            }

            // Build the project instance
            return new Project(dependencyList.ToArray());
        }

        /// <summary>
        /// Loads a node value as <see cref="string"/>.
        /// </summary>
        /// <param name="node">The node to load.</param>
        /// <returns>The loaded string.</returns>
        private string LoadString(XmlNode node)
        {
            // Input validation
            Validate.IsNotNull(node, "The xml node must not be null.");
            Validate.IsNotNullOrWhitespace(node.InnerText, "The node value must be set.");

            // Return as string
            return node.InnerText;
        }

        /// <summary>
        /// Loads a node value as <see cref="Version"/>.
        /// </summary>
        /// <param name="node">The node to load.</param>
        /// <returns>The loaded version.</returns>
        private Version LoadVesion(XmlNode node)
        {
            // Input validation
            Validate.IsNotNull(node, "The xml node must not be null.");
            Validate.IsNotNullOrWhitespace(node.InnerText, "The node value must be set.");

            // Parse and retyrn the value
            return Version.Parse(node.InnerText);
        }

        /// <summary>
        /// Loads a node value as <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="node">The node to load.</param>
        /// <returns>The loaded culture.</returns>
        private CultureInfo LoadCulture(XmlNode node)
        {
            // Look for the culture
            if (null != node && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                // Lookup from all cultures
                CultureInfo culture = CultureInfo
                    .GetCultures(CultureTypes.AllCultures)
                    .FirstOrDefault(c => c.Name == node.InnerText);

                // Return the culture if it found something
                if (null != culture)
                {
                    return culture;
                }
            }

            // Otherwise return invariant
            return CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Loads a node value as <see cref="ProcessorArchitecture"/>.
        /// </summary>
        /// <param name="node">The node to load.</param>
        /// <returns>The loaded architecture.</returns>
        private ProcessorArchitecture LoadArchitecture(XmlNode node)
        {
            // Look for the architecture
            if (null != node && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                // Parse the architecture
                ProcessorArchitecture architecture;
                if (Enum.TryParse(node.InnerText, out architecture))
                {
                    return architecture;
                }
            }

            // Otherwise return none
            return ProcessorArchitecture.None;
        }

        /// <summary>
        /// Loads the project schema.
        /// </summary>
        /// <returns>The loaded project schema from the XSD.</returns>
        private XmlSchema LoadProjectSchema()
        {
            using (StringReader xsdReader = new StringReader(Ressources.PROJECT_SCHEMA))
            {
                return XmlSchema.Read(xsdReader, null);
            }
        }

        /// <summary>
        /// Builds an xml reader settings from xml schema.
        /// </summary>
        /// <param name="schema">The xml schema.</param>
        /// <returns>Built xml reader settings.</returns>
        private XmlReaderSettings BuildSettings(XmlSchema schema)
        {
            // Create new settings
            XmlReaderSettings settings = new XmlReaderSettings();

            // Initialize settings
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(schema);
            settings.ValidationEventHandler += (sender, args) =>
            {
                if (args.Severity == XmlSeverityType.Error)
                    throw new XmlException(args.Message);
            };

            // Return settings
            return settings;
        }
    }
}