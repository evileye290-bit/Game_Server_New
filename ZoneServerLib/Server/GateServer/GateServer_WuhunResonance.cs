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
        //开槽
        private void OnResponse_OpenResonanceGrid(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_OPEN_RESONANCE_GRID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPEN_RESONANCE_GRID>(stream);
            Logger.Log.Write("player {0} request OpenResonanceGrid", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.OpenResonanceGrid();
            }
            else
            {
                Logger.Log.Warn($"OnResponse_OpenResonanceGrid find no player {uid}");
            }
        }

        //共鸣
        private void OnResponse_AddResonance(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ADD_RESONANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ADD_RESONANCE>(stream);
            Logger.Log.Write("player {0} request add hero {1} to resonance", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.AddResonance(msg.HeroId);
            }
            else
            {
                Logger.Log.Warn($"OnResponse_AddResonance find no player {uid}");
            }
        }

        //还原
        private void OnResponse_SubResonance(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SUB_RESONANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SUB_RESONANCE>(stream);
            Logger.Log.Write("player {0} request sub hero {1} from resonance", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SubResonance(msg.HeroId);
            }
            else
            {
                Logger.Log.Warn($"OnResponse_SubResonance find no player {uid}");
            }
        }

        //共鸣升级
        private void OnResponse_ResonanceLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RESONANCE_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RESONANCE_LEVEL_UP>(stream);
            Logger.Log.Write("player {0} request ResonanceLevelUp", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ResonanceLevelUp();
            }
            else
            {
                Logger.Log.Warn($"OnResponse_ResonanceLevelUp find no player {uid}");
            }
        }
        
    }
}
