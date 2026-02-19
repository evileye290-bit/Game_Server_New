using System.IO;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_SaveCrossChallengeDefensive(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_CROSS_CHALLENGE_QUEUE msg = ProtobufHelper.Deserialize<MSG_CG_UPDATE_CROSS_CHALLENGE_QUEUE>(stream);
            MSG_GateZ_UPDATE_CROSS_CHALLENGE_QUEUE request = new MSG_GateZ_UPDATE_CROSS_CHALLENGE_QUEUE();
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

        public void OnResponse_ShowCrossChallengeFinalsInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CROSS_CHALLENGE_FINALS msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_CROSS_CHALLENGE_FINALS>(stream);
            MSG_GateZ_SHOW_CROSS_CHALLENGE_FINALS request = new MSG_GateZ_SHOW_CROSS_CHALLENGE_FINALS();
            request.TeamId = msg.TeamId;
            WriteToZone(request);
        }

        public void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CROSS_CHALLENGE_CHALLENGER msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_CROSS_CHALLENGE_CHALLENGER>(stream);
            MSG_GateZ_SHOW_CROSS_CHALLENGE_CHALLENGER request = new MSG_GateZ_SHOW_CROSS_CHALLENGE_CHALLENGER();
            request.Uid = msg.Uid;
            request.MainId = msg.MainId;
            WriteToZone(request);
        }

        public void OnResponse_ShowCrossChallengeLeaderInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_CROSS_CHALLENGE_LEADER_INFO msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_CROSS_CHALLENGE_LEADER_INFO>(stream);
            MSG_GateZ_SHOW_CROSS_CHALLENGE_LEADER_INFO request = new MSG_GateZ_SHOW_CROSS_CHALLENGE_LEADER_INFO();
            WriteToZone(request);
        }

        public void OnResponse_GetCrossChallengeActiveReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_CHALLENGE_ACTIVE_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_CHALLENGE_ACTIVE_REWARD>(stream);
            MSG_GateZ_GET_CROSS_CHALLENGE_ACTIVE_REWARD request = new MSG_GateZ_GET_CROSS_CHALLENGE_ACTIVE_REWARD();
            WriteToZone(request);
        }


        public void OnResponse_GetCrossChallengePreliminaryReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD>(stream);
            MSG_GateZ_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD request = new MSG_GateZ_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD();
            WriteToZone(request);
        }

        public void OnResponse_EnterCrossChallengeMap(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENTER_CROSS_CHALLENGE_MAP msg = ProtobufHelper.Deserialize<MSG_CG_ENTER_CROSS_CHALLENGE_MAP>(stream);
            MSG_GateZ_ENTER_CROSS_CHALLENGE_MAP request = new MSG_GateZ_ENTER_CROSS_CHALLENGE_MAP();
            WriteToZone(request);
        }

        public void OnResponse_GetCrossChallengeVedio(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_CHALLENGE_VIDEO msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_CHALLENGE_VIDEO>(stream);
            MSG_GateZ_GET_CROSS_CHALLENGE_VIDEO request = new MSG_GateZ_GET_CROSS_CHALLENGE_VIDEO();
            request.TeamId = msg.TeamId;
            request.VedioId = msg.VedioId;
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_GetCrossChallengeServerReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_CHALLENGE_SERVER_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_CHALLENGE_SERVER_REWARD>(stream);
            MSG_GateZ_GET_CROSS_CHALLENGE_SERVER_REWARD request = new MSG_GateZ_GET_CROSS_CHALLENGE_SERVER_REWARD();
            WriteToZone(request);
        }

        public void OnResponse_GetCrossChallengeGuessingInfo(MemoryStream stream)
        {
            MSG_CG_GET_CROSS_CHALLENGE_GUESSING_INFO msg = ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_CHALLENGE_GUESSING_INFO>(stream);
            MSG_GateZ_GET_CROSS_CHALLENGE_GUESSING_INFO request = new MSG_GateZ_GET_CROSS_CHALLENGE_GUESSING_INFO();
            WriteToZone(request);
        }

        public void OnResponse_CrossChallengeGuessingChoose(MemoryStream stream)
        {
            MSG_CG_CROSS_CHALLENGE_GUESSING_CHOOSE msg = ProtobufHelper.Deserialize<MSG_CG_CROSS_CHALLENGE_GUESSING_CHOOSE>(stream);
            MSG_GateZ_CROSS_CHALLENGE_GUESSING_CHOOSE request = new MSG_GateZ_CROSS_CHALLENGE_GUESSING_CHOOSE();
            request.Choose = msg.Choose;
            WriteToZone(request);
        }

        public void OnResponse_CrossChallengeSwapQueue(MemoryStream stream)
        {
            MSG_CG_CROSS_CHALLENGE_SWAP_QUEUE msg = ProtobufHelper.Deserialize<MSG_CG_CROSS_CHALLENGE_SWAP_QUEUE>(stream);
            MSG_GateZ_CROSS_CHALLENGE_SWAP_QUEUE request = new MSG_GateZ_CROSS_CHALLENGE_SWAP_QUEUE();
            request.Queue1 = msg.Queue1;
            request.Queue2 = msg.Queue2;
            WriteToZone(request);
        }

        public void OnResponse_CrossChallengeSwapHero(MemoryStream stream)
        {
            MSG_CG_CROSS_CHALLENGE_SWAP_HERO msg = ProtobufHelper.Deserialize<MSG_CG_CROSS_CHALLENGE_SWAP_HERO>(stream);
            MSG_GateZ_CROSS_CHALLENGE_SWAP_HERO request = new MSG_GateZ_CROSS_CHALLENGE_SWAP_HERO();
            msg.SwapHero.ForEach(x=>request.SwapHero.Add(new GateZ_CROSS_CHALLENGE_SWAP_HERO()
            {
                HeroId = x.HeroId,
                Queue =  x.Queue
            }));
            WriteToZone(request);
        }
    }
}
