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
        private void OnResponse_ChooseCamp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHOOSE_CAMP msg= MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHOOSE_CAMP>(stream);
            Log.Write("player {0} choose camp {1}", msg.Uid, msg.Camp);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.ChooseCamp(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("Choose Camp fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("Choose Camp fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        private void OnResponse_Worship(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_WORSHIP msg= MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_WORSHIP>(stream);
            Log.Write("player {0} worship toRank {1}", msg.PcUid, msg.ToRank);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.WorshipRank(msg.ToRank);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("worship fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("worship fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_CampElect(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_VOTE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_VOTE>(stream);
            Log.Write("player {0} Camp elect toPcUid {1} item {2} num {3}", msg.PcUid, msg.ToPcUid, msg.ItemId, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.CampElect(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("Camp elect fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("Camp elect fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_RunInElection(MemoryStream stream,int uid = 0)
        {
            //MSG_GateZ_RUN_IN_ELECTION
            Log.Write("player {0} run in elect", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.RunInElection();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("run in elect fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("run in elect fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_ShowCampPanelInfo(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_SHOW_CAMP_PANEL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CAMP_PANEL_INFO>(stream);
            Log.Write("player {0} show camp panel info", msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.ShowCampPanelInfos();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("show Camp Panel fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("show Camp Panel fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        private void OnResponse_GetCampRankReward(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_GET_CAMP_REWARD msg= MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CAMP_REWARD>(stream);
            Log.Write("player {0} get camp rank reward", msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.GetCampReward();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("get Camp reward fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("get Camp reward fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        private void OnResponse_ShowCampInfos(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_SHOW_CAMP_INFO msg= MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CAMP_INFO>(stream);
            Log.Write("player {0} show camp info: camp {1} page {2}", msg.Uid, msg.CampId, msg.Page);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.ShowCampInfos(msg.CampId,msg.Page);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("show Camp rank fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("show Camp rank fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        private void OnResponse_ShowCampElectionInfos(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_CAMP_ELECTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_CAMP_ELECTION_INFO>(stream);
            Log.Write("player {0} show camp election info: page {1}", msg.Uid, msg.Page);

            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.ShowElectionInfos(msg.Page);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("show Camp rank fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("show Camp rank fail, can not find player {0} .", msg.Uid);
                }
            }
        }
        private void OnResponse_GetStarLevel(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_STARLEVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_STARLEVEL>(stream);           
            Log.Write("player {0} get camp star level", msg.Uid);

            MSG_ZGC_GET_STARLEVEL response = new MSG_ZGC_GET_STARLEVEL();
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                response.DragonLevel = player.DragonLevel;
                response.TigerLevel = player.TigerLevel;
                response.PhoenixLevel = player.PhoenixLevel;
                response.TortoiseLevel = player.TortoiseLevel;

                int titleLevel = CampStarsLibrary.GetCampTitleLevel(player.HisPrestige);
                response.TitleLevel = titleLevel;

                response.Blessing = player.GetCounterValue(CounterType.CampBlessingCount);
                player.Write(response);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("Camp star level up fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("Camp star level up fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        private void OnResponse_CampStarLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_STAR_LEVELUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_STAR_LEVELUP>(stream);
            Log.Write("player {0} camp star {1} level up", msg.Uid, msg.StarId);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.UpdateStarLevel(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("Camp star level up fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("Camp star level up fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        private void OnResponse_CampGather(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CAMP_GATHER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CAMP_GATHER>(stream);
            Log.Write("player {0} camp gather", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CampGather();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("Camp gather fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("Camp gather fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_GatherDialogueComplete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GATHER_DIALOGUE_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GATHER_DIALOGUE_COMPLETE>(stream);
            Log.Write("player {0} gather dialogue complete id {1} refuse {2}", uid, msg.Id, msg.Refuse.ToString());
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GatherDialogueComplete(msg.Id, msg.Refuse);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("Gather dialogue complete fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("Gather dialogue complete fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
