using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class GodPathManager
    {
        private Dictionary<int, GodPathHero> heroList = new Dictionary<int, GodPathHero>();
        public Dictionary<int, GodPathHero> HeroList => heroList;

        public PlayerChar Owner { get; private set; }

        public GodPathManager(PlayerChar player)
        {
            this.Owner = player;
        }

        public void InitDBInfo(Dictionary<int, GodPathDBInfo> dbInfo)
        {
            foreach (var kv in dbInfo)
            {
                GodPathHero info = new GodPathHero(this);
                info.Set(kv.Value);
                AddGodPathHero(info);
            }
        }

        public GodPathHero AddGodPathHero(int heroId)
        {
            GodPathDBInfo info = new GodPathDBInfo()
            {
                HeroId = heroId,
                Stage = 1,
                Affinity = 0,
                CurrStageState = (int)GodPathTaskState.Ready,
            };

            GodPathHero hero = new GodPathHero(this);
            hero.Init(info);
            AddGodPathHero(hero);

            return hero;
        }

        public void AddGodPathHero(GodPathHero hero)
        {
            heroList[hero.HeroId] = hero;
        }

        public GodPathHero GetGodPathHero(int heroId)
        {
            GodPathHero hero;
            heroList.TryGetValue(heroId, out hero);
            return hero;
        }

        public void OnFinishedHunting(DungeonModel model)
        {
            foreach (var kv in heroList)
            {
                if (kv.Value.HaveTypeTask(GodPathTaskType.HuntingDungeonCount))
                {
                    kv.Value.GetGodPath<GodPathHuntingDungeonCountTask>(GodPathTaskType.HuntingDungeonCount)?.AddCount(model);
                }
            }
        }

        public void OnFinishedSecretArea()
        {
            foreach (var kv in heroList)
            {
                if (kv.Value.HaveTypeTask(GodPathTaskType.SecretAreaTireUpCount))
                {
                    kv.Value.GetGodPath<GodPathSecretAreaTireUpCountTask>(GodPathTaskType.SecretAreaTireUpCount)?.AddCount();
                }
            }
        }

        public void CheckTridentUse()
        {
            foreach (var kv in heroList)
            {
                if (kv.Value.HaveTypeTask(GodPathTaskType.Trident))
                {
                    kv.Value.GetGodPath<GodPathTridentTask>(GodPathTaskType.Trident)?.CheckUse();
                }
            }
        }

        /// <summary>
        /// 完成指定战斗和选择战斗
        /// </summary>
        /// <param name="dungeonId"></param>
        public void OnFinishGodPathDungeon(int dungeonId)
        {
            foreach (var kv in heroList)
            {
                if (kv.Value.HaveTypeTask(GodPathTaskType.AssignFight))
                {
                    kv.Value.GetGodPath<GodPathAssignFightTask>(GodPathTaskType.AssignFight)?.SetFinishState(dungeonId);
                }
            }
        }

        public void RefreshDaily()
        {
            foreach (var kv in heroList)
            {
                kv.Value.DailyReset();
            }
        }

        public MSG_ZMZ_GOD_PATH_INFO GetGodPathTransform()
        {
            MSG_ZMZ_GOD_PATH_INFO godPathInfo = new MSG_ZMZ_GOD_PATH_INFO();
            godPathInfo.AcroessOceanDiff = Owner.AcroessOceanDiff;

            foreach (var kv in heroList)
            {
                godPathInfo.GodHeroList.Add(kv.Value.GenerateGodHeroTransformInfo());
            }

            return godPathInfo;
        }

        public void LoadTransform(MSG_ZMZ_GOD_PATH_INFO godPathInfo)
        {
            foreach (var kv in godPathInfo.GodHeroList)
            {
                GodPathHero godHero = new GodPathHero(this);

                GodPathDBInfo heroInfo = new GodPathDBInfo()
                {
                    HeroId = kv.HeroId,
                    Stage = kv.Stage,
                    Affinity = kv.Affinity,
                    HuntingCount = kv.HuntingCount,
                    SecretAreaTier = kv.SecretAreaTire,
                    AssignFight = kv.AssignFight,
                    SevenFightStage = kv.SevenFightStage,
                    SevenFightWinCount = kv.SevenFightWinCount,
                    TrainBodyHP = kv.TrainBodyHP,
                    TrainBodyStage = kv.TrainBodyStage,
                    CurrStageState = kv.CurrStageState,
                    SevenFightState = kv.SevenFightState,
                    SevenFightHP = kv.SevenFightHP,
                    TrainBodyBuy = kv.TrainBodyBuy,

                    HeartDailyCount = kv.HeartDailyCount,
                    HeartBuyCount = kv.HeartBuyCount,
                    HeartUseCount = kv.HeartUseCount,
                    HeartCurrentValue = kv.HeartCurrentValue,
                    HeartState = kv.HeartState,
                    HeartRewards = kv.HeartRewards,

                    TridentBuyCount = kv.TridentBuyCount,
                    TridentUseCount = kv.TridentUseCount,
                    TridentCurrentValue = kv.TridentCurrentValue,
                    TridentState = kv.TridentState,

                    AcroessOceanPuzzle = kv.AcroessOceanPuzzle,
                };

                godHero.Set(heroInfo);
                AddGodPathHero(godHero);
            }
        }

    }
}
