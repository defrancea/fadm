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
using System.Reflection;

namespace Fadm.Core
{
    /// <summary>
    /// Fadm entry point.
    /// </summary>
    public class FadmEngine
    {
        public ExecutionResult Install(string path)
        {
            // Validate file existence
            if (!File.Exists(path))
            {
                return new ExecutionResult(string.Format("The file '{0}' doesn't exist", path));
            }

            // Only dll file supported for now
            string extension = Path.GetExtension(path);
            if (".dll" != extension)
            {
                return new ExecutionResult(string.Format("The file '{0}' is not a dll", path));
            }

            // Ensure that the repository exists
            string localRepositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fadm", "repository");
            this.EnsureExistingDirectory(localRepositoryPath);

            // Load the assembly an retrieve information
            Assembly assembly = Assembly.LoadFrom(path);
            string name = assembly.GetName().Name;
            string version = assembly.GetName().Version.ToString();

            // Compute the target path and ensure the dependency path existing in the local repository
            string dependencyPath = Path.Combine(localRepositoryPath, name, version);
            this.EnsureExistingDirectory(dependencyPath);

            // Copy the dll to the local repository
            string dllFileTarget = string.Format("{0}-{1}.dll", name, version);
            File.Copy(path, Path.Combine(dependencyPath, dllFileTarget), true);

            // Return execution result
            return new ExecutionResult(string.Format("File installed to '{0}'", dllFileTarget));
        }

        /// <summary>
        /// Ensures that a directory exists.
        /// </summary>
        /// <param name="path">The direcotry path.</param>
        private void EnsureExistingDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}