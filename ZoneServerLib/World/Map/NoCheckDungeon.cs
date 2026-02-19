using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class NoCheckDungeon : DungeonMap
    {
        public NoCheckDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public override void OnPlayerEnter(PlayerChar player, bool reEnter = false)
        {
            base.OnPlayerEnter(player, reEnter);

            //同步前端玩家角色属性
            MSG_ZGC_DUNGEON_EQUIPED_HERO msg = new MSG_ZGC_DUNGEON_EQUIPED_HERO();

            //羁绊
            //foreach (var kv in player.HeroMng.NatureRatio)
            //{
            //    msg.NatureRatio.Add(kv.Key, kv.Value);
            //}

            //伙伴信息
            //Dictionary<int, int> equipHeroList = player.HeroMng.GetEquipHeroIdList().OrderBy(x=>x.Key).ToDictionary(key=>key.Key, value=>value.Value);
            //foreach (var kv in equipHeroList)
            //{
            //    HeroModel model = HeroLibrary.GetHeroModel(kv.Value);
            //    HeroInfo heroInfo = player.HeroMng.GetHeroInfo(kv.Value);
            //    if (model == null || heroInfo == null)
            //    {
            //        continue;
            //    }

            //    MSG_ZGC_EQUIP_HERO_INFO heroMsg = GenerateEquipedHeroInfo(heroInfo);
            //    heroMsg.IsPlayer = kv.Value == player.HeroId;
            //    GenerateHeroInfo(player, heroMsg, model);
            //    msg.HeroNature.Add(heroMsg);
            //}

            foreach (var item in player.HeroMng.GetAllHeroPos())
            {
                HeroModel model = HeroLibrary.GetHeroModel(item.Item1);
                HeroInfo heroInfo = player.HeroMng.GetHeroInfo(item.Item1);
                if (model == null || heroInfo == null)
                {
                    continue;
                }

                MSG_ZGC_EQUIP_HERO_INFO heroMsg = player.GenerateEquipedHeroInfo(heroInfo);
                heroMsg.IsPlayer = item.Item1 == player.HeroId;
                GenerateHeroInfo(player, heroMsg, model);
                heroMsg.Pos = item.Item2;
                msg.HeroNature.Add(heroMsg);
            }
            

            //player 信息
            //HeroInfo playerHero = player.HeroMng.GetHeroInfo(player.HeroId);
            //MSG_ZGC_EQUIP_HERO_INFO playerMsg = GenerateEquipedHeroInfo(playerHero);
            //GenerateHeroInfo(player, playerMsg, HeroLibrary.GetHeroModel(player.HeroId));
            //playerMsg.IsPlayer = true;

            //msg.HeroNature.Add(playerMsg);

            player.Write(msg);
        }


        private void GenerateHeroInfo(PlayerChar player, MSG_ZGC_EQUIP_HERO_INFO msg, HeroModel model)
        {
            List<int> equipedSoulRings = new List<int>();
            foreach (var skillId in model.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null) continue;

                // 魂环技， 通过魂环等级确定技能等级
                if (skillModel.SoulRingPos <= 0 || equipedSoulRings.Contains(skillModel.SoulRingPos)) continue;

                SoulRingItem soulRing = player.SoulRingManager.GetSoulRing(model.Id, skillModel.SoulRingPos);
                if (soulRing == null) continue;

                equipedSoulRings.Add(skillModel.SoulRingPos);

                msg.SoulRings.Add(new MSG_ZGC_EQUIP_HERO_SOULRING() { Id = soulRing.Id, Level = soulRing.Level ,Position = soulRing.Position, Year = soulRing.Year});
            }

            //确定魂骨的相关信息，技能等
            List<SoulBone> temp = player.SoulboneMng.GetEnhancedHeroBones(msg.HeroId);
            if (temp != null)
            {
                foreach (var bone in temp)
                {
                    SoulBoneItem item = player.BagManager.SoulBoneBag.GetItem(bone.Uid) as SoulBoneItem;
                    if (item != null)
                    {
                        //msg.SoulBones.Add(item.Id);
                        msg.SoulBones.Add(item.GenerateMsg());
                    }
                }
            }
        }



        public override void OnPlayerLeave(PlayerChar player, bool cache = false)
        {
            Stop(DungeonResult.Failed);
            base.OnPlayerLeave(player, cache);
        }

        protected override void Start()
        {
            //设置副本开始，结束时间
            SetStartTime(BaseApi.now);
            InitTriggers();
            NotifyDungeonBattleStart();

            State = DungeonState.Started;
        }

        public override void OnStopFighting()
        {
            Stop(DungeonResult.Failed);
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();

            if (player == null) return;

            RewardManager manager = GetFinalReward(player.Uid);
            manager.BreakupRewards();
            player.AddRewards(manager, ObtainWay.StoryDungeon, DungeonModel.Id.ToString());

            //副本类型任务计数
            PlayerAddTaskNum(player);

            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = player.GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = DungeonModel.Id;
            rewardMsg.Result = (int)DungeonResult.Success;
            player.Write(rewardMsg);

            //增加伙伴经验
            player.AddHeroExp(DungeonModel.HeroExp);

            ResetReward();

            //日志
            player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
        }

        protected override void Failed()
        {
            base.Failed();
            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            PcList.Values.FirstOrDefault()?.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
        }

        public override bool NeedCheck()
        {
            return false;
        }
    }
}
