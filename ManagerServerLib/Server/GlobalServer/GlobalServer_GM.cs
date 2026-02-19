using System.Collections;
using DataProperty;
using Logger;
using Message.Global.Protocol.GM;
using Message.Manager.Protocol.MR;
using ServerShared;
using System.IO;
using DBUtility;
using Message.Manager.Protocol.MG;
using ServerFrame;
using System.Linq;
using Message.Manager.Protocol.MZ;
using EnumerateUtility;
using Message.Manager.Protocol.MGate;

namespace ManagerServerLib
{
    public partial class GlobalServer
    {
        private void OnResponse_ArenaInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ARENA_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ARENA_INFO>(stream);
            Log.Write("global request main {0} uid {1} arena info", msg.MainId, msg.Uid);
            MSG_MG_ARENA_INFO response = new MSG_MG_ARENA_INFO();
            response.Uid = msg.Uid;
            response.CustomUid = msg.CustomUid;
            QueryArenaInfo query = new QueryArenaInfo(msg.Uid);
            Api.GameDBPool.Call(query, ret =>
            {
                response.Name = query.Name;
                response.Rank = query.Rank;
                response.HighestRank = query.HighestRank;
                response.YesterdayRank = query.YesterdayRank;
                Write(response);
            });
        }

        private void OnResponse_FamilyInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_FAMILY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_FAMILY_INFO>(stream);
            MSG_MR_GM_FAMILY_INFO request = new MSG_MR_GM_FAMILY_INFO();
            request.CustomUid = msg.CustomUid;
            request.MainId = msg.MainId;
            request.FamilyName = msg.FamilyName;
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(request);
            }
        }

        private void OnResponse_ServerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SERVER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SERVER_INFO>(stream);
            MSG_MG_SERVER_INFO response = new MSG_MG_SERVER_INFO();
            response.CustomUid = msg.CustomUid;
            response.ManagerOn = false;
            response.RelationOn = false;
            response.TotalCount = 0;
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (Api.MainId != msg.MainId)
            {
                Write(response);
                return;
            }
            response.ManagerOn = true;
            if (Api.RelationServer == null)
            {
                response.RelationOn = false;
            }
            else
            {
                response.RelationOn = true;
            }
            int totalCount = 0;
            foreach (var item in zoneManager.ServerList)
            {
                ZoneServer zone = ((ZoneServer)item.Value);
                int dungeonCount = 0;
                MSG_MG_SERVER_INFO.Types.ZONE_INFO zoneInfo = new MSG_MG_SERVER_INFO.Types.ZONE_INFO();
                zoneInfo.SubId = zone.SubId;
                zoneInfo.Memory = (ulong)zone.Memory;
                zoneInfo.Frame = zone.FrameCount;
                zoneInfo.Sleep = zone.SleepTime;
                zoneInfo.OnlineCount = zone.ClientListZone.Count;
                zoneInfo.MapCount = zone.AllMap.Count - dungeonCount;
                zoneInfo.DungeonCount = dungeonCount;
                totalCount += zone.ClientListZone.Count;
                response.ZoneList.Add(zoneInfo);
            }
            response.TotalCount = totalCount;
            Write(response);
        }

        private void OnResponse_GiftCode(MemoryStream stream, int uid = 0)
        {
            MSG_GM_GIFT_CODE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_GIFT_CODE>(stream);
            MSG_MG_GIFT_CODE response = new MSG_MG_GIFT_CODE();
            response.CustomUid = msg.CustomUid;
            response.Received = false;
            string code = msg.Code.Trim();
            string name = code.Substring(0, 4);
            // 取出数据
            Data data = DataListManager.inst.GetData("GiftConfig", name);
            if (data == null)
            {
                Write(response);
                return;
            }
            int isUniversal = data.GetInt("IsUniversal");
            int codeMode = data.GetInt("Mode");
            string subCode = string.Empty;
            switch (codeMode)
            {
                case 1:
                    subCode = data.GetString("Type");
                    break;
                case 2:
                default:
                    subCode = data.GetString("SubType");
                    break;
            }
            QueryGiftCode query = new QueryGiftCode(msg.Uid, codeMode, subCode);
            Api.GameDBPool.Call(query, ret =>
            {
                response.Received = query.Received;
                Write(response);
            });
        }

        private void OnResponse_GameCounter(MemoryStream stream, int uid = 0)
        {
            MSG_GM_GAME_COUNTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_GAME_COUNTER>(stream);
            MSG_MG_GAME_COUNTER response = new MSG_MG_GAME_COUNTER();
            QueryGameCounterInfo query = new QueryGameCounterInfo(msg.Uid);
            Api.GameDBPool.Call(query, ret =>
            {
                //response = query.Msg;
                response.Uid = msg.Uid;
                response.CustomUid = msg.CustomUid;
                Write(response);
            });
        }

        private void OnResponse_ChangeFamilyName(MemoryStream stream, int uid = 0)
        {
            MSG_GM_CHANGE_FAMLIY_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_CHANGE_FAMLIY_NAME>(stream);
            MSG_MR_CHANGE_FAMILY_NAME request = new MSG_MR_CHANGE_FAMILY_NAME();
            request.CustomUid = msg.CustomUid;
            request.MainId = msg.MainId;
            request.OldFamilyName = msg.OldFamilyName;
            request.NewFamliyName = msg.NewFamilyName;
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(request);
            }
        }

        //private void OnResponse_CharAllInfo(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GM_CHAR_ALL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_CHAR_ALL_INFO>(stream);
        //    MSG_MG_CHAR_ALL_INFO response = new MSG_MG_CHAR_ALL_INFO();
        //    response.CustomUid = msg.CustomUid;

        //    Api.GameDBPool.Call(new QueryCharAllInfo(tableName, msg.Uid, msg.Name, response), tableName, DBOperateType.Read, ret =>
        //    {
        //        response.isOnline = false;
        //        ZoneManager zoneManager = server.ZoneMng;
        //        if (server.MainId == msg.MainId)
        //        {
        //            if (zoneManager.GetClient(msg.Uid) != null)
        //            {
        //                response.isOnline = true;
        //            }
        //        }
        //        Write(response);
        //    });
        //}

        private void OnResponse_ItemTypeList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ITEM_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ITEM_TYPE_LIST>(stream);
            MSG_MG_ITEM_TYPE_LIST response = new MSG_MG_ITEM_TYPE_LIST();
            response.CustomUid = msg.CustomUid;
            response.ItemType = msg.ItemType;
            response.Uid = response.Uid;
            Api.GameDBPool.Call(new QueryItemTypeList(msg.Uid, msg.ItemType), ret =>
            {
                DataList dataList = DataListManager.inst.GetDataList("Card");
                if (dataList != null)
                {
                    Data data = dataList.Get(response.ItemType);
                    if (data != null)
                    {
                        response.ItemName = data.GetString("ItemName");
                    }
                }
                Write(response);
            });
        }

        private void OnResponse_PetTypeList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_PET_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_PET_TYPE_LIST>(stream);
            MSG_MG_PET_TYPE_LIST response = new MSG_MG_PET_TYPE_LIST();
            response.CustomUid = msg.CustomUid;
            response.PetType= msg.PetType;
            response.Uid = response.Uid;
            Api.GameDBPool.Call(new QueryPetTypeList(msg.Uid, msg.PetType), ret =>
            {
                DataList dataList = DataListManager.inst.GetDataList("Pet");
                if (dataList != null)
                {
                    Data data = dataList.Get(response.PetType);
                    if (data != null)
                    {
                        response.PetName = data.GetString("PetName");
                    }
                }
                Write(response);
            });

        }

        private void OnResponse_PetMountList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_PET_MOUNT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_PET_MOUNT_LIST>(stream);
            MSG_MG_PET_MOUNT_LIST response = new MSG_MG_PET_MOUNT_LIST();
            response.CustomUid = msg.CustomUid;
            response.Uid = msg.Uid;
            Api.GameDBPool.Call(new QueryPetMountList(msg.Uid), ret =>
            {
                DataList dataList = DataListManager.inst.GetDataList("PetMounts");
                if (dataList != null)
                {
                    foreach (var item in response.PetMountList)
                    {
                        Data data = dataList.Get(item);
                        if (data != null)
                        {
                            response.PetMountNameList.Add(data.GetString("PetName"));
                        }
                    }
                    Data curData = dataList.Get(response.CurPetMount);
                    if (curData != null)
                    {
                        response.CurPetMountName = curData.GetString("PetName");
                    }
                }
                Write(response);
            });
        }

        private void OnResponse_DeletePet(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DELETE_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DELETE_PET>(stream);
            Log.Write("global request delete player {0} pet {1}", msg.Uid, msg.PetUid);
            MSG_MG_DELETE_PET response = new MSG_MG_DELETE_PET();
            response.CustomUid = msg.CustomUid;
            response.Uid = msg.Uid;
            response.PetUid = msg.PetUid;
            Write(response);
            Api.GameDBPool.Call(new QueryGMDeletePet(msg.PetUid));
        }

        private void OnResponse_DeletePetMount(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DELETE_PET_MOUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DELETE_PET_MOUNT>(stream);
            Log.Write("global request delete player {0} petMount {1}", msg.Uid, msg.PetMountType);
            MSG_MG_DELETE_PET_MOUNT response = new MSG_MG_DELETE_PET_MOUNT();
            response.CustomUid = msg.CustomUid;
            response.Uid = msg.Uid;
            response.PetMountType = msg.PetMountType;
            Write(response);
            Api.GameDBPool.Call(new QueryGMDeletePetMount(msg.Uid, msg.PetMountType));
        }

        private void OnResponse_EquipList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_EQUIP_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_EQUIP_LIST>(stream);
            MSG_MG_EQUIP_LIST response = new MSG_MG_EQUIP_LIST();
            response.Uid = msg.Uid;
            response.CustomUid = msg.CustomUid;
            Api.GameDBPool.Call(new QueryEquipList(msg.Uid), ret =>
            {
                DataList dataList = DataListManager.inst.GetDataList("Card");
                if (dataList != null)
                {
                    foreach (var item in response.EquipList)
                    {
                        Data data = dataList.Get(item.ItemType);
                        if (data != null)
                        {
                            item.Name = data.GetString("ItemName");
                        }
                    }
                }
                Write(response);
            });
        }

        private void OnResponse_PetList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_PET_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_PET_LIST>(stream);
            MSG_MG_PET_LIST response = new MSG_MG_PET_LIST();
            response.Uid = msg.Uid;
            response.CustomUid = msg.CustomUid;
            Api.GameDBPool.Call(new QueryPetList(msg.Uid), ret =>
            {
                DataList dataList = DataListManager.inst.GetDataList("Pet");
                if (dataList != null)
                {
                    foreach (var item in response.PetList)
                    {
                        Data data = dataList.Get(item.PetType);
                        if (data != null)
                        {
                            item.Name = data.GetString("PetName");
                        }
                    }
                }
                Write(response);
            });
        }

        private void OnResponse_PetMountStrength(MemoryStream stream, int uid = 0)
        {
            MSG_GM_PET_MOUNT_STRENGTH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_PET_MOUNT_STRENGTH>(stream);
            MSG_MG_PET_MOUNT_STRENGTH response = new MSG_MG_PET_MOUNT_STRENGTH();
            response.Uid = msg.Uid;
            response.CustomUid = msg.CustomUid;
            Api.GameDBPool.Call(new QueryPetMountStrength(msg.Uid), ret =>
            {
                Write(response);
            });
        }

        private void OnResponse_DeleteItem(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DELETE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DELETE_ITEM>(stream);
            Log.Write("global request delete player {0} item {1}", msg.Uid, msg.ItemUid);
            MSG_MG_DELETE_ITEM response = new MSG_MG_DELETE_ITEM();
            response.CustomUid = msg.CustomUid;
            response.ItemUid = msg.ItemUid;
            Write(response);
            Api.GameDBPool.Call(new QueryDeleteItem(msg.Uid, msg.ItemUid));
        }

        private void OnResponse_DeleteChar(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DELETE_CHAR msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DELETE_CHAR>(stream);
            Log.Write("global request delete player {0}", msg.Uid);
            MSG_MG_DELETE_CHAR response = new MSG_MG_DELETE_CHAR();
            response.CustomUid = msg.CustomUid;
            response.Uid = msg.Uid;
            Write(response);
            Api.GameDBPool.Call(new QueryDeleteChar(msg.Uid));
        }



        //private void OnResponse_CharacterListByAccountName(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GM_CHARACTER_LIST_BY_ACCOUNT_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_CHARACTER_LIST_BY_ACCOUNT_NAME>(stream);
        //    MSG_MG_CHARACTER_LIST_BY_ACCOUNT_NAME response = new MSG_MG_CHARACTER_LIST_BY_ACCOUNT_NAME();
        //    response.CustomUid = msg.CustomUid;
        //    response.AccountName = msg.AccountName;
        //    server.DB.Call(new QueryCharacterListByAccountName(response), DBProxyDefault.DefaultTableName, DBProxyDefault.DefaultOperateType, ret =>
        //    {
        //        Write(response);
        //    });
        //}

        private void OnResponse_OrderList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ORDER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ORDER_LIST>(stream);
            MSG_MG_ORDER_LIST response = new MSG_MG_ORDER_LIST
            {
                CustomUid = msg.CustomUid,
                Page = msg.Page
            };

            QueryOrderList query = new QueryOrderList(msg.Uid, msg.OrderId, msg.OrderInfo, msg.Page, msg.PageSize, msg.StartTime, msg.EndTime);

            Api.GameDBPool.Call(query, ret =>
            {
                if ((int)ret == 1)
                {
                    string productName = "";
                    response.DataCount = query.TotalDataCount;
                    foreach (var order in query.orderInfos)
                    {
                        productName = "";
                        Data data = DataListManager.inst.GetData("RechargeGiftItem", order.productId);
                        if (data != null)
                        {
                            productName = data.GetString("beizhu");
                        }
                        
                        response.OrderList.Add(new ORDER_INFO()
                        {
                            OrderId = order.orderId,
                            ProcductId = order.productId,
                            Money = order.money,
                            State = order.state,
                            GetState = order.getState,
                            Time = order.createTime,
                            ProductIdName = productName,
                            RechargeWay = ((RechargeWay)order.way).ToString(),
                            OrderInfo = order.orderInfo
                        });
                    }
                }
                Write(response);
            });
        }

        private void OnResponse_SpecItem(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SPEC_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SPEC_ITEM>(stream);
            MSG_MG_SPEC_ITEM response = new MSG_MG_SPEC_ITEM();
            response.CustomUid = msg.CustomUid;
            response.ItemUid = msg.ItemUid;
            Api.GameDBPool.Call(new QuerySpecItem(), ret =>
            {
                Write(response);
            });
        }

        private void OnResponse_SpecPet(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SPEC_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SPEC_PET>(stream);
            MSG_MG_SPEC_PET response = new MSG_MG_SPEC_PET();
            response.CustomUid = msg.CustomUid;
            response.PetUid = msg.PetUid;
            Api.GameDBPool.Call(new QuerySpecPet(), ret =>
            {
                Write(response);
            });
        }

        private void OnResponse_UpdateItemCount(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UPDATE_ITEM_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UPDATE_ITEM_COUNT>(stream);
            //Api.GameDBPool.Call(new QueryUpdateItemCount(msg.ItemUid, msg.Count));
        }

        //private void OnResponse_SpecEmail(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GM_SPEC_EMAIL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SPEC_EMAIL>(stream);
        //    MSG_MR_SEND_SPEC_EMAIL notify = new MSG_MR_SEND_SPEC_EMAIL();
        //    notify.serverId = msg.ServerId;
        //    notify.Uid = msg.Uid;
        //    notify.title = msg.Title;
        //    notify.content = msg.Content;
        //    notify.reward = msg.reward;
        //    notify.senderName = msg.senderName;
        //    server.Relation.Write(notify);
        //}

        private void OnResponse_UpdateCharData(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UPDATE_CHAR_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UPDATE_CHAR_DATA>(stream);
            Log.Write("gm request update server {0} uid {1} type {2} value {3}", msg.ServerId, msg.Uid, msg.DataType, msg.DataValue);
            Api.GameDBPool.Call(new QueryUpdateCharData(msg.Uid, (GMUpdateCharDataType)msg.DataType, msg.DataValue));
        }

        private void OnResponse_ZoneTransform(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ZONE_TRANSFORM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ZONE_TRANSFORM>(stream);
            Log.WarnLine($"gm request zone transform server {msg.MainId} from zones {string.Join("-", msg.FromZones)} to zones {string.Join("-", msg.ToZones)}");

            ZoneTransformManager.Instance.UpdateZonesInfo(msg.IsForce, msg.FromZones.ToList(), msg.ToZones.ToList());


            //同步发送到各个zone
            MSG_MZ_ZONE_TRANSFORM request = new MSG_MZ_ZONE_TRANSFORM() { MainId = msg.MainId, IsForce = msg.IsForce };
            request.FromZones.AddRange(msg.FromZones);
            request.ToZones.AddRange(msg.ToZones);

            Api.ZoneServerManager.Broadcast(request);

            //同步发送到各个gate
            MSG_MGate_ZONE_TRANSFORM requestGate = new MSG_MGate_ZONE_TRANSFORM() { MainId = msg.MainId, IsForce = msg.IsForce };
            requestGate.FromZones.AddRange(msg.FromZones);
            requestGate.ToZones.AddRange(msg.ToZones);
            Api.ServerManagerProxy.GetFrontendServerManager(ServerType.GateServer)?.Broadcast(requestGate, msg.MainId);
        }

        private void OnResponse_GetItemInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_GET_ITEM_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_GET_ITEM_INFO>(stream);
            Log.WarnLine($"gm request GetItemInfo {msg.ServerId} uid {msg.UserId} item type {msg.RewardType} item id {msg.ItemId}");
            QueryPlayerItemBaseInfo dbQuery = null;
            switch ((RewardType)msg.RewardType)
            {
                case RewardType.Currencies:
                    dbQuery = new QueryPlayerCurrenciesInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.NormalItem:
                    dbQuery = new QueryPlayerNormalItemInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.SoulRing:
                    dbQuery = new QueryPlayerSoulRingInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.SoulBone:
                    dbQuery = new QueryPlayerSoulBoneInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.HiddenWeapon:
                    dbQuery = new QueryPlayerHiddenWeaponInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.Equip:
                    dbQuery = new QueryPlayerEquipInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.ChatFrame:
                    dbQuery = new QueryPlayerChatFrameInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
                case RewardType.HeroFragment:
                    dbQuery = new QueryPlayerHeroFragmentInfo(msg.UserId, msg.RewardType, msg.ItemId);
                    break;
            }

            if (dbQuery == null)
            {
                Log.WarnLine($"gm request GetItemInfo {msg.ServerId} uid {msg.UserId} item type {msg.RewardType} item id {msg.ItemId} got error , illegal reward type ");
                return;
            }

            Api.GameDBPool.Call(dbQuery, obj =>
            {
                MSG_MG_GET_ITEM_INFO info = new MSG_MG_GET_ITEM_INFO()
                {
                    UserId = msg.UserId,
                    RewardType = msg.RewardType,
                    ItemList = { }
                };

                foreach (var x in dbQuery.list)
                {
                    var itemInfo = new MSG_MG_ITEM_INFO()
                    {
                        ItemUid = x.Id, 
                        ItemId = x.ItemId, 
                        Num = x.Num,
                        ItemType = msg.RewardType,
                        ItemName = x.ItemName,
                        Year = x.Year,
                        MainNature = x.MainNature,
                        SpecNature1 = x.SpecNature1,
                        SpecNature2 = x.SpecNature2,
                        SpecNature3 = x.SpecNature3,
                        SpecNature4 = x.SpecNature4,
                        Level = x.Level
                    };
                    info.ItemList.Add(itemInfo);
                }

                Write(info, uid);
            });
        }

        private void OnResponse_ChangeItemNum(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DEL_ITEM_NUM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DEL_ITEM_NUM>(stream);
            Log.WarnLine($"gm request ChangeItemNum uid {msg.UserId} item type {msg.RewardType} item id {msg.ItemId} del num {msg.Num}");

            QueryDeletePlayerItem query = new QueryDeletePlayerItem(msg.ItemUid, msg.UserId, msg.RewardType, msg.ItemId, msg.Num);
            Api.GameDBPool.Call(query, obj =>
            {
                MSG_MG_DEL_ITEM_NUM response = new MSG_MG_DEL_ITEM_NUM()
                {
                    ItemUid = msg.ItemUid, ItemId = msg.ItemId, Num = msg.Num
                };
                if ((int) obj == 0)
                {
                    response.Result = (int) ErrorCode.Fail;
                    Write(response, uid);
                    return;
                }

                response.Result = (int) ErrorCode.Success;
                response.ItemUid = msg.ItemUid;
                Write(response, uid);
            });
        }

        private void OnResponse_DeleteActiveProgress(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DEL_ACTIVITY_PROGRESS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DEL_ACTIVITY_PROGRESS>(stream);
            Log.WarnLine($"gm request ChangeItemNum {msg.ServerId} uid {msg.UserId} activity type {msg.ActivityType} del num {msg.Num}");

            QueryDeleteActivityProgress query = new QueryDeleteActivityProgress(msg.UserId, msg.ActivityType, msg.Num, msg.Price);
            Api.GameDBPool.Call(query, obj =>
            {
                MSG_MG_DEL_ACTIVITY_PROGRESS response = new MSG_MG_DEL_ACTIVITY_PROGRESS()
                {
                    ActivityType = msg.ActivityType,
                    CurNum = query.OldNum,
                };
                if ((int)obj == 0)
                {
                    response.Result = (int)ErrorCode.Fail;
                    Write(response, uid);
                    return;
                }

                AbstractDBQuery updateQuery = null;
                switch ((GMDelActivityType)msg.ActivityType)
                {
                    case GMDelActivityType.FlipCard:
                        updateQuery = new QueryUpdateFlipCardState(msg.UserId, query.OldNum, msg.Num);
                        break;
                    case GMDelActivityType.Trident:
                        updateQuery = new QueryUpdateTridentState(msg.UserId, query.OldNum, msg.Num);
                        break;
                    case GMDelActivityType.SevenDaysBuy:
                        updateQuery = new QueryUpdateSevenDaysBuyState(msg.UserId, query.OldNum, msg.Num);
                        break;
                    case GMDelActivityType.TreasureFlipCard:
                        updateQuery = new QueryUpdateTreasureFlipCardState(msg.UserId, query.OldNum, msg.Num);
                        break;
                }

                if (updateQuery != null)
                {
                    api.GameDBPool.Call(updateQuery);
                }

                response.Result = (int)ErrorCode.Success;
                Write(response, uid);
            });
        }
    }
    
}
