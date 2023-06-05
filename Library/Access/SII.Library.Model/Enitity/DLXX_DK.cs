﻿/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Agro.LibCore.Database;
using NetTopologySuite.Geometries;

namespace Agro.Library.Model
{
    /// <summary>
    /// 地块
    /// </summary>
    [Serializable]
    [DataTable("DLXX_DK", AliasName = "地块")]
    public class DLXX_DK : Entity<DLXX_DK>
	{

		/// <summary>
		/// 标识码(M)
		/// </summary>
		[DataColumn("BSM", AliasName = "标识码")]
        public virtual int BSM { get; set; }

		/// <summary>
		/// 要素代码(M)(eFeatureType)
		/// </summary>
		[DataColumn("YSDM", AliasName = "要素代码")]
		public virtual string YSDM { get; set; } = "211011";

		/// <summary>
		/// 地块代码(M)
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块代码")]
		public virtual string DKBM { get; set; }

		/// <summary>
		/// 地块名称(M)
		/// </summary>
		[DataColumn("DKMC", AliasName = "地块名称")]
		public virtual string DKMC { get; set; }

		/// <summary>
		/// 所有权性质(O)(eSYQXZ)
		/// </summary>
		[DataColumn("SYQXZ", AliasName = "所有权性质")]
		public virtual string SYQXZ { get; set; }

		/// <summary>
		/// 地块类别(M)(eDKLB)
		/// </summary>
		[DataColumn("DKLB", AliasName = "地块类别")]
		public virtual string DKLB { get; set; }

		/// <summary>
		/// 土地利用类型(O)
		/// </summary>
		[DataColumn("TDLYLX", AliasName = "土地利用类型")]
		public virtual string TDLYLX { get; set; }

		/// <summary>
		/// 地力等级(M)(eDLDJ)
		/// </summary>
		[DataColumn("DLDJ", AliasName = "地力等级")]
		public virtual string DLDJ { get; set; }

		/// <summary>
		/// 土地用途(M)(eTDYT)
		/// </summary>
		[DataColumn("TDYT", AliasName = "土地用途")]
		public virtual string TDYT { get; set; }

		/// <summary>
		/// 地块东至(O)
		/// </summary>
		[DataColumn("DKDZ", AliasName = "地块东至")]
		public virtual string DKDZ { get; set; }

		/// <summary>
		/// 地块西至(O)
		/// </summary>
		[DataColumn("DKXZ", AliasName = "地块西至")]
		public virtual string DKXZ { get; set; }

		/// <summary>
		/// 地块南至(O)
		/// </summary>
		[DataColumn("DKNZ", AliasName = "地块南至")]
		public virtual string DKNZ { get; set; }

		/// <summary>
		/// 地块北至(O)
		/// </summary>
		[DataColumn("DKBZ", AliasName = "地块北至")]
		public virtual string DKBZ { get; set; }

		/// <summary>
		/// 地块备注信息
		/// </summary>
		[DataColumn("DKBZXX", AliasName = "地块备注信息")]
		public virtual string DKBZXX { get; set; }

		/// <summary>
		/// 指界人姓名
		/// </summary>
		[DataColumn("ZJRXM", AliasName = "指界人姓名")]
		public virtual string ZJRXM { get; set; }

		/// <summary>
		/// 实测面积(M)
		/// </summary>
		[DataColumn("SCMJ", AliasName = "实测面积")]
        public virtual double SCMJ { get; set; }

        /// <summary>
        /// 实测面积亩
        /// </summary>
        [DataColumn("SCMJM", AliasName = "实测面积(亩)")]
        public virtual double? SCMJM { get; set; }

        /// <summary>
        /// 空间坐标
        /// </summary>
        [DataColumn("KJZB", AliasName = "空间坐标")]
        public virtual string KJZB { get; set; }

        /// <summary>
        /// 图形
        /// </summary>
        [DataColumn("Shape", AliasName = "几何图形")]
        public virtual Geometry Shape { get; set; }

		[DataColumn("SFJBNT", AliasName = "是否基本农田")]
		public virtual string SFJBNT { get; set; }

	};
}