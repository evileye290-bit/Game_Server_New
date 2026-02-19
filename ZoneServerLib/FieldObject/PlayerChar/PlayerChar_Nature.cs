using CommonUtility;
using CommonUtility.Job;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //属性
        public override void InitNature()
        {
            HeroInfo heroInfo = HeroMng.GetPlayerHeroInfo();
            if(heroInfo == null)
            {
                Log.Debug("player {0} init main hero {1} nature does not exist", Uid, HeroId);
                return;
            }
            heroModel =HeroLibrary.GetHeroModel(heroInfo.Id);
            radius = heroModel.Radius;
            HateRatio = heroModel.HateRatio;
            InitNatureExt(NatureValues, NatureRatios);
            InitNature(heroInfo);
            BroadCastHp();
        }

        public void InitNatureWithoutHpUpdate()
        {
            HeroInfo heroInfo = HeroMng.GetPlayerHeroInfo();
            if (heroInfo == null)
            {
                Log.Debug("player {0} init main hero {1} nature does not exist", Uid, HeroId);
                return;
            }
            heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            radius = heroModel.Radius;
            HateRatio = heroModel.HateRatio;
            InitNatureExt(NatureValues, NatureRatios);
            InitNature(heroInfo, false);
        }

        public void GetHeroNature(int heroId)
        {
            MSG_ZGC_HERO_NATURE response = new MSG_ZGC_HERO_NATURE();
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {              
                Log.Warn("player {0} get hero {1} nature does not exist", Uid, heroId);
                response.Result = (int)ErrorCode.HeroNotExist;
                Write(response);
                return;
            }
            response.AwakenLevel = heroInfo.AwakenLevel;
            response.StepsLevel = heroInfo.StepsLevel;
            response.Level = heroInfo.Level;
            response.TitleLevel = heroInfo.TitleLevel;
            response.HeroNature = GetNatureMsg(heroInfo.Nature);
            response.Result = (int)ErrorCode.Success;
            response.FightingCapacity = heroInfo.GetBattlePower();
            Write(response);
        }

        public void GetHeroPower(int heroId)
        {
            MSG_ZGC_GET_HERO_POWER response = new MSG_ZGC_GET_HERO_POWER();
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn("player {0} get hero {1} power does not exist", Uid, heroId);
                return;
            }
            response.HeroId = heroId;
            response.Power = heroInfo.GetBattlePower();
            Write(response);
        }

        private Hero_Nature GetNatureMsg(Natures natures)
        {
            Hero_Nature heroNature = new Hero_Nature();
            //foreach (var item in NatureLibrary.Basic4Nature)
            //{
            //    Hero_Nature_Item info = GetNatureItemMsg(item.Key, heroInfo.Nature);
            //    heroNature.List.Add(info);
            //}
            //foreach (var item in NatureLibrary.Basic9Nature)
            //{
            //    Hero_Nature_Item info = GetNatureItemMsg(item.Key, heroInfo.Nature);
            //    heroNature.List.Add(info);
            //}
            heroNature.Pow = natures.GetNatureValue(NatureType.PRO_POW).ToInt64TypeMsg();
            heroNature.Con = natures.GetNatureValue(NatureType.PRO_CON).ToInt64TypeMsg();
            heroNature.Agi = natures.GetNatureValue(NatureType.PRO_AGI).ToInt64TypeMsg();
            heroNature.Exp = natures.GetNatureValue(NatureType.PRO_EXP).ToInt64TypeMsg();
            heroNature.MaxHp = natures.GetNatureValue(NatureType.PRO_MAX_HP).ToInt64TypeMsg();
            heroNature.Def = natures.GetNatureValue(NatureType.PRO_DEF).ToInt64TypeMsg();
            heroNature.Atk = natures.GetNatureValue(NatureType.PRO_ATK).ToInt64TypeMsg();
            heroNature.Cri = natures.GetNatureValue(NatureType.PRO_CRI).ToInt64TypeMsg();
            heroNature.Hit = natures.GetNatureValue(NatureType.PRO_HIT).ToInt64TypeMsg();
            heroNature.Flee = natures.GetNatureValue(NatureType.PRO_FLEE).ToInt64TypeMsg();
            heroNature.Imp = natures.GetNatureValue(NatureType.PRO_IMP).ToInt64TypeMsg();
            heroNature.Arm = natures.GetNatureValue(NatureType.PRO_ARM).ToInt64TypeMsg();
            heroNature.Res = natures.GetNatureValue(NatureType.PRO_RES).ToInt64TypeMsg();
            return heroNature;
        }

        //private Hero_Nature GetNatureMsg(Hero hero)
        //{
        //    Hero_Nature heroNature = new Hero_Nature();
        //    foreach (var item in NatureLibrary.Basic4Nature)
        //    {
        //        Hero_Nature_Item info = GetNatureItemMsg(item.Key, hero.Nature);
        //        heroNature.List.Add(info);
        //    }
        //    foreach (var item in NatureLibrary.Basic9Nature)
        //    {
        //        Hero_Nature_Item info = GetNatureItemMsg(item.Key, hero.Nature);
        //        heroNature.List.Add(info);
        //    }

        //    //heroNature.Pow = hero.GetNatureValue(NatureType.PRO_POW);
        //    //heroNature.Con = hero.GetNatureValue(NatureType.PRO_CON);
        //    //heroNature.Agi = hero.GetNatureValue(NatureType.PRO_AGI);
        //    //heroNature.Exp = hero.GetNatureValue(NatureType.PRO_EXP);

        //    //heroNature.MaxHp = hero.GetNatureValue(NatureType.PRO_MAX_HP);
        //    //heroNature.Def = hero.GetNatureValue(NatureType.PRO_DEF);
        //    //heroNature.Atk = hero.GetNatureValue(NatureType.PRO_ATK);
        //    //heroNature.Cri = hero.GetNatureValue(NatureType.PRO_CRI);
        //    //heroNature.Hit = hero.GetNatureValue(NatureType.PRO_HIT);
        //    //heroNature.Flee = hero.GetNatureValue(NatureType.PRO_FLEE);
        //    //heroNature.Imp = hero.GetNatureValue(NatureType.PRO_IMP);
        //    //heroNature.Arm = hero.GetNatureValue(NatureType.PRO_ARM);
        //    //heroNature.Res = hero.GetNatureValue(NatureType.PRO_RES);
        //    return heroNature;
        //}

        //public Hero_Nature_Item GetNatureItemMsg(NatureType type, Natures natures)
        //{
        //    Hero_Nature_Item info = new Hero_Nature_Item();
        //    info.NatureType = (int)type;
        //    Int64 value = natures.GetNatureValue(type);
        //    info.Value = GetInt64TypeMsg(value);
        //    return info;
        //}

 

        private ZMZ_HERO_NATURE GetNaturesTransform(HeroInfo heroInfo)
        {
            ZMZ_HERO_NATURE natureInfo = new ZMZ_HERO_NATURE();
           
            foreach (var item in heroInfo.Nature.GetNatureList())
            {
                ZMZ_NATURE heroNature = new ZMZ_NATURE();
                heroNature.Type = (int)item.Key;
                heroNature.BaseValue = item.Value.BaseValue;
                heroNature.AddedValue = item.Value.AddedValue;
                heroNature.BaseRatio = item.Value.Ratio;
                natureInfo.Natures.Add(heroNature);
            }  
            return natureInfo;
        }

        public ZMZ_HERO_NATURE GetNaturesTransform(Dictionary<NatureType, NatureItem> natureList)
        {
            ZMZ_HERO_NATURE natureInfo = new ZMZ_HERO_NATURE();

            foreach (var item in natureList)
            {
                ZMZ_NATURE heroNature = new ZMZ_NATURE();
                heroNature.Type = (int)item.Key;
                heroNature.BaseValue = item.Value.BaseValue;
                heroNature.AddedValue = item.Value.AddedValue;
                heroNature.BaseRatio = item.Value.Ratio;
                natureInfo.Natures.Add(heroNature);
            }
            return natureInfo;
        }

        public ZGC_PET_NATURE GetPetNatureMsg(Dictionary<NatureType, long> natureList)
        {
            ZGC_PET_NATURE natureMsg = new ZGC_PET_NATURE();
            foreach (var nature in natureList)
            {
                switch (nature.Key)
                {
                    case NatureType.PRO_MAX_HP:
                        natureMsg.MaxHp = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_ATK:
                        natureMsg.Atk = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_DEF:
                        natureMsg.Def = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_HIT:
                        natureMsg.Hit = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_FLEE:
                        natureMsg.Flee = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_CRI:
                        natureMsg.Cri = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_RES:
                        natureMsg.Res = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_IMP:
                        natureMsg.Imp = nature.Value.ToInt64TypeMsg();
                        break;
                    case NatureType.PRO_ARM:
                        natureMsg.Arm = nature.Value.ToInt64TypeMsg();
                        break;
                    default:
                        break;
                }
            }
            return natureMsg;
        }
    }
}
