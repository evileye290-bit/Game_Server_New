using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Logger;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        /// <summary>
        /// 伙伴抽卡
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_DrawHero(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRAW_HERO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRAW_HERO>(stream);
            Log.Write("player {0} request draw hero, drawType {1} isFree {2} isItem {3} isSingle {4}", uid, pks.DrawType, pks.IsFree, pks.IsItem, pks.IsSingle);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} DrawHero not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.DrawHeroCard(pks.DrawType, pks.IsFree, pks.IsItem, pks.IsSingle);
        }

        public void OnResponse_ActivateHeroCombo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ACTIVATE_HERO_COMBO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ACTIVATE_HERO_COMBO>(stream);
            Log.Write("player {0} request activate hero combo {1}", uid, pks.ComboId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} activate hero combo not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (pks.ComboId > 0)
            {
                player.ActivateHeroCombo(pks.ComboId);
            }
            else
            {
                player.OnekeyActivateHeroCombo();
            }
        }

    }
}
