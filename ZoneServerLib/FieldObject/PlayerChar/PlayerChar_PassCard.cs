using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        // 通行证
        public PassTaskManager PassCardMng { get; set; }

        public string Passcard;
        public int PasscardLevel;
        public int PasscardExp;
        public void InitPassCardTaskManager()
        {
            PassCardMng = new PassTaskManager(this);
        }

        /// <summary>
        /// 领取等级奖励
        /// </summary>
        public void GetPasscardReward(MSG_GateZ_GET_PASSCARD_LEVEL_REWARD info)
        {
            MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT res = new MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT();
            //某一个 or 全部
            if (info.GetAll)
            {
                res = PassCardMng.GetAllLevelReward(info);
            }
            else
            {
                res = PassCardMng.GetLevelReward(info);
            }

            Write(res);
        }

        /// <summary>
        /// 领取每日奖励
        /// </summary>
        public void GetPasscardDailyReward(MSG_GateZ_GET_PASSCARD_DAILY_REWARD info)
        {
            MSG_ZGC_PASSCARD_DAILY_REWARD_RESULT res = PassCardMng.GetDailyReward(info);
            Write(res);
        }

        /// <summary>
        /// 获取面板信息
        /// </summary>
        public void GetPasscardInfos(MSG_GateZ_GET_PASSCARD_PANEL_INFO info)
        {
            MSG_ZGC_PASSCARD_PANEL_INFO ret = PassCardMng.GeneratePanelInfo();
            Write(ret);
        }

        public void SendPassCardMsg()
        {
            PassCardMng.SetNeedSendPanel();
        }

        /// <summary>
        /// 临时调试用，直接传东西过来购买完成
        /// </summary>
        /// <param name="rechargeId"></param>
        public void UpdatePasscardRechargeLevel(MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL info)
        {
            MSG_ZGC_PASSCARD_RECHARGE_LEVEL_RESULT res = PassCardMng.BuyPasscardLevel(info);
            Write(res);
        }

        /// <summary>
        /// 购买通行证
        /// </summary>
        public void GetPasscardBought(MSG_GateZ_GET_PASSCARD_RECHARGED info)
        {
            //
            //MSG_ZGC_PASSCARD_RECHARGE_RESULT res=PassCardMng.BuyPasscard();
            //Write(res);
        }

        /// <summary>
        /// 领取任务奖励
        /// </summary>
        public void GetPasscardTaskComplete(MSG_GateZ_GET_PASSCARD_TASK_EXP info)
        {
            MSG_ZGC_UPDATE_PASSCARD_TASK res = null;
            if (info.GetAll)
            {
                res = PassCardMng.CompleteAllTask();
            }
            else
            {
                res = PassCardMng.CompleteTask(info.TaskId);
            }
            Write(res);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public void AddPassCardTaskNum(TaskType type, int num = 1)
        {
            PassCardMng.AddTypeTaskNum(type, num);
        }

        public void AddPassCardTaskNum(TaskType type, int param, string paramString)
        {
            PassCardMng.AddTypeTaskNum(type, param, paramString);
        }

        public void AddPassCardTaskNum(TaskType type, int param, string paramString, int num = 1)
        {
            PassCardMng.AddTypeTaskNum(type, param, paramString, num);
        }

        public void AddPassCardTaskNum(TaskType type, int[] param, string[] paramString)
        {
            PassCardMng.AddTypeTaskNum(type, param, paramString);
        }

        public void AddPassCardTaskNumByMainType(ItemReceive itemReceive)
        {
            switch ((MainType)itemReceive.MainType)
            {
                case MainType.Consumable:
                    ItemModel item = BagLibrary.GetItemModel(itemReceive.Id);
                    AddPassCardTaskNumByConsumeType(item.SubType);
                    break;
                default:
                    break;
            }
        }

        private void AddPassCardTaskNumByConsumeType(int subType)
        {
            switch ((ConsumableType)subType)
            {
                case ConsumableType.TreasureMap:
                    AddPassCardTaskNum(TaskType.ReceiveTreasureMap);
                    break;
                default:
                    break;
            }
        }

        public void UpdatePassTask()
        {
            PassCardMng.Update();
        }

        public void LoadDBTaskItemList(List<PassCardTaskItem> list)
        {
            PassCardMng.AddTaskListItem(list);
        }

        public void LoadDBPassCardInfo(QueryLoadPassCardReward reward)
        {

            //是否有高级通行证  通行证等级
            PassCardMng.InitPasscard(Passcard, PasscardLevel, PasscardExp, reward.curPeriod);
            //等级奖励字符串
            //每日奖励发没发
            PassCardMng.LoadReward(reward);

        }

        public void RefreshDailyPassCard()
        {
            PassCardMng.SetNeedDailyRefresh();
        }

        public void RefreshWeeklyPassCard()
        {
            PassCardMng.SetNeedWeeklyRefresh();
        }

        public void RefreshPassCardPeriod()
        {
            PassCardMng.RefreshPassCardPeriod();
        }

        public ZMZ_PASSCARD_INFO GetPassCardTransform()
        {
            return PassCardMng.GetPassCardTransform();
        }

        public void LoadPasscardTransform(ZMZ_PASSCARD_INFO passcardInfo)
        {
            PassCardMng.LoadPasscardTransform(passcardInfo);
        }
    }
}
