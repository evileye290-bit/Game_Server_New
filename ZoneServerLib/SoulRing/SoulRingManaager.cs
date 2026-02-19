using EnumerateUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class SoulRingManager
    {
        private PlayerChar owner { get; set; }
        //Dictionary<heroId, Dictionary<posion, SoulRingItem>>
        private Dictionary<int, Dictionary<int, SoulRingItem>> equipedSoulRings = new Dictionary<int, Dictionary<int, SoulRingItem>>();
        public Dictionary<int, Dictionary<int, SoulRingItem>> EquipedSoulRings { get { return this.equipedSoulRings; } }


        //TODO : 这里是仅仅为了临时调试robot使用的，此处如果clone下去会异常繁琐
        public SoulRingManager Clone()
        {
            SoulRingManager mng = new SoulRingManager();
            Dictionary<int, Dictionary<int, SoulRingItem>> equipedSoulRings = new Dictionary<int, Dictionary<int, SoulRingItem>>();
            equipedSoulRings = this.equipedSoulRings;
            return mng;
        }

        public SoulRingManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public SoulRingManager()
        {

        }

        internal Dictionary<int, Dictionary<int, SoulRingItem>> GetAllEquipedSoulRings()
        {
            return EquipedSoulRings;
        }

        public int GetEquipedCount()
        {
            int count = 0;
            foreach (var item in EquipedSoulRings)
            {
                count += item.Value.Count;
            }
            return count;
        }

        public int GetEquipedCount(int year)
        {
            int count = 0;
            foreach (var item in EquipedSoulRings)
            {
                foreach (var soulRing in item.Value)
                {
                    if (soulRing.Value.Year >= year)
                    {
                        count ++;
                    }
                }
            }
            return count;
        }

        public int GetEquipedCountBySlot(int slot)
        {
            int count = 0;
            foreach (var item in EquipedSoulRings)
            {
                foreach (var soulRing in item.Value)
                {
                    if (soulRing.Value.Position == slot)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void AddEquipSoulRing(SoulRingItem item)
        {
            Dictionary<int, SoulRingItem> soulRings;
            if (!equipedSoulRings.TryGetValue(item.EquipHeroId, out soulRings))
            {
                soulRings = new Dictionary<int, SoulRingItem>();
                equipedSoulRings.Add(item.EquipHeroId, soulRings);
            }
            SoulRingItem soulRing;
            if (!soulRings.TryGetValue(item.Position, out soulRing))
            {
                soulRings.Add(item.Position, item);
            }
        }

        public void DelEquipSoulRing(SoulRingItem item)
        {
            Dictionary<int, SoulRingItem> soulRings;
            if (equipedSoulRings.TryGetValue(item.EquipHeroId,out soulRings))
            {
                if (soulRings.ContainsKey(item.Position))
                {
                    soulRings.Remove(item.Position);
                }
            }
        }

        public bool CheckEquipedSoulRingType(int heroId, int typeId,int solt)
        {
            Dictionary<int, SoulRingItem> soulRings;
            if (equipedSoulRings.TryGetValue(heroId, out soulRings))
            {
                foreach (var item in soulRings)
                {
                    if (item.Value.SoulRingInfo.TypeId == typeId && item.Value.Position !=solt)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal SoulRingItem GetEquipedSoulRing(int heroId, int postion)
        {
            Dictionary<int, SoulRingItem> soulRings;
            if (!equipedSoulRings.TryGetValue(heroId, out soulRings))
            {
                return null;
            }

            SoulRingItem soulRing;
            soulRings.TryGetValue(postion, out soulRing);
            return soulRing;
        }

        internal SoulRingItem GetEquipedSoulRing(int heroId, ulong soulRingUid)
        {
            Dictionary<int, SoulRingItem> soulRings;
            if (!equipedSoulRings.TryGetValue(heroId, out soulRings))
            {
                return null;
            }
            foreach (var item in soulRings)
            {
                if (soulRingUid == item.Value.Uid)
                {
                    return item.Value;
                }
            }
          
            return null;
        }

        //获取所有已装备的魂环
        internal Dictionary<int, SoulRingItem> GetAllEquipedSoulRings(int heroId)
        {
            Dictionary<int, SoulRingItem> soulRings = null;
            equipedSoulRings.TryGetValue(heroId, out soulRings);
            return soulRings;
        }

        public SoulRingItem GetSoulRing(int heroId, int pos)
        {
            Dictionary<int, SoulRingItem> soulRings = null;
            equipedSoulRings.TryGetValue(heroId, out soulRings);
            if(soulRings == null)
            {
                return null;
            }
            SoulRingItem item = null;
            soulRings.TryGetValue(pos, out item);
            return item;
        }

        public bool CheckRepeated(int heroId, int modelId)
        {
            Dictionary<int, SoulRingItem> soulRings;
            if (!equipedSoulRings.TryGetValue(heroId, out soulRings)) return false;

            return soulRings.Select(x => x.Value.modelId == modelId).FirstOrDefault();
        }

        public static List<ItemBasicInfo> GenerateSoulRingReward(RewardDropItem dropItem, RewardDropType type, int job = 0)
        {
            List<ItemBasicInfo> infos = new List<ItemBasicInfo>();
            for (int i = 0; i < dropItem.Num; i++)
            {
                ItemBasicInfo baseInfo = new ItemBasicInfo((int)dropItem.RewardType, dropItem.Id, 1);
                if (dropItem.Params.Count > 0)
                {
                    baseInfo.ParseArrr(dropItem.Params[0]);
                }
                infos.Add(baseInfo);
            }
            return infos;
        }

        internal SoulRingItem GetSoulRing(object heroId, int soulRingPos)
        {
            throw new NotImplementedException();
        }

        public static int GetAffterAddYear(int year, int addYearRatio)
        {
            int currentYear = year;
            if (addYearRatio > 0)
            {
                currentYear = (int)(currentYear * (1.0000f + addYearRatio / 10000.0000f));
            }
            return currentYear;
        }

        public static int GetAddYearRatio(int stepsLevel)
        {
            int sS = 0;
            GroValFactorModel stepsModel = NatureLibrary.GetGroValFactorModel(stepsLevel);
            if (stepsModel != null)
            {
                sS = stepsModel.StepsS;
            }
            return sS;
        }

        public List<ItemBasicInfo> GenerateSoulRingReward(string soulRingReward, int num)
        {
            SoulRingReward ringReward = new SoulRingReward(soulRingReward);

            if (ringReward.GenerateRewardInfo())
            {
                int maxYear = ScriptManager.SoulRing.GetMaxYear(owner.HuntingManager.Research);
                List<ItemBasicInfo> infos = new List<ItemBasicInfo>();
                for (int i = 0; i < num; i++)
                {
                    int ringNum = ringReward.RandNum();
                    for (int j = 0; j < ringNum; j++)
                    {
                        int addYear = ringReward.RandAddYear();
                        int ringId = ringReward.RandSoulRingId();

                        int finalYear = 0;                      
                        if (maxYear + addYear > ringReward.MaxYear)
                        {
                            finalYear = ringReward.MaxYear;
                        }
                        else
                        {
                            finalYear = maxYear + addYear;
                        }
                        ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.SoulRing, ringId, 1, finalYear.ToString());
                        infos.Add(baseInfo);
                    }                  
                }
                return infos;
            }
            return null;
        }

        //魂环互换
        public void HeroSwapSoulRing(HeroInfo fromHero, HeroInfo toHero, List<BaseItem> updateList, List<BaseItem> deleteList)
        {
            Dictionary<int, SoulRingItem> toDic;
            equipedSoulRings.TryGetValue(toHero.Id, out toDic);
            
            List<SoulRingInfo> ringInfoList = new List<SoulRingInfo>();
          
            Dictionary<int, SoulRingItem> fromDic;
            List<int> positions = new List<int>();
            SoulRingItem ringItem;
            equipedSoulRings.TryGetValue(fromHero.Id, out fromDic);
            if (fromDic != null)
            {
                foreach (var item in fromDic)
                {
                    positions.Add(item.Key);
                }
                foreach (var pos in positions)
                {
                    if (fromDic.TryGetValue(pos, out ringItem))
                    {
                        //先拆                      
                        deleteList.Add(ringItem.GenerateDeleteInfo(ringItem.SoulRingInfo));
                        //再装上                      
                        ringItem.SoulRingInfo.EquipHeroId = toHero.Id;
                        ringInfoList.Add(ringItem.SoulRingInfo);
                        updateList.Add(ringItem);
                    }
                }
                positions.Clear();
                equipedSoulRings[toHero.Id] = fromDic;
            }
            else if (toDic != null)
            {
                //fromHero没有魂环
                equipedSoulRings.Remove(toHero.Id);
            }
            if (toDic != null)
            {
                foreach (var item in toDic)
                {
                    positions.Add(item.Key);
                }
                foreach (var pos in positions)
                {
                    if (toDic.TryGetValue(pos, out ringItem))
                    {
                        deleteList.Add(ringItem.GenerateDeleteInfo(ringItem.SoulRingInfo));
                        ringItem.SoulRingInfo.EquipHeroId = fromHero.Id;
                        ringInfoList.Add(ringItem.SoulRingInfo);
                        updateList.Add(ringItem);
                    }
                }
                equipedSoulRings[fromHero.Id] = toDic;
            }
            else if (fromDic != null)
            {
                //toHero没有魂环
                equipedSoulRings.Remove(fromHero.Id);
            }
            owner.BagManager.SoulRingBag.SyncDbBatchUpdateItemInfo(ringInfoList);
        }       
    }
}
