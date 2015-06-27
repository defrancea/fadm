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
    /// Represents an execution result providing more context to the calling code.
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// The status specifying whether it worked.
        /// </summary>
        public ExecutionResultStatus Status { get; private set; }

        /// <summary>
        /// The message containing a description of what happened.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The sub execution results.
        /// </summary>
        public ExecutionResult[] SubExecutionResults { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ExecutionResult"/>.
        /// </summary>
        /// <param name="status">The execution result status.</param>
        /// <param name="message">The execution result message</param>
        public ExecutionResult(ExecutionResultStatus status, string message)
        {
            this.Status = status;
            this.Message = message;
            this.SubExecutionResults = new ExecutionResult[0];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ExecutionResult"/> containing sub task.
        /// </summary>
        /// <param name="status">The execution result status.</param>
        /// <param name="message">The execution result message.</param>
        /// <param name="subExecutionResults">The sub execution results.</param>
        public ExecutionResult(ExecutionResultStatus status, string message, ExecutionResult[] subExecutionResults)
        {
            this.Status = status;
            this.Message = message;
            this.SubExecutionResults = subExecutionResults;
        }
    }
}