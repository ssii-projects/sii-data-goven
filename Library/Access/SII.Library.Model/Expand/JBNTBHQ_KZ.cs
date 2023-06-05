// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 基本农田保护区
    /// </summary>
    [Serializable]
    [DataTable("JBNTBHQ", AliasName = "基本农田保护区")]
    public class JBNTBHQ_KZ : VEC_JBNTBHQ
    {
        #region Properties

        /// <summary>
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        #endregion

        #region Ctor

        public JBNTBHQ_KZ()
        {

        }

        #endregion
    }
}
