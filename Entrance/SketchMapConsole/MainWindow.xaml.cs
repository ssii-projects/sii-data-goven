using Agro.GIS;
using Agro.Library.Common;
using Agro.Module.SketchMap;
using System;
using System.Collections.Generic;
using System.Windows;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common.Repository;
using System.IO;
using System.Text;

namespace SketchMapConsole
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private SkecthMapProperty _prm;
		private readonly Logout _logout = new Logout(null,false);
		public MainWindow()
		{
			InitializeComponent();
		}
		private IFeatureWorkspace GetDB(string cons, string dbType)
		{
			if (dbType == "MySql")
			{
				return MySqlFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);
			}
			return SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);
        }
		/// <summary>
		/// 导出草图
		/// </summary>
		/// <param name="prm"></param>
		/// <param name="djbids"></param>
		public void ExportSketch(SkecthMapProperty prm,string[] djbids,string cons,string dbType)
		{
			_prm = prm;
			//var cons = "Data Source=192.168.0.3;Initial Catalog=agriegov_540302;User ID=sa;Password=ssii@MSSQL#8517";
			//var def = DataSourceConfig.Instance.Load();
			//var cons = def.ConnectionString;
			using (var db = GetDB(cons, dbType))// SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(cons))
			{
				MyGlobal.Connected(db,AppType.DataGoven);
				//if (db == null)
				//{
				//	this.ReportError("数据源无效!");
				//	return;
				//}
				var exporter = new SketchMapExporter(this, null,pageLayout)
				{
					//DeleteJPGFolder = true
				};
				exporter.SetMapProperty(prm);
				DoExport(exporter,djbids);				
			}
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
				//if (concord.Lands == null || concord.Lands.Length == 0)
				//{
				//	ReportWarning($"{concord.CBFMC}[{concord.CBFBM}]无地块数据!");
				//	continue;
				//}
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
					var tmpPath =Path.Combine(filePath, $"{concord.CBFBM}\\");
					ReportInformation($"准备输出JPG路径：{tmpPath}");
					exporter.ExportSketchMapByContractor(concord, lands, filePath, tmpPath,err=>
					{
						if (err != null)
						{
							ReportError(err.Message);
						}
					});
					ReportInformation($"已输出：{concord.CBFMC}[{concord.CBFBM}]");
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
			var djbRepos=DjDjbRepository.Instance;
			var djbEN=djbRepos.Find(t => t.ID == djbid, (c, t) => c(t.CBFBM, t.CBFMC));
			if (djbEN == null)
			{
				throw new Exception($"登记簿ID：{djbid}在数据库中不存在！");
			}
			var cc = new ContractConcord()
			{
				CBFBM =djbEN.CBFBM,// it.CBFBM,
				CBFMC =djbEN.CBFMC// it.CbfMc
			};
			var dkxxs=djbRepos.GetDjDKs(djbid);
			var lst = new List<ContractLand>();
			foreach (var it in dkxxs)
			{
				var c = new ContractLand()
				{
					DKBM =it.DKBM,
					HTMJ = SafeConvertAux.ToDouble(it.HTMJ),// SafeConvertAux.ToDouble(r.GetValue(++i)),
					HTMJM = SafeConvertAux.ToDouble(it.HTMJM),// SafeConvertAux.ToDouble(r.GetValue(++i)),
					IsShared =it.SFQQQG,// r.IsDBNull(++i) ? "" : r.GetString(i),
					DKDZ =it.DKDZ,// GetStr(r, ++i),
					DKNZ =it.DKNZ,// GetStr(r, ++i),
					DKXZ =it.DKXZ,// GetStr(r, ++i),
					DKBZ =it.DKBZ,// GetStr(r, ++i)
				};
				lst.Add(c);
			}
			cc.Lands = lst.ToArray();
			return cc;
		}
		//static string GetStr(IDataReader r, int c)
		//{
		//	return r.IsDBNull(c) ? "" : r.GetString(c);
		//}
		public void ReportInformation(string err)
		{
			//Console.Error.WriteLine(err);
			_logout.WriteInformation(err);
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
		//[DllImport("user32.dll", EntryPoint = "ShowWindow")]
		//public static extern bool ShowWindow(int hwnd, int nCmdShow);

		//public void Load()
		//{
		//	ShowWindow(new System.Windows.Interop.WindowInteropHelper(this).Handle.ToInt32(), 3);
		//}
	}

	public class Logout
	{
		private readonly string logFile;
		public readonly bool IsWriteToFile = true;
		public Logout(string logFile = null,bool isWriteToFile=true)
		{
			IsWriteToFile = isWriteToFile;
			if (isWriteToFile)
			{
				if (logFile == null)
				{
					var logPath = AppDomain.CurrentDomain.BaseDirectory + "log\\";
					if (!Directory.Exists(logPath))
					{
						Directory.CreateDirectory(logPath);
					}
					logFile = Path.Combine(logPath + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt");
				}
				this.logFile = logFile;
				//var filestream = new FileStream(logFile, FileMode.Create);
				//var streamwriter = new StreamWriter(filestream);
				//{
				//	streamwriter.AutoFlush = true;
				//	Console.SetOut(streamwriter);
				//	//Console.SetError(streamwriter);
				//}
			}

			Console.OutputEncoding = Encoding.GetEncoding("GB2312");
			Console.SetOut(TextWriter.Null);
		}
		public void WriteInformation(string msg, bool fWriteHeader = true)
		{
			var str = FormatMsg("info", msg, fWriteHeader);
			WriteMsg(str);
			Console.WriteLine(str);
		}
		public void WriteWarning(string msg, bool fWriteHeader = true)
		{
			var str = FormatMsg("warn", msg, fWriteHeader);
			WriteMsg(str);
			Console.WriteLine(str);
		}
		public void WriteError(string err,bool fWriteHeader=true)
		{
			var msg = FormatMsg("smc_err", err, fWriteHeader);
			WriteMsg(msg);
			Console.Error.WriteLine(msg);
		}

		private string FormatMsg(string tag, string msg, bool fWriteHeader = true) {
			var header = fWriteHeader ? (DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "   ---   ") : "";
			var str = $"[{tag}]{header}{msg}";
			return str;
		}
		//private void WriteText(string tag,string msg, bool fWriteHeader = true)
		//{
		//	var str = FormatMsg(tag, msg, fWriteHeader);
		//	WriteMsg(str);
		//	//if (!IsWriteToFile) return;
		//	//var fileMode = File.Exists(logFile) ? FileMode.Append : FileMode.Create;
		//	//using (var fs = new FileStream(logFile, fileMode, FileAccess.Write))
		//	//using (var sw = new StreamWriter(fs))
		//	//{
		//	//	var str = FormatMsg(tag, msg, fWriteHeader);
		//	//	sw.WriteLine(str);
		//	//}
		//}
		private void WriteMsg(string msg) {
			if (!IsWriteToFile) return;
			
			var fileMode = File.Exists(logFile) ? FileMode.Append : FileMode.Create;
			using (var fs = new FileStream(logFile, fileMode, FileAccess.Write))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine(msg);
			}
		}
	}
}
