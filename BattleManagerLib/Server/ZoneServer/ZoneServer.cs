using ServerFrame;

namespace BattleManagerServerLib
{
    public partial class ZoneServer : FrontendServer
    {
        private BattleManagerServerApi Api
        { get { return (BattleManagerServerApi)api; } }
        public ZoneServer(BaseApi api)
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