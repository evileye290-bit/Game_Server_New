using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;

namespace ZoneServerLib
{
    public class GodPathHuntingDungeonCountTask : BaseGodPathTask
    {
        public int HuntingCount { get; set; }

        public GodPathHuntingDungeonCountTask(GodPathHero goldPathHero, GodPathTaskModel model, int currCount) : base(goldPathHero, model)
        {
            HuntingCount = currCount;
        }

        public override bool Check(HeroInfo hero)
        {
            return HuntingCount >= Model.HungingCount;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.HuntingCount = HuntingCount;
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.HuntingCount = HuntingCount;
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.HuntingCount = HuntingCount;
        }

        public override void Init()
        {
            HuntingCount = 0;
        }

        public override void Reset()
        {
            HuntingCount = 0;
            //SyncDBUpdateHuntingCount();
        }

        public void AddCount(DungeonModel model)
        {
            //if (model.Id == Model.DungeonId && (int)model.Difficulty == Model.HungingDiffcute)
            if ((int)model.Difficulty == Model.HungingDiffcute)
            {
                HuntingCount += 1;
                SyncDBUpdateHuntingCount();
            }
        }

        public void SyncDBUpdateHuntingCount()
        {
            QueryUpdateGodHeroHuntingCount query = new QueryUpdateGodHeroHuntingCount(GodPathHero.Manager.Owner.Uid, GodPathHero.HeroId, HuntingCount);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }
    }
}
