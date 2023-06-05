/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   SymbolUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/11/23 10:50:17
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Agro.GIS
{
    public static class SymbolUtil
    {
        public static ISymbol CreateSimpleMarkerSymbol(Color fillColor
            , Color outLineColor, double markerSize, eSimpleMarkerStyle style = eSimpleMarkerStyle.Circle, double strokeThiness = 1.0)
        {
            var symbol = GisGlobal.SymbolFactory.CreateSimpleMarkerSymbol();
            symbol.FillColor = fillColor;
            symbol.Strock = new PenProperty(outLineColor, strokeThiness);
            symbol.Size = markerSize;
            symbol.Style = style;
            return symbol;
        }
        public static ISymbol CreateSimpleMarkerSymbol(string fillColor
     , string outLineColor, double markerSize, eSimpleMarkerStyle style = eSimpleMarkerStyle.Circle, double strokeThiness = 1.0)
        {
            return CreateSimpleMarkerSymbol(ColorUtil.ConvertFromString(fillColor), ColorUtil.ConvertFromString(outLineColor),
                markerSize, style, strokeThiness);
        }
        public static ILineSymbol CreateSimpleLineSymbol(Color strokeColor, double strokeThickness = 1)
        {
            var symbol = GisGlobal.SymbolFactory.CreateSimpleLineSymbol();
            {
                symbol.StrokeColor = strokeColor;
                symbol.StrokeThickness = strokeThickness;
            };
            return symbol;
        }
        public static ILineSymbol CreateSimpleLineSymbol(string strokeColor, double strokeThickness = 1)
        {
            return CreateSimpleLineSymbol(ColorUtil.ConvertFromString(strokeColor), strokeThickness);
        }


        public static ISymbol CreateDashLineSymbol(Color strokeColor, double strokeThickness, double interval = 1, double mark = 4, double gap = 4)
        {
            return CreateSimpleLineSymbol(strokeColor, strokeThickness);
            //throw new NotImplementedException();
			//var symbol = new CartographicLineSymbol()
			//{
			//	StrokeColor = strokeColor,
			//	StrokeThickness=strokeThickness,
			//};
   //         symbol.Template.Interval = interval;
   //         symbol.Template.AddPatternElement(mark,gap);			
   //         return symbol;
        }
        public static ISymbol CreateDashLineSymbol(string strokeColor, double strokeThickness, double interval = 1, double mark = 4, double gap = 4)
        {
            return CreateDashLineSymbol(ColorUtil.ConvertFromString(strokeColor), strokeThickness, interval, mark, gap);
        }
        public static IFillSymbol CreateSimpleFillSymbol(Color fillColor, ILineSymbol? outline)
        {
            var symbol = GisGlobal.SymbolFactory.CreateSimpleFillSymbol();
            symbol.FillColor = fillColor;
            symbol.Outline = outline;
            return symbol;
        }
        public static IFillSymbol CreateSimpleFillSymbol(string fillColor, ILineSymbol outline)
        {
            return CreateSimpleFillSymbol(ColorUtil.ConvertFromString(fillColor), outline);
        }
        public static IFillSymbol CreateSimpleFillSymbol(string fillColor, string lineColor, double lineWidth = 1)
        {            
            return CreateSimpleFillSymbol(fillColor, CreateSimpleLineSymbol(lineColor, lineWidth));
        }

        public static ISymbol CreateDefaultSelectionMarkerSymbol(double size=15)
        {
            return CreateSimpleMarkerSymbol(Color.FromArgb(100, 174, 97, 170), Color.FromArgb(255, 174, 97, 170), size);
        }

        public static ISymbol CreateDefaultSelectionLineSymbol(double strokeThickness = 3)
        {
            return CreateSimpleLineSymbol(Color.FromArgb(255, 174, 97, 170), strokeThickness);
        }
        public static ISymbol CreateDefaultSelectionFillSymbol(ILineSymbol outline)
        {
            return CreateSimpleFillSymbol(Color.FromArgb(100, 174, 97, 170), outline);
        }

		public static ITextSymbol MakeTextSymbol(double fontSize)
		{
			var symbol =GisGlobal.SymbolFactory.CreateTextSymbol();
			symbol.Font.FontSize = (float)fontSize;
			return symbol;
		}
    }
}
