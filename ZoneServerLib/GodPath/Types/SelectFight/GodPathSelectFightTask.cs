using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;

namespace ZoneServerLib
{
    public class GodPathSelectFightTask : BaseGodPathTask
    {
        public bool Finished { get; private set; }

        public GodPathSelectFightTask(GodPathHero goldPathHero, GodPathTaskModel model, bool finished) : base(goldPathHero, model)
        {
            Finished = finished;
        }

        public override bool Check(HeroInfo hero)
        {
            return Finished;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
        }

        public override void Init()
        {
            Finished = false;
        }

        public override void Reset()
        {
            Finished = false;
        }

        public void SetFinishState(int dungeonId)
        {
            if (dungeonId == Model.DungeonId)
            {
                Finished = true;
                SyncDBUpdateSelectFightState();
            }
        }

        public void SyncDBUpdateSelectFightState()
        {
            //QueryUpdateGodHeroSelectFight query = new QueryUpdateGodHeroSelectFight(GodPathHero.Manager.Owner.Uid, GodPathHero.HeroId, Finished);
            //GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }
    }
}
