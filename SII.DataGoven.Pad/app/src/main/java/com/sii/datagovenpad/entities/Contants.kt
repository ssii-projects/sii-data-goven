package com.sii.datagovenpad.entities

object XzdyFields{
    val TB_NAME="DLXX_XZDY"
    val MC="MC"
}
object DKFields{
    val TB_NAME="VEC_SURVEY_DK"
    val cbfmc="CBFMC"
    val fbfbm="FBFDM"
}

enum class EZt(value:Int){
    /// <summary>
    /// 临时
    /// </summary>
    lins(0),
    /// <summary>
    /// 有效
    /// </summary>
    youxiao (1),
    /// <summary>
    /// 历史
    /// </summary>
    lishi (2),
}

/// <summary>
/// 登记状态
/// </summary>
enum class EDjzt(value: Int)
{

    /// <summary>
    /// 未登记
    /// </summary>
    wdj (0),
    /// <summary>
    /// 登记中
    /// </summary>
    djz(1),
    /// <summary>
    /// 已登记
    /// </summary>
    ydj (2),
}