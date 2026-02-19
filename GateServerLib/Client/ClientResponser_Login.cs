using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateM;
using Message.Gate.Protocol.GateZ;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {

        private void OnResponse_TimeSync(MemoryStream stream)
        {
            MSG_GC_TIME_SYNC msg = new MSG_GC_TIME_SYNC();
            msg.TimeStamp = Timestamp.GetUnixTimeStampSeconds(GateServerApi.now);
            Write(msg);
        }

        public void OnResponse_CharList(MemoryStream stream)
        {
            MSG_CG_CHARACTER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHARACTER_LIST>(stream);
            string accountId = msg.AccountId;
            string channelName = msg.ChannelName;
            Log.Debug("MSG_CG_CHARACTER_LIST with " + msg.AccountId + " " + msg.ChannelName);

            //先校验token
            LoginClient loginClient = server.ClientMng.GetLoginClient(accountId, channelName);
            if (loginClient != null)
            {
                if (msg.Token == loginClient.Token)
                {
                    Token = loginClient.Token;
                    AccountName = accountId;

                    //FIXME:这里需要验证渠道，设备等是否合法。（就是和barrack给过来的一致）
                    if (loginClient.ChannelName == msg.ChannelName)
                    {
                        if (!string.IsNullOrWhiteSpace(msg.ChannelName))
                        {
                            ChannelName = msg.ChannelName;
                        }

                        DeviceId = loginClient.DeviceId;
                        SDKUuid = loginClient.sdkUuid;
                        IsRebated = loginClient.IsRebated;

                        this.channelId = loginClient.channelId;
                        this.idfa = loginClient.idfa;       //苹果设备创建角色时使用
                        this.idfv = loginClient.idfv;       //苹果设备创建角色时使用
                        this.imei = loginClient.imei;       //安卓设备创建角色时使用
                        this.imsi = loginClient.imsi;       //安卓设备创建角色时使用
                        this.anid = loginClient.anid;       //安卓设备创建角色时使用
                        this.oaid = loginClient.oaid;       //安卓设备创建角色时使用
                        this.packageName = loginClient.packageName;//包名
                        this.extendId = loginClient.extendId;   //广告Id，暂时不使用
                        this.caid = loginClient.caid;       //暂时不使用
                        MainId = loginClient.MainId;

                        this.tour = loginClient.tour;                   //是否是游客账号（0:非游客，1：游客）
                        this.platform = loginClient.platform;           //平台名称	统一：ios|android|windows
                        this.clientVersion = loginClient.clientVersion; //游戏的迭代版本，例如1.0.3
                        this.deviceModel = loginClient.deviceModel;     //设备的机型，例如Samsung GT-I9208
                        this.osVersion = loginClient.osVersion;         //操作系统版本，例如13.0.2
                        this.network = loginClient.network;             //网络信息	4G/3G/WIFI/2G
                        this.mac = loginClient.mac;                     //局域网地址                      
                        this.gameId = loginClient.gameId;
                        Log.Info($"Account {loginClient.AccountId} charList ChannelId {loginClient.channelId} Idfa {loginClient.idfa} Idfv { loginClient.idfv} Imei { loginClient.imei} Imsi { loginClient.imsi} Anid{loginClient.anid} Oaid {loginClient.oaid} PackageName {loginClient.packageName} ExtendId {loginClient.extendId} Caid{loginClient.oaid} Tour {loginClient.tour} Platform {loginClient.platform} ClientVersion {loginClient.clientVersion} DeviceModel { loginClient.deviceModel} OsVersion {loginClient.deviceModel} Network {loginClient.network} Mac {loginClient.mac}");
                    }
                    else
                    {
                        Log.Error("accout {0} channel not match: login channel is {1},but msg channel is {2}");
                    }

                    GetTableCharLoginInfo(accountId, msg.ChannelName, MainId);
                }
                else
                {
                    Log.Warn("account {0} channelName {1} request character list fail: token error {2} and {3}", accountId, channelName, msg.Token, loginClient.Token);
                    MSG_GC_CHARACTER_LIST response = new MSG_GC_CHARACTER_LIST();
                    response.Result = (int)ErrorCode.BadToken;
                    Write(response);
                    return;
                }
            }
            else
            {
                Log.Warn("account {0} channelName {1} request character list fail: not find login client", accountId, channelName);
                MSG_GC_CHARACTER_LIST response = new MSG_GC_CHARACTER_LIST();
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
        }

        private void GetTableCharLoginInfo(string accountName, string channelName, int mainId)
        {
            QueryGetCharacterList query = new QueryGetCharacterList(accountName, channelName, mainId);
            server.GameDBPool.Call(query, (ret) =>
            {
                CharacterList = query.CharacterList;
                MSG_GC_CHARACTER_LIST response = new MSG_GC_CHARACTER_LIST();
                response.Result = (int)ErrorCode.Success;

                foreach (var item in query.CharacterList)
                {
                    MSG_GC_ENTER_CHARACTER_INFO info = new MSG_GC_ENTER_CHARACTER_INFO();
                    info.Uid = item.Uid;
                    info.Name = item.Name;
                    info.Level = item.Level;
                    info.Job = item.Job;
                    info.Sex = item.Sex;
                    info.Camp = item.Camp;
                    info.HeroId = item.HeroId;
                    info.HeroLevel = item.HeroLevel;
                    info.HeroTitleLevel = item.HeroTitleLevel;
                    info.HeroAwrakenLevel = item.HeroAwrakenLevel;
                    info.FashionId = item.FashionId;
                    info.FaceIcon = item.FaceIcon;
                    info.ShowFaceJpg = item.ShowFaceJpg;
                    info.FollowerId = item.FollowerId;
                    response.List.Add(info);
                }
                if (!GM)
                {
                    GM = query.GM;
                }
                Write(response);
            });
        }

        private void TryGetTableCharLoginInfo(string accountName, string channelName, int mainId)
        {
            QueryGetCharacterList query = new QueryGetCharacterList(accountName, channelName, mainId);
            server.GameDBPool.Call(query, (ret) =>
            {
                CharacterList = query.CharacterList;


                if (!GM)
                {
                    GM = query.GM;
                }
                //Write(response);

                //假如没有，创建，假如有，直接登入
                if (query.CharacterList.Count > 0)
                {
                    MSG_GC_LOGIN_RESULT response = new MSG_GC_LOGIN_RESULT();
                    response.Result = (int)ErrorCode.Success;

                    CharacterEnterInfo item = query.CharacterList[0];

                    MSG_GC_ENTER_CHARACTER_INFO info = new MSG_GC_ENTER_CHARACTER_INFO();
                    info.Uid = item.Uid;
                    info.Name = item.Name;
                    info.Level = item.Level;
                    info.Job = item.Job;
                    info.Sex = item.Sex;
                    info.Camp = item.Camp;
                    info.HeroId = item.HeroId;
                    info.HeroLevel = item.HeroLevel;
                    info.HeroTitleLevel = item.HeroTitleLevel;
                    info.HeroAwrakenLevel = item.HeroAwrakenLevel;
                    info.FashionId = item.FashionId;
                    info.FaceIcon = item.FaceIcon;
                    info.ShowFaceJpg = item.ShowFaceJpg;
                    info.FollowerId = item.FollowerId;
                    response.Info = info;
                    Write(response);
                    LoginToZone();
                }
                else
                {
                    //通知前端
                    MSG_GC_NEED_CREATECHAR res = new MSG_GC_NEED_CREATECHAR();
                    Write(res);
                }
            });
        }

        public void LoginToZone()
        {
            if (CharacterList != null)
            {
                Log.Write("server req account {0} uid {1} to zone", AccountName, CharacterList[0].Uid);
                LoginToZone(CharacterList[0]);
                return;
            }
        }

        private void OnResponse_Login(MemoryStream stream)
        {
            MSG_CG_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LOGIN>(stream);

            string accountId = msg.AccountId;
            string channelName = msg.ChannelName;
            Log.Debug("MSG_CG_LOGIN with " + msg.AccountId + " " + msg.ChannelName);
            //先校验token
            LoginClient loginClient = server.ClientMng.GetLoginClient(accountId, channelName);
            if (loginClient != null)
            {
                if (msg.Token == loginClient.Token)
                {
                    Token = loginClient.Token;
                    AccountName = accountId;

                    if (loginClient.ChannelName == msg.ChannelName)
                    {
                        if (!string.IsNullOrWhiteSpace(msg.ChannelName))
                        {
                            ChannelName = msg.ChannelName;
                        }
                        DeviceId = loginClient.DeviceId;
                        SDKUuid = loginClient.sdkUuid;
                        IsRebated = loginClient.IsRebated;

                        this.channelId = loginClient.channelId;
                        this.idfa = loginClient.idfa;       //苹果设备创建角色时使用
                        this.idfv = loginClient.idfv;       //苹果设备创建角色时使用
                        this.imei = loginClient.imei;       //安卓设备创建角色时使用
                        this.imsi = loginClient.imsi;       //安卓设备创建角色时使用
                        this.anid = loginClient.anid;       //安卓设备创建角色时使用
                        this.oaid = loginClient.oaid;       //安卓设备创建角色时使用
                        this.packageName = loginClient.packageName;//包名
                        this.extendId = loginClient.extendId;   //广告Id，暂时不使用
                        this.caid = loginClient.caid;       //暂时不使用
                        this.MainId = loginClient.MainId;

                        this.tour = loginClient.tour;                   //是否是游客账号（0:非游客，1：游客）
                        this.platform = loginClient.platform;           //平台名称	统一：ios|android|windows
                        this.clientVersion = loginClient.clientVersion; //游戏的迭代版本，例如1.0.3
                        this.deviceModel = loginClient.deviceModel;     //设备的机型，例如Samsung GT-I9208
                        this.osVersion = loginClient.osVersion;         //操作系统版本，例如13.0.2
                        this.network = loginClient.network;             //网络信息	4G/3G/WIFI/2G
                        this.mac = loginClient.mac;                     //局域网地址
                        this.gameId = loginClient.gameId;

                        Log.Info($"Account Login {loginClient.AccountId} charList ChannelId {loginClient.channelId} Idfa {loginClient.idfa} Idfv { loginClient.idfv} Imei { loginClient.imei} Imsi { loginClient.imsi} Anid{loginClient.anid} Oaid {loginClient.oaid} PackageName {loginClient.packageName} ExtendId {loginClient.extendId} Caid{loginClient.oaid} Tour {loginClient.tour} Platform {loginClient.platform} ClientVersion {loginClient.clientVersion} DeviceModel { loginClient.deviceModel} OsVersion {loginClient.deviceModel} Network {loginClient.network} Mac {loginClient.mac}");

                    }
                    else
                    {
                        Log.Error("accout {0} channel not match: login channel is {1},but msg channel is {2}");
                    }

                    TryGetTableCharLoginInfo(accountId, msg.ChannelName, MainId);
                }
                else
                {
                    MSG_GC_LOGIN_RESULT response = new MSG_GC_LOGIN_RESULT();
                    response.Result = (int)ErrorCode.BadToken;
                    Write(response);
                    Log.Warn("account {0} channelName {1} request character list fail: token error {2} and {3}", accountId, channelName, msg.Token, loginClient.Token);
                    return;
                }
            }
            else
            {
                MSG_GC_LOGIN_RESULT response = new MSG_GC_LOGIN_RESULT();
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                Log.Warn("account {0} channelName {1} request character list fail: not find login client", accountId, channelName);
                return;
            }
        }

        private void OnResponse_CreateCharacter(MemoryStream stream)
        {
            MSG_CG_CREATE_CHARACTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CREATE_CHARACTER>(stream);

            //if (CharacterList.Count > 0 || DataProperty.DataListManager.inst.GetData("ServerListRedirect", MainId) != null)
            if (CharacterList.Count > 0)
            {
                MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();
                response.Result = (int)ErrorCode.NotAllowed;
                Log.Debug("account " + AccountName + " create char with error" + response.Result);
                Write(response);
                return;
            }
            if (string.IsNullOrEmpty(AccountName))
            {
                return;
            }
            if (ReqCreateMsg != null)
            {
                Log.Warn("account {0} request create char {1} sex {2} job {3} systemFashion {4} fail: repeat to create", AccountName, msg.Name, msg.Sex, msg.Job, msg.SystemFashionId);
                return;
            }
            Log.Write("account {0} request create char {1} sex {2} job {3}  systemFashion {4} ", AccountName, msg.Name, msg.Sex, msg.Job, msg.SystemFashionId);

            // 找watchDog发送请求
            BackendServer manager = server.ManagerServerManager.GetSinglePointServer(server.MainId);
            if (manager != null)
            {
                // 从配置中加载
                ReqCreateMsg = new MSG_CG_CREATE_CHARACTER();
                ReqCreateMsg.Name = CharacterInitLibrary.InitName;
                ReqCreateMsg.Sex = CharacterInitLibrary.InitHeroSex;
                ReqCreateMsg.Job = CharacterInitLibrary.InitJob;
                //ReqCreateMsg = msg;
                MSG_GateM_MaxUid request = new MSG_GateM_MaxUid();
                request.AccountName = AccountName;
                request.ChannelName = ChannelName;
                Log.Debug("MSG_GateM_MaxUid  create char with " + AccountName + " " + ChannelName);
                manager.Write(request);
            }
            else
            {
                MSG_GC_CREATE_CHARACTER res = new MSG_GC_CREATE_CHARACTER();
                res.Result = (int)ErrorCode.NotOpen;
                Log.Debug("account " + AccountName + " create char with error" + res.Result);
                Write(res);
            }
        }

        //private void OnResponse_CreateCharacter(MemoryStream stream)
        //{
        //    //return;

        //    MSG_CG_CREATE_CHARACTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CREATE_CHARACTER>(stream);
        //    msg.Name = msg.Name.Trim();
        //    if (string.IsNullOrEmpty(AccountName))
        //    {
        //        return;
        //    }
        //    if (ReqCreateMsg != null)
        //    {
        //        Log.Warn("account {0} request create char {1} sex {2} job {3} systemFashion {4} fail: repeat to create", AccountName, msg.Name, msg.Sex, msg.Job, msg.SystemFashionId);
        //        return;
        //    }
        //    Log.Write("account {0} request create char {1} sex {2} job {3}  systemFashion {4} ", AccountName, msg.Name, msg.Sex, msg.Job, msg.SystemFashionId);

        //    if (IsGm == 0)
        //    {
        //        if (string.IsNullOrEmpty(msg.Name))
        //        {
        //            MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();

        //            response.Result = (int)ErrorCode.BadWord;
        //            Log.Debug("account " + AccountName + " create char with error" + response.Result);
        //            Write(response);
        //            return;
        //        }
        //        // 检查屏蔽字
        //        if (server.NameChecker.HasSpecialSymbol(msg.Name) || server.NameChecker.HasBadWord(msg.Name))
        //        {
        //            MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();
        //            response.Result = (int)ErrorCode.BadWord;
        //            Log.Debug("account " + AccountName + " create char with error" + response.Result);
        //            Write(response);
        //            return;
        //        }
        //        if (server.NameChecker.GetWordLen(msg.Name) > WordLengthLimit.CharNameLenLimit)
        //        {
        //            MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();
        //            response.Result = (int)ErrorCode.NameLength;
        //            Log.Debug("account " + AccountName + " create char with error" + response.Result);
        //            Write(response);
        //            return;
        //        }
        //    }

        //    // 找watchDog发送请求
        //    BackendServer manager = server.ManagerServerManager.GetSinglePointServer(server.MainId);
        //    if (manager != null)
        //    {
        //        // 记录创建角色请求信息
        //        ReqCreateMsg = msg;
        //        MSG_GateM_MaxUid request = new MSG_GateM_MaxUid();
        //        request.AccountName = AccountName;
        //        request.ChannelName = ChannelName;
        //        Log.Debug("MSG_GateM_MaxUid  create char with " + AccountName + " " + ChannelName);
        //        manager.Write(request);
        //    }
        //    else
        //    {
        //        MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();
        //        Log.Debug("account " + AccountName + " create char with error" + response.Result);
        //        response.Result = (int)ErrorCode.NotOpen;
        //        Write(response);
        //    }
        //}

        //private void OnResponse_ToZone(MemoryStream stream)
        //{
        //    MSG_CG_TO_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TO_ZONE>(stream);
        //    Log.Write("account {0} request uid {1} to zone", AccountName, msg.Uid);
        //    if (CharacterList != null)
        //    {
        //        LoginToZone();
        //        return;

        //        //// step 1 检查是否有该角色uid
        //        //foreach (var item in CharacterList)
        //        //{
        //        //    if (item.Uid == msg.Uid)
        //        //    {
        //        //        // 找到角色，进入地图
        //        //        LoginToZone(item);
        //        //        return;
        //        //    }
        //        //}
        //    }
        //    //没有找到角色，通知客户端
        //    MSG_GC_TO_ZONE response = new MSG_GC_TO_ZONE();
        //    response.Result = (int)ErrorCode.Unknown;
        //    Write(response);
        //}

        public void LoginToZone(CharacterEnterInfo loginInfo)
        {
            BackendServer manager = server.ManagerServerManager.GetSinglePointServer(loginInfo.MainId);
            if (manager == null)
            {
                Log.Warn("player {0} request to zone main {1} failed: server not open", loginInfo.Uid, loginInfo.MainId);
                MSG_GC_TO_ZONE response = new MSG_GC_TO_ZONE();
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            MainId = loginInfo.MainId;
            MSG_GateM_CharacterGetZone request = new MSG_GateM_CharacterGetZone();
            request.Uid = loginInfo.Uid;
            request.MapId = loginInfo.MapId;
            request.Channel = loginInfo.Channel;
            request.MainId = manager.MainId;
            request.CreateTimeStamp = Timestamp.GetUnixTimeStamp(AccountCreatedTime);
            request.AccountName = AccountName;
            // 渠道
            request.ChannelName = ChannelName;
            request.Token = Token;
            request.Routed = false;
            manager.Write(request);
        }

        public void NotifyWaitingTime(int index)
        {
            MSG_GC_LOGIN_WAIT_QUEUE notify = new MSG_GC_LOGIN_WAIT_QUEUE();
            notify.Index = index;
            notify.WaitingTime = index * (CONST.LOGIN_QUEUE_PERIOD / 1000);
            Write(notify);
        }

        private void OnResponse_MapLoadingDone(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_MAP_LOADING_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_MAP_LOADING_DONE>(stream);
            MSG_GateZ_MAP_LOADING_DONE request = new MSG_GateZ_MAP_LOADING_DONE();
            request.Uid = Uid;
            request.MapId = msg.MapId;
            request.Channel = msg.Channel;
            WriteToZone(request);
        }

        private void OnResponse_ChangeChannel(MemoryStream stream)
        {
            MSG_CG_CHANGE_CHANNEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHANGE_CHANNEL>(stream);
            MSG_GateZ_CHANGE_CHANNEL request = new MSG_GateZ_CHANGE_CHANNEL();
            request.Uid = Uid;
            request.Channel = msg.Channel;
            WriteToZone(request);
        }

        private void OnResponse_ReconnectLogin(MemoryStream stream)
        {
            if (server.State != ServerState.Started)
            {
                MSG_GC_RECONNECT_LOGIN response = new MSG_GC_RECONNECT_LOGIN();
                response.Result = (int)ErrorCode.ServerNotExist;
                Write(response);
                return;
            }
            MSG_CG_RECONNECT_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RECONNECT_LOGIN>(stream);
            Log.Write("player {0} request reconnect login", msg.Uid);
            Client oldClient = server.ClientMng.FindClientByUid(msg.Uid);
            if (oldClient != null && oldClient.BlowFishKey == msg.OldBlowFish)
            {
                server.ClientMng.RemoveClient(oldClient);
                MSG_GC_RECONNECT_LOGIN response = new MSG_GC_RECONNECT_LOGIN();
                response.Result = (int)ErrorCode.NotClosed;
                Write(response);
                return;
            }

            OfflineClient offlineClient = server.ClientMng.GetOfflineClient(msg.Uid);
            if (offlineClient == null)
            {
                MSG_GC_RECONNECT_LOGIN response = new MSG_GC_RECONNECT_LOGIN();
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            if (offlineClient.AccountName != msg.AccountName)
            {
                MSG_GC_RECONNECT_LOGIN response = new MSG_GC_RECONNECT_LOGIN();
                response.Result = (int)ErrorCode.InvalidAccount;
                Write(response);
                return;
            }
            BackendServer zone = server.ZoneServerManager.GetServer(offlineClient.MainId, offlineClient.SubId);
            if (zone == null)
            {
                MSG_GC_RECONNECT_LOGIN response = new MSG_GC_RECONNECT_LOGIN();
                response.Result = (int)ErrorCode.ServerNotExist;
                Write(response);
                return;
            }
            // 验证通过 准备进入世界
            AccountName = msg.AccountName;
            EnterWorld(msg.Uid, zone);
            MSG_GateZ_RECONNECT_LOGIN request = new MSG_GateZ_RECONNECT_LOGIN();
            request.Uid = Uid;
            request.Token = msg.Token;
            request.Ip = tcp.IP;
            zone.Write(request);
        }

        private void OnResponse_Logout(MemoryStream stream)
        {
            MSG_CG_LOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LOGOUT>(stream);
            Log.Write("player {0} request log out", Uid);
            if (Uid == 0 || curZone == null)
            {
                return;
            }
            MSG_GateZ_LOGOUT notify = new MSG_GateZ_LOGOUT();
            notify.Uid = Uid;
            WriteToZone(notify);
        }


        private void OnResponse_LoginGetSoftwares(MemoryStream stream)
        {
            MSG_CG_LOGIN_GET_SOFTWARES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LOGIN_GET_SOFTWARES>(stream);
            MSG_GateZ_LOGIN_GET_SOFTWARES notify = new MSG_GateZ_LOGIN_GET_SOFTWARES();
            notify.List.AddRange(msg.List);
            WriteToZone(notify);
        }

        
        private void OnResponse_ShipStep(MemoryStream stream)
        {
            MSG_CG_SHIP_STEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHIP_STEP>(stream);
            Log.Write("player {0} ship step {1}", Uid, msg.Step);
            if (CharacterList == null || CharacterList.Count == 0)
            {
                return;
            }
            int uid = CharacterList[0].Uid;
            QueryUpdateShipStep query = new QueryUpdateShipStep(uid, msg.Step);
            server.GameDBPool.Call(query);
        }

    }

}
