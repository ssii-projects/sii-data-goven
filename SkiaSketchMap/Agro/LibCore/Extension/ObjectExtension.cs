﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Agro.LibCore
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
                    return true;
                if (!tpi.CanWrite)
                    return true;
                if (pi.PropertyType != tpi.PropertyType)
                    return true;

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
                    return true;

                var nameTarget = mapping[pi.Name];
                var tpi = tType.GetProperty(nameTarget);
                if (tpi == null)
                    return true;
                if (!tpi.CanWrite)
                    return true;
                if (pi.PropertyType != tpi.PropertyType)
                    return true;

                tpi.SetValue(t, val, null);

                return true;
            }, target);

            return target;
        }

        //public static object ConvertTo(this object source, Type targetType, bool compatibilityConvertValue)
        //{
        //    if (!compatibilityConvertValue)
        //        return ConvertTo(source, targetType);

        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    object target = Activator.CreateInstance(targetType);
        //    Type tType = targetType;

        //    source.TraversalPropertiesInfo((pi, val, t) =>
        //    {
        //        var tpi = tType.GetProperty(pi.Name);
        //        if (tpi == null)
        //            return true;
        //        if (!tpi.CanWrite)
        //            return true;

        //        tpi.SetValue(t, DotNetTypeConverter.Instance.To(val, tpi.PropertyType), null);

        //        return true;
        //    }, target);

        //    return target;
        //}

        //public static object ConvertTo(this object source, Type targetType, bool compatibilityConvertValue, Dictionary<string, string> mapping)
        //{
        //    if (!compatibilityConvertValue)
        //        return ConvertTo(source, targetType, mapping);

        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    object target = Activator.CreateInstance(targetType);
        //    Type tType = targetType;

        //    source.TraversalPropertiesInfo((pi, val, t) =>
        //    {
        //        if (!mapping.ContainsKey(pi.Name))
        //            return true;

        //        var nameTarget = mapping[pi.Name];
        //        var tpi = tType.GetProperty(nameTarget);
        //        if (tpi == null)
        //            return true;
        //        if (!tpi.CanWrite)
        //            return true;

        //        tpi.SetValue(t, DotNetTypeConverter.Instance.To(val, tpi.PropertyType), null);

        //        return true;
        //    }, target);

        //    return target;
        //}

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
                    return true;
                if (!tpi.CanWrite)
                    return true;
                if (pi.PropertyType != tpi.PropertyType)
                    return true;

                tpi.SetValue(t, val, null);

                return true;
            }, target);

            return target;
        }

        //public static T ConvertTo<T>(this object source, bool compatibilityConvertValue) where T : new()
        //{
        //    return (T)ConvertTo(source, typeof(T), compatibilityConvertValue);
        //}

        public static T ConvertTo<T>(this object source, Dictionary<string, string> mapping) where T : new()
        {
            return (T)ConvertTo(source, typeof(T), mapping);
        }

        //public static T ConvertTo<T>(this object source, bool compatibilityConvertValue, Dictionary<string, string> mapping) where T : new()
        //{
        //    return (T)ConvertTo(source, typeof(T), compatibilityConvertValue, mapping);
        //}

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

        //public static void CopyPropertiesFrom(this object target, object source, bool compatibilityConvertValue)
        //{
        //    if (!compatibilityConvertValue)
        //    {
        //        CopyPropertiesFrom(target, source);
        //        return;
        //    }

        //    if (source == null || target == null)
        //        return;

        //    Type typeSource = source.GetType();
        //    Type typeTarget = target.GetType();

        //    PropertyInfo[] listSourceProperty = typeSource.GetProperties();
        //    PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

        //    foreach (PropertyInfo piSource in listSourceProperty)
        //    {
        //        if (!piSource.CanRead)
        //            continue;

        //        var pms = piSource.GetIndexParameters();
        //        if (pms != null && pms.Length > 0)
        //            continue;

        //        object val = piSource.GetValue(source, null);
        //        foreach (PropertyInfo piTarget in listTargetProperty)
        //            if (piTarget.Name == piSource.Name &&
        //                piTarget.CanWrite)
        //            {
        //                piTarget.SetValue(target, DotNetTypeConverter.Instance.To(val, piTarget.PropertyType), null);
        //                break;
        //            }
        //    }
        //}

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

        //public static void CopyPropertiesFrom(this object target, object source, bool compatibilityConvertValue, Dictionary<string, string> mapping)
        //{
        //    if (!compatibilityConvertValue)
        //    {
        //        CopyPropertiesFrom(target, source, mapping);
        //        return;
        //    }

        //    if (source == null || target == null)
        //        return;

        //    Type typeSource = source.GetType();
        //    Type typeTarget = target.GetType();

        //    PropertyInfo[] listSourceProperty = typeSource.GetProperties();
        //    PropertyInfo[] listTargetProperty = typeTarget.GetProperties();

        //    foreach (PropertyInfo piSource in listSourceProperty)
        //    {
        //        if (!mapping.ContainsKey(piSource.Name))
        //            continue;
        //        if (!piSource.CanRead)
        //            continue;

        //        var pms = piSource.GetIndexParameters();
        //        if (pms != null && pms.Length > 0)
        //            continue;

        //        string nameTarget = mapping[piSource.Name];
        //        object val = piSource.GetValue(source, null);
        //        foreach (PropertyInfo piTarget in listTargetProperty)
        //            if (piTarget.Name == nameTarget &&
        //                piTarget.CanWrite)
        //            {
        //                piTarget.SetValue(target, DotNetTypeConverter.Instance.To(val, piTarget.PropertyType), null);
        //                break;
        //            }
        //    }
        //}

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

        //public static void TraversalMethodsInfo(this object source, Func<MethodInfo, bool> method)
        //{
        //    TypeExtension.TraversalMethodsInfo(source.GetType(), method);
        //}

        public static void TraversalFieldsInfo(this object source, Func<FieldInfo, object, bool> method)
        {
            if (method == null || source == null)
                return;

            FieldInfo[] listFieldInfo = source.GetType().GetFields();

            foreach (FieldInfo fi in listFieldInfo)
            {
                object val = fi.GetValue(source);
                if (!method(fi, val))
                    return;
            }
        }

        public static void TraversalPropertiesInfo(this object source, Func<string, object, bool> method)
        {
            if (method == null || source == null)
                return;

            PropertyInfo[] listPropertyInfo = source.GetType().GetProperties();

            foreach (PropertyInfo pi in listPropertyInfo)
            {
                if (!pi.CanRead)
                    continue;

                object val = pi.GetValue(source, null);
                if (!method(pi.Name, val))
                    return;
            }
        }

        public static void TraversalPropertiesInfo(this object source, Func<PropertyInfo, object, bool> method)
        {
            if (method == null || source == null)
                return;

            PropertyInfo[] listPropertyInfo = source.GetType().
                GetProperties().OrderBy(c => c.MetadataToken).ToArray();

            foreach (PropertyInfo pi in listPropertyInfo)
            {
                if (!pi.CanRead)
                    continue;

                try
                {
                    object val = pi.GetValue(source, null);
                    if (!method(pi, val))
                        return;
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
                return;

            PropertyInfo[] listPropertyInfo = source.GetType().GetProperties();

            foreach (PropertyInfo pi in listPropertyInfo)
            {
                if (!pi.CanRead)
                    continue;

                try
                {
                    object val = pi.GetValue(source, null);
                    if (!method(pi, val, argument))
                        return;
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
            var pi = source.GetType().GetProperty(propertyName);
            if (pi == null)
            {
                throw new KeyNotFoundException(propertyName);
            }

            if (propertyValue != null)
			{
				if ((pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?)) && !(propertyValue is DateTime))
				{
					if (DateTime.TryParse(propertyValue.ToString(), out DateTime dt))
					{
						propertyValue = dt;
					}
				}
				else if ((pi.PropertyType == typeof(int) || pi.PropertyType == typeof(int?)) && !(propertyValue is int))
				{
					if (int.TryParse(propertyValue.ToString(), out int n))
						propertyValue = n;
				}
				else if ((pi.PropertyType == typeof(long) || pi.PropertyType == typeof(long?)) && !(propertyValue is long))
				{
					if (long.TryParse(propertyValue.ToString(), out long n))
						propertyValue = n;
				}
				else if ((pi.PropertyType == typeof(double) || pi.PropertyType == typeof(double?)) && !(propertyValue is double))
				{
					if (double.TryParse(propertyValue.ToString(), out double n))
						propertyValue = n;
				}
				else if ((pi.PropertyType == typeof(float) || pi.PropertyType == typeof(float?)) && !(propertyValue is float))
				{
					if (float.TryParse(propertyValue.ToString(), out float n))
						propertyValue = n;
				}
				else if ((pi.PropertyType == typeof(short) || pi.PropertyType == typeof(short?)) && !(propertyValue is short))
				{
					if (short.TryParse(propertyValue.ToString(), out short n))
						propertyValue = n;
				}
				else if ((pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(decimal?)) && !(propertyValue is decimal))
				{
					if (decimal.TryParse(propertyValue.ToString(), out decimal n))
						propertyValue = n;
				}
			}

            pi.SetValue(source, propertyValue, null);
        }

        //public static KeyValueList<string, object> DictProperties(this object source)
        //{
        //    KeyValueList<string, object> dic = new KeyValueList<string, object>();

        //    PropertyInfo[] pis = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    foreach (var pi in pis)
        //        dic[pi.Name] = pi.GetValue(source, null);

        //    return dic;
        //}

        public static Dictionary<string, object> DictPropertiesToDictionary(this object source)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            PropertyInfo[] pis = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var pi in pis)
                dic[pi.Name] = pi.GetValue(source, null);

            return dic;
        }

        //public static KeyValueList<string, object> DictPropertiesEnumToInt(this object source)
        //{
        //    var dic = source.DictProperties();
        //    foreach (var item in dic)
        //    {
        //        if (item.Value == null)
        //            continue;

        //        Type t = item.Value.GetType();
        //        if (t.IsEnum)
        //            item.Value = Convert.ToInt32(item.Value);
        //    }
        //    return dic;
        //}

        #endregion

        #endregion
    }
}
