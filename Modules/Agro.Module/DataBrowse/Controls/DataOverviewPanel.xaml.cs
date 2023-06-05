using Agro.GIS;
using Agro.LibCore;
using NPOI.HSSF.UserModel;
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

namespace Agro.Module.DataBrowse
{
    /// <summary>
    /// 数据分析
    /// </summary>
    public partial class DataOverviewPanel : UserControl
    {
        class GridData
        {
            private readonly Dictionary<int, Dictionary<int, string>> _data = new Dictionary<int, Dictionary<int, string>>();
            public string GetCellText(int row,int col)
            {
                Dictionary<int, string> dic;
                if(_data.TryGetValue(row,out dic))
                {
                    string s;
                    if(dic.TryGetValue(col,out s))
                    {
                        return s;
                    }
                }
                return "";
            }
            public void SetCellText(int row,int col,string s)
            {
                Dictionary<int, string> dic;
                if(!_data.TryGetValue(row,out dic))
                {
                    dic = new Dictionary<int, string>();
                    _data[row] = dic;
                }
                dic[col] = s;
            }
        }
        class RootFolder
        {
            public string Path;
            /// <summary>
            /// 行政编码
            /// </summary>
            public string Xzbm;
            /// <summary>
            /// 年度
            /// </summary>
            public int Year;
            public RootFolder(string path)
            {
                Path = path.TrimEnd(new char[] { '/', '\\' });
                int n =Math.Max(Path.LastIndexOf('/'),Path.LastIndexOf('\\'));
                string s = null;
                if (n >= 0)
                {
                    s = Path.Substring(n + 1);
                    for(int i = 0; i < s.Length; ++i)
                    {
                        var ch = s[i];
                        if (!(ch >= '0' && ch <= '9'))
                        {
                            s = s.Substring(0, i);
                            Xzbm = s;
                            break;
                        }
                    }
                }
                if (Xzbm != null)
                {
                    FileUtil.EnumFiles(System.IO.Path.Combine(path, "权属数据"), fi =>
                     {
                         if (fi.FullName.EndsWith(".mdb"))
                         {
                             s=fi.Name.Substring(Xzbm.Length);
                             s = s.Substring(0, s.Length - 4);
                             Year = SafeConvertAux.ToInt32(s);
                             return false;
                         }
                         return true;
                     });
                }
            }

            ///// <summary>
            ///// 查找权属单位代码表
            ///// </summary>
            ///// <returns></returns>
            //public string FindQsdwXlsFile()
            //{
            //    return null;
            //}
        }
        class Column
        {
            public string Name;
            public int Width;
            public Column(string name,int wi=100)
            {
                Name = name;
                Width = wi;
            }
        }

        private readonly GridData _data1 = new GridData();
        private readonly GridData _data2 = new GridData();
        private readonly GridData _data3 = new GridData();
        private readonly GridData _data4 = new GridData();
        private readonly GridData _data5 = new GridData();
        public DataOverviewPanel()
        {
            InitializeComponent();
            InitGrid1(grid1,_data1);
            InitGrid2(grid2,_data2);
            InitGrid3(grid3,_data3);
            InitGrid4(grid4,_data4);
            InitGrid5(grid5,_data5);

            //#region test
            //Refresh(@"E:\空间数据\511702通川区");
            //#endregion
        }
        public void Refresh(string rootPath)
        {
            var rf = new RootFolder(rootPath);
            long nQssjSumSize= RefreshGrid1(rf);
            RefreshGrid2(rf);
            long nSlsjSumSize= RefreshGrid3(rf);
            long nQtzlSumSize=RefreshGrid4(rf);

            _data5.SetCellText(1, 4, FileUtil.GetFormattedFileLenth(nQssjSumSize+nSlsjSumSize+nQtzlSumSize));

            updateColumnWidth(grid1);
            updateColumnWidth(grid2);
            updateColumnWidth(grid3);
            updateColumnWidth(grid4);
            updateColumnWidth(grid5);

        }
        private long RefreshGrid1(RootFolder rf)
        {
            grid1.SetColLabel(2, rf.Xzbm + rf.Year + ".mdb");
            var qsPath = System.IO.Path.Combine(rf.Path, "权属数据");

            long len1 = 0;
            int nRec1 = 0;
            var file = System.IO.Path.Combine(qsPath, rf.Xzbm + rf.Year + "权属单位代码表.xls");
            if (File.Exists(file)) {
                len1 = GetFileLenth(file);
                _data1.SetCellText(1, 1, FileUtil.GetFormattedFileLenth(len1));

                using (var _fs = File.OpenRead(file))
                {
                    var _book = new HSSFWorkbook(_fs);
                    var sht = _book.GetSheetAt(0);
                    nRec1=sht.LastRowNum + 1;
                    _data1.SetCellText(0, 1, nRec1.ToString());
                }
            }
            file = System.IO.Path.Combine(qsPath, rf.Xzbm + rf.Year + ".mdb");
            var len2 = GetFileLenth(file);
            _data1.SetCellText(1, 2, FileUtil.GetFormattedFileLenth(len2));

            _data1.SetCellText(1, 3, FileUtil.GetFormattedFileLenth(len1+ len2));

            _data5.SetCellText(1, 1, FileUtil.GetFormattedFileLenth(len1+len2));

            return len1 + len2;
        }
        private long RefreshGrid2(RootFolder rf)
        {
            long n = 0;
            var qsPath = System.IO.Path.Combine(rf.Path, "权属数据");
            using (var db = DBAccess.Open(System.IO.Path.Combine(qsPath, rf.Xzbm + rf.Year + ".mdb")))
            {
                var sa = new string[] { "FBF", "CBF", "CBF_JTCY" ,"CBDKXX","CBHT","CBJYQZDJB","CBJYQZ", "CBJYQZ_QZBF", "CBJYQZ_QZHF", "QSLYZLFJ" };
                for(int i = 0; i < sa.Length; ++i)
                {
                    int len = QueryCount(db, sa[i]);
                    _data2.SetCellText(0, i+1, len.ToString());
                    n += len;
                }
                _data2.SetCellText(0, sa.Length +1, n.ToString());
            }
            _data1.SetCellText(0, 2, n.ToString());
            _data5.SetCellText(0, 1, n.ToString());
            var m = SafeConvertAux.ToInt32(_data1.GetCellText(0, 1)) + SafeConvertAux.ToInt32(_data1.GetCellText(0, 2));
            _data1.SetCellText(0, 3, m.ToString());
            return n;
        }
        private long RefreshGrid3(RootFolder rf)
        {
            long lSumLen = 0;
            int nSumRecord = 0;
            var qsPath = System.IO.Path.Combine(rf.Path, "矢量数据");
            var sa = new string[] { "KZD", "QYJX", "XJXZQ", "XJQY",  "CJQY", "ZJQY", "DZDW", "XZDW", "MZDW", "DK", "JZD", "JZX", "JBNTBHQ" };
            for(int i = 0; i < sa.Length; ++i)
            {
                var shpFile = System.IO.Path.Combine(qsPath,sa[i]+ rf.Xzbm + rf.Year + ".shp");
                var dbfFile = System.IO.Path.Combine(qsPath, sa[i] + rf.Xzbm + rf.Year + ".dbf");
                var len = GetFileLenth(shpFile)+GetFileLenth(dbfFile);
                lSumLen += len;
                _data3.SetCellText(1, i + 1, FileUtil.GetFormattedFileLenth(len));
                using (var shp = new ShapeFile())
                {
                    shp.Open(shpFile,"rb",false);
                    try
                    {
                        var cnt = shp.GetRecordCount();
                        nSumRecord += cnt;
                        _data3.SetCellText(0, i + 1, cnt.ToString());
                    }catch(Exception e)
                    {
                        Console.WriteLine("err:" + e.Message);
                    }
                }
            }
            _data3.SetCellText(1, sa.Length + 1, FileUtil.GetFormattedFileLenth(lSumLen));
            _data3.SetCellText(0, sa.Length + 1,nSumRecord.ToString());
            updateColumnWidth(grid1);
            _data5.SetCellText(0, 2, nSumRecord.ToString());
            _data5.SetCellText(1, 2, FileUtil.GetFormattedFileLenth(lSumLen));
            return lSumLen;

        }
        private long RefreshGrid4(RootFolder rf)
        {
            var sa = new string[] { "汇总表格","其他资料","图件","文字报告" };
            int nFolderCount = 0;
            int nFileCount = 0;
            long nLenCount = 0;
            for (int i = 0; i < sa.Length; ++i)
            {
                var sPath = System.IO.Path.Combine(rf.Path, sa[i]);
                int n1 = Directory.Exists(sPath) ? 1:0;
                FileUtil.EnumFolder(sPath, di =>
                {
                    ++n1;
                    return true;
                });
                var col = i + 1;
                _data4.SetCellText(0, col, n1.ToString());
                int i1 = 0;
                long iLen = 0;
                FileUtil.EnumFiles2(sPath, fi =>
                    {
                        iLen += fi.Length;
                        ++i1;
                        return true;
                    });
                _data4.SetCellText(1, col, i1.ToString());
                _data4.SetCellText(2, col, FileUtil.GetFormattedFileLenth(iLen));
                nFolderCount += n1;
                nFileCount += i1;
                nLenCount += iLen;
            }

            string sFileSize = FileUtil.GetFormattedFileLenth(nLenCount);

            _data4.SetCellText(0, sa.Length+1,nFolderCount.ToString());
            _data4.SetCellText(1, sa.Length + 1, nFileCount.ToString());
            _data4.SetCellText(2, sa.Length + 1, sFileSize);

            _data5.SetCellText(0, 3, nFileCount.ToString());
            _data5.SetCellText(1, 3, sFileSize);

            int cnt = 0;
            for(int i = 1; i < 3; ++i)
            {
                cnt += SafeConvertAux.ToInt32(_data5.GetCellText(0, i));
            }
            _data5.SetCellText(0, 4, cnt.ToString());

            return nLenCount;
        }
        private static long GetFileLenth(string file)
        {
            if (File.Exists(file))
            {
                var fi = new System.IO.FileInfo(file);
                return fi.Length;
                //var len = (long)(fi.Length/1024);
                //if (len < 1024)
                //{
                //    return len + "KB";
                //}
                //len = (long)(len / 1024);
                //if (len < 1024)
                //{
                //    return len + "MB";
                //}
                //len = (long)(len / 1024);
                //if (len < 1024)
                //{
                //    return len + "GB";
                //}
            }
            return 0;
        }
        private static void InitGrid1(Agro.LibCore.UI.GridView grid,GridData data)
        {
            data.SetCellText(0, 0, "记录数");
            data.SetCellText(1, 0, "大小");
            var cols = new Column[]
            {
                new Column(""),
                new Column("权属单位代码表"),
                new Column("XXXXXX.mdb"),
                new Column("合计"),
            };
            grid.Cols = cols.Length;
            for(int i = 0; i < cols.Length; ++i)
            {
                grid.SetColLabel(i, cols[i].Name);
            }
            grid.Rows = 2;
            grid.OnGetCellText += (r, c) =>
            {
                return data.GetCellText(r, c);
            };
            grid.SetColWidth(0, 100);
            grid.ShowGrid = false;
            grid.ShowRowHeader = false;
        }
        private static void InitGrid2(Agro.LibCore.UI.GridView grid,GridData data)
        {
            data.SetCellText(0, 0, "记录数");
            //data.SetCellText(1, 0, "大小");
            var cols = new Column[]
            {
                new Column(""),
                new Column("发包方"),
                new Column("承包方"),
                new Column("家庭成员"),
                new Column("承包地块信息"),
                new Column("承包合同"),
                new Column("承包经营权证登记簿"),
                new Column("承包经营权证"),
                new Column("权证补发"),
                new Column("权证换发"),
                new Column("权属来源资料附件"),
                new Column("合计"),
            };
            grid.Cols = cols.Length;
            for (int i = 0; i < cols.Length; ++i)
            {
                grid.SetColLabel(i, cols[i].Name);
            }
            updateColumnWidth(grid);
            grid.Rows = 1;
            grid.OnGetCellText += (r, c) =>
            {
                return data.GetCellText(r, c);
            };
            grid.SetColWidth(0, 100);
            grid.ShowGrid = false;
            grid.ShowRowHeader = false;
        }

        private static void InitGrid3(Agro.LibCore.UI.GridView grid, GridData data)
        {
            data.SetCellText(0, 0, "记录数");
            data.SetCellText(1, 0, "大小");
            var cols = new Column[]
            {
                new Column(""),
                new Column("控制点"),
                new Column("区域界线"),
                new Column("县级行政区"),
                new Column("乡级区域"),
                new Column("村级区域"),
                new Column("组级区域"),
                new Column("点状地物"),
                new Column("线状地物"),
                new Column("面状地物"),
                new Column("地块"),
                new Column("界址点"),
                new Column("界址线"),
                new Column("基本农田保护区"),
                new Column("合计"),
            };
            grid.Cols = cols.Length;
            for (int i = 0; i < cols.Length; ++i)
            {
                grid.SetColLabel(i, cols[i].Name);
            }
            updateColumnWidth(grid);
            grid.Rows = 2;
            grid.OnGetCellText += (r, c) =>
            {
                //if (c == 0)
                //{
                //    if (r == 0)
                //        return "记录数";
                //    if (r == 1)
                //        return "大小";
                //}
                return data.GetCellText(r,c);
            };
            grid.SetColWidth(0, 100);
            grid.ShowGrid = false;
            grid.ShowRowHeader = false;
        }

        private static void InitGrid4(Agro.LibCore.UI.GridView grid,GridData data)
        {
            data.SetCellText(0, 0, "目录数");
            data.SetCellText(1, 0, "文件数");
            data.SetCellText(2, 0, "大小");
            var cols = new Column[]
            {
                new Column(""),
                new Column("汇总表格"),
                new Column("其他资料"),
                new Column("图件"),
                new Column("文字报告"),
                new Column("合计"),
            };
            grid.Cols = cols.Length;
            for (int i = 0; i < cols.Length; ++i)
            {
                grid.SetColLabel(i, cols[i].Name);
            }
            updateColumnWidth(grid);
            grid.Rows = 3;
            grid.OnGetCellText += (r, c) =>
            {
                return data.GetCellText(r,c);
            };
            grid.SetColWidth(0, 100);
            grid.ShowGrid = false;
            grid.ShowRowHeader = false;
        }

        private static void InitGrid5(Agro.LibCore.UI.GridView grid, GridData data)
        {
            data.SetCellText(0, 0, "记录数");
            data.SetCellText(1, 0, "大小");
            var cols = new Column[]
            {
                new Column(""),
                new Column("权属数据"),
                new Column("矢量数据"),
                new Column("其他资料"),
                new Column("合计"),
            };
            grid.Cols = cols.Length;
            for (int i = 0; i < cols.Length; ++i)
            {
                grid.SetColLabel(i, cols[i].Name);
            }
            updateColumnWidth(grid);
            grid.Rows = 2;
            grid.OnGetCellText += (r, c) =>
            {
                return data.GetCellText(r,c);
            };
            grid.SetColWidth(0, 100);
            grid.ShowGrid = false;
            grid.ShowRowHeader = false;
        }

        private static void updateColumnWidth(Agro.LibCore.UI.GridView grid,int nMargin=30)
        {
            #region update ColWidth
            var lstColWidth = new double[grid.Cols];
            int rows = Math.Min(50, grid.Rows);
            for (int c = 0; c < lstColWidth.Length; ++c)
            {
                var colLabel = grid.GetColLabel(c);
                lstColWidth[c] = grid.CalcTextWidth(colLabel) + nMargin;
                for (int i = 0; i < rows; ++i)
                {
                    var s = grid.GetCellText(i, c);
                    var wi = Math.Min(250, grid.CalcTextWidth(s) + nMargin);
                    if (lstColWidth[c] < wi)
                    {
                        lstColWidth[c] = wi;
                    }
                }
            }
            for (int c = 0; c < lstColWidth.Length; ++c)
            {
                grid.SetColWidth(c, (int)lstColWidth[c]);
            }
            #endregion

        }
        private static int QueryCount(IWorkspace db,string tableName)
        {
            var sql = "select count(*) from " + tableName;
            return db.QueryOneInt(sql);
        }
    }
}
