using Agro.LibCore;
using Agro.LibCore.Xml;
using GeoAPI.Geometries;
/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   ILabeler
 * 创 建 人：   颜学铭
 * 创建时间：   2016/12/1 10:02:06
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Agro.GIS
{
    /// <summary>
    /// 标注相对位置
    /// </summary>
    public enum ELabelPos
    {
        Center,
        Left,
        RIght,
        Top,
        Bottom,
        LeftTop,
        RightTop,
        LeftBottom,
        RIghtBottom,
    }
    //public interface IPropertyPage
    //{
    //    object Buddy { get; set; }
    //}
    /// <summary>
    /// 要素标注接口
    /// </summary>
    public interface IFeatureLabeler :IXmlSerializable
    {
        void SetTotalLabelExtends(List<IPolygon> listTotalLabelExtends);
        /// <summary>
        /// 是否允许标注
        /// </summary>
        bool EnableLabel { get; set; }
        string LabelExpression { get;  }
        void SetLabelExpression(string expr);
		ITextSymbol TextSymbol { get; set; }
		///// <summary>
		///// 属性页，用于提供标注设置界面，通常为WPF UserPanel
		///// </summary>
		//IPropertyPage PropertyPage { get; set; }
		void PrepareFilter(ISpatialFilter filter);
        void SetupDC(IDisplay dc,IDisplayTransformation trans);
        void Draw(IFeature feature);//, List<IPolygon> listTotalLabelExtends);
        void ResetDC();
    }
    public abstract class FeatureLabeler
    {
        //protected System.Drawing.Graphics _dc;
        protected IDisplay? _dc;
        protected IDisplayTransformation? _trans;// { get { return _dc?.DisplayTransformation; } }
        #region IFeatureLabeler
        public bool EnableLabel
        {
            get;
            set;
        }

        //public void PrepareFilter(ISpatialFilter filter)
        //{
        //}

        public void SetupDC(IDisplay dc,IDisplayTransformation trans)
        {
            _dc = dc;
            _trans = trans;
        }

        //public void Draw(IFeature g, List<IPolygon> listTotalLabelExtends)
        //{
        //}

        public void ResetDC()
        {
            _dc = null;
            //_trans = null;
        }
        #endregion

        public void ReadXml(XmlElement reader)
        {
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    switch (e.Name)
                    {
                        case "EnableLabel":
                            EnableLabel = e.InnerText == "True";
                            break;
                        default:
                            ReadXmlElement(e);
                            break;
                    }
                }
            }
        }

        protected virtual void ReadXmlElement(XmlElement e)
        {
            System.Diagnostics.Debug.Assert(false, $"not process Property ${e.Name}");
        }
    }

    public class FieldLabelValue
    {
        public IFeatureLayer FeatureLayer;
        public string FieldName;
        public object FieldValue;
    }
}
