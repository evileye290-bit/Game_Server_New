using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_GetHeroNature(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_NATURE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_NATURE>(stream);
            Log.Write("player {0} get hero {1} nature info", msg.PcUid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.GetHeroNature(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("get hero nature fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("get hero nature fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_GetHeroPower(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_HERO_POWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HERO_POWER>(stream);
            Log.Write("player {0} get hero {1} power info", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetHeroPower(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get hero power fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get hero power fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
