using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.GIS;
using Agro.GIS.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using GeoAPI.Geometries;
using System.Drawing;
using System.IO;
using Agro.Module.DataExchange.LandDotCoil;
using Agro.Library.Model;
using Agro.LibCore.Database;
using Agro.Module.DataExchange.Entities;
using Agro.Module.DataExchange.Repository;
using Agro.Library.Common.Repository;
using System.Linq;
using Agro.Module.DataExchange;
using Agro.LibCore.GIS;

namespace Agro.Module.DataOperator
{
	/// <summary>
	/// MapPage.xaml 的交互逻辑
	/// </summary>
	public partial class MapPage : UserControl, IRequestClosing, IDisposable
	{
		class MapPageImpl :MapPageImplBase
		{
			private readonly MapPage p;
			public MapPageImpl(MapPage p)
			{
				this.p = p;
				FinalConstruct();
			}
			public override CreateFeatureToolButton BtnCreateFeatureTool => p.btnCreateFeatureTool;
			public override ModifyFeatureToolButton ModifyFeatureTool => p.btnModifyFeatureTool;
			public override CutFeatureToolButton BtnCutFeatureTool => p.btnCutFeatureTool;
			public override AutoCompletePolygonToolButton BtnAutoComplete => p.btnAutoComplete;
			public override ReshapePolygonEdgeToolButton BtnReshape => p.btnReshape;
			public override MapPanToolButton BtnPan => p.btnPan;
			public override IMapControl MapControl => p.mapControl;
			public override SidebarPage SidePage => p.sidePage;
			public override IdentifyToolButton BtnIdentify => p.btnIdentify;
		}
		class LayerPanelContextMenu
		{
			private readonly MapPage _p;
			private readonly ContextMenu _layerMenu;
			public LayerPanelContextMenu(MapPage p, AppType appType)
			{
				_p = p;
				_layerMenu = p.sidePage.ContextMenu;
				var map = p.mapControl.FocusMap;
				InitContextMenu(appType);
				p.miExportShapeFile.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					ExportToShapefileDialog.ShowDialog(Window.GetWindow(p), fl);
					//var dlg=new KuiDialog(p,"导出为ShapeFile文件")
				};
				p.miExportLayerFile.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					fl.SaveLayerAs(null);
				};
				p.miExportCoords.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					//fl.SaveLayerAs(null);
					var pnl = new ExportCoordsPanel();
					var dlg = new KuiDialog(Window.GetWindow(p), "导出坐标")
					{
						Content = pnl,
						Height = 300,
					};
					dlg.BtnOK.Click += (s1, e1) =>
					{
						var err = pnl.Export(fl, out int cnt);
						if (err != null)
						{
							MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
						else
						{
							MessageBox.Show("共导出" + cnt + "条数据");
							dlg.Close();
							try
							{
								System.Diagnostics.Process.Start("explorer.exe", pnl.OutPath);
							}
							catch { }
						}
					};
					dlg.ShowDialog();
				};

				p.miPropertyTable.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					ShowPropertyTable(fl);
				};
				p.miRemoveLayer.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					if (mi.Tag is ILayer fl)
					{
						var m = p.mapControl.FocusMap;
						m.Layers.RemoveLayer(fl);
						p.Toc.Refresh();
						m.Refresh();
					}
				};
				p.miLayerProperty.Click += (s, e) =>
				{
					if ((s as MenuItem).Tag is ILayer lyr)
					{
						var dlg = new LayerPropertyDialog();
						dlg.ShowProperty(lyr, new LayerPropertyDialog.ShowPropertyPrms()
						{
							OnApplied = () => { p.Toc.Refresh(); p.Map.Refresh(); }
						});
					}
				};
				p.miClearVisibleScale.Click += (s, e) =>
				{
					//var mi = s as MenuItem;
					// as IFeatureLayer;
					if ((s as MenuItem).Tag is ILayer lyr)
					{
						lyr.MaxVisibleScale = null;
						lyr.MinVisibleScale = null;
						map.RefreshLayer(lyr);
					}
				};
				p.miSetMaxVisibleScale.Click += (s, e) =>
				{
					var lyr = (s as MenuItem).Tag as ILayer;
					lyr.MaxVisibleScale = p.Map.MapScale;//.Scale;
					p.Map.RefreshLayer(lyr);
				};
				p.miSetMinVisibleScale.Click += (s, e) =>
				{
					var lyr = (s as MenuItem).Tag as ILayer;
					lyr.MinVisibleScale = p.Map.MapScale;//.Scale;
					p.Map.RefreshLayer(lyr);
				};
				p.miAppend.Click += (s, e) =>
				{
					var lyr = (s as MenuItem).Tag as FeatureLayer;
					p.AppendShapeFile(lyr, true);
				};
				p.miModify.Click += (s, e) =>
				{
					var lyr = (s as MenuItem).Tag as FeatureLayer;
					p.AppendShapeFile(lyr, false);
				};
				p.miUpload.Click += (s, e) =>
				{
					bool? fok;
					var lyr = ((MenuItem)s).Tag as FeatureLayer;
					var fc = lyr!.FeatureClass;
					if (fc is ShapeFileFeatureClass hfc)
					{
						hfc.Flush();
						var shpFile = $"{fc.ConnectionString}{fc.TableName}.shp";
						fok=ImportDkckTask.ShowDialog(Window.GetWindow(p), shpFile, eDatabaseType.ShapeFile);
					}
					else
					{
						fok = ImportDkckTask.ShowDialog(Window.GetWindow(p), fc.ConnectionString, eDatabaseType.SQLite);
					}
					if (fok == true)
					{
						p.Map.Invalidate();
						p.Map.Refresh();
					}
				};
				p.miExportJson.Click += (s, e) => ExportJsonTask.ShowDialog(Window.GetWindow(p));
			}
			private void InitContextMenu(AppType appType)
			{
				var p = _p;
				p.sidePage.ContextMenu = null;
				p.layersControl.TocControl.MouseRightButtonDown += (s, e) =>
				{
					var hi = p.layersControl.TocControl.HitTest(e.GetPosition(p.layersControl.TocControl));
					if (hi == null)
					{
						_layerMenu.PlacementTarget = null;
						_layerMenu.IsOpen = false;
						return;
					}
					var fl = hi.Layer as FeatureLayer;
					p.miPropertyTable.IsEnabled = fl != null;
					p.miExportShapeFile.IsEnabled = fl != null;
					p.miExportLayerFile.IsEnabled = fl != null;

					p.miClearVisibleScale.IsEnabled = hi.Layer.MinVisibleScale != null || hi.Layer.MaxVisibleScale != null;

					#region yxm 2019-3-6
					bool fEditableLayer = p.CanBeTargetLayer(hi.Layer);
					var vis = fEditableLayer ? Visibility.Visible : Visibility.Collapsed;
					p.miAppend.Visibility = fEditableLayer&&fl?.FeatureClass is ShapeFileFeatureClass?Visibility.Visible:Visibility.Collapsed;
					p.miModify.Visibility = p.miAppend.Visibility;
					p.miExportCoords.Visibility = vis;
					p.miUpload.Visibility =appType==AppType.DataGoven?vis:Visibility.Collapsed;
					p.miTargetLayerSep1.Visibility = vis;
					p.miTargetLayerSep2.Visibility = vis;
					#endregion

					#region yxm 2019-10-23 若为未登记地块则显示菜单“导出地块更新包”
					vis = hi.Layer.Tag is ELayerTagType et&&et==ELayerTagType.WDJ_DK ? Visibility.Visible : Visibility.Collapsed;
					p.miTargetLayerSep3.Visibility = vis;
					p.miExportJson.Visibility = vis;
					#endregion


					foreach (var it in _layerMenu.Items)
					{
						AttachTag(it as MenuItem, hi.Layer);
					}
					_layerMenu.PlacementTarget = p.layersControl.TocControl;
					_layerMenu.IsOpen = true;
					MenuUtil.ValidateSeparatorVisibility(_layerMenu.Items);
				};
			}
			private void AttachTag(MenuItem? mi, object tag)
			{
				if (mi != null)
				{
					mi.Tag = tag;
					if (mi.HasItems)
					{
						foreach (var mi1 in mi.Items)
						{
							AttachTag(mi1 as MenuItem, tag);
						}
					}
				}
			}
			private void ShowPropertyTable(FeatureLayer fl)
			{
				if (fl != null)
				{
					_p.mlTableView.ShowPropertyTable(fl);
				}
			}
		}
		class LayerTableViewPopupExtension
		{
			private readonly List<MenuItem> _menuItems = new List<MenuItem>();
			private TableView tableView;
			private IFeatureLayer featureLayer;
			private readonly DlxxDkRepository dkRepos = DlxxDkRepository.Instance;
			private readonly TxbgjlRepository bgjlRepos = TxbgjlRepository.Instance;

			private readonly MapPage _p;
			public LayerTableViewPopupExtension(MapPage p)
			{
				_p = p;


				#region 删除未登记地块
				{
					var menuItem = new MenuItem()
					{
						Header = "删除未登记地块"
					};
					menuItem.Click += (s, e) => DeleteSelectedDK();
					_menuItems.Add(menuItem);
				}
				#endregion

				#region 灭失未登记地块
				{
					var menuItem = new MenuItem()
					{
						Header = "灭失未登记地块"
					};
					menuItem.Click += (s, e) => MieshiSelectedDK();
					_menuItems.Add(menuItem);
				}
				#endregion

				#region 修改发包方编码
				{
					var menuItem = new MenuItem()
					{
						Header = "修改发包方编码"
					};
					menuItem.Click += (s, e) => ModifyFbfBM();
					_menuItems.Add(menuItem);
				}
				#endregion

				p.mlTableView.OnContextMenuPreOpen += prm =>
				{
					tableView = prm.TableView;
					featureLayer = prm.FeatureLayer;
					UpdateContextMenu(prm.ContextMenu, prm.FeatureLayer.Tag);
				};
			}
			private void UpdateContextMenu(ContextMenu cm,object layerTag) {
				bool fShowMenuItem = layerTag is ELayerTagType et&&et==ELayerTagType.WDJ_DK && tableView.Rows > 0;
				if (fShowMenuItem)
				{
					foreach (var menuItem in _menuItems)
					{
						if (!cm.Items.Contains(menuItem))
						{
							cm.Items.Add(menuItem);
						}
					}
				}
				else
				{
					foreach (var menuItem in _menuItems)
					{
						if (cm.Items.Contains(menuItem))
						{
							cm.Items.Remove(menuItem);
						}
					}
				}
			}
			void ModifyFbfBM()
			{
				DLXX_DK en = null;
				var row = tableView.ActiveRow;
				if (row >= 0 && row < tableView.Rows)
				{
					var oid = tableView.GetOIDByRow(row);
					en = dkRepos.Find(t => t.BSM == oid, DLXX_DK.GetSubFields(false));
					if (en == null)
					{
						MessageBox.Show("地块已不存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
				}
				if (en == null)
					return;
				var err = CanModifyFbfBM(en);
				if (err != null)
				{
					MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				var pnl = new SelectFbfPanel();
				var dlg = new KuiDialog(Window.GetWindow(_p), "选择发包方")
				{
					Width = 700,
					Content = pnl
				};
				dlg.BtnOK.Click += (s, e) =>
				{
					err = pnl.OnApply();
					if (err != null)
					{
						UIHelper.ShowError(dlg, err);
						return;
					}
					var ydkbm = en.DKBM;
					var yFBFBM = en.FBFBM;
					en.FBFBM = pnl.SelectedFbf.FbfBM;
					var msg = $"确定将地块(DKBM='{ydkbm}')的发包方编码由 {yFBFBM} 修改为 {en.FBFBM}吗？";
					var mr=MessageBox.Show(msg, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (mr == MessageBoxResult.Yes)
					{
						if (yFBFBM != en.FBFBM)
						{
							en.ZHXGSJ = DateTime.Now;
							Try.Catch(() =>
							{
								var xtpzLshRepository = XtpzLshRepository.Instance;
								var db = MyGlobal.Workspace;
								DBUtil.UseTransaction(db, () =>
								{
									en.DKBM = xtpzLshRepository.QueryNewDKBM(en.FBFBM);
									dkRepos.Update(en, t => t.BSM == en.BSM, (c, t) => c(t.FBFBM, t.ZHXGSJ, t.DKBM));
									db.ExecuteNonQuery($"update DLXX_DK_JZD set DKBM='{en.DKBM}' where DKBM='{ydkbm}'");
									db.ExecuteNonQuery($"update DLXX_DK_JZX set DKBM='{en.DKBM}' where DKBM='{ydkbm}'");
								});
								_p.Map.InvalidateLabelLayer();
								_p.Map.Refresh();
								tableView.ReQuery(tableView.FeatureLayer.UseWhere());
								row = tableView.GetRowByOid(en.BSM);
								if (row >= 0)
								{
									tableView.EnsureRowVisible(row);
									tableView.SetActiveRow(row);
								}
							});
						}
					}
					dlg.Close();
				};
				dlg.ShowDialog();
			}

			string CanModifyFbfBM(DLXX_DK en)
			{
				if (en.DJZT != EDjzt.Wdj)
					return "只有未登记地块才允许修改发包方！";
				return null;
			}
			/// <summary>
			/// 灭失未登记地块
			/// </summary>
			void MieshiSelectedDK()
			{
				DLXX_DK en = null;
				var row = tableView.ActiveRow;
				if (row >= 0 && row < tableView.Rows)
				{
					var oid = tableView.GetOIDByRow(row);
					en = dkRepos.Find(t => t.BSM == oid, DLXX_DK.GetSubFields(false));
					if (en == null)
					{
						MessageBox.Show("地块已不存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
				}
				if (en == null)
					return;
				var err = CanMieshi(en);
				if (err != null)
				{
					MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				var mbr = MessageBox.Show($"确定要灭失选择的地块（DKBM={en.DKBM},DKMC={en.DKMC}）吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
				if (mbr != MessageBoxResult.OK)
					return;

				Try.Catch(() =>
				{
					DBUtil.UseTransaction(MyGlobal.Workspace, () =>
					{
						en.ZT = EDKZT.Lishi;
						en.MSSJ = DateTime.Now;
						dkRepos.Update(en, t => t.BSM == en.BSM, (c, t) => c(t.ZT, t.MSSJ));
						var enBgjl = new DLXX_DK_TXBGJL()
						{
							BGFS=ETXBGLX.Mieshi,
							DKID=en.ID,
							DKBM=en.DKBM,
							YDKID=en.ID,
							YDKBM=en.DKBM,
							BGYY="地块灭失"
						};
						bgjlRepos.Insert(enBgjl);
					});

					_p.Map.InvalidateLayer(featureLayer);
					_p.Map.InvalidateLabelLayer();
					_p.Map.InvalidateSelectionLayer();
					_p.Map.Refresh();
				});
			}
			string CanMieshi(DLXX_DK en)
			{
				if (en.DJZT == EDjzt.Ydj)
					return "该地块状态为已登记，不允许灭失！";
				if (en.MSSJ != null)
				{
					return "该地块已灭失！";
				}
				if (en.SJLY != ESjly.Cs)
				{
					return "只有初始汇交导入的数据才允许灭失！";
				}
				//var lst = bgjlRepos.FindAll(t => t.YDKBM == en.DKBM);
				//if (lst != null && lst.Count > 0)
				//{
				//	return $"该地块已发生变更，对应的变更地块编码（DKBM）为：[{string.Join(",", lst.Select(it => it.DKBM))}]";
				//}
				return null;
			}

			void DeleteSelectedDK()
			{
				DLXX_DK en = null;
				var row=tableView.ActiveRow;
				if (row >= 0 && row < tableView.Rows)
				{
					var oid=tableView.GetOIDByRow(row);
					en=dkRepos.Find(t => t.BSM == oid,DLXX_DK.GetSubFields(false));
					if (en == null)
					{
						MessageBox.Show("地块已不存在！","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
						return;
					}
				}
				if (en == null)
					return;
				var err = CanDelete(en);
				if (err!=null)
				{
					MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				var mbr=MessageBox.Show($"确定要删除选择的地块（DKBM={en.DKBM},DKMC={en.DKMC}）吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
				if (mbr != MessageBoxResult.OK)
					return;

				Try.Catch(() =>
				{
					DBUtil.UseTransaction(MyGlobal.Workspace, () =>
					 {
						 dkRepos.Delete(t => t.BSM == en.BSM);
						 bgjlRepos.Delete(t => t.DKBM == en.DKBM || t.YDKBM == en.DKBM);
					 });
					
					_p.Map.InvalidateLayer(featureLayer);
					_p.Map.InvalidateLabelLayer();
					_p.Map.InvalidateSelectionLayer();
					_p.Map.Refresh();
					tableView.RemoveRow(row);
				});
			}
			string CanDelete(DLXX_DK en)
			{
				if (en.DJZT == EDjzt.Ydj)
					return "该地块状态为已登记，不允许删除！";
				if (en.MSSJ!=null)
				{
					return "该地块已灭失，不允许删除！";
				}
				if (en.SJLY == ESjly.Cs)
				{
					return "该地块为初始汇交数据导入，不允许删除！";
				}
				var lst = bgjlRepos.FindAll(t => t.YDKBM == en.DKBM);
				if (lst != null && lst.Count > 0)
				{
					return $"该地块已发生变更，对应的变更地块编码（DKBM）为：[{string.Join(",", lst.Select(it => it.DKBM))}]";
				}
				return null;
			}

		}
		private TocControl Toc
		{
			get { return layersControl.TocControl; }
		}

		private readonly FeatureVertexViewWrap _featureVertextView;


		public MapControl MapControl
		{
			get
			{
				return mapControl;
			}
		}
		internal GIS.Map Map
		{
			get { return mapControl.FocusMap; }
		}
		private Action OnBeforeDisposed;

		protected readonly IMapPageImpl _impl;

        public MapPage() : this(AppType.DataGoven)
		{

		}
        public MapPage(AppType appType)
		{
			InitializeComponent();
			_impl = new MapPageImpl(this);
			new LayerPanelContextMenu(this,appType);

			_featureVertextView = new FeatureVertexViewWrap(this.sidePage, this.MapControl);
			mlTableView.FinalConstruct(MapControl, sidePage, _featureVertextView);
			this.btnFeatureVertex.OnFeatureSelected += it => { _featureVertextView.Show(it.FeatureLayer, it.Feature); };

			new ToggleSideView(this.sidePage, btnLayer, null, (s, ov, nv) => {
				if (nv)
				{
					var sl = layersControl.TocControl.SelectedLayer;
					layersControl.TocControl.SelectedLayer = sl;
				}
			}).Panel = sidePage.LeftPanel;
			layersControl.OnAddDataButtonClick += MyMapUtil.OnAddDataButtonClick;
			new ToggleSideView(sidePage, btnTopoCheck, () => {
				var pnl = new TopoCheckSelectPanel(Map, layersControl.TocControl);
				pnl.OnViewMap += layer => btnLayer.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
				return pnl;
			});
			new ToggleSideView(this.sidePage, btnLayerSelection, () => { return new LayerSelectionPanel(Map); }, (s, ov, nv) => { if (s.Panel is LayerSelectionPanel lsp) lsp.Refresh(); });
			new ToggleSideView(this.sidePage, btnSearch, () => { return new MapSearchPanel(Map); });

			new ToggleSideView(this.sidePage, btnEdit, () => {
				var efp = new EditFeaturePanel(Map);
				efp.OnFilter += fl => { return CanBeTargetLayer(fl); };
				return efp;
			}, (s, ov, nv) => { if (s.Panel is EditFeaturePanel efp) efp.OnVisible(nv); });

			DependencyObjectUtilEx.FindCommandButton(toolbar, cb =>BindMapBuddy(cb));


			#region yxm 2018-3-8
			BindMapBuddy(btnClear);
			mapControl.TempElements.OnAddElement += me => { btnClear.Visibility = Visibility.Visible; };
			mapControl.TempElements.OnClear += () => btnClear.Visibility = Visibility.Collapsed;
			#endregion

			mapControl.CurrentTool = MapControl.CreatePanTool();

			Toc.MapHost = mapControl;

			msrSS.Map = Map;
			coordSS.Map = Map;
			msSS.Map = Map;

			sidePage.HidePanel(AnchorSide.Bottom);

			btnShapeFile.Click += (s, e) => BtnAddShapeFile_Click();
			btnDownload.Click += (s, e) => DownloadFbfDkTask.ShowDialog(mapControl, Toc);
			btnIdentify.Init(sidePage);

			if (appType == AppType.DataGoven)
			{
				new LayerTableViewPopupExtension(this);
			}
			btnSaveJpg.Click += (s, e) => Try.Catch(() => SaveJpg());
			btnBuildJzdJzx.Click += (s, e) => TestBuildJzdx();

			var wnd = MyGlobal.MainWindow;
			if (wnd != null)
			{
				wnd.Events.OnActivePageChanged += OnActivePageChanged;
			}

			Loaded += (s, e) =>
			  {
				  if (Map.Layers.LayerCount == 0)
				  {
					  LoadData();
				  }
			  };

			MyGlobal.OnDatasourceChanged += () => LoadData();
		}

		private void LoadData()
		{
			Try.Catch(() =>
			{
				Map.Layers.ClearLayers();
				Map.SuprresEvents = true;
				CreateDefaultLayers(this.Map);
			}, false);
			this.Toc.Refresh();
			Map.SuprresEvents = false;

			var t = new MyTask();
			OnBeforeDisposed += () => t.Cancel();
			t.Go(token =>
			{
				Try.Catch(() =>
				{
					OkEnvelope fullEnv = null;
					Map.Connect(fl =>
					{
						if (fl.Name == "村级区域")
						{
							string where = null;
							if (MyGlobal.LoginUser?.Buildin == false)
							{
								where = fl.UseWhere();
							}
							//fullEnv = fl.FeatureClass.GetFullExtent();
						}
						var env = fl.FeatureClass.GetFullExtent();
						if (env != null)
						{
							if (fullEnv == null)
							{
								fullEnv = env;
							}
							else
							{
								fullEnv.Union(env);
							}
						}
					});
					var fullEnv1 = GetFullExtent();
					if (fullEnv1 != null)
					{
						fullEnv = fullEnv1;
					}
					

					if (fullEnv != null)
					{
						Dispatcher.Invoke(() =>
						{
							Map.FullExtent = fullEnv;
							Map.SetExtent(Map.FullExtent);
							Map.UpdateCommandUI();
						});
					}
				});
			});

		}

		protected virtual OkEnvelope GetFullExtent()
		{
			var lyr = MapUtil.FindFeatureLayer(mapControl.FocusMap, fl => StringUtil.isEqualIgnorCase(fl.FeatureClass?.TableName, DLXX_XZDY.GetTableName()));
			if (lyr != null)
			{
				return lyr.FeatureClass.GetFullExtent();// FullExtent;
			}
			return null;
		}
		protected virtual void CreateDefaultLayers(GIS.Map map)
		{
			var wfc = MyGlobal.Workspace;
			#region 管辖区域
			{
				map.Layers.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_XZDY.GetTableName(), "村级区域")
				{
					Where = "JB = 2",
					LabelExpr = "[MC]",
					Selectable = false,
					OnBeforeUseWhere = LayerWhereUtil.OnXzdyBeforeUseWhere
				}, SymbolUtil.CreateSimpleFillSymbol("#EEEAE0BD", "#FFFF0000")));

				map.Layers.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_XZDY.GetTableName(), "组级区域")
				{
					Where = "JB=1",
					LabelExpr = "[MC]",
					Selectable = false,
					MinVisibleScale = 30,
					MaxVisibleScale = 20000,
					OnBeforeUseWhere = LayerWhereUtil.OnXzdyBeforeUseWhere
				}, SymbolUtil.CreateSimpleFillSymbol("#00EFEBDB", "#FFC4BFBD")));
			}
			#endregion

			#region 地块类别
			{
				map.Layers.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_DK.GetTableName(), "地块")
				{
					LabelExpr = "StrContains([CBFMC],'/')?\"共有地块\";[CBFMC]",
					MaxVisibleScale = 50000,
					OnBeforeUseWhere = wh =>
					{
						var s = LayerWhereUtil.OnDKBeforeUseWhere(wh);
						wh = WhereExpression<DLXX_DK>.Where(t => t.ZT == EDKZT.Youxiao && t.MSSJ == null && t.DJZT == EDjzt.Ydj);//  "ZT=1";
						return string.IsNullOrEmpty(s) ? wh : $"{wh} and {s}";
					}
				},SymbolUtil.CreateSimpleFillSymbol("#FFCCE1A0", "#FF728944")));

				map.Layers.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_DK.GetTableName(), "未登记地块")
				{
					LabelExpr = "StrContains([CBFMC],'/')?\"共有地块\";[CBFMC]",
					MaxVisibleScale = 50000,
					LayerTag = ELayerTagType.WDJ_DK,
					OnBeforeUseWhere = wh =>
					{
						var s = LayerWhereUtil.OnDKBeforeUseWhere(wh);
						// $"DJZT<>{(int)EDjzt.Ydj} and MSSJ is null";
						wh = WhereExpression<DLXX_DK>.Where(t => t.MSSJ == null && t.DJZT == EDjzt.Wdj);
						return string.IsNullOrEmpty(s) ? wh : $"{wh} and {s}";
					}
				},SymbolUtil.CreateSimpleFillSymbol("#A3FFBEFF", "#FF728944")));
			}
			#endregion

			#region 界址数据
			//if (false)
			//{
			//	var groupLayer = new GroupLayer(map, "界址数据");
			//	layers.Add(groupLayer);
			//	var layer = CreateLineFeatureLayer(wfc, map, "DLXX_DK_JZX", "界址线"
			//		, "#FF9E9470", 1, null, null, null, 8000);
			//	(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
			//	groupLayer.AddLayer(layer);

			//	layer = CreatePointFeatureLayer(wfc, map, "DLXX_DK_JZD", "界址点"
			//		, "#FFCCE1A0", "#FF728944", 6);
			//	(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
			//	layer.MaxVisibleScale = 8000;
			//	groupLayer.AddLayer(layer);
			//}
			#endregion
			//}
		}

		#region 测试：生成界址点和界址线
		private IFeatureLayer _memJzdLayer;
		private IFeatureLayer _memJzxLayer;
		/// <summary>
		/// yxm 2019-4-24
		/// 测试：生成界址点和界址线
		/// </summary>
		private void TestBuildJzdx()
		{
			var db = MyGlobal.Workspace;

			IFeatureLayer selLayer = null;
			IFeature selFeature = null;
			MapUtil.EnumFeatureLayer(Map, fl =>
			{
				if (CanBeTargetLayer(fl))
				{
					if (fl.SelectionSet.Count == 1)
					{
						selLayer = fl;
						fl.SelectionSet.Get(oid =>
						{
							selFeature = fl.FeatureClass.GetFeatue(oid);
							return false;
						});
						return false;
					}
				}
				return true;
			});

			if (selFeature == null)
			{
				UIHelper.ShowWarning(Window.GetWindow(this), "无选中地块");
				return;
			}

			var param = new DotCoilBuilderParam();
			var t = new DotCoilBuilder(param);
			//var param = new  InitLandDotCoilParam();


			if (_memJzdLayer == null)
			{
				using (var ws = MemoryWorkspaceFactory.Instance.OpenWorkspace("Temp"))
				{
					var tableName = Guid.NewGuid().ToString();
					ws.CreateFeatureClass(tableName, MemFeatureClassUtil.CreateJzdFields(), 0);
					_memJzdLayer = new FeatureLayer(this.Map)
					{
						Name = "Mem Jzd",
						FeatureClass = ws.OpenFeatureClass(tableName)
					};
					Map.Layers.AddLayer(_memJzdLayer);

					tableName = Guid.NewGuid().ToString();
					ws.CreateFeatureClass(tableName, MemFeatureClassUtil.CreateJzxFields(), 0);
					_memJzxLayer = new FeatureLayer(this.Map)
					{
						Name = "Mem Jzx",
						FeatureClass = ws.OpenFeatureClass(tableName)
					};
					Map.Layers.AddLayer(_memJzxLayer);
				}
			}
			else
			{
				_memJzdLayer.FeatureClass.ClearAll();
				_memJzxLayer.FeatureClass.ClearAll();
			}

			param.Dk.OriDkbm = SafeConvertAux.ToStr(IRowUtil.GetRowValue(selFeature, "DKBM"));
			param.Dk.ZjrXm = SafeConvertAux.ToStr(IRowUtil.GetRowValue(selFeature, "ZJRXM"));
			param.Dk.CbfMc = SafeConvertAux.ToStr(IRowUtil.GetRowValue(selFeature, "CBFMC"));
			param.Dk.Shape = selFeature.Shape.Clone() as IGeometry;


			#region intersect query from DLXX_DK
			using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK"))
			{
				var qf = new SpatialFilter()
				{
					SubFields = "DKBM,ZJRXM,CBFMC," + fc.ShapeFieldName,
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects,
					WhereClause = "ZT =1"
				};
				var env = selFeature.Shape.EnvelopeInternal.Clone();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geometry = GeometryUtil.MakePolygon(env);
					fc.SpatialQuery(qf, ft1 =>
					{
						if (ft1.Shape is IGeometry pt)
						{
							var sDKBM = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "DKBM"));
							if (sDKBM != param.Dk.OriDkbm)
							{
								var sZJRXM = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "ZJRXM"));
								var sCBFMC = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "CBFMC"));
								param.AddAroundDk(pt, sZJRXM, sCBFMC);
							}
						}
					});
				}
			}
			#endregion

			#region intersect query from DLXX_DK_JZD
			using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK_JZD"))
			{
				var qf = new SpatialFilter()
				{
					SubFields = "JZDH," + fc.ShapeFieldName,
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects
				};
				var env = selFeature.Shape.EnvelopeInternal.Clone();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geometry = GeometryUtil.MakePolygon(env);
					fc.SpatialQuery(qf, ft1 =>
					{
						if (ft1.Shape is IPoint pt)
						{
							var jzdh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "JZDH"));
							param.AddJzd(new Coordinate(pt.X, pt.Y), jzdh);
						}
					});
				}
			}
			#endregion

			#region intersect query from DLXX_DK_JZX
			using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK_JZX"))
			{
				var qf = new SpatialFilter()
				{
					SubFields = "JZXH,PLDWZJR,PLDWQLR," + fc.ShapeFieldName,
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects
				};
				var env = selFeature.Shape.EnvelopeInternal.Clone();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geometry = GeometryUtil.MakePolygon(env);
					fc.SpatialQuery(qf, ft1 =>
					{
						if (ft1.Shape is ILineString pt)
						{
							var jzxh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "JZXH"));
							var pldwZjr = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "PLDWZJR"));
							var pldwQlr = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "PLDWQLR"));
							var qjzdh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "QJZDH"));
							var zjzdh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "ZJZDH"));
							param.AddJzx(pt, jzxh, pldwZjr, pldwQlr, qjzdh, zjzdh);
						}
					});
				}
			}
			#endregion


			var res = t.Build();

			int nMaxJzdh = SafeConvertAux.ToInt32(db.QueryOne("select max(CAST(SUBSTRING(jzdh,2,LEN(jzdh)-1) as int)) from DLXX_DK_JZD where jzdh is not null and len(jzdh)>1"));
			int nMaxJzxh = SafeConvertAux.ToInt32(db.QueryOne("select max(cast(jzxh as int)) from DLXX_DK_JZX"));
			var jzdFc = _memJzdLayer.FeatureClass;
			foreach (var j in res.lstJzd)
			{
				if (string.IsNullOrEmpty(j.Jzdh))
				{
					j.Jzdh = $"T{++nMaxJzdh}";
				}

				var ft = jzdFc.CreateFeature();
				ft.Shape = GeometryUtil.MakePoint(j.Shape);
				IRowUtil.SetRowValue(ft, "JZDH", j.Jzdh);
				IRowUtil.SetRowValue(ft, "DKBM", param.Dk.Dkbm);
				IRowUtil.SetRowValue(ft, "JZDLX", "3");
				IRowUtil.SetRowValue(ft, "JBLX", "6");
				IRowUtil.SetRowValue(ft, "YSDM", "211021");
				jzdFc.Append(ft);
			}

			var jzxFc = _memJzxLayer.FeatureClass;
			foreach (var j in res.lstJzx)
			{
				var ft = jzxFc.CreateFeature();
				ft.Shape = j.Shape;

				if (string.IsNullOrEmpty(j.Jzxh))
				{
					j.Jzxh = (++nMaxJzxh).ToString();
				}

				//IRowUtil.SetRowValue(ft, "QJZDH", j.Qjzd.Jzdh);
				//IRowUtil.SetRowValue(ft, "ZJZDH", j.Zjzd.Jzdh);

				var qJzdh = string.IsNullOrEmpty(j.sQjzd) ? j.Qjzd.Jzdh : j.sQjzd;
				var zJzdh = string.IsNullOrEmpty(j.sZjzd) ? j.Zjzd.Jzdh : j.sZjzd;
				IRowUtil.SetRowValue(ft, "QJZDH", qJzdh);
				IRowUtil.SetRowValue(ft, "ZJZDH", zJzdh);

				IRowUtil.SetRowValue(ft, "JZXH", j.Jzxh);
				IRowUtil.SetRowValue(ft, "PLDWZJR", j.PldwZjr);
				IRowUtil.SetRowValue(ft, "PLDWQLR", j.PldwQlr);
				IRowUtil.SetRowValue(ft, "YSDM", "211031");
				IRowUtil.SetRowValue(ft, "JXXZ", "600009");
				IRowUtil.SetRowValue(ft, "JZXLB", "08");
				IRowUtil.SetRowValue(ft, "JZXWZ", "2");

				jzxFc.Append(ft);
			}

			Map.InvalidateLayer(_memJzdLayer);
			Map.InvalidateLayer(_memJzxLayer);
			Map.InvalidateSelectionLayer();
			Map.Refresh();
		}

		#endregion
		private void SaveJpg()
		{
			var dlg = new SaveFileDialog()
			{
				//openFileDialog.InitialDirectory="c:";//注意这里写路径时要用c:而不是c:　
				Filter = "Jpge件(*.jpg)|*.jpg",
				//RestoreDirectory = true,
				FilterIndex = 1,
				OverwritePrompt = true,
				//Multiselect = true,
			};
			if (dlg.ShowDialog() != true)
			{
				return;
			}
			var cvWrapper = sidePage.RightPanel as FeaturePropertyPanel;
			try
			{
				if (File.Exists(dlg.FileName))
				{
					File.Delete(dlg.FileName);
				}
				float dpi = 120;
				using (var bmp = new Bitmap(100, 100))
				{
					dpi = bmp.HorizontalResolution;
				}

				using (var bmp1 = LibCore.ImageUtil.ToBitmap(mapControl, mapControl.ActualWidth, mapControl.ActualHeight, dpi))
				{
					Bitmap bmp2 = null;
					#region render rightPanel
					if (sidePage.RightPanel?.Visibility == Visibility.Visible)
					{
						cvWrapper.SaveButtonVisible=false;
						bmp2 = LibCore.ImageUtil.ToBitmap(cvWrapper, cvWrapper.ActualWidth, cvWrapper.ActualHeight, dpi);
					}
					#endregion
					using (var bmp = new Bitmap(bmp1.Width + (bmp2 == null ? 0 : bmp2.Width), bmp1.Height))
					{
						using (var g = LibCore.ImageUtil.CreateGraphics(bmp))
						{
							g.Clear(System.Drawing.Color.White);
							g.DrawImage(bmp1, 0, 0);
							if (bmp2 != null)
							{
								g.DrawImage(bmp2, bmp1.Width, 0);
								bmp2.Dispose();
							}
						}
						bmp.Save(dlg.FileName);
					}
				}
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
			finally
			{
				if (cvWrapper != null)
				{
					cvWrapper.SaveButtonVisible=true;
				}
			}
		}

		private bool CanBeTargetLayer(ILayer layer)
		{
			return layer is IFeatureLayer fl && fl.Tag is ELayerTagType lt && lt == ELayerTagType.DC_DK;
		}
		private void AppendShapeFile(FeatureLayer fl, bool fAppend)
		{
			var pnl = new AppendShapePanel(fl);
			var dlg = new KuiDialog(Window.GetWindow(this), fAppend ? "追加数据" : "修改图形")
			{
				Content = pnl,
				Height = 300,
			};
			pnl.OnComplete = msg =>
			{
				dlg.Close();
				MessageBox.Show(msg, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
			};
			pnl.OnError = msg =>
			{
				dlg.BtnOK.IsEnabled = true;
				MessageBox.Show(msg, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			};
			dlg.BtnOK.Click += (s, e) => pnl.Run(fAppend);
			dlg.Closing += (s, e) =>
			{
				if (pnl.IsRunning)
				{
					e.Cancel = true;
				}
			};
			dlg.ShowDialog();
		}

		private void BindMapBuddy(IMapBuddy buddyControl)
		{
			buddyControl.MapHost = mapControl;
			if (buddyControl is IDisposable d)
			{
				Map.OnDispose += () =>d.Dispose();
			}
		}
		private void OnActivePageChanged(LibCore.UI.TabItem oti, LibCore.UI.TabItem nti)
		{
			try
			{
				if (oti != null)
				{
					var p = MyGlobal.MainWindow.GetPage(oti);
					if (p?.Page == this)
					{
						if (this.Map.Editor.TargetLayer?.FeatureClass is ShapeFileFeatureClass sfc)
						{
							sfc.Flush();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Agro.Module.DataOperator.MainPage::" + ex.Message);
			}
		}

		private void BtnAddShapeFile_Click()
		{
			try
			{
				var map = mapControl.FocusMap;
				bool fSetFullExtent = map.Layers.LayerCount == 0;
				var dlg = new OpenFileDialog()
				{
					//openFileDialog.InitialDirectory="c:";//注意这里写路径时要用c:而不是c:　
					Filter = "Shape文件(*.shp)|*.shp|移动调查交换包|*.dk",
					//RestoreDirectory = true,
					FilterIndex = 1,
					//Multiselect = true,
				};
				if (dlg.ShowDialog() != true)
				{
					return;
				}

				var dbType = dlg.FilterIndex == 1 ? eDatabaseType.ShapeFile : eDatabaseType.SQLite;
				AddShapeCommand.AddShapeFile(mapControl, Toc, dlg.FileName, dbType);
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}

		#region IRequestClosing
		public bool RequestClosing()
		{
			if (Map.IsDocumentDirty)
			{
				var mr = MessageBox.Show("文档已经发生改变，是否保存", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (mr == MessageBoxResult.Cancel)
				{
					return false;
				}
				else if (mr == MessageBoxResult.Yes)
				{
					Map.SaveDocument();
				}
			}
			return true;
		}
		#endregion

		public virtual void Dispose()
		{
			OnBeforeDisposed?.Invoke();
			Map.Dispose();
		}
	}

	class AddShapeCommand
	{
		public static void AddShapeFile(MapControl mc, TocControl toc, string fileName, eDatabaseType dbType, bool fCheckFile = true)
		{
			var fc = OuterDcdkRepository.OpenSrcFeatureClass(dbType, fileName, fCheckFile);

			#region 修改字段别名
			var lst = VEC_SURVEY_DK.GetAttributes();
			for (int i = 0; i < fc.Fields.FieldCount; ++i)
			{
				var field = fc.Fields.GetField(i);
				var it = lst.Find(t => StringUtil.isEqualIgnorCase(t.FieldName, field.FieldName));
				if (it != null)
				{
					field.AliasName = it.AliasName;
				}
			}
			#endregion

			//var fbfBm = OuterDcdkRepository.GetFbfbm(fc);
			var map = mc.FocusMap;
			var fl = new FeatureLayer(map)
			{
				FeatureClass = fc
			};
			if (fl.Renderer is SimpleFeatureRenderer sfr)
			{
				var lineSymbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString("#FF728944"), 1) as ILineSymbol;
				var symbol = SymbolUtil.CreateSimpleFillSymbol(ColorUtil.ConvertFromString("#A0B0CFFF"), lineSymbol);
				sfr.SetSymbol(symbol);
			}

			fl.Tag = ELayerTagType.DC_DK;// new MapPage.LayerTagTarget() { Fbfbm = fbfBm };

			map.Layers.AddLayer(fl);

			toc.Refresh();
			var extent = fc.GetFullExtent();
			if (map.FullExtent == null)
			{
				map.FullExtent = extent;
			}
			map.SetExtent(extent, true);
		}
	}
	public class RepackButton : CommandButton
	{
		public RepackButton()
		{
			Header = "压缩";
			ToolTip = "对编辑图层的数据进行压缩";
			//Image = MyImageSourceUtil.Image32("压缩.png");
			base.OnMapPropertyChanged += this.MapPropertyChanged;
			Click += (s, e) =>
			{
				if (Map != null)
				{
					Repack();
				}
			};
			UpdateCommandUI();
		}
		protected void MapPropertyChanged(IMapControl oldMap1)
		{
			var oldMap = oldMap1 == null ? null : oldMap1.ActiveView.FocusMap as GIS.Map;
			if (oldMap != null)
			{
				oldMap1.OnUpdateCommandUI -= UpdateCommandUI;
			}
			if (Map != null)
			{
				MapHost.OnUpdateCommandUI += UpdateCommandUI;
			}
		}
		private void UpdateCommandUI()
		{
			var tl = Map?.Editor?.TargetLayer;
			IsEnabled = tl != null && tl.FeatureClass != null && tl.FeatureClass is ShapeFileFeatureClass;
		}

		private void Repack()
		{
			try
			{
				var tl = Map.Editor.TargetLayer;
				if (tl != null && tl.FeatureClass != null && tl.FeatureClass is ShapeFileFeatureClass sfc)
				{
					sfc.Repack();
					Map.Editor.Operations.Clear();
				}
				MessageBox.Show("压缩完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}
	}


	public class MapPageShapeFileSource : MapPage
	{
		private readonly IFeatureClass _shpFc;
		public MapPageShapeFileSource(IFeatureClass shpFc)
			:base(AppType.DataOperator_ShapeFile)
		{
			_shpFc = shpFc;
			btnDownload.Visibility = Visibility.Collapsed;
			btnShapeFile.Visibility = Visibility.Collapsed;
			btnEdit.Visibility = Visibility.Collapsed;
		}
		protected override void CreateDefaultLayers(GIS.Map map)
		{
			var fc = _shpFc;
			var fl=map.Layers.AddLayer(MapDocUtil.CreateFeatureLayer(fc.Workspace as IFeatureWorkspace, map, new FeatureLayerMeta(fc.TableName)
			{
				LayerTag = ELayerTagType.DC_DK
			},SymbolUtil.CreateSimpleFillSymbol("#A0B0CFFF","#FF728944"))) as IFeatureLayer;
			fl.FeatureClass = fc;
			map.Editor.TargetLayer = fl;
		}
		protected override OkEnvelope GetFullExtent()
		{
			return Map.Editor.TargetLayer.FullExtent;
		}
		public override void Dispose()
		{
			_shpFc.Dispose();
			base.Dispose();
		}
	}
	public class MapPageSQLiteSource : MapPage
	{
		private readonly IFeatureWorkspace _sqliteDB;

		public MapPageSQLiteSource(IFeatureWorkspace sqliteDB)
			:base(AppType.DataOperator_SQLite)
		{
			_sqliteDB = sqliteDB;
			btnDownload.Visibility = Visibility.Collapsed;
			btnShapeFile.Visibility = Visibility.Collapsed;
			btnEdit.Visibility = Visibility.Collapsed;
			btnRepack.Visibility = Visibility.Collapsed;
			btnLayerSelection.Visibility = Visibility.Collapsed;
			btnCbf.Visibility = Visibility.Visible;
			btnMergeFeatureTool.Visibility = Visibility.Visible;
			btnReshape.Visibility = Visibility.Visible;

			//btnMergeFeatureTool.OnPreStore += OnPreMerge;

			new ToggleSideView(sidePage, btnCbf, () =>
			{
				var lyr = MapUtil.FindFeatureLayer(mapControl.FocusMap, it => it.FeatureClass?.TableName == VEC_SURVEY_DK.GetTableName());
				var pnl = new CbfListPanel(sqliteDB,_impl.GetListFbfdm(lyr));
				pnl.btnCloseRight.Click += (s, e) => sidePage.RightPanel = null;
				return pnl;
			}, (s, ov, nv) => { }, AnchorSide.Right);
			new MergeDkToolHook(this);
		}

		protected override void CreateDefaultLayers(GIS.Map map)
		{
			var wfc = _sqliteDB;
			#region 管辖区域
			{
				var gl = new GroupLayer(map, "行政区域");
				map.Layers.AddLayer(gl);
				//gl.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_XZDY.GetTableName())
				//{
				//	LayerName = "乡级区域",
				//	Where = $"JB=3",
				//	LabelExpr = "[MC]",
				//	Selectable = false
				//},SymbolUtil.CreateSimpleFillSymbol("#00000000", "#FFFF0000")));
				gl.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_XZDY.GetTableName())
				{
					LayerName = "村级区域",
					Where = $"JB=2",
					LabelExpr = "[MC]",
					LabelTextSymbol = SymbolUtil.MakeTextSymbol(12),
					Selectable = false
				}, SymbolUtil.CreateSimpleFillSymbol("#EEEAE0BD", "#FFFF0000")));
				gl.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DLXX_XZDY.GetTableName())
				{
					LayerName = "组级区域",
					Where = $"JB=1",
					LabelExpr = "[MC]",
					LabelTextSymbol = SymbolUtil.MakeTextSymbol(11),
					Selectable = false,
					//MinVisibleScale = 30,
					//MaxVisibleScale = 20000
				}, SymbolUtil.CreateSimpleFillSymbol("#00EFEBDB", "#FFC4BFBD")));
			}
			#endregion

			#region 地块类别
			{
				map.Layers.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(DC_DLXX_DK.GetTableName(), "原地块")
				{
					LayerTag = ELayerTagType.EXPORT_DK
				}, SymbolUtil.CreateSimpleFillSymbol("#FFCCE1A0", "#FF728944")));

				var gl = new GroupLayer(map, "调查地块")
				{
					IsExpanded = true
				};
				map.Layers.AddLayer(gl);
				map.Editor.TargetLayer = gl.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(VEC_SURVEY_DK.GetTableName(), "调查地块")
				{
					LabelExpr = @"[CBFMC] \ [SCMJM]",
					LabelTextSymbol = SymbolUtil.MakeTextSymbol(10),
					LayerTag = ELayerTagType.DC_DK
				}, SymbolUtil.CreateSimpleFillSymbol("#A3FFBEFF", "#FF728944"))) as IFeatureLayer;

				gl.AddLayer(MapDocUtil.CreateFeatureLayer(wfc, map, new FeatureLayerMeta(VEC_SURVEY_DK.GetTableName(), "变更标记")
				{
					Where = $"xgsj is not null",
					Selectable = false,
					LayerTag = ELayerTagType.BGBJ
				}, SymbolUtil.CreateSimpleFillSymbol("#AC97DBF2", "#FF728944")));
			}
			#endregion
		}

		public override void Dispose()
		{
			_sqliteDB.Dispose();
			base.Dispose();
		}
	}
}
