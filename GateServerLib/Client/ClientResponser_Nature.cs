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
        private void OnResponse_GetHeroNature(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_NATURE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_NATURE>(stream);
            MSG_GateZ_HERO_NATURE request = new MSG_GateZ_HERO_NATURE();
            request.HeroId = msg.HeroId;
            request.PcUid = Uid;
          
            WriteToZone(request);
        }

        private void OnResponse_GetHeroPower(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HERO_POWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HERO_POWER>(stream);
            MSG_GateZ_GET_HERO_POWER request = new MSG_GateZ_GET_HERO_POWER();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }
    }
}
