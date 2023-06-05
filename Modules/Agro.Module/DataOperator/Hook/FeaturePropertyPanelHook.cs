using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using Agro.Module.DataExchange;
using Agro.Module.DataExchange.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Agro.LibCore.UI.RowPropertyView;

namespace Agro.Module.DataOperator
{
	class FeaturePropertyPanelHook
	{
		private static readonly ImageSource pickImg= CommonImageUtil.Image16("pick.png");
		private static readonly List<EntityProperty> entityProperties = VEC_SURVEY_DK.GetAttributes();
		private readonly bool _isCreateMode;
		private readonly PicSiziTool _picSiziTool;
		private readonly IMapControl _mc;
		public FeaturePropertyPanelHook(IMapPageImpl p, bool fCreateMode=false)
		{
			_mc = p.MapControl;
			_picSiziTool = new PicSiziTool(p);
			p.SidePage.OnRightPanelChildChanged += (o, n) =>
			{
				_picSiziTool.CurrentFieldItem = null;
				if (_mc.CurrentTool == _picSiziTool)
				{
					_mc.CurrentTool = p.BtnPan.GetTool();
				}
				Console.WriteLine($"OnRightPanelChildChanged:{DateTime.Now}");
			};
			_isCreateMode = fCreateMode;
		}
		public void OnUIUpdated(FeaturePropertyPanel pnl, bool isDcdk,List<ICodeItem> lstFbfbm=null)
		{
			if (isDcdk)
			{
				var db = pnl.TgtFeatureLayer.FeatureClass.Workspace;
				var isTgtShapeFile = db.DatabaseType == eDatabaseType.ShapeFile;
				if (isTgtShapeFile)
				{
					db = MyGlobal.Workspace;
				}
				var lst = pnl.DataSource;
				lst.RemoveAll(it => it.Field.FieldType == eFieldType.eFieldTypeOID
				|| it.Field.FieldType == eFieldType.eFieldTypeDate || it.Field.FieldType == eFieldType.eFieldTypeDateTime
				|| StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.SCBZ)));
				//if (_isCreateMode)
				//{
				//	lst.RemoveAll(it => StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.DKDZ)) || StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.DKNZ)) || StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.DKXZ)) || StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.DKBZ)) || StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(VEC_SURVEY_DK.DKDZ)));
				//}
				var dkbm=pnl.PropertyView.GetItemValue(nameof(VEC_SURVEY_DK.DKBM))?.ToString();
				foreach (var it in lst)
				{
					var fieldName = it.Field.FieldName;
					if (entityProperties.Find(x => StringUtil.isEqualIgnorCase(x.FieldName, fieldName)) is EntityProperty ep)
					{
						if (it.AliasName != "二轮合同面积")
						{
							it.AliasName = ep.AliasName;
						}
						if (!string.IsNullOrEmpty(ep.CodeType))
						{
							it.ComboItems = CodeUtil.QueryCodeItems(ep.CodeType, db).Cast<object>().ToList();
						}
					}
					switch (fieldName)
					{
						case nameof(VEC_SURVEY_DK.CBFBM):
						case nameof(VEC_SURVEY_DK.DKBM): it.IsReadOnly = true; break;
						case nameof(VEC_SURVEY_DK.BGLX):
							{
								var comboLst = new List<object>
								{
									"","新增","分割","合并","图形变更","一般变更"
								};
								it.ComboItems = comboLst;
								if (!isTgtShapeFile)
								{
									comboLst.Add(BglxConst.YcCbf);
									comboLst.Add(BglxConst.Xgqtmj);
								}
								//it.IsReadOnly = true;
							}	break;
						case "DJZT":
						case nameof(VEC_SURVEY_DK.SCBZ):
							it.IsReadOnly = true;
							break;
						case nameof(VEC_SURVEY_DK.FBFDM):
							{
								if (string.IsNullOrEmpty(dkbm))
								{
									if (lstFbfbm != null)
									{
										var items = new List<object>();
										items.AddRange(lstFbfbm);
										it.ComboItems = items;
									}
								}
								else
								{
									it.ControlType = CellControlType.ImageTextBox;
									//it.IsReadOnly = true;
								}
							}
							break;
						case nameof(VEC_SURVEY_DK.DKDZ):
						case nameof(VEC_SURVEY_DK.DKNZ):
						case nameof(VEC_SURVEY_DK.DKXZ):
						case nameof(VEC_SURVEY_DK.DKBZ):
							it.ButtonTooltip = _isCreateMode?null: $"在地图上点击选择{it.AliasName}";
							it.ControlType = CellControlType.ImageTextBox;
							it.ButtonImage = pickImg;// CommonImageUtil.Image16("pick.png");
							it.IsButtonEnable = !_isCreateMode;
							break;
					}

					if (StringUtil.isEqualIgnorCase(fieldName, VEC_SURVEY_DK.GetFieldName(nameof(VEC_SURVEY_DK.ZYY))))
					{
						if (it.Value == null)
						{
							it.Value = DataOperatorUtil.Operator;
						}
					}
				}
			}
		}
		public void OnPreSave(FeaturePropertyPanel pnl,Cancelable cancelable)
		{
			var ft = pnl.TgtFeature;

			if(!DkEditUtil.CanModify(ft)){
				cancelable.Cancel = true;
				return;
			}

			var lst = pnl.DataSource;
			string sBglx = null;// "一般变更";
			object oid = null;
			foreach (var it in lst)
			{
				var field = it.Field;
				var value = it.Value;
				if (field.FieldName == "BGLX")
				{
					if (value != null && value.ToString() != "")
					{
						sBglx = value.ToString();
					}
				}
				else if (field.FieldType == eFieldType.eFieldTypeOID)
				{
					oid =value;
				}
				else if (field.FieldName == "DCBM")
				{
					if (sBglx != null && (value == null || string.IsNullOrEmpty(value.ToString())))
					{
						it.Value = oid;
					}
				}
			}

			IRowUtil.SetRowValue(ft, "xgsj", DateTime.Now);

			if (sBglx == null)
			{
				cancelable.Cancel = true;
				MessageBox.Show("请选择变更类型后再保存！", "提示",MessageBoxButton.OK,MessageBoxImage.Warning);
				return;
			}


			if (DataOperatorUtil.HasOperatorField(ft))
			{
				if (DataOperatorUtil.IsOperatorNullOrEmpty(ft))
				{
					cancelable.Cancel = true;
					MessageBox.Show("作业员不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				else
				{
					var oldFt = pnl.TgtFeatureLayer.FeatureClass.GetFeatue(ft.Oid);
					var zyy = DataOperatorUtil.GetOperator(ft);
					if (DataOperatorUtil.GetOperator(oldFt) != zyy)
					{
						DataOperatorUtil.SetOperator(ft, zyy);//make ft _dicChangedValue valid
					}
				}
			}
		}

		public void TrySave(FeaturePropertyPanel pnl, ICancelable it, bool fShowCancel = true)
		{
			if (pnl.IsDirty()&&pnl.SaveButtonVisible)
			{
				var mb = fShowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
				var res = MessageBox.Show("数据已更改，是否保存", "提示", mb, MessageBoxImage.Question);
				if (res == MessageBoxResult.Yes)
				{
					pnl.Save();
				}
				else if (res == MessageBoxResult.No)
				{
					pnl.Clear();
				}
				else if (res == MessageBoxResult.Cancel)
				{
					it.Cancel = true;
				}
			}
		}

		public void OnImageTextBoxButtonClick(FeaturePropertyPanel p,FieldItem fi)
		{
			var fieldName = fi.Field.FieldName.ToUpper();
			switch (fieldName)
			{
				case nameof(VEC_SURVEY_DK.FBFDM):
					{
						var db = p.TgtFeatureLayer.FeatureClass.Workspace;
						var isTgtShapeFile = db.DatabaseType == eDatabaseType.ShapeFile;
						if (isTgtShapeFile)
						{
							if (p.TgtFeature.Fields.FindField("DJZT") >= 0)
							{
								var djzt = SafeConvertAux.ToInt32(IRowUtil.GetRowValue(p.TgtFeature, "DJZT"));
								if (djzt != (int)EDjzt.Wdj)
								{
									MessageBox.Show("只有未登记的地块才能修改发包方！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
									return;
								}
							}
						}
						ModifyFbfBm(p, fi);
					}break;
				case nameof(VEC_SURVEY_DK.DKDZ):
				case nameof(VEC_SURVEY_DK.DKNZ):
				case nameof(VEC_SURVEY_DK.DKXZ):
				case nameof(VEC_SURVEY_DK.DKBZ):
					_picSiziTool.OnCreate(_mc);
					_mc.CurrentTool = _picSiziTool;
					_picSiziTool.TgtLayer = p.TgtFeatureLayer;
					_picSiziTool.CurrentFieldItem = fi;
					break;
			}
		}
		private void ModifyFbfBm(FeaturePropertyPanel p,FieldItem fi)
		{
			var pnl = new SelectFbfPanel(p.TgtFeatureLayer.FeatureClass.Workspace as IFeatureWorkspace);
			var dlg = new KuiDialog(Window.GetWindow(p), "选择发包方")
			{
				Width = 700,
				Content = pnl
			};
			dlg.BtnOK.Click += (s, e) =>
			{
				var err = pnl.OnApply();
				if (err != null)
				{
					UIHelper.ShowError(dlg, err);
					return;
				}
				fi.Value = pnl.SelectedFbf.FbfBM;
				

				dlg.Close();
			};
			dlg.ShowDialog();
		}
	}
	class PicSiziTool : MapPanTool
	{
		public IFeatureLayer TgtLayer;
		public FieldItem CurrentFieldItem { get; set; }
		private readonly SpatialFilter _qf = new SpatialFilter()
		{
			SpatialRel = eSpatialRelEnum.eSpatialRelWithin
		};
		private readonly IMapPageImpl _p;
		public PicSiziTool(IMapPageImpl p) : base(true, CustomCursors.instance.ActiveVertexCursor)
		{
			_p = p;
		}
		public override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			if (!Draged&& CurrentFieldItem!=null)
			{
				var pt = GetPoint(e);
				var square = Map.Transformation.ToMap(GeometryUtil.MakeSquare(pt.X, pt.Y, 3));
				var fl = TgtLayer;// Map.Editor.TargetLayer;

				var qf = _qf;
				qf.Geometry = Map.Transformation.ToMap(GeometryUtil.MakePoint(pt.X, pt.Y));
				qf.GeometryField = fl.FeatureClass.ShapeFieldName;
				qf.SubFields =$"{fl.FeatureClass.OIDFieldName},{fl.FeatureClass.ShapeFieldName},DKMC,CBFMC";
				qf.WhereClause = fl.UseWhere();

				fl.FeatureClass.SpatialQuery(qf, ft =>
				{
					var str = $"{IRowUtil.GetRowValue(ft, "CBFMC")}[{IRowUtil.GetRowValue(ft, "DKMC")}]";
					CurrentFieldItem.Value = str;
					_hook.CurrentTool = _p.BtnPan.GetTool();
					return false;
				}, NotCancelTracker.Instance);
			}
		}

	}
}
