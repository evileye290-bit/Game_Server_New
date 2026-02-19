using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateZ;
using ServerShared;
using System;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {

        public void OnResponse_TaskComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASK_COMPLETE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASK_COMPLETE>(stream);
            Log.Write("player {0} task complete, taskId {1} npcId {2}", uid, pks.TaskId, pks.ZoneNpcId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} complete task not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} complete task not in map ", uid);
                return;
            }
       
            //处理任务
            player.TaskComplete(pks.TaskId, pks.ZoneNpcId);
        }

        public void OnResponse_TaskCollect(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASK_COLLECT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASK_COLLECT>(stream);
            Log.Write("player {0} task collect goods {1}", uid, pks.ZoneGoodsId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} task collect not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} task collect not in map ", uid);
                return;
            }
            //检查NPC是否包含任务ID
            Goods goods = player.CurrentMap.GetGoodsById(pks.ZoneGoodsId);
            if (goods == null)
            {
                Log.Warn("player {0} task collect not find goods {1} in map {2}", uid, pks.ZoneGoodsId, player.CurrentMap.MapId);
                return;
            }
            float temp = (float)Math.Pow(2 + player.Radius + goods.Radius, 2);
            float dis = (float)Vec2.GetRangePower(player.Position, goods.Position);
            //2默认范围
            if (dis > temp)
            {
                Log.Warn("player {0} task collect not find goods {1} in map {2} px {3}, py {4}    gx {5}, gy {6}",
                    uid, pks.ZoneGoodsId, player.CurrentMap.MapId, player.Position.X, player.Position.Y, goods.Position.X, goods.Position.Y);
                return;
            }
            //处理任务
            player.AddTaskNumForType(TaskType.Collect, 1, true, goods.ZoneGoodsId);
        }

        public void OnResponse_TaskSelect(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASK_SELECT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASK_SELECT>(stream);
            Log.Write("player {0} task select, taskId {1} index {2}", pks.PcUid, pks.TaskId, pks.Index);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} task select not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} task select not in map ", pks.PcUid);
                return;
            }

            player.TaskSelect(pks.TaskId, pks.Index);
        }

        public void OnResponse_TaskMake(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASK_MAKE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASK_MAKE>(stream);
            Log.Write("player {0} task make {1}", pks.PcUid, pks.TaskId);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} task make not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} task make not in map ", pks.PcUid);
                return;
            }
            //处理任务
            player.AddTaskNumForId(pks.TaskId, (int)TaskType.Make);
        }

        public void OnResponse_OpenEmailTask(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_OPENE_EMAIL_TASK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPENE_EMAIL_TASK>(stream);
            Log.Write("player {0} open email task id {1} sendTime {2}", pks.PcUid, pks.Id, pks.SendTime);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} open email task not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} open email task not in map ", pks.PcUid);
                return;
            }
            player.GetEmailTask(pks.Uid, pks.Id, pks.SendTime);

            //if (list.Count > 0)
            //{

            //    //Log.Warn("player {0} open email task has email task count {1}", pks.PcUid, list.Count);
            //    //player.SendErrorCodeMsg(ErrorCode.HasEmailTask);
            //}
            //else
            //{
            //    //删除邮件
            //}
        }


        public void OnResponse_SaveGuideId(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SAVE_GUIDE_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SAVE_GUIDE_ID>(stream);
            Log.Write("player {0} save guided id {1} time {2}", pks.PcUid, pks.GuideId, pks.Time);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} save guide id not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }

            //处理任务
            player.SaveGuideId(pks.GuideId, pks.Time);
        }

        public void OnResponse_SaveMainLineId(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SAVE_MAIN_LINE_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SAVE_MAIN_LINE_ID>(stream);
            Log.Write("player {0} save main line id {1}", pks.PcUid, pks.MainLineId);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} save main line id not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }

            //处理任务
            player.SaveMainLineId(pks.MainLineId);
        }

        public void OnResponse_GetTaskFinishReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASK_FINISH_STATE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASK_FINISH_STATE_REWARD>(stream);
            Log.Write("player {0} GetTaskFinishReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetTaskFinishReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetTaskFinishReward not in map ", uid);
                return;
            }

            player.TaskFinishStateReward((TaskFinishType)pks.RewardType, pks.Index);
        }
    }
}
