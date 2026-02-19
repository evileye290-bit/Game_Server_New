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
        public void OnResponse_SaveCrossBattleDefensive(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_CROSS_QUEUE msg = ProtobufHelper.Deserialize<MSG_CG_UPDATE_CROSS_QUEUE>(stream);
            MSG_GateZ_UPDATE_CROSS_QUEUE request = new MSG_GateZ_UPDATE_CROSS_QUEUE();
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

        public void OnResponse_ShowCrossFinalsInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CROSS_BATTLE_FINALS msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_CROSS_BATTLE_FINALS>(stream);
            MSG_GateZ_SHOW_CROSS_BATTLE_FINALS request = new MSG_GateZ_SHOW_CROSS_BATTLE_FINALS();
            request.TeamId = msg.TeamId;
            WriteToZone(request);
        }

        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CROSS_BATTLE_CHALLENGER msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_CROSS_BATTLE_CHALLENGER>(stream);
            MSG_GateZ_SHOW_CROSS_BATTLE_CHALLENGER request = new MSG_GateZ_SHOW_CROSS_BATTLE_CHALLENGER();
            request.Uid = msg.Uid;
            request.MainId = msg.MainId;
            WriteToZone(request);
        }

        public void OnResponse_ShowCrossLeaderInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CROSS_LEADER_INFO msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_CROSS_LEADER_INFO>(stream);
            MSG_GateZ_SHOW_CROSS_LEADER_INFO request = new MSG_GateZ_SHOW_CROSS_LEADER_INFO();
            WriteToZone(request);
        }



        public void OnResponse_GetCrossActiveReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_BATTLE_ACTIVE_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_BATTLE_ACTIVE_REWARD>(stream);
            MSG_GateZ_GET_CROSS_BATTLE_ACTIVE_REWARD request = new MSG_GateZ_GET_CROSS_BATTLE_ACTIVE_REWARD();
            WriteToZone(request);
        }


        public void OnResponse_GetCrossPreliminaryReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_BATTLE_PRELIMINARY_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_BATTLE_PRELIMINARY_REWARD>(stream);
            MSG_GateZ_GET_CROSS_BATTLE_PRELIMINARY_REWARD request = new MSG_GateZ_GET_CROSS_BATTLE_PRELIMINARY_REWARD();
            WriteToZone(request);
        }

        public void OnResponse_EnterCrossMap(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENTER_CROSS_BATTLE_MAP msg = ProtobufHelper.Deserialize<MSG_CG_ENTER_CROSS_BATTLE_MAP>(stream);
            MSG_GateZ_ENTER_CROSS_BATTLE_MAP request = new MSG_GateZ_ENTER_CROSS_BATTLE_MAP();
            WriteToZone(request);
        }

        public void OnResponse_GetCrossBattleVedio(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_VIDEO msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_VIDEO>(stream);
            MSG_GateZ_GET_CROSS_VIDEO request = new MSG_GateZ_GET_CROSS_VIDEO();
            request.TeamId = msg.TeamId;
            request.VedioId = msg.VedioId;
            WriteToZone(request);
        }

        public void OnResponse_GetCrossServerReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_BATTLE_SERVER_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_BATTLE_SERVER_REWARD>(stream);
            MSG_GateZ_GET_CROSS_BATTLE_SERVER_REWARD request = new MSG_GateZ_GET_CROSS_BATTLE_SERVER_REWARD();
            WriteToZone(request);
        }

        public void OnResponse_GetCrossGuessingInfo(MemoryStream stream)
        {
            MSG_CG_GET_GUESSING_INFO msg = ProtobufHelper.Deserialize<MSG_CG_GET_GUESSING_INFO>(stream);
            MSG_GateZ_GET_GUESSING_INFO request = new MSG_GateZ_GET_GUESSING_INFO();
            WriteToZone(request);
        }

        public void OnResponse_CrossGuessingChoose(MemoryStream stream)
        {
            MSG_CG_CROSS_GUESSING_CHOOSE msg = ProtobufHelper.Deserialize<MSG_CG_CROSS_GUESSING_CHOOSE>(stream);
            MSG_GateZ_CROSS_GUESSING_CHOOSE request = new MSG_GateZ_CROSS_GUESSING_CHOOSE();
            request.Choose = msg.Choose;
            WriteToZone(request);
        }
    }
}
