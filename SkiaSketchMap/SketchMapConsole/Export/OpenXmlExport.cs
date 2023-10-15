using Agro.LibCore;
using DocumentFormat.OpenXml.Packaging;
//using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using SketchMap.Office;
using Agro.Library.Common.Repository;
using System.Reflection.Metadata.Ecma335;
//using NPOI.OpenXmlFormats.Wordprocessing;

namespace SketchMap
{
    internal class OpenXmlExport: OpenXmlWord
    {
        private readonly Logout _logout;
        private readonly List<AgricultureLandRepertory> lands=new();

        /// <summary>
        /// 承包方
        /// </summary>
        public ContractConcord Contractor { get; set; }

        /// <summary>
        /// 地块集合
        /// </summary>
        public List<VEC_CBDK> DKS { get; set; }

        /// <summary>
        /// 示意图属性
        /// </summary>
        public SkecthMapProperty MapProperty { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }=string.Empty;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public bool CanEditor { get; set; }



        public bool DeleteTempFile { get; set; }

        public OpenXmlExport(Logout logout, ContractConcord contractor
            , List<VEC_CBDK> dKS, SkecthMapProperty mapProperty, string filePath)//, bool canEditor, bool deleteTempFile)
        {
            _logout = logout;
            //this.lands = lands;
            Contractor = contractor;
            DKS = dKS;
            MapProperty = mapProperty;
            //FileName = fileName;
            FilePath = filePath;
            //CanEditor = canEditor;
            //DeleteTempFile = deleteTempFile;
        }

        public void Save(ContractConcord concord)
        {
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length <= 6)
            {
                DeleteTable(1);
            }
            var lstPicFiles=new List<string>();
            InitalizeDataInformation();
            AgricultureStrandardLandProgress(lstPicFiles);
            InitalizeAllEngleView();
            var n=DocFilePath.LastIndexOf('.');
            var pdfFile=DocFilePath[..n] +".pdf";
            new PdfGenerator(this).SavePdf(pdfFile);//, overviewPicFile,lstPicFiles);
            Doc.Save();
        }
        ///// <summary>
        ///// 初始化地块列表
        ///// </summary>
        //private void InitalizeAgricultureValue()
        //{
        //    if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length <= 6)
        //    {
        //        DeleteTable(1);
        //    }
        //}
        /// <summary>
        /// 检查数据
        /// </summary>
        /// <param name="data">承包方</param> 
        private void InitalizeDataInformation()
        {
            SetBookmarkValue("CBFMC", Contractor != null ? Contractor.CBFMC : "");
            SetBookmarkValue("CBFBM", Contractor != null ? Contractor.CBFBM : "");
            SetBookmarkValue("CBDKZS", (Contractor != null && Contractor.Lands != null) ? Contractor.Lands.Length.ToString() : "");
            double area = Contractor.Lands.TryToList().Sum(land => land.HTMJM);

            var scale = CsSysInfoRepository.Instance.LoadInt(CsSysInfoRepository.KEY_DKMJSCALE, 2);
            area = Math.Round(area, scale);
            //SetBookmarkValue("CBDKZMJ", area.ToString().ToAreaFormatString(2));
            SetBookmarkValue("CBDKZMJ", area.ToString());//.ToAreaFormatString(2));
            if (MapProperty == null)
            {
                return;
            }
            string orderString = "";
            for (int i = 0; i < 2; i++)
            {
                orderString = i == 0 ? "" : i.ToString();
                SetBookmarkValue("ZTZ" + orderString, MapProperty.DrawPerson);
                SetBookmarkValue("ZTRQ" + orderString, (MapProperty.DrawDate != null && MapProperty.DrawDate.HasValue) ? MapProperty.DrawDate.Value.LongDateString() : "");
                SetBookmarkValue("SHZ" + orderString, MapProperty.CheckPerson);
                SetBookmarkValue("SHRQ" + orderString, (MapProperty.CheckDate != null && MapProperty.CheckDate.HasValue) ? MapProperty.CheckDate.Value.LongDateString() : "");
                SetBookmarkValue("BZDW" + orderString, MapProperty.Company);
            }
        }

        /// <summary>
        /// 填写承包地块信息
        /// </summary>
        private void AgricultureStrandardLandProgress(List<string> lstPicFiles)
        {
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length < 1)
            {
                return;
            }
            int pageCount = InitalizeLandRowInformation();
            InsertPageLandPics(pageCount,lstPicFiles);
        }
        /// <summary>
        /// 初始化地块行信息
        /// </summary>
        private int InitalizeLandRowInformation()
        {
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length == 0)
            {
                return 0;
            }
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length <= 6)
            {
                SetCellValue(0, 7, 5, "1/1");
                DeleteTable(1);
                return 1;
            }
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length <= 18)
            {
                SetCellValue(0, 7, 5, "1/2");
                SetCellValue(1, 4, 5, "2/2");
                return 2;
            }

            var body =Doc.MainDocumentPart!.Document.Body!;

            int landCount = Contractor.Lands.Length - 18;
            int pageCount = landCount / 12;
            pageCount = landCount % 12 == 0 ? pageCount : (pageCount + 1);
            for (int i = 0; i < pageCount; i++)
            {
                var table = (Table)GetTable(1)!.Clone();
                body.AppendChild(table);
            }
            int totalPage = pageCount + 2;
            SetCellValue(0, 7, 5, "1/" + totalPage.ToString());
            for (int index = 0; index < pageCount + 1; index++)
            {
                SetCellValue(index + 1, 4, 5, (index + 2).ToString() + "/" + totalPage.ToString());
            }
            DeleteLastParagraph();
            return totalPage;// pageCount;
        }


        /// <summary>
        /// 插入地块影像信息
        /// </summary>     
        private void InsertPageLandPics(int pageCount, List<string> lstPicFiles)
        {
            if (Contractor == null || Contractor.Lands.Length == 0)
            {
                _logout.WriteWarning($"InsertPageLandPics::Contractor == null || Contractor.Lands.Length == 0");
                return;
            }
            var lands = Contractor.Lands;
            int landIndex = 0;
            _logout.WriteWarning($"InsertPageLandPics::lands.Length={lands.Length} pageCount={pageCount}");
            for (var tableIndex = 0; tableIndex < pageCount; ++tableIndex)
            {
                var rows = tableIndex == 0 ? 3 : 4;
                var startCol = tableIndex == 0 ? 1 : 0;
                for (int rowIndex = 1; rowIndex < rows; rowIndex++)
                {
                    for (int colInex = startCol; colInex < 4; colInex++)
                    {
                        if (landIndex >= lands.Length)
                        {
                            return;
                        }
                        var land = lands[landIndex];
                        string imagePath =Path.Combine(FilePath , land.DKBM + ".jpg");
                        lstPicFiles.Add(imagePath);
                        InsertImageShape(land, rowIndex, colInex,imagePath, tableIndex);
                        landIndex++;
                    }
                }
            }
            //GC.Collect();
        }

        ///// <summary>
        ///// 插入文件
        ///// </summary>
        //private void InsertImageShapeFirstPage(ContractLand land, int rowIndex, int columnIndex, int tableIndex = 0)
        //{
        //    try
        //    {
        //        string imagePath = FilePath + land.DKBM + ".jpg";
        //        InitalizeLandRepertory(land, rowIndex, columnIndex, tableIndex, 130, 140);
        //        if (File.Exists(imagePath))
        //        {
        //            var cell = GetTableCell(tableIndex, rowIndex, columnIndex)!;
        //            SetCellPicture(cell, imagePath, 4.49, 4.83);
        //            //SetTableCellValue(tableIndex, rowIndex, columnIndex, imagePath, 130, 140, false);
        //        }
        //    }
        //    catch (SystemException ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //    }
        //}

        /// <summary>
        /// 插入文件
        /// </summary>
        private void InsertImageShape(ContractLand land, int rowIndex, int columnIndex,string imagePath, int tableIndex = 0)
        {
            try
            {
                //_logout.WriteInformation($"cell({rowIndex},{columnIndex}) picture file {imagePath} start insert ....");

                InitalizeLandRepertory(land, rowIndex, columnIndex, tableIndex, 100, 100);
                if (File.Exists(imagePath))
                {
                    var cell = GetTableCell(tableIndex, rowIndex, columnIndex);
                    if(cell != null)
                    {
                        var width = tableIndex==0? 4.49: 3.53;
                        var height =tableIndex==0? 4.83: 3.53;

                        SetCellPicture(cell!, imagePath, width, height);
                        _logout.WriteInformation($"cell({rowIndex},{columnIndex}) picture file {imagePath} inserted");
                    }
                }
                else
                {
                    _logout.WriteInformation($"cell({rowIndex},{columnIndex}) picture file {imagePath} not exists.!!!");
                }
            }
            catch (SystemException ex)
            {
                _logout.WriteWarning($"InsertImageShape SystemException ex={ex.Message}");

                //System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 初始化地块属性
        /// </summary>
        /// <param name="land"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="tableIndex"></param>
        private void InitalizeLandRepertory(ContractLand land, int rowIndex, int columnIndex, int tableIndex, double width, double height)
        {
            var landRepertory = new AgricultureLandRepertory();
            landRepertory.TableIndex = tableIndex;
            landRepertory.RowIndex = rowIndex;
            landRepertory.ColumnIndex = columnIndex;
            landRepertory.LandNumber = land.DKBM;
            landRepertory.Width = width;
            landRepertory.Height = height;
            landRepertory.Contractor = Contractor;
            landRepertory.MapProperty = MapProperty;
            landRepertory.AgriLand = DKS.Find(ld => ld.DKBM == land.DKBM);
            if (landRepertory.AgriLand != null)
            {
                landRepertory.AgriLand.SCMJ = land.HTMJ;
                landRepertory.AgriLand.SCMJM = land.HTMJM;
            }
            landRepertory.LandCollection = DKS.FindAll(ld => ld.DKBM != land.DKBM);
            landRepertory.ImagePath =Path.Combine(FilePath , land.DKBM + ".jpg");
            landRepertory.EaglePath =Path.Combine(FilePath , Contractor.CBFBM + ".jpg");
            lands.Add(landRepertory);
        }
        /// <summary>
        /// 显示所有鹰眼图
        /// </summary>
        private string InitalizeAllEngleView()
        {
            string fileName =Path.Combine(FilePath , Contractor.CBFBM + ".jpg");
            if (System.IO.File.Exists(fileName))
            {
                var table = GetTable(0);
                var cell = GetTableCell(table!, 1, 0);
                SetCellPicture(cell!, fileName, 6.37, 6.37);
            }
            return fileName;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (DeleteTempFile)
                {
                    try
                    {
                        var dic = new HashSet<string>();
                        FileUtil.EnumFiles(FilePath, fi => dic.Add(fi.FullName));
                        foreach (var it in dic)
                        {
                            System.IO.File.Delete(it);
                        }
                        System.IO.Directory.Delete(FilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
