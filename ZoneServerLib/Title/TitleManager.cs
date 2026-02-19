using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TitleManager
    {
        public PlayerChar Owner { get; set; }

        private int curTitleId;
        public int CurTitleId { get { return curTitleId; } }
        
        private Dictionary<int, TitleInfoItem> titleList = new Dictionary<int, TitleInfoItem>();  
        public Dictionary<int, TitleInfoItem> TitleList { get { return titleList; } }
       
        //已激活的属性
        private Dictionary<int, int> natureList = new Dictionary<int, int>();
        public Dictionary<int, int> NatureList { get{ return natureList; } }
        public TitleManager(PlayerChar owner)
        {
            Owner = owner;
        }

        public void InitTitleInfo(List<DbTitleItem> dbTitleList)
        {
            foreach (var item in dbTitleList)
            {
                TitleInfoItem title = CreateTitleInfoItem(item);              
                TitleInfo titleModel = TitleLibrary.GetTitleById(title.Id);
                switch ((TitleObtainCondition)titleModel.ObtainCondition)
                {
                    case TitleObtainCondition.CampLeader:
                        if (!Owner.CheckHasCampLeaderTitle(titleModel.SubType) && title.State != TitleState.OverTime)
                        {
                            title.State = TitleState.OverTime;
                            title.NeedSyncDb = true;
                        }
                        else if (Owner.CheckHasCampLeaderTitle(titleModel.SubType) && title.State == TitleState.OverTime && title.Used == 1)
                        {
                            title.State = TitleState.Activated;
                            title.NeedSyncDb = true;
                        }
                        break;
                    case TitleObtainCondition.ArenaRankFirst:
                        if (titleModel.SubType == 1 && Owner.ArenaMng.Rank != 1 && title.State != TitleState.OverTime)
                        {
                            title.State = TitleState.OverTime;
                            title.NeedSyncDb = true;
                        }
                        else if (titleModel.SubType == 1 && Owner.ArenaMng.Rank == 1 && title.State == TitleState.OverTime && title.Used == 1)
                        {
                            title.State = TitleState.Activated;
                            title.NeedSyncDb = true;
                        }
                        break;
                    case TitleObtainCondition.CrossBattleFirst:
                        if (titleModel.SubType == 1 && Owner.CrossInfoMng.Info.LastFinalsRank != 1 && title.State != TitleState.OverTime)
                        {
                            title.State = TitleState.OverTime;
                            title.NeedSyncDb = true;
                        }
                        else if (titleModel.SubType == 1 && Owner.CrossInfoMng.Info.LastFinalsRank == 1 && title.State == TitleState.OverTime && title.Used == 1)
                        {
                            title.State = TitleState.Activated;
                            title.NeedSyncDb = true;
                        }
                        break;
                    default:
                        break;
                }
                titleList.Add(title.Id, title);
                if (title.State >= TitleState.Activated)
                {
                    AddNature(titleModel);
                }
                if (title.State == TitleState.Equiped)
                {
                    curTitleId = title.Id;
                }
                SyncTitleToRedis();
            }
        }

        public TitleInfoItem CreateTitleInfoItem(DbTitleItem item)
        {
            TitleInfoItem title = new TitleInfoItem();
            title.Id = item.Id;
            title.State = (TitleState)item.State;
            title.FinishCount = item.FinishCount;
            title.Used = item.Used;
            return title;
        }

        public TitleInfoItem CreateTitleInfoItem(int titleId)
        {
            TitleInfoItem title = new TitleInfoItem();
            title.Id = titleId;
            title.State = TitleState.New;
            title.FinishCount = 1;
            title.Used = 1;
            return title;
        }

        public TitleInfoItem CreateTitleInfoItem(int titleId, int finishCount)
        {
            TitleInfoItem title = new TitleInfoItem();
            title.Id = titleId;
            title.State = TitleState.None;
            title.FinishCount = finishCount;
            return title;
        }

        public void AddNature(TitleInfo titleModel)
        {
            if (titleModel != null)
            {
                foreach (var nature in titleModel.NatureList)
                {
                    if (NatureList.ContainsKey(nature.Key))
                    {
                        natureList[nature.Key] += nature.Value;
                    }
                    else
                    {
                        natureList.Add(nature.Key, nature.Value);
                    }
                }
            }
        }

        public bool ActivateTitle(int titleId)
        {
            TitleInfoItem title;
            if (titleList.TryGetValue(titleId, out title) && title.State > 0 && title.State != TitleState.Activated && title.State != TitleState.OverTime)
            {
                return false;
            }
            if (title == null)
            {
                title = CreateTitleInfoItem(titleId);
                titleList.Add(title.Id, title);
                SyncDbInsertTitleState(title);
            }
            else
            {
                title.Used = 1;
                title.State = TitleState.New;
                SyncDbUpdateTitleInfo(title);
            }
            return true;
        }

        public void ChangeTitle(int titleId)
        {
            TitleInfoItem oldTitle;
            titleList.TryGetValue(curTitleId, out oldTitle);
            if (oldTitle != null)
            {
                oldTitle.State = TitleState.Activated;
                SyncDbUpdateTitleState(oldTitle);
            }

            TitleInfoItem title;
            titleList.TryGetValue(titleId, out title);
            if (title != null)
            {
                curTitleId = title.Id;
                title.State = TitleState.Equiped;
                SyncDbUpdateTitleState(title);
            }
            else
            {
                curTitleId = 0;
            }
            SyncTitleToRedis();
        }

        public void ChangeTitleState(int titleId)
        {
            TitleInfoItem title;
            titleList.TryGetValue(titleId, out title);
            if (title != null && title.State < TitleState.Activated)
            {
                title.State = TitleState.Activated;
                SyncDbUpdateTitleState(title);
            }
        }
          
        public void UpdateTitleConditionCount(TitleObtainCondition conditionType, int count = 1, List<int> paramList = null)
        {
            List<TitleInfo> titleModelList = TitleLibrary.GetTitleListByCondition(conditionType);
            if (titleModelList == null)
            {
                return;
            }
            foreach (var title in titleModelList)
            {
                if (string.IsNullOrEmpty(title.ConditionNumber))
                {
                    continue;
                }
                string[] conditonParam = StringSplit.GetArray("|", title.ConditionNumber);
                if (conditonParam.Length >= 2 && paramList != null)
                {
                    int j = 0;
                    for (int i = 1; i < conditonParam.Length; i++)
                    {
                        if (conditonParam[i].ToInt() > paramList[j])
                        {
                            return;
                        }
                        j++;
                    }
                }
                TitleInfoItem item;
                if (!titleList.TryGetValue(title.Id, out item))
                {
                    item = CreateTitleInfoItem(title.Id, count);
                    titleList.Add(item.Id, item);                 
                    SyncDbInsertTitleFinishCount(item);
                }
                else
                {
                    if (item.FinishCount >= conditonParam[0].ToInt())
                    {
                        switch (conditionType)
                        {
                            case TitleObtainCondition.CampLeader:
                            case TitleObtainCondition.ArenaRankFirst:
                            case TitleObtainCondition.CrossBattleFirst:
                                if (item.State == TitleState.OverTime && item.Used == 1)
                                {
                                    item.State = TitleState.Activated;
                                    SyncDbUpdateTitleState(item);
                                    Owner.SendTitleInfo();
                                }
                                break;
                            default:
                                break;
                        }
                        return;
                    }
                    item.FinishCount += count;
                    SyncDbUpdateFinishCount(item);
                }
                CheckSendTitleCard(item, conditonParam[0].ToInt());
            }
        }

        private void CheckSendTitleCard(TitleInfoItem item, int conditionNumber)
        {
            TitleInfo titleModel = TitleLibrary.GetTitleById(item.Id);
            if (item.FinishCount >= conditionNumber && item.State == TitleState.None)
            {
                Owner.SendPersonEmail(TitleLibrary.EmailId, "", titleModel.Reward);
            }
        }

        public void UpdateTitleState(TitleObtainCondition conditionType, TitleState state, int titleSubType)
        {
            List<TitleInfo> titleModelList = TitleLibrary.GetTitleListByCondition(conditionType);
            if (titleModelList == null)
            {
                return;
            }
            foreach (var title in titleModelList)
            {
                TitleInfoItem item;
                if (titleList.TryGetValue(title.Id, out item) && titleSubType == title.SubType)
                {
                    item.State = state;
                    SyncDbUpdateTitleState(item);
                    if (item.Id == CurTitleId)
                    {
                        curTitleId = 0;
                        SyncTitleToRedis();
                    }
                }
            }
            Owner.SendTitleInfo();
            MSG_GC_CHAR_SIMPLE_INFO simpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
            Owner.GetSimpleInfo(simpleInfo);
            Owner.BroadCastNearbyMsg(simpleInfo);
        }

        public void CheckCurTitle()
        {
            foreach (var title in TitleList)
            {
                TitleInfo titleModel = TitleLibrary.GetTitleById(title.Key);
                CheckUpdateTitleState(title.Value, titleModel);
            }
        }

        private void CheckUpdateTitleState(TitleInfoItem title, TitleInfo titleModel)
        {
            bool hasOverTime = false;
            switch ((TitleObtainCondition)titleModel.ObtainCondition)
            {
                case TitleObtainCondition.CampLeader:
                    if (!Owner.CheckHasCampLeaderTitle(titleModel.SubType))
                    {
                        title.State = TitleState.OverTime;
                        if (title.Id == CurTitleId)
                        {
                            hasOverTime = true;
                        }
                    }
                    break;
                case TitleObtainCondition.ArenaRankFirst:
                    if (titleModel.SubType == 1 && Owner.ArenaMng.Rank != 1)
                    {
                        title.State = TitleState.OverTime;
                        if (title.Id == CurTitleId)
                        {
                            hasOverTime = true;
                        }
                    }
                    break;
                case TitleObtainCondition.CrossBattleFirst:
                    if (titleModel.SubType == 1 && Owner.CrossInfoMng.Info.LastFinalsRank != 1)
                    {
                        title.State = TitleState.OverTime;
                        if (title.Id == CurTitleId)
                        {
                            hasOverTime = true;
                        }
                    }
                    break;
                default:
                    break;
            }
            //无需同步DB,初始化时会检测是否过期
            if (hasOverTime)
            {
                curTitleId = 0;
                SyncTitleToRedis();
                Owner.SendTitleInfo();
                MSG_GC_CHAR_SIMPLE_INFO simpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
                Owner.GetSimpleInfo(simpleInfo);
                Owner.BroadCastNearbyMsg(simpleInfo);
            }
        }

        public bool CheckHasThisTitleCard(int titleId)
        {
            TitleInfoItem title;
            if (TitleList.TryGetValue(titleId, out title) && title.State > 0 && title.Used > 0)
            {
                return true;
            }
            return false;
        }

        public void CheckNeedUpdateTitleState(int titleId)
        {
            TitleInfoItem title;
            if (TitleList.TryGetValue(titleId, out title) && title.State == TitleState.OverTime)
            {
                title.State = TitleState.Activated;
                SyncDbUpdateTitleState(title);
                Owner.SendTitleInfo();
            }
        }

        public MSG_ZMZ_TITLE_INFO GetTitlesTransform()
        {
            MSG_ZMZ_TITLE_INFO msg = new MSG_ZMZ_TITLE_INFO();
            msg.CurTitle = CurTitleId;
            foreach (var title in TitleList)
            {
                ZMZ_TITLE_ITEM item = new ZMZ_TITLE_ITEM() { Id = title.Value.Id, State = (int)title.Value.State, FinishCount = title.Value.FinishCount, Used = title.Value.Used };
                msg.Titles.Add(item);
            }
            return msg;
        }

        public void LoadTitlesTransform(MSG_ZMZ_TITLE_INFO info)
        {
            curTitleId = info.CurTitle;
            foreach (var title in info.Titles)
            {
                TitleInfoItem item = new TitleInfoItem();
                item.Id = title.Id;
                item.State = (TitleState)title.State;
                item.FinishCount = title.FinishCount;
                item.Used = title.Used;

                TitleInfo titleModel = TitleLibrary.GetTitleById(title.Id);

                titleList.Add(item.Id, item);
                if (item.State >= TitleState.Activated)
                {
                    AddNature(titleModel);
                }
            }
        }
        #region db
        private void SyncDbInsertTitleState(TitleInfoItem title)
        {
            Owner.server.GameDBPool.Call(new QueryInsertTitleInfo(Owner.Uid, title.Id, (int)title.State, title.FinishCount, title.Used));
        }

        private void SyncDbInsertTitleFinishCount(TitleInfoItem title)
        {
            Owner.server.GameDBPool.Call(new QueryInsertTitleFinishCount(Owner.Uid, title.Id, title.FinishCount));
        }

        private void SyncDbUpdateTitleInfo(TitleInfoItem title)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateTitleInfo(Owner.Uid, title.Id, (int)title.State, title.Used));
        }

        public void SyncDbUpdateTitleState(TitleInfoItem title)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateTitleState(Owner.Uid, title.Id, (int)title.State));
        }

        private void SyncDbUpdateFinishCount(TitleInfoItem title)
        {
            Owner.server.GameDBPool.Call(new QueryUpdateTitleFinishCount(Owner.Uid, title.Id, title.FinishCount));
        }
        #endregion

        public void SyncTitleToRedis()
        {
            Owner.server.GameRedis.Call(new OperateSetCurTitle(Owner.Uid, curTitleId));
        }
    }
}
