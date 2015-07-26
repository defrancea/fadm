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
using System.Globalization;
using System.Reflection;
using Fadm.Utilities;

namespace Fadm.Model
{
    /// <summary>
    /// Represents a dependency.
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// The dependency name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The dependency version.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// The dependency culture.
        /// </summary>
        public CultureInfo Culture { get; private set; }

        /// <summary>
        /// The processor architecture.
        /// </summary>
        public ProcessorArchitecture Architecture { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Dependency"/>.
        /// </summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="verson">The asembly version.</param>
        public Dependency(string name, Version version)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(name, "The assembly name must me specified.");
            Validate.IsNotNull(version, "The version name must me specified.");

            // Initializes
            this.Name = name;
            this.Version = version;
            this.Culture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Dependency"/>.
        /// </summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="verson">The asembly version.</param>
        /// <param name="culture">The assembly culture.</param>
        /// <param name="architecture">The compatible architecture of the assembly.</param>
        public Dependency(string name, Version version, CultureInfo culture, ProcessorArchitecture architecture)
            : this(name, version)
        {
            // Input validation
            Validate.IsNotNull(culture, "The assembly culture must me specified.");

            // Initializes
            this.Culture = culture;
            this.Architecture = architecture;
        }
    }
}