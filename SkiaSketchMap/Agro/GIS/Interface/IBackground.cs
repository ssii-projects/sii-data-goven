using Agro.LibCore;
using GeoAPI.Geometries;


/*
yxm created at 2019/1/8 15:27:44
*/
namespace Agro.GIS
{
	public interface IBackground
	{
		void Draw(IDisplay dc,IDisplayTransformation trans, IGeometry g);
		OkEnvelope? QueryBounds(IDisplay pIDisplay,IDisplayTransformation trans, IGeometry pIGeometry);
	}

	public interface IMarkerBackgroundSupport
	{
		IMarkerBackground Background { get; set; }
	}

	public interface IMarkerBackground
	{
	}
}
