using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class ZoneServer
    {

        public void OnResponse_BrotherInvite(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_BROTHERS_INVITE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_BROTHERS_INVITE>(stream);

            Client client = ZoneManager.GetClient(msg.FriendUid);
            if (client != null)
            {
                //client
                MSG_RZ_BROTHERS_INVITE res = new MSG_RZ_BROTHERS_INVITE();
                res.InviterUid = uid;
                client.CurZone.Write(res,msg.FriendUid);
            }
            //else
            //{
            //    Logger.Log.Warn($"try get client {msg.FriendUid} failed in brother invite");
            //}
        }

        public void OnResponse_BrotherResponse(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_BROTHERS_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_BROTHERS_RESPONSE>(stream);
            Client client = ZoneManager.GetClient(msg.InviterUid);
            if (client != null)
            {
                MSG_RZ_BROTHERS_RESPONSE res = new MSG_RZ_BROTHERS_RESPONSE();
                res.BrotherUid = msg.ResponserUid;
                res.Agree = msg.Agree;
                client.CurZone.Write(res,msg.InviterUid);
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.ResponserUid} failed in OnResponse_BrotherResponse");

                CheckSendSwornTitleEmail(msg.InviterUid, msg.Agree);
            }
        }


        public void OnResponse_BrotherRemove(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_BROTHERS_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_BROTHERS_REMOVE>(stream);
            Client client = ZoneManager.GetClient(msg.BrotherUid);
            if (client != null)
            {
                MSG_RZ_BROTHERS_REMOVE res = new MSG_RZ_BROTHERS_REMOVE();
                res.BrotherUid = uid;
                client.CurZone.Write(res,msg.BrotherUid);
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.BrotherUid} failed in OnResponse_BrotherRemove");
            }
        }

        private void CheckSendSwornTitleEmail(int inviteUid, bool agree)
        {
            if (!agree)
            {
                return;
            }

            QueryGetTitle queryTitle = new QueryGetTitle(inviteUid, TitleLibrary.SwornTitleId);
            Api.GameDBPool.Call(queryTitle, (ret =>
            {
                if ((int)ret == 0)
                {
                    TitleInfo titleModel = TitleLibrary.GetTitleById(TitleLibrary.SwornTitleId);
                    if (titleModel != null)
                    {
                        Api.EmailMng.SendPersonEmail(inviteUid, TitleLibrary.EmailId, titleModel.Reward);
                    }
                }
            }));
        }
    }
}
