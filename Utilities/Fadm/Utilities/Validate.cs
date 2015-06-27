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

namespace Fadm.Utilities
{
    /// <summary>
    /// Groups validation utilities.
    /// </summary>
    public static class Validate
    {
        /// <summary>
        /// Validates that the received object is not null.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="message">The message to display if the error is thrown.</param>
        public static void IsNotNull(object obj, string message)
        {
            IsTrue(null != obj, message);
        }

        /// <summary>
        /// Validates that the check succedded.
        /// </summary>
        /// <param name="check">The check.</param>
        /// <param name="message">The message to display if the error is thrown.</param>
        public static void IsTrue(bool check, string message)
        {
            // Make sure that the message is set
            if (null == message)
            {
                message = string.Empty;
            }

            // Raise an exception if the check didn't pass
            if (!check)
            {
                throw new ArgumentException(message);
            }
        }
    }
}