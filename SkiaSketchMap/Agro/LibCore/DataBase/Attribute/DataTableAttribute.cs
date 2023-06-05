using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Agro.LibCore.Database
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DataTableAttribute : Attribute
    {
        #region Properties

        //public string FullName { get { return string.Format("{0}.{1}", Schema, TableName); } }
        public string TableName { get; set; }
        //public string Schema { get; set; }

        public string AliasName
        {
            get { return string.IsNullOrEmpty(_AliasName) ? TableName : _AliasName; }
            set { _AliasName = value; }
        }

        #endregion

        #region Fields

        private string _AliasName;

        #endregion

        #region Ctor

        public DataTableAttribute()
            : this(string.Empty)
        {
        }

        public DataTableAttribute(string name)
            : this(name,name)
        {
        }

        public DataTableAttribute(string name,string aliasName)
        {
            TableName = name;
            AliasName = aliasName;
        }

        #endregion

        #region Methods

        //public bool Equals(string schema, string name)
        //{
        //    return TableName == name;
        //}

        public override string ToString()
        {
            return TableName;
        }

        #endregion
    }
}