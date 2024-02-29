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
    [Serializable, DataContract(Name = "TextureBrush", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class TextureBrush : Brush
    {
        public Image Image { get { return ((System.Drawing.TextureBrush)this._brushGdi).Image; } }
        public WrapMode WrapMode {
            get { return ((System.Drawing.TextureBrush)this._brushGdi).WrapMode; }
            set { ((System.Drawing.TextureBrush)this._brushGdi).WrapMode = value; } }
        private RectangleF _dstRect = new RectangleF();
        public RectangleF DstRect { get { return _dstRect; } set { _dstRect = value; } }
        private ImageAttributes _imageAttr = new ImageAttributes();
        public ImageAttributes ImageAttr { get { return _imageAttr; } set { _imageAttr = value; } }

        public override void ToPdfBrush(PdfGraphicsInfo graphicsPdf)
        {

        }

        public override void ToPSBrush(PSGraphicsInfo graphicsPS)
        {

        }

        public override bool IsEmptyColor()
        {
            return (this.Image == null);
        }

        public TextureBrush(Image bitmap)
            : this(bitmap, System.Drawing.Drawing2D.WrapMode.Tile)
        {
        }

        public TextureBrush(Image image, WrapMode wrapMode)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this._brushGdi = new System.Drawing.TextureBrush(image, wrapMode);
        }


        public TextureBrush(Image image, WrapMode wrapMode, RectangleF dstRect)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this._brushGdi = new System.Drawing.TextureBrush(image, wrapMode, dstRect);
            this.DstRect = dstRect;
        }

        public TextureBrush(Image image, WrapMode wrapMode, Rectangle dstRect)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this._brushGdi = new System.Drawing.TextureBrush(image, wrapMode, dstRect);
            this.DstRect = dstRect;
        }


        public TextureBrush(Image image, RectangleF dstRect)
        : this(image, dstRect, (ImageAttributes)null) { }

        public TextureBrush(Image image, RectangleF dstRect,
                            ImageAttributes imageAttr)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this._brushGdi = new System.Drawing.TextureBrush(image, dstRect, imageAttr);
            this.DstRect = dstRect;
            this.ImageAttr = imageAttr;
        }

        public TextureBrush(Image image, Rectangle dstRect)
        : this(image, dstRect, (ImageAttributes)null) { }

        public TextureBrush(Image image, Rectangle dstRect,
                            ImageAttributes imageAttr)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this._brushGdi = new System.Drawing.TextureBrush(image, dstRect, imageAttr);
            this.DstRect = dstRect;
            this.ImageAttr = imageAttr;
        }

        public override Object Clone()
        {
            return this.MemberwiseClone();
        }


        public Matrix Transform
        {
            get { return ((System.Drawing.TextureBrush)this._brushGdi).Transform; }
            set { ((System.Drawing.TextureBrush)this._brushGdi).Transform = value; }
        }


        public void ResetTransform()
        {
            ((System.Drawing.TextureBrush)this._brushGdi).ResetTransform();
        }

        public void MultiplyTransform(Matrix matrix)
        { MultiplyTransform(matrix, MatrixOrder.Prepend); }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            ((System.Drawing.TextureBrush)this._brushGdi).MultiplyTransform(matrix, order);
        }

        public void TranslateTransform(float dx, float dy)
        { TranslateTransform(dx, dy, MatrixOrder.Prepend); }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            ((System.Drawing.TextureBrush)this._brushGdi).TranslateTransform(dx, dy, order);
        }

        public void ScaleTransform(float sx, float sy)
        { ScaleTransform(sx, sy, MatrixOrder.Prepend); }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            ((System.Drawing.TextureBrush)this._brushGdi).ScaleTransform(sx, sy, order);
        }

        public void RotateTransform(float angle)
        { RotateTransform(angle, MatrixOrder.Prepend); }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            ((System.Drawing.TextureBrush)this._brushGdi).RotateTransform(angle, order);
        }
    }

}
