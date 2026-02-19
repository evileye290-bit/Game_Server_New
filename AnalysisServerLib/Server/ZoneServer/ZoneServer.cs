using Message.Analysis.Protocol.AZ;
using Message.IdGenerator;
using Message.Zone.Protocol.ZA;
using ServerFrame;
using System.IO;

namespace AnalysisServerLib
{
    public partial class ZoneServer : FrontendServer
    {
        public AnalysisServerApi Api => (AnalysisServerApi)api;
        public AnalysisManager AnalysisManager { get; private set; }

        public ZoneServer(BaseApi api) : base(api)
        {
            AnalysisManager = new AnalysisManager(this);

        }

        public override void Update(double dt)
        {
            base.Update(dt);
            AnalysisManager.Update();
        }

        protected override void BindResponser()
        { 
            base.BindResponser();
            AddResponser(Id<MSG_ZA_GET_TIMING_GIFT>.Value, OnResponse_GetRecommendGift);
        }

        private void OnResponse_GetRecommendGift(MemoryStream stream, int uid = 0)
        {
            MSG_ZA_GET_TIMING_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZA_GET_TIMING_GIFT>(stream);

            AnalysisManager.CaculateRecommendGift(msg, uid);
        }

    }
}