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

using System.Threading.Tasks;
using EnsureThat;
using Fadm.Core.FadmTask;

namespace Fadm.Core
{
    /// <summary>
    /// Fadm engine handling external operations.
    /// </summary>
    public class FadmEngine : IFadmEngine
    {
        /// <summary>
        /// Delegates to the add task.
        /// </summary>
        /// <param name="path">The file to process.</param>
        /// <returns>The execution result.</returns>
        public async Task<ExecutionResult> AddAsync(string path)
        {
            // Input validation
            Ensure.That(path, "path").IsNotNullOrWhiteSpace();

            // Invoke the task
            return await new AddTask(path).ExecuteAsync();
        }

        /// <summary>
        /// Delegates to the copy task.
        /// </summary>
        /// <param name="path">The file to process.</param>
        /// <returns>The execution result.</returns>
        public async Task<ExecutionResult> CopyAsync(string path)
        {
            // Input validation
            Ensure.That(path, "path").IsNotNullOrWhiteSpace();

            // Invoke the task
            return await new CopyTask(path).ExecuteAsync();
        }

        /// <summary>
        /// Delegates to the install task.
        /// </summary>
        /// <param name="path">The file to install.</param>
        /// <returns>The execution result.</returns>
        public async Task<ExecutionResult> InstallAsync(string path)
        {
            // Input validation
            Ensure.That(path, "path").IsNotNullOrWhiteSpace();

            // Invoke the task
            return await new InstallTask(path).ExecuteAsync();
        }
    }
}