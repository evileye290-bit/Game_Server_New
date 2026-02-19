using CommonUtility;
using ServerModels.Monster;

namespace ZoneServerLib
{
    public class GodHeroCountGen : BaseMonsterGen
    {
        private bool operated = false;
        public GodHeroCountGen(FieldMap map, MonsterGenModel model) : base(map, model)
        {
        }

        public override void Update(float dt)
        {
            if (curDungeon == null || curDungeon.State <= DungeonState.Open)
            {
                return;
            }

            base.Update(dt);

            if (!operated)
            {
                // 种怪
                GenerateMonstersDelay(Model.GenCount, Model.GenDelay);
                operated = true;
            }
        }

    }
}
