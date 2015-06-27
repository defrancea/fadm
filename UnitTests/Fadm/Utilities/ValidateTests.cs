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
using NUnit.Framework;

namespace Fadm.Utilities
{
    /// <summary>
    /// Tests ValidateTests.
    /// </summary>
    [TestFixture]
    public class ValidateTests
    {
        /// <summary>
        /// Tests IsNotNull(object, string) with null value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Foo")]
        public void IsNotNullNull()
        {
            Validate.IsNotNull(null, "Foo");
        }

        /// <summary>
        /// Tests IsNotNull(object, string) with value.
        /// </summary>
        [Test]
        public void IsNotNullNotNull()
        {
            Validate.IsNotNull(new object(), "Foo");
        }

        /// <summary>
        /// Tests IsTrue(bool, string) with false value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Foo")]
        public void IsTrueFalse()
        {
            Validate.IsTrue(false, "Foo");
        }

        /// <summary>
        /// Tests IsTrue(bool, string) with false value.
        /// </summary>
        [Test]
        public void IsTrueTrue()
        {
            Validate.IsTrue(true, "Foo");
        }
    }
}