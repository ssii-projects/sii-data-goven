// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 面状地物
    /// </summary>
    [Serializable]
    [DataTable("MZDW", AliasName = "面状地物")]
    public class MZDW_KZ : VEC_MZDW
    {
        #region Properties

        /// <summary>
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        #endregion

        #region Ctor

        public MZDW_KZ()
        {

        }

        #endregion
    }
}
