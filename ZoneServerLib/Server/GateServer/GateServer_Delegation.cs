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
        private void OnResponse_DelegationList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DELEGATION_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DELEGATION_LIST>(stream);
            Log.Write($"player {msg.PcUid} request get delegation list");
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.GetDelegationList();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("get delegation event fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("get delegation event fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_DelegateHeros(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DELEGATE_HEROS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DELEGATE_HEROS>(stream);
            Log.Write($"player {msg.PcUid} request delegate heros, delegationId {msg.DelegationId} nameId {msg.NameId}");
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {              
                player.DelegateHeros(msg.DelegationId, msg.NameId, msg.HeroIds);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("delegate heros fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("delegate heros fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_CompleteDelegation(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_COMPLETE_DELEGATION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_COMPLETE_DELEGATION>(stream);
            Log.Write($"player {msg.PcUid} request complete delegation, delegationId {msg.DelegationId} nameId {msg.NameId}");
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.CompleteDelegation(msg.DelegationId, msg.NameId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("complete delegation fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("complete delegation fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_GetDelegationRewards(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DELEGATION_REWARDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DELEGATION_REWARDS>(stream);
            Log.Write($"player {msg.PcUid} request get delegation rewards, delegationId {msg.DelegationId} nameId {msg.NameId}");
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {              
                player.GetDelegationRewards(msg.DelegationId, msg.NameId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("get delegation rewards fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("get delegation rewards fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_RefreshDelegation(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_REFRESH_DELEGATION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_REFRESH_DELEGATION>(stream);
            Log.Write($"player {msg.PcUid} request refresh delegation, delegationId {msg.DelegationId} nameId {msg.NameId}");
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.RefreshDelegation(msg.DelegationId, msg.NameId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("refresh delegation fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("refresh delegation fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_BuyDelegationCount(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_DELEGATION_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_DELEGATION_COUNT>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                //player.BuyDelegationCount();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("buy delegation count fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("buy delegation count fail, can not find player {0} .", msg.PcUid);
                }
            }
        }
    }
}
