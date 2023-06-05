using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Agro.Module.ThemeAnaly.View.Control
{
    public class MyDataGrid :DataGrid
    {
        public static readonly DependencyProperty ColumsProperty = DependencyProperty.Register( "Colums", typeof(List<string>), typeof(MyDataGrid), new PropertyMetadata(default(List<string>), PropertiryCallBack));

        private static void PropertiryCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dpObject = d as DataGrid;
            if (dpObject == null) return;
            var columsProperty = e.NewValue as List<string>;
            if (columsProperty == null) return;
            //加入标题
            dpObject.Columns.Add(new DataGridTextColumn { Binding = new Binding { Path = new PropertyPath("Header"), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged } });
            foreach (var item in columsProperty)
            {
                dpObject.Columns.Add(CreateDataGridTextColumn(item));
            }
        }

        private static DataGridTextColumn CreateDataGridTextColumn(string header)
        {
            return new DataGridTextColumn { Header = header, Binding = new Binding { Path = new PropertyPath("Value" + header), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged } };
        }

        public List<string> Colums
        {
            get { return (List<string>)GetValue(ColumsProperty); }
            set { SetValue(ColumsProperty, value); }
        }
    }
}
