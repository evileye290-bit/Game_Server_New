using System.Collections.Generic;
using ServerShared;
using Logger;
using System.IO;
using Message.IdGenerator;
using System.Web.Script.Serialization;
using Message.Manager.Protocol.MG;
using DBUtility;
using ServerFrame;
using ServerModels;

namespace GlobalServerLib
{
    public partial class ManagerServer: FrontendServer
    {
        private GlobalServerApi Api
        { get { return (GlobalServerApi)api; } }
      
        public ManagerServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_MG_MAIN_SERVER_STATE>.Value,OnResponse_MainServerState);
            AddResponser(Id<MSG_MG_COMMAND_RESULT>.Value, OnResponse_CommandResult);
            AddResponser(Id<MSG_MG_ALARM_NOTIFY>.Value, OnResponse_AlarmNotify);
            AddResponser(Id<MSG_MG_CHARACTER_LIST>.Value, OnResponse_CharacterList);
            AddResponser(Id<MSG_MG_CHARACTER_INFO>.Value, OnResponse_CharacterInfo);
            AddResponser(Id<MSG_MG_BAG>.Value, OnResponse_Bag);
            AddResponser(Id<MSG_MG_ORDER_STATE>.Value, OnResponse_OrderState);
            //AddResponser(Id<MSG_MG_ACCOUNT_ID>.Value, OnResponse_AccountId);
            AddResponser(Id<MSG_MG_CUSTOM_RESULT>.Value, OnResponse_CustomResult);
            AddResponser(Id<MSG_MG_CAN_RECEIVE_CHANNEL_TASK>.Value, OnResponse_CanReceiveChannelTask);
            //AddResponser(Id<MSG_MG_RECEIVE_CHANNEL_TASK>.Value, OnResponse_ReceiveChannelTask);
            AddResponser(Id<MSG_MG_HK_USER_INFO>.Value, OnResponse_HKUserInfo);
            AddResponser(Id<MSG_MG_ARENA_INFO>.Value, OnResponse_ArenaInfo);
            AddResponser(Id<MSG_MG_FAMILY_INFO>.Value, OnResponse_FamilyInfo);
            AddResponser(Id<MSG_MG_SERVER_INFO>.Value, OnResponser_ServerInfo);
            AddResponser(Id<MSG_MG_GIFT_CODE>.Value, OnResponse_GiftCode);
            AddResponser(Id<MSG_MG_GAME_COUNTER>.Value, OnResponse_GameCounter);
            AddResponser(Id<MSG_MG_CHANGE_FAMILY_NAME>.Value, OnResponse_ChangeFamilyName);
            AddResponser(Id<MSG_MG_CHAR_ALL_INFO>.Value, OnResponse_CharAllInfo);
            AddResponser(Id<MSG_MG_ITEM_TYPE_LIST>.Value, OnResponse_ItemTypeList);
            AddResponser(Id<MSG_MG_PET_TYPE_LIST>.Value, OnResponse_PetTypeList);
            AddResponser(Id<MSG_MG_PET_MOUNT_LIST>.Value, OnResponse_PetMountList);
            AddResponser(Id<MSG_MG_DELETE_PET>.Value, OnResponse_DeletePet);
            AddResponser(Id<MSG_MG_DELETE_PET_MOUNT>.Value, OnResponse_DeletePetMount);
            AddResponser(Id<MSG_MG_EQUIP_LIST>.Value, OnResponse_EquipList);
            AddResponser(Id<MSG_MG_PET_LIST>.Value, OnResponse_PetList);
            AddResponser(Id<MSG_MG_PET_MOUNT_STRENGTH>.Value, OnResponse_PetMountStrength);
            AddResponser(Id<MSG_MG_DELETE_ITEM>.Value, OnResponse_DeleteItem);
            AddResponser(Id<MSG_MG_DELETE_CHAR>.Value, OnResponse_DeleteChar);
            AddResponser(Id<MSG_MG_CHARACTER_LIST_BY_ACCOUNT_NAME>.Value, OnResponse_CharacterListByAccountName);
            AddResponser(Id<MSG_MG_ORDER_LIST>.Value, OnResponse_OrderList);
            AddResponser(Id<MSG_MG_SPEC_ITEM>.Value, OnResponse_SpecItem);
            AddResponser(Id<MSG_MG_SPEC_PET>.Value, OnResponse_SpecPet);
            AddResponser(Id<MSG_MG_TIP_OFF_INFO>.Value, OnResponse_GetTipOffInfo);
            AddResponser(Id<MSG_MG_IGNORE_TIP_OFF>.Value, OnResponse_IgnoreTipOff);
            AddResponser(Id<MSG_MG_GET_ITEM_INFO>.Value, OnResponse_GetItemInfo);
            AddResponser(Id<MSG_MG_DEL_ITEM_NUM>.Value, OnResponse_DelItemNum);
            AddResponser(Id<MSG_MG_DEL_ACTIVITY_PROGRESS>.Value, OnResponse_DelActivityProgress);

            //http select commond
            AddResponser(Id<MSG_MG_RoleInfo>.Value, OnResponse_RoleInfo);
            //ResponserEnd
        }


        public void OnResponse_MainServerState(MemoryStream stream, int uid = 0)
        {
            MSG_MG_MAIN_SERVER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_MAIN_SERVER_STATE>(stream);
            Log.Write("recv main id {0} all server state {1}", msg.MainId, msg.State);
        }

        public void OnResponse_CommandResult(MemoryStream stream, int uid = 0)
        {
            MSG_MG_COMMAND_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_COMMAND_RESULT>(stream);
            if (msg.Success == true)
            {
                Log.Warn("==============================================================================");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("==============================================================================");
            }
            else
            {
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }
        }

        public void OnResponse_AlarmNotify(MemoryStream stream, int uid = 0)
        {
            MSG_MG_ALARM_NOTIFY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_ALARM_NOTIFY>(stream);
            //Log.Warn(msg.Content);
            Api.AccountDBPool.Call(new QueryAlarm(msg.Type, msg.Main, msg.Sub, msg.Time, msg.Content));
            switch ((AlarmType)msg.Type)
            {
                case AlarmType.DB:
                    //DBExceptionList.Add(DateTime.Now);
                    break;
                case AlarmType.NETWORK:
                    // SendEmail
                    //globalserverApi.SendAlarmMail("网络异常报警", msg.Content);
                    break;
            }
        }

        public void OnResponse_CharacterList(MemoryStream stream, int uid = 0)
        { 
            MSG_MG_CHARACTER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CHARACTER_LIST>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                CharacterList response = new CharacterList();
                response.result = 1;
                response.list = new List<CharacterSimpleInfo>();
                foreach (var item in msg.List)
                {
                    CharacterSimpleInfo info = new CharacterSimpleInfo();
                    info.uid = item.Uid;
                    info.name = item.Name;
                    info.level = item.Level;
                    response.list.Add(info);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                client.WriteString(json);
            }
        }

        public void OnResponse_CharacterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CHARACTER_INFO>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client == null) return;
            CharacterInfo response = new CharacterInfo();
            response.result = 1;
            response.uid = msg.Uid;
            response.name = msg.Name;
            response.level = msg.Level;
            response.prestige = msg.Prestige;
            response.vip = msg.Vip;
            response.sex = msg.Sex;
            response.power = msg.Power;
            response.timeCreated = msg.TimeCreated;
            response.camp = msg.Camp;
            response.job = msg.Job;
            response.gold = msg.Gold;
            response.comsumeDiamond = msg.UsedDiamond;
            response.diamond = msg.Diamond;
            response.family = msg.Family;
            response.lastLoginTime = msg.LastLoginTime;
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(response);
            client.WriteString(json);
        }

        public void OnResponse_Bag(MemoryStream stream, int uid = 0)
        {
            MSG_MG_BAG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_BAG>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client != null)
            {
                BagItemList response = new BagItemList();
                response.result = 1;
                response.list = new List<BagItem>();
                foreach (var item in msg.List)
                {
                    BagItem info = new BagItem();
                    info.uid = item.ItemId;
                    info.type = item.ItemType;
                    info.num = item.Num;
                    response.list.Add(info);
                }
                var jser = new JavaScriptSerializer();
                string json = jser.Serialize(response);
                // 背包信息过长 需要先通知背包信息长度
                int length = json.Length;
                JsonLength lengthNotify = new JsonLength();
                lengthNotify.length = length;
                string lengthJson = jser.Serialize(lengthNotify);
                client.WriteString(lengthJson);
                client.WriteString(json);
            }
        }

        public void OnResponse_OrderState(MemoryStream stream, int uid = 0)
        {
            MSG_MG_ORDER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_ORDER_STATE>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client == null) return;
            OrderState response = new OrderState();
            response.result = 1;
            response.uid = msg.Uid;
            response.orderId = msg.OrderId;
            response.money = msg.Money;
            response.time = msg.Time;
            response.state = msg.State;
            response.productId = msg.ProductId;
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(response);
            client.WriteString(json);
        }

        //public void OnResponse_AccountId(MemoryStream stream, int uid = 0)
        //{
        //    MSG_MG_ACCOUNT_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_ACCOUNT_ID>(stream);
        //    Client client = globalserverApi.ClientMng.FindClient(msg.CustomUid);
        //    if (client == null) return;
        //    AccountId response = new AccountId();
        //    response.Result = 1;
        //    response.accountId = msg.AccountId;
        //    var jser = new JavaScriptSerializer();
        //    string json = jser.Serialize(response);
        //    client.WriteString(json);
        //}

        public void OnResponse_CustomResult(MemoryStream stream, int uid = 0)
        {
            MSG_MG_CUSTOM_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CUSTOM_RESULT>(stream);
            Client client = Api.ClientMng.FindClient(msg.CustomUid);
            if (client == null) return;
            ResponseResult result = new ResponseResult();
            result.result = msg.Result;
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(result);
            client.WriteString(json);
        }

        public void OnResponse_CanReceiveChannelTask(MemoryStream stream, int uid = 0)
        {
            MSG_MG_CAN_RECEIVE_CHANNEL_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_CAN_RECEIVE_CHANNEL_TASK>(stream);
            Api.ChannelServer.SendResult(msg.ResIndex, msg.ErrCode, msg.ErrMsg);
        }

        //public void OnResponse_ReceiveChannelTask(MemoryStream stream, int uid = 0)
        //{
        //    MSG_MG_RECEIVE_CHANNEL_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_RECEIVE_CHANNEL_TASK>(stream);
        //    globalserverApi.ChannelServer.SendResult(msg.ResIndex, msg.errCode, msg.errMsg);
        //}

        public void OnResponse_HKUserInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_HK_USER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_HK_USER_INFO>(stream);
            if (msg.ErrCode != 0)
            {
                Api.ChannelServer.SendHKResult(msg.ResIndex, msg.ErrCode);
                return;
            }
            HKUserInfo response = new HKUserInfo();
            response.data = new List<HKSingleUserInfo>();
            response.resIndex = msg.ResIndex;
            response.ret = msg.ErrCode;
            response.msg = "ok";
            foreach (var item in msg.UserList)
            {
                HKSingleUserInfo info = new HKSingleUserInfo();
                info.roleId = item.Uid;
                info.nick = item.Name;
                info.level = item.Level;
                info.force = item.Power;
                info.gold = item.Gold;
                info.diamond = item.Diamond;
                response.data.Add(info);
            }
            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(response);
            Api.ChannelServer.WriteString(json);
        }

    }
}
