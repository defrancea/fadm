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
using NuGet;

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
            try
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
                    return ExecutionResult.Error("The file '{0}' doesn't exist", descriptorPath);
                }

                // Load descriptor
                DescriptorLoader loader = new DescriptorLoader();
                Project project = await loader.LoadAsync(descriptorPath);

                // Return execution result
                return ExecutionResult.Success("Copy executed").With(from d in project.Dependencies select this.ProcessDependencyAsync(d, descriptorDirectory));
            }

            // Report error if any
            catch (Exception exception)
            {
                return ExecutionResult.Error(exception);
            }
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

                // Compute target path
                string targetDependencyPath = Path.Combine(baseDirectory, "dependency");
                string targetFilePath = Path.Combine(targetDependencyPath, targetFile);

                // Check the dependecy already copied
                if (File.Exists(targetFilePath))
                {
                    return ExecutionResult.Success("Nothing to do for {0}:{1} {2}", dependency.Name, dependency.Version, targetFilePath);
                }

                // Check the dependency exists
                Stream dependencyStream = null;
                if (!File.Exists(dependencyFile))
                {
                    // Search for the dependency in nuget central
                    IPackageRepository repository = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
                    IPackage package;
                    if (!repository.TryFindPackage(dependency.Name, SemanticVersion.Parse(dependency.Version.ToString()), out package))
                    {
                        // Report error if not found
                        return ExecutionResult.Error("Dependency {0}:{1} unknown", dependency.Name, dependency.Version);
                    }

                    // Look for matching lib file
                    string nugetLibFile = string.Format(CultureInfo.InvariantCulture, "{0}{1}", dependency.Name, Path.GetExtension(dependencyFile));
                    IPackageFile[] matchingFile = (from l in package.GetLibFiles() where l.EffectivePath == nugetLibFile select l).ToArray();

                    // Report error if not file found
                    if (!matchingFile.Any())
                    {
                        return ExecutionResult.Error("No lib file {0} found for {1}:{2}", nugetLibFile, dependency.Name, dependency.Version);
                    }

                    // Define stream from nuget
                    dependencyStream = matchingFile.First().GetStream();
                }

                // Return the result
                return await this.RestoreDependencyAsync(dependencyFile, targetFilePath, dependency, dependencyStream);

            }
            catch (Exception exception)
            {
                return ExecutionResult.Error(exception);
            }
        }

        private async Task<ExecutionResult> RestoreDependencyAsync(string sourceFile, string destinationFile, Dependency dependency, Stream stream)
        {
            // Initialize sub execution results
            List<ExecutionResult> subExecutionResults = new List<ExecutionResult>();

            // Retrieve from stream
            if (null != stream)
            {
                FileSystem.EnsureExistingDirectory(Path.GetDirectoryName(sourceFile));
                using (Stream sourceStream = stream)
                using (FileStream destinationStream = File.Create(sourceFile))
                {
                    subExecutionResults.Add(await this.CopyDependencyAsync(sourceStream, destinationStream, dependency, "Downloaded dependency {0}:{1}"));
                }
            }

            // Copy local file
            FileSystem.EnsureExistingDirectory(Path.GetDirectoryName(destinationFile));
            using (FileStream sourceStream = File.OpenRead(sourceFile))
            using (FileStream destinationStream = File.Create(destinationFile))
            {
                subExecutionResults.Add(await this.CopyDependencyAsync(sourceStream, destinationStream, dependency, "Copied dependency {0}:{1}"));
            }

            // Return results
            return ExecutionResult.Success("Restored dependency {0}:{1}", dependency.Name, dependency.Version).With(subExecutionResults);
        }

        /// <summary>
        /// Downloads the depnendency.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="destinationStream">The destination stream.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="message">The displayed message.</param>
        /// <returns>The execytion result.</returns>
        private async Task<ExecutionResult> CopyDependencyAsync(Stream sourceStream, Stream destinationStream, Dependency dependency, string message)
        {
            await sourceStream.CopyToAsync(destinationStream);
            return ExecutionResult.Success(message, dependency.Name, dependency.Version);
        }
    }
}