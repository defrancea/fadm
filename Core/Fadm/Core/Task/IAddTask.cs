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

namespace Fadm.Core.Task
{
    /// <summary>
    /// The add task in charge of adding Fadm to solutions and project files.
    /// </summary>
    public interface IAddTask
    {
        /// <summary>
        /// Adds Fadm installer to a solution or a project file.
        /// </summary>
        /// <param name="path">The file to add the installer to.</param>
        /// <returns>The execution result.</returns>
        ExecutionResult Add(string path);
    }
}