using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Agro.LibCore.AST
{
  //  public static class TextUtil
  //  {
  //      public static Font GetDefaultFont()
  //      {
  //          return new Font("Arial", 16, FontStyle.Regular);
  //      }
  //      private static SizeF CalcTextSize(System.Drawing.Graphics g, string text, Font font)
  //      {
  //          return g.MeasureString(text, font);
  //      }
  //      private static MyRect CalcTextRect(System.Drawing.Graphics g, string text, Font font)
  //      {
  //          var sz = CalcTextSize(g, text, font);
  //          return new MyRect(0, 0, sz.Width, sz.Height);
  //      }
  //      public static System.Windows.Media.Typeface GetTypeface(string fontName = "Arial", bool bold = false)
  //      {
  //          System.Windows.Media.Typeface tf = new System.Windows.Media.Typeface(new System.Windows.Media.FontFamily(fontName),
  //                        System.Windows.FontStyles.Normal,
  //                        bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
  //                        System.Windows.FontStretches.Normal);
  //          return tf;
  //      }
  //      public static MyRect CalcTextRect(string text, System.Windows.Media.Typeface typeFace, double fontSize = 25)
  //      {
  //          //if (text == "日期") fontSize = 100;
  //          System.Windows.Media.FormattedText ft = new System.Windows.Media.FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight
  //              , typeFace, fontSize, System.Windows.Media.Brushes.Black);
  //          return new MyRect(0, 0, ft.Width, ft.Height);
  //      }
  //      /// <summary>
  //      /// 用于解决Graphics.MeasureString度量不准确的问题
  //      /// </summary>
  //      /// <param name="text"></param>
  //      /// <param name="font"></param>
  //      /// <returns></returns>
  //      public static SizeF MeasureString(string text, Font font, StringFormat format,double dpi=0)//, double fontSize = 25)
  //      {
		//	return GDIPlusUtil.MeasureString(text, font, format, dpi);
		//}
  //  }
}
