/*
 * (C) 2017 xx公司版权所有，保留所有权利
 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   ShapeFileImportBase
 * 创 建 人：   颜学铭
 * 创建时间：   2017/5/26 10:37:19
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.GIS;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
  public abstract class ShapeFileImportBase
  {
    public class ImportFieldMap
    {
      public readonly SQLParam Prm;
      public readonly int iShpField;
      public readonly eFieldType fieldType;
      /// <summary>
      /// 都是大写字母
      /// </summary>
      public string FieldName
      {
        get
        {
          return Prm.ParamName;
        }
      }
      public ImportFieldMap(string fieldName, int iShpField_, eFieldType t)
      {
        Prm = new SQLParam() { ParamName = fieldName.ToUpper() };
        iShpField = iShpField_;
        fieldType = t;
      }
    }
    public class GetValueParam
    {
      public ShapeFile shp;
      public int row;
      public ImportFieldMap fieldMap;
      public bool Handled;
    }
    public class LogInfo
    {
      public int RecordCount;
      public TimeSpan Times;
    }

    /// <summary>
    /// 不包括objectid和shape字段
    /// </summary>
    /// <param name="db"></param>
    /// <param name="shp"></param>
    /// <param name="tableName"></param>
    /// <param name="oidFieldName"></param>
    /// <param name="shapeFieldName"></param>
    /// <returns></returns>
    public static List<ImportFieldMap> CreateFieldMap(IWorkspace db, ShapeFile shp, string tableName)
    {
      var lst = new List<ImportFieldMap>();
      var fields = db.QueryFields2(tableName);
      for (var i = 0; i < fields.Count; ++i)
      {
        var field = fields[i];
        if (field.FieldType == eFieldType.eFieldTypeOID || field.FieldType == eFieldType.eFieldTypeGeometry)
        {
          continue;
        }
        var n = shp.FindField(field.FieldName);
        if (n >= 0)
        {
          var m = new ImportFieldMap(field.FieldName, n, field.FieldType);
          lst.Add(m);
        }
      }
      return lst;
    }
  }
}
