using CommonUtility;
using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using EnumerateUtility;
using Logger;

namespace ZoneServerLib
{
    public class IslandHighManager : BasePeriodActivity
    {
        private Dictionary<HighMainEventType, BaseHighEvent> highEvent = new Dictionary<HighMainEventType, BaseHighEvent>();

        private List<MSG_ISLAND_HIGH_HEIGHT_ROCK_EVENT> eventParamList = new List<MSG_ISLAND_HIGH_HEIGHT_ROCK_EVENT>();

        private IslandHighDbInfo islandHighDbInfo;
        public IslandHighDbInfo IslandHighDbInfo => islandHighDbInfo;

        public PlayerChar Owner { get; }
        public int GridIndex => islandHighDbInfo.GridIndex;

        public RewardManager RewardManager = new RewardManager();

        public IslandHighManager(PlayerChar player) : base(RechargeGiftType.IslandHigh)
        {
            Owner = player;
        }

        public void Init(IslandHighDbInfo info)
        {
            islandHighDbInfo = info;
            if (islandHighDbInfo.GridIndex == 0)
            {
                islandHighDbInfo.GridIndex = 1;
            }

            InitHighEvent();
            InitPeriodInfo();
        }

        private void InitHighEvent()
        {
            highEvent.Add(HighMainEventType.GoForward, new HighGoForward(this));
            highEvent.Add(HighMainEventType.AddItem, new HighAddItemEvent(this));
        }

        private BaseHighEvent GetEvent(HighMainEventType eventType)
        {
            BaseHighEvent highEvent;
            this.highEvent.TryGetValue(eventType, out highEvent);
            return highEvent;
        }

        public void AddGrid(int num)
        {
            if (num <= 0) return;

            //首次事件还需要走的步数(没走完就触发了随机事件，随机时间处理完成后吧剩余步数走完)
            int resNum = num;

            List<IslandHighGridModel> eventModels = new List<IslandHighGridModel>();

            for (int tempIndex = 1; tempIndex <= num; tempIndex++)
            {
                resNum -= 1;

                int curGridIndex = GridIndex + 1;
                int curIndex = GetLoopIndex(curGridIndex);

                IslandHighGridModel model = IslandHighLibrary.GetIslandHighGridModel(Period, curIndex);
                if (model == null)
                {
                    Log.Error($"have not this grid model  grid {curGridIndex} reward index ");
                    continue;
                }

                SetGridIndex(curGridIndex);

                if (!string.IsNullOrEmpty(model.Reward))
                {
                    //获得当前格子奖励
                    RewardManager.AddSimpleReward(model.Reward);
                }

                if (model.EventId > 0)
                {
                    IslandHighEventModel eventModel = IslandHighLibrary.HighGetEvent(Period, model.EventId);
                    if (eventModel == null)
                    {
                        Log.Error($"island high have no event model in grid {curGridIndex} modelId {curIndex} period {Period} event id {model.EventId}");
                        continue;
                    }

                    //随机事件和加步数踩上去才会触发。获得道具经过就触发
                    if (eventModel.EventType == HighMainEventType.GoForward || eventModel.EventType == HighMainEventType.Random)
                    {
                        if (resNum == 0)
                        {
                            InvokeEvent(model, curGridIndex);
                        }
                        else
                        {
                            //移动一格
                            AddEvent(curGridIndex, 0, 1);
                        }
                    }
                    else
                    {
                        InvokeEvent(model, curGridIndex);
                    }
                }
                else
                {
                    //移动一格
                    AddEvent(curGridIndex, 0, 1);
                }
            }
        }

        private int GetLoopIndex(int gridIndex)
        {
            int maxIndex = IslandHighLibrary.GetMaxGridIndex(Period);
            if (gridIndex > maxIndex)
            {
                gridIndex = gridIndex % maxIndex;
                if (gridIndex == 0)
                {
                    gridIndex = maxIndex;
                }
            }

            return gridIndex;
        }

        private void InvokeEvent(IslandHighGridModel model, int index)
        {
            if (model == null) return;

            IslandHighEventModel eventModel = IslandHighLibrary.HighGetEvent(Period, model.EventId);
            if(eventModel == null) return;

            bool added = false;
            if (eventModel.EventType == HighMainEventType.Random)
            {
                added = true;
                IslandHighEventModel tempEventModel = IslandHighLibrary.HighRandomEvent();
                AddEvent(index, eventModel.Id, tempEventModel.Id);
                eventModel = tempEventModel;
            }

            BaseHighEvent highEvent = GetEvent(eventModel.EventType);
            if (highEvent != null)
            {
                if (!added)
                {
                    AddEvent(index, eventModel.Id, eventModel.EventParam);
                }

                highEvent.Invoke(eventModel.EventParam);
            }
        }

        private void SetGridIndex(int index)
        {
            islandHighDbInfo.GridIndex = index;
        }

        public void AddEvent(int gridIndex, int eventId, int itemParam)
        {
            eventParamList.Add(new MSG_ISLAND_HIGH_HEIGHT_ROCK_EVENT() { GridIndex = gridIndex, EventType = eventId, EventParam = itemParam });
        }

        public void ResetEventParamList()
        {
            eventParamList.Clear();
        }

        public List<MSG_ISLAND_HIGH_HEIGHT_ROCK_EVENT> GetEventParamList()
        {
            //RemoveLastMoveEvent();
            return eventParamList;
        }

        private void RemoveLastMoveEvent()
        {
            //如果最后一个是向前移动，会导致前端表现上比后端多移动一格
            if (eventParamList.Count <= 1) return;
            int index = eventParamList.Count - 1;

            if (eventParamList[index].EventType == 0)
            {
                eventParamList.RemoveAt(index);
            }
        }

        public void ResetRewardCache()
        {
            RewardManager.Clear();
        }

        public override void Clear()
        {
            islandHighDbInfo.Reset();
            SyncDbHighInfo();
            InitPeriodInfo();
        }

        public MSG_ZGC_ISLAND_HIGH_INFO GenerateMsg()
        {
            MSG_ZGC_ISLAND_HIGH_INFO msg = new MSG_ZGC_ISLAND_HIGH_INFO();
            msg.ErrorCode = (int)ErrorCode.Success;
            msg.GridIndex = GridIndex;
            msg.StageRewardList.Add(IslandHighDbInfo.StageRewardList);
            msg.TotalRewardList.Add(IslandHighDbInfo.TotalRewardList);
            return msg;
        }

        public MSG_ZMZ_ISLAND_HIGH_INFO GenerateTransformInfo()
        {
            MSG_ZMZ_ISLAND_HIGH_INFO msg = new MSG_ZMZ_ISLAND_HIGH_INFO() {GridIndex = islandHighDbInfo.GridIndex};
            msg.StageRewardList.AddRange(islandHighDbInfo.StageRewardList);
            msg.TotalRewardList.AddRange(islandHighDbInfo.TotalRewardList);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_ISLAND_HIGH_INFO msg)
        {
            IslandHighDbInfo info = new IslandHighDbInfo();
            info.GridIndex = msg.GridIndex;
            info.StageRewardList = new List<int>(msg.StageRewardList);
            info.TotalRewardList = new List<int>(msg.TotalRewardList);

            Init(info);
        }

        public void SyncDbHighInfo()
        {
            QueryUpdateIslandHigh query = new QueryUpdateIslandHigh(Owner.Uid, islandHighDbInfo);
            Owner.server.GameDBPool.Call(query);
        }

    }
}
