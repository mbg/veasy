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
using System.ComponentModel;
#endregion

namespace Veasy
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class VeasyNode
    {
        #region Instance Members
        protected VeasyControl parent;
        protected Point2F position;
        protected SizeF size;
        protected Boolean selected = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the parent control of this object.
        /// </summary>
        [Browsable(false)]
        public VeasyControl Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }
        /// <summary>
        /// Gets or sets the position of the object.
        /// </summary>
        [Browsable(false)]
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
        /// Gets or sets the size of this object.
        /// </summary>
        [Browsable(false)]
        public SizeF Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this object
        /// is selected.
        /// </summary>
        [Browsable(false)]
        public Boolean Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
            }
        }
        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        [Browsable(false)]
        public abstract VeasyNodeType NodeType
        {
            get;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of this class.
        /// </summary>
        public VeasyNode()
        {
            this.position = new Point2F(0.0f, 0.0f);
            this.size = new SizeF(80.0f, 80.0f);
        }
        #endregion

        public virtual void NodeAdded()
        {
        }

        #region Render
        /// <summary>
        /// If overriden, this method renders the control.
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="offset"></param>
        public virtual void Render(RenderTarget renderTarget, Point2F offset, Single zoom)
        {

        }
        #endregion

        protected void RenderConnection(RenderTarget target, Point2F p1, Point2F p2, Type type)
        {
            Point2F pm = new Point2F(
                    (p1.X + p2.X) / 2.0f,
                    (p1.Y + p2.Y) / 2.0f);

            PathGeometry p = target.Factory.CreatePathGeometry();
            GeometrySink g = p.Open();

            g.SetFillMode(FillMode.Winding);
            g.BeginFigure(p1, FigureBegin.Hollow);

            BezierSegment b1 = new BezierSegment(p1, new Point2F(pm.X, p1.Y), pm);
            BezierSegment b2 = new BezierSegment(pm, new Point2F(pm.X, p2.Y), p2);

            g.AddBezier(b1);
            g.AddBezier(b2);

            g.EndFigure(FigureEnd.Open);

            g.Close();

            target.DrawGeometry(
                p,
                target.CreateSolidColorBrush(this.GetColourForType(type, this.selected)),
                1.0f);
        }

        public virtual void RenderConnections(RenderTarget renderTarget, Point2F offset, Single zoom)
        {

        }

        #region GetIndicatorColour
        /// <summary>
        /// If overriden, this method returns a colour that indicates the
        /// type of the object.
        /// </summary>
        /// <returns></returns>
        public virtual ColorF GetIndicatorColour()
        {
            return new ColorF(0.0f, 0.0f, 0.0f, 1.0f);
        }
        #endregion

        #region GetConnectorAt
        /// <summary>
        /// If overriden, this method returns the connector at the specified
        /// position or null if there is no connector at the specified location.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual VeasyConnector GetConnectorAt(Point2F point)
        {
            return null;
        }
        #endregion

        #region GetColourForType
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public ColorF GetColourForType(Type baseType)
        {
            return this.GetColourForType(baseType, false);
        }

        public ColorF GetColourForType(Type baseType, Boolean highlighted)
        {
            float strength = highlighted ? 1.0f : 0.8f;

            if (highlighted)
                return new ColorF(1.0f, 1.0f, 0.0f, 1.0f);
            else
                return new ColorF(0.0f, 0.0f, 0.0f, 1.0f);
        }
        #endregion

        #region MarkAllAsRendered
        /// <summary>
        /// If overriden, this method marks all connections that this object
        /// has as not rendered.
        /// </summary>
        public virtual void MarkAllAsNotRendered()
        {
            
        }
        #endregion

        public virtual void DestroyConnections()
        {

        }

        #region GetAllConnections
        /// <summary>
        /// Returns a list of all connections to / from this object.
        /// </summary>
        /// <returns></returns>
        public virtual List<VeasyConnection> GetAllConnections()
        {
            return new List<VeasyConnection>();
        }
        #endregion

        #region GetName
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connector"></param>
        /// <returns></returns>
        public virtual String GetName(VeasyConnector connector)
        {
            return "???";
        }
        #endregion

        #region IsOrphan
        /// <summary>
        /// Gets a value indicating whether this object is an orphan (i.e. whether it has no connections
        /// leading to it)
        /// </summary>
        /// <returns>Returns true if the object is an orphan or false if not.</returns>
        public virtual Boolean IsOrphan()
        {
            return false;
        }
        #endregion

        #region SetPosition
        /// <summary>
        /// Sets the position of the object to the specified coordinates. 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPosition(Single x, Single y)
        {
            this.position = new Point2F(x, y);
        }
        #endregion
    }
}