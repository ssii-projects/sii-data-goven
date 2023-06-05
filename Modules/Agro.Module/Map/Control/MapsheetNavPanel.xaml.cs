using GeoAPI.Geometries;
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.GIS;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Agro.Module.Map
{
	/// <summary>
	/// 图幅导航面板
	/// </summary>
	public partial class MapsheetNavPanel : UserControl
	{
		class ListItem : NotificationObject {
			public readonly IGeometry Shape;
			public string FrameText { get; set; }
			public ListItem(IGeometry g, string frameText)
			{
				Shape = g;
				FrameText = frameText;
			}
			public override string ToString()
			{
				return FrameText;
			}

		}
		private readonly ObservableCollection<ListItem> DataSource = new ObservableCollection<ListItem>();
		public MapsheetNavPanel(MapControl mc)
		{
			InitializeComponent();
			Build(mc);
			listBox.ItemsSource = DataSource;
			listBox.SelectionChanged += (s, e) =>
			  {
				  if (listBox.SelectedItem is ListItem li)
				  {
					  mc.FocusMap.ZoomTo(li.Shape);
				  }
			  };

			MyGlobal.OnDatasourceChanged += () => Build(mc);
		}

		private void Build(MapControl mc)
		{
			DataSource.Clear();
			var map = mc.FocusMap;
			if (mc.FocusMap.FullExtent == null)
			{
				UIHelper.ShowWarning(Window.GetWindow(mc), "当前地图无全图范围！");
				return;
			}
			if (map.SpatialReference == null)
			{
				UIHelper.ShowWarning(Window.GetWindow(mc), "当前地图无坐标系！");
				return;
			}
			var build = new MapSheetBuilder();
			build.CreateFramer(map.FullExtent, si =>
			{
				var g = si.Shape.Project(map.SpatialReference);
				DataSource.Add(new ListItem(g, si.FrameTextNew));
			}, i => { Console.WriteLine("进度：" + i + "%"); });
		}
	}
}
