using ServerShared;
using System.Collections.Generic;
using DataProperty;
using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public partial class DungeonMap : FieldMap
    {
        protected MixSkillManager mixSkillManager;
        
        public void InitMixSkillManager()
        {
            MixSkillModel model = MixSkillLibrary.GetMixSkillModel(DungeonModel.MixSkillPolicy);
            if(model != null)
            {
                mixSkillManager = new MixSkillManager(this, model);
            }
        }


        protected void UpdateMixSkill(float dt)
        {
            if (mixSkillManager == null) return;
            mixSkillManager.Update(dt);
        }

        public void CheckEnableMixSKill(FieldObject caster)
        {
            if (mixSkillManager == null) return;

            int jobValue;
            if (caster.IsPlayer)
            {
                PlayerChar player = caster as PlayerChar;
                jobValue = (int)player.Job;
            }
            else if (caster.IsHero)
            {
                Hero hero = caster as Hero;
                jobValue =(int) hero.GetJobType();
            }else if (caster.IsRobot)
            {
                Robot robot = caster as Robot;
                jobValue = robot.HeroModel.Job;
            }
            else
            {
                return;
            }
            mixSkillManager.CheckEnableMixSkill(jobValue);
        }

       
    }
}
