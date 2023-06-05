// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 调查_承包方
    /// </summary>
    [Serializable]
    [DataTable("DC_CBF", AliasName = "承包方")]
    public class DC_CBF : ATT_CBF
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
        /// 状态
        /// </summary>
        [DataColumn("ZT", AliasName = "状态")]
        public bool ZT { get; set; }

        /// <summary>
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public Guid SZDY { get; set; }

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

        public DC_CBF()
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
            SZDY = Guid.Empty;
            ZT = true;
            CJSJ = DateTime.Now;
            ZHXGSJ = DateTime.Now;
        }

        #endregion
    }
}
