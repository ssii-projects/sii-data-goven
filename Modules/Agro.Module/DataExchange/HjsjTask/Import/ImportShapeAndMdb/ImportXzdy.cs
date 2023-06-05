using Agro.GIS;
using Agro.Library.Common;
using Agro.Library.Model;
using Agro.Module.DataExchange;
using Agro.LibCore;
//using Microsoft.SqlServer.Types;
//using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
using Agro.LibCore.Database;
using GeoAPI.Geometries;
using Agro.LibCore.NPIO;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	public class DYCode
	{
		public string ID { get { return code; } }
		public string sjid
		{
			get
			{
				if (sjCode == null)
				{
					return null;// Guid.Empty;
				}
				return sjCode.ID;
			}
		}
		public string code;
		/// <summary>
		///  名称
		/// </summary>
		public string mc;
		/// <summary>
		/// 扩展名称
		/// </summary>
		public string kzmc
		{
			get
			{
				if (sjCode == null)
				{
					return null;
				}
				return sjCode.qmc;
			}
		}
		/// <summary>
		/// 全名称
		/// </summary>
		public string qmc
		{
			get
			{
				var lst = new List<string>();
				var sj = this;
				while (sj != null)
				{
					lst.Add(sj.mc);
					sj = sj.sjCode;
				}
				string s = null;
				for (int i = lst.Count - 1; i >= 0; --i)
				{
					if (s == null)
					{
						s = lst[i];
					}
					else
					{
						s += lst[i];
					}
				}
				return s;
			}
		}
		/// <summary>
		/// 级别
		/// </summary>
		public eZoneLevel jb;
		/// <summary>
		/// 上级代码
		/// </summary>
		public DYCode sjCode;
		public DYCode(eZoneLevel jb_)
		{
			jb = jb_;
		}
	}

	/// <summary>
	/// 行政区代码
	/// </summary>
	public class DYCodes
	{
		/// <summary>
		/// [行政区代码，DYCode]
		/// </summary>
		public readonly Dictionary<string, DYCode> _dic = new Dictionary<string, DYCode>();
		/// <summary>
		/// 国家代码
		/// </summary>
		public DYCode _chinaCode;
		/// <summary>
		/// 省级代码
		/// </summary>
		public DYCode _shengCode;
		/// <summary>
		/// 市级代码
		/// </summary>
		public DYCode _shiCode;
		//private readonly Dictionary<string, string> _dic = new Dictionary<string, string>();
		public readonly HashSet<string> _existCodes = new HashSet<string>();
		private void Clear()
		{
			_dic.Clear();
			_shengCode = null;
			_shiCode = null;
			_existCodes.Clear();
		}
		public string loadCodes(HJDataRootPath prm)
		{
			string err = null;
			Clear();

			var dic = new Dictionary<string, DYCode>();


			DYCode xianCode = null;//县代码
			DYCode shiCode = null;
			DYCode shengCode = null;

			try
			{
				#region 从.xls文件中读取原始数据->dic（未加工）
				//using (var fs = File.OpenRead(prm.qsXlsFileName))   //打开myxls.xls文件
				using (var sheet = new NPIOSheet())
				{
					sheet.Open(prm.qsXlsFileName);
					//var wk = new HSSFWorkbook(fs);   //把xls文件中的数据写入wk中
					//var sheet = wk.GetSheetAt(0);   //读取当前表数据
					//Console.WriteLine($"sheet.LastRowNum={sheet.LastRowNum}");
					for (int j = 1; j < sheet.Rows; j++)  //LastRowNum 是当前表的总行数
					{
						//var row = sheet.GetRow(j);  //读取当前行数据
						//if (row != null)
						{
							//var c0 = row.GetCell(0);
							//var c1 = row.GetCell(1);
							//if (c0 != null && c1 != null)
							{
								var code = sheet.GetCellText(j, 0).Trim();// c0.ToString().Trim();//.TrimEnd('0');
								if (string.IsNullOrEmpty(code))
								{
									continue;
								}
								var mc = sheet.GetCellText(j, 1);// c1.ToString().Trim();
								if (string.IsNullOrEmpty(mc))
								{
									continue;
								}
								var fZu = !code.EndsWith("00");//是否组级代码
								var c = new DYCode(eZoneLevel.Unknown);
								if (!code.EndsWith("00"))
								{
									c.jb = eZoneLevel.Group;
								}
								else if (!code.EndsWith("00000"))
								{
									c.jb = eZoneLevel.Village;
								}
								else if (!code.EndsWith("00000000"))
								{
									c.jb = eZoneLevel.Town;
								}
								else if (!code.EndsWith("0000000000"))
								{
									c.jb = eZoneLevel.County;
									xianCode = c;
								}
								else if (!code.EndsWith("000000000000"))
								{
									c.jb = eZoneLevel.City;
									shiCode = c;
								}
								else
								{
									c.jb = eZoneLevel.Province;
									shengCode = c;
								}
								c.code = code;
								c.mc = mc;
								dic[code] = c;
							}
						}
					}
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw ex;
			}
			Console.WriteLine($"从文件{prm.qsXlsFileName}读取数据{dic.Count}条");

			_shiCode = shiCode;
			_shengCode = shengCode;

			if (xianCode != null)
			{
				if (shiCode == null)
				{
					shiCode = new DYCode(eZoneLevel.City)
					{
						code = xianCode.code.Substring(0, 4),
					};
					shiCode.mc = RegionCodeUtil.GetCityNameByCode(shiCode.code);
					shiCode.code += "0000000000";
					dic[shiCode.code] = shiCode;
					_shiCode = shiCode;
					shiCode.sjCode = shengCode;
				}
			}

			if (shiCode != null)
			{
				if (shengCode == null)
				{
					shengCode = new DYCode(eZoneLevel.Province)
					{
						code = shiCode.code.Substring(0, 2)
					};
					shengCode.mc = RegionCodeUtil.GetProviceNameByCode(shengCode.code);// mc;
					shengCode.code += "000000000000";
					dic[shengCode.code] = shengCode;
					_shengCode = shengCode;
					_shiCode.sjCode = shengCode;
				}
			}

			try
			{
				#region 建立上下级关系

				foreach (var kv in dic)
				{
					var code = kv.Key;
					var c = kv.Value;

					string sj = null;
					if (c.jb == eZoneLevel.Group)
					{
						sj = code.Substring(0, 12) + "00";
					}
					else if (c.jb == eZoneLevel.Village)
					{
						sj = code.Substring(0, 9) + "00000";
					}
					else if (c.jb == eZoneLevel.Town)
					{
						sj = code.Substring(0, 6) + "00000000";
					}
					else if (c.jb == eZoneLevel.County)
					{
						sj = code.Substring(0, 4) + "0000000000";
					}
					else if (c.jb == eZoneLevel.City)
					{
						sj = code.Substring(0, 2) + "000000000000";
						//shiCode = c;
					}/*else if (c.jb == eZoneLevel.Province)
                {
                    shengCode = c;
                }*/

					if (sj != null)
					{
						if (!dic.TryGetValue(sj, out DYCode sjCode))
						{
							if (c.jb == eZoneLevel.Group)
							{
								err = "找不到 组[" + c.code + "," + c.mc + "]的上级单位";
								return err;
							}
							else if (c.jb == eZoneLevel.Village)
							{
								err = "找不到 村[" + c.code + "," + c.mc + "]的上级单位";
								return err;
							}
							else if (c.jb == eZoneLevel.Town)
							{
								err = "找不到 乡/镇[" + c.code + "," + c.mc + "]的上级单位";
								return err;
							}
							else if (c.jb == eZoneLevel.County)
							{
								var shiMc = RegionCodeUtil.GetCityNameByCode(sj);
								if (shiMc != null)
								{
									//if (sjCode != null)
									//{
									sjCode = new DYCode(eZoneLevel.City)
									{
										code = c.code.Substring(0, 4) + "0000000000",
										mc = shiMc// getShiMc(sjCode.code)
									};
									shiCode = sjCode;
									c.sjCode = sjCode;
								}
								else
								{
									sj = null;
								}
							}
							else
							{
								sj = null;
							}
						}
						if (sj != null)
						{
							c.sjCode = sjCode;
							//if (c.mc.StartsWith(sjCode.qmc))
							//{
							//	c.mc = c.mc.Substring(sjCode.qmc.Length);
							//}
							//else if (c.mc.StartsWith(sjCode.mc))
							//{
							//	c.mc = c.mc.Substring(sjCode.mc.Length);
							//}else
							//{
							//	Console.WriteLine("c.mc=" + c.mc + ",sj.mc=" + sjCode.mc);
							//}
						}
					}
				}
				#endregion

				#region 修正名称
				if (true)
				{
					var lstCode = dic.Values.ToList();
					lstCode.Sort((a, b) =>
					{
						return a.jb < b.jb ? -1 : 1;
					});
					foreach (var c in lstCode)
					{
						var sjCode = c.sjCode;
						if (sjCode != null)
						{
							if (c.mc.StartsWith(sjCode.mc))
							{
								c.mc = c.mc.Substring(sjCode.mc.Length);
							}
							else if (c.mc.StartsWith(sjCode.qmc))
							{
								c.mc = c.mc.Substring(sjCode.qmc.Length);
							}
							//else
							//{
							//	Console.WriteLine("c.mc=" + c.mc + ",sj.mc=" + sjCode.mc);
							//}
						}
					}
				}
				#endregion
			}
			catch (Exception ex)
			{
				throw ex;
			}



			#region 去掉尾部的0
			foreach (var kv in dic)
			{
				//var code = kv.Key;
				var c = kv.Value;
				switch (c.jb)
				{
					case eZoneLevel.Village: c.code = c.code.Substring(0, 12); break;
					case eZoneLevel.Town: c.code = c.code.Substring(0, 9); break;
					case eZoneLevel.County: c.code = c.code.Substring(0, 6); break;
					case eZoneLevel.City: c.code = c.code.Substring(0, 4); break;
					case eZoneLevel.Province: c.code = c.code.Substring(0, 2); break;
				}
			}
			#endregion

			foreach (var kv in dic)
			{
				_dic[kv.Value.code] = kv.Value;
			}
			return err;
		}

	}

	/// <summary>
	/// 导入行政地域
	/// </summary>
	public class ImportXzdy : ImportTableBase, IImportTable
	{
		class RowItem
		{
			public int RowIdx;
			public IGeometry Shp;
		}
		private readonly DYCodes _dyCodes = new DYCodes();
		private readonly HashSet<string> _setXzqBms = new HashSet<string>();
		private readonly Dictionary<string, RowItem> _dicBm2RowIndex = new Dictionary<string, RowItem>();
		protected readonly bool _fCheckDataExist;
		public ImportXzdy(bool fCheckDataExist = true) : base(TableNameConstants.XZQH_XZDY)
		{
			this._fCheckDataExist = fCheckDataExist;
		}
		#region IImportTable
		public void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
		{
			var db = prm.Workspace;
			int cnt = CalcShapeCount(prm) + 2;

			base.RecordCount = 0;

			using (var dt = new DataTable(TableNameConstants.XZQH_XZDY))
			{
				if (_fCheckDataExist && !PreImportCheck(db, reportInfo))
				{
					return;
				}
				_dicBm2RowIndex.Clear();

				var err = _dyCodes.loadCodes(prm);
				if (err != null)
				{
					reportInfo.reportError("错误：" + err);
					return;
				}


				dt.Columns.Add("SHAPE", typeof(IGeometry));
				dt.Columns.Add("ID", typeof(string));
				dt.Columns.Add("BM", typeof(string));
				dt.Columns.Add("MC", typeof(string));
				dt.Columns.Add("JB", typeof(int));
				dt.Columns.Add("SJID", typeof(string));
				dt.Columns.Add("KZMC", typeof(string));
				dt.Columns.Add("KZBM", typeof(string));


				var progress = new ProgressReporter(reportInfo.reportProgress, cnt);

				var srid = db.GetSRID(TableName);
				var spatialReference = SpatialReferenceFactory.CreateFromEpsgCode(srid);
				var tableMeta = db.QueryTableMeta(TableName);

				var dw = Stopwatch.StartNew();
				try
				{
					TaskImportUtil.LoadXzqBms(db, _setXzqBms);
					AppendRow(_dyCodes._chinaCode, dt);
					AppendRow(_dyCodes._shengCode, dt);
					AppendRow(_dyCodes._shiCode, dt);

					var qf = new QueryFilter();
					var sa = new string[] { "XJXZQ", "XJQY", "CJQY", "ZJQY" };
					foreach (var s in sa)
					{
						if (!prm.dicShp.ContainsKey(s) || prm.dicShp[s] == null)
						{
							if (s == "ZJQY")
								continue;
						}
						var sShpFile = prm.dicShp[s];
						using (var shpFc = ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(sShpFile))
						{
							if (srid == 0)
							{
								if (shpFc.SpatialReference == null)
								{
									reportInfo.reportError($"无法读取文件：{sShpFile}的坐标系信息！" );
									return;
								}
								spatialReference = shpFc.SpatialReference;
								srid = shpFc.SpatialReference.AuthorityCode;
								RepairFeatureClassComments(srid);								
							}

							int iCodeField = -1;
							var nRecords = shpFc.Count(null);
							shpFc.Search(qf, r =>
							{
								progress.Step();
								if (iCodeField == -1)
								{
									var codeFieldName = GetShapeCodeFieldName(s);
									iCodeField = r.Fields.FindField(codeFieldName);
								}
								var sCode = SafeConvertAux.ToStr(r.GetValue(iCodeField));
								if (!string.IsNullOrEmpty(sCode))
								{
									if (_setXzqBms.Contains(sCode))
									{
										return true;
									}
									if (_dyCodes._dic.TryGetValue(sCode, out DYCode dyCode))
									{
										var geo = (r as IFeature).Shape;
										AppendRow(dyCode, dt, geo);
									}
								}
								return true;
							});
						}
					}

					#region 查找有属性无图形的单位并追加到数据库
					foreach (var code in _dicBm2RowIndex.Keys)
					{
						_dyCodes._dic.Remove(code);
					}
					foreach (var kv in _dyCodes._dic)
					{
						var jb = kv.Value.jb;
						if (!(jb == eZoneLevel.State || jb == eZoneLevel.Province || jb == eZoneLevel.City))
						{
							AppendRow(kv.Value, dt);
						}
					}
					#endregion
					foreach (var it in _dicBm2RowIndex.Values)
					{
						var r = dt.Rows[it.RowIdx];
						var g = it.Shp;
						if (g != null)
						{
							//var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
							//var sg = SqlGeometry.STGeomFromWKB(bc, srid);
							//r[0] = sg;
							r[0] = g;
						}
					}
					db.BeginTransaction();
					db.SqlBulkCopyByDatatable(tableMeta, dt);

					dt.Rows.Clear();
					db.Commit();
					progress.ForceFinish();
					dw.Stop();
					reportInfo.reportInfo($"结束导入{TableName}，共导入数据{base.RecordCount}条，耗时：{ dw.Elapsed}");
				}
				catch (Exception ex)
				{
					reportInfo.reportError("错误：" + ex.Message);
					db.Rollback();
				}
			}
		}
		#endregion

		private int CalcShapeCount(HJDataRootPath prm)
		{
			int n = 0;
			var sa = new string[] { "XJXZQ", "XJQY", "CJQY", "ZJQY" };
			foreach (var s in sa)
			{
				using (var shp = new ShapeFile())
				{
					if (!prm.dicShp.ContainsKey(s) || prm.dicShp[s] == null)
					{
						if (s == "ZJQY")
							continue;
						else
						{
							throw new Exception("矢量数据下找不到以" + s + "开头的文件!");
						}
					}
					var sShpFile = prm.dicShp[s];
					shp.Open(sShpFile);
					n += shp.GetRecordCount();
				}
			}
			return n;
		}

		private void AppendRow(DYCode c, DataTable dt, IGeometry shp = null)
		{
			if (c != null)
			{
				if (_setXzqBms.Contains(c.code))
				{
					return;
				}
				if (_dicBm2RowIndex.TryGetValue(c.code, out RowItem it))
				{
					if (shp != null)
					{
						var r = dt.Rows[it.RowIdx];
						var g = it.Shp;
						if (g != null)
						{
							var lstPgn = new List<IPolygon>();
							GeometryUtil.EnumPolygon(g, pgn => lstPgn.Add(pgn));
							GeometryUtil.EnumPolygon(shp, pgn => lstPgn.Add(pgn));
							var mpgn = GeometryUtil.MakeMultiPolygon(lstPgn.ToArray(), shp.GetSpatialReference());
							it.Shp = mpgn;
						}
						else { it.Shp = shp; }
					}
				}
				else
				{
					_dicBm2RowIndex[c.code] = new RowItem() { RowIdx = dt.Rows.Count, Shp = shp };
					var r = dt.NewRow();
					int i = -1;
					r[++i] = null;// shp;
					r[++i] = c.ID.ToString();
					r[++i] = c.code;
					r[++i] = string.IsNullOrEmpty(c.mc) ? " " : c.mc;
					r[++i] = c.jb;
					r[++i] = c.sjCode?.ID.ToString();
					r[++i] = c.kzmc;
					r[++i] = c.sjCode?.code;
					dt.Rows.Add(r);
				}
				++base.RecordCount;
			}
		}

		private string GetShapeCodeFieldName(/*ShapeFile shp*/string prefix)
		{
			string codeName = null;
			switch (prefix)
			{
				case "XJXZQ":
					codeName = "XZQDM";
					break;
				case "XJQY":
					codeName = "XJQYDM";
					break;
				case "CJQY":
					codeName = "CJQYDM";
					break;
				case "ZJQY":
					codeName = "ZJQYDM";
					break;
			}
			return codeName;
			//var n = shp.Fields.FindField(codeName);
			//return n;
		}

		/// <summary>
		/// 修复空间表的注释
		/// </summary>
		private void RepairFeatureClassComments(int srid)
		{
      var dic = new Dictionary<string, eGeometryType>
      {
        [DLXX_XZDY.GetTableName()] = eGeometryType.eGeometryPolygon,
        [DLXX_DK.GetTableName()] = eGeometryType.eGeometryPolygon,
        [DLXX_DK_JZD.GetTableName()] = eGeometryType.eGeometryPoint,
        [DLXX_DK_JZX.GetTableName()] = eGeometryType.eGeometryPolyline,
        ["DLXX_DZDW"] = eGeometryType.eGeometryPoint,
        ["DLXX_KZD"] = eGeometryType.eGeometryPoint,
        ["DLXX_JBNTBHQ"] = eGeometryType.eGeometryPolygon,
        ["DLXX_MZDW"] = eGeometryType.eGeometryPolygon,
        ["DLXX_QYJX"] = eGeometryType.eGeometryPolyline,
        ["DLXX_XZDW"] = eGeometryType.eGeometryPolyline,
        ["DLXX_ZJ"] = eGeometryType.eGeometryPoint
      };

      foreach (var kv in dic)
			{
				RepairFeatureClassComments(kv.Key, srid, kv.Value);
			}
		}
		private void RepairFeatureClassComments(string tableName,int srid, eGeometryType geoType)
		{
			if (MyGlobal.Workspace is SqlServer db)
			{
				db.UpdateShapeFieldComments(tableName, "SHAPE", srid, geoType);
			}else if(MyGlobal.Workspace is DBMySql mySql)
			{
                mySql.UpdateShapeFieldComments(tableName, "SHAPE", srid, geoType);
            }
		}
	}


	public class ImportXzdyExp : ImportTableBase, IImportTable
	{
		class Xzdy
		{
			public string ID;
			public string BM;
			public short JB;
			public string SJID;
			//public readonly List<Xzdy> Children = new List<Xzdy>();
		}
		class XzdyExp
		{
			//public string ID;
			public string SJID;
			public short SJJB;
			public string SJBM;
			public string ZJID;
			public short ZJJB;
			public string ZJBM;
		}


		class XzdyList : List<Xzdy>
		{
			private readonly Dictionary<string, Xzdy> _dicID2Entity = new Dictionary<string, Xzdy>();
			private readonly HashSet<string> _ids = new HashSet<string>();

			public void Load(IWorkspace db)
			{
				var sql = "select ID,BM,JB,SJID from DLXX_XZDY where JB<7 order by JB desc";
				db.QueryCallback(sql, r =>
				{
					var en = new Xzdy()
					{
						ID = r.GetString(0),
						BM = r.GetString(1),
						JB = SafeConvertAux.ToShort(r.GetValue(2))
					};
					if (!r.IsDBNull(3))
					{
						en.SJID = r.GetString(3);
					}
					_dicID2Entity[en.ID] = en;
					base.Add(en);
					return true;
				});
			}
			public List<XzdyExp> Build()
			{
				var lst = new List<XzdyExp>();
				for (int i = this.Count - 1; i >= 0; --i)
				{
					Xzdy cun = null;
					var zu = this[i];
					if (zu.JB > 2)
					{
						break;
					}
					//if (zu.JB > 1)
					//{
					if (zu.JB == 2)
					{
						if (IsExists(lst, zu))
						{
							continue;
						}
						else
						{
							cun = zu;
							zu = null;
						}
					}
					//if (cun == null)
					//{
					//    continue;
					//}
					//}
					if (zu != null)
					{
						AddPair(lst, zu, zu);
						cun = FindSjXzdyByID(zu.ID);
						if (cun == null)
						{
							continue;
						}
						AddPair(lst, cun, zu);
					}
					AddPair(lst, cun, cun);
					var xiang = FindSjXzdyByID(cun.ID);
					if (xiang == null)
					{
						continue;
					}
					AddPair(lst, xiang, zu);
					AddPair(lst, xiang, cun);
					AddPair(lst, xiang, xiang);
					var xian = FindSjXzdyByID(xiang.ID);
					if (xian == null)
					{
						continue;
					}
					AddPair(lst, xian, zu);
					AddPair(lst, xian, cun);
					AddPair(lst, xian, xiang);
					AddPair(lst, xian, xian);
					var shi = FindSjXzdyByID(xian.ID);
					if (shi == null)
					{
						continue;
					}
					AddPair(lst, shi, zu);
					AddPair(lst, shi, cun);
					AddPair(lst, shi, xiang);
					AddPair(lst, shi, xian);
					AddPair(lst, shi, shi);
					var sheng = FindSjXzdyByID(shi.ID);
					if (sheng == null)
					{
						continue;
					}
					AddPair(lst, sheng, zu);
					AddPair(lst, sheng, cun);
					AddPair(lst, sheng, xiang);
					AddPair(lst, sheng, xian);
					AddPair(lst, sheng, shi);
					AddPair(lst, sheng, sheng);
				}
				return lst;
			}
			/// <summary>
			/// 根据地域ID查找上级行政地域实体
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
			private Xzdy FindSjXzdyByID(string id)
			{
				if (_dicID2Entity.TryGetValue(id, out Xzdy en))
				{
					if (!string.IsNullOrEmpty(en.SJID))
					{
						if (_dicID2Entity.TryGetValue(en.SJID, out en))
						{
							return en;
						}
					}
				}
				return null;
			}
			private XzdyExp buildExp(Xzdy sjEn, Xzdy zjEn)
			{
				var exp = new XzdyExp()
				{
					//exp.ID = Guid.NewGuid().ToString();
					SJID = sjEn.ID,
					SJBM = sjEn.BM,
					SJJB = sjEn.JB,
					ZJID = zjEn.ID,
					ZJBM = zjEn.BM,
					ZJJB = zjEn.JB
				};
				return exp;
			}
			private void AddPair(List<XzdyExp> lst, Xzdy sjEn, Xzdy zjEn)
			{
				if (sjEn != null && zjEn != null)
				{
					var key = sjEn.ID + "_" + zjEn.ID;
					if (_ids.Contains(key))
					{
						return;
					}
					_ids.Add(key);
					lst.Add(buildExp(sjEn, zjEn));
				}
			}
			private static bool IsExists(List<XzdyExp> lst, Xzdy x)
			{
				var xe = lst.Find(a =>
					{
						return a.SJID == x.ID || a.ZJID == x.ID;
					});
				return xe != null;
			}
		}

		protected Action OnImportFinish;
		protected HJDataRootPath _param;
		public ImportXzdyExp(bool fCheckDataExist = true) : base(TableNameConstants.DLXX_XZDY_EXP) { }
		#region IImportTable

		public void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
		{
			_param = prm;
			var dw = Stopwatch.StartNew();
			try
			{
				var db = prm.Workspace;
				{
					#region using-block
					try
					{
						ClearTableData(db);
						#region 行政地域中必须存在数据
						if (!IsTableHasData(db, "DLXX_XZDY"))
						{
							reportInfo.reportError("必须先导入行政地域表数据后才能执行此操作！");
							return;
						}
						#endregion

						var tableMeta = db.QueryTableMeta(TableName);
						reportInfo.reportInfo("开始导入" + TableName);
						db.BeginTransaction();

						var sTableName = TableName;
						using (var dt = new DataTable
						{
							TableName = TableName
						})
						{
							dt.Columns.Add("ID", typeof(string));
							dt.Columns.Add("SJID", typeof(string));
							dt.Columns.Add("SJJB", typeof(short));
							dt.Columns.Add("SJBM", typeof(string));
							dt.Columns.Add("ZJID", typeof(string));
							dt.Columns.Add("ZJJB", typeof(short));
							dt.Columns.Add("ZJBM", typeof(string));
							var x = new XzdyList();
							x.Load(db);
							var lst = x.Build();
							var cnt = lst.Count;
							base.RecordCount = cnt;
							var progress = new ProgressReporter(reportInfo.reportProgress, RecordCount);
							for (int n = 0; n < lst.Count; ++n)
							{
								var row = dt.NewRow();
								int c = -1;
								var en = lst[n];
								row[++c] = Guid.NewGuid().ToString();
								row[++c] = en.SJID ?? "";
								row[++c] = en.SJJB;
								row[++c] = en.SJBM;
								row[++c] = en.ZJID;
								row[++c] = en.ZJJB;
								row[++c] = en.ZJBM;
								dt.Rows.Add(row);
								if (dt.Rows.Count >= 50000)
								{
									db.SqlBulkCopyByDatatable(tableMeta, dt);
									dt.Rows.Clear();
								}
								progress.Step();
								//ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, n, ref oldProgress);
							}
							if (dt.Rows.Count > 0)
							{
								db.SqlBulkCopyByDatatable(tableMeta, dt);
								dt.Rows.Clear();
							}
							//RunSQLScript(db, prm.sXzqmc);
							db.Commit();
							#endregion

							progress.ForceFinish();
							dw.Stop();
							reportInfo.reportInfo("结束导入" + TableName + "，耗时：" + dw.Elapsed);
						}
					}
					catch (Exception ex)
					{
						db.Rollback();
						Console.WriteLine(ex.Message);
						reportInfo.reportError("错误：" + ex.Message);
					}
					#endregion
				}
			}
			finally
			{
				OnImportFinish?.Invoke();
			}
		}



		///// <summary>
		///// 执行SQLServer 数据库脚本
		///// </summary>
		///// <param name="db"></param>
		///// <param name="xianMC"></param>
		//protected virtual void RunSQLScript(IWorkspace db, string xianMC)
		//{
		//	//			string sql;
		//	//			#region 初始化 DJ_CBJYQ_DJB（登记簿）数据
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_DJB"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_DJB(ID,QLLX,DJLX,DJYY,CBJYQZBM,FBFBM,CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,DKSYT,CBJYQZLSH,FJ,YCBJYQZBH,DBR
		//	//, DJSJ, QSZT, DYZT, YYZT, SFYZX, QXDM, SZDY,LSBB,CBFMC)
		//	//  SELECT R.ID,0 QLLX,0 DJLX,'初始化总登记入库' DJYY,CBJYQZBM,F.FBFBM,R.CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,DKSYT,ISNULL(CBJYQZLSH, '') CBJYQZLSH,R.DJBFJ
		//	//	,YCBJYQZBH,R.DBR,R.DJSJ,1 QSZT,0 DYZT,0 YYZT,0 SFYZX,SUBSTRING(F.FBFBM, 1, 6) QXDM,Z.ID SZDY,0 LSBB,c.CBFMC
		//	//FROM QSSJ_CBJYQZDJB R
		//	//JOIN QSSJ_FBF F ON F.FBFBM = R.FBFBM
		//	//JOIN DLXX_XZDY Z ON Z.ID = F.SZDY
		//	//JOIN QSSJ_CBF C on c.CBFBM=R.CBFBM";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion

		//	//			#region 初始化 DJ_CBJYQ_CBHT（承包合同） 数据
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_CBHT"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_CBHT(ID,DJBID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM)
		//	//SELECT C.ID,R.ID DJBID,C.CBHTBM,C.YCBHTBM,C.FBFBM,C.CBFBM,C.CBFS,C.CBQXQ,C.CBQXZ,C.HTZMJ,C.CBDKZS,C.QDSJ,C.HTZMJM,C.YHTZMJ,C.YHTZMJM
		//	//FROM QSSJ_CBHT C
		//	//JOIN QSSJ_CBJYQZDJB R ON C.CBHTBM=R.CBJYQZBM";
		//	//				db.ExecuteNonQuery(sql);

		//	//				sql = @"UPDATE QSSJ_CBHT SET DJZT=2
		//	//FROM QSSJ_CBHT C 
		//	//WHERE EXISTS (
		//	//  SELECT ID
		//	//  FROM DJ_CBJYQ_CBHT C1
		//	//  WHERE C1.CBHTBM=C.CBHTBM)";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion

		//	//			//ok

		//	//			#region 初始化 DJ_CBJYQ_DKXX（承包地块） 数据
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_DKXX"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_DKXX(ID,DKID,DJBID,DKBM,FBFBM,CBFBM,CBJYQQDFS,HTMJ,CBHTBM,LZHTBM,CBJYQZBM,YHTMJ,HTMJM,YHTMJM,SFQQQG,
		//	//  DKMC,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,ZJRXM,BZ)
		//	//SELECT RL.ID,l.ID DKID,R.ID DJBID,RL.DKBM,RL.FBFBM,RL.CBFBM,RL.CBJYQQDFS,RL.HTMJ,RL.CBHTBM,RL.LZHTBM,RL.CBJYQZBM,RL.YHTMJ,RL.HTMJM,RL.YHTMJM,RL.SFQQQG,
		//	//	L.DKMC,L.YDKBM,L.SYQXZ,L.DKLB,L.TDLYLX,L.DLDJ,L.TDYT,L.SFJBNT,L.SCMJ,L.SCMJM,L.ELHTMJ,L.QQMJ,L.JDDMJ,L.DKDZ,L.DKNZ,L.DKXZ,L.DKBZ,
		//	//	L.DKBZXX,L.ZJRXM,L.DKBZXX
		//	//FROM QSSJ_CBDKXX RL
		//	//JOIN DLXX_DK L ON RL.DKBM=L.DKBM
		//	//JOIN QSSJ_CBJYQZDJB R ON RL.CBJYQZBM=R.CBJYQZBM";
		//	//				db.ExecuteNonQuery(sql);

		//	//				sql = @"UPDATE DLXX_DK SET DJZT=2
		//	//FROM DLXX_DK C 
		//	//WHERE EXISTS (
		//	//  SELECT ID
		//	//  FROM DJ_CBJYQ_DKXX C1
		//	//  WHERE C1.DKBM=C.DKBM
		//	//)";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion


		//	//			#region 初始化 DJ_CBJYQ_CBF（登记承包方） 数据
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_CBF"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_CBF(ID,DJBID,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR)
		//	//SELECT F.ID,R.ID DJBID,F.CBFBM,F.CBFLX,F.CBFMC,F.CBFZJLX,F.CBFZJHM,F.CBFDZ,F.YZBM,F.LXDH,F.CBFCYSL,F.CBFDCRQ,F.CBFDCY,F.CBFDCJS,F.GSJS,F.GSJSR,F.GSSHRQ,F.GSSHR
		//	//FROM QSSJ_CBF F
		//	//JOIN QSSJ_CBHT C ON C.CBFBM=F.CBFBM
		//	//JOIN QSSJ_CBJYQZDJB R ON R.CBJYQZBM=C.CBHTBM";
		//	//				db.ExecuteNonQuery(sql);

		//	//				//-- 更新数据登记状态
		//	//				sql = @"UPDATE QSSJ_CBF SET DJZT=2
		//	//FROM QSSJ_CBF C
		//	//WHERE EXISTS (
		//	//  SELECT ID
		//	//  FROM DJ_CBJYQ_CBF C1
		//	//  WHERE C1.CBFBM=C.CBFBM
		//	//)";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion

		//	//			#region 初始化 DJ_CBJYQ_CBF_JTCY（登记承包方家庭成员） 数据
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_CBF_JTCY"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_CBF_JTCY(ID,DJBID,CBFBM,CBFID,CYXM,CYXB,CYZJLX,CYZJHM,YHZGX,CYBZ,SFGYR,CYBZSM,CSRQ)
		//	//SELECT P.ID,R.ID DJBID,F.CBFBM,F.ID CBFID,P.CYXM,P.CYXB,P.CYZJLX,P.CYZJHM,P.YHZGX,P.CYBZ,P.SFGYR,P.CYBZSM,P.CSRQ
		//	//FROM QSSJ_CBF_JTCY P
		//	//JOIN QSSJ_CBF F ON F.CBFBM=P.CBFBM
		//	//JOIN QSSJ_CBHT C ON C.CBFBM=F.CBFBM
		//	//JOIN QSSJ_CBJYQZDJB R ON R.CBJYQZBM=C.CBHTBM";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion

		//	//			#region 初始化DJ_CBJYQ_QZBF
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_QZBF"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_QZBF(ID,QZID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM)
		//	//SELECT ID,ID QZID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM from QSSJ_CBJYQZ_QZBF";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion

		//	//			#region 初始化DJ_CBJYQ_QZHF
		//	//			if (!IsTableHasData(db, "DJ_CBJYQ_QZHF"))
		//	//			{
		//	//				sql = @"INSERT INTO DJ_CBJYQ_QZHF(ID,QZID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM)
		//	//SELECT ID,ID QZID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM from QSSJ_CBJYQZ_QZHF";
		//	//				db.ExecuteNonQuery(sql);
		//	//			}
		//	//			#endregion

		//	//			//ok

		//	//			//DJ_CBJYQ_QZ在ImportDJ_CBJYQ_QZ类中已处理
		//	//			//			if (false)
		//	//			//			{
		//	//			//				#region  初始化 DJ_CBJYQ_QZ（权证） 数据
		//	//			//				ClearTableData(db, "DJ_CBJYQ_QZ");
		//	//			//				var sqlFmt = @"INSERT INTO DJ_CBJYQ_QZ
		//	//			//SELECT W.ID,R.ID DJBID,W.CBJYQZBM,0 ZSBS,'{0}' SQSJC, YEAR(W.FZRQ),W.FZJG FZJGSZDMC,NULL NDSXH,
		//	//			//	NULL YZSXH,R.CBJYQZLSH,W.FZJG,W.FZRQ,0 DYCS,W.QZSFLQ,W.QZLQRQ,W.QZLQRXM,W.QZLQRZJLX,W.QZLQRZJHM,
		//	//			//	0 SFYZX,NULL ZXYY,NULL ZXRQ,NULL ZXSJ
		//	//			//FROM QSSJ_CBJYQZ W
		//	//			//JOIN QSSJ_CBJYQZDJB R ON R.CBJYQZBM=W.CBJYQZBM";
		//	//			//				sql = string.Format(sqlFmt, xianMC);
		//	//			//				db.ExecuteNonQuery(sql);
		//	//			//				#endregion
		//	//			//			}
		//	//			//not ok

		//	//			#region  初始化 XTPZ_LSH（流水号） 数据
		//	//			if (!IsTableHasData(db, "XTPZ_LSH"))
		//	//			{
		//	//				sql = @"INSERT INTO XTPZ_LSH
		//	//SELECT NEWID() ID, 'CONTRACTOR_FAMILY' LX, F.FBFBM FZM,C.XH BH FROM QSSJ_FBF F
		//	//JOIN (
		//	//  SELECT FBFBM,MAX(CAST(RIGHT(CBFBM,3) AS INT))+1 XH FROM QSSJ_CBF
		//	//  WHERE CBFLX='1'
		//	//  GROUP BY FBFBM
		//	//) C ON F.FBFBM=C.FBFBM";
		//	//				db.ExecuteNonQuery(sql);
		//	//				sql = @"INSERT INTO XTPZ_LSH
		//	//SELECT NEWID() ID, 'CONTRACTOR_PERSON' LX, F.FBFBM FZM,8000+C.XH BH FROM QSSJ_FBF F
		//	//JOIN (
		//	//  SELECT FBFBM,MAX(CAST(RIGHT(CBFBM,3) AS INT))+1 XH FROM QSSJ_CBF
		//	//  WHERE CBFLX='2'
		//	//  GROUP BY FBFBM
		//	//) C ON F.FBFBM=C.FBFBM";
		//	//				db.ExecuteNonQuery(sql);
		//	//				sql = @"INSERT INTO XTPZ_LSH
		//	//SELECT NEWID() ID, 'CONTRACTOR_ORG' LX, F.FBFBM FZM,9000+C.XH BH FROM QSSJ_FBF F
		//	//JOIN (
		//	//  SELECT FBFBM,MAX(CAST(RIGHT(CBFBM,3) AS INT))+1 XH FROM QSSJ_CBF
		//	//  WHERE CBFLX='3'
		//	//  GROUP BY FBFBM
		//	//) C ON F.FBFBM=C.FBFBM";
		//	//				db.ExecuteNonQuery(sql);
		//	//				sql = @"INSERT INTO XTPZ_LSH
		//	//SELECT NEWID() ID, 'CONTRACTLAND' LX, F.FBFBM FZM,L.XH BH FROM QSSJ_FBF F
		//	//JOIN (
		//	//  SELECT FBFBM,MAX(CAST(RIGHT(DKBM,5) AS INT))+1 XH FROM DLXX_DK
		//	//  GROUP BY FBFBM
		//	//) L ON F.FBFBM=L.FBFBM";
		//	//				db.ExecuteNonQuery(sql);

		//	//				new RepairTableJZD().DoRepair(db);
		//	//				new RepairTableJzx().DoRepair(db);
		//	//			}
		//	//            #endregion
		//}
	}
	public abstract class ImportSQLScriptBase : IImportTable
	{
		public string TableName { get; private set; }

		public int RecordCount { get { return -1; } }

		protected readonly bool fCheckDataExist;
		protected IWorkspace db;
		protected readonly int nProgressCount;
		public ImportSQLScriptBase(string tableName, int recordCount = 1, bool fCheckData = true)
		{
			TableName = tableName;
			nProgressCount = recordCount;
			this.fCheckDataExist = fCheckData;
		}
		public abstract void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel);

		protected bool Exists<T>(Expression<Func<T, bool>> wh) where T : class, new()
		{
			return db.QueryOneInt($"select count(1) from {TableName} where {WhereExpression<T>.Where(wh)}") > 0;
		}
	}
}
