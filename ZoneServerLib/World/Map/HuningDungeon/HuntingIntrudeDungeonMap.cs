using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Linq;

namespace ZoneServerLib
{
    public class HuntingIntrudeDungeonMap : HuntingDungeonMap
    {
        private HuntingIntrudeInfo info;

        public HuntingIntrudeDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public void SetIntrudeId(HuntingIntrudeInfo info)
        {
            this.info = info;
        }

        protected override bool HadPassedHuntingDungeon()
        {
            return false;
        }

        protected override bool CheckHuntingPeriodBuffEffectCondition()
        {
            return false;
        }

        private void AddIntrudeBuff()
        {
            if (info == null) return;

            HuntingIntrudeBuffSuitModel model = HuntingLibrary.GetIntrudeBuffSuitModel(info.BuffSuitId);
            if (model == null)
            {
                Log.Warn($"had  not find hunting intrude buff suit {info.BuffSuitId}");
                return;
            }

            model.HeroBuffList.ForEach(h =>HeroAddHuntingIntrudeBuff(h));
            model.MonsterBuffList.ForEach(h => MonsterAddHuntingIntrudeBuff(h));
        }

        public void HeroAddHuntingIntrudeBuff(int id)
        {
            BuffModel model = BuffLibrary.GetBuffModel(id);
            if (model == null)
            {
                Log.Warn($"had  not find hunting intrude buff {id}");
                return;
            }

            HeroList.ForEach(hero => FieldAddHuntingIntrudeBuff(hero.Value, id));
        }

        public void MonsterAddHuntingIntrudeBuff(int id)
        {
            BuffModel model = BuffLibrary.GetBuffModel(id);
            if (model == null)
            {
                Log.Warn($"had  not find hunting intrude buff {id}");
                return;
            }

            MonsterList.ForEach(hero => FieldAddHuntingIntrudeBuff(hero.Value, id));
        }

        private void FieldAddHuntingIntrudeBuff(FieldObject field, int buffId)
        {
            field.AddBuff(field, buffId, 1);
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            // 加到地图里
            AddHero(hero);
            if (IsDungeon)
            {
                DungeonMap map = this as DungeonMap;
                DungeonModel model = map.DungeonModel;

                int PosIndex = 0;
                if (hero.IsAttacker)
                {
                    PosIndex = map.AttackerPosIndex;
                }
                else
                {
                    PosIndex = map.DefenderPosIndex;
                }

                Vec2 tempPosition;

                //设置位置，一定在aoi前
                if (hero.OwnerIsRobot)
                {
                    Robot ow = hero.Owner as Robot;

                    tempPosition = ow.GetHeroPosPosition(hero.HeroId);

                    int heroPos = ow.GetHeroPos(hero.HeroId);
                    hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(heroPos);
                }
                else
                {
                    PlayerChar owner = hero.Owner as PlayerChar;

                    int heroPos = 0;
                    owner.HuntingManager.HuntingIntrudeHeroPos.TryGetValue(hero.HeroId, out heroPos);
                    hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(heroPos);

                    //设置位置，一定在aoi前
                    Vec2 temp = HeroLibrary.GetHeroPos(heroPos);
                    if (temp != null)
                    {
                        temp = model.GetPosition4Count(1, temp);
                    }

                    tempPosition = temp;

                    //hero.SetPosition(temp ?? hero.Position);

                    //tempPosition = owner.HeroMng.GetHeroPos(hero.HeroId);

                    if (map.HeroList.Count >= owner.HeroMng.CallHeroCount())
                    {
                        map.OnePlayerDone = true;//此时至少有一个玩家连同其hero加载完了
                    }
                }

                hero.SetPosition(tempPosition ?? hero.Position);
                hero.InitBaseBattleInfo();
            }

            if (add2Aoi)
            {
                hero.AddToAoi();
                hero.BroadCastHp();
            }
        }

        protected override void Start()
        {
            base.Start();
            AddIntrudeBuff();
        }

        protected override void Success()
        {
            DoReward();
            try
            {
                PlayerChar player = PcList.Values.FirstOrDefault();

                RewardManager mng = new RewardManager();
                mng.AddReward(HuntingLibrary.RandomHuntingIntrudeReward());

                mng.BreakupRewards();
                player.HuntingReward(mng, DungeonModel, this);

                player.HuntingManager.RemoveHuntingIntrudeInfo(info);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
            }
            catch (Exception ex)
            {
                Log.Alert(ex);
            }
            ResetReward();
        }
    }
}
