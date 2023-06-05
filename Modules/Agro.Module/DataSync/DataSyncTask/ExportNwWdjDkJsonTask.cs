using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Module.DataExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Module.DataSync
{
	/// <summary>
	/// 导出内网未登记地块数据
	/// </summary>
	class ExportNwWdjDkJsonTask : Task
	{
		public ExportNwWdjDkJsonTask()
		{
			Name = "导出未登记地块数据更新包";
			Description = "导出内网未登记地块数据更新包";
			PropertyPage = new ExportJsonPanel();
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) =>
			{
				if (0 == t.ErrorCount())
				{
					ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
				}
			};
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			
		}
	}
}
