using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using Logger;
using Message.Analysis.Protocol.AG;
using ServerModels;
using ServerShared;

namespace GlobalServerLib
{
    internal class GMRecordCache
    {
        private Dictionary<int, DateTime> deadLineTime = new Dictionary<int, DateTime>();

        private Dictionary<int, ItemConsumeList> itemProduceResponse = new Dictionary<int, ItemConsumeList>();
        private Dictionary<int, ItemConsumeList> itemConsumeResponse = new Dictionary<int, ItemConsumeList>();
        private Dictionary<int, ItemConsumeList> currencyProduceResponse = new Dictionary<int, ItemConsumeList>();
        private Dictionary<int, ItemConsumeList> currencyConsumeResponse = new Dictionary<int, ItemConsumeList>();
        private Dictionary<int, LoginLogoutList> loginLogoutResponse = new Dictionary<int, LoginLogoutList>();


        public static GMRecordCache Instance = new GMRecordCache();

        public int PageSize { get; set; }

        public void Update(DateTime time)
        {
            DoClear(time);
        }

        private void DoClear(DateTime time)
        {
            if(deadLineTime.Count<=0) return;
            
            var list = deadLineTime.Where(x => x.Value <= time).ToList();
            foreach (var pair in list)
            {
                itemProduceResponse.Remove(pair.Key);
                itemConsumeResponse.Remove(pair.Key);
                currencyProduceResponse.Remove(pair.Key);
                currencyConsumeResponse.Remove(pair.Key);
                loginLogoutResponse.Remove(pair.Key);
                
                deadLineTime.Remove(pair.Key);
            }
        }

        private void AddDeadLine(int uid)
        {
            deadLineTime[uid] = DateTime.Now.AddMinutes(5);
        }

        public void CacheItemProduce(int uid, int page, IEnumerable<AG_ITEM_INFO> items)
        {
            ItemConsumeList response;
            if (!itemProduceResponse.TryGetValue(uid, out response))
            {
                response = new ItemConsumeList {list = new List<ProduceConsumItemInfo>()};
                itemProduceResponse[uid] = response;
            }

            items.ForEach(x => BuildItemProduceConsumeMsg(response.list, x));
            AddDeadLine(uid);
        }

        public void CacheItemConsume(int uid, int page, IEnumerable<AG_ITEM_INFO> items)
        {
            ItemConsumeList response;
            if (!itemConsumeResponse.TryGetValue(uid, out response))
            {
                response = new ItemConsumeList {list = new List<ProduceConsumItemInfo>()};
                itemConsumeResponse[uid] = response;
            }

            items.ForEach(x => BuildItemProduceConsumeMsg(response.list, x));
            AddDeadLine(uid);
        }

        public void CacheCurrencyProduce(int uid, int page, IEnumerable<AG_ITEM_INFO> items)
        {
            ItemConsumeList response;
            if (!currencyProduceResponse.TryGetValue(uid, out response))
            {
                response = new ItemConsumeList {list = new List<ProduceConsumItemInfo>()};
                currencyProduceResponse[uid] = response;
            }

            items.ForEach(x => BuildItemProduceConsumeMsg(response.list, x));
            AddDeadLine(uid);
        }

        public void CacheCurrencyConsume(int uid, int page, IEnumerable<AG_ITEM_INFO> items)
        {
            ItemConsumeList response;
            if (!currencyConsumeResponse.TryGetValue(uid, out response))
            {
                response = new ItemConsumeList {list = new List<ProduceConsumItemInfo>()};
                currencyConsumeResponse[uid] = response;
            }

            items.ForEach(x => BuildItemProduceConsumeMsg(response.list, x));
            AddDeadLine(uid);
        }

        public void CacheLoginLogout(int uid, int page, IEnumerable<AG_LOGIN_LOGOUT_INFO> items)
        {
            LoginLogoutList response;
            if (!loginLogoutResponse.TryGetValue(uid, out response))
            {
                response = new LoginLogoutList() {list = new List<LoginLogoutInfo>()};
                loginLogoutResponse[uid] = response;
            }

            items.ForEach(x =>
            {
                response.list.Add(new LoginLogoutInfo()
                {
                    uid = x.Uid,
                    accountId = x.AccountId,
                    name = x.Name,
                    serverId = x.ServerId,
                    level = x.Level,
                    channel = x.Channel,
                    time = x.Time,
                    isLogin = x.IsLogin,
                    ip = x.Ip,
                    diamond = x.Diamond,
                    gold = x.Gold,
                    exp = x.Exp
                });
            });
            AddDeadLine(uid);
        }

        private void BuildItemProduceConsumeMsg(List<ProduceConsumItemInfo> list, AG_ITEM_INFO msg)
        {
            list.Add(new ProduceConsumItemInfo()
            {
                uid = msg.Uid,
                time = msg.Time,
                level = msg.Level,
                way = msg.Way,
                channel = msg.Channel,
                type = msg.Type,
                currNum = msg.CurrNum,
                changeNum = msg.ChangeNum,
                extraParam = msg.Extraparam,
                modelId = msg.ModelId,
            });
        }


        public bool SendClientItemProduce(Client client, int uid, int page)
        {
            ItemConsumeList response;
            if (!itemProduceResponse.TryGetValue(uid, out response)) return false;

            SendClientItemInfo(client, response, page);

            Log.Info($"gm request SendClientItemProduce globalServer return data count {response.list.Count}");

            return true;
        }

        public bool SendClientItemConsume(Client client, int uid, int page)
        {
            ItemConsumeList response;
            if (!itemConsumeResponse.TryGetValue(uid, out response)) return false;

            SendClientItemInfo(client, response, page);

            Log.Info($"gm request SendClientItemConsume globalServer return data count {response.list.Count}");

            return true;
        }

        public bool SendClientCurrencyProduce(Client client, int uid, int page)
        {
            ItemConsumeList response;
            if (!currencyProduceResponse.TryGetValue(uid, out response)) return false;

            SendClientItemInfo(client, response, page);

            Log.Info($"gm request SendClientCurrencyProduce globalServer return data count {response.list.Count}");

            return true;
        }

        public bool SendClientCurrencyConsume(Client client, int uid, int page)
        {
            ItemConsumeList response;
            if (!currencyConsumeResponse.TryGetValue(uid, out response))
            {
                client.SendFailed();
                return false;
            }

            SendClientItemInfo(client, response, page);

            Log.Info($"gm request SendClientCurrencyConsume globalServer return data count {response.list.Count}");

            return true;
        }

        private void SendClientItemInfo(Client client, ItemConsumeList source, int page)
        {
            int startIndex = (page - 1) * PageSize;
            int endIndex = page * PageSize;

            ItemConsumeList msg = new ItemConsumeList()
            {
                list = new List<ProduceConsumItemInfo>(),
                page = page,
                count = source.list.Count,
                result = 1
            };

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i < source.list.Count)
                {
                    msg.list.Add(source.list[i]);
                }
            }

            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(msg);
            client.WriteString(json);
        }

        public bool SendClientLoginLogout(Client client, int uid, int page)
        {
            LoginLogoutList response;
            if (!loginLogoutResponse.TryGetValue(uid, out response))
            {
                client.SendFailed();
                return false;
            }

            int startIndex = (page - 1) * PageSize;
            int endIndex = page * PageSize;

            LoginLogoutList msg = new LoginLogoutList()
            {
                list = new List<LoginLogoutInfo>(),
                page = page,
                count = response.list.Count,
                result = 1
            };

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i < response.list.Count)
                {
                    msg.list.Add(response.list[i]);
                }
            }

            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(msg);
            client.WriteString(json);

            return true;
        }

        public void ClearItemProduce(int uid)
        {
            itemProduceResponse.Remove(uid);
        }

        public void ClearItemConsume(int uid)
        {
            itemConsumeResponse.Remove(uid);
        }

        public void ClearCurrencyProduce(int uid)
        {
            currencyProduceResponse.Remove(uid);
        }

        public void ClearCurrencyConsume(int uid)
        {
            currencyConsumeResponse.Remove(uid);
        }

        public void ClearLoginLogout(int uid)
        {
            loginLogoutResponse.Remove(uid);
        }
    }
}