using System;
using System.Collections.Generic;

namespace Agro.LibCore
{
    public interface IField
    {
        string FieldName { get; }
        string AliasName { get; set; }
        eFieldType FieldType { get; }
        eGeometryType GeometryType { get; }
        bool Editable { get; }
        bool IsNullable { get; }
        bool PrimaryKey
        {
            get;
        }
        bool AutoIncrement
        {
            get;
        }
        int Length { get; }
        int Precision { get; }
        int Scale { get; }
        object DefaultValue
        {
            get;
        }
        string DomainName
        {
            get;
            set;
        }
		IField Clone();
    }
    public interface IFields
    {
        int FieldCount { get; }
        int FindField(string Name);
        IField FindField(Predicate<IField> predicate);
        int FindFieldByAliasName(string Name);
        IField GetField(int Index);
        IField AddField(IField field);
        void RemoveField(IField field);
        void Clear();
		IFields Clone();
    }

    public class Field : IField
    {
        private string _aliasName;
        public string FieldName
        {
            get;
            set;
        }
        public string AliasName
        {
            get
            {
                return _aliasName?? FieldName;
            }
            set
            {
                _aliasName = value;
            }
        }
        public eFieldType FieldType
        {
            get;
            set;
        }
        public eGeometryType GeometryType { get; set; }
        public bool Editable { get; set; }
        public bool IsNullable { get; set; }
        public bool PrimaryKey
        {
            get;set;
        }
        public bool AutoIncrement
        {
            get; set;
        }
        /// <summary>
        /// 对sting表示Lengh，对Numeric则表示Precision
        /// </summary>
        public int Length { get; set; }
        public int Precision { get
			{
				return Length;
			}
		}
        public int Scale { get; set; }
        public object DefaultValue { get;set;}
        public string DomainName
        {
            get;
            set;
        }
        public Field()
        {
            IsNullable = true;
            Editable = true;
        }
		public IField Clone()
		{
			var field = new Field
			{
				FieldName = this.FieldName,
				_aliasName = this._aliasName,
				FieldType = this.FieldType,
				GeometryType = this.GeometryType,
				Editable=this.Editable,
				IsNullable=this.IsNullable,
				Length=this.Length,
				Scale=this.Scale,
				DomainName=this.DomainName,
			};
			return field;
		}
    }
    public class Fields : IFields
    {
        private readonly List<IField> _fields = new List<IField>();
        public int FieldCount { get { return _fields.Count; } }

        public IField FindField(Predicate<IField> predicate)
        {
            return _fields.Find(predicate);
        }
        public int FindField(string Name)
        {
            return _fields.FindIndex(fi =>
            {
                return StringUtil.isEqualIgnorCase(Name, fi.FieldName);
            });
        }
        public int FindFieldByAliasName(string Name)
        {
            return _fields.FindIndex(fi =>
            {
                return Name.Equals(fi.AliasName);
            });
        }
        public IField GetField(int Index)
        {
            if (Index < 0 || Index >= _fields.Count)
                return null;
            return _fields[Index];
        }
        public IField AddField(IField field)
        {
            _fields.Add(field);
            return field;
        }
        public void RemoveField(IField field)
        {
            _fields.Remove(field);
        }
        public void Clear()
        {
            _fields.Clear();
        }

		public IFields Clone()
		{
			var fields = new Fields();
			foreach (var it in _fields)
			{
				AddField(it.Clone());
			}
			return fields;
		}
	}
}
