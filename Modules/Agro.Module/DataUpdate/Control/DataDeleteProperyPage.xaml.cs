using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Agro.Module.DataUpdate
{
	/// <summary>
	/// ImportSSXzqyProperyPage.xaml 的交互逻辑
	/// </summary>
	public partial class DataDeleteProperyPage : TaskPropertyPage
	{
		class DeleteTask : Task
		{
			class TableItem
			{
				public readonly string TableName;
				public readonly string FieldName;
				public TableItem(string tableName, string fieldName)
				{
					TableName = tableName;
					FieldName = fieldName;
				}
			}
			private readonly List<DLXX_XZDY> codeItems = new List<DLXX_XZDY>();
			public DeleteTask(IEnumerable<DLXX_XZDY> lst)
			{
				Name = "数据删除";
				Description = "删除所选地域下的数据";
				codeItems.AddRange(lst);
				OnFinish += (t, e) =>
				  {
					  ReportInfomation($"数据删除耗时：{t.Elapsed}");
				  };
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				var db = MyGlobal.Workspace;
				var tables = new TableItem[]{
					new TableItem(DLXX_DK.GetTableName(),nameof(DLXX_DK.DKBM)),
					new TableItem(DLXX_DK_JZD.GetTableName(),nameof(DLXX_DK_JZD.DKBM)),
					new TableItem(DLXX_DK_JZX.GetTableName(),nameof(DLXX_DK_JZX.DKBM)),
					new TableItem(QSSJ_CBDKXX.GetTableName(),nameof(QSSJ_CBDKXX.DKBM)),
					new TableItem(QSSJ_CBF.GetTableName(),nameof(QSSJ_CBF.CBFBM)),
					new TableItem(QSSJ_CBF_JTCY.GetTableName(),nameof(QSSJ_CBF_JTCY.CBFBM)),
					new TableItem(QSSJ_CBHT.GetTableName(),nameof(QSSJ_CBHT.CBFBM)),
					new TableItem(QSSJ_CBJYQZ.GetTableName(),nameof(QSSJ_CBJYQZ.CBJYQZBM)),
					new TableItem(QSSJ_CBJYQZDJB.GetTableName(),nameof(QSSJ_CBJYQZDJB.CBJYQZBM)),
					new TableItem(QSSJ_FBF.GetTableName(),nameof(QSSJ_FBF.FBFBM)),
					new TableItem(QSSJ_LZHT.GetTableName(),nameof(QSSJ_LZHT.CBHTBM)),
					new TableItem(QSSJ_CBJYQZ_QZBF.GetTableName(),nameof(QSSJ_CBJYQZ_QZBF.CBJYQZBM)),
					new TableItem(QSSJ_CBJYQZ_QZHF.GetTableName(),nameof(QSSJ_CBJYQZ_QZHF.CBJYQZBM)),
					new TableItem(QSSJ_CBJYQZ_QZZX.GetTableName(),nameof(QSSJ_CBJYQZ_QZZX.CBJYQZBM)),
					new TableItem(QSSJ_QSLYZLFJ.GetTableName(),nameof(QSSJ_QSLYZLFJ.CBJYQZBM)),
					new TableItem(DJ_CBJYQ_CBF.GetTableName(),nameof(DJ_CBJYQ_CBF.CBFBM)),
					new TableItem(DJ_CBJYQ_CBF_JTCY.GetTableName(),nameof(DJ_CBJYQ_CBF_JTCY.CBFBM)),
					new TableItem(DJ_CBJYQ_CBHT.GetTableName(),nameof(DJ_CBJYQ_CBHT.CBFBM)),
					new TableItem(DJ_CBJYQ_DJB.GetTableName(),nameof(DJ_CBJYQ_DJB.CBFBM)),
					new TableItem(DJ_CBJYQ_DKXX.GetTableName(),nameof(DJ_CBJYQ_DKXX.DKBM)),
					new TableItem(DJ_CBJYQ_QZ.GetTableName(),nameof(DJ_CBJYQ_QZ.CBJYQZBM)),
					new TableItem(DJ_CBJYQ_QZBF.GetTableName(),nameof(DJ_CBJYQ_QZBF.CBJYQZBM)),
					new TableItem(DJ_CBJYQ_QZHF.GetTableName(),nameof(DJ_CBJYQ_QZHF.CBJYQZBM)),
				};
				Progress.Reset(codeItems.Count * tables.Length);
				try
				{
					db.BeginTransaction();
					foreach (var it in codeItems)
					{
						var bm = it.BM;
						foreach (var tbi in tables)
						{
							Delete(tbi.TableName, tbi.FieldName, bm);
						}
					}
					db.Commit();
				}
				catch (Exception ex)
				{
					db.Rollback();
					throw ex;
				}
			}
			private void Delete(string tableName, string fieldName,string cunBm)
			{
				var sql = $"delete from {tableName} where {fieldName} like '{cunBm}%'";
				MyGlobal.Workspace.ExecuteNonQuery(sql);
				Progress.Step();
			}
		}
		//class ZoneItem : TreeItemDataBase<ZoneItem>
		//{
		//	public string Title { get { return Entity.MC; } }
		//	public readonly List<CodeItem> Codes;
		//	internal readonly DLXX_XZDY Entity;
		//	public ZoneItem(DLXX_XZDY en, ZoneItem parent = null, List<CodeItem> lstXianBm = null) : base(parent, true)
		//	{
		//		Entity = en;
		//		Codes = lstXianBm;
		//	}
		//}
		public DataDeleteProperyPage()
		{
			InitializeComponent();
			base.DialogWidth = 740;
			base.DialogHeight =440;

			var repos = DlxxXzdyRepository.Instance;
			var lstEn = repos.FindAll(t => (int)t.JB <= (int)eZoneLevel.County, (c, t) => c(t.JB, t.MC, t.BM, t.ID, t.SJID));
			lstEn.Sort((a, b) => a.JB > b.JB ? -1 : a.BM.CompareTo(b.BM));
			if (lstEn.Find(it => it.JB == eZoneLevel.Group) == null)
			{
				tiZu.Visibility = Visibility.Collapsed;
			}

			lpXz.InitZoneBar(lstEn, eZoneLevel.County);
			lpCun.InitZoneBar(lstEn, eZoneLevel.Town);
			lpZu.InitZoneBar(lstEn, eZoneLevel.Village);
			lpXz.OnDeleteClick += BtnDel_Click;
			lpCun.OnDeleteClick += BtnDel_Click;
			lpZu.OnDeleteClick += BtnDel_Click;

			taskPage.HideTaskSelectPanel();
		}

		public void OnDlgClosing(object sender, CancelEventArgs e)
		{
			if (taskPage.Visibility == Visibility.Visible)
			{
				if (taskPage.IsRuning())
				{
					e.Cancel = true;
					MessageBox.Show("任务正在进行...","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
					return;
				}
			}
		}


		//private void UpdateUI()
		//{
		//	btnDel.IsEnabled = lstBox1.SelectedItem != null;
		//}
		private void BtnDel_Click(IEnumerable<DLXX_XZDY> lst)
		{
			try
			{
				taskPage.RemoveAll();
				var task = new DeleteTask(lst);
				taskPage.AddTaskToPool(task);
				taskPage.AutoAjustColumnWidth();
				taskPage.Start();

				sbp.Visibility = Visibility.Hidden;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		//private void InitZoneBar()
		//{
		//	var repos = DlxxXzdyRepository.Instance;
		//	var dicZoneItem = new Dictionary<string, ZoneItem>();
		//	var lstEn=repos.FindAll(t => (int)t.JB <= (int)eZoneLevel.County && (int)t.JB >= (int)eZoneLevel.Group, (c, t) => c(t.JB, t.MC, t.BM,t.ID,t.SJID));
		//	lstEn.Sort((a, b) => a.JB > b.JB ? -1 :a.BM.CompareTo(b.BM));
		//	var fHasGroup = lstEn.Find(it => it.JB == eZoneLevel.Group) != null;
		//	foreach (var en in lstEn)
		//	{
		//		switch (en.JB)
		//		{
		//			case eZoneLevel.County:
		//				{
		//					var zone = new ZoneItem(en, null);
		//					_treeSoure.Add(zone);
		//					dicZoneItem[en.ID] = zone;
		//				}break;
		//			case eZoneLevel.Town:
		//				{
		//					if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
		//					{
		//						var zone = new ZoneItem(en, pZone,new List<CodeItem>());
		//						dicZoneItem[en.ID] = zone;
		//					}
		//				}break;
		//			case eZoneLevel.Village:
		//				{
		//					if (fHasGroup)
		//					{
		//						if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
		//						{
		//							var zone = new ZoneItem(en, pZone, new List<CodeItem>());
		//							dicZoneItem[en.ID] = zone;
		//						}
		//					}
		//					else
		//					{
		//						if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
		//						{
		//							pZone.Codes.Add(new CodeItem(en.BM, en.MC));
		//						}
		//					}
		//				}
		//				break;
		//			case eZoneLevel.Group:
		//				{
		//					if (dicZoneItem.TryGetValue(en.SJID, out var pZone))
		//					{
		//						pZone.Codes.Add(new CodeItem(en.BM, en.MC));
		//					}
		//				}
		//				break;
		//		}
		//	}
		//}

		//private void OnZoneChanged()
		//{
		//	var it1 = treeView.SelectedItem as ZoneItem;
		//	try
		//	{
		//		var lst = lstBox1.ItemsSource as ObservableCollection<CodeItem>;
		//		lst.Clear();
		//		if (it1.Codes != null)
		//		{
		//			lst.AddRange(it1.Codes);
		//		}
		//		//else
		//		//{
		//		//	foreach (var c in it1.Children)
		//		//	{
		//		//		lst.AddRange(c.ListCunBm);
		//		//	}
		//		//}
		//		var str = "操作";
		//		if (lst.Count > 0)
		//		{
		//			str += $"（{lst.Count}）";
		//		}
		//		tbOperate.Text = str;
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//	}
		//}
	}

}
