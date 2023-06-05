using Agro.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using Agro.LibCore;
using Agro.Library.Common.Repository;

namespace Agro.Module.SketchMap
{
    /// <summary>
    ///  农村土地承包经营权宗地示意图
    /// </summary>
    public class SkecthMapExport : WordBase
	{
        #region Fields

        private List<AgricultureLandRepertory> lands;

        #endregion

        #region Properties

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
        public string FileName { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public bool CanEditor { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造函数
        /// </summary>
        public SkecthMapExport()
        {
            lands = new List<AgricultureLandRepertory>();
        }

        #endregion

        #region Methods

        #region Override

        /// <summary>
        /// 填写数据
        /// </summary>
        protected override bool OnSetParamValue(object data)
        {
            try
            {
                InitalizeAgricultureValue();
                InitalizeDataInformation();
                AgricultureStrandardLandProgress();
                InitalizeAllEngleView();
            }
            catch (SystemException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 注销
        /// </summary>
        public void Destroyed(bool fDeleteTempFile=false)
        {
            if (fDeleteTempFile)
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
            //if (CanEditor && lands != null && lands.Count > 0 && !string.IsNullOrEmpty(FileName))
            //{
            //    AgricultureLandRepertory.SerializeXml(lands, FileName);
            //}
            lands = null;
            DKS = null;
            GC.Collect();
        }

        #endregion

        #region CBDKXX

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

            var scale=CsSysInfoRepository.Instance.LoadInt(CsSysInfoRepository.KEY_DKMJSCALE,2);
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
        /// 初始化地块列表
        /// </summary>
        private void InitalizeAgricultureValue()
        {
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length == 0 || Contractor.Lands.Length <= 6)
            {
                DeleteTable(1);
            }
        }

        /// <summary>
        /// 插入文件
        /// </summary>
        private void InsertImageShape(ContractLand land, string bookMark)
        {
            try
            {
                string imagePath = FilePath + land.DKBM + ".jpg";
                if (System.IO.File.Exists(imagePath))
                {
                    InsertImageCellWithoutPading(bookMark, imagePath, 300, 300);
                }
            }
            catch (SystemException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 插入文件
        /// </summary>
        private void InsertImageShape(ContractLand land, int rowIndex, int columnIndex, int tableIndex = 0)
        {
            try
            {
                string imagePath = FilePath + land.DKBM + ".jpg";
                InitalizeLandRepertory(land, rowIndex, columnIndex, tableIndex, 100, 100);
                if (System.IO.File.Exists(imagePath))
                {
                    SetTableCellValue(tableIndex, rowIndex, columnIndex, imagePath, 100, 100, false);
                }
            }
            catch (SystemException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 插入文件
        /// </summary>
        private void InsertImageShapeFirstPage(ContractLand land, int rowIndex, int columnIndex, int tableIndex = 0)
        {
            try
            {
                string imagePath = FilePath + land.DKBM + ".jpg";
                InitalizeLandRepertory(land, rowIndex, columnIndex, tableIndex, 130, 140);
                if (System.IO.File.Exists(imagePath))
                {
                    SetTableCellValue(tableIndex, rowIndex, columnIndex, imagePath, 130, 140, false);
                }
            }
            catch (SystemException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
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
            AgricultureLandRepertory landRepertory = new AgricultureLandRepertory();
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
            landRepertory.ImagePath = FilePath + land.DKBM + ".jpg";
            landRepertory.EaglePath = FilePath + Contractor.CBFBM + ".jpg";
            lands.Add(landRepertory);
        }

        #endregion

        #region Strandard

        /// <summary>
        /// 填写承包地块信息
        /// </summary>
        private void AgricultureStrandardLandProgress()
        {
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length < 1)
            {
                return;
            }
            int pageCount = InitalizeLandRowInformation();
            InitalizeLandBookMarkInformation();
            InitalizeLandImageInformation(pageCount);
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
                SetTableCellValue(0, 7, 5, "1/1");
                DeleteTable(1);
                return 0;
            }
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length <= 18)
            {
                SetTableCellValue(0, 7, 5, "1/2");
                SetTableCellValue(1, 4, 5, "2/2");
                return 0;
            }
            int landCount = Contractor.Lands.Length - 18;
            int pageCount = landCount / 12;
            pageCount = landCount % 12 == 0 ? pageCount : (pageCount + 1);
            for (int i = 0; i < pageCount; i++)
            {
                InsertTableRowClone(1, 0);
            }
            int totalPage = pageCount + 2;
            SetTableCellValue(0, 7, 5, "1/" + totalPage.ToString());
            for (int index = 0; index < pageCount + 1; index++)
            {
                SetTableCellValue(1, index * 5 + 4, 5, (index + 2).ToString() + "/" + totalPage.ToString());
            }
            return pageCount;
        }

        /// <summary>
        /// 插入地块影像信息
        /// </summary> 
        private void InitalizeLandBookMarkInformation()
        {
            if (Contractor == null || Contractor.Lands == null || Contractor.Lands.Length == 0)
            {
                return;
            }
            int landIndex = 0;
            for (int rowIndex = 1; rowIndex < 3; rowIndex++)
            {
                for (int colInex = 1; colInex < 4; colInex++)
                {
                    if (landIndex >= Contractor.Lands.Length)
                    {
                        continue;
                    }
                    ContractLand land = Contractor.Lands[landIndex];
                    InsertImageShapeFirstPage(land, rowIndex, colInex, 0);
                    landIndex++;
                }
            }
        }

        /// <summary>
        /// 插入地块影像信息
        /// </summary>     
        private void InitalizeLandImageInformation(int pageCount)
        {
            if (Contractor == null || Contractor.Lands.Length == 0 || Contractor.Lands.Length <= 6)
            {
                return;
            }
            List<ContractLand> lands = Contractor.Lands.TryToList();
            if (Contractor.Lands.Length > 6)
            {
                lands.RemoveRange(0, 6);
            }
            int landIndex = 0;
            for (int tabIndex = 0; tabIndex < pageCount + 1; tabIndex++)
            {
                for (int rowIndex = 1; rowIndex < 4; rowIndex++)
                {
                    for (int colInex = 0; colInex < 4; colInex++)
                    {
                        if (landIndex >= lands.Count)
                        {
                            continue;
                        }
                        ContractLand land = lands[landIndex];
                        InsertImageShape(land, tabIndex * 5 + rowIndex, colInex, 1);
                        landIndex++;
                    }
                }
            }
            lands = null;
            GC.Collect();
        }

        #endregion

        #region Eagle

        /// <summary>
        /// 显示所有鹰眼图
        /// </summary>
        private void InitalizeAllEngleView()
        {
            string fileName = FilePath + Contractor.CBFBM + ".jpg";
            if (System.IO.File.Exists(fileName))
            {
                InsertImageCellWithoutPading("DYST", fileName, 180, 180);
            }
        }

        #endregion

        #endregion
    }
}
