using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Visifire.Charts;
using Agro.Module.ThemeAnaly.Entity;

namespace Agro.Module.ThemeAnaly.View.Control
{
    /// <summary>
    ///     ChartColum.xaml 的交互逻辑
    /// </summary>
    public  class ChartColum :Chart
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ChartColumWithTitle),
                new PropertyMetadata(null, TitlePropCallBack));

        private static void TitlePropCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dpObject = d as ChartColum;
            dpObject?.Titles.Add(new Title { Text = e.NewValue.ToString() });
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(ObservableCollection<string>), typeof(ChartColum),
                new PropertyMetadata(null, ChartDataPropCallBack));

        private static void ChartDataPropCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dpObject = d as ChartColum;
            if (dpObject != null)
                BindDataMethod(dpObject.Rows, dpObject.RowItems, dpObject, dpObject.ChartData);
        }

        public static readonly DependencyProperty RowItemsProperty =
            DependencyProperty.Register("RowItems", typeof(ObservableCollection<string>), typeof(ChartColum),
                new PropertyMetadata(null, ChartDataPropCallBack));


        public static readonly DependencyProperty ChartDataProperty = DependencyProperty.RegisterAttached(
            "ChartData", typeof(List<CharDataColum>), typeof(ChartColum),
            new PropertyMetadata(default(List<CharDataColum>), ChartDataPropCallBack));


        //public static readonly RoutedEvent ExportEvent = EventManager.RegisterRoutedEvent("Export",
        //    RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChartColum));

        //public static readonly RoutedEvent PrintEvent = EventManager.RegisterRoutedEvent("Print", RoutingStrategy.Bubble,
        //    typeof(RoutedEventHandler), typeof(ChartColum));

        //public event RoutedEventHandler Export
        //{
        //    add
        //    {
        //        AddHandler(ExportEvent, value);
        //    }
        //    remove
        //    {
        //        RemoveHandler(ExportEvent, value);
        //    }
        //}

        //public event RoutedEventHandler Print
        //{
        //    add
        //    {
        //        AddHandler(PrintEvent, value);
        //    }
        //    remove
        //    {
        //        RemoveHandler(PrintEvent, value);
        //    }
        //}


        public List<CharDataColum> ChartData
        {
            get { return (List<CharDataColum>)GetValue(ChartDataProperty); }
            set { SetValue(ChartDataProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public ObservableCollection<string> Rows
        {
            get { return (ObservableCollection<string>)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public ObservableCollection<string> RowItems
        {
            get { return (ObservableCollection<string>)GetValue(RowItemsProperty); }
            set { SetValue(RowItemsProperty, value); }
        }


        private static void BindDataMethod(ObservableCollection<string> rows, ObservableCollection<string> rowItems,
            Chart chart, List<CharDataColum> datas)
        {
            if (rows == null || rowItems == null || datas == null) return;
            rowItems.ToList().ForEach(i =>
            {
                var dataSeries = new DataSeries
                {
                    LabelEnabled = true,
                    LegendText = i
                };
                rows.ToList().ForEach(r =>
                {
                    var dataByRowAndRowItem = datas.FindAll(d => d.Category == r && d.Series == i);
                    foreach (var dataBy in dataByRowAndRowItem)
                    {
                        var datapoint = new DataPoint
                        {
                            AxisXLabel = dataBy.Category,
                            YValue = dataBy.Value
                        };
                        dataSeries.DataPoints.Add(datapoint);
                    }
                });
                chart.Series.Add(dataSeries);
            });
            chart.Legends.Add(new Legend
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
    }
}
