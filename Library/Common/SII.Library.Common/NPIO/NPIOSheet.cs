using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captain.Library.Common
{
    public class NPIOSheet : IDisposable
    {
        private HSSFWorkbook _book = null;
        private ISheet _sht;
        private FileStream _fs = null;
        private readonly double dpi;
        public NPIOSheet()
        {
            using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                //dpiX = g.DpiX;
                //dpiY = g.DpiY;
                dpi = g.DpiX;
            }
        }
        public void Open(string fileName)
        {
            _fs = File.OpenRead(fileName);
            _book = new HSSFWorkbook(_fs);
            _sht = _book.GetSheetAt(0);
           // double dpiX, dpiY;
        }
        public void Create()
        {
            _book = new HSSFWorkbook();
            _sht = _book.CreateSheet();// 创建一个Excel的Sheet 
        }
        /// <summary>
        /// Excel列宽单位为1/10英寸
        /// </summary>
        /// <param name="col"></param>
        /// <param name="width">像素单位</param>
        public void SetColumnWidth(int col,int width)
        {
            double dExcelWidth = (width / dpi + 0.52) * 10;//像素转Excel宽度单位
            width =(int)( dExcelWidth * 256);//Excel宽度单位装NPIO宽度单位
            _sht.SetColumnWidth(col, width);
        }
        public void SetCellText(int row, int col, string val)
        {
            IRow r = _sht.GetRow(row);
            if (r == null)
            {
                r = _sht.CreateRow(row);
            }
            var c = r.GetCell(col);
            if (c == null)
            {
                c = r.CreateCell(col);
            }
            c.SetCellValue(val);
        }
        public void SetCellDouble(int row,int col,double val)
        {
            IRow r = _sht.GetRow(row);
            if (r == null)
            {
                r = _sht.CreateRow(row);
            }
            var c = r.GetCell(col);
            if (c == null)
            {
                c = r.CreateCell(col);
            }
            c.SetCellValue(val);
        }
        public ICellStyle GetCellStyle(int row, int col)
        {
            var r = _sht.GetRow(row);
            var c = r.GetCell(col);
            return c.CellStyle;
        }
        public void SetCellStyle(int row, int col, ICellStyle cs)
        {
            var r = _sht.GetRow(row);
            var c = r.GetCell(col);
            c.CellStyle = cs;
        }

        public void ExportToExcel(string fileName)
        {
            using (FileStream stm = File.OpenWrite(fileName))
            {
                _book.Write(stm);
            }
        }
        public void Dispose()
        {
            if (_fs != null)
            {
                _fs.Close();
                _fs.Dispose();
            }
        }
    }
}
