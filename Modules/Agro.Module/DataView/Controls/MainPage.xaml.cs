/*
 * (C) 2015  公司版权所有,保留所有权利 
 */

using System.Windows.Controls;

namespace Agro.Module.DataView
{
	/// <summary>
	/// 查询统计主界面
	/// </summary>
	public partial class MainPage : UserControl
	{
		/// <summary>
		/// 构造函数:初始化数据字典窗体
		/// </summary>
		public MainPage()
		{
			InitializeComponent();
			navTree.Init(contentPnl);
		}
	}
}
