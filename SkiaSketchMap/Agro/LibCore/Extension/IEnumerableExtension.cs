using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

namespace Agro.LibCore
{
    public static class IEnumerableExtension
    {
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            int i = 0;
            foreach(T it in source)
            {
                if (predicate(it))
                {
                    return i;
                }
                ++i;
            }
            return -1;
        }

        public static T Find<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            foreach (var it in source)
            {
                if (predicate(it))
                {
                    return it;
                }
            }
            return default(T);
        }

        public static List<T> FindAll<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            var lst = new List<T>();
           foreach(var it in source)
            {
                //var it = source[i];
                if (predicate(it)) lst.Add(it);
            }
            return lst;
        }

        /// <summary>
        /// ToList 的安全版本，如果发生异常并不会导致程序崩溃，而是返回null。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> TryToList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return null;

            try { return source.ToList(); }
            catch { return null; }
        }

        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            if (source == null)
                return true;

            bool has = source.GetEnumerator().MoveNext();
            if (!has)
                return true;

            return false;
        }

        //public static IList Clone(this IList source)
        //{
        //    if (source == null)
        //        return null;

        //    MethodInfo mi = source.GetType().GetMethod("Clone");
        //    if (mi != null)
        //        return (IList)mi.Invoke(source, null);

        //    IList list = Activator.CreateInstance(source.GetType()) as IList;
        //    MethodInfo addMethod = list.GetType().GetMethod("Add");

        //    foreach (object item in source)
        //        addMethod.Invoke(list, new object[] { CDObject.TryClone(item) });

        //    return list;
        //}

        //public static void Sort<T>(this List<T> source, string propertyPath, eOrder order)
        //{
        //    source.Sort((a, b) =>
        //    {
        //        var valA = a.GetPropertyValue(propertyPath, true, false) as IComparable;
        //        var valB = b.GetPropertyValue(propertyPath, true, false) as IComparable;

        //        var val = 0;

        //        if (valA != null && valB != null)
        //            val = valA.CompareTo(valB);
        //        else if (valA == null && valB != null)
        //            val = -1;
        //        else if (valA != null && valB == null)
        //            val = 1;
        //        else
        //            val = 0;

        //        return order == eOrder.Ascending ? val : -val;
        //    });
        //}

		public static string ToStr<T>(this IEnumerable<T> source,string separator)
		{
			return String.Join(separator, source);
		}
    }
}
