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
using System.Text;
using System.IO;

#endregion


#if Microsoft_CONTROL
namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{

    public class PSGraphicsInfo
    {
        public static OrionX2.Render.RenderInfo _RI;

        public PSGraphicsInfo(Stream lStrmHeader, Stream lStrmBody, OrionX2.OrionFont.FontManagerConfig fontMgr, System.Windows.Rect rectChart, double renderDpiX, double renderDpiY)
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
            cStrmHeader = lStrmHeader;
            cStrmBody = lStrmBody;
            cdctFontData = new Dictionary<string, PSFontData>();
        }

        public Dictionary<string, PSFontData> cdctFontData;
        public Stream cStrmHeader;
        public Stream cStrmBody;
        public PdfGraphicsInfo.EnumColorSpace ceColorSpace;
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



        // Command Aliases 

        internal static string csPSconcat = "cm";
        internal static string csPSmoveto = "m";
        internal static string csPSlineto = "l";
        internal static string csPScurveto = "c";
        internal static string csPSgsave = "q";
        internal static string csPSgrestore = "Q";
        internal static string csPSsave = "a";
        internal static string csPSrestore = "A";
        internal static string csPSshowpage = "b";
        internal static string csPStranslate = "T";
        internal static string csPSsetrgbcolor = "r";
        internal static string csPSfill = "f";
        internal static string csPSeofill = "f*";
        internal static string csPSclosepath = "h";
        internal static string csPSsetlinewidth = "w";
        internal static string csPSscale = "s";
        internal static string csPSstroke = "S";
        internal static string csPSsetdash = "d";
        //

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
            this.PS_moveto(x1Pt, y1Pt);
            this.PS_lineto(x2Pt, y2Pt);
            pen.ToPSPen(this);
            this.PS_stroke();
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
                    this.PS_moveto(x, y);
                else
                    this.PS_lineto(x, y);
            }
            pen.ToPSPen(this);
            this.PS_stroke();
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
            //float widthPt = (float)UCNV.GetPointFromPixel(width, this.DpiX);
            //float heightPt = (float)UCNV.GetPointFromPixel(height, this.DpiY);
            //float xPt = (float)UCNV.GetPointFromPixel(x, this.DpiX);
            //float yPt = (float)(this.ChartHeightPt - UCNV.GetPointFromPixel(y, this.DpiY));
            //this.cPdfCnts.Ellipse(xPt, yPt, xPt + widthPt, yPt + heightPt);
            using (GraphicsPath lGP = new GraphicsPath())
			{
                lGP.AddEllipse(x, y, width, height);
                WriteGraphicsPath(lGP.PathData, true, true);
            }
            pen.ToPSPen(this);
            this.PS_stroke();
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
            pen.ToPSPen(this);
            this.PS_stroke();
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
            pen.ToPSPen(this);
            this.PS_rectstroke(new RectangleF(xPt, yPt, widthPt, heightPt));
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
                    this.PS_moveto(x, y);
                }
                else
                    this.PS_lineto(x, y);
            }
            this.PS_lineto(x0, y0);
            pen.ToPSPen(this);
            this.PS_stroke();
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
        
            pen.ToPSPen(this);
            this.PS_stroke();
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
            pen.ToPSPen(this);
            this.PS_stroke();
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
            pen.ToPSPen(this);
            this.PS_stroke();
        }

        public void FillEllipse(
            Brush brush,
            RectangleF rect
        )
        {
            if (brush.IsEmptyColor())
                return;
            //float widthPt = (float)UCNV.GetPointFromPixel(rect.Width, this.DpiX);
            //float heightPt = (float)UCNV.GetPointFromPixel(rect.Height, this.DpiY);
            //float xPt = (float)UCNV.GetPointFromPixel(rect.X, this.DpiX);
            //float yPt = this.ChartHeightPt - heightPt - (float)UCNV.GetPointFromPixel(rect.Y, this.DpiY);
            //this.cPdfCnts.Ellipse(xPt, yPt, xPt + widthPt, yPt + heightPt);
            using (GraphicsPath lGP = new GraphicsPath())
            {
                lGP.AddEllipse(rect);
                WriteGraphicsPath(lGP.PathData, true, true);
            }
            brush.ToPSBrush(this);
            this.PS_fill();
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

            if (brush.GetBrushType().Equals(typeof(LinearGradientBrush)))
            {
                WriteTo_LF("q");
                WriteGraphicsPath(path.PathData, true, true);
                WriteTo_LF("clip");
                brush.ToPSBrush(this);
                WriteTo_LF("Q");
            }
            else
            {
                WriteGraphicsPath(path.PathData, true, true);
                brush.ToPSBrush(this);
                this.PS_fill();
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
            brush.ToPSBrush(this);
            this.PS_rectfill(new RectangleF(xPt, yPt, widthPt, heightPt));
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
                    this.PS_moveto(x, y);
                }
                else
                    this.PS_lineto(x, y);
            }
            this.PS_lineto(x0, y0);
            brush.ToPSBrush(this);
            this.PS_fill();
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
            brush.ToPSBrush(this);
            this.PS_fill();
        }



        //
        // Common Methods for PDF

        //

        //


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

            OrionX2.ItemHandler.ItemText textItem = PdfGraphicsInfo.GetItemTextFromGdiFont(s, font, format, xPx, yPx, widthPx, heightPx, this.DpiX, this.DpiY);
            if (textItem == null)
                throw new Exception("GetItemTextFromGdiFont() returns NULL");


            this.PS_save();
            try
            {
                if (textItem.cFont.cFInfo == null)
                    textItem.cFont.cFInfo = OrionX2.Render.RenderInfo._FontMgr.FindFontInfo(font.FontFamily.Name);
                if (textItem.cFont.cFInfo == null)
                    textItem.cFont.cFInfo = new OrionX2.OrionFont.FontInfo();

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
                for (int liLIdx = 0; liLIdx < llLNChars.Count; liLIdx++)
                {
                    for (int liCIdx = 0; liCIdx < llLNChars[liLIdx].LSZFCH.Count; liCIdx++)
                    {
                        string lsCH = llLNChars[liLIdx].LSZFCH[liCIdx].CH.ToString();
                        string lsFNKey = lsCH + textItem.cFont.cFInfo.csFontName_CurLocale.ToLower();
                        PSFontData lFontData = null;
                        if (this.cdctFontData.ContainsKey(lsFNKey))
                        {
                            lFontData = this.cdctFontData[lsFNKey];
                        }
                        else
                        {
                            lFontData = new PSFontData(textItem.cFont.cFInfo.csFontName_CurLocale, lsCH, this.cdctFontData.Count);
                            this.cdctFontData[lsFNKey] = lFontData;
                        }
                        llLNChars[liLIdx].LSZFCH[liCIdx].FontCache = lFontData.csPathName;
                    }
                }

                //
                float boxWidthPt = (float)UCNV.GetPointFromPixel(lBox.cSizePX.Width, this.DpiX);
                float boxHeightPt = (float)UCNV.GetPointFromPixel(lBox.cSizePX.Height, this.DpiY);
                float boxLeftPt = (float)UCNV.GetPointFromPixel(lBox.cLocationPX.X + lBox.cLocationAdjPX.X, this.DpiX);
                float boxTopPt = this.ChartHeightPt - (float)UCNV.GetPointFromPixel(lBox.cLocationPX.Y + lBox.cLocationAdjPX.Y, this.DpiX);
                float boxBottomPt = this.ChartHeightPt - boxHeightPt -
                    (float)UCNV.GetPointFromPixel(lBox.cLocationPX.Y + lBox.cLocationAdjPX.Y, this.DpiX);
                this.PS_translate(boxLeftPt, boxTopPt);
                //
                float lfFontSize = (float)textItem.cFont.cdSize;
                float lfScaleWidth = (float)textItem.GetScaleWidth();
                float lfFontScaleW = lfFontSize / PSFontData._DEFFONTSIZE;
                float lfFontScaleH = lfFontSize / PSFontData._DEFFONTSIZE;
                lfFontScaleW *= lfScaleWidth;
                brush.ToPSBrush(this);
                if (textItem.cFont.cbBold)
                {
                    this.PS_setlinewidth(PSFontData._DEFFONTSIZE * 0.030F);
                }

                for (int iLine = 0; iLine < llLNChars.Count; iLine++)
                {
                    OrionX2.ItemHandler.TextAdjustment.LineChars lineChars = llLNChars[iLine];
                    if (lineChars.LSZFCH == null || lineChars.LSZFCH.Count == 0)
                        continue;
                    PointF lptfRPos = new PointF((float)UCNV.GetPointFromPixel(lineChars.PTF.X, this.DpiX),
                                                -(float)UCNV.GetPointFromPixel(lineChars.PTF.Y, this.DpiY) + lfFontSize * 0.1F);

                    if (textItem.cFont.cbItalic)
                    {
                        foreach (OrionX2.ItemHandler.TextAdjustment.SizeFChar lSZFCH in lineChars.LSZFCH)
                        {
                            SizeF lszfCharGap = new SizeF((float)UCNV.GetPointFromPixel((float)lSZFCH.GapHorz, this.DpiX),
                                                          (float)UCNV.GetPointFromPixel((float)lSZFCH.GapVert, this.DpiY));

                            PS_WriteChar_Style(textItem.cFont.cbFlipVert, textItem.cFont.cbFlipHorz,
                                textItem.cFont.cbItalic, false, textItem.cFont.cbBold,
                                lptfRPos,lfFontScaleW, lfFontScaleH,lfFontSize, lSZFCH);

                            lptfRPos.X += lszfCharGap.Width;
                        }
                    }
                    else
                    {
                        StringBuilder lSB = new StringBuilder();
                        lSB.AppendFormat(csPSgsave + " {0:0.##} {1:0.##} {2} ", lptfRPos.X, lptfRPos.Y, csPStranslate);
                        try
                        {
                            if (lfFontScaleW != 1.0F || lfFontScaleH != 1.0F)
                                lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lfFontScaleW, lfFontScaleH, csPSscale);

                            bool lbIsFirstChar = true;
                            float lfGapHPrevChar = 0F;
                            float lfXPosTransform = 0F;
                            //
                            foreach (OrionX2.ItemHandler.TextAdjustment.SizeFChar lSZFCH in lineChars.LSZFCH)
                            {
                                if (lbIsFirstChar)
                                {
                                    lSB.Append(lSZFCH.FontCache + " ");
                                    lbIsFirstChar = false;
                                }
                                else
                                {
                                    lSB.AppendFormat("{0:0.##} X {1} ", lfGapHPrevChar / lfFontScaleW, lSZFCH.FontCache);
                                }

                                lfGapHPrevChar = (float)UCNV.GetPointFromPixel((float)lSZFCH.GapHorz, this.DpiX);
                                lptfRPos.X += lfGapHPrevChar;
                            }
                            if (textItem.cFont.cbBold)
                            {
                                lSB.Append(csPSgsave + " " + csPSfill + " " + csPSgrestore +
                                    " [] 0 " + csPSsetdash + " " + csPSstroke + " ");
                            }
                            else
                            {
                                lSB.Append(csPSfill + " ");
                            }
                        }
                        finally
                        {
                            lSB.AppendLine(csPSgrestore);
                        }
                        
                        this.WriteTo_LF(lSB.ToString());
                    }


                }

            }
            finally
            {
                this.PS_restore();
            }
            
        }



        public void Save()
		{
            this.PS_save();
        }

        public void Restore()
		{
            this.PS_restore();
        }

        public void TranslateTransform(
            float dx,
            float dy
            )
        {
            iTextSharp.awt.geom.AffineTransform trnsfrm = new iTextSharp.awt.geom.AffineTransform();
            trnsfrm.Translate((float)UCNV.GetPointFromPixel(dx, this.DpiX), this.ChartHeightPt - (float)UCNV.GetPointFromPixel(dy, this.DpiY));
            double[] ldaMtrx = new double[6];
            trnsfrm.GetMatrix(ldaMtrx);
            this.PS_matrix((float)ldaMtrx[0], (float)ldaMtrx[1], (float)ldaMtrx[2], (float)ldaMtrx[3], (float)ldaMtrx[4], (float)ldaMtrx[5]);
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


        //
        // BEGIN: PS_COMMANDS
        internal void PS_arc(float lfX, float lfY, float lfR, float lfAngle1, float lfAngle2)
        {
            WriteTo_LF(lfX.ToString("0.##") + " " +
                                   lfY.ToString("0.##") + " " +
                                   lfR.ToString("0.##") + " " +
                                   lfAngle1.ToString("0.##") + " " +
                                   lfAngle2.ToString("0.##") + " arc");
        }
        internal void PS_curveto(List<PointF> llptfCurveTo)
        {
            string lstrPostScript = string.Empty;
            for (int idx = 0; idx < llptfCurveTo.Count; idx++)
            {
                lstrPostScript += llptfCurveTo[idx].X.ToString("0.###") + " " + llptfCurveTo[idx].Y.ToString("0.###") + " ";
            }
            lstrPostScript += csPScurveto;
            WriteTo_LF(lstrPostScript);
        }
        internal void PS_matrix(float lfArg1, float lfArg2, float lfArg3, float lfArg4, float lfArg5, float lfArg6)
        {
            string lsPS = "matrix [" + lfArg1.ToString() + " "
                                     + lfArg2.ToString() + " "
                                     + lfArg3.ToString() + " "
                                     + lfArg4.ToString() + " "
                                     + lfArg5.ToString() + " "
                                     + lfArg6.ToString() + "]";
            WriteTo_LF(lsPS);
        }
        internal void PS_concat(float lfArg1, float lfArg2, float lfArg3, float lfArg4, float lfArg5, float lfArg6)
        {
            string lsPS = "[" + lfArg1.ToString("0.###") + " "
                              + lfArg2.ToString("0.###") + " "
                              + lfArg3.ToString("0.###") + " "
                              + lfArg4.ToString("0.###") + " "
                              + lfArg5.ToString("0.###") + " "
                              + lfArg6.ToString("0.###") + "] concat";
            WriteTo_LF(lsPS);
        }
        internal void PS_moveto(float lfX, float lfY)
        {
            WriteTo_LF(String.Format("{0:0.###} {1:0.###} {2}", lfX, lfY, csPSmoveto));
        }
        internal void PS_lineto(float lfX, float lfY)
        {
            WriteTo_LF(String.Format("{0:0.###} {1:0.###} {2}", lfX, lfY, csPSlineto));
        }
        internal void PS_rlineto(float lfW, float lfH)
        {
            WriteTo_LF(String.Format("{0:0.###} {1:0.###} rlineto", lfW, lfH));
        }
        internal void PS_closepath()
        {
            WriteTo_LF(csPSclosepath);
        }
        internal void PS_gsave()
        {
            WriteTo_LF(csPSgsave);
        }
        internal void PS_grestore()
        {
            WriteTo_LF(csPSgrestore);
        }
        internal void PS_translate(float lfPosXpt, float lfPosYpt)
        {
            WriteTo_LF(lfPosXpt.ToString("0.###") + " " + lfPosYpt.ToString("0.###") + " " + csPStranslate);
        }
        internal void PS_rectfill(RectangleF lrtfRect)
        {
            WriteTo_LF(lrtfRect.X.ToString("0.###") + " " +
                                   lrtfRect.Y.ToString("0.###") + " " +
                                   lrtfRect.Width.ToString("0.###") + " " +
                                   lrtfRect.Height.ToString("0.###") + " rectfill");
        }
        internal void PS_rectstroke(RectangleF lrtfRect)
        {
            WriteTo_LF(lrtfRect.X.ToString("0.###") + " " +
                                   lrtfRect.Y.ToString("0.###") + " " +
                                   lrtfRect.Width.ToString("0.###") + " " +
                                   lrtfRect.Height.ToString("0.###") + " rectstroke");
        }
        internal void PS_rotate(float lfRotate)
        {
            WriteTo_LF(lfRotate.ToString("0.##") + " rotate");
        }
        internal void PS_fill()
        {
            WriteTo_LF(csPSfill);
        }
        internal void PS_eofill()
        {
            WriteTo_LF(csPSeofill);
        }
        
        internal void PS_setrgbcolor(Color lcrRGB)
        {
            float lfRed = (float)lcrRGB.R / 255F;
            float lfGreen = (float)lcrRGB.G / 255F;
            float lfBlue = (float)lcrRGB.B / 255F;
            if (lcrRGB.A == 0)
            {
                lfRed = 1.0F;
                lfGreen = 1.0F;
                lfBlue = 1.0F;
            }
            WriteTo_LF(lfRed.ToString("0.###") + " " + lfGreen.ToString("0.###") + " " + lfBlue.ToString("0.###") + " " + csPSsetrgbcolor);

        }
        internal void PS_setlinewidth(float lfLineWidth)
        {
            WriteTo_LF(String.Format("{0:0.##} {1}", lfLineWidth, csPSsetlinewidth));
        }
        internal void PS_setmiterlimit(float lfLimit)
        {
            WriteTo_LF(String.Format("{0:0.##} setmiterlimit", lfLimit));
        }
        internal void PS_setdash(DashStyle ldsDashStyle, float lfLineWidth)
        {
            float lfDot = lfLineWidth;
            float lfDash = lfLineWidth * 3F;
            string lstrParam;
            if (ldsDashStyle == DashStyle.Solid)
            {
                lstrParam = "[] 0 ";
            }
            else if (ldsDashStyle == DashStyle.Dot)
            {
                lstrParam = "[" + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + "] 0 ";
            }
            else if (ldsDashStyle == DashStyle.Dash)
            {
                lstrParam = "[" + lfDash.ToString("0.##") + " " + lfDot.ToString("0.##") + "] 0 ";
            }
            else if (ldsDashStyle == DashStyle.DashDot)
            {
                lstrParam = "[" + lfDash.ToString("0.##") + " " + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + "] 0 ";
            }
            else if (ldsDashStyle == DashStyle.DashDotDot)
            {
                lstrParam = "[" + lfDash.ToString("0.##") + " " + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + " " + lfDot.ToString("0.##") + "] 0 ";
            }
            else
            {
                lstrParam = "[] 0 ";
            }
            WriteTo_LF(lstrParam + csPSsetdash);
        }
        internal void PS_setlinejoin(LineJoin leJoin)
		{
            int liLineJoin = 0;
            if (leJoin == LineJoin.Miter)
                liLineJoin = 0;
            else if (leJoin == LineJoin.Round)
                liLineJoin = 1;
            else if (leJoin == LineJoin.Bevel)
                liLineJoin = 2;
            WriteTo_LF(String.Format("{0:0} setlinejoin", liLineJoin));
		}
        internal void PS_setlinecap(LineCap leCap)
        {
            int liLineCap = 0;
            if (leCap == LineCap.Flat)
                liLineCap = 0;
            else if (leCap == LineCap.Round)
                liLineCap = 1;
            else if (leCap == LineCap.Square)
                liLineCap = 2;
            WriteTo_LF(String.Format("{0:0} setlinecap", liLineCap));
        }
        internal void PS_strokepath()
        {
            WriteTo_LF("strokepath");
        }
        internal void PS_stroke()
        {
            //WriteTo_LF("stroke");
            WriteTo_LF(csPSstroke);
        }
        internal void PS_save()
        {
            WriteTo_LF(csPSsave);
        }
        internal void PS_restore()
        {
            WriteTo_LF(csPSrestore);
        }
        internal void PS_showpage()
        {
            WriteTo_LF(csPSshowpage);
        }
        internal void PS_scale(float lfScaleX, float lfScaleY)
        {
            WriteTo_LF(String.Format("{0:0.###} {1:0.###} {2}", lfScaleX, lfScaleY, csPSscale));
        }
        internal void PS_ucache()
        {
            WriteTo_LF("ucache");
        }
        internal void PS_ufill()
        {
            WriteTo_LF("ufill");
        }
        internal void PS_ustroke()
        {
            WriteTo_LF("ustroke");
        }
        internal void PS_setbbox(float lfLX, float lfLY, float lfUX, float lfUY)
        {
            WriteTo_LF(lfLX.ToString("0.##") + " " + lfLY.ToString("0.##") + " " +
                                lfUX.ToString("0.##") + " " + lfUY.ToString("0.##") +
                                " setbbox");
        }

        internal void WriteGraphicsPath(PathData lPathD, bool lbIsPixelData, bool lbNoClosePathAtEnd)
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
                    PS_curveto(llptfCurveTo);
                    llptfCurveTo.Clear();
                }
                if (lPT >= 0x80)
                {
                    if (llptfCurveTo.Count == 0)
                    {
                        PS_lineto(lPP.X, lPP.Y);
                    }
                    else
                    {
                        llptfCurveTo.Add(lPP);
                        PS_curveto(llptfCurveTo);
                    }
                    llptfCurveTo.Clear();
                }
                if (lPT == 0x00)    // Indicates that the point is the start of a figure.
                {
                    if (!lbIsLastCommandCLOSEPATH)
                        PS_closepath();
                    PS_moveto(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x01)   // Indicates that the point is one of the two endpoints of a line.
                {
                    PS_lineto(lPP.X, lPP.Y);
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
                    PS_closepath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0x83)
                {
                    PS_closepath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0xa3)
                {
                    PS_closepath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else
                {
                }
            }
            if (llptfCurveTo.Count == 3)
            {
                PS_curveto(llptfCurveTo);
                llptfCurveTo.Clear();
            }

            if (!lbIsLastCommandCLOSEPATH && !lbNoClosePathAtEnd)
                PS_closepath();

        }
        internal void WriteGraphicsPath_Fast(PathData lPathD, bool lbIsPixelData, bool lbNoClosePathAtEnd)
        {
            StringBuilder lSB = new StringBuilder();

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
                    //PS_curveto(llptfCurveTo);
                    foreach (PointF lptfCT in llptfCurveTo)
                    {
                        lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lptfCT.X, lptfCT.Y, csPScurveto);
                    }

                    llptfCurveTo.Clear();
                }
                if (lPT >= 0x80)
                {
                    if (llptfCurveTo.Count == 0)
                    {
                        lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lPP.X, lPP.Y, csPSlineto);
                        //PS_lineto(lPP.X, lPP.Y);
                    }
                    else
                    {
                        llptfCurveTo.Add(lPP);
                        //PS_curveto(llptfCurveTo);
                        foreach (PointF lptfCT in llptfCurveTo)
                        {
                            lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lptfCT.X, lptfCT.Y, csPScurveto);
                        }
                    }
                    llptfCurveTo.Clear();
                }
                if (lPT == 0x00)    // Indicates that the point is the start of a figure.
                {
                    if (!lbIsLastCommandCLOSEPATH)
                        lSB.AppendFormat("CP ");
                    //PS_closepath();
                    lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lPP.X, lPP.Y, csPSmoveto);
                    //PS_moveto(lPP.X, lPP.Y);
                    lbIsLastCommandCLOSEPATH = false;
                }
                else if (lPT == 0x01)   // Indicates that the point is one of the two endpoints of a line.
                {
                    lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lPP.X, lPP.Y, csPSlineto);
                    //PS_lineto(lPP.X, lPP.Y);
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
                    lSB.AppendFormat(csPSclosepath + " ");
                    //PS_closepath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0x83)
                {
                    lSB.AppendFormat(csPSclosepath + " ");
                    //PS_closepath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else if (lPT == 0xa3)
                {
                    lSB.AppendFormat(csPSclosepath + " ");
                    //PS_closepath();
                    lbIsLastCommandCLOSEPATH = true;
                }
                else
                {
                }
            }
            if (llptfCurveTo.Count == 3)
            {
                //PS_curveto(llptfCurveTo);
                foreach (PointF lptfCT in llptfCurveTo)
                {
                    lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lptfCT.X, lptfCT.Y, csPScurveto);
                }

                llptfCurveTo.Clear();
            }

            if (!lbIsLastCommandCLOSEPATH && !lbNoClosePathAtEnd)
                lSB.AppendFormat(csPSclosepath + " ");
            //PS_closepath();

            WriteTo_LF(lSB.ToString());
        }
        //
        
        internal void PS_WriteChar_Style(bool lbFlipVert, bool lbFlipHorz, bool lbFontStyleItalic, bool lbFontOutline, bool lbFontStyleBold, 
                                   PointF lptfRPos, float lfFontScaleW, float lfFontScaleH, float lfFontSize,
                                   OrionX2.ItemHandler.TextAdjustment.SizeFChar lSZFCH)
        {
            float lfPenWidth = lfFontSize * 0.03F;// OrionConfigInfo.UCNV.GetPointFromMM(lCD.cfPenWidth);
            DashStyle lPenDashStyle = DashStyle.Solid;

            StringBuilder lSB = new StringBuilder();
            lfPenWidth = lfPenWidth / (lfFontSize / PSFontData._DEFFONTSIZE) * 0.8F;

            lSB.AppendLine(csPSgsave);
            try
            {
                if (lbFontStyleItalic)
                {
                    float lfItalicization = lfFontSize / 50F;

                    if (lbFlipVert || lbFlipHorz)
                    {
                        float lfFlipPosX = lptfRPos.X + lfFontSize / 3.0F;
                        if (lbFlipVert && !lbFlipHorz)
                        {
                            lfItalicization = -lfItalicization;
                            lfFlipPosX = lptfRPos.X - lfFontSize / 3.0F + 3.25F;
                        }
                        else if (!lbFlipVert && lbFlipHorz)
                        {
                            lfItalicization = -lfItalicization;
                            lfFlipPosX = lptfRPos.X - lfFontSize / 3.0F + 5F;
                        }
                        else
                        {
                            lfFlipPosX = lptfRPos.X - lfFontSize / 3.0F + 10F;
                        }
                        lSB.AppendFormat("{0:0.##} {1:0.##} {2}\n", lfFlipPosX, lptfRPos.Y, csPStranslate);
                        lSB.AppendFormat("[1 0 {0:0.##} 1 0 0] {1}\n", lfItalicization, csPSconcat);
                    }
                    else
                    {
                        lSB.AppendFormat("{0:0.##} {1:0.##} {2}\n", lptfRPos.X + lfFontSize / 3.0F - 2F, lptfRPos.Y, csPStranslate);
                        lSB.AppendFormat("[1 0 {0:0.##} 1 0 0] {1}\n", lfItalicization, csPSconcat);
                    }
                }
                else
                {
                    lSB.AppendFormat("{0:0.##} {1:0.##} {2}\n", lptfRPos.X, lptfRPos.Y, csPStranslate);
                }
                if (lfFontScaleW != 1.0F || lfFontScaleH != 1.0F)
                    lSB.AppendFormat("{0:0.##} {1:0.##} {2}\n", lfFontScaleW, lfFontScaleH, csPSscale);
                //
                //
                if (lbFlipVert)
                {
                    lSB.AppendFormat("[1 0 0 -1 {0:0.##} {1:0.##}] cm\n",
                        0, -UCNV.GetPointFromPixel((float)lSZFCH.CharHeight, this.DpiY) / lfFontScaleH);
                }
                if (lbFlipHorz)
                {
                    lSB.AppendFormat("[-1 0 0 1 {0:0.##} {1:0.##}] {2}\n",
                        UCNV.GetPointFromPixel((float)lSZFCH.CharWidth, this.DpiX) / lfFontScaleW, 0);
                }
                //
                //
                lSB.Append(lSZFCH.FontCache + " ");
                //
                //
                if (lbFontOutline)
                {
                    if (lbFontStyleBold)
                        lSB.AppendFormat("{0:0.##} {1} ", lfPenWidth * 2, csPSsetlinewidth);
                    else
                        lSB.AppendFormat("{0:0.##} {1} ", lfPenWidth, csPSsetlinewidth);
                    //PS_setdash(lPenDashStyle, lfPenWidth);
                    float lfDot = lfPenWidth;
                    float lfDash = lfPenWidth * 3F;

                    if (lPenDashStyle == DashStyle.Solid)
                    {
                        lSB.Append("[] 0 " + csPSsetdash + " " + csPSstroke + " ");
                    }
                    else if (lPenDashStyle == DashStyle.Dot)
                    {
                        lSB.AppendFormat("[{0:0.##} {1:0.##}] 0 {2} {3} ", lfDot, lfDot, csPSsetdash, csPSstroke);
                    }
                    else if (lPenDashStyle == DashStyle.Dash)
                    {
                        lSB.AppendFormat("[{0:0.##} {1:0.##}] 0 {2} {3} ", lfDash, lfDot, csPSsetdash, csPSstroke);
                    }
                    else if (lPenDashStyle == DashStyle.DashDot)
                    {
                        lSB.AppendFormat("[{0:0.##} {1:0.##} {2:0.##} {3:0.##}] 0 {4} {5} ",
                                                  lfDash, lfDot, lfDot, lfDot, csPSsetdash, csPSstroke);
                    }
                    else if (lPenDashStyle == DashStyle.DashDotDot)
                    {
                        lSB.AppendFormat("[{0:0.##} {1:0.##} {2:0.##} {3:0.##} {4:0.##} {5:0.##}] 0 {6} {7} ",
                                                  lfDash, lfDot, lfDot, lfDot, lfDot, lfDot, csPSsetdash, csPSstroke);
                    }
                    else
                    {
                        lSB.Append("[] 0 " + csPSsetdash + " " + csPSstroke + " ");
                    }
                }
                else
                {
                    if (lbFontStyleBold)
                    {
                        lSB.AppendFormat("{0:0.##} {1} ", PSFontData._DEFFONTSIZE * 0.030F, csPSsetlinewidth);
                        lSB.Append(csPSgsave + " " + csPSfill + " " + csPSgrestore + " [] 0 " + csPSsetdash + " " + csPSstroke + " ");
                    }
                    else
                    {
                        lSB.Append(csPSfill + " ");
                    }
                }
            }
            finally
            {
                lSB.AppendLine(csPSgrestore);
            }
            WriteTo_LF(lSB.ToString());

        }
        internal void PS_WriteChar_Regular(PointF lptfRPos,
                                 float lfFontScaleW,
                                 float lfFontScaleH,
                                 bool lbFontStyleBold,
                                 float lfFontSize,
                                 string lsFontCache,
                                 float lfPenWidth,
                                 DashStyle lPenDashStyle)
        {
            StringBuilder lSB = new StringBuilder();


            lSB.AppendFormat(csPSgsave + " {0:0.##} {1:0.##} {2} ", lptfRPos.X, lptfRPos.Y, csPStranslate);

            if (lfFontScaleW != 1.0F || lfFontScaleH != 1.0F)
                lSB.AppendFormat("{0:0.##} {1:0.##} {2} ", lfFontScaleW, lfFontScaleH, csPSscale);

            lSB.Append(lsFontCache + " ");

            if (lbFontStyleBold)
            {
                lSB.Append(csPSgsave + " " + csPSfill + " " + csPSgrestore + " " + csPSstroke + " ");
            }
            else
            {
                lSB.Append(csPSfill + " ");
            }
            lSB.AppendLine(csPSgrestore);
            WriteTo_LF(lSB.ToString());

        }
        //
        internal void WriteTo_LF(string lsPS)
		{
            byte[] lbyaPSCmds = Encoding.ASCII.GetBytes(lsPS + '\n');
            cStrmBody.Write(lbyaPSCmds, 0, lbyaPSCmds.Length);
		}

        
        // END: PS_COMMANDS

    }
    public  class PSFontData
    {
        public static float _DEFFONTSIZE = 10.0F;
        //
        public string csChar;
        public byte[] cbyaUnicode;
        public string csFontName;
        //
        public string csPathName;
        //
        // Constructors 
        public PSFontData()
        {
            InitVariables();
        }
        public PSFontData(string lsFontName, string lstrCharacter, int liID)
        {
            InitVariables(lsFontName, lstrCharacter, liID);
        }
        //
        // Initialization
        public void InitVariables()
        {
            this.csChar = string.Empty;
            this.cbyaUnicode = null;
            this.csFontName = string.Empty;
            this.csPathName = string.Empty;
        }
        public void InitVariables(string lsFontName, string lsChar, int liID)
        {
            if (lsChar.Length != 1)
            {
                InitVariables();
                return;
            }
            this.csChar = lsChar;
            this.cbyaUnicode = UnicodeEncoding.Unicode.GetBytes(lsChar.ToCharArray());
            this.csFontName = lsFontName;
            this.csPathName = GetPathName_Base62(liID);
        }
        //
        //
        public void CopyFrom(PSFontData lSrc)
        {
            if (lSrc == null)
                return;
            //
            this.csChar = lSrc.csChar;
            this.cbyaUnicode = lSrc.cbyaUnicode;
            this.csFontName = lSrc.csFontName;
            this.csPathName = lSrc.csPathName;
            //
        }

        public string GetPathName_Base62(int liID)
        {
            string lsPathName = "F_";

            //long lFnt = ((long)(this.ciFontId) << 16) + this.cbyaUnicode[1] * 0x100 + this.cbyaUnicode[0];


            lsPathName += SFUNC.ToBase(liID, SFUNC._BASE62);

            return lsPathName;
        }
    }

}
