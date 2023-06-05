using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Agro.Module.DataOperator
{
	class ModifyFeatureToolHook : ModifyScmj
	{
		public ModifyFeatureToolHook(IMapPageImpl p)
		{
			p.ModifyFeatureTool.OnFeatureSelected += OnFeatureSelected;
			p.ModifyFeatureTool.OnPreStore +=OnPreStore;
			p.ModifyFeatureTool.OnAfterStore += it => RefreshBgbjLayer(p.MapControl.FocusMap);

			p.BtnReshape.OnFeatureSelected += OnFeatureSelected;
			p.BtnReshape.OnPreStore += OnPreStore;
			p.BtnReshape.OnAfterStore += it => RefreshBgbjLayer(p.MapControl.FocusMap);
		}
		private void OnFeatureSelected(CancelItem<TargetFeatureItem> it)
		{
			if (!DkEditUtil.CanModify(it.Item.Feature))
			{
				it.Cancel = true;
				return;
			}
		}
		private void OnPreStore(CancelItem<IFeature> it)
		{
			var ft = it.Item;

			if (!DkEditUtil.CanModify(ft))
			{
				it.Cancel = true;
				return;
			}

			var sBGLX = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft, "BGLX"));
			if (string.IsNullOrEmpty(sBGLX))
			{
				sBGLX = "图形变更";
			}

			Update(ft, sBGLX);
		}

	}
}
