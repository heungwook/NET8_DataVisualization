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


	/// <summary>
    /// ChartImage class adds image type and data binding functionality to 
    /// the base ChartPicture class.
	/// </summary>
	internal partial class ChartImage : ChartPicture
	{
		#region Fields

		// Private data members, which store properties values
		private int				_compression = 0;

		// Chart data source object
		private object	_dataSource = null;

		// Indicates that control was bound to the data source
		internal bool	boundToDataSource = false;

#if !Microsoft_CONTROL
		private ChartImageType	imageType = ChartImageType.Png;
#endif
		
		#endregion

		#region Constructor

		/// <summary>
		/// Chart internal constructor.
		/// </summary>
		/// <param name="container">Service container</param>
        internal ChartImage(IServiceContainer container)
            : base(container)
        {
        }

		#endregion // Constructor
	
		#region Properties

		/// <summary>
        /// Gets or sets the data source for the Chart object.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		SRDescription("DescriptionAttributeDataSource"),
		DefaultValue(null),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public object DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				if(_dataSource != value)
				{
					_dataSource = value;
					this.boundToDataSource = false;
				}
			}
		}

#if !Microsoft_CONTROL

		/// <summary>
		/// Image type (Jpeg, BMP, Png)
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(ChartImageType.Png),
		SRDescription("DescriptionAttributeImageType"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public ChartImageType ImageType
		{
			get
			{
				return imageType; 
			}
			set
			{
				imageType = value;
			}
		}

#endif

		/// <summary>
		/// Image compression value
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(0),
		SRDescription("DescriptionAttributeChartImage_Compression"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public int Compression
		{
			get
			{
				return _compression;
			}
			set
			{
				if(value < 0 || value > 100)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartCompressionInvalid));
				}
				_compression = value;
			}
		}

		#endregion

		#region Methods

		#region Image Manipulation


		/// <summary>
		/// Saves image into the metafile stream. 
		/// </summary>
		/// <param name="imageStream">Image stream.</param>
		/// <param name="emfType">Image stream.</param>
        [SecuritySafeCritical]
		public void SaveIntoMetafile(Stream imageStream, EmfType emfType)
		{
			ChartGraph.InitGraphics(null);
			// Check arguments
			if (imageStream == null)
                throw new ArgumentNullException("imageStream");

			// Create temporary Graphics object for metafile
            using (Bitmap bitmap = new Bitmap(this.Width, this.Height))
            {
                using (Graphics newGraphics = Graphics.FromImage(bitmap))
                {
                    IntPtr hdc = IntPtr.Zero;
                    try
                    {
                        System.Security.Permissions.SecurityPermission securityPermission = new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode);
                        securityPermission.Demand();
                        
                        hdc = newGraphics.GetHdc();


                        // Create metafile object to record.
                        using (Metafile metaFile = new Metafile(
                            imageStream,
                            hdc,
                            new Rectangle(0, 0, this.Width, this.Height),
                            MetafileFrameUnit.Pixel,
                            emfType))
                        {

                            // Create graphics object to record metaFile.
                            using (Graphics metaGraphics = Graphics.FromImage(metaFile))
                            {

                                // Note: Fix for issue #3674. Some 3D borders shadows may be drawn outside 
                                // of image boundaries. This causes issues when generated EMF file 
                                // is placed in IE. Image looks shifted down and hot areas do not align.
                                if (this.BorderSkin.SkinStyle != BorderSkinStyle.None)
                                {
                                    metaGraphics.Clip = new Region(new Rectangle(0, 0, this.Width, this.Height));
                                }

                                // Draw chart in the metafile
                                this.ChartGraph.IsMetafile = true;
								
								RenderingGraphicsGDI renderGraphicsGdi = new RenderingGraphicsGDI(metaGraphics);
								this.Paint(renderGraphicsGdi, false);
                                this.ChartGraph.IsMetafile = false;

                            }
                        }
                    }
                    finally
                    {
                        if (hdc != IntPtr.Zero)
                        {
                            newGraphics.ReleaseHdc(hdc);
                        }
                    }
                
				}
			}
		}

		public Bitmap GetImage()
        {
            return this.GetImage(96);
        }
		/// <summary>
		/// Create Image and draw chart picture
		/// </summary>
        public Bitmap GetImage(float resolution)
		{
			// Create a new bitmap
			ChartGraph.InitGraphics(null);
			Bitmap image = null;

            while (image == null)
            {
                bool failed = true;
                try
                {
                    image = new Bitmap(Math.Max(1,Width), Math.Max(1,Height));
                    image.SetResolution(resolution, resolution);
                    failed = false;
                }
                catch (ArgumentException)
                {
                    failed = true;
                }
                catch (OverflowException)
                {
                    failed = true;
                }
                catch (InvalidOperationException)
                {
                    failed = true;
                }
                catch (ExternalException)
                {
                    failed = true;
                }

                if (failed)
                {
                    // if failed to create the image, decrease the size and the resolution of the chart
                    image = null;
                    float newResolution = Math.Max(resolution / 2, 96);
                    Width = (int)Math.Ceiling(Width * newResolution / resolution);
                    Height = (int)Math.Ceiling(Height * newResolution / resolution);
                    resolution = newResolution;
                }
            }

			// Creates a new Graphics object from the 
			// specified Image object.
			Graphics offScreen = Graphics.FromImage( image );



            Color backGroundColor;

            if (this.BackColor != Color.Empty)
                backGroundColor = this.BackColor;
            else
                backGroundColor = Color.White;

            // Get the page color if border skin is visible.
            if (GetBorderSkinVisibility() &&
                this.BorderSkin.PageColor != Color.Empty)
            {
                backGroundColor = this.BorderSkin.PageColor;
            }

            // draw a rctangle first with the size of the control, this prevent strange behavior when printing in the reporting services,
            // without this rectangle, the printed picture is blurry
            Pen pen = new Pen(backGroundColor);
            offScreen.DrawRectangle(pen.ToGdiPen(), 0, 0, Width, Height);
            pen.Dispose();

			// Paint the chart
			RenderingGraphicsGDI renderGraphicsGdi = new RenderingGraphicsGDI(offScreen);
			Paint(renderGraphicsGdi, false);

			// Dispose Graphic object
			offScreen.Dispose();

			// Return reference to the image
			return image;
		}

		#endregion // Image Manipulation

		#region Data Binding

		/// <summary>
		/// Checks if the type of the data source is valid.
		/// </summary>
		/// <param name="dataSource">Data source object to test.</param>
		/// <returns>True if valid data source object.</returns>
		static internal bool IsValidDataSource(object dataSource)
		{
            if( null != dataSource && 
                (
                dataSource is IEnumerable ||
				dataSource is DataSet ||
				dataSource is DataView ||
				dataSource is DataTable ||
				dataSource is System.Data.OleDb.OleDbCommand ||
				dataSource is System.Data.SqlClient.SqlCommand ||
				dataSource is System.Data.OleDb.OleDbDataAdapter ||
				dataSource is System.Data.SqlClient.SqlDataAdapter ||
				// ADDED: for VS2005 compatibility, DT Nov 25, 2005
				dataSource.GetType().GetInterface("IDataSource") != null
				// END ADDED
				)
              )
			{
				return true;
			}

			return false;
		}



		/// <summary>
		/// Gets an list of the data source member names.
		/// </summary>
		/// <param name="dataSource">Data source object to get the members for.</param>
		/// <param name="usedForYValue">Indicates that member will be used for Y values.</param>
		/// <returns>List of member names.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "Too large of a code change to justify making this change")]
		static internal ArrayList GetDataSourceMemberNames(object dataSource, bool usedForYValue)
		{
			ArrayList	names = new ArrayList();
            if (dataSource != null)
            {
                // ADDED: for VS2005 compatibility, DT Nov 25, 2004
                if (dataSource.GetType().GetInterface("IDataSource") != null)
                {
                    try
                    {
                        MethodInfo m = dataSource.GetType().GetMethod("Select");
                        if (m != null)
                        {
                            if (m.GetParameters().Length == 1)
                            {
                                // SQL derived datasource
                                Type selectArgsType = dataSource.GetType().Assembly.GetType("System.Web.UI.DataSourceSelectArguments", true);
                                ConstructorInfo ci = selectArgsType.GetConstructor(new Type[] { });
                                dataSource = m.Invoke(dataSource, new object[] { ci.Invoke(new object[] { }) });
                            }
                            else
                            {
                                // object data source
                                dataSource = m.Invoke(dataSource, new object[] { });
                            }
                        }
                    }
                    catch (TargetException)
                    {
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
                // END ADDED

                // Check all DataTable based data souces
                DataTable dataTable = null;

                if (dataSource is DataTable)
                {
                    dataTable = (DataTable)dataSource;
                }
                else if (dataSource is DataView)
                {
                    dataTable = ((DataView)dataSource).Table;
                }
                else if (dataSource is DataSet && ((DataSet)dataSource).Tables.Count > 0)
                {
                    dataTable = ((DataSet)dataSource).Tables[0];
                }
                else if (dataSource is System.Data.OleDb.OleDbDataAdapter)
                {
                    dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataTable = ((System.Data.OleDb.OleDbDataAdapter)dataSource).FillSchema(dataTable, SchemaType.Mapped);
                }
                else if (dataSource is System.Data.SqlClient.SqlDataAdapter)
                {
                    dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataTable = ((System.Data.SqlClient.SqlDataAdapter)dataSource).FillSchema(dataTable, SchemaType.Mapped);
                }
                else if (dataSource is System.Data.OleDb.OleDbDataReader)
                {
                    // Add table columns names
                    for (int fieldIndex = 0; fieldIndex < ((System.Data.OleDb.OleDbDataReader)dataSource).FieldCount; fieldIndex++)
                    {
                        if (!usedForYValue || ((System.Data.OleDb.OleDbDataReader)dataSource).GetFieldType(fieldIndex) != typeof(string))
                        {
                            names.Add(((System.Data.OleDb.OleDbDataReader)dataSource).GetName(fieldIndex));
                        }
                    }
                }
                else if (dataSource is System.Data.SqlClient.SqlDataReader)
                {
                    // Add table columns names
                    for (int fieldIndex = 0; fieldIndex < ((System.Data.SqlClient.SqlDataReader)dataSource).FieldCount; fieldIndex++)
                    {
                        if (!usedForYValue || ((System.Data.SqlClient.SqlDataReader)dataSource).GetFieldType(fieldIndex) != typeof(string))
                        {
                            names.Add(((System.Data.SqlClient.SqlDataReader)dataSource).GetName(fieldIndex));
                        }
                    }
                }
                else if (dataSource is System.Data.OleDb.OleDbCommand)
                {
                    System.Data.OleDb.OleDbCommand command = (System.Data.OleDb.OleDbCommand)dataSource;
                    if (command.Connection != null)
                    {
                        command.Connection.Open();
                        System.Data.OleDb.OleDbDataReader dataReader = command.ExecuteReader();
                        if (dataReader.Read())
                        {
                            for (int fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                            {
                                if (!usedForYValue || dataReader.GetFieldType(fieldIndex) != typeof(string))
                                {
                                    names.Add(dataReader.GetName(fieldIndex));
                                }
                            }
                        }

                        dataReader.Close();
                        command.Connection.Close();
                    }
                }
                else if (dataSource is System.Data.SqlClient.SqlCommand)
                {
                    System.Data.SqlClient.SqlCommand command = (System.Data.SqlClient.SqlCommand)dataSource;
                    if (command.Connection != null)
                    {
                        command.Connection.Open();
                        System.Data.SqlClient.SqlDataReader dataReader = command.ExecuteReader();
                        if (dataReader.Read())
                        {
                            for (int fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                            {
                                if (!usedForYValue || dataReader.GetFieldType(fieldIndex) != typeof(string))
                                {
                                    names.Add(dataReader.GetName(fieldIndex));
                                }
                            }
                        }

                        dataReader.Close();
                        command.Connection.Close();
                    }
                }


                // Check if DataTable was set
                if (dataTable != null)
                {
                    // Add table columns names
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (!usedForYValue || column.DataType != typeof(string))
                        {
                            names.Add(column.ColumnName);
                        }
                    }
                }

                else if (names.Count == 0 && dataSource is ITypedList)
                {
                    foreach (PropertyDescriptor pd in ((ITypedList)dataSource).GetItemProperties(null))
                    {
                        if (!usedForYValue || pd.PropertyType != typeof(string))
                        {
                            names.Add(pd.Name);
                        }
                    }
                }
                else if (names.Count == 0 && dataSource is IEnumerable)
                {
                    // .Net 2.0 ObjectDataSource processing
                    IEnumerator e = ((IEnumerable)dataSource).GetEnumerator();
                    e.Reset();
                    e.MoveNext();
                    foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(e.Current))
                    {
                        if (!usedForYValue || pd.PropertyType != typeof(string))
                        {
                            names.Add(pd.Name);
                        }

                    }
                }



                // Check if list still empty
                if (names.Count == 0)
                {
                    // Add first column or any data member name
                    names.Add("0");
                }

            }

			return names;
		}

		/// <summary>
		/// Data binds control to the data source
		/// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification="Too large of a code change to justify making this change")]
        internal void DataBind()
        {
            // Set bound flag
            this.boundToDataSource = true;

            object dataSource = this.DataSource;
            if (dataSource != null)
            {

                // Convert data adapters to command object
                if (dataSource is System.Data.OleDb.OleDbDataAdapter)
                {
                    dataSource = ((System.Data.OleDb.OleDbDataAdapter)dataSource).SelectCommand;
                }
                else if (dataSource is System.Data.SqlClient.SqlDataAdapter)
                {
                    dataSource = ((System.Data.SqlClient.SqlDataAdapter)dataSource).SelectCommand;
                }

                // Convert data source to recognizable source for the series
                if (dataSource is DataSet && ((DataSet)dataSource).Tables.Count > 0)
                {
                    dataSource = ((DataSet)dataSource).DefaultViewManager.CreateDataView(((DataSet)dataSource).Tables[0]);

                }
                else if (dataSource is DataTable)
                {
                    dataSource = new DataView((DataTable)dataSource);
                }
                else if (dataSource is System.Data.OleDb.OleDbCommand)
                {
                    System.Data.OleDb.OleDbCommand command = (System.Data.OleDb.OleDbCommand)dataSource;
                    command.Connection.Open();
                    System.Data.OleDb.OleDbDataReader dataReader = command.ExecuteReader();

                    this.DataBind(dataReader, null);

                    dataReader.Close();
                    command.Connection.Close();
                    return;
                }
                else if (dataSource is System.Data.SqlClient.SqlCommand)
                {
                    System.Data.SqlClient.SqlCommand command = (System.Data.SqlClient.SqlCommand)dataSource;
                    command.Connection.Open();
                    System.Data.SqlClient.SqlDataReader dataReader = command.ExecuteReader();

                    this.DataBind(dataReader, null);

                    dataReader.Close();
                    command.Connection.Close();
                    return;
                }
                else if (dataSource is IList)
                {
                    dataSource = dataSource as IList;
                }
                else if (dataSource is IListSource  )
                {
                    if (((IListSource)dataSource).ContainsListCollection && ((IListSource)dataSource).GetList().Count > 0)
                    {
                        dataSource = ((IListSource)dataSource).GetList()[0] as IEnumerable;
                    }
                    else
                    {
                        dataSource = ((IListSource)dataSource).GetList();
                    }
                }
                else
                {
                    dataSource = dataSource as IEnumerable;
                }

                // Data bind
                DataBind(dataSource as IEnumerable, null);
            }
        }

		/// <summary>
		/// Data binds control to the data source
		/// </summary>
		/// <param name="dataSource">Data source to bind to.</param>
		/// <param name="seriesList">List of series to bind.</param>
		internal void DataBind(IEnumerable dataSource, ArrayList seriesList)
		{
			// Data bind series
			if(dataSource != null && this.Common != null)
			{
				//************************************************************
				//** If list of series is not provided - bind all of them.
				//************************************************************
				if(seriesList == null)
				{
					seriesList = new ArrayList();
					foreach(Series series in this.Common.Chart.Series)
					{
                        // note: added for design time data binding
                        if (this.Common.Chart.IsDesignMode())
                        {
                            if (series.YValueMembers.Length > 0)
                            {
                                seriesList.Add(series);
                            }
                        }
                        else
                        {
                            seriesList.Add(series);
                        }
					}
				}

				//************************************************************
				//** Clear all data points in data bound series
				//************************************************************
				foreach(Series series in seriesList)
				{
					if(series.XValueMember.Length > 0 || series.YValueMembers.Length > 0)
					{
						series.Points.Clear();
					}
				}

				//************************************************************
				//** Get and reset data enumerator.
				//************************************************************
				IEnumerator	enumerator = dataSource.GetEnumerator();
				if(enumerator.GetType() != typeof(System.Data.Common.DbEnumerator) )
				{
                    try
                    {
                        enumerator.Reset();
                    }
                    // Some enumerators may not support Resetting 
                    catch (InvalidOperationException)
                    {
                    }
                    catch (NotImplementedException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
				}


				//************************************************************
				//** Loop through the enumerator.
				//************************************************************
				bool	valueExsists = true;
				bool	autoDetectType = true;
				do
				{
					// Move to the next item
					valueExsists = enumerator.MoveNext();

					// Loop through all series 
					foreach(Series series in seriesList)
					{
						if(series.XValueMember.Length > 0 || series.YValueMembers.Length > 0)
						{
							//************************************************************
							//** Check and convert fields names.
							//************************************************************

							// Convert comma separated field names string to array of names
							string[] yFieldNames = null;
							if(series.YValueMembers.Length > 0)
							{
								yFieldNames = series.YValueMembers.Replace(",,", "\n").Split(',');
								for(int index = 0; index < yFieldNames.Length; index++)
								{
									yFieldNames[index] = yFieldNames[index].Replace("\n", ",").Trim();
								}
							}
			
							// Double check that a string object is not provided for data binding
							if(dataSource is string)
							{
                                throw (new ArgumentException(SR.ExceptionDataBindYValuesToString, "dataSource"));
							}

							// Check number of fields
							if(yFieldNames == null || yFieldNames.GetLength(0) > series.YValuesPerPoint)
							{
								throw(new ArgumentOutOfRangeException("dataSource", SR.ExceptionDataPointYValuesCountMismatch(series.YValuesPerPoint.ToString(System.Globalization.CultureInfo.InvariantCulture) ) ) );
							}

							//************************************************************
							//** Create new data point.
							//************************************************************
							if(valueExsists)
							{
								// Auto detect values type
								if(autoDetectType)
								{
									autoDetectType = false;

									// Make sure Y field is not empty
									string	yField = yFieldNames[0];
									int		fieldIndex = 1;
									while(yField.Length == 0 && fieldIndex < yFieldNames.Length)
									{
										yField = yFieldNames[fieldIndex++];
									}

									DataPointCollection.AutoDetectValuesType(series, enumerator, series.XValueMember.Trim(), enumerator, yField);
								}


								// Create new point
								DataPoint	newDataPoint = new DataPoint(series);
								bool		emptyValues = false;
								bool		xValueIsNull = false;
								
								//************************************************************
								//** Get new point X and Y values.
								//************************************************************
								object[]	yValuesObj = new object[yFieldNames.Length];
								object		xValueObj = null;

								// Set X to the value provided or use sequence numbers starting with 1
								if(series.XValueMember.Length > 0)
								{
									xValueObj = DataPointCollection.ConvertEnumerationItem(enumerator.Current, series.XValueMember.Trim());
									if(xValueObj is System.DBNull || xValueObj == null)
									{
										xValueIsNull = true;
										emptyValues = true;
										xValueObj = 0.0;
									}
								}

								if(yFieldNames.Length == 0)
								{
									yValuesObj[0] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, null);
									if(yValuesObj[0] is System.DBNull || yValuesObj[0] == null)
									{
										emptyValues = true;
										yValuesObj[0] = 0.0;
									}
								}
								else
								{
									for(int i = 0; i < yFieldNames.Length; i++)
									{
										if(yFieldNames[i].Length > 0)
										{
											yValuesObj[i] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, yFieldNames[i]);
											if(yValuesObj[i] is System.DBNull || yValuesObj[i] == null)
											{
												emptyValues = true;
												yValuesObj[i] = 0.0;
											}
										}
										else
										{
											yValuesObj[i] = (((Series)seriesList[0]).IsYValueDateTime()) ? DateTime.Now.Date.ToOADate() : 0.0;
										}
									}
								}


								// Add data point if X value is not Null
								if(!xValueIsNull)
								{
									if(emptyValues)
									{
										if(xValueObj != null)
										{
											newDataPoint.SetValueXY(xValueObj, yValuesObj);
										}
										else
										{
											newDataPoint.SetValueXY(0, yValuesObj);
										}
										series.Points.DataPointInit(ref newDataPoint);
										newDataPoint.IsEmpty = true;
										series.Points.Add(newDataPoint);
									}
									else
									{
										if(xValueObj != null)
										{
											newDataPoint.SetValueXY(xValueObj, yValuesObj);
										}
										else
										{
											newDataPoint.SetValueXY(0, yValuesObj);
										}
										series.Points.DataPointInit(ref newDataPoint);
										series.Points.Add(newDataPoint);
									}
								}
                                if (this.Common.Chart.IsDesignMode())
                                {
                                    series["TempDesignData"] = "true";
                                }
							}
						}
					}
				
				} while(valueExsists);

			}
		}


		/// <summary>
		/// Aligns data points using their axis labels.
		/// </summary>
		/// <param name="sortAxisLabels">Indicates if points should be sorted by axis labels.</param>
		/// <param name="sortingOrder">Sorting pointSortOrder.</param>
		internal void AlignDataPointsByAxisLabel(bool sortAxisLabels, PointSortOrder sortingOrder)
		{
			// Find series which are attached to the same X axis in the same chart area
			foreach(ChartArea chartArea in this.ChartAreas)
			{

				// Check if chart area is visible
				if(chartArea.Visible)

				{
					// Create series list for primary and secondary X axis
					ArrayList chartAreaSeriesPrimary = new ArrayList();
					ArrayList chartAreaSeriesSecondary = new ArrayList();
					foreach(Series series in this.Common.Chart.Series)
					{
                        // Check if series belongs to the chart area
                        if (series.ChartArea == chartArea.Name)
						{
							if(series.XSubAxisName.Length == 0)
							{
								if(series.XAxisType == AxisType.Primary)
								{
									chartAreaSeriesPrimary.Add(series);
								}
								else
								{
									chartAreaSeriesSecondary.Add(series);
								}
							}
						}
					}

					// Align series
					AlignDataPointsByAxisLabel(chartAreaSeriesPrimary, sortAxisLabels, sortingOrder);
					AlignDataPointsByAxisLabel(chartAreaSeriesSecondary, sortAxisLabels, sortingOrder);
				}
			}
		}

		/// <summary>
		/// Aligns data points using their axis labels.
		/// </summary>
		/// <param name="seriesList">List of series to align.</param>
		/// <param name="sortAxisLabels">Indicates if points should be sorted by axis labels.</param>
		/// <param name="sortingOrder">Sorting order.</param>
		internal void AlignDataPointsByAxisLabel(
			ArrayList seriesList, 
			bool sortAxisLabels, 
			PointSortOrder sortingOrder)
		{
			// List is empty
			if(seriesList.Count == 0)
			{
				return;
			}

			// Collect information about all points in all series
			bool		indexedX = true;
			bool		uniqueAxisLabels = true;
			ArrayList	axisLabels = new ArrayList();
			foreach(Series series in seriesList)
			{
				ArrayList	seriesAxisLabels = new ArrayList();
				foreach(DataPoint point in series.Points)
				{
					// Check if series has indexed X values
					if(!series.IsXValueIndexed && point.XValue != 0.0)
					{
						indexedX = false;
						break;
					}

					// Add axis label to the list and make sure it's non-empty and unique
					if(point.AxisLabel.Length == 0)
					{
						uniqueAxisLabels = false;
						break;
					}
					else if(seriesAxisLabels.Contains(point.AxisLabel))
					{
						uniqueAxisLabels = false;
						break;
					}
					else if(!axisLabels.Contains(point.AxisLabel))
					{
						axisLabels.Add(point.AxisLabel);
					}

					seriesAxisLabels.Add(point.AxisLabel);
				}
			}

			// Sort axis labels
			if(sortAxisLabels)
			{
				axisLabels.Sort();
				if(sortingOrder == PointSortOrder.Descending)
				{
					axisLabels.Reverse();
				}
			}

			// All series must be indexed
			if(!indexedX)
			{
                throw (new InvalidOperationException(SR.ExceptionChartDataPointsAlignmentFaild));
			}

			// AxisLabel can't be empty or duplicated
			if(!uniqueAxisLabels)
			{
                throw (new InvalidOperationException(SR.ExceptionChartDataPointsAlignmentFaildAxisLabelsInvalid));
			}

			// Assign unique X values for data points in all series with same axis LabelStyle
			if(indexedX && uniqueAxisLabels)
			{
				foreach(Series series in seriesList)
				{
					foreach(DataPoint point in series.Points)
					{
						point.XValue = axisLabels.IndexOf(point.AxisLabel) + 1;
					}

					// Sort points by X value
					series.Sort(PointSortOrder.Ascending, "X");
				}

				// Make sure ther are no missing points
				foreach(Series series in seriesList)
				{
					series.IsXValueIndexed = true;
					for(int index = 0; index < axisLabels.Count; index++)
					{
						if(index >= series.Points.Count ||
							series.Points[index].XValue != index + 1)
						{
							DataPoint newPoint = new DataPoint(series);
							newPoint.AxisLabel = (string)axisLabels[index];
							newPoint.XValue = index + 1;
							newPoint.YValues[0] = 0.0;
							newPoint.IsEmpty = true;
							series.Points.Insert(index, newPoint);
						}
					}
				}

			}

		}

        /// <summary>
        /// Data bind chart to the table. Series will be automatically added to the chart depending on
        /// the number of unique values in the seriesGroupByField column of the data source.
        /// Data source can be the Ole(SQL)DataReader, DataView, DataSet, DataTable or DataRow.
        /// </summary>
        /// <param name="dataSource">Data source.</param>
        /// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
        /// <param name="xField">Name of the field for X values.</param>
        /// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
        /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
        /// <param name="sort">Indicates that series should be sorted by group field.</param>
        /// <param name="sortingOrder">Series sorting order by group field.</param>
		internal void DataBindCrossTab(
			IEnumerable dataSource, 
			string seriesGroupByField, 
			string xField, 
			string yFields, 
			string otherFields,
			bool sort,
			PointSortOrder sortingOrder)
		{
            // Check arguments
            if (dataSource == null)
                throw (new ArgumentNullException("dataSource", SR.ExceptionDataPointInsertionNoDataSource));

            if (dataSource is string)
                throw (new ArgumentException(SR.ExceptionDataBindSeriesToString, "dataSource"));

            if (String.IsNullOrEmpty(yFields))
                throw (new ArgumentException(SR.ExceptionChartDataPointsInsertionFailedYValuesEmpty, "yFields"));

            if (String.IsNullOrEmpty(seriesGroupByField))
                throw (new ArgumentException(SR.ExceptionDataBindSeriesGroupByParameterIsEmpty, "seriesGroupByField"));

            
            // List of series and group by field values
			ArrayList seriesList = new ArrayList();
			ArrayList groupByValueList = new ArrayList();

			// Convert comma separated Y values field names string to array of names
			string[] yFieldNames = null;
			if(yFields != null)
			{
				yFieldNames = yFields.Replace(",,", "\n").Split(',');
				for(int index = 0; index < yFieldNames.Length; index++)
				{
					yFieldNames[index] = yFieldNames[index].Replace("\n", ",");
				}
			}

			// Convert other fields/properties names to two arrays of names
			string[] otherAttributeNames = null;
			string[] otherFieldNames = null;
			string[] otherValueFormat = null;
			DataPointCollection.ParsePointFieldsParameter(
				otherFields,
				ref otherAttributeNames,
				ref otherFieldNames,
				ref otherValueFormat);
			

			// Get and reset enumerator
			IEnumerator	enumerator = DataPointCollection.GetDataSourceEnumerator(dataSource);
			if(enumerator.GetType() != typeof(System.Data.Common.DbEnumerator))
			{
                try
                {
                    enumerator.Reset();
                }
                // Some enumerators may not support Resetting 
                catch (NotSupportedException)
                {
                }
                catch (NotImplementedException)
                {
                }
                catch (InvalidOperationException)
                {
                }

			}

			// Add data points
			bool		valueExsist = true;
			object[]	yValuesObj = new object[yFieldNames.Length];
			object		xValueObj = null;
			bool		autoDetectType = true;

			do 
			{
				// Move to the next objects in the enumerations
				if(valueExsist)
				{
					valueExsist = enumerator.MoveNext();
				}

				// Create and initialize data point
				if(valueExsist)
				{
					// Get value of the group by field
					object groupObj = DataPointCollection.ConvertEnumerationItem(
						enumerator.Current, 
						seriesGroupByField);

					// Check series group by field and create new series if required
					Series series = null;
					int seriesIndex = groupByValueList.IndexOf(groupObj);
					if(seriesIndex >= 0)
					{
						// Select existing series from the list
						series = (Series)seriesList[seriesIndex];
					}
					else
					{
						// Create new series
						series = new Series();
						series.YValuesPerPoint = yFieldNames.GetLength(0);

						// If not the first series in the list copy some properties
						if(seriesList.Count > 0)
						{
							series.XValueType = ((Series)seriesList[0]).XValueType;
							series.autoXValueType = ((Series)seriesList[0]).autoXValueType;
							series.YValueType = ((Series)seriesList[0]).YValueType;
							series.autoYValueType = ((Series)seriesList[0]).autoYValueType;
						}

						// Try to set series name based on grouping vlaue
                        string groupObjStr = groupObj as string;
						if(groupObjStr != null)
						{
                            series.Name = groupObjStr;
						}
						else
						{
							series.Name = seriesGroupByField + " - " + groupObj.ToString();
						}
						
					
						// Add series and group value into the lists
						groupByValueList.Add(groupObj);
						seriesList.Add(series);
					}

					
					// Auto detect valu(s) type
					if(autoDetectType)
					{
						autoDetectType = false;
						DataPointCollection.AutoDetectValuesType(series, enumerator, xField, enumerator, yFieldNames[0]);
					}

					// Create new data point
					DataPoint	newDataPoint = new DataPoint(series);
					bool		emptyValues = false;

					// Set X to the value provided
					if(xField.Length > 0)
					{
						xValueObj = DataPointCollection.ConvertEnumerationItem(enumerator.Current, xField);
						if( DataPointCollection.IsEmptyValue(xValueObj) )
						{
							emptyValues = true;
							xValueObj = 0.0;
						}
					}

					// Set Y values
					if(yFieldNames.Length == 0)
					{
						yValuesObj[0] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, null);
						if( DataPointCollection.IsEmptyValue(yValuesObj[0]) )
						{
							emptyValues = true;
							yValuesObj[0] = 0.0;
						}
					}
					else
					{
						for(int i = 0; i < yFieldNames.Length; i++)
						{
							yValuesObj[i] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, yFieldNames[i]);
							if( DataPointCollection.IsEmptyValue(yValuesObj[i] ) )
							{
								emptyValues = true;
								yValuesObj[i] = 0.0;
							}
						}
					}

					// Set other values
					if(otherAttributeNames != null && 
						otherAttributeNames.Length > 0)
					{
						for(int i = 0; i < otherFieldNames.Length; i++)
						{
							// Get object by field name
							object obj = DataPointCollection.ConvertEnumerationItem(enumerator.Current, otherFieldNames[i]);
							if( !DataPointCollection.IsEmptyValue( obj ) )
							{
								newDataPoint.SetPointCustomProperty(
									obj, 
									otherAttributeNames[i], 
									otherValueFormat[i]);
							}
						}
					}

					// IsEmpty value was detected
					if(emptyValues)
					{
						if(xValueObj != null)
						{
							newDataPoint.SetValueXY(xValueObj, yValuesObj);
						}
						else
						{
							newDataPoint.SetValueXY(0, yValuesObj);
						}
						DataPointCollection.DataPointInit(series, ref newDataPoint);
						newDataPoint.IsEmpty = true;
						series.Points.Add(newDataPoint);
					}
					else
					{
						if(xValueObj != null)
						{
							newDataPoint.SetValueXY(xValueObj, yValuesObj);
						}
						else
						{
							newDataPoint.SetValueXY(0, yValuesObj);
						}
                        DataPointCollection.DataPointInit(series, ref newDataPoint);
						series.Points.Add(newDataPoint);
					}
				}

			} while(valueExsist);

			// Sort series usig values of group by field
			if(sort)
			{
				// Duplicate current list
				ArrayList oldList = (ArrayList)groupByValueList.Clone();

				// Sort list 
				groupByValueList.Sort();
				if(sortingOrder == PointSortOrder.Descending)
				{
					groupByValueList.Reverse();
				}

				// Change order of series in collection
				ArrayList sortedSeriesList = new ArrayList();
				foreach(object obj in groupByValueList)
				{
					sortedSeriesList.Add(seriesList[oldList.IndexOf(obj)]);
				}
				seriesList = sortedSeriesList;
			}

			// Add all series from the list into the series collection
			foreach(Series series in seriesList)
			{
				this.Common.Chart.Series.Add(series);
			}
		}

		/// <summary>
		/// Automatically creates and binds series to specified data table. 
		/// Each column of the table becomes a Y value in a separate series.
		/// Series X value field may also be provided. 
		/// </summary>
		/// <param name="dataSource">Data source.</param>
		/// <param name="xField">Name of the field for series X values.</param>
        internal void DataBindTable(
            IEnumerable dataSource,
            string xField)
        {
            // Check arguments
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");

            // Get list of member names from the data source
            ArrayList dataSourceFields = GetDataSourceMemberNames(dataSource, true);

            // Remove X value field if it's there
            if (xField != null && xField.Length > 0)
            {
                int index = -1;
                for (int i = 0; i < dataSourceFields.Count; i++)
                {
                    if ( String.Equals((string)dataSourceFields[i], xField, StringComparison.OrdinalIgnoreCase ) )
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                {
                    dataSourceFields.RemoveAt(index);
                }
                else
                {
                    // Check if field name passed as index
                    bool parseSucceed = int.TryParse(xField, NumberStyles.Any, CultureInfo.InvariantCulture, out index);
                    if (parseSucceed && index >= 0 && index < dataSourceFields.Count)
                    {
                        dataSourceFields.RemoveAt(index);
                    }
                }
            }

            // Get number of series
            int seriesNumber = dataSourceFields.Count;
            if (seriesNumber > 0)
            {
                // Create as many series as fields in the data source
                ArrayList seriesList = new ArrayList();
                int index = 0;
                foreach (string fieldName in dataSourceFields)
                {
                    Series series = new Series(fieldName);

                    // Set binding properties
                    series.YValueMembers = fieldName;
                    series.XValueMember = xField;

                    // Add to list
                    seriesList.Add(series);
                    ++index;
                }


                // Data bind series
                this.DataBind(dataSource, seriesList);

                // Add all series from the list into the series collection
                foreach (Series series in seriesList)
                {
                    // Clear binding properties
                    series.YValueMembers = String.Empty;
                    series.XValueMember = String.Empty;

                    // Add series into the list
                    this.Common.Chart.Series.Add(series);
                }
            }
        }

		#endregion // Data Binding

        #endregion

    }
	


}
