// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.Library.Handle.ImportShapeAndMdb;
using Agro.Library.Model;
using Agro.Office;
using Agro.Module.DataExchange;
using Agro.Library.Common.Repository;

namespace Agro.Library.Handle
{
  /// <summary>
  /// 导入汇总数据
  /// </summary>
  public class AgricultureDataImport : ImportTableBase, IImportTable
  {
    private readonly bool _fCheckDataExist;
    public AgricultureDataImport(bool fCheckDataExist = true)
        : base(JCSJ_SJHZ.GetTableName())
    {
      _fCheckDataExist = fCheckDataExist;
    }


    /// <summary>
    /// 汇交数据读取
    /// </summary>
    /// <returns></returns>
    private List<DataSummary> DataSummeryReader(InputParam pathInfo)
    {
      if (string.IsNullOrEmpty(pathInfo.SumTablePath))
      {
        return new List<DataSummary>();
      }
      if (!System.IO.Directory.Exists(pathInfo.SumTablePath))
      {
        return new List<DataSummary>();
      }
      string[] files = System.IO.Directory.GetFiles(pathInfo.SumTablePath);
      if (files == null || files.Length == 0)
      {
        return new List<DataSummary>();
      }
      List<string> names = files.ToList().FindAll(na =>
      {
        string extension = System.IO.Path.GetExtension(na).ToLower();
        return extension == ".xls" || extension == ".xlsx";
      });
      List<DataSummary> datas = new List<DataSummary>();
      using (var reader = new DataSummeryReader())
      {
        reader.InitalizeAllSummeryData(names);
        datas = reader.Datas;
      }
      names = null;
      files = null;
      GC.Collect();
      return datas;
    }

    public void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
    {
      var db = MyGlobal.Workspace;
      try
      {
        //if(false)
        if (_fCheckDataExist && !PreImportCheck(db, reportInfo))
        {
          return;
        }

        reportInfo.reportInfo("开始时间:" + DateTime.Now.ToLocalTime());
        List<DataSummary> datas = DataSummeryReader(prm);
        int cnt = datas.Count;
        double oldProgress = 0;
        try
        {
          db.BeginTransaction();
          datas = datas.OrderBy(da => da.ZoneCode).ToList();
          int count = datas.Count > 0 ? datas.Count : 1;
          base.RecordCount = count;
          int index = 0;
          datas.ForEach(da =>
          {
            var en = da.ToModel();
            SjhzRepository.Instance.Insert(en);
            ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, ++index, ref oldProgress);
          });
          db.Commit();

          ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);

          reportInfo.reportInfo("成功导入汇总数据" + datas.Count + "条");
          //this.ReportInfomation("成功导入汇总数据" + datas.Count + "条");
        }
        catch (SystemException ex)
        {
          db.Rollback();
          //result = false;
          reportInfo.reportError("导入汇总数据失败!" + ex.Message);
          //this.ReportError("导入汇总数据失败!" + ex.Message);
        }

        datas = null;
        GC.Collect();
        reportInfo.reportProgress(100);
        //this.ReportProgress(100, "汇总数据处理完毕");
        //return result;
      }
      catch (Exception ex)
      {
        reportInfo.reportError(ex.Message);
        //base.ReportException(ex);
      }
    }
  }


  /// <summary>
  /// 统计实体
  /// </summary>
  public class DataSummary
  {
    #region Filds

    private string zoneCode;

    #endregion

    #region Const

    /// <summary>
    /// 单位编码
    /// </summary>
    public const string UnitCodeName = "UnitCode";

    /// <summary>
    /// 单位名称
    /// </summary>
    public const string UnitNameName = "UnitName";

    /// <summary>
    /// 承包地地块总面积
    /// </summary>
    public const string ContractLandAreaCountName = "ContractLandAreaCount";

    /// <summary>
    /// 非承包地地块总面积
    /// </summary>
    public const string UnContractLandAreaCountName = "UnContractLandAreaCount";

    /// <summary>
    /// 农业用途总面积
    /// </summary>
    public const string AgricureAreaCountName = "AgricureAreaCount";

    /// <summary>
    /// 种植业总面积
    /// </summary>
    public const string PlantAreaCountName = "PlantAreaCount";

    /// <summary>
    /// 林业总面积
    /// </summary>
    public const string ForestAreaCountName = "ForestAreaCount";

    /// <summary>
    /// 畜牧业总面积
    /// </summary>
    public const string AnimalAreaCountName = "AnimalAreaCount";

    /// <summary>
    /// 渔业总面积
    /// </summary>
    public const string FishAreaCountName = "FishAreaCount";

    /// <summary>
    /// 非农业用途总面积
    /// </summary>
    public const string UnAgricureAreaCountName = "UnAgricureAreaCount";

    /// <summary>
    /// 农业与非农业用途总面积
    /// </summary>
    public const string AgricureAndUnAreaCountName = "AgricureAndUnAreaCount";

    /// <summary>
    /// 集体土地所有权面积
    /// </summary>
    public const string CollectivityAreaName = "CollectivityArea";

    /// <summary>
    /// 村民小组所有面积
    /// </summary>
    public const string GroupAreaCountName = "GroupAreaCount";

    /// <summary>
    /// 村级集体经济组织
    /// </summary>
    public const string GroupCollectivityAreaName = "GroupCollectivityArea";

    /// <summary>
    /// 乡级集体经济组织
    /// </summary>
    public const string TownCollectivityAreaName = "TownCollectivityArea";

    /// <summary>
    /// 其他集体经济组织
    /// </summary>
    public const string OtherCollectivityAreaName = "OtherCollectivityArea";

    /// <summary>
    /// 国有土地所有权面积
    /// </summary>
    public const string CountryAreaCountName = "CountryAreaCount";

    /// <summary>
    /// 所有权地面积
    /// </summary>
    public const string CountryAndCollAreaCountName = "CountryAndCollAreaCount";

    /// <summary>
    /// 自留地面积
    /// </summary>
    public const string SelfLandAreaCountName = "SelfLandAreaCount";

    /// <summary>
    /// 机动地面积
    /// </summary>
    public const string MoveLandAreaCountName = "MoveLandAreaCount";

    /// <summary>
    /// 开荒地
    /// </summary>
    public const string WastLandAreaCountName = "WastLandAreaCount";

    /// <summary>
    /// 其他集体土地
    /// </summary>
    public const string OtherCollectivityLandAreaName = "OtherCollectivityLandArea";

    /// <summary>
    /// 非承包方面积合计
    /// </summary>
    public const string UnContractLandTypeAreaName = "UnContractLandTypeArea";

    /// <summary>
    /// 承包地是基本农田面积
    /// </summary>
    public const string ContractIsBaseAreaName = "ContractIsBaseArea";

    /// <summary>
    /// 承包地非基本农田面积
    /// </summary>
    public const string ContractNotBaseAreaName = "ContractNotBaseArea";

    /// <summary>
    /// 承包地是否基本农田面积合计
    /// </summary>
    public const string ContractBaseAreaCountName = "ContractBaseAreaCount";

    /// <summary>
    /// 颁发权证面积
    /// </summary>
    public const string GisterbookAreaName = "GisterbookArea";

    /// <summary>
    /// 家庭承包权证数
    /// </summary>
    public const string FamilyContractBookNumberName = "FamilyContractBookNumber";

    /// <summary>
    /// 其他承包权证数
    /// </summary>
    public const string OtherContractBookNumberName = "OtherContractBookNumber";

    /// <summary>
    /// 承包方总数
    /// </summary>
    public const string FamilyCountName = "FamilyCount";

    /// <summary>
    /// 承包农户数
    /// </summary>
    public const string ContractFamilyCountName = "ContractFamilyCount";

    /// <summary>
    /// 承包农户成员数
    /// </summary>
    public const string ContractFamilyPersonCountName = "ContractFamilyPersonCount";

    /// <summary>
    /// 其他方式承包合计
    /// </summary>
    public const string OtherFamilyCountName = "OtherFamilyCount";

    /// <summary>
    /// 单位承包数量
    /// </summary>
    public const string UnitFamilyCountName = "UnitFamilyCount";

    /// <summary>
    /// 个人承包数量
    /// </summary>
    public const string PersonalFamilyCountName = "PersonalFamilyCount";

    /// <summary>
    /// 发包方数量
    /// </summary>
    public const string SenderCountName = "SenderCount";

    /// <summary>
    /// 承包地地块数量
    /// </summary>
    public const string ContractLandCountName = "ContractLandCount";

    /// <summary>
    /// 非承包地地块数量
    /// </summary>
    public const string UnContractLandCountName = "UnContractLandCount";

    /// <summary>
    /// 颁发权证数量
    /// </summary>
    public const string RegisterBookNumberName = "RegisterBookNumber";

    #endregion

    #region Property

    /// <summary>
    /// 地域等级
    /// </summary>
    public eZoneLevel Level { get; set; }

    /// <summary>
    /// 地域编码
    /// </summary>
    public string ZoneCode
    {
      get { return zoneCode; }
      set
      {
        zoneCode = value;
        switch (zoneCode.Length)
        {
          case 6:
            Level = eZoneLevel.County;
            break;
          case 9:
            Level = eZoneLevel.Town;
            break;
          case 12:
            Level = eZoneLevel.Village;
            break;
          case 14:
            Level = eZoneLevel.Group;
            break;
        }
        //UnitCode = zoneCode.PadRight(14, '0');
        //if (zoneCode.Length == 16)
        //{
        //    UnitCode = zoneCode.Substring(0, 12) + zoneCode.Substring(14, 2);
        //}
      }
    }

    /// <summary>
    /// 权属单位代码
    /// </summary>
    public string UnitCode { get; set; }

    /// <summary>
    /// 权属单位名称
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// 发包方数量
    /// </summary>
    public int SenderCount { get; set; }

    /// <summary>
    /// 承包地地块数量
    /// </summary>
    public int ContractLandCount { get; set; }

    /// <summary>
    /// 承包地地块总面积
    /// </summary>
    public decimal ContractLandAreaCount { get; set; }

    /// <summary>
    /// 非承包地地块数量
    /// </summary>
    public int UnContractLandCount { get; set; }

    /// <summary>
    /// 非承包地地块总面积
    /// </summary>
    public decimal UnContractLandAreaCount { get; set; }

    /// <summary>
    /// 颁发权证数量
    /// </summary>
    public int RegisterBookNumber { get; set; }

    /// <summary>
    /// 农业用途总面积
    /// </summary>
    public decimal AgricureAreaCount { get; set; }

    /// <summary>
    /// 种植业总面积
    /// </summary>
    public decimal PlantAreaCount { get; set; }

    /// <summary>
    /// 林业总面积
    /// </summary>
    public decimal ForestAreaCount { get; set; }

    /// <summary>
    /// 畜牧业总面积
    /// </summary>
    public decimal AnimalAreaCount { get; set; }

    /// <summary>
    /// 渔业总面积
    /// </summary>
    public decimal FishAreaCount { get; set; }

    /// <summary>
    /// 非农业用途总面积
    /// </summary>
    public decimal UnAgricureAreaCount { get; set; }

    /// <summary>
    /// 农业与非农业用途总面积
    /// </summary>
    public decimal AgricureAndUnAreaCount { get; set; }

    /// <summary>
    /// 集体土地所有权面积
    /// </summary>
    public decimal CollectivityArea { get; set; }

    /// <summary>
    /// 村民小组所有面积
    /// </summary>
    public decimal GroupAreaCount { get; set; }

    /// <summary>
    /// 村级集体经济组织
    /// </summary>
    public decimal GroupCollectivityArea { get; set; }

    /// <summary>
    /// 乡级集体经济组织
    /// </summary>
    public decimal TownCollectivityArea { get; set; }

    /// <summary>
    /// 其他集体经济组织
    /// </summary>
    public decimal OtherCollectivityArea { get; set; }

    /// <summary>
    /// 国有土地所有权面积
    /// </summary>
    public decimal CountryAreaCount { get; set; }

    /// <summary>
    /// 所有权地面积
    /// </summary>
    public decimal CountryAndCollAreaCount { get; set; }

    /// <summary>
    /// 自留地面积
    /// </summary>
    public decimal SelfLandAreaCount { get; set; }

    /// <summary>
    /// 机动地面积
    /// </summary>
    public decimal MoveLandAreaCount { get; set; }

    /// <summary>
    /// 开荒地
    /// </summary>
    public decimal WastLandAreaCount { get; set; }

    /// <summary>
    /// 其他集体土地
    /// </summary>
    public decimal OtherCollectivityLandArea { get; set; }

    /// <summary>
    /// 非承包地面积合计
    /// </summary>
    public decimal UnContractLandTypeArea { get; set; }

    /// <summary>
    /// 承包地是基本农田面积
    /// </summary>
    public decimal ContractIsBaseArea { get; set; }

    /// <summary>
    /// 承包地非基本农田面积
    /// </summary>
    public decimal ContractNotBaseArea { get; set; }

    /// <summary>
    /// 承包地是否基本农田面积合计
    /// </summary>
    public decimal ContractBaseAreaCount { get; set; }

    /// <summary>
    /// 家庭承包权证数
    /// </summary>
    public int FamilyContractBookNumber { get; set; }

    /// <summary>
    /// 其他承包权证数
    /// </summary>
    public int OtherContractBookNumber { get; set; }

    /// <summary>
    /// 颁发权证数量
    /// </summary>
    public int RegisterBookCountNumber { get; set; }

    /// <summary>
    /// 颁发权证面积
    /// </summary>
    public decimal GisterbookArea { get; set; }

    /// <summary>
    /// 承包方总数
    /// </summary>
    public int FamilyCount { get; set; }

    /// <summary>
    /// 承包农户数
    /// </summary>
    public int ContractFamilyCount { get; set; }

    /// <summary>
    /// 承包农户成员数
    /// </summary>
    public int ContractFamilyPersonCount { get; set; }

    /// <summary>
    /// 其他方式承包合计
    /// </summary>
    public int OtherFamilyCount { get; set; }

    /// <summary>
    /// 单位承包数量
    /// </summary>
    public int UnitFamilyCount { get; set; }

    /// <summary>
    /// 个人承包数量
    /// </summary>
    public int PersonalFamilyCount { get; set; }

    #endregion

    #region Ctor

    public DataSummary()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    /// 累计
    /// </summary>
    public void Add(DataSummary ds)
    {
      this.ContractLandAreaCount += ds.ContractLandAreaCount;
      this.UnContractLandAreaCount += ds.UnContractLandAreaCount;
      this.AgricureAreaCount += ds.AgricureAreaCount;
      this.PlantAreaCount += ds.PlantAreaCount;
      this.ForestAreaCount += ds.ForestAreaCount;
      this.AnimalAreaCount += ds.AnimalAreaCount;
      this.FishAreaCount += ds.FishAreaCount;
      this.UnAgricureAreaCount += ds.UnAgricureAreaCount;
      this.AgricureAndUnAreaCount += ds.AgricureAndUnAreaCount;
      this.CollectivityArea += ds.CollectivityArea;
      this.GroupAreaCount += ds.GroupAreaCount;
      this.GroupCollectivityArea += ds.GroupCollectivityArea;
      this.TownCollectivityArea += ds.TownCollectivityArea;
      this.OtherCollectivityArea += ds.OtherCollectivityArea;
      this.CountryAreaCount += ds.CountryAreaCount;
      this.CountryAndCollAreaCount += ds.CountryAndCollAreaCount;
      this.SelfLandAreaCount += ds.SelfLandAreaCount;
      this.MoveLandAreaCount += ds.MoveLandAreaCount;
      this.WastLandAreaCount += ds.WastLandAreaCount;
      this.OtherCollectivityLandArea += ds.OtherCollectivityLandArea;
      this.UnContractLandTypeArea += ds.UnContractLandTypeArea;
      this.ContractIsBaseArea += ds.ContractIsBaseArea;
      this.ContractNotBaseArea += ds.ContractNotBaseArea;
      this.ContractBaseAreaCount += ds.ContractBaseAreaCount;
      this.GisterbookArea += ds.GisterbookArea;
      this.FamilyContractBookNumber += ds.FamilyContractBookNumber;
      this.OtherContractBookNumber += ds.OtherContractBookNumber;
      this.FamilyCount += ds.FamilyCount;
      this.ContractFamilyCount += ds.ContractFamilyCount;
      this.ContractFamilyPersonCount += ds.ContractFamilyPersonCount;
      this.OtherFamilyCount += ds.OtherFamilyCount;
      this.UnitFamilyCount += ds.UnitFamilyCount;
      this.PersonalFamilyCount += ds.PersonalFamilyCount;
      this.SenderCount += ds.SenderCount;
      this.ContractLandCount += ds.ContractLandCount;
      this.UnContractLandCount += ds.UnContractLandCount;
      this.RegisterBookNumber += ds.RegisterBookNumber;
    }

    /// <summary>
    /// ClearValue
    /// </summary>
    /// <param name="ds"></param>
    public void ClearValue()
    {
      this.ContractLandAreaCount = 0;
      this.UnContractLandAreaCount = 0;
      this.AgricureAreaCount = 0;
      this.PlantAreaCount = 0;
      this.ForestAreaCount = 0;
      this.AnimalAreaCount = 0;
      this.FishAreaCount = 0;
      this.UnAgricureAreaCount = 0;
      this.AgricureAndUnAreaCount = 0;
      this.CollectivityArea = 0;
      this.GroupAreaCount = 0;
      this.GroupCollectivityArea = 0;
      this.TownCollectivityArea = 0;
      this.OtherCollectivityArea = 0;
      this.CountryAreaCount = 0;
      this.CountryAndCollAreaCount = 0;
      this.SelfLandAreaCount = 0;
      this.MoveLandAreaCount = 0;
      this.WastLandAreaCount = 0;
      this.OtherCollectivityLandArea = 0;
      this.UnContractLandTypeArea = 0;
      this.ContractIsBaseArea = 0;
      this.ContractNotBaseArea = 0;
      this.ContractBaseAreaCount = 0;
      this.GisterbookArea = 0;
      this.FamilyContractBookNumber = 0;
      this.OtherContractBookNumber = 0;
      this.FamilyCount = 0;
      this.ContractFamilyCount = 0;
      this.ContractFamilyPersonCount = 0;
      this.OtherFamilyCount = 0;
      this.UnitFamilyCount = 0;
      this.PersonalFamilyCount = 0;
      this.SenderCount = 0;
      this.ContractLandCount = 0;
      this.UnContractLandCount = 0;
      this.RegisterBookNumber = 0;
    }

    #endregion
  }

  /// <summary>
  /// 数据汇总
  /// </summary>
  public static class JCSJ_SJHZ_CONVERT
  {
    #region 汇总数据

    /// <summary>
    /// 转换到底层实体
    /// </summary>
    /// <param name="data">控制点</param>
    /// <returns>控制点</returns>
    public static JCSJ_SJHZ ToModel(this DataSummary data)
    {
      if (data == null)
      {
        return null;
      }
      var entry = new JCSJ_SJHZ
      {
        BM = data.ZoneCode,
        QSDWDM = data.UnitCode,
        QSDWMC = data.UnitName,
        FBFZS = data.SenderCount,
        CBDKZS = data.ContractLandCount,
        CBDKZMJ = data.ContractLandAreaCount,
        FCBDKZS = data.UnContractLandCount,
        FCBDZMJ = data.UnContractLandAreaCount,
        BFQZZS = data.RegisterBookNumber,
        NYYTZMJ = data.AgricureAreaCount,
        ZZYZMJ = data.PlantAreaCount,
        LYZMJ = data.ForestAreaCount,
        XMYZMJ = data.AnimalAreaCount,
        YYZMJ = data.FishAreaCount,
        FNYYTZMJ = data.UnAgricureAreaCount,
        NYYFNYYTZMJ = data.AgricureAndUnAreaCount,
        JTTDSYQMJ = data.CollectivityArea,
        CMXZSYMJ = data.GroupAreaCount,
        CJJTJJZZMJ = data.GroupCollectivityArea,
        XJJTJJZZMJ = data.TownCollectivityArea,
        QTJTJJZZMJ = data.OtherCollectivityArea,
        GYTDSYQMJ = data.CountryAreaCount,
        SYQMJ = data.CountryAndCollAreaCount,
        ZLDMJ = data.SelfLandAreaCount,
        JDDMJ = data.MoveLandAreaCount,
        KHDMJ = data.WastLandAreaCount,
        QTJTTDMJ = data.OtherCollectivityLandArea,
        FCBDMJHJ = data.UnContractLandTypeArea,
        JBNTMJ = data.ContractIsBaseArea,
        FJBNTMJ = data.ContractNotBaseArea,
        JBNTMJHJ = data.ContractBaseAreaCount,
        JTCBQZS = data.FamilyContractBookNumber,
        QTCBQZS = data.OtherContractBookNumber,
        BFQZSL = data.RegisterBookCountNumber,
        BFQZMJ = data.GisterbookArea,
        CBFZS = data.FamilyCount,
        CBNHS = data.ContractFamilyCount,
        CBNHCYS = data.ContractFamilyPersonCount,
        QTFSCBHJ = data.OtherFamilyCount,
        DWCBZS = data.UnitFamilyCount,
        GRCBZS = data.PersonalFamilyCount
      };
      return entry;
    }

    /// <summary>
    /// 转换到底层实体集合
    /// </summary>
    /// <param name="datas">控制点集合</param>
    /// <returns>控制点集合</returns>
    public static List<JCSJ_SJHZ> ToModelArray(this List<DataSummary> datas)
    {
      if (datas == null || datas.Count == 0)
      {
        return new List<JCSJ_SJHZ>();
      }
      var entrys = new List<JCSJ_SJHZ>();
      foreach (var data in datas)
      {
        var entry = data.ToModel();
        if (entry == null)
        {
          continue;
        }
        entrys.Add(entry);
      }
      return entrys;
    }

    #endregion
  }

  /// <summary>
  /// 汇总表格读取
  /// </summary>
  public class DataSummeryReader : ExcelBase
  {
    #region Propertys

    /// <summary>
    /// 汇总集合
    /// </summary>
    public List<DataSummary> Datas { get; set; }

    #endregion

    #region Ctor

    /// <summary>
    /// 构造方法
    /// </summary>
    public DataSummeryReader()
    {
      Datas = new List<DataSummary>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// 初始化石油汇总数据
    /// </summary>
    public void InitalizeAllSummeryData(List<string> files)
    {
      if (files == null || files.Count == 0)
      {
        return;
      }
      string fileName = files.Find(file => file.LastIndexOf("承包地是否基本农田汇总表.xls") > 0);
      InitalizeFarmerLandData(fileName);
      fileName = files.Find(file => file.LastIndexOf("承包地土地用途汇总表.xls") > 0);
      InitalizeLandPurposeData(fileName);
      fileName = files.Find(file => file.LastIndexOf("承包方汇总表.xls") > 0);
      InitalizeContracotrData(fileName);
      fileName = files.Find(file => file.LastIndexOf("地块汇总表.xls") > 0);
      InitalizeLandData(fileName);
      fileName = files.Find(file => file.LastIndexOf("非承包地地块类别汇总表.xls") > 0);
      InitalizeLandCatalogData(fileName);
      fileName = files.Find(file => file.LastIndexOf("权证信息汇总表.xls") > 0);
      InitalizeWarrantData(fileName);
      SummeryDataProgress();
    }

    /// <summary>
    /// 初始化权证汇总信息
    /// </summary>
    public void InitalizeWarrantData(string fileName)
    {
      object[,] allItem = InitalizeValideData(fileName, 4, 6);
      if (allItem == null)
      {
        return;
      }
      int rowCount = GetRangeRowCount();
      try
      {
        for (int index = 4; index < rowCount; index++)
        {
          string zoneCode = GetString(allItem[index, 0]).TrimSafe();
          if (string.IsNullOrEmpty(zoneCode))
          {
            continue;
          }
          var data = Datas.Find(da => da.ZoneCode == zoneCode);
          if (data == null)
          {
            continue;
          }
          data.RegisterBookCountNumber = GetInt32(allItem[index, 2]);
          data.FamilyContractBookNumber = GetInt32(allItem[index, 3]);
          data.OtherContractBookNumber = GetInt32(allItem[index, 4]);
          data.GisterbookArea = (decimal)GetDouble(allItem[index, 5]);
        }
      }
      catch (SystemException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      finally
      {
        base.Dispose();
        allItem = null;
        GC.Collect();
      }
    }

    /// <summary>
    /// 初始化地块类别汇总信息
    /// </summary>
    public void InitalizeLandCatalogData(string fileName)
    {
      object[,] allItem = InitalizeValideData(fileName, 4, 7);
      if (allItem == null)
      {
        return;
      }
      int rowCount = GetRangeRowCount();
      try
      {
        for (int index = 4; index < rowCount; index++)
        {
          string zoneCode = GetString(allItem[index, 0]).TrimSafe();
          if (string.IsNullOrEmpty(zoneCode))
          {
            continue;
          }
          var data = Datas.Find(da => da.ZoneCode == zoneCode);
          if (data == null)
          {
            continue;
          }
          data.SelfLandAreaCount = (decimal)GetDouble(allItem[index, 2]);
          data.MoveLandAreaCount = (decimal)GetDouble(allItem[index, 3]);
          data.WastLandAreaCount = (decimal)GetDouble(allItem[index, 4]);
          data.OtherCollectivityLandArea = (decimal)GetDouble(allItem[index, 5]);
          data.UnContractLandTypeArea = (decimal)GetDouble(allItem[index, 6]);
        }
      }
      catch (SystemException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      finally
      {
        base.Dispose();
        allItem = null;
        GC.Collect();
      }
    }

    /// <summary>
    /// 初始化地块汇总信息
    /// </summary>
    public void InitalizeLandData(string fileName)
    {
      object[,] allItem = InitalizeValideData(fileName, 4, 8);
      if (allItem == null)
      {
        return;
      }
      int rowCount = GetRangeRowCount();
      try
      {
        for (int index = 4; index < rowCount; index++)
        {
          string zoneCode = GetString(allItem[index, 0]).TrimSafe();
          if (string.IsNullOrEmpty(zoneCode))
          {
            continue;
          }
          var data = Datas.Find(da => da.ZoneCode == zoneCode);
          if (data == null)
          {
            continue;
          }
          data.SenderCount = GetInt32(allItem[index, 2]);
          data.ContractLandCount = GetInt32(allItem[index, 3]);
          data.ContractLandAreaCount = (decimal)GetDouble(allItem[index, 4]);
          data.UnContractLandCount = GetInt32(allItem[index, 5]);
          data.UnContractLandAreaCount = (decimal)GetDouble(allItem[index, 6]);
          data.RegisterBookNumber = GetInt32(allItem[index, 7]);
        }
      }
      catch (SystemException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      finally
      {
        base.Dispose();
        allItem = null;
        GC.Collect();
      }
    }

    /// <summary>
    /// 初始化土地用途汇总信息
    /// </summary>
    public void InitalizeLandPurposeData(string fileName)
    {
      object[,] allItem = InitalizeValideData(fileName, 4, 9);
      if (allItem == null)
      {
        return;
      }
      int rowCount = GetRangeRowCount();
      try
      {
        for (int index = 4; index < rowCount; index++)
        {
          string zoneCode = GetString(allItem[index, 0]).TrimSafe();
          if (string.IsNullOrEmpty(zoneCode))
          {
            continue;
          }
          var data = Datas.Find(da => da.ZoneCode == zoneCode);
          if (data == null)
          {
            continue;
          }
          data.AgricureAreaCount = (decimal)GetDouble(allItem[index, 2]);
          data.PlantAreaCount = (decimal)GetDouble(allItem[index, 3]);
          data.ForestAreaCount = (decimal)GetDouble(allItem[index, 4]);
          data.AnimalAreaCount = (decimal)GetDouble(allItem[index, 5]);
          data.FishAreaCount = (decimal)GetDouble(allItem[index, 6]);
          data.UnAgricureAreaCount = (decimal)GetDouble(allItem[index, 7]);
          data.AgricureAndUnAreaCount = (decimal)GetDouble(allItem[index, 8]);
        }
      }
      catch (SystemException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      finally
      {
        base.Dispose();
        allItem = null;
        GC.Collect();
      }
    }

    /// <summary>
    /// 初始化基本农田汇总信息
    /// </summary>
    public void InitalizeFarmerLandData(string fileName)
    {
      object[,] allItem = InitalizeValideData(fileName, 3, 5);
      if (allItem == null)
      {
        return;
      }
      int rowCount = GetRangeRowCount();
      try
      {
        for (int index = 3; index < rowCount; index++)
        {
          DataSummary data = new DataSummary();
          data.ZoneCode = GetString(allItem[index, 0]).TrimSafe();
          if (string.IsNullOrEmpty(data.ZoneCode))
          {
            continue;
          }
          data.UnitCode = GetString(allItem[index, 0]).TrimSafe();
          data.UnitName = GetString(allItem[index, 1]).TrimSafe();
          data.ContractIsBaseArea = (decimal)GetDouble(allItem[index, 2]);
          data.ContractNotBaseArea = (decimal)GetDouble(allItem[index, 3]);
          data.ContractBaseAreaCount = (decimal)GetDouble(allItem[index, 4]);
          Datas.Add(data);
        }
      }
      catch (SystemException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      finally
      {
        base.Dispose();
        allItem = null;
        GC.Collect();
      }
    }

    /// <summary>
    /// 初始化承包方汇总信息
    /// </summary>
    public void InitalizeContracotrData(string fileName)
    {
      object[,] allItem = InitalizeValideData(fileName, 4, 8);
      if (allItem == null)
      {
        return;
      }
      int rowCount = GetRangeRowCount();
      try
      {
        for (int index = 4; index < rowCount; index++)
        {
          string zoneCode = GetString(allItem[index, 0]).TrimSafe();
          if (string.IsNullOrEmpty(zoneCode))
          {
            continue;
          }
          var data = Datas.Find(da => da.ZoneCode == zoneCode);
          if (data == null)
          {
            continue;
          }
          data.FamilyCount = GetInt32(allItem[index, 2]);
          data.ContractFamilyCount = GetInt32(allItem[index, 3]);
          data.ContractFamilyPersonCount = GetInt32(allItem[index, 4]);
          data.OtherFamilyCount = GetInt32(allItem[index, 5]);
          data.UnitFamilyCount = GetInt32(allItem[index, 6]);
          data.PersonalFamilyCount = GetInt32(allItem[index, 7]);
        }
      }
      catch (SystemException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
      finally
      {
        base.Dispose();
        allItem = null;
        GC.Collect();
      }
    }

    /// <summary>
    /// 判断数据是否有效
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <returns></returns>
    private object[,] InitalizeValideData(string fileName, int rowNumber, int columnNumber)
    {
      if (string.IsNullOrEmpty(fileName))
      {
        return null;
      }
      bool canContinue = Open(fileName);
      if (!canContinue)
      {
        return null;
      }
      object[,] allItem = GetAllRangeValue();//获取所有使用域值
      if (allItem == null)
      {
        return null;
      }
      int rowCount = GetRangeRowCount();
      int columnCount = GetRangeColumnCount();
      if (rowCount < rowNumber || columnCount < columnNumber)
      {
        return null;
      }
      return allItem;
    }

    /// <summary>
    /// 汇总数据处理
    /// </summary>
    private void SummeryDataProgress()
    {
      if (Datas == null || Datas.Count == 0)
      {
        return;
      }
      Datas.ForEach(da => { InitalizeSummeryData(da); });
    }

    /// <summary>
    /// 初始化发包方编码
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private void InitalizeSummeryData(DataSummary data)
    {
      string zoneCode = data.ZoneCode;
      zoneCode = zoneCode.TrimEnd('0');
      if (zoneCode.Length > 12 && zoneCode.Length < 14)
      {
        zoneCode = zoneCode.PadRight(14, '0');
      }
      if (zoneCode.Length > 9 && zoneCode.Length < 12)
      {
        zoneCode = zoneCode.PadRight(12, '0');
      }
      if (zoneCode.Length > 6 && zoneCode.Length < 9)
      {
        zoneCode = zoneCode.PadRight(9, '0');
      }
      if (zoneCode.Length > 4 && zoneCode.Length < 6)
      {
        zoneCode = zoneCode.PadRight(6, '0');
      }
      if (zoneCode.Length > 2 && zoneCode.Length < 4)
      {
        zoneCode = zoneCode.PadRight(9, '0');
      }
      if (zoneCode.Length > 0 && zoneCode.Length < 2)
      {
        zoneCode = zoneCode.PadRight(2, '0');
      }
      data.ZoneCode = zoneCode;
      switch (zoneCode.Length)
      {
        case 2:
          data.Level = eZoneLevel.Province;
          break;
        case 4:
          data.Level = eZoneLevel.City;
          break;
        case 6:
          data.Level = eZoneLevel.County;
          break;
        case 9:
          data.Level = eZoneLevel.Town;
          break;
        case 12:
          data.Level = eZoneLevel.Village;
          break;
        case 14:
          data.Level = eZoneLevel.Group;
          break;
        default:
          break;
      }
    }

    #endregion
  }
}
