using Agro.LibCore;
using System.Xml;
using System.Xml.Serialization;

namespace Agro.GIS
{
    public class DataSourceMetaData
	{
		public string DataSourceType { get; set; }
		public string ConnectionString { get; set; }
		public string TableName { get; set; }
		public string OIDFieldName { get; set; }
		public string ShapeFieldName { get; set; }
		public static string DataSourceTypeString(eDatabaseType dbType)
		{
			switch (dbType)
			{
				//case eDatabaseType.ShapeFile:return nameof(ShapeFileFeatureClass);
				//case eDatabaseType.SqlServer:return nameof(SqlServerFeatureClass);
				//case eDatabaseType.MySql:return nameof(MySqlFeatureClass);
				//case eDatabaseType.SQLite:return nameof(SqliteFeatureClass);
				//case eDatabaseType.Spatialite:return nameof(SpatialiteFeatureClass);
				//case eDatabaseType.Oracle:return nameof(OracleFeatureClass);
				default:
					System.Diagnostics.Debug.Assert(false,$"not impl for dbType:{dbType}");
					return "";
			}
		}
		//public override string ToString()
		//{
		//	if (DataSourceType == nameof(ShapeFileFeatureClass))
		//	{
		//		return ConnectionString + TableName + ".shp";
		//	}
		//	var str = "";
		//	if (!string.IsNullOrEmpty(TableName))
		//	{
		//		str += "TableName=" + TableName;// + ";";
		//	}
		//	if (string.IsNullOrEmpty(ConnectionString))
		//	{
		//		return str;
		//	}
		//	if (str.Length > 0)
		//	{
		//		str += "\r\n";
		//	}

		//	eDatabaseType dbType = eDatabaseType.Null;
		//	switch (DataSourceType)
		//	{
		//		case nameof(MySqlFeatureClass):
		//			dbType = eDatabaseType.MySql;
		//			break;
		//		case nameof(SqlServerFeatureClass):
		//			dbType = eDatabaseType.SqlServer;
		//			break;
		//	}
		//	if (dbType != eDatabaseType.Null)
		//	{
		//		var cs = new ConnectionInfo();
		//		cs.FromConnectionString(dbType, ConnectionString);
		//		str += $"Host={cs.Host}\r\n";
		//		if (!string.IsNullOrEmpty(cs.Port)) str += $"Port={cs.Port}\r\n";
		//		str += $"Database={cs.DatabaseName}\r\n";
		//	}
		//	else
		//	{
		//		str += ConnectionString;
		//	}
		//	return str;
		//}
		internal bool CanConnect()
		{
			return this.TableName != null && this.ConnectionString != null && !string.IsNullOrEmpty(this.DataSourceType);
		}
		internal IFeatureClass Connect()
		{
            throw new NotImplementedException();
            /*
			IFeatureClass fc = null;
			string conStr = this.ConnectionString;
			if (this.TableName != null && this.ConnectionString != null)
			{
				IFeatureWorkspaceFactory wf = null;
				switch (this.DataSourceType)
				{
					case "SqlServerFeatureClass":
						wf = SQLServerFeatureWorkspaceFactory.Instance;
						break;
					case "ShapeFileFeatureClass":
						wf = ShapeFileFeatureWorkspaceFactory.Instance;
						break;
					case "SpatialiteFeatureClass":
						wf = SpatialiteFeatureWorkspaceFactory.Instance;
						break;
					case "SqliteFeatureClass":
						wf = SqliteFeatureWorkspaceFactory.Instance;
						break;
					case "OracleFeatureClass":
						wf = OracleFeatureWorkspaceFactory.Instance;
						break;
                    case "MySqlFeatureClass":
                        wf = MySqlFeatureWorkspaceFactory.Instance;
                        break;
                    default:
						System.Diagnostics.Debug.Assert(false);
						break;
				}
				if (wf != null)
				{
					fc = wf.OpenFeatureClass(conStr, this.TableName,"rb+");
					if (fc.OIDFieldName == null)
					{
						fc.OIDFieldName = OIDFieldName;
					}
					if (fc.ShapeFieldName == null)
					{
						fc.ShapeFieldName = ShapeFieldName;
					}
				}
			}
			return fc;
            */
		}
	}
    public interface IFeatureLayer : ILayer,IFeatureSelection, IDatasourceError
    {
        string? Where { get; set; }
        string OrderByClause { get; set; }
        //string ConnectionString { get; set; }
        DataSourceMetaData DataSourceMeta { get;}
        bool Selectable { get; set; }
        bool Editable { get; set; }
        IFeatureClass FeatureClass{get;set;}
        IFeatureRenderer FeatureRenderer{get;set;}
        IFeatureLabeler FeatureLabeler { get; set; }
        Action OnSelectionChanged { get; set; }
        /// <summary>
        /// [old,new]
        /// </summary>
        Action<IFeatureClass,IFeatureClass> OnSourceChanged { get; set; }
		string UseWhere();
		Func<string, string> OnBeforeUseWhere { get; set; }
	}

    //public class BitmapWrap
    //{
    //    private Bitmap bitmap;
    //    //private readonly object SyncBitmpObj = new object();
    //    public bool Create(int width, int height,bool fForceClearBackground=true)
    //    {
    //        bool fSizeChanged = false;
    //        var bm=bitmap;
    //        //UseBitmap(bm =>
    //        {
    //            fSizeChanged=bm == null || bm.Width != width || bm.Height != height;
    //            if (fSizeChanged)
    //            {
    //                Clear();
    //                bitmap = new Bitmap(width, height);
    //                //Log.WriteLine("BackGraphics.Created(" + width + "," + height + ")");
    //            }
    //            else if (fForceClearBackground)
    //            {
    //                using (var g = ImageUtil.CreateGraphics(bm))
    //                {
    //                    g.Clear(System.Drawing.Color.Transparent);
    //                }
    //            }
    //        }//);
    //        return fSizeChanged;
    //    }
    //    public ImageSource ToImageSource()
    //    {
    //        ImageSource src = null;
    //        //UseBitmap(bm =>
    //        //{
    //            src = ImageUtil.ToImageSource(bitmap);
    //        //});
    //        return src;
    //    }
    //    public Bitmap Bitmap
    //    {
    //        get
    //        {
    //            return bitmap;
    //        }
    //    }
    //    //public void UseBitmap(Action<Bitmap> action,bool fLock=true)
    //    //{
    //    //    if (fLock)
    //    //    {
    //    //        //lock (SyncBitmpObj)
    //    //        {
    //    //            action(bitmap);
    //    //        }
    //    //    }
    //    //    else
    //    //    {
    //    //        action(bitmap);
    //    //    }
    //    //}
    //    public void DrawBitmap(Graphics g)
    //    {
    //        if (bitmap != null)
    //        {
    //            g.DrawImage(bitmap, 0, 0);
    //        }
    //    }
    //    public void SetBackgound(System.Drawing.Color clr)
    //    {
    //        if (bitmap != null)
    //        {
    //            using (var g = ImageUtil.CreateGraphics(bitmap))
    //            {
    //                g.Clear(clr);
    //            }
    //        }
    //    }
    //    public void Dispose()
    //    {
    //        Clear(false);
    //    }
    //    private void Clear(bool fLock=true)
    //    {
    //        //UseBitmap(bm =>
    //        //{
    //            if (bitmap != null)
    //            {
    //                //Log.WriteLine("BackGraphics.Clear");
    //                bitmap.Dispose();
    //                bitmap = null;
    //            }
    //        //},fLock);
    //    }
    //}


    public interface ISelectionSet:IObjectIDSet
    {

    }
    public interface IFeatureSelection
    {
        double BufferDistance { get; set; }
        ISelectionSet SelectionSet { get; }
        ISymbol SelectionSymbol { get; set; }
        //bool queryByEnvelope(Envelope env, string where, string orderBy, Func<IFeature, bool> callback);
    }
    public class SelectionSet :ObjectIDSetBase, ISelectionSet
    {
        public Action OnSelectionChanged
        {
            get
            {
                return base.OnDataChanged;
            }
            set
            {
                base.OnDataChanged = value;
            }
        }
    }

    public class FeatureLayer : LayerBase, IFeatureLayer, IFeatureSelection, IDisposable//, IXmlSerializable
    {
        private bool _rendering = false;
        private bool _released = false;
        //private string _layerName;
        private IFeatureClass? _featureClass;
        private ISymbol _selectionSymbol;
        private IFeatureLabeler _labeler;
        private Exception _datasourceException;
        #region events
        public Action OnSelectionChanged
        {
            get { return (this.SelectionSet as SelectionSet).OnSelectionChanged; }
            set
            {
                (this.SelectionSet as SelectionSet).OnSelectionChanged = value;
            }
        }
        /// <summary>
        /// [old,new]
        /// </summary>
        public Action<IFeatureClass, IFeatureClass> OnSourceChanged { get; set; }
		public Func<string, string> OnBeforeUseWhere { get; set; }
        #endregion

        public FeatureLayer():this(null)
        {

        }
        public FeatureLayer(Map map)
            :base(map)
        {
            Visible = true;
            Selectable = true;
            this.Editable = true;
            SelectionSet = new SelectionSet();
            BufferDistance = 5;
            DataSourceMeta = new DataSourceMetaData();
        }


        #region ILayer
        public new OkEnvelope FullExtent
        {
            get {
                var fc = FeatureClass;
                if (fc != null)
                    return fc.GetFullExtent();
                return null;
            }
        }
        public new IRenderer Renderer
        {
            get { return FeatureRenderer; }
        }
        public new SpatialReference SpatialReference
        {
            get
            {
                return _featureClass?.SpatialReference;
            }
        }
        public override bool Render(LayerRenderMeta meta, ICancelTracker cancel,Action drawCallback)// Func<IFeature, bool> drawFeatureCallback)//Transform matrix)//,IEnvelope notDrawRect=null)
        {
            _rendering = true;
            try
            {
                if (FeatureRenderer == null || FeatureClass == null)// || !FeatureClass.IsOpen)
                {
                    return false;
                }

                var pRender = FeatureRenderer;
                var trans = meta.Transform;
				var ext = meta.GetVisibleBounds();
				if (ext == null)
				{
					return false;
				}
                ext.Project(Map.SpatialReference);
                pRender.SetupDC(meta.DC,meta.Transform);// Graphics, meta.Transform);
                //Log.WriteLine("SetupDC");
                var qf = new SpatialFilter
                {
                    Geometry = GeometryUtil.MakePolygon(ext),//, this.Map.SpatialReference);
                    WhereClause = UseWhere(),
                    GeometryField = this.FeatureClass.ShapeFieldName,
                    SubFields = this.FeatureClass.OIDFieldName + "," + this.FeatureClass.ShapeFieldName,
                    SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects
                };
                FeatureRenderer.PrepareFilter(FeatureClass, qf);
                _ =((IFeatureClassRender)FeatureClass).SpatialQueryAsync(qf, cancel, feature =>
                {
                    feature.Shape.Project(this.Map.SpatialReference);
                    drawCallback();
                    pRender.Draw(feature);
                    return !cancel.Cancel();// fContinue;
                });
                pRender.ResetDC();
                //Log.WriteLine("ResetDC");
                return !cancel.Cancel();// true;
            }
            finally
            {
                _rendering = false;
                if (_released)
                {
                    DoDispose();
                }
            }
        }
		public new void CancelTask()
		{
			FeatureClass?.CancelTask();
		}
        #endregion

        #region IFeatureLayer
        public string Where { get; set; }
		public string UseWhere()
		{
			return OnBeforeUseWhere!=null? OnBeforeUseWhere(Where):Where;
		}
		public string OrderByClause { get; set; }
        public DataSourceMetaData DataSourceMeta { get;private set; }
        public bool Selectable { get; set; }
        public bool Editable { get; set; } = true;
        public IFeatureClass? FeatureClass
        {
            get
            {
                return _featureClass;
            }
            set
			{
				if (_featureClass == value)
				{
					return;
				}
				if (string.IsNullOrEmpty(base.Name)&&value!=null)
				{
					Name = value.AliasName ?? value.TableName;
				}
				var oldFC = _featureClass;
				if (_featureClass != null)
				{
					_featureClass!.OnOpened -= this.OnFeatureClassOpened;
				}
				_featureClass = value;
				if (_featureClass != null)
				{
					this.DataSourceMeta.DataSourceType = _featureClass.ClassName;
					this.DataSourceMeta.ConnectionString = _featureClass.ConnectionString;
					this.DataSourceMeta.TableName = _featureClass.TableName;

					_featureClass.OnOpened += this.OnFeatureClassOpened;
					if (FeatureRenderer == null)
					{
						SetDefaultRender();
					}
				}
				OnSourceChanged?.Invoke(oldFC, value);
			}
        }
        public IFeatureRenderer FeatureRenderer
        {
            get;
            set;
        }
        public IFeatureLabeler FeatureLabeler
        {
            get { return _labeler; }
            set
            {
                _labeler = value;
                if (value is ASTExpressionLabeler astl)
                {
                    astl.SetFeatureLayer(this);
                }
            }
        }
        #endregion

        #region IDatasourceException
        public Exception DatasourceException
        {
            get
            {
                //if (_datasourceException != null)
                //{
                    return _datasourceException;
                //}
                //if (FeatureClass == null)
                //{
                //    return new Exception("未设置数据源");
                //}
                //return null;
            }
            set
            {
                _datasourceException = value;
            }
        }
        #endregion

        #region IFeatureSelection
        public double BufferDistance { get; set; }
        public ISelectionSet SelectionSet { get; private set; }
        public ISymbol SelectionSymbol {
            get
            {
                //if (_selectionSymbol == null && _featureClass!=null)
                //{
                //    return Map.GetDefaultSelectionSymbol(_featureClass.ShapeType);
                //}
                return _selectionSymbol;
            }
            set
            {
                _selectionSymbol = value;
            }
        }



        //bool IFeatureSelection.queryByEnvelope(Envelope env, string where, string orderBy, Func<IFeature, bool> callback)
        //{
        //    if (_featureClass == null)
        //    {
        //        return false;
        //    }
        //    return _featureClass.queryByEnvelope(env, where, orderBy, feature =>
        //    {
        //        if (SelectionSet.Contains(feature.oid))
        //        {
        //            bool fContinue=callback()
        //        }
        //        return true;
        //    });
        //}

        #endregion

        public override void ReadXml(XmlElement reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
        {
           foreach(var node in reader.ChildNodes)
            {
                if (node is XmlElement e)
                {
                    var name = e.Name;
                    var s = e.InnerText;
                    switch (name)
                    {
                        case "Name":
                            this.Name = s;
                            break;
                        case "Description":
                            this.Description = s;
                            break;
                        case "Visible":
                            this.Visible = s == "True";
                            break;
                        case "Selectable":
                            this.Selectable = s == "True";
                            break;
                        case "Editable":
                            this.Editable = s == "True";
                            break;
                        case "Where":
                            this.Where = s;
                            break;
                        case "MinimizeScale":
                            this.MinVisibleScale = SafeConvertAux.ToDouble(s);
                            break;
                        case "MaximizeScale":
                            this.MaxVisibleScale = SafeConvertAux.ToDouble(s);
                            break;
                        case "Alpha":
                            this.Alpha = SafeConvertAux.ToInt32(s);
                            break;
                        case "Renderer":
                            {
                                var t = e.GetAttribute("type");// SerializeUtil.ReadAttribute(reader, "type");
                                var x=SerializeUtil.CreateFeatureRenderer(t);
                                if(x!=null)//if (SerializeUtil.CreateInstance<IFeatureRenderer>(t) is IXmlSerializable x)
                                {
                                    x.ReadXml(e);
                                    this.FeatureRenderer = x;
                                }
                            }
                            break;
                        case "DataSource":
                            {
                                ReadDataSource(e, session);
                            }
                            break;
                        case "FeatureLabeler":
                            {
                                var t = e.GetAttribute("type");
                                var label = SerializeUtil.CreateInstance<IFeatureLabeler>(t);
                                if (label != null)
                                {
                                    label.ReadXml(e); 
                                    this.FeatureLabeler = label;
                                }
                            }
                            break;
                    }
                }
            }
        }


        #region IXmlSerializable

    //    public override void ReadXml(System.Xml.XmlReader reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
    //    {
    //        SerializeUtil.ReadNodeCallback(reader, this.GetType().Name, name =>
    //        {
    //            switch (name)
    //            {
    //                case "Name":
    //                    this.Name = reader.ReadString();
    //                    break;
    //                case "Description":
    //                    this.Description=reader.ReadString();
    //                    break;
    //                case "Visible":
    //                    this.Visible = reader.ReadString() == "True";
    //                    break;
    //                case "Selectable":
    //                    this.Selectable= reader.ReadString() == "True";
    //                    break;
    //                case "Editable":
    //                    this.Editable = reader.ReadString() == "True";
    //                    break;
    //                case "Where":
    //                    this.Where = reader.ReadString();
    //                    break;
    //                case "MinimizeScale":
    //                    this.MinVisibleScale = SafeConvertAux.ToDouble(reader.ReadString());
    //                    break;
    //                case "MaximizeScale":
    //                    this.MaxVisibleScale = SafeConvertAux.ToDouble(reader.ReadString());
    //                    break;
    //                case "Alpha":
    //                    this.Alpha = SafeConvertAux.ToInt32(reader.ReadString());
    //                    break;
    //                case "Renderer":
    //                    {
    //                        var t=SerializeUtil.ReadAttribute(reader, "type");
				//			if (SerializeUtil.CreateInstance<IFeatureRenderer>(t) is IXmlSerializable x)
				//			{
				//				x.ReadXml(reader);
				//				this.FeatureRenderer = x as IFeatureRenderer;
				//			}
				//		}
				//		break;
    //                case "DataSource":
    //                    {
    //                        ReadDataSource(reader,session);
    //                    }break;
    //                case "FeatureLabeler":
    //                    {
    //                        var t=SerializeUtil.ReadAttribute(reader, "type");
    //                        var label= SerializeUtil.CreateInstance<IFeatureLabeler>(t);
    //                        if (label != null)
    //                        {
    //                            SerializeUtil.ReadNodeCallback(reader, name, n =>
    //                            {
    //                                switch (n)
    //                                {
    //                                    case "EnableLabel":
    //                                        label.EnableLabel = reader.ReadString() == "True";
    //                                        break;
    //                                    case "LabelExpression":
    //                                        label.SetLabelExpression(reader.ReadString());
    //                                        break;
				//						case "Symbol":
				//							if (SerializeUtil.ReadSymbol(reader, n) is ITextSymbol ts)
				//							{
				//								label.TextSymbol = ts;
				//							}
				//							break;
    //                                }
    //                            });
    //                            this.FeatureLabeler = label;
    //                        }
    //                    }break;
    //            }
    //        });
    //    }

    //    public new void WriteXml(System.Xml.XmlWriter writer, bool fWriteDataSource = true)
    //    {
    //        writer.WriteStartElement(this.GetType().Name);
    //        SerializeUtil.WriteStringElement(writer, "Name", Name);
    //        if (!string.IsNullOrEmpty(Description))
    //        {
    //            SerializeUtil.WriteStringElement(writer, "Description", Description);
    //        }
    //        if (!Visible)
    //        {
    //            SerializeUtil.WriteStringElement(writer, "Visible", Visible.ToString());
    //        }
    //        if (!Selectable)
    //        {
    //            SerializeUtil.WriteStringElement(writer, "Selectable", Selectable.ToString());
    //        }
    //        if (!Editable)
    //        {
    //            SerializeUtil.WriteStringElement(writer, "Editable", Editable.ToString());
    //        }
    //        if (MinVisibleScale != null)
    //        {
    //            SerializeUtil.WriteDoubleElement(writer, "MinimizeScale", (double)MinVisibleScale);
    //        }
    //        if (MaxVisibleScale != null)
    //        {
    //            SerializeUtil.WriteDoubleElement(writer, "MaximizeScale", (double)MaxVisibleScale);
    //        }
    //        if (Alpha != null)
    //        {
    //            SerializeUtil.WriteIntElement(writer, "Alpha", (int)Alpha);
    //        }
    //        SerializeUtil.WriteStringElement(writer, "Where", Where);

    //        if (Renderer is IXmlSerializable x)
    //        {
    //            x.WriteXml(writer);
    //        }

    //        if (fWriteDataSource&&!string.IsNullOrEmpty(this.DataSourceMeta.DataSourceType))//&& FeatureClass != null)
    //        {
    //            var md = this.DataSourceMeta;

    //            writer.WriteStartElement("DataSource");
    //            writer.WriteAttributeString("type", this.DataSourceMeta.DataSourceType);// FeatureClass.ClassName);
    //            SerializeUtil.WriteStringElement(writer, "TableName", md.TableName);
    //            SerializeUtil.WriteStringElement(writer, "ConnectionString", md.ConnectionString);
    //            SerializeUtil.WriteStringElement(writer, "OIDFieldName", md.OIDFieldName);
    //            SerializeUtil.WriteStringElement(writer, "ShapeFieldName", md.ShapeFieldName);
    //            writer.WriteEndElement();
    //        }
    //        var label = this.FeatureLabeler;// as IXmlSerializable;
    //        if (label != null)
    //        {
    //            writer.WriteStartElement("FeatureLabeler");
    //            SerializeUtil.WriteAttribute(writer, "type", label.GetType().Name);
    //            SerializeUtil.WriteStringElement(writer, "EnableLabel", label.EnableLabel.ToString());
    //            SerializeUtil.WriteStringElement(writer, "LabelExpression", label.LabelExpression);
				//SerializeUtil.WriteSymbol(writer, "Symbol", label.TextSymbol);
    //            writer.WriteEndElement();
    //        }
    //        writer.WriteEndElement();
    //    }

        private void ReadDataSource(XmlElement reader, SerializeSession session)
        {
            string oldMapDocumentPath = session.OldDocumentPath;
            string newMapDocumentPath = session.DocumentPath;
            var t = reader.GetAttribute("type");// SerializeUtil.ReadAttribute(reader, "type");
            this.DataSourceMeta.DataSourceType = t;

            string? tableName = null, conStr = null, ShapeFieldName = null, OIDFieldName = null;
            foreach(XmlNode n in reader.ChildNodes)
            {
                var name = n.Name;
                var s = n.InnerText;
                switch (name)
                {
                    case "TableName":
                        tableName = s;
                        break;
                    case "ConnectionString":
                        conStr = s;
                        break;
                    case "ShapeFieldName":
                        ShapeFieldName = s;
                        break;
                    case "OIDFieldName":
                        OIDFieldName = s;
                        break;
                }
            }

            switch (t)
            {
                case "ShapeFileFeatureClass":
                case "SpatialiteFeatureClass":
                case "SqliteFeatureClass":
                    conStr = MakeNewPath(conStr, oldMapDocumentPath, newMapDocumentPath);
                    break;
            }

            this.DataSourceMeta.ConnectionString = conStr;
            this.DataSourceMeta.TableName = tableName;
            this.DataSourceMeta.ShapeFieldName = ShapeFieldName;
            this.DataSourceMeta.OIDFieldName = OIDFieldName;
        }
   //     private void ReadDataSource(System.Xml.XmlReader reader, SerializeSession session)
   //     {
			//string oldMapDocumentPath = session.OldDocumentPath;
			//string newMapDocumentPath = session.DocumentPath;
   //         var t = SerializeUtil.ReadAttribute(reader, "type");
   //         this.DataSourceMeta.DataSourceType = t;

   //         string tableName = null, conStr = null, ShapeFieldName = null, OIDFieldName=null;
   //         SerializeUtil.ReadNodeCallback(reader, "DataSource", name =>
   //         {
   //             switch (name)
   //             {
   //                 case "TableName":
   //                     tableName = reader.ReadString();
   //                     break;
   //                 case "ConnectionString":
   //                     conStr = reader.ReadString();
   //                     //this.ConnectionString = conStr;
   //                     break;
   //                 case "ShapeFieldName":
   //                     ShapeFieldName = reader.ReadString();
   //                     break;
   //                 case "OIDFieldName":
   //                     OIDFieldName = reader.ReadString();
   //                     break;
   //             }
   //         });

   //         switch (t)
   //         {
   //             case "ShapeFileFeatureClass":
   //             case "SpatialiteFeatureClass":
   //             case "SqliteFeatureClass":
   //                 conStr = MakeNewPath(conStr, oldMapDocumentPath, newMapDocumentPath);
   //                 break;
   //         }

   //         this.DataSourceMeta.ConnectionString = conStr;
   //         this.DataSourceMeta.TableName = tableName;
   //         this.DataSourceMeta.ShapeFieldName = ShapeFieldName;
   //         this.DataSourceMeta.OIDFieldName = OIDFieldName;
   //     }
        #endregion

        public void SaveLayerAs(string fileName)
        {
    //        if (fileName == null)
    //        {
				//var dlg = new Microsoft.Win32.SaveFileDialog
				//{
				//	Filter = "图层文件(*.layer)|*.layer",
				//	RestoreDirectory = true,
				//	FilterIndex = 1,
				//	OverwritePrompt = true
				//};
				//if (dlg.ShowDialog() != true)
    //            {
    //                return;
    //            }
    //            fileName = dlg.FileName;
    //        }
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (var writer = new StreamWriter(fileName))
            using (var x = System.Xml.XmlWriter.Create(writer))
            {
                x.WriteStartElement("Layer");
                WriteXml(x,false);
                x.WriteEndElement();
            }
        }

        public static FeatureLayer LoadFromLyrFile(string fileName)
        {
            throw new NotImplementedException();
            //FeatureLayer fl = null;
            //using (var reader = new StreamReader(fileName))
            //using (var x = new System.Xml.XmlTextReader(reader))
            //{
            //    SerializeUtil.ReadNodeCallback(x, "Layer", name =>
            //    {
            //        if (name == "FeatureLayer")
            //        {
            //            fl = new FeatureLayer();
            //            fl.ReadXml(x, new SerializeSession(FileUtil.GetFilePath(fileName)));
            //        }
            //    });
            //}

            //return fl;
        }

        public void Dispose()
        {
            _released=true;
            if (!_rendering)
            {
                DoDispose();
            }
        }
        private void DoDispose()
        {
            if (FeatureClass != null)
            {
                FeatureClass.Dispose();
                FeatureClass = null;
            }
        }

        private void SetDefaultRender()
        {
            System.Diagnostics.Debug.Assert(_featureClass != null);
            var render = new SimpleFeatureRenderer();
            FeatureRenderer = render;
            var sf = GisGlobal.SymbolFactory;
            switch (_featureClass.ShapeType)
            {
                case eGeometryType.eGeometryPoint:
                case eGeometryType.eGeometryMultipoint:
                    render.SetSymbol(sf.CreateSimpleMarkerSymbol());
                    break;
                case eGeometryType.eGeometryPolyline:
                    render.SetSymbol(sf.CreateSimpleLineSymbol());
                    break;
                case eGeometryType.eGeometryPolygon:
                    render.SetSymbol(sf.CreateSimpleFillSymbol());
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
        }

        private void OnFeatureClassOpened()
        {
            var sr =_featureClass?.SpatialReference;
            if (sr!=null&&Map.SpatialReference == null)
            {
                Map.SetSpatialReference(sr);
            }
        }
    }
}
