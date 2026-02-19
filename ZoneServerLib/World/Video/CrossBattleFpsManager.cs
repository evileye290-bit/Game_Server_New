using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class CrossBattleFpsManager : BattleFpsManager
    {
        public CrossBattleFpsManager(DungeonMap curMap) : base(curMap)
        { 
        }

        public override MSG_ZGC_VIDEO_BTTLE_INFO GenerateVideoBattleInfo(PlayerChar attacker, PlayerRankBaseInfo challenger)
        {
            MSG_ZGC_VIDEO_BTTLE_INFO msg = new MSG_ZGC_VIDEO_BTTLE_INFO();
            msg.MapId = curMap.MapId;
            msg.AttackerWeapon = new MSG_VIDEO_HERO_WEAPON_LIST();
            msg.DefenderWeapon = new MSG_VIDEO_HERO_WEAPON_LIST();

            msg.CrossBattleAttacker = GetChallengerInfo(attacker, msg.AttackerWeapon);
            msg.CrossBattleDefender = GetChallengerInfo(challenger, msg.DefenderWeapon);

            return msg;
        }

        protected virtual MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO GetChallengerInfo(PlayerChar player, MSG_VIDEO_HERO_WEAPON_LIST weaponInfo)
        {
            MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO msg = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO();
            msg.Result = (int)ErrorCode.Success;

            List<MSG_ZGC_HERO_INFO> list;
            Dictionary<int, List<MSG_ZGC_HERO_INFO>> dic = new Dictionary<int, List<MSG_ZGC_HERO_INFO>>();

            foreach (var item in player.HeroMng.CrossQueue.OrderBy(x=>x.Key))
            {
                foreach (var kv in item.Value)
                {
                    MSG_ZGC_HERO_INFO info = GetPlayerHeroInfoMsg(kv.Value);
                    if (dic.TryGetValue(info.CrossQueueNum, out list))
                    {
                        list.Add(info);
                    }
                    else
                    {
                        list = new List<MSG_ZGC_HERO_INFO>();
                        list.Add(info);
                        dic.Add(info.CrossQueueNum, list);
                    }

                    weaponInfo.HiddenWeapon.Add(new MSG_VIDEO_HERO_WEAPON_INFO()
                    {
                        HeroId = kv.Value.Id,
                        HiddenWeaponId = player.HiddenWeaponManager.GetHeroEquipWeaponTypeId(kv.Value.Id)
                    });
                }
            }

            long power = 0;
            foreach (var kv in dic)
            {
                CROSS_BATTLE_HERO_QUEUE info = new CROSS_BATTLE_HERO_QUEUE();
                foreach (var item in kv.Value)
                {
                    power += item.Power;
                    info.BattlePower64 += item.Power;
                    info.HeroList.Add(item);
                }

                info.BattlePower = info.BattlePower64.ToIntValue();
                msg.Queue.Add(info);
            }

            //自己信息
            msg.Info = new CROSS_CHALLENGER_INFO();
            msg.Info.BaseInfo = GetPlayerBaseInfo(player);
            msg.Info.BaseInfo.BattlePower64 = power;
            msg.Info.BaseInfo.BattlePower = power.ToIntValue();
            msg.Info.CrossLevel = player.CrossInfoMng.Info.Level;
            msg.Info.CrossStar = player.CrossInfoMng.Info.Star;

            return msg;
        }

        protected virtual MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO GetChallengerInfo(PlayerRankBaseInfo challenger, MSG_VIDEO_HERO_WEAPON_LIST weaponInfo)
        {
            PlayerCrossFightInfo defender = challenger as PlayerCrossFightInfo;

            MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO msg = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO();
            msg.Result = (int)ErrorCode.Success;

            List<MSG_ZGC_HERO_INFO> list;
            Dictionary<int, List<MSG_ZGC_HERO_INFO>> dic = new Dictionary<int, List<MSG_ZGC_HERO_INFO>>();

            foreach (var item in defender.HeroQueue.OrderBy(x => x.Key))
            {
                foreach (var kv in item.Value)
                {
                    MSG_ZGC_HERO_INFO info = GetPlayerHeroInfoMsg(kv.Value);
                    info.CrossQueueNum = item.Key;

                    if (dic.TryGetValue(info.CrossQueueNum, out list))
                    {
                        list.Add(info);
                    }
                    else
                    {
                        list = new List<MSG_ZGC_HERO_INFO>();
                        list.Add(info);
                        dic.Add(info.CrossQueueNum, list);
                    }

                    List<int> weaponList = kv.Value.HiddenWeapon.ToList(':');
                    if (weaponList.Count == 2)
                    {
                        weaponInfo.HiddenWeapon.Add(new MSG_VIDEO_HERO_WEAPON_INFO()
                        {
                            HeroId = kv.Value.Id,
                            HiddenWeaponId = weaponList[0]
                        });
                    }
                }
            }

            long power = 0;
            foreach (var kv in dic)
            {
                CROSS_BATTLE_HERO_QUEUE info = new CROSS_BATTLE_HERO_QUEUE();
                foreach (var item in kv.Value)
                {
                    power += item.Power;
                    info.BattlePower64 += item.Power;
                    info.HeroList.Add(item);
                }

                info.BattlePower = info.BattlePower64.ToIntValue();

                msg.Queue.Add(info);
            }

            //自己信息
            msg.Info = new CROSS_CHALLENGER_INFO();
            msg.Info.BaseInfo = GetPlayerBaseInfo(defender);
            msg.Info.BaseInfo.BattlePower64 = power;
            msg.Info.BaseInfo.BattlePower = power.ToIntValue();
            msg.Info.CrossLevel = defender.CrossLevel;
            msg.Info.CrossStar = defender.CrossStar;

            return msg;
        }

        protected PLAYER_BASE_INFO GetPlayerBaseInfo(PlayerChar challenger)
        {
            PLAYER_BASE_INFO baseInfo = new PLAYER_BASE_INFO();
            baseInfo.Uid = challenger.Uid;
            baseInfo.Name = challenger.Name;
            baseInfo.Sex = challenger.Sex;
            baseInfo.Level = challenger.Level;
            baseInfo.HeroId = challenger.HeroId;
            baseInfo.GodType = challenger.GodType;

            baseInfo.Icon = challenger.Icon;
            //baseInfo.IconFrame = challenger.Icon;
            baseInfo.ShowDIYIcon = false;
            if (challenger.IsRobot)
            {
                baseInfo.MainId = curMap.server.MainId;
            }
            else
            {
                baseInfo.MainId = BaseApi.GetMainIdByUid(challenger.Uid);
            }
            return baseInfo;
        }

        protected PLAYER_BASE_INFO GetPlayerBaseInfo(PlayerCrossFightInfo challenger)
        {
            PLAYER_BASE_INFO baseInfo = new PLAYER_BASE_INFO();
            baseInfo.Uid = challenger.Uid;
            baseInfo.Name = challenger.Name;
            baseInfo.Sex = challenger.Sex;
            baseInfo.Level = challenger.Level;
            baseInfo.LadderLevel = challenger.LadderLevel;
            baseInfo.HeroId = challenger.HeroId;
            baseInfo.GodType = challenger.GodType;
            baseInfo.BattlePower64 = challenger.BattlePower;
            if (challenger.BattlePower < int.MaxValue)
            {
                baseInfo.BattlePower = (int)challenger.BattlePower;
            }
            baseInfo.Icon = challenger.Icon;
            baseInfo.IconFrame = challenger.IconFrame;
            baseInfo.ShowDIYIcon = false;
            if (challenger.IsRobot)
            {
                baseInfo.MainId = curMap.server.MainId;
            }
            else
            {
                baseInfo.MainId = BaseApi.GetMainIdByUid(challenger.Uid);
            }
            return baseInfo;
        }


        protected MSG_ZGC_HERO_INFO GetPlayerHeroInfoMsg(HeroInfo heroInfo)
        {
            MSG_ZGC_HERO_INFO info = new MSG_ZGC_HERO_INFO();
            info.Id = heroInfo.Id;
            info.Level = heroInfo.Level;
            info.SoulSkillLevel = heroInfo.SoulSkillLevel;
            info.AwakenLevel = heroInfo.AwakenLevel;
            info.StepsLevel = heroInfo.StepsLevel;
            info.GodType = heroInfo.GodType;
            info.EquipIndex = heroInfo.EquipIndex;
            info.CrossQueueNum = heroInfo.CrossQueueNum;
            info.CrossPositionNum = heroInfo.CrossPositionNum;
            info.CrossChallengeQueueNum = heroInfo.CrossChallengeQueueNum;
            info.CrossChallengePositionNum = heroInfo.CrossChallengePositionNum;
            info.Power = heroInfo.GetBattlePower();
            return info;
        }

        protected MSG_ZGC_HERO_INFO GetPlayerHeroInfoMsg(RobotHeroInfo heroInfo)
        {
            MSG_ZGC_HERO_INFO info = new MSG_ZGC_HERO_INFO();
            info.Id = heroInfo.HeroId;
            info.Level = heroInfo.Level;
            //info.SoulSkillLevel = heroInfo.SoulSkillLevel;
            info.AwakenLevel = heroInfo.AwakenLevel;
            info.StepsLevel = heroInfo.StepsLevel;
            info.SoulSkillLevel = heroInfo.SoulSkillLevel;
            info.GodType = heroInfo.GodType;
            info.EquipIndex = heroInfo.EquipIndex;
            info.CrossPositionNum = heroInfo.HeroPos;
            info.Power = heroInfo.BattlePower;
            return info;
        }
    }
}
