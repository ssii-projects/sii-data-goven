// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Library.Model
{
    /// <summary>
    /// 权属界线
    /// </summary>
    [Serializable]
    [DataTable("QYJX", AliasName = "权属界线")]
    public class QYJX_KZ : VEC_QYJX
    {
        #region Properties

        /// <summary>
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        #endregion

        #region Ctor

        public QYJX_KZ()
        {

        }

        #endregion
    }
}
