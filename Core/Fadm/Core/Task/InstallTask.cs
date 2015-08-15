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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fadm.Utilities;

namespace Fadm.Core.FadmTask
{
    /// <summary>
    /// The install task in charge of installing ressources to the local storage.
    /// </summary>
    public class InstallTask : ITask
    {
        /// <summary>
        /// Defines the allowed extensions.
        /// </summary>
        private string[] allowedExtensions = { ".dll", ".exe" };

        /// <summary>
        /// PDB extension.
        /// </summary>
        private const string PDB_EXTENSION = ".pdb";

        /// <summary>
        /// The target file path.
        /// </summary>
        private string targetfilepath;

        /// <summary>
        /// Initializes a new instance of <see cref="InstallTask"/>.
        /// <param name="path">The file to install.</param>
        /// </summary>
        public InstallTask(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Ensure the path is absolute
            targetfilepath = Path.GetFullPath(path);

        }
        /// <summary>
        /// Installs a file to the local repository.
        /// </summary>
        /// <returns>The execution result.</returns>
        public async Task<ExecutionResult> ExecuteAsync()
        {
            // Validate file existence
            if (!File.Exists(targetfilepath))
            {
                return ExecutionResult.Error("The file '{0}' doesn't exist", targetfilepath);
            }

            // Only dll file supported for now
            string extension = Path.GetExtension(targetfilepath);
            if (!allowedExtensions.Contains(extension))
            {
                return ExecutionResult.Error("The file '{0}' must have following extensions [{1}]", targetfilepath, string.Join(",", allowedExtensions));
            }

            try
            {
                // Ensure that the repository exists
                FileSystem.EnsureExistingDirectory(FileSystem.ComputeReporitoryPath());

                // Load the assembly an retrieve information
                Assembly assembly = Assembly.LoadFile(targetfilepath);
                string name = assembly.GetName().Name;
                string version = assembly.GetName().Version.ToString();

                // Compute the target path and ensure the dependency path existing in the local repository
                string dependencyPath = FileSystem.ComputeDependencyDirectoryPath(name, version);
                FileSystem.EnsureExistingDirectory(dependencyPath);

                // Initialize task list
                List<Task<ExecutionResult>> tasks = new List<Task<ExecutionResult>>();

                // Copy the dependency file
                string fileTarget = FileSystem.ComputeDependencyFilePath(name, version, extension.Substring(1));
                tasks.Add(InstallFileAsync(targetfilepath, Path.Combine(dependencyPath, fileTarget), critical: true));

                // Copy the pdb if available
                string pdbPath = Path.ChangeExtension(targetfilepath, PDB_EXTENSION);
                string pdbFileTarget = Path.ChangeExtension(fileTarget, PDB_EXTENSION);
                tasks.Add(InstallFileAsync(pdbPath, Path.Combine(dependencyPath, pdbFileTarget), critical: false));

                // Return execution result
                await Task.WhenAll(tasks);
                return ExecutionResult.Success("Install executed").With(from t in tasks select t.Result);
            }

            // Report error if any
            catch (Exception exception)
            {
                return ExecutionResult.Error(exception);
            }
        }

        /// <summary>
        /// Installs a file asynchronously.
        /// </summary>
        /// <param name="source">The location where the file is copied from.</param>
        /// <param name="destination">The location where the file is copied to.</param>
        /// <param name="critical">Whether critical execution.</param>
        /// <returns>The execution result.</returns>
        private async Task<ExecutionResult> InstallFileAsync(string source, string destination, bool critical)
        {
            try
            {
                // Perform copy asynchronously
                using (FileStream sourceStream = File.OpenRead(source))
                using (FileStream destinationStream = File.Create(destination))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                // Result result
                return ExecutionResult.Success("{0} installed to '{0}'", source, destination);
            }

            // Report error if any
            catch (Exception exception)
            {
                return critical ? ExecutionResult.Error(exception) : ExecutionResult.Warning(exception.Message);
            }
        }
    }
}