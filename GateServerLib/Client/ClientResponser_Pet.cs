using CommonUtility;
using Google.Protobuf.Collections;
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
        public void OnResponse_CallPet(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CALL_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CALL_PET>(stream);
            MSG_GateZ_CALL_PET request = new MSG_GateZ_CALL_PET();
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(request);
        }

        public void OnResponse_RecallPet(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RECALL_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RECALL_PET>(stream);
            MSG_GateZ_RECALL_PET request = new MSG_GateZ_RECALL_PET();
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(request);
        }

        private void OnResponse_HatchPetEgg(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HATCH_PET_EGG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HATCH_PET_EGG>(stream);
            MSG_GateZ_HATCH_PET_EGG requestMsg = new MSG_GateZ_HATCH_PET_EGG();
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_FinishHatchPetEgg(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FINISH_HATCH_PET_EGG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FINISH_HATCH_PET_EGG>(stream);
            MSG_GateZ_FINISH_HATCH_PET_EGG requestMsg = new MSG_GateZ_FINISH_HATCH_PET_EGG();
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_ReleasePet(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RELEASE_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RELEASE_PET>(stream);
            MSG_GateZ_RELEASE_PET requestMsg = new MSG_GateZ_RELEASE_PET();
            foreach (var uid in msg.Uids)
            {
                requestMsg.Uids.Add(ExtendClass.GetUInt64(uid.UidHigh, uid.UidLow));
            }
            WriteToZone(requestMsg);
        }

        private void OnResponse_ShowPetNature(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_PET_NATURE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_PET_NATURE>(stream);
            MSG_GateZ_SHOW_PET_NATURE requestMsg = new MSG_GateZ_SHOW_PET_NATURE();
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_PetLevelUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PET_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PET_LEVEL_UP>(stream);
            MSG_GateZ_PET_LEVEL_UP requestMsg = new MSG_GateZ_PET_LEVEL_UP();
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            requestMsg.Multi = msg.Multi;
            WriteToZone(requestMsg);
        }

        private void OnResponse_UpdateMainQueuePet(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_MAINQUEUE_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_MAINQUEUE_PET>(stream);
            MSG_GateZ_UPDATE_MAINQUEUE_PET requestMsg = new MSG_GateZ_UPDATE_MAINQUEUE_PET();
            requestMsg.QueueNum = msg.QueueNum;
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            requestMsg.Remove = msg.Remove;
            WriteToZone(requestMsg);
        }

        private void OnResponse_PetInherit(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PET_INHERIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PET_INHERIT>(stream);
            MSG_GateZ_PET_INHERIT requestMsg = new MSG_GateZ_PET_INHERIT();
            requestMsg.FromUid = ExtendClass.GetUInt64(msg.FromUid.UidHigh, msg.FromUid.UidLow);
            requestMsg.ToUid = ExtendClass.GetUInt64(msg.ToUid.UidHigh, msg.ToUid.UidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_PetSkillBaptize(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PET_SKILL_BAPTIZE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PET_SKILL_BAPTIZE>(stream);
            MSG_GateZ_PET_SKILL_BAPTIZE requestMsg = new MSG_GateZ_PET_SKILL_BAPTIZE();          
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            requestMsg.Slot = msg.Slot;
            requestMsg.UseItem = msg.UseItem;
            WriteToZone(requestMsg);
        }

        private void OnResponse_PetBreak(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PET_BREAK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PET_BREAK>(stream);
            MSG_GateZ_PET_BREAK requestMsg = new MSG_GateZ_PET_BREAK();
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            foreach (var uid in msg.ConsumeUids)
            {
                requestMsg.ConsumeUids.Add(ExtendClass.GetUInt64(uid.UidHigh, uid.UidLow));
            }
            WriteToZone(requestMsg);
        }

        private void OnResponse_OneKeyPetBreak(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ONE_KEY_PET_BREAK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ONE_KEY_PET_BREAK>(stream);
            MSG_GateZ_ONE_KEY_PET_BREAK requestMsg = new MSG_GateZ_ONE_KEY_PET_BREAK();
            foreach (var item in msg.List)
            {
                requestMsg.List.Add(GeneratePetBreakMsg(item.UidHigh, item.UidLow, item.ConsumeUids));
            }
            WriteToZone(requestMsg);
        }

        private MSG_GateZ_PET_BREAK GeneratePetBreakMsg(uint uidHigh, uint uidLow, RepeatedField<CG_PET_UID> consumeUids)
        {
            MSG_GateZ_PET_BREAK msg = new MSG_GateZ_PET_BREAK();
            msg.Uid = ExtendClass.GetUInt64(uidHigh, uidLow);
            foreach (var uid in consumeUids)
            {
                msg.ConsumeUids.Add(ExtendClass.GetUInt64(uid.UidHigh, uid.UidLow));
            }
            return msg;
        }

        private void OnResponse_PetBlend(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PET_BLEND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PET_BLEND>(stream);
            MSG_GateZ_PET_BLEND requestMsg = new MSG_GateZ_PET_BLEND();
            requestMsg.MainUid = ExtendClass.GetUInt64(msg.MainUid.UidHigh, msg.MainUid.UidLow);
            requestMsg.BlendUid = ExtendClass.GetUInt64(msg.BlendUid.UidHigh, msg.BlendUid.UidLow);
            WriteToZone(requestMsg);
        }

        private void OnResponse_PetFeed(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PET_FEED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PET_FEED>(stream);
            MSG_GateZ_PET_FEED requestMsg = new MSG_GateZ_PET_FEED();
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            requestMsg.ItemId = msg.ItemId;
            WriteToZone(requestMsg);
        }

        private void OnResponse_UpdatePetDungeonQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_PET_DUNGEON_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_PET_DUNGEON_QUEUE>(stream);
            MSG_GateZ_UPDATE_PET_DUNGEON_QUEUE requestMsg = new MSG_GateZ_UPDATE_PET_DUNGEON_QUEUE();
            requestMsg.QueueType = msg.QueueType;
            requestMsg.QueueNum = msg.QueueNum;
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            requestMsg.Remove = msg.Remove;
            WriteToZone(requestMsg);
        }
    }
}
