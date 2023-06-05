using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Agro.Module.ThemeAnaly.Entity;

namespace Agro.Module.ThemeAnaly.Model
{
    public class Model 
    {

        /// <summary>
        /// 获取柱状图数据
        /// </summary>
        /// <returns></returns>
        public static List<CharDataColum> GetCharDataColums(ObservableCollection<string> categories, ObservableCollection<string> serieses )
        {
            var random = new Random();
            return (from c in categories from s in serieses select new CharDataColum {Category = c, Series = s, Value = random.Next(1, 30000)}).ToList();
        }


        public static List<ChartDataPie> GetCharDataPies(ObservableCollection<string> categories)
        {
            var random = new Random();
            return (from c in categories select new ChartDataPie { Category = c, Value = random.Next(0, 30000) }).ToList();
        }
    }
}
