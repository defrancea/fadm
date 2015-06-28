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
        public ExecutionResultFormatter Formatter { get; private set; }

        /// <summary>
        /// Initializes <see cref="ExecutionResultFormatterTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Formatter = new ExecutionResultFormatter();
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void FormatNull()
        {
            Formatter.Format(null);
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with simple one level value.
        /// </summary>
        [Test]
        public void FormatOneLevel()
        {
            Assert.AreEqual(
                "[Success] Nice message\r\n",
                Formatter.Format(new ExecutionResult(ExecutionResultStatus.Success, "Nice message")));
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with empty nester children.
        /// </summary>
        [Test]
        public void FormatNestedEmptyChildren()
        {
            Assert.AreEqual(
                "[Success] Nice message\r\n",
                Formatter.Format(
                    new ExecutionResult(
                        ExecutionResultStatus.Success,
                        "Nice message",
                        new ExecutionResult[0])));
        }

        /// <summary>
        /// Tests Format(ExecutionResult) with nested children.
        /// </summary>
        [Test]
        public void FormatNestedChildren()
        {
            Assert.AreEqual(
                "[Success] Nice message\r\n\t[Error] This one failed\r\n\t[Success] This one worked\r\n",
                Formatter.Format(
                    new ExecutionResult(
                        ExecutionResultStatus.Success,
                        "Nice message",
                        new ExecutionResult[]
                        {
                            new ExecutionResult(ExecutionResultStatus.Error, "This one failed"),
                            new ExecutionResult(ExecutionResultStatus.Success, "This one worked")
                        })));
        }
    }
}