using ServerFrame;

namespace ZoneServerLib
{
    public partial class BattleServer:FrontendServer
    {
        private ZoneServerApi Api
        { get { return (ZoneServerApi)api; } }

        public BattleServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            //ResponserEnd
        }


    }
}
