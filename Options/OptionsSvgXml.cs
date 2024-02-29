using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Orion.DataVisualization.Charting;
//using OrionConfigInfo;

namespace Orion.DataVisualization.Charting.Options
{

    public class SvgXmlInfo : IDisposable
    {
        public string csSvgxml;
        string csversion;
        string csid;
        string csxmlns;
        string csx; //"0px"
        double cdX;
        string csy; //"0px"
        double cdY;
        string cswidth; //"100px"
        double cdWidth;
        string csheight; //"500px"
        double cdHeight;
        string csviewBox; // "0 0 100 500"
        string csenable_background; // "new 0 0 100 500"
        public List<SvgPathInfo> clSvgPaths;
        //
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                ClearPathList();
            }
            _disposed = true;
        }

        public SvgXmlInfo(string lsSvgXml)
        {
            csSvgxml = lsSvgXml;
            SvgXmlInfo_Init();
            ParseSvgXml();
                        
        }

        public void SvgXmlInfo_Init()
        {
            csversion = string.Empty;
            csid = string.Empty;
            csxmlns = string.Empty;
            csx = string.Empty;
            cdX = 0;
            csy = string.Empty;
            cdY = 0;
            cswidth = string.Empty;
            cdWidth = 0;
            csheight = string.Empty;
            cdHeight = 0;
            csviewBox = string.Empty;
            csenable_background = string.Empty;
            this.clSvgPaths = new List<SvgPathInfo>();
        }

        public bool ParseSvgXml()
        {
            bool lbSuccess = false;
            try
            {
                XDocument lxDoc = XDocument.Parse(this.csSvgxml);
                foreach (XAttribute lxAttr in lxDoc.Root.Attributes())
                {
                    if (lxAttr.Name == "version")
                        csversion = lxAttr.Value;
                    else if (lxAttr.Name == "id")
                        csid = lxAttr.Value;
                    else if (lxAttr.Name == "xmlns")
                        csxmlns = lxAttr.Value;
                    else if (lxAttr.Name == "x")
                    {
                        csx = lxAttr.Value;
                        cdX = ParseStringToDouble(csx);
                    }
                    else if (lxAttr.Name == "y")
                    {
                        csy = lxAttr.Value;
                        cdY = ParseStringToDouble(csy);
                    }
                    else if (lxAttr.Name == "width")
                    {
                        cswidth = lxAttr.Value;
                        cdWidth = ParseStringToDouble(cswidth);
                    }
                    else if (lxAttr.Name == "height")
                    {
                        csheight = lxAttr.Value;
                        cdHeight = ParseStringToDouble(csheight);
                    }
                    else if (lxAttr.Name == "viewBox")
                        csviewBox = lxAttr.Value;
                    else if (lxAttr.Name == "enable-background")
                        csenable_background = lxAttr.Value;
                }

                ClearPathList();
                if (this.clSvgPaths == null)
                    this.clSvgPaths = new List<SvgPathInfo>();
                foreach (XElement lxElem in lxDoc.Root.Elements())
                {
                    if (!lxElem.ToString().StartsWith("<path "))
                        continue;
                    SvgPathInfo lSvgPath = new SvgPathInfo();
                    lSvgPath.ParsePath(lxElem);
                    this.clSvgPaths.Add(lSvgPath);
                }
                lbSuccess = true;
            }
            catch
            {
                lbSuccess = false;
                SvgXmlInfo_Init();
            }
            return lbSuccess;
        }

        private double ParseStringToDouble(string lsValue)
        {
            double ldValue;
            lsValue = Regex.Replace(lsValue, "([a-zA-Z].*)", "");
            if (!double.TryParse(lsValue, out ldValue))
                ldValue = 0;
            return ldValue;
        }

        private void ClearPathList()
        {
            if (this.clSvgPaths != null && this.clSvgPaths.Count > 0)
            {
                this.clSvgPaths.ForEach(PTH => PTH.Dispose());
                this.clSvgPaths.Clear();
            }
        }

        public static void GetSvgXmlInfo(string lsSvgXml, ref SvgXmlInfo lSvgXml)
        {
            if (string.IsNullOrWhiteSpace(lsSvgXml))
            {
                if (lSvgXml != null) { lSvgXml.Dispose(); lSvgXml = null; }
            }
            else
            {
                if (lSvgXml == null)
                {
                    lSvgXml = new SvgXmlInfo(lsSvgXml);
                }
                else
                {
                    if (lSvgXml.csSvgxml != lsSvgXml)
                    {
                        lSvgXml.Dispose();
                        lSvgXml = new SvgXmlInfo(lsSvgXml);
                    }
                }
            }
        }

        internal void Draw_Graphics(IRenderingGraphics lGR, RectangleF lrtfBounds, DataPoint point)
        {
            try
            {
                double ldScaleX = (double)lrtfBounds.Width / cdWidth;
                double ldScaleY = (double)lrtfBounds.Height / cdHeight;
                foreach (SvgPathInfo lSvtPath in clSvgPaths)
                    lSvtPath.Draw_Graphics(lGR, lrtfBounds, point, ldScaleX, ldScaleY);
            }
            finally
            {
            }
        }
        /*
        internal void Draw_PS(OrionPostScript lPS, RectangleF lrtfBnd, decimal lmPt, double ldWidth, double ldHeight, double ldScaleW, double ldScaleH)
        {
            foreach (SvgPathInfo lSvtPath in clSvgPaths)
                lSvtPath.Draw_PS(lPS, lrtfBnd, lmPt, ldWidth, ldHeight, ldScaleW, ldScaleH);
        }

        public void Draw_PDF(OrionPDF lPDF, RectangleF lrtfBnd, decimal lmPt, double ldWidth, double ldHeight, double ldScaleW, double ldScaleH)
        {
            foreach (SvgPathInfo lSvtPath in clSvgPaths)
                lSvtPath.Draw_PDF(lPDF, lrtfBnd, lmPt, ldWidth, ldHeight, ldScaleW, ldScaleH);
        }
        */
    }

    public class SvgPathInfo : IDisposable
    {

        public string csPath;
        private System.Drawing.Drawing2D.GraphicsPath _GPath;
        private System.Drawing.Drawing2D.GraphicsPath _GPathYInv;
        private System.Drawing.Drawing2D.GraphicsPath _GPathXInv;
        public string csfill;
        public Color cFillColor;
        public string csstroke;
        public Color cStrokeColor;
        public string csstroke_width;
        public double cdStrokeWidth;
        //
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_GPath != null) { _GPath.Dispose();  _GPath = null; }
                if (_GPathYInv != null) { _GPathYInv.Dispose(); _GPathYInv = null; }
                if (_GPathXInv != null) { _GPathXInv.Dispose(); _GPathXInv = null; }
            }
            _disposed = true;
        }

        public SvgPathInfo()
        {
            csPath = string.Empty;
            _GPath = null;
            _GPathYInv = null;
            _GPathXInv = null;
            csfill = string.Empty;
            cFillColor = Color.Empty;
            csstroke = string.Empty;
            cStrokeColor = Color.Empty;
            csstroke_width = string.Empty;
            cdStrokeWidth = 0;
        }

        public void ParsePath(XElement lxElem)
        {
            try
            {
                if (_GPath != null) { _GPath.Dispose(); _GPath = null; }
                if (_GPathYInv != null) { _GPathYInv.Dispose(); _GPathYInv = null; }
                if (_GPathXInv != null) { _GPathXInv.Dispose(); _GPathXInv = null; }

                foreach (XAttribute lxAttr in lxElem.Attributes())
                {
                    if (lxAttr.Name == "d")
                        csPath = lxAttr.Value;
                    else if (lxAttr.Name == "fill")
                    {
                        csfill = lxAttr.Value;
                        cFillColor = Color.FromGdiColor(ColorTranslator.FromHtml(csfill));
                    }
                    else if (lxAttr.Name == "stroke")
                    {
                        csstroke = lxAttr.Value;
                        cStrokeColor = Color.FromGdiColor(ColorTranslator.FromHtml(csstroke));
                    }
                    else if (lxAttr.Name == "stroke-width")
                    {
                        csstroke_width = lxAttr.Value;
                        if (!double.TryParse(csstroke_width, out cdStrokeWidth))
                            cdStrokeWidth = 0;
                    }
                }
            }
            catch
            {

            }
        }

        public System.Drawing.Drawing2D.GraphicsPath GetGPath(double ldWidth, double ldHeight)
        {
            if (_GPath == null)
            {
                _GPath = new System.Drawing.Drawing2D.GraphicsPath();
                Svg.Path.SvgPathSegmentList llSvgPathSegments = Svg.SvgPathBuilder.Parse(csPath);
                foreach (Svg.Path.SvgPathSegment lSvgPath in llSvgPathSegments)
                {
                    lSvgPath.AddToPath(_GPath);
                }
            }
            return _GPath;
        }

        public System.Drawing.Drawing2D.GraphicsPath GetGPathYInv(double ldWidth, double ldHeight)
        {
            if (_GPathYInv == null && _GPath != null)
            {
                List<PointF> llptfPts = new List<PointF>();
                List<byte> llbyTypes = new List<byte>();
                for (int IX = 0; IX < _GPath.PathPoints.Length; IX++)
                {
                    llbyTypes.Add(_GPath.PathTypes[IX]);
                    llptfPts.Add(new PointF(_GPath.PathPoints[IX].X, (float)ldHeight - _GPath.PathPoints[IX].Y));
                }
                _GPathYInv = new System.Drawing.Drawing2D.GraphicsPath(llptfPts.ToArray(), llbyTypes.ToArray());
            }
            return _GPathYInv;
        }
        public System.Drawing.Drawing2D.GraphicsPath GetGPathXInv(double ldWidth, double ldHeight)
        {
            if (_GPathXInv == null && _GPath != null)
            {
                List<PointF> llptfPts = new List<PointF>();
                List<byte> llbyTypes = new List<byte>();
                for (int IX = 0; IX < _GPath.PathPoints.Length; IX++)
                {
                    llbyTypes.Add(_GPath.PathTypes[IX]);
                    llptfPts.Add(new PointF((float)ldHeight - _GPath.PathPoints[IX].X, _GPath.PathPoints[IX].Y));
                }
                _GPathXInv = new System.Drawing.Drawing2D.GraphicsPath(llptfPts.ToArray(), llbyTypes.ToArray());
            }
            return _GPathXInv;
        }

        public static System.Drawing.Drawing2D.GraphicsPath ParseSvg(string lsLine)
        {
            System.Drawing.Drawing2D.GraphicsPath lGPath = new System.Drawing.Drawing2D.GraphicsPath();
            Svg.Path.SvgPathSegmentList llSvgPathSegments = Svg.SvgPathBuilder.Parse(lsLine);
            foreach (Svg.Path.SvgPathSegment lSvgPath in llSvgPathSegments)
            {
                lSvgPath.AddToPath(lGPath);
            }
            return lGPath;
        }

        internal void Draw_Graphics(IRenderingGraphics lGR, RectangleF lrtfBounds, DataPoint point, double ldScaleX, double ldScaleY)
        {
            try
            {
                //System.Drawing.Drawing2D.GraphicsPath lGP = point.YValues[0] < 0 ? 
                //    GetGPathYInv(lrtfBounds.Width, lrtfBounds.Height) : GetGPath(lrtfBounds.Width, lrtfBounds.Height);
                System.Drawing.Drawing2D.GraphicsPath lGP = GetGPath(lrtfBounds.Width, lrtfBounds.Height);
                if (lGP == null)
                    return;
                PointF[] lPathPts = new PointF[lGP.PathData.Points.Length];
                byte[] lPathTypes = new byte[lGP.PathData.Points.Length];

                for (int IX = 0; IX < lGP.PathData.Points.Length; IX++)
                {
                    if (point.YValues[0] >= 0)
                        lPathPts[IX] = new PointF((lGP.PathData.Points[IX].X) * (float)ldScaleX + lrtfBounds.X,
                                              (lGP.PathData.Points[IX].Y) * (float)ldScaleY + lrtfBounds.Y);
                    else
                        lPathPts[IX] = new PointF((lGP.PathData.Points[IX].X) * (float)ldScaleX + lrtfBounds.X,
                                              lrtfBounds.Height - (lGP.PathData.Points[IX].Y) * (float)ldScaleY + lrtfBounds.Y);
                    lPathTypes[IX] = lGP.PathData.Types[IX];
                }
                using (System.Drawing.Drawing2D.GraphicsPath lGPNew = new System.Drawing.Drawing2D.GraphicsPath(lPathPts, lPathTypes))
                {
                    lGR.FillPath(new SolidBrush(cFillColor), lGPNew);
                    if (cdStrokeWidth > 0)
                    {
                        double ldLineWidth = (cdStrokeWidth / 72) * lGR.DpiX;//(cdStrokeWidth / 25.4) * lGR.DpiX;
                        if (ldLineWidth < 0)
                            ldLineWidth = 1;
                        lGR.DrawPath(new Pen(cFillColor, (float)ldLineWidth), lGPNew);
                    }
                }
            }
            catch
            {
            }
        }

        /*
        internal void Draw_PS(OrionPostScript lPS, RectangleF lrtfBnd, decimal lmPt, double ldWidth, double ldHeight, double ldScaleW, double ldScaleH)
        {
            try
            {
                System.Drawing.Drawing2D.GraphicsPath lGP = lmPt < 0 ? GetGPathYInv(ldWidth, ldHeight) : GetGPath(ldWidth, ldHeight);
                if (lGP == null)
                    return;
                System.Drawing.Drawing2D.PathData lPData = new System.Drawing.Drawing2D.PathData();
                lPData.Points = new PointF[lGP.PathData.Points.Length];
                lPData.Types = new byte[lGP.PathData.Points.Length];
                for (int IX = 0; IX < lGP.PathData.Points.Length; IX++)
                {
                    lPData.Points[IX] = new PointF((lGP.PathData.Points[IX].X) * (float)ldScaleW + lrtfBnd.X,
                                                    lrtfBnd.Height - (lGP.PathData.Points[IX].Y) * (float)ldScaleH + lrtfBnd.Y);
                    lPData.Types[IX] = lGP.PathData.Types[IX];
                }
                lPS.WriteGraphicsPath(lPData, false, lPS.cfDpiX, lPS.cfDpiY, false);
                if (cdStrokeWidth == 0)
                {
                    lPS.PS_setrgbcolor(cFillColor.RGB);
                    lPS.PS_fill();
                }
                else
                {
                    lPS.PS_gsave();
                    lPS.PS_setrgbcolor(cFillColor.RGB);
                    lPS.PS_fill();
                    lPS.PS_grestore();
                    lPS.PS_setlinewidth((float)cdStrokeWidth);
                    lPS.PS_setrgbcolor(cStrokeColor.RGB);
                    lPS.PS_stroke();
                }

            }
            catch (Exception lExe)
            {
                ORIONDEBUG.LOG(LogInfo.EnumLogLevel.ERROR, "SvgPathInfo::Draw_PDF()", lExe);
            }
        }


        public void Draw_PDF(OrionPDF lPDF, RectangleF lrtfBnd, decimal lmPt, double ldWidth, double ldHeight, double ldScaleW, double ldScaleH)
        {
            try
            {
                System.Drawing.Drawing2D.GraphicsPath lGP = lmPt < 0 ? GetGPathYInv(ldWidth, ldHeight) : GetGPath(ldWidth, ldHeight);
                if (lGP == null)
                    return;
                System.Drawing.Drawing2D.PathData lPData = new System.Drawing.Drawing2D.PathData();
                lPData.Points = new PointF[lGP.PathData.Points.Length];
                lPData.Types = new byte[lGP.PathData.Points.Length];
                for (int IX = 0; IX < lGP.PathData.Points.Length; IX++)
                {
                    lPData.Points[IX] = new PointF((lGP.PathData.Points[IX].X) * (float)ldScaleW + lrtfBnd.X,
                                            lrtfBnd.Height - (lGP.PathData.Points[IX].Y) * (float)ldScaleH + lrtfBnd.Y);
                    lPData.Types[IX] = lGP.PathData.Types[IX];
                }
                lPDF.WriteGraphicsPath(lPData, false, 0, false, lPDF.cPDFWriter.DirectContent);
                if (cdStrokeWidth == 0)
                {
                    lPDF.SetColorFill(lPDF.cPDFWriter.DirectContent, cFillColor);
                    lPDF.cPDFWriter.DirectContent.Fill();
                }
                else
                {
                    lPDF.SetColorFill(lPDF.cPDFWriter.DirectContent, cFillColor);
                    lPDF.cPDFWriter.DirectContent.SetLineWidth((float)cdStrokeWidth);
                    lPDF.SetColorStroke(lPDF.cPDFWriter.DirectContent, cStrokeColor);
                    lPDF.cPDFWriter.DirectContent.FillStroke();
                }
            }
            catch (Exception lExe)
            {
                ORIONDEBUG.LOG(LogInfo.EnumLogLevel.ERROR, "SvgPathInfo::Draw_PDF()", lExe);
            }
        }
        */
    }

}
