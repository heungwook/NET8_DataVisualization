//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation?
//   Copyright ?Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		GdiGraphics.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	GdiGraphics
//
//  Purpose:	GdiGraphics class is chart GDI+ rendering engine. It 
//              implements IChartRenderingEngine interface by mapping 
//              its methods to the drawing methods of GDI+. This 
//              rendering engine do not support animation.
//
//	Reviwed:	AG - Jul 15, 2003
//              AG - Microsoft 14, 2007
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
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL

using Orion.DataVisualization.Charting.Utilities;
using Orion.DataVisualization.Charting.Borders3D;
#else
    //using System.Web.UI.DataVisualization.Charting.Utilities;
    //using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif

using OrionX2.ConfigInfo;
using iTextSharp.text.pdf;
using System.Collections.Generic;

#endregion


#if Microsoft_CONTROL
namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{

    public class PdfGraphicsInfo
    {
        public static OrionX2.Render.RenderInfo _RI;
        public enum EnumColorSpace
        {
            DEFAULT = 0,
            DeviceRGB = 1,
            DeviceCMYK = 2,
            DeviceGray = 3
        }

        public PdfGraphicsInfo(OrionX2.OrionFont.FontManagerConfig fontMgr, System.Windows.Rect rectChart, double renderDpiX, double renderDpiY, bool lbUseKValueForCMYKEqual)
		{
			OrionX2.Render.RenderInfo._FontMgr = fontMgr;
			if (_RI == null)
				_RI = new OrionX2.Render.RenderInfo();
			this.DpiX = renderDpiX;
			this.DpiY = renderDpiY;
			this.cptChartLocationPt = new System.Windows.Point(UCNV.GetPointFromPixel(rectChart.X, this.DpiX),
														  UCNV.GetPointFromPixel(rectChart.Y, this.DpiY));
			this.cszChartSizePt = new System.Windows.Size(UCNV.GetPointFromPixel(rectChart.Width, this.DpiX),
														  UCNV.GetPointFromPixel(rectChart.Height, this.DpiY));
			this.cbUseKValueForCMYKEqual = lbUseKValueForCMYKEqual;
		}

		public bool cbUseKValueForCMYKEqual;
        public iTextSharp.text.Document cPdfDoc;
        public iTextSharp.text.pdf.PdfWriter cPdfWrt;
        public iTextSharp.text.pdf.PdfContentByte cPdfCnts { get { return cPdfWrt.DirectContent; } }
        public EnumColorSpace ceColorSpace;
        public System.Windows.Point cptChartLocationPt;
        public System.Windows.Size cszChartSizePt;
        public float ChartHeightPt { get => (float)(cszChartSizePt.Height); }
        public double DpiX { get { return _RI.cdDpiX; } set { _RI.cdDpiX = value; } }
        public double DpiY { get { return _RI.cdDpiY; } set { _RI.cdDpiY = value; } }
        public bool IsClipEmpty;
        public Region Clip;
        public Matrix Transform { set { TransformSet(value); } }
        public SmoothingMode SmoothingMode;
        public TextRenderingHint TextRenderingHint;
        public CompositingQuality CompositingQuality;
        public InterpolationMode InterpolationMode;


        public void DrawLine(
            Pen pen,
            PointF pt1px,
            PointF pt2px
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            float x1Pt = (float)UCNV.GetPointFromPixel(pt1px.X, this.DpiX);
            float y1Pt = (float)(this.ChartHeightPt - UCNV.GetPointFromPixel(pt1px.Y, this.DpiY));
            float x2Pt = (float)UCNV.GetPointFromPixel(pt2px.X, this.DpiX);
            float y2Pt = (float)(this.ChartHeightPt - UCNV.GetPointFromPixel(pt2px.Y, this.DpiY));
            this.cPdfCnts.MoveTo(x1Pt, y1Pt);
            this.cPdfCnts.LineTo(x2Pt, y2Pt);
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void DrawLines(
            Pen pen,
            PointF[] points
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            if (points == null || points.Length <= 1)
                return;

            for (int i = 0; i < points.Length; i++)
            {
                float x = (float)UCNV.GetPointFromPixel(points[i].X, this.DpiX);
                float y = (float)(this.ChartHeightPt - UCNV.GetPointFromPixel(points[i].Y, this.DpiX));
                if (i == 0)
                    this.cPdfCnts.MoveTo(x, y);
                else
                    this.cPdfCnts.LineTo(x, y);
            }
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void DrawEllipse(
            Pen pen,
            float x,
            float y,
            float width,
            float height
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            float widthPt = (float)UCNV.GetPointFromPixel(width, this.DpiX);
            float heightPt = (float)UCNV.GetPointFromPixel(height, this.DpiY);
            float xPt = (float)UCNV.GetPointFromPixel(x, this.DpiX);
            float yPt = (float)(this.ChartHeightPt - UCNV.GetPointFromPixel(y, this.DpiY));
            this.cPdfCnts.Ellipse(xPt, yPt, xPt + widthPt, yPt + heightPt);
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void DrawCurve(
            Pen pen,
            PointF[] points,
            int offset,
            int numberOfSegments,
            float tension
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            using (GraphicsPath lGP = new GraphicsPath())
            {
                lGP.AddCurve(points, offset, numberOfSegments, tension);
                WriteGraphicsPath(lGP.PathData, true, true);
            }
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void DrawRectangle(
            Pen pen,
            float x,
            float y,
            float width,
            float height
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            float widthPt = (float)UCNV.GetPointFromPixel(width, this.DpiX);
            float heightPt = (float)UCNV.GetPointFromPixel(height, this.DpiY);
            float xPt = (float)UCNV.GetPointFromPixel(x, this.DpiX);
            float yPt = this.ChartHeightPt - heightPt - (float)UCNV.GetPointFromPixel(y, this.DpiY);
            this.cPdfCnts.Rectangle(xPt, yPt, widthPt, heightPt);
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void DrawPolygon(
            Pen pen,
            PointF[] points
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            if (points == null || points.Length <= 1)
                return;
            float x0 = 0f;
            float y0 = 0f;
            for(int i = 0; i < points.Length; i++)
			{
                float x = (float)UCNV.GetPointFromPixel(points[i].X, this.DpiX);
                float y = this.ChartHeightPt - (float)UCNV.GetPointFromPixel(points[i].Y, this.DpiX);
                if (i == 0)
                {
                    x0 = x;
                    y0 = y;
                    this.cPdfCnts.MoveTo(x, y);
                }
                else
                    this.cPdfCnts.LineTo(x, y);
            }
            this.cPdfCnts.LineTo(x0, y0);
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }



        public void DrawPath(
            Pen pen,
            GraphicsPath path
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            if (path == null || path.PathData == null)
                return;

            WriteGraphicsPath(path.PathData, true, true);
        
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }


        public void DrawPie(
            Pen pen,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            using (GraphicsPath lGP = new GraphicsPath())
            {
                lGP.AddPie(x, y, width, height, startAngle, sweepAngle);
                WriteGraphicsPath(lGP.PathData, true, true);
            }
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void DrawArc(
            Pen pen,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            if (pen.Color.IsEmpty || pen.Color.A == 0)
                return;
            using (GraphicsPath lGP = new GraphicsPath())
            {
                lGP.AddArc(x, y, width, height, startAngle, sweepAngle);
                WriteGraphicsPath(lGP.PathData, true, true);
            }
            pen.ToPdfPen(this);
            this.cPdfCnts.Stroke();
        }

        public void FillEllipse(
            Brush brush,
            RectangleF rect
        )
        {
            if (brush.IsEmptyColor())
                return;
            float widthPt = (float)UCNV.GetPointFromPixel(rect.Width, this.DpiX);
            float heightPt = (float)UCNV.GetPointFromPixel(rect.Height, this.DpiY);
            float xPt = (float)UCNV.GetPointFromPixel(rect.X, this.DpiX);
            float yPt = this.ChartHeightPt - heightPt - (float)UCNV.GetPointFromPixel(rect.Y, this.DpiY);
            this.cPdfCnts.Ellipse(xPt, yPt, xPt + widthPt, yPt + heightPt);
            brush.ToPdfBrush(this);
            this.cPdfCnts.Fill();
        }

        public void FillPath(
            Brush brush,
            GraphicsPath path
            )
        {
            if (brush.IsEmptyColor())
                return;
            if (path == null || path.PathData == null)
                return;

            if (brush.GetBrushType().Equals(typeof(LinearGradientBrush)) 
                && ((LinearGradientBrush)brush).WrapMode == WrapMode.TileFlipX)
            {
                WriteGraphicsPath(path.PathData, true, true);
                ((LinearGradientBrush)brush).ToPdfBrushFlipXLeft(this);
                this.cPdfCnts.Fill();
                WriteGraphicsPath(path.PathData, true, true);
                ((LinearGradientBrush)brush).ToPdfBrushFlipXRight(this);
                this.cPdfCnts.Fill();
            }
            else
            {
                WriteGraphicsPath(path.PathData, true, true);
                brush.ToPdfBrush(this);
                this.cPdfCnts.Fill();
            }
        }

        public void FillRegion(
            Brush brush,
            Region region
            )
        {
            if (brush.IsEmptyColor())
                return;
            if (region == null)
                return;

            byte[] regionData = region.GetRegionData().Data;
            //RectangleF[] regionScans = region.

            //WriteGraphicsPath(path.PathData, true, true);

            //brush.ToPdfBrush(this);
            //this.cPdfCnts.Fill();
        }

        public void FillRectangle(
            Brush brush,
            RectangleF rect
            )
        {
            if (brush.IsEmptyColor())
                return;
            this.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);

        }

        public void FillRectangle(
            Brush brush,
            float x,
            float y,
            float width,
            float height
            )
        {
            if (brush.IsEmptyColor())
                return;
            float widthPt = (float)UCNV.GetPointFromPixel(width, this.DpiX);
            float heightPt = (float)UCNV.GetPointFromPixel(height, this.DpiY);
            float xPt = (float)UCNV.GetPointFromPixel(x, this.DpiX);
            float yPt = this.ChartHeightPt - heightPt - (float)UCNV.GetPointFromPixel(y, this.DpiY);
            this.cPdfCnts.Rectangle(xPt, yPt, widthPt, heightPt);
            brush.ToPdfBrush(this);
            this.cPdfCnts.Fill();
        }

        public void FillPolygon(
            Brush brush,
            PointF[] points
            )
        {
            if (brush.IsEmptyColor())
                return;
            if (points == null || points.Length <= 1)
                return;
            float x0 = 0f;
            float y0 = 0f;
            for (int i = 0; i < points.Length; i++)
            {
                float x = (float)UCNV.GetPointFromPixel(points[i].X, this.DpiX);
                float y = this.ChartHeightPt - (float)UCNV.GetPointFromPixel(points[i].Y, this.DpiX);
                if (i == 0)
                {
                    x0 = x;
                    y0 = y;
                    this.cPdfCnts.MoveTo(x, y);
                }
                else
                    this.cPdfCnts.LineTo(x, y);
            }
            this.cPdfCnts.LineTo(x0, y0);
            brush.ToPdfBrush(this);
            this.cPdfCnts.Fill();
        }

        public void FillPie(
            Brush brush,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            if (brush.IsEmptyColor())
                return;
            using (GraphicsPath lGP = new GraphicsPath())
            {
                lGP.AddPie(x, y, width, height, startAngle, sweepAngle);
                WriteGraphicsPath(lGP.PathData, true, true);
            }
            brush.ToPdfBrush(this);
            this.cPdfCnts.Fill();
        }



        //
        // Common Methods for PDF
        internal void SetColorStroke(Color lColor)
        {
            if (lColor.IsEmpty || lColor.A == 0)
                return;

            if (this.ceColorSpace == EnumColorSpace.DeviceGray)
            {
                this.cPdfCnts.SetColorStroke(new GrayColor((float)lColor.GetGray() / 255F));
            }
            else if (this.ceColorSpace == EnumColorSpace.DeviceRGB)
            {
                this.cPdfCnts.SetColorStroke(new iTextSharp.text.BaseColor(lColor.ToColor()));
            }
            else if (this.ceColorSpace == EnumColorSpace.DeviceCMYK)
            {
                if (lColor.IsSpotColor)
                {
                    PdfSpotColor lSpotColor = new PdfSpotColor(
                        lColor.SpotColorName,
                        new CMYKColor((float)lColor.CMYK.C, (float)lColor.CMYK.M,
                                      (float)lColor.CMYK.Y, (float)lColor.CMYK.K));
                    this.cPdfCnts.SetColorStroke(new SpotColor(lSpotColor, (float)lColor.SpotColorTint));
                }
                else
                {
                    if (this.cbUseKValueForCMYKEqual &&
                        (lColor.CMYK.C == lColor.CMYK.K && lColor.CMYK.M == lColor.CMYK.K && lColor.CMYK.Y == lColor.CMYK.K))
                    {
                        this.cPdfCnts.SetColorStroke(new CMYKColor(0F, 0F, 0F, (float)lColor.CMYK.K));
                    }
                    else
                    {
                        this.cPdfCnts.SetColorStroke(new CMYKColor(
                                (float)lColor.CMYK.C, (float)lColor.CMYK.M,
                                (float)lColor.CMYK.Y, (float)lColor.CMYK.K));
                    }
                }
            }
        }
        internal void SetColorFill(Color lColor)
        {
            if (lColor.IsEmpty || lColor.A == 0)
                return;

            if (this.ceColorSpace == EnumColorSpace.DeviceGray)
            {
                this.cPdfCnts.SetColorFill(new GrayColor(lColor.GetGray()));
            }
            else if (this.ceColorSpace == EnumColorSpace.DeviceRGB)
            {
                this.cPdfCnts.SetColorFill(new iTextSharp.text.BaseColor(lColor.ToColor()));
            }
            else if (this.ceColorSpace == EnumColorSpace.DeviceCMYK)
            {
                if (lColor.IsSpotColor)
                {
                    PdfSpotColor lSpotColor = new PdfSpotColor(
                        lColor.SpotColorName,
                        new CMYKColor((float)lColor.CMYK.C, (float)lColor.CMYK.M,
                                      (float)lColor.CMYK.Y, (float)lColor.CMYK.K) );
                    this.cPdfCnts.SetColorFill(new SpotColor(lSpotColor, (float)lColor.SpotColorTint));
                }
                else
                {
                    if (this.cbUseKValueForCMYKEqual && 
                        (lColor.CMYK.C == lColor.CMYK.K && lColor.CMYK.M == lColor.CMYK.K && lColor.CMYK.Y == lColor.CMYK.K))
					{
                        this.cPdfCnts.SetColorFill(new CMYKColor(0F, 0F, 0F, (float)lColor.CMYK.K));
                    }
                    else
					{
                        this.cPdfCnts.SetColorFill(new CMYKColor((float)lColor.CMYK.C, (float)lColor.CMYK.M,
                                                                 (float)lColor.CMYK.Y, (float)lColor.CMYK.K));

                    }
                    
                }

            }
        }
        //
        public void CurveTo(List<PointF> llptfCurveTo)
        {
            if (llptfCurveTo.Count == 2)
                this.cPdfCnts.CurveTo(llptfCurveTo[0].X, llptfCurveTo[0].Y, llptfCurveTo[1].X, llptfCurveTo[1].Y);
            else if (llptfCurveTo.Count == 3)
                this.cPdfCnts.CurveTo(llptfCurveTo[0].X, llptfCurveTo[0].Y, llptfCurveTo[1].X, llptfCurveTo[1].Y, llptfCurveTo[2].X, llptfCurveTo[2].Y);
        }
        public void SetLineDash(DashStyle ldsDashStyle, float lfLineWidth)
        {
            if (lfLineWidth == 0F)
                lfLineWidth = 0.1F;

            float lfDot = lfLineWidth;
            float lfDash = lfLineWidth * 3F;

            if (ldsDashStyle == DashStyle.Solid)
            {
                this.cPdfCnts.SetLineDash(lfDash);
            }
            else if (ldsDashStyle == DashStyle.Dot)
            {
                float[] lfaDash = { lfDot, lfDot };
                this.cPdfCnts.SetLineDash(lfaDash, 0);
            }
            else if (ldsDashStyle == DashStyle.Dash)
            {
                float[] lfaDash = { lfDash, lfDot };
                this.cPdfCnts.SetLineDash(lfaDash, 0);
            }
            else if (ldsDashStyle == DashStyle.DashDot)
            {
                float[] lfaDash = { lfDash, lfDot, lfDot, lfDot };
                this.cPdfCnts.SetLineDash(lfaDash, 0);
            }
            else if (ldsDashStyle == DashStyle.DashDotDot)
            {
                float[] lfaDash = { lfDash, lfDot, lfDot, lfDot, lfDot, lfDot };
                this.cPdfCnts.SetLineDash(lfaDash, 0);
            }
            else
            {
                this.cPdfCnts.SetLineDash(lfDash, 0);
            }

        }

        public void SetLineJoin(LineJoin lineJoin)
		{
            if (lineJoin == LineJoin.Bevel)
                cPdfCnts.SetLineJoin(PdfContentByte.LINE_JOIN_BEVEL);
            else if (lineJoin == LineJoin.Miter)
                cPdfCnts.SetLineJoin(PdfContentByte.LINE_JOIN_MITER);
            else if (lineJoin == LineJoin.Round)
                cPdfCnts.SetLineJoin(PdfContentByte.LINE_JOIN_ROUND);
        }

        public void SetLineCap(LineCap lineCap)
        {
            if (lineCap == LineCap.Round)
                cPdfCnts.SetLineCap(PdfContentByte.LINE_CAP_ROUND);
            else if (lineCap == LineCap.Flat)
                cPdfCnts.SetLineJoin(PdfContentByte.LINE_CAP_BUTT);
            else if (lineCap == LineCap.Square)
                cPdfCnts.SetLineJoin(PdfContentByte.LINE_CAP_PROJECTING_SQUARE);
        }

        //
        //
        public void WriteFontPath(PathData lPathD, float lfDpiX, float lfDpiY, bool lbNoClosePathAtEnd)
        {
            PointF[] lptfaPathPoint = lPathD.Points;
            List<PointF> llptfCurveTo = new List<PointF>();
            byte[] lbyaPathType = lPathD.Types;
            bool lbIsLastCommandCLOSEPATH = true;
            for (int idx = 0; idx < lptfaPathPoint.Length; idx++)
            {
                PointF lPP = new PointF(UCNV.GetPointFromPixel(lptfaPathPoint[idx].X, lfDpiX),
                                        -UCNV.GetPointFromPixel(lptfaPathPoint[idx].Y, lfDpiY));

                //lPP.Y = -lPP.Y;
                byte lPT = lbyaPathType[idx];

                if (llptfCurveTo.Count == 3)
                {
                    this.CurveTo(llptfCurveTo);
                    llptfCurveTo.Clear();
                }
                if (lPT >= 0x80)
                {
                    if (llptfCurveTo.Count == 0)
                    {
                        this.cPdfCnts.LineTo(lPP.X, lPP.Y);
                    }
                    else
                    {
                        llptfCurveTo.Add(lPP);
                        this.CurveTo(llptfCurveTo);
                        llptfCurveTo.Clear();
                    }
                }
                if (lPT == 0x00)    // Indicates that the point is the start of a figure.
                {
                    if (!lbIsLastCommandCLOSEPATH)
                        this.cPdfCnts.ClosePath();

                    this.cPdfCnts.MoveTo(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x01)   // Indicates that the point is one of the two endpoints of a line.
                {
                    this.cPdfCnts.LineTo(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x03)   // Indicates that the point is an endpoint or control point of a cubic Bezier spline.
                {
                    llptfCurveTo.Add(lPP);
                    lbIsLastCommandCLOSEPATH = false;
                    //PS_LineTo(lFStream, lPP.X, lPP.Y);
                }
                else if (lPT == 0x07)   // Masks all bits except for the three low-order bits, which indicate the point type.
                {
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x20)   // Specifies that the point is a marker.
                {
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x80)   // Specifies that the point is the last point in a closed subpath (figure).
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0x83)
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0xa3)
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else
                {
                }
            }
            if (llptfCurveTo.Count == 3)
            {
                this.CurveTo(llptfCurveTo);
                llptfCurveTo.Clear();

            }
            if (!lbIsLastCommandCLOSEPATH && !lbNoClosePathAtEnd)
                this.cPdfCnts.ClosePath();

        }
        public void WriteGraphicsPath(PathData lPathD, bool lbIsPixelData, bool lbNoClosePathAtEnd)
        {
            PointF[] lptfaPathPoint = lPathD.Points;
            List<PointF> llptfCurveTo = new List<PointF>();
            byte[] lbyaPathType = lPathD.Types;
            bool lbIsLastCommandCLOSEPATH = true;
            for (int idx = 0; idx < lptfaPathPoint.Length; idx++)
            {
                PointF lPP;
                if (lbIsPixelData)
                {
                    lPP = new PointF((float)UCNV.GetPointFromPixel(lptfaPathPoint[idx].X, this.DpiX),
                                     this.ChartHeightPt - (float)UCNV.GetPointFromPixel(lptfaPathPoint[idx].Y, this.DpiY));
                }
                else
                {
                    lPP = lptfaPathPoint[idx];
                }
                //lPP.Y = -lPP.Y;
                byte lPT = lbyaPathType[idx];

                if (llptfCurveTo.Count == 3)
                {
                    this.CurveTo(llptfCurveTo);
                    llptfCurveTo.Clear();
                }
                if (lPT >= 0x80)
                {
                    if (llptfCurveTo.Count == 0)
                    {
                        this.cPdfCnts.LineTo(lPP.X, lPP.Y);
                    }
                    else
                    {
                        llptfCurveTo.Add(lPP);
                        this.CurveTo(llptfCurveTo);
                        llptfCurveTo.Clear();
                    }
                }
                if (lPT == 0x00)    // Indicates that the point is the start of a figure.
                {
                    if (!lbIsLastCommandCLOSEPATH)
                        this.cPdfCnts.ClosePath();

                    this.cPdfCnts.MoveTo(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x01)   // Indicates that the point is one of the two endpoints of a line.
                {
                    this.cPdfCnts.LineTo(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x03)   // Indicates that the point is an endpoint or control point of a cubic Bezier spline.
                {
                    llptfCurveTo.Add(lPP);
                    lbIsLastCommandCLOSEPATH = false;
                    //PS_LineTo(lFStream, lPP.X, lPP.Y);
                }
                else if (lPT == 0x07)   // Masks all bits except for the three low-order bits, which indicate the point type.
                {
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x20)   // Specifies that the point is a marker.
                {
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x80)   // Specifies that the point is the last point in a closed subpath (figure).
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0x83)
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0xa3)
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else
                {
                }
            }
            if (llptfCurveTo.Count == 3)
            {
                this.CurveTo(llptfCurveTo);
                llptfCurveTo.Clear();

            }
            if (!lbIsLastCommandCLOSEPATH && !lbNoClosePathAtEnd)
                this.cPdfCnts.ClosePath();

        }
        public void WriteGraphicsPath(PathData lPathD, bool lbHeightInverse, float lfHeight, bool lbNoClosePathAtEnd)
        {
            PointF[] lptfaPathPoint = lPathD.Points;
            List<PointF> llptfCurveTo = new List<PointF>();
            byte[] lbyaPathType = lPathD.Types;
            bool lbIsLastCommandCLOSEPATH = true;
            for (int idx = 0; idx < lptfaPathPoint.Length; idx++)
            {
                PointF lPP = lptfaPathPoint[idx];
                if (lbHeightInverse)
                {
                    lPP.Y = lfHeight - lPP.Y;
                }
                byte lPT = lbyaPathType[idx];

                if (llptfCurveTo.Count == 3)
                {
                    this.CurveTo(llptfCurveTo);
                    llptfCurveTo.Clear();
                }
                if (lPT >= 0x80)
                {
                    if (llptfCurveTo.Count == 0)
                    {
                        this.cPdfCnts.LineTo(lPP.X, lPP.Y);
                    }
                    else
                    {
                        llptfCurveTo.Add(lPP);
                        this.CurveTo(llptfCurveTo);
                        llptfCurveTo.Clear();
                    }
                }
                if (lPT == 0x00)    // Indicates that the point is the start of a figure.
                {
                    if (!lbIsLastCommandCLOSEPATH)
                        this.cPdfCnts.ClosePath();

                    this.cPdfCnts.MoveTo(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x01)   // Indicates that the point is one of the two endpoints of a line.
                {
                    this.cPdfCnts.LineTo(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x03)   // Indicates that the point is an endpoint or control point of a cubic Bezier spline.
                {
                    llptfCurveTo.Add(lPP);
                    lbIsLastCommandCLOSEPATH = false;
                    //PS_LineTo(lFStream, lPP.X, lPP.Y);
                }
                else if (lPT == 0x07)   // Masks all bits except for the three low-order bits, which indicate the point type.
                {
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x20)   // Specifies that the point is a marker.
                {
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x80)   // Specifies that the point is the last point in a closed subpath (figure).
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0x83)
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0xa3)
                {
                    this.cPdfCnts.ClosePath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else
                {
                }
            }
            if (llptfCurveTo.Count == 3)
            {
                this.CurveTo(llptfCurveTo);
                llptfCurveTo.Clear();

            }
            if (!lbIsLastCommandCLOSEPATH && !lbNoClosePathAtEnd)
                this.cPdfCnts.ClosePath();

        }

        public void DrawString(
            string s,
            Font font,
            Brush brush,
            PointF point,
            StringFormat format
            )
        {
            this.DrawString(s, font, brush, point.X, point.Y, 0F, 0F, format);
        }

        public void DrawString(
            string s,
            Font font,
            Brush brush,
            RectangleF layoutRectangle,
            StringFormat format)
		{
            this.DrawString(s, font, brush, layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, format);
        }

        public void DrawString(
            string s,
            Font font,
            Brush brush,
            float xPx, float yPx, float widthPx, float heightPx,
            StringFormat format)
        {

            if (string.IsNullOrWhiteSpace(s))
                return;

            OrionX2.ItemHandler.ItemText textItem = GetItemTextFromGdiFont(s, font, format, xPx, yPx, widthPx, heightPx, this.DpiX, this.DpiY);
            if (textItem == null)
                throw new Exception("GetItemTextFromGdiFont() returns NULL");


            this.cPdfCnts.SaveState();
            try
            {
                if (textItem.cFont.cFInfo == null)
                    textItem.cFont.cFInfo = OrionX2.Render.RenderInfo._FontMgr.FindFontInfo(font.FontFamily.Name);
                if (textItem.cFont.cFInfo == null)
                    textItem.cFont.cFInfo = new OrionX2.OrionFont.FontInfo();
                BaseFont pdfBFont = textItem.cFont.cFInfo.GetPDFBaseFont();
                if (pdfBFont == null)
                {
                    try
                    {
                        pdfBFont = iTextSharp.text.FontFactory.GetFont(font.FontFamily.Name, BaseFont.IDENTITY_H, BaseFont.EMBEDDED).BaseFont;
                    }
					catch
					{
                        pdfBFont = iTextSharp.text.FontFactory.GetFont("malgun.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED).BaseFont;
                    }
                }
                if (brush.GetBrushType().Equals(typeof(SolidBrush)))
				{
                    Color colorBrush = (((SolidBrush)brush).Color);
                    textItem.cForeground = OrionColor.FromACMYKBytes((byte)colorBrush.A,
                        (byte)colorBrush.C, (byte)colorBrush.M, (byte)colorBrush.Y, (byte)colorBrush.K);
                }
                else
                {
                    textItem.cForeground = OrionColor.FromCMYKBytes(0, 0, 0, 255);
                }
                
                System.Windows.Size lszTBoxPX;
                OrionX2.ItemHandler.TextAdjustment.SizeFChar[] lszfaChars = OrionX2.OrionFont.GlyphInfoCache.CharsSizeMeasure(textItem, _RI, null, out lszTBoxPX);
                OrionX2.ItemHandler.BoxInfo lBox = new OrionX2.ItemHandler.BoxInfo(textItem, _RI, lszTBoxPX);
                List<OrionX2.ItemHandler.TextAdjustment.LineChars> llLNChars = textItem.cTAdjustment.SetLineAlignment_Text(textItem, _RI, ref lBox, lszfaChars);
                //

                brush.ToPdfBrush(this);
                this.cPdfCnts.SetFontAndSize(pdfBFont, (float)textItem.cFont.cdSize);
                this.cPdfCnts.SetTextRise(0F);
                this.cPdfCnts.SetLeading(0F);
                if (textItem.cFont.cbBold)
                {
                    this.cPdfCnts.SetLineWidth((float)textItem.cFont.cdSize / 100F * 3F);
                    this.SetLineDash(DashStyle.Solid, (float)textItem.cFont.cdSize / 100F * 3F);
                    this.cPdfCnts.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
                }
                else
                {
                    this.cPdfCnts.SetLineWidth((float)textItem.cFont.cdSize / 100F);
                    this.cPdfCnts.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
                }

                float boxWidthPt = (float)UCNV.GetPointFromPixel(lBox.cSizePX.Width, this.DpiX);
                float boxHeightPt = (float)UCNV.GetPointFromPixel(lBox.cSizePX.Height, this.DpiY);
                float boxLeftPt = (float)UCNV.GetPointFromPixel(lBox.cLocationPX.X + lBox.cLocationAdjPX.X, this.DpiX);
                float boxBottomPt = this.ChartHeightPt - boxHeightPt -
                    (float)UCNV.GetPointFromPixel(lBox.cLocationPX.Y + lBox.cLocationAdjPX.Y, this.DpiX);
                iTextSharp.awt.geom.AffineTransform transBox = new iTextSharp.awt.geom.AffineTransform();
                transBox.Translate(boxLeftPt, boxBottomPt);
                this.cPdfCnts.Transform(transBox);

                float lfItalicization = 0F;
                if (textItem.cFont.cbItalic)
                    lfItalicization = (float)textItem.cFont.cdSize / 20F;
                float lfAspectRatio = (float)textItem.cFont.cdAspectRatio / 100F;
                for (int iLine = 0; iLine < llLNChars.Count; iLine++)
                {
                    OrionX2.ItemHandler.TextAdjustment.LineChars lineChars = llLNChars[iLine];
                    if (lineChars.LSZFCH == null || lineChars.LSZFCH.Count == 0)
                        continue;
                    this.cPdfCnts.SaveState();
                    try
                    {
                        //float charBaseLinePt = (float)UCNV.GetPointFromPixel(lineChars.LSZFCH[0].cGlyph.cdBaseline, this.DpiY) * 2.0F;
                        float charHMargen = (float)textItem.cFont.cdSize * 0.275F;
                        float lineLeftPt = (float)UCNV.GetPointFromPixel(lineChars.PTF.X, this.DpiX);
                        float lineBottomPt = (float)UCNV.GetPointFromPixel(lineChars.PTF.Y, this.DpiY);

                        if (lfAspectRatio == 1.0F)
                        {
                            this.cPdfCnts.ConcatCTM(1F, 0F, lfItalicization, 1F, lineLeftPt, lineBottomPt + charHMargen);
                        }
                        else
						{
                            iTextSharp.awt.geom.AffineTransform transLine = new iTextSharp.awt.geom.AffineTransform();
                            transLine.Translate(lineLeftPt, lineBottomPt + charHMargen);
                            transLine.Scale(lfAspectRatio, 1);
                            this.cPdfCnts.ConcatCTM(transLine);
                            this.cPdfCnts.ConcatCTM(1F, 0F, lfItalicization, 1F, 0, 0);
                        }
                        foreach (OrionX2.ItemHandler.TextAdjustment.SizeFChar lSZFCH in lineChars.LSZFCH)
                        {
                            bool lbFontChanged = false;
                            try
                            {
                                if (!pdfBFont.CharExists(lSZFCH.CH)) 
                                {
                                    iTextSharp.text.pdf.BaseFont pdfDefaultFont = OrionX2.Render.RenderInfo._FontMgr.GetDefaultPdfBaseFont();
                                    if (pdfDefaultFont != null)
									{
                                        lbFontChanged = true;
                                        this.cPdfCnts.SetFontAndSize(pdfDefaultFont, (float)textItem.cFont.cdSize);
                                    }
                                }

                                SizeF charGapPt = new SizeF((float)UCNV.GetPointFromPixel((float)lSZFCH.GapHorz, this.DpiX),
                                                              (float)UCNV.GetPointFromPixel((float)lSZFCH.GapVert, this.DpiY));
                                this.cPdfCnts.BeginText();
                                this.cPdfCnts.ShowText(lSZFCH.CH.ToString());
                                this.cPdfCnts.EndText();
                                this.cPdfCnts.ConcatCTM(1, 0, 0, 1, (float)(charGapPt.Width / lfAspectRatio), 0);
                            }
                            catch { }
                            finally
							{
                                if (lbFontChanged)
                                    this.cPdfCnts.SetFontAndSize(pdfBFont, (float)textItem.cFont.cdSize);
                            }
                        }
                    }
                    finally
                    {
                        this.cPdfCnts.RestoreState();
                    }
                    /*
                    for (int iChar = 0;iChar < lineChars.LSZFCH.Count;iChar++)
					{
                        float charYPt = lineYPt - (float)UCNV.GetPointFromPixel(lineChars.LSZFCH[iChar].CharHeight, DpiY);
                        this.cPdfCnts.BeginText();
                        this.cPdfCnts.SetTextMatrix(charXPt, charYPt);
                        this.cPdfCnts.ShowText(lineChars.LSZFCH[iChar].CH.ToString());
                        this.cPdfCnts.EndText();
                        float charGapPt = (float)UCNV.GetPointFromPixel(lineChars.LSZFCH[iChar].GapHorz, DpiX);
                        charXPt += charGapPt;
                    }
                    */


                }

            }
            finally
            {
                this.cPdfCnts.RestoreState();
            }
        }

        public static OrionX2.ItemHandler.ItemText GetItemTextFromGdiFont(string textCnts, Font fontGdi, StringFormat formatText,
            double xPx, double yPx, double widthPx, double heightPx, double dpiX, double dpiY)
        {
            OrionX2.ItemHandler.ItemText textItem = new OrionX2.ItemHandler.ItemText();

            float fontSizePt = 10; //font.SizeInPoints; ;// * 96.0F / (float)this.DpiX;
            if (fontGdi.Unit == GraphicsUnit.Pixel)
            {
                fontSizePt = (float)UCNV.GetPointFromPixel(fontGdi.Size, dpiX);
            }
            else if (fontGdi.Unit == GraphicsUnit.Display)
            {
                fontSizePt = (float)UCNV.GetDIUFromPixel(fontGdi.Size, dpiX);
            }
            else
                fontSizePt = fontGdi.SizeInPoints * 96.0F / (float)dpiX;

            string lsFontNameTmp = fontGdi.FontFamily.Name;

            try
            {
                double xMM = UCNV.GetMMFromPixel(xPx, dpiX);
                double yMM = UCNV.GetMMFromPixel(yPx, dpiY);
                double widthMM = UCNV.GetMMFromPixel(widthPx, dpiX);
                double heightMM = UCNV.GetMMFromPixel(heightPx, dpiY);
                
                textItem.cLocation = new OrionX2.Structs.NLocation(xMM, yMM);
                textItem.cSize = new OrionX2.Structs.NSize(widthMM, heightMM);
                textItem.SetData(_RI, textCnts);
                textItem.cFont.cFInfo = OrionX2.Render.RenderInfo._FontMgr.FindFontInfo(lsFontNameTmp);
                if (textItem.cFont.cFInfo == null)
                {
                    textItem.cFont.cFInfo = new OrionX2.OrionFont.FontInfo();

                }
                textItem.cFont.cdRotation = 0;
                textItem.cFont.cdSize = fontSizePt;
                textItem.cFont.cbBold = fontGdi.Bold;
                textItem.cFont.cbItalic = fontGdi.Italic;
                textItem.cFont.cbUnderline = fontGdi.Underline;
                textItem.cFont.cbStrikethru = fontGdi.Strikeout;
                //textItem.cFont.cdAspectRatio = 100;
                if (formatText.Alignment == StringAlignment.Near)
                    textItem.ceHorzAlign = OrionX2.Enums.HorizontalAlignment.Left;
                else if (formatText.Alignment == StringAlignment.Center)
                    textItem.ceHorzAlign = OrionX2.Enums.HorizontalAlignment.Center;
                else if (formatText.Alignment == StringAlignment.Far)
                {
                    textItem.ceHorzAlign = OrionX2.Enums.HorizontalAlignment.Right;
                    textItem.cLocation.X -= UCNV.GetMMFromPoint(fontSizePt) * 0.25;
                }
                if (formatText.LineAlignment == StringAlignment.Near)
                    textItem.ceVertAlign = OrionX2.Enums.VerticalAlignment.Top;
                else if (formatText.LineAlignment == StringAlignment.Center)
                    textItem.ceVertAlign = OrionX2.Enums.VerticalAlignment.Center;
                else if (formatText.LineAlignment == StringAlignment.Far)
                    textItem.ceVertAlign = OrionX2.Enums.VerticalAlignment.Bottom;

                textItem.cForeground = new OrionColor(System.Drawing.Color.FromArgb(255, 0, 0, 0));
                // Prepare to draw string
                if (textItem.cTAdjustment.cbUseTextAutoSize && textItem.cSize.Width > 0)
                {
                    if (textItem.cTAdjustment.cbUseLineAutoWrap && textItem.cTAdjustment.cbUseLineWrapForce)
                    {
                        textItem.cTAdjustment.SetOptimalSize_ForceLineBreak(textItem, _RI, null);
                    }
                    else
                    {
                        textItem.cTAdjustment.SetOptimalSize(textItem, _RI, null);
                    }
                }
                else
                {
                    textItem.cTAdjustment.SetDefaultSize(textItem);
                }
            }
            catch
            {
                return null;
            }
            return textItem;
        }

        public static SizeF MeasureString(
            string text,
            Font font, double dpiX, double dpiY)
        {
            return MeasureString(text, font, new SizeF(0, 0), new StringFormat(), dpiX, dpiY);
        }
        public static SizeF MeasureString(
            string text,
            Font font,
            SizeF layoutArea,
            StringFormat stringFormat, double dpiX, double dpiY)
        {
            SizeF sizeText = new SizeF(0, 0);
            try
            {
                OrionX2.ItemHandler.ItemText textItem = GetItemTextFromGdiFont(text, font, stringFormat, 0, 0, layoutArea.Width, layoutArea.Height, dpiX, dpiY);
                System.Windows.Size lszTBoxPX;
                OrionX2.ItemHandler.TextAdjustment.SizeFChar[] lszfaChars = OrionX2.OrionFont.GlyphInfoCache.CharsSizeMeasure(textItem, _RI, null, out lszTBoxPX);
                OrionX2.ItemHandler.BoxInfo lBox = new OrionX2.ItemHandler.BoxInfo(textItem, _RI, lszTBoxPX);
                List<OrionX2.ItemHandler.TextAdjustment.LineChars> llLNChars = textItem.cTAdjustment.SetLineAlignment_Text(textItem, _RI, ref lBox, lszfaChars);
                RectangleF rtfAllLines = OrionX2.ItemHandler.TextAdjustment.GetAllLinesRTF(llLNChars);

                sizeText = new SizeF(rtfAllLines.Size.Width, rtfAllLines.Height);
            }
            catch
            {
                sizeText = new SizeF(0, 0);
            }
            return sizeText;

        }

        public void Save()
		{
            this.cPdfCnts.SaveState();
        }

        public void Restore()
		{
            this.cPdfCnts.RestoreState();
        }

        public void TranslateTransform(
            float dx,
            float dy
            )
        {
            iTextSharp.awt.geom.AffineTransform trnsfrm = new iTextSharp.awt.geom.AffineTransform();
            trnsfrm.Translate((float)UCNV.GetPointFromPixel(dx, this.DpiX), this.ChartHeightPt - (float)UCNV.GetPointFromPixel(dy, this.DpiY));
            this.cPdfCnts.Transform(trnsfrm);
        }


        public void TransformSet(Matrix mtrx)
		{
            iTextSharp.awt.geom.AffineTransform atrnsfrm = new iTextSharp.awt.geom.AffineTransform();
            double m00, m10, m01, m11, m02, m12;
            float[] mtrxElements = mtrx.Elements;
            m00 = mtrxElements[0];
            m01 = mtrxElements[1];
            m10 = mtrxElements[2];
            m11 = mtrxElements[3];
            m02 = UCNV.GetPointFromPixel(mtrxElements[4], this.DpiX);
            m12 = this.ChartHeightPt - UCNV.GetPointFromPixel(mtrxElements[5], this.DpiY);
            atrnsfrm.SetTransform(m00, m10, m01, m11, m02, m12);
        }

    }


}
