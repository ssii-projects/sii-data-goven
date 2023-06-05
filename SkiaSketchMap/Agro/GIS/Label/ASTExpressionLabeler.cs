using GeoAPI.Geometries;
using Agro.LibCore.AST;
using System.Xml;
using Agro.LibCore;

namespace Agro.GIS
{
    /// <summary>
    /// 一种基于抽象语法树实现的表达式标注
    /// yxm 2016-1-29 created
    /// </summary>
    public class ASTExpressionLabeler : FeatureLabeler, IFeatureLabeler//, IXmlSerializable
    {
        public class RowEntity : AstEntityBase
        {
            //private object _o;
            //private IGeometry _geom;
            private IRow _featrue;
            public RowEntity(IRow feature = null)
            {
                _featrue = feature;
            }
            public void SetFeature(IRow feature)
            {
                _featrue = feature;
            }

            public override object GetFieldValue(int iField)
            {
                return _featrue.GetValue(iField);
            }
            public override object GetPropertyValue(string propertyName)
            {
                if (_featrue != null)
                {
                    var iField = _featrue.Fields.FindField(propertyName);
                    return iField >= 0 ? _featrue.GetValue(iField) : null;// _o == null ? null : _o.GetPropertyValue(propertyName);
                }
                return null;
            }
            public override int FindField(string fieldName)
            {
                if (_featrue != null)
                {
                    var iField = _featrue.Fields.FindField(fieldName);
                    return iField;
                }
                return -1;
            }
            public override IGeometry GetGeometry()
            {
                return null;// _featrue == null ? null : _featrue.Shape;
            }
        }
        public class FeatureEntity : AstEntityBase
        {
            private IFeature _featrue;
            public FeatureEntity(IFeature feature = null)
            {
                _featrue = feature;
            }
            public void SetFeature(IFeature feature)
            {
                _featrue = feature;
            }
            public override object GetFieldValue(int iField)
            {
                if (_featrue != null && iField >= 0)
                {
                    return _featrue.GetValue(iField);
                }
                return null;
            }
            public override object GetPropertyValue(string propertyName)
            {
                if (_featrue != null)
                {
                    var iField = _featrue.Fields.FindField(propertyName);
                    return iField >= 0 ? _featrue.GetValue(iField) : null;// _o == null ? null : _o.GetPropertyValue(propertyName);
                }
                return null;
            }
            public override int FindField(string fieldName)
            {
                if (_featrue != null)
                {
                    var iField = _featrue.Fields.FindField(fieldName);
                    return iField;
                }
                return -1;
            }
            public override IGeometry GetGeometry()
            {
                return _featrue == null ? null : _featrue.Shape;
            }
        }

        /// <summary>
        /// 获取或设置标注使用的表达式。
        /// </summary>
        public string LabelExpression
        {
            get;
            private set;
        } = "";
        public ITextSymbol TextSymbol
        {
            get;
            set;
        } = SymbolUtil.MakeTextSymbol(14);

        /// <summary>
        /// 是否允许多个标注之间相互重叠。
        /// </summary>
        public bool AllowOverlapping { get; set; }


        #region Fields
        private ASTNode? _originAst = null;
        private readonly FeatureEntity _currentFeature = new();
        private IFeatureLayer? _featureLayer;
        private FormulaSymbol formulaSymbol = new();
        #endregion

        private readonly FieldLabelValue _fieldLabelValue = new();
        private List<IPolygon>? listTotalLabelExtends;
        /// <summary>
        /// 创建 ASTExpressionLabeler 的新实例。
        /// </summary>
        public ASTExpressionLabeler()
        {
            string calcFieldValue(string fieldName)
            {
                var o = _currentFeature.GetPropertyValue(fieldName);
                if (_featureLayer?.Map?.OnGetLabelValue != null)
                {
                    var v = _fieldLabelValue;
                    v.FeatureLayer = _featureLayer;
                    v.FieldName = fieldName;
                    v.FieldValue = o;
                    _featureLayer.Map.OnGetLabelValue(v);
                    o = v.FieldValue;
                }
                return  o?.ToString()??"";
            };


            formulaSymbol.OnCallFieldValue = calcFieldValue;
            formulaSymbol.ShouldDraw = rc =>
            {
                if (!AllowOverlapping)
                {
                    var lst = listTotalLabelExtends!;
                    var labelExtendGeometry = new Envelope(rc.left, rc.right, rc.top, rc.bottom).ToPolygon();
                    if (lst.Any(c => c.Intersects(labelExtendGeometry)))
                        return false;
                    lst.Add(labelExtendGeometry);
                }
                return true;
            };
        }
        internal void SetFeatureLayer(IFeatureLayer fl)
        {
            _featureLayer = fl;
        }

        public void SetLabelExpression(string expr)
        {
            LabelExpression = expr;
            _originAst = new SimpleAstLabelExpress().BuildAST(expr);
        }

        #region Methods - Serialize

        protected override void ReadXmlElement(XmlElement e)
        {
            switch (e.Name)
            {
                case "LabelExpression":
                    SetLabelExpression(e.InnerText);
                    break;
                case "Symbol":
                    if (SerializeUtil.ReadSymbol(e) is ITextSymbol ts)
                    {
                        TextSymbol = ts;
                    }
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, $"not process Property ${e.Name}");
                    break;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
            //writer.WriteStartElement("AstLabelExpression");
            ////writer.WriteValue(LabelProperty);
            //writer.WriteString(LabelExpression);
            //writer.WriteEndElement();
        }

        #endregion

        #region IFeatureLabeler
        public void SetTotalLabelExtends(List<IPolygon> lst)
        {
            this.listTotalLabelExtends = lst;
        }
        public void PrepareFilter(ISpatialFilter filter)
        {
            var fields = GetFields();
            if (fields == null)
            {
                return;
            }
            var ss = filter.SubFields;
            if (ss != null)
            {
                ss = "," + ss.ToUpper() + ",";
            }
            string? str = null;
            foreach (var fieldName in fields)
            {
                if (ss.Contains("," + fieldName.ToUpper() + ","))
                {
                    continue;
                }
                if (str == null)
                {
                    str = fieldName;
                }
                else
                {
                    str += "," + fieldName;
                }
            }
            if (str != null)
            {
                if (string.IsNullOrEmpty(filter.SubFields))
                {
                    filter.SubFields = str;
                }
                else
                {
                    filter.SubFields += "," + str;
                }
            }
        }
        public new void SetupDC(IDisplay dc, IDisplayTransformation trans)
        {
            base.SetupDC(dc, trans);
            formulaSymbol.SetupDC(dc, trans);
        }
        public void Draw(IFeature feature)//, List<IPolygon> listTotalLabelExtends)
        {
            if(_originAst==null) return;
            //this.listTotalLabelExtends = listTotalLabelExtends;

            _currentFeature.SetFeature(feature);
            var labelAst = SymplifyNodeHelper.Simplify(_originAst, _currentFeature);
            if (labelAst == null)
                return;
            var geo = feature.Shape;
            formulaSymbol.ASTNode=labelAst;
            if (geo is IPoint pt)
            {
                formulaSymbol.Draw(pt);
            }
            else
            {
                for (int i = 0; i < geo.NumGeometries; ++i)
                {
                    var g = geo.GetGeometryN(i);
                    if (!g.IsEmpty)
                    {
                        try
                        {
                            formulaSymbol.Draw(g.PointOnSurface);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"rowid[{feature.Oid}]:" + ex.ToString());
                        }
                    }
                }
            }
        }
        public new void ResetDC()
        {
            base.ResetDC();
            formulaSymbol.ResetDC();
        }
        #endregion

        private string[]? GetFields()
        {
            return _originAst == null ? null : ASTNodeUtil.GetFields(_originAst);
        }
    }

}
