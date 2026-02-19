using System;
using System.IO;
using Message.IdGenerator;
using Logger;
using Message.Barrack.Protocol.BM;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using EnumerateUtility;

namespace ManagerServerLib
{
    public class BarrackServer : BackendServer
    {
        private ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }

        public BarrackServer(BaseApi api) : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_BM_NOTIFY_ADDICTION_INFO>.Value, OnResponse_NotifyAddiction);
            AddResponser(Id<MSG_BM_NOTIFY_SERVER_STATE_INFO>.Value, OnResponse_InGameCount);

            AddResponser(Id<MSG_BM_GET_RUNAWA_TYPE>.Value, OnResponse_GetRunAwayType);
            AddResponser(Id<MSG_BM_GET_SDK_GIFT>.Value, OnResponse_GetSdkGift);
            
        }

        public void OnResponse_NotifyAddiction(MemoryStream stream,int uid=0)
        {
            MSG_BM_NOTIFY_ADDICTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BM_NOTIFY_ADDICTION_INFO>(stream);
            Api.AddictionMng.AddUnderAgeAccountId(msg);
        }

        public void OnResponse_InGameCount(MemoryStream stream, int uid = 0)
        {
            MSG_BM_NOTIFY_SERVER_STATE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BM_NOTIFY_SERVER_STATE_INFO>(stream);

            Api.ZoneServerManager.Broadcast(new MSG_MZ_NOTIFY_SERVER_STATE_INFO { InGameCount = msg.InGameCount, ServerCount = msg.ServerCount });
        }

        private void OnResponse_GetRunAwayType(MemoryStream stream, int uid = 0)
        {
            MSG_BM_GET_RUNAWA_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BM_GET_RUNAWA_TYPE>(stream);

            Client client = Api.ZoneServerManager.GetClient(msg.Uid);
            if (client != null)
            {
                client.Zone?.Write(new MSG_MZ_GET_RUNAWA_TYPE() {RunAwayType = msg.RunAwayType, Uid = msg.Uid, InterveneId = msg.InterveneId , DataBox = msg.DataBox });
            }
        }

        private void OnResponse_GetSdkGift(MemoryStream stream, int uid = 0)
        {
            MSG_BM_GET_SDK_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BM_GET_SDK_GIFT>(stream);

            Client client = Api.ZoneServerManager.GetClient(msg.Uid);
            if (client != null)
            {
                client.Zone?.Write(new MSG_MZ_GET_SDK_GIFT()
                {
                    Uid = msg.Uid,
                    ActionId = msg.ActionId,
                    GiftId = msg.GiftId,
                    Param = msg.Param,
                    SdkActionType = msg.SdkActionType,
                    DataBox = msg.DataBox
                });
            }
        }
    }
}