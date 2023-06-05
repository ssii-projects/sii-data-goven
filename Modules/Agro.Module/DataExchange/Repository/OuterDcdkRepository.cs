using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.LibCore.Repository;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;

namespace Agro.Module.DataExchange.Repository
{
	public class BglxConst
	{
		public const string New = "新增";
		public const string Split = "分割";
		public const string Merge = "合并";
		public const string Modify = "图形变更";
		public const string Ybbg = "一般变更";

		public const string Xgqtmj = "修改其它面积";

		/// <summary>
		/// 变更类型中在一般变更下面新增一项"移除"用于记录该地块从现有的承包方中移除。(2021-3-24)
		/// </summary>
		public const string YcCbf = "移除承包方";
	}
	public class OuterDcdkRepository: FeatureClassRepository<VEC_SURVEY_DK>
	{
		private readonly XtpzLshRepository xtpzLshRepository = XtpzLshRepository.Instance;
		private readonly eDatabaseType _databaseType;
		public OuterDcdkRepository(IFeatureClass fc, bool fDispos = true) : base(fc, fDispos)
		{
			_databaseType = fc.DatabaseType;
		}
		public static IFeatureClass OpenSrcFeatureClass(eDatabaseType databaseType,string fileName,bool fCheckFile=true)
		{
			IFeatureClass fc;
			if (databaseType == eDatabaseType.ShapeFile)
			{
				fc= ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(fileName,"rb+");
			}
			else
			{

				fc= SqliteFeatureWorkspaceFactory.Instance.OpenFeatureClass(fileName, VEC_SURVEY_DK.GetTableName());
			}
			if (fCheckFile)
			{
				var err = CheckTable(fc,databaseType);
				if (err != null)
				{
					fc.Dispose();
					throw new Exception(err);
				}
			}
			return fc;
		}

		/// <summary>
		/// 表结构及坐标系一致性，不包含数据检查
		/// </summary>
		/// <param name="fc"></param>
		/// <returns></returns>
		private static string? CheckTable(IFeatureClass fc, eDatabaseType databaseType)
		{
			if (fc.ShapeType != eGeometryType.eGeometryPolygon)
			{
				return "输入的shp文件必须是面要素类型！";
			}

			if (MyGlobal.Workspace.IsTableExists(DLXX_DK.GetTableName()))
			{
				var srcSrid = fc.SpatialReference?.AuthorityCode;
				var srid = MyGlobal.Workspace.GetSRID(DLXX_DK.GetTableName());
				if (srid != srcSrid)
				{
					return $"输入源的坐标系[{srcSrid}]与数据库[{ srid}]不一致！";
				}
			}

			#region 检查字段的有效性
			var fields = fc.Fields;
			var lst = EntityUtil.GetAttributes<VEC_SURVEY_DK>();

			lst.RemoveAll(it => it.PropertyName == nameof(VEC_SURVEY_DK.JDDMJ));
			if (databaseType == eDatabaseType.ShapeFile)
			{
				lst.RemoveAll(it => it.Tag != null);
			}


			foreach (var it in lst)
			{
				if (!it.Insertable)
					continue;
				var iField = fields.FindField(it.FieldName);
				if (iField < 0)
				{
                    #region yxm 2022-12-5 可以不保护不动产单元号字段（BDCDYH）
                    if (BDC_JR_DK.GetFieldName(nameof(BDC_JR_DK.BDCDYH)).Equals(it.FieldName, StringComparison.CurrentCultureIgnoreCase))
					{
						continue;
					}
                    #endregion

                    return $"加载源中缺少字段：{it.FieldName}";
                }
				var field = fields.GetField(iField);
				var fieldType = FieldsUtil.ToFieldType(it);
				if (fieldType != field.FieldType)
				{
					return $"源字段{it.FieldName}的字段类型必须是:{FieldsUtil.ToFieldTypeString(fieldType)}";
				}
				if (fieldType == eFieldType.eFieldTypeString)
				{
					if (it.Length != field.Length)
					{
						return $"字段：{ field.FieldName }的长度必须是{ it.Length}";
					}
				}

				//if (fieldType == eFieldType.eFieldTypeDouble)
				//{
				//	if (it.Precision != field.Precision)
				//	{
				//		return "字段：" + field.FieldName + "的长度必须是" + it.Precision;
				//	}
				//	if (it.Scale < field.Scale)
				//	{
				//		return "字段：" + field.FieldName + "小数位数必须是" + it.Scale;
				//	}
				//}
			}
			#endregion
			return null;
		}

		/// <summary>
		/// 获取发包方编码
		/// </summary>
		/// <param name="fc"></param>
		/// <param name="fOnlyInDcdk">是否只包含调查地块中的发包方（仅对SQLite数据源有效）</param>
		/// <returns></returns>
		public static List<ICodeItem> GetFbfbm(IFeatureClass fc,bool fOnlyInDcdk=false)
		{
			var lst = new List<ICodeItem>();
			var sFBFBMField = nameof(DLXX_DK.FBFBM);
			switch (fc.Workspace.DatabaseType)
			{
				case eDatabaseType.ShapeFile:
				case eDatabaseType.SQLite:
					sFBFBMField = nameof(VEC_SURVEY_DK.FBFDM);
					break;
			}
			if (fc.Workspace is SQLiteWorkspace db)
			{
				//var sql = $"select distinct {sFBFBMField} from {fc.TableName} where {nameof(VEC_SURVEY_DK.FBFDM)} is not null";
				var sql = $"select distinct FBFBM,FBFMC from {DC_QSSJ_FBF.GetTableName()} where FBFBM is not null";
				if (fOnlyInDcdk)
				{
					sql += $" and FBFBM in (select distinct {nameof(VEC_SURVEY_DK.FBFDM)} from {VEC_SURVEY_DK.GetTableName()})";
				}
				db.QueryCallback(sql, r =>
				 {
					 var fbfbm = r.GetString(0);
					 var fbfmc = r.GetString(1);
					 lst.Add(new CodeItem(fbfbm,$"{fbfbm}({fbfmc})", fbfmc));
					 //lst.Add(r.GetString(0));
				 });
			}
			else
			{
				var qf = SqlUtil.MakeQueryFilter(sFBFBMField);
				fc.Search(qf, r =>
				{
					var sFbfbm = SafeConvertAux.ToStr(r.GetValue(0));
					if (!string.IsNullOrEmpty(sFbfbm))
					{
						if (lst.Find(it => it.GetName() == sFbfbm) == null)
						{
							lst.Add(new CodeItem(sFbfbm, sFbfbm));
						}
						//if (!lst.Contains(sFbfbm))
						//{
						//	lst.Add(sFbfbm);
						//}
					}
				});
			}
			return lst;
		}

		public static string? NullableCheck(VEC_SURVEY_DK en)
		{
			var lst=EntityUtil.GetAttributes(en);
			foreach (var it in lst)
			{
				if (!it.Nullable)
				{
					if (en.GetPropertyValue(it.PropertyName) == null)
					{
						return $"第{ en.GetObjectID()} 行记录有错，字段：{ it.FieldName }[{it.AliasName }]的值不允许为空！"; ;
					}
				}
			}
			return null;
		}

		public ETXBGLX GetBGLX(VEC_SURVEY_DK en)
		{
			switch (en.BGLX)
			{
				case BglxConst.New:return ETXBGLX.Xinz;
				case BglxConst.Modify:return ETXBGLX.Txbg;
				case BglxConst.Split:return ETXBGLX.Fenge;
				case BglxConst.Merge:return ETXBGLX.Hebing;
				case BglxConst.Ybbg:return ETXBGLX.Sxbg;
				case BglxConst.Xgqtmj:return ETXBGLX.Xgqtmj;
				case BglxConst.YcCbf:return ETXBGLX.YcCbf;
			}
			return ETXBGLX.None;
		}

		public static ESjly FromBGLX(ETXBGLX bglx)
		{
			var eSjly = ESjly.Cs;
			switch (bglx)
			{
				case ETXBGLX.Fenge: eSjly = ESjly.Chaifen; break;
				case ETXBGLX.Hebing: eSjly = ESjly.Hebing; break;
				case ETXBGLX.Xinz: eSjly = ESjly.Xinz; break;
				case ETXBGLX.Txbg: eSjly = ESjly.Xiugai; break;
				case ETXBGLX.Sxbg:eSjly = ESjly.Sxbg;break;
			}
			return eSjly;
		}

		internal void LoadChangeData(ICancelTracker cancel, Action<VEC_SURVEY_DK> callback,bool fRecycle=false)
		{
			string? err = null;

			var subFields = VEC_SURVEY_DK.GetSubFields(p =>
			  {
				  if (_databaseType == eDatabaseType.ShapeFile&&p.Tag!=null)
				  {
					  return false;
				  }
				  if (base._fc.Fields.FindField(p.FieldName) < 0)
				  {
					  return false;
				  }
				  return true;
			  });

			FindCallback(t => (t.SCBZ != "1" || t.SCBZ==null) && t.BGLX != null&&t.Shape !=null, (en,ft) =>
			{
				var oid = en.GetObjectID();

				var g = en.Shape;

				var bglx = GetBGLX(en);

				if (!(bglx ==ETXBGLX.Xinz || bglx==ETXBGLX.Fenge || bglx==ETXBGLX.Txbg||bglx==ETXBGLX.Hebing||bglx==ETXBGLX.Sxbg||bglx==ETXBGLX.Xgqtmj||bglx==ETXBGLX.YcCbf))
				{
					err = $"第{oid}行记录有错，变更类型（BGLX）的值必须是（新增、分割、合并、图形变更、一般变更、修改其它面积、移除承包方）中的一种，该行变更类型是：{ en.BGLX}";
					return false;
				}
				if ((bglx == ETXBGLX.Sxbg ||bglx==ETXBGLX.YcCbf)&& string.IsNullOrEmpty(en.DKBM))
				{
					err = $"第{oid}行记录有错，针对变更类型（BGLX）为‘{en.BGLX}’的记录，地块编码不能为空！";
					return false;
				}
				if (!(bglx == ETXBGLX.Xinz||bglx==ETXBGLX.Sxbg||bglx==ETXBGLX.Xgqtmj||bglx==ETXBGLX.YcCbf) && string.IsNullOrEmpty(en.BGYY))
				{
					err = $"第{oid}行记录有错，针对变更类型（BGLX）不是新增的记录，必须要填写变更原因";
					return false;
				}
				err = NullableCheck(en);
				if (err == null && xtpzLshRepository.FindByFbfbm(en.FBFDM) == null)
				{
					err = $"第{oid}行记录有错，数据库中不存在发包方编码：{en.FBFDM}";
				}
				err = Check(err, () =>
				{
					#region 检查地块编码字段
					if (bglx !=ETXBGLX.Xinz)
					{
						if (string.IsNullOrEmpty(en.DKBM))
						{
							return $"第{oid}行记录有错，变更类型（BGLX）不是“新增”的必须填写地块编码（DKBM）";
						}
						var sDKBM = en.DKBM.Trim();
						if (sDKBM.Length != 19)
						{
							return "第" + oid + "行记录有错，地块编码（DKBM）的长度必须是19位";
						}
						//else if (!DlxxDkRepository.Instance.Exists(t => t.ZT == EDKZT.Youxiao && t.DKBM == sDKBM))
						//{
						//	return "第" + oid + "行记录有错，该行地块编码（DKBM）的值" + sDKBM + "在数据库中不存在！";
						//}
					}
					#endregion
					return err;
				});
				err = Check(err, () =>
				{
					#region 检查地块类别字段的值
					var sDKLB = en.DKLB?.Trim();
					if (!(sDKLB == "10" || sDKLB == "21" || sDKLB == "22" || sDKLB == "23" || sDKLB == "99"))
					{
						err = $"第{oid}行记录有错，地块类别（DKLB）的值必须是（10,21,22,23,99）中的一种，该行地块类别是：{sDKLB}";
					}
					#endregion
					return err;
				});
				err = Check(err, () =>
				{
					#region 检查地力等级字段的值
					if (en.DLDJ != null)
					{
						//	err = $"第{oid}行记录有错，地力等级（DLDJ）的值必须是（01,02,03,04,05,06,07,08,09,10）中的一种，改行的地力等级是：NULL";
						//	return err;
						//}
						var sDLDJ = en.DLDJ.Trim();
						if (sDLDJ.Length != 2)
						{
							err = $"第{oid}行记录有错，地力等级（DLDJ）的值必须是（01,02,03,04,05,06,07,08,09,10）中的一种，改行的地力等级是：{sDLDJ}长度为{sDLDJ.Length}";
						}
						else
						{
							if (!int.TryParse(sDLDJ, out var nDLDJ))
							{
								nDLDJ = 0;
							}
							if (!(nDLDJ >= 1 && nDLDJ <= 10))
							{
								err = $"第{oid}行记录有错，地力等级（DLDJ）的值必须是（01,02,03,04,05,06,07,08,09,10）中的一种，该行地力等级是：{sDLDJ}";
							}
						}
					}
					#endregion
					return err;
				});
				err = Check(err, () =>
				{
					#region 检查土地用途字段的值
					var sTDYT = en.TDYT?.Trim();
					if (!int.TryParse(sTDYT, out int nTDYT))
					{
						nTDYT = 0;
					}
					if (!(nTDYT >= 1 && nTDYT <= 5))
					{
						return "第" + oid + "行记录有错，土地用途（TDYT）的值必须是（1,2,3,4,5）中的一种，该行土地用途是：" + sTDYT;
					}
					#endregion
					return err;
				});
				err = Check(err, () =>
				{
					#region 检查是否基本农田字段的值
					var sSFJBNT = en.SFJBNT?.Trim();
					if (!int.TryParse(sSFJBNT, out int nSFJBNT))
					{
						nSFJBNT = 0;
					}
					if (!(nSFJBNT >= 1 && nSFJBNT <= 2))
					{
						return $"第{oid}行记录有错，是否基本农田（SFJBNT）的值必须是（1,2）中的一种，该行是否基本农田是：{en.TDYT}";
					}
					#endregion
					return err;
				});
				err = Check(err, () => en.SCMJ <= 0 ? "第" + oid + "行记录有错，实测面积（SCMJ）的值必须>0，该行实测面积是：" + en.SCMJ : err);
				err = Check(err, () => en.SCMJM <= 0 ? "第" + oid + "行记录有错，实测面积（亩）（SCMJM）的值必须>0，该行实测面积（亩）是：" + en.SCMJM : null);
				err = Check(err, () =>
				{
					#region 检查所有权性质字段的值（要么为空，要么为{10,30,31,32,33,34}）
					var sSYQXZ = en.SYQXZ;
					if (!string.IsNullOrEmpty(sSYQXZ))
					{
						if (!(sSYQXZ == "10" || sSYQXZ == "30" || sSYQXZ == "31" || sSYQXZ == "32" || sSYQXZ == "33" || sSYQXZ == "34"))
						{
							return "第" + oid + "行记录有错，所有权性质的值要么为空，要么必须是{10,30,31,32,33,34}中的一种，所填值为：" + sSYQXZ;
						}
					}
					#endregion
					return err;
				});
				err = Check(err, () => g == null ? "第" + oid + "行记录有错，无图形！" : null);
				err = Check(err, () => !(g is IPolygon || g is IMultiPolygon) ? "第" + oid + "行记录有错，图形类型不是面状图形！" : null);
				err = Check(err, () => Topology.HasSelfIntersect(g.AsBinary(), (x, y) => { }) ? $"第{oid}行记录有错，图形存在自相交！" : null);
				if (err == null)
				{
					callback(en);
				}
				return err == null;
			}, subFields, fRecycle, cancel);
			if (err != null)
			{
				throw new Exception(err);
			}
		}
		private string? Check(string? err, Func<string?> callback)
		{
			if (err != null) return err;
			return callback();
			//return err != null ? callback() : null;
		}
	}
}
