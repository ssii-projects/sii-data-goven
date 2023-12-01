using Agro.Library.Model;
using DataOperatorTool.OData;
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

namespace DataOperatorTool
{
    /// <summary>
    /// SelectFbfPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SelectFbfPanel : UserControl
    {
        public class FbfItem
        {
            public string FbfBM { get; set; }
            public string FbfMC { get; set; }
        }
        public class ZoneItem
        {
            public readonly DLXX_XZDY Entity; 
            public ZoneItem(DLXX_XZDY en)
            {
                Entity = en;
            }
            public override string ToString()
            {
                return Entity.MC;
            }
        }
        public FbfItem? SelectedFbf { get; private set; }
        private readonly DownLoadService service=DownLoadService.Instance;
        public SelectFbfPanel()
        {
            InitializeComponent();
            cbCounty.ItemsSource=new List<ZoneItem>();
            cbTown.ItemsSource = new List<ZoneItem>();
            cbVillage.ItemsSource = new List<ZoneItem>();
            cbGroup.ItemsSource = new List<ZoneItem>();

            cbCounty.SelectionChanged +=async (s, e) =>
            {
                if(cbCounty.SelectedItem is ZoneItem xIt)
                {
                   var m=await service.GetSubZones(xIt.Entity.ID);
                   FillComboSource(cbTown, m);
                    if (m.Value.Length == 1)
                    {
                        cbTown.SelectedIndex = 0;
                    }
                }
            };
            cbTown.SelectionChanged += async (s, e) =>
            {
                if (cbTown.SelectedItem is ZoneItem xIt)
                {
                    var m = await service.GetSubZones(xIt.Entity.ID);
                    FillComboSource(cbVillage, m);
                    if(m.Value.Length==1)
                    {
                        cbVillage.SelectedIndex = 0;
                    }
                }
            };
            cbVillage.SelectionChanged += async (s, e) =>
            {
                if (cbVillage.SelectedItem is ZoneItem xIt)
                {
                    var m = await service.GetSubZones(xIt.Entity.ID);
                    FillComboSource(cbGroup, m);
                }
            };

            Loaded +=async (s, e) =>
            {
               var m=await service.GetCounties();
                FillComboSource(cbCounty, m,true);
            };
            
        }
        public string? OnApply()
        {
            if (cbCounty.SelectedIndex < 0)
            {
                return "还未选择县";
            }
            if (cbTown.SelectedIndex < 0)
            {
                return "还未选择乡";
            }
            if (cbVillage.SelectedIndex < 0)
            {
                return "还未选择村";
            }
            var townIt = (ZoneItem)cbTown.SelectedItem;
            SelectedFbf = new FbfItem();
            var mc = ((ZoneItem)cbCounty.SelectedItem).Entity.MC
                +townIt.Entity.MC
                + ((ZoneItem)cbVillage.SelectedItem).Entity.MC;
            if(cbGroup.SelectedItem is ZoneItem it)
            {
                mc += it.ToString();
                SelectedFbf.FbfBM = it.Entity.BM;
            }
            else
            {
                SelectedFbf.FbfBM=townIt.Entity.BM;
            }
            SelectedFbf.FbfMC = mc;
            return null;
        }
        private void FillComboSource(ComboBox cb,ZonesModel m,bool select=false)
        {
            var lst = (List<ZoneItem>)cb.ItemsSource;
            lst.Clear();
            foreach (var it in m.Value)
            {
                lst.Add(new ZoneItem(it));
            }
            if(select&&lst.Count> 0)
            {
                cb.SelectedIndex= 0;
            }
        }
    }
}
