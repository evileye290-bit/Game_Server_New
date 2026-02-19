using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using static DBUtility.QueryUpdateHeroInfo;

namespace ZoneServerLib
{
    public class GodPathHero
    {
   
        readonly Dictionary<GodPathTaskType, BaseGodPathTask> tasks = new Dictionary<GodPathTaskType, BaseGodPathTask>();

        public GodPathManager Manager { get; set; }
        public int HeroId { get; private set; }
        public int Stage { get; private set; }
        public int Affinity { get; private set; }//亲和力
        public GodPathTaskState CurrStageState { get; private set; }

        public GodPathHero(GodPathManager manager)
        {
            Manager = manager;
        }

        public void Set(GodPathDBInfo info)
        {
            HeroId = info.HeroId;
            Stage = info.Stage;
            Affinity = info.Affinity;
            CurrStageState = (GodPathTaskState)info.CurrStageState;

            BuildGodPathTask(info, false);
        }
        public void Init(GodPathDBInfo info)
        {
            HeroId = info.HeroId;
            Stage = info.Stage;
            Affinity = info.Affinity;
            CurrStageState = (GodPathTaskState)info.CurrStageState;

            BuildGodPathTask(info, true);
            //保存数据
            SyncDBInsertHeroInfo();
        }

        /// <summary>
        /// 绑定数据
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isNew">是否初始化</param>
        public void BuildGodPathTask(GodPathDBInfo info, bool isNew)
        {
            List<GodPathTaskModel> taskList = GodPathLibrary.GetHeroStageTask(info.HeroId, info.Stage);
            if (taskList == null)
            {
                Logger.Log.Error($"BuildGodPath error, have not this hero task, hero {info.HeroId} stage {info.Stage}");
                return;
            }
            tasks.Clear();
            foreach (var kv in taskList)
            {
                BaseGodPathTask godPath = BuildGodPath(kv, info);
                if (isNew)
                {
                    //需要初始化数据
                    godPath.Init();
                }
                tasks.Add(godPath.Model.GodPathType, godPath);
            }
        }

        public BaseGodPathTask BuildGodPath(GodPathTaskModel model, GodPathDBInfo info)
        {
            switch (model.GodPathType)
            {
                case GodPathTaskType.Level: return new GodPathLevelTask(this, model);
                case GodPathTaskType.SuperSoulRingCount: return new GodPathSuperSoulRingCountTask(this, model);
                case GodPathTaskType.PosSoulRingIsSuper:return new GodPathPosSoulRingIsSuperTask(this, model);
                case GodPathTaskType.SoulRingMaxLevelCount:return new GodPathSoulRingMaxLevelCountTask(this, model);
                case GodPathTaskType.PosSoulRingMaxLevel:return new GodPathPosSoulRingMaxLevelTask(this, model);
                case GodPathTaskType.HuntingDungeonCount:return new GodPathHuntingDungeonCountTask(this, model, info.HuntingCount);
                case GodPathTaskType.SecretAreaTireUpCount:return new GodPathSecretAreaTireUpCountTask(this, model, info.SecretAreaTier);
                case GodPathTaskType.AssignFight:return new GodPathAssignFightTask(this, model, info.AssignFight);
                case GodPathTaskType.SevenDouluoFight: return new GodPathSevenDouluoFightTask(this, model, info);
                case GodPathTaskType.TarinBody:return new GodPathTarinBodyTask(this, model, info);
                //case GodPathTaskType.Height: return new GodPathHighTask(this, model, info);
                case GodPathTaskType.Trident:return new GodPathTridentTask(this, model, info);
                case GodPathTaskType.OceanHeart: return new GodPathOceanHeartTask(this, model, info);
                case GodPathTaskType.AcrossOcean: return new GodPathAcrossOceanTask(this, model, info);
                default:return new BaseGodPathTask(this, model);
            }
        }

        public bool CurrStageIsReady()
        {
            return CurrStageState == GodPathTaskState.Ready;
        }
        public bool CurrStageIsOpening()
        {
            return CurrStageState == GodPathTaskState.Opening;
        }
        public bool CurrStageIsFinished()
        {
            return CurrStageState == GodPathTaskState.Finished;
        }
            

        public bool CheckMaxStage()
        {
            return Stage >= GodPathLibrary.MaxStage;
        }

        public bool HaveTypeTask(GodPathTaskType type)
        {
            return tasks.ContainsKey(type);
        }

        public T GetGodPath<T>(GodPathTaskType type) where T : BaseGodPathTask
        {
            BaseGodPathTask obj;
            tasks.TryGetValue(type, out obj);
            return obj as T;
        }

        public void AddAffinity(int value)
        {
            Affinity += value;

            int needAffinity = GetNeedAffinity();

            if (Affinity >= needAffinity)
            {
                CurrStageState = GodPathTaskState.Opening;
                Affinity = needAffinity;
            }

            SyncDBUpdateAffinityAndState();
        }

        public int GetNeedAffinity()
        {
            return GodPathLibrary.GetCostAffinity(Stage);
        }

        public bool CheckFinished()
        {
            if (tasks.Count <= 0) return false;

            HeroInfo heroInfo = GetHero(HeroId);
            if (heroInfo == null) return false;

            foreach (var kv in tasks)
            {
                if (!kv.Value.Check(heroInfo)) return false;
            }

            return true;
        }

        //public bool IsFirstStage()
        //{
        //    return Stage == 1 && CurrStageIsReady();
        //}

        public void SetStageFinished()
        {
            //重置亲和力
            Affinity = 0;
            CurrStageState = GodPathTaskState.Finished;

            if (Stage >= GodPathLibrary.MaxStage)
            {
                SyncDBUpdateAffinityAndState();
            }
            else
            { 
                GotoNextStage();
            }
        }

        private void GotoNextStage()
        {
            //当前阶段任务未完成不能进入下一考
            if (!CurrStageIsFinished()) return;

            //升级的时候重置所有已经达成的条件
            foreach (var kv in tasks)
            {
                kv.Value.Reset();
            }

            Stage += 1;
            Affinity = 0;
            CurrStageState = GodPathTaskState.Ready;

            GodPathDBInfo info = GetBaseDbInfo();

            BuildGodPathTask(info, true);

            SyncDBUpdateHeroInfo();
        }

        //public void SetIsGod()
        //{
        //    QueryUpdateGodHeroStageFinished queryStage = new QueryUpdateGodHeroStageFinished(Manager.Owner.Uid, HeroId, (int)GodPathTaskState.Finished);
        //    QueryUpdateHeroIsGod queryHero = new QueryUpdateHeroIsGod(Manager.Owner.Uid, HeroId, true);
        //    DBQueryTransaction transaction = new DBQueryTransaction(new List<AbstractDBQuery>() { queryStage, queryHero});
        //    Manager.Owner.server.GameDBPool.Call(transaction);
        //}

        public void DailyReset() 
        {
            tasks.Values.ForEach(x => x.DailyReset());

            SyncDBUpdateHeroInfo();
        }

        public HeroInfo GetHero(int heroId) => Manager.Owner.HeroMng.GetHeroInfo(heroId);
        public Dictionary<int, SoulRingItem> GetEquipedSoulRing() => Manager.Owner.SoulRingManager.GetAllEquipedSoulRings(HeroId);
        public SoulRingItem GetEquipedSoulRing(int pos) => Manager.Owner.SoulRingManager.GetEquipedSoulRing(HeroId, pos);


        public MSG_GOD_HERO_INFO GenerateGodHeroInfo()
        {
            MSG_GOD_HERO_INFO msg = new MSG_GOD_HERO_INFO()
            {
                HeroId = HeroId,
                Affinity = this.Affinity,
                Stage = this.Stage,
                CurrStageState = (int)this.CurrStageState,
            };

            foreach (var kv in tasks)
            {
                kv.Value.GenerateMsg(msg);
            }
            return msg;
        }

        public ZMZ_GOD_HERO_INFO GenerateGodHeroTransformInfo()
        {
            ZMZ_GOD_HERO_INFO msg = new ZMZ_GOD_HERO_INFO()
            {
                HeroId = HeroId,
                Affinity = this.Affinity,
                Stage = this.Stage,
                CurrStageState = (int)this.CurrStageState,
            };

            foreach (var kv in tasks)
            {
                kv.Value.GenerateTransformInfo(msg);
            }
            return msg;
        }


        #region DB

        public void SyncDBInsertHeroInfo()
        {
            GodPathDBInfo info = GetBaseDbInfo();

            foreach (var kv in tasks)
            {
                kv.Value.GenerateDBInfo(info);
            }

            QueryInsertGodPath query = new QueryInsertGodPath(Manager.Owner.Uid, info);
            Manager.Owner.server.GameDBPool.Call(query);
        }

        public void SyncDBUpdateHeroInfo()
        {
            GodPathDBInfo info = GetBaseDbInfo();

            foreach (var kv in tasks)
            {
                kv.Value.GenerateDBInfo(info);
            }

            QueryUpdateGodHeroInfo query = new QueryUpdateGodHeroInfo(Manager.Owner.Uid, info);
            Manager.Owner.server.GameDBPool.Call(query);
        }

        private GodPathDBInfo GetBaseDbInfo()
        {
            return new GodPathDBInfo()
            {
                HeroId = this.HeroId,
                Stage = this.Stage,
                Affinity = this.Affinity,
                CurrStageState = (int)this.CurrStageState,
                AcroessOceanPuzzle = "",
            };
        }

        public void SyncDBUpdateAffinityAndState()
        {
            QueryUpdateGodHeroAffinity query = new QueryUpdateGodHeroAffinity(Manager.Owner.Uid, HeroId, Affinity, (int)CurrStageState);
            Manager.Owner.server.GameDBPool.Call(query);
        }

        public void SyncDBUpdateStageState()
        {
            QueryUpdateGodHeroStageFinished query = new QueryUpdateGodHeroStageFinished(Manager.Owner.Uid, HeroId, (int)GodPathTaskState.Finished);
            Manager.Owner.server.GameDBPool.Call(query);
        }

        #endregion
    }
}
