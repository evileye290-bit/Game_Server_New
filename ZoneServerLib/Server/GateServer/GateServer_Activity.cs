using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_ActivityComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ACTIVITY_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ACTIVITY_COMPLETE>(stream);
            Log.Write("player {0} complete activity {1}", uid, pks.ActivityId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} complete activity not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} complete activity not in map ", uid);
                return;
            }

            //处理任务
            player.ActivityComplete(pks.ActivityId);
        }

        public void OnResponse_ActivityTypeComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ACTIVITY_TYPE_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ACTIVITY_TYPE_COMPLETE>(stream);
            Log.Write("player {0} complete type activity {1}", uid, pks.ActivityType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} complete type activity not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} complete type activity not in map ", uid);
                return;
            }

            //处理任务
            player.ActivityTypeComplete((EnumerateUtility.Activity.ActivityAction)pks.ActivityType);
        }

        public void OnResponse_ActivityRelatedComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ACTIVITY_RELATED_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ACTIVITY_RELATED_COMPLETE>(stream);
            Log.Write("player {0} complete related activity {1}", uid, pks.ActivityId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} complete related activity not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} complete related activity not in map ", uid);
                return;
            }

            //处理任务
            player.ActivityRelatedComplete(pks.ActivityId);
        }

        public void OnResponse_RechargeRebateReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RECHARGE_REBATE_GET_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RECHARGE_REBATE_GET_REWARD>(stream);
            Log.Write("player {0} RechargeRebateReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} RechargeRebateReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} RechargeRebateReward not in map ", uid);
                return;
            }

            player.RechargeRebateReward();
        }

        public void OnResponse_SpceilActivityComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPECIAL_ACTIVITY_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPECIAL_ACTIVITY_COMPLETE>(stream);
            Log.Write("player {0} complete Spceil activity {1}", uid, pks.ActivityId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} complete activity not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} complete activity not in map ", uid);
                return;
            }

            //处理任务
            player.SpecialActivityComplete(pks.ActivityId);
        }

        public void OnResponse_RunawayActivityComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RUNAWAY_ACTIVITY_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RUNAWAY_ACTIVITY_COMPLETE>(stream);
            Log.Write("player {0} complete Runaway activity {1}", uid, pks.ActivityId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} complete Runaway activity not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} complete Runaway activity not in map ", uid);
                return;
            }

            //处理任务
            player.RunawayActivityComplete(pks.ActivityId);
        }

        public void OnResponse_GetWebPayRebateReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_WEBPAY_REBATE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_WEBPAY_REBATE_REWARD>(stream);
            Log.Write("player {0} Get WebPay Rebate Reward {1}", uid, pks.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} Get WebPay Rebate Reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} Get WebPay Rebate Reward not in map ", uid);
                return;
            }
            
            player.GetWebPayRebateReward(pks.Id);
        }
    }
}
