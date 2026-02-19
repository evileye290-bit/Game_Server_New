using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateZ;
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
        public void OnResponse_TaskFlyPathFinding(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASKFLY_STARTPATHFINDING pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASKFLY_STARTPATHFINDING>(stream);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} TaskFly pPathFinding not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} TaskFly PathFinding not in map ", pks.PcUid);
                return;
            }

            player.TaskFlyPathFinding();
        }

        public void OnResponse_TaskFlyDone(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TASKFLY_FLY_DONE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TASKFLY_FLY_DONE>(stream);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} TaskFly FlyDone not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} TaskFly FlyDone not in map ", pks.PcUid);
                return;
            }
            player.SetFlyPositionOrChangeMap();

        }
    }
}
