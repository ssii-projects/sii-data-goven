using Agro.GIS.Toolbox.PyramidBuilder;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using Agro.Module.PreproccessTool;
using GeoAPI.Geometries;
using System.Windows;
using System.Windows.Controls;

namespace Agro.Module
{
	/// <summary>
	/// 预处理工具（模块入口）
	/// </summary>
	public class PreproccessToolPage : Decorator
	{
		public PreproccessToolPage()
		{
			var taskPage = new TaskPage()
			{
				Title = "预处理工具",
				ShowRootNode = false
			};
			this.Child = taskPage;

			var icon = CommonImageUtil.Image32("bsSame.png");
			taskPage.AddTask(new CoordinateChangeTask() { Icon = icon });
			taskPage.AddTask(new CoordinateConvertTask() { Icon = icon });
			taskPage.AddTask(new CoordinateDefineTask() { Icon = icon });
			taskPage.AddTask(new AppendMarkTask() { Icon = icon });
			taskPage.AddTask(new RemoveMarkTask() { Icon = icon });
			taskPage.AddTask(new DataTranslateTask() { Icon = icon });
			taskPage.AddTask(new RasterMosaicTask() { Icon = icon });
			taskPage.AddTask(new DataMosaicTask() { Icon = icon });
			taskPage.AddTask(new DummyTask("影像服务数据生成") { Icon = icon });

			if(MyGlobal.Workspace is SqlServer)
			{
                taskPage.AddTask(new DummyTask("空间索引管理") { Icon = icon });
            }
			
			taskPage.AddTask(new DummyTask("界址点界址线生成") { Icon = icon });

			taskPage.OnPreAddTaskToPool = it =>
			{
				if (it.Item is DummyTask task)
				{
					switch (task.Name)
					{
						case "影像服务数据生成":
							Try.Catch(() =>
							{
								it.Cancel = true;
								var pnl = new BuildPyramidPanel();
								var ws = MyGlobal.Workspace;
								var srid = ws.GetSRID(DLXX_XZDY.GetTableName());
								Envelope fullEnv = null;
								try
								{
									fullEnv = ws.GetFullExtent(DLXX_XZDY.GetTableName(), "shape");
								}
								catch { }
								var sr = SpatialReferenceFactory.CreateFromEpsgCode(srid);
								pnl.ShowDialog(Window.GetWindow(taskPage), sr, fullEnv, dlg => dlg.WindowState = WindowState.Maximized);
							});
							break;
						//BuildPyramidTask.ShowDialog(Window.GetWindow(taskPage));
						case "空间索引管理":
							{
								it.Cancel = true;
								var pnl = new SpatialIndexManagePanel();
								pnl.ShowDialog(Window.GetWindow(taskPage));
							}
							break;
						case "界址点界址线生成":
							{
								it.Cancel = true;
								var dlg = new JzdxBuild.MainWindow
								{
									Owner = Window.GetWindow(this)
								};
								_ = dlg.ShowDialog();
							}
							break;
					}
				}

			};

		}
	}

}
