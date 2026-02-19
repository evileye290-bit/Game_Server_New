using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
using System.IO;

namespace GateServerLib
{
    partial class Client
    {
        private void OnResponse_CastSkill(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CAST_SKILL msg = ProtobufHelper.Deserialize<MSG_CG_CAST_SKILL>(stream);
            MSG_GateZ_CAST_SKILL request = new MSG_GateZ_CAST_SKILL();
            request.Uid = Uid;
            request.TargetId = msg.TargetId;
            request.SkillId = msg.SkillId;
            request.AngleX = msg.AngleX;
            request.AngleY = msg.AngleY;
            request.TargetPosX = msg.TargetPosX;
            request.TargetPosY = msg.TargetPosY;
            WriteToZone(request);
        }

        private void OnResponse_CastHeroSkill(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CAST_HERO_SKILL msg = ProtobufHelper.Deserialize<MSG_CG_CAST_HERO_SKILL>(stream);
            MSG_GateZ_CAST_HERO_SKILL request = new MSG_GateZ_CAST_HERO_SKILL();
            request.Uid = Uid;
            request.HeroId = msg.HeroId;
            request.SkillId = msg.SkillId;
			WriteToZone(request);
        }
    }
}
