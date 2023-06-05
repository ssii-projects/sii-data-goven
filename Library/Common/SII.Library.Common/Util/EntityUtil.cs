using Agro.LibCore.Database;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common.Util
{
	public abstract class EntityWrap{
		public abstract string TableName { get; }
		public abstract EntityProperty Attribute(string propertyName);
		public abstract List<EntityProperty> GetProperties(List<EntityProperty> lst = null, bool fContainBinaryField = true);
	}
    public class Entity<T>: EntityWrap where T : new()
    {
		public T en;
        public Entity()
        {
            en = new T();
        }

		public override string TableName
		{
			get
			{
				var t = typeof(T);
				var att = t.GetCustomAttributes(typeof(DataTableAttribute), false).FirstOrDefault();
				var da = att as DataTableAttribute;
				return da.TableName;
			}
		}

		public static string GetTableName()
        {
            var t = typeof(T);
            var att = t.GetCustomAttributes(typeof(DataTableAttribute), false).FirstOrDefault();
            var da = att as DataTableAttribute;
            return da.TableName;
        }

		public override EntityProperty Attribute(string propertyName)
		{
			var t = typeof(T);
			var fi = t.GetProperty(propertyName);
			var o = fi.GetCustomAttributes(typeof(DataColumnAttribute), false).FirstOrDefault() as DataColumnAttribute;
			//var dt = o.ColumnType;
			var ep = new EntityProperty();
			ep.PropertyName = propertyName;
			ep.ColumnName = o.ColumnName;
			ep.AliasName = o.AliasName;
			ep.FieldType = fi.PropertyType;
			return ep;// as DotNetSharp.Data.DataColumnAttribute;
		}

		public override List<EntityProperty> GetProperties( List<EntityProperty> lst=null, bool fContainBinaryField = true)
        {
            if (lst == null)
            {
                lst = new List<EntityProperty>();
            }else
            {
                lst.Clear();
            }
            var t = typeof(T);
            var pis=t.GetProperties();
           foreach(var pi in pis)
            {
                bool fBinaryField = false;
                if(pi.PropertyType == typeof(byte[])
                    ||pi.PropertyType==typeof(Geometry))
                {
                    fBinaryField = true;
                }
                if (!fContainBinaryField && fBinaryField)
                {
                    continue;
                }
                var ca = Attribute(pi.Name);
                lst.Add(ca);
            }
            return lst;
        }
	}
}
