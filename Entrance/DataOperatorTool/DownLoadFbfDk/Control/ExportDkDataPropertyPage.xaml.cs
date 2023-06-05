using Agro.LibCore;
using Agro.LibCore.UI;
using System.Windows;

namespace DataOperatorTool
{
	public class ExportDkDataPropertyPageBase:TaskPropertyPage
	{
	}
	/// <summary>
	/// 导出调查地块属性页
	/// </summary>
	public partial class ExportDkDataPropertyPage : ExportDkDataPropertyPageBase
	{
        public ExportDkDataPropertyPage()
        {
            InitializeComponent();
            this.DialogHeight = 320;
			//tbShpFilePath.Filter = "ShapeFile (*.shp)|*.shp";
			tbFbfBM.OnButtonClick += SelectFbf;
			tbFbfBM.TextChanged += (s, e) => Fbfbm = tbFbfBM.Text.Trim();
			tbShpFilePath.OnButtonClick += () =>
			{
                var d = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "ShapeFile (*.shp)|*.shp",//|移动调查交换包|*.dk",
                    OverwritePrompt = true,
                    RestoreDirectory = true
                };

				if (tbFbfBM.Tag is SelectFbfPanel.FbfItem fbf)
				{
					d.FileName = string.IsNullOrEmpty(fbf.FbfMC) ? fbf.FbfBM : fbf.FbfMC;
				}

				var result = d.ShowDialog();
				if (result == true)
				{
					tbShpFilePath.Text = d.FileName;
					ExportFilePath = d.FileName;
				}
			};
		}
		/// <summary>
		/// 发包方编码
		/// </summary>
		public string Fbfbm { get; set; } = string.Empty;
        /// <summary>
        /// 导出文件路径
        /// </summary>
        public string ExportFilePath { get; set; }= string.Empty;

		public eDatabaseType DatabaseType { get; set; }

		public override string Apply()
        {
            Fbfbm = tbFbfBM.Text.Trim();
			ExportFilePath = tbShpFilePath.Text;
            if (string.IsNullOrEmpty(Fbfbm))
            {
                return "未输入发包方编码！";
            }
            if (string.IsNullOrEmpty(ExportFilePath))
            {
                return "未输入导出文件！";
            }
			DatabaseType = ExportFilePath.EndsWith(".dk") ? eDatabaseType.SQLite : eDatabaseType.ShapeFile;
            return null;
        }

		private void SelectFbf() {
			var pnl = new SelectFbfPanel();
			var dlg = new KuiDialog(Window.GetWindow(this), "选择发包方")
			{
				Width = 740,
				Height = 380,
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
				  tbFbfBM.Tag = pnl.SelectedFbf;//.FbfMC;
				  tbFbfBM.Text = pnl.SelectedFbf.FbfBM;
				  dlg.Close();
			  };
			dlg.ShowDialog();
		}
    }
}
