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
using System.Linq;
using System.Windows;
using System.Windows.Forms;

using Microsoft;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.DirectX;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using Microsoft.WindowsAPICodePack.DirectX.Graphics;
using System.ComponentModel;
#endregion

namespace Veasy
{
    public delegate void VeasySelectionChangeDelegate(VeasyControl sender, VeasyNode selectedObject);
    public delegate void VeasyOpenContextMenuDelegate(VeasyControl sender, Int32 x, Int32 y); 

    /// <summary>
    /// 
    /// </summary>
    public class VeasyControl : Control, IComponent
    {
        #region Instance Members
        private D2DFactory factory;
        private DWriteFactory directWriteFactory;
        private HwndRenderTarget renderTarget;
        private ColorF backgroundColour;
        private ColorF workspaceColour;
        private ColorF borderColour;
        private SolidColorBrush workspaceBrush;
        private SolidColorBrush borderBrush;
        private Int32 margin = 10;
        private Int32 padding = 8;
        private SizeF workspaceSize = new SizeF(4000, 2000);
        private Point2F offset = new Point2F(0, 0);
        private Point2F lastKnownCursorPosition;
        private Boolean hasMoved;
        private Boolean isMoving = false;
        private Boolean isConnecting = false;
        private List<VeasyNode> objects;
        private VeasyNode selectedObject = null;
        private VeasyConnector selectedConnector = null;
        private Point2F selectionOffset;
        private Boolean isCtrlDown = false;
        private Point2F targetOffset;
        private VeasyConnector highlightedConnector = null;
        private Single zoom = 1.0f;
        private VeasyNamingManager namingManager = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the size of the workspace.
        /// </summary>
        public SizeF WorkspaceSize
        {
            get
            {
                return this.workspaceSize;
            }
            set
            {
                this.workspaceSize = value;
            }
        }

        public VeasyNode SelectedObject
        {
            get
            {
                return this.selectedObject;
            }
        }

        public List<VeasyNode> Nodes
        {
            get
            {
                return this.objects;
            }
        }

        protected virtual RenderTargetProperties RenderProperties
        {
            get
            {
                RenderTargetProperties props =
                    new RenderTargetProperties();

                props.PixelFormat = new PixelFormat(
                    Format.B8G8R8A8UNorm,
                    AlphaMode.Ignore);
                props.Usage = RenderTargetUsages.None;
                props.RenderTargetType = RenderTargetType.Hardware;

                return props;
            }
        }

        public VeasyNamingManager NamingManager
        {
            get
            {
                return this.namingManager;
            }
        }
        #endregion

        #region Events
        public event VeasySelectionChangeDelegate OnSelectionChanged = delegate { };
        public event VeasyOpenContextMenuDelegate OnContextMenu = delegate { };
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyControl()
        {
            this.SetStyle(ControlStyles.Selectable, true);
            this.UpdateStyles();

            this.backgroundColour = new ColorF(0.15f, 0.15f, 0.15f, 1.0f);
            this.workspaceColour = new ColorF(0.60f, 0.60f, 0.60f, 1.0f);
            this.borderColour = new ColorF(0.0f, 0.0f, 0.0f, 1.0f);

            this.objects = new List<VeasyNode>();
            this.namingManager = new VeasyNamingManager();
        }
        #endregion

        #region SetWorkspaceSize
        /// <summary>
        /// Changes the size of the workspace area.
        /// </summary>
        /// <param name="width">The width of the workspace.</param>
        /// <param name="height">The height of the workspace.</param>
        public void SetWorkspaceSize(Single width, Single height)
        {
            this.workspaceSize = new SizeF(width, height);
        }
        #endregion

        #region InitialiseDirect2D
        /// <summary>
        /// Initialises the Direct2D factory.
        /// </summary>
        private void InitialiseDirect2D()
        {
            // :: We don't need to do this if the factory is already
            // :: initialised.
            if (this.factory != null)
                return;

            // :: Initialise the Direct2D factory and set it to MultiThreaded,
            // :: just in case.
            this.factory = D2DFactory.CreateFactory(
                D2DFactoryType.Multithreaded);

            this.directWriteFactory = DWriteFactory.CreateFactory();
        }
        #endregion

        #region InitialiseResources
        /// <summary>
        /// Initialises the resources for the Direct2D device.
        /// </summary>
        private void InitialiseResources()
        {
            // :: We don't need to do this if the resources have already
            // :: been initialised.
            if (this.renderTarget != null)
                return;

            // :: Calculate the size of the control.
            SizeU size = new SizeU(
                (UInt32)this.Width,
                (UInt32)this.Height);

            // :: Configure the render target.
            HwndRenderTargetProperties props = new HwndRenderTargetProperties(
                this.Handle, size, Microsoft.WindowsAPICodePack.DirectX.Direct2D1.PresentOptions.RetainContents);

            // :: Create the render target for the Direct2D device.
            this.renderTarget = this.factory.CreateHwndRenderTarget(
                this.RenderProperties, props);

            // :: Initialise some brushes.
            this.workspaceBrush = this.renderTarget.CreateSolidColorBrush(this.workspaceColour);
            this.borderBrush = this.renderTarget.CreateSolidColorBrush(this.borderColour);
        }
        #endregion

        #region MarkAllAsRendered
        /// <summary>
        /// Marks all connections as not rendered.
        /// </summary>
        private void MarkAllAsNotRendered()
        {
            foreach (VeasyNode obj in this.objects)
                obj.MarkAllAsNotRendered();
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the Veasy graph.
        /// </summary>
        public void Render()
        {
            // :: Initialise the Direct2D factory and all of its
            // :: resources. This part will be skipped if the
            // :: resources are already initialised.
            this.InitialiseDirect2D();
            this.InitialiseResources();

            if (this.renderTarget.IsOccluded)
                return;

            // :: Mark all connections as not rendered.
            this.MarkAllAsNotRendered();

            // :: Begin rendering and clear the render target with
            // :: the background colour.
            this.renderTarget.BeginDraw();
            this.renderTarget.Clear(this.backgroundColour);

            // :: Render the workspace area.
            this.RenderWorkspace();

            // :: Calculate the offset at which controls may be
            // :: drawn.
            Point2F renderOffset = new Point2F(
                ((margin + 1 + padding) - this.offset.X) * this.zoom,
                ((margin + 1 + padding) - this.offset.Y) * this.zoom);

            // ::
            if (this.isConnecting)
            {
                this.renderTarget.DrawLine(
                    new Point2F(
                        this.targetOffset.X * this.zoom,    // :: Position of the cursor on the local x axis
                        this.targetOffset.Y * this.zoom),   // :: Position of the cursor on the local y axis
                    new Point2F(
                            (renderOffset.X + this.selectedConnector.InheritedPosition.X + 4.0f) * this.zoom,
                            (renderOffset.Y + this.selectedConnector.InheritedPosition.Y + 4.0f) * this.zoom),
                            this.renderTarget.CreateSolidColorBrush(this.selectedObject.GetColourForType(this.selectedConnector.DataType, true)),
                            1.0f);
            }

            foreach (VeasyNode obj in this.objects)
                obj.RenderConnections(this.renderTarget, renderOffset, this.zoom);

            // :: Render the Veasy items on the workspace.
            foreach (VeasyNode obj in this.objects)
            {
                obj.Render(this.renderTarget, renderOffset, this.zoom);
            }


            // :: Finish rendering.
            this.renderTarget.EndDraw();
        }
        #endregion

        #region RenderWorkspace
        /// <summary>
        /// Renders the workspace area.
        /// </summary>
        private void RenderWorkspace()
        {
            // :: Calculate the coordinates for the workspace. Note that
            // :: these coordinates are screen coordinates rather than
            // :: client coordinates.
            RectF workspaceRect = new RectF(
                this.margin - this.offset.X,
                this.margin - this.offset.Y,
                (this.margin + this.workspaceSize.Width) - this.offset.X,
                (this.margin + this.workspaceSize.Height) - this.offset.Y);

            // :: Draw the background of the workspace area.
            this.renderTarget.FillRectangle(
                workspaceRect, this.workspaceBrush);

            // :: Draw a small border around the workspace area.
            this.renderTarget.DrawRectangle(
                workspaceRect, this.borderBrush, 1.0f);
        }
        #endregion

        #region OnPaint
        /// <summary>
        /// Triggered when the control needs to be redrawn.
        /// </summary>
        /// <param name="e">Unused.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.DesignMode)
            {
                // :: Do not render the control if we are
                // :: in design mode.
            }
            else
            {
                // :: Render the Veasy graph.
                this.Render();
            }
        }
        #endregion

        #region ClearSelection
        /// <summary>
        /// Clears the selection.
        /// </summary>
        private void ClearSelection()
        {
            if(this.selectedObject != null)
                this.selectedObject.Selected = false;

            this.selectedConnector = null;
            this.selectedObject = null;

            this.OnSelectionChanged(this, null);
        }
        #endregion

        #region OnMouseDown
        /// <summary>
        /// Triggered when the user presses a mouse button while the
        /// cursor is above the control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // :: Set the focus to this control.
            this.Focus();

            // :: 
            if (e.Button == MouseButtons.Right)
            {
                this.lastKnownCursorPosition = new Point2F(
                    Cursor.Position.X, Cursor.Position.Y);
                this.hasMoved = false;

                Cursor.Hide();

                this.isMoving = true;
            }
            else if (e.Button == MouseButtons.Left)
            {
                this.ClearSelection();

                VeasyNode obj = this.GetObjectAtPosition(
                    new Point2F(e.X, e.Y), out this.selectionOffset);

                if (obj != null)
                {
                    obj.Selected = true;

                    this.selectedObject = obj;

                    // todo: find if there is a connector at the position.
                    VeasyConnector connector = obj.GetConnectorAt(this.selectionOffset);

                    if (connector != null)
                    {
                        this.selectedConnector = connector;
                    }

                    this.OnSelectionChanged(this, obj);


                }

                // :: Render the graph to show updates.
                this.Render();
            }
        }
        #endregion

        #region OnMouseMove
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;

            if (this.highlightedConnector != null)
            {
                this.highlightedConnector.Highlighted = false;
                this.highlightedConnector = null;
            }

            if (this.isMoving)
            {
                // :: Calculate the relative mouse movement.
                Int32 relX = (Int32)(this.lastKnownCursorPosition.X - Cursor.Position.X);
                Int32 relY = (Int32)(this.lastKnownCursorPosition.Y - Cursor.Position.Y);

                // :: Change the offset by the relative mouse movement.
                this.offset.X += relX;
                this.offset.Y += relY;

                // :: Verify that the movement doesn't exceed the boundaries.
                if (this.offset.X < 0)
                    this.offset.X = 0;
                if (this.offset.Y < 0)
                    this.offset.Y = 0;
                // todo: far limits

                // :: Update the last known cursor position.
                Point2F newPosition = new Point2F(
                    Cursor.Position.X, Cursor.Position.Y);

                if (newPosition != this.lastKnownCursorPosition)
                    this.hasMoved = true;

                this.lastKnownCursorPosition = newPosition;

                // :: Render the graph.
                this.Render();
            }
            else if (this.selectedObject != null && e.Button == MouseButtons.Left && isCtrlDown) 
            {
                this.isConnecting = false;

                this.Cursor = Cursors.SizeAll;

                // :: Calculate the offset at which controls may be
                // :: drawn.
                Point2F renderOffset = new Point2F(
                    (margin + 1 + padding) - this.offset.X,
                    (margin + 1 + padding) - this.offset.Y);

                this.selectedObject.Position = new Point2F(
                    e.X + offset.X  - this.selectionOffset.X ,
                    e.Y + offset.Y - this.selectionOffset.Y );

                this.Render();
            }
            else if (this.selectedConnector != null && e.Button == MouseButtons.Left)
            {
                if (this.selectedConnector.ConnectorType == VeasyConnectorType.Output ||
                    this.selectedConnector.ConnectorType == VeasyConnectorType.Modifier)
                {
                    this.isConnecting = true;
                    this.targetOffset = new Point2F(e.X, e.Y);

                    Point2F localOffset;

                    // :: Try and get the Veasy object at the current
                    // :: client position of the cursor. Store the offset
                    // :: to the top left corner of the object in a variable.
                    VeasyNode obj = this.GetObjectAtPosition(
                        new Point2F(e.X, e.Y), out localOffset);

                    // :: Only proceed if an object was found at the target
                    // :: location.
                    if (obj != null)
                    {
                        // :: Try and get a connector at the target location.
                        VeasyConnector connector = obj.GetConnectorAt(localOffset);

                        // :: If there is a connector at the target location,
                        // :: we can try to connect it with the selected connector.
                        if (connector != null && connector != this.selectedConnector)
                        {
                            this.highlightedConnector = connector;
                            connector.Highlighted = true;
                        }
                    }

                    this.Render();
                }
            }
        }
        #endregion

        #region OnMouseUp
        /// <summary>
        /// Triggered when a mouse button has been released while the cursor
        /// is above this control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // :: If a connector is highlighted, change that.
            if (this.highlightedConnector != null)
            {
                this.highlightedConnector.Highlighted = false;
                this.highlightedConnector = null;
            }

            if (e.Button == MouseButtons.Left)
            {
                // :: If we are currently in the process of creating
                // :: a new connection, verify whether there is a
                // :: connector at the position at which the mouse
                // :: button has been released.
                if (this.isConnecting)
                {
                    Point2F localOffset;

                    // :: Try and get the Veasy object at the current
                    // :: client position of the cursor. Store the offset
                    // :: to the top left corner of the object in a variable.
                    VeasyNode obj = this.GetObjectAtPosition(
                        new Point2F(e.X, e.Y), out localOffset);

                    // :: Only proceed if an object was found at the target
                    // :: location.
                    if (obj != null)
                    {
                        // :: Try and get a connector at the target location.
                        VeasyConnector connector = obj.GetConnectorAt(localOffset);

                        // :: If there is a connector at the target location,
                        // :: we can try to connect it with the selected connector.
                        if (connector != null)
                            this.Connect(this.selectedConnector, connector);
                    }
                }

                // :: Stop connecting and render the graph to either
                // :: remove the line that was used for the connecting
                // :: process or to show new connections that have been
                // :: established.
                this.isConnecting = false;
                this.Render();
            }

            if (this.isMoving && e.Button == MouseButtons.Right)
            {
                Cursor.Show();
                this.isMoving = false;

                if (!this.hasMoved)
                    this.OnContextMenu(this, e.X, e.Y);
            }
        }
        #endregion

        #region OnMouseWheel
        /// <summary>
        /// Triggered when the mouse wheel is rotated in either direction.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Single change = e.Delta * 0.001f;
            Single newValue = this.zoom + change;
            
            //if(newValue > 0.1f && newValue < 2.0f)
                //this.zoom += change;

            //this.Render();
        }
        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                this.isCtrlDown = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                this.isCtrlDown = false;
            if (e.KeyCode == Keys.Delete && this.selectedObject != null)
                this.RemoveItem(this.selectedObject);
        }

        #region OnResize
        /// <summary>
        /// Triggered when the control has been resized.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            // :: If the render target has not yet been initialised,
            // :: there is no work here to do.
            if (this.renderTarget == null)
                return;

            // :: Resize the render target with the updated control size.
            this.renderTarget.Resize(new SizeU(
                (UInt32)this.Width,
                (UInt32)this.Height));
        }
        #endregion

        #region GetObjectAtPosition
        /// <summary>
        /// Gets the Veasy object at the specified client position. 
        /// </summary>
        /// <param name="position">The client position to look at.</param>
        /// <param name="offset">
        /// Returns the relative offset to the upper left corner of the object.</param>
        /// <returns>
        /// Returns the Veasy object at the specified location or null if there is 
        /// no object at that location.</returns>
        private VeasyNode GetObjectAtPosition(Point2F position, out Point2F offset)
        {
            // :: Calculate the offset at which controls may be
            // :: drawn.
            Point2F renderOffset = new Point2F(
                ((margin + 1 + padding) - this.offset.X),
                ((margin + 1 + padding) - this.offset.Y));

            // todo: reverse
            for (Int32 i = this.objects.Count - 1; i >= 0; i--)
            {
                VeasyNode obj = this.objects[i];

                if (position.X >= (obj.Position.X + renderOffset.X) * this.zoom &&
                    position.Y >= (obj.Position.Y + renderOffset.Y) * this.zoom &&
                    position.X <= ((obj.Position.X + obj.Size.Width) + renderOffset.X) * this.zoom &&
                    position.Y <= ((obj.Position.Y + obj.Size.Height) + renderOffset.Y) * this.zoom)
                {
                    offset = new Point2F(
                        position.X - (obj.Position.X + renderOffset.X),
                        position.Y - (obj.Position.Y + renderOffset.Y));

                    return obj;
                }
            }

            offset = new Point2F();

            return null;
        }
        #endregion

        #region Connect
        /// <summary>
        /// Attempts to establish a connection between two connectors. Note that
        /// this method does not necessarily do this if the two connectors are
        /// incompatible.
        /// </summary>
        /// <param name="a">The first connector.</param>
        /// <param name="b">The second connector.</param>
        private void Connect(VeasyConnector a, VeasyConnector b)
        {
            // :: We cannot connect connectors of the same connector
            // :: type (e.g. an output can't be connected to another
            // :: output connector).
            if (a.ConnectorType == b.ConnectorType)
                return;

            if (a.ConnectorType == VeasyConnectorType.Modifier &&
                b.ConnectorType != VeasyConnectorType.Variable)
                return;

            // :: The two connectors must have the same type or a type
            // :: that has a common base class.
            //if (a.DataType != b.DataType)
              //  return;
            if (!a.DataType.IsArray)
            {
                if (!b.Parent.GetType().IsSubclassOf(a.DataType) && b.Parent.GetType() != a.DataType)
                    return;
            }
            else
            {
                bool isSublcass = b.Parent.GetType().IsSubclassOf(a.DataType.GetElementType());
                bool isSame = b.Parent.GetType() == a.DataType.GetElementType();

                if (!isSublcass && !isSame)
                    return;
            }
            if (!b.DataType.IsArray)
            {
                if (!a.Parent.GetType().IsSubclassOf(b.DataType) && a.Parent.GetType() != b.DataType)
                    return;
            }
            else
            {
                bool isSublcass = a.Parent.GetType().IsSubclassOf(b.DataType.GetElementType());
                bool isSame = a.Parent.GetType() == b.DataType.GetElementType();

                if (!isSublcass && !isSame)
                    return;
            }

            // todo: more validation

            // :: Verify that the connectors allow more connections to be
            // :: established.
            if (a.MaxConnections != -1)
            {
                if (a.Connections.Count >= a.MaxConnections)
                    return;
            }
            if (b.MaxConnections != -1)
            {
                if (b.Connections.Count >= b.MaxConnections)
                    return;
            }

            // :: Establish the connection between the two connectors.
            VeasyConnection connection = new VeasyConnection(a, b);

            // :: Add the connection to both connectors.
            a.Connections.Add(connection);
            b.Connections.Add(connection);
        }
        #endregion

        #region AddItem
        /// <summary>
        /// Adds a new object at the default position.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        public void AddItem(VeasyNode obj)
        {
            this.AddItem(obj,
                (((this.Width / this.zoom) / 2) - ((obj.Size.Width) / 2)),
                (((this.Height / this.zoom) / 2) - ((obj.Size.Height) / 2)));
        }
        /// <summary>
        /// Adds a new object at the specified screen position.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <param name="x">
        /// The x coordinate of the position where the object should be added, relative to the left of 
        /// the current view.
        /// </param>
        /// <param name="y">
        /// The y coordinate of the position where the object should be added, relative to the top of 
        /// the current view.
        /// </param>
        public void AddItem(VeasyNode obj, float x, float y)
        {
            obj.Parent = this;
            obj.Position = new Point2F(
                this.offset.X + x, this.offset.Y + y);

            this.objects.Add(obj);
            obj.NodeAdded();

            this.Render();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="usePosition"></param>
        public void AddItem(VeasyNode obj, Boolean usePosition)
        {
            if (!usePosition)
                this.AddItem(obj);
            else
            {
                obj.Parent = this;

                this.objects.Add(obj);
                this.Render();
            }
        }
        #endregion

        #region RemoveItem
        /// <summary>
        /// Removes an item from the graph and destroys all connections.
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveItem(VeasyNode obj)
        {
            if (null == obj)
                return;

            obj.DestroyConnections();

            this.objects.Remove(obj);

            // :: We need to render the control in order to show the
            // :: changes to the graph.
            this.Render();
        }
        #endregion

        internal TextLayout CreateHeaderTextFormat(
            String headerText, Single width, Single height)
        {
            TextFormat format = this.directWriteFactory.CreateTextFormat(
                "Verdana", 12.0f * this.zoom, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal);

            TextLayout layout = this.directWriteFactory.CreateTextLayout(
                headerText, format, width, height);

            layout.ParagraphAlignment = ParagraphAlignment.Center;
            layout.TextAlignment = TextAlignment.Center;
            layout.Trimming = new Trimming(TrimmingGranularity.Character, '.', 3);

            return layout;
        }

        internal TextLayout CreateSmallTextFormat(String text, TextAlignment alignment)
        {
            TextFormat format = this.directWriteFactory.CreateTextFormat(
                "Verdana", 10.0f * this.zoom, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal);

            TextLayout layout = this.directWriteFactory.CreateTextLayout(
                text, format, 40.0f * this.zoom, 12.0f * this.zoom);

            layout.TextAlignment = alignment;
            layout.Trimming = new Trimming(TrimmingGranularity.Character, 1, 3);

            return layout;
        }

        #region IsObjectAtPosition
        /// <summary>
        /// Gets a value indicating whether an object is located at the specified position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Returns true if an object is located at the specified position.</returns>
        public Boolean IsObjectAtPosition(Int32 x, Int32 y)
        {
            Point2F offset;

            return this.GetObjectAtPosition(new Point2F(x, y), out offset) != null;
        }
        #endregion

        #region HasOrphans
        /// <summary>
        /// Gets a value indicating whether the graph contains any orphaned nodes.
        /// </summary>
        /// <param name="includeFirst">
        /// If set to true, the first node in the list will be tested. By default the first node is NOT tested 
        /// because it is supposed to be the root node and is therefore expected to be orphaned.</param>
        /// <returns></returns>
        public Boolean HasOrphans(Boolean includeFirst)
        {
            Int32 startIndex = 1;

            if (includeFirst)
                startIndex = 0;

            for (Int32 i = startIndex; i < this.objects.Count; i++)
            {
                if (this.objects[i].IsOrphan())
                    return true;
            }

            return false;
        }
        #endregion

        #region GetOrphans
        /// <summary>
        /// Gets a list of all orphaned nodes in the graph.
        /// </summary>
        /// <param name="includeFirst">
        /// Set this value to true if the first node should be counted as orphan if it has no incoming connections.
        /// </param>
        /// <returns></returns>
        public List<VeasyNode> GetOrphans(Boolean includeFirst)
        {
            List<VeasyNode> orphans = new List<VeasyNode>();

            Int32 startIndex = 1;

            if (includeFirst)
                startIndex = 0;

            for (Int32 i = startIndex; i < this.objects.Count; i++)
            {
                if (this.objects[i].IsOrphan())
                    orphans.Add(this.objects[i]);
            }

            return orphans;
        }
        #endregion
    }
}