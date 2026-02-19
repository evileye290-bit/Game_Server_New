using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {

        private void OnResponse_HeroList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_LIST>.Value, stream);
            }
        }
        private void OnResponse_HeroChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_CHANGE>.Value, stream);
            }
        }
        private void OnResponse_HeroLevelUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_LEVEL_UP>.Value, stream);
            }
        }

        private void OnResponse_HeroAwaken(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_AWAKEN>.Value, stream);
            }
        }


        private void OnResponse_CallHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CALL_PET>.Value, stream);
            }
        }

        private void OnResponse_RecallHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECALL_PET>.Value, stream);
            }
        }

        private void OnResponse_ChangeFollower(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_CHANGE_FOLLOWER>.Value, stream);
            }
        }

        private void OnResponse_ChangeMainHero(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MAIN_HERO_CHANGE>.Value, stream);
            }
            
        }

        private void OnResponse_UpdateHeroPos(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_HERO_POS_RESULT>.Value, stream);
            }
        }

        private void OnResponse_InitHeroPos(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INIT_HERO_POS>.Value, stream);
            }
        }

        private void OnResponse_EquipHeroResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIP_HERO_RESULT>.Value, stream);
            }
        }

        private void OnResponse_HeroStepsUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_STEPS_UP>.Value, stream);
            }
        }

        private void OnResponse_HeroGodStepsUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_GOD_STEPS_UP>.Value, stream);
            }
        }

        private void OnResponse_OnekeyHeroStepsUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ONEKEY_HERO_STEPS_UP>.Value, stream);
            }
        }

        private void OnResponse_HeroTitleUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_TITLE_UP>.Value, stream);
            }
        }

        private void OnResponse_HeroRevert(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_REVERT>.Value, stream);
            }
        }

        private void OnResponse_MainBattleQueueInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MAINQUEUE_INFO>.Value, stream);
            }
        }

        private void OnResponse_UpdateMainQueueHeroPos(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_MAINQUEUE_HEROPOS>.Value, stream);
            }
        }

        private void OnResponse_UnlockMainQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UNLOCK_MAINQUEUE>.Value, stream);
            }
        }

        private void OnResponse_ChangeMainQueueName(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHANGE_MAINQUEUE_NAME>.Value, stream);
            }
        }

        private void OnResponse_MainQueueDispatchBattle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MAINQUEUE_DISPATCH_BATTLE>.Value, stream);
            }
        }

        private void OnResponse_HeroInherit(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_INHERIT>.Value, stream);
            }
        }
    }
}
