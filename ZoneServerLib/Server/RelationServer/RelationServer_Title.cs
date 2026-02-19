using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_LoseArenaFirst(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_LOSE_ARENA_FIRST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_LOSE_ARENA_FIRST>(stream);
            PlayerChar player = Api.PCManager.FindPcAnyway(msg.Uid);
            if (player != null)
            {
                player.TitleMng.UpdateTitleState(TitleObtainCondition.ArenaRankFirst, TitleState.OverTime, 1);
            }
            else
            {
                UpdateOfflinePcArenaFirstTitleState(msg.Uid, TitleObtainCondition.ArenaRankFirst, TitleState.OverTime);
            }
        }

        public void UpdateOfflinePcArenaFirstTitleState(int uid, TitleObtainCondition conditionType, TitleState state)
        {
            List<TitleInfo> titleModelList = TitleLibrary.GetTitleListByCondition(conditionType);
            if (titleModelList == null)
            {
                return;
            }
            foreach (var title in titleModelList)
            {
                if (title.SubType == 1)
                {
                    Api.GameDBPool.Call(new QueryUpdateTitleState(uid, title.Id, (int)state));
                }
            }
        }
    }
}
