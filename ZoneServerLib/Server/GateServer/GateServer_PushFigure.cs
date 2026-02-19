using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_PushFigureFinishTask(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PUSHFIGURE_FINISHTASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PUSHFIGURE_FINISHTASK>(stream);
            Log.Write("player {0} request PushFigureFinishTask {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} PushFigureFinishTask not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.PushFigureFinishTask(msg.Id);
        }
    }
}
