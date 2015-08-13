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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fadm.Core.Loader;
using Fadm.Model;
using Fadm.Utilities;

namespace Fadm.Core.FadmTask
{
    /// <summary>
    /// The copy task in charge of retrieving the dependencies from the local storage.
    /// </summary>
    public class CopyTask : ICopyTask
    {
        /// <summary>
        /// Copy the dependencies from the local repository.
        /// </summary>
        /// <param name="path">The file path to copy the dependencies for.</param>
        /// <returns>The execution result.</returns>
        public ExecutionResult Copy(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Ensure the path is absolute
            path = Path.GetFullPath(path);

            // Resolve descriptor path
            string descriptorDirectory = path;
            if (!string.IsNullOrWhiteSpace(Path.GetExtension(path)))
            {
                descriptorDirectory = Path.GetDirectoryName(path);
            }
            string descriptorPath = Path.Combine(descriptorDirectory, "fadm.xml");

            // Validate file existence
            if (!File.Exists(descriptorPath))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' doesn't exist", descriptorPath));
            }

            // Load descriptor
            DescriptorLoader loader = new DescriptorLoader();
            Project project = loader.Load(descriptorPath);

            // Process each dependencies
            List<ExecutionResult> subExecutionResults = new List<ExecutionResult>();
            foreach (Dependency dependency in project.Dependencies)
            {
                // Read from model
                string name = dependency.Name;
                string version = dependency.Version.ToString();
                string targetFile = FileSystem.ComputeFileName(name, version, "dll");

                // Local repository path
                string dependencyPath = FileSystem.ComputeDependencyDirectoryPath(name, version);
                string dependencyFile = Path.Combine(dependencyPath, targetFile);

                // Check the dependency exists
                if (!File.Exists(dependencyFile))
                {
                    subExecutionResults.Add(new ExecutionResult(
                        ExecutionResultStatus.Error,
                        string.Format(CultureInfo.InvariantCulture, "Dependency '{0}' unknown", dependencyFile)));
                    continue;
                }

                // Compute target path
                string targetDependencyPath = Path.Combine(descriptorDirectory, "dependency");
                string targetFilePath = Path.Combine(targetDependencyPath, targetFile);

                // Check the dependecy already copied
                if (File.Exists(targetFilePath))
                {
                    subExecutionResults.Add(new ExecutionResult(
                        ExecutionResultStatus.Warning,
                        string.Format(CultureInfo.InvariantCulture, "Dependency '{0}' already copied", targetFilePath)));
                    continue;
                }

                // Copy dependency
                FileSystem.EnsureExistingDirectory(targetDependencyPath);
                File.Copy(dependencyFile, targetFilePath, true);

                // Compute sub execution result
                subExecutionResults.Add(new ExecutionResult(
                    ExecutionResultStatus.Success,
                    string.Format(CultureInfo.InvariantCulture, "Dependency '{0}' copied successfully", targetFilePath)));
            }

            // Return execution result
            return new ExecutionResult(ExecutionResultStatus.Success, "Dependencies copied successfully", subExecutionResults.ToArray());
        }
    }
}