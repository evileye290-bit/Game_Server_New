using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Logger;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_WelfareTriggerState(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_WELFARE_TRIGGER_STATE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_WELFARE_TRIGGER_STATE>(stream);
            Log.Write("player {0} request Welfare trigger state, id {1} state {2}", uid, pks.Id, pks.State);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} Welfare trigger state not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} Welfare trigger state not in map ", uid);
                return;
            }

            player.WelfareTriggerChangeState(pks.Id, pks.State);
        }
    }
}
