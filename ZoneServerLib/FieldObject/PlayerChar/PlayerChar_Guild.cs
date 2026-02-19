using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShared;
using Message.Relation.Protocol.RZ;
using Message.Manager.Protocol.MZ;
using Logger;
using CommonUtility;
using Message.Zone.Protocol.ZR;
using DataUtility;
using Message.Gate.Protocol.GateZ;
using Message.Gate.Protocol.GateC;
using EnumerateUtility;
using System.Text.RegularExpressions;
using RedisUtility;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //家族
        public int FamilyId = 0;
        public string GuildName = "";
        public int GuildIcon = 0;

        public MSG_GateZ_CREATE_GUILD createGuildMsg = null;

        public int GetFamilyId()
        {
            return FamilyId;
        }
        public string GetGuidName()
        {
            return GuildName;
        }
        public int GetGuidIcon()
        {
            return GuildIcon;
        }

        public void CreateGuild(MSG_GateZ_CREATE_GUILD msg)
        {
            MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
            if (createGuildMsg !=null)
            {
                //操作太频繁
                Log.WarnLine($"player {Uid} create guild failed: operate so quickly");
                response.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查创建条件
            ErrorCode resultCode = CheckGuild();
            if (resultCode != ErrorCode.Success)
            {            
                response.Result = (int)resultCode;
                Write(msg);
                return;
            }
            createGuildMsg = msg;
            //发到realation
            server.SendToRelation(new MSG_ZR_MAX_GUILDID());
        }

        /// <summary>
        /// 创建公会
        /// </summary>
        /// <param name="createGuildMsg"></param>
        public void CreateGuild(int guildId)
        {
            MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
            if (createGuildMsg == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
            }
          
            if (GetCoins(CurrenciesType.diamond)< GuildLib.CreateGuildCost)
            {
                //frTODO: 这里是回调的 ，有可能存在钻石回调前CheckGuild()是够的，回调时不够的情况？
                //可先扣，如果不成功再给返回钻石?????
                Log.ErrorLine("player {0} create guild got an error,diamond not enough",uid, GetCoins(CurrenciesType.diamond)); 
            }
            else
            {
                //扣钻石
                //frTODO:在这里口钻石是否合理！?????
                //frTODO:写入数据
                //frTODO:发公告？？？？
            }
            //创建完成。
            ClearCreateGuildMsg();

        }

        /// <summary>
        /// 清除创建信息(以便重新创建)
        /// </summary>
        public void ClearCreateGuildMsg()
        {
            createGuildMsg = null;
        }

        private ErrorCode CheckGuild()
        {
            //frTODO:检查是否可以创建公会
            //检查功能开启条件。
            //检查消耗货币数
            return ErrorCode.Success;
        }

       
    }

    //public class FamilyPractice
    //{
    //    private int id;
    //    public int Id
    //    { get { return id; } }

    //    private int level;
    //    public int Level
    //    { get { return level; } }

    //    private int exp;
    //    public int Exp
    //    { get { return exp; } }

    //    public FamilyPractice(int id, int exp)
    //    {
    //        this.id = id;
    //        this.exp = exp;
    //        level = Calc.GetFamilyPracticeLevel(exp);
    //    }

    //    // 升级时返回true 否则返回false
    //    public bool AddExp(int exp)
    //    {
    //        this.exp += exp;
    //        int newLevel = Calc.GetFamilyPracticeLevel(this.exp);
    //        if (level < newLevel)
    //        {
    //            level = newLevel;
    //            return true;
    //        }
    //        return false;
    //    }
    //}
}
