using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Agro.LibCore
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class EnumNameAttribute : Attribute
    {
        #region Fields

        private string description;

        private FieldInfo infoField;

        #endregion

        #region Properties

        public bool IsLanguageName { get; set; }

        public string Description
        {
            get { return GetDescription(); }
        }

        public object Value
        {
            get
            {
                if (infoField == null)
                    return null;

                return infoField.GetValue(null);
            }
        }

        #endregion

        #region Ctor

        public EnumNameAttribute(string description)
        {
            this.description = description;
        }

        #endregion

        #region Methods

        #region Methods - Helper

        private string GetDescription()
        {
            if (!IsLanguageName)
                return description;

            return LanguageAttribute.GetLanguage(description);
        }

        #endregion

        #region Methods - Static

        public static EnumNameAttribute[] GetAttributes(Type enumType)
        {
            Type typeAttr = typeof(EnumNameAttribute);
            FieldInfo[] fis = enumType.GetFields();
            ArrayList list = new ArrayList();
            foreach (FieldInfo fi in fis)
            {
                if (!fi.FieldType.IsEnum)
                    continue;

                object[] objs = fi.GetCustomAttributes(typeAttr, false);
                EnumNameAttribute edn = null;
                if (objs.Length != 0)
                    edn = objs[0] as EnumNameAttribute;
                else
                    edn = new EnumNameAttribute(fi.Name);
                edn.infoField = fi;
                list.Add(edn);
            }

            return (EnumNameAttribute[])list.ToArray(typeof(EnumNameAttribute));
        }

        public static string GetDescription(Type enumType)
        {
            EnumNameAttribute[] attrs =
                (EnumNameAttribute[])enumType.GetCustomAttributes(typeof(EnumNameAttribute), false);
            if (attrs.Length == 1)
                return attrs[0].GetDescription();

            return enumType.ToString();
        }

        public static string GetDescription(object enumValue)
        {
            EnumNameAttribute attr = GetAttribute(enumValue);
            if (attr != null)
                return attr.GetDescription();

            return enumValue.ToString();
        }

        public static EnumNameAttribute GetAttribute(object enumValue)
        {
            if (enumValue == null)
                throw new ArgumentNullException("enumValue");

            EnumNameAttribute[] attrs = GetAttributes(enumValue.GetType());
            foreach (EnumNameAttribute attr in attrs)
                if (attr.infoField.Name == enumValue.ToString())
                    return attr;

            return null;
        }

        public static object GetValue(Type enumType, string description)
        {
            EnumNameAttribute[] attrs = EnumNameAttribute.GetAttributes(enumType);
            foreach (EnumNameAttribute attr in attrs)
                if (attr.Description == description)
                    return attr.Value;

            return null;
        }

        #endregion

        #endregion
    }
}