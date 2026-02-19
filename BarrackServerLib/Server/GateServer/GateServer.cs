using Logger;
using Message.Barrack.Protocol.BGate;
using Message.Gate.Protocol.GateB;
using Message.IdGenerator;
using ServerFrame;
using ServerShared;
using System.IO;

namespace BarrackServerLib
{
    public class GateServer : FrontendServer
    {
        private BarrackServerApi Api
        {get {return (BarrackServerApi)api;}}

        public string ClientIp = string.Empty;
        public int ClientPort = 0;
        public int Fps = 0;
        public int SleepTime;
        public int ClientCount;
        public int InGameCount;

        public GateServer(BaseApi api):base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GateB_FPS_INFO>.Value, OnResponse_FpsInfo);
            AddResponser(Id<MSG_GateB_GATE_IP_INFO>.Value, OnResponse_IpInfo);
            //ResponserEnd
        }

        public override void OnResponse_RegistServer(MemoryStream stream, int uid = 0)
        {
            base.OnResponse_RegistServer(stream);
            ((GateServerManager)serverManager).CalcLoginGate(MainId);
        }

        public void OnResponse_FpsInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateB_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateB_FPS_INFO>(stream);
            Log.Write("gate main {0} gate id {1} fps {2} sleep {3} client count {4} in game count {5}", msg.MainId, msg.GateId, msg.Fps, msg.SleepTime, msg.ClientCount, msg.InGameCount);
            Fps = msg.Fps;
            SleepTime = msg.SleepTime;
            ClientCount = msg.ClientCount;
            InGameCount = msg.InGameCount;
            ((GateServerManager)serverManager).CalcLoginGate(MainId);
            MSG_BGate_GatesInfo response = new MSG_BGate_GatesInfo();
            //response.inGameTotalCount = ((GateServerManager)serverManager).InGameTotalCount;
            response.GatesCount = ((GateServerManager)serverManager).ServerList.Count;
            Write(response);
        }

        public void OnResponse_IpInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateB_GATE_IP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateB_GATE_IP_INFO>(stream);
            ClientIp = msg.IP;
            ClientPort = int.Parse(msg.Port);
        }

        //public void NotifyLoginInfo(string account_id, string channel_name, int token)
        //{
        //    MSG_BGate_LOGIN notity = new MSG_BGate_LOGIN();
        //    notity.AccountId = account_id;
        //    notity.ChannelName = channel_name;
        //    notity.Token = token;
        //    Write(notity);
        //}

        public void Shutdown()
        {
            MSG_BGate_Shutdown notify = new MSG_BGate_Shutdown();
            Write(notify);

            state = ServerState.Stopping;
        }
    }
}
