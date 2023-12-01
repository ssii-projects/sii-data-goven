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

    public class PostDkxx
    {
        public string Dkbm { get; set; } = string.Empty;
        public string Bdcdyh { get; set; } = string.Empty;
        public string Dkmc { get; set; } = string.Empty;
        public string Dklb { get; set; } = string.Empty;
        public string Tdlylx { get; set; } = string.Empty;
        public string Dldj { get; set; } = string.Empty;
        public string Tdyt { get; set; } = string.Empty;
        public string Sfjbnt { get; set; } = string.Empty;
        public string Dkdz { get; set; } = string.Empty;
        public string Dknz { get; set; } = string.Empty;
        public string Dkxz { get; set; } = string.Empty;
        public string Dkbz { get; set; } = string.Empty;
        public string Dkbzxx { get; set; } = string.Empty;
        public string Cbfmc { get; set; } = string.Empty;
        public decimal Scmj { get; set; }
        public decimal? Scmjm { get; set; } = decimal.Zero;
        public decimal? Elhtmj { get; set; } = decimal.Zero;
        public decimal? Jddmj { get; set; } = decimal.Zero;
        public string Fbfbm { get; set; } = string.Empty;
        public string Syqxz { get; set; } = string.Empty;
        public string Zjrxm { get; set; } = string.Empty;
        public string Bglx { get; set; } = string.Empty;
        public string Bgyy { get; set; } = string.Empty;
        public string Shp { get; set; } = string.Empty;
        public string Tyzb { get; set; } = string.Empty;

        public PostDkxx(VEC_SURVEY_DK v, int srid, WKTWriter wktWriter)
        {
            Dkbm = v.DKBM;
            Bdcdyh = v.BDCDYH;
            Dkmc = v.DKMC;
            Dklb = v.DKLB;
            Tdlylx = v.TDLYLX;
            Dldj = v.DLDJ;
            Tdyt = v.TDYT;
            Sfjbnt = v.SFJBNT;
            Dkdz = v.DKDZ;
            Dknz = v.DKNZ;
            Dkxz = v.DKXZ;
            Dkbz = v.DKBZ;
            Dkbzxx = v.DKBZXX;
            Cbfmc = v.CBFMC;
            Scmj = v.SCMJ;
            Scmjm = v.SCMJM;
            Elhtmj = v.ELHTMJ;
            Jddmj = v.JDDMJ;
            Fbfbm = v.FBFDM;
            Syqxz = v.SYQXZ;
            Zjrxm = v.ZJRXM;
            Bglx = v.BGLX;
            Bgyy = v.BGYY;
            Shp = wktWriter.Write(v.Shape);
            Tyzb = srid.ToString();
        }
    }
    public class PostDkxxs
    {
        public PostDkxx[]? Dkxx { get; set; }
    }
}
