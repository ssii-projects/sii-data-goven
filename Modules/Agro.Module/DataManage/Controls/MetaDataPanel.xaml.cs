using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Agro.Module.DataManage
{
    /// <summary>
    /// MetaDataPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MetaDataPanel : UserControl
    {
        class GridData
        {
            private readonly Dictionary<int, Dictionary<int, string>> _data = new Dictionary<int, Dictionary<int, string>>();
            public string GetCellText(int row, int col)
            {
                Dictionary<int, string> dic;
                if (_data.TryGetValue(row, out dic))
                {
                    string s;
                    if (dic.TryGetValue(col, out s))
                    {
                        return s;
                    }
                }
                return "";
            }
            public void SetCellText(int row, int col, string s)
            {
				if (!_data.TryGetValue(row, out Dictionary<int, string> dic))
				{
					dic = new Dictionary<int, string>();
					_data[row] = dic;
				}
				dic[col] = s;
            }
        }
        public MetaDataPanel(string metaXmlFile)
        {
            InitializeComponent();
            if (File.Exists(metaXmlFile))
            {
                parseMetaFile(metaXmlFile);
            }
        }
        private void parseMetaFile(string metaXmlFile)
        {
            var xx = new XmlDocument();
            xx.Load(metaXmlFile);
            parseDataIdInfo(xx);
            parseResSysInfo(xx);
            parseLayerInfo(xx);
            parseDqInfo(xx);
        }
        private void parseDataIdInfo(XmlDocument xx)
        {
            var node = xx.SelectSingleNode("/Metadata/dataIdInfo");
            foreach (var n in node.ChildNodes)
            {
				if (n is XmlElement xe)
				{
					TextBlock tb = null;
					switch (xe.Name)
					{
						case "title":
							tb = tbName;
							break;
						case "date":
							tb = tbDate;
							break;
						case "geoID":
							tb = tbgeoID;
							break;
						case "dataEdition":
							tb = tbdataEdition;
							break;
						case "dataLang":
							tb = tbdataLang;
							break;
						case "idAbs":
							tb = tbidAbs;
							break;
						case "status":
							tb = tbstatus;
							break;
						case "ending":
							tb = tbending;
							break;
						case "rpOrgName":
							tb = tbrpOrgName;
							break;
						case "rpCnt":
							tb = tbrpCnt;
							break;
						case "voiceNum":
							tb = tbvoiceNum;
							break;
						case "faxNum":
							tb = tbfaxNum;
							break;
						case "cntAddress":
							tb = tbcntAddress;
							break;
						case "cntCode":
							tb = tbcntCode;
							break;
						case "cntEmail":
							tb = tbcntEmail;
							break;
						case "classCode":
							tb = tbclassCode;
							break;
					}
					if (tb != null)
					{
						tb.Text = tb.Text + xe.InnerText;
					}
				}
				//var s=n.ToString();
				//Console.WriteLine(s);
			}
        }
        private void parseResSysInfo(XmlDocument xx)
        {
            var node = xx.SelectSingleNode("/Metadata/refSysInfo");
            foreach (var n in node.ChildNodes)
            {
                var xe = n as XmlElement;
                if (xe != null)
                {
                    TextBlock tb = null;
                    switch (xe.Name)
                    {
                        case "coorRSID":
                            tb = tbcoorRSID;
                            break;
                        case "centralMer":
                            tb = tbcentralMer;
                            break;
                        case "eastFAL":
                            tb = tbeastFAL;
                            break;
                        case "northFAL":
                            tb = tbnorthFAL;
                            break;
                        case "coorFDKD":
                            tb = tbcoorFDKD;
                            break;
                    }
                    if (tb != null)
                    {
                        tb.Text = tb.Text + xe.InnerText;
                    }
                }
            }
        }
        private void parseDqInfo(XmlDocument xx)
        {
            var node = xx.SelectSingleNode("/Metadata/dqInfo");
            foreach (var n in node.ChildNodes)
            {
                var xe = n as XmlElement;
                if (xe != null)
                {
                    TextBlock tb = null;
                    switch (xe.Name)
                    {
                        case "dqStatement":
                            tb = tbdqStatement;
                            break;
                        case "dqLineage":
                            tb = tbdqLineage;
                            break;
                    }
                    if (tb != null)
                    {
                        tb.Text = tb.Text + xe.InnerText;
                    }
                }
            }
        }
        private void parseLayerInfo(XmlDocument xx)
        {
            var sa = new string[] { "图层名称", "数据集要素类型名称", "与数据集要素类名称对应的主要属性列表", "数据量" };
            grid1.Cols = sa.Length;
            int i = 0;
            for(; i < sa.Length; ++i)
            {
                grid1.SetColLabel(i, sa[i]);
            }
            grid1.SetColWidth(1, 125);
            grid1.SetColWidth(2,520);
            var data = new GridData();
            grid1.OnGetCellText += (r, c) =>
            {
                return data.GetCellText(r, c);
            };
            var nl = xx.SelectNodes("/Metadata/contInfo/layer");
            grid1.Rows = nl.Count;
            i = 0;
            foreach (var n in nl)
            {
                var xe0 = n as XmlElement;
                if (xe0 == null)
                {
                    continue;
                }
                var nl1=xe0.ChildNodes;
                foreach (var n1 in nl1)
                {
					if (n1 is XmlElement xe)
					{
						switch (xe.Name)
						{
							case "layerName":
								data.SetCellText(i, 0, xe.InnerText);
								break;
							case "catFetTyps":
								data.SetCellText(i, 1, xe.InnerText);
								break;
							case "attrTypList":
								{
									string fields = null;
									foreach (var n2 in xe.ChildNodes)
									{
										var xe1 = n2 as XmlElement;
										if (xe1 != null)
										{
											var s = xe1.InnerText;
											if (fields == null)
											{
												fields = s;
											}
											else
											{
												fields += "、" + s;
											}
										}
									}
									if (fields != null)
									{
										data.SetCellText(i, 2, fields);
									}
								}
								break;
							case "capacity":
								data.SetCellText(i, 3, xe.InnerText);
								break;
						}
					}
				}
                ++i;
            }

        }

        private static void updateColumnWidth(Agro.LibCore.UI.GridView grid, int nMargin = 30)
        {
            #region update ColWidth
            var lstColWidth = new double[grid.Cols];
            int rows = Math.Min(50, grid.Rows);
            for (int c = 0; c < lstColWidth.Length; ++c)
            {
                var colLabel = grid.GetColLabel(c);
                lstColWidth[c] = grid.CalcTextWidth(colLabel) + nMargin;
                for (int i = 0; i < rows; ++i)
                {
                    var s = grid.GetCellText(i, c);
                    var wi = Math.Min(250, grid.CalcTextWidth(s) + nMargin);
                    if (lstColWidth[c] < wi)
                    {
                        lstColWidth[c] = wi;
                    }
                }
            }
            for (int c = 0; c < lstColWidth.Length; ++c)
            {
                grid.SetColWidth(c, (int)lstColWidth[c]);
            }
            #endregion

        }
    }
}
