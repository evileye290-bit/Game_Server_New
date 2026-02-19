using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_AbsorbSoulRing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ABSORB_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ABSORB_SOULRING>(stream);
            MSG_GateZ_ABSORB_SOULRING requestMsg = new MSG_GateZ_ABSORB_SOULRING();
            requestMsg.HeroId = msg.HeroId;
            requestMsg.SoulRingUid = ExtendClass.GetUInt64(msg.SoulRingUidHigh,msg.SoulRingUidLow);
            requestMsg.Slot = msg.Slot;
            WriteToZone(requestMsg);
        }
        private void OnResponse_HelpAbsorbSoulRing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HELP_ABSORB_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HELP_ABSORB_SOULRING>(stream);
            MSG_GateZ_HELP_ABSORB_SOULRING requestMsg = new MSG_GateZ_HELP_ABSORB_SOULRING();
            requestMsg.HeroId = msg.HeroId;
            requestMsg.PcUids.AddRange(msg.PcUids);
            WriteToZone(requestMsg);
        }

        private void OnResponse_GetAbsorbInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_ABSORBINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_ABSORBINFO>(stream);
            MSG_GateZ_GET_ABSORBINFO requestMsg = new MSG_GateZ_GET_ABSORBINFO();
            requestMsg.HeroId = msg.HeroId;
            WriteToZone(requestMsg);
        }

        private void OnResponse_CancelAbsorb(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CANCEL_ABSORB msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CANCEL_ABSORB>(stream);
            MSG_GateZ_CANCEL_ABSORB requestMsg = new MSG_GateZ_CANCEL_ABSORB();
            requestMsg.HeroId = msg.HeroId;
            WriteToZone(requestMsg);
        }

        private void OnResponse_FinishAbsorb(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ABSORB_FINISH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ABSORB_FINISH>(stream);
            MSG_GateZ_ABSORB_FINISH requestMsg = new MSG_GateZ_ABSORB_FINISH();
            requestMsg.HeroId = msg.HeroId;
            WriteToZone(requestMsg);
        }

        private void OnResponse_GetHelpThanksList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HELP_THANKS_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HELP_THANKS_LIST>(stream);
            MSG_GateZ_GET_HELP_THANKS_LIST requestMsg = new MSG_GateZ_GET_HELP_THANKS_LIST();
            requestMsg.Uids.AddRange(msg.Uids);
            WriteToZone(requestMsg);
        }

        private void OnResponse_ThankFriend(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_THANK_FRIEND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_THANK_FRIEND>(stream);
            MSG_GateZ_THANK_FRIEND requestMsg = new MSG_GateZ_THANK_FRIEND();
            requestMsg.FriendUid = msg.FriendUid;
            requestMsg.ItemUid = ExtendClass.GetUInt64(msg.ItemUidHigh, msg.ItemUidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_EnhanceSoulRing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENHANCE_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ENHANCE_SOULRING>(stream);
            MSG_GateZ_ENHANCE_SOULRING requestMsg = new MSG_GateZ_ENHANCE_SOULRING();
            requestMsg.HeroId = msg.HeroId;
            requestMsg.SoulRingUid = ExtendClass.GetUInt64(msg.SoulRingUidHigh, msg.SoulRingUidLow);
            requestMsg.Type = msg.Type;
            WriteToZone(requestMsg);
        }

        private void OnResponse_OneKeyEnhanceSoulRing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ONEKEY_ENHANCE_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ONEKEY_ENHANCE_SOULRING>(stream);
            MSG_GateZ_ONEKEY_ENHANCE_SOULRING requestMsg = new MSG_GateZ_ONEKEY_ENHANCE_SOULRING();
            requestMsg.HeroId = msg.HeroId;
            requestMsg.SoulRingUid = ExtendClass.GetUInt64(msg.SoulRingUidHigh, msg.SoulRingUidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_GetAllAbsorbInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_GET_All_ABSORBINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_All_ABSORBINFO>(stream);
            MSG_GateZ_GET_All_ABSORBINFO requestMsg = new MSG_GateZ_GET_All_ABSORBINFO();
            WriteToZone(requestMsg);
        }

        private void OnResponse_GetAbsorbFriendInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_FRIEND_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_FRIEND_INFO>(stream);
            MSG_GateZ_GET_FRIEND_INFO requestMsg = new MSG_GateZ_GET_FRIEND_INFO();
            requestMsg.FriendUids.AddRange(msg.FriendUids);
            requestMsg.HeroId = msg.HeroId;
            WriteToZone(requestMsg);
        }

        private void OnResponse_ShowHeroSoulRing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_HERO_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_HERO_SOULRING>(stream);
            MSG_GateZ_SHOW_HERO_SOULRING requestMsg = new MSG_GateZ_SHOW_HERO_SOULRING();          
            WriteToZone(requestMsg);
        }

        private void OnResponse_ReplaceBetterSoulRing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_REPLACE_BETTER_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_REPLACE_BETTER_SOULRING>(stream);
            MSG_GateZ_REPLACE_BETTER_SOULRING request = new MSG_GateZ_REPLACE_BETTER_SOULRING();
            request.HeroId = msg.HeroId;
            foreach (var item in msg.SoulRings)
            {
                request.SoulRings.Add(new GateZ_SOULRING_ITEM() { SoulRingUid = ExtendClass.GetUInt64(item.SoulRingUidHigh, item.SoulRingUidLow), Slot = item.Slot });
            }
            WriteToZone(request);
        }
        
        private void OnResponse_SelectSoulRingElement(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SELECT_SOULRING_ELEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SELECT_SOULRING_ELEMENT>(stream);
            MSG_GateZ_SELECT_SOULRING_ELEMENT request = new MSG_GateZ_SELECT_SOULRING_ELEMENT()
            {
                HeroId = msg.HeroId, 
                ElementId = msg.ElementId,
                Pos = msg.Pos
            };
            WriteToZone(request);
        }
    }
}
