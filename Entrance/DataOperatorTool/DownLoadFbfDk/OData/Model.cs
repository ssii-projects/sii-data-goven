using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.Library.Model;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataOperatorTool.OData
{
    public class ValueModel<T>
    {
        public T[]? Value { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string? NextLink { get; set; }
    }
    public class LoginModel
    {
        public string Token { get; set; }
        public string ExpireAt { get; set; }
    }
    public class ZonesModel :ValueModel<DLXX_XZDY>
    {
    }
    public class LandModel: SURVEY_DKBase<LandModel>
    {
        public string? FBFBM { get; set; }
        public string? Shape { get; set; }

        public DLXX_DK ToDkEn()
        {
            var en = new DLXX_DK();
            //{
            //    DKBM=this.DKBM,
            //    DKMC=this.DKMC,
            //    DKLB=this.DKLB,
            //    TDLYLX = this.TDLYLX,
            //    DLDJ = this.DLDJ,

            //};
            var lst=EntityUtil.GetAttributes<DLXX_DK>(false);
            foreach (var it in lst)
            {
                if (this.HasProperty(it.PropertyName))
                {
                    var o = this.GetPropertyValue(it.PropertyName, false, false);
                    en.SetPropertyValue(it.PropertyName, o);
                }
            }
            en.FBFBM = this.FBFBM;

            var sa = Shape?.Split(';');
            if (sa?.Length > 0)
            {
                var g=new WKTReader().Read(sa[1]);
                en.Shape = g;
            }
            return en;
        }
    }
    public class LandsModel:ValueModel<LandModel>
    { }

    public class DkbmAndCbfBm
    {
        public string DKBM { get; set; } = string.Empty;
        public string? CBFBM { get; set; }
    }
}
