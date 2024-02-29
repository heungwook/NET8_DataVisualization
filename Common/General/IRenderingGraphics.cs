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
	public interface IRenderingGraphics
	{
		#region Methods

		void DrawRectangle(Pen lPn, Rectangle lRT);
		void FillPath(Brush lBrush, GraphicsPath lGP);
		void DrawPath(Pen lPn, GraphicsPath lGP);

		#endregion // Methods

		#region Properties
	    float DpiX { get; }
		float DpiY { get; }

		CompositingQuality CompositingQuality { get; set; }
		InterpolationMode InterpolationMode { get; set; }
		Matrix Transform { get; set; }

		PSGraphicsInfo GraphicsPS { get; set; }
		PdfGraphicsInfo GraphicsPdf { get; set; }
		Graphics GraphicsGdi { get; set; }

		#endregion // Properties
	}
}
