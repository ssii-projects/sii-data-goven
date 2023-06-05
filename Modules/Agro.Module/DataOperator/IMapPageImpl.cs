using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore.UI;
using Agro.Module.DataExchange.Repository;
using System.Collections.Generic;

namespace Agro.Module.DataOperator
{
	public interface IMapPageImpl
	{
		SidebarPage SidePage { get; }
		CreateFeatureToolButton BtnCreateFeatureTool { get; }
		ModifyFeatureToolButton ModifyFeatureTool{get;}
		CutFeatureToolButton BtnCutFeatureTool { get; }
		AutoCompletePolygonToolButton BtnAutoComplete { get; }
		ReshapePolygonEdgeToolButton BtnReshape { get; }
		MapPanToolButton BtnPan { get; }

		IdentifyToolButton BtnIdentify{get;}
		IMapControl MapControl { get; }
		List<ICodeItem> GetListFbfdm(IFeatureLayer fl, bool fOnlyInDcdk = false);
	}
	abstract class MapPageImplBase: IMapPageImpl
	{
		private readonly Dictionary<IFeatureLayer, List<ICodeItem>> _dicLyrFbfbms = new Dictionary<IFeatureLayer, List<ICodeItem>>();

		public abstract SidebarPage SidePage { get; }
		public abstract CreateFeatureToolButton BtnCreateFeatureTool { get; }
		public abstract ModifyFeatureToolButton ModifyFeatureTool { get; }
		public abstract CutFeatureToolButton BtnCutFeatureTool { get; }
		public abstract AutoCompletePolygonToolButton BtnAutoComplete { get; }
		public abstract ReshapePolygonEdgeToolButton BtnReshape { get; }
		public abstract IdentifyToolButton BtnIdentify { get; }
		public abstract MapPanToolButton BtnPan { get; }
		public abstract IMapControl MapControl { get; }
		protected void FinalConstruct()
		{
			new IdentifyHook(this);
			new CreateFeatureToolHook(this);
			new ModifyFeatureToolHook(this);

			new AutoCompleteToolHook(this);
			new CutFeatureToolHook(this);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fl"></param>
		/// <param name="fOnlyInDcdk">是否只包含调查地块中的发包方（仅对SQLite数据源有效）</param>
		/// <returns></returns>
		public List<ICodeItem> GetListFbfdm(IFeatureLayer fl, bool fOnlyInDcdk = false)
		{
			return OuterDcdkRepository.GetFbfbm(fl.FeatureClass, fOnlyInDcdk);
			//if (!_dicLyrFbfbms.TryGetValue(fl, out var lstFbfbms))
			//{
			//	lstFbfbms = OuterDcdkRepository.GetFbfbm(fl.FeatureClass,fOnlyInDcdk);
			//	_dicLyrFbfbms[fl] = lstFbfbms;
			//}
			//return lstFbfbms;
		}
	}

	enum ELayerTagType
	{
		/// <summary>
		/// 导出地块
		/// </summary>
		EXPORT_DK,
		/// <summary>
		/// 调查地块（可编辑）
		/// </summary>
		DC_DK,
		/// <summary>
		/// 未登记地块
		/// </summary>
		WDJ_DK,
		/// <summary>
		/// 变更标记
		/// </summary>
		BGBJ,
	}
}
