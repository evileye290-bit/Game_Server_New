using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class Hero
    {
        public override void InitNature()
        {
            if (this.heroInfo == null)
            {
                return;
            }
            InitNature(heroInfo);
        }

        //public void GetHeroNature(int heroId)
        //{
        //    MSG_ZGC_HERO_NATURE response = new MSG_ZGC_HERO_NATURE();
        //    HeroInfo heroInfo = this.heroInfo??(Owner as PlayerChar)?.HeroMng.GetHeroInfo(heroId);
        //    if (heroInfo == null)
        //    {
        //        Log.Warn("hero {0} does not exist", heroId);
        //        response.Result = (int)ErrorCode.HeroNotExist;
        //        (Owner as PlayerChar)?.Write(response);
        //        return;
        //    }
        //    response.AwakenLevel = heroInfo.AwakenLevel;
        //    response.Level = heroInfo.Level;
        //    response.TitleLevel = heroInfo.TitleLevel;
        //    response.StepsLevel = heroInfo.StepsLevel;
        //    response.HeroNature = GetNature(heroInfo);
        //    response.Result = (int)ErrorCode.Success;

        //    (Owner as PlayerChar)?.Write(response);
        //}

        //private Hero_Nature GetNature(HeroInfo heroInfo)
        //{
        //    Hero_Nature heroNature = new Hero_Nature();
        //    heroNature.Pow = heroInfo.GetNatureValue(NatureType.PRO_POW);
        //    heroNature.Con = heroInfo.GetNatureValue(NatureType.PRO_CON);
        //    heroNature.Agi = heroInfo.GetNatureValue(NatureType.PRO_AGI);
        //    heroNature.Exp = heroInfo.GetNatureValue(NatureType.PRO_EXP);
        //    heroNature.MaxHp = heroInfo.GetNatureValue(NatureType.PRO_MAX_HP);
        //    heroNature.Def = heroInfo.GetNatureValue(NatureType.PRO_DEF);
        //    heroNature.Atk = heroInfo.GetNatureValue(NatureType.PRO_ATK);
        //    heroNature.Cri = heroInfo.GetNatureValue(NatureType.PRO_CRI);
        //    heroNature.Hit = heroInfo.GetNatureValue(NatureType.PRO_HIT);
        //    heroNature.Flee = heroInfo.GetNatureValue(NatureType.PRO_FLEE);
        //    heroNature.Imp = heroInfo.GetNatureValue(NatureType.PRO_IMP);
        //    heroNature.Arm = heroInfo.GetNatureValue(NatureType.PRO_ARM);
        //    heroNature.Res = heroInfo.GetNatureValue(NatureType.PRO_RES);
        //    return heroNature;
        //}


        //private ZMZ_HERO_NATURE GetNaturesTransform(HeroInfo heroInfo)
        //{
        //    ZMZ_HERO_NATURE natureInfo = new ZMZ_HERO_NATURE();

        //    foreach (var item in heroInfo.Nature.GetNatureList())
        //    {
        //        ZMZ_NATURE heroNature = new ZMZ_NATURE();
        //        heroNature.Type = (int)item.Key;
        //        heroNature.BaseValue = item.Value.BaseValue;
        //        heroNature.BaseRatio = item.Value.Ratio;
        //        natureInfo.Natures.Add(heroNature);
        //    }
        //    return natureInfo;
        //}
    }
}
