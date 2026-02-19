using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
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
        public void OnResponse_SpaceTimeJoinTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPACE_TIME_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPACE_TIME_JOIN_TEAM>(stream);
            MSG_GateZ_SPACE_TIME_JOIN_TEAM request = new MSG_GateZ_SPACE_TIME_JOIN_TEAM();
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_SpaceTimeQuitTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPACE_TIME_QUIT_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPACE_TIME_QUIT_TEAM>(stream);
            MSG_GateZ_SPACE_TIME_QUIT_TEAM request = new MSG_GateZ_SPACE_TIME_QUIT_TEAM();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }
        
        public void OnResponse_SpaceTimeRefreshCardPool(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPACETIME_REFRESH_CARD_POOL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPACETIME_REFRESH_CARD_POOL>(stream);
            MSG_GateZ_SPACETIME_REFRESH_CARD_POOL request = new MSG_GateZ_SPACETIME_REFRESH_CARD_POOL();
            WriteToZone(request);
        }

        public void OnResponse_SpaceTimeHeroStepUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPACETIME_HERO_STEPUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPACETIME_HERO_STEPUP>(stream);
            MSG_GateZ_SPACETIME_HERO_STEPUP request = new MSG_GateZ_SPACETIME_HERO_STEPUP();
            request.IndexList.AddRange(msg.IndexList);
            WriteToZone(request);
        }

        public void OnResponse_UpdateSpaceTimeHeroQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_SPACETIME_HERO_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_SPACETIME_HERO_QUEUE>(stream);
            MSG_GateZ_UPDATE_SPACETIME_HERO_QUEUE request = new MSG_GateZ_UPDATE_SPACETIME_HERO_QUEUE();
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

        public void OnResponse_SpaceTimeExecuteEvent(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPACETIME_EXECUTE_EVENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPACETIME_EXECUTE_EVENT>(stream);
            MSG_GateZ_SPACETIME_EXECUTE_EVENT request = new MSG_GateZ_SPACETIME_EXECUTE_EVENT();
            request.Type = msg.Type;
            request.Param = msg.Param;
            request.ArrParam.AddRange(msg.ArrParam);
            WriteToZone(request);
        }
        
        public void OnResponse_SpaceTimeGetStageAward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPACETIME_GET_STAGE_AWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPACETIME_GET_STAGE_AWARD>(stream);
            MSG_GateZ_SPACETIME_GET_STAGE_AWARD request = new MSG_GateZ_SPACETIME_GET_STAGE_AWARD();
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_SpaceTimeReset(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_SPACETIME_RESET request = new MSG_GateZ_SPACETIME_RESET();
            WriteToZone(request);
        }
        
        public void OnResponse_SelectGuideSoulItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SELECT_GUIDESOUL_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SELECT_GUIDESOUL_ITEM>(stream);
            MSG_GateZ_SELECT_GUIDESOUL_ITEM request = new MSG_GateZ_SELECT_GUIDESOUL_ITEM();
            request.ItemId = msg.ItemId;
            WriteToZone(request);
        }
        
        public void OnResponse_SpaceTimeEnterNextLevel(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_SPACETIME_ENTER_NEXTLEVEL request = new MSG_GateZ_SPACETIME_ENTER_NEXTLEVEL();
            WriteToZone(request);
        }
        
        public void OnResponse_SpaceTimeBeastSettlement(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_SPACETIME_BEAST_SETTLEMENT request = new MSG_GateZ_SPACETIME_BEAST_SETTLEMENT();
            WriteToZone(request);
        }
        
        public void OnResponse_SpaceTimeHouseRandomParam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_SPACETIME_HOUSE_RANDOM_PARAM request = new MSG_GateZ_SPACETIME_HOUSE_RANDOM_PARAM();
            WriteToZone(request);
        }
        
        public void OnResponse_EnterSpaceTimeTower(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_ENTER_SPACETIME_TOWER request = new MSG_GateZ_ENTER_SPACETIME_TOWER();
            WriteToZone(request);
        }
        
        public void OnResponse_SpaceTimeGetPastRewards(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_SPACETIME_GET_PAST_REWARDS request = new MSG_GateZ_SPACETIME_GET_PAST_REWARDS();
            WriteToZone(request);
        }
    }
}
