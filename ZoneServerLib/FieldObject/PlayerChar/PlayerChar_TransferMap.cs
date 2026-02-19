using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public void TransferEnterMap(int mapId)
        {
            MSG_ZGC_TRANSFER_ENTER_MAP msg = new MSG_ZGC_TRANSFER_ENTER_MAP();
            //传送进地图开启限制           
            MapModel model = MapLibrary.GetMap(mapId);
            if (!CheckCanEnterMap(model))
            {
                Log.Warn("player {0} transfer enter map fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                msg.Result = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }

            if (mapId == 0)
            {
                Log.Warn("player {0} transfer enter map {2} fail : map not exists", Uid,  mapId);
                return;
            }

            if (CurrentMapId == mapId)
            {
                Log.Warn("player {0} transfer enter map {2} fail : already in this map", Uid, mapId);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            Vec2 targetPos = TransferMapLibrary.GetBeginPos(mapId);
            if (targetPos == null)
            {
                Log.Warn("player {0} transfer enter map {2} fail : beginPos not exists", Uid, mapId);
                return;
            }
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
            AskForEnterMap(mapId, CurrentMap.Channel, targetPos);
        }

        private bool CheckLevelLimit(MapModel model)
        {
            if (model.CrossLevelLimit > 0)
            {
                if (Level >= model.CrossLevelLimit)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool CheckTaskIdLimit(MapModel model)
        {
            if (model.CrossMainTaskIdLimits.Count > 0)
            {
                foreach (var taskId in model.CrossMainTaskIdLimits)
                {
                    if (MainTaskId >= taskId)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckCanEnterMap(MapModel model)
        {
            if (CheckLevelLimit(model) && CheckTaskIdLimit(model))
            {
                return true;
            }
            return false;
        }

        public void SendAutoPathFindingMsg()
        {
            MSG_ZGC_AUTOPATHFINDING msg = new MSG_ZGC_AUTOPATHFINDING();
            msg.Result = (int)ErrorCode.NotOpen;
            Write(msg);
        }
    }
}
