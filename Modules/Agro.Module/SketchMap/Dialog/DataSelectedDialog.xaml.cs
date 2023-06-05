using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Microsoft.Win32;
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

/*
yxm created at 2019/1/16 10:10:08
*/
namespace Agro.Module.SketchMap
{
	/// <summary>
	/// DataSelectedDialog.xaml 的交互逻辑
	/// </summary>
	public partial class DataSelectedDialog : TaskPropertyPage
	{
		//public enum DType
		//{
		//	Null,
		//	DatFile,
		//	CbfItem
		//}
		//#region Propertys

		/// <summary>
		/// 参数
		/// </summary>
		public readonly TaskSketchMapArgumet Argument= new TaskSketchMapArgumet();


		/// <summary>
		/// 行政区代码
		/// </summary>
		public string ZoneCode { get; set; }


		public TaskSketchMapArgumet.DType DataType { get { return Argument.DataType; } }
		public DataSelectedDialog()
		{
			InitializeComponent();
			//txtFilePath.Filter = "Dat文件(*.dat)|*.dat";
			this.Loaded += (s, e) => LoadState();
#if DEBUG
			txtOutputFilePath.Text = @"D:\tmp\导出地块示意图";// D:\MyProjects\2019\导出地块示意图";
			txtDrawPerson.Text = "张三";
			txtCheckPerson.Text = "李四";
			dtpDrawTime.SelectedDate = DateTime.Now;
			dtpCheckTime.SelectedDate = DateTime.Now;
#endif
		}

		public override string Apply()
		{
			string? errorString = null;
			if (Argument.FileNames.Count == 0)
			{
				errorString = "未选择数据所在路径!";
			}
			if (string.IsNullOrEmpty(Argument.OutputPath))
			{
				errorString += "\r\n未选择数据保存路径!";
			}
			if (errorString == null)
			{
				SaveState();
			}
			return errorString;
		}

		/// <summary>
		/// 输出路径改变
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void txtOutputFilePath_TextChanged(object sender, TextChangedEventArgs e)
		{
			Argument.OutputPath = txtOutputFilePath.Text;
		}
		/// <summary>
		/// 地图类型选择改变
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//chkNeighbor.IsEnabled = cmbType.SelectedIndex != 1;
			//this.Header = cmbType.SelectedIndex == 1 ? "流转地块分布图" : "承包地块示意图";
			//bool isSummery = cmbType.SelectedIndex == 2;
			//txtFolderPath.Visibility = isSummery ? Visibility.Visible : Visibility.Collapsed;
			//zone.Visibility = txtFolderPath.Visibility;
			//txtFilePath.Visibility = isSummery ? Visibility.Collapsed : Visibility.Visible;
			//txtOutputFilePath.Visibility = txtFilePath.Visibility;
			//labSavePath.Text = cmbType.SelectedIndex == 2 ? "行政区域选择：" : "数据保存路径：";
		}

		void SaveState()
		{
			var p = MyGlobal.Persist;
			p.SaveSettingInfo("A2FADDA1-32D0-4257-BBDE-FD50F6363C60", txtOutputFilePath.Text.Trim());

			var data = Argument.MapProperty;
			data.DrawPerson = txtDrawPerson.Text.Trim();
			data.DrawDate = dtpDrawTime.SelectedDate;
			data.CheckPerson = txtCheckPerson.Text.Trim();
			data.CheckDate = dtpCheckTime.SelectedDate;
			data.Company = txtCompany.Text;
			data.UseNeighbor = chkNeighbor.IsChecked == true;
			data.SaveDocFormat = chkWord.IsChecked == true;
			data.SavePdfFormat = chkPdf.IsChecked == true;
			data.SaveJpgFormat = chkJpg.IsChecked == true;
			//data.IsSketchMap=cmbType.SelectedIndex==true ;

			SkecthMapProperty.SerializeXml(Argument.MapProperty);
		}
		void LoadState() {
			var p = MyGlobal.Persist;
			txtOutputFilePath.Text=p.LoadSettingInfo("A2FADDA1-32D0-4257-BBDE-FD50F6363C60").ToString();

			var data = SkecthMapProperty.DeserializeXml();
			if (data == null)
			{
				return;
			}
			txtDrawPerson.Text = data.DrawPerson;
			dtpDrawTime.SelectedDate = data.DrawDate;
			txtCheckPerson.Text = data.CheckPerson;
			dtpCheckTime.SelectedDate = data.CheckDate;
			txtCompany.Text = data.Company;
			chkNeighbor.IsChecked = data.UseNeighbor;
			chkWord.IsChecked = data.SaveDocFormat;
			chkPdf.IsChecked = data.SavePdfFormat;
			chkJpg.IsChecked = data.SaveJpgFormat;
			cmbType.SelectedIndex = data.IsSketchMap;
			Argument.MapProperty = data;
		}




		private void ComboBoxItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (sender is ComboBoxItem bi)
			{
				cmbDataSource.SelectedItem = sender;
				if (bi == cbiDatFile)
				{
					ShowSelectDatFileDialog();
				}
				else if (bi == cbiCbf)
				{
					ShowCbfSelectDialog();
				}
			}
		}
		void ShowSelectDatFileDialog()
		{
			var dlg = new OpenFileDialog()
			{
				Filter = "Dat文件(*.dat)|*.dat",
				Multiselect=true
			};
			if (true != dlg.ShowDialog())
				return;

			Argument.FileNames.Clear();
			foreach (var file in dlg.FileNames)
			{
				Argument.FileNames.Add(file);
			}
			//if (dlg.FileNames.Length <= 1)
			//{
			//	dkcFile.Visibility = Visibility.Collapsed;
			//	return;
			//}
			dkcFile.Visibility = Visibility.Visible;
			lstBox.ItemsSource = null;
			lstBox.ItemsSource = dlg.FileNames;
			Argument.DataType = TaskSketchMapArgumet.DType.DatFile;
		}
		void ShowCbfSelectDialog()
		{
			var pnl = new SelectCbfPanel();
			var dlg = new KuiDialog(Window.GetWindow(this), "选择承包方")
			{
				Width = 700,
				Content = pnl
			};
			dlg.BtnOK.Click += (s, e) =>
			{
				var err = pnl.OnApply();
				if (err != null)
				{
					UIHelper.ShowError(dlg, err);
					return;
				}

				Argument.FileNames.Clear();
				foreach (var file in pnl._lstCbf)
				{
					if(file.IsSelected)
					Argument.FileNames.Add(file);
				}
				//if (dlg.FileNames.Length <= 1)
				//{
				//	dkcFile.Visibility = Visibility.Collapsed;
				//	return;
				//}
				dkcFile.Visibility = Visibility.Visible;
				lstBox.ItemsSource = null;
				lstBox.ItemsSource =Argument.FileNames;// dlg.FileNames;

				Argument.DataType = TaskSketchMapArgumet.DType.CbfItem;
				dlg.Close();
			};
			dlg.ShowDialog();
		}
		///// <summary>
		///// 初始化检查参数
		///// </summary>
		///// <returns></returns>
		//private TaskSketchMapArgumet InitalizeArgument()
		//{
		//	var argument = new TaskSketchMapArgumet
		//	{
		//		FileNames = FileNames,
		//		OutputPath = OutputPath,
		//		MapProperty = MapProperty
		//	};
		//	//argument.Workpage = Workpage;
		//	return argument;
		//}
	}
}
