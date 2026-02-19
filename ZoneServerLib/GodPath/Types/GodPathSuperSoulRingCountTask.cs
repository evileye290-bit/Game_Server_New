using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class GodPathSuperSoulRingCountTask : BaseGodPathTask
    {
        public GodPathSuperSoulRingCountTask(GodPathHero goldPathHero, GodPathTaskModel model) : base(goldPathHero, model)
        {
        }

        public override bool Check(HeroInfo hero)
        {
            if (Model.SuperSoulRingCount <= 0) return false;

            Dictionary<int, SoulRingItem> soulRings = GodPathHero.GetEquipedSoulRing();
            if (soulRings == null) return false;

            int haveCount = soulRings.Values.Where(x => x.Year >= x.MaxYear).Count();

            return haveCount >= Model.SuperSoulRingCount;
        }
    }
}
