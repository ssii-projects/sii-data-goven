using Agro.GIS;
using Agro.LibCore;
using GeoAPI.Geometries;
using System.Linq;
using System.Windows;

namespace Agro.Module.DataOperator
{
	class MergeDkToolHook: ModifyScmj
	{
		public MergeDkToolHook(MapPageSQLiteSource p)
		{
			p.btnMergeFeatureTool.OnPreStore += OnPreMerge;
		}
		private void OnPreMerge(CancelItem<EditOperationPack> it)
		{
			var eop = it.Item;
			var tgtFt = eop.Updates.FirstOrDefault();
			var sCbfbm = IRowUtil.GetRowValue(tgtFt, "CBFBM")?.ToString();
			foreach (var ft in eop.Deletes)
			{
				var str = IRowUtil.GetRowValue(ft, "CBFBM")?.ToString();
				if (sCbfbm != str)
				{
					it.Cancel = true;
					MessageBox.Show("相同承包方的地块才能合并", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
				if (!DkEditUtil.CanModify(ft, false))
				{
					it.Cancel = true;
					MessageBox.Show("有地块登记状态为“登记中”，不允许合并！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}
			if (!(tgtFt.Shape is IPolygon))
			{
				it.Cancel = true;
				MessageBox.Show("合并后地块不是简单面对象！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			base.Update(tgtFt, "合并");
		}
	}
}
