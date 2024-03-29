using System.Drawing;

namespace Orion.DataVisualization.Charting.Options.Svg.Path
{
    public sealed class SvgLineSegment : SvgPathSegment
    {
        public SvgLineSegment(PointF start, PointF end)
        {
            this.Start = start;
            this.End = end;
        }

        public override void AddToPath(System.Drawing.Drawing2D.GraphicsPath graphicsPath)
        {
            graphicsPath.AddLine(this.Start, this.End);
        }
        
        public override string ToString()
		{
        	return "L" + this.End.ToSvgString();
		}

    }
}