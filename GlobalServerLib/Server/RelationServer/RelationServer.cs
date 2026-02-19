using ServerFrame;

namespace GlobalServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class RelationServer : FrontendServer
    {
        public GlobalServerApi Api
        { get { return (GlobalServerApi)api; } }

        public RelationServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
        }

    }
}