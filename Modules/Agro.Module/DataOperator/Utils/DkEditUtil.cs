using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Agro.Module.DataOperator
{
	class DkEditUtil
	{
		/// <summary>
		/// 地块是否允许修改：
		///		1.登记中的地块不允许修改
		/// </summary>
		/// <param name="ft"></param>
		/// <param name="fShowWarningDialog">是否显示警告对话框</param>
		/// <returns></returns>
		public static bool CanModify(IFeature ft, bool fShowWarningDialog = true)
		{
			if (ft.Fields.FindField("DJZT") >= 0)
			{
				var nDjzt = SafeConvertAux.ToInt32(IRowUtil.GetRowValue(ft, "DJZT"));
				if (nDjzt == (int)EDjzt.Djz)
				{
					if (fShowWarningDialog)
					{
						MessageBox.Show("该地块登记状态为“登记中”，不允许修改！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
					}
					return false;
				}
			}
			return true;
		}
	}
}
