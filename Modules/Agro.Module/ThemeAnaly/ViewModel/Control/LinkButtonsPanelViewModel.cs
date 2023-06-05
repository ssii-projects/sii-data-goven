namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class LinkButtonsPanelViewModel 
    {
        /// <summary>
        /// 实时数据对比分析表
        /// </summary>
        public RelayCommand RealTimeTableCommand { get; set; }

        /// <summary>
        /// 实时数据对比分析图
        /// </summary>
        public RelayCommand RealTimeImageCommand { get; set; }

        /// <summary>
        /// 承包地面积对比分析
        /// </summary>
        public RelayCommand AreaCommand { get; set; }

        /// <summary>
        /// 实测面积与合同面积差异对比分析
        /// </summary>
        public RelayCommand DiffrenceCommand { get; set; }

        /// <summary>
        /// 承包地用途结构分析
        /// </summary>
        public RelayCommand PurposeCommand { get; set; }


        /// <summary>
        /// 承包经营权取得方式结构分析
        /// </summary>
        public RelayCommand GetWayCommand { get;set; }


        /// <summary>
        /// 承包地地力等级结构分析
        /// </summary>
        public RelayCommand LandLevelCommand { get; set; }


        /// <summary>
        /// 农户人均承包地面积水平分析
        /// </summary>
        public RelayCommand PerCapitaAreaCommand { get; set; }


        /// <summary>
        /// 农户人口空间分布情况分析
        /// </summary>
        public RelayCommand PopulationCommand { get; set; }

    }
}
