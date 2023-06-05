// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 调查_承包合同
    /// </summary>
    [Serializable]
    [DataTable("DC_CBHT", AliasName = "承包合同")]
    public class DC_CBHT : ATT_CBHT
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 发包方标识
        /// </summary>
        [DataColumn("FBFID", AliasName = "发包方标识")]
        public Guid FBFID { get; set; }

        /// <summary>
        /// 承包方标识
        /// </summary>
        [DataColumn("CBFID", AliasName = "承包方标识")]
        public Guid CBFID { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [DataColumn("ZT", AliasName = "状态")]
        public bool ZT { get; set; }

        /// <summary>
        /// 登记状态
        /// </summary>
        [DataColumn("DJZT", AliasName = "登记状态")]
        public bool DJZT { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataColumn("CJSJ", AliasName = "创建时间")]
        public DateTime CJSJ { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [DataColumn("ZHXGSJ", AliasName = "最后修改时间")]
        public DateTime? ZHXGSJ { get; set; }

        #endregion

        #region Ctor

        public DC_CBHT()
        {
            Initalize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 初始化内部数据
        /// </summary>
        public void Initalize()
        {
            ID = Guid.NewGuid();
            FBFID = Guid.Empty;
            CBFID = Guid.Empty;
            ZT = true;
            DJZT = true;
            CJSJ = DateTime.Now;
            ZHXGSJ = DateTime.Now;
        }

        #endregion
    }
}
