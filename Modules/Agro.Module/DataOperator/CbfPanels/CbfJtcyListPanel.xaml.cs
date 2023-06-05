using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System;
using Agro.LibCore.UI;
using FieldItem = Agro.LibCore.UI.EntityPropertyView.FieldItem;

namespace Agro.Module.DataOperator
{
	/// <summary>
	/// CbfJtcyListPanel.xaml 的交互逻辑
	/// </summary>
	public partial class CbfJtcyListPanel : UserControl
	{
		public class CbfJtcyItem:NotificationObject
		{
			public string CYXM { get; set; }
			public string CYZJHM { get; set; }
			public string YHZGX { get; set; }
			public readonly DC_QSSJ_CBF_JTCY Entity = new DC_QSSJ_CBF_JTCY();
			public bool IsDirty { get; set; } = false;
			public CbfJtcyItem(DC_QSSJ_CBF_JTCY en)
			{
				Entity.OverWrite(en);
				YHZGX = CodeUtil.Jtgx(en.YHZGX);
				CYXM = en.CYXM;
				CYZJHM = en.CYZJHM;
			}
		}
		public readonly ObservableCollection<CbfJtcyItem> DataSource = new ObservableCollection<CbfJtcyItem>();
		private readonly DcCbfJtcyRepos repos;
		private DC_QSSJ_CBF cbfEntity;
		private readonly List<CbfJtcyItem> delItems = new List<CbfJtcyItem>();
		private bool fSuppressEvent = false;

		public readonly Action OnValueChanged;
		public CbfJtcyListPanel(DcCbfJtcyRepos jtcyRepos, Action onValueChanged)
		{
			OnValueChanged = onValueChanged;
			InitializeComponent();
			lstBox.ItemsSource = DataSource;
			repos = jtcyRepos;// new DcCbfJtcyRepos(db);
			lstBox.SelectionChanged += (s, e) =>
			{
				Flush();
				UpdateUI();
			};
			jtcyProperty.OnCustomUI += ds =>
			  {
				  var db = repos.Db;
				  for (int i = ds.Count; --i >= 0;)
				  {
					  var it = ds[i];
					  var fieldName = it.Field.FieldName;
					  if ("ID,CBFBM".Contains(fieldName) || it.Field.FieldType == eFieldType.eFieldTypeDateTime)
					  {
						  ds.RemoveAt(i);
						  continue;
					  }

					  if (!string.IsNullOrEmpty(it.Field.CodeType))
					  {
						  it.ComboItems = CodeUtil.QueryCodeItems(it.Field.CodeType, db).Cast<object>().ToList();
					  }
				  }
			  };
			jtcyProperty.OnValueChanged += fi =>
			{
				//var fi = fix as FieldItem;
				if (!fSuppressEvent)
				{
					var ft = DataSource.Find(it => it.Entity == jtcyProperty.Entity);
					if (ft != null)
					{
						var fieldName = fi.Field.FieldName;
						if ("CYXM".Contains(fieldName))
						{
							ft.CYXM = fi.Value?.ToString();
							ft.RaisePropertyChanged(fieldName);
						}else if ("CYZJHM".Contains(fieldName))
						{
							ft.CYZJHM = fi.Value?.ToString();
							ft.RaisePropertyChanged(fieldName);
						}
						else if ("YHZGX".Contains(fieldName))
						{
							ft.YHZGX = fi.Value?.ToString();
							ft.RaisePropertyChanged(fieldName);
						}

						if (jtcyProperty.IsDirty())
						{
							ft.IsDirty = true;
						}
					}
					OnValueChanged();
				}
			};
			btnDel.Click += (s, e) =>
			  {
				  if (lstBox.SelectedItem is CbfJtcyItem ji)
				  {
					  delItems.Add(ji);
					  var idx = lstBox.SelectedIndex;
					  DataSource.Remove(ji);
					  if (DataSource.Count > 0)
					  {
						  lstBox.SelectedIndex = Math.Min(idx, DataSource.Count - 1);
					  }
					  OnValueChanged();
				  }
			  };
			btnReset.Click += (s, e) => { Reset(); OnValueChanged(); };
			btnAdd.Click += (s, e) =>
		  {
			  var it = new CbfJtcyItem(new DC_QSSJ_CBF_JTCY() { ID = null,CBFBM=cbfEntity.CBFBM });
			  DataSource.Add(it);
			  lstBox.SelectedItem = it;
			  OnValueChanged();
		  };

			#region yxm 2021-4-13 家庭成员证件类型默认设置为“居民身份证”
			jtcyProperty.OnGetItemDefaultValue = it =>
			  {
				  if (StringUtil.isEqualIgnorCase(it.Field.FieldName, DC_QSSJ_CBF_JTCY.GetFieldName(nameof(DC_QSSJ_CBF_JTCY.CYZJLX)))){
					  return "居民身份证";
				  }
				  return null;
			  };
			#endregion
		}

		/// <summary>
		/// 查找承包方
		/// </summary>
		/// <returns></returns>
		public CbfJtcyItem FindCbf()
		{
			return DataSource.Find(it => IsHuzu(it.Entity));
		}
		public void UpdateUI(DC_QSSJ_CBF cbfEn)
		{
			cbfEntity = cbfEn;
			Reset();
		}
		public bool IsDirty()
		{
			if (delItems.Count > 0) return true;
			for (var i = DataSource.Count; --i >= 0;)
			{
				var it = DataSource[i];
				if (it.IsDirty) return true;
				if (it.Entity.ID == null) return true;
			}
			return false;
		}
		public string LocateError()
		{
			if (lstBox.SelectedItem is CbfJtcyItem it)
			{
				var err = CheckError(it);
				if (err != null)
				{
					return err;
				}
			}
			var nHzCount = 0;
			foreach (var itx in DataSource)
			{
				nHzCount += IsHuzu(itx.Entity) ? 1 : 0;
				if (itx != lstBox.SelectedItem)
				{
					var err = CheckError(itx);
					if (err != null)
					{
						lstBox.SelectedItem = itx;
						return err;
					}
				}
			}
			if (nHzCount > 1)
			{
				return "与户主关系为'本人'或'户主'的只能有1个（户主只能有1个）";
			}
			else if (nHzCount == 0)
			{
				return "至少应该有一个户主（与户主关系为：'本人'或'户主'）";
			}
			return null;
		}
		public void Save()
		{
			var hzEn = DataSource.Find(it=>IsHuzu(it.Entity))?.Entity;
			cbfEntity.CBFCYSL = DataSource.Count;
			cbfEntity.CBFMC = hzEn?.CYXM;
			cbfEntity.CBFZJLX = hzEn?.CYZJLX;
			cbfEntity.CBFZJHM = hzEn?.CYZJHM;
			cbfEntity.ZHXGSJ = DateTime.Now;
			repos.Delete(t => t.CBFBM == cbfEntity.CBFBM);
			foreach (var it in DataSource)
			{
				var en = it.Entity;
				if (en.ID == null)
				{
					en.ID = Guid.NewGuid().ToString();
				}
				repos.Insert(en);
			}
		}
		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			base.OnPreviewKeyUp(e);
			try
			{
				if (e.OriginalSource is TextBox tb && tb.Tag is FieldItem fi)
				{
					if (e.Key == Key.Back || e.Key >= Key.D0 && e.Key < Key.Z||e.Key>=Key.NumPad0&&e.Key<=Key.NumPad9)
					{
						var fieldName = fi.Field.PropertyName;
						if (fieldName == nameof(DC_QSSJ_CBF_JTCY.CYZJHM))
						{
							var text = tb.Text.Trim();
							var lst = jtcyProperty.DataSource;
							var zjlx = lst.Find(it =>it.Field.FieldName == nameof(DC_QSSJ_CBF_JTCY.CYZJLX));
							if (zjlx?.Value?.ToString() == "居民身份证")
							{
								PersonIDUtil.Parse(text.Trim(), (rq, xb) =>
								{
									var rqItem = lst.Find(it => it.Field.FieldName == nameof(DC_QSSJ_CBF_JTCY.CSRQ));
									rqItem.Value = rq;
									if (xb != ESex.Unknown)
									{
										var xbItem = lst.Find(it => it.Field.FieldName == nameof(DC_QSSJ_CBF_JTCY.CYXB));
										xbItem.Value = xb == ESex.Male ? "男" : "女";
									}
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		public void Flush()
		{
			if (jtcyProperty.IsDirty())
			{
				jtcyProperty.Flush();
			}
		}
		private void UpdateUI()
		{
			var hasSelected = lstBox.SelectedItem != null;
			btnDel.IsEnabled = hasSelected;
			if (lstBox.SelectedItem is CbfJtcyItem ji)
			{
				jtcyProperty.UpdateUI(ji.Entity);
			}
			else
			{
				jtcyProperty.UpdateUI(null);
			}
		}
		private string CheckError(CbfJtcyItem it)
		{
			var en = it.Entity;
			return EntityUtil.CheckErrorForNull(en, p => p.PropertyName == nameof(DC_QSSJ_CBF_JTCY.ID));
			//foreach (var p in DC_QSSJ_CBF_JTCY.GetAttributes())
			//{
			//	if (!p.Nullable&&p.PropertyName!=nameof(DC_QSSJ_CBF_JTCY.ID))
			//	{
			//		var o = en.GetPropertyValue(p.PropertyName);
			//		if (o == null||p.PropertyType==typeof(string)&&string.IsNullOrEmpty(o.ToString()))
			//		{
			//			return $"{p.AliasName}不能为空！";
			//		}
			//	}
			//}
			//return null;
		}
		private bool IsHuzu(DC_QSSJ_CBF_JTCY en)
		{
			var name = CodeUtil.Code2Name(CodeType.JTGX, en.YHZGX);
			return "本人" == name || "户主" == name;
		}
		private void Reset()
		{
			try
			{
				fSuppressEvent = true;
				DataSource.Clear();
				delItems.Clear();
				repos.FindCallback(t => t.CBFBM == cbfEntity.CBFBM, it => DataSource.Add(new CbfJtcyItem(it.Item)));
				if (DataSource.Count > 0)
				{
					lstBox.SelectedIndex = 0;
				}
				//UpateSaveButtonUI();
			}
			finally
			{
				fSuppressEvent = false;
			}
		}
	}
}
