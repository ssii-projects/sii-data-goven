using Microsoft.Win32;
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

namespace Agro.Module.ThemeAnaly.View.Control
{
    /// <summary>
    /// ChartPies.xaml 的交互逻辑
    /// </summary>
    public partial class ChartPies : UserControl
    {
        public ChartPies()
        {
            InitializeComponent();
        }
        public void ExportImage()
        {
            //chart.Export();
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)this.lstBox.ActualWidth, (int)this.lstBox.ActualHeight, 96d, 96d, PixelFormats.Default);
            targetBitmap.Render(this.lstBox);
            PngBitmapEncoder saveEncoder = new PngBitmapEncoder();
            saveEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));
            var dlg = new SaveFileDialog()
            {
                Filter = "png文件 (*.png)|*.png",
                OverwritePrompt=true
            };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                try
                {
                    if (File.Exists(dlg.FileName))
                    {
                        File.Delete(dlg.FileName);
                    }
                    using (var fs = System.IO.File.Open(dlg.FileName, System.IO.FileMode.OpenOrCreate))
                    {
                        saveEncoder.Save(fs);
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
