using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_HuntingInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_INFO>(stream);
            MSG_GateZ_HUNTING_INFO request = new MSG_GateZ_HUNTING_INFO();
            request.Uid = Uid;
            WriteToZone(request);
        }

        public void OnResponse_HuntingSweep(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_SWEEP>(stream);
            MSG_GateZ_HUNTING_SWEEP request = new MSG_GateZ_HUNTING_SWEEP();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_ContinueHunting(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CONTINUE_HUNTING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CONTINUE_HUNTING>(stream);
            MSG_GateZ_CONTINUE_HUNTING request = new MSG_GateZ_CONTINUE_HUNTING();
            request.Continue = msg.Continue;
            WriteToZone(request);
        }

        public void OnResponse_HuntingActivityUnlock(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_ACTICITY_UNLOCK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_ACTICITY_UNLOCK>(stream);
            MSG_GateZ_HUNTING_ACTICITY_UNLOCK request = new MSG_GateZ_HUNTING_ACTICITY_UNLOCK();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_HuntingActivitySweep(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_ACTICITY_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_ACTICITY_SWEEP>(stream);
            MSG_GateZ_HUNTING_ACTICITY_SWEEP request = new MSG_GateZ_HUNTING_ACTICITY_SWEEP();
            request.Id = msg.Id;
            request.Type = msg.Type;
            WriteToZone(request);
        }

        public void OnResponse_HuntingHelp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_HELP>(stream);
            MSG_GateZ_HUNTING_HELP request = new MSG_GateZ_HUNTING_HELP() { DungeonId = msg.DungeonId};
            WriteToZone(request);
        }

        public void OnResponse_HuntingHelpAnswer(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_HELP_ANSWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_HELP_ANSWER>(stream);
            MSG_GateZ_HUNTING_HELP_ANSWER request = new MSG_GateZ_HUNTING_HELP_ANSWER() { Agree = msg.Agree, AskHelpUid = msg.AskHelpUid };
            WriteToZone(request);
        }

        public void OnResponse_HuntingIntrudeChallenge(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_INTRUDE_CHALLENGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_INTRUDE_CHALLENGE>(stream);
            MSG_GateZ_HUNTING_INTRUDE_CHALLENGE request = new MSG_GateZ_HUNTING_INTRUDE_CHALLENGE() { Id = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow) };
            WriteToZone(request);
        }

        public void OnResponse_HuntingIntrudeUpdateHeroPos(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HUNTING_INTRUDE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HUNTING_INTRUDE_HERO_POS>(stream);
            MSG_GateZ_HUNTING_INTRUDE_HERO_POS request = new MSG_GateZ_HUNTING_INTRUDE_HERO_POS();
            msg.HeroPos.ForEach(x => request.HeroPos.Add(new MSG_GateZ_HERO_POS() { HeroId = x.HeroId, Delete = x.Delete, PosId = x.PosId }));
            WriteToZone(request);
        }

    }
}
