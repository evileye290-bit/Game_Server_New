using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;

namespace ZoneServerLib
{
    public class GodPathSecretAreaTireUpCountTask : BaseGodPathTask
    {
        public int TireCount { get; set; }

        public GodPathSecretAreaTireUpCountTask(GodPathHero goldPathHero, GodPathTaskModel model, int count) : base(goldPathHero, model)
        {
            TireCount = count;
        }

        public override bool Check(HeroInfo hero)
        {
            return TireCount >= Model.SecretAreaTireCount;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.SecretAreaTier = TireCount;
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.SecretAreaTire = TireCount;
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.SecretAreaTire = TireCount;
        }

        public override void Init()
        {
            TireCount = 0;
        }

        public override void Reset()
        {
            TireCount = 0;
            //SyncDBUpdateTire();
        }

        public void AddCount()
        {
            TireCount += 1;
            SyncDBUpdateTire();
        }

        public void SyncDBUpdateTire()
        {
            QueryUpdateGodHeroSecretAreaTire query = new QueryUpdateGodHeroSecretAreaTire(GodPathHero.Manager.Owner.Uid, GodPathHero.HeroId, TireCount);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }
    }
}
