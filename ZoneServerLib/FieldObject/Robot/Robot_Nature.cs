using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class Robot
    {

        public override void InitNature()
        {
            InitRobotNature(heroInfo);
        }

        public void InitRobotNature(HeroInfo info)
        {
            if (info == null)
            {
                return;
            }
            InitNatures(info);
            //SetNatureAddedValue(NatureType.PRO_MAX_HP, info.GetNatureValue(NatureType.PRO_MAX_HP));
            //SetNatureAddedValue(NatureType.PRO_ATK, info.GetNatureValue(NatureType.PRO_ATK));
            //SetNatureAddedValue(NatureType.PRO_DEF, info.GetNatureValue(NatureType.PRO_DEF));
            //SetNatureAddedValue(NatureType.PRO_HIT, info.GetNatureValue(NatureType.PRO_HIT));
            //SetNatureAddedValue(NatureType.PRO_FLEE, info.GetNatureValue(NatureType.PRO_FLEE));
            //SetNatureAddedValue(NatureType.PRO_CRI, info.GetNatureValue(NatureType.PRO_CRI));
            //SetNatureAddedValue(NatureType.PRO_RES, info.GetNatureValue(NatureType.PRO_RES));
            //SetNatureAddedValue(NatureType.PRO_IMP, info.GetNatureValue(NatureType.PRO_IMP));
            //SetNatureAddedValue(NatureType.PRO_ARM, info.GetNatureValue(NatureType.PRO_ARM));
            //SetNatureAddedValue(NatureType.PRO_HP, info.GetNatureValue(NatureType.PRO_HP));

            nature.AddNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, HeroModel.PRO_RUN_IN_BATTLE);
            SetNatureBaseValue(NatureType.PRO_SPD, GetNatureValue(NatureType.PRO_RUN_IN_BATTLE));
        }
    }
}
