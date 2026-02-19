using CommonUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class Pet
    {
        public override void InitNature()
        {
            InitNature(petInfo);
        }

        private void InitNature(PetInfo petInfo)
        {
            if (petInfo == null)
            {
                return;
            }

            InitNatures(petInfo);

            //最后设置
            SetNatureBaseValue(NatureType.PRO_HP, GetNatureValue(NatureType.PRO_MAX_HP));
        }

        private void InitNatures(PetInfo petInfo)
        {
            if (petInfo == null)
            {
                return;
            }
            ResetNature();

            foreach (var item in petInfo.Nature.GetNatureList())
            {
                if (item.Key == NatureType.PRO_HIT)
                {
                    SetNatureBaseValue(item.Key, (long)(item.Value.Value * PetLibrary.PetConfig.BattleHitFactor));
                }
                else
                {
                    SetNatureBaseValue(item.Key, item.Value.Value);
                }
            }
        }
    }
}
