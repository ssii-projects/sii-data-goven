﻿// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 登记_权证补发
    /// </summary>
    [Serializable]
    [DataTable("DJ_CBJYQZ_QZBF", AliasName = "登记权证补发")]
    public class DJ_QZBF : ATT_CBJYQZ_QZBF
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 权证标识
        /// </summary>
        [DataColumn("QZID", AliasName = "权证标识")]
        public Guid QZID { get; set; }
  
        #endregion

        #region Ctor

        public DJ_QZBF()
        {
            ID = Guid.NewGuid();
            QZID = Guid.Empty;
        }

        #endregion
    }
}
