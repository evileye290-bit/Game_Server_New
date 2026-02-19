using DBUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ThemeFireworkManager
    {
        private PlayerChar owner { get; set; }

        private ThemeFireworkInfo info = new ThemeFireworkInfo();
        public ThemeFireworkInfo Info { get { return info; } }

        public ThemeFireworkManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(ThemeFireworkInfo info)
        {
            this.info = info;
        }

        public void UpdateThemeFireworkUseInfo(int score, bool highest, int num)
        {
            info.Score += score * num;
            if (highest)
            {
                info.HighestUseCount += num;
            }
            SyncDbUpdateThemeFireworkUseInfo();
        }

        public void UpdateScoreRewards(int rewardId)
        {
            info.ScoreRewards.Add(rewardId);
            SyncDbUpdateScoreRewards();
        }

        public void UpdateHighestUseCountRewards(int rewardId)
        {
            info.HighestUseCountRewards.Add(rewardId);
            SyncDbUpdateHighestUseCountRewards();
        }

        public void Clear()
        {
            info.Clear();
        }

        private void SyncDbUpdateThemeFireworkUseInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateThemeFireworkUseInfo(owner.Uid, info.Score, info.HighestUseCount));
        }

        private void SyncDbUpdateScoreRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateThemeFireworkScoreRewards(owner.Uid, string.Join("|", info.ScoreRewards)));
        }

        private void SyncDbUpdateHighestUseCountRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateThemeFireworkUseCountRewards(owner.Uid, string.Join("|", info.HighestUseCountRewards)));
        }

        public MSG_ZMZ_THEME_FIREWORK GenerateTransformMsg()
        {
            MSG_ZMZ_THEME_FIREWORK msg = new MSG_ZMZ_THEME_FIREWORK();
            msg.Score = info.Score;
            msg.HighestUseCount = info.HighestUseCount;
            msg.ScoreRewards.AddRange(info.ScoreRewards);
            msg.HighestUseCountRewards.AddRange(info.HighestUseCountRewards);
            return msg;
        }

        public void LoadTransformMsg(MSG_ZMZ_THEME_FIREWORK msg)
        {
            info.Score = msg.Score;
            info.HighestUseCount = msg.HighestUseCount;
            info.ScoreRewards.AddRange(msg.ScoreRewards);
            info.HighestUseCountRewards.AddRange(msg.HighestUseCountRewards);
        }
    }
}
