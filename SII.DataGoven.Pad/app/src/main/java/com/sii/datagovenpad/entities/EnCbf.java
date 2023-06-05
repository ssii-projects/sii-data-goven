package com.sii.datagovenpad.entities;

import com.sii.orm.DataColumn;
import com.sii.orm.DataTable;
import com.sii.orm.EntityBase;
import com.sii.util.DateTimeUtil;
import com.sii.util.GuidUtil;

@DataTable("DC_QSSJ_CBF")
public class EnCbf extends EntityBase<EnCbf> {
    public static EnCbf instance=new EnCbf();

    public String id= GuidUtil.getNewGUID();
    @DataColumn(codeType = "发包方编码")
    public String fbfbm="";
    @DataColumn("CBFBM")
    public String cbfbm;
    @DataColumn("CBFMC")
    public String cbfmc;

    /**
     * 承包方类型
     */
    @DataColumn(codeType = "承包方类型")
    public String cbfLx="";

    @DataColumn(codeType = "证件类型")
    public String cbfZjlx="";
    /**
     * 承包方证件号码
     */
    public String cbfZjhm;
    public String cbfDz;
    /**
     * 邮政编码
     */
    public String yzbm;
    /**
     * 联系电话
     */
    public String lxdh;

    /**
     * 承包方成员数量
     */
    public int cbfCysl=0;
    /**
     * 承包方调查日期
     */
    public String cbfDcrq;
    /**
     * 承包方调查员
     */
    public String cbfDcy;
    /**
     * 承包方调查记事
     */
    public String cbfDcjs;
    /**
     * 公示记事
     */
    public String gsJs;
    /**
     * 公示记事人
     */
    public String gsJsr;
    /**
     * 公示审核日期
     */
    public String gsShrq;
    /**
     * 公示审核人
     */
    public String gsShr;
    /**
     * 最后修改时间
     */
    public String zhxgsj;

    /**
     * 状态
     */
    public int zt=EZt.lins.ordinal();
    /**
     * 登记状态
     */
    public int djzt=EDjzt.wdj.ordinal();
    /**
     * 创建时间
     */
    public String cjsj= DateTimeUtil.getCurrentTimeString();
}

