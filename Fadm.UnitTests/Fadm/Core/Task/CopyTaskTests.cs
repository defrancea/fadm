﻿/*
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
using Fadm.Core;
using Fadm.Core.FadmTask;
using Fadm.Utilities;
using NUnit.Framework;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Tests CopyTask.
    /// </summary>
    [TestFixture]
    public class CopyTaskTests
    {
        /// <summary>
        /// Tests Copy(string) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CopyNull()
        {
            new CopyTask(null);
        }

        /// <summary>
        /// Tests Copy(string) with failure.
        /// </summary>
        /// <param name="fileName">The file name which must failed copying.</param>
        [Test]
        [TestCase("Doesn't exist.dll")]
        [TestCase("Moq.xml")]
        public void CopyError(string fileName)
        {
            ExecutionResult result = new CopyTask(fileName).ExecuteAsync().Result;
            Assert.AreEqual(ExecutionResultStatus.Error, result.Status);
        }

        /// <summary>
        /// Tests Copy(string) with success.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="dependencyVersion">The dependency version.</param>
        /// <param name="dependencyExtension">The dependency extension.</param>
        /// <param name="descriptorPath">The descriptor path.</param>
        /// <param name="descriptorPath">The descriptor name.</param>
        /// <param name="index">The index to assert.</param>
        [Test]
        [TestCase("WithPdb", "1.0.0.0", "dll", "Copy", "", 0)]
        [TestCase("WithPdb", "1.0.0.0", "dll", "Copy", "foo.csproj", 0)]
        [TestCase("UTSampleDependency", "1.0.0.0", "dll", "Copy", "", 1)]
        [TestCase("UTSampleDependency", "1.0.0.0", "dll", "Copy", "foo.csproj", 1)]
        public void CopySuccess(
            string dependencyName,
            string dependencyVersion,
            string dependencyExtension,
            string descriptorPath,
            string descriptorName,
            int index)
        {
            // Compute dependency path
            string dependencyFilePath = FileSystem.ComputeDependencyFilePath(dependencyName, dependencyVersion, dependencyExtension);
            string fileName = FileSystem.ComputeFileName(dependencyName, dependencyVersion, dependencyExtension);
            string descriptorLocation = Path.Combine("Ressources", descriptorPath, descriptorName);
            string copiedDependencyLocation = Path.Combine("Ressources", descriptorPath, "dependency");

            // Remove the dependency path if it exists
            if (Directory.Exists(copiedDependencyLocation))
            {
                Directory.Delete(copiedDependencyLocation, true);
            }

            // Trigger install
            ExecutionResult install1result = new InstallTask(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", Path.Combine("Ressources", "Assembly", "UTSampleWithPdb"), "dll")).ExecuteAsync().Result;
            ExecutionResult install2result = new InstallTask(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", Path.Combine("Ressources", "Assembly", "UTSample"), "dll")).ExecuteAsync().Result;

            // Trigger copy
            ExecutionResult resultCopy = new CopyTask(descriptorLocation).ExecuteAsync().Result;
            Assert.AreEqual(ExecutionResultStatus.Success, resultCopy.Status);

            // Assert output
            List<ExecutionResult> executionResults = resultCopy.BlockingSubExecutionResults.ToList();
            Assert.AreEqual(true, Directory.Exists(copiedDependencyLocation));
            Assert.AreEqual(true, File.Exists(Path.Combine(copiedDependencyLocation, fileName)));
            Assert.AreEqual(ExecutionResultStatus.Success, executionResults[index].Status);
            Assert.AreEqual(true, executionResults[index].Message.StartsWith("Restored dependency"));
        }
    }
}