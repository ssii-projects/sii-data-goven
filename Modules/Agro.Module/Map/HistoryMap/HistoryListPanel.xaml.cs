using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.GIS;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using GeoAPI.Geometries;
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

namespace Agro.Module.Map
{
	/// <summary>
	/// HistoryListPanel.xaml 的交互逻辑
	/// </summary>
	public partial class HistoryListPanel : UserControl
	{
		private static TreeItemData _dummyNode = null;
		public class TreeItemData : TreeItemDataBase<TreeItemData>
		{
			static TreeItemData()
			{
				//_errImg = MyImageUtil.Image24("exclamation.png");
			}

			private static ImageSource icon = CommonImageUtil.Image24("calendar.png");
			private static ImageSource iconLeaf = CommonImageUtil.Image16("region.png");
			//private string _title;
			private bool _fBusy = false;

			/// <summary>
			/// 图层名称
			/// </summary>
			public string Title
			{
				get
				{
					if (Date != null)
					{
						return ((DateTime)Date).ToString("D");
					}
					return Dkbm;
				}
				//get { return _title; }
				//set { _title = value; NotifyPropertyChanged("Title"); }
			}
			private ImageSource _icon;
			/// <summary>
			/// 图层样式
			/// </summary>
			public ImageSource Icon
			{
				get { return _icon; }
				set { _icon = value; RaisePropertyChanged(nameof(Icon)); }
			}

			public bool IsBusy
			{
				get
				{
					return _fBusy;
				}
				set
				{
					_fBusy = value;
					RaisePropertyChanged(nameof(IsBusy));
					RaisePropertyChanged("IsReady");
				}
			}

			private bool _isExpanded;


			public bool IsExpanded
			{
				get { return _isExpanded; }
				set
				{
					_isExpanded = value;
					if (value)
					{
						if (Children.Count == 1 && Children[0] == _dummyNode)
						{
							FillChildren();
						}
					}
					RaisePropertyChanged(nameof(IsExpanded));
				}
			}
			private readonly HistoryListPanel _p;

			internal readonly DateTime? Date;
			internal readonly string Dkbm;
			internal readonly int OID;

			public TreeItemData(HistoryListPanel p, DateTime? date,string dkbm,int oid, TreeItemData parent = null)//, bool fExpanded=true)
				: base(parent)
			{
				_p = p;
				_icon =date==null?iconLeaf: icon;
				Date = date;
				Dkbm = dkbm;
				OID = oid;
			}

			internal TreeItemData GetParent()
			{
				return (TreeItemData)base.Parent;
			}
			internal void FillChildren()
			{
				Children.Clear();
				if (Date == null)
				{
					return;
				}
				if (_isExpanded)
				{
					QueryDkbms((DateTime)Date, (dkbm,oid) =>
					{
						var n1 = new TreeItemData(_p, null,dkbm,oid, this);
						Children.Add(n1);
					});
				}
				else
				{
					if (Date!=null)
					{
						Children.Add(_dummyNode);
					}
				}
			}
		}


		private readonly ObservableCollection<TreeItemData> _dataSoure = new ObservableCollection<TreeItemData>();
		private readonly ObservableCollection<string> _monthSource = new ObservableCollection<string>();
		private readonly ObservableCollection<string> _yearSource = new ObservableCollection<string>();

		private readonly MapControl _mc;
		private readonly TimeSliderControl _timeSlider;
		public HistoryListPanel(MapControl mc, TimeSliderControl tsc)
		{
			_mc = mc;
			_timeSlider = tsc;
			InitializeComponent();
			treeView.ItemsSource = _dataSoure;
			cbMonth.ItemsSource = _monthSource;
			cbYear.ItemsSource = _yearSource;
			cbYear.SelectionChanged += (s, e) =>
			{
				var nYear = 0;
				if (cbYear.SelectedIndex > 0)
				{
					nYear = SafeConvertAux.ToInt32(cbYear.SelectedValue);
				}
				ResetComboMonth(nYear);
				//RefreshTree(nYear, 0);
			};
			cbMonth.SelectionChanged += (s, e) =>
			{
				var nYear = 0;
				if (cbYear.SelectedIndex > 0)
				{
					nYear = SafeConvertAux.ToInt32(cbYear.SelectedValue);
				}
				var month = 0;
				if (cbMonth.SelectedIndex > 0)
				{
					month = SafeConvertAux.ToInt32(cbMonth.SelectedValue);
				}
				RefreshTree(nYear, month);
			};

			Refresh();
		}
		public void Refresh()
		{
			_yearSource.Clear();
			_yearSource.Add("所有年份");
			RefreshTree(0, 0, d =>
			{
				var sYear = d.Year.ToString();
				if (!_yearSource.Contains(sYear))
				{
					_yearSource.Add(sYear);
				}
			});			
			cbYear.SelectedIndex = 0;
		}
		private void RefreshTree(int Year,int Month,Action<DateTime> callback=null)
		{
			_dataSoure.Clear();
			QueryDates(Year, Month, d =>
			  {
				  var td = new TreeItemData(this, d, null, 0);
				  td.FillChildren();
				  _dataSoure.Add(td);
				  callback?.Invoke(d);
			  });
		}

		private void ResetComboMonth(int year)
		{
			_monthSource.Clear();
			_monthSource.Add("所有月份");
			if (year > 0)
			{
				var lst = new List<int>();
				//var sql = "select distinct DJSJ from (select t1.*, t2.DJSJ from DLXX_DK_TXBGJL t1 left join DLXX_DK t2 on t1.DKID = t2.ID ) t where t.DJSJ like '"+year+"%' order by t.DJSJ";
				//var sql = "select distinct DJSJ from DLXX_DK  where SJLY>0 and DJSJ like '" + year + "%' order by DJSJ";
				//var sql = "select distinct CJSJ from DLXX_DK  where SJLY>0 and CJSJ like '" + year + "%' order by CJSJ";
				var sql = "select distinct convert(date,CJSJ) from DLXX_DK  where SJLY>0 and CJSJ like '" + year + "%' order by convert(date,CJSJ)";
				MyGlobal.Workspace.QueryCallback(sql, r =>
				 {
					 if (!r.IsDBNull(0) && r.GetValue(0) is DateTime d)
					 {
						 if (!lst.Contains(d.Month))
						 {
							 lst.Add(d.Month);
						 }
					 }
					 return true;
				 });
				foreach (var m in lst)
				{
					_monthSource.Add(m.ToString());
				}
			}
			cbMonth.SelectedIndex = 0;
		}
		private void BtnZoomTo_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.Tag is TreeItemData ti && ti.Dkbm!=null)
			{
				var sql = "select shape from DLXX_DK where BSM=" + ti.OID;
				var o=MyGlobal.Workspace.QueryOne(sql);
				if (o is IGeometry g)
				{
					var env = g.EnvelopeInternal;
					var newEnv=env.ScaleAt(4, env.Centre.X, env.Centre.Y);
					_mc.FocusMap.ZoomTo(GeometryUtil.MakePolygon(newEnv,g.GetSpatialReference()));
					_timeSlider.SetValue((DateTime)ti.GetParent().Date);
				}
			}
		}



		private static void QueryDkbms(DateTime dt, Action<string,int> callback)
		{
			//var sql = "  select DKBM,BSM from (select t1.DKBM, t2.DJSJ,t2.BSM from DLXX_DK_TXBGJL t1 left join DLXX_DK t2 on t1.DKID = t2.ID) t where t.DJSJ = '" + dt + "'";
			var sd = dt.ToString();
			var sql = $"select DKBM,BSM from DLXX_DK where SJLY>0 and convert(date,CJSJ) = convert(date,'{sd}')";
			MyGlobal.Workspace.QueryCallback(sql, r =>
			{
				if (!r.IsDBNull(0))
				{
					var oid = SafeConvertAux.ToInt32(r.GetValue(1));
					callback(r.GetString(0),oid);
				}
				return true;
			});
		}

		private static void QueryDates(int Year, int Month,Action<DateTime> callback)
		{
			string wh = null;
			if (Month > 0)
			{
				wh = " like '" + Year + "-" + Month.ToString().PadLeft(2, '0')+"%'";
			}
			else if (Year > 0)
			{
				wh += " like '" + Year + "%'";
			}

			//var sql = "select distinct DJSJ from (select t1.*, t2.DJSJ from DLXX_DK_TXBGJL t1 left join DLXX_DK t2 on t1.DKID = t2.ID ) t";
			//var sql = "select distinct DJSJ from DLXX_DK where SJLY>0";
			var sql = "select distinct convert(date,CJSJ) from DLXX_DK where SJLY>0";
			if (wh != null)
			{
				sql += " and CJSJ " + wh;
			}
			sql+= " order by convert(date,CJSJ) desc";
			MyGlobal.Workspace.QueryCallback(sql, r =>
			 {
				 if (!r.IsDBNull(0))
				 {
					 if (r.GetValue(0) is DateTime d)
					 {
						 callback(d);
					 }
				 }
				 return true;
			 });
		}
	}
}
