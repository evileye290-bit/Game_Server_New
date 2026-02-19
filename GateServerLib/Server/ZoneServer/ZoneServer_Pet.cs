using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_CallPet(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CALL_PET>.Value, stream);
            }
        }

        private void OnResponse_RecallPet(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECALL_PET>.Value, stream);
            }
        }

        private void OnResponse_PetInfoList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_LIST>.Value, stream);
            }
        }

        private void OnResponse_PetEggList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_EGG_LIST>.Value, stream);
            }
        }

        private void OnResponse_UpdatePetEgg(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_PET_EGG>.Value, stream);
            }
        }

        private void OnResponse_HatchPetEgg(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HATCH_PET_EGG>.Value, stream);
            }
        }

        private void OnResponse_FinishHatchPetEgg(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FINISH_HATCH_PET_EGG>.Value, stream);
            }
        }

        private void OnResponse_ReleasePet(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RELEASE_PET>.Value, stream);
            }
        }

        private void OnResponse_ShowPetNature(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_PET_NATURE>.Value, stream);
            }
        }

        private void OnResponse_PetsChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PETS_CHANGE>.Value, stream);
            }
        }

        private void OnResponse_PetLevelUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_LEVEL_UP>.Value, stream);
            }
        }

        private void OnResponse_UpdateMainQueuePet(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_MAINQUEUE_PET>.Value, stream);
            }
        }

        private void OnResponse_PetInherit(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_INHERIT>.Value, stream);
            }
        }

        private void OnResponse_PetSkillBaptize(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_SKILL_BAPTIZE>.Value, stream);
            }
        }

        private void OnResponse_PetBreak(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_BREAK>.Value, stream);
            }
        }

        private void OnResponse_OneKeyPetBreak(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ONE_KEY_PET_BREAK>.Value, stream);
            }
        }

        private void OnResponse_PetBlend(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_BLEND>.Value, stream);
            }
        }

        private void OnResponse_PetFeed(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_FEED>.Value, stream);
            }
        }

        private void OnResponse_UpdatePetDungeonQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_PET_DUNGEON_QUEUE>.Value, stream);
            }
        }

        private void OnResponse_PetDungeonQueueList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PET_DUNGEON_QUEUE_LIST>.Value, stream);
            }
        }
    }
}
