using Agro.LibCore;
using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Agro.Module.PreproccessTool
{
	class DummyTask : Task
	{
		public DummyTask(string name)
		{
			Name = name;
		}
		protected override void DoGo(ICancelTracker cancel)
		{
		}
	}
	///// <summary>
	///// 去除代号
	///// </summary>
	//class BuildPyramidTask : Task
	//{
	//	/// <summary>
	//	/// 去除代号
	//	/// </summary>
	//	public BuildPyramidTask()
	//	{
	//		base.Name = "影像服务数据生成";
	//	}

	//	public static void ShowDialog(Window owner)
	//	{
	//		Try.Catch(() =>
	//		{
	//			var pnl = new BuildPyramidPanel();
	//			pnl.ShowDialog(owner);
	//		});
	//	}

	//	protected override void DoGo(ICancelTracker cancel)
	//	{
	//	}
	//}
}