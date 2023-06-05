/*
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.5
 * 文 件 名：   UIHelper
 * 创 建 人：   颜学铭
 * 创建时间：   2016/6/1 11:15:46
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace JzdxBuild
{
    public static class UIHelper
    {
        public static void ShowErrorMessage(Exception ex)
        {
            MessageBox.Show("错误" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
