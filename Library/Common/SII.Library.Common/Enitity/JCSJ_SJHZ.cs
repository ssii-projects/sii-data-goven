/*
 * (C) 2015 公司版权所有，保留所有权利
 */
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace Agro.Library.Model
{
    /// <summary>
    /// 汇总实体
    /// </summary>
    [DataTable("SJHZ", AliasName = "汇总表")]
    public class JCSJ_SJHZ : Entity<JCSJ_SJHZ > 
	{
        #region Filds

        private string zoneCode;

		#endregion

		#region Property
		/// <summary>
		/// 唯一标识
		/// </summary>
		[DataColumn("ID", AliasName = "唯一标识")]
		public Guid ID { get; set; } = Guid.NewGuid();

		/// <summary>
		/// 地域等级
		/// </summary>
		[DataColumn("DJ", AliasName = "地域等级")]
        public eZoneLevel DJ { get; set; }

        /// <summary>
        /// 地域编码
        /// </summary>
        [DataColumn("BM", AliasName = "地域编码")]
        public string BM
        {
            get { return zoneCode; }
            set
            {
                zoneCode = value;
                switch (zoneCode.Length)
                {
                    case 2:
                        DJ = eZoneLevel.Province;
                        break;
                    case 4:
                        DJ = eZoneLevel.City;
                        break;
                    case 6:
                        DJ = eZoneLevel.County;
                        break;
                    case 9:
                        DJ = eZoneLevel.Town;
                        break;
                    case 12:
                        DJ = eZoneLevel.Village;
                        break;
                    case 14:
                        DJ = eZoneLevel.Group;
                        break;
                }
            }
        }

        /// <summary>
        /// 权属单位代码
        /// </summary>
        [DataColumn("QSDWDM", AliasName = "权属单位代码")]
        public string QSDWDM { get; set; }

        /// <summary>
        /// 权属单位名称
        /// </summary>
        [DataColumn("QSDWMC", AliasName = "权属单位名称")]
        public string QSDWMC { get; set; }

        /// <summary>
        /// 发包方总数
        /// </summary>
        [DataColumn("FBFZS", AliasName = "发包方总数")]
        public int FBFZS { get; set; }

        /// <summary>
        /// 承包地块总数
        /// </summary>
        [DataColumn("CBDKZS", AliasName = "承包地块总数")]
        public int CBDKZS { get; set; }

        /// <summary>
        /// 承包地块总面积
        /// </summary>
        [DataColumn("CBDKZMJ", AliasName = "承包地块总面积")]
        public decimal CBDKZMJ { get; set; }

        /// <summary>
        /// 非承包地地块数量
        /// </summary>
        [DataColumn("FCBDKZS", AliasName = "非承包地地块总数")]
        public int FCBDKZS { get; set; }

        /// <summary>
        /// 非承包地总面积
        /// </summary>
        [DataColumn("FCBDZMJ", AliasName = "非承包地总面积")]
        public decimal FCBDZMJ { get; set; }

        /// <summary>
        /// 颁发权证总数
        /// </summary>
        [DataColumn("BFQZZS", AliasName = "颁发权证总数")]
        public int BFQZZS { get; set; }

        /// <summary>
        /// 农业用途总面积
        /// </summary>
        [DataColumn("NYYTZMJ", AliasName = "农业用途总面积")]
        public decimal NYYTZMJ { get; set; }

        /// <summary>
        /// 种植业总面积
        /// </summary>
        [DataColumn("ZZYZMJ", AliasName = "种植业总面积")]
        public decimal ZZYZMJ { get; set; }

        /// <summary>
        /// 林业总面积
        /// </summary>
        [DataColumn("LYZMJ", AliasName = "林业总面积")]
        public decimal LYZMJ { get; set; }

        /// <summary>
        /// 畜牧业总面积
        /// </summary>
        [DataColumn("XMYZMJ", AliasName = "畜牧业总面积")]
        public decimal XMYZMJ { get; set; }

        /// <summary>
        /// 渔业总面积
        /// </summary>
        [DataColumn("YYZMJ", AliasName = "渔业总面积")]
        public decimal YYZMJ { get; set; }

        /// <summary>
        /// 非农业用途总面积
        /// </summary>
        [DataColumn("FNYYTZMJ", AliasName = "非农业用途总面积")]
        public decimal FNYYTZMJ { get; set; }

        /// <summary>
        /// 农业与非农业用途总面积
        /// </summary>
        [DataColumn("NYYFNYYTZMJ", AliasName = "农业与非农业用途总面积")]
        public decimal NYYFNYYTZMJ { get; set; }

        /// <summary>
        /// 集体土地所有权面积
        /// </summary>
        [DataColumn("JTTDSYQMJ", AliasName = "集体土地所有权面积")]
        public decimal JTTDSYQMJ { get; set; }

        /// <summary>
        /// 村民小组所有面积
        /// </summary>
        [DataColumn("CMXZSYMJ", AliasName = "村民小组所有面积")]
        public decimal CMXZSYMJ { get; set; }

        /// <summary>
        /// 村级集体经济组织面积
        /// </summary>
        [DataColumn("CJJTJJZZMJ", AliasName = "村级集体经济组织面积")]
        public decimal CJJTJJZZMJ { get; set; }

        /// <summary>
        /// 乡级集体经济组织面积
        /// </summary>
        [DataColumn("XJJTJJZZMJ", AliasName = "乡级集体经济组织面积")]
        public decimal XJJTJJZZMJ { get; set; }

        /// <summary>
        /// 其他集体经济组织面积
        /// </summary>
        [DataColumn("QTJTJJZZMJ", AliasName = "其他集体经济组织面积")]
        public decimal QTJTJJZZMJ { get; set; }

        /// <summary>
        /// 国有土地所有权面积
        /// </summary>
        [DataColumn("GYTDSYQMJ", AliasName = "国有土地所有权面积")]
        public decimal GYTDSYQMJ { get; set; }

        /// <summary>
        /// 所有权面积
        /// </summary>
        [DataColumn("SYQMJ", AliasName = "所有权面积")]
        public decimal SYQMJ { get; set; }

        /// <summary>
        /// 自留地面积
        /// </summary>
        [DataColumn("ZLDMJ", AliasName = "自留地面积")]
        public decimal ZLDMJ { get; set; }

        /// <summary>
        /// 机动地面积
        /// </summary>
        [DataColumn("JDDMJ", AliasName = "机动地面积")]
        public decimal JDDMJ { get; set; }

        /// <summary>
        /// 开荒地面积
        /// </summary>
        [DataColumn("KHDMJ", AliasName = "开荒地面积")]
        public decimal KHDMJ { get; set; }

        /// <summary>
        /// 其他集体土地面积
        /// </summary>
        [DataColumn("QTJTTDMJ", AliasName = "其他集体土地面积")]
        public decimal QTJTTDMJ { get; set; }

        /// <summary>
        /// 非承包地面积合计
        /// </summary>
        [DataColumn("FCBDMJHJ", AliasName = "非承包地面积合计")]
        public decimal FCBDMJHJ { get; set; }

        /// <summary>
        /// 承包地是基本农田面积
        /// </summary>
        [DataColumn("JBNTMJ", AliasName = "承包地是基本农田面积")]
        public decimal JBNTMJ { get; set; }

        /// <summary>
        /// 承包地非基本农田面积
        /// </summary>
        [DataColumn("FJBNTMJ", AliasName = "承包地非基本农田面积")]
        public decimal FJBNTMJ { get; set; }

        /// <summary>
        /// 承包地是否基本农田面积合计
        /// </summary>
        [DataColumn("JBNTMJHJ", AliasName = "承包地是否基本农田面积合计")]
        public decimal JBNTMJHJ { get; set; }

        /// <summary>
        /// 家庭承包权证数
        /// </summary>
        [DataColumn("JTCBQZS", AliasName = "家庭承包权证数")]
        public int JTCBQZS { get; set; }

        /// <summary>
        /// 其他承包权证数
        /// </summary>
        [DataColumn("QTCBQZS", AliasName = "其他承包权证数")]
        public int QTCBQZS { get; set; }

        /// <summary>
        /// 颁发权证数量
        /// </summary>
        [DataColumn("BFQZSL", AliasName = "颁发权证数量")]
        public int BFQZSL { get; set; }

        /// <summary>
        /// 颁发权证面积
        /// </summary>
        [DataColumn("BFQZMJ", AliasName = "颁发权证面积")]
        public decimal BFQZMJ { get; set; }

        /// <summary>
        /// 承包方总数
        /// </summary>
        [DataColumn("CBFZS", AliasName = "承包方总数")]
        public int CBFZS { get; set; }

        /// <summary>
        /// 承包农户数
        /// </summary>
        [DataColumn("CBNHS", AliasName = "承包农户数")]
        public int CBNHS { get; set; }

        /// <summary>
        /// 承包农户成员数
        /// </summary>
        [DataColumn("CBNHCYS", AliasName = "承包农户成员数")]
        public int CBNHCYS { get; set; }

        /// <summary>
        /// 其他方式承包合计
        /// </summary>
        [DataColumn("QTFSCBHJ", AliasName = "其他方式承包合计")]
        public int QTFSCBHJ { get; set; }

        /// <summary>
        /// 单位承包数量
        /// </summary>
        [DataColumn("DWCBZS", AliasName = "单位承包数量")]
        public int DWCBZS { get; set; }

        /// <summary>
        /// 个人承包数量
        /// </summary>
        [DataColumn("GRCBZS", AliasName = "个人承包数量")]
        public int GRCBZS { get; set; }

        #endregion


        #region Methods

        /// <summary>
        /// 累计
        /// </summary>
        public void Add(JCSJ_SJHZ  data)
        {
            if (data == null)
            {
                return;
            }
            FBFZS += data.FBFZS;
            CBDKZS += data.CBDKZS;
            CBDKZMJ += data.CBDKZMJ;
            FCBDKZS += data.FCBDKZS;
            FCBDZMJ += data.FCBDZMJ;
            BFQZZS += data.BFQZZS;
            NYYTZMJ += data.NYYTZMJ;
            ZZYZMJ += data.ZZYZMJ;
            LYZMJ += data.LYZMJ;
            XMYZMJ += data.XMYZMJ;
            YYZMJ += data.YYZMJ;
            FNYYTZMJ += FNYYTZMJ;
            NYYFNYYTZMJ += data.NYYFNYYTZMJ;
            JTTDSYQMJ += data.JTTDSYQMJ;
            CMXZSYMJ += data.CMXZSYMJ;
            CJJTJJZZMJ += data.CJJTJJZZMJ;
            XJJTJJZZMJ += data.XJJTJJZZMJ;
            QTJTJJZZMJ += data.QTJTJJZZMJ;
            GYTDSYQMJ += data.GYTDSYQMJ;
            SYQMJ += data.SYQMJ;
            ZLDMJ += data.ZLDMJ;
            JDDMJ += data.JDDMJ;
            KHDMJ += data.KHDMJ;
            QTJTTDMJ += data.QTJTTDMJ;
            FCBDMJHJ += data.FCBDMJHJ;
            JBNTMJ += data.JBNTMJ;
            FJBNTMJ += data.FJBNTMJ;
            JBNTMJHJ += data.JBNTMJHJ;
            JTCBQZS += data.JTCBQZS;
            QTCBQZS += data.QTCBQZS;
            BFQZSL += data.BFQZSL;
            BFQZMJ += data.BFQZMJ;
            CBFZS += data.CBFZS;
            CBNHS += data.CBNHS;
            CBNHCYS += data.CBNHCYS;
            QTFSCBHJ += data.QTFSCBHJ;
            DWCBZS += data.DWCBZS;
            GRCBZS += data.GRCBZS;
    }

        /// <summary>
        /// ClearValue
        /// </summary>
        public void ClearValue()
        {
            FBFZS = 0;
            CBDKZS = 0;
            CBDKZMJ = 0;
            FCBDKZS = 0;
            FCBDZMJ = 0;
            BFQZZS = 0;
            NYYTZMJ = 0;
            ZZYZMJ = 0;
            LYZMJ = 0;
            XMYZMJ = 0;
            YYZMJ = 0;
            FNYYTZMJ = 0;
            NYYFNYYTZMJ = 0;
            JTTDSYQMJ = 0;
            CMXZSYMJ = 0;
            CJJTJJZZMJ = 0;
            XJJTJJZZMJ = 0;
            QTJTJJZZMJ = 0;
            GYTDSYQMJ = 0;
            SYQMJ = 0;
            ZLDMJ = 0;
            JDDMJ = 0;
            KHDMJ = 0;
            QTJTTDMJ = 0;
            FCBDMJHJ = 0;
            JBNTMJ = 0;
            FJBNTMJ = 0;
            JBNTMJHJ = 0;
            JTCBQZS = 0;
            QTCBQZS = 0;
            BFQZSL = 0;
            BFQZMJ = 0;
            CBFZS = 0;
            CBNHS = 0;
            CBNHCYS = 0;
            QTFSCBHJ = 0;
            DWCBZS = 0;
            GRCBZS = 0;
        }

        #endregion
    }
}
