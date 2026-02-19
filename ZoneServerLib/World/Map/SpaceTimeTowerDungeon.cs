using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;
using EnumerateUtility.SpaceTimeTower;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class SpaceTimeTowerDungeon : DungeonMap
    {
        private PlayerChar owner;
        private List<GuideSoulEffectModel> guideSoulItemsEffects;

        public SpaceTimeTowerDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            guideSoulItemsEffects = new List<GuideSoulEffectModel>();
        }

        protected override void Start()
        {
            base.Start();
            InitGuideSoulItemsEffect();
        }

        protected override void Success()
        {
            SetSpeedUp(false);
            DoReward();
            
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                NotifySpeedUpEnd(player);

                RewardManager mng = GetFinalReward(player.Uid);

                RewardManager finalRewards = FilterRewardItems(player, mng);
                
                DungeonRewardsSettlement(finalRewards, 0);
                
                player?.SpaceTimeTowerSuccess(DungeonModel.SonType);
                
                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
            }

            ResetReward();

        }

        protected override void Failed()
        {
            SetSpeedUp(false);
            DoFailReward();
            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }

            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                RewardManager finalRewards = GetFinalReward(player.Uid);
                
                DungeonRewardsSettlement(finalRewards, 0);
                
                player.SpaceTimeTowerFail(pointState);
                NotifySpeedUpEnd(player);
            }

            PcList.Values.FirstOrDefault()?.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
            
            ResetReward();
        }
      
        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            PlayerChar player = owner;

            // 加到地图里
            AddHero(hero);

            if (IsDungeon)
            {
                DungeonMap map = this as DungeonMap;
                DungeonModel model = map.DungeonModel;

                int pos = player.SpaceTimeTowerMng.GetHeroPos(hero.HeroId);
                hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(pos);

                //设置位置，一定在aoi前
                Vec2 temp = HeroLibrary.GetHeroPos(pos);
                if (temp != null)
                {
                    temp = model.GetPosition4Count(1, temp);
                }

                hero.SetPosition(temp ?? hero.Position);

                hero.InitBaseBattleInfo();
            }

            hero.AddToAoi();
            hero.BroadCastHp();

            if (HeroList.Count >= player.SpaceTimeTowerMng.HeroQueue.Count())
            {
                OnePlayerDone = true;//此时至少有一个玩家连同其hero加载完了
            }
        }

        public void AddAttackerMirror(PlayerChar player, Dictionary<int, SpaceTimeHeroInfo> stHeros, PetInfo pet)
        {
            player.IsAttacker = true;

            List<HeroInfo> heros = new List<HeroInfo>();
            Dictionary<int, int> poses = new Dictionary<int, int>();

            foreach (var hero in stHeros)
            {
                HeroInfo heroInfo = hero.Value.ConvertToHeroInfo(SpaceTimeTowerLibrary.HeroLevel, SpaceTimeTowerLibrary.SoulRingSkillLevel);
                heros.Add(heroInfo);
                poses.Add(heroInfo.Id, hero.Value.PositionNum);
            }

            Robot robot = Robot.CopyFromPlayer(server, player);
            robot.IsAttacker = true;
            robot.EnterMap(this);
            robot.SetOwnerUid(player.Uid);
            AddRobot(robot);

            robot.SetHeroPoses(poses);
            robot.SetHeroInfos(heros);
            robot.CopyHeros2SpaceTimeTowerDungeon(player);

            if (pet != null)
            {
                Dictionary<int, PetInfo> queuePet = new Dictionary<int, PetInfo>();
                queuePet.Add(1, pet);
                robot.CopyPet2CrossMap(player, queuePet);
            }
        }

        public void SetPlayer(PlayerChar owner)
        {
            this.owner = owner;
        }

        private void InitGuideSoulItemsEffect()
        {
            foreach (var kv in owner.SpaceTimeTowerMng.GuideSoulRestCounts)
            {
                GuideSoulItemModel itemModel = SpaceTimeTowerLibrary.GetGuideSoulItemModel(kv.Key);
                if (itemModel == null)continue;
                GuideSoulItemDoEffect(itemModel.EffectIdList);
            }
        }

        private void GuideSoulItemDoEffect(List<int> effectIdList)
        {
            foreach (int effectId in effectIdList)
            {
                GuideSoulEffectModel effectModel = SpaceTimeTowerLibrary.GetGuideSoulEffectModel(effectId);
                if (effectModel == null)continue;
                guideSoulItemsEffects.Add(effectModel);
                DoEffectOnHeros(effectModel);
            }
        }

        private void DoEffectOnHeros(GuideSoulEffectModel effectModel)
        {
            foreach (var hero in HeroList)
            {
                DoEffect(effectModel, hero.Value);
            }
        }

        private void DoEffectOnMonster(Monster monster)
        {
            foreach (var effect in guideSoulItemsEffects)
            {
                DoEffect(effect, monster);
            }
        }
        
        private void DoEffect(GuideSoulEffectModel model, FieldObject target)
        {
            //int value = (int)(model.Param2.Key * growth + model.Param2.Value);
            switch (model.Type)
            {
                case GuideSoulEffectType.HeroAddTrigger:
                {
                    Hero hero = target as Hero;
                    if (hero == null)
                    {
                        return;
                    }
                    TriggerCreatedByGuideSoulItem trigger = new TriggerCreatedByGuideSoulItem(hero, model.Param1, 1, hero);
                    hero.AddTrigger(trigger);
                } 
                    break;
                case GuideSoulEffectType.MonsterAddTrigger:
                {
                    Monster monster = target as Monster;
                    if (monster == null)
                    {
                        return;
                    }
                    TriggerCreatedByGuideSoulItem trigger = new TriggerCreatedByGuideSoulItem(monster, model.Param1, 1, monster);
                    monster.AddTrigger(trigger);
                }
                    break;
                default:
                    break;
            }
        }

        public override Monster CreateMonster(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            Monster monster = base.CreateMonster(id, position, monGenerator, hp);
            DoEffectOnMonster(monster);
            return monster;
        }

        public RewardManager FilterRewardItems(PlayerChar player, RewardManager mng)
        {
            RewardManager finalRewards = new RewardManager();
            int saveItemId = 0;
            foreach (var item in mng.AllRewards)
            {
                if ((RewardType) item.RewardType == RewardType.GuideSoulItem && !player.SpaceTimeTowerMng.GuideSoulRestCounts.ContainsKey(item.Id) && saveItemId == 0)
                {
                    saveItemId = item.Id;
                    finalRewards.AddReward(item);
                }
                else if ((RewardType) item.RewardType != RewardType.GuideSoulItem)
                {
                    finalRewards.AddReward(item);
                }
            }
            return finalRewards;
        }

        private void DoFailReward()
        {
            string generalReward = DungeonModel.Data.GetString("GeneralReward");
            if (!string.IsNullOrEmpty(generalReward))
            {
                AddShardRewards(generalReward);
            }
        }

        private void DungeonRewardsSettlement(RewardManager manager,int time)
        {
            manager.BreakupRewards();

            owner.AddRewards(manager, ObtainWay.SpaceTimeTowerDungeonReward, DungeonModel.Id.ToString());

            if (IsDungeon)
            {
                MSG_ZGC_DUNGEON_REWARD msg = new MSG_ZGC_DUNGEON_REWARD();
                msg.PassTime = time;
                msg.DungeonId = DungeonModel.Id;
                msg.Result = (int)DungeonResult;
                manager.GenerateRewardMsg(msg.Rewards);

                owner.CheckCacheRewardMsg(msg);
            }
        }
    }
}
