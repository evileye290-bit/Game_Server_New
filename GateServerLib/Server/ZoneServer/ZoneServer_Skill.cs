using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SkillAlarm(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SKILL_ALARM>.Value, stream);
            }
        }
        private void OnResponse_SkillStart(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SKILL_START>.Value, stream);
            }
        }

        private void OnResponse_SkillEff(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SKILL_EFF>.Value, stream);
            }
        }

        private void OnResponse_SkillEnergyList (MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SKILL_ENERGY_LIST>.Value, stream);
            }
        }

        private void OnResponse_SkillEnergy (MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SKILL_ENERGY>.Value, stream);
            }
        }

        private void OnResponse_HeroSkillReady(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_SKILL_READY>.Value, stream);
            }
        }

        private void OnResponse_Damage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DAMAGE>.Value, stream);
            }
        }

        private void OnResponse_CastHeroSkill(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAST_HERO_SKILL>.Value, stream);
            }
        }

        private void OnResponse_HateInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HATE_INFO>.Value, stream);
            }
        }

        private void OnResponse_RealBodyTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REALBODY_TIME>.Value, stream);
            }
        }

        private void OnResponse_AddNormalSkillEnergy(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ADD_NORMAL_SKILL_ENERGY>.Value, stream);
            }
        }

        private void OnResponse_Mark(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MARK>.Value, stream);
            }
        }

        private void OnResponse_MixSkill(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if(client != null)
            {
                client.Write(Id<MSG_ZGC_MIX_SKILL>.Value, stream);
            }
        }

        private void OnResponse_MixSkillEffect(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MIX_SKILL_EFFECT>.Value, stream);
            }
        }

        private void OnResponse_BuffSpecEnd(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUFF_SPEC_END>.Value, stream);
            }
        }

        private void OnResponse_CastSkill(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAST_SKILL>.Value, stream);
            }
        }

        private void OnResponse_BattleStart(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_STARTFIGHTING>.Value, stream);
            }
        }

        private void OnResponse_SkillEnergyChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SKILL_ENERGY_CHANGE>.Value, stream);
            }
        }
    }
}