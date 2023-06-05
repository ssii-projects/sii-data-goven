using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.Entity
{
    public class ChartData:NotifyObject
    {
        private string _category;
        private double _value;

        /// <summary>
        /// 水平轴 类别
        /// </summary>
        public string Category
        {
            get { return _category; }
            set { _category = value; OnPropertyChanged(nameof(Category));}
        }

        /// <summary>
        /// 值
        /// </summary>
        public double Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged(nameof(Value));}
        }
    }
}
