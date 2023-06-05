using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
    /// <summary>
    /// BDC_JR_DK：不动产单元（承包地）
    /// </summary>
    [DataTable("BDC_JR_DK", AliasName = "BDC_JR_DK")]
    public class BDC_JR_DK : Entity<BDC_JR_DK>
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataColumn("ID", AliasName = "ID", Nullable = false, Length = 38)]
        public string ID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 地块编码
        /// </summary>
        [DataColumn("DKBM", AliasName = "地块编码", Nullable = false, Length = 19)]
        public string DKBM { get; set; }

        /// <summary>
        /// 不动产单元号
        /// </summary>
        [DataColumn("BDCDYH", AliasName = "不动产单元号", Length = 28)]
        public string BDCDYH { get; set; }
    }
}