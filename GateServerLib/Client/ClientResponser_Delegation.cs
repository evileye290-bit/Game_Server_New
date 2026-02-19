using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
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
        private void OnResponse_DelegationList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DELEGATION_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DELEGATION_LIST>(stream);
            MSG_GateZ_DELEGATION_LIST request = new MSG_GateZ_DELEGATION_LIST();
            request.PcUid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_DelegateHeros(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DELEGATE_HEROS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DELEGATE_HEROS>(stream);
            MSG_GateZ_DELEGATE_HEROS request = new MSG_GateZ_DELEGATE_HEROS();
            request.PcUid = Uid;          
            request.DelegationId = msg.Id;
            request.NameId = msg.NameId;          
            foreach (var item in msg.HeroIds)
            {
                request.HeroIds.Add(item);
            }                   
            WriteToZone(request);
        }

        private void OnResponse_CompleteDelegation(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_COMPLETE_DELEGATION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_COMPLETE_DELEGATION>(stream);
            MSG_GateZ_COMPLETE_DELEGATION request = new MSG_GateZ_COMPLETE_DELEGATION();
            request.PcUid = Uid;
            request.DelegationId = msg.Id;
            request.NameId = msg.NameId;          
            WriteToZone(request);
        }

        private void OnResponse_GetDelegationRewards(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DELEGATION_REWARDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DELEGATION_REWARDS>(stream);
            MSG_GateZ_DELEGATION_REWARDS request = new MSG_GateZ_DELEGATION_REWARDS();
            request.PcUid = Uid;
            request.DelegationId = msg.Id;
            request.NameId = msg.NameId;
            WriteToZone(request);
        }

        private void OnResponse_RefreshDelegation(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_REFRESH_DELEGATION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_REFRESH_DELEGATION>(stream);
            MSG_GateZ_REFRESH_DELEGATION request = new MSG_GateZ_REFRESH_DELEGATION();
            request.PcUid = Uid;
            request.DelegationId = msg.Id;
            request.NameId = msg.NameId;         
            WriteToZone(request);
        }

        private void OnResponse_BuyDelegationCount(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_DELEGATION_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_DELEGATION_COUNT>(stream);
            MSG_GateZ_BUY_DELEGATION_COUNT request = new MSG_GateZ_BUY_DELEGATION_COUNT();
            request.PcUid = Uid;         
            WriteToZone(request);
        }
    }
}
