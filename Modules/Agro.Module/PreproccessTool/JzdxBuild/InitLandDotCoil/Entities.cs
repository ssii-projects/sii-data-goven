using GeoAPI.Geometries;
using System.Collections.Generic;

namespace JzdxBuild
{
    /// <summary>
    /// 承包地实体
    /// </summary>
    public class Zd_cbdEntity
    {

    }
    /// <summary>
    /// 承包地字段常量
    /// </summary>
    public class Zd_cbdFields
    {
        //public const string TABLE_NAME = "zd_cbd";
        //public const string RowID = "rowid";
        //public const string ID = "ID";
        /// <summary>
        /// 地域代码
        /// </summary>
        //public const string ZLDM = "ZLDM";
        public const string Shape = "shape";
        ///// <summary>
        ///// 权利人名称
        ///// </summary>
        //public const string Qlrmc = "qlrmc";
        public const string DKBM = "DKBM";
        /// <summary>
        /// 地块名称
        /// </summary>
        public const string DKMC = "DKMC";
        ///// <summary>
        ///// 扩展属性
        ///// </summary>
        //public const string DKKZXX = "DKKZXX";
        public const string DKLB = "DKLB";
        public const string TDLYLX = "TDLYLX";
        public const string DLDJ = "DLDJ";
        public const string TDYT = "TDYT";
        public const string SFJBNT = "SFJBNT";
        public const string SCMJ = "SCMJ";
        public const string DKDZ = "DKDZ";
        public const string DKXZ = "DKXZ";
        public const string DKNZ = "DKNZ";
        public const string DKBZ = "DKBZ";
        public const string DKBZXX = "DKBZXX";
        /// <summary>
        /// 指界人姓名
        /// </summary>
        public const string ZJRXM = "ZJRXM";
    }

    /// <summary>
    /// 界址点字段常量
    /// </summary>
    public class JzdFields
    {
        //public const string TABLE_NAME = "JZD";
        //public const string ID = "ID";
        //public const string DKID = "DKID";
        public const string BSM = "BSM";
        public const string YSDM = "YSDM";
        ///// <summary>
        ///// 标识码
        ///// </summary>
        //public const string BSM = "BSM";
        /// <summary>
        /// 统编界址点号
        /// </summary>
        //public const string TBJZDH = "TBJZDH";
        /// <summary>
        /// 界址点号
        /// </summary>
        public const string JZDH = "JZDH";
        /// <summary>
        /// 界标类型
        /// </summary>
        public const string JBLX = "JBLX";
        /// <summary>
        /// 界址点类型
        /// </summary>
        public const string JZDLX = "JZDLX";

		/// <summary>
		/// 地块编码
		/// </summary>
		public const string DKBM = "DKBM";
		public const string XZBZ = "XZBZ";
        public const string YZBZ = "YZBZ";
        ///// <summary>
        ///// 创建时间
        ///// </summary>
        //public const string CJSJ = "CJSJ";
        ///// <summary>
        ///// 地域编码
        ///// </summary>
        //public const string DYBM = "DYBM";

        ///// <summary>
        ///// 是否关键界址点
        ///// </summary>
        //public const string SFKY = "SFKY";
        //public const string SHAPE = "shape";
    }

    /// <summary>
    /// 界址点实体
    /// </summary>
    public class JzdEntity
    {
        //public int rowID;
        /// <summary>
        /// 界址点ID
        /// </summary>
        //public string ID;
        ///// <summary>
        ///// 标识码
        ///// </summary>
        //public string BSM;
        /// <summary>
        /// 界址点号
        /// </summary>
        //public short JZDH;
        ///// <summary>
        ///// 统编界址点号
        ///// </summary>
        //public int TBJZDH;
        /// <summary>
        /// 界标类型
        /// </summary>
        //public short JBLX;
        ///// <summary>
        ///// 界址点类型
        ///// </summary>
        //public short JZDLX;
        ///// <summary>
        ///// 地块标识
        ///// </summary>
        //public string dkID;
        ///// <summary>
        ///// 地域编码
        ///// </summary>
        //public string DYBM;
        //public string JZDSSQLLX;
        ///// <summary>
        ///// 地块编码
        ///// </summary>
        //public string dkBM;

        /// <summary>
        /// 是否关键界址点
        /// </summary>
        public bool SFKY=false;
        public Coordinate shape;
        /// <summary>
        /// 是否环的最后一个点
        /// </summary>
        public bool fRingLastPoint=false;

        public readonly HashSet<ShortZd_cbd> Dks = new HashSet<ShortZd_cbd>();

        public string GetDkbm()
        {
            string dkbm = null;
            foreach (var dk in Dks)
            {
                if (dkbm == null) dkbm = dk.DKBM;
                else dkbm += "/" + dk.DKBM;
            }
            return dkbm;
        }
    }

    /// <summary>
    /// 界址线字段常量
    /// </summary>
    public class JzxFields
    {
        /// <summary>
        /// 标识码
        /// </summary>
        public const string BSM = "BSM";
        /// <summary>
        /// 要素代码
        /// </summary>
        public const string YSDM = "YSDM";
        /// <summary>
        /// 界线性质
        /// </summary>
        public const string JXXZ = "JXXZ";
        /// <summary>
        /// 界址线类别
        /// </summary>
        public const string JZXLB = "JZXLB";
        /// <summary>
        /// 界址线位置
        /// </summary>
        public const string JZXWZ = "JZXWZ";
 
        /// <summary>
        /// 界址线说明
        /// </summary>
        public const string JZXSM = "JZXSM";
        /// <summary>
        /// 毗邻地物指界人
        /// </summary>
        public const string PLDWZJR = "PLDWZJR";
        /// <summary>
        /// 毗邻地物权利人
        /// </summary>
        public const string PLDWQLR = "PLDWQLR";

		#region yxm 2021-4-15
		/// <summary>
		/// 界址线号
		/// </summary>
		public const string JZXH = "JZXH";
        public const string DKBM = "DKBM";
        public const string QJZDH = "QJZDH";
        public const string ZJZDH = "ZJZDH";
		#endregion

		public const string SHAPE = "shape";
    }

    /// <summary>
    /// 界址线字段常量
    /// </summary>
    public class JzxEntity
    {
        ///// <summary>
        ///// 界线性质
        ///// </summary>
        //public string JXXZ;
        ///// <summary>
        ///// 界址线类别
        ///// </summary>
        //public string JZXLB;
        /// <summary>
        /// 界址线位置
        /// </summary>
        public string JZXWZ;


        /// <summary>
        /// 界址线说明
        /// </summary>
        public string JZXSM;
        /// <summary>
        /// 毗邻地物指界人
        /// </summary>
        public string PLDWZJR;
        /// <summary>
        /// 毗邻地物权利人
        /// </summary>
        public string PLDWQLR;

        public Coordinate[] Shape;
        //public List<JzdEntity> lstJzd;
        public void Clear()
        {
            JZXWZ = null;
            JZXSM = null;
            PLDWQLR = null;
            PLDWZJR = null;
            Shape = null;
        }
    }

    
}
