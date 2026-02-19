using DataProperty;
using Message.IdGenerator;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Global.Protocol.GR;
using Message.Relation.Protocol.RG;
using Message.Relation.Protocol.RR;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerFrame;
using ServerShared;
using System.IO;
using System.Linq;

namespace RelationServerLib
{
    public class GlobalServer : BaseGlobalServer
    {
        private RelationServerApi Api
        { get { return (RelationServerApi)api; } }

        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GR_SHUTDOWN>.Value, OnResponse_ShutDown);

            AddResponser(Id<MSG_GR_SEND_ITEM>.Value, OnResponse_SendItem);
            AddResponser(Id<MSG_GR_SEND_MAIL>.Value, OnResponse_SendMail);
            AddResponser(Id<MSG_GR_SPEC_EMAIL>.Value, OnResponse_SendSpecEmail);
            AddResponser(Id<MSG_GR_WELFARE_SEND_MAIL>.Value, OnResponse_SendWelfareMail);
            AddResponser(Id<MSG_GR_MERGE_SERVE_REWARD>.Value, OnResponse_SendMergeServerReward);
            //ResponserEnd  
        }


        private void OnResponse_ShutDown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown relation");
            CONST.ALARM_OPEN = false;
            Api.State = ServerState.Stopping;
            Api.StoppingTime = RelationServerApi.now.AddMinutes(1);
        }

        public void OnResponse_SendSpecEmail(MemoryStream stream, int uid = 0)
        {
            MSG_GR_SPEC_EMAIL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GR_SPEC_EMAIL>(stream);
            Log.Write("send specil mail to main id {0} uid {1} title {2} sender {3} text {4} reward {5}",
                msg.ServerId, msg.Uid, msg.Title, msg.SenderName, msg.Content, msg.Reward);
            ZoneServerManager zoneManager = Api.ZoneManager;
            if (msg.ServerId == Api.MainId)
            {
                if (msg.Uid == 0)
                {
                    //server.SendSpecialEmailAll(msg.ServerId, msg.Title, msg.senderName, msg.Content, msg.reward);
                    Log.Error("SendSpecEmail error type all server id {0} title {1}", msg.ServerId, msg.Title);
                }
                else
                {
                    Api.EmailMng.SendCustomEmail(msg.Uid, msg.Title, msg.SenderName, msg.Content, msg.Reward);
                }
            }
        }

        public void OnResponse_SendMail(MemoryStream stream, int uid = 0)
        {
            MSG_GR_SEND_MAIL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GR_SEND_MAIL>(stream);
            Log.Warn("send mail to main id {0} uid {1} mail {2} reward {3}", msg.ServerId, msg.Uid, msg.MailId, msg.Reward);
            if (msg.Uid > 0)
            {
                Api.EmailMng.SendPersonEmail(msg.Uid, msg.MailId, msg.Reward);
            }
            else
            {
                // 群发
                Api.EmailMng.SendSystemEmailAll(msg.MailId);
            }
        }

        public void OnResponse_SendWelfareMail(MemoryStream stream, int uid = 0)
        {
            MSG_GR_WELFARE_SEND_MAIL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GR_WELFARE_SEND_MAIL>(stream);
            Log.Warn("send welfare mail to main id {0} uid {1} mail {2} reward {3}", msg.ServerId, msg.Uid, msg.MailId, msg.Reward);
            if (msg.Uid > 0)
            {
                Api.EmailMng.SendPersonEmail(msg.Uid, msg.MailId, msg.Reward);
                Api.BILoggerMng.WelfareAccountTaLog(msg.Uid, "", MainId);
            }
        }

        public void OnResponse_SendItem(MemoryStream stream, int uid = 0)
        {
            MSG_GR_SEND_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GR_SEND_ITEM>(stream);
            Log.Warn("send item to main id {0} uid {1} item {2}", msg.ServerId, msg.Uid, msg.Item);
            //ZoneManager zoneManager = null;
            //if (msg.ServerId != server.MainId)
            //{
            //    Log.Warn("send item to main id {0} uid {1} item {2} faield: no such server", msg.ServerId, msg.Uid, msg.item);
            //    return;
            //}
            if (msg.Uid <= 0)
            {
                Log.Warn("send item to main id {0} uid {1} item {2} faield: no uid", msg.ServerId, msg.Uid, msg.Item);
                return;
            }

            //string item = msg.Item.Replace('@', '|');
            //Client client = zoneManager.GetClient(msg.Uid);
            //ZServer zServer = null;
            //if (client != null && client.CurZone != null)
            //{
            //    zServer = client.CurZone;
            //}
            //else
            //{
            //    // 不在线 随便找一个Zone
            //    foreach (var item in zoneManager.ZoneList)
            //    {
            //        if (item.Value != null)
            //        {
            //            zServer = item.Value;
            //            break;
            //        }
            //    }
            //}
            //if (zServer == null)
            //{
            //    return;
            //}
            //Api.EmailMng.SendPersonEmail(msg.Uid, 102, item);
        }

        private void OnResponse_SendMergeServerReward(MemoryStream stream, int uid = 0)
        {
            MSG_GR_MERGE_SERVE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GR_MERGE_SERVE_REWARD>(stream);
            Log.Warn("got MergeServerReward request");

            Api.ThemeBossMng.MergeServerReward();
            Api.ContributionMng.MergeServerReward();
            Api.CampActivityMng.MergeServerReward();
            Api.CampRewardMng.MergeServerReward();
        }
    }
}