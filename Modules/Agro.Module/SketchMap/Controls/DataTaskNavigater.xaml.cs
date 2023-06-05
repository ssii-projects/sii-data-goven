using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using Agro.Library.Common;
using Agro.LibCore;
using CbfItem = Agro.Module.SketchMap.TaskSketchMapArgumet.CbfItem;
namespace Agro.Module.SketchMap
{
    /// <summary>
    /// TreeViewNavigater.xaml 的交互逻辑
    /// </summary>
    public partial class DataTaskNavigater : UserControl
    {
        #region Delegate

        public delegate void ContractorChangedEventHandler(ContractConcord concord);

        public event ContractorChangedEventHandler ConcordChanged;

        public delegate void AgricultureLandChangedEventHandler(ContractLand land);

        public event AgricultureLandChangedEventHandler AgricultureLandChanged;

		#endregion

		#region Propertys

		/// <summary>
		/// 是否是承包地块示意图
		/// </summary>
		public bool IsSketchMap { get; set; } = true;

        /// <summary>
        /// 是否汇交成果数据
        /// </summary>
        public bool IsSummery { get; set; }

        /// <summary>
        /// 文件集合
        /// </summary>
        public List<string> FileNames { get; set; }

        /// <summary>
        /// 承包合同
        /// </summary>
        public List<ContractConcord> Concords { get;private set; }

        /// <summary>
        /// 当前承包方
        /// </summary>
        public ContractConcord Concord { get; set; }

        /// <summary>
        /// 当前地块
        /// </summary>
        public ContractLand AgriLand { get; set; }

		#endregion


		private TaskSketchMapArgumet.DType _dataType = TaskSketchMapArgumet.DType.Null;
		public DataTaskNavigater()
        {
            InitializeComponent();
        }


        #region Methods

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void LoadData(List<object> lst)// fileNames)
        {
            if (lst == null || lst.Count == 0)
            {
                return;
            }
			_dataType =lst[0] is CbfItem? TaskSketchMapArgumet.DType.CbfItem: TaskSketchMapArgumet.DType.DatFile;
			if (_dataType == TaskSketchMapArgumet.DType.DatFile)
			{
				var fileNames = new List<string>(lst.Count);
				foreach (var o in lst) fileNames.Add(o.ToString());
				FileNames = fileNames;
				List<string> names = new List<string>();
				fileNames.ForEach(da => names.Add(System.IO.Path.GetFileName(da)));
				lstView.ItemsSource = names;
				lstView.SelectedIndex = 0;
				names = null;
			}
			else
			{
				Concords = new List<ContractConcord>(lst.Count);
				foreach (var o in lst)
				{
					Concords.Add(SketchMapUtil.ToContractConcord(o as CbfItem));
				}
				lstContractor.ItemsSource = Concords;
				lstContractor.SelectedIndex = 0;
			}
            ShowData();
        }
		//public void LoadData(

        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearData()
        {
            lstView.ItemsSource = null;
            lstContractor.ItemsSource = null;
            lstLand.ItemsSource = null;
            lstData.Text = "";
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refersh()
        {
            ConcordChanged?.Invoke(Concord);
            AgricultureLandChanged?.Invoke(AgriLand);
        }

		#endregion



		#region Events

		/// <summary>
		/// 数据选择改变
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lstView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstView.SelectedItem == null)
            {
                return;
            }
            string name = lstView.SelectedItem.ToString();
            string fileName = FileNames.Find(nam => name == System.IO.Path.GetFileName(nam));
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
			if (IsSketchMap)
			{
				Concords = DataExchange.Deserialize(fileName);
				lstContractor.ItemsSource = Concords;
				lstContractor.SelectedIndex = 0;
			}
			else
			{
				var datas = DataExchange.ReaderData(fileName);
				string info = "";
				foreach (var data in datas)
				{
					info += "受让方：" + data.SRF + "\r\n\r\n";
					info += "鉴证书编号：" + data.JZSBM + "\r\n\r\n";
					info += "地块总数：" + data.Lands.Length.ToString() + "宗\r\n\r\n";
					info += "地块总面积：" + data.Lands.Sum(da => da.DKMJM).ToString() + "亩\r\n\r\n";
				}
				int index = info.LastIndexOf("亩");
				if (index > 0)
				{
					info = info.Substring(0, index + 1);
				}
				lstData.Text = info;
				datas = null;
				GC.Collect();
			}
        }

        /// <summary>
        /// 承包方选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstContractor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Concord = lstContractor.SelectedItem as ContractConcord;
            ConcordChanged?.Invoke(Concord);
            if (Concord == null)
            {
                return;
            }
            lstLand.ItemsSource = Concord.Lands;
            lstLand.SelectedIndex = 0;
        }

        /// <summary>
        /// 承包地块选择改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstLand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AgriLand = lstLand.SelectedItem as ContractLand;
            AgricultureLandChanged?.Invoke(AgriLand);
        }

        #endregion

        #region Helper

        /// <summary>
        /// 显示数据
        /// </summary>
        private void ShowData()
        {
			ocDat.Visibility = _dataType == TaskSketchMapArgumet.DType.DatFile ? Visibility.Visible : Visibility.Collapsed;
			ocCbf.Visibility = IsSketchMap ? Visibility.Visible : Visibility.Collapsed;
            //lstContractor.Visibility = IsSketchMap ? Visibility.Visible : Visibility.Collapsed;
            ocCbdk.Visibility = IsSketchMap ? Visibility.Visible : Visibility.Collapsed;
            //lstLand.Visibility = IsSketchMap ? Visibility.Visible : Visibility.Collapsed;
            //labTransLand.Visibility = IsSketchMap || IsSummery ? Visibility.Collapsed : Visibility.Visible;
            //lstData.Visibility = IsSketchMap || IsSummery ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}
