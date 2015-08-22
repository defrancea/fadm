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
using EnsureThat;
using Fadm.Core;

namespace Fadm.CommandLine.Mapping
{
    /// <summary>
    /// Formats execution result to a writer.
    /// </summary>
    public class ExecutionResultTextRenderer : IExecutionResultRenderer
    {
        /// <summary>
        /// The writer used to output the execution.
        /// </summary>
        public TextWriter writer;

        /// <summary>
        /// Initializes a new instance of <see cref="ExecutionResultTextRenderer"/>
        /// </summary>
        /// <param name="writer">The writer used to output the execution.</param>
        public ExecutionResultTextRenderer(TextWriter writer)
        {
            // Input validation
            Ensure.That(writer, "writer").IsNotNull();

            // Initialize
            this.writer = writer;
        }

        /// <summary>
        /// Formats an execution result.
        /// </summary>
        /// <param name="executionResult">The execution result to process.</param>
        /// <returns>The result formatted as a string.</returns>
        public void Render(ExecutionResult executionResult)
        {
            Render(executionResult, 0);
        }

        /// <summary>
        /// Formats an execution result.
        /// </summary>
        /// <param name="executionResult">The execution result to process.</param>
        /// <param name="depth">The current formatting depth.</param>
        private void Render(ExecutionResult executionResult, int depth)
        {
            // Input validation
            Ensure.That(executionResult, "executionResult").IsNotNull();
            Ensure.That(0 <= depth, "positive depth").IsTrue();

            // Append current execution result to the builder
            writer.WriteLine(string.Format("{0}[{1}] {2}", new string('\t', depth), executionResult.Status, executionResult.Message));

            // Execute the formatting for all sub result
            foreach (ExecutionResult subExecutionResult in executionResult.BlockingSubExecutionResults)
            {
                Render(subExecutionResult, depth + 1);
            }
        }
    }
}