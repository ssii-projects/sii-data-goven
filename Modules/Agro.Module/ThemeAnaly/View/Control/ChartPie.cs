using Agro.Module.ThemeAnaly.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Visifire.Charts;

namespace Agro.Module.ThemeAnaly.View.Control
{
    //public class ChartValue
    //{
    //    public string Header;
    //    public double Value;
    //    public ChartValue(string header,double v =20)
    //    {
    //        Header = header;
    //        Value = v;
    //    }
    //}
    /// <summary>
    /// ChartPie.xaml 的交互逻辑
    /// </summary>
    public  class ChartPie:Chart
    {
       

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(ChartPie), new PropertyMetadata(default(string), TitlePropertyCallback));


        public static readonly DependencyProperty RowsProperty =
           DependencyProperty.Register("Rows", typeof(List<CharDataColum>), typeof(ChartPie),
               new PropertyMetadata(null, ChartDataPropCallBack));


        public List<CharDataColum> Rows
        {
            get { return (List<CharDataColum>)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }



        //#region yxm 2017-6-22
        //public static readonly DependencyProperty datasProperty =
        //   DependencyProperty.Register("datas", typeof(List<ChartDataPie>), typeof(ChartPie),
        //       new PropertyMetadata(null, ChartDataPropCallBack));

        //public List<ChartDataPie> datas { get
        //    {
        //        return (List<ChartDataPie>)GetValue(datasProperty);
        //    } set
        //    {
        //        SetValue(datasProperty,value);
        //    }
        //}
        //public ChartPie()
        //{

        //}
        //#endregion
        private static void ChartDataPropCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dpObject = d as ChartPie;
            if (dpObject != null)
            {
                BindDataMethod(dpObject.Rows, dpObject, dpObject.Title);
            }
        }


        private static void TitlePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chatPie = d as ChartPie;
            chatPie?.Titles.Add(new Title { Text = e.NewValue.ToString() });
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private static void BindDataMethod(List<CharDataColum> rows, Chart chart,string category)// List<ChartDataPie> datas)
        {
            if (!(rows?.Count > 0)) return;
            //var datas = Model.Model.GetCharDataPies(rows);
            var dataSeries = new DataSeries
            {
                LabelEnabled = true,
                RenderAs = RenderAs.Pie,
            };
            rows.ToList().ForEach(r =>
            {
                //var dataByRowAndRowItem = datas?.FindAll(d => d.Category == r.Header);
                //if (dataByRowAndRowItem != null)
                //    foreach (var dataBy in dataByRowAndRowItem)
                //    {
                if (r.Category == category)
                {
                    var datapoint = new DataPoint
                    {
                        AxisXLabel = r.Series,//Header,// dataBy.Category,
                        YValue = r.Value// dataBy.Value
                    };
                    dataSeries.DataPoints.Add(datapoint);
                    //}
                }
            });
            chart.Series.Add(dataSeries);
            chart.Legends.Add(new Legend
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

    }
}
