using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_BrotherInvite(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BROTHERS_INVITE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BROTHERS_INVITE>(stream);
            Log.Write("player {0} request brother invite {1}", uid, msg.FriendUid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.BrotherInvite(msg.FriendUid);
                player.KomoeEventLogFriendFlow(8, "义结金兰申请", msg.FriendUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline. .can not invite friend", uid);
                }
                else
                {
                    Log.WarnLine("player {0} invite friend fail：can not find player.", uid);
                }
            }
        }

        public void OnResponse_BrotherResponse(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BROTHERS_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BROTHERS_RESPONSE>(stream);
            Log.Write("player {0} request brother response :inviterUid {1} agree {2}", uid, msg.InviterUid, msg.Agree.ToString());

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.BrotherInviteResponse(msg.InviterUid,msg.Agree);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not response brother invite ", uid);
                }
                else
                {
                    Log.WarnLine("player {0} response brother invite fail ,can not find player.", uid);
                }
            }
        }


        public void OnResponse_BrotherRemove(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BROTHERS_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BROTHERS_REMOVE>(stream);
            Log.Write("player {0} request remove brother {1}", uid, msg.BrotherUid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.RemoveBrother(msg.BrotherUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not remove brother ", uid);
                }
                else
                {
                    Log.WarnLine("player {0} remove brother fail ,can not find player.", uid);
                }
            }
        }

        


    }
}
