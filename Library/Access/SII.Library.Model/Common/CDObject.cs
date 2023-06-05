using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;
using Agro.LibCore;

namespace Agro.Library.Model
{
    public interface ICD : ICloneable, IDisposable
    {
    }

    [Serializable]
    //[DataContract(XNamespace = "")]
    public abstract class CDObject : ICD
    {
        #region Methods

        #region Methods - Static

        public static object TryClone(object obj)
        {
            ICloneable ic = obj as ICloneable;
            if (ic != null)
                return ic.Clone();

            IList list = obj as IList;
            if (list != null)
                return list.Clone();

            return obj;
        }

        #endregion

        #region Methods - Virtual

        public virtual object Clone()
        {
            object newObj = MemberwiseClone();

            newObj.TraversalPropertiesInfo(ClonePropertyHandler, newObj);

            return newObj;
        }

        public virtual void Dispose()
        {
            //this.TraversalPropertiesInfo(DisposePropertyHandler);
        }

        #endregion

        #region Methods - Private

        private bool ClonePropertyHandler(PropertyInfo pi, object value, object target)
        {
            if (!pi.CanWrite)
                return true;

            pi.SetValue(target, CDObject.TryClone(value), null);

            return true;
        }

        private bool DisposePropertyHandler(string name, object value)
        {
            IDisposable id = value as IDisposable;
            if (id == null)
                return true;

            id.Dispose();

            return true;
        }

        #endregion

        #endregion
    }
}
