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
        private void OnResponse_OpenResonanceGrid(MemoryStream stream)
        {
            MSG_CG_OPEN_RESONANCE_GRID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPEN_RESONANCE_GRID>(stream);
            MSG_GateZ_OPEN_RESONANCE_GRID request = new MSG_GateZ_OPEN_RESONANCE_GRID();
            WriteToZone(request);
        }

        private void OnResponse_AddResonance(MemoryStream stream)
        {
            MSG_CG_ADD_RESONANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ADD_RESONANCE>(stream);
            MSG_GateZ_ADD_RESONANCE request = new MSG_GateZ_ADD_RESONANCE();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        private void OnResponse_SubResonance(MemoryStream stream)
        {
            MSG_CG_SUB_RESONANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SUB_RESONANCE>(stream);
            MSG_GateZ_SUB_RESONANCE request = new MSG_GateZ_SUB_RESONANCE();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }


        private void OnResponse_ResonanceLevelUp(MemoryStream stream)
        {
            MSG_CG_RESONANCE_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RESONANCE_LEVEL_UP>(stream);
            MSG_GateZ_RESONANCE_LEVEL_UP request = new MSG_GateZ_RESONANCE_LEVEL_UP();
            WriteToZone(request);
        }

        
    }
}
