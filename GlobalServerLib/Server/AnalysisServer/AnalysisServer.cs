using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using Logger;
using Message.Analysis.Protocol.AG;
using Message.IdGenerator;
using ServerFrame;
using ServerModels;

namespace GlobalServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class AnalysisServer : FrontendServer
    {
        GlobalServerApi Api { get { return (GlobalServerApi)api; } }

        public AnalysisServer(BaseApi api) : base(api)
        { }

        protected override void BindResponser()
        {
            base.BindResponser();

            AddResponser(Id<MSG_AG_ITEM_PRODUCE>.Value, OnResponse_ItemProduce);
            AddResponser(Id<MSG_AG_ITEM_CONSUME>.Value, OnResponse_ItemConsume);
            AddResponser(Id<MSG_AG_CURRENCY_PRODUCE>.Value, OnResponse_CurrencyProduce);
            AddResponser(Id<MSG_AG_CURRENCY_CONSUME>.Value, OnResponse_CurrencyConsume);
            AddResponser(Id<MSG_AG_LOGINORLOGOUT>.Value, OnResponse_LoginOrLogout);
            AddResponser(Id<MSG_AG_COMMAND_RESULT>.Value, OnResponse_CommandResult);
            //ResponseEnd
        }

        public void OnResponse_CommandResult(MemoryStream stream, int uid = 0)
        {
            MSG_AG_COMMAND_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AG_COMMAND_RESULT>(stream);
            if (msg.Success)
            {
                Log.Warn("====================================================");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("====================================================");
            }
            else
            {
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }
        }

        public void OnResponse_ItemProduce(MemoryStream stream, int uid = 0)
        {
            MSG_AG_ITEM_PRODUCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AG_ITEM_PRODUCE>(stream);

            Log.Info("gm request LoginOrLogout globalServer");

            Client client = Api.ClientMng.FindClient(uid);
            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }

            GMRecordCache.Instance.CacheItemProduce(msg.Uid, msg.Page, msg.ItemList);
            if (msg.IsLast)
            {
                GMRecordCache.Instance.SendClientItemProduce(client, msg.Uid,1);
            }

            Log.Info("gm request LoginOrLogout globalServer cache");
        }
        
        public void OnResponse_ItemConsume(MemoryStream stream, int uid = 0)
        {
            MSG_AG_ITEM_CONSUME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AG_ITEM_CONSUME>(stream);
            Client client = Api.ClientMng.FindClient(uid);

            Log.Info("gm request ItemConsume globalServer");

            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }

            GMRecordCache.Instance.CacheItemConsume(msg.Uid, msg.Page, msg.ItemList);
            if (msg.IsLast)
            {
                GMRecordCache.Instance.SendClientItemConsume(client, msg.Uid,1);
            }


            Log.Info("gm request LoginOrLogout globalServer cache");
        }

        public void OnResponse_CurrencyProduce(MemoryStream stream, int uid = 0)
        {
            MSG_AG_CURRENCY_PRODUCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AG_CURRENCY_PRODUCE>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            Log.Info("gm request CurrencyProduce globalServer");

            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }
            
            GMRecordCache.Instance.CacheCurrencyProduce(msg.Uid, msg.Page, msg.ItemList);
            if (msg.IsLast)
            {
                GMRecordCache.Instance.SendClientCurrencyProduce(client, msg.Uid,1);
            }

            Log.Info("gm request LoginOrLogout globalServer cache");
        }

        public void OnResponse_CurrencyConsume(MemoryStream stream, int uid = 0)
        {
            MSG_AG_CURRENCY_CONSUME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AG_CURRENCY_CONSUME>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            Log.Info("gm request CurrencyConsume globalServer");

            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }
            
            GMRecordCache.Instance.CacheCurrencyConsume(msg.Uid, msg.Page, msg.ItemList);
            if (msg.IsLast)
            {
                GMRecordCache.Instance.SendClientCurrencyConsume(client, msg.Uid,1);
            }
            
            Log.Info("gm request LoginOrLogout globalServer cache");
        }

        public void OnResponse_LoginOrLogout(MemoryStream stream, int uid = 0)
        {
            MSG_AG_LOGINORLOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_AG_LOGINORLOGOUT>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            Log.Info("gm request LoginOrLogout globalServer");

            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }

            GMRecordCache.Instance.CacheLoginLogout(msg.Uid, msg.Page, msg.ItemList);
            if (msg.IsLast)
            {
                GMRecordCache.Instance.SendClientLoginLogout(client, msg.Uid,1);
            }

            Log.Info("gm request LoginOrLogout globalServer cache");
        }
    }
}
