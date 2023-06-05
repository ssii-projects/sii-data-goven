package com.sii.datagovenpad.entities;


import com.sii.orm.DataColumn;
import com.sii.orm.DataTable;
import com.sii.orm.EntityBase;

@DataTable("VEC_SURVEY_DK")
public class EnDk extends EntityBase<EnDk> {
    public static EnDk instance=new EnDk();

    @DataColumn(codeType = "发包方编码")
    public String fbfbm;
    @DataColumn("CBFBM")
    public String cbfbm;
    public String cbfmc;
}