using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 笛卡尔坐标系象限类型
    /// </summary>
    public enum CglQuadrantType
    {
        /// <summary>
        /// 坐标原点
        /// </summary>
        OriginPoint = 0,
        /// <summary>
        /// 一象限
        /// </summary>
        One = 1,
        /// <summary>
        /// 二象限
        /// </summary>
        Two = 2,
        /// <summary>
        /// 三象限
        /// </summary>
        Tree = 3,
        /// <summary>
        /// 四象限
        /// </summary>
        Four = 4,
        /// <summary>
        /// x正半轴
        /// </summary>
        XPositive,
        /// <summary>
        /// x负半轴
        /// </summary>
        XNegative,
        /// <summary>
        /// y正半轴
        /// </summary>
        YPositive,
        /// <summary>
        /// y负半轴
        /// </summary>
        YNegative,
    }
}
