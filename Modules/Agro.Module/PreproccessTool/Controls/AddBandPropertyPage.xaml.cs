using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// 添加带号对话框
    /// </summary>
    public partial class AddBandPropertyPage : Agro.LibCore.UI.TaskPropertyPage
    {
        private bool IsBandControlVisible
        {
            get { return dpBand.Visibility == Visibility.Visible; }
        }

        public readonly List<string> InputShpFiles = new List<string>();

        public Func<string> OnApply;

        private readonly bool _fMultiselect;

        public AddBandPropertyPage(bool fShowBandControl=true,bool fMultiselect=false,Func<string> onApply=null)
        {
            _fMultiselect = fMultiselect;
            OnApply = onApply;
            InitializeComponent();
            Height = 320;
            if (fMultiselect)
            {
                tbShpFilePath.IsReadOnly = true;
            }
            if (!fShowBandControl)
            {
                dpBand.Visibility = Visibility.Collapsed;
            }
            btnShpFilePath.Click += (s, e) =>
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "ShapeFile (*.shp)|*.shp"
                };
                if (fMultiselect)
                {
                    openFileDialog.Multiselect = true;
                }
                var result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    if (!fMultiselect)
                    {
                        this.tbShpFilePath.Text = openFileDialog.FileName;
                    }else
                    {
                        InputShpFiles.Clear();
                        string files = null;
                        foreach (var file in openFileDialog.FileNames)
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
            //base.Check += (s, e) =>
            //{
            //    var err = IsOK();
            //    if (err == null)
            //    {
            //        e.IsValided = true;
            //        ShapeFileFullName = tbShpFilePath.Text.Trim();
            //        OutputFileFullName = tbOutShpFilePath.Text.Trim();
            //        if (!OutputFileFullName.ToLower().EndsWith(".shp"))
            //        {
            //            OutputFileFullName += ".shp";
            //        }
            //        Band = SafeConvertAux.ToInt32(tbBand.Text.Trim());
            //    }else
            //    {
            //        MessageBox.Show(err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //};
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
                Band = SafeConvertAux.ToInt32(tbBand.Text.Trim());
            }
            //else
            //{
            //    MessageBox.Show(err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            return err;
        }

        public string ShapeFileFullName
        {
            get;
            private set;
            //get
            //{
            //    return tbShpFilePath.Text.Trim();
            //}
        }
        public string OutputFileFullName
        {
            get;
            private set;
        }
        public int Band
        {
            get;
            private set;
            //get
            //{
            //    return SafeConvertAux.ToInt32(tbBand.Text.Trim());
            //}
        }
        private string IsOK()
        {
            string s = null;
            if (_fMultiselect)
            {
                if (InputShpFiles.Count == 0)
                {
                    return "未选择输入文件！";
                }
            }
            else
            {
                s = tbShpFilePath.Text.Trim();
                if (s.Length == 0)
                {
                    return "未选择输入文件！";
                }
                if (!File.Exists(s))
                {
                    return "文件：" + s + "不存在！";
                }
                if (!s.ToLower().EndsWith(".shp"))
                {
                    return "输入文件必须以.shp结尾！";
                }
            }
            if (IsBandControlVisible)
            {
                if (tbBand.Text.Trim().Length == 0)
                {
                    return "未输入代号！";
                }
                var n = SafeConvertAux.ToInt32(tbBand.Text.Trim());
                if (n.ToString().Length != 2)
                {
                    return "代号必须输入两位数字！";
                }
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
            if (OnApply != null)
            {
                return OnApply();
            }
            return null;
        }
    }
}
