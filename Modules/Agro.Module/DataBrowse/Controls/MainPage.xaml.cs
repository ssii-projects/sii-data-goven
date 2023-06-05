using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.GIS;
using Agro.Library.Common.Util;

namespace Agro.Module.DataBrowse
{
    public partial class MainPage : UserControl,IDisposable
    {
        class TableItem
        {
            public string TableName { get; set; }
            public string AliasName { get; set; }
            public string FileName { get; set; }
            internal object Tag;
            public TableItem(string tableName,string aliasName,string fileName=null)
            {
                TableName = tableName;
                AliasName = aliasName;
                FileName = fileName;
            }
        }

        class RowData : List<string>
        {

        }
        class MDBTableData
        {
            public readonly List<ComboItemData> Columns = new List<ComboItemData>();
            //public readonly List<EntityProperty> Columns = new List<EntityProperty>();
            public readonly List<RowData> Values = new List<RowData>();
            public string GetValue(int row, int col)
            {
                if (row >= 0 && row < Values.Count)
                {
                    var r = Values[row];
                    if (col < 0 || col > r.Count)
                    {
                        return "";
                    }
                    var o = r[col];
                    return o;// == null ? "" : o.ToString();
                }
                return "";
            }
            public void ClearValues()
            {
                Values.Clear();
            }
        }

        class ComboItemData
        {
            public string FieldName;
            public eFieldType FieldType;
            public ComboItemData(string fieldName,eFieldType fieldType)
            {
                FieldName = fieldName;
                FieldType = fieldType;
            }
            public override string ToString()
            {
                return FieldName;
            }
        }

        /// <summary>
        /// 文件目录
        /// </summary>
        //public FilePathInfo CurrentPath { get; set; }
        public readonly HJDataRootPath CurrentPath = new HJDataRootPath();



        //private System.Collections.ObjectModel.ObservableCollection<IDataPagerProvider> listDataSourceProperty;
        //private System.Collections.ObjectModel.ObservableCollection<IDataPagerProvider> listDataSourceGeometry;

        //private BaseTableInfo infoPropertyFields = new BaseTableInfo();
        //private ShapeFileInfo infoGeometryFields = new ShapeFileInfo();

        //private TaskQueue taskQueue;

        private readonly List<TableItem> listDataSourceProperty = new List<TableItem>();
        private readonly List<TableItem> listDataSourceGeometry = new List<TableItem>();

        private readonly MDBTableData _mdbTableData = new MDBTableData();
        //private readonly List<string> _fields = new List<string>();
        public MainPage()
        {
            InitializeComponent();
            //InitializeComponent();
            //listDataSourceProperty = new ObservableCollection<IDataPagerProvider>();
            //listDataSourceGeometry = new ObservableCollection<IDataPagerProvider>();
            listBoxProperty.ItemsSource = listDataSourceProperty;
            listBoxGeometry.ItemsSource = listDataSourceGeometry;
            //taskQueue = new TaskQueueDispatcher(Dispatcher);
            cbFields.ItemsSource = new List<ComboItemData>();
            txtPath.OnButtonClick += () =>
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = "请选择汇交成果所在文件夹：";
                fbd.SelectedPath = txtPath.Text.Trim();
                if (string.IsNullOrEmpty(fbd.SelectedPath))
                    fbd.SelectedPath = Environment.CurrentDirectory;

                var dr = fbd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK || dr == System.Windows.Forms.DialogResult.Yes)
                {
                    var err=CurrentPath.Init(fbd.SelectedPath);
                    if (err != null)
                    {
                        MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        txtPath.Text = fbd.SelectedPath;
                        OnPathChanged();
                    }
                }
                this.Focus();
            };

            RaiseMouseWheel(listBoxProperty);
            RaiseMouseWheel(listBoxGeometry);

            //grid.ShowGrid = false;
            //mdbView.RowHeight = mdbView.ColHeaderHeight = 28;
            mdbView.OnDrawCell += (dc, ci) =>
            {
                var r = ci.Row;
                if ((r % 2) == 0 && r != mdbView.ActiveRow)
                {
                    ci.background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF1, 0xF1, 0xF1));// System.Windows.Media.Colors.LightGray);
                }
            };
            mdbView.OnGetCellText += (r, c) =>
            {
                return _mdbTableData.GetValue(r,c);
            };

            dbfView.OnDrawCell += (dc, ci) =>
            {
                var r = ci.Row;
                if ((r % 2) == 0 && r != dbfView.ActiveRow)
                {
                    ci.background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF1, 0xF1, 0xF1));// System.Windows.Media.Colors.LightGray);
                }
            };

            listBoxProperty.GotFocus += (s, e) => SetVisible(mdbView);
            listBoxGeometry.GotFocus += (s, e) => SetVisible(dbfView);

            txtSearch.OnButtonClick += () =>
            {
                if (mdbView.Visibility == Visibility.Visible)
                {
                    UpdateMDBTableUI();
                }else
                {
                    UpdateShapeTableUI();
                }
            };
        }

        private void SetVisible(Agro.LibCore.UI.GridView grid)
        {

            if (grid.Visibility != Visibility.Visible)
            {
                if (grid == mdbView)
                {
                    dbfView.Visibility = Visibility.Collapsed;
                    mdbView.Visibility = Visibility.Visible;
                }
                else
                {
                    mdbView.Visibility = Visibility.Collapsed;
                    dbfView.Visibility = Visibility.Visible;
                }
            }
            UpdateComboSource();
        }
        private void UpdateComboSource()
        {
            var lst=cbFields.ItemsSource as List<ComboItemData>;
            lst.Clear();
            cbFields.ItemsSource = null;
            if (mdbView.Visibility == Visibility.Visible)
            {
                lst.AddRange(_mdbTableData.Columns);
                lst.RemoveAll(i => { return i.FieldType != eFieldType.eFieldTypeString; });
            }
            else
            {
                IFeatureClass fc = dbfView.Tag as IFeatureClass;
                if (fc != null)
                {
                    var fields = fc.Fields;
                    for(int i = 0; i < fields.FieldCount; ++i)
                    {
                        var field = fields.GetField(i);
                        if (field.FieldType==eFieldType.eFieldTypeString)//!(field.FieldType == eFieldType.eFieldTypeOID || field.FieldType == eFieldType.eFieldTypeGeometry))
                        {
                            lst.Add(new ComboItemData(field.FieldName,field.FieldType) );// field.FieldName);
                        }
                    }
                }
            }
            cbFields.ItemsSource = lst;
        }
        private static void RaiseMouseWheel(ListBox lb)
        {
            lb.PreviewMouseWheel += (sender, e) =>
            {

                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);

                eventArg.RoutedEvent = UIElement.MouseWheelEvent;

                eventArg.Source = sender;

                lb.RaiseEvent(eventArg);
            };
        }
        private void OnPathChanged()
        {
            try
            {
                listBoxProperty.ItemsSource = null;
                listBoxGeometry.ItemsSource = null;


                #region 设置ListBox1的数据源
                listDataSourceProperty.Clear();
                using (var db = DBAccess.Open(CurrentPath.mdbFileName))
                {
                    var n = (CurrentPath.RootPath + "权属数据").Length;
                    var fileName = CurrentPath.mdbFileName.Substring(n + 1);
                    var lst = GetMDBTableItems();
                    foreach (var ti in lst)
                    {
                        if(db.IsTableExists(ti.TableName))
                        {
                            ti.FileName = fileName;
                            listDataSourceProperty.Add(ti);
                        }
                    }
                    expander1.IsExpanded = listDataSourceProperty.Count > 0;
                }
                #endregion

                #region 设置ListBox2的数据源
                listDataSourceGeometry.Clear();
                var lstShapeNames = GetShapeFileTableItems();
                FileUtil.EnumFiles(CurrentPath.RootPath + "矢量数据", fi =>
                   {
                       var fileName = fi.Name.ToUpper();
                       if (fileName.EndsWith(".SHP"))
                       {
                           var ti = lstShapeNames.Find(i =>
                             {
                                 return fileName.StartsWith(i.TableName);
                             });
                           if (ti != null)
                           {
                               ti.FileName = fileName;
                               ti.TableName = fileName.Substring(0,fileName.LastIndexOf('.'));
                               listDataSourceGeometry.Add(ti);
                           }
                       }
                       return true;
                   });
                expander2.IsExpanded = listDataSourceGeometry.Count > 0;
                #endregion

                listBoxProperty.ItemsSource = listDataSourceProperty;
                listBoxGeometry.ItemsSource = listDataSourceGeometry;

            }
            catch (Exception ex)
            {
                UIHelper.ShowExceptionMessage(ex);
            }
        }

        private static List<TableItem> GetMDBTableItems()
        {
            var lst = new List<TableItem>()
            {
                new TableItem("CBDKXX","承包地块信息"),
                new TableItem("CBF","承包方"),
                new TableItem("CBF_JTCY","家庭成员"),
                new TableItem("CBHT","承包合同"),
                new TableItem("CBJYQZDJB","承包经营权登记簿"),
                new TableItem("CBJYQZ","承包经营权证"),
                new TableItem("CBJYQZ_QZBF","权证补发"),
                new TableItem("CBJYQZ_QZHF","权证换发"),
                new TableItem("CBJYQZ_QZZX","权证注销"),
                new TableItem("LZHT","流转合同"),
                new TableItem("FBF","发包方"),
                new TableItem("QSLYZLFJ","权属来源资料附件"),
            };
            return lst;
        }

        private static List<TableItem> GetShapeFileTableItems()
        {
            var lst = new List<TableItem>()
            {
                new TableItem("KZD","控制点"),
                new TableItem("XJXZQ","县级行政区"),
                new TableItem("XJQY","乡级区域"),
                new TableItem("CJQY","村级区域"),
                new TableItem("ZJQY","组级区域"),
                new TableItem("DK","地块"),
                new TableItem("JZD","界址点"),
                new TableItem("JZX","界址线"),
                new TableItem("JBNTBHQ","基本农田保护区"),
                new TableItem("DZDW","点状地物"),
                new TableItem("MZDW","面状地物"),
                new TableItem("XZDW","线状地物"),
                new TableItem("QYJX","区域界线"),
                new TableItem("ZJ","注记"),
            };
            return lst;
        }
        //#region Methods


        //public void AddPropertyTable(IDataPagerProvider provider)
        //{
        //    listDataSourceProperty.Add(provider);
        //}

        //public void AddGeometryTable(IDataPagerProvider provider)
        //{
        //    listDataSourceGeometry.Add(provider);
        //}


        //#region Methods - Override

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    var border = GetTemplateChild("toolbarShadow") as Border;
        //    border.BorderThickness = new Thickness(0);
        //}

        //#endregion

        //#region Methods - Events

        //private void MetroButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        OpenFileDialog ofd = new OpenFileDialog();
        //        ofd.Filter = "Access 文件(*.mdb)|*.mdb";
        //        ofd.Multiselect = false;

        //        var b = ofd.ShowDialog();
        //        if (b == null || !b.Value)
        //            return;

        //        string fileName = ofd.FileName;

        //        var ds = DataBase.CreateDbContext(fileName);
        //        var dq = new DynamicQuery(ds);

        //        var elements = dq.GetElements();
        //        elements.ForEach(c =>
        //        {
        //            if (c.TableName.StartsWith("MSys") || c.TableName.Contains("~"))
        //                return;

        //            listDataSourceProperty.Add(new DataPagerProviderAccess(
        //                   ds, c.Schema, c.TableName, c.TableName, System.IO.Path.GetFileName(fileName)));
        //        });
        //        //var dlg = new SelectTableDialogAccess(Workpage) { DataSource = ds };
        //        //Workpage.Page.ShowDialog(dlg, (val, r) =>
        //        //{
        //        //    if (!val.Value)
        //        //        return;

        //        //    listDataSourceProperty.Add(new DataPagerProviderAccess(
        //        //        ds, dlg.GetSelectedTable().Schema, dlg.GetSelectedTable().TableName, System.IO.Path.GetFileName(fileName)));
        //        //});
        //    }
        //    catch (Exception ex)
        //    {
        //        Workpage.Page.ShowDialog(new MessageDialog()
        //        {
        //            MessageGrade = eMessageGrade.Exception,
        //            Message = ex.ToString(),
        //            Header = "添加 Access"
        //        });
        //    }
        //}

        private void MetroButton_Click_1(object sender, RoutedEventArgs e)
        {
            //    try
            //    {
            //        OpenFileDialog ofd = new OpenFileDialog();
            //        ofd.Filter = "Shapefile 文件(*.shp)|*.shp";
            //        ofd.Multiselect = false;

            //        var b = ofd.ShowDialog();
            //        if (b == null || !b.Value)
            //            return;

            //        string fileName = ofd.FileName;

            //        Directory.GetFiles(System.IO.Path.GetDirectoryName(fileName), "*.shp").ToList().ForEach(c =>
            //        {
            //            listDataSourceGeometry.Add(
            //                new DataPagerProviderShapefile(ProviderShapefile.CreateDataSource(
            //                    System.IO.Path.GetDirectoryName(c), false), null,
            //                    System.IO.Path.GetFileNameWithoutExtension(c),
            //                    System.IO.Path.GetFileNameWithoutExtension(c),
            //                    System.IO.Path.GetFileName(c)));
            //        });
            //        //var dlg = new SelectTableDialogAccess(Workpage) { DataSource = ds };
            //        //Workpage.Page.ShowDialog(dlg, (val, r) =>
            //        //{
            //        //    if (!val.Value)
            //        //        return;

            //        //    listDataSourceProperty.Add(new DataPagerProviderAccess(
            //        //        ds, dlg.GetSelectedTable().Schema, dlg.GetSelectedTable().TableName, System.IO.Path.GetFileName(fileName)));
            //        //});
            //    }
            //    catch (Exception ex)
            //    {
            //        Workpage.Page.ShowDialog(new MessageDialog()
            //        {
            //            MessageGrade = eMessageGrade.Exception,
            //            Message = ex.ToString(),
            //            Header = "添加 Shapefile"
            //        });
            //    }
        }

        //private static void updateColumnWidth(Captain.LibCore.UI.GridView grid)
        //{
        //    #region update ColWidth
        //    var lstColWidth = new double[grid.Cols];
        //    int rows = Math.Min(50, grid.Rows);
        //    for (int c = 0; c < lstColWidth.Length; ++c)
        //    {
        //        var colLabel = grid.GetColLabel(c);
        //        lstColWidth[c] = grid.CalcTextWidth(colLabel) + 10;
        //        for (int i = 0; i < rows; ++i)
        //        {
        //            var s = grid.GetCellText(i, c);
        //            var wi = Math.Min(250, grid.CalcTextWidth(s) + 10);
        //            if (lstColWidth[c] < wi)
        //            {
        //                lstColWidth[c] = wi;
        //            }
        //        }
        //    }
        //    for (int c = 0; c < lstColWidth.Length; ++c)
        //    {
        //        grid.SetColWidth(c, (int)lstColWidth[c]+10);
        //    }
        //    #endregion

        //}
        private string GetWhere(bool fShpView=false)
        {
            string where = null;
            var filterText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(filterText))
            {
                var cid = cbFields.SelectedItem as ComboItemData;
                if (cid != null)
                {
                    var fieldName = cid.FieldName;
                    if (fShpView)
                    {
                        fieldName = "[" + fieldName + "]";
                    }
                    if (checkBoxFuzzy.IsChecked == true)
                    {
                        where = fieldName + " = '" + filterText.Replace("'", "''") + "'";
                    }
                    else
                    {
                        where = fieldName + " like '%" + filterText.Replace("'", "''") + "%'";
                    }
                }
            }
            return where;
        }
        private void UpdateMDBTableUI()
        {
            var ti = listBoxProperty.SelectedItem as TableItem;
            if (ti == null)
            {
                return;
            }

            string where = GetWhere();

            _mdbTableData.Columns.Clear();
            _mdbTableData.ClearValues();
            using (var db = DBAccess.Open(CurrentPath.mdbFileName))
            {
                var fieldNames = db.QueryFields2(ti.TableName);
                int cols = fieldNames.Count;
                mdbView.Rows = 0;
                mdbView.Cols = cols;
                string subFields = null;
                for (int c = 0; c < fieldNames.Count; ++c)
                {
                    var field = fieldNames[c];
                    var fieldName = field.FieldName;
                    _mdbTableData.Columns.Add(new ComboItemData(field.FieldName, field.FieldType));
                    mdbView.SetColLabel(c, fieldName);
                    if (subFields == null)
                    {
                        subFields = fieldName;
                    }
                    else
                    {
                        subFields += "," + fieldName;
                    }
                }
                var sql = "select " + subFields + " from " + ti.TableName;
                if (where != null)
                {
                    sql += " where " + where;
                }
                db.QueryCallback(sql, r =>
                {
                    var rd = new RowData();
                    for (int i = 0; i < cols; ++i)
                    {
                        var o = r.GetValue(i);
                        rd.Add(o == null ? "" : o.ToString());
                    }
                    _mdbTableData.Values.Add(rd);
                    return true;
                });
                mdbView.Rows = _mdbTableData.Values.Count;
                GridViewUtil.UpdateColumnWidth(mdbView);
                mdbView.UpdateLayout();
                mdbView.Refresh();
            }
        }
        private void UpdateShapeTableUI()
        {
			if (!(listBoxGeometry.SelectedItem is TableItem ti))
			{
				return;
			}
			if (dbfView.Tag is IFeatureClass fc)
			{
				fc.Dispose();
			}
			var wh = GetWhere(true);
            fc = ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass(CurrentPath.RootPath + "矢量数据", ti.TableName);
            dbfView.Tag = fc;
            dbfView.Init(fc);
            if (wh != null)
            {
                dbfView.ReQuery(wh);
            }
            GridViewUtil.UpdateColumnWidth(dbfView);
            dbfView.UpdateLayout();
            dbfView.Refresh();
        }
        private void listBoxProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateMDBTableUI();
                SetVisible(mdbView);
            }
            catch (Exception ex)
            {
                UIHelper.ShowExceptionMessage(ex);
            }
        }

        private void listBoxGeometry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateShapeTableUI();
                SetVisible(dbfView);
            }
            catch (Exception ex)
            {
                UIHelper.ShowExceptionMessage(ex);
            }
        }

        //private void dataGrid_InitializeColumn(object sender, InitializeDataGridColumnEventArgs e)
        //{
        //    var col = new DataGridTextColumn();
        //    col.Header = e.Property.AliasName;

        //    var b = new Binding(string.Format("{0}", e.Property.ColumnName));
        //    col.Binding = b;

        //    if (e.Property.ColumnType == eDataType.Decimal ||
        //        e.Property.ColumnType == eDataType.Double ||
        //        e.Property.ColumnType == eDataType.Float ||
        //        e.Property.ColumnType == eDataType.Int16 ||
        //        e.Property.ColumnType == eDataType.Int32 ||
        //        e.Property.ColumnType == eDataType.Int64 ||
        //        e.Property.ColumnType == eDataType.Byte)
        //    {
        //        var style = new Style(typeof(TextBlock));
        //        style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right));
        //        col.ElementStyle = style;

        //        var style1 = new Style(typeof(DataGridColumnHeader));
        //        style1.BasedOn = Application.Current.TryFindResource("Metro_DataGrid_HeaderContainer_Style") as Style;
        //        style1.Setters.Add(new Setter(DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Right));
        //        col.HeaderStyle = style1;
        //    }
        //    else if (e.Property.ColumnType == eDataType.DateTime)
        //    {
        //        b.Converter = new DateTimeToStringConverter();
        //    }

        //    e.Column = col;

        //    if (dataGrid.DataSource is DataPagerProviderAccess)
        //    {
        //        var ds = dataGrid.DataSource as DataPagerProvider;
        //        var info = typeof(BaseTableInfo).GetProperties().FirstOrDefault(c => c.Name == string.Format("{0}ZD", ds.ElementName));
        //        if (info != null)
        //        {
        //            List<FieldInformation> fields = info.GetValue(infoPropertyFields, null) as List<FieldInformation>;

        //            var field = fields.FirstOrDefault(c => c.FieldName == e.Property.ColumnName);
        //            if (field != null)
        //                e.Column.Header = field.AliseName;
        //        }
        //    }
        //    else if (dataGrid.DataSource is DataPagerProviderShapefile)
        //    {
        //        var ds = dataGrid.DataSource as DataPagerProvider;
        //        if ((e.Column.Header as string) == "___FID___")
        //        {
        //            e.Column.Header = "序号";
        //        }
        //        else
        //        {
        //            var info = typeof(ShapeFileInfo).GetProperties().FirstOrDefault(c => c.Name.EndsWith("ZD") && ds.ElementName.StartsWith(c.Name.Remove(c.Name.Length - 2, 2)));
        //            if (info != null)
        //            {
        //                List<FieldInformation> fields = info.GetValue(infoGeometryFields, null) as List<FieldInformation>;

        //                var field = fields.FirstOrDefault(c => c.FieldName == e.Property.ColumnName);
        //                if (field != null)
        //                    e.Column.Header = field.AliseName;
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 关联数据
        ///// </summary>
        //private void miRelation_Click(object sender, RoutedEventArgs e)
        //{
        //    DataPagerProvider provider = dataGrid.DataSource as DataPagerProvider;
        //    if (provider == null || dataGrid.SelectedItem == null)
        //    {
        //        return;
        //    }
        //    if (CurrentPath == null || string.IsNullOrEmpty(CurrentPath.DataBasePath))
        //    {
        //        return;
        //    }
        //    if (provider.ElementName != CBDKXX.TableName && provider.ElementName != CBF.TableName &&
        //        provider.ElementName != CBHT.TableName && provider.ElementName != CBJYQZ.TableName &&
        //        provider.ElementName != CBJYQZDJB.TableName)
        //    {
        //        return;
        //    }
        //    object selectItem = dataGrid.SelectedItem;
        //    if (selectItem == null)
        //    {
        //        return;
        //    }
        //    RelevanceInfoPage page = new RelevanceInfoPage();
        //    Workpage.TaskQueue.Cancel();
        //    Workpage.TaskQueue.DoWithInterruptCurrent(
        //        go =>
        //        {
        //            page.DataBaseFilePath = CurrentPath.DataBasePath;
        //            switch (provider.ElementName)
        //            {
        //                case CBDKXX.TableName:
        //                    CBDKXX dk = selectItem.ConvertTo<CBDKXX>();
        //                    page.Parameter = dk;
        //                    break;
        //                case CBF.TableName:
        //                    CBF cbf = selectItem.ConvertTo<CBF>();
        //                    try
        //                    {
        //                        cbf.CBFCYSL = Convert.ToInt32(selectItem.GetPropertyValue("CBFCYSL"));
        //                    }
        //                    catch
        //                    { }
        //                    page.Parameter = cbf;
        //                    break;
        //                case CBHT.TableName:
        //                    CBHT ht = selectItem.ConvertTo<CBHT>();
        //                    try
        //                    {
        //                        ht.CBDKZS = Convert.ToInt32(selectItem.GetPropertyValue("CBDKZS"));
        //                    }
        //                    catch
        //                    { }
        //                    try
        //                    {
        //                        ht.HTZMJ = Convert.ToDouble(selectItem.GetPropertyValue("HTZMJ"));
        //                        ht.HTZMJ = Math.Round(ht.HTZMJ, 2);
        //                    }
        //                    catch
        //                    { }
        //                    page.Parameter = ht;
        //                    break;
        //                case CBJYQZ.TableName:
        //                    CBJYQZ qz = selectItem.ConvertTo<CBJYQZ>();
        //                    page.Parameter = qz;
        //                    break;
        //                case CBJYQZDJB.TableName:
        //                    CBJYQZDJB djb = selectItem.ConvertTo<CBJYQZDJB>();
        //                    try
        //                    {
        //                        djb.DKSYT = selectItem.GetPropertyValue("DKSYT") as string;
        //                    }
        //                    catch
        //                    { }
        //                    page.Parameter = djb;
        //                    break;
        //            }
        //        },
        //        completed =>
        //        {
        //            if (completed.Instance.IsStopPending)
        //                return;
        //            Workpage.Page.ShowDialog(page);
        //        }, null, null, null, null, null, null, null);
        //}

        //private void txtSearch_Searching(object sender, EventArgs e)
        //{
        //    Search(dataGrid.DataSource as DataPagerProvider, txtSearch.Text.TrimSafe(), !checkBoxFuzzy.IsChecked.Value);
        //}

        //private void MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    if (dataGrid.CurrentCell == null || dataGrid.CurrentCell.Column == null || dataGrid.CurrentCell.Item == null)
        //        return;

        //    var content = dataGrid.CurrentCell.Column.GetCellContent(dataGrid.CurrentCell.Item);
        //    Clipboard.SetText((content as TextBlock).Text);
        //}

        //private void dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    DataPagerProvider provider = dataGrid.DataSource as DataPagerProvider;
        //    if (provider == null)
        //    {
        //        separator.Visibility = Visibility.Collapsed;
        //        miRelation.Visibility = Visibility.Collapsed;
        //        return;
        //    }
        //    if (provider.ElementName != CBDKXX.TableName && provider.ElementName != CBF.TableName &&
        //       provider.ElementName != CBHT.TableName && provider.ElementName != CBJYQZ.TableName &&
        //       provider.ElementName != CBJYQZDJB.TableName)
        //    {
        //        miRelation.Visibility = Visibility.Collapsed;
        //        separator.Visibility = Visibility.Collapsed;
        //        return;
        //    }
        //    miRelation.Visibility = Visibility.Visible;
        //    separator.Visibility = Visibility.Visible;
        //}


        ///// <summary>
        ///// 属性查看
        ///// </summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //    var dataProvider = listBoxProperty.SelectedItem as IDataPagerProvider;
            //    if (dataProvider == null)
            //        return;
            //    ReportProperty repPropety = new ReportProperty();
            //    repPropety.DataProvider = dataProvider;
            //    Workpage.Page.ShowDialog(repPropety);

        }

        /// <summary>
        /// 属性查看
        /// </summary>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //    var dataProvider = listBoxGeometry.SelectedItem as IDataPagerProvider;
            //    if (dataProvider == null)
            //        return;
            //    ReportProperty repPropety = new ReportProperty();
            //    repPropety.DataProvider = dataProvider;
            //    repPropety.dgNeed.Visibility = Visibility.Collapsed;
            //    Workpage.Page.ShowDialog(repPropety);

        }


        /// <summary>
        /// 文件路径更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //    SettingsChangedEventArgs args = new SettingsChangedEventArgs() { Name = "SelectedDataFolder" };
            //    Workpage.Message.Send(sender, args);
            //    try
            //    {
            //        listBoxProperty.SelectedIndex = 0;
            //    }catch(SystemException ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine(ex.Message);
            //    }
        }

        /// <summary>
        /// 查看目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text))
            {
                return;
            }
            System.Diagnostics.Process.Start(txtPath.Text);
        }

        /// <summary>
        /// 数据分析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAnaly_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(CurrentPath) || !Directory.Exists(CurrentPath))
            if(CurrentPath.ErrorInfo!=null)
            {
                //MessageBox.Show("请先选择有效的汇交成果路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                MessageBox.Show("请先选择有效的汇交成果路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var page = new DataOverviewPanel();
            page.Refresh(CurrentPath.RootPath);
            MyGlobal.MainWindow.OpenPage(page, "数据分析", MyImageSourceUtil.Image16("Analy.png"));
            //    var workpage = Workpage.Workspace.CreateWorkpage<DataOverviewPanel>();
            //    var page = workpage.Page.Content as DataOverviewPanel;
            //    page.Refresh(CurrentPath.Path);
            //    //if (onCreated != null)
            //    //{
            //    //    onCreated(page);
            //    //}
            //    //TODO... page Params
            //    Workpage.Workspace.AddWorkpage(workpage);
            //    workpage.Activate();
        }

        /// <summary>
        /// 地图浏览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMapView_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPath.ErrorInfo != null)
            {
                MessageBox.Show("请先选择有效的汇交成果路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var map = new DxExport.MapPage();
            map.Init(CurrentPath);
            MyGlobal.MainWindow.OpenPage(map, "地图浏览", CommonImageUtil.Image16("Map.png"));
            //    var workpage = Workpage.Workspace.GetWorkpage(typeof(AgricultureMapPage));
            //    if (workpage != null)
            //    {
            //        workpage.Activate();
            //        return;
            //    }
            //    workpage = Workpage.Workspace.AddWorkpage<AgricultureMapPage>();
            //    AgricultureMapPage mapViewer = workpage.Page.Content as AgricultureMapPage;
            //    mapViewer.CurrentPath = CurrentPath;
            //    mapViewer.SelectPath = txtPath.Text;
            //    workpage.Activate();
        }

        public void Dispose()
        {
            var fc = dbfView.Tag as IFeatureClass;
            if (fc != null)
            {
                fc.Dispose();
            }
        }

        //#endregion

        //#region Methods - Private

        //private void Search(DataPagerProvider provider, string value, bool isFuzzy)
        //{
        //    if (cbFields.SelectedIndex < 0)
        //        return;

        //    var cols = new List<MatchColumn>();
        //    if (cbFields.SelectedIndex == 0)
        //        cols.AddRange((cbFields.ItemsSource as List<SearchFieldItem>).
        //            Where(c => !c.Type.IsNullOrBlank()).
        //            Select(c => MatchColumn.Create(c.Name, c.DataType, provider.DataSource.ProviderName)).
        //            ToList());
        //    else
        //        cols.Add(MatchColumn.Create(
        //            (cbFields.SelectedItem as SearchFieldItem).Name,
        //            (cbFields.SelectedItem as SearchFieldItem).DataType, provider.DataSource.ProviderName));

        //    StringBuilder b = new StringBuilder();
        //    foreach (var col in cols)
        //    {
        //        if (b.Length == 0 && isFuzzy)
        //            b.AppendFormat("( {0} )", col.BuildContains(value));
        //        else if (b.Length == 0 && !isFuzzy)
        //            b.AppendFormat("( {0} )", col.BuildEquals(value));
        //        else if (isFuzzy)
        //            b.AppendFormat(" || ( {0} )", col.BuildContains(value));
        //        else
        //            b.AppendFormat(" || ( {0} )", col.BuildEquals(value));
        //    }

        //    provider.FilterExpression = value.IsNullOrBlank() ? null : b.ToString();
        //    provider.IsEnabledFilter = true;

        //    dataGrid.Refresh();


        //}

        ///// <summary>
        ///// 右键菜单
        ///// </summary>
        //private void ResetSearch(DataPagerProvider provider)
        //{
        //    taskQueue.Cancel();

        //    string lastSelected = null;

        //    taskQueue.DoWithInterruptCurrent(
        //        go =>
        //        {
        //            if (go.Instance.IsStopPending)
        //                return;

        //            List<FieldInformation> fields = new List<FieldInformation>();
        //            if (dataGrid.DataSource is DataPagerProviderAccess)
        //            {
        //                var ds = dataGrid.DataSource as DataPagerProvider;
        //                var info = typeof(BaseTableInfo).GetProperties().FirstOrDefault(c => c.Name == string.Format("{0}ZD", ds.ElementName));
        //                if (info != null)
        //                    fields = info.GetValue(infoPropertyFields, null) as List<FieldInformation>;
        //            }
        //            else if (dataGrid.DataSource is DataPagerProviderShapefile)
        //            {
        //                var ds = dataGrid.DataSource as DataPagerProvider;
        //                var info = typeof(ShapeFileInfo).GetProperties().FirstOrDefault(c => c.Name.EndsWith("ZD") && ds.ElementName.StartsWith(c.Name.Remove(c.Name.Length - 2, 2)));
        //                if (info != null)
        //                    fields = info.GetValue(infoGeometryFields, null) as List<FieldInformation>;
        //            }

        //            //var layer = item.Layer as VectorLayer;
        //            var names = provider.GetProperties().Where(c => c.ColumnType != eDataType.Geometry && c.ColumnType != eDataType.DateTime).ToList().Select(c => new SearchFieldItem()
        //            {
        //                Name = c.ColumnName,
        //                AliasName = fields.Any(f => f.FieldName == c.ColumnName) ? fields.FirstOrDefault(f => f.FieldName == c.ColumnName).AliseName : c.ColumnName,
        //                Type = EnumNameAttribute.GetDescription(c.ColumnType),
        //                DataType = c.ColumnType
        //            }).ToList();



        //            go.Instance.Argument.UserState = names;

        //        }, completed =>
        //        {
        //            List<SearchFieldItem> list = new List<SearchFieldItem>();
        //            list.Add(new SearchFieldItemAll());

        //            if (completed.Result is List<SearchFieldItem>)
        //                list.AddRange(completed.Result as List<SearchFieldItem>);

        //            cbFields.ItemsSource = list;

        //            var ns = list.FirstOrDefault(c => c.Name == lastSelected);
        //            cbFields.SelectedItem = ns;

        //            if (cbFields.SelectedIndex < 0)
        //                cbFields.SelectedIndex = 0;

        //        }, terminated =>
        //        {
        //            string errorMsg = terminated.Exception.ToString();
        //            if (terminated.GetType() == typeof(FileNotFoundException) && terminated.Exception.Message.Contains(".dbf"))
        //            {
        //                errorMsg = "dbf文件不存在";
        //            }
        //            Workpage.Page.ShowDialog(new MessageDialog()
        //            {
        //                Header = "查找",
        //                Message = string.Format("初始化查找功能时出错。{0}",
        //                terminated.Exception)
        //            });

        //        }, progressChanged =>
        //        {
        //        }, started =>
        //        {
        //            var li = cbFields.SelectedItem as SearchFieldItem;
        //            if (li != null)
        //                lastSelected = li.Name;

        //            cbFields.ItemsSource = null;
        //            //dataGrid.IsBusy = true;
        //        }, ended =>
        //        {
        //            //IsLayerFieldsLoading = false;

        //        }, alert =>
        //        {
        //        }, stopped =>
        //        {
        //        }, null);
        //}

        //#endregion

        //#region Methods - Helper

        ///// <summary>
        ///// 初始化默认数据
        ///// </summary>
        //public void InitalizeData()
        //{
        //    string filePath = txtPath.Text;// @"F:\Data\220581梅河口市";
        //    if (string.IsNullOrEmpty(filePath))
        //    {
        //        return;
        //    }
        //    GainInfo ginfo = new GainInfo(filePath);
        //    if (ginfo == null || !ginfo.IsValide)
        //    {
        //        return;
        //    }
        //    CurrentPath = FilePathManager.GetCurrentPath(filePath);
        //    if (CurrentPath == null)
        //    {
        //        return;
        //    }
        //    FilePathManager fpm = new FilePathManager();
        //    string victorName = fpm.VictorName;
        //    string categoryName = fpm.CategoryName;
        //    InitalizeDatabase(ginfo.ZoneCode + ginfo.Year);
        //    fpm = null;
        //    CurrentPath.YearCode = ginfo.Year;
        //    CurrentPath.ZoneCode = ginfo.ZoneCode;
        //    CurrentPath.UnitName = ginfo.UnitName;
        //}

        ///// <summary>
        ///// 检查属性数据目录
        ///// </summary>
        //private void InitalizeDatabase(string extentName)
        //{
        //    string dataBasefilePath = string.Empty;
        //    string[] files = Directory.GetFiles(CurrentPath.ThroneFilePath);
        //    foreach (string path in files)
        //    {
        //        string name = Path.GetFileNameWithoutExtension(path);
        //        string fullname = Path.GetFileName(path);
        //        if (path.ToLower().LastIndexOf(".mdb") > 0 && name.ToLower().Equals(extentName))
        //        {
        //            dataBasefilePath = path;
        //        }
        //    }
        //    CurrentPath.DataBasePath = dataBasefilePath;
        //}

        //#endregion

        //#endregion
    }

    
}
