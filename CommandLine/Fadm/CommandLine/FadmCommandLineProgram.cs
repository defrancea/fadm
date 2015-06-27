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
using CommandLine;
using Fadm.CommandLine.Mapping;
using Fadm.Core;

namespace Fadm.CommandLine
{
    /// <summary>
    /// Fadm command line entry point.
    /// </summary>
    public class FadmCommandLineProgram
    {
        /// <summary>
        /// The Main method taking the execution entry point.
        /// It directly delegates to the program execution
        /// </summary>
        /// <param name="args">The console arguments.</param>
        public static void Main(string[] args)
        {
            new FadmCommandLineProgram().Execute(args);
        }

        /// <summary>
        /// The engine doing all operation.
        /// </summary>
        public FadmEngine FadmEngine { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="FadmCommandLineProgram"/>.
        /// </summary>
        public FadmCommandLineProgram()
        {
            this.FadmEngine = new FadmEngine();
        }

        /// <summary>
        /// Executes the command lines and delegates to the appropriate command.
        /// </summary>
        /// <param name="args">The console arguments.</param>
        public void Execute(string[] args)
        {
            // Parse the program arguments and provide a callback in order to handle the proper command
            Parser.Default.ParseArgumentsStrict(args, new FadmCommand(), (verb, executedCommand) =>
            {
                // Delegates to the right method depending on the entered command
                switch(verb)
                {
                    case FadmCommand.ADD:
                        Add((AddCommand)executedCommand);
                        break;

                    case FadmCommand.INSTALL:
                        Install((InstallCommand)executedCommand);
                        break;
                }
            });
        }

        /// <summary>
        /// Adds a dependency to a project.
        /// </summary>
        /// <param name="add">The add command containing the user input.</param>
        public void Add(AddCommand add)
        {
            // Todo: implement
            Console.WriteLine("Add");
        }

        /// <summary>
        /// Installs a file to the local repository.
        /// </summary>
        /// <param name="install">The install method containing the user input.</param>
        public void Install(InstallCommand install)
        {
            ExecutionResult result = this.FadmEngine.Install(install.FilePath);
            Console.WriteLine(result.Message);
        }
    }
}