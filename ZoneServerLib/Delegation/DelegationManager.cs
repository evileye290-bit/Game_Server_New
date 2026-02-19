using DBUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DelegationManager
    {
        public PlayerChar Owner { get; private set; }

        private Dictionary<int, DelegationItem> delegationItemList = new Dictionary<int, DelegationItem>();
        //private List<int> nameIdList;
        private List<int> delegatedHeroList = new List<int>();
                  
        public DelegationManager(PlayerChar owner)
        {
            Owner = owner;
        }

        public void AddDelegationInfo(DelegationInfo info)
        {
            BindDelegationInfo(info);
        }

        private void BindDelegationInfo(DelegationInfo info)
        {
            BindDelegationItem(info);
        }

        private void BindDelegationItem(DelegationInfo delegationInfo)
        {
            foreach (var delegation in delegationInfo.ItemList)
            {
                DelegationItem item = new DelegationItem();
                item.Id = delegation.DelegationId;
                item.Name = delegation.NameId;
                item.State = delegation.State;
                item.HeroList = new List<int>();
                foreach (var heroId in delegation.HeroIds)
                {
                    item.HeroList.Add(heroId);
                    AddDelegatedHero(heroId);
                }
                item.EndTime = delegation.EndTime;
                AddDelegationItem(item);
            }
        }

        public void AddDelegationItem(DelegationItem item)
        {
            if (!delegationItemList.ContainsKey(item.Name))
            {
                delegationItemList.Add(item.Name, item);
            }
            else
            {
                Log.Warn("player {0} AddDelegationItem has add name{1}", Owner.Uid, item.Name);
            }
        }

        //public DelegationItem GetDelegationItem(int id)
        //{
        //    DelegationItem item;
        //    delegationItemList.TryGetValue(id, out item);
        //    return item;

        //}     

        public Dictionary<int, DelegationItem> GetDelegationList()
        {
            return delegationItemList;
        }         

        //public List<int> GetDelegationNameList()
        //{
        //    UpdateNameIdList();

        //    return nameIdList;
        //}

        //public void AddNameIdList(int id)
        //{
        //    List<DelegationName> nameList = DelegationLibrary.GetNamesByLevel(Owner.Level);
        //    List<int> tempList = new List<int>();
        //    foreach (var item in nameList)
        //    {
        //        tempList.Add(item.Id);               
        //    }
        //    if (tempList.Contains(id))
        //    {
        //        AddNameId(id);
        //    }
        //}

        //private void AddNameId(int id)
        //{
        //    if (!nameIdList.Contains(id))
        //    {
        //        nameIdList.Add(id);

        //    }
        //}

        //public void SubNameIdList(int id)
        //{
        //    if (nameIdList.Contains(id))
        //    {
        //        nameIdList.Remove(id);
        //    }          
        //}

        public void AddDelegatedHero(int heroId)
        {
            delegatedHeroList.Add(heroId);
        }  

        public void RemoveDelegatedHero(List<int> heroIds)
        {
            foreach (var heroId in heroIds)
            {
                delegatedHeroList.Remove(heroId);
            }
        }

        public bool RemoveDelegationItem(int nameId)
        {
            return delegationItemList.Remove(nameId);
        }

        public DelegationItem GetDelegationItem(int nameId)
        {
            DelegationItem item;
            delegationItemList.TryGetValue(nameId, out item);
            return item;
        }

        public int GetEventNumByMainTaskId(int taskId)
        {
            return DelegationLibrary.GetEventNumByMainTaskId(taskId);
        }

        //public int GetRestDelegateCount(int delegateCount, int buyCount)
        //{
        //    return DelegationLibrary.RenewDelegateCount + buyCount - delegateCount;
        //}

        public int GetAddDelegationCount(PlayerChar player, int num, int delegateCount)
        {
            int count = 0;
            foreach (var delegation in delegationItemList)
            {
                if (delegation.Value.State != (int)DelegationType.None)
                {
                    count++;
                }
            }
            return num - delegateCount - count;         
        }
        
        public List<int> GetDelegatedHeros()
        {
            return delegatedHeroList;
        }

        public bool CheckCanDelegate(int heroId)
        {
            if (delegatedHeroList.Contains(heroId))
            {
                return false;
            }
            return true;
        }   

        public string BuildDelegationNameStr()
        {
            if (delegationItemList.Count == 0)
            {
                return "";
            }
            return string.Join("|", delegationItemList.Keys);
        }

        public string BuildDelegationIdStr()
        {
            if (delegationItemList.Count == 0)
            {
                return "";
            }
            return string.Join("|", delegationItemList.Values.Select(x => x.Id));
        }

        public string BuildDelegationStateStr()
        {
            if (delegationItemList.Count == 0)
            {
                return "";
            }
            return string.Join("|", delegationItemList.Values.Select(x => x.State));
        }

        public string BuildDelegationHerosStr()
        {
            if (delegationItemList.Count == 0)
            {
                return "";
            }
            List<string> itemsHeros = new List<string>();
            foreach (var item in delegationItemList)
            {
                string heros = "";
                foreach (var hero in item.Value.HeroList)
                {
                    if (hero == item.Value.HeroList.Last())
                    {
                        heros += hero;
                    }
                    else
                    {
                        heros += hero + ":";
                    }
                }
                itemsHeros.Add(heros);
            }
            return string.Join("|", itemsHeros);//
        }

        public string BuildDelegationEndTimeStr()
        {
            if (delegationItemList.Count == 0)
            {
                return "";
            }
            return string.Join("|", delegationItemList.Values.Select(x => x.EndTime));
        }

        public void CheckNeedGuarantee(int taskId, DelegationModelList delegations)
        {
            DelegationGuarantee guaranteeModel = DelegationLibrary.GetGuaranteedNumByMainTaskId(taskId);
            if (guaranteeModel == null)
            {
                return;
            }
            int newEventCount = DelegationLibrary.GetEventNumByMainTaskId(taskId);
            if (newEventCount < guaranteeModel.NewEventNum)
            {
                return;
            }
            int count = 0;
            Dictionary<int, int> qualityDic = new Dictionary<int, int>();
            foreach (var item in delegationItemList)
            {
                DelegationModel delegation = DelegationLibrary.GetDelegationById(item.Value.Id);
                if (guaranteeModel.StarWeightDic.ContainsValue(delegation.Quality))
                {
                    count++;
                }
                qualityDic.Add(item.Value.Name, delegation.Quality);
            }
            int restCount = guaranteeModel.GuaranteeNum - count;
            if (restCount <= 0)
            {
                return;
            }
            //保底星级的事件数量不足需要根据权重随机星级满足的事件
            qualityDic = qualityDic.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            List<int> nameList = qualityDic.Keys.ToList();           
            for (int i = 0; i < restCount; i++)
            {
                DelegationItem delegation;
                if (delegationItemList.TryGetValue(nameList[i], out delegation) && delegation.State == (int)DelegationType.None)
                {
                    int quality = DelegationLibrary.RandomGuaranteeQuality(taskId);
                    DelegationModel newModel = delegations.GetDelegationByQuality(quality);
                    if (newModel != null)
                    {
                        delegation.Id = newModel.Id;
                    }
                }
            }                     
        }
    }
}
