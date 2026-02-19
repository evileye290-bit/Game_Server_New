using ServerModels.Travel;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib.Travel
{
    public class TravelManager
    {
        private PlayerChar owner;
        private Dictionary<int, TravelHeroItem> infoList = new Dictionary<int, TravelHeroItem>();
        public int BattlePower;
        public TravelManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(Dictionary<int, TravelHeroItem> infoList)
        {
            this.infoList = infoList;
            foreach (var kv in infoList)
            {
                foreach (var card in kv.Value.CardList)
                {
                    if (card.Value.Level == 0)
                    {
                        card.Value.Level = TravelLibrary.OldCardLevel;
                    }
                }
            }
            InitNaturList();
        }

        public void InitNaturList()
        {
            owner.NatureValues.Clear();
            owner.NatureRatios.Clear();
            BattlePower = 0;
            foreach (var kv in infoList)
            {
                foreach (var card in kv.Value.CardList)
                {
                    InitCardNature(card.Value);
                }
            }
        }

        public bool AddCardId(TravelHeroItem heroTravelItem, int cardId)
        {
            //已经激活，增加经验，判断升级
            TravelCardInfo info = TravelLibrary.GetCardInfo(cardId);
            if (info != null)
            {
                TravelCardItem card = heroTravelItem.GetCardItem(cardId);
                if (card != null)
                {

                    int addExp = info.AddExp;
                    int currentLevelExp = TravelLibrary.GetCardLevelExp(card.Level);

                    if (addExp + card.Exp >= currentLevelExp)
                    {
                        //需要升级
                        int newLevelExp = TravelLibrary.GetCardLevelExp(card.Level + 1);
                        if (newLevelExp > 0)
                        {
                            //说明有新等级
                            card.Exp = addExp + card.Exp - currentLevelExp;

                            int oldLevel = card.Level;
                            card.Level++;
                            int newLevel = card.Level;

                            heroTravelItem.AddCardItem(card);
                            AddCardNature(info, oldLevel, newLevel);

                            //增加战力
                            foreach (var hero in owner.HeroMng.GetHeroInfoList())
                            {
                                int heroPower = hero.Value.GetBattlePower();
                                hero.Value.UpdateBattlePower(heroPower + (newLevel - oldLevel) * info.LevelPower);
                            }
                            owner.HeroMng.NotifyClientBattlePower();
                            //Log.Debug("player {0} Activate travel card : new power is {1}", uid, HeroMng.CalcBattlePower());
                            owner.SyncHeroChangeMessage(owner.HeroMng.GetHeroInfoList().Values.ToList());

                            return true;
                        }
                        else
                        {
                            //没有下一级，已经到达满级，不增加经验
                            card.Exp = currentLevelExp;
                            heroTravelItem.AddCardItem(card);
                            return false;
                        }
                    }
                    else
                    {
                        //不用升级，直接添加经验
                        card.Exp += addExp;
                        heroTravelItem.AddCardItem(card);
                        //AddCardNature(card);
                        //InitNaturList();
                        return false;//不更新属性
                    }
                }
                else
                {
                    //没有激活增加初始
                    card = new TravelCardItem();
                    card.Id = cardId;
                    card.Level = 1;
                    card.Exp = 0;
                    heroTravelItem.AddCardItem(card);

                    InitCardNature(card);

                    //增加战力
                    foreach (var hero in owner.HeroMng.GetHeroInfoList())
                    {
                        int heroPower = hero.Value.GetBattlePower();
                        hero.Value.UpdateBattlePower(heroPower + info.BattlePower);
                    }
                    owner.HeroMng.NotifyClientBattlePower();
                    //Log.Debug("player {0} Activate travel card : new power is {1}", uid, HeroMng.CalcBattlePower());
                    owner.SyncHeroChangeMessage(owner.HeroMng.GetHeroInfoList().Values.ToList());

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void InitCardNature(TravelCardItem card)
        {
            TravelCardInfo info = TravelLibrary.GetCardInfo(card.Id);
            if (info != null)
            {
                InitCardNature(info, card.Level);
            }
        }

        public void InitCardNature(TravelCardInfo info, int level)
        {
            int value = 0;
            foreach (var nature in info.NatureValues)
            {
                owner.NatureValues.TryGetValue(nature.Key, out value);
                owner.NatureValues[nature.Key] = value + nature.Value + (level - 1) * info.LevelValues;
            }
            foreach (var nature in info.NatureRatios)
            {
                owner.NatureRatios.TryGetValue(nature.Key, out value);
                owner.NatureRatios[nature.Key] = value + nature.Value + (level - 1) * info.LevelRatios;
            }
            BattlePower += info.BattlePower + (level - 1) * info.LevelPower;
        }

        public void AddCardNature(TravelCardInfo info, int oldLevel, int newLevel)
        {
            int value = 0;
            foreach (var nature in info.NatureValues)
            {
                owner.NatureValues.TryGetValue(nature.Key, out value);
                owner.NatureValues[nature.Key] = value + nature.Value + (newLevel - oldLevel) * info.LevelValues;
            }
            foreach (var nature in info.NatureRatios)
            {
                owner.NatureRatios.TryGetValue(nature.Key, out value);
                owner.NatureRatios[nature.Key] = value + nature.Value + (newLevel - oldLevel) * info.LevelRatios;
            }
            BattlePower += info.BattlePower + (newLevel - oldLevel) * info.LevelPower;
        }

        public Dictionary<int, TravelHeroItem> GetHeroList()
        {
            return infoList;
        }

        public TravelHeroItem GetHeroTravelInfo(int heroId)
        {
            TravelHeroItem info;
            infoList.TryGetValue(heroId, out info);
            return info;
        }

        public void AddHeroTravelInfo(TravelHeroItem info)
        {
            infoList[info.Id] = info;
        }

        public int GetCurrentSlotCount()
        {
            int count = 0;
            foreach (var item in infoList)
            {
                if (item.Value.StartTime > 0)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
