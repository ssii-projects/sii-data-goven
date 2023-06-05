using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.Entity
{
    /// <summary>
    /// 柱状图数据实体
    /// </summary>
    public class CharDataColum
    {
        /// <summary>
        /// 水平轴 类别
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 系列
        /// </summary>
        public string Series { get; set; }


        public double Value { get; set; }


    }
}
