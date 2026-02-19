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
        public void OnResponse_SaveDefensive(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SAVE_DEFEMSIVE msg = ProtobufHelper.Deserialize<MSG_CG_SAVE_DEFEMSIVE>(stream);
            MSG_GateZ_SAVE_DEFEMSIVE request = new MSG_GateZ_SAVE_DEFEMSIVE();
            request.HeroIds.AddRange(msg.HeroIds);
            request.HeroPoses.AddRange(msg.HeroPoses);
            WriteToZone(request);
        }


        public void OnResponse_ResetArenaFightTime(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RESET_ARENA_FIGHT_TIME msg = ProtobufHelper.Deserialize<MSG_CG_RESET_ARENA_FIGHT_TIME>(stream);
            MSG_GateZ_RESET_ARENA_FIGHT_TIME request = new MSG_GateZ_RESET_ARENA_FIGHT_TIME();
            WriteToZone(request);
        }


        public void OnResponse_GetRankLevelReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_RANK_LEVEL_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_RANK_LEVEL_REWARD>(stream);
            MSG_GateZ_GET_RANK_LEVEL_REWARD request = new MSG_GateZ_GET_RANK_LEVEL_REWARD();
            request.Level = msg.Level;
            WriteToZone(request);
        }

        public void OnResponse_GetArenaChallenger(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_ARENA_CHALLENGERS msg = ProtobufHelper.Deserialize<MSG_CG_GET_ARENA_CHALLENGERS>(stream);
            MSG_GateZ_GET_ARENA_CHALLENGERS request = new MSG_GateZ_GET_ARENA_CHALLENGERS();
            WriteToZone(request);
        }

        public void OnResponse_ShowArenaRankInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_ARENA_RANK_INFO msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_ARENA_RANK_INFO>(stream);
            MSG_GateZ_SHOW_ARENA_RANK_INFO request = new MSG_GateZ_SHOW_ARENA_RANK_INFO();
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_ShowArenaChallengerInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_ARENA_CHALLENGER msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_ARENA_CHALLENGER>(stream);
            MSG_GateZ_SHOW_ARENA_CHALLENGER request = new MSG_GateZ_SHOW_ARENA_CHALLENGER();
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_EnterArenaMap(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENTER_ARENA_MAP msg = ProtobufHelper.Deserialize<MSG_CG_ENTER_ARENA_MAP>(stream);
            MSG_GateZ_ENTER_ARENA_MAP request = new MSG_GateZ_ENTER_ARENA_MAP();
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_EnterVersusMap(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_VERSUS_PLAYER msg = ProtobufHelper.Deserialize<MSG_CG_VERSUS_PLAYER>(stream);
            MSG_GateZ_VERSUS_PLAYER request = new MSG_GateZ_VERSUS_PLAYER();
            request.ChallengerUid = msg.ChallengerUid;
            WriteToZone(request);
        }
    }
}
