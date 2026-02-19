using RedisUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class BaseRankClient
    {
        public int Uid = 0;
        public int MainId = 0;

        public string Name = string.Empty;
        public int Icon = 0;
        public int GodType = 0;
        public bool ShowDIYIcon = false;
        public int IconFrame = 0;

        public int Level = 0;
        public int HeroId = 0;
        public int BattlePower = 0;
        public int CampId = 0;
        public int Family = 0;
        /// <summary>
        /// 声望
        /// </summary>
        public int HisPrestige = 0;

        public bool IsOnline = false;
        public int LastLogoutTime = 0;

        public int Rank = 0;

        public void Init(PlayerRankInfo info)
        {
            Uid = info.Uid;
            MainId = info.MainId;
            Name = info.Name;
            Icon = info.Icon;
            GodType = info.GodType;
            ShowDIYIcon = info.ShowDIYIcon;
            IconFrame = info.IconFrame;
            Level = info.Level;
            CampId = info.CampId;
            BattlePower = info.BattlePower;

            HisPrestige = info.HisPrestige;


            IsOnline = info.IsOnline;
            LastLogoutTime = info.LastLogoutTime;
        }
    }
}
