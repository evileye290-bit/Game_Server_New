using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public ThemePassMamager ThemePassMamager { get; set; }
        public void InitThemePassManager()
        {
            ThemePassMamager = new ThemePassMamager(this);
        }

        public void InitThemePassInfo(Dictionary<int, DbThemePassItem> list)
        {
            ThemePassMamager.InitThemePassInfo(list);
        }

        public void SendThemePassInfo()
        {          
            MSG_ZGC_THEME_PASS_LIST msg = ThemePassMamager.GenerateThemePassInfo();
            Write(msg);
        }

        public void GetThemePassReward(int themeType, bool getAll, bool isSuper, List<int> rewardLevels)
        {
            if (getAll)
            {
                ThemePassMamager.GetAllLevelReward(themeType, getAll, isSuper, rewardLevels);
            }
            else
            {
                ThemePassMamager.GetLevelReward(themeType, getAll, isSuper, rewardLevels);
            }
        }

        public void CheckUpdateThemePass()
        {
            Dictionary<int, RechargeGiftModel> themePassDic = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemePass);
            foreach (var item in themePassDic)
            {
                int timeType = 0;
                if (item.Value.StartTime != DateTime.MinValue)
                {
                    timeType = 1;
                }
                else if (item.Value.ServerOpenDayEnd > 0)
                {
                    timeType = 2;
                }
                ThemePassMamager.CheckUpdateThemePass(timeType, item.Value);
            }
            SendThemePassInfo();
        }

        /// <summary>
        /// 主题通行证加经验
        /// </summary>
        public void AddThemePassExp(int expItemId, int expItemCount)
        {
            ThemePassMamager.AddThemePassExp(expItemId, expItemCount);
        }

        public ZMZ_THEME_INFO GenerateThemeInfoTransformMsg()
        {
            ZMZ_THEME_INFO msg = new ZMZ_THEME_INFO();
            ThemePassMamager.GenerateThemePassTransformMsg(msg);
            ThemeBossManager.GenerateThemeBossTransformMsg(msg);
            CarnivalBossMng.GenerateCarnivalBossTransformMsg(msg);
            return msg;
        }

        public void LoadThemeInfoTransform(ZMZ_THEME_INFO info)
        {
            ThemePassMamager.LoadThemePassInfoTransform(info);
            ThemeBossManager.LoadThemeBossInfoTransform(info.ThemeBossInfo);
            CarnivalBossMng.LoadCarnivalBossInfoTransform(info.CarnivalBossInfo);
        }
    }
}
