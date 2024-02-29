//-------------------------------------------------------------
// <copyright company=묺icrosoft Corporation?
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
using System.Collections.Generic;
using System.Runtime.Serialization;

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
    [Serializable, DataContract(Name = "LinearGradientBrush", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class LinearGradientBrush : Brush
    {
        private bool interpolationColorsWasSet = false;
        private PointF _point1;
        public PointF Point1 { get { return this._point1; } set { this._point1 = value; } }
        private PointF _point2;
        public PointF Point2 { get { return this._point2; } set { this._point2 = value; } }
        private Color _color1;
        public Color Color1 { get { return this._color1; } set { this._color1 = new Color(value); } }
        private Color _color2;
        public Color Color2 { get { return this._color2; } set { this._color2 = new Color(value); } }
        /*
        //
	    // 요약:
	    //     선형 그라데이션의 방향을 지정합니다.
	    public enum LinearGradientMode
	    {
		    //
		    // 요약:
		    //     왼쪽에서 오른쪽으로 향하는 그라데이션을 지정합니다.
		    Horizontal,
		    //
		    // 요약:
		    //     위쪽에서 아래쪽으로 향하는 그라데이션을 지정합니다.
		    Vertical,
		    //
		    // 요약:
		    //     왼쪽 위에서 오른쪽 아래로 향하는 그라데이션을 지정합니다.
		    ForwardDiagonal,
		    //
		    // 요약:
		    //     오른쪽 위에서 왼쪽 아래로 향하는 그라데이션을 지정합니다.
		    BackwardDiagonal
	    }
        */
        private LinearGradientMode _linearGradientMode;
        public LinearGradientMode LinearGradientMode { get { return _linearGradientMode; } set { _linearGradientMode = value; } }
        private float _angle;
        public float Angle { get { return _angle; } set { _angle = value; } }
        private bool _isAngleScaleable;
        public bool IsAngleScaleable { get { return _isAngleScaleable; } set { _isAngleScaleable = value; } }
        private ColorBlend _interpolationColors;


        public override void ToPdfBrush(PdfGraphicsInfo graphicsPdf)
        {
            /*
            float lfXS = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.X, graphicsPdf.DpiX);
            float lfYS = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.Y, graphicsPdf.DpiY));
            float lfXE = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.X, graphicsPdf.DpiX);
            float lfYE = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.Y, graphicsPdf.DpiY));
            float lfXM = (lfXE + lfXS) / 2F;
            float lfWidth = Math.Abs(lfXE - lfXS);
            
            //if (this.WrapModeEnum == EnumWrapMode.WrapModeTileFlipX)
            {
                
                List<float> llfCoords = new List<float>();
                llfCoords.Add(lfXS);
                llfCoords.Add(lfYS);
                llfCoords.Add(lfXE + lfWidth * 0.5F);// + (float)Math.Abs(lfXE - lfXS) * 0.5F);
                llfCoords.Add(lfYE);
                List<float> llfC0 = new List<float>();
                llfC0.Add((float)this.Color1.CMYK.C);
                llfC0.Add((float)this.Color1.CMYK.M);
                llfC0.Add((float)this.Color1.CMYK.Y);
                llfC0.Add((float)this.Color1.CMYK.K);
                List<float> llfC1 = new List<float>();
                llfC1.Add((float)this.Color2.CMYK.C);
                llfC1.Add((float)this.Color2.CMYK.M);
                llfC1.Add((float)this.Color2.CMYK.Y);
                llfC1.Add((float)this.Color2.CMYK.K);
                List<float> llfDomain = new List<float>();
                llfDomain.Add(0F);
                llfDomain.Add(1F);


                iTextSharp.text.pdf.PdfFunction lShFunc = iTextSharp.text.pdf.PdfFunction.Type2(graphicsPdf.cPdfWrt,
                    llfDomain.ToArray(), null, llfC0.ToArray(), llfC1.ToArray(), 1F);
                iTextSharp.text.pdf.PdfShading lShading = iTextSharp.text.pdf.PdfShading.Type2(graphicsPdf.cPdfWrt,
                    new iTextSharp.text.pdf.CMYKColor(0, 0, 0, 0), llfCoords.ToArray(), new float[] { 0F, 1F }, lShFunc,
                    new bool[] { false, false });
                iTextSharp.text.pdf.PdfShadingPattern lShPattern = new iTextSharp.text.pdf.PdfShadingPattern(lShading);

                graphicsPdf.cPdfCnts.SetShadingFill(lShPattern);
                //graphicsPdf.cPdfCnts.SaveState();
                //graphicsPdf.cPdfCnts.Fill();
                //graphicsPdf.cPdfCnts.RestoreState();
            }
            return;
            {
                
                List<float> llfCoords = new List<float>();
                llfCoords.Add(lfXE + lfWidth * 0.5F);
                llfCoords.Add(lfYS);
                llfCoords.Add(lfXE + lfWidth);// + (float)Math.Abs(lfXE - lfXS) * 0.5F);
                llfCoords.Add(lfYE);
                List<float> llfC0 = new List<float>();
                llfC0.Add((float)this.Color1.CMYK.C);
                llfC0.Add((float)this.Color1.CMYK.M);
                llfC0.Add((float)this.Color1.CMYK.Y);
                llfC0.Add((float)this.Color1.CMYK.K);
                List<float> llfC1 = new List<float>();
                llfC1.Add((float)this.Color2.CMYK.C);
                llfC1.Add((float)this.Color2.CMYK.M);
                llfC1.Add((float)this.Color2.CMYK.Y);
                llfC1.Add((float)this.Color2.CMYK.K);
                List<float> llfDomain = new List<float>();
                llfDomain.Add(0F);
                llfDomain.Add(1F);


                iTextSharp.text.pdf.PdfFunction lShFunc = iTextSharp.text.pdf.PdfFunction.Type2(graphicsPdf.cPdfWrt,
                    llfDomain.ToArray(), null, llfC1.ToArray(), llfC0.ToArray(), 1F);
                iTextSharp.text.pdf.PdfShading lShading = iTextSharp.text.pdf.PdfShading.Type2(graphicsPdf.cPdfWrt,
                    new iTextSharp.text.pdf.CMYKColor(0, 0, 0, 0), llfCoords.ToArray(), new float[] { 0F, 1F }, lShFunc,
                    new bool[] { false, false });
                iTextSharp.text.pdf.PdfShadingPattern lShPattern = new iTextSharp.text.pdf.PdfShadingPattern(lShading);

                graphicsPdf.cPdfCnts.SetShadingFill(lShPattern);
                //graphicsPdf.cPdfCnts.SaveState();
                graphicsPdf.cPdfCnts.Fill();
                //graphicsPdf.cPdfCnts.RestoreState();
            }

            return;*/
                //
                //
                //
                float xSpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.X, graphicsPdf.DpiX) + (float)graphicsPdf.cptChartLocationPt.X; 
            float ySpt = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.Y, graphicsPdf.DpiY));
            float xEpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.X, graphicsPdf.DpiX) + (float)graphicsPdf.cptChartLocationPt.X;
            float yEpt = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.Y, graphicsPdf.DpiY));
            iTextSharp.text.pdf.PdfShading axialShading = iTextSharp.text.pdf.PdfShading.SimpleAxial(graphicsPdf.cPdfWrt, 
                xSpt, ySpt, xEpt, yEpt,
                Color1.ToPdfBaseColor(PdfGraphicsInfo.EnumColorSpace.DeviceRGB), Color2.ToPdfBaseColor(PdfGraphicsInfo.EnumColorSpace.DeviceRGB), true, true);

            iTextSharp.text.pdf.PdfShadingPattern shadingPattern = new iTextSharp.text.pdf.PdfShadingPattern(axialShading);
            graphicsPdf.cPdfCnts.SetShadingFill(shadingPattern);
            //graphicsPdf.cPdfCnts.SaveState();

            //graphicsPdf.cPdfCnts.Rectangle(xSpt, ySpt, xEpt - xSpt, 10);
            //graphicsPdf.cPdfCnts.FillStroke();
            //graphicsPdf.cPdfCnts.RestoreState();
            /*
            iTextSharp.text.pdf.PdfShading axialShading1 = iTextSharp.text.pdf.PdfShading.SimpleAxial(graphicsPdf.cPdfWrt,
                xEpt + (float)Math.Abs(xSpt - xEpt) * 0.5F, ySpt, xEpt + (float)Math.Abs(xSpt - xEpt), yEpt,
                Color2.ToPdfBaseColor(graphicsPdf.ceColorSpace), Color1.ToPdfBaseColor(graphicsPdf.ceColorSpace), false, false);
            iTextSharp.text.pdf.PdfShadingPattern shadingPattern1 = new iTextSharp.text.pdf.PdfShadingPattern(axialShading1);
            graphicsPdf.cPdfCnts.SetShadingFill(shadingPattern1);
            graphicsPdf.cPdfCnts.Fill();
            graphicsPdf.cPdfCnts.Rectangle(xSpt, ySpt+ 10, xEpt - xSpt, 10);
            graphicsPdf.cPdfCnts.SetColorFill(iTextSharp.text.BaseColor.BLACK);*/
        }
        public void ToPdfBrushFlipXLeft(PdfGraphicsInfo graphicsPdf)
        {
            float xSpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.X, graphicsPdf.DpiX) + (float)graphicsPdf.cptChartLocationPt.X;
            float ySpt = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.Y, graphicsPdf.DpiY));
            float xEpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.X, graphicsPdf.DpiX) + (float)graphicsPdf.cptChartLocationPt.X;
            float yEpt = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.Y, graphicsPdf.DpiY));
            iTextSharp.text.pdf.PdfShading axialShading = iTextSharp.text.pdf.PdfShading.SimpleAxial(graphicsPdf.cPdfWrt,
                xSpt, ySpt, xEpt, yEpt,
                Color1.ToPdfBaseColor(graphicsPdf.ceColorSpace), Color2.ToPdfBaseColor(graphicsPdf.ceColorSpace), false, false);

            iTextSharp.text.pdf.PdfShadingPattern shadingPattern = new iTextSharp.text.pdf.PdfShadingPattern(axialShading);
            graphicsPdf.cPdfCnts.SetShadingFill(shadingPattern);
        }
        public void ToPdfBrushFlipXRight(PdfGraphicsInfo graphicsPdf)
        {
            float xSpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.X, graphicsPdf.DpiX) + (float)graphicsPdf.cptChartLocationPt.X;
            float ySpt = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.Y, graphicsPdf.DpiY));
            float xEpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.X, graphicsPdf.DpiX) + (float)graphicsPdf.cptChartLocationPt.X;
            float yEpt = (float)(graphicsPdf.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.Y, graphicsPdf.DpiY));
            float widthPt = Math.Abs(xEpt - xSpt);
            iTextSharp.text.pdf.PdfShading axialShading = iTextSharp.text.pdf.PdfShading.SimpleAxial(graphicsPdf.cPdfWrt,
                xEpt, ySpt, xEpt + widthPt, yEpt,
                Color2.ToPdfBaseColor(graphicsPdf.ceColorSpace), Color1.ToPdfBaseColor(graphicsPdf.ceColorSpace), false, false);

            iTextSharp.text.pdf.PdfShadingPattern shadingPattern = new iTextSharp.text.pdf.PdfShadingPattern(axialShading);
            graphicsPdf.cPdfCnts.SetShadingFill(shadingPattern);
        }


        public override void ToPSBrush(PSGraphicsInfo graphicsPS)
		{
            float xSpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.X, graphicsPS.DpiX);// + (float)graphicsPS.cptChartLocationPt.X;
            float ySpt = (float)(graphicsPS.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point1.Y, graphicsPS.DpiY));
            float xEpt = (float)OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.X, graphicsPS.DpiX);// + (float)graphicsPS.cptChartLocationPt.X;
            float yEpt = (float)(graphicsPS.cszChartSizePt.Height - OrionX2.ConfigInfo.UCNV.GetPointFromPixel(Point2.Y, graphicsPS.DpiY));
            float widthPt = Math.Abs(xEpt - xSpt);

            if (this.WrapMode == WrapMode.TileFlipX)
			{
                graphicsPS.WriteTo_LF("<<");
                graphicsPS.WriteTo_LF("/ShadingType 2");
                graphicsPS.WriteTo_LF(String.Format("/Coords [{0:0.##} {1:0.##} {2:0.##} {3:0.##}]", xSpt, ySpt, xEpt, yEpt));
                graphicsPS.WriteTo_LF("/ColorSpace /DeviceRGB");
                graphicsPS.WriteTo_LF("/Function");
                graphicsPS.WriteTo_LF("<<");
                graphicsPS.WriteTo_LF("/FunctionType 2 /Domain [ 0 1 ]");

                graphicsPS.WriteTo_LF(String.Format("/C0 [{0:0.##} {1:0.##} {2:0.##}]",
                                                          (float)(this.Color1.R) / 255F,
                                                          (float)(this.Color1.G) / 255F,
                                                          (float)(this.Color1.B) / 255F));
                graphicsPS.WriteTo_LF(String.Format("/C1 [{0:0.##} {1:0.##} {2:0.##}]",
                                                          (float)(this.Color2.R) / 255F,
                                                          (float)(this.Color2.G) / 255F,
                                                          (float)(this.Color2.B) / 255F));

                graphicsPS.WriteTo_LF("/N 1.0000");
                graphicsPS.WriteTo_LF(">>");
                graphicsPS.WriteTo_LF(">>");
                graphicsPS.WriteTo_LF("shfill");
                graphicsPS.WriteTo_LF("<<");
                graphicsPS.WriteTo_LF("/ShadingType 2");
                graphicsPS.WriteTo_LF(String.Format("/Coords [{0:0.##} {1:0.##} {2:0.##} {3:0.##}]", xEpt, ySpt, xEpt + widthPt, yEpt));
                graphicsPS.WriteTo_LF("/ColorSpace /DeviceRGB");
                graphicsPS.WriteTo_LF("/Function");
                graphicsPS.WriteTo_LF("<<");
                graphicsPS.WriteTo_LF("/FunctionType 2 /Domain [ 0 1 ]");

                graphicsPS.WriteTo_LF(String.Format("/C0 [{0:0.##} {1:0.##} {2:0.##}]",
                                                          (float)(this.Color2.R) / 255F,
                                                          (float)(this.Color2.G) / 255F,
                                                          (float)(this.Color2.B) / 255F));
                graphicsPS.WriteTo_LF(String.Format("/C1 [{0:0.##} {1:0.##} {2:0.##}]",
                                                          (float)(this.Color1.R) / 255F,
                                                          (float)(this.Color1.G) / 255F,
                                                          (float)(this.Color1.B) / 255F));

                graphicsPS.WriteTo_LF("/N 1.0000");
                graphicsPS.WriteTo_LF(">>");
                graphicsPS.WriteTo_LF(">>");
                graphicsPS.WriteTo_LF("shfill");

            }
            else
			{
                graphicsPS.WriteTo_LF("<<");
                graphicsPS.WriteTo_LF("/ShadingType 2");
                graphicsPS.WriteTo_LF(String.Format("/Coords [{0:0.##} {1:0.##} {2:0.##} {3:0.##}]", xSpt, ySpt, xEpt, yEpt));
                graphicsPS.WriteTo_LF("/ColorSpace /DeviceRGB");
                graphicsPS.WriteTo_LF("/Function");
                graphicsPS.WriteTo_LF("<<");
                graphicsPS.WriteTo_LF("/FunctionType 2 /Domain [ 0 1 ]");

                graphicsPS.WriteTo_LF(String.Format("/C0 [{0:0.##} {1:0.##} {2:0.##}]",
                                                          (float)(this.Color1.R) / 255F,
                                                          (float)(this.Color1.G) / 255F,
                                                          (float)(this.Color1.B) / 255F));
                graphicsPS.WriteTo_LF(String.Format("/C1 [{0:0.##} {1:0.##} {2:0.##}]",
                                                          (float)(this.Color2.R) / 255F,
                                                          (float)(this.Color2.G) / 255F,
                                                          (float)(this.Color2.B) / 255F));

                graphicsPS.WriteTo_LF("/N 1.0000");
                graphicsPS.WriteTo_LF(">>");
                graphicsPS.WriteTo_LF(">>");
                graphicsPS.WriteTo_LF("shfill");
            }

        }

        



        public override bool IsEmptyColor()
        {
            return ((this.Color1.IsEmpty || this.Color1.A == 0)
                     && (this.Color2.IsEmpty || this.Color2.A == 0));
        }

        public LinearGradientBrush(PointF point1, PointF point2,
                                   Color color1, Color color2)
        {
            this._brushGdi = new System.Drawing.Drawing2D.LinearGradientBrush(point1, point2, color1.ToColor(), color2.ToColor());
            this.Point1 = point1;
            this.Point2 = point2;
            this.Color1 = color1;
            this.Color2 = color2;
        }

        public LinearGradientBrush(Point point1, Point point2,
                                   Color color1, Color color2)
        {
            this._brushGdi = new System.Drawing.Drawing2D.LinearGradientBrush(point1, point2, color1.ToColor(), color2.ToColor());
            this.Point1 = point1;
            this.Point2 = point2;
            this.Color1 = color1;
            this.Color2 = color2;
        }


        public LinearGradientBrush(RectangleF rect, Color color1, Color color2,
                                   LinearGradientMode linearGradientMode)
        {
            //validate the rect
            if (rect.Width == 0.0 || rect.Height == 0.0)
            {
                throw new ArgumentException("Invalid rect" + rect.ToString());
            }
            this._brushGdi = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color1.ToColor(), color2.ToColor(), linearGradientMode);

            this.LinearGradientMode = linearGradientMode;
            this.Color1 = color1;
            this.Color2 = color2;
            PointF[] points = RectangleToPoints(rect, this.LinearGradientMode);
            this.Point1 = points[0];
            this.Point2 = points[1];
        }

        public LinearGradientBrush(Rectangle rect, Color color1, Color color2,
                                   LinearGradientMode linearGradientMode)
        {
            //validate the rect
            if (rect.Width == 0 || rect.Height == 0)
            {
                throw new ArgumentException("Invalid rect" + rect.ToString());
            }

            this._brushGdi = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color1.ToColor(), color2.ToColor(), linearGradientMode);

            this.LinearGradientMode = linearGradientMode;
            this.Color1 = color1;
            this.Color2 = color2;
            PointF[] points = RectangleToPoints(rect, this.LinearGradientMode);
            this.Point1 = points[0];
            this.Point2 = points[1];
        }

        public LinearGradientBrush(RectangleF rect, Color color1, Color color2,
                                 float angle)
            : this(rect, color1, color2, angle, false) { }

        public LinearGradientBrush(RectangleF rect, Color color1, Color color2,
                                 float angle, bool isAngleScaleable)
        {
            //validate the rect
            if (rect.Width == 0.0 || rect.Height == 0.0)
            {
                throw new ArgumentException("Invalid rect" + rect.ToString());
            }

            this._brushGdi = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color1.ToColor(), color2.ToColor(), angle, isAngleScaleable);

            this.Color1 = color1;
            this.Color2 = color2;
            this.Angle = angle;
            this.IsAngleScaleable = isAngleScaleable;
            PointF[] points = RectangleToPoints(rect, LinearGradientMode.Horizontal);
            PointF pointCenter = new PointF((points[0].X + points[1].X) / 2F, (points[0].Y + points[1].Y) / 2F);
            Matrix mtrx = new Matrix();
            mtrx.RotateAt(this.Angle, pointCenter);
            mtrx.TransformPoints(points);
            this.Point1 = points[0];
            this.Point2 = points[1];
        }


        public LinearGradientBrush(Rectangle rect, Color color1, Color color2,
                                   float angle)
            : this(rect, color1, color2, angle, false)
        {
        }

        public LinearGradientBrush(Rectangle rect, Color color1, Color color2,
                                 float angle, bool isAngleScaleable)
        {
            //validate the rect
            if (rect.Width == 0 || rect.Height == 0)
            {
                throw new ArgumentException("Invalid rect" + rect.ToString());
            }

            this._brushGdi = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color1.ToColor(), color2.ToColor(), angle, isAngleScaleable);

            this.Color1 = color1;
            this.Color2 = color2;
            this.Angle = angle;
            this.IsAngleScaleable = isAngleScaleable;
            PointF[] points = RectangleToPoints(rect, LinearGradientMode.Horizontal);
            PointF pointCenter = new PointF((points[0].X + points[1].X) / 2F, (points[0].Y + points[1].Y) / 2F);
            Matrix mtrx = new Matrix();
            mtrx.RotateAt(this.Angle, pointCenter);
            mtrx.TransformPoints(points);
            this.Point1 = points[0];
            this.Point2 = points[1];
        }


        internal static PointF[] RectangleToPoints(RectangleF rect, LinearGradientMode mode)
		{
            PointF[] points = new PointF[2] { new PointF(0F, 0F), new PointF(0, 0) };

            if (mode == LinearGradientMode.Horizontal) //     왼쪽에서 오른쪽으로 향하는 그라데이션을 지정합니다.
            {
                points[0].X = rect.Left;
                points[0].Y = rect.Top + rect.Height / 2F;
                points[1].X = rect.Right;
                points[1].Y = points[0].Y;
            }
            else if (mode == LinearGradientMode.Vertical) //     위쪽에서 아래쪽으로 향하는 그라데이션을 지정합니다.
            {
                points[0].X = rect.Left + rect.Width / 2F;
                points[0].Y = rect.Top;
                points[1].X = points[0].X;
                points[1].Y = rect.Bottom;
            }
            else if (mode == LinearGradientMode.ForwardDiagonal) //     왼쪽 위에서 오른쪽 아래로 향하는 그라데이션을 지정합니다.
            {
                points[0].X = rect.Left;
                points[0].Y = rect.Top;
                points[1].X = rect.Right;
                points[1].Y = rect.Bottom;
            }
            else if (mode == LinearGradientMode.BackwardDiagonal) //     오른쪽 위에서 왼쪽 아래로 향하는 그라데이션을 지정합니다.
            {
                points[0].X = rect.Right;
                points[0].Y = rect.Top;
                points[1].X = rect.Left;
                points[1].Y = rect.Bottom;
            }

            return points;
        }


        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        /**
         * Get/set colors
         */

        private void _SetLinearColors(Color color1, Color color2)
        {
            ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).LinearColors = new System.Drawing.Color[2] { color1.ToColor(), color2.ToColor() };
            this.Color1 = color1;
            this.Color2 = color2;
        }

        private Color[] _GetLinearColors()
        {
            Color[] lineColor = new Color[2]
            {
                this.Color1, this.Color2
            };


            return lineColor;
        }


        public Color[] LinearColors
        {
            get { return _GetLinearColors(); }
            set { _SetLinearColors(value[0], value[1]); }
        }


        private RectangleF _GetRectangle()
        {
            return new RectangleF(this.Point1, new SizeF(Math.Abs(this.Point2.X - this.Point1.X), Math.Abs(this.Point2.Y - this.Point1.Y)));
        }

        public RectangleF Rectangle
        {
            get { return _GetRectangle(); }
        }

        public bool GammaCorrection {
            get { return ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).GammaCorrection; }
            set { ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).GammaCorrection = value; } }



        public Blend Blend
        {
            get { return ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).Blend; }
            set { ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).Blend = value; }
        }


        /*
            public enum WrapMode
	        {
		        //
		        // 요약:
		        //     그라데이션이나 질감을 바둑판 모양으로 배열합니다.
		        Tile,
		        //
		        // 요약:
		        //     질감이나 그라데이션을 좌우로 대칭 이동한 다음 바둑판 모양으로 배열합니다.
		        TileFlipX,
		        //
		        // 요약:
		        //     질감이나 그라데이션을 상하로 대칭 이동한 다음 바둑판 모양으로 배열합니다.
		        TileFlipY,
		        //
		        // 요약:
		        //     질감이나 그라데이션을 좌우 및 상하로 대칭 이동한 다음 바둑판 모양으로 배열합니다.
		        TileFlipXY,
		        //
		        // 요약:
		        //     질감이나 그라데이션이 바둑판 모양으로 배열되지 않습니다.
		        Clamp
	        }
         */
        public WrapMode WrapMode { 
            get { return ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).WrapMode; }
            set { ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).WrapMode = value; } }

        private ColorBlend _GetInterpolationColors()
        {
            if (!interpolationColorsWasSet)
            {
                throw new ArgumentException("InterpolationColorsColorBlend Not Set");
            }
            if (this._interpolationColors == null || this._interpolationColors.Colors.Length == 0)
            {
                return new ColorBlend();
            }

            int count = this._interpolationColors.Colors.Length;


            ColorBlend colorBlend = new ColorBlend(count);

            for (int i = 0; i < count; i++)
            {
                colorBlend.Colors[i] = new Color(this._interpolationColors.Colors[i]);
                colorBlend.Positions[i] = this._interpolationColors.Positions[i];
            }

            return colorBlend;
        }

        private void _SetInterpolationColors(ColorBlend colorBlend)
        {
            ((System.Drawing.Drawing2D.LinearGradientBrush)this._brushGdi).InterpolationColors = colorBlend.ToGdiColorBlend();

            interpolationColorsWasSet = true;
            if (colorBlend == null || colorBlend.Colors.Length == 0)
                this._interpolationColors = new ColorBlend();
            int count = colorBlend.Colors.Length;

            this._interpolationColors = new ColorBlend(count);

            for (int i = 0; i < count; i++)
            {
                this._interpolationColors.Colors[i] = new Color(colorBlend.Colors[i]);
                this._interpolationColors.Positions[i] = colorBlend.Positions[i];
            }
        }

        public ColorBlend InterpolationColors
        {
            get { return _GetInterpolationColors(); }
            set { _SetInterpolationColors(value); }
        }


        /*
        public void SetSigmaBellShape(float focus)
        {
            SetSigmaBellShape(focus, (float)1.0);
        }

        public void SetSigmaBellShape(float focus, float scale)
        {
            this._focus = focus;
            this._scale = scale;
        }

        public void SetBlendTriangularShape(float focus)
        {
            SetBlendTriangularShape(focus, (float)1.0);
        }

        public void SetBlendTriangularShape(float focus, float scale)
        {
            this._focus = focus;
            this._scale = scale;
        }



        private ColorBlend _GetInterpolationColors()
        {
            ColorBlend blend;

            if (!interpolationColorsWasSet)
            {
                throw new ArgumentException(SR.GetString(SR.InterpolationColorsCommon,
                                            SR.GetString(SR.InterpolationColorsColorBlendNotSet), ""));
            }
            // Figure out the size of blend factor array

            int retval = 0;
            int status = SafeNativeMethods.Gdip.GdipGetLinePresetBlendCount(new HandleRef(this, this.NativeBrush), out retval);

            if (status != SafeNativeMethods.Gdip.Ok)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }

            // Allocate temporary native memory buffer

            int count = retval;

            IntPtr colors = IntPtr.Zero;
            IntPtr positions = IntPtr.Zero;

            try
            {
                int size = checked(4 * count);
                colors = Marshal.AllocHGlobal(size);
                positions = Marshal.AllocHGlobal(size);

                // Retrieve horizontal blend factors

                status = SafeNativeMethods.Gdip.GdipGetLinePresetBlend(new HandleRef(this, this.NativeBrush), colors, positions, count);

                if (status != SafeNativeMethods.Gdip.Ok)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }

                // Return the result in a managed array

                blend = new ColorBlend(count);

                int[] argb = new int[count];
                Marshal.Copy(colors, argb, 0, count);
                Marshal.Copy(positions, blend.Positions, 0, count);

                // copy ARGB values into Color array of ColorBlend
                blend.Colors = new Color[argb.Length];

                for (int i = 0; i < argb.Length; i++)
                {
                    blend.Colors[i] = Color.FromArgb(argb[i]);
                }
            }
            finally
            {
                if (colors != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(colors);
                }
                if (positions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(positions);
                }
            }

            return blend;
        }

        private void _SetInterpolationColors(ColorBlend blend)
        {
            interpolationColorsWasSet = true;

            // Validate the ColorBlend object.
            if (blend == null)
            {
                throw new ArgumentException(SR.GetString(SR.InterpolationColorsCommon,
                                            SR.GetString(SR.InterpolationColorsInvalidColorBlendObject), ""));
            }
            else if (blend.Colors.Length < 2)
            {
                throw new ArgumentException(SR.GetString(SR.InterpolationColorsCommon,
                                            SR.GetString(SR.InterpolationColorsInvalidColorBlendObject),
                                            SR.GetString(SR.InterpolationColorsLength)));
            }
            else if (blend.Colors.Length != blend.Positions.Length)
            {
                throw new ArgumentException(SR.GetString(SR.InterpolationColorsCommon,
                                            SR.GetString(SR.InterpolationColorsInvalidColorBlendObject),
                                            SR.GetString(SR.InterpolationColorsLengthsDiffer)));
            }
            else if (blend.Positions[0] != 0.0f)
            {
                throw new ArgumentException(SR.GetString(SR.InterpolationColorsCommon,
                                            SR.GetString(SR.InterpolationColorsInvalidColorBlendObject),
                                            SR.GetString(SR.InterpolationColorsInvalidStartPosition)));
            }
            else if (blend.Positions[blend.Positions.Length - 1] != 1.0f)
            {
                throw new ArgumentException(SR.GetString(SR.InterpolationColorsCommon,
                                            SR.GetString(SR.InterpolationColorsInvalidColorBlendObject),
                                            SR.GetString(SR.InterpolationColorsInvalidEndPosition)));
            }


            // Allocate temporary native memory buffer
            // and copy input blend factors into it.

            int count = blend.Colors.Length;

            IntPtr colors = IntPtr.Zero;
            IntPtr positions = IntPtr.Zero;

            try
            {
                int size = checked(4 * count);
                colors = Marshal.AllocHGlobal(size);
                positions = Marshal.AllocHGlobal(size);

                int[] argbs = new int[count];
                for (int i = 0; i < count; i++)
                {
                    argbs[i] = blend.Colors[i].ToArgb();
                }

                Marshal.Copy(argbs, 0, colors, count);
                Marshal.Copy(blend.Positions, 0, positions, count);

                // Set blend factors

                int status = SafeNativeMethods.Gdip.GdipSetLinePresetBlend(new HandleRef(this, this.NativeBrush), new HandleRef(null, colors), new HandleRef(null, positions), count);

                if (status != SafeNativeMethods.Gdip.Ok)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                if (colors != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(colors);
                }
                if (positions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(positions);
                }
            }
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.InterpolationColors"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a <see cref='System.Drawing.Drawing2D.ColorBlend'/> that defines a multi-color linear
        ///       gradient.
        ///    </para>
        /// </devdoc>
        public ColorBlend InterpolationColors
        {
            get { return _GetInterpolationColors(); }
            set { _SetInterpolationColors(value); }
        }





        private void _SetTransform(Matrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException("matrix");

            int status = SafeNativeMethods.Gdip.GdipSetLineTransform(new HandleRef(this, this.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));

            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);
        }

        private Matrix _GetTransform()
        {
            Matrix matrix = new Matrix();

            // NOTE: new Matrix() will throw an exception if matrix == null.

            int status = SafeNativeMethods.Gdip.GdipGetLineTransform(new HandleRef(this, this.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));

            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);

            return matrix;
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.Transform"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a <see cref='System.Drawing.Drawing2D.Matrix'/> that defines a local geometrical transform for
        ///       this <see cref='System.Drawing.Drawing2D.LinearGradientBrush'/>.
        ///    </para>
        /// </devdoc>
        public Matrix Transform
        {
            get { return _GetTransform(); }
            set { _SetTransform(value); }
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.ResetTransform"]/*' />
        /// <devdoc>
        ///    Resets the <see cref='System.Drawing.Drawing2D.LinearGradientBrush.Transform'/> property to identity.
        /// </devdoc>
        public void ResetTransform()
        {
            int status = SafeNativeMethods.Gdip.GdipResetLineTransform(new HandleRef(this, this.NativeBrush));

            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.MultiplyTransform"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Multiplies the <see cref='System.Drawing.Drawing2D.Matrix'/> that represents the local geometrical
        ///       transform of this <see cref='System.Drawing.Drawing2D.LinearGradientBrush'/> by the specified <see cref='System.Drawing.Drawing2D.Matrix'/> by prepending the specified <see cref='System.Drawing.Drawing2D.Matrix'/>.
        ///    </para>
        /// </devdoc>
        public void MultiplyTransform(Matrix matrix)
        {
            MultiplyTransform(matrix, MatrixOrder.Prepend);
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.MultiplyTransform1"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Multiplies the <see cref='System.Drawing.Drawing2D.Matrix'/> that represents the local geometrical
        ///       transform of this <see cref='System.Drawing.Drawing2D.LinearGradientBrush'/> by the specified <see cref='System.Drawing.Drawing2D.Matrix'/> in the specified order.
        ///    </para>
        /// </devdoc>
        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            int status = SafeNativeMethods.Gdip.GdipMultiplyLineTransform(new HandleRef(this, this.NativeBrush),
                                                new HandleRef(matrix, matrix.nativeMatrix),
                                                order);

            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);
        }


        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.TranslateTransform"]/*' />
        /// <devdoc>
        ///    Translates the local geometrical transform
        ///    by the specified dimmensions. This method prepends the translation to the
        ///    transform.
        /// </devdoc>
        public void TranslateTransform(float dx, float dy)
        { TranslateTransform(dx, dy, MatrixOrder.Prepend); }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.TranslateTransform1"]/*' />
        /// <devdoc>
        ///    Translates the local geometrical transform
        ///    by the specified dimmensions in the specified order.
        /// </devdoc>
        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateLineTransform(new HandleRef(this, this.NativeBrush),
                                                            dx,
                                                            dy,
                                                            order);
            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.ScaleTransform"]/*' />
        /// <devdoc>
        ///    Scales the local geometric transform by the
        ///    specified amounts. This method prepends the scaling matrix to the transform.
        /// </devdoc>
        public void ScaleTransform(float sx, float sy)
        { ScaleTransform(sx, sy, MatrixOrder.Prepend); }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.ScaleTransform1"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Scales the local geometric transform by the
        ///       specified amounts in the specified order.
        ///    </para>
        /// </devdoc>
        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipScaleLineTransform(new HandleRef(this, this.NativeBrush),
                                                        sx,
                                                        sy,
                                                        order);

            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);
        }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.RotateTransform"]/*' />
        /// <devdoc>
        ///    Rotates the local geometric transform by the
        ///    specified amount. This method prepends the rotation to the transform.
        /// </devdoc>
        public void RotateTransform(float angle)
        { RotateTransform(angle, MatrixOrder.Prepend); }

        /// <include file='doc\LinearGradientBrush.uex' path='docs/doc[@for="LinearGradientBrush.RotateTransform1"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Rotates the local geometric transform by the specified
        ///       amount in the specified order.
        ///    </para>
        /// </devdoc>
        public void RotateTransform(float angle, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipRotateLineTransform(new HandleRef(this, this.NativeBrush),
                                                         angle,
                                                         order);

            if (status != SafeNativeMethods.Gdip.Ok)
                throw SafeNativeMethods.Gdip.StatusException(status);
        }
        */
    }

}
