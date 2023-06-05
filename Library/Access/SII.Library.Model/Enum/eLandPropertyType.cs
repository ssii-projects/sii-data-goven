using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Model
{
    /// <summary>
    /// 土地所有权类型
    /// </summary>
    public enum eLandPropertyType
    {
        /// <summary>
        /// 国有土地所有权
        /// </summary>
        Stated = 10,

        /// <summary>
        /// 国有土地使用权
        /// </summary>
        UsageState = 20,

        /// <summary>
        /// 集体土地所有权
        /// </summary>
        Collectived = 30,

        /// <summary>
        /// 村民小组
        /// </summary>
        GroupOfPeople = 31,

        /// <summary>
        /// 村集体经济组织
        /// </summary>
        VillageCollective = 32,

        /// <summary>
        /// 乡集体经济组织
        /// </summary>
        TownCollective = 33,

        /// <summary>
        /// 其它农民集体经济组织
        /// </summary>
        OtherCollective = 34,

        /// <summary>
        /// 集体土地使用权
        /// </summary>
        UsageCollective = 40,

        /// <summary>
        /// 其它新型体经济组织
        /// </summary>
        Other = 0
    };
}
