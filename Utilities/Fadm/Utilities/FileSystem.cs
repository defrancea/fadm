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

namespace Fadm.Utilities
{
    public class FileSystem
    {
        /// <summary>
        /// Ensures that a directory exists.
        /// </summary>
        /// <param name="path">The direcotry path.</param>
        public static void EnsureExistingDirectory(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Check the file system and create the directory if needed.
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Computes the local repository path.
        /// </summary>
        public static string ComputeReporitoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fadm", "repository");
        }

        /// <summary>
        /// Computes the dependency directory path based on name and version.
        /// </summary>
        /// <param name="name">The name of the dependency.</param>
        /// <param name="version">The version of the dependency.</param>
        /// <returns>The computed dependency path in the local repository.</returns>
        public static string ComputeDependencyDirectoryPath(string name, string version)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(name, "name must not be null.");
            Validate.IsNotNullOrWhitespace(version, "version must not be null.");

            // Return the computed value
            return Path.Combine(ComputeReporitoryPath(), name, version);
        }

        /// <summary>
        /// Computes the dependency file path based on name and version.
        /// </summary>
        /// <param name="name">The name of the dependency.</param>
        /// <param name="version">The version of the dependency.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The computed dependency path in the local repository.</returns>
        public static string ComputeDependencyFilePath(string name, string version, string extension)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(name, "name must not be null.");
            Validate.IsNotNullOrWhitespace(version, "version must not be null.");
            Validate.IsNotNullOrWhitespace(extension, "extension must not be null.");

            string fileTarget = string.Format("{0}-{1}.{2}", name, version, extension);

            // Return the computed value
            return Path.Combine(ComputeDependencyDirectoryPath(name, version), fileTarget);
        }
    }
}