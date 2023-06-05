// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Library.Model
{
    /// <summary>
    /// 点状地物
    /// </summary>
    [Serializable]
    [DataTable("DZDW", AliasName = "点状地物")]
    public class DZDW_KZ : VEC_DZDW
    {
        #region Properties

        /// <summary>
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        #endregion

        #region Ctor

        public DZDW_KZ()
        {

        }

        #endregion
    }
}
