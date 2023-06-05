namespace Agro.LibCore
{
    public static class ObjectArrayExtension
    {
        #region Methods

        public static string[] ToStringArray(this object[] source)
        {
            if (source == null)
                return null;

            List<string> list = new List<string>();

            for (int i = 0; i < source.Length; i++)
            {
                object param = source[i];
                if (param != null)
                    list.Add(param.ToString());
                else
                    list.Add(null);
            }

            return list.ToArray();

        }

        //public static string ToDeepString(this object[] source)
        //{
        //    StringBuilder b = new StringBuilder();
        //    ToDeepString(b, source);

        //    return b.ToString();
        //}

        //private static void ToDeepString(StringBuilder b, IEnumerable args)
        //{
        //    b.Append("(");

        //    foreach (object arg in args)
        //    {
        //        b.Append("(");

        //        if (arg == null)
        //            b.Append("NULL");
        //        else if (arg is IEnumerable && !(arg is string))
        //            ToDeepString(b, arg as IEnumerable);
        //        else if (arg is Expression)
        //            b.Append(Evaluator.PartialEval(arg as Expression).ToString());
        //        else
        //            b.Append(arg);

        //        b.Append(")");
        //    }

        //    b.Append(")");
        //}

        #endregion
    }
}
