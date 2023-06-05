using Agro.GIS;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/4/24 16:26:37
*/
namespace Agro.Module.DataExchange.LandDotCoil
{
	public class MemFeatureClassUtil
	{
		public static IFields CreateJzdFields()
		{
			var fields = new Fields();
			fields.AddField(FieldsUtil.CreateOIDField("BSM"));
			fields.AddField(FieldsUtil.CreateShapeField(eGeometryType.eGeometryPoint));
			fields.AddField(FieldsUtil.CreateStringField("YSDM", 6));
			fields.AddField(FieldsUtil.CreateStringField("JZDH", 10));
			fields.AddField(FieldsUtil.CreateStringField("JBLX", 1));
			fields.AddField(FieldsUtil.CreateStringField("JZDLX", 1));
			fields.AddField(FieldsUtil.CreateStringField("DKBM", 19));
			return fields;
		}
		public static IFields CreateJzxFields()
		{
			var fields = new Fields();
			fields.AddField(FieldsUtil.CreateOIDField("BSM"));
			fields.AddField(FieldsUtil.CreateShapeField(eGeometryType.eGeometryPolyline));
			fields.AddField(FieldsUtil.CreateStringField("YSDM", 6));
			fields.AddField(FieldsUtil.CreateStringField("JXXZ", 6));
			fields.AddField(FieldsUtil.CreateStringField("JZXLB", 2));
			fields.AddField(FieldsUtil.CreateStringField("JZXWZ", 1));
			fields.AddField(FieldsUtil.CreateStringField("JZXSM", 300));
			fields.AddField(FieldsUtil.CreateStringField("PLDWZJR",100));
			fields.AddField(FieldsUtil.CreateStringField("PLDWQLR", 100));

			fields.AddField(FieldsUtil.CreateStringField("JZXH", 10));
			fields.AddField(FieldsUtil.CreateStringField("QJZDH", 10));
			fields.AddField(FieldsUtil.CreateStringField("ZJZDH", 10));
			return fields;
		}
		public static IFields CreateDkFields()
		{
			var fields = new Fields();
			fields.AddField(FieldsUtil.CreateOIDField("BSM"));
			fields.AddField(FieldsUtil.CreateShapeField(eGeometryType.eGeometryPolygon));
			fields.AddField(FieldsUtil.CreateStringField("DKBM", 19));
			fields.AddField(FieldsUtil.CreateStringField("DKMC", 50));
			fields.AddField(FieldsUtil.CreateStringField("DKLB", 2));
			fields.AddField(FieldsUtil.CreateStringField("TDLYLX", 3));
			fields.AddField(FieldsUtil.CreateStringField("DLDJ", 2));
			fields.AddField(FieldsUtil.CreateStringField("TDYT", 1));
			fields.AddField(FieldsUtil.CreateStringField("SFJBNT", 1));
			return fields;
		}
	}
}
