using Agro.FrameWork;
using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common.Repository;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using System;

namespace Agro.Library.Common
{
  public enum AppType
	{
		/// <summary>
		/// 承包经营权项目
		/// </summary>
		DataGoven = 0,
		/// <summary>
		/// 数据操作工具
		/// </summary>
		DataOperator_ShapeFile,
		DataOperator_SQLite,
	}
	public class MyGlobal
	{
		static MyGlobal()
		{
			Persist = new Persist(PathHelper.PersistDirectory + "persist.xml");
		}
		//public static AppType AppType = AppType.DataGoven;
		public static Action OnDatasourceChanged;
		public static IMainWindow MainWindow { get; set; }
		public static void Connected(IFeatureWorkspace db,AppType appType, SEC_ID_USER loginUser = null)
		{
			LoginUser = loginUser;
			var fDatasouceChanged = Workspace != db;
			if (fDatasouceChanged)
			{

				var ows = Workspace;
				if (appType == AppType.DataGoven)
				{
					RepairDBUtil.Repair(db);
					if (loginUser != null)
					{
						LogUtil.WriteLoginLog(db, loginUser.Name);
					}
				}

				Workspace = db;
				CsSysInfoRepository.Instance.ChangeSource(db);
				AppOption.DkmjScale = CsSysInfoRepository.Instance.LoadInt(CsSysInfoRepository.KEY_DKMJSCALE, 2);

				OnDatasourceChanged?.Invoke();

				ows?.Dispose();
			}
		}
		public static IFeatureWorkspace Workspace
		{
			get;
			private set;
		}

		//#region 针对数据操作工具
		///// <summary>
		///// 作业员
		///// </summary>
		//public static string Operator { get; set; }
		//#endregion

		public static SEC_ID_USER LoginUser { get; private set; }
		public static Persist Persist { get; private set; }
		public static readonly AppOption AppOption = new AppOption();
		public static AppConfig AppConfig { get; set; } = new AppConfig();
		public static void ShutDown()
		{
			if (Workspace != null)
			{
				Workspace.Dispose();
			}
		}
	}

	public enum EExportFbfMode
	{
		SingleExport,
		BatchExport
	}



	/// <summary>
	/// 系统设置
	/// </summary>
	public class AppOption
	{
		/// <summary>
		/// 地块面积小数点位数（对应DLXX_DK.SCMJ字段）
		/// </summary>
		public int DkmjScale { get; set; } = 2;

		public EExportFbfMode ExportFbfMode
		{
			get
			{
				return MyGlobal.AppConfig.ExportFbfMode;
			}
			set
			{
				MyGlobal.AppConfig.ExportFbfMode = value;
			}
		}
	}
}
