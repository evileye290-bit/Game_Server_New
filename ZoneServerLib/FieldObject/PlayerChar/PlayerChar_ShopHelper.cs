using System;
using System.Collections.Generic;
using System.Linq;
using EnumerateUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        private List<int> cachedEquipQualityList = new List<int>();
        private List<int> cachedSoulBoneQualityList = new List<int>();

        public int GetShopItemQuality(TowerShopItemType itemType)
        {
            switch (itemType)
            {
                case TowerShopItemType.Equip:
                    return Math.Min(TowerLibrary.EquipMaxQuality, ScriptManager.Shop.GetEquipQuality(cachedEquipQualityList));
                case TowerShopItemType.SoulBone:
                    return Math.Min(TowerLibrary.SoulBoneMaxQuality, ScriptManager.Shop.GetSoulBoneQuality(cachedSoulBoneQualityList));
                case TowerShopItemType.Other:
                    return ScriptManager.Shop.GetOtherQuality(BattlePower);
                default:
                    return 1;
            }
        }

        /// <summary>
        /// 获取4个最大战力hero，备用
        /// </summary>
        public void Cache5MaxBattlePowerHeroInfo()
        {
            cachedEquipQualityList.Clear();
            cachedSoulBoneQualityList.Clear();

            List<HeroInfo> heroInfos = new List<HeroInfo>();

            var enumable = HeroMng.GetHeroInfoList().Values.ToList().OrderByDescending(x => x.GetBattlePower()).GetEnumerator();
            while (enumable.MoveNext())
            {
                heroInfos.Add(enumable.Current);

                if (heroInfos.Count >= 5) break;
            }

            GetMaxBattlePowerHeroEquipQuality(heroInfos);
            GetMaxBattlePowerHeroSoulBoneQuality(heroInfos);
        }

        private void GetMaxBattlePowerHeroEquipQuality(List<HeroInfo> heroInfos)
        {
            foreach (var kv in heroInfos)
            {
                for (int i = 1; i <= 4; i++)
                {
                    EquipmentItem equipment = EquipmentManager.GetEquipedItem(kv.Id, i);

                    //没穿装备默认0
                    cachedEquipQualityList.Add(equipment == null ? 0 : equipment.Model.Grade);
                }
            }

            //没穿魂骨品质补0
            int needCount = 5 * 4;
            for (int i = 0; i < needCount - cachedEquipQualityList.Count; i++)
            {
                cachedEquipQualityList.Add(0);
            }
        }

        private void GetMaxBattlePowerHeroSoulBoneQuality(List<HeroInfo> heroInfos)
        {
            int addCount = 0;
            int needCount = 5 * 6;

            foreach (var kv in heroInfos)
            {
                List<SoulBone> soulBones = SoulboneMng.GetEnhancedHeroBones(kv.Id);
                if (soulBones == null) continue;

                addCount += soulBones.Count;
                soulBones.ForEach(x => cachedSoulBoneQualityList.Add(x.Prefix));
            }

            //没穿魂骨品质补0

            for (int i = 0; i < needCount - addCount; i++)
            {
                cachedSoulBoneQualityList.Add(0);
            }
        }

    }
}
