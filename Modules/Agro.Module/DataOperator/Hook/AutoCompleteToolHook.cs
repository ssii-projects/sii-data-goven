using Agro.GIS;
using System.Windows;

namespace Agro.Module.DataOperator
{
	class AutoCompleteToolHook : CreateFeatureBase
	{
		public AutoCompleteToolHook(IMapPageImpl p)
		{
			var map = p.MapControl.FocusMap;
			p.BtnAutoComplete.OnPreStore += it =>
			  {
				  var pack = it.Item;
				  if (pack.Adds.Count > 1)
				  {
					  it.Cancel = true;
					  MessageBox.Show("不允许存入多部分！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
				  }
				  else if (pack.Adds.Count == 1)
				  {
					  var ft = pack.Adds[0];
					  Update(ft, "新增");
					  it.Cancel = ShowCreateDialog(p,map.Editor.TargetLayer, ft);
				  }
			  };
			p.BtnAutoComplete.OnAfterStore += eop =>
			{
				if (eop.Adds?.Count == 1)
				{
					var fl = map.Editor.TargetLayer;
					var fc = fl.FeatureClass;

					var ft = eop.Adds[0];

					IRowUtil.SetRowValue(ft, "DCBM", ft.Oid);
					fc.Update(ft);

					RefreshBgbjLayer(map);

					if (p.BtnIdentify.GetTool() is IdentifyTool tool)
					{
						if (tool.Map == null)
						{
							tool.OnCreate(p.MapControl);
						}
						tool.ShowProperty(fl, ft);
					}
				}
			};
		}
	}
}
