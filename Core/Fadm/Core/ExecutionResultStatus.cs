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

namespace Fadm.Core
{
    /// <summary>
    /// Represents an execution result status specifying whether it succedded.
    /// </summary>
    public enum ExecutionResultStatus : int
    {
        /// <summary>
        /// The execution succedded.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The execution failed.
        /// </summary>
        Error = 1,

        /// <summary>
        /// The execution completeted with warning.
        /// </summary>
        Warning = 2
    }
}
