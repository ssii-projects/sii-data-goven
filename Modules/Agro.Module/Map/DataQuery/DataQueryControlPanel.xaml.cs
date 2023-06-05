using GeoAPI.Geometries;
using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.UI;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Agro.Library.Common;
using Agro.Library.Common.Util;

namespace Agro.Module.Map
{
	/// <summary>
	/// DataQueryControlPanel.xaml 的交互逻辑
	/// </summary>
	public partial class DataQueryControlPanel : UserControl
	{
		private readonly MapControl _mc;
		public DataQueryControlPanel(MapControl mc)
		{
			_mc = mc;
			InitializeComponent();

			foreach (var tb in toolbar.Children)
			{
				if (tb is IDataQueryToolButton dt)
				{
					dt.Init(this);
				}
			}

			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				DependencyObjectUtilEx.FindCommandButton(toolbar, cb =>BindMapBuddy(cb));
				mc.OnUpdateCommandUI +=() =>
				  {
					  var ct = mc.CurrentTool;
					  tbBufferSize.IsEnabled = ct is PointQueryTool || ct is LineQueryTool;
				  };
			}
			tbQuyuCx.Click += (s, e) =>
			  {
				  var p = new QuyuSelectPanel();
				  var dlg = new KuiDialog(Window.GetWindow(this), "选择行政区域")
				  {
					  Content =p
				  };
				  dlg.SetIcon(CommonImageUtil.Image16("query16.png"));
				  dlg.BtnOK.Click += (s1, e1) =>
					{
						if (p.zoneTree.CurrentZone == null)
						{
							UIHelper.ShowWarning(dlg, "未选择行政区域！");
							return;
						}
						dlg.Close();
						PopResult(p.zoneTree.CurrentZone);
					};
				  dlg.ShowDialog();
				  tbQuyuCx.IsChecked = false;

			  };
		}
		public double BufferSize
		{
			get
			{
				return tbBufferSize.Value;
			}
		}

		public void PopResult(object o)
		{
			var p = new DataQueryResultPanel(_mc);
			if (o is IGeometry g)
			{
				p.Query(g);
			}
			else
			{
				p.Query(o as ShortZone);
			}
			var dlg = new KuiDialog(Window.GetWindow(_mc), "数据查询")
			{
				Content = p,
				Width=970,
				Height=520, 
			};
			dlg.BtnOK.Header = "导出";
			dlg.BtnCancel.Header = "关闭";

			dlg.SetIcon(CommonImageUtil.Image16("query16.png"));
			dlg.BtnOK.Click+=(s,e)=>
			{
				p.ExportExcel();
			};
			dlg.ShowDialog();
		}

		private void BindMapBuddy(IMapBuddy buddyControl)
		{
			buddyControl.MapHost = _mc;
			if (buddyControl is IDisposable)
			{
				_mc.FocusMap.OnDispose += () => (buddyControl as IDisposable).Dispose();
			}
		}
	}
}
