using Message.Barrack.Protocol.BG;
using Message.Global.Protocol.GR;
using ServerFrame;
using System.IO;

namespace GlobalServerLib
{
    public partial class BarrackServer : FrontendServer
    {
        private void OnResponse_KTPlayInfo(MemoryStream stream, int uid = 0)
        {
            MSG_BG_KTPLAY_INFOS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_KTPLAY_INFOS>(stream);
            MSG_GR_SEND_MAIL request = new MSG_GR_SEND_MAIL();
            request.ServerId = Api.MainId;
            if (msg.Uid < 0)
            {
                return;
            }
            request.Uid=msg.Uid;
            request.MailId = 1005;
            string reward = "";
            for (int i = 0; i < msg.Rewards.Count;i++ )
            {
                int itemId = msg.Rewards[i].ItemId;
                int num = msg.Rewards[i].Num;
                reward += itemId + ":" + num;
                if (i < (msg.Rewards.Count - 1))
                {
                    reward += "|";
                }
            }
            request.Reward = reward;
            Logger.Log.Write("player {0} get kt rewards {1}", msg.Uid, request.Reward);
            FrontendServer rServer = Api.RelationServerManager.GetWatchDogServer();
            if (rServer != null)
            {
                rServer.Write(request);
            }
        }
    }
}
