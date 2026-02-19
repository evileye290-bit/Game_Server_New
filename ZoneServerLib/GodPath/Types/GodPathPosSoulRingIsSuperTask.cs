using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class GodPathPosSoulRingIsSuperTask : BaseGodPathTask
    {
        public GodPathPosSoulRingIsSuperTask(GodPathHero goldPathHero, GodPathTaskModel model) : base(goldPathHero, model)
        {
        }

        public override bool Check(HeroInfo hero)
        {
            if (Model.Position <= 0) return false;

            SoulRingItem soulRing = GodPathHero.GetEquipedSoulRing(Model.Position);
            if (soulRing == null) return false;

            return soulRing.Year >= soulRing.MaxYear;

        }
    }
}
