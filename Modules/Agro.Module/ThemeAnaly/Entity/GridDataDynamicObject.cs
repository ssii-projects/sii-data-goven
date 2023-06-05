using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.Entity
{
    public class GridDataDynamicObject: DynamicObject
    {
        internal readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!_properties.Keys.Contains(binder.Name))
            {
                _properties.Add(binder.Name, value.ToString());
            }
            return true;
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _properties.TryGetValue(binder.Name, out result);
        }
    }
}
