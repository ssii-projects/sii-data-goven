using Agro.GIS;
using Agro.GIS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Module.Map
{
	interface IDataQueryToolButton
	{
		void Init(DataQueryControlPanel p);
	}
	class PointQueryToolButton : ToolButton, IDataQueryToolButton
	{
		private PointQueryTool _tool = null;// new LineQueryTool();
		private DataQueryControlPanel _p;
		public PointQueryToolButton()
		{
			Header = "点查询";
			ToolTip = "在地图上绘制点进行查询";
			//Image = MyImageSourceUtil.Image32("MeasureArea.png");
			//base.OnMapPropertyChanged += this.MapPropertyChanged;
			//UpdateCommandUI();
		}
		public void Init(DataQueryControlPanel p)
		{
			_p = p;
		}
		//protected void MapPropertyChanged(IMapControl oldMap)
		//{
		//	if (oldMap != null)
		//	{
		//		oldMap.OnUpdateCommandUI -= UpdateCommandUI;
		//	}
		//	if (MapHost != null)
		//	{
		//		MapHost.OnUpdateCommandUI += UpdateCommandUI;
		//	}
		//}
		protected override void UpdateCommandUI()
		{
			IsEnabled = true;
			bool fCheck = MapHost != null && MapHost.CurrentTool is PointQueryTool;
			IsChecked = fCheck;
		}
		protected override ITool DoCreateTool()
		{
			return new PointQueryTool(_p);
		}
	}
	class LineQueryToolButton : ToolButton, IDataQueryToolButton
	{
		private LineQueryTool _tool = null;// new LineQueryTool();
		private DataQueryControlPanel _p;
		public LineQueryToolButton()
		{
			Header = "线查询";
			ToolTip = "在地图上绘制线条进行查询";
			//Image = MyImageSourceUtil.Image32("MeasureArea.png");
			//base.OnMapPropertyChanged += this.MapPropertyChanged;
			//UpdateCommandUI();
		}
		public void Init(DataQueryControlPanel p)
		{
			_p = p;
		}
		//protected void MapPropertyChanged(IMapControl oldMap)
		//{
		//	if (oldMap != null)
		//	{
		//		oldMap.OnUpdateCommandUI -= UpdateCommandUI;
		//	}
		//	if (MapHost != null)
		//	{
		//		MapHost.OnUpdateCommandUI += UpdateCommandUI;
		//	}
		//}
		protected override void UpdateCommandUI()
		{
			IsEnabled = true;
			bool fCheck = MapHost != null && MapHost.CurrentTool is LineQueryTool;
			IsChecked = fCheck;
		}
		protected override ITool DoCreateTool()
		{
			return new LineQueryTool(_p);
		}
	}
	class AreaQueryToolButton : ToolButton, IDataQueryToolButton
	{
		//private AreaQueryTool _tool = null;// new AreaQueryTool();
		private DataQueryControlPanel _p;
		public AreaQueryToolButton()
		{
			Header = "多边形查询";
			ToolTip = "在地图上绘制多边形进行查询";
			//Image = MyImageSourceUtil.Image32("MeasureArea.png");
			//base.OnMapPropertyChanged += this.MapPropertyChanged;
			//UpdateCommandUI();
		}
		public void Init(DataQueryControlPanel p)
		{
			_p = p;
		}
		//protected void MapPropertyChanged(IMapControl oldMap)
		//{
		//	if (oldMap != null)
		//	{
		//		oldMap.OnUpdateCommandUI -= UpdateCommandUI;
		//	}
		//	if (MapHost != null)
		//	{
		//		MapHost.OnUpdateCommandUI += UpdateCommandUI;
		//	}
		//}
		protected override void UpdateCommandUI()
		{
			IsEnabled = true;
			bool fCheck = MapHost != null && MapHost.CurrentTool is AreaQueryTool;
			IsChecked = fCheck;
		}
		protected override ITool DoCreateTool()
		{
			return new AreaQueryTool(_p);
		}
	}

	class BoxQueryToolButton : ToolButton, IDataQueryToolButton
	{
		private DataQueryControlPanel _p;
		public BoxQueryToolButton()
		{			
			Header = "拉框查询";
			ToolTip = "在地图上拉框进行查询";
			//Image = MyImageSourceUtil.Image32("MeasureArea.png");
			//base.OnMapPropertyChanged += this.MapPropertyChanged;
			//UpdateCommandUI();
		}
		public void Init(DataQueryControlPanel p)
		{
			_p = p;
		}
		protected void MapPropertyChanged(IMapControl oldMap)
		{
			if (oldMap != null)
			{
				oldMap.OnUpdateCommandUI -= UpdateCommandUI;
			}
			if (MapHost != null)
			{
				MapHost.OnUpdateCommandUI += UpdateCommandUI;
			}
		}
		protected override void UpdateCommandUI()
		{
			IsEnabled = true;
			bool fCheck = MapHost != null && MapHost.CurrentTool is BoxQueryTool;
			IsChecked = fCheck;
		}
		protected override ITool DoCreateTool()
		{
			return new BoxQueryTool(_p);
		}
	}
}
