using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.NPIO;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Model;
using Agro.Library.Common.Repository;
using System.IO;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 导出汇交格式数据
	/// </summary>
	class ExportHjsj : GroupTask
	{
		public static class GlobalBSM
		{
			private static int _bsm = 0;
			public static void Reset()
			{
				_bsm = 0;
			}
			public static int BSM { get { return ++_bsm; } }
		}
		abstract class MyTaskBase : Task
		{
			/// <summary>
			/// 以"/"结尾
			/// </summary>
			public string OutPath { get; set; }
			public IWorkspace mdb { get; set; }
			/// <summary>
			/// 县编码
			/// </summary>
			public string Xbm { get; set; }
			public string Xmc { get; set; }
			public short Year { get; set; }
		}
		#region 导出矢量数据
		class MyField : Field
		{
			public int iDbfFieldIndex;
		}
		abstract class ExportShapeBase : MyTaskBase
		{
			protected readonly List<MyField> dbfFields = new List<MyField>();
			protected static void WriteFieldString(ShapeFile shp, int row, int iShpField, IFeature r, int rCol, Func<object, object> onVal = null)
			{
				var o = r.GetValue(rCol);
				if (onVal != null)
				{
					o = onVal(o);
				}
				if (o == null)
				{
					shp.WriteFieldNull(row, iShpField);
				}
				else
				{
					var fok = shp.WriteFieldString(row, iShpField, o.ToString());
					Debug.Assert(fok);
				}
			}
			protected static void WriteFieldDouble(ShapeFile shp, int row, int iShpField, IFeature r, int rCol)
			{
				var o = r.GetValue(rCol);
				if (o == null)
				{
					shp.WriteFieldNull(row, iShpField);
				}
				else
				{
					shp.WriteFieldDouble(row, iShpField, SafeConvertAux.ToDouble(o));
				}
			}
			protected void AddFieldString(string fieldName, int len, int iDbfFieldIndex, string aliasName = null)
			{
				var field = new MyField()
				{
					FieldName = fieldName,
					AliasName = aliasName ?? fieldName,
					Length = len,
					FieldType = eFieldType.eFieldTypeString,
					iDbfFieldIndex = iDbfFieldIndex
				};
				dbfFields.Add(field);
			}
			protected void AddFieldDouble(string fieldName, int len, int scale, int iDbfFieldIndex)
			{
				var field = new MyField()
				{
					FieldName = fieldName,
					AliasName = fieldName,
					Length = len,
					Scale = scale,
					FieldType = eFieldType.eFieldTypeDouble,
					iDbfFieldIndex = iDbfFieldIndex
				};
				dbfFields.Add(field);
			}
			protected void AddFieldInt(string fieldName, int len, int iDbfFieldIndex)
			{
				var field = new MyField()
				{
					FieldName = fieldName,
					AliasName = fieldName,
					Length = len,
					//Scale = scale,
					FieldType = eFieldType.eFieldTypeInteger,
					iDbfFieldIndex = iDbfFieldIndex
				};
				dbfFields.Add(field);
			}
			protected void CreateShapeFields(ShapeFile shp)
			{
				foreach (var field in dbfFields)
				{
					if (field.FieldType == eFieldType.eFieldTypeString)
					{
						AddFieldString(shp, field.FieldName, field.Length);
					}
					else if (field.FieldType == eFieldType.eFieldTypeDouble)
					{
						shp.AddField(field.FieldName, DBFFieldType.FTDouble, field.Precision, field.Scale);
					}
					else if (field.FieldType == eFieldType.eFieldTypeInteger)
					{
						shp.AddField(field.FieldName, DBFFieldType.FTDouble, field.Length);
					}
				}
			}
			protected static void AddFieldString(ShapeFile shp, string fieldName, int len)
			{
				shp.AddField(fieldName, DBFFieldType.FTString, len);
			}
		}
		class ExportXzdyBase : ExportShapeBase
		{
			private const string SRC_TABLE_NAME = "DLXX_XZDY";

			protected string ShpPrefix;
			protected short Jb;
			private readonly string YSDM;
			public ExportXzdyBase(eZoneLevel jb)
			{
				int i = -1;
				AddFieldString("YSDM", 6, ++i);

				switch (jb)
				{
					case eZoneLevel.County:
						AddFieldString("XZQMC", 100, ++i);
						AddFieldString("XZQDM", 6, ++i);
						YSDM = "162010";
						break;
					case eZoneLevel.Town:
						AddFieldString("XJQYMC", 100, ++i);
						AddFieldString("XJQYDM", 9, ++i);
						YSDM = "162020";
						break;
					case eZoneLevel.Village:
						AddFieldString("CJQYMC", 100, ++i);
						AddFieldString("CJQYDM", 12, ++i);
						YSDM = "162030";
						break;
					case eZoneLevel.Group:
						AddFieldString("ZJQYMC", 100, ++i);
						AddFieldString("ZJQYDM", 14, ++i);
						YSDM = "162040";
						break;
				}

				AddFieldInt("BSM", 9, ++i);
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				var sShpFile = base.OutPath + @"矢量数据\" + ShpPrefix + base.Xbm + base.Year + ".shp";
				if (File.Exists(sShpFile))
				{
					ShapeFileUtil.DeleteShapeFile(sShpFile);
				}
				//var crsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"data\SpatialReferences\Projected Coordinate Systems");
				var db = MyGlobal.Workspace;

				var dw = Stopwatch.StartNew();
				try
				{
					using (var fc = db.OpenFeatureClass(SRC_TABLE_NAME))
					using (var shp = new ShapeFile())
					{
						var srid = fc.SpatialReference.AuthorityCode;// db.GetSRID("DLXX_DK");
						var prjTxt = SpatialReferenceUtil.FindEsriSRS(/*crsPath,*/ srid);
						if (prjTxt == null)
						{
							throw new Exception("无法识别SRID=" + srid + "的坐标系！");
						}

						var err = shp.Create(sShpFile, EShapeType.SHPT_POLYGON, prjTxt);
						if (err != null)
						{
							throw new Exception(err);
						}
						CreateShapeFields(shp);

						int nRecordCount = 0;
						var sql = "select count(1) from " + SRC_TABLE_NAME + " t where jb=" + Jb;
						db.QueryCallback(sql, r =>
						{
							nRecordCount = r.GetInt32(0);
							return false;
						});
						//double oldProgress = 0;

						base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");

						var progress = new ProgressReporter(ReportProgress, nRecordCount);
						int i = 0;
						var qf = new QueryFilter()
						{
							SubFields = fc.ShapeFieldName + ",bm,mc,kzmc",
							WhereClause = "JB=" + Jb + " and BM is not null and MC is not null and KZMC is not null"
						};
						fc.Search(qf, r0 =>
						{
							var r = r0 as IFeature;
							var g = r.Shape;// r.GetShape(0);
							if (g is IPolygon)
							{
								g = GeometryUtil.MakeCW(g as IPolygon);
							}


							shp.WriteWKB(i, g?.AsBinary());
							shp.WriteFieldString(i, 0, YSDM);
							var bm = r.GetValue(1).ToString();
							var s = r.GetValue(3).ToString();
							var mc = r.GetValue(2).ToString();
							if (mc.StartsWith(s))
							{
								mc = mc.Substring(s.Length);
							}
							shp.WriteFieldString(i, 1, mc);
							shp.WriteFieldString(i, 2, bm);
							shp.WriteFieldInt(i, 3, GlobalBSM.BSM);
							++i;
							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, i, ref oldProgress);
							return true;
						});

						dw.Stop();
						base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

						progress.ForceFinish();
						//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nRecordCount, ref oldProgress);
					}
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}
		}

		/// <summary>
		/// 导出县级行政区
		/// </summary>
		class ExportXjxzq : ExportXzdyBase
		{
			public ExportXjxzq() : base(eZoneLevel.County)
			{
				base.ShpPrefix = "XJXZQ";
				base.Jb = 4;
			}
		}
		/// <summary>
		/// 导出乡级区域
		/// </summary>
		class ExportXjqy : ExportXzdyBase
		{
			public ExportXjqy() : base(eZoneLevel.Town)
			{
				base.ShpPrefix = "XJQY";
				base.Jb = 3;
			}
		}
		/// <summary>
		/// 导出村级区域
		/// </summary>
		class ExportCjqy : ExportXzdyBase
		{
			public ExportCjqy() : base(eZoneLevel.Village)
			{
				base.ShpPrefix = "CJQY";
				base.Jb = 2;
			}
		}
		/// <summary>
		/// 导出组级区域
		/// </summary>
		class ExportZjqy : ExportXzdyBase
		{
			public ExportZjqy() : base(eZoneLevel.Group)
			{
				base.ShpPrefix = "ZJQY";
				base.Jb = 1;
			}
		}

		class ExportShapComm : ExportShapeBase
		{
			protected readonly string _srcTableName;

			protected readonly string ShpPrefix;
			protected readonly string _where;
			protected readonly EShapeType _shapeType;
			protected bool _isExportData = true;

			//protected Action OnPreQuery;
			protected Func<string, object, object> OnGetValue;
			//public string TableAliasName;
			public ExportShapComm(string srcTableName, string shpPrefix, string wh, EShapeType shpType = EShapeType.SHPT_POLYGON)
			{
				_srcTableName = srcTableName;
				ShpPrefix = shpPrefix;
				_where = wh;
				_shapeType = shpType;
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				var sShpFile = base.OutPath + @"矢量数据\" + ShpPrefix + base.Xbm + base.Year + ".shp";
				if (File.Exists(sShpFile))
				{
					ShapeFileUtil.DeleteShapeFile(sShpFile);
				}
				var db = MyGlobal.Workspace;

				var dw = Stopwatch.StartNew();
				try
				{
					using (var fc = db.OpenFeatureClass(_srcTableName))
					using (var shp = new ShapeFile())
					{
						var srid = fc.SpatialReference.AuthorityCode;
						var prjTxt = SpatialReferenceUtil.FindEsriSRS(srid);
						if (prjTxt == null)
						{
							throw new Exception("无法识别SRID=" + srid + "的坐标系！");
						}

						var err = shp.Create(sShpFile, _shapeType, prjTxt);
						if (err != null)
						{
							throw new Exception(err);
						}
						CreateShapeFields(shp);

						if (_isExportData)
						{
							//OnPreQuery?.Invoke();
							int nRecordCount = 0;
							var sql = "select count(1) from " + _srcTableName;
							if (!string.IsNullOrEmpty(_where))
							{
								sql += " where " + _where;
							}
							db.QueryCallback(sql, r =>
							{
								nRecordCount = r.GetInt32(0);
								return false;
							});
							Progress.Reset(nRecordCount);

							int i = 0;

							var fields = fc.ShapeFieldName;
							int cnt = 0;
							for (i = 0; i < dbfFields.Count; ++i)
							{
								var field = dbfFields[i];
								if (field.iDbfFieldIndex >= 0)
								{
									fields += "," + field.AliasName;
									++cnt;
								}
							}

							base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");

							i = 0;
							var qf = new QueryFilter()
							{
								SubFields = fields,
								WhereClause = _where,
							};
							fc.Search(qf, r0 =>
							{
								var r = r0 as IFeature;
								var g = r.Shape;
								if (g is IPolygon pgn)
								{
									g = GeometryUtil.MakeCW(pgn);
								}


								shp.WriteWKB(i, g?.AsBinary());
								for (int j = 0; j < cnt; ++j)
								{
									var field = dbfFields[j];
									if (field.FieldType == eFieldType.eFieldTypeString)
									{
										WriteFieldString(shp, i, field.iDbfFieldIndex, r, j + 1, o =>
										{
											if (OnGetValue != null)
											{
												o = OnGetValue(field.FieldName, o);
											}
											return o;
										});
									}
									else if (field.FieldType == eFieldType.eFieldTypeDouble)
									{
										WriteFieldDouble(shp, i, field.iDbfFieldIndex, r, j + 1);
									}
									else if (field.FieldType == eFieldType.eFieldTypeInteger)
									{
										if (field.FieldName == "BSM")
										{
											shp.WriteFieldDouble(i, field.iDbfFieldIndex, GlobalBSM.BSM);
										}
										else
										{
											WriteFieldDouble(shp, i, field.iDbfFieldIndex, r, j + 1);
										}
									}
								}
								++i;
								Progress.Step();
								return true;
							});
						}
						dw.Stop();
						base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);
						Progress.ForceFinish();
					}
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}
		}
		/// <summary>
		/// 导出地块
		/// </summary>
		class ExportDk : ExportShapComm
		{
			/// <summary>
			/// 是否导出未确权地块
			/// </summary>
			public bool ExportWQQDK = true;

			private const string dkWhere = "ZT=1 and DKBM in (select distinct DKBM from DJ_CBJYQ_DKXX where DJBID in (select ID from DJ_CBJYQ_DJB where QSZT=1 and ID in(select DJBID from DJ_CBJYQ_CBHT where CBDKZS>0)))";
			public ExportDk() : base("DLXX_DK", "DK", dkWhere)
			{
				int i = -1;
				AddFieldString("DKBM", 19, ++i);
				AddFieldString("DKLB", 2, ++i);
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("DKMC", 50, ++i);
				AddFieldString("SYQXZ", 2, ++i);
				AddFieldString("TDLYLX", 3, ++i);
				AddFieldString("DLDJ", 2, ++i);
				AddFieldString("TDYT", 1, ++i);
				AddFieldString("SFJBNT", 1, ++i);
				AddFieldString("DKDZ", 50, ++i);
				AddFieldString("DKXZ", 50, ++i);
				AddFieldString("DKNZ", 50, ++i);
				AddFieldString("DKBZ", 50, ++i);
				AddFieldString("DKBZXX", 254, ++i);
				AddFieldString("ZJRXM", 100, ++i);
				AddFieldString("KJZB", 254, ++i);
				AddFieldDouble("SCMJ", 16, 2, ++i);
				AddFieldDouble("SCMJM", 16, 2, ++i);

				OnGetValue = (fieldName, o) =>
				{
					if (o == null)
					{
						switch (fieldName)
						{
							case "YSDM": return "211011";
							case "DKMC": return "集体";
							case "ZJRXM": return "未指界";
							case "KJZB": return "100";
						}
					}
					return o;
				};
			}
		}

		/// <summary>
		/// 导出基本农田保护区
		/// </summary>
		class ExportJBNTBHQ : ExportShapComm
		{
			public ExportJBNTBHQ() : base("DLXX_JBNTBHQ", "JBNTBHQ", null)
			{
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("BHQBH", 13, ++i);
				AddFieldDouble("JBNTMJ", 16, 2, ++i);
			}
		}



		/// <summary>
		/// 导出界址点
		/// </summary>
		class ExportJZD : ExportShapComm
		{
			public ExportJZD() : base("DLXX_DK_JZD", "JZD", null, EShapeType.SHPT_POINT)
			{
				_isExportData = false;
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("JZDH", 10, ++i);
				AddFieldString("JBLX", 1, ++i);
				AddFieldString("JZDLX", 1, ++i);
				AddFieldString("DKBM", 254, ++i);
				AddFieldDouble("XZBZ", 12, 3, ++i);
				AddFieldDouble("YZBZ", 12, 3, ++i);
			}
		}

		/// <summary>
		/// 导出界址点
		/// </summary>
		class ExportJZX : ExportShapComm
		{
			public ExportJZX() : base("DLXX_DK_JZX", "JZX", null, EShapeType.SHPT_ARC)
			{
				_isExportData = false;
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("JXXZ", 6, ++i);
				AddFieldString("JZXLB", 2, ++i);
				AddFieldString("JZXWZ", 1, ++i);
				AddFieldString("JZXSM", 254, ++i);
				AddFieldString("PLDWQLR", 100, ++i);
				AddFieldString("PLDWZJR", 100, ++i);
				AddFieldString("JZXH", 10, ++i);
				AddFieldString("QJZDH", 10, ++i);
				AddFieldString("ZJZDH", 10, ++i);
				AddFieldString("DKBM", 254, ++i);
			}
		}

		/// <summary>
		/// 导出控制点
		/// </summary>
		class ExportKzd : ExportShapComm
		{
			public ExportKzd() : base("DLXX_KZD", "KZD", null, EShapeType.SHPT_POINT)
			{
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("KZDMC", 50, ++i);
				AddFieldString("KZDDH", 10, ++i);
				AddFieldString("KZDLX", 6, ++i);
				AddFieldString("KZDDJ", 20, ++i);
				AddFieldString("BSLX", 1, ++i);
				AddFieldString("BZLX", 1, ++i);
				AddFieldString("KZDZT", 100, ++i);
				AddFieldDouble("X80", 11, 3, ++i);
				AddFieldDouble("Y80", 11, 3, ++i);
				AddFieldDouble("X2000", 11, 3, ++i);
				AddFieldDouble("Y2000", 11, 3, ++i);
				AddFieldString("DZJ", 254, ++i);
			}
		}

		/// <summary>
		/// 导出面状地物
		/// </summary>
		class ExportMZDW : ExportShapComm
		{
			public ExportMZDW() : base("DLXX_MZDW", "MZDW", null, EShapeType.SHPT_POLYGON)
			{
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("DWMC", 10, ++i);
				AddFieldDouble("MJ", 16, 2, ++i);
				AddFieldString("BZ", 50, ++i);
			}
		}

		/// <summary>
		/// 导出区域界线
		/// </summary>
		class ExportQYJX : ExportShapComm
		{
			public ExportQYJX() : base("DLXX_QYJX", "QYJX", null, EShapeType.SHPT_ARC)
			{
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("JXLX", 6, ++i);
				AddFieldString("JXXZ", 6, ++i);
			}
		}

		/// <summary>
		/// 导出线状地物
		/// </summary>
		class ExportXZDW : ExportShapComm
		{
			public ExportXZDW() : base("DLXX_XZDW", "XZDW", null, EShapeType.SHPT_ARC)
			{
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("DWMC", 10, ++i);
				AddFieldDouble("CD", 11, 2, ++i);
				AddFieldDouble("KD", 11, 2, ++i);
				AddFieldString("BZ", 50, ++i);
			}
		}

		/// <summary>
		/// 导出点状地物
		/// </summary>
		class ExportDZDW : ExportShapComm
		{
			public ExportDZDW() : base("DLXX_DZDW", "DZDW", null, EShapeType.SHPT_POINT)
			{
				int i = -1;
				AddFieldInt("BSM", 9, ++i);
				AddFieldString("YSDM", 6, ++i);
				AddFieldString("DWMC", 10, ++i);
				AddFieldString("BZ", 50, ++i);
			}
		}

		#endregion

		#region 导出权属数据
		class ExportMdb1 : MyTaskBase
		{
			private readonly string _srcTableName;
			public readonly string _tgtTableName;
			private readonly Func<string, string> OnTgtFieldToSrcField;
			private readonly string _srcWhere;

			public object Tag;

			protected Action<List<Field>, ICancelTracker> OnPreInport;
			protected Func<IDataReader, List<Field>, int, object, object> OnPreInsert;
			protected Action OnEnd;
			public ExportMdb1(string srcTableName, string tgtTableName, string srcWhere = null, Func<string, string> onTgtFieldToSrcField = null)
			{
				_srcTableName = srcTableName;
				_tgtTableName = tgtTableName;
				_srcWhere = srcWhere;
				OnTgtFieldToSrcField = onTgtFieldToSrcField;
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				var dw = Stopwatch.StartNew();

				try
				{
					var db = MyGlobal.Workspace;
					var lstField = base.mdb.QueryFields2(_tgtTableName);
					var sql = $"select count(1) from {_srcTableName}";
					if (!string.IsNullOrEmpty(_srcWhere))
					{
						sql += $" where {_srcWhere}";
					}
					int nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));

					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");

					var prms = new List<SQLParam>();
					var insertSql = $"insert into {_tgtTableName}(";
					int i = 0;
					string sValues = null;
					string fields = null;
					foreach (var field in lstField)
					{
						var fieldName = field.FieldName;
						var srcFieldName = fieldName;
						if (OnTgtFieldToSrcField != null)
						{
							srcFieldName = OnTgtFieldToSrcField(fieldName);
						}
						if (fields == null)
						{
							fields = srcFieldName;
							insertSql += fieldName;
							sValues += mdb.SqlFunc.GetParamPrefix() + fieldName;
						}
						else
						{
							fields += "," + srcFieldName;
							insertSql += "," + fieldName;
							sValues += "," + mdb.SqlFunc.GetParamPrefix() + fieldName;
						}

						prms.Add(new SQLParam() { ParamName = fieldName });
						++i;
					}

					insertSql += ") values(" + sValues + ")";

					var progress = new ProgressReporter(ReportProgress, nRecordCount);
					sql = $"select {fields} from {_srcTableName} ";
					if (!string.IsNullOrEmpty(_srcWhere))
					{
						sql += $" where {_srcWhere}";
					}

					OnPreInport?.Invoke(lstField, cancel);

					mdb.BeginTransaction();
					db.QueryCallback(sql, r =>
					{
						for (i = 0; i < lstField.Count; ++i)
						{
							var fNull = r.IsDBNull(i);
							var o = fNull ? DBNull.Value : r.GetValue(i);
							var field = lstField[i];
							if (!field.IsNullable && fNull)
							{
								if (field.FieldType == eFieldType.eFieldTypeString)
								{
									o = "";
								}
								else if (field.FieldType == eFieldType.eFieldTypeDateTime)
								{
									o = new DateTime(1900, 1, 1);
								}
							}

							if (o is string str1)
							{//2021-3-24：所有字符串字段值去除空格，包括中间的空格。(为了通过质检）
								o = str1.Trim().Replace(" ", "").Replace("　", "");
							}

							#region yxm 2020-3-12 CBJYQZDJB中DKSYT字段超长的临时处理方式
							switch (_tgtTableName)//== "CBJYQZDJB")
							{
								case "CBJYQZDJB":
									if (field.FieldName == "DKSYT")
									{
										if (string.IsNullOrEmpty(o?.ToString()) || o.ToString().Length > field.Length)
										{
											var iField = lstField.FindIndex(it => it.FieldName == "CBJYQZBM");
											var iFieldFbfbm = lstField.FindIndex(it => it.FieldName == "FBFBM");
											if (iField >= 0 && iFieldFbfbm >= 0)
											{
												var cbjyqzBm = r.GetValue(iField);
												var fbfBm = r.GetValue(iFieldFbfbm);
												o = $"/图件\\{fbfBm}\\DKSYT{cbjyqzBm}.pdf";
											}
										}
									}
									break;
								case "LZHT":
									if (field.FieldName == "LZHTBM")
									{
										if (o is string sLzhtbm && sLzhtbm.Length > 18)
										{
											o = sLzhtbm.Substring(0, 18);
										}
									}
									break;
								case "CBHT":
									{
										if (field.FieldName == "HTZMJM")
										{
											var iField = lstField.FindIndex(it => it.FieldName == "HTZMJ");
											var mj = Math.Round(SafeConvertAux.ToDouble(r.GetValue(iField)) * 0.0015, 2);
											o = mj;
										}
									}
									break;
							}
							#endregion

							if (o is string str && field.Length > 0 && str.Length > field.Length)
							{
								var msg = $"字符串{str}长度大于数据库字段：{field.FieldName}的长度:{field.Length}";
								ReportError(msg);
							}

							if (o is DateTime dt)
							{
								o = new DateTime(dt.Year, dt.Month, dt.Day);
							}


							if (OnPreInsert != null)
							{
								o = OnPreInsert(r, lstField, i, o);
							}
							prms[i].ParamValue = o;
						}
						try
						{
							mdb.ExecuteNonQuery(insertSql, prms);
						}
						catch (Exception ex)
						{
							base.ReportException(ex);
							return false;
						}
						progress.Step();
						return true;
					});
					mdb.Commit();
					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					OnEnd?.Invoke();
					progress.ForceFinish();
					//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nRecordCount, ref oldProgress);
				}
				catch (Exception ex)
				{
					OnEnd?.Invoke();
					Try.Catch(() => mdb.Rollback(), false);
					base.ReportException(ex);
				}
			}
		}

		class ExportCBJYQZ : ExportMdb1
		{
			public ExportCBJYQZ(string where) :
				base("DJ_CBJYQ_QZ", "CBJYQZ", where)
			{
				OnEnd = () =>
				{
					try
					{
						var sql = "UPDATE CBJYQZ a, CBJYQZDJB b SET a.FZRQ = DATEADD('Y', +1, b.DJSJ) WHERE a.CBJYQZBM = b.CBJYQZBM and a.FZRQ <= b.DJSJ";
						mdb.ExecuteNonQuery(sql);
					}
					catch { }
					try
					{
						var sql = "UPDATE CBJYQZ SET QZLQRQ = DATEADD('Y', +1 , FZRQ) where FZRQ >=  QZLQRQ";
						mdb.ExecuteNonQuery(sql);
					}
					catch { }
				};
			}
		}

		class ExportCBJYQZDJB : ExportMdb1
		{
			public ExportCBJYQZDJB(string djbWh) :
				base("DJ_CBJYQ_DJB", "CBJYQZDJB", djbWh, tgtField => tgtField == "DJBFJ" ? "FJ" : tgtField)
			{
				OnEnd = () =>
				{
					try
					{
						var sql = "UPDATE CBJYQZDJB SET DKSYT = '图件\\' + FBFBM + '\\DKSYT' + CBJYQZBM + '.pdf'";
						mdb.ExecuteNonQuery(sql);
					}
					catch { }
					try
					{
						var sql = "UPDATE CBHT a, CBJYQZDJB b SET a.QDSJ = DATEADD('Y', -1, b.DJSJ) WHERE a.CBHTBM = b.CBJYQZBM and a.QDSJ >= b.DJSJ";
						mdb.ExecuteNonQuery(sql);
					}
					catch { }
				};
			}
		}

		/// <summary>
		/// 2021-3-24: 导出数据时保证CBDKXX表中的CBJYQQDFS的值与该地块所在合同(CBHT)中的字段CBFS一致。（为了通过质检）
		/// </summary>
		class ExportCbdkxx : ExportMdb1
		{
			public ExportCbdkxx(string srcWhere) :
				base("DJ_CBJYQ_DKXX", "CBDKXX", srcWhere)
			{
				var iCBHTBM = -1;
				var iCBJYQQDFS = -1;
				var iSFQQQG = -1;
				var dicCbhtbm2CBFS = new Dictionary<string, string>();
				OnPreInport = (lstField, cancel) =>
				{
					iCBHTBM = lstField.FindIndex(it => it.FieldName == "CBHTBM");
					iCBJYQQDFS = lstField.FindIndex(it => it.FieldName == "CBJYQQDFS");
					iSFQQQG = lstField.FindIndex(it => it.FieldName == "SFQQQG");
					var sql = "SELECT distinct a.CBHTBM,b.CBFS from DJ_CBJYQ_DKXX a left join DJ_CBJYQ_CBHT b on a.CBHTBM=b.CBHTBM where a.CBJYQQDFS<> b.CBFS and b.CBFS is not null";
					MyGlobal.Workspace.QueryCallback(sql, r => dicCbhtbm2CBFS[r.GetString(0)] = r.GetString(1), cancel);
				};
				OnPreInsert = (r, lstField, i, o) =>
				{
					if (i == iCBJYQQDFS)
					{
						var cbhtbm = r.GetString(iCBHTBM);
						if (dicCbhtbm2CBFS.TryGetValue(cbhtbm, out var cbfs))
						{
							o = cbfs;
						}
					}
					else if (i == iSFQQQG)
					{
						if (o?.ToString() == "1")
						{
							o = "2";
						}
					}
					return o;
				};
				OnEnd = () => dicCbhtbm2CBFS.Clear();
			}
		}

		/// <summary>
		/// 2021/3/24 为了导出数据通过质检
		/// </summary>
		class ExportCbf : ExportMdb1
		{
			class TmpItem
			{
				public string YZBM;
				public string FBFMC;
			}
			public ExportCbf(string srcWhere) :
				base("DJ_CBJYQ_CBF", "CBF", srcWhere)
			{
				var iCBFBM = -1;
				var iCBFDCY = -1;
				var iGSJSR = -1;
				var iGSSHR = -1;
				var iYZBM = -1;
				var iCBFDZ = -1;
				var dicFbfbm2Yzbm = new Dictionary<string, TmpItem>();
				OnPreInport = (lstField, cancel) =>
				{
					for (var i = lstField.Count; --i >= 0;)
					{
						switch (lstField[i].FieldName.ToUpper())
						{
							case "CBFBM": iCBFBM = i; break;
							case "CBFDCY": iCBFDCY = i; break;
							case "GSJSR": iGSJSR = i; break;
							case "GSSHR": iGSSHR = i; break;
							case "YZBM": iYZBM = i; break;
							case "CBFDZ": iCBFDZ = i; break;
						}
					}
					var sql = "select a.FBFBM,a.YZBM,a.FBFMC from QSSJ_FBF a left join DJ_CBJYQ_CBF b on a.FBFBM=SUBSTRING(b.CBFBM,1,14) where b.YZBM is null or b.CBFDZ is null";
					MyGlobal.Workspace.QueryCallback(sql, r =>
					{
						dicFbfbm2Yzbm[r.GetString(0)] = new TmpItem()
						{
							YZBM = r.IsDBNull(1) ? "" : r.GetString(1),
							FBFMC = r.IsDBNull(2) ? "" : r.GetString(2)
						};
					}, cancel);
				};
				OnPreInsert = (r, lstField, i, o) =>
				{
					if (o == null || (o is string s && s.Trim().Length == 0))
					{
						if (i == iYZBM)
						{
							var sFBFBM = r.GetString(iCBFBM).Substring(0, 14);
							if (dicFbfbm2Yzbm.TryGetValue(sFBFBM, out var v))
							{
								o = v.YZBM;
							}
						}
						else if (i == iCBFDZ)
						{
							var sFBFBM = r.GetString(iCBFBM).Substring(0, 14);
							if (dicFbfbm2Yzbm.TryGetValue(sFBFBM, out var v))
							{
								o = v.FBFMC;
							}
						}
						else if (i == iCBFDCY || i == iGSJSR || i == iGSSHR)
						{
							o = "农业农村局";
						}
					}
					return o;
				};
				OnEnd = () => dicFbfbm2Yzbm.Clear();
			}
		}

		class ExportCbfJtcy : ExportMdb1
		{
			public ExportCbfJtcy(string srcWhere) : base("DJ_CBJYQ_CBF_JTCY", "CBF_JTCY", srcWhere)
			{
				int iCYXB = -1;
				int iCYZJHM = -1;
				int iCBFBM = -1;
				int iCYXM = -1;
				var dicCbfmc = new Dictionary<string, string>();
				OnPreInport = (lstField, cancel) =>
				{
					iCYXB = lstField.FindIndex(it => it.FieldName == "CYXB");
					iCYZJHM = lstField.FindIndex(it => it.FieldName == "CYZJHM");
					iCBFBM = lstField.FindIndex(it => it.FieldName == "CBFBM");
					iCYXM = lstField.FindIndex(it => it.FieldName == "CYXM");
					var sql = "select b.CBFBM,b.CBFZJHM,b.CBFMC from DJ_CBJYQ_CBF_JTCY a left join DJ_CBJYQ_CBF b on a.CBFID=b.ID and a.CYZJHM=b.CBFZJHM where a.CYXM<> b.CBFMC";
					MyGlobal.Workspace.QueryCallback(sql, r =>
					{
						var key = r.GetString(0) + "_" + r.GetString(1);
						var mc = r.GetString(2);
						dicCbfmc[key] = mc;
					}, cancel);
				};
				OnPreInsert = (r, lstField, i, o) =>
				{
					if (i == iCYXB)
					{//成员性别与18位身份证号码一致
						if (!r.IsDBNull(iCYZJHM))
						{
							var sfz = r.GetString(iCYZJHM);
							if (sfz.Length == 18)
							{
								var n = SafeConvertAux.ToInt32(sfz.Substring(16, 1));
								o = (n % 2) == 1 ? "1" : "2";
							}
						}
					}
					else if (i == iCYXM)
					{//承包方名称与其在家庭成员中名称一致
						var key = r.GetString(iCBFBM) + "_" + r.GetString(iCYZJHM);
						if (dicCbfmc.TryGetValue(key, out var xm))
						{
							o = xm;
						}
					}
					return o;
				};
				OnEnd = () => dicCbfmc.Clear();
			}
		}

		class ExportCbht : ExportMdb1
		{
			class TmpItem
			{
				public double HTZMJ;
				public double HTZMJM;
			}
			public ExportCbht(string srcWhere) : base("DJ_CBJYQ_CBHT", "CBHT", srcWhere)
			{
				var iCBHTBM = -1;
				var iHTZMJ = -1;
				var iHTZMJM = -1;
				var dicItem = new Dictionary<string, TmpItem>();
				OnPreInport = (lstField, cancel) =>
				{
					iCBHTBM = lstField.FindIndex(it => it.FieldName == "CBHTBM");
					iHTZMJ = lstField.FindIndex(it => it.FieldName == "HTZMJ");
					iHTZMJM = lstField.FindIndex(it => it.FieldName == "HTZMJM");
					var sql = "select CBHTBM,SUM(SCMJ) as htzmj, SUM(SCMJM) as htzmjm from DJ_CBJYQ_DKXX where djbid in(select DJBID from DJ_CBJYQ_CBHT where HTZMJ=0) group by CBHTBM";
					MyGlobal.Workspace.QueryCallback(sql, r =>
					{
						var key = r.GetString(0) + "_" + r.GetString(1);
						var mc = r.GetString(2);
						dicItem[r.GetString(0)] = new TmpItem()
						{
							HTZMJ = SafeConvertAux.ToDouble(r.GetValue(1)),
							HTZMJM = SafeConvertAux.ToDouble(r.GetValue(2))
						};
					}, cancel);
				};
				OnPreInsert = (r, lstField, i, o) =>
				{
					if (i == iHTZMJ || i == iHTZMJM)
					{
						var cbhtbm = r.GetString(iCBHTBM);
						if (dicItem.TryGetValue(cbhtbm, out var it))
						{
							if (i == iHTZMJ)
							{
								o = it.HTZMJ;
							}
							else
							{
								o = it.HTZMJM;
							}
						}
					}
					return o;
				};
				OnEnd = () => dicItem.Clear();
			}
		}

		#endregion

		#region 导出行政地域
		class ExportXzdy : MyTaskBase
		{
			protected override void DoGo(ICancelTracker cancel)
			{
				var db = MyGlobal.Workspace;
				var sql = "select count(1) from DLXX_XZDY";
				try
				{
					var dw = Stopwatch.StartNew();
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));

					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");

					Progress.Reset(nRecordCount);

					using (var sht = new NPIOSheet())
					{
						sht.Create();
						sht.SetCellText(0, 0, "权属单位代码");
						sht.SetCellText(0, 1, "权属单位名称");
						var lst = new List<Tuple<string, string>>();
						sql = $"select bm,id from DLXX_XZDY where jb<5 and bm is not null order by bm";
						db.QueryCallback(sql, r => lst.Add(new Tuple<string, string>(r.GetString(0), r.GetString(1))));
						var repos = new XzdyFullNameBuilder();
						int i = 0;
						foreach (var it in lst)
						{
							++i;
							var bm = Append0(it.Item1);
							var mc = repos.GetFullName(it.Item2);
							sht.SetCellText(i, 0, bm);
							sht.SetCellText(i, 1, mc);
							Progress.Step();
						}

						sht.ExportToExcel(base.OutPath + @"权属数据\" + base.Xbm + base.Year + "权属单位代码表.xls");
					}
					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					Progress.ForceFinish();
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			private string Append0(string bm)
			{
				while (bm.Length < 14)
				{
					bm += "0";
				}
				return bm;
			}
		}
		#endregion

		#region 导出汇总表

		abstract class ExportExportBase : MyTaskBase
		{
			public class ItemBase
			{
				/// <summary>
				/// 权属单位代码
				/// </summary>
				public string DwDm;
				/// <summary>
				/// 权属单位名称
				/// </summary>
				public string DwMc;
			}
			public class DyItem
			{
				public eZoneLevel zoneLevel;
				public string fullName;
			}

			protected readonly string _tmplFileName;

			protected readonly string djbidWh;


			public ExportExportBase(string tmplFileName)
			{
				_tmplFileName = tmplFileName;
				var djbWh = $"QSZT=1  and ID in(select DJBID from DJ_CBJYQ_CBHT where CBDKZS>0)";
				djbidWh = $"DJBID in (select ID from DJ_CBJYQ_DJB where {djbWh})";
			}
			protected NPIOSheet OpenSheet()
			{
				var sht = new NPIOSheet();
				var file = AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\" + _tmplFileName + ".xls";
				sht.Open(file);
				return sht;
			}
			/// <summary>
			/// 将发包方编码转换为行政地域编码（去掉尾部补齐的0）
			/// </summary>
			/// <param name="fbfbm"></param>
			/// <returns></returns>
			private static string ConvertFbfbm2ZoneCode(string fbfbm)
			{
				if (fbfbm.EndsWith("00000"))
				{
					fbfbm = fbfbm.Substring(0, 9);
				}
				else if (fbfbm.EndsWith("00"))
				{
					fbfbm = fbfbm.Substring(0, 12);
				}
				return fbfbm;
			}

			protected T FindItem<T>(List<T> lst, string fbfbm) where T : ItemBase
			{
				fbfbm = ConvertFbfbm2ZoneCode(fbfbm);
				return lst.Find(it => it.DwDm == fbfbm);
			}

			protected Dictionary<string, DyItem> QueryDybm2Dymc()
			{
				var dic = new Dictionary<string, DyItem>();
				//var sql = "select bm,mc,kzmc from DLXX_XZDY where jb in(1,2,3,4) and bm is not null and mc is not null";
				var sql = "select bm,id,jb from DLXX_XZDY where jb in(1,2,3,4) and bm is not null and mc is not null";
				var dic1 = new Dictionary<string, DyItem>();
				MyGlobal.Workspace.QueryCallback(sql, r =>
				{
					var bm = r.GetString(0);
					var id = r.GetString(1);
					var jb = r.GetInt32(2);
					var level = (eZoneLevel)jb;// eZoneLevel.Group;
																		 //switch (jb)
																		 //{
																		 //	case 2:level = eZoneLevel.Village;break;
																		 //	case 3:level = eZoneLevel.Town;break;
																		 //	case 4:level = eZoneLevel.County;break;
																		 //}
					dic1[bm] = new DyItem()
					{
						fullName = id,
						zoneLevel = level
					};
					//var mc = r.GetString(1);
					//var kzmc = r.GetString(2);
					//dic[bm] =kzmc+ mc;
					return true;
				});
				var repos = new XzdyFullNameBuilder();
				foreach (var kv in dic1)
				{
					var it = kv.Value;
					it.fullName = repos.GetFullName(it.fullName);
					dic[kv.Key] = it;// repos.GetFullName(kv.Value);
				}
				return dic;
			}

			protected string Append0(string bm)
			{
				while (bm.Length < 14)
				{
					bm += "0";
				}
				return bm;
			}
		}
		/// <summary>
		/// 按承包地是否基本农田汇总表
		/// </summary>
		class ExportReport1 : ExportExportBase
		{
			class Item : ItemBase
			{
				///// <summary>
				///// 权属单位代码
				///// </summary>
				//public string DwDm;
				///// <summary>
				///// 权属单位名称
				///// </summary>
				//public string DwMc;
				/// <summary>
				/// 基本农田面积
				/// </summary>
				public double JbntMj;
				/// <summary>
				/// 非基本农田面积
				/// </summary>
				public double FjbntMj;
				/// <summary>
				/// 合计面积
				/// </summary>
				public double Hjmj
				{
					get
					{
						return JbntMj + FjbntMj;
					}
				}
				internal eZoneLevel Level;
				internal bool IsEqual(Item rhs)
				{
					return DwDm == rhs.DwDm && JbntMj == rhs.JbntMj && FjbntMj == rhs.FjbntMj;
				}
			}
			public ExportReport1() : base("按承包地是否基本农田汇总表")
			{
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var db = MyGlobal.Workspace;

					var lstAppend = new List<Item>();
					var lst = new List<Item>();
					var dicDw = QueryDybm2Dymc();
					foreach (var kv in dicDw)
					{
						var it = new Item()
						{
							DwDm = kv.Key,
							DwMc = kv.Value.fullName,
							Level = kv.Value.zoneLevel
						};
						lst.Add(it);
					}

					var dw = Stopwatch.StartNew();
					var sql = $"select count(distinct CONCAT(fbfbm,SFJBNT)) from DJ_CBJYQ_DKXX where {djbidWh}";
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));
					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");
					var progress = new ProgressReporter(ReportProgress, nRecordCount);

					sql = $"select b.FBFBM,a.SFJBNT,sum(a.HTMJM) from DJ_CBJYQ_DKXX a LEFT JOIN DLXX_DK b on a.DKBM=b.DKBM where  b.ZT=1 and a.DKLB='{eDklbCode.Cbdk}' and a.{djbidWh} GROUP BY b.FBFBM,a.SFJBNT";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var sfjbnt = SafeConvertAux.ToInt32(r.GetString(1));
						var area = SafeConvertAux.ToDouble(r.GetValue(2));

						#region 2022-2-18

						var item = FindItem(lst, fbfbm);
						if (item != null && item.Level != eZoneLevel.Group)
						{
							var it = lstAppend.Find(jt => jt.DwDm == item.DwDm);
							if (it == null)
							{
								it = new Item()
								{
									DwDm = item.DwDm,
									DwMc = item.DwMc,
								};
								lstAppend.Add(it);
							}
							switch (sfjbnt)
							{
								case 1: it.JbntMj += area; break;
								case 2: it.FjbntMj += area; break;
							}
						}

						#endregion

						foreach (var it in lst)
						{
							if (fbfbm.StartsWith(it.DwDm))
							{
								switch (sfjbnt)
								{
									case 1: it.JbntMj += area; break;
									case 2: it.FjbntMj += area; break;
								}
							}
						}
						progress.Step();
						// ProgressUtil.ReportProgress(ReportProgress, nRecordCount, ++nProgress, ref oldProgress);
						return true;
					}, cancel);


					foreach (var it in lstAppend)//.ForEach(it =>
					{
						if (null == lst.Find(x => x.IsEqual(it)))
						{
							lst.Add(it);
						}
					}

					lst.Sort((a, b) =>
					{
						if (a.DwDm == b.DwDm)
						{
							return a.Hjmj > b.Hjmj ? -1 : 1;
						}
						return a.DwDm.CompareTo(b.DwDm);
					});
					//lst.Sort((a, b) => { return a.DwDm.CompareTo(b.DwDm); });

					using (var sht = base.OpenSheet())
					{
						int row = 3;
						foreach (var it in lst)
						{
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellDouble(row, 2, it.JbntMj);
							sht.SetCellDouble(row, 3, it.FjbntMj);
							sht.SetCellDouble(row, 4, it.Hjmj);
							++row;

							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nProgress, ref oldProgress);
						}

						var outFile = base.OutPath + @"汇总表格\" + base.Xbm + base.Xmc + base._tmplFileName + ".xls";
						sht.ExportToExcel(outFile);
					}

					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					progress.ForceFinish();
					//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nRecordCount, ref oldProgress);
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			//private Item FindItem(List<Item> lst, string fbfbm)
			//{
			//	fbfbm = ConvertFbfbm2ZoneCode(fbfbm);
			//	return lst.Find(it => it.DwDm == fbfbm);
			//}
		}

		/// <summary>
		/// 按承包地土地用途汇总表
		/// </summary>
		class ExportReport2 : ExportExportBase
		{
			class Item : ItemBase
			{
				///// <summary>
				///// 权属单位代码
				///// </summary>
				//public string DwDm;
				///// <summary>
				///// 权属单位名称
				///// </summary>
				//public string DwMc;
				/// <summary>
				/// 种植业面积
				/// </summary>
				public double ZzyMj;
				/// <summary>
				/// 林业面积
				/// </summary>
				public double LinyeMj;

				/// <summary>
				/// 畜牧业面积
				/// </summary>
				public double XmyMj;
				/// <summary>
				/// 渔业面积
				/// </summary>
				public double YuyeMj;
				/// <summary>
				/// 非农用途面积
				/// </summary>
				public double FnytMj;


				/// <summary>
				/// 合计面积
				/// </summary>
				public double Hjmj1
				{
					get
					{
						return ZzyMj + LinyeMj + XmyMj + YuyeMj;
					}
				}
				public double Hjmj2
				{
					get
					{
						return Hjmj1 + FnytMj;
					}
				}
				internal eZoneLevel Level;
				internal bool IsEqual(Item rhs)
				{
					return DwDm == rhs.DwDm && ZzyMj == rhs.ZzyMj && LinyeMj == rhs.LinyeMj && XmyMj == rhs.XmyMj && YuyeMj == rhs.YuyeMj && FnytMj == rhs.FnytMj;
				}
			}
			public ExportReport2() : base("按承包地土地用途汇总表")
			{
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var db = MyGlobal.Workspace;

					var lstAppend = new List<Item>();
					var lst = new List<Item>();
					var dicDw = QueryDybm2Dymc();
					foreach (var kv in dicDw)
					{
						var it = new Item()
						{
							DwDm = kv.Key,
							DwMc = kv.Value.fullName,
							Level = kv.Value.zoneLevel
						};
						lst.Add(it);
					}

					var dw = Stopwatch.StartNew();

					var sql = $"select count(distinct CONCAT(fbfbm,tdyt)) from DJ_CBJYQ_DKXX where {djbidWh}";
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));
					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");
					var progress = new ProgressReporter(ReportProgress, nRecordCount);

					//sql = $"select FBFBM,TDYT,SUM(HTMJM) from DJ_CBJYQ_DKXX where {djbidWh} group by FBFBM,TDYT";
					sql = $"select a.FBFBM,b.TDYT,SUM(a.HTMJM) from DJ_CBJYQ_DKXX a left join DLXX_DK b on a.DKBM=b.DKBM where b.ZT=1 and a.DKLB='{eDklbCode.Cbdk}' and  a.{djbidWh} GROUP BY a.FBFBM,b.TDYT";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var tdyt = SafeConvertAux.ToInt32(r.GetString(1));
						var area = SafeConvertAux.ToDouble(r.GetValue(2));

						#region 2022-2-18

						var item = FindItem(lst, fbfbm);
						if (item != null && item.Level != eZoneLevel.Group)
						{
							var it = lstAppend.Find(jt => jt.DwDm == item.DwDm);
							if (it == null)
							{
								it = new Item()
								{
									DwDm = item.DwDm,
									DwMc = item.DwMc,
								};
								lstAppend.Add(it);
							}
							switch (tdyt)
							{
								case 1: it.ZzyMj += area; break;
								case 2: it.LinyeMj += area; break;
								case 3: it.XmyMj += area; break;
								case 4: it.YuyeMj += area; break;
								case 5: it.FnytMj += area; break;
							}
						}

						#endregion

						foreach (var it in lst)
						{
							if (fbfbm.StartsWith(it.DwDm))
							{
								switch (tdyt)
								{
									case 1: it.ZzyMj += area; break;
									case 2: it.LinyeMj += area; break;
									case 3: it.XmyMj += area; break;
									case 4: it.YuyeMj += area; break;
									case 5: it.FnytMj += area; break;
								}
							}
						}
						progress.Step();
						// ProgressUtil.ReportProgress(ReportProgress, nRecordCount, ++nProgress, ref oldProgress);
						return true;
					}, cancel);

					lstAppend.ForEach(it =>
					{
						if (null == lst.Find(x => x.IsEqual(it)))
						{
							lst.Add(it);
						}
					});

					lst.Sort((a, b) =>
					{
						if (a.DwDm == b.DwDm)
						{
							return a.Hjmj2 > b.Hjmj2 ? -1 : 1;
						}
						return a.DwDm.CompareTo(b.DwDm);
					});
					//lst.Sort((a, b) => { return a.DwDm.CompareTo(b.DwDm); });

					using (var sht = base.OpenSheet())
					{
						int row = 4;
						foreach (var it in lst)
						{
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellDouble(row, 2, it.Hjmj1);
							sht.SetCellDouble(row, 3, it.ZzyMj);
							sht.SetCellDouble(row, 4, it.LinyeMj);
							sht.SetCellDouble(row, 5, it.XmyMj);
							sht.SetCellDouble(row, 6, it.YuyeMj);
							sht.SetCellDouble(row, 7, it.FnytMj);
							sht.SetCellDouble(row, 8, it.Hjmj2);
							++row;

							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nProgress, ref oldProgress);
						}

						var outFile = base.OutPath + @"汇总表格\" + base.Xbm + base.Xmc + base._tmplFileName + ".xls";
						sht.ExportToExcel(outFile);
					}

					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					progress.ForceFinish();
					//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nRecordCount, ref oldProgress);
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			//private Item FindItem(List<Item> lst, string fbfbm)
			//{
			//	fbfbm = ConvertFbfbm2ZoneCode(fbfbm);
			//	return lst.Find(it => it.DwDm == fbfbm);
			//}
		}

		/// <summary>
		/// 按承包方汇总表
		/// </summary>
		class ExportReport3 : ExportExportBase
		{
			class Item : ItemBase
			{
				///// <summary>
				///// 权属单位代码
				///// </summary>
				//public string DwDm;
				///// <summary>
				///// 权属单位名称
				///// </summary>
				//public string DwMc;
				/// <summary>
				/// 承包农户数量
				/// </summary>
				public int CblhSl;
				/// <summary>
				/// 家庭成员数量
				/// </summary>
				public int JtcySl;

				/// <summary>
				/// 单位承包数量
				/// </summary>
				public int DwcbSl;
				/// <summary>
				/// 个人承包数量
				/// </summary>
				public int GrcbSl;



				/// <summary>
				/// 其他方式承包合计
				/// </summary>
				public double QtfscbSlHj
				{
					get
					{
						return DwcbSl + GrcbSl;
					}
				}
				/// <summary>
				/// 承包总数
				/// </summary>
				public double Cbzs
				{
					get
					{
						return CblhSl + QtfscbSlHj;
					}
				}
				internal eZoneLevel Level;

				internal bool IsEqual(Item rhs)
				{
					return DwDm == rhs.DwDm && CblhSl == rhs.CblhSl && JtcySl == rhs.JtcySl && DwcbSl == rhs.DwcbSl && GrcbSl == rhs.GrcbSl;
				}
			}
			class ResItem : Item
			{

			}
			public ExportReport3() : base("按承包方汇总表")
			{
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var db = MyGlobal.Workspace;

					var lstAppend = new List<Item>();
					var lst = new List<Item>();
					var dicDw = QueryDybm2Dymc();
					foreach (var kv in dicDw)
					{
						var it = new Item()
						{
							DwDm = kv.Key,
							DwMc = kv.Value.fullName,
							Level = kv.Value.zoneLevel
						};
						lst.Add(it);
					}

					var dw = Stopwatch.StartNew();

					var sFrom = @"DJ_CBJYQ_DKXX t  left join DJ_CBJYQ_CBF k on t.CBFBM=k.CBFBM
where t.DJBID in (select id from DJ_CBJYQ_DJB where QSZT = 1 and id in(select DJBID from DJ_CBJYQ_CBHT where CBDKZS > 0))";

					var sql = $"select count(distinct CONCAT(SUBSTRING(t.dkbm,1,14),t.cbfbm,k.CBFLX)) from {sFrom}";
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));// lst.Count;
					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");
					var progress = new ProgressReporter(ReportProgress, nRecordCount);
					sql = $"select SUBSTRING(CBFBM, 1,14),CBFLX,count(distinct CBFBM) from DJ_CBJYQ_CBF where {djbidWh} GROUP BY SUBSTRING(CBFBM, 1,14),CBFLX";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var cbflx = SafeConvertAux.ToShort(r.GetValue(1));
						var cnt = SafeConvertAux.ToInt32(r.GetValue(2));

						#region 2022-2-18

						var item = FindItem(lst, fbfbm);
						if (item != null && item.Level != eZoneLevel.Group)
						{
							var it = lstAppend.Find(jt => jt.DwDm == item.DwDm);
							if (it == null)
							{
								it = new Item()
								{
									DwDm = item.DwDm,
									DwMc = item.DwMc,
								};
								lstAppend.Add(it);
							}
							switch (cbflx)
							{
								case 1://农户
									it.CblhSl += cnt;
									break;
								case 2://个人
									it.GrcbSl += cnt;
									break;
								case 3:
									it.DwcbSl += cnt;
									break;
							}
						}

						#endregion

						foreach (var it in lst)
						{
							if (fbfbm.StartsWith(it.DwDm))
							{
								switch (cbflx)
								{
									case 1://农户
										it.CblhSl += cnt;
										break;
									case 2://个人
										it.GrcbSl += cnt;
										break;
									case 3:
										it.DwcbSl += cnt;
										break;
								}
							}
						}
						return true;
					}, cancel);

					sql = $"select SUBSTRING(CBFBM, 1,14),sum(CBFCYSL) cnt from DJ_CBJYQ_CBF where cbflx='1' and {djbidWh} GROUP BY SUBSTRING(CBFBM, 1,14)";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var cnt = SafeConvertAux.ToInt32(r.GetValue(1));

						#region 2022-2-18

						var item = FindItem(lst, fbfbm);
						if (item != null && item.Level != eZoneLevel.Group)
						{
							var it = lstAppend.Find(jt => jt.DwDm == item.DwDm);
							if (it == null)
							{
								it = new Item()
								{
									DwDm = item.DwDm,
									DwMc = item.DwMc,
								};
								lstAppend.Add(it);
							}
							it.JtcySl += cnt;
						}

						#endregion

						foreach (var it in lst)
						{
							if (fbfbm.StartsWith(it.DwDm))
							{
								it.JtcySl += cnt;
							}
						}
						progress.Step();
					}, cancel);


					foreach (var it in lstAppend)
					{
						if (null == lst.Find(x => x.IsEqual(it)))
						{
							lst.Add(it);
						}
					}

					lst.Sort((a, b) =>
					{
						if (a.DwDm == b.DwDm)
						{
							return a.Cbzs > b.Cbzs ? -1 : 1;
						}
						return a.DwDm.CompareTo(b.DwDm);
					});
					//lst.Sort((a, b) => { return a.DwDm.CompareTo(b.DwDm); });

					using (var sht = base.OpenSheet())
					{
						int row = 4;
						foreach (var it in lst)
						{
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellDouble(row, 2, it.Cbzs);
							sht.SetCellDouble(row, 3, it.CblhSl);
							sht.SetCellDouble(row, 4, it.JtcySl);
							sht.SetCellDouble(row, 5, it.QtfscbSlHj);
							sht.SetCellDouble(row, 6, it.DwcbSl);
							sht.SetCellDouble(row, 7, it.GrcbSl);
							++row;

							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nProgress, ref oldProgress);
						}

						var outFile = base.OutPath + @"汇总表格\" + base.Xbm + base.Xmc + base._tmplFileName + ".xls";
						sht.ExportToExcel(outFile);
					}

					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					progress.ForceFinish();
					//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nRecordCount, ref oldProgress);
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			//private Item FindItem(List<Item> lst, string fbfbm)
			//{
			//	fbfbm = ConvertFbfbm2ZoneCode(fbfbm);
			//	return lst.Find(it => it.DwDm == fbfbm);
			//}
		}

		/// <summary>
		/// 按地块汇总表
		/// </summary>
		class ExportReport4 : ExportExportBase
		{
			class Item : ItemBase
			{
				/// <summary>
				///发包方数量
				/// </summary>
				public int FbfSl;
				/// <summary>
				/// 承包地块总数
				/// </summary>
				public int CbdkSl;
				/// <summary>
				/// 承包地块总面积
				/// </summary>
				public double CbdkZmj;

				/// <summary>
				/// 非承包地块总数
				/// </summary>
				public int FcbdkSl;
				/// <summary>
				/// 非承包地块总面积
				/// </summary>
				public double FcbdkZmj;

				/// <summary>
				/// 颁发权证数量
				/// </summary>
				public int BfqzSl;



				internal eZoneLevel Level;

				/// <summary>
				/// 有效统计数据总量
				/// </summary>
				internal int mass
				{
					get
					{
						return CbdkSl + FcbdkSl + BfqzSl;
					}
				}
				internal void SumRhs(Item rhs)
				{
					FbfSl += rhs.FbfSl;
					CbdkSl += rhs.CbdkSl;
					CbdkZmj += rhs.CbdkZmj;
					FcbdkSl += rhs.FcbdkSl;
					FcbdkZmj += rhs.FcbdkZmj;
					BfqzSl += rhs.BfqzSl;
				}
				internal bool IsEqual(Item rhs)
				{
					return DwDm == rhs.DwDm && CbdkSl == rhs.CbdkSl && CbdkZmj == rhs.CbdkZmj && FcbdkSl == rhs.FcbdkSl
						&& FcbdkZmj == rhs.FcbdkZmj && BfqzSl == rhs.BfqzSl;
				}

			}
			public ExportReport4() : base("按地块汇总表")
			{
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var db = MyGlobal.Workspace;

					var lst = new List<Item>();
					var dicDw = QueryDybm2Dymc();
					foreach (var kv in dicDw)
					{
						var it = new Item()
						{
							DwDm = kv.Key,
							DwMc = kv.Value.fullName,
							Level = kv.Value.zoneLevel
						};
						lst.Add(it);
					}

					var dw = Stopwatch.StartNew();

					var sql = $"select count(distinct fbfbm) from DJ_CBJYQ_DKXX where  DKLB='10' and {djbidWh}";
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));
					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");
					var progress = new ProgressReporter(ReportProgress, nRecordCount);

					sql = @"select fbfbm,
	sum(case when dklb<>'10' or dklb is null then 1 else 0 end) as fcbdkZs,
	sum(case when dklb<>'10' or dklb is null then scmjm else 0 end) as fcbdkZmj";
					//sql+=$" from dlxx_dk where ZT=1 and DJZT={(int)EDjzt.Ydj} group by fbfbm";
					sql += $" from dlxx_dk where ZT=1 and dkbm in (select dkbm from DJ_CBJYQ_DKXX where {djbidWh}) group by fbfbm";

					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var fcbdkSl = SafeConvertAux.ToInt32(r.GetValue(1));
						var fcbdkZmj = SafeConvertAux.ToDouble(r.GetValue(2));
						var item = FindItem(lst, fbfbm);
						if (item != null)
						{
							item.FcbdkSl += fcbdkSl;
							item.FcbdkZmj += fcbdkZmj;
						}
						progress.Step();
						return true;
					}, cancel);

					//sql = $"select fbfbm,sum(htmjm) as mj,count(distinct DKBM) as cnt from DJ_CBJYQ_DKXX where  DKLB='10' and {djbidWh} group by fbfbm";
					sql = $"select fbfbm,sum(htmjm) as mj,count(distinct DKBM) as cnt from DJ_CBJYQ_DKXX where dkbm in (select dkbm from dlxx_dk where zt=1) and {djbidWh} group by fbfbm";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var cbdkSl = SafeConvertAux.ToInt32(r.GetValue(2));
						var cbdkZmj = SafeConvertAux.ToDouble(r.GetValue(1));

						var item = FindItem(lst, fbfbm);
						if (item != null)
						{
							item.CbdkSl += cbdkSl;
							item.CbdkZmj += cbdkZmj;
						}
						progress.Step();
						return true;
					}, cancel);

					sql = $" select SUBSTRING(CBJYQZBM,1,14) as fbfbm,count(distinct CBJYQZBM) from DJ_CBJYQ_QZ where {djbidWh}  group by SUBSTRING(CBJYQZBM,1,14)  ";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var bfqzSl = SafeConvertAux.ToInt32(r.GetValue(1));
						var item = FindItem(lst, fbfbm);
						if (item != null)
						{
							item.BfqzSl += bfqzSl;
						}

						progress.Step();
						return true;
					}, cancel);

					for (var i = lst.Count; --i >= 0;)
					{
						var it = lst[i];

						if (it.Level == eZoneLevel.Group || it.mass > 0)
						{
							it.FbfSl = 1;
						}

						if ((it.Level == eZoneLevel.Village || it.Level == eZoneLevel.Town) && it.mass > 0)
						{
							var append = new Item()
							{
								DwDm = it.DwDm,
								DwMc = it.DwMc,
								Level = it.Level
							};
							lst.Add(append);
						}
					}

					lst.Sort((a, b) =>
					{
						if (a.DwDm == b.DwDm)
						{
							return a.FbfSl < b.FbfSl ? -1 : 1;// a.setFbfbm.Count > b.setFbfbm.Count ? -1 : 1;
						}
						return a.DwDm.CompareTo(b.DwDm);
					});

					#region 汇总村级统计数据
					for (var i = 0; i < lst.Count - 1;)
					{
						var it = lst[i];
						if (it.Level == eZoneLevel.Village)
						{
							var dm = it.DwDm.Substring(0, 12);
							for (var j = i + 1; j < lst.Count; ++j)
							{
								var jt = lst[j];
								if (jt.DwDm.StartsWith(dm))
								{
									it.SumRhs(jt);
									if (j == lst.Count - 1)
									{
										i = j;
									}
								}
								else
								{
									i = j;
									break;
								}
							}
						}
						else
						{
							++i;
						}
					}
					#endregion

					#region 汇总乡级统计数据
					for (var i = 0; i < lst.Count - 1;)
					{
						var it = lst[i];
						if (it.Level == eZoneLevel.Town)
						{
							var dm = it.DwDm.Substring(0, 9);
							for (var j = i + 1; j < lst.Count; ++j)
							{
								var jt = lst[j];
								if (jt.DwDm.StartsWith(dm))
								{
									if (jt.Level == eZoneLevel.Town)
									{
										it.SumRhs(jt);
									}
									else if (jt.Level == eZoneLevel.Village)
									{
										it.SumRhs(jt);
										if (j + 1 < lst.Count && lst[j + 1].DwDm == jt.DwDm)
										{
											++j;
										}
									}
									if (j == lst.Count - 1)
									{
										i = j;
									}
								}
								else
								{
									i = j;
									break;
								}
							}
						}
						else
						{
							++i;
						}
					}
					#endregion

					#region 汇总县级统计数据
					for (var i = 1; i < lst.Count; ++i)
					{
						var it = lst[i];
						if (it.Level == eZoneLevel.Town)
						{
							lst[0].SumRhs(it);
							if (i < lst.Count - 1 && lst[i + 1].DwDm == it.DwDm)
							{
								++i;
							}
						}
					}
					#endregion

					#region yxm 2022-2-21 去掉重复记录
					for (var i = lst.Count; --i >= 0;)
					{
						if (i > 0)
						{
							var it = lst[i];
							var jt = lst[i - 1];
							if (it.DwDm == jt.DwDm && it.FbfSl == jt.FbfSl && it.CbdkSl == jt.CbdkSl && it.CbdkZmj == jt.CbdkZmj
								&& it.FcbdkSl == jt.FcbdkSl && it.FcbdkZmj == jt.FcbdkZmj && it.BfqzSl == jt.BfqzSl)
							{
								lst.RemoveAt(i);
							}
						}
					}
					#endregion

					using (var sht = base.OpenSheet())
					{
						int row = 4;
						foreach (var it in lst)
						{
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellDouble(row, 2, it.FbfSl);
							sht.SetCellDouble(row, 3, it.CbdkSl);
							sht.SetCellDouble(row, 4, it.CbdkZmj);
							sht.SetCellDouble(row, 5, it.FcbdkSl);
							sht.SetCellDouble(row, 6, it.FcbdkZmj);
							sht.SetCellDouble(row, 7, it.BfqzSl);
							++row;

							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nProgress, ref oldProgress);
						}

						var outFile = base.OutPath + @"汇总表格\" + base.Xbm + base.Xmc + base._tmplFileName + ".xls";
						sht.ExportToExcel(outFile);
					}

					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					progress.ForceFinish();
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}
		}

		/// <summary>
		/// 按非承包地地块类别汇总表
		/// </summary>
		class ExportReport5 : ExportExportBase
		{
			class Item : ItemBase
			{
				///// <summary>
				///// 权属单位代码
				///// </summary>
				//public string DwDm;
				///// <summary>
				///// 权属单位名称
				///// </summary>
				//public string DwMc;

				/// <summary>
				/// 自留地总面积
				/// </summary>
				public double ZldZmj;

				/// <summary>
				/// 机动地总面积
				/// </summary>
				public double JddZmj;

				/// <summary>
				/// 开荒地总面积
				/// </summary>
				public double KfdZmj;

				/// <summary>
				/// 其他集体土地总面积
				/// </summary>
				public double QtjttdZmj;

				/// <summary>
				/// 颁发权证数量
				/// </summary>
				public double SumZmj
				{
					get
					{
						return ZldZmj + JddZmj + KfdZmj + QtjttdZmj;
					}
				}

				//public readonly HashSet<string> setFbfbm = new HashSet<string>();
			}
			public ExportReport5() : base("按非承包地地块类别汇总表")
			{
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var db = MyGlobal.Workspace;

					var lst = new List<Item>();
					var dicDw = QueryDybm2Dymc();
					foreach (var kv in dicDw)
					{
						var it = new Item()
						{
							DwDm = kv.Key,
							DwMc = kv.Value.fullName,
						};
						lst.Add(it);
					}

					var dw = Stopwatch.StartNew();

					var sql = "select count(distinct fbfbm) from dlxx_dk ";
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));
					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");
					var progress = new ProgressReporter(ReportProgress, nRecordCount);

					//sql = $"select fbfbm, dklb,sum(scmjm) from dlxx_dk where zt=1 and DJZT={(int)EDjzt.Ydj} and dklb is not null and dklb<>'10' group by fbfbm,dklb";
					sql = $"select fbfbm, dklb,sum(scmjm) from dlxx_dk where zt=1 and DKBM in (select DKBM from DJ_CBJYQ_DKXX where {djbidWh}) and dklb is not null and dklb<>'10' group by fbfbm,dklb";


					db.QueryCallback(sql, r =>
					{
						var dkbm = r.GetString(0);
						var dklb = r.GetString(1);
						var mj = SafeConvertAux.ToDouble(r.GetValue(2));
						foreach (var it in lst)
						{
							if (dkbm.StartsWith(it.DwDm))
							{
								switch (dklb)
								{
									case "21": it.ZldZmj += mj; break;
									case "22": it.JddZmj += mj; break;
									case "23": it.KfdZmj += mj; break;
									case "99": it.QtjttdZmj += mj; break;
								}
							}
						}
						progress.Step();
						return true;
					}, cancel);

					lst.Sort((a, b) => { return a.DwDm.CompareTo(b.DwDm); });

					using (var sht = base.OpenSheet())
					{
						int row = 4;
						foreach (var it in lst)
						{
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellDouble(row, 2, it.ZldZmj);
							sht.SetCellDouble(row, 3, it.JddZmj);
							sht.SetCellDouble(row, 4, it.KfdZmj);
							sht.SetCellDouble(row, 5, it.QtjttdZmj);
							sht.SetCellDouble(row, 6, it.SumZmj);
							++row;

							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nProgress, ref oldProgress);
						}

						var outFile = base.OutPath + @"汇总表格\" + base.Xbm + base.Xmc + base._tmplFileName + ".xls";
						sht.ExportToExcel(outFile);
					}

					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					progress.ForceFinish();
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			///// <summary>
			///// 查询承包方家庭成员数量
			///// 返回[承包方编码,家庭成员数量]
			///// </summary>
			///// <returns></returns>
			//private Dictionary<string, short> QueryCbfJtcys()
			//{
			//	var dic = new Dictionary<string, short>();
			//	var sql = "select cbfbm,count(1) from [QSSJ_CBF_JTCY] where cbfbm is not null group by cbfbm";
			//	MyGlobal.Workspace.QueryCallback(sql, r =>
			//	{
			//		var cbfbm = r.GetString(0);
			//		var n = SafeConvertAux.ToShort(r.GetValue(1));
			//		dic[cbfbm] = n;
			//		return true;
			//	});
			//	return dic;
			//}
		}

		/// <summary>
		/// 按权证信息汇总表
		/// </summary>
		class ExportReport6 : ExportExportBase
		{
			class Item : ItemBase
			{
				///// <summary>
				///// 权属单位代码
				///// </summary>
				//public string DwDm;
				///// <summary>
				///// 权属单位名称
				///// </summary>
				//public string DwMc;



				/// <summary>
				/// 家庭承包权证数量
				/// </summary>
				public int JtcbQzSl;

				/// <summary>
				/// 其它方式承包权证数量
				/// </summary>
				public int QtcbQzsl;

				/// <summary>
				/// 颁发权证面积
				/// </summary>
				public double QzZmj;

				internal bool IsEqual(Item rhs)
				{
					return DwDm == rhs.DwDm && JtcbQzSl == rhs.JtcbQzSl && QtcbQzsl == rhs.QtcbQzsl && QzZmj == rhs.QzZmj;
				}

				/// <summary>
				/// 颁发权证总数量
				/// </summary>
				public int QzZsl
				{
					get
					{
						return JtcbQzSl + QtcbQzsl;
					}
				}

				internal eZoneLevel Level;
			}
			public ExportReport6() : base("按权证信息汇总表")
			{
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var db = MyGlobal.Workspace;

					var lstAppend = new List<Item>();
					var lst = new List<Item>();
					var dicDw = QueryDybm2Dymc();
					foreach (var kv in dicDw)
					{
						var it = new Item()
						{
							DwDm = kv.Key,
							DwMc = kv.Value.fullName,
							Level = kv.Value.zoneLevel
						};
						lst.Add(it);
					}

					var dw = Stopwatch.StartNew();

					//var sql = $"select count(distinct CONCAT(fbfbm,cbjyqqdfs)) from DJ_CBJYQ_DKXX where {djbidWh} ";
					var sql = $"select count(distinct fbfbm) from DJ_CBJYQ_DKXX where {djbidWh} ";
					var nRecordCount = SafeConvertAux.ToInt32(db.QueryOne(sql));
					base.ReportInfomation("开始" + base.Name + ",共" + nRecordCount + "条");
					var progress = new ProgressReporter(ReportProgress, nRecordCount);

					//sql = $"select fbfbm,sum(htmjm) as zmj from DJ_CBJYQ_DKXX where {djbidWh} group by fbfbm,cbjyqqdfs";
					sql = $"select fbfbm,sum(ROUND(HTZMJ*0.0015,2)) from DJ_CBJYQ_CBHT where {djbidWh} GROUP BY fbfbm";

					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var mj = SafeConvertAux.ToDouble(r.GetValue(1));

						#region 2022-2-18

						var item = FindItem(lst, fbfbm);
						if (item != null && item.Level != eZoneLevel.Group)
						{
							var it = lstAppend.Find(jt => jt.DwDm == item.DwDm);
							if (it == null)
							{
								it = new Item()
								{
									DwDm = item.DwDm,
									DwMc = item.DwMc,
								};
								lstAppend.Add(it);
							}
							it.QzZmj += mj;
						}

						#endregion

						foreach (var it in lst)
						{
							if (fbfbm.StartsWith(it.DwDm))
							{
								it.QzZmj += mj;
							}
						}
						progress.Step();
					}, cancel);

					sql = $"select b.FBFBM,b.CBFS,count(distinct a.CBJYQZBM) as cnt,sum(b.htzmjm) as zmj from DJ_CBJYQ_QZ a  join DJ_CBJYQ_CBHT b on a.DJBID=b.DJBID where a.{djbidWh} group by b.FBFBM,b.CBFS";
					db.QueryCallback(sql, r =>
					{
						var fbfbm = r.GetString(0);
						var cbjyqqdfs = r.GetString(1);
						var cnt = SafeConvertAux.ToInt32(r.GetValue(2));

						#region 2022-2-18

						var item = FindItem(lst, fbfbm);
						if (item != null && item.Level != eZoneLevel.Group)
						{
							var it = lstAppend.Find(jt => jt.DwDm == item.DwDm);
							if (it == null)
							{
								it = new Item()
								{
									DwDm = item.DwDm,
									DwMc = item.DwMc,
								};
								lstAppend.Add(it);
							}
							if (cbjyqqdfs == "110")
							{
								it.JtcbQzSl += cnt;
							}
							else
							{
								it.QtcbQzsl += cnt;
							}
						}

						#endregion

						foreach (var it in lst)
						{
							if (fbfbm.StartsWith(it.DwDm))
							{
								if (cbjyqqdfs == "110")
								{
									it.JtcbQzSl += cnt;
								}
								else
								{
									it.QtcbQzsl += cnt;
								}
							}
						}
					}, cancel);

					foreach (var it in lstAppend)
					{
						if (null == lst.Find(x => x.IsEqual(it)))
						{
							lst.Add(it);
						}
					}
					lst.Sort((a, b) =>
					{
						if (a.DwDm == b.DwDm)
						{
							return a.QzZsl > b.QzZsl ? -1 : 1;
						}
						return a.DwDm.CompareTo(b.DwDm);
					});
					//lst.Sort((a, b) => { return a.DwDm.CompareTo(b.DwDm); });

					using (var sht = base.OpenSheet())
					{
						int row = 4;
						foreach (var it in lst)
						{
							sht.SetCellText(row, 0, Append0(it.DwDm));
							sht.SetCellText(row, 1, it.DwMc);
							sht.SetCellDouble(row, 2, it.QzZsl);
							sht.SetCellDouble(row, 3, it.JtcbQzSl);
							sht.SetCellDouble(row, 4, it.QtcbQzsl);
							sht.SetCellDouble(row, 5, it.QzZmj);
							++row;

							progress.Step();
							//ProgressUtil.ReportProgress(ReportProgress, nRecordCount, nProgress, ref oldProgress);
						}

						var outFile = base.OutPath + @"汇总表格\" + base.Xbm + base.Xmc + base._tmplFileName + ".xls";
						sht.ExportToExcel(outFile);
					}

					dw.Stop();
					base.ReportInfomation("结束" + base.Name + "，耗时：" + dw.Elapsed);

					progress.ForceFinish();
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			//private Item FindItem(List<Item> lst, string fbfbm)
			//{
			//	fbfbm = ConvertFbfbm2ZoneCode(fbfbm);
			//	return lst.Find(it => it.DwDm == fbfbm);
			//}

			///// <summary>
			///// 查询承包方家庭成员数量
			///// 返回[承包方编码,家庭成员数量]
			///// </summary>
			///// <returns></returns>
			//private Dictionary<string, short> QueryCbfJtcys()
			//{
			//	var dic = new Dictionary<string, short>();
			//	var sql = "select cbfbm,count(1) from [QSSJ_CBF_JTCY] where cbfbm is not null group by cbfbm";
			//	MyGlobal.Workspace.QueryCallback(sql, r =>
			//	{
			//		var cbfbm = r.GetString(0);
			//		var n = SafeConvertAux.ToShort(r.GetValue(1));
			//		dic[cbfbm] = n;
			//		return true;
			//	});
			//	return dic;
			//}
		}
		#endregion


		//private ExportMdb1 exportQZBF;
		//private ExportMdb1 exportQZHF;
		//private ExportMdb1 exportQZZX;
		//private readonly TaskPage taskPage;
		public ExportHjsj()
		{
			//taskPage = page;
			base.Name = "导出汇交格式数据";
			base.Description = "导出农业部制定的汇交格式数据";
			var p = new ExportHjsjPropertyPage();
			base.PropertyPage = p;
			p.OnAppled += () => ResetTasks();

			//taskPage.OnUpdateUI += () =>
			//  {
			//	  var prm = (ExportHjsjPropertyPage)PropertyPage;
			//	  exportQZBF.Visible = prm.OptionItems.Find(it => it.OptionType == ExportHjsjPropertyPage.OptionType.ExportBZ).IsSelected;
			//	  exportQZHF.Visible = prm.OptionItems.Find(it => it.OptionType == ExportHjsjPropertyPage.OptionType.ExportHZ).IsSelected;
			//	  exportQZZX.Visible = prm.OptionItems.Find(it => it.OptionType == ExportHjsjPropertyPage.OptionType.ExportZX).IsSelected;
			//  };
		}
		private void ResetTasks()
		{
			ClearTasks();

			var prm = (ExportHjsjPropertyPage)PropertyPage;

			var djbWh = "QSZT=1  and ID in(select DJBID from DJ_CBJYQ_CBHT where CBDKZS>0)";
			var djbidWh = $"DJBID in (select ID from DJ_CBJYQ_DJB where {djbWh})";

			#region for test
			//if (true)
			//{
			//     AddTask(new ExportReport1(), "导出按承包地是否基本农田汇总表");
			//     AddTask(new ExportReport2(), "导出按承包地土地用途汇总表");
			//     AddTask(new ExportReport3(), "导出按承包方汇总表");
			//     AddTask(new ExportReport4(), "导出按地块汇总表");
			//     AddTask(new ExportReport5(), "导出按非承包地地块类别汇总表");
			//     AddTask(new ExportReport6(), "导出按权证信息汇总表");
			//     return;
			//}
			#endregion

			//if (false)
			//{//test
			// //AddTask(new ExportMdb1("DJ_CBJYQ_QZBF", "CBJYQZ_QZBF", djbidWh), "导出权属数据承包经营权证权证颁发");
			//	AddTask(new ExportXzdy(), "导出权属单位代码表");
			//	AddTask(new ExportReport1(), "导出按承包地是否基本农田汇总表");
			//	AddTask(new ExportReport2(), "导出按承包地土地用途汇总表");
			//	AddTask(new ExportReport3(), "导出按承包方汇总表");
			//	AddTask(new ExportReport4(), "导出按地块汇总表");
			//	AddTask(new ExportReport5(), "导出按非承包地地块类别汇总表");
			//	AddTask(new ExportReport6(), "导出按权证信息汇总表");

			//	//AddTask(new ExportMdb1("DJ_CBJYQ_DJB", "CBJYQZDJB", "QSZT = 1", tgtField => tgtField == "DJBFJ" ? "FJ" : tgtField), "导出权属数据承包经营权登记簿");

			//	//AddTask(new ExportMdb1("DJ_CBJYQ_DJB", "CBJYQZDJB", "QSZT = 1", tgtField => tgtField == "DJBFJ" ? "FJ" : tgtField), "导出权属数据承包经营权登记簿");
			//	//AddTask(new ExportMdb1("DJ_CBJYQ_QZ", "CBJYQZ", "DJBID in (select ID from DJ_CBJYQ_DJB where QSZT=1)"), "导出权属数据承包经营权证");
			//	//AddTask(new ExportMdb1("DJ_CBJYQ_QZBF", "CBJYQZ_QZBF"), "导出权属数据承包经营权证权证颁发");
			//	//AddTask(new ExportMdb1("DJ_CBJYQ_QZHF", "CBJYQZ_QZHF"), "导出权属数据承包经营权证权证换发");
			//	//AddTask(new ExportMdb1("DJ_CBJYQ_QZ", "CBJYQZ_QZZX", "SFYZX=1"), "导出权属数据承包经营权证权证注销");
			//	return;
			//}

			#region 导出矢量数据
			if (true)
			{
				AddTask(new ExportXjxzq(), "导出县级行政区");
				AddTask(new ExportXjqy(), "导出乡级区域");
				AddTask(new ExportCjqy(), "导出村级区域");
				AddTask(new ExportZjqy(), "导出组级区域");
				AddTask(new ExportDk(), "导出地块");
				AddTask(new ExportJBNTBHQ(), "导出基本农田保护区");
				AddTask(new ExportJZD(), "导出界址点");
				AddTask(new ExportJZX(), "导出界址线");
				AddTask(new ExportKzd(), "导出控制点");
				AddTask(new ExportMZDW(), "导出面状地物");
				AddTask(new ExportQYJX(), "导出区域界线");
				AddTask(new ExportXZDW(), "导出线状地物");
				AddTask(new ExportDZDW(), "导出点状地物");
			}
			#endregion

			//exportQZBF = new ExportMdb1("DJ_CBJYQ_QZBF", "CBJYQZ_QZBF", djbidWh);
			//exportQZHF = new ExportMdb1("DJ_CBJYQ_QZHF", "CBJYQZ_QZHF", djbidWh);
			//exportQZZX = new ExportMdb1("DJ_CBJYQ_QZ", "CBJYQZ_QZZX", "SFYZX=1")
			//{
			//	Visible = false
			//};

			#region 导出权属数据
			if (true)
			{
				//AddTask(new ExportMdb("QSSJ_CBDKXX", "CBDKXX"), "导出权属数据承包地块信息");
				//AddTask(new ExportMdb("QSSJ_CBF", "CBF"), "导出权属数据承包方");
				//AddTask(new ExportMdb("QSSJ_CBF_JTCY", "CBF_JTCY"), "导出权属数据承包方家庭成员");

				AddTask(new ExportCbdkxx($"DKBM in (select dkbm from DLXX_DK where zt={(int)EDKZT.Youxiao}) and {djbidWh}"), "导出权属数据承包地块信息");
				AddTask(new ExportCbf(djbidWh), "导出权属数据承包方");
				AddTask(new ExportCbfJtcy(djbidWh), "导出权属数据承包方家庭成员");



				AddTask(new ExportCbht(djbidWh), "导出权属数据承包合同");

				//AddTask(new ExportMdb1("DJ_CBJYQ_QZ", "CBJYQZ", djbidWh + " and SFYZX=0"), "导出权属数据承包经营权证");
				AddTask(new ExportCBJYQZ(djbidWh + " and SFYZX=0"), "导出权属数据承包经营权证");

				//AddTask(new ExportMdb1("DJ_CBJYQ_DJB", "CBJYQZDJB", djbWh, tgtField => tgtField == "DJBFJ" ? "FJ" : tgtField), "导出权属数据承包经营权登记簿");
				AddTask(new ExportCBJYQZDJB(djbWh), "导出权属数据承包经营权登记簿");

				if (prm.OptionItems.Find(it => it.OptionType == ExportHjsjPropertyPage.OptionType.ExportBZ).IsSelected)
				{
					AddTask(new ExportMdb1("DJ_CBJYQ_QZBF", "CBJYQZ_QZBF", djbidWh), "导出权属数据承包经营权证权证补发");
				}
				if (prm.OptionItems.Find(it => it.OptionType == ExportHjsjPropertyPage.OptionType.ExportHZ).IsSelected)
				{
					AddTask(new ExportMdb1("DJ_CBJYQ_QZHF", "CBJYQZ_QZHF", djbidWh), "导出权属数据承包经营权证权证换发");
				}
				if (prm.OptionItems.Find(it => it.OptionType == ExportHjsjPropertyPage.OptionType.ExportZX).IsSelected)
				{
					AddTask(new ExportMdb1("DJ_CBJYQ_QZ", "CBJYQZ_QZZX", "SFYZX=1"), "导出权属数据承包经营权证权证注销");
				}


				//AddTask(exportQZBF, "导出权属数据承包经营权证权证补发");
				//AddTask(exportQZHF, "导出权属数据承包经营权证权证换发");
				//AddTask(exportQZZX, "导出权属数据承包经营权证权证注销");

				if (false)
				{//2021-3-13 暂不输出以下几张表的数据					
					AddTask(new ExportMdb1("DJ_CBJYQ_LZHT", "LZHT", djbidWh), "导出权属数据流转合同");
				}

				AddTask(new ExportMdb1("QSSJ_FBF", "FBF"), "导出权属数据发包方");
				AddTask(new ExportMdb1("QSSJ_QSLYZLFJ", "QSLYZLFJ"), "导出权属数据权属来源资料附件");
			}
			#endregion

			AddTask(new ExportXzdy(), "导出权属单位代码表");

			#region 导出汇总表
			AddTask(new ExportReport1(), "导出按承包地是否基本农田汇总表");
			AddTask(new ExportReport2(), "导出按承包地土地用途汇总表");
			AddTask(new ExportReport3(), "导出按承包方汇总表");
			AddTask(new ExportReport4(), "导出按地块汇总表");
			AddTask(new ExportReport5(), "导出按非承包地地块类别汇总表");
			AddTask(new ExportReport6(), "导出按权证信息汇总表");
			#endregion
		}

		protected override void DoGo(ICancelTracker cancel)
		{
			GlobalBSM.Reset();

			var prm = (ExportHjsjPropertyPage)PropertyPage;
			string xbm = null, xmc = null;
			var db = MyGlobal.Workspace;

			db.QueryCallback($"select bm,mc from DLXX_XZDY where JB={(int)eZoneLevel.County} and bm is not null", r =>
			{
				xbm = r.GetString(0);
				xmc = r.GetString(1);
				return false;
			});
			if (string.IsNullOrEmpty(xbm))
			{
				base.ReportError("未找到县级行政区代码！");
				return;
			}
			var year = DateTime.Now.Year;
			var outPath = prm.OutPath;
			var dw = Stopwatch.StartNew();
			base.ReportInfomation("开始" + base.Name);

			Try.Catch(() => RepaireDB(), false);

			CreatePath(outPath);

			WriteVecMetaFile(outPath + $"矢量数据\\SL{xbm}{year}.xml", xbm);

			var mdbFile = outPath + @"权属数据\" + xbm + year + ".mdb";
			File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\权属数据.mdb", mdbFile);
			using (var mdb = DBAccess.Open(mdbFile))
			{
				foreach (var task in base.Tasks)
				{
					if (task is MyTaskBase tb)
					{
						tb.OutPath = outPath;
						tb.Xbm = xbm;
						tb.Xmc = xmc;
						tb.Year = (short)year;
						tb.mdb = mdb;
					}
				}
				base.DoGo(cancel);
			}
			dw.Stop();
			base.ReportInfomation("结束" + base.Name + ",耗时" + dw.Elapsed);
		}

		private void WriteVecMetaFile(string file, string xbm)
		{
			var str = Properties.Resources.SL2205812016;
			str = str.Replace("<geoID>220581</geoID>", $"<geoID>{xbm}</geoID>");
			var centralMer = 111.0;
			try
			{
				using (var fc = MyGlobal.Workspace.OpenFeatureClass(DLXX_XZDY.GetTableName()))
				{
					var sr = fc.SpatialReference;
					Console.WriteLine(sr);
					centralMer = fc.SpatialReference?.GetCentralMeridian() ?? 111.0;
				}
			}
			catch
			{
			}
			str = str.Replace("<centralMer>132</centralMer>", $"<centralMer>{centralMer}</centralMer>");

			File.WriteAllText(file, str);
		}

		private string CreatePath(string rootPath)
		{
			var sa = "矢量数据,权属数据,汇总表格,其他资料,图件,文字报告,栅格数据".Split(',');
			foreach (var s in sa)
			{
				var path = rootPath + s;
				if (Directory.Exists(path))
				{
					return "目录" + path + "存在！";
				}
				//Directory.CreateDirectory(path);
			}
			foreach (var s in sa)
			{
				var path = rootPath + s;
				Directory.CreateDirectory(path);
			}
			return null;
		}

		private void AddTask(ITask task, string taskName)
		{
			task.Name = taskName;
			AddTask(task);
		}

		private void RepaireDB()
		{
			var db = MyGlobal.Workspace;
			//--DK表中YSBM存在为空的值，值应该为211011，多了一个YSDM_1字段，DKMC如果为空，直接赋值"集体"给地块名称，ZJRXM为空直接赋值"未指界"给指界人名称，KJZB为空直接赋值"1"给空间坐标
			//db.ExecuteNonQuery("update DLXX_DK set YSDM = 211011 where YSDM is null");
			//db.ExecuteNonQuery("update DLXX_DK set DKMC = '集体' where DKMC is null");
			//db.ExecuteNonQuery("update DLXX_DK set ZJRXM = '未指界' where ZJRXM is null");
			//db.ExecuteNonQuery("update DLXX_DK set KJZB = '100' where KJZB is null");
			//--将CBJYQZ_QZBF(权证补发)中权证补发原因值(补证登记)去掉空格，CBJYQZ_QZZX(权证注销)中注销原因值(保管不善 不慎遗失)去掉空格，CBJYQZDJB(承包经营权登记簿)中承包期限值(2005年01月1日 至 2028年12月31日)去掉空格。
			//db.ExecuteNonQuery("update DJ_CBJYQ_QZBF set QZBFYY = REPLACE(QZBFYY, ' ', '')");
			//db.ExecuteNonQuery("update DJ_CBJYQ_QZHF set QZHFYY = REPLACE(QZHFYY, ' ', '')");
			//db.ExecuteNonQuery("update DJ_CBJYQ_DJB set CBQX = REPLACE(CBQX, ' ', '') WHERE QSZT = 1");
			//--CBF(承包方)中承包方地址值为空，邮政编码为空，则使用该承包方所在发包方的FBFMC与YZBM赋值给承包方.
			//db.ExecuteNonQuery("update a set a.CBFDZ = b.FBFMC from DJ_CBJYQ_CBF a, QSSJ_FBF b where a.CBFDZ is null");
			//db.ExecuteNonQuery("update a set a.YZBM = b.YZBM from DJ_CBJYQ_CBF a, QSSJ_FBF b where a.YZBM is null");
			//db.ExecuteNonQuery("update DJ_CBJYQ_CBF set CBFDCY = '农业农村局' where CBFDCY is null");
			//db.ExecuteNonQuery("update DJ_CBJYQ_CBF set GSJSR = '农业农村局' where GSJSR is null");
			//db.ExecuteNonQuery("update DJ_CBJYQ_CBF set GSSHR = '农业农村局' where GSSHR is null");
			db.ExecuteNonQuery("update a set a.GSSHRQ = DATEADD(dd, -3, b.QDSJ), a.CBFDCRQ = DATEADD(dd, -4, b.QDSJ) from DJ_CBJYQ_CBF a, DJ_CBJYQ_CBHT b where a.DJBID = b.DJBID and(a.CBFDCRQ is null or a.GSSHRQ is null or a.CBFDCRQ >= a.GSSHRQ or a.GSSHRQ >= b.QDSJ)");
			//--CBDKXX中是否确权确股字段值(1)不正确
			//db.ExecuteNonQuery("update DJ_CBJYQ_DKXX set SFQQQG = 2 where SFQQQG = 1");
			//--更新承包合同中的起始期限起等于登记簿中起始期限起
			db.ExecuteNonQuery("update  a set a.CBQXQ = b.CBQXQ from DJ_CBJYQ_CBHT a, DJ_CBJYQ_DJB b where a.DJBID = b.ID and b.QSZT = 1 and a.CBQXQ<> b.CBQXQ");
			//--更新承包合同开始日期与结束日期一致
			db.ExecuteNonQuery("update DJ_CBJYQ_DJB set CBQXQ = DATEADD(dd, -1, CBQXZ) where CBQXQ = CBQXZ");
			db.ExecuteNonQuery("update DJ_CBJYQ_CBHT set CBQXQ = DATEADD(dd, -1, CBQXZ) where CBQXQ = CBQXZ");
			db.ExecuteNonQuery("UPDATE DJ_CBJYQ_DJB SET[CBQX] = CONVERT(varchar, CBQXQ, 102) + '日至' + CONVERT(varchar, CBQXZ, 102) + '日' where CBQXQ = DATEADD(dd, -1, CBQXZ)");
			db.ExecuteNonQuery("UPDATE DJ_CBJYQ_DJB SET[CBQX] = STUFF(CBQX, 5, 1, '年') where CBQXQ = DATEADD(dd, -1, CBQXZ)");
			db.ExecuteNonQuery("UPDATE DJ_CBJYQ_DJB SET[CBQX] = STUFF(CBQX, 8, 1, '月') where CBQXQ = DATEADD(dd, -1, CBQXZ)");
			db.ExecuteNonQuery("UPDATE DJ_CBJYQ_DJB SET[CBQX] = STUFF(CBQX, 17, 1, '年') where CBQXQ = DATEADD(dd, -1, CBQXZ)");
			db.ExecuteNonQuery("UPDATE DJ_CBJYQ_DJB SET[CBQX] = STUFF(CBQX, 20, 1, '月') where CBQXQ = DATEADD(dd, -1, CBQXZ)");
			//--承包方名称与其在家庭成员中名称不一致
			//db.ExecuteNonQuery("update  a set a.CYXM = b.CBFMC from DJ_CBJYQ_CBF_JTCY a, DJ_CBJYQ_CBF b where a.CBFID = b.ID and a.CYZJHM = b.CBFZJHM and a.CYXM<> b.CBFMC");
			//--承包合同中合同总面积为0
			//db.ExecuteNonQuery("update DJ_CBJYQ_CBHT set HTZMJ = a.htzmj, HTZMJM = a.htzmjm from(select SUM(SCMJ) as htzmj, SUM(SCMJM) as htzmjm, DJBID from DJ_CBJYQ_DKXX group by DJBID)a, DJ_CBJYQ_CBHT WHERE DJ_CBJYQ_CBHT.DJBID = a.DJBID and DJ_CBJYQ_CBHT.HTZMJ = 0.0");
			//--修改家庭成员性别
			//db.ExecuteNonQuery("update DJ_CBJYQ_CBF_JTCY set CYXB = 2 where CYXB = 1 and SUBSTRING(CYZJHM,17,1) % 2 = 0 and LEN(CYZJHM) = 18");
			//db.ExecuteNonQuery("update DJ_CBJYQ_CBF_JTCY set CYXB = 1 where CYXB = 2 and SUBSTRING(CYZJHM,17,1) % 2 = 1 and LEN(CYZJHM) = 18");

			//--去除发包方负责人证件号码含有空值
			//db.ExecuteNonQuery("update QSSJ_FBF set FZRZJHM = TRIM(FZRZJHM)");

		}
	}
}
