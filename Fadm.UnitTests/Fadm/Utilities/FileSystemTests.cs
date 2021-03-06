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
using System.IO;
using NUnit.Framework;

namespace Fadm.Utilities
{
    /// <summary>
    /// Tests FileSystem.
    /// </summary>
    [TestFixture]
    public class FileSystemTests
    {
        /// <summary>
        /// Initializes <see cref="FileSystemTests"/>.
        /// </summary>
        [SetUp]
        [TearDown]
        public void Initialize()
        {
            string folderName = "TestFolder";
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
        }

        /// <summary>
        /// Tests EnsureExistingDirectory(string) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureExistingDirectoryNull()
        {
            FileSystem.EnsureExistingDirectory(null);
        }

        /// <summary>
        /// Tests EnsureExistingDirectory(string).
        /// </summary>
        [Test]
        [TestCase("TestFolder")]
        [TestCase("TestFolder/TestSubFolder")]
        public void EnsureExistingDirectoryNotNullOneLevel(string folderName)
        {
            Assert.AreEqual(false, Directory.Exists(folderName));
            FileSystem.EnsureExistingDirectory(folderName);
            Assert.AreEqual(true, Directory.Exists(folderName));
            Directory.Delete(folderName);
        }

        /// <summary>
        /// Tests ComputeFileName(string, string, string).
        /// </summary>
        [Test]
        public void ComputeFileName()
        {
            Assert.AreEqual("name-version.extension", FileSystem.ComputeFileName("name", "version", "extension"));
        }

        /// <summary>
        /// Tests ComputeFileName(string, string, string) with invalid input.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <param name="version">The file version.</param>
        /// <param name="extension">The file extension.</param>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        [TestCase("a", "b", "")]
        [TestCase("a", "b", null)]
        [TestCase("a", "", "c")]
        [TestCase("a", null, "c")]
        [TestCase("", "b", "c")]
        [TestCase(null, "b", "c")]
        public void ComputeFileNameInvalid(string name, string version, string extension)
        {
            FileSystem.ComputeFileName(name, version, extension);
        }
    }
}