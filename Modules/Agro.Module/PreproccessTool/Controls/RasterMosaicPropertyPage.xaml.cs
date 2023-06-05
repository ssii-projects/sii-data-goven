using Agro.LibCore.UI;
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

namespace Agro.Module.PreproccessTool
{
    /// <summary>
    ///影像数据拼接属性页
    /// </summary>
    public partial class RasterMosaicPropertyPage : TaskPropertyPage
    {
        public readonly List<string> InputShpFiles = new List<string>();
        public RasterMosaicPropertyPage()
        {
            InitializeComponent();
            Height = 320;
            btnShpFilePath.Click += (s, e) =>
            {
                var d = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "Tif文件 (*.tif)|*.tif|Img文件(*.img)|*.img"
                };
                d.Multiselect = true;
                var result = d.ShowDialog();
                if (result == true)
                {
                    InputShpFiles.Clear();
                    string files = null;
                    foreach (var file in d.FileNames)
                    {
                        InputShpFiles.Add(file);
                        if (files == null)
                        {
                            files = "\"" + file + "\"";
                        }
                        else
                        {
                            files += " \"" + file + "\"";
                        }
                    }
                    if (files != null)
                    {
                        tbShpFilePath.Text = files;
                    }
                }
            };
            btnOutShpFilePath.Click += (s, e) =>
            {
                var openFileDialog = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "影像文件 (*.img)|*.img"
                };
                openFileDialog.OverwritePrompt = true;
                var result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    this.tbOutShpFilePath.Text = openFileDialog.FileName;
                }
            };
        }
        public string OutputFileFullName
        {
            get;
            private set;
        }

        public override string Apply()
        {
            var err = IsOK();
            if (err == null)
            {
                OutputFileFullName = tbOutShpFilePath.Text.Trim();
            }
            return err;
        }

        private string IsOK()
        {
            if (InputShpFiles.Count == 0)
            {
                return "未输入文件路径！";
            }
            var s = tbOutShpFilePath.Text.Trim();
            if (s.Length == 0)
            {
                return "未选择输出文件路径！";
            }
            return null;
        }
    }
}
