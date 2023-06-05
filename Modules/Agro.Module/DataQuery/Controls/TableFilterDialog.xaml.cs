using Agro.LibCore;
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
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

namespace Agro.Module.DataQuery
{
    /// <summary>
    /// 自定义查询对话框 的交互逻辑
    /// </summary>
    public partial class TableFilterDialog : UserControl
    {
        /// <summary>
        /// 唯一值
        /// </summary>
        public class UniqueValue
        {
            /// <summary>
            /// 字段是否字符串类型
            /// </summary>
            public readonly bool fStringType;
            public readonly string Value;
            public UniqueValue(string value, bool isStrType)
            {
                fStringType = isStrType;
                //code = new Code(strCode, strName);
                Value = value;
            }
            public override string ToString()
            {
                return Value;// string.IsNullOrEmpty(code.strName) || code.strName == code.iCode ? code.iCode : (code.iCode + "-" + code.strName);
            }
        }

        //返回唯一值
        private readonly List<UniqueValue> _lsUniqueValues = new List<UniqueValue>();
        //定位
        private readonly List<UniqueValue> _lsGoto = new List<UniqueValue>();

        private ContentPanel.QueryTypeItem _table;
        private List<ContentPanel.FieldColumn> _lstFields;
        private IWorkspace _db;

        public TableFilterDialog(ContentPanel.QueryTypeItem table, List<ContentPanel.FieldColumn> lstFields
            , IWorkspace db,string wh)
        {
            _table = table;
            _lstFields = lstFields;
            _db = db;
            InitializeComponent();
            lboxFields.ItemsSource = lstFields;
            if (!string.IsNullOrEmpty(wh))
            {
                txtSql.Text = wh;
            }
        }

        public string GetWhere()
        {
            return txtSql.Text.Trim();
        }
        /// <summary>
        /// 检查表达式的正确性，正确返回null否则返回错误信息
        /// </summary>
        /// <returns></returns>
        public string CheckWhere()
        {
            try
            {
                var wh = GetWhere();
                if (string.IsNullOrEmpty(wh))
                {
                    return "未输入查询条件！";
                }
                var sql = "select * from " + _table.TableName + " where (" + wh + ")";// and 1<0";
                _db.QueryCallback(sql, r => { return false; });
                return null;
            }catch(Exception ex)
            {
                return ex.Message;
            }
        }
        //选择字段
        private void lboxFields_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = lboxFields.SelectedItem as ContentPanel.FieldColumn;
            string field = " " +fi.ColumnAttribute.FieldName + " ";
            int start = txtSql.SelectionStart;
            string sText = txtSql.Text;
            txtSql.Text = "";
            txtSql.Text += sText.Substring(0, start) + field + sText.Substring(start + txtSql.SelectionLength);
            txtSql.SelectionStart = start + field.Length;
            txtSql.Focus();
            //btnVerify.IsEnabled = btnSave.IsEnabled = true;
        }
        //运算符
        private void btnYunSuan_Click(object sender, RoutedEventArgs e)
        {
            string text = " " + (sender as Button).Content.ToString().Replace(" ", "") + " ";
            if (sender == btnKuoHao)
            {
                //如果没有选中任何内容则前移光标,反之在选中内容前后加括号
                if (string.IsNullOrEmpty(txtSql.SelectedText))
                {
                    int start = txtSql.SelectionStart;
                    string sText = txtSql.Text;
                    txtSql.Text = "";
                    txtSql.Text += sText.Substring(0, start) + text + sText.Substring(start + txtSql.SelectionLength);
                    txtSql.SelectionStart = start + text.Length - 2;
                    txtSql.Focus();
                }
                else
                {
                    txtSql.SelectedText = "(" + txtSql.SelectedText + ")";
                }
            }
            else
            {
                int start = txtSql.SelectionStart;
                string sText = txtSql.Text;
                txtSql.Text = "";
                txtSql.Text += sText.Substring(0, start) + text + sText.Substring(start + txtSql.SelectionLength);
                txtSql.SelectionStart = start + text.Length;
                txtSql.Focus();
            }
            //btnVerify.IsEnabled = btnSave.IsEnabled = true;
        }

        //唯一值选择
        private void lboxUniqueValues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UniqueValue uv = lboxUniqueValues.SelectedItem as UniqueValue;
            if (uv.fStringType)
            {
                string code = " '" + uv.Value + "'";
                int start = txtSql.SelectionStart;
                string sText = txtSql.Text;
                txtSql.Text = "";
                txtSql.Text += sText.Substring(0, start) + code + sText.Substring(start + txtSql.SelectionLength);
                txtSql.SelectionStart = start + code.Length;
                txtSql.Focus();
            }
            else
            {
                string code = " " + uv.Value + " ";
                int start = txtSql.SelectionStart;
                string sText = txtSql.Text;
                txtSql.Text = "";
                txtSql.Text += sText.Substring(0, start) + code + sText.Substring(start + txtSql.SelectionLength);
                txtSql.SelectionStart = start + code.Length;
                txtSql.Focus();
            }
            //btnVerify.IsEnabled = btnSave.IsEnabled = true;
        }

        //返回唯一值按钮
        private void btnGetUniqueValues_Click(object sender, RoutedEventArgs e)
        {
            if (lboxFields.SelectedItem == null)
                return;
            lboxUniqueValues.IsEnabled = true;
            var fi = lboxFields.SelectedItem as ContentPanel.FieldColumn;

            _lsGoto.Clear();
            //var lst = new List<string>();
            var fStrField = fi.ColumnAttribute.IsStringField||fi.ColumnAttribute.IsDateTimeField;
            var sql = "select distinct " + fi.ColumnAttribute.FieldName + " from " +EntityUtil.GetTableName(_table.entity)
				+ " where "+fi.ColumnAttribute.FieldName + " is not null";
            int cnt = 0;
            _db.QueryCallback(sql, r =>
            {
                var o = r.GetValue(0);
                if (o != null)
                {
                    _lsGoto.Add(new UniqueValue(o.ToString(), fStrField));
                }
                if (++cnt > 100)
                {
                    return false;
                }
                return true;
            });
            lboxUniqueValues.ItemsSource = null;
            lboxUniqueValues.ItemsSource = _lsGoto;
            //IDataStatistics_Example(/*_client,*/ lboxFields.SelectedItem.Tag.ToString());//(lboxFields.SelectedItem as LayersField).FieldName);

        }
        //定位过滤
        private void txtGoTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            _lsGoto.Clear();
            string strGo = txtGoTo.Text.Trim();
            foreach (UniqueValue uv in _lsUniqueValues)
            {
                if (uv.Value.Contains(strGo))// || uv.Value.Contains(strGo) || CrazyCoderPinyin.SpellSearch(uv.code.strPym, strGo))
                {
                    _lsGoto.Add(uv);
                }
            }
            lboxUniqueValues.ItemsSource = null;
            lboxUniqueValues.ItemsSource = _lsGoto;
        }
    }
}
