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
    public sealed class VeasyConnection
    {
        #region Instance Members
        private VeasyConnector start;
        private VeasyConnector end;
        private Boolean rendered = false;
        private Boolean highlight = false;
        #endregion

        #region Properties
        public VeasyConnector Start
        {
            get
            {
                return this.start;
            }
        }
        public VeasyConnector End
        {
            get
            {
                return this.end;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this connection needs
        /// to be rendered by a Veasy control.
        /// </summary>
        public Boolean Rendered
        {
            get
            {
                return this.rendered;
            }
            set
            {
                this.rendered = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Boolean Highlight
        {
            get
            {
                return this.highlight;
            }
            set
            {
                this.highlight = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyConnection(VeasyConnector start, VeasyConnector end)
        {
            this.start = start;
            this.end = end;
        }
        #endregion

        public void Destroy()
        {
            this.start.Connections.Remove(this);
            this.end.Connections.Remove(this);
        }

        public String GetName(VeasyNode origin)
        {
            if (this.start.Parent == origin)
            {
                return this.end.Parent.GetName(this.end);
            }
            else
            {
                return this.start.Parent.GetName(this.start);
            }
        }
    }
}