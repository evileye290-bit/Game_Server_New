using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
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
        public void OnResponse_DrawHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRAW_HERO msg = ProtobufHelper.Deserialize<MSG_CG_DRAW_HERO>(stream);
            MSG_GateZ_DRAW_HERO request = new MSG_GateZ_DRAW_HERO();
            request.IsFree = msg.IsFree;
            request.IsItem = msg.IsItem;
            request.IsSingle = msg.IsSingle;
            request.DrawType = msg.DrawType;
            WriteToZone(request);
        }

        public void OnResponse_ActivateHeroCombo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ACTIVATE_HERO_COMBO msg = ProtobufHelper.Deserialize<MSG_CG_ACTIVATE_HERO_COMBO>(stream);
            MSG_GateZ_ACTIVATE_HERO_COMBO request = new MSG_GateZ_ACTIVATE_HERO_COMBO();
            request.ComboId = msg.ComboId;
            WriteToZone(request);
        }
    }
}
