using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_HeroGodUnlock(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_GOD_UNLOCK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_GOD_UNLOCK>(stream);
            WriteToZone(new MSG_GateZ_HERO_GOD_UNLOCK() { HeroId = msg.HeroId, GodType = msg.GodType });
        }

        public void OnResponse_HeroGodEquip(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_GOD_EQUIP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_GOD_EQUIP>(stream);
            WriteToZone(new MSG_GateZ_HERO_GOD_EQUIP() { HeroId = msg.HeroId, GodType = msg.GodType });
        }
    }
}
