using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public static class GuildLib
    {
        /// <summary>
        /// 公会名字长度上限
        /// </summary>
        public static int NameLenLimit;
        /// <summary>
        /// 公会宣言长度上限
        /// </summary>
        public static int SignatureLenLimit;
        /// <summary>
        /// 公会宣言默认值
        /// </summary>
        public static string SignatureDeafault;
        /// <summary>
        /// 创建公会消耗钻石数
        /// </summary>
        public static int CreateGuildCost;
        public static void LoodDatas()
        {
            // Init GuildConfig
            Data guildConfig = DataListManager.inst.GetData("GuildConfig", 1);
            NameLenLimit = guildConfig.GetInt("NameLenLimit");
            SignatureLenLimit = guildConfig.GetInt("SignatureLenLimit");
            SignatureDeafault = guildConfig.GetString("SignatureDeafault");
            CreateGuildCost = guildConfig.GetInt("CreateGuildCost");
        }
    }
}
