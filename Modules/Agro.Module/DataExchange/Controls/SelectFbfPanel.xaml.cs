using Agro.GIS;
using Agro.Library.Common;
using Agro.Library.Model;
using System.Collections.ObjectModel;
using System.Windows.Controls;

/*
yxm created at 2019/4/11 10:09:49
*/
namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 选择发包方界面
	/// </summary>
	public partial class SelectFbfPanel : UserControl
	{
		public class FbfItem
		{
			public string FbfBM { get; set; }
			public string FbfMC { get; set; }
		}
		public readonly ObservableCollection<FbfItem> _lstFbf = new ObservableCollection<FbfItem>();
		private ShortZone _curZone;
		private readonly IFeatureWorkspace _db;
		public bool ContainTownNode = false;
		public SelectFbfPanel(IFeatureWorkspace db=null)
		{
			InitializeComponent();
			if (db?.DatabaseType == LibCore.eDatabaseType.ShapeFile)
			{
				db = MyGlobal.Workspace;
			}
			_db = db ?? MyGlobal.Workspace;
			if (db != null)
			{
				zoneTree.FinalConstruct(db);
			}
			listView1.ItemsSource = _lstFbf;
			zoneTree.OnZoneSelected += RefreshList;
		}
		public FbfItem SelectedFbf { get; private set; }

		public string OnApply()
		{

			if (listView1.SelectedItem is FbfItem it)
			{
				SelectedFbf = it;//.FbfBM;
				return null;
			}
			var fValidNode = _curZone.Level == Library.Model.eZoneLevel.Village;
			if (ContainTownNode)
			{
				fValidNode |= _curZone.Level == Library.Model.eZoneLevel.Town;
			}
			if (fValidNode)
			{
				SelectedFbf = new FbfItem()
				{
					FbfBM=_curZone.Code,
					FbfMC=_curZone.Name
				};
				return null;
			}
			if (_lstFbf.Count == 0)
			{
				return "当前地域下无发包方";
			}
			return "请先选择发包方";
		}
		private void RefreshList(ShortZone zone)
		{
			_curZone = zone;
			_lstFbf.Clear();
			if ((int)zone.Level >= (int)eZoneLevel.County)
			{
				return;
			}
			var tbName = QSSJ_FBF.GetTableName();
			if (_db.DatabaseType == LibCore.eDatabaseType.SQLite)
			{
				tbName = DC_QSSJ_FBF.GetTableName();
			}
			var sql = $"select FBFBM,FBFMC from {tbName} where FBFBM like '{zone.Code}%'";
			_db.QueryCallback(sql, r =>
			 {
				 var it = new FbfItem()
				 {
					 FbfBM=r.GetString(0),
					 FbfMC=r.IsDBNull(1)?"":r.GetString(1).Trim()
				 };
				 _lstFbf.Add(it);
				 return true;
			 });
		}
	}
}
