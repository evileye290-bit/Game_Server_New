using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RZ;
using System.IO;
using static DBUtility.QuerySetCrossBossRankReward;

namespace RelationServerLib
{
    public partial class CrossServer
    {
        public void OnResponse_GetHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_HIDDER_WEAPON_VALUE>(stream);
            Log.Write($"player {uid} GetHidderWeaponInfo from main {MainId} ");
    
            MSG_RZ_GET_HIDDER_WEAPON_VALUE msg = new MSG_RZ_GET_HIDDER_WEAPON_VALUE();
            msg.Uid = pks.Uid;
            msg.Value = pks.Value;
            msg.TotalValue = pks.TotalValue;
            msg.FirstValue = pks.FirstValue;

            HFPlayerBaseInfoItem item;
            foreach (var kv in pks.BaseInfo)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = kv.Key;
                item.Value = kv.Value;
                msg.BaseInfo.Add(item);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetHidderWeaponInfo not find client ");
            }
        }

        public void OnResponse_GetSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_SEA_TREASURE_VALUE>(stream);
            Log.Write($"player {uid} GetSeaTreasureInfo from main {MainId} ");

            MSG_RZ_GET_SEA_TREASURE_VALUE msg = new MSG_RZ_GET_SEA_TREASURE_VALUE();
            msg.Uid = pks.Uid;
            msg.Value = pks.Value;
            msg.TotalValue = pks.TotalValue;
            msg.FirstValue = pks.FirstValue;

            HFPlayerBaseInfoItem item;
            foreach (var kv in pks.BaseInfo)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = kv.Key;
                item.Value = kv.Value;
                msg.BaseInfo.Add(item);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetSeaTreasureInfo not find client ");
            }
        }

        public void OnResponse_GetDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_DIVINE_LOVE_VALUE>(stream);
            Log.Write($"player {uid} GetDivineLoveInfo from main {MainId} ");

            MSG_RZ_GET_DIVINE_LOVE_VALUE msg = new MSG_RZ_GET_DIVINE_LOVE_VALUE();
            msg.Uid = pks.Uid;
            msg.Value = pks.Value;
            msg.TotalValue = pks.TotalValue;
            msg.FirstValue = pks.FirstValue;

            HFPlayerBaseInfoItem item;
            foreach (var kv in pks.BaseInfo)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = kv.Key;
                item.Value = kv.Value;
                msg.BaseInfo.Add(item);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetDivineLoveInfo not find client ");
            }
        }

        public void OnResponse_GetStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_STONE_WALL_VALUE>(stream);
            Log.Write($"player {uid} GetStoneWallInfo from main {MainId} ");

            MSG_RZ_GET_STONE_WALL_VALUE msg = new MSG_RZ_GET_STONE_WALL_VALUE();
            msg.Uid = pks.Uid;
            msg.Value = pks.Value;
            msg.TotalValue = pks.TotalValue;
            msg.FirstValue = pks.FirstValue;

            HFPlayerBaseInfoItem item;
            foreach (var kv in pks.BaseInfo)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = kv.Key;
                item.Value = kv.Value;
                msg.BaseInfo.Add(item);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetStoneWallInfo not find client ");
            }
        }

        public void OnResponse_ClearValue(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CLEAR_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CLEAR_VALUE>(stream);
            Log.Write($"cross ClearValue from main {MainId} ");
            switch ((RechargeGiftType)pks.GiftType)
            {
                case RechargeGiftType.HiddenWeapon:
                    Api.GameDBPool.Call(new QueryClearHiddenWeapon());
                    break;
                case RechargeGiftType.SeaTreasure:
                    Api.GameDBPool.Call(new QueryClearSeaTreasureBlessing());
                    break;
                case RechargeGiftType.Garden:
                    Api.GameDBPool.Call(new QueryClearGarden());
                    break;
                case RechargeGiftType.DivineLove:
                    Api.GameDBPool.Call(new QueryClearDivineLove());
                    break;
                case RechargeGiftType.StoneWall:
                    Api.GameDBPool.Call(new QueryClearStoneWall());
                    break;
                case RechargeGiftType.CarnivalBoss:
                    Api.GameDBPool.Call(new QueryClearCarnivalBoss());
                    break;
                case RechargeGiftType.IslandHigh:
                    Api.GameDBPool.Call(new QueryClearIslandHighInfo());
                    break;
                case RechargeGiftType.Roulette:
                    Api.GameDBPool.Call(new QueryClearRouletteInfo());
                    break;
                case RechargeGiftType.Canoe:
                    Api.GameDBPool.Call(new QueryClearCanoe());
                    break;
                default:
                    break;
            }

            MSG_RZ_CLEAR_VALUE msg = new MSG_RZ_CLEAR_VALUE();
            msg.GiftType = pks.GiftType;
            Api.ZoneManager.Broadcast(msg);
        }
        
    }
}
