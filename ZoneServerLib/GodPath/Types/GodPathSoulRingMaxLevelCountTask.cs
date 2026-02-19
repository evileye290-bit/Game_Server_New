using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class GodPathSoulRingMaxLevelCountTask : BaseGodPathTask
    {
        public GodPathSoulRingMaxLevelCountTask(GodPathHero goldPathHero, GodPathTaskModel model) : base(goldPathHero, model)
        {
        }

        public override bool Check(HeroInfo hero)
        {
            if (Model.SoulRingMaxLevelCount <= 0) return false;

            Dictionary<int, SoulRingItem> soulRings = GodPathHero.GetEquipedSoulRing();
            if (soulRings == null) return false;

            int haveCount = soulRings.Values.Where(x => x.Level >= x.GetMaxLevel()).Count();

            return haveCount >= Model.SoulRingMaxLevelCount;
        }
    }
}
