using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_SecretAreaInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SECRET_AREA_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SECRET_AREA_INFO>(stream); 
            Log.Write("player {0} request SecretAreaInfo", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetSecretAreaInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("SecretAreaInfo fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("SecretAreaInfo fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_SecretAreaSweep(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SECRET_AREA_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SECRET_AREA_SWEEP>(stream);
            Log.Write("player {0} request secret area sweep {1}", uid, msg.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SecretAreaSweep(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("SecretAreaSweep fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("SecretAreaSweep fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_SecretAreaRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SECRET_AREA_RANK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SECRET_AREA_RANK_INFO>(stream);
            Log.Write("player {0} request secret area rank info", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                MSG_ZR_SECRET_AREA_RANK_LIST notify = new MSG_ZR_SECRET_AREA_RANK_LIST();
                notify.RankType = msg.RankType;
                notify.Page = msg.Page;
                Api.SendToRelation(notify, player.Uid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("SecretAreaRankInfo fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("SecretAreaRankInfo fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_SecretAreaContinueFight(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_SECRET_AREA_CONT_FIGHT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SECRET_AREA_CONT_FIGHT>(stream);
            //Log.Write("player {0} request secret area continue fight {1}", uid, msg.ContinueFight);
            //PlayerChar player = Api.PCManager.FindPc(uid);
            //if (player != null)
            //{
            //    player.ChangeSecretAreaContinueFightState(msg.ContinueFight);
            //}
            //else
            //{
            //    player = Api.PCManager.FindOfflinePc(uid);
            //    if (player != null)
            //    {
            //        Log.WarnLine("SecretAreaContinueFight fail, player {0} is offline.", uid);
            //    }
            //    else
            //    {
            //        Log.WarnLine("SecretAreaContinueFight fail, can not find player {0} .", uid);
            //    }
            //}
        }
    }
}
