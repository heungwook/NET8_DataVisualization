
#region Used namespaces

using System.Resources;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.IO;
using System.Xml;
using System.Reflection;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;
using System.Globalization;


using Orion.DataVisualization.Charting.Data;
using Orion.DataVisualization.Charting.ChartTypes;
using Orion.DataVisualization.Charting.Utilities;
using Orion.DataVisualization.Charting.Borders3D;
using Orion.DataVisualization.Charting;
using Orion.DataVisualization.Charting.Formulas;
using System.Net;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Orion.DataVisualization.Charting.Options
{
    public class OptionsLabelStyle
    {
        public bool cbUseForceAlignment;
        public StringAlignment ceHorzAlignment;
        public StringAlignment ceVertAlignment;

        public OptionsLabelStyle()
        {
            cbUseForceAlignment = false;
            ceHorzAlignment = StringAlignment.Center;
            ceVertAlignment = StringAlignment.Center;
    }
    }
}
