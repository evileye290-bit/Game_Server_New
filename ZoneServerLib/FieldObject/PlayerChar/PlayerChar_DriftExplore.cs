using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        private DriftExploreManager driftExploreMng;
        public DriftExploreManager DriftExploreMng
        {
            get => driftExploreMng;
            set => driftExploreMng = value;
        }

        public void InitDriftExplore()
        {
            driftExploreMng = new DriftExploreManager(this);
        }
        public void DriftExploreOpen()
        {
            if(IsDriftExploreOpening())
            {
                DriftExploreMng.RefreshDriftExplore();
            }
            
            DriftExploreMng.SendDriftExploreAllInfo();
        }
        
        public bool IsDriftExploreOpening()
        {
            return CheckLimitOpen(LimitType.DriftExplore);
        }
        public void RefreshWeeklyDriftExplore()
        {
            DriftExploreMng.RefreshDriftExplore();
            SendDriftExploreInfo();
        }
        
        public void SendDriftExploreInfo()
        {
            //任务完成信息

            driftExploreMng.SendDriftExploreTasksInfo();
            driftExploreMng.SendDriftExploreInfo();

        }
        
        /// <summary>
        /// 领取任务奖励
        /// </summary>
        public void DriftExploreTaskReward(int taskId)
        {
            MSG_ZGC_DRIFT_EXPLORE_TASK_REWARD res = new MSG_ZGC_DRIFT_EXPLORE_TASK_REWARD();
            res.TaskId = taskId;
            res.Result = (int) ErrorCode.Success;

            DriftExploreTaskInfo task;
            driftExploreMng.DriftExploreTasks.TryGetValue(taskId, out task);
            if (task == null)
            {
                Log.Warn($"player {Uid} get DriftExplore task {taskId} finish reward failed: not find task");
                res.Result = (int)ErrorCode.Fail;
                return;
            }

            if (!task.Rewarded && task.CurNum >= task.ParamNum)
            {
                DriftExploreTaskModel taskModel = driftExploreMng.GetDriftExploreTaskByTaskId(task.Id);
                if (taskModel == null)
                {
                    Log.Warn($"player {Uid} get DriftExplore task reward not find {task.Id} in xml");
                    res.Result = (int)ErrorCode.Fail;
                }

                if (!driftExploreMng.CheckParentRewarded(task))
                {
                    Log.Warn($"player {Uid} get DriftExplore task {task.Id} parent not Rewarded");
                    res.Result = (int)ErrorCode.Fail;
                }

                driftExploreMng.GetDriftExploreReward(task);

                RewardManager manager = new RewardManager();
                string rewardString = taskModel.Reward;
                if (!string.IsNullOrEmpty(rewardString))
                {
                    //获取奖励
                    manager = GetSimpleReward(rewardString, ObtainWay.DriftExplore);
                    manager.GenerateRewardItemInfo(res.Rewards);
                }
                res.Result = (int)ErrorCode.Success;
            }
            else
            {
                Log.Warn($"player {Uid} complete DriftExplore task {taskId} failed: not complete");
                res.Result = (int)ErrorCode.Fail;
            }

            Write(res);
        }
        
        /// <summary>
        /// 领取积分奖励
        /// </summary>
        public void DriftExploreReward()
        {
            MSG_ZGC_DRIFT_EXPLORE_REWARD res = new MSG_ZGC_DRIFT_EXPLORE_REWARD();
            res.Result = (int) ErrorCode.Fail;

            DriftExploreConfigModel model = driftExploreMng.GetDriftExploreConfig();
            int finishNum = model.FinishNum;
            if (finishNum == 0)
            {
                Log.Warn($"player {Uid} DriftExploreReward index failed: xml FinishNum error");
                res.Result = (int)ErrorCode.Fail;
                Write(res);
                return;
            }
          
            if (DriftExploreMng.CheckGotDriftExploreReward())
            {
                Log.Warn($"player {Uid} DriftExploreReward failed: already rewarded");
                res.Result = (int)ErrorCode.ActivityFinishStateHadReward;
                Write(res);
                return;
            }

            if (!DriftExploreMng.CheckDriftExploreNum(finishNum))
            {
                Log.Warn($"player {Uid} DriftExploreReward failed: not reach num limit");
                res.Result = (int)ErrorCode.Fail;
                Write(res);
                return;
            }

            driftExploreMng.UpdateDriftExploreReward();

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Reward);
            AddRewards(manager, ObtainWay.DriftExplore);

            manager.GenerateRewardMsg(res.Rewards);
            res.Result = (int)ErrorCode.Success;
            Write(res);
        }
        
        public void AddDriftExploreTaskNum(TaskType type, double num = 1, bool replace = false, object obj = null)
        {
            if (CheckLimitOpen(LimitType.DriftExplore))
            {
                driftExploreMng.AddTypeTaskNum(type, num, replace, obj);
            }
        }
    }
}
