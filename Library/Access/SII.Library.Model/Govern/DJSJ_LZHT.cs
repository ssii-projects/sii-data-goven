// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 登记_流转合同
    /// </summary>
    [Serializable]
    [DataTable("DJ_LZHT", AliasName = "流转合同")]
    public class DJ_LZHT : ATT_LZHT
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 权利标识
        /// </summary>
        [DataColumn("QLID", AliasName = "权利标识")]
        public Guid QLID { get; set; }

        /// <summary>
        /// 所在地域
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        [DataColumn("CJYH", AliasName = "创建用户")]
        public string CJYH { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataColumn("CJSJ", AliasName = "创建时间")]
        public DateTime? CJSJ { get; set; }

        /// <summary>
        /// 最后修改用户
        /// </summary>
        [DataColumn("ZHXGYH", AliasName = "最后修改用户")]
        public string ZHXGYH { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [DataColumn("ZHXGSJ", AliasName = "最后修改时间")]
        public DateTime? ZHXGSJ { get; set; }

        #endregion

        #region Ctor

        public DJ_LZHT()
        {
            ID = Guid.NewGuid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 初始化内部数据
        /// </summary>
        public void Initalize()
        {
            ID = Guid.NewGuid();
            QLID = Guid.Empty;
            CJSJ = DateTime.Now;
            ZHXGSJ = DateTime.Now;
        }

        #endregion
    }
}
