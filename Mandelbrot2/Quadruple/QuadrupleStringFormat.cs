/*
    Copyright (c) 2011 Jeff Pasternack.  All rights reserved. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Quadruple
{

    /// <summary>
    /// Determines the format of the string produced by Quad.ToString(QuadrupleStringFormat).
    /// ScientificApproximate is the default.
    /// </summary>
    public enum QuadrupleStringFormat
    {
        /// <summary>
        /// Obtains the quadruple in scientific notation.  Only ~52 bits of significand precision are used to create this string.
        /// </summary>
        ScientificApproximate,

        /// <summary>
        /// Obtains the quadruple in scientific notation with full precision.  This can be very expensive to compute and takes time linear in the value of the exponent.
        /// </summary>
        ScientificExact,

        /// <summary>
        /// Obtains the quadruple in hexadecimal exponential format, consisting of a 64-bit hex integer followed by the binary exponent,
        /// also expressed as a (signed) 64-bit hexadecimal integer.
        /// E.g. ffffffffffffffff*2^-AB3
        /// </summary>
        HexExponential,

        /// <summary>
        /// Obtains the quadruple in decimal exponential format, consisting of a 64-bit decimal integer followed by the 64-bit signed decimal integer exponent.
        /// E.g. 34592233*2^34221
        /// </summary>
        DecimalExponential
    }

}
