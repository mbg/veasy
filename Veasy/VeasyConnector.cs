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
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Collections.Generic;
#endregion

namespace Veasy
{
    /// <summary>
    /// 
    /// </summary>
    public class VeasyConnector
    {
        #region Instance Members
        private VeasyNode parent;
        private Type dataType;
        private VeasyConnectorType connectorType;
        private String displayName = String.Empty;
        private Point2F position = new Point2F();
        private List<VeasyConnection> connections;
        private Int32 maxConnections = -1;
        private Boolean highlighted = false;
        private ColorF colourDefault;
        private ColorF colourHighlighted;
        #endregion

        #region Properties
        public VeasyNode Parent
        {
            get
            {
                return this.parent;
            }
        }
        /// <summary>
        /// Gets or sets the display name of the connector.
        /// </summary>
        public String DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = value;
            }
        }
        /// <summary>
        /// Gets or sets the type of this connector.
        /// </summary>
        public VeasyConnectorType ConnectorType
        {
            get
            {
                return this.connectorType;
            }
            set
            {
                this.connectorType = value;
            }
        }
        /// <summary>
        /// Gets or sets the data type of this connector.
        /// </summary>
        public Type DataType
        {
            get
            {
                return this.dataType;
            }
            set
            {
                this.dataType = value;
            }
        }
        /// <summary>
        /// Gets or sets the position of the connector.
        /// </summary>
        public Point2F Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }
        /// <summary>
        /// Gets the position of the connector relative to the global
        /// top left corner.
        /// </summary>
        public Point2F InheritedPosition
        {
            get
            {
                Single offsetX = 0.0f;
                Single offsetY = 0.0f;

                if (this.connectorType == VeasyConnectorType.Modifier)
                    offsetY = 0.0f;
                
                else
                    offsetY = 4.0f;

                return new Point2F(
                    this.parent.Position.X + this.position.X + offsetX,
                    this.parent.Position.Y + this.position.Y + offsetY);
            }
        }
        /// <summary>
        /// Gets a list of connections that this connector has with other
        /// connectors.
        /// </summary>
        public List<VeasyConnection> Connections
        {
            get
            {
                return this.connections;
            }
        }
        public Int32 MaxConnections
        {
            get
            {
                return this.maxConnections;
            }
            set
            {
                this.maxConnections = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the connector should be highlighted.
        /// </summary>
        public Boolean Highlighted
        {
            get
            {
                return this.highlighted;
            }
            set
            {
                this.highlighted = value;
            }
        }

        public ColorF ColourDefault
        {
            get
            {
                return this.colourDefault;
            }
            set
            {
                this.colourDefault = value;
            }
        }

        public ColorF ColourHighlighted
        {
            get
            {
                return this.colourHighlighted;
            }
            set
            {
                this.colourHighlighted = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyConnector(VeasyNode parent)
        {
            this.parent = parent;
            this.connections = new List<VeasyConnection>();
        }
        #endregion

        #region MarkConnectionsAsNotRendered
        /// <summary>
        /// Marks all connections that this connector has as not rendered.
        /// </summary>
        public void MarkConnectionsAsNotRendered()
        {
            foreach (VeasyConnection connection in this.connections)
                connection.Rendered = false;
        }
        #endregion

        public void DestroyConnections()
        {
            // :: We need to convert the list of connections to an array here
            // :: because destroying a connection will modify the list and
            // :: the foreach loop will therefore throw an exception.
            VeasyConnection[] cons = this.connections.ToArray();

            foreach (VeasyConnection c in cons)
                c.Destroy();
        }

        public String GetName()
        {
            return "???";
        }
    }
}