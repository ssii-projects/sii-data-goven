using GeoAPI.Geometries;
using System.Drawing;


/*
yxm created at 2019/1/16 16:41:59
*/
namespace Agro.GIS
{
	public interface IMapSurround
	{
		Map Map { get; set; }
		//string MapID { get; }
		//string Name { get; set; }
		////[propget, helpstring("property Icon")] HRESULT Icon([out, retval] OLE_HANDLE* pVal);
		//OkEnvelope QueryBounds(IDisplay pIDisplay, OkEnvelope oldBounds);
		//bool FitToBounds(IDisplay Display, OkEnvelope Bounds);
		//void DelayEvents(bool delay);
		//void Refresh();
		//void Draw(IDisplay Display, ICancelTracker trackCancel, OkEnvelope Bounds);
	}
	public interface INorthArrow : IMapSurround
	{
		Color Color { get; set; }
		double Size { get; set; }
		double Angle { get; }
		/// <summary>
		/// degrees
		/// </summary>
		double CalibrationAngle { get; set; }
		IPoint ReferenceLocation { get; set; }
	}
	public interface IMarkerNorthArrow:INorthArrow
	{
		IMarkerSymbol MarkerSymbol { get; set; }
	};
	public interface INumberFormat 
	{
		string ValueToString(double Value);
		double StringToValue(string str);
	}
	public interface INumericFormat
	{
		OkRoundingOptionEnum RoundingOption { get; set; }
		int RoundingValue { get; set; }
		OkNumericAlignmentEnum AlignmentOption { get; set; }
		int AlignmentWidth { get; set; }
		bool UseSeparator { get; set; }
		bool ZeroPad { get; set; }
		bool ShowPlusSign { get; set; }
	};
	public interface IScaleText : IMapSurround
	{
		string Text { get; }
		ITextSymbol Symbol { get; set; }
		OkScaleTextStyleEnum Style { get; set; }
		string Format { get; set; }
		OkUnits PageUnits { get; set; }
		OkUnits MapUnits { get; set; }
		string PageUnitLabel { get; set; }
		string MapUnitLabel { get; set; }
		INumberFormat NumberFormat { get; set; }
	};


	//public interface IMapSurroundFrame : IFrameElement
	//{
	//	IMapSurround MapSurround { get; set; }
	//	IMapFrame MapFrame { get; set; }
	//}

	//public class IMapSurroundImpl
	//{
	//	protected bool m_bDelayEvents;
	//	public IMap Map { get; set; }
	//	public string Name { get; set; }

	//	public void DelayEvents(bool delay)
	//	{
	//		m_bDelayEvents = delay;
	//	}
	//}
}
