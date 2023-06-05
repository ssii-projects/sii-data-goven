using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.AST;
using Agro.Library.Common;
using SkiaMap;

namespace ConsoleApp1
{
    internal class SketchMapExportTest
    {

        public static void Test()
        {
            var expr = "[SCMJM] \\ [DKBM]";
            var parser = new SimpleAstLabelExpress();
            var node=parser.BuildAST(expr);
            Console.WriteLine(node);

            GisGlobal.SymbolFactory = SkiaSymbolFactory.Instance;

            CloneTest();

            using var fws=SqliteFeatureWorkspaceFactory.Instance.OpenWorkspace("./MHK.db");
            using var pl = new PageLayout(new SkiaDisplay());
            pl.OpenDocument(@"./地块四至示意图.kpd", false);

            ConnectSource(pl,fws);

            pl.SaveToImage("./ttx.jpg", 300, i => { }, NotCancelTracker.Instance);
        }
        private static void ConnectSource(PageLayout pl,IFeatureWorkspace fws)
        {
            #region 连接数据源
            var map = pl.FocusMap;
            //map.SetSpatialReference(_spatialReference, false);
            var layer1 = map.Layers.GetLayer(0) as IFeatureLayer;
            var layer2 = map.Layers.GetLayer(1) as IFeatureLayer;
            var fc=fws.OpenFeatureClass("DLXX_DK");
            layer2.FeatureClass = fc;
            layer2.FeatureLabeler.SetLabelExpression("[name] \\ [code]");
            //layer2.Where = "rowid<100";

            map.SetSpatialReference (fc.SpatialReference,false);
            var fullEnv = fc.GetFullExtent();// new OkEnvelope(env);
            var c = fullEnv.Centre;
            var env = fullEnv;// new OkEnvelope(fullEnv.ScaleAt(0.5, c.X, c.Y)!);
            map.FullExtent = fullEnv;
            map.SetExtent(env, false);
            /*
            layer2.Where = null;
            var fc1 = MyMemorySourceUtil.CreateAreaFeatureClass("fc1");
            var fc2 = MyMemorySourceUtil.CreateAreaFeatureClass("fc2");
            layer1.FeatureClass = fc1;
            layer2.FeatureClass = fc2;
            Envelope env = null;
            foreach (var en in lands)
            {
                var ft = fc1.CreateFeature();
                ft.Shape = en.Shape.Geometry;
                if (env == null)
                {
                    env = ft.Shape.EnvelopeInternal;
                }
                else
                {
                    env.ExpandToInclude(ft.Shape.EnvelopeInternal);
                }
                fc1.Append(ft);
            }
            var fullEnv = new OkEnvelope(env);
            map.FullExtent = fullEnv;
            map.SetExtent(fullEnv, false);

            foreach (var land in concord.Lands)
            {
                var localLand = lands.Find(ld => ld.DKBM == land.DKBM);
                if (localLand == null)
                {
                    continue;
                }
                var ft = fc2.CreateFeature();
                ft.Shape = localLand.Shape.Geometry;
                fc2.Append(ft);
            }*/

            #endregion
        }

        private static void CloneTest()
        {
            {
                var symbol = GisGlobal.SymbolFactory.CreateSimpleLineSymbol();
                var t = symbol.Clone();
                Console.WriteLine(t);
            }
            {
                var symbol = GisGlobal.SymbolFactory.CreateCharacterMarkerSymbol();
                symbol.CharacterIndex = 117;
                var t = symbol.Clone();
                Console.WriteLine(t);
            }
        }
    }

}
