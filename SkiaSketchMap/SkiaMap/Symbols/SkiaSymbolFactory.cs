

using Agro.GIS;
using SkiaMap.Symbol;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace SkiaMap
{
    public class SkiaSymbolFactory : ISymbolFactory
    {
        internal readonly static ConcurrentDictionary<string, SKTypeface> dicTypeFace = new();
        private SkiaSymbolFactory() { }
        public static SkiaSymbolFactory Instance { get; } = new SkiaSymbolFactory();

        public static void RegisterFont(Stream stream, string? customName = null)
        {
            using var fontData = SKData.Create(stream);
            foreach (int item in Enumerable.Range(0, 256))
            {
                SKTypeface sKTypeface = SKTypeface.FromData(fontData, item);
                if (sKTypeface == null)
                {
                    break;
                }

                string key = customName ?? sKTypeface.FamilyName;
                dicTypeFace[key] = sKTypeface;
            }
        }

        public static void RegisterFontFromEmbeddedResource(string pathName)
        {
            using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(pathName);
            if (stream == null)
            {
                throw new ArgumentException("Cannot load font file from an embedded resource. Please make sure that the resource is available or the path is correct: " + pathName);
            }

            RegisterFont(stream);
        }

        #region ISymbolFactory
        public ILineSymbol CreateSimpleLineSymbol()
        {
            return new SimpleLineSymbol();
        }
        public IFillSymbol CreateSimpleFillSymbol()
        {
            return new SimpleFillSymbol();
        }
        public ITextSymbol CreateTextSymbol()
        {
            return new TextSymbol();
        }
        public ICharacterMarkerSymbol CreateCharacterMarkerSymbol()
        {
            return new CharacterMarkerSymbol();
        }
        public IMarkerSymbol CreateSimpleMarkerSymbol()
        {
            return new SimpleMarkerSymbol();
        }
        public ISymbol? CreateSymbol(string typeName)
        {
            var symbolNamespace = "SkiaMap.Symbol";
            //return SerializeUtil.CreateInstance<ISymbol>("SkiaMap.Symbol", typeName);
            var type = System.Type.GetType(symbolNamespace + "." + typeName);
            if (type == null)
                return null;// default(T);
            try
            {
                var t = Activator.CreateInstance(type);//name.FullName,"DxComm.GIS."+ typeName) ;
                return t as ISymbol;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Assert(false, ex.Message);
                throw ex;
            }
        }
        #endregion
    }
}
