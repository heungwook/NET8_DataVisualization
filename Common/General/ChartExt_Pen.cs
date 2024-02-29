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
using System.Runtime.Serialization;

#if Microsoft_CONTROL
using Orion.DataVisualization.Charting.Utilities;
using Orion.DataVisualization.Charting.Borders3D;
#else
using System.Web.UI.DataVisualization.Charting.Utilities;
using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif
using OrionX2.ConfigInfo;
#endregion

#if Microsoft_CONTROL
namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    [Serializable, DataContract(Name = "Pen", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class Pen : ICloneable, IDisposable
    {
        private System.Drawing.Pen _penGdi;
        private Color _color;
        public Color Color { get { return _color; } set { _color = new Color(value); _penGdi.Color = _color.ToColor(); } }
        private Brush _brush;
        public Brush Brush { get { return _brush; } set { _brush = (Brush)value.Clone(); _penGdi.Brush = _brush.ToGdiBrush(); } }
        public float Width { get { return _penGdi.Width; } set { _penGdi.Width = value; } }
        public LineCap StartCap { get { return _penGdi.StartCap; } set { _penGdi.StartCap = value; } }
        public LineCap EndCap { get { return _penGdi.EndCap; } set { _penGdi.EndCap = value; } }
        public DashCap DashCap { get { return _penGdi.DashCap; } set { _penGdi.DashCap = value; } }
        public LineJoin LineJoin { get { return _penGdi.LineJoin; } set { _penGdi.LineJoin = value; } }
        public CustomLineCap CustomStartCap { get { return _penGdi.CustomStartCap; } set { _penGdi.CustomStartCap = value; } }
        public CustomLineCap CustomEndCap { get { return _penGdi.CustomEndCap; } set { _penGdi.CustomEndCap = value; } }
        public float MiterLimit { get { return _penGdi.MiterLimit; } set { _penGdi.MiterLimit = value; } }
        public PenAlignment Alignment { get { return _penGdi.Alignment; } set { _penGdi.Alignment = value; } }
        public Matrix Transform { get { return _penGdi.Transform; } set { _penGdi.Transform = value; } }
        public PenType PenType { get { return _penGdi.PenType; } }
        public DashStyle DashStyle { get { return _penGdi.DashStyle; } set { _penGdi.DashStyle = value; } }
        public float DashOffset { get { return _penGdi.DashOffset; } set { _penGdi.DashOffset = value; } }
        public float[] DashPattern { get { return _penGdi.DashPattern; } set { _penGdi.DashPattern = value; } }
        public float[] CompoundArray { get { return _penGdi.CompoundArray; } set { _penGdi.CompoundArray = value; } }

        public System.Drawing.Pen ToGdiPen()
		{
            return _penGdi;
		}

        public void ToPdfPen(PdfGraphicsInfo graphicsPdf)
		{
            if (this.PenType == PenType.SolidColor)
            {
                Color strokeColor;
                float strokeWidthPt;
                if (this.Brush == null)
				{
                    strokeColor = this.Color;
                    strokeWidthPt = (float)UCNV.GetPointFromPixel(this.Width, graphicsPdf.DpiX) * 0.85F;
				}
                else
				{
                    strokeColor = ((SolidBrush)this.Brush).Color;
                    strokeWidthPt = (float)UCNV.GetPointFromPixel(this.Width, graphicsPdf.DpiX) * 0.85F;

                }
                graphicsPdf.SetColorStroke(strokeColor);
                graphicsPdf.cPdfCnts.SetLineWidth(strokeWidthPt);
				graphicsPdf.SetLineDash(this.DashStyle, strokeWidthPt);
                graphicsPdf.SetLineJoin(this.LineJoin);
                graphicsPdf.SetLineCap(this.StartCap);
                float miterLimitPt = (float)UCNV.GetPointFromPixel(this.MiterLimit, graphicsPdf.DpiX);
                graphicsPdf.cPdfCnts.SetMiterLimit(miterLimitPt);
                //?graphicsPdf.cPdfCnts.SetGState()?
            }
        }

        public void ToPSPen(PSGraphicsInfo graphicsPS)
        {
            if (this.PenType == PenType.SolidColor)
            {
                Color strokeColor;
                float strokeWidthPt;
                if (this.Brush == null)
                {
                    strokeColor = this.Color;
                    strokeWidthPt = (float)UCNV.GetPointFromPixel(this.Width, graphicsPS.DpiX) * 0.85F;
                }
                else
                {
                    strokeColor = ((SolidBrush)this.Brush).Color;
                    strokeWidthPt = (float)UCNV.GetPointFromPixel(this.Width, graphicsPS.DpiX) * 0.85F;

                }
                graphicsPS.PS_setrgbcolor(strokeColor);
                graphicsPS.PS_setlinewidth(strokeWidthPt);
                graphicsPS.PS_setdash(this.DashStyle, strokeWidthPt);
                graphicsPS.PS_setlinejoin(this.LineJoin);
                graphicsPS.PS_setlinecap(this.StartCap);
                float miterLimitPt = (float)UCNV.GetPointFromPixel(this.MiterLimit, graphicsPS.DpiX);
                graphicsPS.PS_setmiterlimit(miterLimitPt);
            }
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            if (_penGdi != null)
            {
                _penGdi.Dispose();
                _penGdi = null;
            }
            _disposed = true;

            GC.SuppressFinalize(this);

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }


        public Pen(Color color) : this(color, (float)1.0)
        {
        }

        public Pen(Color color, float width)
        {
            this._penGdi = new System.Drawing.Pen(color.ToColor(), width);

            this.Color = color;
        }
        public Pen(Brush brush) : this(brush, (float)1.0)
        {
        }

        public Pen(Brush brush, float width)
        {
            if (brush == null)
                throw new ArgumentNullException("brush");

            this._penGdi = new System.Drawing.Pen(brush.ToGdiBrush(), width);

            this.Brush = brush;
        }


        public void SetLineCap(LineCap startCap, LineCap endCap, DashCap dashCap)
        {
            this._penGdi.SetLineCap(startCap, endCap, dashCap);
        }

        public void ResetTransform()
        {
            this._penGdi.ResetTransform();
        }

        public void MultiplyTransform(Matrix matrix)
        {
            MultiplyTransform(matrix, MatrixOrder.Prepend);

        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            this._penGdi.MultiplyTransform(matrix, order);
        }

        public void TranslateTransform(float dx, float dy)
        {
            TranslateTransform(dx, dy, MatrixOrder.Prepend);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            this._penGdi.TranslateTransform(dx, dy, order);
        }

        public void ScaleTransform(float sx, float sy)
        {
            ScaleTransform(sx, sy, MatrixOrder.Prepend);
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            this._penGdi.ScaleTransform(sx, sy, order);
        }

        public void RotateTransform(float angle)
        {
            RotateTransform(angle, MatrixOrder.Prepend);
        }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            this._penGdi.RotateTransform(angle, order);
        }
    }
}
