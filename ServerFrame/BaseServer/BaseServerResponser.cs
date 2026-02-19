using System.IO;

namespace ServerFrame
{
    // 用于解决同类型集群互联情况下，通过装饰模式
    // FrontServer与BackendServer相同协议处理逻辑的抽离
    public class BaseServerResponser : BaseServer
    {
        private BaseServer outterServer = null;
        public BaseServerResponser(BaseApi api, BaseServer outterServer)
            : base(api)
        {
            this.outterServer = outterServer;
        }

        // 通用协议处理, 在初始化时自动加入回调列表
        public override void OnResponse_RegistServer(MemoryStream stream, int uid = 0)
        {
            outterServer.OnResponse_RegistServer(stream, uid);
        }

        public override void OnResponse_NotifyServer(MemoryStream stream, int uid = 0)
        {
            outterServer.OnResponse_NotifyServer(stream, uid);
        }

        public override void OnResponse_HeartbeatPing(MemoryStream stream, int uid = 0)
        {
            outterServer.OnResponse_HeartbeatPing(stream, uid);
        }

        public override void OnResponse_HeartbeatPong(MemoryStream stream, int uid = 0)
        {
            outterServer.OnResponse_HeartbeatPong(stream, uid);
        }

        public override void OnResponse_RegistSuccess(MemoryStream stream, int uid = 0)
        {
            outterServer.OnResponse_RegistSuccess(stream, uid);
        }
        
    }
}
