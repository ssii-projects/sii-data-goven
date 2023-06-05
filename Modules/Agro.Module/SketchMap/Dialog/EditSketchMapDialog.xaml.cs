using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace Agro.Module.SketchMap
{
    public partial class EditSketchMapDialog :UserControl, IDisposable
    {
		class Item
		{
			private VEC_CBDK? _en;
			private bool _fHasDK = true;
			public string DKBM { get { return _land.DKBM; } }
			internal VEC_CBDK? Entity
			{
				get
				{
					if (!_fHasDK)
					{
						return null;
					}
					if (_en == null)
					{
						_en = SketchMapUtil.QueryDKByDkbm(_land);// DKBM);
						if (_en == null)
						{
							_fHasDK = false;
						}
						//else
						//{
						//	if (_land.DKDZ != null) _en.DKDZ = _land.DKDZ;
						//	if (_land.DKNZ != null) _en.DKNZ = _land.DKNZ;
						//	if (_land.DKXZ != null) _en.DKXZ = _land.DKXZ;
						//	if (_land.DKBZ != null) _en.DKBZ = _land.DKBZ;
						//}
					}
					return _en;
				}
			}

			private readonly ContractLand _land;
			public Item(ContractLand land)//string dkbm)
			{
				//DKBM = land.DKBM;// dkbm;
				_land = land;
			}
		}
		#region Properties

		/// <summary>
		/// 当前地块
		/// </summary>
		Item? AgriLand { get
			{
				return lstLand.SelectedItem as Item;
			}
		}
		//public AgricultureLandRepertory AgriLand { get; set; }

		///// <summary>
		///// 地块数组
		///// </summary>
		//public readonly List<AgricultureLandRepertory> LandArray;//{ get; set; }

		/// <summary>
		/// 坐标系
		/// </summary>
		//public SpatialReference Reference { set; get; }

		#endregion

		private readonly SpatialReference _spatialReference;
		private readonly string RootPath;
		/// <summary>
		/// 当前承包方
		/// </summary>
		private ContractConcord Concord;
		private readonly ExportSketchMapTask _task;
		public EditSketchMapDialog(DataTaskNavigater nav, ExportSketchMapTask task)
        {
			_task = task;
			_spatialReference = _task._spatialReference;
			RootPath = MainPage.TaskOutputPath(task);
			Concord = nav.Concord;

			InitializeComponent();
			DataContext = this;

			var lstDkbm = new List<Item>();
			foreach (var land in nav.Concord.Lands)
			{

				lstDkbm.Add(new Item(land));
			}
			lstLand.ItemsSource = lstDkbm;
			DependencyObjectUtilEx.FindCommandButton(toolbar, cb =>BindMapBuddy(cb));
			btnSelectTool.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			this.Loaded += (s, e) => lstLand.SelectedIndex = 0;
		}
		public void ShowDialog(Window owner)
		{
			var dlg = new KuiDialog(owner, "编辑示意图")
			{
				Content=this
			};
			
			dlg.BtnOK.Click += (s, e) => dlg.Close();
			dlg.ShowDialog();
		}

		private void BindMapBuddy(IMapBuddy buddyControl)
		{
			var map = pageLayout;
			buddyControl.MapHost = map;
			if (buddyControl is IDisposable d)
			{
				map.OnDispose += () =>d.Dispose();
			}
		}

		///// <summary>
		///// 初始化地块数据
		///// </summary>
		//private void InitalizeLandData()
		//{
		//	var pl = pageLayout.ActiveView as IPageLayout;
		//	string tmplFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\地块示意图\地块四至示意图.kpd";
		//	pl.OpenDocument(tmplFile, false);
		//}

		/// <summary>
		/// 销毁
		/// </summary>
		public void Dispose()
        {
			pageLayout.Dispose();
            GC.Collect();
        }


		private string ImagePath(Item it,string ext=".jpg")
		{
			var path = RootPath;
			if (!(path.EndsWith("/") || path.EndsWith("\\"))){
				path += "\\";
			}
			path += Concord.CBFMC + @"\Jpeg\" + it.DKBM + ext;// ".jpg";
			return path;
		}
        #region Events

        /// <summary>
        /// 复制到剪切板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				string fileName = ImagePath(AgriLand);
				pageLayout.SaveToImage(fileName, 300, i => { }, null);
				var image =LibCore.ImageUtil.LoadImageFromFile(fileName);

				//var image = localView.SaveToImage(1);
				Clipboard.SetImage(image);
            }
            catch (Exception ex)
            {
				UIHelper.ShowExceptionMessage(ex);
            }
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (AgriLand == null)
            {
                return;
            }
            try
            {
				string fileName = ImagePath(AgriLand);

				var path = FileUtil.GetFilePath(fileName);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
				pageLayout.SaveToImage(fileName, 300, i => { }, null);

				fileName = ImagePath(AgriLand, ".kpt");
				pageLayout.SaveToXml(fileName);
            }
            catch (Exception ex)
            {
				UIHelper.ShowExceptionMessage(ex);
            }
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //var list = localView.SelectedItems.
            //    Where(c => c != localView.Paper && (c.Tag is bool && !((bool)c.Tag) || !(c.Tag is bool))).
            //    ToList();
            //list.ForEach(c =>
            //{
            //    c.IsSelected = false;
            //    localView.Items.Remove(c);
            //    c.Dispose();
            //});
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
        }

        /// <summary>
        /// 地块选择改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			try
			{
				var land = lstLand.SelectedItem as Item;
				var en = land.Entity;
				if (en?.Shape?.Geometry == null)
				{
					pageLayout.Visibility = Visibility.Collapsed;
					return;
				}
				if (pageLayout.Visibility != Visibility.Visible)
				{
					pageLayout.Visibility = Visibility.Visible;
				}
				tabNeighbor.Text = $"四至：东至：{ en.DKDZ} 南至：{en.DKNZ} 西至：{en.DKXZ} 北至：{en.DKBZ}";

				var pl = pageLayout.ActiveView as IPageLayout;
				var fileName = ImagePath(land, ".kpt");
				if (!File.Exists(fileName)) {
					fileName = null;
				}
				var fUpdateSz = fileName == null;

				var lands = SketchMapUtil.QueryNeareastDK(en);
				SketchMapUtil.InitalizeOwnerView(pl, _spatialReference, en, lands, fileName, fUpdateSz);
				pl.ZoomToWhole();
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}

		#endregion
	}
}
