using Agro.GIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
    public class IRowUtil
    {
        public static int CopyValues(IRow from, IRow to, bool fCopyShapeField = true)
        {
            int num = 0;
            IFields fields = to.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.GetField(i);
                if (!field.Editable)
                {
                    continue;
                }

                string fieldName = field.FieldName;
                int num2 = from.Fields.FindField(fieldName);
                if (num2 >= 0)
                {
                    IField field2 = from.Fields.GetField(num2);
                    if (IsSameField(field2, field) && (field.FieldType != eFieldType.eFieldTypeGeometry || fCopyShapeField))
                    {
                        object value = from.GetValue(num2);
                        to.SetValue(i, value);
                        num++;
                    }
                }
            }

            return num;
        }

        public static void SameRowCopyValues(IRow from, IRow to)
        {
            IFields fields = to.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.GetField(i);
                if (field.Editable)
                {
                    object value = from.GetValue(i);
                    to.SetValue(i, value);
                }
            }
        }

        public static bool IsSameField(IField fa, IField fb)
        {
            if (fa.FieldType != fb.FieldType)
            {
                return false;
            }

            if (fa.FieldType == eFieldType.eFieldTypeGeometry)
            {
                return fa.GeometryType == fb.GeometryType;
            }

            if ((fa.FieldType == eFieldType.eFieldTypeDouble || fa.FieldType == eFieldType.eFieldTypeSingle) && (fa.Length != fb.Length || fa.Scale != fb.Scale))
            {
                return false;
            }

            if (fa.FieldName != fb.FieldName)
            {
                return false;
            }

            return true;
        }

        public static object GetRowValue(IRow row, string fieldName)
        {
            int num = row.Fields.FindField(fieldName);
            if (num >= 0)
            {
                return row.GetValue(num);
            }

            return null;
        }

        public static void SetRowValue(IRow row, string fieldName, object? val)
        {
            int num = row.Fields.FindField(fieldName);
            if (num >= 0)
            {
                row.SetValue(num, val);
            }
        }

        public static void SetRowValueIfNull(IRow ft, string fieldName, object? value)
        {
            if (GetRowValue(ft, fieldName) == null)
            {
                SetRowValue(ft, fieldName, value);
            }
        }
    }
}
