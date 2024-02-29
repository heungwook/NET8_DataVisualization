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
    /// ChartPicture class represents chart content like legends, titles, 
    /// chart areas and series. It provides methods for positioning and 
    /// drawing all chart elements.
	/// </summary>
    internal partial class ChartPicture : ChartElement, IServiceProvider
	{
        #region Fields

        /// <summary>
			/// Indicates that chart exceptions should be suppressed.
			/// </summary>
			private bool					_suppressExceptions = false;

			// Chart Graphic object
            internal ChartGraphics ChartGraph { get; set; }

			// Private data members, which store properties values
			private GradientStyle			_backGradientStyle = GradientStyle.None;
			private Color					_backSecondaryColor = Color.Empty;
			private Color					_backColor = Color.White;
			private string					_backImage = "";
			private ChartImageWrapMode		_backImageWrapMode = ChartImageWrapMode.Tile;
			private Color					_backImageTransparentColor = Color.Empty;
			private ChartImageAlignmentStyle			_backImageAlign = ChartImageAlignmentStyle.TopLeft;
			private Color					_borderColor = Color.White;
			private int						_borderWidth = 1;
			private ChartDashStyle			_borderDashStyle = ChartDashStyle.NotSet;
			private ChartHatchStyle			_backHatchStyle = ChartHatchStyle.None;
			private AntiAliasingStyles		_antiAliasing = AntiAliasingStyles.All;
			private TextAntiAliasingQuality	_textAntiAliasingQuality = TextAntiAliasingQuality.High;
			private bool					_isSoftShadows = true;
			private int						_width = 300;
			private int						_height = 300;
			private	DataManipulator			_dataManipulator = new DataManipulator();
			internal HotRegionsList			hotRegionsList = null;
			private BorderSkin	            _borderSkin = null;
#if !Microsoft_CONTROL
			private	bool					_isMapEnabled = true;
			private	MapAreasCollection		_mapAreas = null;
#endif
            // Chart areas collection
            private ChartAreaCollection     _chartAreas = null;

			// Chart legend collection
			private LegendCollection		_legends = null;

			// Chart title collection
			private TitleCollection			_titles = null;

		    // Chart annotation collection
			private	AnnotationCollection	_annotations = null;

			// Annotation smart labels class
			internal AnnotationSmartLabel	annotationSmartLabel = new AnnotationSmartLabel();

			// Chart picture events
            internal event EventHandler<ChartPaintEventArgs> BeforePaint;
            internal event EventHandler<ChartPaintEventArgs> AfterPaint;

			// Chart title position rectangle
			private RectangleF				_titlePosition = RectangleF.Empty;

			// Element spacing size
			internal const float			elementSpacing = 3F;

			// Maximum size of the font in percentage
			internal const float			maxTitleSize = 15F;

			// Printing indicator
			internal bool					isPrinting = false;

			// Indicates chart selection mode
			internal bool					isSelectionMode = false;

            private FontCache               _fontCache = new FontCache();
            
			// Position of the chart 3D border
			private RectangleF				_chartBorderPosition = RectangleF.Empty;

#if Microsoft_CONTROL

   			// Saving As Image indicator
			internal bool					isSavingAsImage = false;

            // Indicates that chart background is restored from the double buffer
			// prior to drawing top level objects like annotations, cursors and selection.
			internal bool					backgroundRestored = false;

            // Buffered image of non-top level chart elements
		    internal		Bitmap				nonTopLevelChartBuffer = null;

#endif // Microsoft_CONTROL

        #endregion

            #region Constructors

            /// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="container">Service container</param>
		public ChartPicture(IServiceContainer container) 
		{
			if(container == null)
			{
				throw(new ArgumentNullException(SR.ExceptionInvalidServiceContainer));
			}

			// Create and set Common Elements
            Common = new CommonElements(container);
			ChartGraph= new ChartGraphics(Common);
			hotRegionsList = new HotRegionsList(Common);

			// Create border properties class
			_borderSkin = new BorderSkin(this);

			// Create a collection of chart areas
			_chartAreas = new ChartAreaCollection(this);

			// Create a collection of legends
			_legends = new LegendCollection(this);

			// Create a collection of titles
			_titles = new TitleCollection(this);

			// Create a collection of annotations
			_annotations = new AnnotationCollection(this);

			// Set Common elements for data manipulator
			_dataManipulator.Common = Common;

#if !Microsoft_CONTROL
			// Create map areas collection
			_mapAreas = new MapAreasCollection();
#endif
        }

		/// <summary>
		/// Returns Chart service object
		/// </summary>
		/// <param name="serviceType">Service AxisName</param>
		/// <returns>Chart picture</returns>
		[EditorBrowsableAttribute(EditorBrowsableState.Never)]
		object IServiceProvider.GetService(Type serviceType)
		{
			if(serviceType == typeof(ChartPicture))
			{
				return this;
			}
			throw (new ArgumentException( SR.ExceptionChartPictureUnsupportedType( serviceType.ToString() ) ) );
		}

		#endregion

		#region Painting and selection methods

        /// <summary>
        /// Performs empty painting.
        /// </summary>
        internal void PaintOffScreen() // For HitTest Only
        {
			ChartGraph.InitGraphics(null);
			// Check chart size
			// NOTE: Fixes issue #4733
			if (this.Width <= 0 || this.Height <= 0)
            {
                return;
            }
            
            // Set process Mode to hot regions
            this.Common.HotRegionsList.ProcessChartMode |= ProcessMode.HotRegions;
#if Microsoft_CONTROL
            this.Common.HotRegionsList.hitTestCalled = true;
#endif // Microsoft_CONTROL

            // Enable selection mode
            this.isSelectionMode = true;

            // Hot Region list does not exist. Create the list.
            //this.common.HotRegionsList.List = new ArrayList();
            this.Common.HotRegionsList.Clear();

            // Create a new bitmap
            Bitmap image = new Bitmap(Math.Max(1,Width), Math.Max(1,Height));

            // Creates a new Graphics object from the 
            // specified Image object.
            Graphics offScreen = Graphics.FromImage(image);

            // Connect Graphics object with Chart Graphics object
            ChartGraph.Graphics.GraphicsGdi = offScreen;

            // Remember the previous dirty flag
#if Microsoft_CONTROL			
			bool oldDirtyFlag = this.Common.Chart.dirtyFlag;
#endif //Microsoft_CONTROL


            Paint(ChartGraph.Graphics, false);

            image.Dispose();

            // Restore the previous dirty flag
#if Microsoft_CONTROL			
			this.Common.Chart.dirtyFlag = oldDirtyFlag;
#endif //Microsoft_CONTROL

            // Disable selection mode
            this.isSelectionMode = false;

			// Set process Mode to hot regions
			this.Common.HotRegionsList.ProcessChartMode |= ProcessMode.HotRegions;

        }

		/// <summary>
		/// Gets text rendering quality.
		/// </summary>
		/// <returns>Text rendering quality.</returns>
		internal TextRenderingHint GetTextRenderingHint()
		{
			TextRenderingHint result = TextRenderingHint.SingleBitPerPixelGridFit;
			if( (this.AntiAliasing & AntiAliasingStyles.Text) == AntiAliasingStyles.Text )
			{
				result = TextRenderingHint.ClearTypeGridFit;
				if(this.TextAntiAliasingQuality == TextAntiAliasingQuality.Normal)
				{
					result = TextRenderingHint.AntiAlias;
				}
				else if(this.TextAntiAliasingQuality == TextAntiAliasingQuality.SystemDefault)
				{
					result = TextRenderingHint.SystemDefault;
				}
			}
			else
			{
				result = TextRenderingHint.SingleBitPerPixelGridFit;
			}

			return result;
		}

        internal bool GetBorderSkinVisibility()
        {
            return _borderSkin.SkinStyle != BorderSkinStyle.None && this.Width > 20 && this.Height > 20;
        }


		/// <summary>
		/// This function paints a chart.
		/// </summary>
		/// <param name="graph">The graph provides drawing object to the display device. A Graphics object is associated with a specific device context.</param>
		/// <param name="paintTopLevelElementOnly">Indicates that only chart top level elements like cursors, selection or annotation objects must be redrawn.</param>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "3#svg")]
        internal void Paint(
			IRenderingGraphics renderGraphicsGdi, 
			bool paintTopLevelElementOnly ) // GDI Paint
		{
			if (renderGraphicsGdi.GetType().Equals(typeof(RenderingGraphicsGDI)))
				ChartGraph.InitGraphics(null);

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

			bool	resetHotRegionList = false;
            
			if( 
                this.Common.HotRegionsList.hitTestCalled 
                || IsToolTipsEnabled() 
                )
			{
				Common.HotRegionsList.ProcessChartMode = ProcessMode.HotRegions | ProcessMode.Paint;

				this.Common.HotRegionsList.hitTestCalled = false;

				// Clear list of hot regions 
				if(paintTopLevelElementOnly)
				{
					// If repainting only top level elements (annotations) - 
					// clear top level objects hot regions only
					for(int index = 0; index < this.Common.HotRegionsList.List.Count; index++)
					{
						HotRegion region = (HotRegion)this.Common.HotRegionsList.List[index];
						if(region.Type == ChartElementType.Annotation)
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
			if(resetHotRegionList)
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


#endif	//#if Microsoft_CONTROL

			// Check if control was data bound
			ChartImage chartImage = this as ChartImage;
			if(chartImage != null && !chartImage.boundToDataSource)
			{
				if(this.Common != null && this.Common.Chart != null && !this.Common.Chart.IsDesignMode())
				{
					this.Common.Chart.DataBind();
				}
			}

			// Connect Graphics object with Chart Graphics object
			ChartGraph.Graphics = renderGraphicsGdi;

			Common.graph = ChartGraph;

			// Set anti alias mode
			ChartGraph.AntiAliasing = _antiAliasing;
			ChartGraph.softShadows = _isSoftShadows;
			ChartGraph.TextRenderingHint = GetTextRenderingHint();

			try
			{
				// Check if only chart area cursors and annotations must be redrawn
				if(!paintTopLevelElementOnly)
				{
					// Fire Before Paint event
					OnBeforePaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

					// Flag indicates that resize method should be called 
					// after adjusting the intervals in 3D charts
					bool	resizeAfterIntervalAdjusting = false;

					// RecalculateAxesScale paint chart areas
					foreach (ChartArea area in _chartAreas )
					{

						// Check if area is visible
						if(area.Visible)

						{
							area.Set3DAnglesAndReverseMode();
							area.SetTempValues();
							area.ReCalcInternal();

							// Resize should be called the second time
							if( area.Area3DStyle.Enable3D && !area.chartAreaIsCurcular)
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
					foreach (ChartArea area in _chartAreas )
					{
						if( area.Area3DStyle.Enable3D  && 
							!area.chartAreaIsCurcular

							&& area.Visible

							)

						{
							// Make correction for interval in 3D space
                            intervalReCalculated = true;
							area.Estimate3DInterval( ChartGraph );
							area.ReCalcInternal();
						}
					}

					// Resize chart areas after updating 3D interval
					if(resizeAfterIntervalAdjusting)
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
						ChartGraph.FillRectangleAbs( new RectangleF( 0, 0, Width-1 , Height-1 ), 
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
							PenAlignment.Inset );

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
						ChartGraph.FillRectangleAbs( new RectangleF( 0, 0, Width-1 , Height-1 ), 
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
							PenAlignment.Inset );
					}

					// Call BackPaint event
                    this.Chart.CallOnPrePaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

					// Call paint function for each chart area.
					foreach (ChartArea area in _chartAreas )
					{

						// Check if area is visible
						if(area.Visible)

						{
							area.Paint(ChartGraph);
						}
					}

					// This code is introduced because of GetPointsInterval method, 
					// which is very time consuming. There is no reason to calculate 
					// interval after painting.
					foreach (ChartArea area in _chartAreas )
					{
						// Reset interval data
						area.intervalData = double.NaN;
					}

					// Draw Legends
					foreach(Legend legendCurrent in this.Legends)
					{
						legendCurrent.Paint(ChartGraph);
					}

					// Draw chart titles from the collection
					foreach(Title titleCurrent in this.Titles)
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
				if(!this.isSelectionMode)
				{
					foreach (ChartArea area in _chartAreas )
					{

						// Check if area is visible
						if(area.Visible)

						{
							area.PaintCursors(ChartGraph, paintTopLevelElementOnly);
						}
					}
				}

				// Return default values
				foreach (ChartArea area in _chartAreas )
				{

					// Check if area is visible
					if(area.Visible)

					{
						area.Restore3DAnglesAndReverseMode();
						area.GetTempValues();
					}
				}
            }
			catch(System.Exception)
			{
				throw;
			}
			finally
			{
				// Fire After Paint event
				OnAfterPaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

				// Restore temp values for each chart area
				foreach (ChartArea area in _chartAreas )
				{

					// Check if area is visible
					if(area.Visible)

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
		protected virtual void OnBeforePaint(ChartPaintEventArgs e) 
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
		protected virtual void OnAfterPaint(ChartPaintEventArgs e) 
		{
			if (AfterPaint != null) 
			{
				//Invokes the delegates.
				AfterPaint(this, e); 
			}
		}

        internal override void Invalidate()
        {
            base.Invalidate();

#if Microsoft_CONTROL
            if (Chart!=null)
                Chart.Invalidate();
#endif
        }
		#endregion

		#region Resizing methods

		/// <summary>
		/// Resize the chart picture.
		/// </summary>
		/// <param name="chartGraph">Chart graphics.</param>
		public void Resize(ChartGraphics chartGraph)
		{
			Resize(chartGraph, false);
		}

		/// <summary>
		/// Resize the chart picture.
		/// </summary>
		/// <param name="chartGraph">Chart graphics.</param>
		/// <param name="calcAreaPositionOnly">Indicates that only chart area position is calculated.</param>
		public void Resize(ChartGraphics chartGraph, bool calcAreaPositionOnly)
		{
			// Set the chart size for Common elements
			Common.Width = _width;
			Common.Height = _height;

			// Set the chart size for Chart graphics
			chartGraph.SetPictureSize( _width, _height );

			// Initialize chart area(s) rectangle
			RectangleF	chartAreasRectangle = new RectangleF(0, 0, _width - 1, _height - 1);
			chartAreasRectangle = chartGraph.GetRelativeRectangle(chartAreasRectangle);

			//******************************************************
			//** Get the 3D border interface
			//******************************************************
			_titlePosition = RectangleF.Empty;
			IBorderType	border3D = null;
			bool	titleInBorder = false;

			if(_borderSkin.SkinStyle != BorderSkinStyle.None)
			{
				// Set border size
				this._chartBorderPosition = chartGraph.GetAbsoluteRectangle(chartAreasRectangle);

				// Get border interface 
				border3D = Common.BorderTypeRegistry.GetBorderType(_borderSkin.SkinStyle.ToString());
				if(border3D != null)
				{
                    border3D.Resolution = chartGraph.Graphics.DpiX;
					// Check if title should be displayed in the border
					titleInBorder = border3D.GetTitlePositionInBorder() != RectangleF.Empty;
					_titlePosition = chartGraph.GetRelativeRectangle(border3D.GetTitlePositionInBorder());
					_titlePosition.Width = chartAreasRectangle.Width - _titlePosition.Width;

					// Adjust are position to the border size
					border3D.AdjustAreasPosition(chartGraph, ref chartAreasRectangle);
				}
			}

			//******************************************************
			//** Calculate position of all titles in the collection
			//******************************************************
			RectangleF	frameTitlePosition  = RectangleF.Empty;
			if(titleInBorder)
			{
				frameTitlePosition = new RectangleF(_titlePosition.Location, _titlePosition.Size);
			}
			foreach(Title title in this.Titles)
			{
                if (title.DockedToChartArea == Constants.NotSetValue &&
					title.Position.Auto &&
					title.Visible)
				{
					title.CalcTitlePosition(chartGraph, ref chartAreasRectangle, ref frameTitlePosition, elementSpacing);
				}
			}

			//******************************************************
			//** Calculate position of all legends in the collection
			//******************************************************
			this.Legends.CalcLegendPosition(chartGraph, ref chartAreasRectangle, elementSpacing);

			//******************************************************
			//** Calculate position of the chart area(s)
			//******************************************************
			chartAreasRectangle.Width -= elementSpacing;
			chartAreasRectangle.Height -= elementSpacing;
			RectangleF areaPosition = new RectangleF();


			// Get number of chart areas that requeres automatic positioning
			int	areaNumber = 0;
			foreach (ChartArea area in _chartAreas )
			{

				// Check if area is visible
				if(area.Visible)

				{
					if(area.Position.Auto)
					{
						++areaNumber;
					}
				}
			}

			// Calculate how many columns & rows of areas we going to have
			int	areaColumns = (int)Math.Floor(Math.Sqrt(areaNumber));
			if(areaColumns < 1)
			{
				areaColumns = 1;
			}
			int	areaRows = (int)Math.Ceiling(((float)areaNumber) / ((float)areaColumns));

			// Set position for all areas
			int	column = 0;
			int	row = 0;
			foreach (ChartArea area in _chartAreas )
			{

				// Check if area is visible
				if(area.Visible)

				{
					if(area.Position.Auto)
					{
						// Calculate area position
						areaPosition.Width = chartAreasRectangle.Width / areaColumns - elementSpacing;
						areaPosition.Height = chartAreasRectangle.Height / areaRows - elementSpacing;
						areaPosition.X = chartAreasRectangle.X + column * (chartAreasRectangle.Width / areaColumns) + elementSpacing; 
						areaPosition.Y = chartAreasRectangle.Y + row * (chartAreasRectangle.Height / areaRows) + elementSpacing; 

						// Calculate position of all titles in the collection docked outside of the chart area
						TitleCollection.CalcOutsideTitlePosition(this, chartGraph, area, ref areaPosition, elementSpacing);

						// Calculate position of the legend if it's docked outside of the chart area
						this.Legends.CalcOutsideLegendPosition(chartGraph, area, ref areaPosition, elementSpacing);

						// Set area position without changing the Auto flag
						area.Position.SetPositionNoAuto(areaPosition.X, areaPosition.Y, areaPosition.Width, areaPosition.Height);

						// Go to next area
						++row;
						if(row >= areaRows)
						{
							row = 0;
							++column;
						}
					}
					else
					{
						RectangleF rect = area.Position.ToRectangleF();

						// Calculate position of all titles in the collection docked outside of the chart area
						TitleCollection.CalcOutsideTitlePosition(this, chartGraph, area, ref rect, elementSpacing);

						// Calculate position of the legend if it's docked outside of the chart area
						this.Legends.CalcOutsideLegendPosition(chartGraph, area, ref rect, elementSpacing);
					}
				}
			}

			//******************************************************
			//** Align chart areas Position if required
			//******************************************************
			AlignChartAreasPosition();

			//********************************************************
			//** Check if only chart area position must be calculated.
			//********************************************************
			if(!calcAreaPositionOnly)
			{

				//******************************************************
				//** Call Resize function for each chart area.
				//******************************************************
				foreach (ChartArea area in _chartAreas )
				{

					// Check if area is visible
					if(area.Visible)

					{
						area.Resize(chartGraph);
					}
				}

				//******************************************************
				//** Align chart areas InnerPlotPosition if required
				//******************************************************
				AlignChartAreas(AreaAlignmentStyles.PlotPosition);

				//******************************************************
				//** Calculate position of the legend if it's inside
				//** chart plotting area
				//******************************************************

				// Calculate position of all titles in the collection docked outside of the chart area
				TitleCollection.CalcInsideTitlePosition(this, chartGraph, elementSpacing);
				
				this.Legends.CalcInsideLegendPosition(chartGraph, elementSpacing);
			}
		}

		/// <summary>
		/// Minimum and maximum do not have to be calculated 
		/// from data series every time. It is very time 
		/// consuming. Minimum and maximum are buffered 
		/// and only when this flags are set Minimum and 
		/// Maximum are refreshed from data.
		/// </summary>
		internal void ResetMinMaxFromData()
		{
            if (_chartAreas != null)
            {
                // Call ResetMinMaxFromData function for each chart area.
                foreach (ChartArea area in _chartAreas)
                {

                    // Check if area is visible
                    if (area.Visible)
                    {
                        area.ResetMinMaxFromData();
                    }
                }
            }
		}

		/// <summary>
		/// RecalculateAxesScale the chart picture.
		/// </summary>
		public void Recalculate()
		{
			// Call ReCalc function for each chart area.
			foreach (ChartArea area in _chartAreas )
			{

				// Check if area is visible
				if(area.Visible)

				{
					area.ReCalcInternal();
				}
			}
		}
	
		#endregion
		
		#region Chart picture properties

        // VSTS 96787-Text Direction (RTL/LTR)	
#if !Microsoft_CONTROL
        private RightToLeft rightToLeft = RightToLeft.No;
#endif //!Microsoft_CONTROL
        /// <summary>
        /// Gets or sets the RightToLeft type.
        /// </summary>
        [
        DefaultValue(System.Windows.Forms.RightToLeft.No)
        ]
        public System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
#if Microsoft_CONTROL
                return this.Common.Chart.RightToLeft;
#else // !WIN_CONTROL
                return this.rightToLeft;
#endif // WIN_CONTROL
            }
            set
            {
#if Microsoft_CONTROL
                this.Common.Chart.RightToLeft = value;
#else // !Microsoft_CONTROL
                this.rightToLeft = value;
#endif // Microsoft_CONTROL
            }
        }

		/// <summary>
		/// Indicates that non-critical chart exceptions will be suppressed.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeSuppressExceptions"),
		]
		internal bool SuppressExceptions
		{
			set
			{
				_suppressExceptions = value;
			}
			get
			{
				return _suppressExceptions;
			}
		}

		/// <summary>
		/// Chart border skin style.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(BorderSkinStyle.None),
		SRDescription("DescriptionAttributeBorderSkin"),
#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		public BorderSkin BorderSkin
		{
			get
			{
				return _borderSkin;
			}
			set
			{
				_borderSkin = value;
			}
		}

#if ! Microsoft_CONTROL

		/// <summary>
		/// Indicates that chart image map is enabled.
		/// </summary>
		[
		SRCategory("CategoryAttributeMap"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapEnabled"),
		PersistenceMode(PersistenceMode.InnerProperty),
		DefaultValue(true)
		]
		public bool IsMapEnabled
		{
			get
			{
				return _isMapEnabled;
			}
			set
			{
				_isMapEnabled = value;
			}
		}

		/// <summary>
		/// Chart map areas collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeMap"),
		SRDescription("DescriptionAttributeMapAreas"),
		PersistenceMode(PersistenceMode.InnerProperty),
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
		]
		public MapAreasCollection MapAreas
		{
			get
			{
                return _mapAreas;
			}
		}
#endif

		/// <summary>
		/// Reference to chart area collection
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		SRDescription("DescriptionAttributeChartAreas"),
#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.InnerProperty),
#endif
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
		]
		public ChartAreaCollection ChartAreas
		{
			get
			{
				return _chartAreas;
			}
		}

		/// <summary>
		/// Chart legend collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		SRDescription("DescriptionAttributeLegends"),
        Editor(Editors.LegendCollectionEditor.Editor, Editors.LegendCollectionEditor.Base),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		public LegendCollection Legends
		{
			get
			{
				return _legends;
			}
		}

		/// <summary>
		/// Chart title collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeCharttitle"),
		SRDescription("DescriptionAttributeTitles"),
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		public TitleCollection Titles
		{
			get
			{
				return _titles;
			}
		}



		/// <summary>
		/// Chart annotation collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		SRDescription("DescriptionAttributeAnnotations3"),
        Editor(Editors.AnnotationCollectionEditor.Editor, Editors.AnnotationCollectionEditor.Base),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		public AnnotationCollection Annotations
		{
			get
			{
				return _annotations;
			}
		}



		/// <summary>
		/// Background color for the Chart
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "White"),
        SRDescription("DescriptionAttributeBackColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
#if !Microsoft_CONTROL
			if(value == Color.Empty  || value.A != 255 || value == Color.Transparent)
			{
				// NOTE: Transparent colors are valid
			}
#endif
                _backColor = value;
			}
		}

		/// <summary>
		/// Border color for the Chart
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "White"),
		SRDescription("DescriptionAttributeBorderColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public Color BorderColor
		{
			get
			{
                return _borderColor;
			}
			set
			{
                _borderColor = value;
			}
		}

		/// <summary>
		/// Chart width
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(300),
		SRDescription("DescriptionAttributeWidth"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public int Width
		{
			get
			{
                return _width;
			}
			set
			{
                this.InspectChartDimensions(value, this.Height);
                _width = value;
                Common.Width = _width;
			}
		}

		/// <summary>
		/// Series Data Manipulator
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		SRDescription("DescriptionAttributeDataManipulator"),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public DataManipulator DataManipulator
		{
			get
			{
                return _dataManipulator;
			}
		}




		/// <summary>
		/// Chart height
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(300),
		SRDescription("DescriptionAttributeHeight3"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public int Height
		{
			get
			{
                return _height;
			}
			set
			{
                this.InspectChartDimensions(this.Width, value);
                _height = value;
				Common.Height = value;
			}
		}

		/// <summary>
		/// Back Hatch style
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartHatchStyle.None),
        SRDescription("DescriptionAttributeBackHatchStyle"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute),
	#endif
        Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		public ChartHatchStyle BackHatchStyle
		{
			get
			{
                return _backHatchStyle;
			}
			set
			{
                _backHatchStyle = value;
			}
		}

		/// <summary>
		/// Chart area background image
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
        SRDescription("DescriptionAttributeBackImage"),
        Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base),
	    #if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
	    #endif
		NotifyParentPropertyAttribute(true)
		]
		public string BackImage
		{
			get
			{
                return _backImage;
			}
			set
			{
                _backImage = value;
			}
		}

		/// <summary>
		/// Chart area background image drawing mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartImageWrapMode.Tile),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeImageWrapMode"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public ChartImageWrapMode BackImageWrapMode
		{
			get
			{
                return _backImageWrapMode;
			}
			set
			{
                _backImageWrapMode = value;
			}
		}

		/// <summary>
		/// Background image transparent color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public Color BackImageTransparentColor
		{
			get
			{
                return _backImageTransparentColor;
			}
			set
			{
                _backImageTransparentColor = value;
			}
		}

		/// <summary>
		/// Background image alignment used by ClampUnscale drawing mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartImageAlignmentStyle.TopLeft),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackImageAlign"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public ChartImageAlignmentStyle BackImageAlignment
		{
			get
			{
                return _backImageAlign;
			}
			set
			{
                _backImageAlign = value;
			}
		}

		/// <summary>
		/// Indicates that smoothing is applied while drawing shadows.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeSoftShadows3"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public bool IsSoftShadows
		{
			get
			{
                return _isSoftShadows;
			}
			set
			{
                _isSoftShadows = value;
			}
		}

		/// <summary>
		/// Specifies whether smoothing (antialiasing) is applied while drawing chart.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(typeof(AntiAliasingStyles), "All"),
		SRDescription("DescriptionAttributeAntiAlias"),
	#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public AntiAliasingStyles AntiAliasing
		{
			get
			{
                return _antiAliasing;
			}
			set
			{
                _antiAliasing = value;
			}
		}

		/// <summary>
		/// Specifies the quality of text antialiasing.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(typeof(TextAntiAliasingQuality), "High"),
		SRDescription("DescriptionAttributeTextAntiAliasingQuality"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
#endif
		]
		public TextAntiAliasingQuality TextAntiAliasingQuality
		{
			get
			{
                return _textAntiAliasingQuality;
			}
			set
			{
                _textAntiAliasingQuality = value;
			}
		}

		/// <summary>
		/// A type for the background gradient
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(GradientStyle.None),
        SRDescription("DescriptionAttributeBackGradientStyle"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute),
	#endif
        Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]
		public GradientStyle BackGradientStyle
		{
			get
			{
				return _backGradientStyle;
			}
			set
			{
				_backGradientStyle = value;
			}
		}

		/// <summary>
		/// The second color which is used for a gradient
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public Color BackSecondaryColor
		{
			get
			{
                return _backSecondaryColor;
			}
			set
			{
#if !Microsoft_CONTROL
			if(value != Color.Empty && (value.A != 255 || value == Color.Transparent))
			{
				throw (new ArgumentException( SR.ExceptionBackSecondaryColorIsTransparent));
			}
#endif
                _backSecondaryColor = value;
			}
		}

		/// <summary>
		/// The width of the border line
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(1),
		SRDescription("DescriptionAttributeChart_BorderlineWidth"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public int BorderWidth
		{
			get
			{
                return _borderWidth;
			}
			set
			{
				if(value < 0)
				{
					throw(new ArgumentOutOfRangeException("value", SR.ExceptionChartBorderIsNegative));
				}
                _borderWidth = value;
			}
		}

		/// <summary>
		/// The style of the border line
		/// </summary>
		[

		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartDashStyle.NotSet),
        SRDescription("DescriptionAttributeBorderDashStyle"),
	#if !Microsoft_CONTROL
	PersistenceMode(PersistenceMode.Attribute)
	#endif
		]
		public ChartDashStyle BorderDashStyle
		{
			get
			{
                return _borderDashStyle;
			}
			set
			{
                _borderDashStyle = value;
			}
		}

        /// <summary>
        /// Gets the font cache.
        /// </summary>
        /// <value>The font cache.</value>
        internal FontCache FontCache 
        {
            get { return _fontCache; }
        }

		#endregion	

		#region Chart areas alignment methods

		/// <summary>
		/// Checks if any of the chart areas are aligned.
		/// Also checks if the chart ares name in AlignWithChartArea property is valid.
		/// </summary>
		/// <returns>True if at least one area requires alignment.</returns>
		private bool IsAreasAlignmentRequired()
		{
			bool	alignmentRequired = false;

			// Loop through all chart areas
			foreach(ChartArea area in this.ChartAreas)
			{

				// Check if chart area is visible
				if(area.Visible)

				{
					// Check if area is aligned
                    if (area.AlignWithChartArea != Constants.NotSetValue)
					{
						alignmentRequired = true;

						// Check the chart area used for alignment
                        if (this._chartAreas.IndexOf(area.AlignWithChartArea)<0)
                        {
                            throw (new InvalidOperationException(SR.ExceptionChartAreaNameReferenceInvalid(area.Name, area.AlignWithChartArea)));
                        }
					}
				}
			}

			return alignmentRequired;
		}

		/// <summary>
		/// Creates a list of the aligned chart areas.
		/// </summary>
		/// <param name="masterArea">Master chart area.</param>
		/// <param name="type">Alignment type.</param>
		/// <param name="orientation">Vertical or Horizontal orientation.</param>
		/// <returns>List of areas that area aligned to the master area.</returns>
		private ArrayList GetAlignedAreasGroup(ChartArea masterArea, AreaAlignmentStyles type, AreaAlignmentOrientations orientation)
		{
			ArrayList	areaList = new ArrayList();

			// Loop throught the chart areas and get the ones aligned with specified master area
			foreach(ChartArea area in this.ChartAreas)
			{

				// Check if chart area is visible
				if(area.Visible)

				{
					if(area.Name != masterArea.Name && 
						area.AlignWithChartArea == masterArea.Name && 
						(area.AlignmentStyle & type) == type &&
						(area.AlignmentOrientation & orientation) == orientation )
					{
						// Add "slave" area into the list
						areaList.Add(area);
					}
				}
			}

			// If list is not empty insert "master" area in the beginning
			if(areaList.Count > 0)
			{
				areaList.Insert(0, masterArea);
			}

			return areaList;
		}

		/// <summary>
		/// Performs specified type of alignment for the chart areas.
		/// </summary>
		/// <param name="type">Alignment type required.</param>
		internal void AlignChartAreas(AreaAlignmentStyles type)
		{
			// Check if alignment required
			if(IsAreasAlignmentRequired())
			{
				// Loop through all chart areas
				foreach(ChartArea area in this.ChartAreas)
				{

					// Check if chart area is visible
					if(area.Visible)

					{
						// Get vertical areas alignment group using current area as a master
						ArrayList alignGroup = GetAlignedAreasGroup(
							area, 
							type, 
							AreaAlignmentOrientations.Vertical);

						// Align each area in the group
						if(alignGroup.Count > 0)
						{
							AlignChartAreasPlotPosition(alignGroup, AreaAlignmentOrientations.Vertical);
						}

						// Get horizontal areas alignment group using current area as a master
						alignGroup = GetAlignedAreasGroup(
							area, 
							type, 
							AreaAlignmentOrientations.Horizontal);

						// Align each area in the group
						if(alignGroup.Count > 0)
						{
							AlignChartAreasPlotPosition(alignGroup, AreaAlignmentOrientations.Horizontal);
						}
					}
				}
			}
		}

		/// <summary>
		/// Align inner plot position of the chart areas in the group.
		/// </summary>
		/// <param name="areasGroup">List of areas in the group.</param>
		/// <param name="orientation">Group orientation.</param>
		private void AlignChartAreasPlotPosition(ArrayList areasGroup, AreaAlignmentOrientations orientation)
		{
			//****************************************************************
			//** Find the smalles size of the inner plot 
			//****************************************************************
			RectangleF	areaPlotPosition = ((ChartArea)areasGroup[0]).PlotAreaPosition.ToRectangleF();
			foreach(ChartArea area in areasGroup)
			{
				if(area.PlotAreaPosition.X > areaPlotPosition.X)
				{
					areaPlotPosition.X += area.PlotAreaPosition.X - areaPlotPosition.X;
					areaPlotPosition.Width -= area.PlotAreaPosition.X - areaPlotPosition.X;
				}
				if(area.PlotAreaPosition.Y > areaPlotPosition.Y)
				{
					areaPlotPosition.Y += area.PlotAreaPosition.Y - areaPlotPosition.Y;
					areaPlotPosition.Height -= area.PlotAreaPosition.Y - areaPlotPosition.Y;
				}
				if(area.PlotAreaPosition.Right < areaPlotPosition.Right)
				{
					areaPlotPosition.Width -= areaPlotPosition.Right - area.PlotAreaPosition.Right;
					if(areaPlotPosition.Width < 5)
					{
						areaPlotPosition.Width = 5;
					}
				}
				if(area.PlotAreaPosition.Bottom < areaPlotPosition.Bottom)
				{
					areaPlotPosition.Height -= areaPlotPosition.Bottom - area.PlotAreaPosition.Bottom;
					if(areaPlotPosition.Height < 5)
					{
						areaPlotPosition.Height = 5;
					}
				}
			}

			//****************************************************************
			//** Align inner plot position for all areas
			//****************************************************************
			foreach(ChartArea area in areasGroup)
			{
				// Get curretn plot position of the area
				RectangleF	rect = area.PlotAreaPosition.ToRectangleF();

				// Adjust area position
				if( (orientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
				{
					rect.X = areaPlotPosition.X;
					rect.Width = areaPlotPosition.Width;
				}
				if( (orientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
				{
					rect.Y = areaPlotPosition.Y;
					rect.Height = areaPlotPosition.Height;
				}

				// Set new plot position in coordinates relative to chart picture
				area.PlotAreaPosition.SetPositionNoAuto(rect.X, rect.Y, rect.Width, rect.Height);

				// Set new plot position in coordinates relative to chart area position
				rect.X = (rect.X - area.Position.X) / area.Position.Width * 100f;
				rect.Y = (rect.Y - area.Position.Y) / area.Position.Height * 100f;
				rect.Width = rect.Width / area.Position.Width * 100f;
				rect.Height = rect.Height / area.Position.Height * 100f;
				area.InnerPlotPosition.SetPositionNoAuto(rect.X, rect.Y, rect.Width, rect.Height);

				if( (orientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
				{
					area.AxisX2.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
					area.AxisX.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
				}
				if( (orientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
				{
					area.AxisY2.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
					area.AxisY.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
				}
			}

		}

		/// <summary>
		/// Aligns positions of the chart areas.
		/// </summary>
		private void AlignChartAreasPosition()
		{
			// Check if alignment required
			if(IsAreasAlignmentRequired())
			{
				// Loop through all chart areas
				foreach(ChartArea area in this.ChartAreas)
				{

					// Check if chart area is visible
					if(area.Visible)

					{
						// Check if area is alignd by Position to any other area
                        if (area.AlignWithChartArea != Constants.NotSetValue && (area.AlignmentStyle & AreaAlignmentStyles.Position) == AreaAlignmentStyles.Position)
						{
							// Get current area position
							RectangleF	areaPosition = area.Position.ToRectangleF();

							// Get master chart area
							ChartArea	masterArea = this.ChartAreas[area.AlignWithChartArea];

							// Vertical alignment
							if((area.AlignmentOrientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
							{
								// Align area position
								areaPosition.X = masterArea.Position.X;
								areaPosition.Width = masterArea.Position.Width;
							}

							// Horizontal alignment
							if((area.AlignmentOrientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
							{
								// Align area position
								areaPosition.Y = masterArea.Position.Y;
								areaPosition.Height = masterArea.Position.Height;
							}

							// Set new position
							area.Position.SetPositionNoAuto(areaPosition.X, areaPosition.Y, areaPosition.Width, areaPosition.Height);
						}
					}
				}
			}
        }

#if Microsoft_CONTROL

        /// <summary>
		/// Align chart areas cursor.
		/// </summary>
		/// <param name="changedArea">Changed chart area.</param>
		/// <param name="orientation">Orientation of the changed cursor.</param>
		/// <param name="selectionChanged">AxisName of change cursor or selection.</param>
		internal void AlignChartAreasCursor(ChartArea changedArea, AreaAlignmentOrientations orientation, bool selectionChanged)
		{
			// Check if alignment required
			if(IsAreasAlignmentRequired())
			{
				// Loop through all chart areas
				foreach(ChartArea area in this.ChartAreas)
				{

				// Check if chart area is visible
					if(area.Visible)

					{
						// Get vertical areas alignment group using current area as a master
						ArrayList alignGroup = GetAlignedAreasGroup(
							area, 
							AreaAlignmentStyles.Cursor, 
							orientation);

						// Align each area in the group if it contains changed area
						if(alignGroup.Contains(changedArea))
						{
							// Set cursor position for all areas in the group
							foreach(ChartArea groupArea in alignGroup)
							{
								groupArea.alignmentInProcess = true;

								if(orientation == AreaAlignmentOrientations.Vertical)
								{
									if(selectionChanged)
									{
										groupArea.CursorX.SelectionStart = changedArea.CursorX.SelectionStart;
										groupArea.CursorX.SelectionEnd = changedArea.CursorX.SelectionEnd;
									}
									else
									{
										groupArea.CursorX.Position = changedArea.CursorX.Position;
									}
								}
								if(orientation == AreaAlignmentOrientations.Horizontal)
								{
									if(selectionChanged)
									{
										groupArea.CursorY.SelectionStart = changedArea.CursorY.SelectionStart;
										groupArea.CursorY.SelectionEnd = changedArea.CursorY.SelectionEnd;
									}
									else
									{
										groupArea.CursorY.Position = changedArea.CursorY.Position;
									}
								}

								groupArea.alignmentInProcess = false;
							}
						}
					}
				}
			}
        }

        /// <summary>
		/// One of the chart areas was zoomed by the user.
		/// </summary>
		/// <param name="changedArea">Changed chart area.</param>
		/// <param name="orientation">Orientation of the changed scaleView.</param>
		/// <param name="disposeBufferBitmap">Area double fuffer image must be disposed.</param>
		internal void AlignChartAreasZoomed(ChartArea changedArea, AreaAlignmentOrientations orientation, bool disposeBufferBitmap)
		{
			// Check if alignment required
			if(IsAreasAlignmentRequired())
			{
				// Loop through all chart areas
				foreach(ChartArea area in this.ChartAreas)
				{

				// Check if chart area is visible
					if(area.Visible)

					{
						// Get vertical areas alignment group using current area as a master
						ArrayList alignGroup = GetAlignedAreasGroup(
							area, 
							AreaAlignmentStyles.AxesView, 
							orientation);

						// Align each area in the group if it contains changed area
						if(alignGroup.Contains(changedArea))
						{
							// Set cursor position for all areas in the group
							foreach(ChartArea groupArea in alignGroup)
							{
								// Clear image buffer
								if(groupArea.areaBufferBitmap != null && disposeBufferBitmap)
								{
									groupArea.areaBufferBitmap.Dispose();
									groupArea.areaBufferBitmap = null;
								}

								if(orientation == AreaAlignmentOrientations.Vertical)
								{
									groupArea.CursorX.SelectionStart = double.NaN;
									groupArea.CursorX.SelectionEnd = double.NaN;
								}
								if(orientation == AreaAlignmentOrientations.Horizontal)
								{
									groupArea.CursorY.SelectionStart = double.NaN;
									groupArea.CursorY.SelectionEnd = double.NaN;
								}
							}
						}
					}
				}
			}
        }

#endif //Microsoft_CONTROL

        /// <summary>
		/// Align chart areas axes views.
		/// </summary>
		/// <param name="changedArea">Changed chart area.</param>
		/// <param name="orientation">Orientation of the changed scaleView.</param>
		internal void AlignChartAreasAxesView(ChartArea changedArea, AreaAlignmentOrientations orientation)
		{
			// Check if alignment required
			if(IsAreasAlignmentRequired())
			{
				// Loop through all chart areas
				foreach(ChartArea area in this.ChartAreas)
				{

					// Check if chart area is visible
					if(area.Visible)

					{
						// Get vertical areas alignment group using current area as a master
						ArrayList alignGroup = GetAlignedAreasGroup(
							area, 
							AreaAlignmentStyles.AxesView, 
							orientation);

						// Align each area in the group if it contains changed area
						if(alignGroup.Contains(changedArea))
						{
							// Set cursor position for all areas in the group
							foreach(ChartArea groupArea in alignGroup)
							{
								groupArea.alignmentInProcess = true;

								if(orientation == AreaAlignmentOrientations.Vertical)
								{
									groupArea.AxisX.ScaleView.Position = changedArea.AxisX.ScaleView.Position;
									groupArea.AxisX.ScaleView.Size = changedArea.AxisX.ScaleView.Size;
									groupArea.AxisX.ScaleView.SizeType = changedArea.AxisX.ScaleView.SizeType;

									groupArea.AxisX2.ScaleView.Position = changedArea.AxisX2.ScaleView.Position;
									groupArea.AxisX2.ScaleView.Size = changedArea.AxisX2.ScaleView.Size;
									groupArea.AxisX2.ScaleView.SizeType = changedArea.AxisX2.ScaleView.SizeType;
								}
								if(orientation == AreaAlignmentOrientations.Horizontal)
								{
									groupArea.AxisY.ScaleView.Position = changedArea.AxisY.ScaleView.Position;
									groupArea.AxisY.ScaleView.Size = changedArea.AxisY.ScaleView.Size;
									groupArea.AxisY.ScaleView.SizeType = changedArea.AxisY.ScaleView.SizeType;

									groupArea.AxisY2.ScaleView.Position = changedArea.AxisY2.ScaleView.Position;
									groupArea.AxisY2.ScaleView.Size = changedArea.AxisY2.ScaleView.Size;
									groupArea.AxisY2.ScaleView.SizeType = changedArea.AxisY2.ScaleView.SizeType;
								}

								groupArea.alignmentInProcess = false;
							}
						}
					}
				}
			}
		}

		#endregion

		#region Helper methods

        /// <summary>
        /// Inspects the chart dimensions.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        internal void InspectChartDimensions(int width, int height)
        {
            if (this.Chart.IsDesignMode() && ((width * height) > (100 * 1024 *1024)))
            {
                throw new ArgumentException(SR.ExceptionChartOutOfLimits);
            }
            if (width < 0)
            {
                throw new ArgumentException(SR.ExceptionValueMustBeGreaterThan("Width", "0px"));
            }
            if (height < 0)
            {
                throw new ArgumentException(SR.ExceptionValueMustBeGreaterThan("Height", "0px"));
            }
        }

 		/// <summary>
		/// Loads chart appearance template from file.
		/// </summary>
		/// <param name="name">Template file name to load from.</param>
		public void LoadTemplate(string name)
		{
            // Check arguments
            if (name == null)
                throw new ArgumentNullException("name");

			// Load template data into the stream
#if Microsoft_CONTROL
			Stream	stream = new FileStream(name, FileMode.Open, FileAccess.Read);
#else	// Microsoft_CONTROL
			Stream	stream = LoadTemplateData(name);
#endif	// Microsoft_CONTROL

			// Load template from stream
			LoadTemplate(stream);

			// Close tempate stream
			stream.Close();
		}

		/// <summary>
		/// Loads chart appearance template from stream.
		/// </summary>
		/// <param name="stream">Template stream to load from.</param>
        public void LoadTemplate(Stream stream)
        {
            // Check arguments
            if (stream == null)
                throw new ArgumentNullException("stream");

            ChartSerializer serializer = (ChartSerializer)this.Common.container.GetService(typeof(ChartSerializer));
            if (serializer != null)
            {
                // Save previous serializer properties
                string oldSerializableContent = serializer.SerializableContent;
                string oldNonSerializableContent = serializer.NonSerializableContent;
                SerializationFormat oldFormat = serializer.Format;
                bool oldIgnoreUnknownXmlAttributes = serializer.IsUnknownAttributeIgnored;
                bool oldTemplateMode = serializer.IsTemplateMode;

                // Set serializer properties
                serializer.Content = SerializationContents.Appearance;
                serializer.SerializableContent += ",Chart.Titles,Chart.Annotations," +
                                                  "Chart.Legends,Legend.CellColumns,Legend.CustomItems,LegendItem.Cells," +
                                                  "Chart.Series,Series.*Style," +
                                                  "Chart.ChartAreas,ChartArea.Axis*," +
                                                  "Axis.*Grid,Axis.*TickMark, Axis.*Style," +
                                                  "Axis.StripLines, Axis.CustomLabels";
                serializer.Format = SerializationFormat.Xml;
                serializer.IsUnknownAttributeIgnored = true;
                serializer.IsTemplateMode = true;

                try
                {
                    // Load template
                    serializer.Load(stream);
                }
                catch (Exception ex)
                {
                    throw (new InvalidOperationException(ex.Message));
                }
                finally
                {
                    // Restore previous serializer properties
                    serializer.SerializableContent = oldSerializableContent;
                    serializer.NonSerializableContent = oldNonSerializableContent;
                    serializer.Format = oldFormat;
                    serializer.IsUnknownAttributeIgnored = oldIgnoreUnknownXmlAttributes;
                    serializer.IsTemplateMode = oldTemplateMode;
                }
            }
        }

#if !Microsoft_CONTROL

		/// <summary>
		/// Loads template data from the URL.
		/// </summary>
		/// <param name="url">Template URL.</param>
		/// <returns>Stream with template data or null if error.</returns>
		private Stream LoadTemplateData(string url)
		{
            Debug.Assert(url != null, "LoadTemplateData: handed a null url string");

			Stream	dataStream = null;

			// Try to load as relative URL using the Control object
			if(dataStream == null)
			{
                if (this.Common != null && 
                    this.Common.Chart != null &&
                    this.Common.Chart.Page != null)
                {
                    try
                    {
                        dataStream = new FileStream(
                            this.Common.Chart.Page.MapPath(url),
                            FileMode.Open);
                    }
                    catch (NotSupportedException)
                    {
                        dataStream = null;
                    }
                    catch (SecurityException)
                    {
                        dataStream = null;
                    }
                    catch (FileNotFoundException)
                    {
                        dataStream = null;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        dataStream = null;
                    }
                    catch (PathTooLongException)
                    {
                        dataStream = null;
                    }
                }
			}

			// Try to load image using the Web Request
			if(dataStream == null)
			{
				Uri	templateUri = null;
                try
                {
                    // Try to create URI directly from template URL (will work in case of absolute URL)
                    templateUri = new Uri(url);
                }
                catch (UriFormatException)
                {
                    templateUri = null;
                }

				// Make absolute URL using web form document URL
				if(templateUri == null)
				{
                    if (this.Common != null && this.Common.Chart != null)
					{
						string	webFormUrl = this.Common.Chart.webFormDocumentURL;
						int slashIndex = webFormUrl.LastIndexOf('/');
						if(slashIndex != -1)
						{
							webFormUrl = webFormUrl.Substring(0, slashIndex + 1);
						}

                        try
                        {
                            templateUri = new Uri(new Uri(webFormUrl), url);
                        }
                        catch (UriFormatException)
                        {
                            templateUri = null;
                        }
					}
				}

				// Load image from file or web resource
				if(templateUri != null)
				{
                    try
                    {
                        WebRequest request = WebRequest.Create(templateUri);
                        dataStream = request.GetResponse().GetResponseStream();
                    }
                    catch (NotSupportedException)
                    {
                        dataStream = null;
                    }
                    catch (NotImplementedException)
                    {
                        dataStream = null;
                    }
                    catch (SecurityException)
                    {
                        dataStream = null;
                    }
				}
			}

			// Try to load as file
			if(dataStream == null)
			{
                dataStream = new FileStream(url, FileMode.Open);
			}

			return dataStream;
		}

#endif	// Microsoft_CONTROL



#if !Microsoft_CONTROL


        /// <summary>
        /// Writes chart map tag into the stream.
        /// </summary>
        /// <param name="output">Html writer to output the data to.</param>
        /// <param name="mapName">Chart map name.</param>
        internal void WriteChartMapTag(HtmlTextWriter output, string mapName)
		{
            output.WriteLine();
            output.AddAttribute(HtmlTextWriterAttribute.Name, mapName);
            output.AddAttribute(HtmlTextWriterAttribute.Id, mapName);
            output.RenderBeginTag(HtmlTextWriterTag.Map);

			//****************************************************
			//** Fire map areas customize event
			//****************************************************

			// Make sure only non-custom items are passed into the event handler
			MapAreasCollection	custCollection = new MapAreasCollection();

			// Move all non-custom items
            for (int index = 0; index < _mapAreas.Count; index++)
			{
                if (!_mapAreas[index].IsCustom)
				{
                    custCollection.Add(_mapAreas[index]);
                    _mapAreas.RemoveAt(index);
					--index;
				}
			}

			// Call a notification event, so that area items collection can be modified by user
            Common.Chart.CallOnCustomizeMapAreas(custCollection);

			// Add customized items
			foreach(MapArea area in custCollection)
			{
				area.IsCustom = false;
                _mapAreas.Add(area);
			}

			//****************************************************
			//** Add all map areas
			//****************************************************
            foreach (MapArea area in _mapAreas)
			{
                area.RenderTag(output, this.Common.Chart);
			}
            // if this procedure is enforced to run the image maps have to have at least one map area. 
            if (_mapAreas.Count == 0)
            {
                output.Write("<area shape=\"rect\" coords=\"0,0,0,0\" alt=\"\" />");
            }
			
            //****************************************************
			//** End of the map
			//****************************************************
            output.RenderEndTag();
            
			return;
		}

#endif

		/// <summary>
		/// Returns the default title from Titles collection.
		/// </summary>
		/// <param name="create">Create title if it doesn't exists.</param>
		/// <returns>Default title.</returns>
		internal Title GetDefaultTitle(bool create)
		{
			// Check if default title exists
			Title	defaultTitle = null;
			foreach(Title title in this.Titles)
			{
				if(title.Name == "Default Title")
				{
					defaultTitle = title;
				}
			}

			// Create new default title
			if(defaultTitle == null && create)
			{
				defaultTitle = new Title();
				defaultTitle.Name = "Default Title";
				this.Titles.Insert(0, defaultTitle);
			}

			return defaultTitle;
		}

		/// <summary>
		/// Checks if tooltips are enabled
		/// </summary>
		/// <returns>true if tooltips enabled</returns>
		private bool IsToolTipsEnabled()
		{
			
			// Data series loop
			foreach( Series series in Common.DataManager.Series )
			{
				// Check series tooltips
				if( series.ToolTip.Length > 0)
				{
					// ToolTips enabled
					return true;
				}

				// Check series tooltips
				if( series.LegendToolTip.Length > 0 ||
					series.LabelToolTip.Length > 0)
				{
					// ToolTips enabled
					return true;
				}

				// Check point tooltips only for "non-Fast" chart types
				if( !series.IsFastChartType() )
				{
					// Data point loop
					foreach( DataPoint point in series.Points )
					{
						// ToolTip empty
						if( point.ToolTip.Length > 0)
						{
							// ToolTips enabled
							return true;
						}
						// ToolTip empty
						if( point.LegendToolTip.Length > 0 ||
							point.LabelToolTip.Length > 0)
						{
							// ToolTips enabled
							return true;
						}
					}
				}
			}

			// Legend items loop
			foreach( Legend legend in Legends )
			{
				foreach( LegendItem legendItem in legend.CustomItems )
				{
					// ToolTip empty
					if( legendItem.ToolTip.Length > 0 )
					{
						return true;
					}
				}
			}

			// Title items loop
			foreach( Title title in Titles )
			{
				// ToolTip empty
				if( title.ToolTip.Length > 0 )
				{
					return true;
				}
			}

			return false;
		}

		#endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {   
                // Dispose managed resources
                if (ChartGraph != null)
                {
                    ChartGraph.Dispose();
                    ChartGraph = null;
                }
                if (_legends != null)
                {
                    _legends.Dispose();
                    _legends = null;
                }
                if (_titles != null)
                {
                    _titles.Dispose();
                    _titles = null;
                }
                if (_chartAreas != null)
                {
                    _chartAreas.Dispose();
                    _chartAreas = null;
                }
                if (_annotations != null)
                {
                    _annotations.Dispose();
                    _annotations = null;
                }
                if (hotRegionsList != null)
                {
                    hotRegionsList.Dispose();
                    hotRegionsList = null;
                }
                if (_fontCache != null)
                {
                    _fontCache.Dispose();
                    _fontCache = null;
                }
                if (_borderSkin != null)
                {
                    _borderSkin.Dispose();
                    _borderSkin = null;
                }
#if ! Microsoft_CONTROL
                if (_mapAreas != null)
                {
                    _mapAreas.Dispose();
                    _mapAreas = null;
                }
#endif

#if Microsoft_CONTROL
                if (nonTopLevelChartBuffer != null)
                {
                    nonTopLevelChartBuffer.Dispose();
                    nonTopLevelChartBuffer = null;
                }
#endif
            }
            base.Dispose(disposing);
        }

        #endregion
    }

	/// <summary>
	/// Event arguments of Chart paint event.
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ChartPaintEventArgs : EventArgs
	{
		#region Fields

		// Private fields
        private object          _chartElement = null;
        private ChartGraphics   _chartGraph = null;
		private CommonElements	_common = null;
		private Chart			_chart = null;
		private ElementPosition _position = null;

		#endregion

		#region Properties


        /// <summary>
        /// Gets the chart element of the event.
        /// </summary>
        /// <value>The chart element.</value>
        public object ChartElement
        {
            get
            {
                return _chartElement;
            }
        } 


		/// <summary>
		/// Gets the ChartGraphics object of the event.
		/// </summary>
		public ChartGraphics ChartGraphics
		{
			get
			{
				return _chartGraph;
			}
		} 

		/// <summary>
		/// Chart Common elements.
		/// </summary>
		internal CommonElements CommonElements
		{
			get
			{
				return _common;
			}
		} 

		/// <summary>
		/// Chart element position in relative coordinates of the event.
		/// </summary>
		public ElementPosition Position
		{
			get
			{
                return _position;
			}
		} 

		/// <summary>
		/// Chart object of the event.
		/// </summary>
	    public  Chart Chart
		{
			get
			{
                if (_chart == null && _common != null)
				{
                    _chart = _common.Chart;
				}

                return _chart;
			}
		} 

		#endregion

		#region Methods

		/// <summary>
		/// Default constructor is not accessible
		/// </summary>
		private ChartPaintEventArgs()
		{
		}

        /// <summary>
        /// Paint event arguments constructor.
        /// </summary>
        /// <param name="chartElement">Chart element.</param>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="common">Common elements.</param>
        /// <param name="position">Position.</param>
        internal ChartPaintEventArgs(object chartElement, ChartGraphics chartGraph, CommonElements common, ElementPosition position)
		{
            this._chartElement = chartElement;
            this._chartGraph = chartGraph;
            this._common = common;
            this._position = position;
		}

		#endregion
	}

    /// <summary>
    /// Event arguments of localized numbers formatting event.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class FormatNumberEventArgs : EventArgs
    {
        #region Fields

        // Private fields
        private double _value;
        private string _format;
        private string _localizedValue;
        private ChartValueType _valueType = ChartValueType.Auto;
        private object _senderTag;
        private ChartElementType _elementType = ChartElementType.Nothing;

        #endregion

        #region Properties

        /// <summary>
        /// Value to be formatted.
        /// </summary>
        public double Value
        {
            get { return this._value; }
        }

        /// <summary>
        /// Localized text.
        /// </summary>
        public string LocalizedValue
        {
            get { return _localizedValue; }
            set { _localizedValue = value; }
        }

        /// <summary>
        /// Format string.
        /// </summary>
        public string Format
        {
            get { return _format; }
        }

        /// <summary>
        /// Value type.
        /// </summary>
        public ChartValueType ValueType
        {
            get { return _valueType; }
        }

        /// <summary>
        /// The sender object of the event.
        /// </summary>
        public object SenderTag
        {
            get { return _senderTag; }
        }

        /// <summary>
        /// Chart element type.
        /// </summary>
        public ChartElementType ElementType
        {
            get { return _elementType; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Default constructor is not accessible
        /// </summary>
        private FormatNumberEventArgs()
        {
        }

        /// <summary>
        /// Object constructor.
        /// </summary>
        /// <param name="value">Value to be formatted.</param>
        /// <param name="format">Format string.</param>
        /// <param name="valueType">Value type..</param>
        /// <param name="localizedValue">Localized value.</param>
        /// <param name="senderTag">Chart element object tag.</param>
        /// <param name="elementType">Chart element type.</param>
        internal FormatNumberEventArgs(double value, string format, ChartValueType valueType, string localizedValue, object senderTag, ChartElementType elementType)
        {
            this._value = value;
            this._format = format;
            this._valueType = valueType;
            this._localizedValue = localizedValue;
            this._senderTag = senderTag;
            this._elementType = elementType;
        }

        #endregion
    }

    #region FontCache
    /// <summary>
    /// Font cache class helps ChartElements to reuse the Font instances
    /// </summary>
    internal class FontCache : IDisposable
    {
        #region Static

        // Default font family name
        private static string _defaultFamilyName;

        /// <summary>
        /// Gets the default font family name.
        /// </summary>
        /// <value>The default font family name.</value>
        public static string DefaultFamilyName
        {
            get
            {
                if (_defaultFamilyName == null)
                {
                    // Find the "Microsoft Sans Serif" font
                    foreach (FontFamily fontFamily in FontFamily.Families)
                    {
                        if (fontFamily.Name == "Microsoft Sans Serif")
                        {
                            _defaultFamilyName = fontFamily.Name;
                            break;
                        }
                    }
                    // Not found - use the default Sans Serif font
                    if (_defaultFamilyName == null)
                    {
                        _defaultFamilyName = FontFamily.GenericSansSerif.Name;
                    }
                }
                return _defaultFamilyName;
            }
        }
        #endregion

        #region Fields

        // Cached fonts dictionary 
        private Dictionary<KeyInfo, Font> _fontCache = new Dictionary<KeyInfo, Font>(new KeyInfo.EqualityComparer());

        #endregion // Fields

        #region Properties
        /// <summary>
        /// Gets the default font.
        /// </summary>
        /// <value>The default font.</value>
        public Font DefaultFont 
        { 
            get { return this.GetFont(DefaultFamilyName, 8);  }
        }

        /// <summary>
        /// Gets the default font.
        /// </summary>
        /// <value>The default font.</value>
        public Font DefaultBoldFont
        {
            get { return this.GetFont(DefaultFamilyName, 8, FontStyle.Bold); }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the font.
        /// </summary>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="size">The size.</param>
        /// <returns>Font instance</returns>
        public Font GetFont(string familyName, int size)
        {
            KeyInfo key = new KeyInfo(familyName, size);
            if (!this._fontCache.ContainsKey(key))
            {
                this._fontCache.Add(key, new Font(familyName, size));
            }
            return this._fontCache[key];
        }

        /// <summary>
        /// Gets the font.
        /// </summary>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="size">The size.</param>
        /// <param name="style">The style.</param>
        /// <returns>Font instance</returns>
        public Font GetFont(string familyName, float size, FontStyle style)
        {
            KeyInfo key = new KeyInfo(familyName, size, style);
            if (!this._fontCache.ContainsKey(key))
            {
                this._fontCache.Add(key, new Font(familyName, size, style));
            }
            return this._fontCache[key];
        }

        /// <summary>
        /// Gets the font.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="size">The size.</param>
        /// <param name="style">The style.</param>
        /// <returns>Font instance</returns>
        public Font GetFont(FontFamily family, float size, FontStyle style)
        {
            KeyInfo key = new KeyInfo(family, size, style);
            if (!this._fontCache.ContainsKey(key))
            {
                this._fontCache.Add(key, new Font(family, size, style));
            }
            return this._fontCache[key];
        }

        /// <summary>
        /// Gets the font.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="size">The size.</param>
        /// <param name="style">The style.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Font instance</returns>
        public Font GetFont(FontFamily family, float size, FontStyle style, GraphicsUnit unit)
        {
            KeyInfo key = new KeyInfo(family, size, style, unit);
            if (!this._fontCache.ContainsKey(key))
            {
                this._fontCache.Add(key, new Font(family, size, style, unit));
            }
            return this._fontCache[key];
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (Font font in _fontCache.Values)
            {
                font.Dispose();
            }
            _fontCache.Clear();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region FontKeyInfo struct
        /// <summary>
        /// Font key info
        /// </summary>
        private class KeyInfo 
        { 
            string          _familyName;
            float           _size = 8;
            GraphicsUnit    _unit = GraphicsUnit.Point;
            FontStyle       _style = FontStyle.Regular;
            int             _gdiCharSet = 1;

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyInfo"/> class.
            /// </summary>
            /// <param name="familyName">Name of the family.</param>
            /// <param name="size">The size.</param>
            public KeyInfo(string familyName, float size)
            {
                this._familyName = familyName;
                this._size = size;
            }
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyInfo"/> class.
            /// </summary>
            /// <param name="familyName">Name of the family.</param>
            /// <param name="size">The size.</param>
            /// <param name="style">The style.</param>
            public KeyInfo(string familyName, float size, FontStyle style)
            {
                this._familyName = familyName;
                this._size = size;
                this._style = style;
            }
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyInfo"/> class.
            /// </summary>
            /// <param name="family">The family.</param>
            /// <param name="size">The size.</param>
            /// <param name="style">The style.</param>
            public KeyInfo(FontFamily family, float size, FontStyle style)
            {
                this._familyName = family.ToString();
                this._size = size;
                this._style = style;
            }
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyInfo"/> class.
            /// </summary>
            /// <param name="family">The family.</param>
            /// <param name="size">The size.</param>
            /// <param name="style">The style.</param>
            /// <param name="unit">The unit.</param>
            public KeyInfo(FontFamily family, float size, FontStyle style, GraphicsUnit unit)
            {
                this._familyName = family.ToString();
                this._size = size;
                this._style = style;
                this._unit = unit;
            }

            #region IEquatable<FontKeyInfo> Members
            /// <summary>
            /// KeyInfo equality comparer
            /// </summary>
            internal class EqualityComparer : IEqualityComparer<KeyInfo> 
            {
                /// <summary>
                /// Determines whether the specified objects are equal.
                /// </summary>
                /// <param name="x">The first object of type <paramref name="x"/> to compare.</param>
                /// <param name="y">The second object of type <paramref name="y"/> to compare.</param>
                /// <returns>
                /// true if the specified objects are equal; otherwise, false.
                /// </returns>
                public bool Equals(KeyInfo x, KeyInfo y)
                {
                    return
                        x._size == y._size &&
                        x._familyName == y._familyName &&
                        x._unit == y._unit &&
                        x._style == y._style &&
                        x._gdiCharSet == y._gdiCharSet;
                }

                /// <summary>
                /// Returns a hash code for the specified object.
                /// </summary>
                /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
                /// <returns>A hash code for the specified object.</returns>
                /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
                public int GetHashCode(KeyInfo obj)
                {
                    return obj._familyName.GetHashCode() ^ obj._size.GetHashCode();
                }
            }
            #endregion
        }
        #endregion
    }
    #endregion


}
