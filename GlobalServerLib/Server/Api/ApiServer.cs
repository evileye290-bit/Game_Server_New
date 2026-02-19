using DataProperty;
using Logger;
using Message.Global.Protocol.GA;
using Message.Global.Protocol.GB;
using Message.Global.Protocol.GBM;
using Message.Global.Protocol.GCross;
using Message.Global.Protocol.GGate;
using Message.Global.Protocol.GM;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;

namespace GlobalServerLib
{
    public partial class GlobalServerApi
    {
        private DateTime lastManagerHeartBeatTime = DateTime.Now;

        public ChannelServer ChannelServer
        { get { return null; } }

        public FrontendServerManager ManagerServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.ManagerServer); } }

        public FrontendServerManager RelationServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.RelationServer); } }

        public FrontendServerManager ZoneServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.ZoneServer); } }

        //public ChatManagerServer ChatManagerServer
        //{ get { return (ChatManagerServer)(serverManagerProxy.GetSinglePointFrontendServer(ServerType.ChatManagerServer, ClusterId)); } }

        public BattleManagerServer BattleManagerServer
        { get { return (BattleManagerServer)(serverManagerProxy.GetSinglePointFrontendServer(ServerType.BattleManagerServer, ClusterId)); } }

        public FrontendServerManager BarrackServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.BarrackServer); } }

        public FrontendServerManager BattleServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.BattleServer); } }

        public FrontendServerManager GateServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.GateServer); } }

        public FrontendServerManager CrossServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.CrossServer); } }

        public FrontendServerManager AnalysisServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.AnalysisServer); } }

        public FrontendServerManager PayServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.PayServer); } }

        private HttpCommondServer httpGmServer = null;

        public HttpCommondServer HttpGmServer
        {
            get { return httpGmServer; }
            set { httpGmServer = value; }
        }

        public void InitGmHttpServer()
        {
            //DataList tempData = DataListManager.inst.GetDataList("ServerConfig");
            //Data globalData = tempData.Get("GlobalServer");
            Data globalData = DataListManager.inst.GetData("GlobalServer", 1);
            ushort controlPort = (ushort)globalData.GetInt("controlPort");
            string ips = globalData.GetString("controlIp");
            string[] ip = ips.Split('_');
            List<string> ipList = new List<string>(ip);
            string password = globalData.GetString("controlWord");
            httpGmServer = new HttpCommondServer(ipList, controlPort.ToString(), password);
            httpGmServer.Init(this);
        }

        public void UpdateGmHttpServer()
        {
            Data globalData = DataListManager.inst.GetData("GlobalServer", 1);
            List<string> ipList = globalData.GetStringList("controlIp", "_");
            string password = globalData.GetString("controlWord");
            httpGmServer.UpdateIpList(ipList, password);
        }

        #region NotInUse
        //    private void ExcuteCommand(string cmd)
        //    {
        //        string[] cmdArr = cmd.Split(' ');
        //        string serverType = "";
        //        if (cmdArr.Length == 0)
        //        {
        //            return;
        //        }
        //        string help = "";
        //        switch (cmdArr[0])
        //        {
        //            case "chatinfo":
        //                {
        //                    // 显示该指定main id下 该map有多少线，每个线挂在哪个zone，当前地图有多少人，有多少人在切往该图中
        //                    help = "chatinfo Usage: chatinfo world/nearby";
        //                    if (cmdArr.Length != 2)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    if (ChatManagerServer == null)
        //                    {
        //                        return;
        //                    }
        //                    switch (cmdArr[1])
        //                    {
        //                        case "world":
        //                            MSG_GCM_WORLD_CHAT_INFO worldMsg = new MSG_GCM_WORLD_CHAT_INFO();
        //                            ChatManagerServer.Write(worldMsg);
        //                            break;
        //                        case "nearby":
        //                            MSG_GCM_NEARBY_CHAT_INFO nearbyMsg = new MSG_GCM_NEARBY_CHAT_INFO();
        //                            ChatManagerServer.Write(nearbyMsg);
        //                            break;
        //                        default:
        //                            Log.Warn(help);
        //                            break;
        //                    }
        //                }
        //                break;
        //            case "mapinfo":
        //                {
        //                    // 显示该指定main id下 该map有多少线，每个线挂在哪个zone，当前地图有多少人，有多少人在切往该图中
        //                    help = "MapInfo Usage: mapinfo mainId mapId";
        //                    if (cmdArr.Length != 3)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId, mapId;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out mapId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                    if (mServer == null|| mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("MapInfo failed: Manager server {0} not ready", mainId);
        //                        return;
        //                    }
        //                    MSG_GM_MAP_INFO msg = new MSG_GM_MAP_INFO();
        //                    msg.MainId = mainId;
        //                    msg.MapId = mapId;
        //                    mServer.Write(msg);
        //                }
        //                break;
        //            case "zoneinfo":
        //                {
        //                    // 显示该zone有多少图 每个图当前多少人，多少人在切往该图中，该zone CPU信息 帧率 人数统计信息
        //                    help = "ZoneInfo Usage: zoneinfo mainId subId";
        //                    if (cmdArr.Length != 3)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId, subId;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                    if (mServer == null || mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("ZoneInfo failed: Manager server {0} not ready", mainId);
        //                        return;
        //                    }
        //                    MSG_GM_ZONE_INFO msg = new MSG_GM_ZONE_INFO();
        //                    msg.MainId = mainId;
        //                    msg.SubId = subId;
        //                    mServer.Write(msg);
        //                }
        //                break;

        //            case "allzoneinfo":
        //                {
        //                    // 显示该main id下所有zone的人数 CPU 帧率 挂载map数 挂载副本数
        //                    help = "AllZoneInfo Usage: allzoneinfo mainId";
        //                    if (cmdArr.Length != 2)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                    if (mServer == null || mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("AllZoneInfo failed: Manager server {0} not ready", mainId);
        //                        return;
        //                    }
        //                    MSG_GM_ALL_ZONE_INFO msg = new MSG_GM_ALL_ZONE_INFO();
        //                    msg.MainId = mainId;
        //                    mServer.Write(msg);
        //                }
        //                break;
        //            case "battleinfo":
        //                {
        //                    // battle CPU信息 帧率 
        //                    help = "BattleInfo Usage: battleinfo mainId subId";
        //                    if (cmdArr.Length != 3)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId, subId;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    if (BattleManagerServer == null)
        //                    {
        //                        Log.Warn("BattleInfo failed: battle manager server not ready");
        //                        return;
        //                    }
        //                    else
        //                    {
        //                        MSG_GBM_BATTLE_INFO msgBattleInfo = new MSG_GBM_BATTLE_INFO();
        //                        msgBattleInfo.MainId = mainId;
        //                        msgBattleInfo.subId = subId;
        //                        BattleManagerServer.Write(msgBattleInfo);
        //                    }

        //                }
        //                break;
        //            case "allbattleinfo":
        //                {
        //                    // 显示该main id下所有battle  CPU FPS
        //                    help = "AllBattleInfo Usage: allBattleinfo";
        //                    if (BattleManagerServer != null)
        //                    {
        //                        MSG_GBM_ALL_BATTLE_INFO msg = new MSG_GBM_ALL_BATTLE_INFO();
        //                        BattleManagerServer.Write(msg);
        //                    }
        //                }
        //                break;
        //            case "gateinfo":
        //                {
        //                     //gate CPU信息 帧率 
        //                    help = "gateinfo Usage: gateinfo mainId subId";
        //                    if (cmdArr.Length != 3)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId, subId;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    RequestGateInfo(mainId, subId);
        //                }
        //                break;
        //            case "allgateinfo":
        //                {
        //                    MSG_GB_ALL_GATE_INFO msg = new MSG_GB_ALL_GATE_INFO();
        //                    //BarrackServer.Broadcast(msg);
        //                    FrontendServer barrack = BarrackServerManager.GetWatchDogServer();
        //                    barrack.Write(msg);
        //                }
        //                break;
        //            case "relationinfo":
        //                {
        //                    //gate CPU信息 帧率 
        //                    help = "relationinfo Usage: relationinfo mainId";
        //                    if (cmdArr.Length != 2)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int relationMainId;
        //                    if (int.TryParse(cmdArr[1], out relationMainId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    MSG_GM_RELATION_FPS_INFO msg= new MSG_GM_RELATION_FPS_INFO();
        //                    msg.MainId = relationMainId;
        //                    if (relationMainId != 0)
        //                    {
        //                        FrontendServer mserverForRelation = ManagerServerManager.GetSinglePointServer(relationMainId);
        //                        if (mserverForRelation == null)
        //                        {
        //                            Log.Warn("relationinfo failed: relation server {0} not exist", relationMainId);
        //                            return;
        //                        }
        //                        mserverForRelation.Write(msg);
        //                    }
        //                    else
        //                    {
        //                        ManagerServerManager.Broadcast(msg);
        //                    }
        //                }
        //                break;
        //            case "barrackinfo":
        //                {
        //                    //gate CPU信息 帧率 
        //                    help = "barrckinfo Usage: barrckinfo";
        //                    MSG_GB_FPS_INFO msg = new MSG_GB_FPS_INFO();
        //                    BarrackServerManager.Broadcast(msg);
        //                }
        //                break;
        //            case "managerinfo":
        //                {
        //                    //gate CPU信息 帧率 
        //                    help = "managerinfo Usage: managerinfo mainId";
        //                    if (cmdArr.Length != 2)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    MSG_GM_FPS_INFO msg = new MSG_GM_FPS_INFO();
        //                    if (mainId != 0)
        //                    {
        //                        FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                        if (mServer == null)
        //                        {
        //                            Log.Warn("managerinfo failed: manager server {0} not exist", mainId);
        //                            return;
        //                        }
        //                        mServer.Write(msg);
        //                    }
        //                    else
        //                    {
        //                        ManagerServerManager.Broadcast(msg);
        //                    }
        //                }
        //                break;
        //            case "battlemanagerinfo":
        //                {
        //                    //gate CPU信息 帧率 
        //                    help = "battlemanagerinfo Usage: battlemanagerinfo";
        //                    if (BattleManagerServer == null) 
        //                    {
        //                        Log.Warn("battlemanagerinfo failed: battle manager not exist");
        //                        return;
        //                    }
        //                    else
        //                    {
        //                        MSG_GBM_FPS_INFO msg = new MSG_GBM_FPS_INFO();
        //                        BattleManagerServer.Write(msg);
        //                    }
        //                }
        //                break;
        //            case "kick":
        //                {
        //                    help = "Kick Usage: Kick mainId playerUid";
        //                    if (cmdArr.Length != 3)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId;
        //                    int uid;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out uid) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                    if (mServer == null || mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("kick failed: Manager server {0} not ready", mainId);
        //                        return;
        //                    }
        //                    MSG_GM_KICK_PLAYER msg = new MSG_GM_KICK_PLAYER();
        //                    msg.MainId = mainId;
        //                    msg.Uid = uid;
        //                    mServer.Write(msg);
        //                }
        //                break;
        //            case "freeze":
        //                {
        //                    help = "Freeze Usage: freeze mainId uid freezeType hour";
        //                    if (cmdArr.Length < 4)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    int mainId;
        //                    int uid;
        //                    int freezeType;
        //                    int hour = 0;
        //                    if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out uid) == false || int.TryParse(cmdArr[3], out freezeType) == false)
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    if (freezeType == (int)FreezeState.Freeze)
        //                    {
        //                        if (cmdArr.Length != 5)
        //                        {
        //                            Log.Warn(help);
        //                            return;
        //                        }
        //                        if (int.TryParse(cmdArr[4], out hour) == false)
        //                        {
        //                            if (cmdArr.Length != 5)
        //                            {
        //                                Log.Warn(help);
        //                                return;
        //                            }
        //                        }
        //                    }
        //                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                    if (mServer == null || mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("freeze failed: Manager server {0} not ready", mainId);
        //                        return;
        //                    }

        //                    DateTime freezeTime = DateTime.MinValue;
        //                    if (freezeType == (int)FreezeState.Freeze)
        //                    {
        //                        freezeTime = GlobalServerApi.now.AddHours(hour);
        //                    }
        //                    string tableName = DB.GetTableName("character", uid, DBTableParamType.Character);
        //                    DB.Call(new QueryFreezePlayer(uid, tableName, (int)FreezeState.Freeze, freezeTime, ""), tableName);

        //                    MSG_GM_FREEZE_PLAYER msg = new MSG_GM_FREEZE_PLAYER();
        //                    msg.MainId = mainId;
        //                    msg.Uid = uid;
        //                    msg.freezeType = freezeType;
        //                    msg.hour = hour;
        //                    mServer.Write(msg);
        //                }
        //                break;
        //            case "shutdownzone":
        //                ShutdownZone(cmdArr); 
        //                break;
        //            case "shutdownbarrack":
        //                ShutdownBarrack(cmdArr);
        //                break;
        //            case "shutdownrelation":
        //                ShutdownRelation(cmdArr);
        //                break;
        //            case "shutdowncountry":
        //                ShutdownCountry(cmdArr);
        //                break;
        //            case "shutdownbattlemanager":
        //                ShutdownBattleManager(cmdArr);
        //                break;
        //            case "shutdownbattle":
        //                ShutdownBattle(cmdArr);
        //                break;
        //            case "shutdownbattle1":
        //                ShutdownBattleDirectly(cmdArr);
        //                break;
        //            case "shutdownzone1":
        //                ShutdownZoneDirectly(cmdArr);
        //                break;
        //            case "shutdownrelation1":
        //                ShutdownRelationDerectly(cmdArr);
        //                break;
        //            case "shutdowngate":
        //                ShutdownGate(cmdArr);
        //                break;
        //            case "shutdowngate1":
        //                ShutdownGateDirectly(cmdArr);
        //                break;
        //            case "shutdownchatmanager":
        //                ShutdownChatManager(cmdArr);
        //                break;
        //            case "sendemail":
        //                {
        //                    help = "sendemail Usage: sendemail mainId emailId \"where\"sql\" (saveTime);  if omission (saveTime),  will save no delete time";
        //                    int mainId;
        //                    int emailId;
        //                    int saveTime = 0;
        //                    string sqlConditions = string.Empty;
        //                    if (cmdArr.Length == 4)
        //                    {
        //                        if (int.TryParse(cmdArr[1], out mainId) == false ||
        //                            int.TryParse(cmdArr[2], out emailId) == false)
        //                        {
        //                            Log.Warn(help);
        //                            return;
        //                        }
        //                    }
        //                    else if (cmdArr.Length == 5)
        //                    {
        //                        if (int.TryParse(cmdArr[1], out mainId) == false ||
        //                            int.TryParse(cmdArr[2], out emailId) == false ||
        //                            int.TryParse(cmdArr[4], out saveTime) == false)
        //                        {
        //                            Log.Warn(help);
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    sqlConditions = cmdArr[3].Replace("\"", " ");
        //                    if (mainId == 0)
        //                    {
        //                        foreach (var item in ManagerServerManager.ServerList)
        //                        {
        //                            MSG_GM_SEND_EMAIL msgEmail = new MSG_GM_SEND_EMAIL();
        //                            msgEmail.EmailId = emailId;
        //                            msgEmail.SaveTime = saveTime;
        //                            msgEmail.MainId = item.Value.MainId;
        //                            msgEmail.SqlConditions = sqlConditions.Trim();
        //                            item.Value.Write(msgEmail);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                        if (mServer == null || mServer.State != ServerState.Started)
        //                        {
        //                            Log.Warn("sendemail failed: Manager server {0} not ready", mainId);
        //                            return;
        //                        }
        //                        MSG_GM_SEND_EMAIL msgEmail = new MSG_GM_SEND_EMAIL();
        //                        msgEmail.EmailId = emailId;
        //                        msgEmail.SaveTime = saveTime;
        //                        msgEmail.MainId = mainId;
        //                        msgEmail.SqlConditions = sqlConditions.Trim();
        //                        mServer.Write(msgEmail);
        //                    }
        //                }
        //                break;
        //            case "announcement":
        //                MSG_GB_REFRESH_ANNOUNCEMENT msgAnnouncement = new MSG_GB_REFRESH_ANNOUNCEMENT();
        //                BarrackServerManager.Broadcast(msgAnnouncement);
        //                break;
        //            case "updatexml":
        //                {
        //                    help = @"updatexml Usage: updatexml or updatexml serverType;";
        //                    if (cmdArr.Length == 1)
        //                    {
        //                        serverType = "";
        //                    }
        //                    else if (cmdArr.Length == 2 )
        //                    {
        //                        serverType = cmdArr[1];
        //                    }
        //                    else
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }

        //                    if (serverType == "zone" || serverType == "")
        //                    {
        //                        foreach (var item in ManagerServerManager.ServerList.Values)
        //                        {
        //                            MSG_GM_UPDATE_XML msgUpdateXml = new MSG_GM_UPDATE_XML();
        //                            msgUpdateXml.mainId = item.MainId;
        //                            item.Write(msgUpdateXml);
        //                        }
        //                    }

        //                    if (serverType == "gate" || serverType == "")
        //                    {
        //                        MSG_GGate_UPDATE_XML msgUpdateGateXml = new MSG_GGate_UPDATE_XML();
        //                        GateServerManager.Broadcast(msgUpdateGateXml);
        //                    }

        //                    if (serverType == "battle" || serverType == "")
        //                    {
        //                        if (BattleManagerServer != null)
        //                        {
        //                            MSG_GBM_UPDATE_XML updateXmlMsg = new MSG_GBM_UPDATE_XML();
        //                            BattleManagerServer.Write(updateXmlMsg);
        //                        }
        //                    }

        //                    if(serverType == "chat" || serverType=="")
        //                    {
        //                        if (ChatManagerServer != null)
        //                        {
        //                            MSG_GCM_UPDATE_XML updateXmlCm = new MSG_GCM_UPDATE_XML();
        //                            ChatManagerServer.Write(updateXmlCm);
        //                        }
        //                    }
        //                }
        //                break;
        //            case "updateranklist":
        //                {
        //                    help = @"updatranklist Usage: updatranklist";
        //                    foreach (var item in ManagerServerManager.ServerList.Values)
        //                    {
        //                        ManagerServer manager = (ManagerServer)item;
        //                        if (manager.WatchDog)
        //                        {
        //                            MSG_GM_UPDATE_RANKLIST msgUpdateRank = new MSG_GM_UPDATE_RANKLIST();
        //                            item.Write(msgUpdateRank);
        //                        }
        //                    }
        //                }
        //                break;
        //            case "openwhitelist":
        //                MSG_GGate_WHITE_LIST msgOpenWhiteList = new MSG_GGate_WHITE_LIST();
        //                msgOpenWhiteList.open = true;
        //                GateServerManager.Broadcast(msgOpenWhiteList);
        //                break;
        //            case "closewhitelist":
        //                MSG_GGate_WHITE_LIST msgCloseWhiteList = new MSG_GGate_WHITE_LIST();
        //                msgCloseWhiteList.open = false;
        //                GateServerManager.Broadcast(msgCloseWhiteList);
        //                break;
        //            case "reloadblacklist":
        //                MSG_GB_BLACKLIST_RELOAD msgReloadBlack = new MSG_GB_BLACKLIST_RELOAD();
        //                BarrackServerManager.Broadcast(msgReloadBlack);
        //                break;
        //            case "rechargetest":
        //                {
        //                    help = @"rechargetest Usage: rechargetest pcUid rechargeType; eg. rechargetest 105 1 ";
        //                    int pcUid;
        //                    int rechargeType;
        //                    if (cmdArr.Length == 3)
        //                    {
        //                        if (int.TryParse(cmdArr[1], out pcUid) == false ||
        //                            int.TryParse(cmdArr[2], out rechargeType) == false)
        //                        {
        //                            Log.Warn(help);
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }

        //                    FrontendServer mServer = ManagerServerManager.GetOneServer();
        //                    if (mServer == null || mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("rechargetest failed: WatchDog Manager server not ready");
        //                        return;
        //                    }
        //                    MSG_GM_ECHARGE_TEST msg = new MSG_GM_ECHARGE_TEST();
        //                    msg.Uid = pcUid;
        //                    msg.rechargeType = rechargeType;
        //                    mServer.Write(msg);
        //                }
        //                break;
        //            case "reloadfamily":
        //                {
        //                    help = @"reloadfamily Usage: reloadfamliy mainId familiyId";
        //                    int mainId;
        //                    int familyId;
        //                    if (cmdArr.Length == 3)
        //                    {
        //                        if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out familyId) == false)
        //                        {
        //                            Log.Warn(help);
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Log.Warn(help);
        //                        return;
        //                    }
        //                    FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                    if (mServer == null || mServer.State != ServerState.Started)
        //                    {
        //                        Log.Warn("reloadfamily failed: Manager server {0} not ready", mainId);
        //                        return;
        //                    }
        //                    MSG_GM_RELOAD_FAMILY msg = new MSG_GM_RELOAD_FAMILY();
        //                    msg.MainId = mainId;
        //                    msg.FamilyId = familyId;
        //                    mServer.Write(msg);
        //                }
        //                break;
        //            case "waitcount":
        //                help = @"waitcount Usage: waitcount number";
        //                int waitCount;
        //                if (cmdArr.Length == 2)
        //                {
        //                    if (int.TryParse(cmdArr[1], out waitCount) == false)
        //                    {
        //                        return;
        //                    }
        //                }
        //                else
        //                {
        //                    Log.Warn(help);
        //                    return;
        //                }
        //                MSG_GGate_WAIT_COUNT msgWaitCount = new MSG_GGate_WAIT_COUNT();
        //                msgWaitCount.count = waitCount;
        //                GateServerManager.Broadcast(msgWaitCount);
        //                break;
        //            case "fullcount":
        //                FullCountCommand(cmdArr);
        //                break;
        //            case "waitsec":
        //                WaitsecCommand(cmdArr);
        //                break;
        //            case "setfps":
        //                SetFpsCommand(cmdArr);
        //                break;
        //            case "stopfighting":
        //                StopFightingCommand(cmdArr);
        //                break;
        //            case "addbuff":
        //                AddBuffCommand(cmdArr);
        //                break;
        //            case "buzyonlinecount":
        //                BusyonlineCountCommand(cmdArr);
        //                break;
        //            case "shutdownall":
        //                ShutdownAll();
        //                break;
        //            case "help":
        //                HelpCommand();
        //                break;
        //            default:
        //                Log.Warn("command {0} not support, try command 'help' for more infomation", cmd);
        //                break;
        //        }
        //    }

        //    private void FullCountCommand(string[] cmdArr)
        //    {
        //        string help = @"fullcount Usage: fullcount number";

        //        int fullCount;
        //        if (cmdArr.Length == 2)
        //        {
        //            if (int.TryParse(cmdArr[1], out fullCount) == false)
        //            {
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Log.Warn(help);
        //            return;
        //        }

        //        MSG_GGate_FULL_COUNT msg = new MSG_GGate_FULL_COUNT();
        //        msg.Count = fullCount;
        //        GateServerManager.Broadcast(msg);
        //    }

        //    private void WaitsecCommand(string[] cmdArr)
        //    {
        //        string help = @"waitsec Usage: waitsec number";
        //        int waitSec;
        //        if (cmdArr.Length == 2)
        //        {
        //            if (int.TryParse(cmdArr[1], out waitSec) == false)
        //            {
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        MSG_GGate_WAIT_SEC msg = new MSG_GGate_WAIT_SEC();
        //        msg.second = waitSec;
        //        GateServerManager.Broadcast(msg);
        //    }

        //    private void SetFpsCommand(string[] cmdArr)
        //    {
        //        string help = @"setFps Usage: FPS servername（just like：zone、battle、gate） mainId/gateId subId";
        //        string servername = "";

        //        int FPS;
        //        if (int.TryParse(cmdArr[1], out FPS))
        //        {
        //            if (cmdArr.Length > 2)
        //            {
        //                servername = cmdArr[2];

        //                switch (servername)
        //                {
        //                    case "zone":
        //                        {
        //                            int nMainId;
        //                            int nSubId;
        //                            MSG_GM_SET_ZONE_FPS msg2zone = new MSG_GM_SET_ZONE_FPS();
        //                            msg2zone.Fps = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out nMainId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                msg2zone.mainId = nMainId;
        //                            }
        //                            if (!int.TryParse(cmdArr[4], out nSubId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                msg2zone.subId = nSubId;
        //                            }

        //                            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                            if (mServer == null || mServer.State != ServerState.Started)
        //                            {
        //                                Log.Warn("set zone fps failed: manager server not ready");
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                mServer.Write(msg2zone);
        //                            }
        //                        }
        //                        break;
        //                    case "battle":
        //                        {
        //                            int nMainId;
        //                            int nSubId;
        //                            MSG_GBM_SET_BATTLE_FPS msg2battle = new MSG_GBM_SET_BATTLE_FPS();
        //                            msg2battle.Fps = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out nMainId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                msg2battle.mainId = nMainId;
        //                            }
        //                            if (!int.TryParse(cmdArr[4], out nSubId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                msg2battle.subId = nSubId;
        //                            }

        //                            if (BattleManagerServer == null || BattleManagerServer.State != ServerState.Started)
        //                            {
        //                                Log.Warn("setfps battle failed: battle manager server not ready");
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                BattleManagerServer.Write(msg2battle);
        //                            }
        //                        }
        //                        break;
        //                    case "gate":
        //                        {
        //                            int nMainId;
        //                            int nGateId;
        //                            MSG_GGATE_SET_FPS msg = new MSG_GGATE_SET_FPS();
        //                            msg.FPS = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out nMainId))
        //                            {
        //                                return;
        //                            }

        //                            if (!int.TryParse(cmdArr[4], out nGateId))
        //                            {
        //                                return;
        //                            }
        //                            SetGateFps(nMainId, nGateId, FPS);
        //                        }
        //                        break;

        //                    case "manager":
        //                        {
        //                            int nMainId;
        //                            MSG_GM_SET_FPS msg = new MSG_GM_SET_FPS();
        //                            msg.FPS = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out nMainId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {

        //                            }

        //                            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                            if (mServer == null || mServer.State != ServerState.Started)
        //                            {
        //                                Log.Warn("set manager fps failed: manager server not ready");
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                mServer.Write(msg);
        //                            }
        //                        }
        //                        break;
        //                    case "battlemanager":
        //                        {
        //                            int nMainId;
        //                            MSG_GBM_SET_FPS msg = new MSG_GBM_SET_FPS();
        //                            msg.FPS = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out nMainId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {

        //                            }

        //                            if (BattleManagerServer == null || BattleManagerServer.State != ServerState.Started)
        //                            {
        //                                Log.Warn("set manager fps failed: manager server not ready");
        //                                return;
        //                            }
        //                            else
        //                            {
        //                                BattleManagerServer.Write(msg);
        //                            }
        //                        }
        //                        break;
        //                    case "barrack":
        //                        {
        //                            int barrackId;
        //                            MSG_GB_SET_FPS msg = new MSG_GB_SET_FPS();
        //                            msg.FPS = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out barrackId))
        //                            {
        //                                return;
        //                            }

        //                            BarrackServerManager.Broadcast(msg);
        //                        }
        //                        break;
        //                    case "relation":
        //                        {
        //                            int relationMainId;
        //                            MSG_GM_SET_Relation_FPS msg = new MSG_GM_SET_Relation_FPS();
        //                            msg.Fps = (double)FPS;

        //                            if (!int.TryParse(cmdArr[3], out relationMainId))
        //                            {
        //                                return;
        //                            }
        //                            else
        //                            {

        //                            }

        //                            if (relationMainId != 0)
        //                            {
        //                                FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //                                if (mServer == null || mServer.State != ServerState.Started)
        //                                {
        //                                    Log.Warn("setfps failed: relation server {0} not exist", relationMainId);
        //                                    return;
        //                                }
        //                                mServer.Write(msg);
        //                            }
        //                            else
        //                            {
        //                                ManagerServerManager.Broadcast(msg);
        //                            }
        //                        }
        //                        break;
        //                }
        //            }
        //            else
        //            {
        //                Log.Warn(help);
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //    }

        //    private void StopFightingCommand(string[] cmdArr)
        //    {
        //        int battleResult = 1;
        //        int.TryParse(cmdArr[1], out battleResult);
        //        MSG_GBattle_STOP_FIGHTING battleResultMsg = new MSG_GBattle_STOP_FIGHTING();
        //        battleResultMsg.result = battleResult;
        //        BattleServerManager.Broadcast(battleResultMsg);
        //    }

        //    private void AddBuffCommand(string[] cmdArr)
        //    {
        //        int dstUid = 1;
        //        int buffId = 2;
        //        string paramString = "";
        //        if (cmdArr.Length >= 3)
        //        {
        //            if (int.TryParse(cmdArr[1], out dstUid) == false)
        //            {
        //                return;
        //            }
        //            if (int.TryParse(cmdArr[2], out buffId) == false)
        //            {
        //                return;
        //            }
        //            if (cmdArr.Length == 4)
        //            {
        //                paramString = cmdArr[3].ToString();
        //            }
        //            MSG_GBattle_ADD_BUFF addBuffMsg = new MSG_GBattle_ADD_BUFF();
        //            addBuffMsg.uid = dstUid;
        //            addBuffMsg.buffId = buffId;
        //            addBuffMsg.paramString = paramString;
        //            BattleServerManager.Broadcast(addBuffMsg);
        //        }
        //        else
        //        {
        //            Log.Warn("");
        //            return;
        //        }
        //    }

        //    private void BusyonlineCountCommand(string[] cmdArr)
        //    {
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn("buzyonlinecount usage: buzyonlinecount count");
        //            return;
        //        }
        //        int onlineCount = 1;
        //        int.TryParse(cmdArr[1], out onlineCount);
        //        MSG_GM_BUZY_ONLINE_COUNT buzyonlineCountMsg = new MSG_GM_BUZY_ONLINE_COUNT();
        //        buzyonlineCountMsg.count = onlineCount;
        //        ManagerServerManager.Broadcast(buzyonlineCountMsg);
        //    }

        //    private void ShutdownZone(string[] cmdArr)
        //    {
        //        string help = "shutdownzone Usage: shutdownzone mainId subId; if subId == 0, will close all zone in mainId";
        //        if (cmdArr.Length != 3)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int mainId;
        //        int subId;
        //        if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        if (mainId != 0)
        //        {
        //            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //            if (mServer == null || mServer.State != ServerState.Started)
        //            {
        //                Log.Warn("shutdown failed: Manager server {0} not ready", mainId);
        //                return;
        //            }
        //            MSG_GM_SHUTDOWN_ZONE msgShutdonwZone = new MSG_GM_SHUTDOWN_ZONE();
        //            msgShutdonwZone.mainId = mainId;
        //            msgShutdonwZone.subId = subId;
        //            mServer.Write(msgShutdonwZone);
        //        }
        //        else
        //        {
        //            foreach (var item in ManagerServerManager.ServerList.Values)
        //            {
        //                ManagerServer manager = (ManagerServer)item;
        //                MSG_GM_SHUTDOWN_ZONE msgShutdonwZone = new MSG_GM_SHUTDOWN_ZONE();
        //                msgShutdonwZone.mainId = manager.MainId;
        //                msgShutdonwZone.subId = subId;
        //                manager.Write(msgShutdonwZone);
        //            }
        //        }
        //    }

        //    private void ShutdownBarrack(string[] cmdArr)
        //    {
        //        //string help = "shutdownbarrack Usage: shutdownbarrack";
        //        MSG_GB_SHUTDOWN msgShutdownBarrack = new MSG_GB_SHUTDOWN();
        //        BarrackServerManager.Broadcast(msgShutdownBarrack);
        //    }

        //    private void ShutdownRelation(string[] cmdArr)
        //    {
        //        string help = "shutdownrelation Usage: shutdownrelation main; if main == 0, will close all relations";
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int relationMainId;
        //        if (int.TryParse(cmdArr[1], out relationMainId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        MSG_GM_SHUTDOWN_RELATION msgShutdownRelation = new MSG_GM_SHUTDOWN_RELATION();
        //        if (relationMainId != 0)
        //        {
        //            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(relationMainId);
        //            if (mServer == null || mServer.State != ServerState.Started)
        //            {
        //                Log.Warn("shutdown failed: relation server {0} not exist", relationMainId);
        //                return;
        //            }
        //            mServer.Write(msgShutdownRelation);
        //        }
        //        else
        //        {
        //            ManagerServerManager.Broadcast(msgShutdownRelation);
        //        }
        //    }
        //    private void ShutdownCountry(string[] cmdArr)
        //    {
        //        string help = "shutdowncountry Usage: shutdowncountry mainId; if manId == 0, will close all countrys";
        //        int managerMainId = 0;
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        if (int.TryParse(cmdArr[1], out managerMainId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        MSG_GM_SHUTDOWN_MAIN msgShutdownMain = new MSG_GM_SHUTDOWN_MAIN();
        //        if (managerMainId != 0)
        //        {
        //            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
        //            if (mServer == null || mServer.State != ServerState.Started)
        //            {
        //                Log.Warn("shutdown failed: barrack server {0} not exist", managerMainId);
        //                return;
        //            }
        //            mServer.Write(msgShutdownMain);
        //        }
        //        else
        //        {
        //            ManagerServerManager.Broadcast(msgShutdownMain);
        //        }
        //    }
        //    private void ShutdownBattleManager(string[] cmdArr)
        //    {
        //        string help = "shutdownbattlemanager Usage: shutdownbattlemanager;  close battle manager server";
        //        if (cmdArr.Length != 1)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }

        //        if (BattleManagerServer == null)
        //        {
        //            Log.Warn("shutdown failed: battle manager not exist");
        //            return;
        //        }
        //        else
        //        {
        //            MSG_GBM_SHUTDOWN_BATTLEMANAGER msgShutdownBM = new MSG_GBM_SHUTDOWN_BATTLEMANAGER();
        //            BattleManagerServer.Write(msgShutdownBM);
        //        }

        //    }
        //    private void ShutdownBattle(string[] cmdArr)
        //    {
        //        string help = "shutdownbattle Usage: shutdownbattle battleId;if battleId == 0, will close all battles";
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int subId;
        //        if (int.TryParse(cmdArr[1], out subId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        if (BattleManagerServer == null)
        //        {
        //            Log.Warn("shutdown failed: battle manager server not ready");
        //            return;
        //        }
        //        else
        //        {
        //            MSG_GBM_SHUTDOWN_BATTLE msgShutdonwBattle = new MSG_GBM_SHUTDOWN_BATTLE();
        //            msgShutdonwBattle.subId = subId;
        //            BattleManagerServer.Write(msgShutdonwBattle);
        //        }
        //    }
        //    private void ShutdownBattleDirectly(string[] cmdArr)
        //    {
        //        string help = "shutdownbattle1 Usage: shutdownbattle battleId; if battleId == 0, will close all battles";
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int subId;
        //        if (int.TryParse(cmdArr[1], out subId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        MSG_GBattle_SHUTDOWN_Battle msg = new MSG_GBattle_SHUTDOWN_Battle();
        //        if (BattleServerManager == null)
        //        {
        //            Log.Warn(String.Format("shutdown battle subId {1} failed: find battle manager failed", subId));
        //        }
        //        if (subId == 0)
        //        {
        //            BattleServerManager.Broadcast(msg);
        //        }
        //        else
        //        {
        //            FrontendServer battleServer = BattleServerManager.GetServer(MainId, subId);
        //            if (battleServer == null || battleServer.State != ServerState.Started)
        //            {
        //                Log.Warn("shutdown battle failed: battle server not ready");
        //                return;
        //            }
        //            else
        //            {
        //                battleServer.Write(msg);
        //            }
        //        }
        //    }
        //    private void ShutdownZoneDirectly(string[] cmdArr)
        //    {
        //        string help = "shutdownbattle1 Usage: shutdownbattle mainId subId; ; if subId == 0, will close all battle in mainId";
        //        if (cmdArr.Length != 3)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int mainId;
        //        int subId;
        //        if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        MSG_GZ_SHUTDOWN_ZONE msg = new MSG_GZ_SHUTDOWN_ZONE();
        //        if (ZoneServerManager == null)
        //        {
        //            Log.Warn(String.Format("shutdown zone mainId{0} subId{1} failed: find zone manager failed", mainId, subId));
        //        }
        //        if (subId == 0)
        //        {
        //            ZoneServerManager.Broadcast(msg);
        //        }
        //        else
        //        {
        //            FrontendServer zoneServer = ZoneServerManager.GetServer(mainId, subId);
        //            if (zoneServer == null || zoneServer.State != ServerState.Started)
        //            {
        //                Log.Warn("shutdown zone failed: zone server not ready");
        //                return;
        //            }
        //            else
        //            {
        //                zoneServer.Write(msg);
        //            }
        //        }
        //    }
        //    private void ShutdownRelationDerectly(string[] cmdArr)
        //    {
        //        string help = "shutdownrelation1 Usage: shutdownrelation1 main; if main == 0, will close all relations";
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int relationMainId;
        //        if (int.TryParse(cmdArr[1], out relationMainId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }

        //        MSG_GR_SHUTDOWN msg = new MSG_GR_SHUTDOWN();
        //        if (RelationServerManager == null)
        //        {
        //            Log.Warn(String.Format("shutdown relation mainId {0} failed: find relation manager failed", relationMainId));
        //        }

        //        FrontendServer relationServer = RelationServerManager.GetSinglePointServer(relationMainId);
        //        if (relationServer == null || relationServer.State != ServerState.Started)
        //        {
        //            Log.Warn("shutdown relation failed: relation server not ready");
        //            return;
        //        }
        //        else
        //        {
        //            relationServer.Write(msg);
        //        }
        //    }
        //    private void ShutdownGate(string[] cmdArr)
        //    {
        //        string help = "shutdowngate Usage: shutdowngate gateId; if gateId == 0, will close all gate in mainId";
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int subId;
        //        if (int.TryParse(cmdArr[1], out subId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        MSG_GB_SHUTDOWN_GATE notify = new MSG_GB_SHUTDOWN_GATE();
        //        BarrackServerManager.Broadcast(notify);
        //    }
        //    private void ShutdownGateDirectly(string[] cmdArr)
        //    {
        //        string help = "shutdowngate1 Usage: shutdowngate1 gateId; if gateId == 0, will close all gate in mainId";
        //        if (cmdArr.Length != 2)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }
        //        int subId;
        //        if (int.TryParse(cmdArr[1], out subId) == false)
        //        {
        //            Log.Warn(help);
        //            return;
        //        }

        //        MSG_GGATE_SHUTDOWN_GATE msg = new MSG_GGATE_SHUTDOWN_GATE();
        //        if (subId == 0)
        //        {
        //            GateServerManager.Broadcast(msg);
        //        }
        //        else
        //        {
        //            FrontendServer gateServer = GateServerManager.GetServer(mainId, subId);
        //            if (gateServer == null || gateServer.State != ServerState.Started)
        //            {
        //                return;
        //            }
        //            else
        //            {
        //                gateServer.Write(msg);
        //            }
        //        }
        //    }
        //    private void ShutdownChatManager(string[] cmdArr)
        //    {
        //        if (ChatManagerServer != null)
        //        {
        //            MSG_GCM_SHUTDOWN shutdownChatManager = new MSG_GCM_SHUTDOWN();
        //            ChatManagerServer.Write(shutdownChatManager);
        //        }
        //    }
        //    private void ShutdownAll()
        //    {
        //        // step 1 gate
        //        MSG_GB_SHUTDOWN_GATE shutdownGate = new MSG_GB_SHUTDOWN_GATE();
        //        BarrackServerManager.Broadcast(shutdownGate);
        //        // step 2 barrack
        //        MSG_GB_SHUTDOWN shutdownBarrack = new MSG_GB_SHUTDOWN();
        //        BarrackServerManager.Broadcast(shutdownBarrack);
        //        // step 3 battle
        //        if (BattleManagerServer != null)
        //        {
        //            MSG_GBM_SHUTDOWN_BATTLE shutdonwBattle = new MSG_GBM_SHUTDOWN_BATTLE();
        //            shutdonwBattle.subId = 0;
        //            BattleManagerServer.Write(shutdonwBattle);
        //            // step 4 battle manager
        //            MSG_GBM_SHUTDOWN_BATTLEMANAGER shutdownBM = new MSG_GBM_SHUTDOWN_BATTLEMANAGER();
        //            BattleManagerServer.Write(shutdownBM);
        //        }
        //        // step 5 country
        //        MSG_GM_SHUTDOWN_MAIN shutdownMain = new MSG_GM_SHUTDOWN_MAIN();
        //        ManagerServerManager.Broadcast(shutdownMain);
        //        // step 6 chat manager
        //        if (ChatManagerServer != null)
        //        {
        //            MSG_GCM_SHUTDOWN shutdownChatManager = new MSG_GCM_SHUTDOWN();
        //            ChatManagerServer.Write(shutdownChatManager);
        //        }
        //    }
        //    private void RequestGateInfo(int mainId, int subId)
        //    {
        //        MSG_GGate_GATE_FPS_INFO msg = new MSG_GGate_GATE_FPS_INFO();
        //        if (subId == 0)
        //        {
        //            GateServerManager.Broadcast(msg);
        //        }
        //        else
        //        {
        //            FrontendServer gateServer = GateServerManager.GetServer(mainId, subId);
        //            if (gateServer == null || gateServer.State != ServerState.Started)
        //            {
        //                Log.Warn("gateinfo failed: {0} not ready", gateServer.ServerName);
        //                return;
        //            }
        //            else
        //            {
        //                gateServer.Write(msg);
        //            }
        //        }
        //    }
        //    private void SetGateFps(int mainId, int subId, int fps)
        //    {
        //        MSG_GGATE_SET_FPS msg = new MSG_GGATE_SET_FPS();
        //        msg.FPS = (double)fps;
        //        if (subId == 0)
        //        {
        //            GateServerManager.Broadcast(msg);
        //        }
        //        else
        //        {
        //            FrontendServer gateServer = GateServerManager.GetServer(mainId, subId);
        //            if (gateServer == null || gateServer.State != ServerState.Started)
        //            {
        //                return;
        //            }
        //            else
        //            {
        //                gateServer.Write(msg);
        //            }
        //        }
        //    }

        //    private void HelpCommand()
        //    {
        //        Log.Info("mapinfo Usage: mapinfo mainId mapId, will show the map detail info in the mainId logic server group");
        //        Log.Info("chatinfo Usage: chatinfo type(world or nearby), will show the type of chatrooms info");
        //        Log.Info("zoneinfo Usage: zoneinfo mainId subId, will show the zone detail info in the mainId logic server group");
        //        Log.Info("allzoneinfo Usage: AllZoneInfo mainId, will show all zone details info in the mainId logic server group");
        //        Log.Info("battleinfo Usage: battleinfo mainId subId, will show the battle server detail");
        //        Log.Info("allbattleinfo Usage: just input allbattleinfo, will show all the battle servers' detail");
        //        Log.Info("gateinfo Usage: gateinfo mainId subId. will show the gate detail");
        //        Log.Info("allgateinfo Usage: just input allgateinfo, will show all the gate servers detail");
        //        Log.Info("relationinfo Usage: relationinfo mainId, will show the relation detail in the mainId logic server group");
        //        Log.Info("managerinfo Usage: managerinfo mainId, will show the manager detail in the mainId logic server group");
        //        Log.Info("barrackinfo Usage: just input barrackinfo, will show the barrack server detail");
        //        Log.Info("battlemanagerinfo Usage: just input battlemanagerinfo, will show the battle manager server detail");
        //        Log.Info("kick Usage: kick mainId uid");
        //        Log.Info("freeze Usage: freeze mainId uid freezeType hour");
        //        Log.Info("sendemail Usage: sendemail mainId emailId (saveTime)");
        //        Log.Info("updatexml Usage: just input updatexml, will make all servers reload xml or updatexml manager/zone/battle/gate/barrack...serverType to make spec type server reload xml");
        //        Log.Info("updateranklist Usage: just input updateranklist, will reload rank");
        //        Log.Info("shutdownzone Usage: shutdownzone mainId subId, will close the spec zone through manager");
        //        Log.Info("shutdownzone1 Usage: shutdownzone1 mainId subId, will close the spec zone directly");
        //        Log.Info("shutdownbarrack Usage: just input shutdownbarrack, will close barrack server");
        //        Log.Info("shutdownrelation Usage: shutdownrelation mainId, will close the spec relation server through its manager");
        //        Log.Info("shutdownrelation1 Usage: shutdownrelation1 mainId, will close the spec relation server directly");
        //        Log.Info("shutdowncountry Usage: shutdowncountry mainId, will close the mainId logic server group");
        //        Log.Info("shutdownbattlemanager Usage: just input shutdownbattlemanager, will close battle maanger server");
        //        Log.Info("shutdownbattle Usage: shutdownbattle mainId subId, will close the spec battle through battlemanager");
        //        Log.Info("shutdownbattle1 Usage: shutdownbattle mainId subId, will close the spec battle directly");
        //        Log.Info("shutdowngate Usage: shutdowngate gateId, will close the spec gate through barrack");
        //        Log.Info("shutdowngate1 Usage: shutdowngate1 gateId, will close the spec gate directly");
        //        Log.Info("shutdownchatmanager Usage: just input shutdownchatmanager, will close the chat manager server");
        //        Log.Info("openwhitelist Usage: just input 'openwhitelist', client which ip in white list can log in");
        //        Log.Info("closewhitelist Usage: just input 'closewhitelist', client no matter whether ip in white list can log in");
        //        Log.Info("setFps Usage: FPS servername（just like：zone、battle、gate） mainId/gateId subId fps, will set the server fps");
        //        Log.Info("reloadblacklist Usage: just input 'closewhitelist', barrack will reload black list from db");
        //        Log.Info("stopfighting Usage: stopfighting 1, will stop all battle and the attacker will win");
        //        Log.Info("buzyonlinecount Usage: buzyonlinecount num, set ballance online count for every logic server group");
        //        Log.Info("fullcount Usage: fullcount number, if the number of players in game is greater than fullcount, no one can login");
        //        Log.Info("waitcount Usage: waitcount number, if the number of players in game is greater than waitcount, new client has to wait");
        //        Log.Info("waitsec Usage: waitcount number, if the number of players in game is greater than waitcount, client has to wait spec seconds one by one");
        //        Log.Info("Still Comfused ? Contact To Hansome TrailMa! Tel:15942487320");
        //    }
        #endregion

    }
}