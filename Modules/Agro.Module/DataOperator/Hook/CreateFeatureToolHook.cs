using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Model;
using System;
using System.Windows;

namespace Agro.Module.DataOperator
{
	class ModifyScmj
	{
		protected void Update(IFeature ft, string sBGLX)
		{
			if (ft.Shape != null)
			{
				var area = ft.Shape.Area;
				var scale = MyGlobal.AppOption.DkmjScale;
				IRowUtil.SetRowValue(ft, "SCMJ", Math.Round(area, scale));
				IRowUtil.SetRowValue(ft, "SCMJM", Math.Round(area * 0.0015, scale));
				IRowUtil.SetRowValue(ft, "BGLX", sBGLX);
				SetFeatureValue(ft, "XGSJ", DateTime.Now);
				if (ft.Oid >= 0)
				{
					var sDcbm = IRowUtil.GetRowValue(ft, "DCBM");
					if (sDcbm == null)
					{
						IRowUtil.SetRowValue(ft, "DCBM", ft.Oid);
					}
				}
			}

			DataOperatorUtil.SetDefaultOperatorIfNullOrEmpty(ft);
		}
		protected void RefreshBgbjLayer(GIS.Map map)
		{
			var lyr=MapUtil.FindFeatureLayer(map, fl => fl.Tag is ELayerTagType et && et == ELayerTagType.BGBJ);
			if (lyr != null)
			{
				map.RefreshLayer(lyr);
			}
		}
		private static void SetFeatureValue(IFeature ft, string fieldName, object val)
		{
			var iField = ft.Fields.FindField(fieldName);
			if (iField >= 0)
			{
				ft.SetValue(iField, val);
			}
		}
	}
	class CreateFeatureBase : ModifyScmj
	{
		protected bool ShowCreateDialog(IMapPageImpl p, IFeatureLayer fl, IFeature ft)
		{
			bool fCancel = true;
			IRowUtil.SetRowValue(ft, "DJZT",EDjzt.Wdj);

			DataOperatorUtil.SetDefaultOperator(ft);


			var hook = new FeaturePropertyPanelHook(p, true);
			var pnl = new FeaturePropertyPanel(p.MapControl)
			{
				HeaderVisible = false,
				SaveButtonVisible = false
			};
			pnl.OnCustomUI += _ => hook.OnUIUpdated(pnl, true, p.GetListFbfdm(pnl.TgtFeatureLayer,true));
			pnl.PropertyView.OnGetItemDefaultValue = it =>
			  {
				  switch (it.Field.FieldName)
				  {
					  case nameof(VEC_SURVEY_DK.DKLB):return "承包地块";
					  case nameof(VEC_SURVEY_DK.TDLYLX):return "水田";
					  case nameof(VEC_SURVEY_DK.TDYT):return "种植业";
					  case nameof(VEC_SURVEY_DK.SFJBNT):return "基本农田";
					  case nameof(VEC_SURVEY_DK.SYQXZ):return "村民小组";
					  case nameof(VEC_SURVEY_DK.FBFDM):
						  {
							  var lst=it.GetCodeList();
							  if (lst?.Count == 1)
							  {
								  return lst[0].GetName();
							  }
						  }break;
				  }
				  return null;
			  };
			var dlg = new KuiDialog(Window.GetWindow(p.MapControl.Container), "新建地块")
			{
				Content = pnl,
			};
			pnl.UpdateUI(fl, ft);
			pnl.DataSource.RemoveAll(it => it.IsReadOnly
			|| it.Field.FieldType == eFieldType.eFieldTypeDate || it.Field.FieldType == eFieldType.eFieldTypeDateTime
			|| StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.DKBM))
			|| StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.CBFBM))
			);
			dlg.BtnOK.Click += (s, e) =>
			{
				string err = null;
				try
				{
					pnl.Flush();					
					
					var lst = VEC_SURVEY_DK.GetAttributes(false);
					foreach (var it in lst)
					{
						if (it.Nullable == false)
						{
							var o = IRowUtil.GetRowValue(ft, it.FieldName);
							if (o == null || o.ToString().Trim().IsNullOrEmpty())
							{
								throw new Exception($"{it.AliasName}不能为空！");
							}
						}
					}

					if (DataOperatorUtil.HasOperatorField(ft))
					{
						var o = DataOperatorUtil.GetOperator(ft);
						if (string.IsNullOrEmpty(o))
						{
							throw new Exception("作业员不能为空！");
						}
					}
					IRowUtil.SetRowValue(ft, "XGSJ", DateTime.Now);

				}
				catch (Exception ex)
				{
					err = ex.Message;
				}
				if (err != null)
				{
					MessageBox.Show(err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					fCancel = false;
					dlg.Close();
				}
			};
			dlg.ShowDialog();
			return fCancel;
		}
	}
	class CreateFeatureToolHook : CreateFeatureBase
	{
		public CreateFeatureToolHook(IMapPageImpl p)
		{
			p.BtnCreateFeatureTool.OnFeatureCreated += it =>
			{
				var ft = it.Item;

				Update(ft, "新增");
				if (SafeConvertAux.ToDouble(IRowUtil.GetRowValue(ft, "ELHTMJ")) == 0)
				{
					IRowUtil.SetRowValue(ft, "ELHTMJ", IRowUtil.GetRowValue(ft, "SCMJM"));
				}
				it.Cancel = ShowCreateDialog(p, p.MapControl.FocusMap.Editor.TargetLayer, ft);
			};
			p.BtnCreateFeatureTool.OnFeatureSaved += ft =>
			{
				var map = p.MapControl.FocusMap;
				var fl = map.Editor.TargetLayer;
				var fc =fl.FeatureClass;
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

			};
		}


	}
}
