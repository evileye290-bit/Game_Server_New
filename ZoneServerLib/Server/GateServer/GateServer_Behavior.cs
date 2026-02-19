using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_CharacterMove(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHARACTER_MOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHARACTER_MOVE>(stream);
            PlayerChar pc = Api.PCManager.FindPc(msg.Uid);
            if (pc == null || !pc.IsMapLoadingDone || pc.NotStableInMap())
            {
                return;
            }

            if(pc.InDungeon && pc.IsDead)
            {
                return;
            }
            if (!pc.CanMove())
            {
                return;
            }
            //Log.Warn("player {0} OnResponse_CharacterMove X {1} Y {2} map id {3} ", pc.Uid, msg.x, msg.y, pc.CurrentMap.MapId);
            float fX = msg.X;
            float fY = msg.Y;

            //if (pc.CurrentMap.IsHighPrecision) //用高精度格子
            //{
            //    fX = msg.x * 2;
            //    fY = msg.y * 2;
            //}

            if (!pc.CurrentMap.IsWalkableAt((int)Math.Round(fX), (int)Math.Round(fY), true))
            {
                //Log.Warn("player {0} request to move map {1} x {2} y {3} failed: not walkable", pc.Uid, pc.CurrentMap.MapID, pks.x, pks.y);
                return;
            }

            if (pc.CurrentMap.Model.IsAutoBattle)
            {
                //后端自动战斗不允许移动
                return;
            }
            //if(pc.IsMapLoadingDone == false)
            //{
            //    Log.Warn("player {0} move before map loading done", pc.Uid);
            //}

            pc.MoveHandler.NeedFindPath = false;
            //Log.Debug("from {0} {1}", pc.MoveHandler.CurPosition.x, pc.MoveHandler.CurPosition.Y);
            //Log.Debug("to   {0} {1}", msg.x, msg.y);
            //Log.Debug("duration {0}", pc.MoveHandler.GetDuration(new Vec2(msg.x, msg.y), pc.MoveHandler.CurPosition));

            pc.SetDestination(new Vec2(msg.X, msg.Y));
            pc.FsmManager.SetNextFsmStateType(FsmStateType.RUN, true);
        }

        public void OnResponse_MoveZone(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_MOVE_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_MOVE_ZONE>(stream);
            Log.Write("player {0} request enter map {1} ", msg.Uid, msg.MapId);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                Log.Warn("player {0} request enter map {1} failed: player not exist", msg.Uid, msg.MapId);
                return;
            }
            
            if (player != null && player.CurrentMap != null)
            {
                // 默认请求1线
                player.AskForEnterMap(msg.MapId, CONST.MAIN_MAP_CHANNEL, Api.MapManager.GetBeginPosition(msg.MapId));
            }
        }

        //bool tryAutoPathFinding = false;
        //DateTime tempTryAuto = DateTime.Now;
        public void OnResponse_AutoPathFinding(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_AUTOPATHFINDING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_AUTOPATHFINDING>(stream);
            PlayerChar pc = Api.PCManager.FindPc(uid);
            if (pc == null || !pc.IsMapLoadingDone || pc.NotStableInMap())//||ZoneServerApi.now <= pc.LoadingDonePathFindWaiting)
            {
                return;
            }
            Log.Write($"player {uid} autoPathFinding with targetId {msg.TargetId} targetType {msg.TargetType}");
            pc.EndFly();
            pc.EndTreausureFly();
            Log.Debug($"OnResponse_AutoPathFinding with targetId {msg.TargetId} targetType {(FindPathType)msg.TargetType}");
            //if (tryAutoPathFinding)
            //{
            //    if (DateTime.Now > tempTryAuto.AddMinutes(1))
            //    {
            //        tryAutoPathFinding = false;
            //        tempTryAuto = DateTime.Now;
            //    }
            //    return;
            //}
            //tryAutoPathFinding = true;

            switch ((FindPathType)msg.TargetType)
            {
                case FindPathType.TaskNPC:
                    pc.DoTaskFly(msg.TargetId);
                    break;
                case FindPathType.Treasure:
                    pc.AutoPathFinding(pc.ShovelTreasureMng.ZoneTreasureId, msg.TargetType);
                    break;
                //case FindPathType.Treasure:
                //    if (pc.IsTreasurePathFinding())
                //    {
                //        pc.AutoPathFinding(msg.TargetId, msg.TargetType);
                //    }
                //    break;
                //case FindPathType.Fish:
                //    pc.DoFishFly(msg.TargetId);
                //    break;
                case FindPathType.Npc:
                case FindPathType.Monster:
                case FindPathType.Goods:
                default:
                    pc.AutoPathFinding(msg.TargetId, msg.TargetType);
                    break;
            }
        }

        public void OnResponse_CrossPortal(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CROSS_PORTAL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CROSS_PORTAL>(stream);
            PlayerChar pc = Api.PCManager.FindPc(uid);
            if (pc == null || !pc.IsMapLoadingDone || pc.NotStableInMap())
            {
                return;
            }
            Log.Write($"player {uid} CrossPortal PortalId {msg.PortalId}");
            pc.CrossPortal(msg.PortalId);
        }
    }
}
