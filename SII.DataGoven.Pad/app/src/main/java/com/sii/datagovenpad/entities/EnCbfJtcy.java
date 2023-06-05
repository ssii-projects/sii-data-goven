package com.sii.datagovenpad.entities;

import com.sii.orm.DataColumn;
import com.sii.orm.DataTable;
import com.sii.orm.EntityBase;
import com.sii.util.GuidUtil;

import java.util.Date;

@DataTable("DC_QSSJ_CBF_JTCY")
public class EnCbfJtcy  extends EntityBase<EnCbfJtcy> {
    public static EnCbfJtcy instance=new EnCbfJtcy();

    public String id= GuidUtil.getNewGUID();

    @DataColumn("CBFBM")
    public String cbfbm;
    /**
     * 成员姓名
     */
    public String cyxm="";
    /**
     * 成员性别
     */
    @DataColumn(codeType = "性别")
    public String cyxb="";
    /**
     * 与户主关系
     */
    @DataColumn(codeType = "家庭关系")
    public String yhzgx;

    @DataColumn(codeType = "证件类型")
    public String cyZjlx;
    /**
     * 成员证件号码
     */
    public String cyZjhm;
    /**
     * 是否共有人
     */
    @DataColumn(codeType = "是否")
    public String sfgyr;
    /**
     * 成员备注
     */
    @DataColumn(codeType = "成员备注")
    public String cybz;
    /**
     * 成员备注说明
     */
    public String cybzsm;

    public String csrq;
}