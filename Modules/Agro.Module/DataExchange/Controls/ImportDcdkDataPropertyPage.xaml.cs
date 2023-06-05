using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// ImportDcdkDataPropertyPage.xaml 的交互逻辑
	/// </summary>
	public partial class ImportDcdkDataPropertyPage : TaskPropertyPage
    {
        public ImportDcdkDataPropertyPage()
        {
            InitializeComponent();
            DialogHeight = 320;
			tbShpFilePath.Filter = "ShapeFile (*.shp)|*.shp|移动调查交换包|*.dk";
			LoadSeting();
		}
        public override string Apply()
        {
			FileName = tbShpFilePath.Text;
            if (string.IsNullOrEmpty(FileName))
            {
                return "未输入调查地块数据路径！";
            }
            try
            {
				DatabaseType= tbShpFilePath.Text.EndsWith(".dk")?eDatabaseType.SQLite:eDatabaseType.ShapeFile;
				if(DatabaseType==eDatabaseType.ShapeFile)
				{
					using (var shp = new ShapeFile())
					{
						shp.Open(FileName);
						var st = shp.GetShapeType();
						if (!(st == EShapeType.SHPT_POLYGON || st == EShapeType.SHPT_POLYGONZ || st == EShapeType.SHPT_POLYGONM))
						{
							return "输入ShapeFile文件不是面要素类型！";
						}
					}
				}
				SaveSeting();
			}
			catch(Exception ex)
            {
                return ex.Message;
            }
            return null;
        }


		public eDatabaseType DatabaseType { get; set; }

		private string _shpfile;
        public string FileName { get { return _shpfile; } set { _shpfile = value;tbShpFilePath.Text = value; } }

		private void LoadSeting()
		{
			var s = MyGlobal.Persist.LoadSettingInfo("FF793F27-E2E9-4C73-A363-1875633F9406") as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbShpFilePath.Text = s;
			}
		}
		private void SaveSeting()
		{
			if (!string.IsNullOrEmpty(FileName))
			{
				MyGlobal.Persist.SaveSettingInfo("FF793F27-E2E9-4C73-A363-1875633F9406", FileName);
			}
		}
	}
}
