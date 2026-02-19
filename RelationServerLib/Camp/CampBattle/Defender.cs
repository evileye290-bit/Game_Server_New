using CommonUtility;
using Google.Protobuf.Collections;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class Defender
    {
        RepeatedField<HERO_INFO> heroList;
        public int TotalBattlePower;
        public int DefQueueId;
        //public Defender(PLAY_BASE_INFO playerInfo,int defQueueId, RepeatedField<HERO_INFO> heroList)
        public Defender(int defQueueId, RepeatedField<HERO_INFO> heroList)
        {
            //this.PlayerInfo = playerInfo.Clone();
            TotalBattlePower = 0;
            DefQueueId = defQueueId;
            this.heroList = heroList.Clone();
            foreach (var item in heroList)
            {
                TotalBattlePower += item.BattlePower;
            }
        }

        public void UpdateHeroList(RepeatedField<HERO_INFO> heroList)
        {
            this.heroList = heroList.Clone();
            TotalBattlePower = 0;
            foreach (var item in heroList)
            {
                TotalBattlePower += item.BattlePower;
            }
        }

        public List<CAMP_CHALLENGER_HERO_INFO> GetHeroList()
        {
            List<CAMP_CHALLENGER_HERO_INFO> list = new List<CAMP_CHALLENGER_HERO_INFO>();
            foreach (var item in heroList)
            {
                if (item.HeroNature.Hp == 0)
                {
                    continue;
                }
                var heroInfo = GetCampChallengerHeroInfoMsgData(item);
                list.Add(heroInfo);
            }
            return list;
        }

        private CAMP_CHALLENGER_HERO_INFO GetCampChallengerHeroInfoMsgData(HERO_INFO hero)
        {
            CAMP_CHALLENGER_HERO_INFO info = new CAMP_CHALLENGER_HERO_INFO();
            info.Id = hero.Id;
            info.Level = hero.Level;
            info.AwakenLevel = hero.AwakenLevel;
            info.EquipIndex = hero.EquipIndex;
            info.StepsLevel = hero.StepsLevel;
            info.HeroId = hero.HeroId;
            info.GodType = hero.GodType;
            info.Name = hero.Name;
            info.BattlePower = hero.BattlePower;
            info.HeroNature = GetHeroNature(hero.HeroNature);
            info.SoulRings.AddRange(GetSoulRingInfo(hero.SoulRings));
            info.DefensiveQueueNum = hero.DefensiveQueueNum;
            info.DefensivePositionNum= hero.DefensivePositionNum;
            return info;
        }

        private List<CAMP_CHALLENGER_HERO_SOULRING> GetSoulRingInfo(RepeatedField<HERO_SOULRING> soulRings)
        {
            List<CAMP_CHALLENGER_HERO_SOULRING> list = new List<CAMP_CHALLENGER_HERO_SOULRING>();
            foreach (var soulRing in soulRings)
            {
                CAMP_CHALLENGER_HERO_SOULRING soulRingData = new CAMP_CHALLENGER_HERO_SOULRING();
                soulRingData.Pos = soulRing.Pos;
                soulRingData.Level = soulRing.Level;
                soulRingData.SpecId = soulRing.SpecId;
                soulRingData.Year = soulRing.Year;
                list.Add(soulRingData);
            }
            return list;
        }

        private RZ_Hero_Nature GetHeroNature(ZR_Hero_Nature nature)
        {
            RZ_Hero_Nature heroNature = new RZ_Hero_Nature();
            foreach (var item in nature.List)
            {
                RZ_Hero_Nature_Item info = new RZ_Hero_Nature_Item();
                info.NatureType = item.NatureType;
                info.Value = item.Value;
                heroNature.List.Add(info);
            }
            heroNature.Hp = nature.Hp;
            heroNature.MaxHp = nature.MaxHp;
            //heroNature.Pow = nature.Pow;
            //heroNature.Con = nature.Con;
            //heroNature.Agi = nature.Agi;
            //heroNature.Exp = nature.Exp;
            //heroNature.MaxHp = nature.MaxHp;
            //heroNature.Def = nature.Def;
            //heroNature.Atk = nature.Atk;
            //heroNature.Cri = nature.Cri;
            //heroNature.Hit = nature.Hit;
            //heroNature.Flee = nature.Flee;
            //heroNature.Imp = nature.Imp;
            //heroNature.Arm = nature.Arm;
            //heroNature.Res = nature.Res;
            return heroNature;
        }

        public DEFENDER_INFO GetDefenderStruct()
        {
            DEFENDER_INFO info = new DEFENDER_INFO();
            //info.PlayerInfo = null;
            info.HeroList.AddRange(heroList);
            return info;
        }

   
    }
}
