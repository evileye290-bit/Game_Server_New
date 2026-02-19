using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class GodPathTarinBodyTask : BaseGodPathTask
    {
        public int TrainBodyHP { get; set; }
        public int TrainBodyStage { get; set; }
        public bool TrainBodyBuy { get; set; }

        public GodPathTarinBodyTask(GodPathHero goldPathHero, GodPathTaskModel model, GodPathDBInfo info) : base(goldPathHero, model)
        {
            TrainBodyHP = info.TrainBodyHP;
            TrainBodyStage = info.TrainBodyStage;
            TrainBodyBuy = info.TrainBodyBuy;
        }

        public override bool Check(HeroInfo hero)
        {
            return TrainBodyHP > 0 && TrainBodyStage >= GodPathLibrary.TrainBodyMaxStage;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.TrainBodyHP = TrainBodyHP;
            info.TrainBodyStage = TrainBodyStage;
            info.TrainBodyBuy = TrainBodyBuy;
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.TrainBodyHP = TrainBodyHP;
            msg.TrainBodyStage = TrainBodyStage;
            msg.TrainBodyBuy = TrainBodyBuy;
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.TrainBodyHP = TrainBodyHP;
            msg.TrainBodyStage = TrainBodyStage;
            msg.TrainBodyBuy = TrainBodyBuy;
        }

        public override void Init()
        {
            TrainBodyHP = GodPathLibrary.TrainBodyHP;
            TrainBodyStage = 0;
            TrainBodyBuy = false;
        }

        public override void Reset()
        {
            TrainBodyHP = 0;
            TrainBodyStage = 0;
            TrainBodyBuy = false;
        }

        public override void DailyReset()
        {
            TrainBodyHP = GodPathLibrary.TrainBodyHP;
            //SyncDBTrainBodyInfo();
        }

        public int CaculateShield()
        {
            int add = GodPathLibrary.TrainBodyBasicShield + (TrainBodyBuy ? GodPathLibrary.TrainBodyAddShield : 0);
            List<int> trainBodyIds = GodPathLibrary.GetStageShieldHero(GodPathHero.HeroId, GodPathHero.Stage)?.GetShieldHeroIds(TrainBodyStage + 1);

            //羁绊加成
            if (trainBodyIds != null)
            {
                foreach (var kv in trainBodyIds)
                {
                    var model = GodPathLibrary.GetPathHeroShieldModel(kv);
                    if (model == null) continue;

                    HeroInfo hero = GodPathHero.Manager.Owner.HeroMng.GetHeroInfo(model.HeroId);
                    if (hero == null) continue;

                    if (hero.StepsLevel >= model.StepsLevel && hero.Level >= model.Level)
                    {
                        add += model.Shield;
                    }
                }
            }

            return add;
        }

        public void DeleteHP(int hp)
        {
            TrainBodyHP -= hp;
            if (TrainBodyHP <= 0)
            {
                TrainBodyHP = 0;
            }
            SyncDBTrainBodyHP();
        }

        public void AddStage()
        {
            TrainBodyStage += 1;
            TrainBodyBuy = false;
            SyncDBTrainBodyStage();
        }

        public void SetBuy()
        {
            TrainBodyBuy = true;
            SyncDBTrainBodyStage();
        }

        private void SyncDBTrainBodyInfo()
        {
            QueryUpdateGodHeroTrainBody query = new QueryUpdateGodHeroTrainBody(GodPathHero.Manager.Owner.Uid, 
                GodPathHero.HeroId, TrainBodyHP, TrainBodyStage, TrainBodyBuy);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }

        private void SyncDBTrainBodyHP()
        {
            QueryUpdateGodHeroTrainBodyHP query = new QueryUpdateGodHeroTrainBodyHP(GodPathHero.Manager.Owner.Uid, GodPathHero.HeroId, TrainBodyHP);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }

        private void SyncDBTrainBodyStage()
        {
            QueryUpdateGodHeroTrainBodyStage query = new QueryUpdateGodHeroTrainBodyStage(GodPathHero.Manager.Owner.Uid, GodPathHero.HeroId, TrainBodyStage, TrainBodyBuy);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }

    }
}
