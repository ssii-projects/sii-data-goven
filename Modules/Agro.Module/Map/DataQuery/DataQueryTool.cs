using GeoAPI.Geometries;
using Agro.GIS;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Agro.Module.Map
{

	class PointQueryTool : CreateGeometryTool
	{
		private readonly DataQueryControlPanel _p;
		public PointQueryTool(DataQueryControlPanel p) : base(true)
		{
			_p = p;
			base.SetGeometryType(eGeometryType.eGeometryPoint);
			base.OnFinish += geo =>
			{
				if (geo != null)
				{
					var g = new WPFShape(Map);// base._activeGraphic;
					if (_p.BufferSize > 0)
					{
						base.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 100, 0));
						geo = geo.Buffer(_p.BufferSize);
						g.Fill = base.Fill;
					}
					//base._activeGraphic = null;
					g.AddToMap(Map);
					g.ResetGeometry(geo);
					base._hook.TempElements.AddElement(g);
					_p.PopResult(geo);
				}
			};
		}

		public override void OnSetCurrent(bool fCurrentTool)
		{
			Map.Cursor = fCurrentTool ? CustomCursors.instance.CreateFeatureCursor : CustomCursors.instance.DefaultCursor;
			base.Map.SnapEnviroment.EnableAutoUpdateSnaps = fCurrentTool;
		}
	}

	class LineQueryTool : CreateGeometryTool
	{
		private readonly DataQueryControlPanel _p;
		public LineQueryTool(DataQueryControlPanel p) : base(true)
		{
			_p = p;
			base.SetGeometryType(eGeometryType.eGeometryPolyline);
			base.OnFinish += geo =>
			{
				if (geo != null)
				{
					var g = new WPFShape(Map);// base._activeGraphic;
					if (_p.BufferSize > 0)
					{
						base.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 100, 0));
						geo = geo.Buffer(_p.BufferSize);
						g.Fill = base.Fill;
					}
					//base._activeGraphic = null;
					g.AddToMap(Map);
					g.ResetGeometry(geo);
					base._hook.TempElements.AddElement(g);
					_p.PopResult(geo);
				}
			};
		}

		public override void OnSetCurrent(bool fCurrentTool)
		{
			Map.Cursor = fCurrentTool ? CustomCursors.instance.CreateFeatureCursor : CustomCursors.instance.DefaultCursor;
			base.Map.SnapEnviroment.EnableAutoUpdateSnaps = fCurrentTool;
			//if (!fCurrentTool)
			//{
			//    Clear();
			//}
		}
	}

	class AreaQueryTool : CreateGeometryTool
	{
		public AreaQueryTool(DataQueryControlPanel p) : base(true)
		{
			base.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 100, 0));
			base.SetGeometryType(eGeometryType.eGeometryPolygon);
			base.OnFinish += geo =>
			{
				if (geo != null)
				{
					var g = new WPFShape(Map);// base._activeGraphic;
					//base._activeGraphic = null;
					g.AddToMap(Map);
					g.ResetGeometry(geo);
					//_graphics.Add(g);
					_hook.TempElements.AddElement(g);
					p.PopResult(geo);
					//g.SetLabelExpression("\"" + ToLableExpr(geo) + "\"");
				}
			};
			//base.OnActiveGraphicCreated += g =>
			//{
			//	g.OnLabelShapeCreated += ls => { ls.PointOnSurface = false; };
			//	base._activeGraphic.OnGeometryChanged += (geo) =>
			//	{
			//		string labelStr = null;
			//		if (geo != null && geo.Area > 0)
			//		{
			//			labelStr = "\"" + ToLableExpr(geo) + "\"";
			//		}
			//		g.SetLabelExpression(labelStr);
			//	};
			//};
		}

		public override void OnSetCurrent(bool fCurrentTool)
		{
			Map.Cursor = fCurrentTool ? CustomCursors.instance.CreateFeatureCursor : CustomCursors.instance.DefaultCursor;
			base.OnSetCurrent(fCurrentTool);
		}
		//private string ToLableExpr(IGeometry geo)
		//{
		//	var sr = geo.GetSpatialReference();
		//	if (sr != null && sr.IsGEOGCS() && Map.ProjectedCRS != null)
		//	{
		//		geo.Project(Map.ProjectedCRS);
		//	}
		//	double dArea = Math.Round(geo.Area, 2);
		//	var msg = /*"面积：" +*/dArea.ToString();
		//	sr = geo.GetSpatialReference();
		//	if (sr != null && sr.IsPROJCS())
		//	{
		//		msg += "平方米\"\\";
		//		msg += "\"" + Math.Round(dArea * 0.0015, 2) + "亩";
		//	}
		//	return msg;
		//}
	}

	/// <summary>
	/// 框选查询
	/// </summary>
	class BoxQueryTool : SelectFeatureTool
	{
		protected WPFShape _activeGraphic;
		private readonly DataQueryControlPanel _p;
		public BoxQueryTool(DataQueryControlPanel p)
		{
			_p = p;
		}
		protected override void Select(IGeometry geo)
		{
			if (geo != null)
			{
				geo = Map.Transformation.ToMap(geo);
				if (_activeGraphic == null)
				{
					_activeGraphic = new WPFShape(base.Map)
					{
						Fill = new SolidColorBrush(Color.FromArgb(100, 255, 100, 0))
					};
				}
				//geo.SetSpatialReference(Map.SpatialReference);
				_activeGraphic.AddToMap(Map);
				_activeGraphic.ResetGeometry(geo);
				_hook.TempElements.AddElement(_activeGraphic);
				_activeGraphic = null;
				_p.PopResult(geo);
			}
		}
		public override void OnSetCurrent(bool fCurrentTool)
		{
			Map.Cursor = fCurrentTool ? CustomCursors.instance.CreateFeatureCursor : CustomCursors.instance.DefaultCursor;
			base.OnSetCurrent(fCurrentTool);
		}
	}
}
