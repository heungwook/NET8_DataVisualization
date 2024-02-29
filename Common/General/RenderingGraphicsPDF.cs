//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation?
//   Copyright ?Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		IChartRenderingEngine.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	IChartRenderingEngine, IChartAnimationEngine
//
//  Purpose:	Defines interfaces which must be implemented by 
//				every rendering and animation engine class. These 
//              interfaces are used in GDI+, SVG and Flash rendering. 
//              Note that animation is only available in SVG and 
//              Flash rendering engines.
//
//	Reviwed:	AG - Jul 15, 2003
//              AG - MArch 14, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections;
using OrionX2.ConfigInfo;

#endregion

#if WINFORMS_CONTROL
    namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    /// <summary>
    /// IChartRenderingEngine interface defines a set of methods and properties 
    /// which must be implemented by any chart rendering engine. It contains 
    /// methods for drawing basic shapes.
    /// </summary>
    internal class RenderingGraphicsPDF : IRenderingGraphics
    {
		#region Constructors

		public RenderingGraphicsPDF(PdfGraphicsInfo graphicsPdf, Graphics graphicsGdi)
		{
			this.GraphicsPdf = graphicsPdf;
			this.GraphicsGdi = graphicsGdi;
		}

		#endregion // Constructors

		#region Methods

		public void DrawRectangle(Pen lPn, Rectangle lRT)
		{
			GraphicsGdi.DrawRectangle(lPn.ToGdiPen(), lRT);
			GraphicsPdf.DrawRectangle(lPn, lRT.X, lRT.Y, lRT.Width, lRT.Height);
		}
		public void FillPath(Brush lBrush, GraphicsPath lGP)
		{
			GraphicsGdi.FillPath(lBrush.ToGdiBrush(), lGP); 
			GraphicsPdf.FillPath(lBrush, lGP);
		}
		public void DrawPath(Pen lPn, GraphicsPath lGP)
		{
			GraphicsGdi.DrawPath(lPn.ToGdiPen(), lGP);
			GraphicsPdf.DrawPath(lPn, lGP);
		}
		#endregion // Methods

		#region Properties

		public float DpiX { get => (float)GraphicsGdi.DpiX; }
        public float DpiY { get => (float)GraphicsGdi.DpiY; }

		public CompositingQuality CompositingQuality
		{
			get { return GraphicsPdf.CompositingQuality; }
			set { GraphicsPdf.CompositingQuality = value; GraphicsPdf.CompositingQuality = value; }
		}
		public InterpolationMode InterpolationMode
		{
			get { return GraphicsPdf.InterpolationMode; }
			set { GraphicsPdf.InterpolationMode = value; GraphicsPdf.InterpolationMode = value; }
		}

		public Matrix Transform
		{
			get { return GraphicsGdi.Transform; }
			set { GraphicsGdi.Transform = value;  GraphicsPdf.Transform = value; }
		}

		#endregion // Properties

		#region Fields

		public PdfGraphicsInfo GraphicsPdf { get; set; }
		public PSGraphicsInfo GraphicsPS { get; set; }
		public Graphics GraphicsGdi { get; set; }

		#endregion // Fields
	}
}
