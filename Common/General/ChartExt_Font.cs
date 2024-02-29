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
using OrionX2.ConfigInfo;

#if Microsoft_CONTROL
namespace Orion.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    [Serializable, DataContract(Name = "Font", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class XFont : OrionX2.OrionFont.NFont
    {
        public static string _DefaultFontName = "Malgun Gothic";
        public string csFontname {
            get { if (cFInfo == null) cFInfo = GetDefaultFontInfo(); return cFInfo.csFontName_CurLocale;  } 
        }
        //
        [OnDeserializing]
        private void OnDeserializing(StreamingContext lSC)
        {
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext lSC)
        {
        }
        [OnSerializing]
        private void OnSerializing(StreamingContext lSC)
        {
        }
        [OnSerialized]
        private void OnSerialized(StreamingContext lSC)
        {
        }
        //

        public XFont(string lsFontName) : base(lsFontName)
        {
            
        }
        public XFont(string lsFontName, double ldFontSizePt) : base(lsFontName, ldFontSizePt)
        {

        }

        public OrionX2.OrionFont.FontInfo GetDefaultFontInfo()
		{
            OrionX2.OrionFont.FontInfo lFI = null;
            if (OrionX2.Render.RenderInfo._FontMgr == null)
                lFI = new OrionX2.OrionFont.FontInfo();
            else
                lFI = OrionX2.Render.RenderInfo._FontMgr.FindFontInfo(_DefaultFontName);
            return lFI;
        }

        public System.Drawing.Font ToGdiFont()
		{
            FontStyle gdiFontStyle = FontStyle.Regular;
            if (this.cbBold)
                gdiFontStyle |= FontStyle.Bold;
            if (this.cbItalic)
                gdiFontStyle |= FontStyle.Italic;
            System.Drawing.Font gdiFont = new System.Drawing.Font(this.cFInfo.csFontName_en_US, (float)this.cdSize, gdiFontStyle);
            return gdiFont;
		}

        public void FromGdiFont(System.Drawing.Font gdiFont, double Dpi)
		{
            this.cbBold = gdiFont.Bold;
            this.cbItalic = gdiFont.Italic;
            this.cdSize = gdiFont.Size;
            if (gdiFont.Unit == GraphicsUnit.Pixel)
                this.cdSize = UCNV.GetPointFromPixel(gdiFont.Size, Dpi);
            else if (gdiFont.Unit == GraphicsUnit.Display)
                this.cdSize = (float)UCNV.GetDIUFromPixel(gdiFont.Size, Dpi);
            if (OrionX2.Render.RenderInfo._FontMgr != null)
                this.cFInfo = OrionX2.Render.RenderInfo._FontMgr.FindFontInfo(gdiFont.Name);
		}

    }
}
