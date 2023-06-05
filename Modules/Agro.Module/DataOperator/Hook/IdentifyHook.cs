using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Model;
using Agro.Module.DataExchange.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Agro.Module.DataOperator
{
	class IdentifyHook
	{
		private readonly IMapPageImpl _p;
		private readonly FeaturePropertyPanelHook hook;

		public IdentifyHook(IMapPageImpl p)
		{
			_p = p;
			hook = new FeaturePropertyPanelHook(p);

			p.BtnIdentify.OnPanelCreated += pnl =>
			{
				p.SidePage.OnChildPreChanged += (tgt, nSide) =>
				{
					if (nSide == AnchorSide.Right)
					{
						var rp = p.SidePage.GetPanel(nSide);
						if (rp == pnl)
						{
							hook.TrySave(pnl, tgt);
						}
					}
				};

				pnl.PropertyView.OnValueChanged = it =>
				  {
					  var fl = pnl.TgtFeatureLayer;
					  var tgtLayer = false;
					  if (fl?.Tag is ELayerTagType ltt)
					  {
						  tgtLayer = ltt == ELayerTagType.DC_DK;
						  //hook.OnUIUpdated(pnl, ltt == ELayerTagType.DC_DK || ltt == ELayerTagType.EXPORT_DK, _p.GetListFbfdm(fl));
					  }
					  if (tgtLayer&&StringUtil.isEqualIgnorCase(it.Field.FieldName,nameof(VEC_SURVEY_DK.FBFDM)))
					  {
						  var dkbm = pnl.PropertyView.GetItemValue(nameof(VEC_SURVEY_DK.DKBM))?.ToString();
						  if (!string.IsNullOrEmpty(dkbm))
						  {
							  pnl.PropertyView.SetItemValue(nameof(VEC_SURVEY_DK.BGLX), BglxConst.YcCbf);
						  }
					  }
				  };

				pnl.OnPreUpdateUI += it => hook.TrySave(pnl, it, false);
				pnl.OnCustomUI += _ =>
				 {
					 var fl = pnl.TgtFeatureLayer;
					 var tgtLayer = false;
					 if (fl?.Tag is ELayerTagType ltt)
					 {
						 tgtLayer = ltt == ELayerTagType.DC_DK;
						 var fOnlyInDcdk = false;
						 if (tgtLayer)
						 {
							 var dkbm = pnl.PropertyView.GetItemValue(nameof(VEC_SURVEY_DK.DKBM))?.ToString();
							 fOnlyInDcdk = string.IsNullOrEmpty(dkbm);
						 }
						 hook.OnUIUpdated(pnl, ltt == ELayerTagType.DC_DK || ltt == ELayerTagType.EXPORT_DK, _p.GetListFbfdm(fl, fOnlyInDcdk));
					 }
					 pnl.SaveButtonVisible = tgtLayer;
				 };
				pnl.OnPreSave += it =>
				{
					var fl = pnl.TgtFeatureLayer;
					var tgtLayer = fl.Tag is ELayerTagType ltt && ltt == ELayerTagType.DC_DK;
					if (tgtLayer)
					{
						hook.OnPreSave(pnl, it);
					}
				};
				pnl.OnImageTextBoxButtonClick += (fp, fi) => hook.OnImageTextBoxButtonClick(fp, fi);
			};
		}
	}
}
