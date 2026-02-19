using ServerModels;

namespace ZoneServerLib
{
    public class GodPathLevelTask : BaseGodPathTask
    {
        public GodPathLevelTask(GodPathHero goldPathHero, GodPathTaskModel model) : base(goldPathHero, model)
        {
        }

        public override bool Check(HeroInfo hero)
        {
            if (hero == null) return false;
            return hero.Level >= Model.Level;
        }
    }
}
