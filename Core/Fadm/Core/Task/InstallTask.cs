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

using System.IO;
using System.Linq;
using System.Reflection;
using Fadm.Utilities;

namespace Fadm.Core.FadmTask
{
    /// <summary>
    /// The install task in charge of installing ressources to the local storage.
    /// </summary>
    public class InstallTask : IInstallTask
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
        /// Installs a file to the local repository.
        /// </summary>
        /// <param name="path">The file path to install.</param>
        /// <returns>The execution result.</returns>
        public ExecutionResult Install(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Ensure the path is absolute
            path = Path.GetFullPath(path);

            // Validate file existence
            if (!File.Exists(path))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' doesn't exist", path));
            }

            // Only dll file supported for now
            string extension = Path.GetExtension(path);
            if (!allowedExtensions.Contains(extension))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' must have following extensions [{1}]", path, string.Join(",", allowedExtensions)));
            }

            // Ensure that the repository exists
            FileSystem.EnsureExistingDirectory(FileSystem.ComputeReporitoryPath());

            // Load the assembly an retrieve information
            Assembly assembly = Assembly.LoadFile(path);
            string name = assembly.GetName().Name;
            string version = assembly.GetName().Version.ToString();

            // Compute the target path and ensure the dependency path existing in the local repository
            string dependencyPath = FileSystem.ComputeDependencyDirectoryPath(name, version);
            FileSystem.EnsureExistingDirectory(dependencyPath);

            // Copy the dependency to the local repository
            string fileTarget = FileSystem.ComputeDependencyFilePath(name, version, extension.Substring(1));
            File.Copy(path, Path.Combine(dependencyPath, fileTarget), true);

            // Copy the pdb if available
            string pdbPath = Path.ChangeExtension(path, PDB_EXTENSION);
            string pdbFileTarget = Path.ChangeExtension(fileTarget, PDB_EXTENSION);
            ExecutionResult pdbExecutionResult;
            if (File.Exists(pdbPath))
            {
                File.Copy(pdbPath, Path.Combine(dependencyPath, pdbFileTarget), true);
                pdbExecutionResult = new ExecutionResult(ExecutionResultStatus.Success, string.Format("PDB installed to '{0}'", pdbFileTarget));
            }
            else
            {
                pdbExecutionResult = new ExecutionResult(ExecutionResultStatus.Warning, string.Format("PDB not found at '{0}'", pdbPath));
            }

            // Return execution result
            return new ExecutionResult(ExecutionResultStatus.Success, string.Format("File installed to '{0}'", fileTarget), new ExecutionResult[] { pdbExecutionResult });
        }
    }
}