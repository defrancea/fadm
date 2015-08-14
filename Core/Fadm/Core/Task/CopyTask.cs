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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fadm.Core.Loader;
using Fadm.Model;
using Fadm.Utilities;

namespace Fadm.Core.FadmTask
{
    /// <summary>
    /// The copy task in charge of retrieving the dependencies from the local storage.
    /// </summary>
    public class CopyTask : ITask
    {

        /// <summary>
        /// The target file path.
        /// </summary>
        private string targetfilepath;

        /// <summary>
        /// Initializes a new instance of <see cref="CopyTask"/>.
        /// <param name="path">The file to copy.</param>
        /// </summary>
        public CopyTask(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Initialize
            targetfilepath = Path.GetFullPath(path);
        }

        /// <summary>
        /// Copy the dependencies from the local repository.
        /// </summary>
        /// <returns>The execution result.</returns>
        public async Task<ExecutionResult> ExecuteAsync()
        {
            // Resolve descriptor path
            string descriptorDirectory = targetfilepath;
            if (!string.IsNullOrWhiteSpace(Path.GetExtension(targetfilepath)))
            {
                descriptorDirectory = Path.GetDirectoryName(targetfilepath);
            }
            string descriptorPath = Path.Combine(descriptorDirectory, "fadm.xml");

            // Validate file existence
            if (!File.Exists(descriptorPath))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' doesn't exist", descriptorPath));
            }

            // Load descriptor
            DescriptorLoader loader = new DescriptorLoader();
            Project project = await loader.LoadAsync(descriptorPath);

            // Process each dependencies
            List<Task<ExecutionResult>> processingTasks = new List<Task<ExecutionResult>>();
            foreach (Dependency dependency in project.Dependencies)
            {
                processingTasks.Add(this.ProcessDependencyAsync(dependency, descriptorDirectory));
            }

            // Return execution result
            Task.WaitAll(processingTasks.ToArray());
            return new ExecutionResult(ExecutionResultStatus.Success, "Dependencies copied successfully", (from r in processingTasks select r.Result).ToArray());
        }

        /// <summary>
        /// Procces a dependency asynchronously.
        /// </summary>
        /// <param name="dependency">The dependency to process.</param>
        /// <param name="baseDirectory">The base directory.</param>
        /// <returns>The execution result.</returns>
        private async Task<ExecutionResult> ProcessDependencyAsync(Dependency dependency, string baseDirectory)
        {
            try
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
                    return new ExecutionResult(
                        ExecutionResultStatus.Error,
                        string.Format(CultureInfo.InvariantCulture, "Dependency '{0}' unknown", dependencyFile));
                }

                // Compute target path
                string targetDependencyPath = Path.Combine(baseDirectory, "dependency");
                string targetFilePath = Path.Combine(targetDependencyPath, targetFile);

                // Check the dependecy already copied
                if (File.Exists(targetFilePath))
                {
                    return new ExecutionResult(
                        ExecutionResultStatus.Warning,
                        string.Format(CultureInfo.InvariantCulture, "Dependency '{0}' already copied", targetFilePath));
                }

                // Copy dependency
                FileSystem.EnsureExistingDirectory(targetDependencyPath);
                using (FileStream sourceStream = File.Open(dependencyFile, FileMode.Open))
                using (FileStream destinationStream = File.Create(targetFilePath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                // Compute sub execution result
                return new ExecutionResult(
                    ExecutionResultStatus.Success,
                    string.Format(CultureInfo.InvariantCulture, "Dependency '{0}' copied successfully", targetFilePath));
            }
            catch (Exception exception)
            {
                return new ExecutionResult(ExecutionResultStatus.Error, exception.Message);
            }
        }
    }
}