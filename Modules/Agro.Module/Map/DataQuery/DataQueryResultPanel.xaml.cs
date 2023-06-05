using GeoAPI.Geometries;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.NPIO;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Agro.Module.Map
{
	/// <summary>
	/// DataQueryResultPanel.xaml 的交互逻辑
	/// </summary>
	public partial class DataQueryResultPanel : UserControl
	{
		class DkItem : NotificationObject
		{
			public string DkMc
			{
				get;set;
			}
			public string DkBm { get; set; }
			public string Tdyt { get; set; }
			public string Jbnt { get; set; }
			public string Dldj { get; set; }
			public double	Scmj{ get; set; }
			public int Oid;
		}
		class DkFieldIndex
		{
			public int iDkmc;
			public int iDkbm;
			public int iTdyt;
			public int iJbnt;
			public int iDldj;
			public int iScmj;
			public int iQqmj;
			public int iScmjm;
			public int iELHTMJ;
			//public int iFbfbm;
			public DkFieldIndex(IFeature ft)
			{
				var fds = ft.Fields;
				iDkmc = fds.FindField("DKMC");
				iDkbm = fds.FindField("DKBM");
				iTdyt = fds.FindField("TDYT");
				iJbnt = fds.FindField("SFJBNT");
				iDldj = fds.FindField("DLDJ");
				iScmj = fds.FindField("SCMJ");
				iQqmj = fds.FindField("QQMJ");
				iScmjm = fds.FindField("SCMJM");
				iELHTMJ = fds.FindField("ELHTMJ");
				//iFbfbm = fds.FindField("FBFBM");
			}
		}

		class CbfItem : NotificationObject
		{
			public string Cbfmc { get; set; }
			public string Cbfbm;
			public override string ToString()
			{
				return Cbfmc;
			}
		}
		/// <summary>
		/// 家庭成员
		/// </summary>
		class JtcyItem: NotificationObject
		{
			public string Cymc { get; set; }
			public string Zjhm { get; set; }
			public string Jtgx { get; set; }
			public string Csrq { get; set; }
			public string Sfgyr { get; set; }
			public string Bz { get; set; }
			public string Cbfbm;
		}

		class Temp
		{
			public double sumQqmj = 0;
			public double sumScmjm = 0;
			public double sumELHTMJ = 0;
			public readonly HashSet<string> setDkbm = new HashSet<string>();
		}
		private readonly ObservableCollection<CbfItem> CbfDataSource = new ObservableCollection<CbfItem>();
		private readonly ObservableCollection<DkItem> DkDataSource = new ObservableCollection<DkItem>();
		private readonly ObservableCollection<JtcyItem> JtcyDataSource = new ObservableCollection<JtcyItem>();
		private readonly MapControl _mc;
		private readonly Temp tmp = new Temp();
		private readonly List<DkItem> _dkItems = new List<DkItem>();
		private readonly List<JtcyItem> _jtcyItems = new List<JtcyItem>();

		public DataQueryResultPanel(MapControl mc)
		{
			_mc = mc;
			InitializeComponent();
			lstBox.ItemsSource = CbfDataSource;
			listView1.ItemsSource = JtcyDataSource;
			listView2.ItemsSource = DkDataSource;
			btnExportShp.IsEnabled = _dkItems.Count > 0;
			lstBox.SelectionChanged += (s, e) => OnCbfSelected();
		}

		public void Query(IGeometry geo)
		{
			_dkItems.Clear();
			var map = _mc.FocusMap;
			var db = MyGlobal.Workspace;
			using (var fc = db.OpenFeatureClass(DLXX_DK.GetTableName()))
			{

				IGeometry square = null;
				bool fPoint = geo is IPoint;
				if (fPoint)
				{
					var pt = geo as IPoint;
					square = map.Transformation.ToMap(GeometryUtil.MakeSquare(pt.X, pt.Y, 3));
				}

				DkFieldIndex dfi = null;
				var qf = new SpatialFilter()
				{
					SpatialRel = eSpatialRelEnum.eSpatialRelIntersects,
				};
				qf.SubFields = fc.OIDFieldName + ",DKMC,DKBM,TDYT,SFJBNT,DLDJ,SCMJ,QQMJ,SCMJM,ELHTMJ";
				qf.GeometryField = fc.ShapeFieldName;
				qf.WhereClause = LayerWhereUtil.UseWhere(null, "DKBM");
				var shpType = fc.ShapeType;

				var cancel = new NotCancelTracker();
				if (shpType == eGeometryType.eGeometryPolygon)
				{
					qf.Geometry = geo;
					qf.SpatialRel = fPoint ? eSpatialRelEnum.eSpatialRelWithin : eSpatialRelEnum.eSpatialRelIntersects;
				}
				else
				{
					qf.Geometry = fPoint ? square : geo;
					qf.SpatialRel = eSpatialRelEnum.eSpatialRelIntersects;
				}
				fc.SpatialQuery(qf, feature =>
				{
					if (dfi == null)
					{
						dfi = new DkFieldIndex(feature);
					}
					OnNextDkFeature(feature, dfi, tmp);
				}, cancel);

				tbDksl.Text = _dkItems.Count.ToString();
				tbQqmj.Text = tmp.sumQqmj.ToString();
				tbScmjm.Text = tmp.sumScmjm.ToString();
				tbElhtmj.Text = tmp.sumELHTMJ.ToString();
				OnQuery(tmp.setDkbm);
			}
		}
		public void Query(ShortZone zone)
		{
			_dkItems.Clear();
			var map = _mc.FocusMap;
			var db = MyGlobal.Workspace;
			using (var fc = db.OpenFeatureClass(DLXX_DK.GetTableName()))
			{

				DkFieldIndex dfi = null;

				var qf = new QueryFilter()
				{
					WhereClause = "DKBM like '" + zone.Code + "%'"
				};
				qf.SubFields =fc.OIDFieldName+",DKMC,DKBM,TDYT,SFJBNT,DLDJ,SCMJ,QQMJ,SCMJM,ELHTMJ";
				var shpType = fc.ShapeType;

				fc.Search(qf, feature =>
				{
					if (dfi == null)
					{
						dfi = new DkFieldIndex(feature as IFeature);
					}
					OnNextDkFeature(feature as IFeature, dfi, tmp);
					return true;
				});

				tbDksl.Text = _dkItems.Count.ToString();
				tbQqmj.Text = tmp.sumQqmj.ToString();
				tbScmjm.Text = tmp.sumScmjm.ToString();
				tbElhtmj.Text = tmp.sumELHTMJ.ToString();

				OnQuery(tmp.setDkbm);
				//int nCbfCount = 0;
				//SqlHelper.ConstructIn(tmp.setDkbm, sin =>
				//{
				//	var sql = "select CYXM,CYZJHM,YHZGX,CSRQ,SFGYR,CYBZSM from QSSJ_CBF_JTCY where CBFBM in (select CBFBM from QSSJ_CBDKXX where DKBM in ("
				//	+ sin + "))";
				//	db.QueryCallback(sql, r =>
				//	{
				//		var it = new JtcyItem()
				//		{
				//			Cymc = r.IsDBNull(0) ? "" : r.GetString(0),
				//			Zjhm = r.IsDBNull(1) ? "" : r.GetString(1),
				//			Jtgx = r.IsDBNull(2) ? "" : CodeUtil.Jtgx(r.GetString(2)),
				//			Sfgyr = r.IsDBNull(4) ? "" : CodeUtil.Sfgyr(r.GetString(4)),

				//			Bz = r.IsDBNull(5) ? "" : r.GetString(5),
				//		};
				//		if (!r.IsDBNull(3) && r.GetValue(3) is DateTime dt)
				//		{
				//			it.Csrq = dt.ToString("d");
				//		}
				//		JtcyDataSource.Add(it);
				//		return true;
				//	});

				//	sql = "select count(distinct CBFBM) from QSSJ_CBDKXX where DKBM in (" + sin + ")";
				//	db.QueryCallback(sql, r =>
				//	{
				//		nCbfCount += r.GetInt32(0);
				//		return false;
				//	});
				//});

				//tbCbfCount.Text = nCbfCount.ToString();
				//tbJtcySl.Text = JtcyDataSource.Count.ToString();

				//btnExportShp.IsEnabled = DkDataSource.Count > 0;
			}
		}
		public void ExportExcel()
		{
			var ofd = new SaveFileDialog()
			{
				Filter = "Excel文件(*.xls)|*.xls",
				OverwritePrompt = true,
				RestoreDirectory = true,
			};
			if (ofd.ShowDialog() != true)
				return;
			var file = ofd.FileName;
			try
			{
				if (File.Exists(file))
				{
					File.Delete(file);
				}
				using (var sht = new NPIOSheet())
				{
					var templFile = AppDomain.CurrentDomain.BaseDirectory + "Data/DataQueryTempl.xls";

					sht.Open(templFile);
					var style1 = sht.GetCellStyle(0, 0);
					var style2 = sht.GetCellStyle(1, 1);
					var style3 = sht.GetCellStyle(2, 1);
					var style4 = sht.GetCellStyle(3, 1);

					int i = 1;
					int c = 0;
					for (int j = 0; j < CbfDataSource.Count; ++j)
					{
						SetCellText(sht, ++i, 0, CbfDataSource[j].Cbfmc, style4);
					}
					int k = ++i;
					i = 3;
					foreach (var it in _jtcyItems)
					{
						c = 0;
						SetCellText(sht,i, ++c, it.Cymc,style4);
						SetCellText(sht,i, ++c, it.Zjhm, style4);
						SetCellText(sht,i, ++c, it.Jtgx, style4);
						SetCellText(sht,i, ++c, it.Csrq, style4);
						SetCellText(sht,i, ++c, it.Sfgyr, style4);
						SetCellText(sht,i, ++c, it.Bz, style4);
						++i;
					}
					SetCellText(sht, i, 1, "地块",style2);
					++i;
					var sa = new string[] { "地块名称", "地块编码", "土地用途", "基本农田", "地力等级", "实测面积" };
					for (c = 0; c < sa.Length; ++c)
					{
						SetCellText(sht, i, c + 1, sa[c],style3);
					}
					++i;
					foreach (var it in _dkItems)
					{
						c = 0;
						SetCellText(sht,i, ++c, it.DkMc,style4);
						SetCellText(sht,i, ++c, it.DkBm, style4);
						SetCellText(sht,i, ++c, it.Tdyt, style4);
						SetCellText(sht,i, ++c, it.Jbnt, style4);
						SetCellText(sht,i, ++c, it.Dldj, style4);
						SetCellText(sht,i, ++c, it.Scmj.ToString(), style4);
						++i;
					}
					if (i < k)
					{
						i = k;
					}
					SetCellText(sht,++i,0,"统计结果",style1);

					++i;
					c = 0;
					SetCellText(sht, i, ++c, "承包方", style3);
					SetCellText(sht, i, ++c, "家庭成员", style3);
					SetCellText(sht, i, ++c, "地块", style3);
					//SetCellText(sht, i, ++c, "", style3);
					SetCellText(sht, i, ++c, "二轮合同面积(亩)", style3);
					SetCellText(sht, i, ++c, "实测面积(亩)", style3);
					SetCellText(sht, i, ++c, "确权面积(亩)", style3);
					++i;
					c = 0;
					SetCellText(sht, i, ++c, tbCbfCount.Text, style4);
					SetCellText(sht, i, ++c, tbJtcySl.Text, style4);
					SetCellText(sht, i, ++c, tbDksl.Text, style4);
					//++c;
					SetCellText(sht, i,++c, tbElhtmj.Text, style4);
					SetCellText(sht, i, ++c, tbScmjm.Text, style4);
					SetCellText(sht, i, ++c, tbQqmj.Text, style4);

					SetCellText(sht, i, 0, "数量", style3);
					//SetCellText(sht, i, 4, "总计", style3);

					sht.ExportToExcel(file);
					Win32.ShellExecute(IntPtr.Zero, "open", file, "", "", ShowCommands.SW_SHOWNORMAL);
				}
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}

		private void OnQuery(HashSet<string> setDkbm)
		{
			var db = MyGlobal.Workspace;
			var setCbf = new HashSet<string>();

			SqlUtil.ConstructIn(setDkbm, sin =>
			{
				var sql = "select CYXM,CYZJHM,YHZGX,CSRQ,SFGYR,CYBZSM,CBFBM from QSSJ_CBF_JTCY where CBFBM in (select CBFBM from QSSJ_CBDKXX where DKBM in ("
				+ sin + "))";
				db.QueryCallback(sql, r =>
				{
					var it = new JtcyItem()
					{
						Cymc = r.IsDBNull(0) ? "" : r.GetString(0),
						Zjhm = r.IsDBNull(1) ? "" : r.GetString(1),
						Jtgx = r.IsDBNull(2) ? "" : CodeUtil.Jtgx(r.GetString(2)),
						Sfgyr = r.IsDBNull(4) ? "" : CodeUtil.Sfgyr(r.GetString(4)),
						Bz = r.IsDBNull(5) ? "" : r.GetString(5),
						Cbfbm=r.IsDBNull(6)?"":r.GetString(6),
					};
					if (!r.IsDBNull(3) && r.GetValue(3) is DateTime dt)
					{
						it.Csrq = dt.ToString("d");
					}
					_jtcyItems.Add(it);
					return true;
				});

				sql = "select CBFBM,CBFMC from QSSJ_CBF where CBFBM in (select CBFBM from QSSJ_CBDKXX where DKBM in ("+sin+")) and CBFBM is not null and CBFMC is not null";
				db.QueryCallback(sql, r =>
				 {
					 var cbfbm = r.GetString(0);
					 if (!setCbf.Contains(cbfbm))
					 {
						 setCbf.Add(cbfbm);
						 var it = new CbfItem()
						 {
							 Cbfbm = cbfbm,
							 Cbfmc = r.GetString(1)
						 };
						 CbfDataSource.Add(it);
					 }
					 return true;
				 });
			},null,100);

			tbCbfCount.Text = setCbf.Count.ToString();
			tbJtcySl.Text = _jtcyItems.Count.ToString();
			if (CbfDataSource.Count > 0)
			{
				lstBox.SelectedIndex = 0;
			}
			btnExportShp.IsEnabled = _dkItems.Count > 0;
		}
		private static void SetCellText(NPIOSheet sht, int row, int col, string text, ICellStyle style)
		{
			sht.SetCellText(row, col, text);
			sht.SetCellStyle(row, col, style);
		}
		private void OnNextDkFeature(IFeature ft, DkFieldIndex fi,Temp tmp)
		{
			var it = new DkItem() {
				Oid=ft.Oid,
				DkMc = SafeConvertAux.ToStr(ft.GetValue(fi.iDkmc)),
				DkBm = SafeConvertAux.ToStr(ft.GetValue(fi.iDkbm)),
				Tdyt = CodeUtil.Tdyt(ft.GetValue(fi.iTdyt)),
				Jbnt = CodeUtil.Jbnt(ft.GetValue(fi.iJbnt)),
				Dldj=CodeUtil.Dldj(ft.GetValue(fi.iDldj)),
				Scmj = SafeConvertAux.ToDouble(ft.GetValue(fi.iScmj))
			};
			if (it.DkBm != null)
			{
				tmp.setDkbm.Add(it.DkBm);
			}
			tmp.sumQqmj += SafeConvertAux.ToDouble(ft.GetValue(fi.iQqmj));
			tmp.sumScmjm += SafeConvertAux.ToDouble(ft.GetValue(fi.iScmjm));
			tmp.sumELHTMJ += SafeConvertAux.ToDouble(ft.GetValue(fi.iELHTMJ));
			_dkItems.Add(it);
		}

		private void BtnExportShp_Click(object s, RoutedEventArgs e)
		{
			var ofd = new SaveFileDialog()
			{
				Filter = "ESRI Shapefile(*.shp)|*.shp",
				OverwritePrompt = true,
				RestoreDirectory = true,
			};
			if (ofd.ShowDialog() != true)
				return;
			var file= ofd.FileName;
			if (!ShapeFileFeatureWorkspaceFactory.ParseShpFileName(file, out string cons, out string tableName)) {
				UIHelper.ShowWarning(Window.GetWindow(this), "文件名无效！");
				return;
			}
			try
			{
				if (File.Exists(file))
				{
					ShapeFileUtil.DeleteShapeFile(file);
				}
				
				using (var ws = ShapeFileFeatureWorkspaceFactory.Instance.OpenWorkspace(cons))
				using (var fc0 = MyGlobal.Workspace.OpenFeatureClass(DLXX_DK.GetTableName()))
				{
					var fields = new Fields();
					var fields0 = fc0.Fields;
					for (int i = 0; i < fields0.FieldCount; ++i)
					{
						
						var field0 = fields0.GetField(i);
						var field = field0.Clone() as Field;
						if (!string.IsNullOrEmpty(field0.AliasName))
						{
							field.FieldName = AliasName(field0);//.AliasName;
						}
						fields.AddField(field);
						//dic[field0.FieldName] = field0.AliasName;
					}

					//FieldsUtil.AddGeometryField(fields, "Shape", null, eGeometryType.eGeometryPolygon);
					//FieldsUtil.AddTextField(fields, "地块名称", null, 50);
					//FieldsUtil.AddTextField(fields, "地块编码", null, 50);
					//FieldsUtil.AddTextField(fields, "土地用途", null, 20);
					//FieldsUtil.AddTextField(fields, "基本农田", null, 20);
					//FieldsUtil.AddTextField(fields, "地力等级", null, 10);
					//FieldsUtil.AddDoubleField(fields, "实测面积", null, 15, 2);
					var srid = _mc.FocusMap.SpatialReference?.AuthorityCode;
					ws.CreateFeatureClass(tableName, fields, srid != null ? (int)srid : 0);
					using (var fc =ws.OpenFeatureClass(tableName))
					{
						var ft = fc.CreateFeature();
						var oids = new ObjectIDSet();
						var qf = new QueryFilter()
						{
							Oids = oids
						};

						foreach (var it in _dkItems)
						{
							oids.Add(it.Oid);
						}

						fc0.Search(qf, r =>
						 {
							 var ft0 = r as IFeature;
							 ft.Oid = -1;

							 for (int i = 0; i < ft0.Fields.FieldCount; ++i)
							 {
								 var field0 = ft0.Fields.GetField(i);
								 var c=ft.Fields.FindField(AliasName(field0));
								 if (c >= 0)
								 {
									 var o = ft0.GetValue(i);
									 ft.SetValue(c,o);
								 }
							 }
							 ft.Shape = ft0.Shape;
							 fc.Append(ft);
							 return true;
						 });
					}
				}
				MessageBox.Show("导出结束", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				//UIHelper.ShowMessage(Window.GetWindow(this),"导出结束","提示",
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}
		private void OnCbfSelected()
		{
			try
			{
				DkDataSource.Clear();
				JtcyDataSource.Clear();
				if (lstBox.SelectedItem is CbfItem it)
				{
					var sql = "select DKBM from QSSJ_CBDKXX where CBFBM='" + it.Cbfbm + "' and DKBM is not null";
					MyGlobal.Workspace.QueryCallback(sql, r =>
					 {
						 var dkbm = r.GetString(0);
						 if (tmp.setDkbm.Contains(dkbm))
						 {
							 var it1 = _dkItems.Find(a => { return a.DkBm == dkbm; });
							 DkDataSource.Add(it1);
						 }
						 return true;
					 });

					var lst=_jtcyItems.Where(a =>
					{
						return a.Cbfbm == it.Cbfbm;
					});
					foreach (var it1 in lst)
					{
						JtcyDataSource.Add(it1);
					}
				}
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}
		private static string AliasName(IField field)
		{
			var s = field.AliasName;
			s=s.Replace("(", "").Replace(")","").Replace("（","").Replace("）","");
			return s;
		}
	}
}
