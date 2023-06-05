using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Agro.LibCore.Database
{
	/// <summary>
	/// 参考JPA的Column定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DataColumnAttribute : Attribute
    {
		#region Properties

		/// <summary>
		/// 标注字段在数据库表中所对应字段的名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// unique属性表示该字段是否为唯一标识，默认为false。如果表中有一个字段需要唯一标识，则既可以使用该标记，也可以使用@Table标记中的@UniqueConstraint。
		/// </summary>
		public bool Unique { get; set; }

		/// <summary>
		/// 表示该字段是否可以为null值，默认为true。
		/// </summary>
		public bool Nullable { get; set; } = true;

		/// <summary>
		/// 表示在使用“INSERT”脚本插入数据时，是否需要插入该字段的值。
		/// </summary>
		public bool Insertable { get; set; } = true;
		/// <summary>
		/// 表示在使用“UPDATE”脚本插入数据时，是否需要更新该字段的值。
		/// insertable和updatable属性一般多用于只读的属性，例如主键和外键等。这些字段的值通常是自动生成的。
		/// </summary>
		public bool Updatable { get; set; } = true;
        public bool PrimaryKey
        {
            get; set;
        }
		/// <summary>
		/// 不使用
		/// </summary>
		//private eFieldType ColumnType { get; set; }

		/// <summary>
		/// 表示字段的长度，当字段的类型为varchar时，该属性才有效，默认为255个字符。
		/// </summary>
		public int Length { get; set; }


		/// <summary>
		/// 当字段类型为double或decimal时，precision表示数值的总长度
		/// </summary>
		public int Precision { get; set; }
		/// <summary>
		/// 当字段类型为double或decimal时，表示小数点所占的位数
		/// </summary>
		public int Scale { get; set; }


        //public bool PrimaryKey { get; set; }



        /// <summary>
        /// 是否自增长
        /// </summary>
        public bool AutoIncrement
        {
            get; set;
        }
        //      public bool Enabled { get; set; }

        /// <summary>
        /// 字段别名
        /// </summary>
        public string AliasName { get; set; }

        public string CodeType
        {
            get; set;
        }
		/// <summary>
		/// 当字段类型为IGeometry时有效
		/// </summary>
		public eGeometryType GeometryType { get; set; } = eGeometryType.eGeometryNull;

		public eFieldType FieldType { get; set; } = eFieldType.eFieldTypeNull;

        public object DefaultValue
        {
            get; set;
        }
        public object Tag
        {
            get; set;
        }
		//{
		//    get { return string.IsNullOrEmpty(_AliasName) ? Name : _AliasName; }
		//    set { _AliasName = value; }
		//}

		//public string MemberName
		//{
		//    get { return string.IsNullOrEmpty(_MemberName) ? Name : _MemberName; }
		//    set { _MemberName = value; }
		//}

		#endregion

		//#region Fields

		//private string _AliasName;
		//private string _MemberName;

		//#endregion

		#region Ctor

		public DataColumnAttribute()
          //  : this(string.Empty, -1, 0, 0,  true)//, false, true)
        {
        }

        public DataColumnAttribute(string name)
          //  : this(name,  -1, 0, 0, true)//, false, true)
        {
			Name = name;
        }

        //public DataColumnAttribute(string name, /*eFieldType type,*/ int size, int precision, int scale,/* bool primaryKey,*/ bool nullable)//, /*bool auto,*/ bool enable)
        //{
        //    Name = name;
        //    //ColumnType = type;
        //    Length = size;
        //    //PrimaryKey = primaryKey;
        //    Precision = precision;
        //    Scale = scale;
        //    Nullable = nullable;
        //    //Auto = auto;
        //    //Enabled = enable;
        //}

        #endregion

        #region Methods

        public override string ToString()
        {
			return Name;// string.Format("{0}, {1}", Name, ColumnType);
        }

        #endregion
    }
}
