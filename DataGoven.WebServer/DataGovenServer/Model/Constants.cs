﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Library.Model
{
	/// <summary>
	/// 变更类型
	/// </summary>
	public enum EBGLX
	{
		/// <summary>
		/// 无变更
		/// </summary>
		None = -1,
		/// <summary>
		/// 图形变更（一般界址变更）
		/// </summary>
		Txbg = 0,
		/// <summary>
		/// 分割
		/// </summary>
		Fenge = 1,

		/// <summary>
		/// 合并
		/// </summary>
		Hebing = 2,

		/// <summary>
		/// 灭失
		/// </summary>
		Mieshi = 3,

		/// <summary>
		/// 新增
		/// </summary>
		Xinz = 9,

		/// <summary>
		/// 属性变更
		/// </summary>
		Sxbg = 10,
	}
	/// <summary>
	/// DLXX_DK 状态(ZT)字段
	/// </summary>
	public enum EDKZT
	{
		/// <summary>
		/// 临时
		/// </summary>
		Lins = 0,
		/// <summary>
		/// 有效
		/// </summary>
		Youxiao = 1,
		/// <summary>
		/// 历史
		/// </summary>
		Lishi = 2,
	}
	/// <summary>
	/// 数据来源，
	/// DLXX_DK SJLY字段
	/// </summary>
	public enum ESjly
	{
		/// <summary>
		/// 初始
		/// </summary>
		Cs = 0,
		/// <summary>
		/// 新增
		/// </summary>
		Xinz = 1,
		/// <summary>
		/// 修改
		/// </summary>
		Xiugai = 2,
		/// <summary>
		/// 拆分
		/// </summary>
		Chaifen = 3,
		/// <summary>
		/// 合并
		/// </summary>
		Hebing = 4,
	}

	/// <summary>
	/// 登记状态
	/// </summary>
	public enum EDjzt
	{

		/// <summary>
		/// 未登记
		/// </summary>
		Wdj = 0,
		/// <summary>
		/// 登记中
		/// </summary>
		Djz = 1,
		/// <summary>
		/// 已登记
		/// </summary>
		Ydj = 2,
	}

	/// <summary>
	/// 权属状态
	/// </summary>
	public enum EQszt
	{
		/// <summary>
		/// 临时
		/// </summary>
		Lins = 0,
		/// <summary>
		/// 现势
		/// </summary>
		Xians = 1,
		/// <summary>
		/// 历史
		/// </summary>
		History = 2,
		/// <summary>
		/// 终止
		/// </summary>
		End = 3
	}

	/// <summary>
	/// 登记小类
	/// </summary>
	public enum EDjxl
	{
		/// <summary>
		/// 首次登记
		/// </summary>
		SCDJ = 0,
		/// <summary>
		/// 分户登记
		/// </summary>
		FHDJ = 1,
		/// <summary>
		/// 合户登记
		/// </summary>
		HEHDJ = 2,
		/// <summary>
		/// 转让登记
		/// </summary>
		ZRDJ = 3,
		/// <summary>
		/// 互换登记
		/// </summary>
		HUHDJ = 4,
		/// <summary>
		/// 一般登记 / 其它变更登记
		/// </summary>
		YBBGDJ = 5,
		/// <summary>
		/// 注销登记
		/// </summary>
		ZXDJ = 6,
		/// <summary>
		/// 权证补发
		/// </summary>
		QZBF = 7,
		/// <summary>
		/// 权证换发
		/// </summary>
		QZHF = 8
	}

	/// <summary>
	/// 承包经营权取得方式
	/// </summary>
	public enum ECbjyqQDFS
	{
		/// <summary>
		/// 承包
		/// </summary>
		CB = 100,
		/// <summary>
		/// 家庭承包
		/// </summary>
		JTCB = 110,
		/// <summary>
		/// 其它方式承包
		/// </summary>
		QTFSCB = 120,
		/// <summary>
		/// 招标
		/// </summary>
		ZB = 121,
		/// <summary>
		/// 拍卖
		/// </summary>
		PM = 122,
		/// <summary>
		/// 公开协商
		/// </summary>
		GKXS = 123,
		/// <summary>
		/// 其它招标方式
		/// </summary>
		QTZBFS = 129,
		/// <summary>
		/// 转让
		/// </summary>
		Zr = 200,
		/// <summary>
		/// 互换
		/// </summary>
		Huh = 300,
		/// <summary>
		/// 其它方式
		/// </summary>
		QTFS = 900,
	}
	/// <summary>
	/// 是否代码表
	/// </summary>
	public enum EYesNo
	{
		/// <summary>
		/// 是
		/// </summary>
		Yes = 1,
		/// <summary>
		/// 否
		/// </summary>
		No = 2,
	}
	/// <summary>
	/// DJ_CBJYQ_QZ.SFYZX
	/// 权证是否已注销
	/// </summary>
	public enum ESfyzx
	{
		/// <summary>
		/// 已注销
		/// </summary>
		YZX = 1,
		/// <summary>
		/// 未注销
		/// </summary>
		WZX = 0,
	}

	/// <summary>
	/// 权利类型 DJ_CBJYQ_DJB.QLLX 
	/// 参考：SELECT * FROM XTPZ_DJ_QLLX
	/// </summary>
	public enum EQllx
	{
		/// <summary>
		/// 承包经营权
		/// </summary>
		Cbjyq = 9
	}

	public static class CodeType
	{
		/// <summary>
		/// 承包方式
		/// </summary>
		public static readonly string CBFS = "承包经营权取得方式";
		public static readonly string SYQXZ = "所有权性质";
	}
}
