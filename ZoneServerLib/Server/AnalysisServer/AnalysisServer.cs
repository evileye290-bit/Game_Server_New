using Message.Analysis.Protocol.AZ;
using Message.IdGenerator;
using ServerFrame;
using System.IO;

namespace ZoneServerLib
{
    public partial class AnalysisServer : BackendServer
    {
        private ZoneServerApi Api =>(ZoneServerApi)api;

        public AnalysisServer(BaseApi api) : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();

            AddResponser(Id<MSG_AZ_GET_TIMING_GIFT>.Value, OnResponse_GetRecommendGift);

            //ResponserEnd
        }

        private void OnResponse_GetRecommendGift(MemoryStream stream, int uid = 0)
        {
            MSG_AZ_GET_TIMING_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AZ_GET_TIMING_GIFT>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);

            if (player == null)
            {
                Logger.Log.Warn($"OnResponse_GetRecommendGift error gift id {msg.ProductId}, error info not find pc {uid}");
                return;
            }

            player.ActionManager.RecommendTimingGiftResult(msg.ProductId, msg.ActionId, msg.TimingGiftType, msg.Level, msg.DataBox, msg.ResetRecentMaxMoney);
        }
    }
}