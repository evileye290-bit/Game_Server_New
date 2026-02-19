using EnumerateUtility.Timing;
using Message.Relation.Protocol.RR;
using Message.Relation.Protocol.RZ;
using RedisUtility;

namespace RelationServerLib
{
    public partial class RelationServerApi
    {
        double tempNeedRefreshTime = 0;
        double checkTickTime = 1000;
        private bool CheckNeedRefresh(double dt)
        {
            tempNeedRefreshTime += dt;
            if (tempNeedRefreshTime > checkTickTime)
            {
                tempNeedRefreshTime = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void TimingRefresh(TimingType task)
        {
            switch (task)
            {
                //case TimingType.PopRankClear:
                //    if (WatchDog)
                //    {
                //        Logger.Log.Write("clear pop rank!");
                //        redis.Call(new OperateClearPopRank());
                //        MSG_RZ_POP_RANK_CLEAR notifyClearPopRank = new MSG_RZ_POP_RANK_CLEAR();
                //        ZoneManager.Broadcast(notifyClearPopRank);
                //        MSG_RR_POP_RANK_CLEAR notifyClearPopRankRelation = new MSG_RR_POP_RANK_CLEAR();
                //        BroadcastToAllRelations(notifyClearPopRankRelation);
                //        // TODO 结算奖励
                //    }
                //    break;
                //case TimingType.PopRankRefresh:
                //    Logger.Log.Write("refresh pop rank");
                //    MSG_RZ_POP_RANK_REFRESH notifyRefreshPopRank = new MSG_RZ_POP_RANK_REFRESH();
                //    ZoneManager.Broadcast(notifyRefreshPopRank);
                //    break;
                default:
                    break;
            }
        }
    }
}
