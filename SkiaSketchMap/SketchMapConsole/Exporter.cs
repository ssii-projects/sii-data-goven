using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using SketchMapConsole;
using SketchMapConsole.Util;
using SkiaMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SketchMap
{
    internal class Exporter
    {
        private readonly Logout _logout = new();//, false);
        private SkecthMapProperty _prm;
        public void Test(string[] args)
        {
            foreach (var arg in args)
            {
                //Console.WriteLine($"Argument={arg}");
                ReportInformation($"Argument={arg}",false);
            }
            GisGlobal.SymbolFactory = SkiaSymbolFactory.Instance;

            SkiaSymbolFactory.RegisterFontFromEmbeddedResource(EmbeddedResourceUtil.GetFontPath("esri_40.ttf"));
            var prm = new SkecthMapProperty
            {
                SavePdfFormat = true
            };
            _prm = prm;

            string dbType = "MySql";
            string cons = string.Empty;
            string[]? djbids = null;
            foreach (var arg in args)
            {
                var n = arg.IndexOf(':');
                //var sa = arg.Split(':');
                var key = arg.Substring(0, n);// sa[0].ToLower().Trim();
                var val = arg.Substring(n + 1);// sa[1];
                                               //Console.WriteLine($"key={key},value={val}");
                switch (key)
                {
                    case "djbid":
                        djbids = val.Split(',');
                        break;
                    case "path": prm.OutputPath = val; break;
                    case "ztz": prm.DrawPerson = val; break;
                    case "ztrq": prm.DrawDate = DateTime.Parse(val); break;
                    case "shz": prm.CheckPerson = val; break;
                    case "shrq": prm.CheckDate = DateTime.Parse(val); break;
                    case "ztdw": prm.Company = val; break;
                    case "cons": cons = val; break;
                    case "dbType":dbType= val; break;
                }
            }
            if (string.IsNullOrEmpty(cons))
            {
                throw new Exception("命令行参数异常，未传入cons参数");
            }
            if (string.IsNullOrEmpty(prm.OutputPath))
            {
                throw new Exception("命令行参数异常，未传入path参数");
            }
            if (djbids == null || djbids.Length == 0)
            {
                throw new Exception("命令行参数异常，未传入djbid参数");
            }


            if (!Directory.Exists(prm.OutputPath))
            {
                Directory.CreateDirectory(prm.OutputPath);
            }


            //cons = "server=localhost;uid=root;pwd=123456;Database=xlqnjq;Charset=utf8;AllowLoadLocalInfile=true;AllowUserVariables=true;Connection Timeout=3000";
            using var db = GetDB(cons,dbType);// MySqlFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);
            MyGlobal.Workspace = db;

            var exporter = new SketchMapExporter(_logout)// this, null, pageLayout)
            {
                //DeleteJPGFolder = true
            };
            exporter.SetMapProperty(prm);
            DoExport(exporter, djbids);

        }

        private IFeatureWorkspace GetDB(string cons, string dbType)
        {
            if (dbType == "Sqlite")
            {
                return SqliteFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);
            }
            return MySqlFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);
        }

        private void DoExport(SketchMapExporter exporter, string[] djbids)
        {
            var concords = new List<ContractConcord>();
            foreach (var djbid in djbids)
            {
                try
                {
                    var con = ToContractConcord(djbid);
                    if (con.Lands == null || con.Lands.Length == 0)
                    {
                        ReportWarning($"{con.CBFMC}[{con.CBFBM}]无地块数据!");
                        continue;
                    }
                    concords.Add(con);
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
            }

            foreach (var concord in concords)
            {
                var cbfBM = DataExchange.InitalizeSenderCode(concord);
                var lands = SketchMapUtil.QueryDKByCbfbm(cbfBM, concord.Lands, NotCancelTracker.Instance, false);
                if (lands.Count == 0)
                {
                    ReportWarning($"{concord.CBFMC}[{concord.CBFBM}]无地块图形数据!");
                    continue;
                }

                var filePath = _prm.OutputPath;
                try
                {
                    ReportInformation($"准备输出：{concord.CBFMC}[{concord.CBFBM}]");
                    var tmpPath = Path.Combine(filePath, $"{concord.CBFBM}");
                    ReportInformation($"准备输出JPG路径：{tmpPath}");
                    var outFile=exporter.ExportSketchMapByContractor(concord, lands, filePath, tmpPath, err =>
                    {
                        if (err != null)
                        {
                            ReportError(err.Message);
                        }
                    });
                    ReportInformation($"已输出：{concord.CBFMC}[{concord.CBFBM}]-{outFile}");
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
                GC.Collect();
                Console.WriteLine(concord.CBFMC + "地块示意图成功导出!");
            }
        }

        private static ContractConcord ToContractConcord(string djbid)
        {
            var db = MyGlobal.Workspace;
            var djbRepos = DjDjbRepository.Instance;
            var djbEN = djbRepos.Find(t => t.ID == djbid, (c, t) => c(t.CBFBM, t.CBFMC));
            if (djbEN == null)
            {
                throw new Exception($"登记簿ID：{djbid}在数据库中不存在！");
            }
            var cc = new ContractConcord()
            {
                CBFBM = djbEN.CBFBM,
                CBFMC = djbEN.CBFMC
            };
            var dkxxs = djbRepos.GetDjDKs(djbid);
            var lst = new List<ContractLand>();
            foreach (var it in dkxxs)
            {
                var c = new ContractLand()
                {
                    DKBM = it.DKBM,
                    HTMJ = SafeConvertAux.ToDouble(it.HTMJ),
                    HTMJM = SafeConvertAux.ToDouble(it.HTMJM),
                    IsShared = it.SFQQQG,
                    DKDZ = it.DKDZ,
                    DKNZ = it.DKNZ,
                    DKXZ = it.DKXZ,
                    DKBZ = it.DKBZ,
                };
                lst.Add(c);
            }
            cc.Lands = lst.ToArray();
            return cc;
        }

        public void ReportInformation(string err,bool fWriteToConsole = true)
        {
            //Console.Error.WriteLine(err);
            _logout.WriteInformation(err,true,fWriteToConsole);
        }
        public void ReportError(string err)
        {
            //Console.Error.WriteLine(err);
            _logout.WriteError(err);
        }
        private void ReportWarning(string msg)
        {
            _logout.WriteWarning(msg);
        }
    }

    public class Logout
    {
        private readonly string? logFile;
        public readonly bool IsWriteToFile = true;
        public Logout(string? logFile = null, bool isWriteToFile = true)
        {
            IsWriteToFile = isWriteToFile;
            if (isWriteToFile)
            {
                if (logFile == null)
                {
                    var logPath =Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "log");
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    else
                    {
                        var now=DateTime.Now;
                        var lst=new List<string>();
                        FileUtil.EnumFiles(logPath, fi =>
                        {
                            var n=fi.Name.LastIndexOf('.');
                            var s=fi.Name.Substring(0, n);
                            var sa=s.Split(' ');
                            if (sa.Length > 0)
                            {
                                s = sa[0];
                            }
                            if(DateTime.TryParse(s, out DateTime dt))
                            {
                                if (dt.Year != now.Year || dt.Month != now.Month || dt.Day != now.Day)
                                {
                                    lst.Add(fi.FullName);
                                }
                            }
                            return true;
                        });
                        foreach(var fi in lst)
                        {
                            try
                            {
                                File.Delete(fi);
                            }catch { }
                        }
                    }
                    logFile = Path.Combine(logPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff") + ".txt");
                    if (File.Exists(logFile))
                    {
                        try
                        {
                            File.Delete(logFile);
                        }
                        catch { }
                    }
                }
                this.logFile = logFile;
            }

            Console.OutputEncoding = Encoding.UTF8;// Encoding.GetEncoding(Encoding.UTF8);
            //Console.SetOut(TextWriter.Null);
        }
        public void WriteInformation(string msg, bool fWriteHeader = true, bool fWriteToConsole = true)
        {
            var str = FormatMsg("info", msg, fWriteHeader);
            WriteMsg(str);
            if (fWriteToConsole)
            {
                Console.WriteLine(str);
            }
        }
        public void WriteWarning(string msg, bool fWriteHeader = true)
        {
            var str = FormatMsg("warn", msg, fWriteHeader);
            WriteMsg(str);
            Console.WriteLine(str);
        }
        public void WriteError(string err, bool fWriteHeader = true)
        {
            var msg = FormatMsg("smc_err", err, fWriteHeader);
            WriteMsg(msg);
            Console.Error.WriteLine(msg);
        }

        private string FormatMsg(string tag, string msg, bool fWriteHeader = true)
        {
            var header = fWriteHeader ? (DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "   ---   ") : "";
            var str = $"[{tag}]{header}{msg}";
            return str;
        }
        private void WriteMsg(string msg)
        {
            if (!IsWriteToFile||string.IsNullOrEmpty(logFile)) return;

            var fileMode = File.Exists(logFile) ? FileMode.Append : FileMode.Create;
            using var fs = new FileStream(logFile, fileMode, FileAccess.Write);
            using var sw = new StreamWriter(fs);
            sw.WriteLine(msg);
        }
    }
}
