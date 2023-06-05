using Agro.LibCore;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// 数据平移属性页
    /// </summary>
    public partial class DataTranslatePropertyPage : TaskPropertyPage
    {
        public DataTranslatePropertyPage()
        {
            InitializeComponent();
            Height = 350;
            btnShpFilePath.Click += (s, e) =>
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "ShapeFile (*.shp)|*.shp"
                };
                var result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    this.tbShpFilePath.Text = openFileDialog.FileName;
                }
            };
            btnOutShpFilePath.Click += (s, e) =>
            {
                var openFileDialog = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "ShapeFile (*.shp)|*.shp"
                };
                openFileDialog.OverwritePrompt = true;
                var result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    this.tbOutShpFilePath.Text = openFileDialog.FileName;
                }
            };
        }

        public string ShapeFileFullName
        {
            get;
            private set;
        }
        public string OutputFileFullName
        {
            get;
            private set;
        }
        public double XOffset
        {
            get;
            private set;
        }
        public double YOffset
        {
            get;
            private set;
        }

        public override string Apply()
        {
            var err = IsOK();
            if (err == null)
            {
                ShapeFileFullName = tbShpFilePath.Text.Trim();
                OutputFileFullName = tbOutShpFilePath.Text.Trim();
                if (!OutputFileFullName.ToLower().EndsWith(".shp"))
                {
                    OutputFileFullName += ".shp";
                }
                XOffset = SafeConvertAux.ToDouble(tbXOffset.Text.Trim());
                YOffset = SafeConvertAux.ToDouble(tbYOffset.Text.Trim());
            }
            //else
            //{
            //    MessageBox.Show(err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            return err;
        }

        private string IsOK()
        {
            var s = tbShpFilePath.Text.Trim();
            if (s.Length == 0)
            {
                return "未输入文件路径！";
            }
            if (!File.Exists(s))
            {
                return "文件：" + s + "不存在！";
            }
            if (!s.ToLower().EndsWith(".shp"))
            {
                return "输入文件必须以.shp结尾！";
            }
            s = tbOutShpFilePath.Text.Trim();
            if (s.Length == 0)
            {
                return "未选择输出文件路径！";
            }
            if (!s.ToLower().EndsWith(".shp"))
            {
                return "输出文件必须以.shp结尾！";
            }
            XOffset = SafeConvertAux.ToDouble(tbXOffset.Text.Trim());
            YOffset = SafeConvertAux.ToDouble(tbYOffset.Text.Trim());
            if (XOffset == 0 && YOffset == 0)
                return "未输入偏移！";
            return null;
        }
    }
}
