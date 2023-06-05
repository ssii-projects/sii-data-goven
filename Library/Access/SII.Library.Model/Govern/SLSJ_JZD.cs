// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;
using NetTopologySuite.Geometries;

namespace Agro.Library.Model
{
    /// <summary>
    /// 界址点
    /// </summary>
    [Serializable]
    [DataTable("DK_JZD", AliasName = "界址点")]
    public class JZD : VEC_JZD
    {
        #region Properties

        /// <summary>
        /// 标识码(M)
        /// </summary>
        [DataColumn("BSM", AliasName = "标识码", Auto = true)]
        public override int BSM { get; set; }

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 地块标识
        /// </summary>
        [DataColumn("DKID", AliasName = "地块标识")]
        public Guid DKID { get; set; }

        /// <summary>
        /// 几何图形
        /// </summary>
        [DataColumn("SHAPE", AliasName = "几何图形")]
        public override Geometry Shape { get; set; }

        #endregion

        #region Ctor

        public JZD()
        {
            ID = Guid.NewGuid();
            DKID = Guid.Empty;
        }

        #endregion
    }
}
