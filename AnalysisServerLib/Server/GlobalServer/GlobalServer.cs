using Message.Analysis.Protocol.AG;
using Message.Global.Protocol.GA;
using Message.IdGenerator;
using ServerFrame;
using System.IO;
using DBUtility;
using System.Collections.Generic;
using System;
using CommonUtility;
using Google.Protobuf.Collections;
using Logger;

namespace AnalysisServerLib
{
    public partial class GlobalServer : BaseGlobalServer
    {
        private AnalysisServerApi Api => (AnalysisServerApi)api;

        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();

            AddResponser(Id<MSG_GA_ITEM_PRODUCE>.Value, OnResponse_ItemProduce);
            AddResponser(Id<MSG_GA_ITEM_CONSUME>.Value, OnResponse_ItemConsume);
            AddResponser(Id<MSG_GA_CURRENCY_PRODUCE>.Value, OnResponse_CurrencyProduce);
            AddResponser(Id<MSG_GA_CURRENCY_CONSUME>.Value, OnResponse_CurrencyConsume);
            AddResponser(Id<MSG_GA_LOGINORLOGOUT>.Value, OnResponse_LoginOrLogout);
            AddResponser(Id<MSG_GA_SHUTDOWN_GATE>.Value, OnResponse_ShutDown);
            AddResponser(Id<MSG_GA_UPDATE_XML>.Value, OnResponse_UpdateXml);
            //ResponserEnd
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_GA_UPDATE_XML msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GA_UPDATE_XML>(stream);
            if (msg.Type == 1)
            {
                //Api.UpdateServerXml();
            }
            else
            {
                Api.UpdateXml();
            }
            Log.Write("GM update xml");
        }

        private void OnResponse_ShutDown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown gate");
            Api.StopServer(1);

            MSG_AG_COMMAND_RESULT msg2global = new MSG_AG_COMMAND_RESULT();
            msg2global.Success = false;
            msg2global.Info.Add(String.Format("analysis main {0} sub {1} frame {2} sleep time {3} memory{4}",
         Api.MainId, Api.SubId, 0, 0, 0));
            Write(msg2global);
        }

        private void OnResponse_ItemProduce(MemoryStream stream, int uid = 0)
        {
            MSG_GA_ITEM_PRODUCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GA_ITEM_PRODUCE>(stream);

            Log.Info("gm request ItemProduce analysisServer {0}", msg);

            List<AbstractDBQuery> queryList = new List<AbstractDBQuery>();
            DateTime fromTime = Timestamp.TimeStampToDateTime(msg.Info.FromTime);
            DateTime toTime = Timestamp.TimeStampToDateTime(msg.Info.ToTime);
            int days = (int)(toTime - fromTime).TotalDays + 1;
            int queryDays = 0;
            for (int i = 0; i < days; i++)
            {
                DateTime date = fromTime.Date.AddDays(i);
                //if (date < Api.OpenServerDate || date > api.Now()) continue;

                string tableName = "ITEMPRODUCE_" + date.ToString("yyyy_MM_dd");
                queryList.Add(new QueryItmeProduce(msg.Info.Uid, msg.Way, tableName));

                if (queryDays++ > 60) break;
            }

            MSG_AG_ITEM_PRODUCE response = new MSG_AG_ITEM_PRODUCE()
            {
                Page = msg.Info.Page,
                Uid = msg.Info.Uid
            };
            DBQuerysWithoutTransaction querys = new DBQuerysWithoutTransaction(queryList);
            Api.LogDBPool.Call(querys, ret =>
            {
                if ((int)ret == 1)
                {
                    int count = 0;
                    foreach (var kv in queryList)
                    {
                        QueryItmeProduce query = kv as QueryItmeProduce;
                        if (query?.list.Count <= 0) continue;

                        foreach (var info in query.list)
                        {
                            if (++count > 50)
                            {
                                count = 0;
                                Write(response, uid);
                                response = new MSG_AG_ITEM_PRODUCE()
                                {
                                    Page = msg.Info.Page,
                                    Uid = msg.Info.Uid
                                };
                            }
                            BuildItemProduceConsumeInfo(response.ItemList, info);
                        }
                        Log.Info($"gm request ItemProduce analysisServer data count {query.list.Count}");
                    }
                }
                response.IsLast = true;
                Write(response, uid);
            });
        }

        private void OnResponse_ItemConsume(MemoryStream stream, int uid = 0)
        {
            MSG_GA_ITEM_CONSUME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GA_ITEM_CONSUME>(stream);

            Log.Info("gm request ItemConsume analysisServer {0}", msg);

            List<AbstractDBQuery> queryList = new List<AbstractDBQuery>();
            DateTime fromTime = Timestamp.TimeStampToDateTime(msg.Info.FromTime).Date;
            DateTime toTime = Timestamp.TimeStampToDateTime(msg.Info.ToTime).Date;
            int days = (int)(toTime - fromTime).TotalDays + 1;
            int queryDays = 0;
            for (int i = 0; i < days; i++)
            {
                DateTime date = fromTime.Date.AddDays(i);
                //if (date < Api.OpenServerDate || date > api.Now()) continue;

                string tableName = "ITEMCONSUME_" + date.ToString("yyyy_MM_dd");
                queryList.Add(new QueryItmeConsume(msg.Info.Uid, msg.Way, tableName));
                if (queryDays++ > 60) break;
            }

            Log.Info($"gm request ItemConsume analysisServer days {days} start time {fromTime} end time {toTime} query count {queryList.Count}");

            MSG_AG_ITEM_CONSUME response = new MSG_AG_ITEM_CONSUME()
            {
                Page = msg.Info.Page,
                Uid = msg.Info.Uid
            };
            DBQuerysWithoutTransaction querys = new DBQuerysWithoutTransaction(queryList);
            Api.LogDBPool.Call(querys, ret =>
            {
                if ((int)ret == 1)
                {
                    int count = 0;
                    foreach (var kv in queryList)
                    {
                        QueryItmeConsume query = kv as QueryItmeConsume;
                        if (query?.list.Count <= 0) continue;

                        foreach (var info in query.list)
                        {
                            if (++count > 50)
                            {
                                count = 0;
                                Write(response, uid);
                                response = new MSG_AG_ITEM_CONSUME(){Page = msg.Info.Page, Uid = msg.Info.Uid};
                            }
                            BuildItemProduceConsumeInfo(response.ItemList, info);
                        }
                    }
                    Log.Info($"gm request ItemConsume analysisServer  date count {count}");
                }
                else
                {
                    Log.Info("gm request ItemConsume analysisServer got error"+ querys.ErrorText);
                }
                response.IsLast = true;
                Write(response, uid);
            });
        }

        private void OnResponse_CurrencyProduce(MemoryStream stream, int uid = 0)
        {
            MSG_GA_ITEM_PRODUCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GA_ITEM_PRODUCE>(stream);

            Log.Info("gm request CurrencyProduce analysisServer {0}", msg);

            List<AbstractDBQuery> queryList = new List<AbstractDBQuery>();
            DateTime fromTime = Timestamp.TimeStampToDateTime(msg.Info.FromTime);
            DateTime toTime = Timestamp.TimeStampToDateTime(msg.Info.ToTime);
            int days = (int)(toTime - fromTime).TotalDays + 1;
            int queryDays = 0;
            for (int i = 0; i < days; i++)
            {
                DateTime date = fromTime.Date.AddDays(i);
                //if (date < Api.OpenServerDate || date > api.Now()) continue;

                string tableName = "OBTAINCURRENCY_" + date.ToString("yyyy_MM_dd");
                queryList.Add(new QueryCurrencyProduce(msg.Info.Uid, msg.Way, tableName));
                if (queryDays++ > 60) break;
            }

            MSG_AG_CURRENCY_PRODUCE response = new MSG_AG_CURRENCY_PRODUCE()
            {
                Page = msg.Info.Page,
                Uid = msg.Info.Uid
            };
            DBQuerysWithoutTransaction querys = new DBQuerysWithoutTransaction(queryList);
            Api.LogDBPool.Call(querys, ret =>
            {
                if ((int)ret == 1)
                {
                    int count = 0;
                    foreach (var kv in queryList)
                    {
                        QueryCurrencyProduce query = kv as QueryCurrencyProduce;
                        if (query?.list.Count <= 0) continue;

                        foreach (var info in query.list)
                        {
                            if (++count > 50)
                            {
                                count = 0;
                                Write(response, uid);
                                response = new MSG_AG_CURRENCY_PRODUCE(){Page = msg.Info.Page, Uid = msg.Info.Uid};
                            }
                            BuildItemProduceConsumeInfo(response.ItemList, info);
                        }
                        Log.Info($"gm request CurrencyProduce analysisServer data count {query.list.Count}");
                    }
                }
                response.IsLast = true;
                Write(response, uid);
            });
        }

        private void OnResponse_CurrencyConsume(MemoryStream stream, int uid = 0)
        {
            MSG_GA_ITEM_CONSUME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GA_ITEM_CONSUME>(stream);

            Log.Info($"gm request CurrencyConsume analysisServer");

            List<AbstractDBQuery> queryList = new List<AbstractDBQuery>();
            DateTime fromTime = Timestamp.TimeStampToDateTime(msg.Info.FromTime);
            DateTime toTime = Timestamp.TimeStampToDateTime(msg.Info.ToTime);
            int days = (int)(toTime - fromTime).TotalDays + 1;
            int queryDays = 0;
            for (int i = 0; i < days; i++)
            {
                DateTime date = fromTime.Date.AddDays(i);
                //if (date < Api.OpenServerDate || date > api.Now()) continue;

                string tableName = "CONSUMECURRENCY_" + date.ToString("yyyy_MM_dd");
                queryList.Add(new QueryCurrencyConsume(msg.Info.Uid, msg.Way, tableName));
                if (queryDays++ > 60) break;
            }

            MSG_AG_CURRENCY_CONSUME response = new MSG_AG_CURRENCY_CONSUME()
            {
                Page = msg.Info.Page,
                Uid = msg.Info.Uid
            };
            DBQuerysWithoutTransaction querys = new DBQuerysWithoutTransaction(queryList);
            Api.LogDBPool.Call(querys, ret =>
            {
                if ((int)ret == 1)
                {
                    int count = 0;
                    foreach (var kv in queryList)
                    {
                        QueryCurrencyConsume query = kv as QueryCurrencyConsume;
                        if (query?.list.Count <= 0) continue;

                        foreach (var info in query.list)
                        {
                            if (++count > 50)
                            {
                                count = 0;
                                Write(response, uid);
                                response = new MSG_AG_CURRENCY_CONSUME(){Page = msg.Info.Page, Uid = msg.Info.Uid};
                            }
                            BuildItemProduceConsumeInfo(response.ItemList, info);
                        }
                        Log.Info($"gm request CurrencyConsume analysisServer data count {query.list.Count}");
                    }
                }
                response.IsLast = true;
                Write(response, uid);
            });
        }

        private void BuildItemProduceConsumeInfo(RepeatedField<AG_ITEM_INFO> field, ItemProduceConsumeInfo info)
        {
            field.Add(new AG_ITEM_INFO()
            {
                Uid = info.uid,
                Time = info.time,
                Level = info.level,
                Way = info.way,
                Channel = info.channel,
                Type = info.type,
                CurrNum = info.currNum,
                ChangeNum = info.changeNum,
                ModelId = info.modelId,
            });
        }

        public void OnResponse_LoginOrLogout(MemoryStream stream, int uid = 0)
        {
            MSG_GA_LOGINORLOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GA_LOGINORLOGOUT>(stream);

            Log.Info("gm request LoginOrLogout analysisServer {0}", msg);

            List<AbstractDBQuery> queryList = new List<AbstractDBQuery>();
            DateTime fromTime = Timestamp.TimeStampToDateTime(msg.Info.FromTime);
            DateTime toTime = Timestamp.TimeStampToDateTime(msg.Info.ToTime);
            int days = (int)(toTime - fromTime).TotalDays + 1;
            int queryDays = 0;
            for (int i = 0; i < days; i++)
            {
                DateTime date = fromTime.Date.AddDays(i);
                //if (date < Api.OpenServerDate || date > api.Now()) continue;

                string tableName = (msg.IsLogin ? "LOGIN_" : "LOGOUT_") + date.ToString("yyyy_MM_dd");
                queryList.Add(new QueryLoginLogoutLog(msg.Info.Uid, msg.IsLogin, tableName));
                if (queryDays++ > 60) break;
            }
            MSG_AG_LOGINORLOGOUT response = new MSG_AG_LOGINORLOGOUT()
            {
                Page = msg.Info.Page,
                Uid = msg.Info.Uid
            };
            Log.Info("gm request LoginOrLogout analysisServer {0} query count {1}", msg, queryList.Count);

            DBQuerysWithoutTransaction querys = new DBQuerysWithoutTransaction(queryList);
            Api.LogDBPool.Call(querys, ret =>
            {
                if ((int)ret == 1)
                {
                    int count = 0;
                    foreach (var kv in queryList)
                    {
                        QueryLoginLogoutLog query = kv as QueryLoginLogoutLog;
                        if (query?.list.Count <= 0) continue;

                        foreach (var info in query.list)
                        {
#if DEBUG
                            if (++count > 2)
#else
                            if (++count > 50)
#endif
                            {
                                count = 0;
                                Write(response, uid);
                                response = new MSG_AG_LOGINORLOGOUT(){Page = msg.Info.Page, Uid = msg.Info.Uid};
                            }
                            response.ItemList.Add(BuildLoginLogoutInfo(msg, info));
                        }
                        Log.Info($"gm request LoginOrLogout analysisServer data count {query.list.Count}");
                    }
                    response.IsLast = true;
                    Write(response, uid);
                }
                else
                {
                    Log.Info("gm request LoginOrLogout analysisServer {0} query count {1} error {2}", msg, queryList.Count, querys.ErrorText);
                }
            });
        }

        private AG_LOGIN_LOGOUT_INFO BuildLoginLogoutInfo(MSG_GA_LOGINORLOGOUT msg, LoginLogoutInfo x)
        {
            return new AG_LOGIN_LOGOUT_INFO()
            {
                Uid = msg.Info.Uid,
                AccountId = x.accountId,
                Name = x.name,
                ServerId = x.serverId,
                Level = x.level,
                Channel = x.channel,
                Time = x.time,
                IsLogin = msg.IsLogin,
                Ip = x.ip,
                Diamond = x.diamond,
                Gold = x.gold,
                Exp = x.exp
            };
        }
    }
}