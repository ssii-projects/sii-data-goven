using Agro.Library.Common;
using Agro.Module.ThemeAnaly.ViewModel.Control;
using Agro.LibCore;
using Agro.LibCore.NPIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Agro.Module.ThemeAnaly.View.Control
{
    /// <summary>
    /// TablePanel.xaml 的交互逻辑
    /// </summary>
    public partial class TablePanel
    {
        public TablePanel()
        {
            InitializeComponent();
            TitleBar.ExpottAction += Export;
        }
        private void Export()
        {
            var model = (DataContext as TablePanelViewModel).TableViewModel;
            //model.TableViewModel.Colums[0]
            //foreach(var r in model.GridDataDynamicObjects)
            //{

            //}
            var ofd = new SaveFileDialog();
            ofd.Filter = "Excel文件(*.xls)|*.xls";
            ofd.OverwritePrompt = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != true)
                return;
            try
            {
                var file = ofd.FileName;
                ExportExcel(file,model);
                Win32.ShellExecute(IntPtr.Zero, "open", file, null, null, ShowCommands.SW_SHOWNORMAL);
            }
            catch (Exception ex)
            {
                UIHelper.ShowExceptionMessage(ex);
            }
        }
        private void ExportExcel(string fileName, TableViewModel grid)
        {
            using (var sht = new NPIOSheet())   //打开myxls.xls文件
            {
                sht.Create();
                int cols = grid.Colums.Count+1;
                for (int c = 1; c < cols; ++c)
                {
                    //var colWidth = grid.GetColWidth(c);
                    //sht.SetColumnWidth(c, colWidth);
                    var colLabel = grid.Colums[c - 1];
                    sht.SetCellText(0, c, colLabel);
                }

                for (int r = 0; r < grid.GridDataDynamicObjects.Count; ++r)
                {
                    var gdo=grid.GridDataDynamicObjects[r];
                    for (int c = 0; c < cols; ++c)
                    {
                        //var o = grid.GetCellText(r, c);
                        string o = null;
                        if (c == 0)
                        {
                            o=gdo._properties["Header"].ToString();
                        }else
                        {
                            var colLabel = grid.Colums[c - 1];
                            var o1 = gdo._properties["Value" + colLabel];
                            o =o1==null?null:o1.ToString();
                        }
                        if (string.IsNullOrEmpty(o))
                        {
                            sht.SetCellText(r + 1, c, o);
                        }
                        else
                        {
                            if (c > 0)
                            {
                                double d = SafeConvertAux.ToDouble(o);
                                sht.SetCellDouble(r + 1, c, d);
                            }
                            else
                            {
                                sht.SetCellText(r + 1, c, o);
                            }
                        }
                    }
                }
                sht.ExportToExcel(fileName);
            }
        }
    }
}
