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
using Fadm.Core;
using Moq;
using NUnit.Framework;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Tests FadmCommandLineProgram.
    /// </summary>
    [TestFixture]
    public class FadmCommandLineProgramTests
    {
        /// <summary>
        /// The program instance.
        /// </summary>
        public FadmCommandLineProgram Program { get; private set; }

        /// <summary>
        /// The engine mock.
        /// </summary>
        public Mock<IFadmEngine> Engine { get; private set; }

        /// <summary>
        /// The formatter mock.
        /// </summary>
        public Mock<IExecutionResultFormatter> Formatter { get; private set; }

        /// <summary>
        /// Initializes <see cref="FadmCommandLineProgramTests"/>.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Engine = new Mock<IFadmEngine>();
            Formatter = new Mock<IExecutionResultFormatter>();
            Program = new FadmCommandLineProgram(Engine.Object, Formatter.Object);
        }

        /// <summary>
        /// Tests Execute(string[]) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyNull()
        {
            Program.Execute(null);
        }

        /// <summary>
        /// Tests Execute(string[]) for add command.
        /// </summary>
        [Test]
        public void ExecuteAdd()
        {
            ExecutionResult result = null;
            Engine
                .Setup(instance => instance.Add(It.IsAny<string>()))
                .Returns(new ExecutionResult(ExecutionResultStatus.Error, "Add executed"));
            Formatter
                .Setup(instance => instance.Format(It.IsAny<ExecutionResult>()))
                .Callback<ExecutionResult>(executionResult => result = executionResult);
            Program.Execute(new string[] { "add", "foo.sln" });
            Engine.Verify(instance => instance.Add("foo.sln"));

            Assert.IsNotNull(result);
            Assert.AreEqual(ExecutionResultStatus.Error, result.Status);
            Assert.AreEqual("Add executed", result.Message);
        }

        /// <summary>
        /// Tests Execute(string[]) for copy command.
        /// </summary>
        [Test]
        public void ExecuteCopy()
        {
            ExecutionResult result = null;
            Engine
                .Setup(instance => instance.Copy(It.IsAny<string>()))
                .Returns(new ExecutionResult(ExecutionResultStatus.Error, "Copy executed"));
            Formatter
                .Setup(instance => instance.Format(It.IsAny<ExecutionResult>()))
                .Callback<ExecutionResult>(executionResult => result = executionResult);
            Program.Execute(new string[] { "copy", "foo.csproj" });
            Engine.Verify(instance => instance.Copy("foo.csproj"));

            Assert.IsNotNull(result);
            Assert.AreEqual(ExecutionResultStatus.Error, result.Status);
            Assert.AreEqual("Copy executed", result.Message);
        }

        /// <summary>
        /// Tests Execute(string[]) for install command.
        /// </summary>
        [Test]
        public void ExecuteInstall()
        {
            ExecutionResult result = null;
            Engine
                .Setup(instance => instance.Install(It.IsAny<string>()))
                .Returns(new ExecutionResult(ExecutionResultStatus.Success, "Install executed"));
            Formatter
                .Setup(instance => instance.Format(It.IsAny<ExecutionResult>()))
                .Callback<ExecutionResult>(executionResult => result = executionResult);
            Program.Execute(new string[] { "install", "foo.dll" });
            Engine.Verify(instance => instance.Install("foo.dll"));

            Assert.IsNotNull(result);
            Assert.AreEqual(ExecutionResultStatus.Success, result.Status);
            Assert.AreEqual("Install executed", result.Message);
        }
    }
}