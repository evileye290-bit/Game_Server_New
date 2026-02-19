using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;

namespace ZoneServerLib
{
    public class BaseGodPathTask
    {
        public GodPathTaskModel Model { get; private set; }
        public GodPathHero GodPathHero { get; private set; }

        public BaseGodPathTask(GodPathHero godPathHero, GodPathTaskModel model)
        {
            Model = model;
            GodPathHero = godPathHero;
        }

        public virtual void Init() { }
        public virtual void Reset() { }
        public virtual void DailyReset() { }
        public virtual bool Check(HeroInfo hero) { return false; }
        public virtual void GenerateDBInfo(GodPathDBInfo info) { }
        public virtual void GenerateMsg(MSG_GOD_HERO_INFO msg) { }
        public virtual void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg) { }
    }
}
