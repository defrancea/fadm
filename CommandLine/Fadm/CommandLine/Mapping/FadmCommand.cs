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

using CommandLine;
using CommandLine.Text;

namespace Fadm.CommandLine.Mapping
{
    /// <summary>
    /// Represents Fadm command containing all sub commands.
    /// </summary>
    public class FadmCommand
    {
        /// <summary>
        /// Defines "add" sub command name.
        /// </summary>
        public const string ADD = "add";

        /// <summary>
        /// Defines "install" sub command name.
        /// </summary>
        public const string INSTALL = "install";

        /// <summary>
        /// Defines the "add" sub command.
        /// </summary>
        /// <returns></returns>
        [VerbOption(ADD, HelpText = "Add a dependency to a project")]
        public AddCommand Add { get; set; }

        /// <summary>
        /// Defines the "install" sub command.
        /// </summary>
        [VerbOption(INSTALL, HelpText = "Install a file to the local repository.")]
        public InstallCommand Install { get; set; }

        /// <summary>
        /// Generates the help display.
        /// </summary>
        /// <param name="command">The command the help is requested for.</param>
        [HelpVerbOption]
        public string GetUsage(string command)
        {
            return HelpText.AutoBuild(this, command);
        }
    }
}