using Agro.GIS;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Agro.Module.PreproccessTool
{
    /// <summary>
    /// 坐标系转换
    /// </summary>
    class CoordinateConvertTask : Task
    {
        /// <summary>
        /// 坐标系转换
        /// </summary>
        public CoordinateConvertTask()
        {
            base.Name = "坐标系转换";
            base.PropertyPage = new CrsPropertyPage(() => OnApply(),false,true);
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as CrsPropertyPage;
            var ri = new ReportInfo();
            ri.reportProgress += i =>
            {
                this.ReportProgress((int)i);

            };
            ri.reportError += msg =>
            {
                this.ReportError(msg);
            };
            ri.reportInfo += msg =>
            {
                this.ReportInfomation(msg);
            };
            try
            {
                var dstCRS = d.SelectedSpatialReference;
                DoProcess(d.InputFileFullName, d.OutputFileFullName, dstCRS, ri);
                //ShapeFileUtil.ChangePrjFile(d.InputFileFullName, d.NewCRS.ToEsriString());
                //ri.reportProgress(100);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private void DoProcess(string shpFile, string outFile, SpatialReference targetSR, ReportInfo reportInfo)
        {
            if (File.Exists(outFile))
            {
                ShapeFileUtil.DeleteShapeFile(outFile);
            }
            var ext = FileUtil.GetFileExtension(outFile);
            if (ext != null && ext.ToLower() == ".shp")
            {
                outFile = outFile.Substring(0, outFile.Length - 4);
            }
            //double oldProgress = 0;
            var srcSR = ShapeFileUtil.GetSpatialReference(shpFile);
            using (var shp = new ShapeFile())
            {
                shp.Open(shpFile);
                var err = shp.CopyStruct(outFile, targetSR.ToEsriString());
                if (err != null)
                {
                    reportInfo.reportError(err);
                    return;
                }
                int nFieldCount = shp.GetFieldCount();
                var lstFieldType = new List<DBFFieldType>();
                for (int i = 0; i < nFieldCount; ++i)
                {
                    lstFieldType.Add(shp.GetFieldType(i));
                }
                var cnt = shp.GetRecordCount();
				var progress = new ProgressReporter(reportInfo.reportProgress, cnt);
				using (var shp1 = new ShapeFile())
                {
                    shp1.Open(outFile, "rb+");
                    for (int i = 0; i < cnt; ++i)
                    {
						progress.Step();
                        var wkb = shp.GetWKB(i);
                        if (wkb != null)
                        {
                            var g = WKBHelper.FromBytes(wkb);
                            g = SpatialReference.ProjectGeometry(g, srcSR, targetSR);
                            shp1.WriteWKB(i, g.AsBinary());
                        }
                        //else
                        //{
                        //    shp1.WriteWKB(i, wkb);
                        //}
                        for (int j = 0; j < nFieldCount; ++j)
                        {
                            var o = shp.GetFieldValue(i, j);
                            if (o == null)
                            {
                                shp1.WriteFieldNull(i, j);
                            }
                            else
                            {
                                var fieldType = lstFieldType[j];
                                switch (fieldType)
                                {
                                    case DBFFieldType.FTDate:
                                    case DBFFieldType.FTShort:
                                    case DBFFieldType.FTInteger:
                                        shp1.WriteFieldInt(i, j, (int)o);
                                        break;
                                    case DBFFieldType.FTFloat:
                                    case DBFFieldType.FTDouble:
                                        shp1.WriteFieldDouble(i, j, (double)o);
                                        break;
                                    case DBFFieldType.FTLogical:
                                        {
                                            var b = (bool)shp.GetFieldBool(i, j);
                                            shp1.WriteFieldBool(i, j, b);
                                        }
                                        break;
                                    case DBFFieldType.FTString:
                                        shp1.WriteFieldString(i, j, o.ToString());
                                        break;
                                }
                            }
                        }

                        //ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, i, ref oldProgress);
                    }
                }
				progress.ForceFinish();
               // ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);
            }
        }

        private string OnApply()
        {
            var d = PropertyPage as CrsPropertyPage;
            var s = d.tbShpFilePath.Text.Trim();
            if (s.Length == 0)
            {
                return "未输入文件路径！";
            }
            if (!File.Exists(s))
            {
                return "文件：" + s + "不存在！";
            }
            if (!s.ToLower().EndsWith(".shp"))
            {
                return "输入文件必须以.shp结尾！";
            }

            s = d.tbOutShpFilePath.Text.Trim();
            if (s.Length == 0)
            {
                return "未选择输出文件路径！";
            }
            if (!s.ToLower().EndsWith(".shp"))
            {
                return "输出文件必须以.shp结尾！";
            }

            if (d.crsPnl.SelectedSpatialReference == null)
            {
                return "未选择新的坐标系！";
            }
            var srcPrj = ShapeFileUtil.GetPrjText(d.tbShpFilePath.Text.Trim());
            if (string.IsNullOrEmpty(srcPrj))
            {
                return "输入文件未定义坐标系！";
            }
            var sr = ShapeFileUtil.GetSpatialReference(d.tbShpFilePath.Text.Trim());
            if (sr == null)
            {
                return "当前系统无法处理输入文件所定义的坐标系！";
            }
            return null;
        }
    }
}