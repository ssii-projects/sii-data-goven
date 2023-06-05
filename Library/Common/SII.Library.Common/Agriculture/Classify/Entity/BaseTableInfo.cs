/*
 * (C)2016 公司版权所有,保留所有权利
*/
using System;
using System.Collections.Generic;
using NetTopologySuite.IO;
using Agriculture.Entity;

namespace Agriculture.Classify
{
    /// <summary>
    /// 属性数据库列表
    /// </summary>
    public class BaseTableInfo
    {
        #region Fields

        /// <summary>
        /// 字段信息
        /// </summary>
        private FieldInformation fieldInfo;

        /// <summary>
        /// 应该包含的表名称
        /// </summary>
        public static List<FileCondition> TableNameList
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("TableName"), typeof(List<FileCondition>)) as List<FileCondition>;
                if (list != null)
                {
                    return list;
                }
                list = new List<FileCondition>()
                {
                    new FileCondition(){Name=CBDKXXEX.TableName,IsNecessary=true,AliesName=CBDKXXEX.TableNameCN},
                    new FileCondition(){Name=CBF.TableName,IsNecessary=true,AliesName=CBF.TableNameCN},
                    new FileCondition(){Name=CBF_JTCY.TableName,IsNecessary=true,AliesName=CBF_JTCY.TableNameCN},
                    new FileCondition(){Name=CBHT.TableName,IsNecessary=true,AliesName=CBHT.TableNameCN},
                    new FileCondition(){Name=CBJYQZDJB.TableName,IsNecessary=true,AliesName=CBJYQZDJB.TableNameCN},
                    new FileCondition(){Name=CBJYQZ.TableName,IsNecessary=true,AliesName=CBJYQZ.TableNameCN},
                    new FileCondition(){Name=CBJYQZ_QZBF.TableName,IsNecessary=false,AliesName=CBJYQZ_QZBF.TableNameCN},
                    new FileCondition(){Name=CBJYQZ_QZHF.TableName,IsNecessary=false,AliesName=CBJYQZ_QZHF.TableNameCN},
                    new FileCondition(){Name=CBJYQZ_QZZX.TableName,IsNecessary=false,AliesName=CBJYQZ_QZZX.TableNameCN},
                    new FileCondition(){Name=LZHT.TableName,IsNecessary=false,AliesName=LZHT.TableNameCN},
                    new FileCondition(){Name=FBF.TableName,IsNecessary=true,AliesName=FBF.TableNameCN },
                    new FileCondition(){Name=QSLYZLFJ.TableName,IsNecessary=false,AliesName=QSLYZLFJ.TableNameCN },
                    //new FileCondition(){Name=JZXSMEX.TableName,IsNecessary=false,AliesName=JZXSMEX.TableNameCN},
                    //new FileCondition(){Name=SGSJ.TableName,IsNecessary=false,AliesName=SGSJ.TableNameCN}     
                };
                return list;
            }
        }

        #endregion

        #region Ctor

        public BaseTableInfo()
        {
            fieldInfo = new FieldInformation();
        }

        #endregion

        #region 字段信息

        /// <summary>
        /// 承包地块字段信息
        /// </summary>
        public virtual List<FieldInformation> CBDKXXZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBDKXXZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                        fieldInfo.CreateEntity("地块代码",CBDKXX.FDKBM, "String", 19),
                        fieldInfo.CreateEntity("发包方代码",CBDKXX.FFBFBM, "String", 14),
                        fieldInfo.CreateEntity("承包方代码",CBDKXX.FCBFBM, "String", 18),
                        fieldInfo.CreateEntity("承包经营权取得方式",CBDKXX.FCBJYQQDFS, "String", 3),
                        fieldInfo.CreateEntity("确权(合同)面积",CBDKXX.FHTMJ, "Double",15,2,true,false),
                        fieldInfo.CreateEntity("确权合同面积(亩)",CBDKXX.FHTMJM, "Double",15,2,false,false,false),
                        fieldInfo.CreateEntity("原承包合同面积",CBDKXX.FYHTMJ, "Double",15,2,false,false,false),
                        fieldInfo.CreateEntity("原承包合同面积(亩)",CBDKXX.FYHTMJM, "Double",15,2,false,false,false),
                        fieldInfo.CreateEntity("承包合同代码",CBDKXX.FCBHTBM, "String", 19),
                        fieldInfo.CreateEntity("流转合同代码",CBDKXX.FLZHTBM, "String", 18,0,false,true,false),
                        fieldInfo.CreateEntity("承包经营权证(登记簿)代码",CBDKXX.FCBJYQZBM, "String", 19),
                        fieldInfo.CreateEntity("是否确权确股",CBDKXX.FSFQQQG, "String", 1,0,false,true,false)
                    };
                return list;
            }
        }

        /// <summary>
        /// 发包方字段
        /// </summary>
        public virtual List<FieldInformation> FBFZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("FBFZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                        fieldInfo.CreateEntity("发包方代码",FBF.FFBFBM, "String", 14),
                        fieldInfo.CreateEntity("发包方名称",FBF.FFBFMC, "String", 50),
                        fieldInfo.CreateEntity("发包方负责人姓名",FBF.FFBFFZRXM, "String", 50),
                        fieldInfo.CreateEntity("负责人证件类型",FBF.FFZRZJLX, "String", 1),
                        fieldInfo.CreateEntity("负责人证件号码",FBF.FFZRZJHM, "String", 30),
                        fieldInfo.CreateEntity("联系电话",FBF.FLXDH, "String", 15,0,false,true,false),
                        fieldInfo.CreateEntity("发包方地址",FBF.FFBFDZ, "String", 100),
                        fieldInfo.CreateEntity("邮政编码",FBF.FYZBM, "String", 6),
                        fieldInfo.CreateEntity("发包方调查员",FBF.FFBFDCY, "String", 254,0,true,false),
                        fieldInfo.CreateEntity("发包方调查日期",FBF.FFBFDCRQ, "DateTime", 8,0,true,false),
                        fieldInfo.CreateEntity("发包方调查记事",FBF.FFBFDCJS, "String",254,0,false,false)
                    };
                return list;
            }
        }

        /// <summary>
        /// 承包方字段
        /// </summary>
        public virtual List<FieldInformation> CBFZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBFZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包方代码",CBF.FCBFBM, "String", 18),
                    fieldInfo.CreateEntity("承包方类型",CBF.FCBFLX, "String", 1),
                    fieldInfo.CreateEntity("承包方(代表名称)",CBF.FCBFMC, "String", 50),
                    fieldInfo.CreateEntity("承包方(代表)证件类型",CBF.FCBFZJLX, "String", 1),
                    fieldInfo.CreateEntity("承包方(代表)证件号码",CBF.FCBFZJHM, "String", 20),
                    fieldInfo.CreateEntity("承包方地址",CBF.FCBFDZ, "String", 100),
                    fieldInfo.CreateEntity("邮政编码",CBF.FYZBM, "String", 6),
                    fieldInfo.CreateEntity("联系电话",CBF.FLXDH, "String", 20,0,false,true,false),
                    fieldInfo.CreateEntity("承包方成员数量",CBF.FCBFCYSL, "Int",2,0,true,false),
                    fieldInfo.CreateEntity("承包方调查日期",CBF.FCBFDCRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("承包方调查员",CBF.FCBFDCY, "String", 50),
                    fieldInfo.CreateEntity("公示记事",CBF.FGSJS, "String", 254,0,false,false),
                    fieldInfo.CreateEntity("公示记事人",CBF.FGSJSR, "String", 50),
                    fieldInfo.CreateEntity("公示审核日期",CBF.FGSSHRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("公示审核人",CBF.FGSSHR, "String", 50),
                    fieldInfo.CreateEntity("承包方调查记事",CBF.FCBFDCJS, "String", 254,0,false,false),

                };
                return list;
            }
        }

        /// <summary>
        /// 家庭成员字段
        /// </summary>
        public virtual List<FieldInformation> CBF_JTCYZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBF_JTCYZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包方代码",CBF_JTCY.FCBFBM, "String", 18),
                    fieldInfo.CreateEntity("成员姓名",CBF_JTCY.FCYXM, "String", 50),
                    fieldInfo.CreateEntity("成员性别",CBF_JTCY.FCYXB, "String", 1),
                    fieldInfo.CreateEntity("成员证件类型",CBF_JTCY.FCYZJLX, "String", 1),
                    fieldInfo.CreateEntity("成员证件号码",CBF_JTCY.FCYZJHM, "String", 20),
                    fieldInfo.CreateEntity("与户主关系",CBF_JTCY.FYHZGX, "String", 2),
                    fieldInfo.CreateEntity("成员备注",CBF_JTCY.FCYBZ, "String", 1,0,false,true,false),
                    fieldInfo.CreateEntity("是否共有人",CBF_JTCY.FSFGYR, "String", 1,0,false,true,false),
                    fieldInfo.CreateEntity("成员备注说明",CBF_JTCY.FCYBZSM, "String", 254,0,false,false,false),
                };
                return list;
            }
        }

        /// <summary>
        /// 登记簿字段
        /// </summary>
        public virtual List<FieldInformation> CBJYQZDJBZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBJYQZDJBZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包经营权证(登记簿)代码",CBJYQZDJB.FCBJYQZBM, "String", 19),
                    fieldInfo.CreateEntity("发包方代码",CBJYQZDJB.FFBFBM, "String", 14),
                    fieldInfo.CreateEntity("承包方代码",CBJYQZDJB.FCBFBM, "String", 18),
                    fieldInfo.CreateEntity("承包方式",CBJYQZDJB.FCBFS, "String", 3),
                    fieldInfo.CreateEntity("承包期限",CBJYQZDJB.FCBQX, "String", 30),
                    fieldInfo.CreateEntity("承包期限起",CBJYQZDJB.FCBQXQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("承包期限止",CBJYQZDJB.FCBQXZ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("地块示意图",CBJYQZDJB.FDKSYT, "String",254,0,true,false),
                    fieldInfo.CreateEntity("原承包经营权证编号",CBJYQZDJB.FYCBJYQZBH, "String", 100,0,false,true,false),
                    fieldInfo.CreateEntity("承包经营权证流水号",CBJYQZDJB.FCBJYQZLSH, "String", 254,0,true,false),
                    fieldInfo.CreateEntity("登记簿附记",CBJYQZDJB.FDJBFJ, "String", 254,0,false,false,false),
                    fieldInfo.CreateEntity("登簿人",CBJYQZDJB.FDBR, "String", 50),
                    fieldInfo.CreateEntity("登记时间",CBJYQZDJB.FDJSJ, "DateTime", 8,0,true,false)
                };
                return list;
            }
        }

        /// <summary>
        /// 承包合同字段
        /// </summary>
        public virtual List<FieldInformation> CBHTZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBHTZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包合同代码",CBHT.FCBHTBM, "String", 19),
                    fieldInfo.CreateEntity("原承包合同代码",CBHT.FYCBHTBM, "String", 19,0,false),
                    fieldInfo.CreateEntity("发包方代码",CBHT.FFBFBM, "String", 14),
                    fieldInfo.CreateEntity("承包方代码",CBHT.FCBFBM, "String", 18),
                    fieldInfo.CreateEntity("承包方式",CBHT.FCBFS, "String", 3),
                    fieldInfo.CreateEntity("承包期限起",CBHT.FCBQXQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("承包期限止",CBHT.FCBQXZ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("确权(合同)总面积",CBHT.FHTZMJ, "Double",15,2,true,false),
                    fieldInfo.CreateEntity("确权(合同)总面积亩",CBHT.FHTZMJM, "Double",15,2,false,false,false),
                    fieldInfo.CreateEntity("承包地块总数",CBHT.FCBDKZS, "Int",3,0,true,false),
                    fieldInfo.CreateEntity("签订时间",CBHT.FQDSJ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("原承包合同总面积",CBHT.FYHTZMJ, "Double",15,2,false,false,false),
                    fieldInfo.CreateEntity("原承包合同总面积(亩)",CBHT.FYHTZMJM, "Double",15,2,false,false,false),
                };
                return list;
            }
        }

        /// <summary>
        /// 流转合同字段
        /// </summary>
        public virtual List<FieldInformation> LZHTZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("LZHTZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包合同代码",LZHT.FCBHTBM, "String", 19),
                    fieldInfo.CreateEntity("流转合同代码",LZHT.FLZHTBM, "String", 18),
                    fieldInfo.CreateEntity("承包方代码",LZHT.FCBFBM, "String", 18),
                    fieldInfo.CreateEntity("受让方代码",LZHT.FSRFBM, "String", 18),
                    fieldInfo.CreateEntity("流转方式",LZHT.FLZFS, "String", 3),
                    fieldInfo.CreateEntity("流转期限",LZHT.FLZQX, "String", 10),
                    fieldInfo.CreateEntity("流转前土地用途",LZHT.FLZQTDYT, "String",1,0,false,true,false),
                    fieldInfo.CreateEntity("流转后土地用途",LZHT.FLZHTDYT, "String",1,0,false,true,false),
                    fieldInfo.CreateEntity("流转期限开始日期",LZHT.FLZQXKSRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("流转期限结束日期",LZHT.FLZQXJSRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("流转面积",LZHT.FLZMJ, "Double",15,2,true,false),
                    fieldInfo.CreateEntity("流转面积(亩)",LZHT.FLZMJM, "Double",15,2,false,false,false),
                    fieldInfo.CreateEntity("流转地块数",LZHT.FLZDKS, "Int",3,0,true,false),
                    fieldInfo.CreateEntity("流转费用说明",LZHT.FLZJGSM, "String", 100),
                    fieldInfo.CreateEntity("合同签订日期",LZHT.FHTQDRQ, "DateTime",8,0,true,false)
                };
                return list;
            }
        }

        /// <summary>
        /// 经营权权证字段
        /// </summary>
        public virtual List<FieldInformation> CBJYQZZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBJYQZZD"), typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包经营权证(登记簿)",CBJYQZ.FCBJYQZBM, "String", 19),
                    fieldInfo.CreateEntity("发证机关",CBJYQZ.FFZJG, "String", 50),
                    fieldInfo.CreateEntity("发证日期",CBJYQZ.FFZRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("权证是否领取",CBJYQZ.FQZSFLQ, "String", 1),
                    fieldInfo.CreateEntity("权证领取日期",CBJYQZ.FQZLQRQ, "DateTime", 8,0,false,false),
                    fieldInfo.CreateEntity("权证领取人姓名",CBJYQZ.FQZLQRXM, "String", 50,0,false,true),
                    fieldInfo.CreateEntity("权证领取人证件类型",CBJYQZ.FQZLQRZJLX, "String", 1,0,false),
                    fieldInfo.CreateEntity("权证领取人证件号码",CBJYQZ.FQZLQRZJHM, "String", 20,0,false)
                };
                return list;
            }
        }

        /// <summary>
        /// 经营权权补发证字段
        /// </summary>
        public virtual List<FieldInformation> CBJYQZ_QZBFZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBJYQZ_QZBFZD"),
                    typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包经营权证(登记簿)代码",CBJYQZ_QZBF.FCBJYQZBM, "String", 19),
                    fieldInfo.CreateEntity("权证补发原因",CBJYQZ_QZBF.FQZBFYY, "String", 200),
                    fieldInfo.CreateEntity("补发日期",CBJYQZ_QZBF.FBFRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("权证补发领取日期",CBJYQZ_QZBF.FQZBFLQRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("权证补发领取人姓名",CBJYQZ_QZBF.FQZBFLQRXM, "String", 50),
                    fieldInfo.CreateEntity("权证补发领取人证件类型",CBJYQZ_QZBF.FBFLQRZJLX, "String", 1),
                    fieldInfo.CreateEntity("权证补发领取人证件号码",CBJYQZ_QZBF.FBFLQRZJHM, "String", 20)
                };
                return list;
            }
        }

        /// <summary>
        /// 经营权权换发证字段
        /// </summary>
        public virtual List<FieldInformation> CBJYQZ_QZHFZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBJYQZ_QZHFZD"),
                    typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包经营权证(登记簿)代码",CBJYQZ_QZHF.FCBJYQZBM, "String", 19),
                    fieldInfo.CreateEntity("权证换发原因",CBJYQZ_QZHF.FQZHFYY, "String", 200),
                    fieldInfo.CreateEntity("换发日期",CBJYQZ_QZHF.FHFRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("权证换发领取日期",CBJYQZ_QZHF.FQZHFLQRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("权证换发领取人姓名",CBJYQZ_QZHF.FQZHFLQRXM, "String", 50),
                    fieldInfo.CreateEntity("权证换发领取人证件类型",CBJYQZ_QZHF.FHFLQRZJLX, "String", 1),
                    fieldInfo.CreateEntity("权证换发领取人证件号码",CBJYQZ_QZHF.FHFLQRZJHM, "String", 20)
                };
                return list;
            }
        }

        /// <summary>
        /// 权证注销
        /// </summary>
        public virtual List<FieldInformation> CBJYQZ_QZZXZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("CBJYQZ_QZZXZD"),
                     typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包经营权证(登记簿)代码",CBJYQZ_QZZX.FCBJYQZBM, "String", 19),
                    fieldInfo.CreateEntity("注销原因",CBJYQZ_QZZX.FZXYY, "String", 200),
                    fieldInfo.CreateEntity("注销日期",CBJYQZ_QZZX.FZXRQ, "DateTime", 8,0,true,false)
                };
                return list;
            }
        }

        /// <summary>
        /// 附件字段
        /// </summary>
        public virtual List<FieldInformation> QSLYZLFJZD
        {
            get
            {
                var list = ToolSerialization.DeserializeXml(CheckHelper.GetConfigXmlPath("QSLYZLFJZD"),
                      typeof(List<FieldInformation>)) as List<FieldInformation>;
                if (list == null)
                    list = new List<FieldInformation>()
                    {
                    fieldInfo.CreateEntity("承包经营权证(登记簿)代码",QSLYZLFJ.FCBJYQZBM, "String", 19),
                    fieldInfo.CreateEntity("资料附件编号",QSLYZLFJ.FZLFJBH, "String", 20),
                    fieldInfo.CreateEntity("资料附件名称",QSLYZLFJ.FZLFJMC, "String", 100),
                    fieldInfo.CreateEntity("资料附件日期",QSLYZLFJ.FZLFJRQ, "DateTime", 8,0,true,false),
                    fieldInfo.CreateEntity("附件",QSLYZLFJ.FFJ, "String",254,0,true,false)
                };
                return list;
            }
        }

        #endregion

        #region 信息

        /// <summary>
        /// 创建实例
        /// </summary>
        public static bool CreateEntity(string name, string type, int length = 0,
            bool nullable = true, bool checkLength = true, bool isNecessary = true)
        {
            return true;
        }

        #endregion
    }
}
