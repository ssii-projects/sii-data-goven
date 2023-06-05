using Agro.GIS;
using Agro.LibCore;
using System;
using System.Collections.Generic;
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
yxm created at 2019/3/6 16:54:26
*/
namespace Agro.Module.DataOperator
{
	/// <summary>
	/// AppendShapePanel.xaml 的交互逻辑
	/// </summary>
	public partial class AppendShapePanel : UserControl
	{
		public readonly FeatureLayer _tgtLayer;
		public bool IsRunning { get; private set; } = false;
		private readonly MyTask _task = new MyTask();

		public Action<string> OnComplete;
		public Action<string> OnError;
		public AppendShapePanel(FeatureLayer tgtLayer)
		{
			_tgtLayer = tgtLayer;
			InitializeComponent();
			tbPath.Filter= "ShapeFile (*.shp)|*.shp";
		}
		public string ShapeFile { get { return tbPath.Text; } }

		public void Run(bool fAppend)
		{
			var err = DoRun(fAppend);
			if (err!=null)
				OnError(err);
		}
		private string DoRun(bool fAppend)
		{
			string err = null;
			if (string.IsNullOrEmpty(tbPath.Text))
			{
				return "未选择导入文件";
			}

			var fc = ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(tbPath.Text);
			
			if (fc.ShapeType != LibCore.eGeometryType.eGeometryPolygon)
			{
				fc.Dispose();
				return "输入Shape文件必须为面对象类型";
			}
			try
			{
				dpProgress.Visibility = Visibility.Visible;
				if (fAppend)
				{
					AppendAsync(fc);
				}
				else
				{
					int n=fc.Fields.FindField("DKBM");
					if (n < 0)
					{
						return "输入Shape文件不包含地块编码(DKBM)字段";
					}
					ModifyAsync(fc);
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return err;
		}
		private void AppendAsync(IFeatureClass fc)
		{
			var tgtFc = _tgtLayer.FeatureClass;
			var qf = new QueryFilter();
			var dicPro = new Dictionary<string, object>();
			var setDkbm = new HashSet<string>();
			tgtFc.Search(qf, r =>
			{
					var sDkbm = SafeConvertAux.ToStr(IRowUtil.GetRowValue(r, "DKBM"));
				setDkbm.Add(sDkbm);
				if (dicPro.Count == 0)
				{
					var fields = r.Fields;
					var sa = new string[] { "DKBM", "DKLB", "TDLYLX", "DLDJ", "TDYT", "SFJBNT", "FBFDM", "SYQXZ" };
					for (int i = 0; i < fields.FieldCount; ++i)
					{
						var field = fields.GetField(i);
						if (sa.Contains(field.FieldName))
						{
							dicPro[field.FieldName] = IRowUtil.GetRowValue(r, field.FieldName);
						}
					}
				}
				return true;
			});
			qf.SubFields = null;
			var cnt = fc.Count(null);
			int nImported = 0;
			_task.Go(token =>
			{
				IsRunning = true;
				try
				{
					int i = 0;
					double nOldProgress = 0;
					int iDkbmField = -2;
					fc.Search(qf, r =>
					{
						ProgressUtil.ReportProgress(n => InvokeUtil.Invoke(this, () => progressBar.Value = n), cnt, ++i, ref nOldProgress);
						if (iDkbmField == -2)
						{
							iDkbmField = r.Fields.FindField("DKBM");
						}
						if (iDkbmField >= 0)
						{
							var sDkbm = SafeConvertAux.ToStr(r.GetValue(iDkbmField));
							if (setDkbm.Contains(sDkbm))
							{
								return true;
							}
						}
						var tgtFt = tgtFc.CreateFeature();
						foreach (var kv in dicPro)
						{
							IRowUtil.SetRowValue(tgtFt, kv.Key, kv.Value);
						}
						IRowUtil.CopyValues(r, tgtFt);
						IRowUtil.SetRowValue(tgtFt, "BGLX", "新增");
						var nArea = Math.Round(tgtFt.Shape.Area, 2);
						IRowUtil.SetRowValue(tgtFt, "SCMJ", nArea);
						IRowUtil.SetRowValue(tgtFt, "SCMJM", Math.Round(nArea * 0.0015, 2));
						tgtFc.Append(tgtFt);
						++nImported;
						return true;
					});

					IsRunning = false;
					fc.Dispose();

					InvokeUtil.Invoke(this, () => OnComplete($"成功导入{nImported}条，已忽略{cnt-nImported}条"));
				}
				catch (Exception ex)
				{
					IsRunning = false;
					InvokeUtil.Invoke(this, ()=>OnError(ex.Message));// () => UIHelper.ShowExceptionMessage(Window.GetWindow(this), ex));
				}
			});
		}

		private void ModifyAsync(IFeatureClass fc)
		{
			var tgtFc = _tgtLayer.FeatureClass;
			var qf = new QueryFilter()
			{
				SubFields = "DKBM," + tgtFc.OIDFieldName
			};
			var dicDkbm2Oid = new Dictionary<string, int>();
			tgtFc.Search(qf, r =>
			{
				var sDkbm = SafeConvertAux.ToStr(IRowUtil.GetRowValue(r, "DKBM"));
				dicDkbm2Oid[sDkbm] = r.Oid;
				return true;
			});
			qf.SubFields = "DKBM,"+fc.ShapeFieldName;
			var cnt = fc.Count(null);
			_task.Go(token =>
			{
				IsRunning = true;
				try
				{
					int i = 0;
					double nOldProgress = 0;
					int iDkbmField = -2;
					fc.Search(qf, r =>
					{
						ProgressUtil.ReportProgress(n => InvokeUtil.Invoke(this, () => progressBar.Value = n), cnt, ++i, ref nOldProgress);
						if (iDkbmField == -2)
						{
							iDkbmField = r.Fields.FindField("DKBM");
						}
						var sDkbm = SafeConvertAux.ToStr(r.GetValue(iDkbmField));
						if (dicDkbm2Oid.TryGetValue(sDkbm, out int oid))
						{
							var g = (r as IFeature).Shape;
							var tgtFt = tgtFc.GetFeatue(oid);
							tgtFt.Shape = g;
							IRowUtil.SetRowValue(tgtFt, "BGLX", "图形变更");
							var nArea = Math.Round(tgtFt.Shape.Area, 2);
							IRowUtil.SetRowValue(tgtFt, "SCMJ", nArea);
							IRowUtil.SetRowValue(tgtFt, "SCMJM", Math.Round(nArea * 0.0015, 2));
							tgtFc.Update(tgtFt);
						}
						return true;
					});
				}
				catch (Exception ex)
				{
					InvokeUtil.Invoke(this, () => UIHelper.ShowExceptionMessage(Window.GetWindow(this), ex));
				}
			}, t =>
			{
				IsRunning = false;
				fc.Dispose();
			});
		}
	}
}
