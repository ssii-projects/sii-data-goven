using System;
using System.Windows;


/*
yxm created at 2019/6/11 14:26:29
*/
namespace Agro.LibCore
{
	public class Try
	{
		public static void Catch(Action action,bool fShowMessage=true)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (fShowMessage)
				{
					//MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
	}
}
