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
using Fadm.Core;
using Fadm.Core.Task;
using NUnit.Framework;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Tests AddTask.
    /// </summary>
    [TestFixture]
    public class AddTaskTests
    {
        /// <summary>
        /// The add task.
        /// </summary>
        public IAddTask AddTask { get; set; }

        /// <summary>
        /// Initializes <see cref="ExecutionResultFormatterTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            // Initialize
            AddTask = new AddTask();
        }

        /// <summary>
        /// Tests Add(string) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNull()
        {
            AddTask.Add(null);
        }

        /// <summary>
        /// Tests Add(string).
        /// </summary>
        [Test]
        [TestCase(false, ExecutionResultStatus.Success, "NoTarget.csproj")]
        [TestCase(false, ExecutionResultStatus.Success, "TargetNoExec.csproj")]
        [TestCase(false, ExecutionResultStatus.Success, "TargetAnotherExec.csproj")]
        [TestCase(true,  ExecutionResultStatus.Warning, "AlreadyAdded.csproj")]
        public void Add(bool alreadyExist, ExecutionResultStatus executionResultStatus, string fileName)
        {
            string sourceFile = Path.Combine("Ressources", "Csproj", fileName);
            string destinationFile = sourceFile.Replace(".csproj", "Copy.csproj");
            File.Copy(sourceFile, destinationFile, true);
            Assert.AreEqual(alreadyExist, IsFadmAdded(destinationFile));
            ExecutionResult result = AddTask.Add(destinationFile);
            Assert.AreEqual(executionResultStatus, result.Status);
            Assert.AreEqual(true, IsFadmAdded(destinationFile));
        }

        /// <summary>
        /// Check wheter Fadm is already added.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>True if already added, False otherwise.</returns>
        private bool IsFadmAdded(string path)
        {
            // Parse the project as a xml document
            XmlDocument document = new XmlDocument();
            document.Load(path);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("ns", @"http://schemas.microsoft.com/developer/msbuild/2003");

            // After build execution added by Fadm
            XmlNode postBuildEventNode = document.SelectSingleNode(@"//ns:Project/ns:Target[@Name='AfterBuild']/ns:Exec[@Command='Fadm install $(TargetPath)']", namespaceManager);
            XmlNode buildImportNode = document.SelectSingleNode(@"//ns:Project/ns:Import[@Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets']", namespaceManager);

            // Return true if both nodes are in the document
            return (null != postBuildEventNode && null != buildImportNode);
        }
    }
}