using Agro.LibCore.UI;
using Agro.LibCore;
using Agro.Library.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Linq;

namespace Agro.Module.DataUpdate
{
	/// <summary>
	/// DataDeleteListPanel.xaml 的交互逻辑
	/// </summary>
	public partial class DataDeleteListPanel : UserControl
	{
		class ZoneListItem : NotificationObject
		{
			public string Title { get { return Entity.MC; } }
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
			internal readonly DLXX_XZDY Entity;
			public ZoneListItem(DLXX_XZDY en)
			{
				Entity = en;
			}
		}
		class ZoneTreeItem : TreeItemDataBase<ZoneTreeItem>
		{
			public string Title { get { return Entity.MC; } }

			private bool _isExpanded;
			public bool IsExpanded
			{
				get { return _isExpanded; }
				set
				{
					_isExpanded = value;
					RaisePropertyChanged(nameof(IsExpanded));
				}
			}

			public readonly List<ZoneListItem> Codes;
			internal readonly DLXX_XZDY Entity;
			public ZoneTreeItem(DLXX_XZDY en, ZoneTreeItem parent = null, List<ZoneListItem> lstXianBm = null) : base(parent, true)
			{
				Entity = en;
				Codes = lstXianBm;
			}
		}

		private readonly ObservableCollection<ZoneTreeItem> _treeSoure = new ObservableCollection<ZoneTreeItem>();
		private readonly ObservableCollection<ZoneListItem> _lstSource=new ObservableCollection<ZoneListItem>();

		public Action<IEnumerable<DLXX_XZDY>> OnDeleteClick; 
		public DataDeleteListPanel()
		{
			InitializeComponent();
			treeView.ItemsSource = _treeSoure;
			treeView.SelectedItemChanged += (s, e) => OnZoneChanged();
			lstBox.ItemsSource = _lstSource;
		}

		public void InitZoneBar(List<DLXX_XZDY> lstEn, eZoneLevel leafNodeJb)
		{
			var dicZoneItem = new Dictionary<string, ZoneTreeItem>();
			foreach (var en in lstEn)
			{
				if (en.JB < leafNodeJb)
				{
					continue;
				}
				switch (en.JB)
				{
					case eZoneLevel.County:
						{
							var zone = new ZoneTreeItem(en, null, leafNodeJb == en.JB ? new List<ZoneListItem>() : null);
							if (zone.Codes == null)
							{
								zone.IsExpanded = true;
							}
							_treeSoure.Add(zone);
							dicZoneItem[en.ID] = zone;
							TreeItemDataBase<ZoneTreeItem>.SetSelectedItem(treeView, zone);
						}
						break;
					case eZoneLevel.Town:
					case eZoneLevel.Village:
						{
							if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
							{
								var zone = new ZoneTreeItem(en, pZone, leafNodeJb == en.JB ? new List<ZoneListItem>() : null);
								dicZoneItem[en.ID] = zone;
							}
						}
						break;
					//case eZoneLevel.Village:
					//	{
					//		if (fHasGroup)
					//		{
					//			if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
					//			{
					//				var zone = new ZoneItem(en, pZone, new List<CodeItem>());
					//				dicZoneItem[en.ID] = zone;
					//			}
					//		}
					//		else
					//		{
					//			if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
					//			{
					//				pZone.Codes.Add(new CodeItem(en.BM, en.MC));
					//			}
					//		}
					//	}
					//	break;
					//case eZoneLevel.Group:
					//	{
					//		if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
					//		{
					//			pZone.Codes.Add(new CodeItem(en.BM, en.MC));
					//		}
					//	}
					//	break;
				}
			}
			foreach (var en in lstEn)
			{
				if ((int)en.JB == ((int)leafNodeJb) - 1)
				{
					if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
					{
						pZone.Codes.Add(new ZoneListItem(en));
					}
				}
			}
		}

		private void OnZoneChanged()
		{
			var it1 = treeView.SelectedItem as ZoneTreeItem;
			try
			{
				var lst = _lstSource;
				lst.Clear();
				if (it1.Codes != null)
				{
					lst.AddRange(it1.Codes);
				}
				//else
				//{
				//	foreach (var c in it1.Children)
				//	{
				//		lst.AddRange(c.ListCunBm);
				//	}
				//}
				//var str = "操作";
				//if (lst.Count > 0)
				//{
				//	str += $"（{lst.Count}）";
				//}
				//tbOperate.Text = str;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		private void BtnDel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var lst = _lstSource.FindAll(it => it.IsSelected);
				if (lst.Count == 0)
				{
					MessageBox.Show("未选择待删除项！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				
				var mr = MessageBox.Show("确定要删除所选行政地域下的所有数据吗（慎用）？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				if (mr != MessageBoxResult.Yes)
				{
					return;
				}
				OnDeleteClick(lst.Select(it => it.Entity));
				
				//var codeItems = new List<CodeItem>();
				//foreach (var it in lstBox1.SelectedItems)
				//{
				//	codeItems.Add((CodeItem)it);
				//}

				//taskPage.RemoveAll();
				//var task = new DeleteTask(codeItems);
				//taskPage.AddTaskToPool(task);
				//taskPage.AutoAjustColumnWidth();
				//taskPage.Start();

				//sbp.Visibility = Visibility.Hidden;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void Btn_Click(object sender, RoutedEventArgs e)
		{
			foreach (var cl in _lstSource)
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
	}
}
