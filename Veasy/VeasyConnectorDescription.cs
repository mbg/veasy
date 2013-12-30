/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2013 Michael Gale
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#region References
using System;
#endregion

namespace Veasy
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class VeasyConnectorDescription
    {
        #region Instance Members
        /// <summary>
        /// The user-friendly name of the output.
        /// </summary>
        private String name;
        private Int32 maxConnections;
        private Type acceptedType;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the user-friendly name of the output.
        /// </summary>
        public String Name
        {
            get
            {
                return this.name;
            }
        }
        public Int32 MaxConnections
        {
            get
            {
                return this.maxConnections;
            }
        }
        public Type AcceptedType
        {
            get
            {
                return this.acceptedType;
            }
            set
            {
                this.acceptedType = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyConnectorDescription(
            String name, Int32 maxConnections, Type acceptedType)
        {
            this.name = name;
            this.maxConnections = maxConnections;
            this.acceptedType = acceptedType;
        }
        #endregion
    }
}