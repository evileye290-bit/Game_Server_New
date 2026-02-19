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
        private void OnResponse_ChooseCampResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHOOSE_CAMP_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} choose camp not find client", pcUid);
            }
        }

        private void OnResponse_SendCampBaseInfo(MemoryStream stream,int pcUid)
        {
            //MSG_ZGC_CAMP_BASE
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_BASE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} send camp base not find client", pcUid);
            }
        }

        //
        private void OnResponse_CampRewardResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CAMP_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get camp reward not find client", pcUid);
            }
        }

        private void OnResponse_WorshipResult(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_WORSHIP_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} worship result not find client", pcUid);
            }
        }

        private void OnResponse_CampVoteResult(MemoryStream stream, int pcUid)
        {
            //MSG_ZGC_CAMP_VOTE_RESULT
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_VOTE_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp vote result not find client", pcUid);
            }
        }

        private void OnResponse_CampRunInElection(MemoryStream stream,int pcUid)
        {
            //MSG_ZGC_RUN_IN_ELECTION_RESULT
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RUN_IN_ELECTION_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} run in camp vote result not find client", pcUid);
            }
        }

        private void OnResponse_ShowCampInfos(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show camp rank infos not find client", pcUid);
            }
        }

        private void OnResponse_ShowCampPanelInfos(MemoryStream stream,int pcUid)
        {
            //MSG_ZGC_CAMP_PANEL_INFO
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_PANEL_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show camp panel infos not find client", pcUid);
            }
        }

        private void OnResponse_ShowElectionInfos(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_ELECTION_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show camp election infos not find client", pcUid);
            }
        }


        private void OnResponse_GetStarLevel(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_STARLEVEL>.Value,stream);
            }
            else
            {
                Log.WarnLine("player {0} get camp star level not find client", pcUid);
            }
        }

        private void OnResponse_CampStarLevelUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_STAR_LEVELUP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp star level up not find client", pcUid);
            }
        }

        private void OnResponse_CampGather(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_GATHER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp gather not find client", pcUid);
            }
        }

        private void OnResponse_GatherDialogueComplete(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GATHER_DIALOGUE_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} gather dialogue complete not find client", pcUid);
            }
        }

        private void OnResponse_CampWorshipShow(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_WORSHIP_SHOW>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_CampWorshipShow not find client", pcUid);
            }
        }

        private void OnResponse_CampWorshipShowUpdate(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_WORSHIP_SHOW_UPDATE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_CampWorshipShow not find client", pcUid);
            }
        }

        
    }
}
