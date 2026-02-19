using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_GetCrossBossInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_GET_CROSS_BOSS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_BOSS_INFO>(stream);
            MSG_GateZ_GET_CROSS_BOSS_INFO request = new MSG_GateZ_GET_CROSS_BOSS_INFO();
            WriteToZone(request);
        }

        public void OnResponse_UpdateCrossBossQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_CROSS_BOSS_QUEUE msg = ProtobufHelper.Deserialize<MSG_CG_UPDATE_CROSS_BOSS_QUEUE>(stream);
            MSG_GateZ_UPDATE_CROSS_BOSS_QUEUE request = new MSG_GateZ_UPDATE_CROSS_BOSS_QUEUE();
            foreach (var item in msg.HeroDefInfos)
            {
                HERO_DEFENSIVE_DATA heroDefDate = new HERO_DEFENSIVE_DATA();
                heroDefDate.HeroId = item.HeroId;
                heroDefDate.QueueNum = item.QueueNum;
                heroDefDate.PositionNum = item.PositionNum;
                request.HeroDefInfos.Add(heroDefDate);
            }
            WriteToZone(request);
        }

        public void OnResponse_GetCrossBossPassReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_BOSS_PASS_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_BOSS_PASS_REWARD>(stream);
            MSG_GateZ_GET_CROSS_BOSS_PASS_REWARD request = new MSG_GateZ_GET_CROSS_BOSS_PASS_REWARD();
            request.DungeonId = msg.DungeonId;
            WriteToZone(request);
        }

        public void OnResponse_EnterCrossBossMap(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENTER_CROSS_BOSS_MAP msg = ProtobufHelper.Deserialize<MSG_CG_ENTER_CROSS_BOSS_MAP>(stream);
            MSG_GateZ_ENTER_CROSS_BOSS_MAP request = new MSG_GateZ_ENTER_CROSS_BOSS_MAP();
            WriteToZone(request);
        }

        public void OnResponse_CrossBossChallenger(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CROSS_BOSS_CHALLENGER msg = ProtobufHelper.Deserialize<MSG_CG_CROSS_BOSS_CHALLENGER>(stream);
            MSG_GateZ_CROSS_BOSS_CHALLENGER request = new MSG_GateZ_CROSS_BOSS_CHALLENGER();
            WriteToZone(request);
        }

        public void OnResponse_ChallengeCrossBoss(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHALLENGE_CROSS_BOSS_MAP msg = ProtobufHelper.Deserialize<MSG_CG_CHALLENGE_CROSS_BOSS_MAP>(stream);
            MSG_GateZ_CHALLENGE_CROSS_BOSS_MAP request = new MSG_GateZ_CHALLENGE_CROSS_BOSS_MAP();
            WriteToZone(request);
        }

        public void OnResponse_GetCrossBossRankReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_BOSS_RANK_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_BOSS_RANK_REWARD>(stream);
            MSG_GateZ_GET_CROSS_BOSS_RANK_REWARD request = new MSG_GateZ_GET_CROSS_BOSS_RANK_REWARD();
            request.DungeonId = msg.DungeonId;
            WriteToZone(request);
        }
    }
}
