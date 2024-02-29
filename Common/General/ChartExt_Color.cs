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
    [Serializable, DataContract(Name = "Color", Namespace = "http://ns.orionsoft.com/Orion/DataVisualization/Charting")]
    public class Color : OrionX2.ConfigInfo.OrionColor, IEquatable<Color>
    {
        public new int A { get { return (int)(Math.Round(base.A * 255.0)); } set { base.A = ((double)value) / 255.0; } }
        public new int R { get { return (int)(Math.Round(base.R * 255.0)); } set { base.R = ((double)value) / 255.0; } }
        public new int G { get { return (int)(Math.Round(base.G * 255.0)); } set { base.G = ((double)value) / 255.0; } }
        public new int B { get { return (int)(Math.Round(base.B * 255.0)); } set { base.B = ((double)value) / 255.0; } }


        public Color()
        {
            IsEmpty = false;
            this._RGB = new ColorRGB();
            this._CMYK = RgbToCmyk(this._RGB);
            IsSpotColor = false;
            SpotColorName = "White";
            SpotColorTint = 1.0;
        }
        public Color(System.Drawing.Color lARGB)
        {
            IsEmpty = false;
            this._RGB = new ColorRGB(lARGB);
            this._CMYK = RgbToCmyk(this._RGB);
            IsSpotColor = false;
            SpotColorName = "White";
            SpotColorTint = 1.0;
        }

        public Color(Color lInit)
        {
            IsEmpty = lInit.IsEmpty;
            this._RGB = new ColorRGB(lInit.RGB);
            this._CMYK = new ColorCMYK(lInit.CMYK);
            IsSpotColor = lInit.IsSpotColor;
            SpotColorName = lInit.SpotColorName;
            SpotColorTint = lInit.SpotColorTint;
        }

        public Color(ColorRGB lInit)
        {
            IsEmpty = false;
            this._RGB = new ColorRGB(lInit);
            this._CMYK = RgbToCmyk(this._RGB);
            IsSpotColor = false;
            SpotColorName = "White";
            SpotColorTint = 1.0;
        }

        public Color(ColorCMYK lInit)
        {
            IsEmpty = false;
            this._CMYK = new ColorCMYK(lInit);
            this._RGB = CmykToRgb(this._CMYK);
            IsSpotColor = false;
            SpotColorName = "White";
            SpotColorTint = 1.0;

        }

        #region Static Properties

        public static Color Transparent { get => new Color(System.Drawing.Color.FromArgb(0, 0, 0, 0)); }

        public static Color Empty { get {
                Color lCrEmpty = new Color();
                lCrEmpty.IsEmpty = true;
                return lCrEmpty;
            }
        }

        public new static Color FromRGBBytes(byte lbyRed, byte lbyGreen, byte lbyBlue)
        {
            return new Color(new ColorRGB(1.0, lbyRed / 255.0, lbyGreen / 255.0, lbyBlue / 255.0));
        }
        public new static Color FromARGBBytes(byte lbyAlpha, byte lbyRed, byte lbyGreen, byte lbyBlue)
        {
            return new Color(new ColorRGB(lbyAlpha / 255.0, lbyRed / 255.0, lbyGreen / 255.0, lbyBlue / 255.0));
        }
        public new static Color FromCMYKBytes(byte lbyCyan, byte lbyMagenta, byte lbyYellow, byte lbyKey)
        {
            return new Color(new ColorCMYK(1.0, lbyCyan / 255.0, lbyMagenta / 255.0, lbyYellow / 255.0, lbyKey / 255.0));
        }
        public new static Color FromACMYKBytes(byte lbyAlpha, byte lbyCyan, byte lbyMagenta, byte lbyYellow, byte lbyKey)
        {
            return new Color(new ColorCMYK(lbyAlpha / 255.0, lbyCyan / 255.0, lbyMagenta / 255.0, lbyYellow / 255.0, lbyKey / 255.0));
        }

        public static Color FromArgb(double ldAlpha0_255, Color lCR)
        {
            Color lNewColor = new Color(lCR);
            lNewColor.Alpha = ldAlpha0_255 / 255.0;
            return lNewColor;
        }

        public static Color FromArgb(double ldAlpha, System.Drawing.Color lCR)
        {
            return new Color(System.Drawing.Color.FromArgb((int)ldAlpha, lCR.R, lCR.G, lCR.B));

        }

        public static Color FromArgb(int liAlpha, int liRed, int liGreen, int liBlue)
        {
            return new Color(System.Drawing.Color.FromArgb(liAlpha, liRed, liGreen, liBlue));

        }

        public static Color FromArgb(int liRed, int liGreen, int liBlue)
        {
            return Color.FromARGBBytes(255, (byte)liRed, (byte)liGreen, (byte)liBlue);
        }

        public static Color FromGdiColor(System.Drawing.Color lGdiColor)
        {
            return new Color(lGdiColor);
        }


        public static Color FromArgb(Color lCR)
        {
            return new Color(lCR);

        }

        public static bool TryParseACMYKBytes(string lsACMYK, out Color lACMYKColor)
        {
            lACMYKColor = new Color();
            bool lbSuccess = false;
            try
            {
                int liA, liC, liM, liY, liK;
                if (lsACMYK.Contains("ACMYK:"))
                    lsACMYK = lsACMYK.Replace("ACMYK:", "");
                string[] lsaACMYK = lsACMYK.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (lsaACMYK.Length != 5 || !int.TryParse(lsaACMYK[0], out liA)
                    || !int.TryParse(lsaACMYK[1], out liC) || !int.TryParse(lsaACMYK[2], out liM)
                    || !int.TryParse(lsaACMYK[3], out liY) || !int.TryParse(lsaACMYK[4], out liK))
                    return false;
                lACMYKColor.A = liA;
                lACMYKColor.C = liC;
                lACMYKColor.M = liM;
                lACMYKColor.Y = liY;
                lACMYKColor.K = liK;
                lbSuccess = true;
            }
            catch
            {
                lbSuccess = false;
            }
            return lbSuccess;
        }

        public string ToStringACMYKBytes()
        {
            return "ACMYK:" + A.ToString() + "," + C.ToString() + "," + M.ToString() + "," + Y.ToString() + "," + K.ToString();
        }
        public iTextSharp.text.pdf.PdfSpotColor ToPdfSpotColor()
        {
            return new iTextSharp.text.pdf.PdfSpotColor(
                        this.SpotColorName,
                        new iTextSharp.text.pdf.CMYKColor((float)this.CMYK.C, (float)this.CMYK.M,
                                                          (float)this.CMYK.Y, (float)this.CMYK.K));
        }
        public iTextSharp.text.BaseColor ToPdfBaseColor(PdfGraphicsInfo.EnumColorSpace colorSpace) // SpotColor Not Supported
		{
            iTextSharp.text.BaseColor pdfColor = null;
            if (this.IsEmpty || this.A == 0)
                return pdfColor;


            if (colorSpace == PdfGraphicsInfo.EnumColorSpace.DeviceGray)
            {
                pdfColor = new iTextSharp.text.pdf.GrayColor(this.GetGray());
            }
            else if (colorSpace == PdfGraphicsInfo.EnumColorSpace.DeviceRGB)
            {
                pdfColor = new iTextSharp.text.BaseColor(this.ToColor());
            }
            else if (colorSpace == PdfGraphicsInfo.EnumColorSpace.DeviceCMYK)
            {
                pdfColor = new iTextSharp.text.pdf.CMYKColor((float)this.CMYK.C, (float)this.CMYK.M, 
                                                             (float)this.CMYK.Y, (float)(this.CMYK.K));

            }

            return pdfColor;
        }

        //
        //
        public static Color White { get => Color.FromACMYKBytes(255, 0, 0, 0, 0); }
        //
        public new static Color Cyan { get => Color.FromACMYKBytes(255, 255, 0, 0, 0); }
        public new static Color Magenta { get => Color.FromACMYKBytes(255, 0, 255, 0, 0); }
        public new static Color Yellow { get => Color.FromACMYKBytes(255, 0, 0, 255, 0); }
        public new static Color Black { get => Color.FromACMYKBytes(255, 0, 0, 0, 255); }
        public new static Color Red { get => Color.FromARGBBytes(255, 255, 0, 0); }
        public new static Color Green { get => Color.FromARGBBytes(255, 0, 255, 0); }
        public new static Color Blue { get => Color.FromARGBBytes(255, 0, 0, 255); }
        //
        public static Color Gray { get => Color.FromARGBBytes(255, 128, 128, 128); }
        public static Color DarkGray { get => FromGdiColor(System.Drawing.Color.DarkGray); }
        public static Color LightGray { get => FromGdiColor(System.Drawing.Color.LightGray); }
        public static Color Purple { get => FromGdiColor(System.Drawing.Color.Purple); }
        public static Color Lime { get => FromGdiColor(System.Drawing.Color.Lime); }
        public static Color Fuchsia { get => FromGdiColor(System.Drawing.Color.Fuchsia); }
        public static Color Teal { get => FromGdiColor(System.Drawing.Color.Teal); }
        public static Color Aqua { get => FromGdiColor(System.Drawing.Color.Aqua); }
        public static Color Navy { get => FromGdiColor(System.Drawing.Color.Navy); }
        public static Color Maroon { get => FromGdiColor(System.Drawing.Color.Maroon); }
        public static Color Olive { get => FromGdiColor(System.Drawing.Color.Olive); }
        public static Color Silver { get => FromGdiColor(System.Drawing.Color.Silver); }
        public static Color Tomato { get => FromGdiColor(System.Drawing.Color.Tomato); }
        public static Color Moccasin { get => FromGdiColor(System.Drawing.Color.Moccasin); }
        public static Color SkyBlue { get => FromGdiColor(System.Drawing.Color.SkyBlue); }
        public static Color LimeGreen { get => FromGdiColor(System.Drawing.Color.LimeGreen); }
        public static Color MediumOrchid { get => FromGdiColor(System.Drawing.Color.MediumOrchid); }
        public static Color LightCoral { get => FromGdiColor(System.Drawing.Color.LightCoral); }
        public static Color SteelBlue { get => FromGdiColor(System.Drawing.Color.SteelBlue); }
        public static Color YellowGreen { get => FromGdiColor(System.Drawing.Color.YellowGreen); }
        public static Color Turquoise { get => FromGdiColor(System.Drawing.Color.Turquoise); }
        public static Color HotPink { get => FromGdiColor(System.Drawing.Color.HotPink); }
        public static Color Khaki { get => FromGdiColor(System.Drawing.Color.Khaki); }
        public static Color Tan { get => FromGdiColor(System.Drawing.Color.Tan); }
        public static Color DarkSeaGreen { get => FromGdiColor(System.Drawing.Color.DarkSeaGreen); }
        public static Color CornflowerBlue { get => FromGdiColor(System.Drawing.Color.CornflowerBlue); }
        public static Color Plum { get => FromGdiColor(System.Drawing.Color.Plum); }
        public static Color CadetBlue { get => FromGdiColor(System.Drawing.Color.CadetBlue); }
        public static Color PeachPuff { get => FromGdiColor(System.Drawing.Color.PeachPuff); }
        public static Color LightSalmon { get => FromGdiColor(System.Drawing.Color.LightSalmon); }
        public static Color DarkGoldenrod { get => FromGdiColor(System.Drawing.Color.DarkGoldenrod); }
        public static Color OliveDrab { get => FromGdiColor(System.Drawing.Color.OliveDrab); }
        public static Color Peru { get => FromGdiColor(System.Drawing.Color.Peru); }
        public static Color ForestGreen { get => FromGdiColor(System.Drawing.Color.ForestGreen); }
        public static Color Chocolate { get => FromGdiColor(System.Drawing.Color.Chocolate); }
        public static Color LightSeaGreen { get => FromGdiColor(System.Drawing.Color.LightSeaGreen); }
        public static Color SandyBrown { get => FromGdiColor(System.Drawing.Color.SandyBrown); }
        public static Color Firebrick { get => FromGdiColor(System.Drawing.Color.Firebrick); }
        public static Color SaddleBrown { get => FromGdiColor(System.Drawing.Color.SaddleBrown); }
        public static Color Lavender { get => FromGdiColor(System.Drawing.Color.Lavender); }
        public static Color LavenderBlush { get => FromGdiColor(System.Drawing.Color.LavenderBlush); }
        public static Color LemonChiffon { get => FromGdiColor(System.Drawing.Color.LemonChiffon); }
        public static Color MistyRose { get => FromGdiColor(System.Drawing.Color.MistyRose); }
        public static Color Honeydew { get => FromGdiColor(System.Drawing.Color.Honeydew); }
        public static Color AliceBlue { get => FromGdiColor(System.Drawing.Color.AliceBlue); }
        public static Color WhiteSmoke { get => FromGdiColor(System.Drawing.Color.WhiteSmoke); }
        public static Color AntiqueWhite { get => FromGdiColor(System.Drawing.Color.AntiqueWhite); }
        public static Color LightCyan { get => FromGdiColor(System.Drawing.Color.LightCyan); }
        public static Color BlueViolet { get => FromGdiColor(System.Drawing.Color.BlueViolet); }
        public static Color RoyalBlue { get => FromGdiColor(System.Drawing.Color.RoyalBlue); }
        public static Color MediumVioletRed { get => FromGdiColor(System.Drawing.Color.MediumVioletRed); }
        public static Color Orchid { get => FromGdiColor(System.Drawing.Color.Orchid); }
        public static Color MediumSlateBlue { get => FromGdiColor(System.Drawing.Color.MediumSlateBlue); }
        public static Color MediumBlue { get => FromGdiColor(System.Drawing.Color.MediumBlue); }
        public static Color Sienna { get => FromGdiColor(System.Drawing.Color.Sienna); }
        public static Color DarkRed { get => FromGdiColor(System.Drawing.Color.DarkRed); }       
        public static Color Brown { get => FromGdiColor(System.Drawing.Color.Brown); }        
        public static Color Gold { get => FromGdiColor(System.Drawing.Color.Gold); }     
        public static Color DeepPink { get => FromGdiColor(System.Drawing.Color.DeepPink); }      
        public static Color Crimson { get => FromGdiColor(System.Drawing.Color.Crimson); }
        public static Color Orange { get => FromGdiColor(System.Drawing.Color.Orange); }
        public static Color DarkOrange { get => FromGdiColor(System.Drawing.Color.DarkOrange); }
        public static Color OrangeRed { get => FromGdiColor(System.Drawing.Color.OrangeRed); }
        public static Color SeaGreen { get => FromGdiColor(System.Drawing.Color.SeaGreen); }
        public static Color MediumAquamarine { get => FromGdiColor(System.Drawing.Color.MediumAquamarine); }
        public static Color DarkCyan { get => FromGdiColor(System.Drawing.Color.DarkCyan); }
        public static Color MediumSeaGreen { get => FromGdiColor(System.Drawing.Color.MediumSeaGreen); }
        public static Color MediumTurquoise { get => FromGdiColor(System.Drawing.Color.MediumTurquoise); }
        public static Color LightSteelBlue { get => FromGdiColor(System.Drawing.Color.LightSteelBlue); }



        #endregion // Static Properties


        public virtual bool Equals(Color obj)
		{
            return Equals(obj as OrionColor);
		}

        #region Static Methods

        public static bool operator ==(Color lhs, Color rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);
        #endregion // Static Methods


        public int GetGray()
        {
            double ldGray = 0.299 * _RGB.R + 0.587 * _RGB.G + 0.114 * _RGB.B;

            if (ldGray < 0.0)
                ldGray = 0.0;
            if (ldGray > 255.0)
                ldGray = 255.0;

            return (int)ldGray;
        }

    }
}
