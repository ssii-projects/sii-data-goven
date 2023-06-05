using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchMap
{
    public class TaskSketchMapArgumet// : TaskArgument
    {
        public class CbfItem
        {
            /// <summary>
            /// 承包方编码
            /// </summary>
            public string CBFBM;
            /// <summary>
            /// 承包方名称
            /// </summary>
            public string CbfMc { get; set; }
            public bool IsSelected { get; set; }

            public override string ToString()
            {
                return CbfMc;
            }
        }
        public enum DType
        {
            Null,
            DatFile,
            CbfItem
        }
        internal DType DataType = DType.Null;
        /// <summary>
        /// 文件集合
        /// [dat文件路径或SelectCbfPanel.CbfItem]
        /// </summary>
        public List<object> FileNames { get; private set; } = new List<object>();

        /// <summary>
        /// 输出路径
        /// </summary>
        public string OutputPath { get; set; }

        //[Enabled(false)]
        public SkecthMapProperty MapProperty { get; set; } = new SkecthMapProperty();


        public TaskSketchMapArgumet()
        {
        }
    }
}
