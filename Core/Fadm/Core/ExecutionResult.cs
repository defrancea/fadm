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
using System.Linq;
using Fadm.Utilities;

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
        public List<ExecutionResult> SubExecutionResults { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ExecutionResult"/>.
        /// </summary>
        /// <param name="status">The execution result status.</param>
        /// <param name="message">The execution result message</param>
        /// <param name="parameters">The parametersm null or empty is a valid value.</param>
        private ExecutionResult(ExecutionResultStatus status, string message, params object[] parameters)
        {
            // Input validation
            Validate.IsNotNull(message, "message must not be null.");

            // Initialize
            this.Status = status;
            this.Message = this.BuildMessage(message, parameters);
            this.SubExecutionResults = new List<ExecutionResult>();
        }

        /// <summary>
        /// Builds an success result from a message.
        /// </summary>
        /// <param name="message">The execution result message</param>
        /// <param name="parameters">The parametersm null or empty is a valid value.</param>
        /// <returns>The result.</returns>
        public static ExecutionResult Success(string message, params object[] parameters)
        {
            // Input validation
            Validate.IsNotNull(message, "message must not be null.");

            // Build result
            return new ExecutionResult(ExecutionResultStatus.Success, message, parameters);
        }

        /// <summary>
        /// Builds an warning result from a message.
        /// </summary>
        /// <param name="message">The execution result message</param>
        /// <param name="parameters">The parametersm null or empty is a valid value.</param>
        /// <returns>The result.</returns>
        public static ExecutionResult Warning(string message, params object[] parameters)
        {
            // Input validation
            Validate.IsNotNull(message, "message must not be null.");

            // Build result
            return new ExecutionResult(ExecutionResultStatus.Warning, message, parameters);
        }

        /// <summary>
        /// Builds an error result from a message.
        /// </summary>
        /// <param name="message">The execution result message</param>
        /// <param name="parameters">The parametersm null or empty is a valid value.</param>
        /// <returns>The result.</returns>
        public static ExecutionResult Error(string message, params object[] parameters)
        {
            // Input validation
            Validate.IsNotNull(message, "message must not be null.");

            // Build result
            return new ExecutionResult(ExecutionResultStatus.Error, message, parameters);
        }

        /// <summary>
        /// Builds an error result from an exception.
        /// </summary>
        /// <param name="exception">The source exception.</param>
        /// <returns>The result.</returns>
        public static ExecutionResult Error(Exception exception)
        {
            // Input validation
            Validate.IsNotNull(exception, "exception must not be null");

            // Build result
            return new ExecutionResult(ExecutionResultStatus.Error, exception.Message);
        }

        /// <summary>
        /// Chains am execution result adding sub results.
        /// </summary>
        /// <param name="subExecutionResults">The sub execution results.</param>
        /// <returns>The execution results.</returns>
        public ExecutionResult With(IEnumerable<ExecutionResult> subExecutionResults)
        {
            // Input validation
            Validate.IsNotNull(subExecutionResults, "subExecutionResults must not be null");

            // Add to sub execution results
            this.SubExecutionResults.AddRange(subExecutionResults);

            // Return this for fluent calls
            return this;
        }

        /// <summary>
        /// Returns the current instance as enumerable.
        /// </summary>
        /// <returns>The instance as enumerable.</returns>
        public IEnumerable<ExecutionResult> AsEnumerable()
        {
            return new[] { this };
        }

        /// <summary>
        /// Builds the mesage based on the template and the parameters.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="parameters">The parametersm null or empty is a valid value.</param>
        /// <returns>The message.</returns>
        private string BuildMessage(string template, params object[] parameters)
        {
            // Input validation
            Validate.IsNotNull(template, "template must not be null");

            // Return template only if no parameter are set
            if (null == parameters || !parameters.Any())
                return template;

            // Otherwise, return the formatted pattern
            return string.Format(CultureInfo.InvariantCulture, template, parameters);
        }
    }
}