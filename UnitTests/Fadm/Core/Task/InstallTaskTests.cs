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
using Fadm.Core;
using Fadm.Core.Task;
using Fadm.Utilities;
using NUnit.Framework;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Tests InstallTask.
    /// </summary>
    [TestFixture]
    public class InstallTaskTests
    {
        /// <summary>
        /// The install task.
        /// </summary>
        public IInstallTask InstallTask { get; set; }

        /// <summary>
        /// Initializes <see cref="ExecutionResultFormatterTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            // Initialize
            InstallTask = new InstallTask();
        }

        /// <summary>
        /// Tests Install(string) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InstallNull()
        {
            InstallTask.Install(null);
        }

        /// <summary>
        /// Tests Install(string) with failure.
        /// </summary>
        /// <param name="fileName">The file name which must failed installing.</param>
        [Test]
        [TestCase("Doesn't exist.dll")]
        [TestCase("Moq.xml")]
        public void InstallError(string fileName)
        {
            ExecutionResult result = InstallTask.Install(fileName);
            Assert.AreEqual(ExecutionResultStatus.Error, result.Status);
        }

        /// <summary>
        /// Tests Install(string) with success.
        /// </summary>
        /// <param name="ressourceFile">The ressource file to install.</param>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="dependencyVersion">The dependency version.</param>
        /// <param name="dependencyExtension">The dependency extension.</param>
        /// <param name="expectedSubStatus">The expected sub result stats.</param>
        /// <param name="expectedSubMessage">The expected sub result message.</param>
        [Test]
        [TestCase("UTSample", "UTSampleDependency", "1.0.0.0", "dll", ExecutionResultStatus.Warning, "PDB not found at")]
        [TestCase("UTSample", "Test", "1.0.0.0", "exe", ExecutionResultStatus.Warning, "PDB not found at")]
        [TestCase("UTSampleWithPdb", "WithPdb", "1.0.0.0", "dll", ExecutionResultStatus.Success, "PDB installed to")]
        public void InstallSuccess(
            string ressourceFile,
            string dependencyName,
            string dependencyVersion,
            string dependencyExtension,
            ExecutionResultStatus expectedSubStatus,
            string expectedSubMessage)
        {
            // Compute dependency path
            string dependencyDirectoryPath = FileSystem.ComputeDependencyDirectoryPath(dependencyName, dependencyVersion);
            string dependencyFilePath = FileSystem.ComputeDependencyFilePath(dependencyName, dependencyVersion, dependencyExtension);
            string ressourceFilePath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", Path.Combine("Ressources", "Assembly", ressourceFile), dependencyExtension);

            // Remove the dependency path if it exists
            if (Directory.Exists(dependencyDirectoryPath))
            {
                Directory.Delete(dependencyDirectoryPath, true);
            }

            // Assert initial status
            Assert.AreEqual(true, File.Exists(ressourceFilePath));
            Assert.AreEqual(false, Directory.Exists(dependencyDirectoryPath));
            Assert.AreEqual(false, File.Exists(dependencyFilePath));

            // Trigger install
            ExecutionResult result = InstallTask.Install(ressourceFilePath);

            // Assert output
            Assert.AreEqual(ExecutionResultStatus.Success, result.Status);
            Assert.AreEqual(1, result.SubExecutionResults.Length);
            Assert.AreEqual(expectedSubStatus, result.SubExecutionResults[0].Status);
            Assert.AreEqual(true, result.SubExecutionResults[0].Message.StartsWith(expectedSubMessage));
            Assert.AreEqual(true, Directory.Exists(dependencyDirectoryPath));
            Assert.AreEqual(true, File.Exists(dependencyFilePath));
        }
    }
}