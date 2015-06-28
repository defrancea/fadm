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
        /// Tests Add(string) with value no target.
        /// </summary>
        [Test]
        public void AddNoTarget()
        {
            File.Copy("Ressources/NoTarget.csproj", "Ressources/NoTargetCopy.csproj", true);
            string fileName = "Ressources/NoTargetCopy.csproj";
            Assert.AreEqual(false, IsFadmAdded(fileName));
            ExecutionResult result = AddTask.Add(fileName);
            Assert.AreEqual(ExecutionResultStatus.Success, result.Status);
            Assert.AreEqual(true, IsFadmAdded(fileName));
        }

        /// <summary>
        /// Tests Add(string) with value no exec.
        /// </summary>
        [Test]
        public void AddTargetNoExec()
        {
            File.Copy("Ressources/TargetNoExec.csproj", "Ressources/TargetNoExecCopy.csproj", true);
            string fileName = "Ressources/TargetNoExecCopy.csproj";
            Assert.AreEqual(false, IsFadmAdded(fileName));
            ExecutionResult result = AddTask.Add(fileName);
            Assert.AreEqual(ExecutionResultStatus.Success, result.Status);
            Assert.AreEqual(true, IsFadmAdded(fileName));
        }

        /// <summary>
        /// Tests Install(string) with value another exec.
        /// </summary>
        [Test]
        public void AddTargetAnotherExec()
        {
            File.Copy("Ressources/TargetAnotherExec.csproj", "Ressources/TargetAnotherExecCopy.csproj", true);
            string fileName = "Ressources/TargetAnotherExecCopy.csproj";
            Assert.AreEqual(false, IsFadmAdded(fileName));
            ExecutionResult result = AddTask.Add(fileName);
            Assert.AreEqual(ExecutionResultStatus.Success, result.Status);
            Assert.AreEqual(true, IsFadmAdded(fileName));
        }

        /// <summary>
        /// Tests Install(string) with value Fadm already added.
        /// </summary>
        [Test]
        public void AddTargetAlreadyThere()
        {
            string fileName = "Ressources/AlreadyAdded.csproj";
            Assert.AreEqual(true, IsFadmAdded(fileName));
            ExecutionResult result = AddTask.Add(fileName);
            Assert.AreEqual(ExecutionResultStatus.Warning, result.Status);
            Assert.AreEqual(true, IsFadmAdded(fileName));
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
            return (null != postBuildEventNode);
        }
    }
}