using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_ItemUse(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_USE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_USE>(stream);
            MSG_GateZ_ITEM_USE requestMsg = new MSG_GateZ_ITEM_USE();
            requestMsg.PcUid = Uid;
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            requestMsg.Num = msg.Num;
            WriteToZone(requestMsg);
        }

        private void OnResponse_ItemUseBatch(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_USE_BATCH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_USE_BATCH>(stream);
            MSG_GateZ_ITEM_USE_BATCH requestMsg = new MSG_GateZ_ITEM_USE_BATCH();
            msg.Items.ForEach(x=>requestMsg.Items.Add(new MSG_ITEM_USER_INFO() {Num = x.Num, Uid = ExtendClass.GetUInt64(x.UidHigh, x.UidLow) }));
            WriteToZone(requestMsg);
        }

        private void OnResponse_ItemSell(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_SELL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_SELL>(stream);
            MSG_GateZ_ITEM_SELL requestMsg = new MSG_GateZ_ITEM_SELL();
            requestMsg.PcUid = Uid;
            requestMsg.MainType = msg.MainType;

            foreach (var item in msg.Items)
            {
                requestMsg.Items.Add(new MSG_GateZ_ITEM_ID_NUM() {Uid = ExtendClass.GetUInt64(item.UidHigh, item.UidLow), Num = item.Num });
            }

            WriteToZone(requestMsg);
        }

        private void OnResponse_ItemBuy(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_BUY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_BUY>(stream);
            MSG_GateZ_ITEM_BUY requestMsg = new MSG_GateZ_ITEM_BUY();
            requestMsg.PcUid = Uid;
            requestMsg.Id = msg.Id;
            requestMsg.Num = msg.Num;
            WriteToZone(requestMsg);
        }

        //private void OnResponse_EquipFaceFrame(MemoryStream stream)
        //{
        //    if (curZone == null) return;
        //    MSG_CG_EQUIP_FACEFRAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIP_FACEFRAME>(stream);
        //    MSG_GateZ_EQUIP_FACEFRAME requestMsg = new MSG_GateZ_EQUIP_FACEFRAME();
        //    requestMsg.PcUid = Uid;
        //    requestMsg.Id = msg.Id;
        //    WriteToZone(requestMsg);
        //}

        //private void OnResponse_EquipFashion(MemoryStream stream)
        //{
        //    if (curZone == null) return;
        //    MSG_CG_EQUIP_FASHION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIP_FASHION>(stream);
        //    MSG_GateZ_EQUIP_FASHION requestMsg = new MSG_GateZ_EQUIP_FASHION();
        //    requestMsg.PcUid = Uid;
        //    requestMsg.Id = msg.Id;
        //    WriteToZone(requestMsg);
        //}

        private void OnResponse_UseFireworks(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_USE_FIREWORKS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_USE_FIREWORKS>(stream);
            MSG_GateZ_USE_FIREWORKS requestMsg = new MSG_GateZ_USE_FIREWORKS();           
            requestMsg.Id = msg.Id;
            WriteToZone(requestMsg);
        }

        //private void OnResponse_ExchangeItem(MemoryStream stream)
        //{
        //    if (Uid == 0 || curZone == null) return;
        //    MSG_CG_EXCHANGEITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EXCHANGEITEM>(stream);
        //    MSG_GateZ_EXCHANGEITEM request = new MSG_GateZ_EXCHANGEITEM();
        //    request.fromId = msg.fromId;
        //    request.toId = msg.toId;
        //    request.fromNum = msg.fromNum;
        //    request.PcUid = Uid;
        //    WriteToZone(request);
        //}

        private void OnResponse_ComposeItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_COMPOSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_COMPOSE>(stream);
            MSG_GateZ_ITEM_COMPOSE request = new MSG_GateZ_ITEM_COMPOSE();
            request.Id = msg.Id;
            request.MainType = msg.MainType;
            request.Num = msg.Num;
            request.PcUid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_ForgeItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_FORGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_FORGE>(stream);
            MSG_GateZ_ITEM_FORGE request = new MSG_GateZ_ITEM_FORGE();
            request.MainType = msg.MainType;
            request.Id = msg.Id;
            request.Num = msg.Num;
            request.PcUid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_ResolveItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_RESOLVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_RESOLVE>(stream);
            MSG_GateZ_ITEM_RESOLVE request = new MSG_GateZ_ITEM_RESOLVE();
            request.MainType = msg.MainType;
            request.PcUid = Uid;
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.Num = msg.Num;
            WriteToZone(request);
        }

        private void OnResponse_BagSpaceInc(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BAGSPACE_INC msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BAGSPACE_INC>(stream);
            MSG_GateZ_BAGSPACEINC request = new MSG_GateZ_BAGSPACEINC();
            request.PcUid = Uid;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        private void OnResponse_SmeltSoulBones(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SMELT_SOULBONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SMELT_SOULBONE>(stream);
            MSG_GateZ_SMELT_SOULBONE request = new MSG_GateZ_SMELT_SOULBONE();
            request.PcUid = Uid;
            foreach (var item in msg.MainBones)
            {
                MSG_GateZ_SOULBONE bone = new MSG_GateZ_SOULBONE();
                ulong high = item.UidHigh;
                ulong low = item.UidLow;
                high = high << 32;
                bone.Uid = high + low;
                request.MainBones.Add(bone);
            }
            foreach (var item in msg.SecBones)
            {
                MSG_GateZ_SOULBONE bone = new MSG_GateZ_SOULBONE();
                ulong high = item.UidHigh;
                ulong low = item.UidLow;
                high = high << 32;
                bone.Uid = high + low;
                request.SecBones.Add(bone);
            }
            WriteToZone(request);
        }

        private void OnResponse_EquipSoulBone(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_EQUIP_SOULBONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIP_SOULBONE>(stream);
            MSG_GateZ_EQUIP_SOULBONE request = new MSG_GateZ_EQUIP_SOULBONE();
            request.PcUid = Uid;
            request.Hero = msg.HeroId;

            ulong high = msg.UidHigh;
            ulong low = msg.UidLow;
            high = high << 32;
            request.Uid = high + low;
            WriteToZone(request);
        }

        private void OnResponse_ItemBatchResolve(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_BATCH_RESOLVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_BATCH_RESOLVE>(stream);
            MSG_GateZ_ITEM_BATCH_RESOLVE request = new MSG_GateZ_ITEM_BATCH_RESOLVE();
            request.MainType = msg.MainType;
            foreach (var item in msg.Items)
            {
                request.Items.Add(new GateZ_ITEM_RESOLVE() {Uid = ExtendClass.GetUInt64(item.UidHigh, item.UidLow), Num = item.Num });
            }

            WriteToZone(request);
        }

        private void OnResponse_ItemBatchResolveNew(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_BATCH_RESOLVE_NEW msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_BATCH_RESOLVE_NEW>(stream);
            MSG_GateZ_ITEM_BATCH_RESOLVE request = new MSG_GateZ_ITEM_BATCH_RESOLVE();
            request.MainType = msg.MainType;
            foreach (var item in msg.Items)
            {
                request.Items.Add(new GateZ_ITEM_RESOLVE() { Uid = item.Uid, Num = item.Num });
            }

            WriteToZone(request);
        }

        private void OnResponse_ReceiveItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RECEIVE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RECEIVE_ITEM>(stream);
            MSG_GateZ_RECEIVE_ITEM response = new MSG_GateZ_RECEIVE_ITEM();
            response.ItemId = msg.ItemId;
            WriteToZone(response);
        }

        private void OnResponse_SoulBoneQuenching(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SOULBONE_QUENCHING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SOULBONE_QUENCHING>(stream);
            MSG_GateZ_SOULBONE_QUENCHING request = new MSG_GateZ_SOULBONE_QUENCHING();
            request.MainBone = ExtendClass.GetUInt64(msg.MainBone.UidHigh, msg.MainBone.UidLow);
            request.SubBone = ExtendClass.GetUInt64(msg.SubBone.UidHigh, msg.SubBone.UidLow);
            request.LockIndex.AddRange(msg.LockIndex);

            WriteToZone(request);
        }

        private void OnResponse_ItemExchangeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ITEM_EXCHANGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ITEM_EXCHANGE_REWARD>(stream);
            MSG_GateZ_ITEM_EXCHANGE_REWARD response = new MSG_GateZ_ITEM_EXCHANGE_REWARD();
            response.Id = msg.Id;
            WriteToZone(response);
        }

        private void OnResponse_OpenChooseBox(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_OPEN_CHOOSE_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPEN_CHOOSE_BOX>(stream);
            MSG_GateZ_OPEN_CHOOSE_BOX response = new MSG_GateZ_OPEN_CHOOSE_BOX();
            foreach (var item in msg.Items)
            {
                GateZ_CHOOSE_BOX_ITEM info = new GateZ_CHOOSE_BOX_ITEM();
                foreach (var index in item.Value.List)
                {
                    info.List[index] = index;
                }
                response.Items[item.Key] = info;
            }
            response.ItemUid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(response);
        }
    }
}
