using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_HeroGodUnlock(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_GOD_UNLOCK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_GOD_UNLOCK>(stream);
            Log.Write("player {0} request to unlock hero god, hero {1} godType {2}", uid, msg.HeroId, msg.GodType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine($"player {uid} hero {msg.HeroId} unlock failed: no such player");
                return;
            }
            player.HeroGodUnlock(msg.HeroId, msg.GodType);
        }

        public void OnResponse_HeroGodEquip(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_GOD_EQUIP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_GOD_EQUIP>(stream);
            Log.Write("player {0} request to equip hero god, hero {1} godType {2}", uid, msg.HeroId, msg.GodType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine($"player {uid} hero {msg.HeroId} HeroGodEquip failed: no such player");
                return;
            }
            player.HeroGodEquip(msg.HeroId, msg.GodType);
        }
    }
}
