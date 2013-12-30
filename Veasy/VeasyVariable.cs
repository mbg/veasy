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
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
#endregion

namespace Veasy
{
    /// <summary>
    /// 
    /// </summary>
    public class VeasyVariable : VeasyNode
    {
        #region Instance Members
        private ColorF borderColour;
        private VeasyConnector connector;
        private Boolean saved;
        private String name;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public Boolean Saved
        {
            get
            {
                return this.saved;
            }
            set
            {
                this.saved = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public String Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        [Browsable(false)]
        public VeasyConnector Connector
        {
            get
            {
                return this.connector;
            }
        }
        [Browsable(false)]
        public override VeasyNodeType NodeType
        {
            get 
            {
                return VeasyNodeType.Variable; 
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyVariable()
        {
            this.borderColour = this.GetColour(false);
            this.connector = new VeasyConnector(this);
            this.connector.ConnectorType = VeasyConnectorType.Variable;
            this.connector.DataType = typeof(Object);
            this.connector.Position = new Point2F(40.0f, 40.0f);
            this.connector.MaxConnections = -1;
        }
        #endregion

        protected virtual ColorF GetColour(Boolean highlighted)
        {
            if (highlighted)
                return new ColorF(1.0f, 1.0f, 0.0f, 1.0f);
            else
                return new ColorF(0.0f, 0.0f, 0.0f, 1.0f);
        }

        protected virtual String GetValue()
        {
            return "<No Value>";
        }

        #region Render
        /// <summary>
        /// Renders the variable.
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="offset"></param>
        public override void Render(RenderTarget renderTarget, Point2F offset, Single zoom)
        {
            Single halfWidth = this.size.Width / 2;
            Single halfHeight = this.size.Height / 2;

            Ellipse ellipse = new Ellipse(
                new Point2F(
                    (offset.X + this.position.X + halfWidth) * zoom,
                    (offset.Y + this.position.Y + halfHeight) * zoom),
                    halfWidth * zoom,
                    halfHeight * zoom);

            ColorF activeBorderColour = this.borderColour;

            if (this.selected)
                activeBorderColour = this.GetColour(this.selected);

            renderTarget.FillEllipse(
                ellipse,
                renderTarget.CreateSolidColorBrush(new ColorF(.4f, .4f, .4f, 1.0f)));
            renderTarget.DrawEllipse(
                ellipse,
                renderTarget.CreateSolidColorBrush(activeBorderColour),
                1.0f);

            renderTarget.DrawTextLayout(
                new Point2F(
                    (offset.X + this.position.X) * zoom,
                    (offset.Y + this.position.Y) * zoom),
                this.parent.CreateHeaderTextFormat(
                    this.GetValue(),
                    this.size.Width * zoom,
                    this.size.Height * zoom),
                renderTarget.CreateSolidColorBrush(
                    new ColorF(1.0f, 1.0f, 1.0f, 1.0f)));
        }
        #endregion

        public override void DestroyConnections()
        {
            this.connector.DestroyConnections();
        }

        public override void RenderConnections(RenderTarget renderTarget, Point2F offset, Single zoom)
        {
            foreach (VeasyConnection connection in this.connector.Connections)
            {
                // :: Do not render this connection if it has already been
                // :: rendered before.
                if (connection.Rendered && !this.selected)
                    continue;

                VeasyNode start = connection.Start.Parent;
                VeasyNode end = connection.End.Parent;

                if (start != this && start.Selected ||
                    end != this && end.Selected)
                    continue;

                renderTarget.DrawLine(
                    new Point2F(
                        (offset.X + connection.Start.InheritedPosition.X + 4.0f) * zoom,
                        (offset.Y + connection.Start.InheritedPosition.Y + 4.0f) * zoom),
                    new Point2F(
                        (offset.X + connection.End.InheritedPosition.X + 4.0f) * zoom,
                        (offset.Y + connection.End.InheritedPosition.Y + 4.0f) * zoom),
                    renderTarget.CreateSolidColorBrush(this.GetColour(this.selected)),
                    1.0f);

                // :: Mark the connection as rendered so we don't render
                // :: it twice. This will later be reset by the Veasy control.
                connection.Rendered = true;
            }
        }

        #region GetConnectorAt
        /// <summary>
        /// Gets the connector at the specified location or null if there
        /// is no connector at the specified location.
        /// </summary>
        /// <param name="point">The location to look at.</param>
        /// <returns></returns>
        public override VeasyConnector GetConnectorAt(Point2F point)
        {
            return this.connector;
        }
        #endregion

        public override List<VeasyConnection> GetAllConnections()
        {
            return this.connector.Connections;
        }

        public override string GetName(VeasyConnector connector)
        {
            return this.name;
        }

        #region IsOrphan
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsOrphan()
        {
            return this.connector.Connections.Count == 0;
        }
        #endregion
    }
}