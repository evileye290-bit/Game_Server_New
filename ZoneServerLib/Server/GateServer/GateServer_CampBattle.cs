using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerShared;
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

        private void OnResponse_GetCampBattleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CAMPBATTLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CAMPBATTLE_INFO>(stream);
            Log.Write("player {0} request get camp battle info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetCampBattleInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get camp battle info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get camp battle info fail, can not find player {0} .", uid);
                }
            }
        }


        private void OnResponse_GetFortInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FORT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FORT_INFO>(stream);
            Log.Write("player {0} request get fort info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetFortInfo(msg.FortId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get camp battle fort info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get camp battle fort fail, can not find player {0} .", uid);
                }
            }
        }


        private void OnResponse_GetCampBattleRankList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CAMPBATTLE_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CAMPBATTLE_RANK_LIST>(stream);
            Log.Write($"player {uid} request get camp battle rank list: type {msg.Type} page {msg.Page} camp {msg.Camp}");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetCampBattleRankList(msg.Type,msg.Page,msg.Camp);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get camp battle rank list fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get camp battle rank list, can not find player {0} .", uid);
                }
            }
        }


        private void OnResponse_OpenCampBox(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_OPEN_CAMP_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPEN_CAMP_BOX>(stream);
            Log.Write("player {0} request open camp box", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.OpenCampBox();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("open camp box fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("open camp box fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_CheckInBattleRank(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHECK_IN_BATTLE_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHECK_IN_BATTLE_RANK>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                Log.Warn("OnResponse_CheckInBattleRank  MSG_GateZ_CHECK_IN_BATTLE_RANK 已经废弃");
                //player.CheckInBattleRank();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get camp battle fort info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get camp battle fort fail, can not find player {0} .", uid);
                }
            }
        }



        private void OnResponse_UseNatureItem(MemoryStream stream, int uid = 0)
        {
            MSG_Gate_USE_NATURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_Gate_USE_NATURE_ITEM>(stream);
            Log.Write("player {0} request use nature item: fort {1} item {2}", uid, msg.FortId, msg.ItemId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UseNatureItem(msg.FortId,msg.ItemId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("use nature item fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("use nature item fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_UpdateDefensiveQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_DEFENSIVE_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_DEFENSIVE_QUEUE>(stream);
            Log.Write("player {0} request update defensive queue", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UpdateDefensiveQueue(msg.HeroDefInfos);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("update defensive queue fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("update defensive queue fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_GiveUpFort(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GIVEUP_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GIVEUP_FORT>(stream);
            Log.Write("player {0} request give up fort {1}", uid, msg.FortId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GiveUpFort(msg.FortId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("give up fort info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("give up fort fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_HoldFort(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HOLD_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HOLD_FORT>(stream);
            Log.Write("player {0} request hold fort {1}", uid, msg.FortId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HoldFort(msg.FortId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("hold fort info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("hold fort fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_GetCampBattleAnnouce(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CAMPBATTLE_ANNOUNCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CAMPBATTLE_ANNOUNCE>(stream);
            Log.Write("player {0} get camp battle announce", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SendCampBattleAnnoucementList();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get camp battle announce fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get camp battle announce fail, can not find player {0} .", uid);
                }
            }
        }

    }
}
