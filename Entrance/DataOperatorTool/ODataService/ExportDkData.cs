using Agro.GIS;
using Agro.LibCore.Database;
using Agro.LibCore;
using Agro.Library.Common.Repository;
using Agro.Library.Common;
using Agro.Library.Model;
using Agro.Module.DataExchange;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Agro.LibCore.Task;
using System.IO;
using GeoAPI.Geometries;
using DataOperatorTool.OData;

namespace DataOperatorTool
{
    /// <summary>
    /// 导出地块数据
    /// </summary>
    public class ExportDkData : Task
    {

        public ExportDkData()
        {
            base.Name = "导出地块数据";
            base.Description = "导出符合农业部要求格式的调查地块数据";
            base.PropertyPage = new ExportDkDataPropertyPage();
            base.OnFinish += (t, s) => base.ReportInfomation($"耗时：{t.Elapsed}");
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = (ExportDkDataPropertyPage)base.PropertyPage;
            try
            {
                var dkShpFile = d.ExportFilePath;
                var nRecordCount =DoExportDk(d.Fbfbm, d, cancel);
                base.ReportInfomation($"共导出{nRecordCount}条记录");
            }
            catch (Exception ex)
            {
                base.ReportException(ex);
            }
        }

        private async System.Threading.Tasks.Task<int> DoExportDk(string fbfbm, ExportDkDataPropertyPage prm, ICancelTracker cancel)
        {
            if (string.IsNullOrEmpty(fbfbm))
            {
                throw new Exception("未输入发包方编码！");
            }

            //var srcRepos = DlxxDkRepository.Instance;
            var service = DownLoadService.Instance;


            int nRecordCount = await service.CountDk(fbfbm);// srcRepos.Count(t => t.ZT == EDKZT.Youxiao && t.FBFBM.StartsWith(fbfbm));

            if (nRecordCount == 0)
            {
                throw new Exception("发包方编码" + fbfbm + "无效！");
            }

            var srid = await service.GetSRID();// srcRepos.GetSrid();

            var db = MyGlobal.Workspace;

            //#region yxm 2022-12-5 检查数据库中是否存在表BDC_JR_DK，目前仅枝江市包含该业务
            //var isTableBDC_JR_DKExists = db.IsTableExists(BDC_JR_DK.GetTableName());
            //#endregion

            using var tgtFc = CreateTgtFeatureClass(prm, srid, fbfbm);//)//, out IFeatureClass? fcDcDk))
                                                                      //{
            try
            {
                //var progress = new ProgressReporter(ReportProgress, nRecordCount);
                Progress.Reset(nRecordCount + 1, "导出地块");

                tgtFc.Workspace.BeginTransaction();

                var lstEn = new List<DLXX_DK>();
                var lstFields = EntityUtil.GetIntersectionAttributes<VEC_SURVEY_DK, DLXX_DK>((t, u) => t.FieldName == u.FieldName).Select(t => t.FieldName).ToList();
                lstFields.Add(nameof(DLXX_DK.FBFBM));
                if (prm.DatabaseType == eDatabaseType.SQLite)
                {
                    lstFields.Add(nameof(DC_DLXX_DK.DJZT));
                }
                var subFields = SubFields.Make(lstFields);
                //srcRepos.FindCallback(t => t.ZT == EDKZT.Youxiao && t.FBFBM == fbfbm, en =>
                //srcRepos.FindCallback1(t => t.ZT == EDKZT.Youxiao && t.FBFBM.StartsWith(fbfbm), it => lstEn.Add(it.Item), subFields, false, cancel);

                await service.GetLands(fbfbm, m =>
                {
                    Console.WriteLine(m);
                    foreach(var it in m.Value)
                    {
                        lstEn.Add(it.ToDkEn());
                    }
                });

                //var dicDkbm2BDCDYH = new Dictionary<string, string>();//[DKBM,不动产单元号] 2022-12-5

                var dicDkbm2Cbfbm = new Dictionary<string, string>();
                await service.GetDkbm2Cbfbm(fbfbm, m =>
                {
                    foreach(var it in m.Value)
                    {
                        if (it.CBFBM != null)
                        {
                            dicDkbm2Cbfbm[it.DKBM] = it.CBFBM;
                        }
                    }
                });
                //SqlUtil.ConstructIn(lstEn, sin =>
                //{
                //    db.QueryCallback($"select DKBM,CBFBM from {QSSJ_CBDKXX.GetTableName()} where DKBM in({sin})", r =>
                //    {
                //        var dkbm = r.GetString(0);
                //        var cbfbm = SafeConvertAux.ToStr(r.GetValue(1));
                //        dicDkbm2Cbfbm[dkbm] = cbfbm;
                //    });

                //    //#region yxm 2022-12-5 如果数据库中存在表BDC_JR_DK，目前仅枝江市包含该业务
                //    //if (isTableBDC_JR_DKExists)
                //    //{
                //    //    db.QueryCallback($"select DKBM,BDCDYH from {BDC_JR_DK.GetTableName()} where DKBM in({sin})", r =>
                //    //    {
                //    //        var dkbm = r.GetString(0);
                //    //        var bdcDyh = SafeConvertAux.ToStr(r.GetValue(1));
                //    //        dicDkbm2BDCDYH[dkbm] = bdcDyh;
                //    //    });
                //    //}
                //    //#endregion
                //}, en => en.DKBM);

                var ft = tgtFc.CreateFeature();
                Envelope? env = null;
                foreach (var en in lstEn)
                {
                    if (!dicDkbm2Cbfbm.TryGetValue(en.DKBM, out string? cbfbm))
                    {
                        cbfbm = null;
                    }
                    IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.CBFBM), cbfbm);

                    //#region yxm 2022-12-5 如果数据库中存在表BDC_JR_DK，目前仅枝江市包含该业务
                    //if (isTableBDC_JR_DKExists)
                    //{
                    //    if (!dicDkbm2BDCDYH.TryGetValue(en.DKBM, out string? bdcDyh))
                    //    {
                    //        bdcDyh = null;
                    //    }
                    //    IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.BDCDYH), bdcDyh);
                    //}
                    //#endregion

                    if (en.Shape?.IsEmpty == false)
                    {
                        if (env == null)
                        {
                            env = en.Shape.EnvelopeInternal;
                        }
                        else
                        {
                            env.ExpandToInclude(en.Shape.EnvelopeInternal);
                        }
                    }
                    EntityUtil.WriteToFeature(en, ft);
                    IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.FBFDM), en.FBFBM);
                    tgtFc.Append(ft);
                    Progress.Step();
                }

                //if (env != null && fcDcDk != null)
                //{
                //    base.ExportYDk(fcDcDk, env, lstFields, cancel);
                //}

                tgtFc.Workspace.Commit();



                Progress.ForceFinish();

                //fcDcDk?.Dispose();
            }
            catch (Exception ex)
            {
                tgtFc.Workspace.Rollback();
                throw ex;
            }
            //}

            return nRecordCount;
        }


        private IFeatureClass CreateTgtFeatureClass(ExportDkDataPropertyPage prm, int srid, string fbfbm)//, out IFeatureClass? fcDcDK)
        {
            string dkFileName = prm.ExportFilePath;
            //if (prm.DatabaseType == eDatabaseType.SQLite)
            //{
            //    return base.CreateTgtFeatureClass(dkFileName, srid, fbfbm, out fcDcDK);
            //}
            //else
            //{
                if (File.Exists(dkFileName))
                {
                    ShapeFileUtil.DeleteShapeFile(dkFileName);
                }
                //fcDcDK = null;

                //var fContainFieldBDCDYH = MyGlobal.Workspace.IsTableExists(BDC_JR_DK.GetTableName());

                ShapeFileFeatureWorkspaceFactory.ParseShpFileName(dkFileName, out string cons, out string tableName);
                using var ws = ShapeFileFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);

                ws.CreateFeatureClass(tableName, VEC_SURVEY_DK.ConvertToFields(it =>
                {
                    if (it.Tag != null) { return false; }
                    var isBdckyhField = nameof(VEC_SURVEY_DK.BDCDYH).Equals(it.FieldName, StringComparison.CurrentCultureIgnoreCase);
                    if (isBdckyhField)
                    {
                        return false;// fContainFieldBDCDYH;
                    }
                    return true;
                }), srid);
                return ws.OpenFeatureClass(tableName, "rb+");
            //}
        }


    }
}
