// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 调查_家庭成员
    /// </summary>
    [Serializable]
    [DataTable("DC_CBF_JTCY", AliasName = "家庭成员")]
    public class DC_JTCY : ATT_CBF_JTCYEXP
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 承包方标识
        /// </summary>
        [DataColumn("CBFID", AliasName = "承包方标识")]
        public Guid CBFID { get; set; }

        /// <summary>
        /// 是否共有人(O)(eWhether)
        /// </summary>
        [DataColumn("SFGYR", AliasName = "是否共有人")]
        public bool SFGYR { get; set; }

        #endregion

        #region Ctor

        public DC_JTCY()
        {
            ID = Guid.NewGuid();
        }

        #endregion
    }
}
