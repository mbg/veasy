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
using System.Reflection;
using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using System.ComponentModel;
#endregion

namespace Veasy
{
    /// <summary>
    /// 
    /// </summary>
    public class VeasyControlStructure : VeasyNode
    {
        #region Instance Members
        private Int32 margin = 8;
        private String displayName;
        private List<VeasyConnector> inputs;
        private List<VeasyConnector> modifiers;
        private List<VeasyConnector> outputs;
        private Single initialHeightOffset = 20.0f;
        private Single initialWidthOffset = 30.0f;
        private Single connectorMarginTop = 4.0f;
        #endregion

        #region Properties
        [Browsable(false)]
        public List<VeasyConnector> InputConnectors
        {
            get
            {
                return this.inputs;
            }
        }
        [Browsable(false)]
        public List<VeasyConnector> ModifierConnectors
        {
            get
            {
                return this.modifiers;
            }
        }
        [Browsable(false)]
        public List<VeasyConnector> OutputConnectors
        {
            get
            {
                return this.outputs;
            }
        }
        [Browsable(false)]
        public override VeasyNodeType NodeType
        {
            get 
            {
                return VeasyNodeType.Action;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyControlStructure()
        {
            this.size = this.GetSize();
            this.displayName = this.GetDisplayName();

            this.inputs = new List<VeasyConnector>();
            this.modifiers = new List<VeasyConnector>();
            this.outputs = new List<VeasyConnector>();

            // :: Scan the properties of the object type for inputs,
            // :: modifiers and outputs.
            Single currentInputHeight = this.initialHeightOffset;
            Single currentOutputHeight = this.initialHeightOffset;
            Single currentModifierWidth = this.initialWidthOffset;

            foreach (VeasyConnectorDescription via in this.GetInputs())
            {
                VeasyConnector connector = new VeasyConnector(this);
                connector.DisplayName = via.Name;
                connector.ConnectorType = VeasyConnectorType.Input;
                connector.DataType = via.AcceptedType;
                connector.MaxConnections = via.MaxConnections;

                connector.Position = new Point2F(
                    0.0f,
                    currentInputHeight);

                this.inputs.Add(connector);

                currentInputHeight += 12.0f;
            }

            foreach (VeasyConnectorDescription vma in this.GetModifiers())
            {
                VeasyConnector connector = new VeasyConnector(this);
                connector.DisplayName = vma.Name;
                connector.ConnectorType = VeasyConnectorType.Modifier;
                connector.DataType = vma.AcceptedType;
                connector.MaxConnections = vma.MaxConnections;

                connector.Position = new Point2F(
                     currentModifierWidth,
                     this.size.Height - this.margin);

                this.modifiers.Add(connector);

                currentModifierWidth += 30.0f;
            }

            foreach (VeasyConnectorDescription voa in this.GetOutputs())
            {
                VeasyConnector connector = new VeasyConnector(this);
                connector.DisplayName = voa.Name;
                connector.ConnectorType = VeasyConnectorType.Output;
                connector.DataType = voa.AcceptedType;
                connector.MaxConnections = voa.MaxConnections;

                connector.Position = new Point2F(
                    this.margin + (this.size.Width - (2 * margin)),
                    currentOutputHeight);

                this.outputs.Add(connector);

                currentOutputHeight += 12.0f;
            }
        }
        #endregion

        protected virtual String GetDisplayName()
        {
            return "<Unknown Action>";
        }

        protected virtual SizeF GetSize()
        {
            return new SizeF(200.0f, 100.0f);
        }

        protected virtual VeasyConnectorDescription[] GetInputs()
        {
            return new VeasyConnectorDescription[0];
        }

        protected virtual VeasyConnectorDescription[] GetModifiers()
        {
            return new VeasyConnectorDescription[0];
        }

        protected virtual VeasyConnectorDescription[] GetOutputs()
        {
            return new VeasyConnectorDescription[0];
        }

        #region Render
        /// <summary>
        /// Renders the control structure and all of its children.
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="offset"></param>
        public override void Render(RenderTarget renderTarget, Point2F offset, Single zoom)
        {
            Point2F renderPosition = new Point2F(
                offset.X + this.position.X,
                offset.Y + this.position.Y);
            Single bodyWidth = this.size.Width - (2 * this.margin);

            RectF bodyRect = new RectF(
                (offset.X + this.position.X + margin) * zoom,
                (offset.Y + this.position.Y) * zoom,
                (offset.X + this.position.X + margin + (this.size.Width - (margin * 2))) * zoom,
                (offset.Y + this.position.Y + (this.size.Height - 8)) * zoom);

            // ::
            renderTarget.FillRectangle(
                bodyRect,
                renderTarget.CreateSolidColorBrush(new ColorF(.4f, .4f, .4f, 1.0f)));

            // ::
            ColorF borderColour = new ColorF(0.0f, 0.0f, 0.0f, 1.0f);

            if(this.selected)
                borderColour = new ColorF(1.0f, 1.0f, 0.0f, 1.0f);

            renderTarget.DrawRectangle(
                bodyRect,
                renderTarget.CreateSolidColorBrush(borderColour),
                1.0f);

            // ::
            renderTarget.DrawTextLayout(
                new Point2F(
                    (offset.X + this.position.X + margin) * zoom,
                    (offset.Y + this.position.Y + 2.0f) * zoom),
                this.parent.CreateHeaderTextFormat(
                    this.displayName,
                    (this.size.Width - (2 * margin)) * zoom,
                    16.0f * zoom),
                renderTarget.CreateSolidColorBrush(new ColorF(1.0f, 1.0f, 1.0f, 1.0f)));

            // :: Lastly, we want to render all connectors.
            foreach (VeasyConnector connector in this.inputs)
            {
                RectF connectorRect = new RectF(
                    renderPosition.X * zoom,
                    (renderPosition.Y + connector.Position.Y + connectorMarginTop) * zoom,
                    (renderPosition.X + margin) * zoom,
                    (renderPosition.Y + connector.Position.Y + connectorMarginTop + 8.0f) * zoom);

                renderTarget.FillRectangle(
                    connectorRect,
                    renderTarget.CreateSolidColorBrush(this.GetColourForType(connector.DataType, connector.Highlighted || this.selected)));

                renderTarget.DrawTextLayout(
                    new Point2F(
                        (renderPosition.X + margin + 5.0f) * zoom,
                        (renderPosition.Y + connector.Position.Y) * zoom),
                    this.parent.CreateSmallTextFormat(connector.DisplayName, TextAlignment.Leading),
                    renderTarget.CreateSolidColorBrush(new ColorF(1.0f, 1.0f, 1.0f, 1.0f)));
            }

            foreach (VeasyConnector connector in this.outputs)
            {
                RectF connectorRect = new RectF(
                    (renderPosition.X + connector.Position.X) * zoom,
                    (renderPosition.Y + connector.Position.Y + connectorMarginTop) * zoom,
                    (renderPosition.X + connector.Position.X + 8.0f) * zoom,
                    (renderPosition.Y + connector.Position.Y + connectorMarginTop + 8.0f) * zoom);

                renderTarget.FillRectangle(
                    connectorRect,
                    renderTarget.CreateSolidColorBrush(this.GetColourForType(connector.DataType, connector.Highlighted || this.selected)));

                renderTarget.DrawTextLayout(
                    new Point2F(
                        (renderPosition.X + margin + (bodyWidth - 45.0f)) * zoom,
                        (renderPosition.Y + connector.Position.Y )* zoom),
                        this.parent.CreateSmallTextFormat(connector.DisplayName, TextAlignment.Trailing),
                        renderTarget.CreateSolidColorBrush(new ColorF(1.0f, 1.0f, 1.0f, 1.0f)));
            }

            foreach (VeasyConnector connector in this.modifiers)
            {
                RectF connectorRect = new RectF(
                    (renderPosition.X + connector.Position.X) * zoom,
                    (renderPosition.Y + connector.Position.Y) * zoom,
                    (renderPosition.X + connector.Position.X + 8.0f) * zoom,
                    (renderPosition.Y + connector.Position.Y + 8.0f) * zoom);

                renderTarget.FillRectangle(
                    connectorRect,
                    renderTarget.CreateSolidColorBrush(this.GetColourForType(connector.DataType, connector.Highlighted || this.selected)));

                renderTarget.DrawTextLayout(
                    new Point2F(
                        (renderPosition.X + connector.Position.X - 20.0f) * zoom,
                        (renderPosition.Y + connector.Position.Y - 15.0f) * zoom),
                        this.parent.CreateSmallTextFormat(connector.DisplayName, TextAlignment.Center),
                        renderTarget.CreateSolidColorBrush(new ColorF(1.0f, 1.0f, 1.0f, 1.0f)));
            }
        }
        #endregion

        #region RenderConnections
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="offset"></param>
        public override void RenderConnections(RenderTarget renderTarget, Point2F offset, Single zoom)
        {
            foreach (VeasyConnector connector in this.inputs)
                this.RenderConnectionsForConnector(connector, renderTarget, offset, zoom);
            foreach (VeasyConnector connector in this.modifiers)
                this.RenderConnectionsForConnector(connector, renderTarget, offset, zoom);
            foreach (VeasyConnector connector in this.outputs)
                this.RenderConnectionsForConnector(connector, renderTarget, offset, zoom);
        }
        #endregion

        #region RenderConnectionsForConnector
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        private void RenderConnectionsForConnector(
            VeasyConnector connector, RenderTarget target, Point2F offset, Single zoom)
        {
            foreach (VeasyConnection connection in connector.Connections)
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

                Point2F p1 = new Point2F(
                        (offset.X + connection.Start.InheritedPosition.X + 4.0f) * zoom,
                        (offset.Y + connection.Start.InheritedPosition.Y + 4.0f) * zoom);
                Point2F p2 = new Point2F(
                        (offset.X + connection.End.InheritedPosition.X + 4.0f) * zoom,
                        (offset.Y + connection.End.InheritedPosition.Y + 4.0f) * zoom);

                this.RenderConnection(target, p1, p2, connector.DataType);

                // :: Mark the connection as rendered so we don't render
                // :: it twice. This will later be reset by the Veasy control.
                connection.Rendered = true;
            }
        }
        #endregion

        #region GetConnectorAt
        /// <summary>
        /// Gets the connector at the specified location or null if there
        /// is no connector at the specified location.
        /// </summary>
        /// <param name="point">The location to look at.</param>
        /// <returns></returns>
        public override VeasyConnector GetConnectorAt(Point2F point)
        {
            if (point.X >= 0 && point.X <= this.margin)
            {
                foreach (VeasyConnector connector in this.inputs)
                {
                    if (point.Y >= connector.Position.Y &&
                        point.Y <= connector.Position.Y + this.margin + connectorMarginTop)
                        return connector;
                }
            }
            else if (point.X >= this.margin + (this.size.Width - (2 * this.margin)) &&
                point.X <= this.size.Width)
            {
                foreach (VeasyConnector connector in this.outputs)
                {
                    if (point.Y >= connector.Position.Y &&
                        point.Y <= connector.Position.Y + this.margin + connectorMarginTop)
                        return connector;
                }
            }
            else if (point.Y >= this.size.Height - this.margin &&
                point.Y <= this.size.Height)
            {
                foreach (VeasyConnector connector in this.modifiers)
                {
                    if (point.X >= connector.Position.X &&
                        point.X <= connector.Position.X + this.margin + connectorMarginTop)
                        return connector;
                }
            }

            return null;
        }
        #endregion

        public override void MarkAllAsNotRendered()
        {
            foreach (VeasyConnector connector in this.outputs)
                connector.MarkConnectionsAsNotRendered();
            foreach (VeasyConnector connector in this.modifiers)
                connector.MarkConnectionsAsNotRendered();
            foreach (VeasyConnector connector in this.inputs)
                connector.MarkConnectionsAsNotRendered();
        }

        public override void DestroyConnections()
        {
            foreach (VeasyConnector c in this.inputs)
                c.DestroyConnections();
            foreach (VeasyConnector c in this.modifiers)
                c.DestroyConnections();
            foreach (VeasyConnector c in this.outputs)
                c.DestroyConnections();
        }

        #region GetOutputConnections
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public VeasyConnection[] GetOutputConnections(String field)
        {
            foreach (VeasyConnector c in this.outputs)
            {
                if (c.DisplayName.Equals(field))
                    return c.Connections.ToArray();
            }

            return new VeasyConnection[0];
        }
        #endregion

        #region GetModifierConnections
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public VeasyConnection[] GetModifierConnections()
        {
            List<VeasyConnection> connections = new List<VeasyConnection>();

            foreach (VeasyConnector c in this.modifiers)
            {
                foreach (VeasyConnection con in c.Connections)
                    connections.Add(con);
            }

            return connections.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public VeasyConnection[] GetModifierConnections(String field)
        {
            foreach (VeasyConnector c in this.modifiers)
            {
                if (c.DisplayName.Equals(field))
                    return c.Connections.ToArray();
            }

            return null;
        }
        #endregion

        public override List<VeasyConnection> GetAllConnections()
        {
            List<VeasyConnection> result = new List<VeasyConnection>();

            foreach (VeasyConnector c in this.inputs)
            {
                foreach (VeasyConnection con in c.Connections)
                    result.Add(con);
            }
            foreach (VeasyConnector c in this.modifiers)
            {
                foreach (VeasyConnection con in c.Connections)
                    result.Add(con);
            }
            foreach (VeasyConnector c in this.outputs)
            {
                foreach (VeasyConnection con in c.Connections)
                    result.Add(con);
            }

            return result;
        }

        public VeasyConnector GetInputConnector(String name)
        {
            foreach (VeasyConnector connector in this.inputs)
            {
                if (connector.DisplayName == name)
                    return connector;
            }

            return null;
        }

        public VeasyConnector GetModifierConnector(String name)
        {
            foreach (VeasyConnector connector in this.modifiers)
            {
                if (connector.DisplayName == name)
                    return connector;
            }

            return null;
        }

        public VeasyConnector GetOutputConnector(String name)
        {
            foreach (VeasyConnector connector in this.outputs)
            {
                if (connector.DisplayName == name)
                    return connector;
            }

            return null;
        }

        #region GetName
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connector"></param>
        /// <returns></returns>
        public override string GetName(VeasyConnector connector)
        {
            return String.Format(
                "{0} ({1})",
                this.displayName,
                connector.DisplayName);
        }
        #endregion

        #region IsOrphan
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Boolean IsOrphan()
        {
            Boolean isOrphan = true;

            foreach (VeasyConnector inc in this.inputs)
            {
                if (inc.Connections.Count > 0)
                {
                    isOrphan = false;
                    break;
                }
            }

            return isOrphan;
        }
        #endregion
    }
}