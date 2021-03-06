﻿/*
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
using System.Threading.Tasks;
using EnsureThat;

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
        /// The synchronous sub execution results.
        /// </summary>
        private List<ExecutionResult> syncSubExecutionResults;

        /// <summary>
        /// The asynchronous sub execution results.
        /// </summary>
        private List<Task<ExecutionResult>> asyncSubExecutionResults;

        /// <summary>
        /// Executions resutls waiting for the completing with a blocking calls.
        /// </summary>
        public IEnumerable<ExecutionResult> BlockingSubExecutionResults
        {
            get
            {
                // Produce all synchrone tasks first
                foreach (ExecutionResult executionResult in this.syncSubExecutionResults)
                {
                    yield return executionResult;
                }

                // For each asynchronous execution, wait for the first next completion and produce it
                List<Task<ExecutionResult>> executing = this.asyncSubExecutionResults;
                while (0 < executing.Count)
                {
                    int index = Task.WaitAny(executing.ToArray());
                    yield return executing[index].Result;
                    executing.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ExecutionResult"/>.
        /// </summary>
        /// <param name="status">The execution result status.</param>
        /// <param name="message">The execution result message</param>
        /// <param name="parameters">The parametersm null or empty is a valid value.</param>
        private ExecutionResult(ExecutionResultStatus status, string message, params object[] parameters)
        {
            // Input validation
            Ensure.That(message, "message").IsNotNullOrWhiteSpace();

            // Initialize
            this.Status = status;
            this.Message = this.BuildMessage(message, parameters);
            this.syncSubExecutionResults = new List<ExecutionResult>();
            this.asyncSubExecutionResults = new List<Task<ExecutionResult>>();
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
            Ensure.That(message, "message").IsNotNullOrWhiteSpace();

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
            Ensure.That(message, "message").IsNotNullOrWhiteSpace();

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
            Ensure.That(message, "message").IsNotNullOrWhiteSpace();

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
            Ensure.That(exception, "exception").IsNotNull();

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
            Ensure.That(subExecutionResults, "subExecutionResults").IsNotNull();

            // Add to sub execution results
            this.syncSubExecutionResults.AddRange(subExecutionResults);

            // Return this for fluent calls
            return this;
        }

        /// <summary>
        /// Chains am execution result adding sub results.
        /// </summary>
        /// <param name="subExecutionResults">The sub execution results.</param>
        /// <returns>The execution results.</returns>
        public ExecutionResult With(IEnumerable<Task<ExecutionResult>> subExecutionResults)
        {
            // Input validation
            Ensure.That(subExecutionResults, "subExecutionResults").IsNotNull();

            // Add to sub execution results
            this.asyncSubExecutionResults.AddRange(subExecutionResults);

            // Return this for fluent calls
            return this;
        }

        /// <summary>
        /// Returns the current instance as a task.
        /// </summary>
        /// <returns>The instance as a task.</returns>
        public Task<ExecutionResult> AsTask()
        {
            return Task.FromResult(this);
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
            Ensure.That(template, "template").IsNotNullOrWhiteSpace();

            // Return template only if no parameter are set
            if (null == parameters || !parameters.Any())
                return template;

            // Otherwise, return the formatted pattern
            return string.Format(CultureInfo.InvariantCulture, template, parameters);
        }
    }
}