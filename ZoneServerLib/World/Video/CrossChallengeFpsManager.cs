using System.Collections.Generic;
using System.Linq;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class CrossChallengeFpsManager : CrossBattleFpsManager
    {
        public CrossChallengeFpsManager(DungeonMap curMap) : base(curMap)
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

        protected override MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO GetChallengerInfo(PlayerChar player, MSG_VIDEO_HERO_WEAPON_LIST weaponInfo)
        {
            MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO msg = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO();
            msg.Result = (int)ErrorCode.Success;

            List<MSG_ZGC_HERO_INFO> list;
            Dictionary<int, List<MSG_ZGC_HERO_INFO>> dic = new Dictionary<int, List<MSG_ZGC_HERO_INFO>>();

            foreach (var item in player.HeroMng.CrossChallengeQueue.OrderBy(x => x.Key))
            {
                foreach (var kv in item.Value)
                {
                    MSG_ZGC_HERO_INFO info = GetPlayerHeroInfoMsg(kv.Value);
                    if (dic.TryGetValue(info.CrossChallengeQueueNum, out list))
                    {
                        list.Add(info);
                    }
                    else
                    {
                        list = new List<MSG_ZGC_HERO_INFO>();
                        list.Add(info);
                        dic.Add(info.CrossChallengeQueueNum, list);
                    }

                    weaponInfo.HiddenWeapon.Add(new MSG_VIDEO_HERO_WEAPON_INFO()
                    {
                        HeroId = kv.Value.Id,
                        HiddenWeaponId = player.HiddenWeaponManager.GetHeroEquipWeaponTypeId(kv.Value.Id)
                    });
                }
            }

            int power = 0;
            foreach (var kv in dic)
            {
                CROSS_BATTLE_HERO_QUEUE info = new CROSS_BATTLE_HERO_QUEUE();
                foreach (var item in kv.Value)
                {
                    info.BattlePower += item.Power;
                    info.HeroList.Add(item);
                }
                msg.Queue.Add(info);
            }

            //自己信息
            msg.Info = new CROSS_CHALLENGER_INFO();
            msg.Info.BaseInfo = GetPlayerBaseInfo(player);
            msg.Info.BaseInfo.BattlePower = power;
            msg.Info.CrossLevel = player.CrossInfoMng.Info.Level;
            msg.Info.CrossStar = player.CrossInfoMng.Info.Star;

            return msg;
        }

        protected override MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO GetChallengerInfo(PlayerRankBaseInfo challenger, MSG_VIDEO_HERO_WEAPON_LIST weaponInfo)
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
                    info.CrossChallengeQueueNum = item.Key;

                    if (dic.TryGetValue(info.CrossChallengeQueueNum, out list))
                    {
                        list.Add(info);
                    }
                    else
                    {
                        list = new List<MSG_ZGC_HERO_INFO>();
                        list.Add(info);
                        dic.Add(info.CrossChallengeQueueNum, list);
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
                    info.BattlePower += item.Power;
                    info.HeroList.Add(item);
                }
                power += info.BattlePower;
                msg.Queue.Add(info);
            }

            //自己信息
            msg.Info = new CROSS_CHALLENGER_INFO();
            msg.Info.BaseInfo = GetPlayerBaseInfo(defender);
            msg.Info.CrossLevel = defender.CrossLevel;
            msg.Info.CrossStar = defender.CrossStar;
            if (power < int.MaxValue)
            {
                msg.Info.BaseInfo.BattlePower = (int)power;
            }
            msg.Info.BaseInfo.BattlePower64 = power;

            return msg;
        }

    }
}