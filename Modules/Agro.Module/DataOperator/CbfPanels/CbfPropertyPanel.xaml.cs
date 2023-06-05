using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Agro.Library.Common.Repository;
using System;
using Agro.LibCore.Util;

namespace Agro.Module.DataOperator
{
	/// <summary>
	/// CbfPropertyPanel.xaml 的交互逻辑
	/// </summary>
	public partial class CbfPropertyPanel : UserControl
	{
		private KuiDialog dlg;
		private readonly DcCbfRepos repos;
		private CbfJtcyListPanel cbfJtcy { get { return tiJtcy.Content as CbfJtcyListPanel; } }
		private CbfListPanel.CbfListItem _pit;
		private readonly List<ICodeItem> _lsfFbfbm;
		private bool IsCreateMode { get { return _pit == null; } }
		public Action<DC_QSSJ_CBF> OnCbfCreated;
		public CbfPropertyPanel(DcCbfRepos cbfRepos,DcCbfJtcyRepos jtcyRepos, List<ICodeItem> lstFbfbm)
		{
			_lsfFbfbm = lstFbfbm;
			repos = cbfRepos;// new DcCbfRepos(db);
			InitializeComponent();
			cbfProperty.MaxLabelWidth = 240;
			cbfProperty.OnCustomUI +=ds=>
			  {
				  var db = repos.Db;
				  for (int i = ds.Count; --i >= 0;)
				  {
					  var it = ds[i];
					  var fieldName = it.Field.FieldName;
					  if ("ID,ZT,DJZT".Contains(fieldName) || it.Field.FieldType == eFieldType.eFieldTypeDateTime)
					  {
						  ds.RemoveAt(i);
						  continue;
					  }

					  if ("CBFBM,CBFMC,CBFZJLX,CBFZJHM,CBFCYSL".Contains(fieldName)
					  ||!IsCreateMode&& fieldName == nameof(DC_QSSJ_CBF.FBFBM)&&_pit?.IsNewCbf()!=true)
					  {
						  it.IsReadOnly = true;
					  }
					  if (!string.IsNullOrEmpty(it.Field.CodeType))
					  {
						  it.ComboItems = CodeUtil.QueryCodeItems(it.Field.CodeType, db).Cast<object>().ToList();
					  }
					  else if (fieldName == nameof(DC_QSSJ_CBF.FBFBM))
					  {
						  if (lstFbfbm != null)
						  {
							  var items = new List<object>();
							  foreach (var fbfbm in lstFbfbm)
							  {
								  items.Add(fbfbm);
							  }
							  it.ComboItems = items;
						  }
					  }
				  }
			  };
			cbfProperty.OnValueChanged += it =>
			{
				if (StringUtil.isEqualIgnorCase(it.Field.FieldName, nameof(DC_QSSJ_CBF.FBFBM))){
					if (it.GetCodeItem() is CodeItem ci)
					{
						cbfProperty.SetItemValue(nameof(DC_QSSJ_CBF.CBFDZ), ci.Tag);
					}
				}
				UpateSaveButtonUI();
			};
			tiJtcy.Content = new CbfJtcyListPanel(jtcyRepos,()=> UpateSaveButtonUI());

			tabControl.SelectionChanged += (s, e) =>
			  {
				  if (e.OriginalSource==tabControl && tabControl.SelectedItem == tiCbf)
				  {
					  if (cbfJtcy.IsDirty())
					  {
						  cbfJtcy.Flush();
						  var cbf = cbfJtcy.FindCbf();

						  cbfProperty.SetItemValue(nameof(DC_QSSJ_CBF.CBFMC), cbf?.CYXM);
						  cbfProperty.SetItemValue(nameof(DC_QSSJ_CBF.CBFZJHM), cbf?.CYZJHM);
						  cbfProperty.SetItemValue(nameof(DC_QSSJ_CBF.CBFCYSL), cbfJtcy.DataSource.Count);
						  cbfProperty.SetItemValue(nameof(DC_QSSJ_CBF.CBFZJLX), CodeUtil.Code2Name(CodeType.ZJLX, cbf?.Entity?.CYZJLX));

						  

						  //var en = cbfProperty.Entity as DC_QSSJ_CBF;
						  //en.CBFMC = cbf?.CYXM;
						  //en.CBFZJHM = cbf?.CYZJHM;
						  //en.CBFCYSL = cbfJtcy.DataSource.Count;
						  //en.CBFZJLX = cbf?.Entity?.CYZJLX;
						  //cbfProperty.UpdateUI(en);
					  }
				  }
			  };

			//yxm 2021-4-13 设置承包方类型默认为“农户”
			cbfProperty.OnGetItemDefaultValue = it =>
			  {
				  if (StringUtil.isEqualIgnorCase(it.Field.FieldName,DC_QSSJ_CBF.GetFieldName(nameof(DC_QSSJ_CBF.CBFLX))))
				  {
					  return "农户";
				  }
				  else if (StringUtil.isEqualIgnorCase(it.Field.FieldName, DC_QSSJ_CBF.GetFieldName(nameof(DC_QSSJ_CBF.CBFDZ))))
				  {
					  var lst = cbfProperty.GetCodeList(DC_QSSJ_CBF.GetFieldName(nameof(DC_QSSJ_CBF.FBFBM)));
					  if (lst?.Count() == 1)
					  {
						  return ((CodeItem)lst[0]).Tag;
					  }
				  }
				  return null;
			  };
			//Loaded += (s, e) =>
			//  {
			//	  var lst=cbfProperty.GetCodeList(DC_QSSJ_CBF.GetFieldName(nameof(DC_QSSJ_CBF.FBFBM)));
			//	  if (lst?.Count() == 1) {
			//		  cbfProperty.SetItemValue(DC_QSSJ_CBF.GetFieldName(nameof(DC_QSSJ_CBF.CBFDZ)), ((CodeItem)lst[0]).Tag);
			//	  }
			//  };

		}
		public void ShowDialog(Window owner, CbfListPanel.CbfListItem cbfIt)
		{
			_pit = cbfIt;
			var cbfID = cbfIt?.ID;
			DC_QSSJ_CBF en;
			if (IsCreateMode)
			{
				en = new DC_QSSJ_CBF()
				{
					CBFBM = Guid.NewGuid().ToString(),
					CJSJ=DateTime.Now,
					ZT=(int)EDKZT.Lins,
					DJZT=(int)EDjzt.Wdj
				};
				if (_lsfFbfbm?.Count == 1)
				{
					en.FBFBM = _lsfFbfbm[0].GetCode().ToString();
				}
			}
			else
			{
				en = repos.Find(t => t.ID == cbfID);
			}
			cbfProperty.UpdateUI(en);
			cbfJtcy.UpdateUI(en);
			var fBtnOkClicked = false;
			dlg = new KuiDialog(owner, "承包方属性")
			{
				Content = this,
				Width = 720,
				Height = 550,
			};
			dlg.BtnOK.Click += (s, e) =>
			  {
				  try
				  {
					  fBtnOkClicked = true;
					  if (IsDirty())
					  {
						  Save();
					  }
					  dlg.Close();
				  }
				  catch (Exception ex)
				  {
					  MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				  }
			  };
			dlg.Closed += (s, e) => dlg.Content = null;
			dlg.Closing += (s, e) =>
			  {
				  try
				  {
					  if (!fBtnOkClicked)
					  {
						  if (IsDirty())
						  {
							  var mr = MessageBox.Show("文档已改变，是否保存？", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
							  switch (mr)
							  {
								  case MessageBoxResult.Cancel: e.Cancel = true; return;
								  case MessageBoxResult.Yes:
									  Save();
									  break;
							  }
						  }
					  }
				  }
				  catch (Exception ex)
				  {
					  e.Cancel = true;
					  MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				  }
			  };
			dlg.BtnOK.IsEnabled = false;
			dlg.ShowDialog();
		}
		private void Save()
		{
			try
			{
				var err = CheckError();
				if(err!=null) throw new Exception(err);
				cbfProperty.Flush();
				repos.Db.BeginTransaction();
				var fJtcyDirty = cbfJtcy.IsDirty();
				if (fJtcyDirty)
				{
					cbfJtcy.Flush();
					cbfJtcy.Save();
				}

				var en = cbfProperty.Entity as DC_QSSJ_CBF;

				en.ZHXGSJ = DateTime.Now;
				if (_pit != null)
				{
					repos.Update(en, t => t.ID == en.ID);
				}
				else
				{
					repos.Insert(en);
				}
				repos.Db.Commit();
				if (fJtcyDirty)
				{
					cbfJtcy.UpdateUI(en);
				}
				if (_pit != null)
				{
					_pit.CBFMC = en.CBFMC;
				}
				if (IsCreateMode)
				{
					OnCbfCreated?.Invoke(en);
				}
			}
			catch (Exception ex)
			{
				repos.Db.Rollback();
				throw ex;
			}
		}
		private bool IsDirty()
		{
			return cbfJtcy.IsDirty() || cbfProperty.IsDirty();
		}
		private void UpateSaveButtonUI()
		{
			if (dlg != null)
			{
				dlg.BtnOK.IsEnabled = IsDirty();
			}
		}
		private string CheckError()
		{
			if (IsCreateMode && cbfJtcy.DataSource.Count == 0)
			{
				tabControl.SelectedItem = tiJtcy;
				return "未录入家庭成员！";
			}
			string err = null;
			var fJtcyDirty = cbfJtcy.IsDirty();
			if (fJtcyDirty)
			{
				cbfJtcy.Flush();
				err = cbfJtcy.LocateError();
				if (err != null)
				{
					if (tabControl.SelectedItem != tiJtcy)
					{
						tabControl.SelectedItem = tiJtcy;
					}
					return err;
				}
			}
			cbfProperty.Flush();
			var en = cbfProperty.Entity as DC_QSSJ_CBF;
			err= EntityUtil.CheckErrorForNull(en);
			if (err != null&&tabControl.SelectedItem!=tiCbf)
			{
				tabControl.SelectedItem = tiCbf;
			}
			return err;
		}
	}
}
