using Message.IdGenerator;
using Logger;
using Message.Manager.Protocol.MM;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using ServerShared;
using ServerShared.Map;
using System.Collections.Generic;
using System.IO;

namespace ManagerServerLib
{
    public class ManagerServerResponser:BaseServerResponser
    {
        public ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }

        public ManagerServerResponser(BaseApi api, BaseServer outterServer)
            : base(api, outterServer)
        { 
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_MM_ONLINE_INFO>.Value, OnResponse_OnlineInfo);
            AddResponser(Id<MSG_MM_RECHARGE_GET_REWARD>.Value, OnResponse_RechargeGetReward);
        }

        private void OnResponse_OnlineInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MM_ONLINE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MM_ONLINE_INFO>(stream);
            Log.Write("manager {0} online info {1}", msg.MainId, msg.OnlineCount);
            List<MapBallenceInfo> mapList = new List<MapBallenceInfo>();
            foreach (var item in msg.MapList)
            {
                MapBallenceInfo info = new MapBallenceInfo();
                info.MapId = item.MapId;
                info.UniformLeft = item.UniformLeft;
                info.MaxLeft = item.MaxLeft;
                info.MaxChannel = item.MaxChannel;
                info.MinChannel = item.MinChannel;
                info.FullChannelList.AddRange(item.FullChannelList);
                mapList.Add(info);
            }
            Api.BallenceProxy.RecordBallenceInfo(msg.MainId, msg.OnlineCount, mapList);
        }

        private void OnResponse_RechargeGetReward(MemoryStream stream, int uid = 0)
        {
            //MSG_MM_RECHARGE_GET_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_MM_RECHARGE_GET_REWARD>(stream);
            //Log.Write("manager recharge get reward  find {0}", pks.Uid);
            ////通知客户端
            //Client client = Api.ZoneServerManager.GetClient(pks.Uid);
            //if (client != null && client.Zone != null && client.Zone.State == ServerState.Started)
            //{
            //    MSG_MZ_RECHARGE_GET_REWARD msg = new MSG_MZ_RECHARGE_GET_REWARD();
            //    msg.Uid = pks.Uid;
            //    msg.RechargeType = pks.RechargeType;
            //    msg.Money = pks.Money;
            //    msg.OrderId = pks.OrderId;
            //    msg.Time = pks.Time;
            //    client.Zone.Write(msg);
            //    return;
            //}
        }
    }
}
