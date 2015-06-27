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

using System.Text;
using Fadm.Core;

namespace Fadm.CommandLine.Mapping
{
    public class ExecutionResultFormatter
    {
        /// <summary>
        /// Formats an execution result.
        /// </summary>
        /// <param name="executionResult">The execution result to process.</param>
        /// <returns>The result formatted as a string.</returns>
        public string Format(ExecutionResult executionResult)
        {
            return Format(new StringBuilder(), executionResult, 0).ToString();
        }

        /// <summary>
        /// Formats an execution result.
        /// </summary>
        /// <param name="stringBuilder">The string builder passed to the whole hierarchy during the formatting.</param>
        /// <param name="executionResult">The execution result to process.</param>
        /// <param name="depth">The current formatting depth.</param>
        /// <returns>A stirng builder containing the formatted execution result.</returns>
        private StringBuilder Format(StringBuilder stringBuilder, ExecutionResult executionResult, int depth)
        {
            // Append current execution result to the builder
            stringBuilder.AppendLine(string.Format("{0}[{1}] {2}", new string('\t', depth), executionResult.Status, executionResult.Message));

            // Execute the formatting for all sub result
            foreach (ExecutionResult subExecutionResult in executionResult.SubExecutionResults)
            {
                Format(stringBuilder, subExecutionResult, depth + 1);
            }

            // Return the string builder
            return stringBuilder;
        }
    }
}