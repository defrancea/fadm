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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
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
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                // Define schema from xsd
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("urn:project-schema", XmlReader.Create(new StringReader(Ressources.PROJECT_SCHEMA)));

                // Load xml document and validate the content
                XDocument document = XDocument.Load(stream);
                document.Validate(schemas, (sender, evt) => { throw new XmlException(evt.Message, evt.Exception); });

                // Build and return the project
                return new Project((
                    from d in document
                        .Descendants("{urn:project-schema}Project")
                        .Descendants("{urn:project-schema}Dependencies")
                        .Descendants("{urn:project-schema}Dependency")
                    select new Dependency(
                        d.Descendants("{urn:project-schema}Name").First().Value,
                        Version.Parse(d.Descendants("{urn:project-schema}Version").First().Value),
                        ParseCulture(d.Descendants("{urn:project-schema}Culture").Select(culture => culture.Value).FirstOrDefault()),
                        ParseArchitecture(d.Descendants("{urn:project-schema}Architecture").Select(arch => arch.Value).FirstOrDefault())
                        )
                    ).ToArray());
            }
        }

        /// <summary>
        /// Parses a value to <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed culture.</returns>
        private CultureInfo ParseCulture(string value)
        {
            // Look for the culture
            if (!string.IsNullOrWhiteSpace(value))
            {
                // Lookup from all cultures
                CultureInfo culture = CultureInfo
                    .GetCultures(CultureTypes.AllCultures)
                    .FirstOrDefault(c => c.Name == value);

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
        /// Parses a value value to <see cref="ProcessorArchitecture"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed architecture.</returns>
        private ProcessorArchitecture ParseArchitecture(string value)
        {
            // Look for the architecture
            if (!string.IsNullOrWhiteSpace(value))
            {
                // Parse the architecture
                ProcessorArchitecture architecture;
                if (Enum.TryParse(value, out architecture))
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