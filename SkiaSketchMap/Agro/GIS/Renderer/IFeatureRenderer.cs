using Agro.LibCore;
using Agro.LibCore.Xml;
using GeoAPI.Geometries;
using System.Xml;

namespace Agro.GIS
{
    public class LegendClass
    {
        private ISymbol? _symbol;
        private string _label = "";
        private LegendInfo? _legendInfo;
        public string Description { get; set; } = "";
        //ILegendClassFormat Format { get; set; }
       public string Label { get { return _label; }
           set
           {
               _label = value;
               fireLegendChanged();
           }
       }
       public ISymbol? Symbol { 
           get {
                return _symbol;//.Clone() as ISymbol; 
           }
           set
           {
               _symbol = value;
               fireLegendChanged();
           }
       }
       internal void SetLegendInfo(LegendInfo? li)
       {
           _legendInfo = li;
       }
        internal void fireLegendChanged()
        {
            _legendInfo?.FireLegendChanged();
        }
    }
    public class LegendGroup
    {
        private readonly List<LegendClass> LegendClasses = new();
        private LegendInfo? _legendInfo;
        public string? Heading { get; set; }
        public bool Visible { get; set; }
        public int ClassCount
        {
            get
            {
                return LegendClasses.Count;
            }
        }
        public void AddClass(LegendClass legendClass)
        {
            legendClass.SetLegendInfo(_legendInfo);
            LegendClasses.Add(legendClass);
        }
        public void ClearClasses()
        {
            LegendClasses.Clear();
            fireLegendChanged();
        }
        public LegendClass? GetClass(int Index)
        {
            if (Index >= 0 && Index < ClassCount)
            {
                return LegendClasses[Index];
            }
            return null;
        }
        public void InsertClass(int Index, LegendClass LegendClass)
        {
            LegendClasses.Insert(Index, LegendClass);
            fireLegendChanged();
        }
        public void RemoveClass(int Index)
        {
            if (Index >= 0 && Index < ClassCount)
            {
                LegendClasses.RemoveAt(Index);
                fireLegendChanged();
            }
        }
        internal void SetLegendInfo(LegendInfo li)
        {
            _legendInfo = li;
            foreach (var lc in LegendClasses)
            {
                lc.SetLegendInfo(li);
            }
        }
        internal void fireLegendChanged()
        {
            if (_legendInfo != null)
            {
                _legendInfo.FireLegendChanged();
            }
        }
    }
    public class LegendInfo
    {
        private readonly List<LegendGroup> LegendGroups = new();

        #region events
        public Action? OnLegendInfoChanged;
        #endregion

        public void Add(LegendGroup lg)
        {
            lg.SetLegendInfo(this);
            LegendGroups.Add(lg);
            FireLegendChanged();
        }
        public int Count
        {
            get { return LegendGroups.Count; }
        }
        public LegendGroup? Get(int i)
        {
            if (i >= 0 && i < LegendGroups.Count)
            {
                return LegendGroups[i];
            }
            return null;
        }

		internal void FireLegendChanged()
		{
			OnLegendInfoChanged?.Invoke();
		}
    }
    public interface IRenderer
    {
        LegendInfo LegendInfo { get; }
    }
    public interface IFeatureRenderer : IRenderer
    {
        void PrepareFilter(IFeatureClass fc, IQueryFilter queryFilter);
        void SetupDC(IDisplay dc,IDisplayTransformation trans);
        void Draw(IFeature g);
        void ResetDC();
        void ReadXml(XmlElement reader);
    }
    public class SimpleFeatureRenderer : IFeatureRenderer, IXmlSerializable
    {
        private readonly LegendInfo _legendInfo = new();
        private LegendClass? _legendClass => _legendInfo.Get(0)!.GetClass(0);
        private ISymbol? _pSymbol;
        public Action<IGeometry>? OnPreDraw;
        //{
        //    get
        //    {
        //        return _legendClass.Symbol;
        //    }
        //}
        public SimpleFeatureRenderer()
        {
            var lg=new LegendGroup();
            lg.AddClass(new LegendClass());
            _legendInfo.Add(lg);
        }
        public void SetSymbol(ISymbol pSymbol)
        {
            if (_legendClass != null)
            {
                _legendClass.Symbol = pSymbol;
            }
        }
        public void PrepareFilter(IFeatureClass fc, IQueryFilter queryFilter)
        {
        }
		public void SetupDC(IDisplay dc,IDisplayTransformation trans)//System.Drawing.Graphics dc, IDisplayTransformation trans)
		{
			_pSymbol = _legendClass?.Symbol;
            _pSymbol?.SetupDC(dc, trans);
		}
        public void Draw(IFeature feature)
        {
            if (_pSymbol != null)
            {
                OnPreDraw?.Invoke(feature.Shape);
                _pSymbol.Draw(feature.Shape);
            }
        }
        public void ResetDC()
        {
            if (_pSymbol != null)
            {
                _pSymbol.ResetDC();
                _pSymbol = null;
            }
        }
        public LegendInfo LegendInfo
        {
            get { return _legendInfo; }
        }

        public void ReadXml(XmlElement reader)
        {
            foreach(var o in reader.ChildNodes)
            {
                if (o is XmlElement e) {
                    var name = e.Name;
                    if (name == "Symbol")
                    {
                        var symbol = SerializeUtil.ReadSymbol(e);
                        if (symbol != null)
                        {
                            SetSymbol(symbol);
                        }
                    }
                }
            }
        }

        //public void WriteXml(XmlElement writer)
        //{
        //    throw new NotImplementedException();
        //}

        //#region IXmlSerializable
        //public System.Xml.Schema.XmlSchema? GetSchema()
        //{
        //    return null;
        //}

        //public void ReadXml(System.Xml.XmlReader reader)
        //{
        //    SerializeUtil.ReadNodeCallback(reader, "Renderer", name =>
        //    {
        //        if (name == "Symbol")
        //        {
        //            var symbol=SerializeUtil.ReadSymbol(reader, name);
        //            if (symbol != null)
        //            {
        //                SetSymbol(symbol);
        //            }
        //        }
        //    });
        //}

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Renderer");
            writer.WriteAttributeString("type", "SimpleFeatureRenderer");
            if (_legendClass?.Symbol != null)
            {
                SerializeUtil.WriteSymbol(writer, "Symbol", _legendClass.Symbol);
            }
            writer.WriteEndElement();
        }
        //#endregion
    }

   // public interface IUniqueValueRenderer:IFeatureRenderer
   // {
   //     int FieldCount { get;}
   //     //void SetField(int Index, string Field);
   //     //string GetField(int Index);
   //     void AddField(string fieldName);
   //     void AddValue(string Value, string Heading, ISymbol Symbol);
   //     ISymbol DefaultSymbol { get; set; }
   //     bool UseDefaultSymbol { get; set; }
   // }
   // public class UniqueValueRenderer : IUniqueValueRenderer
   // {
   //     class ValueItem
   //     {
   //         //public string Heading;
   //         public string Value;
   //         //public ISymbol Symbol;
   //         public LegendClass LegendClass;
   //         public ValueItem(string v, LegendClass s)
   //         {
   //             //Heading = h;
   //             Value = v;
   //             //Symbol = s;
   //             LegendClass = s;
   //         }
   //     }
   //     private List<string> _fields = new List<string>();
   //     private List<ValueItem> _values = new List<ValueItem>();
   //     private readonly LegendInfo _legendInfo = new LegendInfo();
   //     private LegendGroup _legendGroup = new LegendGroup();
   //     private Dictionary<string, LegendClass> _dicValueSymbol = new Dictionary<string, LegendClass>();
   //     private LegendClass _defaultLegendClass = new LegendClass();
   //     private bool _fUseDefaultSymbol;
   //     private int _startFieldIndex = -1;
   //     public UniqueValueRenderer()
   //     {
   //         _defaultLegendClass.Label = "默认";
   //         _legendInfo.Add(_legendGroup);
   //     }

   //     public int FieldCount
   //     {
   //         get
   //         {
   //             return _fields.Count;
   //         }
   //     }
   //     public void AddField(string fieldName)
   //     {
   //         _fields.Add(fieldName);
   //     }
   //     public void AddValue(string Value, string Heading, ISymbol Symbol)
   //     {
			//var lc = new LegendClass()
			//{
			//	Label = Heading,
			//	Symbol = Symbol
			//};
   //         _legendGroup.AddClass(lc);
   //         _values.Add(new ValueItem(Value,lc));
   //         _dicValueSymbol[Value] = lc;
   //     }

   //     public ISymbol DefaultSymbol
   //     {
   //         get
   //         {
   //             return _defaultLegendClass.Symbol;
   //         }
   //         set
   //         {
   //             _defaultLegendClass.Symbol = value;
   //         }
   //     }
   //     public bool UseDefaultSymbol { get { return _fUseDefaultSymbol; }
   //         set
   //         {
   //             _fUseDefaultSymbol = value;
   //             if (value)
   //             {
   //                 if (_legendGroup.GetClass(0) != _defaultLegendClass)
   //                 {
   //                     _legendGroup.InsertClass(0, _defaultLegendClass);
   //                 }
   //             }else
   //             {
   //                 if (_legendGroup.GetClass(0) == _defaultLegendClass)
   //                 {
   //                     _legendGroup.RemoveClass(0);
   //                 }
   //             }
   //         }
   //     }
   //     #region IFeatureRenderer
   //     public LegendInfo LegendInfo
   //     {
   //         get
   //         {
   //             return _legendInfo;
   //         }
   //     }

   //     public void PrepareFilter(IFeatureClass fc, IQueryFilter queryFilter)
   //     {
   //         var str = queryFilter.SubFields;
   //         _startFieldIndex = str.Split(',').Length;
            
   //         foreach (var s in _fields)
   //         {
   //             str += "," + s;
   //         }
   //         queryFilter.SubFields = str;
   //     }
   //     public void SetupDC(IDisplay dc)//Graphics dc, IDisplayTransformation trans)
   //     {
   //         for(int i = 0; i < _legendGroup.ClassCount; ++i)
   //         {
   //             var lc=_legendGroup.GetClass(i);
   //             var pSymbol =lc?.Symbol;
   //             pSymbol?.SetupDC(dc);//, trans);
   //         }
   //         //if (UseDefaultSymbol && DefaultSymbol != null)
   //         //{
   //         //    DefaultSymbol.SetupDC(dc, trans);
   //         //}
   //     }
   //     public void Draw(IFeature g)
   //     {
   //         string value = "";
   //         for(int i = 0; i < _fields.Count; ++i)
   //         {
   //             var v=g.GetValue(i + _startFieldIndex);
   //             var s = v == null ? "" : v.ToString();
   //             if (value == null)
   //             {
   //                 value = s;
   //             }else
   //             {
   //                 value += "," + s;
   //             }
   //         }
            
   //         if(_dicValueSymbol.TryGetValue(value,out var lc))
   //         {
   //             if (lc != null&&lc.Symbol!=null)
   //             {
   //                 lc.Symbol.Draw(g.Shape);
   //             }
   //         }else
   //         {
   //             if (UseDefaultSymbol && DefaultSymbol != null)
   //             {
   //                 DefaultSymbol.Draw(g.Shape);
   //             }
   //         }
   //     }


   //     public void ResetDC()
   //     {
			//for (int i = 0; i < _legendGroup.ClassCount; ++i)
			//{
			//	var lc = _legendGroup.GetClass(i);
			//	var pSymbol = lc?.Symbol;
			//	pSymbol?.ResetDC();
			//}
   //     }


   //     #endregion
   // }
}
