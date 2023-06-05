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
    /// 线状地物
    /// </summary>
    [Serializable]
    [DataTable("XZDW", AliasName = "线状地物")]
    public class XZDW : VEC_XZDW
    {
        #region Properties

        /// <summary>
        /// 标识码(M)
        /// </summary>
        [DataColumn("BSM", AliasName = "标识码", Auto = true)]
        public override int BSM { get; set; }

        /// <summary>
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        /// <summary>
        /// 几何图形
        /// </summary>
        [DataColumn("SHAPE", AliasName = "几何图形")]
        public override Geometry Shape { get; set; }

        #endregion

        #region Ctor

        public XZDW()
        {

        }

        #endregion
    }
}
