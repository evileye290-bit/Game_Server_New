using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ScriptFunctions;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_Suggest(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SUGGEST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SUGGEST>(stream);
            //Log.Write($"player {msg.Uid} Suggest");

            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                Log.Warn("player {0} request suggest failed: not find player", msg.Uid);
                return;
            }
            int emailId = ScriptManager.Task.CheckCode(msg.Suggest);
            if (emailId == 0)
            {
                MSG_ZGC_SUGGEST response = new MSG_ZGC_SUGGEST();
                response.Result = (int)ErrorCode.Success;

                if (player.CheckCounter(CounterType.Suggest))
                {
                    response.Result = (int)ErrorCode.MaxCount;
                    player.Write(response);
                    Log.Warn("player {0} request suggest failed: suggest already max count", msg.Uid);
                    return;
                }
                // 验证通过 记录吐槽埋点
                player.UpdateCounter(CounterType.Suggest, 1);
                // UID|time.now|msg.Type|msg.Suggest|
                //string log = msg.Uid.ToString() + "|" + Api.now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + msg.Suggest;
                //server.TrackingLoggerMng.Write(log, TrackingLogType.SUGGEST);

                player.RecordGameCommentLog(msg);
                player.Write(response);
            }
            else
            {
                MSG_ZM_DEBUG_RECHARGE pks = new MSG_ZM_DEBUG_RECHARGE();
                pks.RechargeId = emailId;
                pks.Uid = uid;
                Api.ManagerServer.Write(pks);
            }
          
        }
    
    }
}
