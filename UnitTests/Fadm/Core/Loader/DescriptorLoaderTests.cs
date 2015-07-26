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
using System.IO;
using System.Xml;
using Fadm.Model;
using NUnit.Framework;

namespace Fadm.Core.Loader
{
    /// <summary>
    /// Tests DescriptorLoader.
    /// </summary>
    [TestFixture]
    public class ProjectTests
    {
        /// <summary>
        /// The descriptor builder.
        /// </summary>
        public DescriptorLoader DescriptorBuilder { get; private set; }

        /// <summary>
        /// Initializes <see cref="ProjectTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            this.DescriptorBuilder = new DescriptorLoader();
        }

        /// <summary>
        /// Tests Load with invalid file name.
        /// </summary>
        /// <param name="invalidFileName">An invalid file name.</param>
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("doesn't exist")]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadEmpty(string invalidFileName)
        {
            this.DescriptorBuilder.Load(invalidFileName);
        }

        /// <summary>
        /// Tests Load with invalid descriptor.
        /// </summary>
        /// <param name="fileName">The invalid descriptor.</param>
        [Test]
        [TestCase("InvalidProject.xml")]
        [TestCase("NoXml.xml")]
        [TestCase("Empty.xml")]
        [ExpectedException(typeof(XmlException))]
        public void LoadInvalid(string fileName)
        {
            this.DescriptorBuilder.Load(Path.Combine("Ressources", "Project", fileName));
        }

        /// <summary>
        /// Tests load with valid descriptor.
        /// </summary>
        /// <param name="index">Index to assert.</param>
        /// <param name="expectedName">The expected name.</param>
        /// <param name="expectedVersion">The expected version.</param>
        /// <param name="expectedCulture">The expected culture.</param>
        /// <param name="expectedArchitecture">The expected architecture.</param>
        [Test]
        [TestCase(0, "Simple.Dependency", "1.2.3.4", "", "None")]
        [TestCase(1, "Culture.Dependency", "1.2.3.4", "en-US", "None")]
        [TestCase(2, "Architecture.Dependency", "1.2.3.4", "", "MSIL")]
        [TestCase(3, "All.Dependency", "1.2.3.4", "en-US", "MSIL")]
        public void LoadDescriptor(
            int index,
            string expectedName,
            string expectedVersion,
            string expectedCulture,
            string expectedArchitecture)
        {
            // Load the descriptor
            string path = Path.Combine("Ressources", "Project", "SimpleProject.xml");
            Project project = new DescriptorLoader().Load(path);

            // Assert
            Assert.AreEqual(expectedName, project.Dependencies[index].Name);
            Assert.AreEqual(expectedVersion, project.Dependencies[index].Version.ToString());
            Assert.AreEqual(expectedCulture, project.Dependencies[index].Culture.ToString());
            Assert.AreEqual(expectedArchitecture, project.Dependencies[index].Architecture.ToString());
        }
    }
}