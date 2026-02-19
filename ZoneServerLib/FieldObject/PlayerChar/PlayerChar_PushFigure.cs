using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public PushFigureManager pushFigureManager = null;

        public void InitPushFigureManager()
        {
            pushFigureManager = new PushFigureManager(this);
        }

        public void LoadPushFigureInfo(int id, int status)
        { 
            pushFigureManager.Init(id, (PushFigureStatus)status);
        }

        public ErrorCode ChechCreatePushFigureDungeon(int dungeonId)
        {
            PushFigureModel model = PushFigureLibrary.GetPushFigureModelBuDungeonId(dungeonId);
            if (model == null) return ErrorCode.Fail;

            if (model.Id != pushFigureManager.Id)
            {
                return ErrorCode.Fail;
            }

            if (pushFigureManager.PushFigureStatus == PushFigureStatus.Finished)
            {
                return ErrorCode.PushFigureTaskFinished;
            }

            if (pushFigureManager.PushFigureStatus == PushFigureStatus.NotOpen)
            { 
                return ErrorCode.PushFigureNeedFinishBefore;
            }

            return ErrorCode.Success;
        }

        public void PushFigureSuccess(RewardManager manager, int dungeonId)
        {
            PushFigureModel model = PushFigureLibrary.GetPushFigureModelBuDungeonId(dungeonId);
            if (model == null) return;

            if (model.Id == pushFigureManager.Id)
            {
                string rewardStr = model.Data.GetString("Reward");
                if (!string.IsNullOrEmpty(rewardStr))
                {
                    manager.AddSimpleReward(rewardStr);
                }

                pushFigureManager.SetFinished();
            }

            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.PushFigure, model.Data.Name);

            if (CurrentMap.IsDungeon)
            {
                MSG_ZGC_DUNGEON_REWARD msg = new MSG_ZGC_DUNGEON_REWARD();
                msg.DungeonId = dungeonId;
                msg.Result = (int)DungeonResult.Success;
                manager.GenerateRewardMsg(msg.Rewards);

                CheckCacheRewardMsg(msg);
            }

            AddTaskNumForType(TaskType.PushFigureComplete, 1, true, model.Id);
        }

        public void SendPushFigurateInfo()
        {
            MSG_ZGC_PUSHFIGURE_INFO msg = GeneratePushFigureInfo();
            Write(msg);
        }

        public void PushFigureFinishTask(int taskId)
        {
            MSG_ZGC_PUSHFIGURE_FINISHTASK msg = new MSG_ZGC_PUSHFIGURE_FINISHTASK();
            if (taskId < pushFigureManager.Id || (taskId == pushFigureManager.Id && pushFigureManager.PushFigureStatus == PushFigureStatus.Finished))
            {
                Log.Warn($"player {Uid} push figure finish task {taskId} failed: already finish");
                msg.Result = (int)ErrorCode.PushFigureTaskFinished;
                Write(msg);
                return;
            }

            if (taskId > pushFigureManager.Id)
            {
                Log.Warn($"player {Uid} push figure finish task {taskId} failed: not finish");
                msg.Result = (int)ErrorCode.PushFigureNeedFinishBefore;
                Write(msg);
                return;
            }

            if (pushFigureManager.PushFigureStatus != PushFigureStatus.Opening)
            {
                Log.Warn($"player {Uid} push figure finish task {taskId} failed: not open");
                msg.Result = (int)ErrorCode.PushFigureNotOpening;
                Write(msg);
                return;
            }

            PushFigureModel model = PushFigureLibrary.GetPushFigureModel(taskId);
            if (model == null)
            {
                Log.Warn($"player {Uid} push figure finish task {taskId} failed: not find push figure");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            string rewardStr = model.Data.GetString("Reward");
            if (!string.IsNullOrEmpty(rewardStr))
            {
                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(rewardStr, true);

                AddRewards(manager, ObtainWay.PushFigure, model.Type.ToString());
                manager.GenerateRewardItemInfo(msg.Rewards);
            }

            msg.Result = (int)ErrorCode.Success;
            pushFigureManager.SetFinished();
            Write(msg);
        }

        public MSG_ZGC_PUSHFIGURE_INFO GeneratePushFigureInfo()
        {
            MSG_ZGC_PUSHFIGURE_INFO msg = new MSG_ZGC_PUSHFIGURE_INFO()
            {
                Id = pushFigureManager.Id,
                Status = (int)pushFigureManager.PushFigureStatus
            };
            return msg;
        }

        public MSG_ZMZ_PUSHFIGURE_INFO GeneratePushFigureTransformMsg()
        {
            MSG_ZMZ_PUSHFIGURE_INFO msg = new MSG_ZMZ_PUSHFIGURE_INFO()
            {
                Id = pushFigureManager.Id,
                Status = (int)pushFigureManager.PushFigureStatus
            };
            return msg;
        }

        public void LoadPushFigureTransform(MSG_ZMZ_PUSHFIGURE_INFO info)
        {
            pushFigureManager.Init(info.Id, (PushFigureStatus)info.Status);
        }
    }
}
