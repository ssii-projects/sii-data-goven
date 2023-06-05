using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Captain.NetCore;

namespace Captain.Library.Model
{
    public static class ObjectExtension
    {
        #region Class

        public class ConvertToPropertyParameters
        {
            #region Properties

            public string FromName { get; internal set; }
            public object FromValue { get; internal set; }
            public PropertyInfo FromProperty { get; internal set; }

            public string ToName { get; set; }
            public object ToValue { get; set; }
            public PropertyInfo ToProperty { get; internal set; }

            public bool Skip { get; set; }

            #endregion
        }

        #endregion

        #region Methods

        #region Methods - Copy

        public static object ConvertTo(this object source, Type targetType)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            object target = Activator.CreateInstance(targetType);
            Type tType = targetType;

            source.TraversalPropertiesInfo((pi, val, t) =>
            {
                var tpi = tType.GetProperty(pi.Name);
                if (tpi == null)
                {
                    return true;
                }
                if (!tpi.CanWrite)
                {
                    return true;
                }
                bool isSameType = IsIntegerType(tpi.PropertyType);
                if (!isSameType && pi.PropertyType != tpi.PropertyType && tpi.PropertyType.FullName != "System.Double")
                {
                    return true;
                }
                if (isSameType)
                {
                    val = InitalizeDataType(tpi.PropertyType, val);
                }

                tpi.SetValue(t, val, null);

                return true;
            }, target);

            return target;
        }

        public static object ConvertTo(this object source, Type targetType, Dictionary<string, string> mapping)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            object target = Activator.CreateInstance(targetType);
            Type tType = targetType;

            source.TraversalPropertiesInfo((pi, val, t) =>
            {
                if (!mapping.ContainsKey(pi.Name))
                {
                    return true;
                }
                var nameTarget = mapping[pi.Name];
                var tpi = tType.GetProperty(nameTarget);
                if (tpi == null)
                {
                    return true;
                }
                if (!tpi.CanWrite)
                {
                    return true;
                }
                bool isSameType = IsIntegerType(tpi.PropertyType);
                if (!isSameType && pi.PropertyType != tpi.PropertyType && tpi.PropertyType.FullName != "System.Double")
                {
                    return true;
                }
                if (isSameType)
                {
                    val = InitalizeDataType(tpi.PropertyType, val);
                }

                tpi.SetValue(t, val, null);

                return true;
            }, target);

            return target;
        }

        public static object ConvertTo(this object source, Type targetType, bool compatibilityConvertValue)
        {
            if (!compatibilityConvertValue)
                return ConvertTo(source, targetType);

            if (source == null)
                throw new ArgumentNullException("source");

            object target = Activator.CreateInstance(targetType);
            Type tType = targetType;

            source.TraversalPropertiesInfo((pi, val, t) =>
            {
                var tpi = tType.GetProperty(pi.Name);
                if (tpi == null)
                {
                    return true;
                }
                if (!tpi.CanWrite)
                {
                    return true;
                }
                bool isSameType = IsIntegerType(tpi.PropertyType);
                if (isSameType)
                {
                    val = InitalizeDataType(tpi.PropertyType, val);
                }
                tpi.SetValue(t, DotNetTypeConverter.Instance.To(val, tpi.PropertyType), null);

                return true;
            }, target);

            return target;
        }

        public static object ConvertTo(this object source, Type targetType, bool compatibilityConvertValue, Dictionary<string, string> mapping)
        {
            if (!compatibilityConvertValue)
                return ConvertTo(source, targetType, mapping);

            if (source == null)
                throw new ArgumentNullException("source");

            object target = Activator.CreateInstance(targetType);
            Type tType = targetType;

            source.TraversalPropertiesInfo((pi, val, t) =>
            {
                if (!mapping.ContainsKey(pi.Name))
                {
                    return true;
                }
                var nameTarget = mapping[pi.Name];
                var tpi = tType.GetProperty(nameTarget);
                if (tpi == null)
                {
                    return true;
                }
                if (!tpi.CanWrite)
                {
                    return true;
                }
                bool isSameType = IsIntegerType(tpi.PropertyType);
                if (isSameType)
                {
                    val = InitalizeDataType(tpi.PropertyType, val);
                }
                tpi.SetValue(t, DotNetTypeConverter.Instance.To(val, tpi.PropertyType), null);

                return true;
            }, target);

            return target;
        }

        public static T ConvertTo<T>(this object source) where T : new()
        {
            if (source == null)
                throw new ArgumentNullException("source");

            T target = new T();
            Type tType = typeof(T);

            source.TraversalPropertiesInfo((pi, val, t) =>
            {
                var tpi = tType.GetProperty(pi.Name);
                if (tpi == null)
                {
                    return true;
                }
                if (!tpi.CanWrite)
                {
                    return true;
                }
                bool isSameType = IsIntegerType(tpi.PropertyType);
                if (!isSameType && pi.PropertyType != tpi.PropertyType && tpi.PropertyType.FullName != "System.Double")
                {
                    return true;
                }
                if (isSameType)
                {
                    val = InitalizeDataType(tpi.PropertyType, val);
                }
                tpi.SetValue(t, val, null);

                return true;
            }, target);

            return target;
        }

        public static T ConvertTo<T>(this object source, bool compatibilityConvertValue) where T : new()
        {
            return (T)ConvertTo(source, typeof(T), compatibilityConvertValue);
        }

        public static T ConvertTo<T>(this object source, Dictionary<string, string> mapping) where T : new()
        {
            return (T)ConvertTo(source, typeof(T), mapping);
        }

        public static T ConvertTo<T>(this object source, bool compatibilityConvertValue, Dictionary<string, string> mapping) where T : new()
        {
            return (T)ConvertTo(source, typeof(T), compatibilityConvertValue, mapping);
        }

        public static T ConvertTo<T>(this object source, Action<ConvertToPropertyParameters> callback) where T : new()
        {
            if (source == null)
                throw new ArgumentNullException("source");

            T target = new T();
            Type tType = typeof(T);

            source.TraversalPropertiesInfo((pi, val, t) =>
            {
                ConvertToPropertyParameters p = new ConvertToPropertyParameters();
                p.FromName = pi.Name;
                p.ToName = pi.Name;
                p.Skip = false;

                var tpi = tType.GetProperty(p.ToName);
                if (tpi == null)
                    return true;
                if (!tpi.CanWrite)
                    return true;
                //if (pi.PropertyType != tpi.PropertyType)
                //    return true;

                p.FromValue = val;
                p.ToValue = val;
                p.FromProperty = pi;
                p.ToProperty = tpi;

                callback(p);
                if (p.Skip)
                    return true;

                tpi.SetValue(t, p.ToValue, null);

                return true;
            }, target);

            return target;
        }

        public static void CopyPropertiesFrom(this object target, object source)
        {
            if (source == null || target == null)
                return;

            Type typeSource = source.GetType();
            Type typeTarget = target.GetType();

            PropertyInfo[] listSourceProperty = typeSource.GetProperties();
            PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

            foreach (PropertyInfo piSource in listSourceProperty)
            {
                if (!piSource.CanRead)
                    continue;

                var pms = piSource.GetIndexParameters();
                if (pms != null && pms.Length > 0)
                    continue;

                object val = piSource.GetValue(source, null);
                foreach (PropertyInfo piTarget in listTargetProperty)
                    if (piTarget.Name == piSource.Name &&
                        piTarget.PropertyType.FullName == piSource.PropertyType.FullName &&
                        piTarget.CanWrite)
                    {
                        piTarget.SetValue(target, val, null);
                        break;
                    }
            }
        }

        public static void CopyPropertiesFrom(this object target, object source, bool compatibilityConvertValue)
        {
            if (!compatibilityConvertValue)
            {
                CopyPropertiesFrom(target, source);
                return;
            }

            if (source == null || target == null)
                return;

            Type typeSource = source.GetType();
            Type typeTarget = target.GetType();

            PropertyInfo[] listSourceProperty = typeSource.GetProperties();
            PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

            foreach (PropertyInfo piSource in listSourceProperty)
            {
                if (!piSource.CanRead)
                    continue;

                var pms = piSource.GetIndexParameters();
                if (pms != null && pms.Length > 0)
                    continue;

                object val = piSource.GetValue(source, null);
                foreach (PropertyInfo piTarget in listTargetProperty)
                    if (piTarget.Name == piSource.Name &&
                        piTarget.CanWrite)
                    {
                        piTarget.SetValue(target, DotNetTypeConverter.Instance.To(val, piTarget.PropertyType), null);
                        break;
                    }
            }
        }

        public static void CopyPropertiesFrom(this object target, object source, Dictionary<string, string> mapping)
        {
            if (source == null || target == null)
                return;

            Type typeSource = source.GetType();
            Type typeTarget = target.GetType();

            PropertyInfo[] listSourceProperty = typeSource.GetProperties();
            PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

            foreach (PropertyInfo piSource in listSourceProperty)
            {
                if (!mapping.ContainsKey(piSource.Name))
                    continue;
                if (!piSource.CanRead)
                    continue;

                var pms = piSource.GetIndexParameters();
                if (pms != null && pms.Length > 0)
                    continue;

                string nameTarget = mapping[piSource.Name];
                object val = piSource.GetValue(source, null);
                foreach (PropertyInfo piTarget in listTargetProperty)
                    if (piTarget.Name == nameTarget &&
                        piTarget.PropertyType.FullName == piSource.PropertyType.FullName &&
                        piTarget.CanWrite)
                    {
                        piTarget.SetValue(target, val, null);
                        break;
                    }
            }
        }

        public static void CopyPropertiesFrom(this object target, object source, bool compatibilityConvertValue, Dictionary<string, string> mapping)
        {
            if (!compatibilityConvertValue)
            {
                CopyPropertiesFrom(target, source, mapping);
                return;
            }

            if (source == null || target == null)
                return;

            Type typeSource = source.GetType();
            Type typeTarget = target.GetType();

            PropertyInfo[] listSourceProperty = typeSource.GetProperties();
            PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

            foreach (PropertyInfo piSource in listSourceProperty)
            {
                if (!mapping.ContainsKey(piSource.Name))
                    continue;
                if (!piSource.CanRead)
                    continue;

                var pms = piSource.GetIndexParameters();
                if (pms != null && pms.Length > 0)
                    continue;

                string nameTarget = mapping[piSource.Name];
                object val = piSource.GetValue(source, null);
                foreach (PropertyInfo piTarget in listTargetProperty)
                    if (piTarget.Name == nameTarget &&
                        piTarget.CanWrite)
                    {
                        piTarget.SetValue(target, DotNetTypeConverter.Instance.To(val, piTarget.PropertyType), null);
                        break;
                    }
            }
        }

        public static void CopyPropertiesFromExcept(this object target, object source, string[] exceptProperties)
        {
            if (source == null || target == null)
                return;

            if (exceptProperties == null)
                exceptProperties = new string[0];

            Type typeSource = source.GetType();
            Type typeTarget = target.GetType();

            PropertyInfo[] listSourceProperty = typeSource.GetProperties();
            PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

            foreach (PropertyInfo piSource in listSourceProperty)
            {
                if (!piSource.CanRead)
                    continue;

                if (exceptProperties.Contains(piSource.Name))
                    continue;

                var pms = piSource.GetIndexParameters();
                if (pms != null && pms.Length > 0)
                    continue;

                object val = piSource.GetValue(source, null);
                foreach (PropertyInfo piTarget in listTargetProperty)
                    if (piTarget.Name == piSource.Name &&
                        piTarget.PropertyType.FullName == piSource.PropertyType.FullName &&
                        piTarget.CanWrite)
                    {
                        piTarget.SetValue(target, val, null);
                        break;
                    }
            }
        }

        #endregion

        #region Methods - Traversal

        public static void TraversalMethodsInfo(this object source, Func<MethodInfo, bool> method)
        {
            TypeExtension.TraversalMethodsInfo(source.GetType(), method);
        }

        public static void TraversalFieldsInfo(this object source, Func<FieldInfo, object, bool> method)
        {
            if (method == null || source == null)
            {
                return;
            }
            FieldInfo[] listFieldInfo = source.GetType().GetFields();
            foreach (FieldInfo fi in listFieldInfo)
            {
                object val = fi.GetValue(source);
                if (!method(fi, val))
                {
                    return;
                }
            }
        }

        public static void TraversalPropertiesInfo(this object source, Func<string, object, bool> method)
        {
            if (method == null || source == null)
            {
                return;
            }
            PropertyInfo[] listPropertyInfo = source.GetType().GetProperties();

            foreach (PropertyInfo pi in listPropertyInfo)
            {
                if (!pi.CanRead)
                {
                    continue;
                }
                object val = pi.GetValue(source, null);
                if (!method(pi.Name, val))
                {
                    return;
                }
            }
        }

        public static void TraversalPropertiesInfo(this object source, Func<PropertyInfo, object, bool> method)
        {
            if (method == null || source == null)
            {
                return;
            }
            PropertyInfo[] listPropertyInfo = source.GetType().GetProperties().OrderBy(c => c.MetadataToken).ToArray();
            foreach (PropertyInfo pi in listPropertyInfo)
            {
                if (!pi.CanRead)
                {
                    continue;
                }
                try
                {
                    object val = pi.GetValue(source, null);
                    if (!method(pi, val))
                    {
                        return;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public static void TraversalPropertiesInfo(this object source, Func<PropertyInfo, object, object, bool> method, object argument)
        {
            if (method == null || source == null)
            {
                return;
            }
            PropertyInfo[] listPropertyInfo = source.GetType().GetProperties();
            foreach (PropertyInfo pi in listPropertyInfo)
            {
                if (!pi.CanRead)
                {
                    continue;
                }
                try
                {
                    object val = pi.GetValue(source, null);
                    if (!method(pi, val, argument))
                    {
                        return;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        #endregion

        #region Methods - Extend

        public static int GetLiteralLength(this object source)
        {
            return source == null ? -1 : source.ToString().Length;
        }

        public static string ToStringOrEmpty(this object source)
        {
            return source == null ? string.Empty : source.ToString();
        }

        #endregion

        #region Methods - Properties

        public static bool HasProperty(this object source, string propertyName)
        {
            PropertyInfo pi = source.GetType().GetProperty(propertyName);
            if (pi == null)
                return false;

            return true;
        }

        public static object GetPropertyValue(this object source, string propertyName, bool deep = false, bool throwNotFound = true)
        {
            if (deep)
                return GetPropertyValueDeep(source, propertyName, throwNotFound);

            PropertyInfo pi = source.GetType().GetProperty(propertyName);
            if (pi == null && throwNotFound)
                throw new KeyNotFoundException(propertyName);
            else if (pi == null)
                return null;

            return pi.GetValue(source, null);
        }

        public static T GetPropertyValue<T>(this object source, string propertyName, bool deep = false, bool throwNotFound = true)
        {
            return (T)GetPropertyValue(source, propertyName, deep, throwNotFound);
        }

        private static object GetPropertyValueDeep(object source, string propertyName, bool throwNotFound = true)
        {
            var pts = propertyName.Split('.');

            foreach (var property in pts)
            {
                source = source.GetPropertyValue(property, false, throwNotFound);
                if (source == null)
                    return null;
            }

            return source;
        }

        public static void SetPropertyValue(this object source, string propertyName, object propertyValue)
        {
            PropertyInfo pi = source.GetType().GetProperty(propertyName);
            if (pi == null)
            {
                throw new KeyNotFoundException(propertyName);
            }
            pi.SetValue(source, propertyValue, null);
        }

        public static KeyValueList<string, object> DictProperties(this object source)
        {
            KeyValueList<string, object> dic = new KeyValueList<string, object>();

            PropertyInfo[] pis = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var pi in pis)
                dic[pi.Name] = pi.GetValue(source, null);

            return dic;
        }

        public static Dictionary<string, object> DictPropertiesToDictionary(this object source)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            PropertyInfo[] pis = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var pi in pis)
                dic[pi.Name] = pi.GetValue(source, null);

            return dic;
        }

        public static KeyValueList<string, object> DictPropertiesEnumToInt(this object source)
        {
            var dic = source.DictProperties();
            foreach (var item in dic)
            {
                if (item.Value == null)
                    continue;

                Type t = item.Value.GetType();
                if (t.IsEnum)
                    item.Value = Convert.ToInt32(item.Value);
            }
            return dic;
        }

        #endregion

        #region Methods - Convert

        /// <summary>
        /// 是否int类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsIntegerType(this Type type)
        {
            bool isIntegerType = false;
            switch (type.FullName)
            {
                case "System.Int16":
                    isIntegerType = true;
                    break;
                case "System.Int32":
                    isIntegerType = true;
                    break;
                case "System.Int64":
                    isIntegerType = true;
                    break;
                default:
                    break;
            }
            return isIntegerType;
        }

        /// <summary>
        /// 初始化枚举类型数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object InitalizeTypeData(this Type type, object value)
        {
            if (type == null)
            {
                return value;
            }
            object obj = value;
            switch (type.FullName)
            {
                case "System.Int16":
                    Int16 val16 = 0;
                    Int16.TryParse(value.ToString(), out val16);
                    obj = val16;
                    break;
                case "System.Int32":
                    Int32 val32 = 0;
                    Int32.TryParse(value.ToString(), out val32);
                    obj = val32;
                    break;
                case "System.Int64":
                    Int64 val64 = 0;
                    Int64.TryParse(value.ToString(), out val64);
                    obj = val64;
                    break;
                default:
                    break;
            }
            return obj;
        }

        /// <summary>
        /// 初始化枚举类型数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object InitalizeDataType(this Type type, object value, bool increment = false)
        {
            if (type == null)
            {
                return value;
            }
            object obj = value;
            string name = type.FullName;
            if (name.Contains("System.Int16"))
            {
                Int16 val16 = 0;
                Int16.TryParse(value.ToString(), out val16);
                obj = increment ? (val16 + 1) : val16;
            }
            if (name.Contains("System.Int32"))
            {
                Int32 val32 = 0;
                Int32.TryParse(value.ToString(), out val32);
                obj = increment ? (val32 + 1) : val32;
            }
            if (name.Contains("System.Int64"))
            {
                Int64 val64 = 0;
                Int64.TryParse(value.ToString(), out val64);
                obj = increment ? (val64 + 1) : val64;
            }
            return obj;
        }

        /// <summary>
        /// 转换成整数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int ToInt32(this object source)
        {
            if (source == null)
            {
                return 0;
            }
            int value = 0;
            Int32.TryParse(source.ToString(), out value);
            return value;
        }

        /// <summary>
        /// 转换成双精度数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double ToDouble(this object source)
        {
            if (source == null)
            {
                return 0.0;
            }
            double value = 0.0;
            double.TryParse(source.ToString(), out value);
            return value;
        }

        /// <summary>
        /// 转换成日期
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this object source)
        {
            if (source == null)
            {
                return null;
            }
            DateTime value = DateTime.Now;
            DateTime.TryParse(source.ToString(), out value);
            return value;
        }

        /// <summary>
        /// 转换成布尔值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Boolean? ToBoolean(this object source)
        {
            if (source == null)
            {
                return null;
            }
            bool value = true;
            Boolean.TryParse(source.ToString(), out value);
            return value;
        }

        #endregion

        #endregion
    }
}
