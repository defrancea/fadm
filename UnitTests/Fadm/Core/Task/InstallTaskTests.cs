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
using Fadm.Core;
using Fadm.Core.Task;
using Fadm.Utilities;
using NUnit.Framework;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Tests AddTask.
    /// </summary>
    [TestFixture]
    public class InstallTaskTests
    {
        /// <summary>
        /// The install task.
        /// </summary>
        public IInstallTask InstallTask { get; set; }

        /// <summary>
        /// The installed depdendnecy directory path.
        /// </summary>
        public string DependencyDirectoryPath { get; set; }

        /// <summary>
        /// The installed depdendnecy file path.
        /// </summary>
        public string DependencyFilePath { get; set; }

        /// <summary>
        /// Initializes <see cref="ExecutionResultFormatterTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            // Initialize
            InstallTask = new InstallTask();
            DependencyDirectoryPath = FileSystem.ComputeDependencyDirectoryPath("UTSampleDependency", "1.0.0.0");
            DependencyFilePath = FileSystem.ComputeDependencyFilePath("UTSampleDependency", "1.0.0.0", "dll");

            // Remove the dependency path if it exists
            if (Directory.Exists(DependencyDirectoryPath))
            {
                Directory.Delete(DependencyDirectoryPath, true);
            }
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
        /// Tests Install(string) with null value.
        /// </summary>
        [Test]
        public void InstallDoesntExist()
        {
            ExecutionResult result = InstallTask.Install("Doesn't exist.dll");
            Assert.AreEqual(ExecutionResultStatus.Error, result.Status);
        }

        /// <summary>
        /// Tests Install(string) with null value.
        /// </summary>
        [Test]
        public void InstallNotDll()
        {
            ExecutionResult result = InstallTask.Install("Moq.xml");
            Assert.AreEqual(ExecutionResultStatus.Error, result.Status);
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with null value.
        /// </summary>
        [Test]
        public void InstallSuccess()
        {
            Assert.AreEqual(true, File.Exists("UTSample.dll"));
            Assert.AreEqual(false, Directory.Exists(DependencyDirectoryPath));
            Assert.AreEqual(false, File.Exists(DependencyFilePath));
            ExecutionResult result = InstallTask.Install("UTSample.dll");
            Assert.AreEqual(ExecutionResultStatus.Success, result.Status);
            Assert.AreEqual(true, Directory.Exists(DependencyDirectoryPath));
            Assert.AreEqual(true, File.Exists(DependencyFilePath));
        }
    }
}