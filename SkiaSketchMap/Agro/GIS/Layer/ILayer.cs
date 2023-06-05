using Agro.LibCore;
/*
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   ILayer
 * 创 建 人：   颜学铭
 * 创建时间：   2017/3/10 10:38:17
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Xml;

namespace Agro.GIS
{
    public enum ELayerType
    {
        ShapeFile,
        Spatialite,
        SQLite,
        RdbTileLayer,
        FileRasterLayer
    }
    public interface IDatasourceError{
        Exception DatasourceException { get;}
    }
    public interface ILayer
    {
        string Name { get; set; }
        string Description { get; set; }
        bool Visible { get; set; }
		bool TocCheckBoxVisible { get; set; }
        double? MinVisibleScale { get; set; }
        double? MaxVisibleScale { get; set; }
		bool IsBusy { get; set; }
		/// <summary>
		/// 透明度0-255
		/// </summary>
		int? Alpha { get; set; }
        Map? Map { get; set; }
        OkEnvelope? FullExtent { get; }
        IRenderer? Renderer { get; }
        SpatialReference? SpatialReference { get; }
        object? Tag { get; set; }
        /// <summary>
        /// 图层比例是否可见
        /// </summary>
        /// <param name="mapScale"></param>
        /// <returns></returns>
        bool IsScaleVisible(double mapScale);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="drawFeatureCallback"></param>
        /// <returns>所有绘制项都完成了则返回true</returns>
        bool Render(LayerRenderMeta meta, ICancelTracker cancel, Action drawCallback);// Func<IFeature, bool> drawFeatureCallback = null);
        void CancelTask();

        void ReadXml(XmlElement reader, SerializeSession session);
        void ReadXml(XmlReader reader, SerializeSession session);//string oldMapDocumentPath,string newMapDocumentPath);
        void WriteXml(XmlWriter writer, bool fWriteDataSource = true);
    }

    public interface IRasterLayer : ILayer, IDatasourceError
    {
        string ConnectionString { get; }
        void Load(string fileName);
    }

	internal interface IBusyChanged
	{
		Action<object, bool> OnBusyChanged { get; set; }
	}

    public class GroupLayer : LayerCollection, ILayer
	{
        public GroupLayer()
            : this(null, null)
        {
        }

		public GroupLayer(Map map, string name)
            : base(map)
        {
            Name = name;
            Visible = true;
        }

        public override SpatialReference SpatialReference
        {
            get
            {
                SpatialReference sr = null;
                foreach (var layer in _layers)
                {
                    sr = layer.SpatialReference;
                    if (sr != null)
                    {
                        break;
                    }
                }
                return sr;
            }
        }

        public override void ReadXml(XmlElement reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
        {
            foreach(XmlNode n in reader.ChildNodes)
            {
                var s = n.InnerText;
                switch (n.Name)
                {
                    case "Name":
                        this.Name = s;
                        break;
                    case "Description":
                        this.Description = s;
                        break;
                    case "Visible":
                        this.Visible = s== "True";
                        break;
                    case "IsExpanded":
                        this.IsExpanded = s== "True";
                        break;
                    case "Layers":
                        SerializeUtil.ReadLayers(reader, Map, layer =>
                        {
                            base.AddLayer(layer);
                        }, session);// oldMapDocumentPath,newMapDocumentPath);
                        break;
                }
            }
        }


      //  #region IXmlSerializable



      //  public override void ReadXml(System.Xml.XmlReader reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
      //  {
      //      SerializeUtil.ReadNodeCallback(reader, this.GetType().Name, name =>
      //      {
      //          switch (name)
      //          {
      //              case "Name":
      //                  this.Name = reader.ReadString();
      //                  break;
      //              case "Description":
      //                  this.Description = reader.ReadString();
      //                  break;
      //              case "Visible":
      //                  this.Visible = reader.ReadString() == "True";
      //                  break;
      //              case "IsExpanded":
      //                  this.IsExpanded = reader.ReadString() == "True";
      //                  break;
      //              case "Layers":
						//SerializeUtil.ReadLayers(reader, Map, name, layer =>
						//{
						//	base.AddLayer(layer);
						//}, session);// oldMapDocumentPath,newMapDocumentPath);
      //                  break;
      //          }
      //      });
      //  }

      //  public override void WriteXml(System.Xml.XmlWriter writer, bool fWriteDataSource = true)
      //  {
      //      writer.WriteStartElement("GroupLayer");
      //      SerializeUtil.WriteStringElement(writer, "Name", Name);
      //      if (!string.IsNullOrEmpty(Description))
      //      {
      //          SerializeUtil.WriteStringElement(writer, "Description", Description);
      //      }
      //      if (!Visible)
      //      {
      //          SerializeUtil.WriteStringElement(writer, "Visible", Visible.ToString());
      //      }
      //      if (this.IsExpanded)
      //      {
      //          SerializeUtil.WriteStringElement(writer, "IsExpanded", this.IsExpanded.ToString());
      //      }
      //      if (MinVisibleScale != null)
      //      {
      //          SerializeUtil.WriteDoubleElement(writer, "MinimizeScale", (double)MinVisibleScale);
      //      }
      //      if (MaxVisibleScale != null)
      //      {
      //          SerializeUtil.WriteDoubleElement(writer, "MaximizeScale>", (double)MaxVisibleScale);
      //      }
      //      if (Alpha != null)
      //      {
      //          SerializeUtil.WriteIntElement(writer, "Alpha", (int)Alpha);
      //      }
      //      writer.WriteStartElement("Layers");
      //      foreach (var layer in _layers)
      //      {
      //          layer.WriteXml(writer, fWriteDataSource);
      //          //var x = layer as IXmlSerializable;
      //          //if (x != null)
      //          //{
      //          //    x.WriteXml(writer);
      //          //}
      //      }
      //      writer.WriteEndElement();
      //      writer.WriteEndElement();
      //  }
      //  #endregion
    }


}
