/*
 * (C)2016 公司版权所有,保留所有权利
*/
using System;
using System.Collections.Generic;
using Agriculture.Entity;
using System.Reflection;

namespace Agriculture.Classify
{
    /// <summary>
    ///数据库(表)状态
    /// </summary>
    public class DataBaseCondition
    {
        #region Properties

        /// <summary>
        /// 数据库文件信息
        /// </summary>
        public FileCondition FileInfo { get; set; }

        /// <summary>
        /// 承包地块信息
        /// </summary>
        public FileCondition LandInfo { get; set; }

        /// <summary>
        /// 承包方
        /// </summary>
        public FileCondition VirtualPerson { get; set; }

        /// <summary>
        /// 承包方家庭成员
        /// </summary>
        public FileCondition Person { get; set; }

        /// <summary>
        /// 承包合同
        /// </summary>
        public FileCondition Concord { get; set; }

        /// <summary>
        /// 承包经营权登记簿
        /// </summary>
        public FileCondition RigsterBook { get; set; }

        /// <summary>
        /// 承包经营权证
        /// </summary>
        public FileCondition Warrant { get; set; }

        /// <summary>
        /// 承包经营权证补发
        /// </summary>
        public FileCondition WarrantPatch { get; set; }

        /// <summary>
        /// 承包经营权证换发
        /// </summary>
        public FileCondition WarrantExchange { get; set; }

        /// <summary>
        /// 承包经营权证注销
        /// </summary>
        public FileCondition WarrantLayout { get; set; }

        /// <summary>
        /// 流转合同
        /// </summary>
        public FileCondition TransforConcord { get; set; }

        /// <summary>
        /// 发包方
        /// </summary>
        public FileCondition Contractor { get; set; }

        /// <summary>
        /// 权属来源资料附件
        /// </summary>
        public FileCondition AffixFile { get; set; }

        ///// <summary>
        ///// 栅格数据
        ///// </summary>
        //public FileCondition RuleData { get; set; }

        ///// <summary>
        ///// 界址线说明
        ///// </summary>
        //public FileCondition LineDeclare{ get; set; }

        #endregion

        #region Ctor

        public DataBaseCondition()
        {
            var tableInfo = new BaseTableInfo();
            var propinfo = typeof(BaseTableInfo).GetProperties();
            var propList = new List<PropertyInfo>();
            foreach (var item in propinfo)
            {
                propList.Add(item);
            }
            LandInfo = CreateByName(CBDKXXEX.TableName, tableInfo, propList);
            VirtualPerson = CreateByName(CBF.TableName, tableInfo, propList);
            Person = CreateByName(CBF_JTCY.TableName, tableInfo, propList);
            Concord = CreateByName(CBHT.TableName, tableInfo, propList);
            RigsterBook = CreateByName(CBJYQZDJB.TableName, tableInfo, propList);
            Warrant = CreateByName(CBJYQZ.TableName, tableInfo, propList);
            WarrantPatch = CreateByName(CBJYQZ_QZBF.TableName, tableInfo, propList);
            WarrantExchange = CreateByName(CBJYQZ_QZHF.TableName, tableInfo, propList);
            WarrantLayout = CreateByName(CBJYQZ_QZZX.TableName, tableInfo, propList);
            TransforConcord = CreateByName(LZHT.TableName, tableInfo, propList);
            Contractor = CreateByName(FBF.TableName, tableInfo, propList);
            AffixFile = CreateByName(QSLYZLFJ.TableName, tableInfo, propList);
        }

        private FileCondition CreateByName(string name, BaseTableInfo tableInfo, List<PropertyInfo> infoList)
        {
            var bcon = BaseTableInfo.TableNameList.Find(t => t.Name == name);
            if (bcon != null)
            {
                var condition = new FileCondition() { Name = name };
                condition.AliesName = bcon.AliesName;
                condition.IsNecessary = bcon.IsNecessary;
                var zd = infoList.Find(t => t.Name == name + "ZD");
                if (zd != null)
                {
                    condition.FieldList = zd.GetValue(tableInfo, null) as List<FieldInformation>;
                }
                return condition;
            }
            return null;
        }
        #endregion
    }



    /// <summary>
    /// 矢量文件(表)状态
    /// </summary>
    public class ShapeFileCondition
    {
        #region Properties

        /// <summary>
        /// 控制点文件
        /// </summary>
        public FileCondition ControlPoint { get; set; }

        /// <summary>
        /// 地块文件
        /// </summary>
        public FileCondition Land { get; set; }

        /// <summary>
        /// 界址点文件
        /// </summary>
        public FileCondition LandPoint { get; set; }

        /// <summary>
        /// 界址线文件
        /// </summary>
        public FileCondition LandLine { get; set; }

        /// <summary>
        /// 区域界线文件
        /// </summary>
        public FileCondition ZoneLine { get; set; }

        /// <summary>
        /// 县级行政区文件
        /// </summary>
        public FileCondition County { get; set; }

        /// <summary>
        /// 乡级区域文件
        /// </summary>
        public FileCondition Town { get; set; }

        /// <summary>
        /// 村级区域文件
        /// </summary>
        public FileCondition Village { get; set; }

        /// <summary>
        /// 组级区域文件
        /// </summary>
        public FileCondition Group { get; set; }

        /// <summary>
        /// 线状地物文件
        /// </summary>
        public FileCondition LineThing { get; set; }

        /// <summary>
        /// 面状地物文件
        /// </summary>
        public FileCondition AreaThing { get; set; }

        /// <summary>
        /// 点状地物文件
        /// </summary>
        public FileCondition PointThing { get; set; }

        /// <summary>
        /// 基本农田保护区文件
        /// </summary>
        public FileCondition ProtectArea { get; set; }

        /// <summary>
        /// 注记文件
        /// </summary>
        public FileCondition Record { get; set; }

        /// <summary>
        /// 元数据文件
        /// </summary>
        public FileCondition XML { get; set; }

        public List<FileCondition> LandList { get; set; }

        public List<FileCondition> PointList { get; set; }

        public List<FileCondition> LineList { get; set; }

        /// <summary>
        /// 是否存在地块分表
        /// </summary>
        public bool IsLandSpare { get { return LandList.Count > 0 ? true : false; } }

        /// <summary>
        /// 是否存在界址点分表
        /// </summary>
        public bool IsPointSpare { get { return PointList.Count > 0 ? true : false; } }

        /// <summary>
        /// 是否存在界址线分表
        /// </summary>
        public bool IsLineSpare { get { return LineList.Count > 0 ? true : false; } }

        #endregion

        #region Ctor

        public ShapeFileCondition()
        {
            ShapeFileInfo fileInfo = new ShapeFileInfo();
            var propinfo = typeof(ShapeFileInfo).GetProperties();
            var propList = new List<PropertyInfo>();
            foreach (var item in propinfo)
            {
                propList.Add(item);
            }
            ControlPoint = CreateByName(KZD.TableName, fileInfo, propList);// new FileCondition() { Name = KZD.TableName, AliesName = KZD.TableNameCN, FieldList = fileInfo.KZDZD, IsNecessary = false };
            Land = CreateByName(DK.TableName, fileInfo, propList);// new FileCondition() { Name = DK.TableName, AliesName = DK.TableNameCN, FieldList = fileInfo.DKZD, IsNecessary = true };
            LandPoint = CreateByName(JZD.TableName, fileInfo, propList);//  new FileCondition() { Name = JZD.TableName, AliesName = JZD.TableNameCN, FieldList = fileInfo.JZDZD, IsNecessary = true };
            LandLine = CreateByName(JZX.TableName, fileInfo, propList);//  new FileCondition() { Name = JZX.TableName, AliesName = JZX.TableNameCN, FieldList = fileInfo.JZXZD, IsNecessary = true };
            ProtectArea = CreateByName(JBNTBHQ.TableName, fileInfo, propList);//  new FileCondition() { Name = JBNTBHQ.TableName, AliesName = JBNTBHQ.TableNameCN, FieldList = fileInfo.JBNTBHQZD, IsNecessary = true };
            AreaThing = CreateByName(MZDW.TableName, fileInfo, propList);//  new FileCondition() { Name = MZDW.TableName, AliesName = MZDW.TableNameCN, FieldList = fileInfo.MZDWZD, IsNecessary = false };
            PointThing = CreateByName(DZDW.TableName, fileInfo, propList);//  new FileCondition() { Name = DZDW.TableName, AliesName = DZDW.TableNameCN, FieldList = fileInfo.DZDWZD, IsNecessary = false };
            LineThing = CreateByName(XZDW.TableName, fileInfo, propList);//  new FileCondition() { Name = XZDW.TableName, AliesName = XZDW.TableNameCN, FieldList = fileInfo.XZDWZD, IsNecessary = false };
            Village = CreateByName(CJQY.TableName, fileInfo, propList);//  new FileCondition() { Name = CJQY.TableName, AliesName = CJQY.TableNameCN, FieldList = fileInfo.CJQYZD, IsNecessary = false };
            Group = CreateByName(ZJQY.TableName, fileInfo, propList);// new FileCondition() { Name = ZJQY.TableName, AliesName = ZJQY.TableNameCN, FieldList = fileInfo.ZJQYZD, IsNecessary = false };
            County = CreateByName(XJXZQ.TableName, fileInfo, propList);// new FileCondition() { Name = XJXZQ.TableName, AliesName = XJXZQ.TableNameCN, FieldList = fileInfo.XJXZQZD, IsNecessary = true };
            Town = CreateByName(XJQY.TableName, fileInfo, propList);// new FileCondition() { Name = XJQY.TableName, AliesName = XJQY.TableNameCN, FieldList = fileInfo.XJQYZD, IsNecessary = false };
            ZoneLine = CreateByName(QYJX.TableName, fileInfo, propList);// new FileCondition() { Name = QYJX.TableName, AliesName = QYJX.TableNameCN, FieldList = fileInfo.QYJXZD, IsNecessary = true };
            XML = new FileCondition() { };
            Record = CreateByName(ZJ.TableName, fileInfo, propList);//  new FileCondition() { Name = ZJ.TableName, AliesName = ZJ.TableNameCN, FieldList = fileInfo.ZJZD, IsNecessary = false };
            LandList = new List<FileCondition>();
            PointList = new List<FileCondition>();
            LineList = new List<FileCondition>();
        }


        private FileCondition CreateByName(string name, ShapeFileInfo tableInfo, List<PropertyInfo> infoList)
        {
            var bcon = ShapeFileInfo.TableNameList.Find(t => t.Name == name);
            if (bcon != null)
            {
                var condition = new FileCondition() { Name = name };
                condition.AliesName = bcon.AliesName;
                condition.IsNecessary = bcon.IsNecessary;
                var zd = infoList.Find(t => t.Name == name + "ZD");
                if (zd != null)
                {
                    condition.FieldList = zd.GetValue(tableInfo, null) as List<FieldInformation>;
                }
                return condition;
            }
            return null;
        }
        #endregion
    }
}
