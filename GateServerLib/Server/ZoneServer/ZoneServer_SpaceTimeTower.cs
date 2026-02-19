using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SpaceTimeTowerInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACE_TIME_TOWER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get space time tower info not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACE_TIME_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time hero join team not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeQuitTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACE_TIME_QUIT_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time hero quit team not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeHeroChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACE_TIME_HERO_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time hero change not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeHeroStepUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_HERO_STEPUP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time hero step up not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeRefreshCardPool(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_REFRESH_CARD_POOL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time refresh card pool not find client", pcUid);
            }
        }

        private void OnResponse_UpdateSpaceTimeHeroQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_SPACETIME_HERO_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update space time hero queue not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeExecuteEvent(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_EXECUTE_EVENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time execute event not find client", pcUid);
            }
        }

        private void OnResponse_NotifySpaceTimeOpenTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_TIME>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} notify space time openTime not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeGetStageAward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_GET_STAGE_AWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} notify space time openTime not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeReset(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_RESET>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time reset not find client", pcUid);
            }
        }

        private void OnResponse_SpaceTimeDungeonSettlement(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_DUNGEON_SETTLEMENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time dungeon settlement not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeShopInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_SHOP_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time shop info not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeGuideSoulRestCounts(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_GUIDESOUL_RESTCOUNTS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time guide soul rest counts not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeBeastSettlement(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_BEAST_SETTLEMENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time beast dungeon settlement not find client", pcUid);
            }
        }
        
        private void OnResponse_SelectGuideSoulItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SELECT_GUIDESOUL_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} select guide soul item not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeEnterNextLevel(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_ENTER_NEXTLEVEL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time enter next level not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeHouseRandomParam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_HOUSE_RANDOM_PARAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time house random param not find client", pcUid);
            }
        }
        
        private void OnResponse_EnterSpaceTimeTower(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ENTER_SPACETIME_TOWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} enter space time tower not find client", pcUid);
            }
        }
        
        private void OnResponse_SpaceTimeGetPastRewards(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPACETIME_GET_PAST_REWARDS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space time get past rewards not find client", pcUid);
            }
        }
    }
}
