using Agro.GIS;
using System.Collections.Generic;

namespace Agro.Module.DataOperator
{
	class CutFeatureToolHook : ModifyScmj
	{
		public CutFeatureToolHook(IMapPageImpl p)
		{
			p.BtnCutFeatureTool.OnFeatureSelected += it =>
			  {
				  if (!DkEditUtil.CanModify(it.Item.Feature))
				  {
					  it.Cancel = true;
				  }
			  };
			p.BtnCutFeatureTool.OnPreStore += it =>
			{
				Update(it.Item.Updates);
				Update(it.Item.Adds);
			};

			//p.BtnCutFeatureTool.OnFeaturePreUpdated += ft => Update(ft, "分割");
			//p.BtnCutFeatureTool.OnFeaturePreAdded += ft => Update(ft, "分割");
			p.BtnCutFeatureTool.OnAfterStore += it =>
			{
				UpdateDcbm(p,it.Adds);
				UpdateDcbm(p,it.Updates);
				//foreach (var ft in it.Adds)
				//{
				//	var fc = p.MapControl.FocusMap.Editor.TargetLayer.FeatureClass;
				//	IRowUtil.SetRowValue(ft, "DCBM", ft.Oid);
				//	fc.Update(ft);
				//}
			};
		}
		private void UpdateDcbm(IMapPageImpl p,List<IFeature> lst)
		{
			var fc = p.MapControl.FocusMap.Editor.TargetLayer.FeatureClass;
			foreach (var ft in lst)
			{
				IRowUtil.SetRowValue(ft, "DCBM", ft.Oid);
				fc.Update(ft,false);
			}
		}
		private void Update(List<IFeature> features) {
			foreach (var ft in features)
			{
				Update(ft, "分割");
			}
		}
	}
}
