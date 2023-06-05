using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Agro.Module.Map
{
	/// <summary>
	/// TimeSliderControl.xaml 的交互逻辑
	/// </summary>
	public partial class TimeSliderControl : UserControl
	{
		private DateTime BeginDate;
		private readonly DateTime EndDate = DateTime.Now;
		public Action<DateTime> OnValueChanged;
		public TimeSliderControl()
		{
			InitializeComponent();
			TimeSlider.ValueChanged += (s, e) =>
			  {
				  var cur=BeginDate.AddDays(TimeSlider.Value);
				  label1.Content = cur.ToString("d");
				  OnValueChanged?.Invoke(cur);
			  };
		}
		public void Init(DateTime beginDate)
		{
			BeginDate = beginDate;
			var nDays=DateDiff(beginDate, EndDate);
			TimeSlider.Maximum = nDays;
			TimeSlider.Value = nDays;
		}
		public void SetValue(DateTime d)
		{
			var nDays = DateDiff(BeginDate,d);
			TimeSlider.Value = nDays;
		}
		private static int DateDiff(DateTime dateStart, DateTime dateEnd)
		{
			DateTime start = Convert.ToDateTime(dateStart.ToShortDateString());
			DateTime end = Convert.ToDateTime(dateEnd.ToShortDateString());

			TimeSpan sp = end.Subtract(start);

			return sp.Days;
		}
	}
}
