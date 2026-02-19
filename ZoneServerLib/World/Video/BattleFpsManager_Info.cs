using System.Collections.Generic;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    partial class BattleFpsManager
    {
        public virtual MSG_ZGC_VIDEO_BTTLE_INFO GenerateVideoBattleInfo(PlayerChar attacker, PlayerRankBaseInfo challenger)
        {
            MSG_ZGC_VIDEO_BTTLE_INFO msg = new MSG_ZGC_VIDEO_BTTLE_INFO();
            msg.MapId = curMap.MapId;

            msg.Attacker = GetChallengerInfo(attacker);
            msg.Defender = GetChallengerInfo(challenger);

            return msg;
        }

        public MSG_ZGC_VIDEO_BTTLE_INFO GenerateVideoBattleInfo(PlayerRankBaseInfo attacker, PlayerRankBaseInfo challenger)
        {
            MSG_ZGC_VIDEO_BTTLE_INFO msg = new MSG_ZGC_VIDEO_BTTLE_INFO();
            msg.MapId = curMap.MapId;

            msg.Attacker = GetChallengerInfo(attacker);
            msg.Defender = GetChallengerInfo(challenger);

            return msg;
        }

        private MSG_ZGC_VIDEO_SHOW_INFO GetChallengerInfo(PlayerChar attacker)
        {
            long battlePower = 0;
            MSG_ZGC_VIDEO_SHOW_INFO msg = new MSG_ZGC_VIDEO_SHOW_INFO();
            msg.Info = GetArenaRankBaseInfo(attacker);
            BuidHeroInfo(msg.HeroList, attacker, out battlePower);
            msg.Info.BaseInfo.BattlePower64 = battlePower;
            msg.Info.BaseInfo.BattlePower = battlePower.ToIntValue();
            return msg;
        }

        private MSG_ZGC_VIDEO_SHOW_INFO GetChallengerInfo(PlayerRankBaseInfo challenger)
        {
            int battlePower = 0;
            MSG_ZGC_VIDEO_SHOW_INFO msg = new MSG_ZGC_VIDEO_SHOW_INFO();
            msg.Info = GetArenaRankBaseInfo(challenger);
            BuidHeroInfo(msg.HeroList, challenger, out battlePower);

            if (battlePower == 0 && challenger.IsRobot)
            {
                battlePower = 10000;
            }
            msg.Info.BaseInfo.BattlePower = battlePower;
            return msg;
        }

        private void BuidHeroInfo(RepeatedField<CHALLENGER_HERO_INFO> heroList, PlayerChar player, out long BattlePower)
        {
            long battlePower = 0;
            foreach (var item in player.HeroMng.GetAllHeroPos())
            {
                HeroInfo heroInfo = player.HeroMng.GetHeroInfo(item.Item1);
                if (heroInfo == null) continue;

                battlePower += heroInfo.GetBattlePower();
                CHALLENGER_HERO_INFO info = new CHALLENGER_HERO_INFO
                {
                    Id = heroInfo.Id,
                    Level = heroInfo.Level,
                    AwakenLevel = heroInfo.AwakenLevel,
                    StepsLevel = heroInfo.StepsLevel,
                    GodType = heroInfo.GodType,
                };

                heroList.Add(info);
            }
            BattlePower = battlePower;
        }

        private void BuidHeroInfo(RepeatedField<CHALLENGER_HERO_INFO> heroList, PlayerRankBaseInfo challenger, out int BattlePower)
        {
            int battlePower = 0;
            challenger.HeroInfos.ForEach(heroInfo =>
            {
                CHALLENGER_HERO_INFO info = new CHALLENGER_HERO_INFO
                {
                    Id = heroInfo.HeroId,
                    Level = heroInfo.Level,
                    AwakenLevel = heroInfo.AwakenLevel,
                    StepsLevel = heroInfo.StepsLevel,
                    SoulSkillLevel = heroInfo.SoulSkillLevel,
                    GodType = heroInfo.GodType,
                };
                battlePower += heroInfo.BattlePower;
            });
            BattlePower = battlePower;
        }

        private CHALLENGER_INFO GetArenaRankBaseInfo(PlayerChar player)
        {
            CHALLENGER_INFO info = new CHALLENGER_INFO();
            info.IsRobot = false;

            info.BaseInfo = new PLAYER_BASE_INFO();
            info.BaseInfo.Uid = player.Uid;
            info.BaseInfo.Name = player.Name;
            info.BaseInfo.Sex = player.Sex;
            info.BaseInfo.Level = player.Level;
            info.BaseInfo.BattlePower = info.DefensivePower;
            info.BaseInfo.HeroId = player.HeroId;
            info.BaseInfo.GodType = player.GodType;

            return info;
        }

        private CHALLENGER_INFO GetArenaRankBaseInfo(PlayerRankBaseInfo challenger)
        {
            CHALLENGER_INFO info = new CHALLENGER_INFO();
            info.Rank = challenger.Rank;
            info.IsRobot = challenger.IsRobot;
            info.Defensive.AddRange(challenger.Defensive);
            info.HeroGod.AddRange(challenger.HeroGod);
            info.DefensivePower = challenger.DefensivePower;

            info.BaseInfo = new PLAYER_BASE_INFO();
            info.BaseInfo.Uid = challenger.Uid;
            info.BaseInfo.Name = challenger.Name;
            info.BaseInfo.Sex = challenger.Sex;
            info.BaseInfo.Level = challenger.Level;
            info.BaseInfo.LadderLevel = challenger.LadderLevel;
            info.BaseInfo.HeroId = challenger.HeroId;
            info.BaseInfo.GodType = challenger.GodType;
            info.BaseInfo.Icon = challenger.Icon;
            info.BaseInfo.ShowDIYIcon = challenger.ShowDIYIcon;
            info.BaseInfo.IconFrame = challenger.IconFrame;
            info.BaseInfo.BattlePower64 = challenger.BattlePower;
            if (challenger.BattlePower < int.MaxValue)
            {
                info.BaseInfo.BattlePower = (int)challenger.BattlePower;
            }

            return info;
        }
    }
}
