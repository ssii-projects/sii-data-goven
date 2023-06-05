using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore.CRS
{
    internal class AuthorityCodeHandler
    {
        public Dictionary<int, string> ReadDefault()
        {
            using var str = DeflateStreamReader.DecodeEmbeddedResource("Agro.LibCore.Geo.CRS.epsg.ds");
            return ReadFromStream(str, "EPSG");
        }
        private Dictionary<int, string> ReadFromStream(Stream s, string authority= "EPSG")
        {
            var dic = new Dictionary<int, string>();
            using var sr = new StreamReader(s);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;
                if (!line.StartsWith("<") || !line.EndsWith("<>")) continue;

                var endAuthorityCode = line.IndexOf('>', 1);
                var authorityCode = line.Substring(1, endAuthorityCode - 1);
                var projString = line.Substring(endAuthorityCode + 1, line.Length - 2 - (endAuthorityCode + 1)).Trim();

                var srid = SafeConvertAux.ToInt32(authorityCode);
                dic[srid] = projString;
                //Add(string.Format("{0}:{1}", authority, authorityCode), string.Empty, projString, false);
            }
            return dic;
        }
    }
}
