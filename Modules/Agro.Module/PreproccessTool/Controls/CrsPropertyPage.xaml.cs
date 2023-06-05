using Agro.GIS;
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
    /// CrsPropertyPage.xaml 的交互逻辑
    /// </summary>
    public partial class CrsPropertyPage : TaskPropertyPage
    {
        private readonly Func<string> OnApply;
        public CrsPropertyPage(Func<string> onApply, bool fShowOriginCRS,bool fShowOutFileBox)
        {
            OnApply = onApply;
            InitializeComponent();

            if (!fShowOriginCRS)
            {
                dpOriginCRS.Visibility = Visibility.Collapsed;
            }
            if (!fShowOutFileBox)
            {
                dpOutFile.Visibility = Visibility.Collapsed;
            }

            crsPnl.Refresh();
            btnShpFilePath.Click += (s, e) =>
            {
                var d = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "ShapeFile (*.shp)|*.shp"
                };
                var result = d.ShowDialog();
                if (result == true)
                {
                    tbShpFilePath.Text = d.FileName;
                    InputFileFullName = d.FileName;
                    if (fShowOriginCRS)
                    {
                        var sr = ShapeFileUtil.GetSpatialReference(d.FileName);
                        if (sr != null)
                        {
                            tbOriginCRS.Text = sr.Name;
                        }
                        else
                        {
                            var prjText = ShapeFileUtil.GetPrjText(d.FileName);
                            tbOriginCRS.Text = prjText == null ? "未定义" : prjText;
                        }
                        //dpOriginCRS.Visibility = Visibility.Visible;
                    }
                }
            };
            btnOutShpFilePath.Click += (s, e) =>
            {
                var d = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "ShapeFile (*.shp)|*.shp"
                };
                d.OverwritePrompt = true;
                var result = d.ShowDialog();
                if (result == true)
                {
                    tbOutShpFilePath.Text = d.FileName;
                    OutputFileFullName = d.FileName;
                }
            };
        }
        public override string Apply()
        {
            var err= OnApply();
            if (err == null)
            {
                SelectedSpatialReference = crsPnl.SelectedSpatialReference;
            }
            return err;
            //var err = IsOK();
            //return err;
        }

        public string InputFileFullName
        {
            get;
            private set;
        }
        public string OutputFileFullName
        {
            get;
            private set;
        }
        public SpatialReference SelectedSpatialReference
        {
            get; set;
        }
    }
}
