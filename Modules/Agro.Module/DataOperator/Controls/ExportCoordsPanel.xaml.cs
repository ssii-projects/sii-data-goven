using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/*
yxm created at 2019/3/8 11:21:08
*/
namespace Agro.Module.DataOperator
{
	/// <summary>
	/// ExportCoordsPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ExportCoordsPanel : UserControl
	{
		public ExportCoordsPanel()
		{
			InitializeComponent();
			var path=MyGlobal.Persist.LoadSettingInfo("A7EBDAA37-6547-4166-99B0-11BFEFFAAFC0").ToString();
			txtPath.Text = path;
		}
		public string OutPath { get; private set; }
		public string Export(IFeatureLayer fl, out int nExportCount)
		{
			nExportCount = 0;
			string err = null;
			var dir = txtPath.Text.Trim();
			if (dir.Length == 0)
			{
				err = "未选择输出文件！";
				return err;
			}
			if (!Directory.Exists(dir))
			{
				err = "文件路径不存在！";
				return err;
			}
			OutPath = dir;
			nExportCount = ExportSelection(fl, dir);
			SaveState();
			return err;
		}
		private int ExportSelection(IFeatureLayer fl, string outPath)//, ShapeFile shp)
		{
			var fc = fl.FeatureClass;
			var qf = new SpatialFilter()
			{
				SubFields =fc.ShapeFieldName+",CBFMC,DKBM",// "Shape"// MakeExportSubFields(fc)
			};
			int nCurrShapeID = 0;
			qf.Oids = fl.SelectionSet;
			fc.Search(qf, r => {
				nCurrShapeID = Export(r, nCurrShapeID,outPath);
				return true;
			});
			return nCurrShapeID;
		}
		private int Export(IRow r, int nCurrShapeID,string outPath)
		{
			var g = (r as IFeature).Shape;
			var cbfmc = SafeConvertAux.ToStr(IRowUtil.GetRowValue(r, "CBFMC"));
			var dkbm = SafeConvertAux.ToStr(IRowUtil.GetRowValue(r, "dkbm"));
			var sa=g.Coordinates;
			string str = null;
			for (int i = 0; i < sa.Length; ++i)
			{
				var c = sa[i];
				string s = (i + 1) + "," + Math.Round(c.X, 2) + "," + Math.Round(c.Y, 2)+",";
				if (str == null) str = s;
				else str += "\r\n" + s;
			}
			if (!(outPath.EndsWith("/") || outPath.EndsWith("\\")))
			{
				outPath += "\\";
			}
			File.WriteAllText(outPath + cbfmc + dkbm + ".txt",str);
			return ++nCurrShapeID;
		}
		private void SaveState()
		{
			MyGlobal.Persist.SaveSettingInfo("A7EBDAA37-6547-4166-99B0-11BFEFFAAFC0", txtPath.Text);
		}
	}
}
