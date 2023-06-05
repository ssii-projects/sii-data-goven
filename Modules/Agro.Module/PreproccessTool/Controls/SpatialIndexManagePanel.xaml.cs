using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
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

namespace Agro.Module.PreproccessTool
{
	/// <summary>
	/// SpatialIndexManagePanel.xaml 的交互逻辑
	/// </summary>
	public partial class SpatialIndexManagePanel : UserControl
	{
		class Item: NotificationObject
		{
			public readonly string TableName;
			public string Name { get; private set; }

			private bool _fSelected;
			public bool IsSelected
			{
				get { return _fSelected; }
				set
				{
					_fSelected = value;
					base.RaisePropertyChanged(nameof(IsSelected));
				}
			}
			private ImageSource tickPng;
			public ImageSource Image { get { return tickPng; } set
				{
					tickPng = value;
					base.RaisePropertyChanged(nameof(Image));
				}
			}
			public bool HasIndex { get { return Image != null; } }
			public Item(string name, string tableName)
			{
				Name = name;
				TableName = tableName;
			}
			public override string ToString()
			{
				return Name;
			}
		}

		private readonly ImageSource tickPng;
		public SpatialIndexManagePanel()
		{
			InitializeComponent();
			tickPng = LibMapImageSourceUtil.Image16("tick.png");
			var items = new Item[]
			{
				new Item("行政地域","DLXX_XZDY"),
				new Item("地块","DLXX_DK"),
				new Item("界址点","DLXX_DK_JZD"),
				new Item("界址线","DLXX_DK_JZX"),
				//new Item("点状地物","DLXX_DZDW"),
				//new Item("面状地物","DLXX_MZDW"),
				//new Item("线状地物","DLXX_XZDW"),
				//new Item("控制点","DLXX_KZD"),
				//new Item("区域界线","DLXX_QYJX"),
				//new Item("基本农田保护区","DLXX_JBNTBHQ"),
			};
			lstBox.ItemsSource = items;
			foreach (var it in items)
			{
				if (IdxName(it.TableName) != null)
				{
					it.Image = tickPng;
				}
			}
			

			lstBox.SelectionChanged += (s, e) => UpdateUI();
			UpdateUI();

		}
		public void ShowDialog(Window owner)
		{
			var dlg = new KuiDialog(owner, "空间索引管理")
			{
				Content = this
			};
			dlg.Height = 400;
			dlg.HideBottom();
			dlg.ShowDialog();
		}
		void UpdateUI()
		{
			//if (lstBox.SelectedItem is Item it)
			//{
			//	var idxExist = IdxName(it.TableName) != null;
			//	btnCreate.IsEnabled = !idxExist;
			//	btnDel.IsEnabled = idxExist;
			//	btnReCreate.IsEnabled = idxExist;
			//}
			//else
			//{
			//	btnCreate.IsEnabled = false;
			//	btnDel.IsEnabled = false;
			//	btnReCreate.IsEnabled = false;
			//}
		}
		string IdxName(string tableName) {
			var sql = $"select name  from sys.spatial_indexes where object_id=object_id('{tableName}')";
			var o=MyGlobal.Workspace.QueryOne(sql);
			return o?.ToString();
		}

		private void Btn1_Click(object sender, RoutedEventArgs e)
		{
			var DataSource = lstBox.ItemsSource as Item[];
			foreach (var cl in DataSource)
			{
				if (sender == btnSelectAll)
				{
					cl.IsSelected = true;
				}
				else if (sender == btnNotSelectAll)
				{
					cl.IsSelected = false;
				}
				else if (sender == btnXorSelect)
				{
					cl.IsSelected = !cl.IsSelected;
				}
			}
		}
		private void Btn_Click(object sender, RoutedEventArgs e)
		{
			Try.Catch(() =>
			{
				var items = lstBox.ItemsSource as Item[];
				if (0 == items.Count(it => it.IsSelected))
				{
					MessageBox.Show("未选择要操作的记录", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				var oldCursor = Cursor;
				Cursor = Cursors.Wait;
				//var tableName = (lstBox.SelectedItem as Item).TableName;
				if (sender == btnDel)
				{
					foreach (var it in items)
					{
						if (it.IsSelected&&it.HasIndex)
						{
							DropSpatialIndex(it.TableName);
							it.Image = IdxName(it.TableName) != null ? tickPng : null;
						}
					}
					//DropSpatialIndex(tableName);
				}
				else if (sender == btnCreate)
				{
					foreach (var it in items)
					{
						if (it.IsSelected && !it.HasIndex)
						{
							CreateIndex(it.TableName);
							it.Image = IdxName(it.TableName) != null ? tickPng : null;
							UIHelper.DoEvents();
						}
					}
					//CreateIndex(tableName);
				}
				else if (sender == btnReCreate)
				{
					//DropSpatialIndex(tableName);
					//CreateIndex(tableName);
				}
				UpdateUI();
				Cursor = oldCursor;
			});
		}
		void CreateIndex(string tableName)
		{
			var boundingBox=BoundingBox();
			if (boundingBox == null)
			{
				throw new Exception("获取行政地域范围失败！");
			}
			var db = MyGlobal.Workspace;
			using (var fc = db.OpenFeatureClass(tableName))
			{
				var idxName = $"s_idx_{tableName.ToLower()}";
				//				var sql = $"CREATE SPATIAL INDEX {idxName} ON {tableName}({fc.ShapeFieldName})USING GEOMETRY_AUTO_GRID";
				//				sql += $"WITH(BOUNDING_BOX = (487120.366822, 3106411.794583, 534959.415698, 3148956.620308),";
				//sql+="CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]";

				var sql = $"CREATE SPATIAL INDEX {idxName} ON {tableName}({fc.ShapeFieldName})";
				sql += $"WITH(BOUNDING_BOX = ({boundingBox}));";
				MyGlobal.Workspace.ExecuteNonQuery(sql);
			}
		}
		string BoundingBox()
		{
			var db = MyGlobal.Workspace;
			using (var fc = db.OpenFeatureClass("DLXX_XZDY"))
			{
				var fe=fc.GetFullExtent();
				if (fe != null)
				{
					var minx = fe.MinX;
					var maxx = fe.MaxX;
					var miny = fe.MinY;
					var maxy = fe.MaxY;
					if (fc.SpatialReference?.IsPROJCS() == true)
					{
						var buffer = 10000;
						minx = Math.Floor(minx) - buffer;
						maxx = Math.Ceiling(maxx) + buffer;
						miny = Math.Floor(miny) - buffer;
						maxy = Math.Ceiling(maxy) + buffer;
					}
					return $"{minx},{miny},{maxx},{maxy}";
				}
			}
			return null;
		}
		void DropSpatialIndex(string tableName) {
			var idxName = IdxName(tableName);
			if (idxName != null)
			{
				var sql = $"DROP INDEX {idxName} ON {tableName}";
				MyGlobal.Workspace.ExecuteNonQuery(sql);
			}
		}
	}
}
