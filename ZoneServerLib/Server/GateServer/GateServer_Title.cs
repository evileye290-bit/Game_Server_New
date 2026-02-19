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
        public void OnResponse_ChangeTitle(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHANGE_TITLE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHANGE_TITLE>(stream);
            int titleId = pks.Title;
            Log.Write("player {0} change title {1}", pks.PCUid, titleId);
            PlayerChar player = Api.PCManager.FindPc(pks.PCUid);
            if (player == null)
            {
                Log.Warn("player {0} ChangeTitle not in gateid {1} pc list", pks.PCUid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} ChangeTitle not in map ", pks.PCUid);
                return;
            }

            player.ChangeTitle(titleId);           
        }

        public void OnResponse_GetTitleConditionCount(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TITLE_CONDITION_COUNT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TITLE_CONDITION_COUNT>(stream);
            int titleId = pks.TitleId;
            Log.Write("player {0} get title {1} condition count", uid, titleId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get title condition count not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} get title condition count not in map ", uid);
                return;
            }

            player.GetTitleConditionCount(titleId);
        }

        public void OnResponse_LookTitle(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_LOOK_TITLE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LOOK_TITLE>(stream);
            int titleId = pks.TitleId;
            Log.Write("player {0} look title {1}", uid, titleId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} look title not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  look title not in map ", uid);
                return;
            }

            player.LookTitle(titleId);
        }
    }
}
