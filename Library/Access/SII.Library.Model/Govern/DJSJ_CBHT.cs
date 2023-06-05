// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 登记_承包合同
    /// </summary>
    [Serializable]
    [DataTable("DJ_CBHT", AliasName = "登记承包合同")]
    public class DJ_CBHT : ATT_CBHTEXP
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 登记簿标识
        /// </summary>
        [DataColumn("QLID", AliasName = "登记簿标识")]
        public Guid QLID { get; set; }

        #endregion

        #region Ctor

        public DJ_CBHT()
        {
            ID = Guid.NewGuid();
            QLID = Guid.Empty;
        }

        #endregion
    }
}
