//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation?
//   Copyright ?Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartGraphics.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartGraphics
//
//  Purpose:	Chart graphic class is used for drawing Chart 
//				elements as Rectangles, Pie slices, lines, areas 
//				etc. This class is used in all classes where 
//				drawing is necessary. The GDI+ graphic class is 
//				used throw this class. Encapsulates a GDI+ chart 
//				drawing functionality
//
//	Reviewed:	GS - Jul 31, 2002
//				AG - August 7, 2002
//              AG - Microsoft 16, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
using Orion.DataVisualization.Charting.Utilities;
using Orion.DataVisualization.Charting.Borders3D;
#else
using System.Web.UI.DataVisualization.Charting.Utilities;
using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif

#endregion

#if Microsoft_CONTROL
namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    internal class ChartExtension
	{
		/*
		internal static void InitFor(RenderingType rndrType)
		{
			if (rndrType == RenderingType.Gdi)
				InitForGdi();
			else if (rndrType == RenderingType.Pdf)
				InitForPdf();
		}

		internal static void InitForGdi()
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void InitForPdf()
		{
			
		}
		*/
	}


}
