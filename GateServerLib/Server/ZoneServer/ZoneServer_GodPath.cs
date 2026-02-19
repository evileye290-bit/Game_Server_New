using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_GodHeroInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_HERO_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodHeroInfo words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathBuyPower(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_BUY_POWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathBuyPower words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathSevenFightStart(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_SEVEN_FIGHT_START>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathSevenFightStart words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathSevenFightNextStage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathSevenFightNextStage words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathUseItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_USE_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathUseItem words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathTrainBodyBuyShield(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_TRAIN_BODY_BUY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathTrainBodyBuyShield words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathTrainBody(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_TRAIN_BODY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathTrainBody words not find client", pcUid);
            }
        }

        public void OnResponse_GodPathFinishStageTask(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_FINISH_STAGE_TASK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathFinishStageTask words not find client", pcUid);
            }
        }


        private void OnResponse_GodPathOceanHeartBuyCount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_BUY_OCEAN_HEART>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathOceanHeartBuyCount words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathOceanHeartRepaint(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_REPAINT_OCEAN_HEART>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathOceanHeartRepaint words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathOceanHeartDraw(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_OCEAN_HEART_DRAW>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathOceanHeartDraw words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathTridentBuy(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_BUY_TRIDENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathTridentBuy words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathTridentUse(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_USE_TRIDENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathTridentUse words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathTridentResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_TRIDENT_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathTridentResult words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathTridentPush(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_PUSH_TRIDENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathTridentPush( words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathAcrossOceanLightPuzzle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_LIGHT_PUZZLE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathAcrossOceanLightPuzzle( words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathAcrossOceanSweep(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_ACROSS_OCEAN_SWEEP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathAcrossOceanSweep( words not find client", pcUid);
            }
        }

        private void OnResponse_GodPathAcrossPassNewDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GOD_PATH_ACROSS_OCEAN_NEW_DUNGEON>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathAcrossPassNewDungeon( words not find client", pcUid);
            }
        }
    }
}
