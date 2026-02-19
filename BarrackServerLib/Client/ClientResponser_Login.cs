using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Barrack.Protocol.BarrackC;
using Message.Barrack.Protocol.BGate;
using Message.Barrack.Protocol.BM;
using Message.Client.Protocol.CBarrack;
using ServerLogger;
using ServerLogger.KomoeLog;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BarrackServerLib
{
    partial class Client
    {
        private void OnResponse_GameLoad(MemoryStream stream)
        {
            MSG_CB_GAME_LOAD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CB_GAME_LOAD>(stream);
            KomoeEventLogGameLoad(msg.DeviceId, msg.ChannelId, msg.LoadId, msg.LoadStep, msg.LoadTime, msg.LoadType, msg.LoadName);
        }

        private void OnResponse_Login(MemoryStream stream)
        {
            try
            {
                MSG_CB_USER_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CB_USER_LOGIN>(stream);
                Log.Write("account {0} channel {1} sdk id {2} with {3} {4} {5} {6} request to login", msg.AccountName, msg.ChannelName, msg.SdkId, msg.MainId, msg.Token, msg.UnderAge, msg.SdkUuid);
                MSG_BC_USER_LOGIN_ERROR response = new MSG_BC_USER_LOGIN_ERROR();

                if (server.State != ServerShared.ServerState.Started)
                {
                    response.Result = (int)ErrorCode.NotOpen;
#if DEBUG
                    Log.Debug("login response " + msg.AccountName + " " + response.Result + " " + msg.Token);
#endif
                    Write(response);
                    return;
                }

                // 检查白名单
                if (!server.AuthMng.CheckWhite(msg.MainId, Ip, IsTestAccount))
                {
                    response.Result = (int)ErrorCode.NotOpen;
#if DEBUG
                    Log.Debug("login response whitelist " + msg.AccountName + " " + response.Result + " " + msg.Token);
#endif
                    Write(response);
                    return;
                }

#if DEBUG
                if (msg.MainId == 1271)
                {
                    msg.MainId = 1001;
                }
#endif


                //某几个服务器维护中，或者测试未对外
                if (!server.IsOpening(msg.MainId))
                {
                    //对内测试号可以登录
                    if (!IsTestAccount)
                    {
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                }

                if (server.IsMaintainingServer(msg.MainId))
                {
                    //对外维护对内测试号可以登录
                    if (!IsTestAccount)
                    {
                        response.Result = (int)ErrorCode.Maintaining;
                        Write(response);
                        return;
                    }
                }

                // 检查版本号
                if (!server.AuthMng.CheckVersion(msg.Version))
                {
                    MSG_BC_USER_LOGIN BadResponse = new MSG_BC_USER_LOGIN();
                    BadResponse.Result = (int)ErrorCode.BadVersion;

#if DEBUG
                    Log.Debug("login response " + msg.AccountName + " " + response.Result + " " + msg.Token);
#endif
                    Write(BadResponse);
                    return;
                }

                Init(msg.AccountName, msg.ChannelName, msg.SdkId, msg.DeviceId, msg.Token, msg.MainId, msg.UnderAge, msg.SdkUuid,
                    msg.ChannelId, msg.Idfa, msg.Idfv, msg.Imei, msg.Imsi, msg.Anid, msg.Oaid, msg.PackageName, msg.ExtendId, msg.Caid,
                    msg.Tour, msg.Platform, msg.ClientVersion, msg.DeviceModel, msg.OsVersion, msg.Network, msg.Mac);

                Client otherClient = server.ClientMng.FindClientByAccount(accountRealName);
                if (otherClient != null)
                {
                    //多点同时登陆踢掉之前的客户端连接，保持当前的客户端
                    server.ClientMng.RemoveClient(otherClient);
                    //server.ClientMng.RemoveClient(this);
                    // todo sdk 验证队列清理
                }

                //添加到List中
                server.ClientMng.AddClientByAccount(accountRealName, this);

                if (server.AuthMng.AntiAddiction)
                {
                    bool check = server.AntiAddictionServ.BeforeSDKLoginCheck(msg);
                    if (!check)
                    {
#if DEBUG
                        Log.Write("before sdk login check anti addiction check");
                        Log.Debug("login response " + Account + " " + (int)ErrorCode.Addiction + " " + msg.Token);
#endif
                        response.Result = (int)ErrorCode.Addiction;
                        this.Write(response);
                        return;
                    }
                }

                // 是否需要走sdk verify
                if (server.AuthMng.CheckSdkToken)
                {
                    DoLoginAsyncWithSDKCheck(msg);
                }
                else
                {

#if DEBUG
                    DoLogin(msg, true);
#else
                // 直接进入登录流程
                Login();
#endif
                }
            }
            catch (Exception e)
            {
                Log.Error($"OnResponse_Login error {e}");
            }
        }

        private void DoLoginAsyncWithSDKCheck(MSG_CB_USER_LOGIN msg)
        {
            //if (msg.ChannelName.Equals(SEASDKApi.ChannelId))
            try
            {
                if (msg.Platform == "ios")
                {
                    IsIos = true;
                    gameId = SEASDKApi_IOS.Instance.GameId;
                    SEAVerifyInfo check = SEASDKApi_IOS.Instance.Verify(this, msg.SdkId, msg.Token, destMainId);
                    DoLoginAsync(msg, check.Checked, true, new Dictionary<string, object>());
                }
                else
                {
                    switch ((SdkType)msg.SdkType)
                    {
                        case SdkType.HuaWei:
                            {
                                SEAVerifyInfo check = SEASDKApi_Huawei.Instance.Verify(this, msg.SdkId, msg.Token, destMainId);
                                DoLoginAsync(msg, check.Checked, false, new Dictionary<string, object>());
                            }
   
                            break;
                        default:
                            {
                                SEAVerifyInfo check = SEASDKApi_Android.Instance.Verify(this, msg.SdkId, msg.Token, destMainId);
                                DoLoginAsync(msg, check.Checked, false, new Dictionary<string, object>());
                            }
                            break;
                    }
                   /* switch ((SdkType)msg.SdkType)
                    {
                        case SdkType.HuaWei:
                            {
#if DEBUG
                                Log.Info("huawei login param");
#endif

                                gameId = SEASDKApi_Huawei.Instance.GameId;
                                SEAVerifyInfo check = SEASDKApi_Huawei.Instance.Verify(this, msg.SdkId, msg.Token, destMainId);
                                DoLoginAsync(msg, check.Checked, false, new Dictionary<string, object>());
                            }
                            break;
                        default:
                            {
                                gameId = SEASDKApi_Android.Instance.GameId;
                                SEAVerifyInfo check = SEASDKApi_Android.Instance.Verify(this, msg.SdkId, msg.Token, destMainId);
                                DoLoginAsync(msg, check.Checked, false, new Dictionary<string, object>());
                            }
                            break;
                    }*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"client {Account} DoLoginAsyncWithSDKCheck failed: {ex}");
            }
        }

        public void DoLoginAsync(MSG_CB_USER_LOGIN pks, bool success, bool isIos, Dictionary<string, object> ans)
        {
            //将内容放进刷新队列
            AccountEnter enter = new AccountEnter(this, pks, success, isIos, ans);
            server.AccountEnterMng.Add(enter);
        }

        public void DoLogin(MSG_CB_USER_LOGIN pks, bool success)
        {
            if (success)
            {
                if (server.AntiAddictionServ != null && server.AuthMng.AntiAddiction)
                {
                    server.AntiAddictionServ.AfterSDKLogin(pks);
                }

                Login();
            }
            else
            {
                Write(new MSG_BC_USER_LOGIN_ERROR() { Result = (int)ErrorCode.SDKCheckFailed });

#if DEBUG
                Log.Debug("login response " + Account + " " + (int)ErrorCode.SDKCheckFailed + " " + pks.Token);
#endif
            }
        }

        public void IOSDoLogin(MSG_CB_USER_LOGIN pks, bool success)
        {
            if (success)
            {
                if (server.AntiAddictionServ != null && server.AuthMng.AntiAddiction)
                {
                    server.AntiAddictionServ.AfterSDKLogin(pks);
                }

                Login();
            }
            else
            {
                Write(new MSG_BC_USER_LOGIN_ERROR() { Result = (int)ErrorCode.SDKCheckFailed });

#if DEBUG
                Log.Debug("login response " + Account + " " + (int)ErrorCode.SDKCheckFailed + " " + pks.Token);
#endif
            }
        }

        public void Login()
        {
            QueryLoadAccount queryLoad = new QueryLoadAccount(account, channelName);
            server.AccountDBPool.Call(queryLoad, (ret) =>
            {
                int result = (int)ret;
                if (result > 0)
                {
                    if (server.CheckCanEnterOldServer(destMainId, queryLoad.LoginServers, IsTestAccount, account))
                    {
                        InitLoginServers(queryLoad.LoginServers, queryLoad.IsTestAccount, queryLoad.IsRebated);
                        LoginToGate();
                        NotifyManagerAddictionInfo();
                    }
                    else
                    {
                        MSG_BC_USER_LOGIN_ERROR response = new MSG_BC_USER_LOGIN_ERROR();
                        response.Result = (int)ErrorCode.FullPC;
                        this.Write(response);
                    }
                }
                // 帐号不存在，则创建帐号
                else
                {
                    if (server.CheckCanEnterOldServer(destMainId, "", IsTestAccount, account))
                    {
                        server.AccountDBPool.Call(new QueryCreateAccount(account, channelName, sdkId, deviceId, password, BarrackServerApi.now.ToString(), sdkUuid), (ret2) =>
                        {
                            if ((int)ret2 == 1)
                            {
                                //string log = string.Format("{0}|{1}|{2}|{3}|{4}", destMainId, account, channelName, BarrackServerApi.now.ToString("yyyy-MM-dd HH:mm:ss"), destMainId);
                                //server.TrackingLoggerMng.Write(log, TrackingLogType.CREATEACCOUNT);//todo 需要放到createchar那里
                                //server.BILoggerMng.RecordActivateLog(account, DeviceId, channelName, destMainId.ToString(), sdkUuid);
                                server.BILoggerMng.ActivateTaLog(account, DeviceId, channelId, destMainId, sdkUuid);
                                KomoeEventLogCreateAccount(destMainId);

                                LoginToGate();
                                NotifyManagerAddictionInfo();
                            }
                        });
                    }
                    else
                    {
                        MSG_BC_USER_LOGIN_ERROR response = new MSG_BC_USER_LOGIN_ERROR();
                        response.Result = (int)ErrorCode.FullPC;
                        this.Write(response);
                    }
                }
            });
        }

        /*
          * 玩家注册表	create_account	输入账号注册成功	
             model	string	设备机型	设备的机型，例如Samsung GT-I9208
             os_version	string	操作系统版本	操作系统版本，例如13.0.2
             network	string	网络信息	4G/3G/WIFI/2G
             mac	string	mac 地址	局域网地址
             ip	string	玩家登录IP	玩家登录IP
             cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
         */
        public void KomoeEventLogCreateAccount(int destMainId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();
            infDic.Add("b_udid", DeviceId);
            infDic.Add("b_sdk_udid", sdkUuid);
            infDic.Add("b_sdk_uid", account);
            infDic.Add("b_account_id", account);
            infDic.Add("b_tour_indicator", tour);
            //infDic.Add("b_role_id", account);

            infDic.Add("b_game_base_id", KomoeLogConfig.GameBaseId);
            //if (platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", gameId);
            infDic.Add("b_platform", platform);
            infDic.Add("b_zone_id", destMainId);
            infDic.Add("b_channel_id", channelId);
            infDic.Add("b_version", clientVersion);
            infDic.Add("level", 1);
            //infDic.Add("role_name", "");

            string logId = $"{KomoeLogConfig.GameBaseId}-{KomoeLogEventType.create_account}-{account}-{Timestamp.GetUnixTimeStampSeconds(server.Now())}-{1}";
            infDic.Add("b_log_id", logId);
            infDic.Add("b_eventname", KomoeLogEventType.create_account.ToString());

            infDic.Add("b_utc_timestamp", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            infDic.Add("b_datetime", server.Now().ToString("yyyy-MM-dd HH:mm:ss"));

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("model", deviceModel);
            properties.Add("os_version", osVersion);
            properties.Add("network", network);
            properties.Add("mac", mac);
            properties.Add("ip", Ip);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
         * game_load	玩家打开游戏，开始加载资源	
         * 
         load_id	string	加载节点ID	如：100-游戏启动；可参考提供节点信息，根据游戏可调整
         load_step	int	步骤ID	如：开始下载更新，下载内容1，记录为1001
         load_name	string	下载或加载步骤名称	如：开始下载更新
         load_time	float	下载或加载耗费时间：记录秒数，记录为float	
         load_type	string	类型(区分本次为游戏更新还是常规加载)	如：游戏更新:0，资源加载:1
         cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
      */
        public void KomoeEventLogGameLoad(string deviceId, string channelId, int load_id, int load_step, float load_time, int load_type, string load_name)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            Dictionary<string, object> infDic = new Dictionary<string, object>();
            infDic.Add("b_udid", deviceId);
            infDic.Add("b_sdk_udid", deviceId);
            infDic.Add("b_sdk_uid", "");
            infDic.Add("b_account_id", "");
            infDic.Add("b_tour_indicator", 0);
            infDic.Add("b_role_id", "");

            infDic.Add("b_game_base_id", KomoeLogConfig.GameBaseId);
            //if (platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", gameId);
            infDic.Add("b_platform", platform);
            infDic.Add("b_zone_id", 0);
            infDic.Add("b_channel_id", channelId);
            infDic.Add("b_version", server.AuthMng.Version);
            infDic.Add("level", 1);
            infDic.Add("role_name", "");

            string logId = $"{KomoeLogConfig.GameBaseId}-{KomoeLogEventType.game_load}-{deviceId}-{Timestamp.GetUnixTimeStampSeconds(server.Now())}-{1}";
            infDic.Add("b_log_id", logId);
            infDic.Add("b_eventname", KomoeLogEventType.game_load.ToString());

            infDic.Add("b_utc_timestamp", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            infDic.Add("b_datetime", server.Now().ToString("yyyy-MM-dd HH:mm:ss"));

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("load_id", load_id);
            properties.Add("load_step", load_step);
            properties.Add("load_name", load_name);
            properties.Add("load_time", load_time);
            properties.Add("load_type", load_type);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }


        public void NotifyManagerAddictionInfo()
        {
            if (server.AuthMng.AntiAddiction && server.AntiAddictionServ != null)
            {
                AddictionInfo addicInfo = server.AntiAddictionServ.GetLoginAddictionInfo(AccountRealName);
                if (addicInfo == null)
                {
                    Log.Warn($"try get accountId {AccountRealName} addictionInfo got null");
                    return;
                }
                MSG_BM_NOTIFY_ADDICTION_INFO aInfo2M = new MSG_BM_NOTIFY_ADDICTION_INFO();
                string channel = string.IsNullOrEmpty(channelName) ? "default" : channelName;
                aInfo2M.AccountId = $"{account}${channel}";
                aInfo2M.ServerTime = addicInfo.serverTime;
                aInfo2M.RemoveUnderAge = !UnderAge;
                server.ManagerServerManager.GetSinglePointServer(destMainId).Write(aInfo2M);
            }
        }

        public void LoginToGate()
        {
            MSG_BC_USER_LOGIN response = new MSG_BC_USER_LOGIN();
            // 1. 根据main id与 gate 集群状态分配一个gate，根据和服配置获取重定向服务器Id
            int gateServerId = server.ServersConfig.GetRedirectServerId(destMainId);
            GateServer gate = server.GateServerManager.GetLoginGate(gateServerId);

            if (gate == null)
            {
                Write(new MSG_BC_USER_LOGIN_ERROR() { Result = (int)ErrorCode.NotOpen });
                return;
            }

            // 2. 通知gate准备
            int token = ((int)BarrackServerApi.now.Millisecond + BarrackServerApi.Random.Next(1, 1000)).GetHashCode();
            MSG_BGate_LOGIN notify = new MSG_BGate_LOGIN
            {
                AccountId = account,
                ChannelName = channelName,
                DeviceId = deviceId,
                Token = token,
                SdkUuid = sdkUuid,
                IsRebate = IsRebated,
                ChannelId = channelId,
                Idfa = idfa,
                Idfv = idfv,
                Imei = imei,
                Imsi = imsi,
                Anid = anid,
                Oaid = oaid,
                PackageName = packageName,
                ExtendId = extendId,
                Caid = caid,
                MainId = destMainId,
                Tour = tour,
                Platform = platform,
                ClientVersion = clientVersion,
                DeviceModel = deviceModel,
                OsVersion = osVersion,
                Network = network,
                Mac = mac,
                GameId = gameId
            };
            gate.Write(notify);

            // 3. 通知client 连gate
            response.Result = (int)ErrorCode.Success;
            response.GateIp = gate.ClientIp;
            response.GatePort = gate.ClientPort;
            response.Token = token;
#if DEBUG
            Log.Debug("login response " + this.account + " " + response.Result + " " + response.Token);
#endif
            Write(response);

            // 4. 同步db 最近登录时间与最近登录服
            RecordLoginTimeAndServer();
        }

        private void OnResponse_LoginServers(MemoryStream stream)
        {
            MSG_CB_LOGIN_SERVERS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CB_LOGIN_SERVERS>(stream);
            Log.Write("account {0} channel {1} request login servers", msg.AccountName, msg.ChannelName);
            QueryLoadAccount queryLoad = new QueryLoadAccount(msg.AccountName, msg.ChannelName);

            server.AccountDBPool.Call(queryLoad, (ret) =>
            {
                IsTestAccount = queryLoad.IsTestAccount;

                MSG_BC_LOGIN_SERVERS response = new MSG_BC_LOGIN_SERVERS()
                {
                    InWeight = queryLoad.IsTestAccount,
                    Timestemp = Timestamp.GetUnixTimeStampSeconds(server.Now())
                };

                BuildServerInfo(server.ServersConfig.RecommendServer, response.RecommendServerInfo);

                if (!string.IsNullOrEmpty(queryLoad.LoginServers))
                {
                    var infos = SimpleCharacterInfo.GetSimpleCharacterInfos(queryLoad.LoginServers);
                    int count = 0;
                    foreach (var info in infos.Values.OrderByDescending(x => x.Time))
                    {
                        ++count;
                        if (count > 8) break;
                        response.LoginServers.Add(new BC_CHARACTER_SIMPLE_INFO()
                        {
                            ServerId = info.ServerId,
                            Level = info.Level,
                            HeroId = info.HeroId,
                            GodType = info.GodType,
                            Info = BuildServerInfo(info.ServerId),
                        });
                    }
                }
                Write(response);
            });
        }

        private void OnResponse_ServerState(MemoryStream stream)
        {
            MSG_CB_SERVER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CB_SERVER_STATE>(stream);

            MSG_BC_SERVER_STATE response = new MSG_BC_SERVER_STATE();
            response.Timestemp = Timestamp.GetUnixTimeStampSeconds(server.Now());

            BuildServerInfo(msg.Servers, response.ServerList);

            Write(response);
        }

        private void BuildServerInfo(IEnumerable<int> serverIds, RepeatedField<BC_SERVER_STATE_INFO> infos)
        {
            foreach (var id in serverIds)
            {
                //当前服务器没有开启 不需要发送给前端信息(测试账号除外，测试账号可以显示未堆外开放的服务器)
                if (!server.IsOpening(id))
                {
                    if (!IsTestAccount)
                    {
                        continue;
                    }
                }

                BC_SERVER_STATE_INFO info = BuildServerInfo(id);
                if (info != null)
                {
                    infos.Add(info);
                }
            }
        }

        private BC_SERVER_STATE_INFO BuildServerInfo(int id)
        {
            //if (server.IsMaintainingServer(id)) return null;

            //ManagerServer manager = server.ManagerServerManager.GetOneServer(id) as ManagerServer;
            //if (manager == null) return null;

            //if (manager?.State == ServerShared.ServerState.Started || !server.AuthMng.IsWhite(id))
            int registCharacterCount = 0;
            int redirectId = server.ServersConfig.GetRedirectServerId(id);
            ManagerServer manager = server.ManagerServerManager.GetOneServer(redirectId) as ManagerServer;
            if (manager != null)
            {
                if (manager.State == ServerShared.ServerState.Started)
                {
                    registCharacterCount = manager.RegistCharacterCount;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                ServerItemModel data = server.ServersConfig.Get(id);
                if (data != null)
                {
                    registCharacterCount = data.RegistCharacterCount;
                }
                else
                {
                    return null;
                }
            }

            if (!server.AuthMng.IsWhite(id))
            {
                return new BC_SERVER_STATE_INFO()
                {
                    ServerId = id,
                    //OnlineCount = manager.OnlineCharacterCount,
                    RegistCount = registCharacterCount,
                    IsNew = server.IsNewServer(redirectId),
                    LineUp = server.IsLineUpServer(redirectId),
                    Recommend = server.IsRecommendServer(redirectId),
                    IsMaintaining = server.IsMaintainingServer(redirectId),
                };
            }
            return null;
        }
    }
}
