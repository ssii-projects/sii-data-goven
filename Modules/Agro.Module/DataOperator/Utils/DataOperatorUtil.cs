using Agro.GIS;
using Agro.Library.Model;

namespace Agro.Module.DataOperator
{
	/// <summary>
	/// 数据操作模块辅助类
	/// </summary>
	public static class DataOperatorUtil
	{
		/// <summary>
		/// 作业员
		/// </summary>
		public static string Operator { get; set; } = "";
        /// <summary>
        /// 是否包含作业员字段
        /// </summary>
        /// <param name="ft"></param>
        /// <returns></returns>
        public static bool HasOperatorField(IFeature ft)
		{
			var zyyFieldName = VEC_SURVEY_DK.GetFieldName(nameof(VEC_SURVEY_DK.ZYY));
			var iZyyField = ft.Fields.FindField(zyyFieldName);
			return iZyyField >= 0;
		}

		public static bool IsOperatorNullOrEmpty(IFeature ft)
		{
			return string.IsNullOrEmpty(GetOperator(ft));
		}

		/// <summary>
		/// 获取作业员
		/// </summary>
		/// <param name="ft"></param>
		/// <returns></returns>
		public static string GetOperator(IFeature ft)
		{
			string oper= null;
			var zyyFieldName = VEC_SURVEY_DK.GetFieldName(nameof(VEC_SURVEY_DK.ZYY));
			var iZyyField = ft.Fields.FindField(zyyFieldName);
			if (iZyyField >= 0)
			{
				var o = ft.GetValue(iZyyField);
				if (o != null)
				{
					oper = o.ToString().Trim();
				}
			}
			return oper;
		}
		public static void SetOperator(IFeature ft,string oper)
		{
			var zyyFieldName = VEC_SURVEY_DK.GetFieldName(nameof(VEC_SURVEY_DK.ZYY));
			IRowUtil.SetRowValue(ft, zyyFieldName, oper);
		}

		

	    /// <summary>
		/// 设置作业员默认值（登录用户名）
		/// </summary>
		/// <param name="ft"></param>
		public static void SetDefaultOperator(IFeature ft)
		{
			SetOperator(ft, Operator);
		}

		public static void SetDefaultOperatorIfNullOrEmpty(IFeature ft)
		{
			if (string.IsNullOrEmpty(GetOperator(ft)))
			{
				SetDefaultOperator(ft);
			}
		}

		//public 
	}
}
