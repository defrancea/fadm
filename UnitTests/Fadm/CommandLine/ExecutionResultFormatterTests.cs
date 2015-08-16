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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fadm.CommandLine.Mapping;
using Fadm.Core;
using NUnit.Framework;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Tests ExecutionResultFormatter.
    /// </summary>
    [TestFixture]
    public class ExecutionResultFormatterTests
    {
        /// <summary>
        /// The formatter instance.
        /// </summary>
        public ExecutionResultTextRenderer Renderer { get; private set; }

        /// <summary>
        /// The execution output.
        /// </summary>
        public StringWriter ExecutionOutput { get; private set; }

        /// <summary>
        /// Initializes <see cref="ExecutionResultFormatterTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Renderer = new ExecutionResultTextRenderer(ExecutionOutput = new StringWriter());
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void FormatNull()
        {
            Renderer.Render(null);
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with simple one level value.
        /// </summary>
        [Test]
        public void FormatOneLevel()
        {
            Renderer.Render(ExecutionResult.Success("Nice message"));
            Assert.AreEqual(
                string.Format(CultureInfo.InvariantCulture, "[Success] Nice message{0}", Environment.NewLine),
                ExecutionOutput.ToString());
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with empty nester children.
        /// </summary>
        [Test]
        public void FormatNestedEmptyChildren()
        {
            Renderer.Render(ExecutionResult.Success("Nice message"));
            Assert.AreEqual(
                string.Format(CultureInfo.InvariantCulture, "[Success] Nice message{0}", Environment.NewLine),
                ExecutionOutput.ToString());
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with nested children.
        /// </summary>
        [Test]
        public void FormatNestedChildren()
        {
            string outputMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "[Success] Nice message{0}\t[Error] This one failed{0}\t[Success] This one worked{0}",
                    Environment.NewLine);

            Renderer.Render(ExecutionResult
                    .Success("Nice message")
                    .With(new ExecutionResult[]
                        {
                            ExecutionResult.Error("This one failed"),
                            ExecutionResult.Success("This one worked")
                        }));

            Assert.AreEqual(outputMessage, ExecutionOutput.ToString());
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with parallel asynchronous execution.
        /// </summary>
        [Test]
        public void FormatNestedParallel()
        {
            // Build expected output
            StringBuilder builder = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "[Success] Nice message{0}", Environment.NewLine));
            builder.Append(string.Join(string.Empty, Enumerable.Repeat(string.Format(CultureInfo.InvariantCulture, "\t[Success] Done{0}", Environment.NewLine), 5)));

            // Render 5 concurrent execution
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Renderer.Render(ExecutionResult
                    .Success("Nice message")
                    .With(new Task<ExecutionResult>[]
                        {
                            Spawn("Done"),
                            Spawn("Done"),
                            Spawn("Done"),
                            Spawn("Done"),
                            Spawn("Done")
                        }));
            stopWatch.Stop();

            // Assert rendered and execution time
            Assert.AreEqual(builder.ToString(), ExecutionOutput.ToString());
            Assert.IsTrue(1500 > stopWatch.ElapsedMilliseconds);
        }

        private async Task<ExecutionResult> Spawn(string value)
        {
            await Task.Delay(1000);
            return ExecutionResult.Success(value);
        }
    }
}