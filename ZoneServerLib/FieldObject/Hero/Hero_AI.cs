using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    partial class Hero
    {
        public float HateRange { get; private set; }
        public float HateRefreshTime { get; private set; }

        public override void InitAI()
        {
            hateManager = new HeroHateManager(this, HateRange, HateRefreshTime);

            BindTriggers();
            
            BindSoulRingSkills();
            BindSoulBoneSkills();
            StepsSpecialEffect();
            HiddenWeaponEffect();
            EquipEnchantEffect();
            SchoolPoolEffect();
            PetEffect();
            PassiveSkillEffect();

            MSG_ZGC_SKILL_ENERGY_LIST skillMsg = skillManager.GetSkillEnergyMsg();
            PlayerChar player = owner as PlayerChar;
            player?.Write(skillMsg);
        }
    }
}
