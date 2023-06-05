/*
 * (C) 2017 xx公司版权所有，保留所有权利
 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   eFieldType
 * 创 建 人：   颜学铭
 * 创建时间：   2017/5/23 9:47:28
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore
{
    public enum eFieldType
    {
		eFieldTypeNull = -1,
		#region 此部分保持和ArcGIS定义一致
		eFieldTypeSmallInteger = 0,
        eFieldTypeInteger = 1,
        eFieldTypeSingle = 2,
        eFieldTypeDouble = 3,
        eFieldTypeString = 4,
        eFieldTypeDate = 5,
        eFieldTypeOID = 6,
        eFieldTypeGeometry = 7,
        eFieldTypeBlob = 8,
        eFieldTypeRaster = 9,
        eFieldTypeGUID = 10,
        eFieldTypeGlobalID = 11,
        eFieldTypeXML = 12,
        #endregion

        //yxm extension
        eFieldTypeBool = 13,
        eFieldTypeDateTime = 14,
        eFieldTypeTime = 15,
        eFieldTypeTimeStamp = 16,
        eFieldTypeByte=17,
		eFieldTypeBigInt=18,

	}

    /// <summary>
    /// 与ArcGIS保持一致
    /// </summary>
    public enum eGeometryType
    {
        eGeometryNull = 0,
        eGeometryPoint = 1,
        eGeometryMultipoint = 2,
        eGeometryPolyline = 3,
        eGeometryPolygon = 4,
        eGeometryEnvelope = 5,
        eGeometryPath = 6,
        eGeometryAny = 7,
        eGeometryMultiPatch = 9,
        eGeometryRing = 11,
        eGeometryLine = 13,
        eGeometryCircularArc = 14,
        eGeometryBezier3Curve = 15,
        eGeometryEllipticArc = 16,
        eGeometryBag = 17,
        eGeometryTriangleStrip = 18,
        eGeometryTriangleFan = 19,
        eGeometryRay = 20,
        eGeometrySphere = 21,
        eGeometryTriangles = 22,
    }
}
