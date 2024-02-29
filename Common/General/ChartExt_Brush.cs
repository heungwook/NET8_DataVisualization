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

#endregion

#if Microsoft_CONTROL
namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    [Serializable, DataContract(Name = "Brush", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    [KnownType(typeof(SolidBrush))]
    [KnownType(typeof(HatchBrush))]
    [KnownType(typeof(PathGradientBrush))]
    [KnownType(typeof(LinearGradientBrush))]
    [KnownType(typeof(TextureBrush))]
    public abstract class Brush : ICloneable, IDisposable
    {
        internal System.Drawing.Brush _brushGdi;

        internal Color _color;
        
        internal Type GetBrushType()
		{
            if (this.GetType().Equals(typeof(SolidBrush)))
                return typeof(SolidBrush);
            else if (this.GetType().Equals(typeof(HatchBrush)))
                return typeof(HatchBrush);
            else if (this.GetType().Equals(typeof(PathGradientBrush)))
                return typeof(PathGradientBrush);
            else if (this.GetType().Equals(typeof(LinearGradientBrush)))
                return typeof(LinearGradientBrush);
            else if (this.GetType().Equals(typeof(TextureBrush)))
                return typeof(TextureBrush);
            else
                return typeof(SolidBrush);
        }


        public abstract object Clone();

        public System.Drawing.Brush ToGdiBrush()
		{
            return _brushGdi;
		}
        public abstract void ToPdfBrush(PdfGraphicsInfo graphicsPdf);
        public abstract void ToPSBrush(PSGraphicsInfo graphicsPS);

        public abstract bool IsEmptyColor();

        private bool _disposed = false;
        public void Dispose()
		{
            if (_disposed)
			{
                return;
			}
            _disposed = true;
            GC.SuppressFinalize(this);

        }

    }

    [Serializable, DataContract(Name = "SolidBrush", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class SolidBrush : Brush
    {
        public Color Color { get { return this._color; } set { this._color = new Color(value); this._brushGdi = new System.Drawing.SolidBrush(this._color.ToColor()); }  }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        public override void ToPdfBrush(PdfGraphicsInfo graphicsPdf)
		{
            graphicsPdf.SetColorFill(this.Color);
		}
        public override void ToPSBrush(PSGraphicsInfo graphicsPS)
        {
            graphicsPS.PS_setrgbcolor(this.Color);
        }

        public override bool IsEmptyColor()
		{
            return (this.Color.IsEmpty || this.Color.A == 0);
		}

		public SolidBrush(Color color)
        {
            this._brushGdi = new System.Drawing.SolidBrush(color.ToColor());
            this._color = color;
        }


    }

    [Serializable, DataContract(Name = "HatchBrush", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class HatchBrush : Brush
	{
        public HatchStyle HatchStyle { get { return ((System.Drawing.Drawing2D.HatchBrush)this._brushGdi).HatchStyle; } }
        private Color _foregroundColor;
        public Color ForegroundColor { get { return _foregroundColor; } set { _foregroundColor = new Color(value); } }
        private Color _backgroundColor;
        public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = new Color(value); } }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        public override void ToPdfBrush(PdfGraphicsInfo graphicsPdf)
        {

        }
        public override void ToPSBrush(PSGraphicsInfo graphicsPS)
        {

        }

        public override bool IsEmptyColor()
        {
            return ((this.ForegroundColor.IsEmpty || this.ForegroundColor.A == 0) 
                     && (this.ForegroundColor.IsEmpty || this.ForegroundColor.A == 0));
        }

        public HatchBrush(HatchStyle hatchstyle, Color foreColor) :
            this(hatchstyle, foreColor, Color.FromARGBBytes(1, 0, 0, 0))
        {
        }

        public HatchBrush(HatchStyle hatchstyle, Color foreColor, Color backColor)
        {
            this._brushGdi = new System.Drawing.Drawing2D.HatchBrush(hatchstyle, foreColor.ToColor(), backColor.ToColor());
            this.ForegroundColor = foreColor;
            this.BackgroundColor = backColor;
        }

    }

    [Serializable, DataContract(Name = "PathGradientBrush", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class PathGradientBrush : Brush
	{
        private PointF[] _points;
        public PointF[] Points { get { return _points; }  
            set {
                if (value == null || value.Length == 0)
				{
                    _points = value;
                    return;
				}
                _points = new PointF[value.Length];
                for (int i = 0; i < value.Length; i++)
                    _points[i] = value[i];
            }
        }
        public WrapMode WrapMode { 
            get { return ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).WrapMode; }
            set { ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).WrapMode = value; } }
        private GraphicsPath _graphicsPath;
        public GraphicsPath GraphicsPath { get { return _graphicsPath; } set { _graphicsPath = (GraphicsPath)value.Clone(); } }

        private Color _centerColor;
        public Color CenterColor { get { return _centerColor; } set { _centerColor = new Color(value); } }
        private Color[] _sourroundColors;
        public Color[] SurroundColors
        {
            get { return _GetSurroundColors(); }
            set { _SetSurroundColors(value); }
        }
        public PointF CenterPoint { 
            get { return ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).CenterPoint; } 
            set { ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).CenterPoint = value; } }
        public Matrix Transform { 
            get { return ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).Transform; }
            set { ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).Transform = value; } }
        public PointF FocusScales { 
            get { return ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).FocusScales; }
            set { ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).FocusScales = value; } }
        public Blend Blend
        {
            get { return ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).Blend; }
            set { ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).Blend = value; }
        }
        private ColorBlend _interpolationColors;

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        public override void ToPdfBrush(PdfGraphicsInfo graphicsPdf)
        {

        }
        public override void ToPSBrush(PSGraphicsInfo graphicsPS)
        {

        }
        public override bool IsEmptyColor()
        {
            return false;
        }

        public PathGradientBrush(PointF[] points)
            : this(points, System.Drawing.Drawing2D.WrapMode.Clamp)
        {
        }

        public PathGradientBrush(PointF[] points, WrapMode wrapMode)
		{
            if (points == null)
                throw new ArgumentNullException("points");

            this._brushGdi = new System.Drawing.Drawing2D.PathGradientBrush(points, wrapMode);
            this.Points = points;
        }

        public PathGradientBrush(Point[] points)
            : this(points, System.Drawing.Drawing2D.WrapMode.Clamp)
        {
        }

        public PathGradientBrush(Point[] points, WrapMode wrapMode)
		{
            if (points == null)
                throw new ArgumentNullException("points");

            this._brushGdi = new System.Drawing.Drawing2D.PathGradientBrush(points, wrapMode);
            PointF[] lptfPts = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                lptfPts[i] = points[i];
            this.Points = lptfPts;
        }

        public PathGradientBrush(GraphicsPath path)
		{
            if (path == null)
                throw new ArgumentNullException("path");

            this._brushGdi = new System.Drawing.Drawing2D.PathGradientBrush(path);
            this.GraphicsPath = path;
        }


        private void _SetSurroundColors(Color[] colors)
        {
            System.Drawing.Color[] colorsGdi = new System.Drawing.Color[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                colorsGdi[i] = colors[i].ToColor();
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).SurroundColors = colorsGdi;

            int count = colors.Length;
            this._sourroundColors = new Color[count];

            for (int i = 0; i < colors.Length; i++)
                this._sourroundColors[i] = new Color(colors[i]);

        }

        private Color[] _GetSurroundColors()
        {
            int count = this._sourroundColors.Length;

            Color[] colors = new Color[count];
            for (int i = 0; i < count; i++)
                colors[i] = new Color(this._sourroundColors[i]);

            return colors;
        }




        private ColorBlend _GetInterpolationColors()
        {
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
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).InterpolationColors = colorBlend.ToGdiColorBlend();

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


        public void ResetTransform()
        {
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).ResetTransform();
        }

        public void MultiplyTransform(Matrix matrix)
        {
            MultiplyTransform(matrix, MatrixOrder.Prepend);
        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).MultiplyTransform(matrix, order);
        }

        public void TranslateTransform(float dx, float dy)
        {
            TranslateTransform(dx, dy, MatrixOrder.Prepend);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).TranslateTransform(dx, dy, order);
        }

        public void ScaleTransform(float sx, float sy)
        {
            ScaleTransform(sx, sy, MatrixOrder.Prepend);
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).ScaleTransform(sx, sy, order);
        }

        public void RotateTransform(float angle)
        {
            RotateTransform(angle, MatrixOrder.Prepend);
        }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            ((System.Drawing.Drawing2D.PathGradientBrush)this._brushGdi).RotateTransform(angle, order);
        }

    }

}
