using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using ZoneServerLib.Welfare;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //运营活动

        public WelfareManager WelfareMng = new WelfareManager();

        public void LoadWelfareInfoList(List<WelfareTriggerItem> list)
        {
            foreach (var item in list)
            {
                WelfareTriggerInfo info = WelfareLibrary.GetTriggerInfo(item.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    item.State = WelfareTriggerState.Timeout;
                    //保存数据库
                    SyncDbUpdateWelfareTriggerItem(item);
                    continue;
                }
                if (info.EndDays > 0)
                {
                    DateTime stratTime = Timestamp.TimeStampToDateTime(item.StartTime);
                    double day = (ZoneServerApi.now - stratTime).TotalDays;
                    if (day > info.EndDays)
                    {
                        item.State = WelfareTriggerState.Timeout;
                        //保存数据库
                        SyncDbUpdateWelfareTriggerItem(item);
                    }
                }
                if (WelfareMng.CheckTriggerItem(item.Id))
                {
                    Log.Warn("player {0} LoadWelfareInfoList add trigger item error: has same id {1}", Uid, item.Id);
                }
                else
                {
                    WelfareMng.AddTriggerItem(item);
                }
            }
        }

        public void LoadWelfareTriggerTransform(RepeatedField<MSG_ZMZ_WELFARE_TRIGGER_INFO> triggers)
        {
            foreach (var item in triggers)
            {
                WelfareTriggerItem trigger = new WelfareTriggerItem();
                trigger.Id = item.Id;
                trigger.StartTime = item.StartTime;
                trigger.EndTime = item.EndTime;
                trigger.State = (WelfareTriggerState)item.State;

                WelfareTriggerInfo info = WelfareLibrary.GetTriggerInfo(item.Id);
                if (info == null)
                {
                    //说明今天没有这个活动，可以删除了
                    trigger.State = WelfareTriggerState.Timeout;
                    //保存数据库
                    SyncDbUpdateWelfareTriggerItem(trigger);
                    continue;
                }

                WelfareMng.AddTriggerItem(trigger);
            }
        }

        public void AddWelfareTriggerItem(WelfareConditionType type, int parm)
        {
            List<WelfareTriggerItem> addList = new List<WelfareTriggerItem>();
            //获取当前类型所有活动
            Dictionary<int, WelfareTriggerInfo> dic = WelfareLibrary.GetTriggerInfos(type, parm);
            if (dic != null && dic.Count > 0)
            {
                foreach (var item in dic)
                {
                    //查看是否有这个活动
                    if (WelfareMng.CheckTriggerItem(item.Key))
                    {
                        //说明已经添加过福利
                        //Log.Warn("player {0} add welfare trigger  has id {1} info.", Uid, item.Key);
                        continue;
                    }
                    else
                    {
                        WelfareTriggerItem trigger = new WelfareTriggerItem();
                        trigger.Id = item.Value.Id;
                        trigger.State = WelfareTriggerState.Start;
                        trigger.StartTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

                        switch (item.Value.Type)
                        {
                            case WelfareType.Quest:
                                break;
                            case WelfareType.Email:
                                if (item.Value.ChannelLimit.Count > 0)
                                {
                                    if (!item.Value.ChannelLimit.Contains(ChannelName))
                                    {
                                        continue;
                                    }
                                }
                                ChangeWelfareTriggerState(trigger, item.Value, WelfareTriggerState.End);
                                break;
                            default:
                                ChangeWelfareTriggerState(trigger, item.Value, WelfareTriggerState.End);
                                break;
                        }

                        addList.Add(trigger);
                        WelfareMng.AddTriggerItem(trigger);
                    }
                }
            }

            if (addList.Count > 0)
            {
                //TODO 记录数据库
                foreach (var item in addList)
                {
                    SyncDbInsertWelfareTriggerItem(item);
                }
                //TODO 发消息给前台
                SyncWelfareTriggerChangeMessage(addList);
            }
        }

        public void WelfareTriggerChangeState(int id, int state)
        {
            MSG_ZGC_WELFARE_TRIGGER_STATE msg = new MSG_ZGC_WELFARE_TRIGGER_STATE();
            msg.Id = id;
            //查看今天是否有这个活动
            WelfareTriggerInfo info = WelfareLibrary.GetTriggerInfo(id);
            if (info == null)
            {
                //说明今天没有这个活动
                Log.Warn("player {0} WelfareTriggerChangeState not find trigger info: {1}", Uid, id);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //查看身上是否有这个活动
            WelfareTriggerItem triiger = WelfareMng.GetTriggerItemForId(id);
            if (triiger == null)
            {
                //是空说明可能是没有做过这个活动，但是今天是有这个活动的，所以直接新增一个
                Log.Warn("player {0} WelfareTriggerChangeState not find trigger item: {1}", Uid, id);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查任务完成条件
            if (!CheckAndChangeWelfareTriggerState(triiger, info, (WelfareTriggerState)state))
            {
                Log.Warn("player {0} WelfareTriggerChangeState has error: id {1}", Uid, id);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }


            msg.ErrorCode =(int)ErrorCode.Success;
            Write(msg);

            //TODO 更新数据库
            SyncDbUpdateWelfareTriggerItem(triiger);

            //TODO 发消息给前台
            SyncWelfareTriggerChangeMessage(new List<WelfareTriggerItem>() { triiger });
        }

        public void SyncWelfareTriggerChangeMessage(List<WelfareTriggerItem> updateList)
        {
            MSG_ZGC_WELFARE_TRIGGER_CHANGE msg = new MSG_ZGC_WELFARE_TRIGGER_CHANGE();
            if (updateList != null)
            {
                foreach (var item in updateList)
                {
                    msg.UpdateList.Add(GetWelfareTriggerMsgInfo(item));
                }
            }
            Write(msg);
        }

        private MSG_ZGC_WELFARE_TRIGGER_INFO GetWelfareTriggerMsgInfo(WelfareTriggerItem item)
        {
            MSG_ZGC_WELFARE_TRIGGER_INFO info = new MSG_ZGC_WELFARE_TRIGGER_INFO();
            info.Id = item.Id;
            info.Time = item.StartTime;
            info.State = (int)item.State;
            return info;
        }

        public List<MSG_ZGC_WELFARE_TRIGGER_INFO> GetWelfareTriggerListMessage()
        {
            List<MSG_ZGC_WELFARE_TRIGGER_INFO> list = new List<MSG_ZGC_WELFARE_TRIGGER_INFO>();
            Dictionary<int, WelfareTriggerItem> triggerList = WelfareMng.GetTriggerList();
            foreach (var trigger in triggerList)
            {
                if (trigger.Value.State == WelfareTriggerState.Start)
                {
                    list.Add(GetWelfareTriggerMsgInfo(trigger.Value));
                }
            }
            return list;
        }

        public bool CheckAndChangeWelfareTriggerState(WelfareTriggerItem trigger, WelfareTriggerInfo info, WelfareTriggerState newState)
        {
            if (trigger.State != WelfareTriggerState.Start)
            {
                //状态不正确
                Log.Warn("player {0} CheckChangeWelfareTriggerState trigger {1} has error: state is {2} new is {3}.", Uid, info.Id, trigger.State, newState);
                return false;
            }
            if (info.EndDays > 0)
            {
                DateTime stratTime = Timestamp.TimeStampToDateTime(trigger.StartTime);
                DateTime endTime = stratTime.AddDays(info.EndDays);
                if (ZoneServerApi.now > endTime)
                {
                    //说明时间不正确
                    Log.Warn("player {0} CheckChangeWelfareTriggerState trigger {1} has error: endTime {2} .", Uid, info.Id, endTime);
                    return false;
                }
            }

            switch (info.Type)
            {
                case WelfareType.Quest:
                    if (newState == WelfareTriggerState.Start)
                    {
                        trigger.Wait = true;
                        trigger.WaitTime = ZoneServerApi.now;
                        return true;
                    }
                    else if (newState == WelfareTriggerState.End)
                    {
                        //检查状态和时间
                        if (!trigger.Wait)
                        {
                            Log.Warn("player {0} CheckChangeWelfareTriggerState trigger {1} has error: Wait is {2} .", Uid, info.Id, trigger.Wait);
                            return false;
                        }
                        else
                        {
                            if (info.CheckTime > 0)
                            {
                                double passTime = (ZoneServerApi.now - trigger.WaitTime).TotalSeconds;
                                if (passTime< info.CheckTime)
                                {
                                    Log.Warn("player {0} CheckChangeWelfareTriggerState trigger {1} has error: Wait time is {2} .", Uid, info.Id, trigger.WaitTime);
                                    return false;
                                }
                            }
                        }

                        ChangeWelfareTriggerState(trigger, info, newState);
                        return true;
                    }
                    else
                    {
                        Log.Warn("player {0} CheckChangeWelfareTriggerState trigger {1} has error: state is {2} new is {3} .", Uid, info.Id, trigger.State, newState);
                        return false;
                    }
                case WelfareType.Email:
                default:
                    Log.Warn("player {0} CheckChangeWelfareTriggerState trigger {1} has error: type is {2} .", Uid, info.Id, info.Type);
                    return false;
            }
        }

        public void ChangeWelfareTriggerState(WelfareTriggerItem trigger, WelfareTriggerInfo info, WelfareTriggerState newState)
        {
            trigger.State = newState;

            if (newState == WelfareTriggerState.End)
            {
                trigger.EndTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

                //发送奖励邮件
                foreach (var item in info.EmailList)
                {
                    SendPersonEmail(item.Key, "", "", 0, item.Value);
                }
            }
        }

        public void SyncDbUpdateWelfareTriggerItem(WelfareTriggerItem item)
        {
            server.GameDBPool.Call(new QueryUpdateWelfareTriggerItem(Uid, item.Id, (int)item.State, item.StartTime, item.EndTime));
        }

        public void SyncDbInsertWelfareTriggerItem(WelfareTriggerItem item)
        {
            server.GameDBPool.Call(new QueryInsertWelfareTriggerItem(Uid, item.Id, (int)item.State, item.StartTime, item.EndTime));
        }

    }
}
