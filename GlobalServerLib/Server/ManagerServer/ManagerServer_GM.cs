using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using EnumerateUtility;
using Message.Manager.Protocol.MG;
using ServerFrame;
using ServerModels;

namespace GlobalServerLib
{
    public partial class ManagerServer : FrontendServer
    {
        public void OnResponse_ArenaInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_ARENA_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_ARENA_INFO>(stream);
            
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                ArenaInfo result = new ArenaInfo();
                result.uid = msg.Uid;
                result.name = msg.Name;
                result.rank = msg.Rank;
                result.highestRank = msg.HighestRank;
                result.yesterdayRank = msg.YesterdayRank;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_FamilyInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_FAMILY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_FAMILY_INFO>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            { 
                FamilyInfo result = new FamilyInfo();
                result.familyName = msg.FamilyName;
                result.familyLevel = msg.Level;
                result.familyRank = msg.Rank; 
                result.familyContribution = msg.Contribution;
                result.familyMemberList = new List<FamilyMemberInfo>();
                result.familyDungeonList = new List<FamilyDungeonInfo>();
                foreach(var item in msg.MemberList)
                {
                    FamilyMemberInfo member = new FamilyMemberInfo();
                    member.uid = item.Uid;
                    member.name = item.Name;
                    member.famlilyTitle = item.Title;
                    result.familyMemberList.Add(member);
                }
                foreach (var item in msg.DungeonList)
                {
                    FamilyDungeonInfo dungeon = new FamilyDungeonInfo();
                    dungeon.familyDungeonId = item.DungeonId;
                    dungeon.familyCurHp = item.CurHp;
                    dungeon.familyMaxHp = item.MaxHp;
                    result.familyDungeonList.Add(dungeon);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponser_ServerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_SERVER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_SERVER_INFO>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                ServerInfo result = new ServerInfo();
                result.mainId = msg.MainId;
                result.managerOn = msg.ManagerOn;
                result.relationOn = msg.RelationOn;
                result.totalCount = msg.TotalCount;
                result.zoneList = new List<ZoneInfo>();
                foreach (var item in msg.ZoneList)
                {
                    ZoneInfo zoneInfo = new ZoneInfo();
                    zoneInfo.subId = item.SubId;
                    zoneInfo.memory = item.Memory;
                    zoneInfo.frame = item.Frame;
                    zoneInfo.sleep = item.Sleep;
                    zoneInfo.onlineCount = item.OnlineCount;
                    zoneInfo.mapCount = item.MapCount;
                    zoneInfo.dungeonCount = item.DungeonCount;
                    result.zoneList.Add(zoneInfo);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            } 
        }

        public void OnResponse_GiftCode(MemoryStream stream, int uid = 0)
        {
            MSG_MG_GIFT_CODE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_GIFT_CODE>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                GigtCode result = new GigtCode();
                result.received = msg.Received;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_GameCounter(MemoryStream stream, int uid = 0)
        {
            MSG_MG_GAME_COUNTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_GAME_COUNTER>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                GameCounter result = new GameCounter();
                result.uid = msg.Uid;
                result.name = msg.Name;
                result.coinDungeonCount = msg.CoinDungeonCount;
                result.expDungeonCount = msg.ExpDungeonCount;
                result.treasureDungeonCount = msg.TreasureDunegonCount;
                result.teamNormalDungeonCount1 = msg.TeamNormalDungeonCount1;
                result.teamNormalDunegonCount2 = msg.TeamNormalDunegonCount2;
                result.teamNormalDunegonCount3 = msg.TeamNormalDunegonCount3;
                result.teamGuardDungeonCount = msg.TeamGuardDungeonCount;
                result.teamBossDungeonCount = msg.TeamBossDungeonCount;
                result.pveExpTime = msg.PveExpTime;
                result.pvpExp = msg.PvpExp;
                result.pvpExpTime = msg.PvpExpTime;
                result.familyBossCount = msg.FamilyBossCount;
                result.weekDiamondObtainFreq = msg.WeekDiamondObtainFreq;
                result.diamondObtainFreq = msg.DiamondObtainFreq;
                result.challengeNumber = msg.ChallengeNumber;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_ChangeFamilyName(MemoryStream stream, int uid = 0)
        {
            MSG_MG_CHANGE_FAMILY_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CHANGE_FAMILY_NAME>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                ChangeFmailyName result = new ChangeFmailyName();
                switch (msg.Result)
                { 
                    //case (int)ErrorCode.FamilyNotExist:
                    //    result.Result = -1;
                    //    break;
                    //case (int)ErrorCode.FamilyNameExist:
                    //    result.Result = -2;
                    //    break;
                    case (int)ErrorCode.Success:
                        result.result = 1;
                        break;
                    default:
                        result.result = msg.Result;
                        break;
                }
                result.oldName = msg.OldFamilyName;
                result.newName = msg.NewFamilyName;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_CharAllInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_CHAR_ALL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CHAR_ALL_INFO>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                CharAllInfo result = new CharAllInfo();
                result.uid = msg.Uid;
                result.name = msg.Name;
                result.accountName= msg.AccountName;
                result.level = msg.Level;
           
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_ItemTypeList(MemoryStream stream, int uid = 0)
        {
            MSG_MG_ITEM_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_ITEM_TYPE_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                ItemTypeList result = new ItemTypeList();
                result.uid = msg.Uid;
                result.itemType = msg.ItemType;
                result.itemList = new List<ItemSimpleInfo>();
                foreach (var item in msg.ItemList)
                {
                    ItemSimpleInfo info = new ItemSimpleInfo();
                    info.uid = item.ItemUid.ToString();
                    info.num = item.Num;
                    result.itemList.Add(info);
                }
                result.itemName = msg.ItemName;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_PetTypeList(MemoryStream stream, int uid = 0)
        {
            MSG_MG_PET_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_PET_TYPE_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                PetTypeList result = new PetTypeList();
                result.uid = msg.Uid;
                result.petType = msg.PetType;
                result.petList = new List<PetSimpleInfo>();
                foreach (var item in msg.PetList)
                {
                    PetSimpleInfo info = new PetSimpleInfo();
                    info.uid = item.PetUid.ToString();
                    info.num = item.Num;
                    result.petList.Add(info);
                }
                result.petName = msg.PetName;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_PetMountList(MemoryStream stream, int uid = 0)
        {
            MSG_MG_PET_MOUNT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_PET_MOUNT_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                PetMountList result = new PetMountList();
                result.uid = msg.Uid;
                result.petMountTypeList = new List<string>();
                foreach (var item in msg.PetMountNameList)
                {
                    result.petMountTypeList.Add(item);
                }
                result.curPetMount = msg.CurPetMountName;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_DeletePet(MemoryStream stream, int uid = 0)
        {
            MSG_MG_DELETE_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_DELETE_PET>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                DeletePet result = new DeletePet();
                result.uid = msg.Uid;
                result.petUid = msg.PetUid.ToString();
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_DeletePetMount(MemoryStream stream, int uid = 0)
        {
            MSG_MG_DELETE_PET_MOUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_DELETE_PET_MOUNT>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                DeletePetMount result = new DeletePetMount();
                result.uid = msg.Uid;
                result.petMountType = msg.PetMountType;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_EquipList(MemoryStream stream, int uid = 0)
        {
            MSG_MG_EQUIP_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_EQUIP_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                EquipList result = new EquipList();
                result.uid = msg.Uid;
                result.equipList = new List<EquipInfo>();
                foreach (var item in msg.EquipList)
                {
                    EquipInfo info = new EquipInfo();
                    info.itemType = item.ItemType;
                    info.strengthLevel = item.StrengthLevel;
                    info.starLevel = item.StarLevel;
                    info.mosaics = item.Mosaics;
                    info.vipMosaics = item.VipMosaics;
                    info.itemName = item.Name;
                    result.equipList.Add(info);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_PetList(MemoryStream stream, int uid = 0)
        {
            MSG_MG_PET_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_PET_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                PetList result = new PetList();
                result.uid = msg.Uid;
                result.petList = new List<GMPetInfo>();
                foreach (var item in msg.PetList)
                {
                    GMPetInfo info = new GMPetInfo();
                    info.petType = item.PetType;
                    info.exp = item.Exp;
                    info.level = item.Level;
                    info.petStarLevel = item.StarLevel;
                    info.petName = item.Name;
                    result.petList.Add(info);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_PetMountStrength(MemoryStream stream, int uid = 0)
        {
            MSG_MG_PET_MOUNT_STRENGTH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_PET_MOUNT_STRENGTH>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                PetMountStrength result = new PetMountStrength();
                result.uid = msg.Uid;
                result.exp = msg.Exp;
                result.level = msg.Level;
                result.petMountStarLevel = msg.PetMountStarLevel;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_DeleteItem(MemoryStream stream, int uid = 0)
        {
            MSG_MG_DELETE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_DELETE_ITEM>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                DeleteItem result = new DeleteItem();
                result.itemUid = msg.ItemUid.ToString();
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_DeleteChar(MemoryStream stream, int uid = 0)
        {
            MSG_MG_DELETE_CHAR msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_DELETE_CHAR>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                DeleteChar result = new DeleteChar();
                result.uid = msg.Uid;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_CharacterListByAccountName(MemoryStream stream, int uid = 0)
        {
            MSG_MG_CHARACTER_LIST_BY_ACCOUNT_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CHARACTER_LIST_BY_ACCOUNT_NAME>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                CharacterListByAccountName result = new CharacterListByAccountName();
                result.accountName = msg.AccountName;
                result.charInfoList = new List<CharInfo>();
                foreach (var item in msg.CharacterList)
                {
                    CharInfo info = new CharInfo();
                    info.uid = item.Uid;
                    info.name = item.Name;
                    result.charInfoList.Add(info);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_OrderList(MemoryStream stream, int uid = 0)
        {
            MSG_MG_ORDER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_ORDER_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                Orders result = new Orders();
                result.page = msg.Page;
                result.dataCount = msg.DataCount;
                result.result = 1;
                result.orderList = new List<OrderInfo>();
                foreach (var item in msg.OrderList)
                {
                    OrderInfo info = new OrderInfo
                    {
                        orderId = item.OrderId,
                        money = item.Money,
                        productId = item.ProcductId,
                        time = item.Time,
                        state = item.State,
                        getState = item.GetState,
                        productName = item.ProductIdName,
                        rechargeWay = item.RechargeWay,
                        orderInfo = item.OrderInfo
                    };
                    result.orderList.Add(info);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_SpecItem(MemoryStream stream, int uid = 0)
        {
            MSG_MG_SPEC_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_SPEC_ITEM>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                SpecItem result = new SpecItem();
                result.itemUid = msg.ItemUid.ToString();
                result.strengthLevel = msg.StrengthLevel;
                result.starLevel = msg.StarLevel;
                result.mosaics = msg.Mosaics;
                result.vipMosaics = msg.VipMosaics;
                result.itemType = msg.ItemType;
                result.uid = msg.OwnerUid;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_SpecPet(MemoryStream stream, int uid = 0)
        {
            MSG_MG_SPEC_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_SPEC_PET>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                SpecPet result = new SpecPet();
                result.petUid = msg.PetUid.ToString();
                result.exp = msg.Exp;
                result.level = msg.Level;
                result.petStarLevel = msg.StarLevel;
                result.petType = msg.PetType;
                result.uid = msg.OwnerUid;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(result);
                client.WriteString(json);
            }
        }

        public void OnResponse_GetTipOffInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_TIP_OFF_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_TIP_OFF_INFO>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                TipOffList response = new TipOffList();             
                response.curPage = msg.CurPage;
                response.totalCount = msg.TotalCount;
                response.list = new List<TipOff>();
                foreach (var item in msg.List)
                {
                    TipOff info = new TipOff();
                    info.id = item.Id;
                    info.serverId = item.ServerId;
                    info.name = item.Name;
                    info.destUid = item.DestUid;
                    info.destName = item.DestName;
                    info.type = item.Type;                  
                    info.content = item.Content;
                    info.description = item.Description;
                    info.time = item.Time;
                    response.list.Add(info);
                }
                response.result = 1;
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                client.WriteString(json);
            }
        }

        public void OnResponse_IgnoreTipOff(MemoryStream stream, int uid = 0)
        {
            MSG_MG_IGNORE_TIP_OFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_IGNORE_TIP_OFF>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                IgnoreTipOff response = new IgnoreTipOff();
                response.result = 1;
                
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                client.WriteString(json);
            }
        }

        private void OnResponse_GetItemInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_GET_ITEM_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_GET_ITEM_INFO>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            if (client != null)
            {
                ResGetItemInfo response = new ResGetItemInfo()
                {
                    UserId = msg.UserId, RewardType = msg.RewardType, result = 1
                };
                response.ItemList = new List<ResItemInfo>(msg.ItemList.Count);

                foreach (var info in msg.ItemList)
                {
                    var itemInfo = new ResItemInfo()
                    {
                        ItemUid = info.ItemUid.ToString(), 
                        ItemId = info.ItemId, 
                        RewardType = info.ItemType,
                        ItemNum = info.Num,
                        ItemName = info.ItemName,
                        Year = info.Year,
                        MainNature = info.MainNature,
                        SpecNature1 = info.SpecNature1,
                        SpecNature2 = info.SpecNature2,
                        SpecNature3 = info.SpecNature3,
                        SpecNature4 = info.SpecNature4,
                        Level = info.Level
                    };
                    response.ItemList.Add(itemInfo);
                }

                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                client.WriteString(json);

                Logger.Log.WarnLine("gm global got GetItemInfo return to client !");
            }
        }

        private void OnResponse_DelItemNum(MemoryStream stream, int uid = 0)
        {
            MSG_MG_DEL_ITEM_NUM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_DEL_ITEM_NUM>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            if (client != null)
            {
                ResDeleteItemNum response = new ResDeleteItemNum()
                {
                    ItemUid = msg.ItemUid, ItemId = msg.ItemId, ItemNum = msg.Num, result = msg.Result
                };

                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                client.WriteString(json);
                Logger.Log.WarnLine("gm global got DelItemNum return to client !");
            }
        }

        private void OnResponse_DelActivityProgress(MemoryStream stream, int uid = 0)
        {
            MSG_MG_DEL_ACTIVITY_PROGRESS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_DEL_ACTIVITY_PROGRESS>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            if (client != null)
            {
                ResDelActivityProgress response = new ResDelActivityProgress();
                response.result =  msg.Result;
                response.CurNum = msg.CurNum;
                response.ActivityType = msg.ActivityType;

                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                client.WriteString(json);
                Logger.Log.WarnLine("gm global got DelActivityProgress return to client !");
            }
        }
        
    }
}
