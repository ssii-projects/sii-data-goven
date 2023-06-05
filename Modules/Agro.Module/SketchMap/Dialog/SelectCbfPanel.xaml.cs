using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CbfItem = Agro.Module.SketchMap.TaskSketchMapArgumet.CbfItem;
namespace Agro.Module.SketchMap
{
	/// <summary>
	/// SelectCbfPanel.xaml 的交互逻辑
	/// </summary>
	public partial class SelectCbfPanel : UserControl,IDisposable
	{

		internal readonly ObservableCollection<CbfItem> _lstCbf = new();

		public SelectCbfPanel()
		{
			InitializeComponent();
			lstBox.ItemsSource = _lstCbf;

            zoneTree.ContainGroupNode = true;
            zoneTree.OnZoneSelected += RefreshList;
		}

        public void Dispose()
        {
            zoneTree.Dispose();
        }

        public string OnApply()
		{
			return null;
		}
        private void RefreshList(ShortZone zone)
        {
            _lstCbf.Clear();
            var cbfBm = zone.Code.PadRight(14, '0');
            //if ((int)zone.Level >= 3)
            //{
            //    cbfBm = zone.Code.PadRight(14, '0');
            //}
            var sql = $"select CBFBM,CBFMC from DJ_CBJYQ_DJB where SFYZX=0 and QSZT={(int)EQszt.Xians} and CBFBM like '{cbfBm}%' and CBFBM is not null and CBFMC is not null order by CBFMC";
            MyGlobal.Workspace.QueryCallback(sql, r =>
            {
                var it = new CbfItem()
                {
                    CBFBM = r.GetString(0),
                    CbfMc = r.GetString(1).Trim()
                };
                _lstCbf.Add(it);
                return true;
            });
        }
	}
}
