using Agro.LibCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace JzdxBuild
{
  /// <summary>
  /// MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow : Window
  {
    private bool _fRuning = false;
    private readonly DKQlr _qlrMgr = new DKQlr();
    public MainWindow()
    {
      InitializeComponent();
      Closing += (s, e) =>
      {
        if (_fRuning)
        {
          e.Cancel = true;
          MessageBox.Show("正在处理，请等待！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      };
    }
    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new OpenFileDialog
      {
        Filter = "Shape文件(*.shp)|*.shp",
        RestoreDirectory = true,
        FilterIndex = 1
      };
      if (dlg.ShowDialog() == true)
      {
        tbCBDShapeFile.Text = dlg.FileName;
      }
    }

    [DllImport("shell32.dll")]
    public static extern int ShellExecute(IntPtr hwnd, string lpszOp, string lpszFile, string lpszParams, string lpszDir, int fsShowCmd);

    private void doRun(bool fUseShellExec = false)
    {
      tbProgress.Text = "";
      tbProgress.Visibility = Visibility.Visible;
      var sShpCBDFile = tbCBDShapeFile.Text.Trim();
      if (string.IsNullOrEmpty(sShpCBDFile))
      {
        MessageBox.Show("未选择地块shape文件！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }
      if (!File.Exists(sShpCBDFile))
      {
        MessageBox.Show("请选择有效的地块shape文件！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }
      if (fUseShellExec)
      {
        #region 生成承包方名称
        var mdbPath = FileUtil.GetFilePath(sShpCBDFile) + "../权属数据/";

        if (!Directory.Exists(mdbPath))
        {
          throw new Exception("目录" + mdbPath + "不存在！");
        }

        _qlrMgr.Init(mdbPath, sShpCBDFile);

        #endregion
      }
      var s = sShpCBDFile.Replace('\\', '/');
      int n = s.LastIndexOf('/');
      var path = s.Substring(0, n + 1);
      var name = s.Substring(n + 1);
      if (!name.Contains("DK"))
      {
        MessageBox.Show("请选择有效的地块shape文件（以DK开头的.shp文件）！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }
      var sShpJzdFile = path + name.Replace("DK", "JZD");
      var sShpJzxFile = path + name.Replace("DK", "JZX");
      if (!File.Exists(sShpJzdFile))
      {
        MessageBox.Show("文件" + sShpJzdFile + "不存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }
      if (!File.Exists(sShpJzxFile))
      {
        MessageBox.Show("文件" + sShpJzxFile + "不存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }


      var param = new InitLandDotCoilParam
      {
        nJzdBSMStartVal = SafeConvertAux.ToInt32(tbBSM.Text.Trim()),
        sJzdYSDMVal = tbYSDM.Text.Trim(),
        sJBLXVal = tbJBLX.Text.Trim(),
        sJZDLXVal = tbJZDLX.Text.Trim(),

        nJzxBSMStartVal = SafeConvertAux.ToInt32(tbJzxBsm.Text.Trim()),
        sJzxYSDMVal = tbJzxYSDM.Text.Trim(),
        JXXZ = tbJxxz.Text.Trim(),
        JZXLB = tbJZXLB.Text.Trim(),
        PLDWZJR = tbPLDWZJR.Text.Trim(),
        JZXWZ = tbJZXWZ.Text.Trim(),
        AddressLinedbiDistance = SafeConvertAux.ToDouble(tbJzxTolerance.Text.Trim())
      };

      if (param.sJBLXVal.Length > 1)
      {
        MessageBox.Show("JBLX的长度不能大于1！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }
      if (param.sJZDLXVal.Length > 1)
      {
        MessageBox.Show("JZDLX的长度不能大于1！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }


      var t = new InitLandDotCoil(param);
      t.OnQueryCbdQlr += en =>
      {
        var sQlr = _qlrMgr.GetQlr(en.rowid);
        if (sQlr == null)
        {
          sQlr = en.rowid.ToString();
        }
        return sQlr;
      };
      t.ReportProgress += (msg, i) =>
      {
        tbProgress.Text = msg + ":" + i + "%";
        System.Windows.Forms.Application.DoEvents();
      };
      t.ReportInfomation += msg =>
      {
        Console.WriteLine(msg);
      };
      t.DoInit(sShpCBDFile, sShpJzdFile, sShpJzxFile);
    }
    private void btnRun_Click(object sender, RoutedEventArgs e)
    {
      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      try
      {
        _fRuning = true;
        btnRun.IsEnabled = false;



        doRun(true);

        sw.Stop();
        MessageBox.Show("完成，用时：" + sw.Elapsed);
      }
      catch (Exception ex)
      {
        UIHelper.ShowErrorMessage(ex);
      }
      finally
      {
        _fRuning = false;
      }
    }
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

  }
}
