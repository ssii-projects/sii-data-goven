using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.LibCore.UI;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Agro.Module.DataOperator
{
	/// <summary>
	/// CbfListPanel.xaml 的交互逻辑
	/// </summary>
	public partial class CbfListPanel : UserControl
	{
		public class CbfListItem:NotificationObject
		{
			public string ID;
			public string CBFBM { get; set; }

			private string _cbfmc;
			public string CBFMC { get { return _cbfmc; } set
				{
					_cbfmc = value;
					RaisePropertyChanged(nameof(CBFMC));
				}
			}

			/// <summary>
			/// 是否新增承包方
			/// </summary>
			/// <returns></returns>
			public bool IsNewCbf()
			{
				return CBFBM?.Length > 18;
			}
		}
		private readonly ObservableCollection<CbfListItem> DataSource = new ObservableCollection<CbfListItem>();

		//private readonly IFeatureWorkspace _db;
		private readonly DcCbfRepos cbfRepos;
		private readonly DcCbfJtcyRepos jtcyRepos;
		private readonly CbfPropertyPanel cbfDialog;
		public CbfListPanel(IFeatureWorkspace db, List<ICodeItem> lstFbfbm)
		{
			//_db = db;
			InitializeComponent();
			cbfRepos = new DcCbfRepos(db);
			jtcyRepos = new DcCbfJtcyRepos(db);
			cbfDialog = new CbfPropertyPanel(cbfRepos, jtcyRepos, lstFbfbm);
			cbfDialog.OnCbfCreated += en =>
			  {
				  var it = new CbfListItem()
				  {
					  ID=en.ID,
					  CBFBM=en.CBFBM,
					  CBFMC=en.CBFMC
				  };
				  DataSource.Add(it);
				  lstBox.SelectedItem = it;
				  lstBox.ScrollIntoView(it);
			  };
			lstBox.ItemsSource = DataSource;
			txtSearch.OnButtonClick = () => Refresh();
			lstBox.MouseDoubleClick += (s, e) =>
			  {
				  if (lstBox.SelectedItem is CbfListItem li)
				  {
					  cbfDialog.ShowDialog(Window.GetWindow(this), li);
				  }
			  };
			btnAdd.Click += (s, e) =>cbfDialog.ShowDialog(Window.GetWindow(this), null);
			btnDel.Click += (s, e) => DeleteCbf();
			CheckBoxUtil.OnCheckChanged(ckbOnlyShowAppend, _ => Refresh());
			lstBox.SelectionChanged += (s, e) => UpdateButtonState();
			Refresh();
		}
		public void Refresh()
		{
			string keyWord = txtSearch.Text.Trim().Replace("'", "''");
			DataSource.Clear();

			string wh = null;
			if (!string.IsNullOrEmpty(keyWord))
			{
				wh=$"(cbfmc like '%{keyWord}%' or cbfbm in (select cbfbm from DC_QSSJ_CBF_JTCY where CYXM like '%{keyWord}%'))";
			}
			if (ckbOnlyShowAppend.IsChecked == true)
			{
				if (wh != null)
				{
					wh += " and ";
				}
				wh += "ZHXGSJ is not null and LENGTH(CBFBM)>20";
			}
			cbfRepos.FindCallback(wh, it =>
			 {
				 var en = it.Item;
				 DataSource.Add(new CbfListItem()
				 {
					 ID=en.ID,
					 CBFBM=en.CBFBM,
					 CBFMC=en.CBFMC
				 });
			 }, SubFields.Make<DC_QSSJ_CBF>((c, t) => c(t.ID, t.CBFBM, t.CBFMC)));
			UpdateButtonState();
		}
		private void UpdateButtonState() {
			btnDel.IsEnabled = lstBox.SelectedItem is CbfListItem it && it.IsNewCbf();
		}
		private void DeleteCbf()
		{
			try
			{
				if (lstBox.SelectedItem is CbfListItem it && it.IsNewCbf())
				{
					var mr= MessageBox.Show("确定要删除选中的承包方吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (mr == MessageBoxResult.Yes)
					{
						var db = cbfRepos.Db;
						try
						{
							db.BeginTransaction();
							jtcyRepos.Delete(t => t.CBFBM == it.CBFBM);
							cbfRepos.Delete(t => t.ID == it.ID);
							db.Commit();
							DataSource.Remove(it);
							UpdateButtonState();
						}
						catch (Exception e)
						{
							db.Rollback();
							throw e;
						}
					}
				}
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}
	}
}
