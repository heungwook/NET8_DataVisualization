//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation?
//   Copyright ?Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		Chart.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartImage, ChartPicture, ChartPaintEventArgs
//
//  Purpose:	This file contains classes, which are used for Image 
//				creation and chart painting. This file has also a 
//				class, which is used for Paint events arguments.
//
//	Reviewed:	GS - August 2, 2002
//				AG - August 8, 2002
//              AG - Microsoft 16, 2007
//
//===================================================================

#region Used Namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Data;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Xml;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Security;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#if Microsoft_CONTROL

	using Orion.DataVisualization.Charting.Data;
	using Orion.DataVisualization.Charting.ChartTypes;
	using Orion.DataVisualization.Charting.Utilities;
	using Orion.DataVisualization.Charting.Borders3D;
	using Orion.DataVisualization.Charting;
#else
	using System.Web;
	using System.Web.UI;
	using System.Net;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
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
	#region Enumerations

#if !Microsoft_CONTROL

	/// <summary>
	/// An enumeration of supported image types
	/// </summary>
	public enum ChartImageType
	{
		/// <summary>
		/// BMP image format
		/// </summary>
		Bmp,
		/// <summary>
		/// Jpeg image format
		/// </summary>
		Jpeg, 

		/// <summary>
		/// Png image format
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Png")]
        Png,
    		
		/// <summary>
		/// Enhanced Meta File (Emf) image format.
		/// </summary>
		Emf,

    };
#endif


	#endregion

	/// <summary>
	/// ChartImage class adds image type and data binding functionality to 
	/// the base ChartPicture class.
	/// </summary>
	internal partial class ChartImage
	{
		// -ChartPDF
		public void GetPS(PSGraphicsInfo graphicsPS)
		{

			RenderPS renderingEnginePS = new RenderPS(graphicsPS);
			ChartGraph.InitGraphics(renderingEnginePS); 

			using (MemoryStream lMS = new MemoryStream())
			//using (Bitmap bitmap = new Bitmap(this.Width, this.Height))
			{
				using (Graphics newGraphics = Graphics.FromHwndInternal(IntPtr.Zero))
				{
					IntPtr hdc = IntPtr.Zero;
					try
					{
						System.Security.Permissions.SecurityPermission securityPermission = new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode);
						securityPermission.Demand();

						hdc = newGraphics.GetHdc();

						using (Metafile metaFile = new Metafile(
							lMS,
							hdc,
							new Rectangle(0, 0, this.Width, this.Height),
							MetafileFrameUnit.Pixel,
							EmfType.EmfPlusOnly))
						{
							
							using (Graphics metaGraphics = Graphics.FromImage(metaFile))
							{
								//metaGraphics.PageUnit = GraphicsUnit.Pixel;
							
								if (this.BorderSkin.SkinStyle != BorderSkinStyle.None)
								{
									metaGraphics.Clip = new Region(new Rectangle(0, 0, this.Width, this.Height));
								}

								this.ChartGraph.IsMetafile = true;
								RenderingGraphicsPS renderGraphicsPS = new RenderingGraphicsPS(graphicsPS, metaGraphics);
								this.PaintPS(renderGraphicsPS, false);
								//this.Paint(renderGraphicsPdf, false);
								this.ChartGraph.IsMetafile = false;

							}
						}
					}
					catch (Exception ex)
					{

					}
					finally
					{
						if (hdc != IntPtr.Zero)
						{
							newGraphics.ReleaseHdc(hdc);
						}
					}
				}

				//byte[] chartBytes = lMS.ToArray();
				//File.WriteAllBytes(@"C:\TEST\chart001.emf", chartBytes);
			}
		}
	}

	internal partial class ChartPicture
	{

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "3#svg")]
		internal void PaintPS(
			IRenderingGraphics renderGraphicsPS,
			bool paintTopLevelElementOnly)
		{

#if Microsoft_CONTROL

			// Reset restored and saved backgound flags
			this.backgroundRestored = false;

#endif // Microsoft_CONTROL

			// Reset Annotation Smart Labels 
			this.annotationSmartLabel.Reset();

			// Do not draw the control if size is less than 5 pixel
			if (this.Width < 5 || this.Height < 5)
			{
				return;
			}

#if Microsoft_CONTROL

			bool resetHotRegionList = false;

			if (
				this.Common.HotRegionsList.hitTestCalled
				|| IsToolTipsEnabled()
				)
			{
				Common.HotRegionsList.ProcessChartMode = ProcessMode.HotRegions | ProcessMode.Paint;

				this.Common.HotRegionsList.hitTestCalled = false;

				// Clear list of hot regions 
				if (paintTopLevelElementOnly)
				{
					// If repainting only top level elements (annotations) - 
					// clear top level objects hot regions only
					for (int index = 0; index < this.Common.HotRegionsList.List.Count; index++)
					{
						HotRegion region = (HotRegion)this.Common.HotRegionsList.List[index];
						if (region.Type == ChartElementType.Annotation)
						{
							this.Common.HotRegionsList.List.RemoveAt(index);
							--index;
						}
					}
				}
				else
				{
					// If repainting whole chart - clear all hot regions
					resetHotRegionList = true;
				}
			}
			else
			{
				Common.HotRegionsList.ProcessChartMode = ProcessMode.Paint;

				// If repainting whole chart - clear all hot regions
				resetHotRegionList = true;
			}

			// Reset hot region list
			if (resetHotRegionList)
			{
				this.Common.HotRegionsList.Clear();
			}

#else
			if( this.IsMapEnabled )
			{
				Common.HotRegionsList.ProcessChartMode |= ProcessMode.ImageMaps | ProcessMode.Paint;
				
				// Clear any existing non-custom image map areas
				for(int index = 0; index < this.MapAreas.Count; index++)
				{
					MapArea mapArea = this.MapAreas[index];
					if(!mapArea.IsCustom)
					{
						this.MapAreas.RemoveAt(index);
						--index;
					}
				}
			}


#endif //#if Microsoft_CONTROL

			// Check if control was data bound
			ChartImage chartImage = this as ChartImage;
			if (chartImage != null && !chartImage.boundToDataSource)
			{
				if (this.Common != null && this.Common.Chart != null && !this.Common.Chart.IsDesignMode())
				{
					this.Common.Chart.DataBind();
				}
			}

			// Connect Graphics object with Chart Graphics object
			ChartGraph.Graphics = renderGraphicsPS;

			Common.graph = ChartGraph;

			// Set anti alias mode
			ChartGraph.AntiAliasing = _antiAliasing;
			ChartGraph.softShadows = _isSoftShadows;
			ChartGraph.TextRenderingHint = GetTextRenderingHint();

			try
			{
				// Check if only chart area cursors and annotations must be redrawn
				if (!paintTopLevelElementOnly)
				{
					// Fire Before Paint event
					OnBeforePaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

					// Flag indicates that resize method should be called 
					// after adjusting the intervals in 3D charts
					bool resizeAfterIntervalAdjusting = false;

					// RecalculateAxesScale paint chart areas
					foreach (ChartArea area in _chartAreas)
					{

						// Check if area is visible
						if (area.Visible)

						{
							area.Set3DAnglesAndReverseMode();
							area.SetTempValues();
							area.ReCalcInternal();

							// Resize should be called the second time
							if (area.Area3DStyle.Enable3D && !area.chartAreaIsCurcular)
							{
								resizeAfterIntervalAdjusting = true;
							}
						}
					}

					// Call Customize event
					this.Common.Chart.CallOnCustomize();

					// Resize picture
					Resize(ChartGraph, resizeAfterIntervalAdjusting);


					// This code is introduce because labels has to 
					// be changed when scene is rotated.
					bool intervalReCalculated = false;
					foreach (ChartArea area in _chartAreas)
					{
						if (area.Area3DStyle.Enable3D &&
							!area.chartAreaIsCurcular
							&& area.Visible
							)

						{
							// Make correction for interval in 3D space
							intervalReCalculated = true;
							area.Estimate3DInterval(ChartGraph);
							area.ReCalcInternal();
						}
					}

					// Resize chart areas after updating 3D interval
					if (resizeAfterIntervalAdjusting)
					{
						// NOTE: Fixes issue #6808.
						// In 3D chart area interval will be changed to compenstae for the axis rotation angle.
						// This will cause all standard labels to be changed. We need to call the customize event 
						// the second time to give user a chance to modify those labels.
						if (intervalReCalculated)
						{
							// Call Customize event
							this.Common.Chart.CallOnCustomize();
						}

						// Resize chart elements
						Resize(ChartGraph);
					}


					//***********************************************************************
					//** Draw chart 3D border
					//***********************************************************************
					if (GetBorderSkinVisibility())
					{
						// Fill rectangle with page color
						ChartGraph.FillRectangleAbs(new RectangleF(0, 0, Width - 1, Height - 1),
							_borderSkin.PageColor,
							ChartHatchStyle.None,
							"",
							ChartImageWrapMode.Tile,
							Color.Empty,
							ChartImageAlignmentStyle.Center,
							GradientStyle.None,
							Color.Empty,
							_borderSkin.PageColor,
							1,
							ChartDashStyle.Solid,
							PenAlignment.Inset);

						// Draw 3D border
						ChartGraph.Draw3DBorderAbs(
							_borderSkin,
							this._chartBorderPosition,
							BackColor,
							BackHatchStyle,
							BackImage,
							BackImageWrapMode,
							BackImageTransparentColor,
							BackImageAlignment,
							BackGradientStyle,
							BackSecondaryColor,
							BorderColor,
							BorderWidth,
							BorderDashStyle);
					}

					// Paint Background
					else
					{
						ChartGraph.FillRectangleAbs(new RectangleF(0, 0, Width - 1, Height - 1),
							BackColor,
							BackHatchStyle,
							BackImage,
							BackImageWrapMode,
							BackImageTransparentColor,
							BackImageAlignment,
							BackGradientStyle,
							BackSecondaryColor,
							BorderColor,
							BorderWidth,
							BorderDashStyle,
							PenAlignment.Inset);
					}

					// Call BackPaint event
					this.Chart.CallOnPrePaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

					// Call paint function for each chart area.
					foreach (ChartArea area in _chartAreas)
					{

						// Check if area is visible
						if (area.Visible)

						{
							area.Paint(ChartGraph);
						}
					}

					// This code is introduced because of GetPointsInterval method, 
					// which is very time consuming. There is no reason to calculate 
					// interval after painting.
					foreach (ChartArea area in _chartAreas)
					{
						// Reset interval data
						area.intervalData = double.NaN;
					}

					// Draw Legends
					foreach (Legend legendCurrent in this.Legends)
					{
						legendCurrent.Paint(ChartGraph);
					}

					// Draw chart titles from the collection
					foreach (Title titleCurrent in this.Titles)
					{
						titleCurrent.Paint(ChartGraph);
					}

					// Call Paint event
					this.Chart.CallOnPostPaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));
				}

				// Draw annotation objects 
				this.Annotations.Paint(ChartGraph, paintTopLevelElementOnly);

				// Draw chart areas cursors in all areas.
				// Only if not in selection 
				if (!this.isSelectionMode)
				{
					foreach (ChartArea area in _chartAreas)
					{

						// Check if area is visible
						if (area.Visible)

						{
							area.PaintCursors(ChartGraph, paintTopLevelElementOnly);
						}
					}
				}

				// Return default values
				foreach (ChartArea area in _chartAreas)
				{

					// Check if area is visible
					if (area.Visible)

					{
						area.Restore3DAnglesAndReverseMode();
						area.GetTempValues();
					}
				}
			}
			catch (System.Exception)
			{
				throw;
			}
			finally
			{
				// Fire After Paint event
				OnAfterPaintPdf(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

				// Restore temp values for each chart area
				foreach (ChartArea area in _chartAreas)
				{

					// Check if area is visible
					if (area.Visible)

					{
						area.Restore3DAnglesAndReverseMode();
						area.GetTempValues();
					}
				}

#if !Microsoft_CONTROL
                if (this.Chart.IsDesignMode())
                {
                    this.Chart.MapAreas.RemoveNonCustom();
                }
#endif //!Microsoft_CONTROL
			}
		}

		/// <summary>
		/// Invoke before paint delegates.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnBeforePaintPS(ChartPaintEventArgs e)
		{
			if (BeforePaint != null)
			{
				//Invokes the delegates.
				BeforePaint(this, e);
			}
		}

		/// <summary>
		/// Invoke after paint delegates.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnAfterPaintPS(ChartPaintEventArgs e)
		{
			if (AfterPaint != null)
			{
				//Invokes the delegates.
				AfterPaint(this, e);
			}
		}


		// ChartPDF-

	}


}
