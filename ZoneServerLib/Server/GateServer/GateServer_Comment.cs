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
        public void OnResponse_CommentGame(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SAVE_GAME_COMMENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SAVE_GAME_COMMENT>(stream);
            Log.Write("player {0} game comment thumbsUp {1}", pks.PcUid, pks.ThumbsUp);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} CommentGame not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            //request.ThumbsUp = thumbsUp;
            //request.JustClose = close;
            //request.Comment = comment;
            Log.Debug("player {0} game comment thumbsUp {1} ", pks.PcUid, pks.ThumbsUp);

            //todo 埋点
            string log = string.Format("{0}|{1}|{2}", player.Uid, ZoneServerApi.now.ToString("yyyy-MM-dd HH:mm:ss"),pks.ThumbsUp);
            Api.TrackingLoggerMng.Write(log, TrackingLogType.GAMECOMMENT);
        }
    }
}
