using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Printing;

namespace Agro.Office
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum DocumentPrinterType
    {
        PrintView = 1,//打印预览

        Print = 2,//打印
    }

    /// <summary>
    /// 地域操作事件
    /// </summary>
    public class DocumentPrinterEventArgs : EventArgs
    {
        public PrintDocument Docuemnt { get; set; }//实体
        public bool Success { get; set; }//成功标志
        public string Information { get; set; }//信息
    }

    /// <summary>
    /// 宗地对象操作
    /// </summary>
    public class DocumentObjectPrinter
    {
        public delegate void DocumentObjectPrinterDelegate(DocumentPrinterType operateType, DocumentPrinterEventArgs args);
        public static event DocumentObjectPrinterDelegate DocumentPrinterEvent;

        /// <summary>
        /// 预览文档
        /// </summary>
        /// <returns></returns>
        public static DocumentPrinterEventArgs PrintViewDocument(PrintDocument document, DocumentPrinterType operatorType)
        {
            DocumentPrinterEventArgs args = new DocumentPrinterEventArgs();
            args.Docuemnt = document;
            args.Success = true;
            if (DocumentPrinterEvent != null)
            {
                DocumentPrinterEvent(operatorType, args);
            }
            return args;
        }

    }
}
