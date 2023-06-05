/// <summary>
///项目:  Lucky.AbstractEntity
///描述:  定义所有实体的基类 
///版本:1.0
///日期:2012/5/4
///作者:颜学铭
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.Database
{
    /// <summary>
    ///类:  Lucky.AbstractEntity.DynamicEntityField
    ///描述:  定义所有实体的基类 
    ///版本:1.0
    ///日期:2012/5/4
    ///作者:颜学铭
    ///更新:沈超
    /// </summary>
    public class DynamicEntityField
    {
		internal readonly EntityProperty FieldDefinition;// { get; private set; }
														 /// <summary>
														 /// 字段名称
														 /// </summary>
		public string FieldName
		{
			get { return FieldDefinition.FieldName; }
		}
		//          private set;
		//      } 
		//      /// <summary>
		//      /// 中文名称
		//      /// </summary>
		//      public string AliasName { get; set; } 
		///// <summary>
		///// 是否自增长字段
		///// </summary>
		//public bool Auto { get; set; }
		/// <summary>
		/// 字段值
		/// </summary>
		public object FieldValue;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fieldName">字段（对大小写无要求）</param>
        /// <param name="fieldValue">值</param>
        public DynamicEntityField(/*string fieldName*/ EntityProperty ep, object fieldValue = null)
        {
			FieldDefinition = ep;
			//SetFieldName(fieldName);
			this.FieldValue = fieldValue;
        }
        ///// <summary>
        ///// 设置字段名
        ///// </summary>
        ///// <param name="fieldName">字段（对大小写无要求）</param>
        //public void SetFieldName(string fieldName)
        //{
        //    this.FieldName = fieldName[0].ToString().ToUpper() + fieldName.Substring(1).ToLower();
        //}
    }

    /// <summary>
    ///类:  Lucky.AbstractEntity.DynamicEntity
    ///描述:  动态实体 
    ///版本:1.0
    ///日期:2012/5/4
    ///作者:颜学铭
    ///更新:沈超
    /// </summary>
    public class DynamicEntity
    {
        /// <summary>
        /// [字段名,DynamicEntityField]，
        /// 字段名约束：首字母大写，其余小写
        /// </summary>
        public readonly List<DynamicEntityField> Fields = new List<DynamicEntityField>();
        ///// <summary>
        ///// 修改标识
        ///// </summary>
        //public EModifyFlag modifyFlag = EModifyFlag.MF_UNCHANGE;

        /// <summary>
        /// 按字段名设置字段值
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="val">值</param>
        public void SetFieldValue(string fieldName, object val)
        {
            DynamicEntityField de = Fields.Find((x) => { return x.FieldName.ToLower() == fieldName.ToLower(); });
            if (de != null)
                de.FieldValue = val;
        }
        /// <summary>
        /// 根据字段名获取字段值
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <returns>值</returns>
        public object GetFieldValue(string fieldName)
        {
            DynamicEntityField de = Fields.Find((x) => { return x.FieldName.ToLower() == fieldName.ToLower(); });
            return de?.FieldValue;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rhs">动态实体</param>
        public DynamicEntity(DynamicEntity rhs = null)
        {
            OverWrite(rhs);
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="rhs">实体对象</param>
        /// <returns>实体对象</returns>
        public DynamicEntity OverWrite(DynamicEntity rhs)
        {
            if (rhs != null)
            {
                //TableName = rhs.TableName;
                //modifyFlag = rhs.modifyFlag;
                Fields.Clear();
                Fields.AddRange(rhs.Fields);
            }
            return this;
        }
    }
    /// <summary>
    ///类:  Lucky.AbstractEntity.DynamicEntityTable
    ///描述:  动态实体表 
    ///版本:1.0
    ///日期:2012/5/4
    ///作者:颜学铭
    ///更新:沈超
    /// </summary>
    public class DynamicEntityTable
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName;
        /// <summary>
        /// 字段
        /// </summary>
        public List<DynamicEntityField> Columns = null;
        /// <summary>
        /// 行
        /// </summary>
        public List<DynamicEntity> Rows = new List<DynamicEntity>();
    }
}
