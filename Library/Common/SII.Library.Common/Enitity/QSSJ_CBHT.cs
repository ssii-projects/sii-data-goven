/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;


namespace Agro.Library.Model
{
	/// <summary>
	/// 承包合同
	/// </summary>
	[Serializable]
    [DataTable("QSSJ_CBHT", AliasName = "承包合同")]
    public class QSSJ_CBHT : EntityCBHTBase<QSSJ_CBHT>
    {
		public EQszt ZT { get; set; }=EQszt.Xians;

		[DataColumn(AliasName = "登记状态")]
		public EDjzt DJZT { get; set; } = EDjzt.Ydj;

		[DataColumn(AliasName ="最后修改时间")]
		public DateTime? ZHXGSJ { get; set; }

		[DataColumn(AliasName = "创建时间")]
		public DateTime CJSJ { get; set; } = DateTime.Now;
	}
}
