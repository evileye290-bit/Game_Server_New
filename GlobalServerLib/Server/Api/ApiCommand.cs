using EnumerateUtility;
using Logger;
using Message.Global.Protocol.GA;
using Message.Global.Protocol.GB;
using Message.Global.Protocol.GBattle;
using Message.Global.Protocol.GBM;
using Message.Global.Protocol.GCross;
using Message.Global.Protocol.GGate;
using Message.Global.Protocol.GM;
using Message.Global.Protocol.GR;
using Message.Global.Protocol.GZ;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using Message.Global.Protocol.GP;

namespace GlobalServerLib
{
    partial class GlobalServerApi
    {
        private delegate bool CommandResponser(string[] cmdArr);

        Dictionary<string, CommandResponser> commandResponsers = new Dictionary<string, CommandResponser>();

        private void AddCommand(string cmdflag, CommandResponser responser)
        {
            commandResponsers.Add(cmdflag, responser);
        }

        private void BindCommond()
        {
            AddCommand("help", HelpCommand);

            AddCommand("setfps", SetFpsCommand);

            //AddCommand("chatinfo", RequestChatInfoCommand);
            AddCommand("mapinfo", RequestMapInfoCommand);

            AddCommand("zoneinfo", RequstZoneInfoCommand);
            AddCommand("allzoneinfo", RequstAllZoneInfoCommand);

            AddCommand("battleinfo", RequstBattleInfoCommand);
            AddCommand("allbattleinfo", RequstAllBattleInfoCommand);

            AddCommand("gateinfo", RequstGateInfoCommand);
            AddCommand("allgateinfo", RequstAllGateInfoCommand);

            AddCommand("barrackinfo", RequestBarrackInfoCommand);
            AddCommand("allbarrackinfo", RequestAllBarrackInfoCommand);

            AddCommand("payinfo", RequestPayInfoCommand);
            AddCommand("allpayinfo", RequestAllPayInfoCommand);

            AddCommand("relationinfo", RequstRelationInfoCommand);
            AddCommand("managerinfo", RequstManagerInfoCommand);
            AddCommand("battlemanagerinfo", RequstBattleManagerInfoCommand);


            AddCommand("shutdownpay", ShutdownPayCommand);
            AddCommand("shutdownbarrack", ShutdownBarrackCommand);

            AddCommand("shutdowncountry", ShutdownCountryCommand);

            AddCommand("shutdownbattlemanager", ShutdownBattleManagerCommand);

            //AddCommand("shutdownchatmanager", ShutdownChatManagerCommand);

            AddCommand("shutdownzone", ShutdownZoneCommand);
            AddCommand("shutdownzone1", ShutdownZoneDirectlyCommand);

            AddCommand("shutdownrelation", ShutdownRelationCommand);
            AddCommand("shutdownrelation1", ShutdownBattleDirectlyCommand);

            AddCommand("shutdowngate", ShutdownGateCommand);
            AddCommand("shutdowngate1", ShutdownGateDirectlyCommand);

            AddCommand("shutdownbattle", ShutdownBattleCommand);
            AddCommand("shutdownbattle1", ShutdownBattleDirectlyCommand);

            AddCommand("shutdownall", ShutdownAllCommand);


            AddCommand("kick", KickCommand);
            AddCommand("freeze", FreezeCommand);
            AddCommand("sendemail", SendEmailCommand);
            AddCommand("announcement", AnnouncementCommand);
            AddCommand("updatexml", UpdateXmlCommand);
            AddCommand("updateserverxml", UpdateServerXmlCommand);
            AddCommand("updatewlfare", UpdateWlfareCommand);
            AddCommand("updatranklist", UpdateRankListCommand);

            AddCommand("openwhitelist", OpenWhiteListCommand);
            AddCommand("closewhitelist", CloseWhiteListCommand);
            AddCommand("reloadblacklist", ReloadBlackListCommand);

            AddCommand("rechargetest", RechargeTestCommand);
            AddCommand("reloadfamily", ReloadFamilyCommand);

            AddCommand("waitcount", WaitCountCommand);
            AddCommand("fullcount", FullCountCommand);
            AddCommand("buzyonlinecount", BusyonlineCountCommand);

            AddCommand("waitsec", WaitsecCommand);
            AddCommand("stopfighting", StopFightingCommand);
            AddCommand("addbuff", AddBuffCommand);
            AddCommand("addreward", AddRewardCommand);
            AddCommand("equiphero", EquipHeroCommand);
            AddCommand("allchat", AllChatCommand);
            AddCommand("absorbsoul", AbsorbSoulRingCommand);
            AddCommand("absorbfinish", AbsorbFinishCommand);
            AddCommand("addheroexp", AddHeroExpCommand);
            AddCommand("heroawaken", HeroAwakenCommand);
            AddCommand("herolevelup", HeroLevelUpCommand);
            AddCommand("updateheropos", UpdateHeroPosCommand);
            AddCommand("zonetrans", ZoneTransform);
            AddCommand("giftopen", TriggerGiftOpen);

            AddCommand("newserver", NewServers);
            AddCommand("lineupserver", LineupServers);
            AddCommand("recommendserver", RecommendServers);
            AddCommand("loginlogout", LoginOutInfo);

            AddCommand("mergeserver", MergeServerReward);

            AddCommand("updatepayglobalinfo", UpdatePayGlobalInfo);
        }

        public void InitCommand()
        {
            BindCommond();
        }

        private void ExcuteCommand(string cmd)
        {
            string[] cmdArr = cmd.Split(' ');
            if (cmdArr.Length == 0)
            {
                return;
            }
            CommandResponser responser;
            if (commandResponsers.TryGetValue(cmdArr[0], out responser))
            {
                responser(cmdArr);
            }
            else
            {
                Log.Warn("command {0} not support, try command 'help' for more infomation", cmd);
            }
        }

        private bool HelpCommand(string[] cmdArr)
        {
            Log.Info("mapinfo Usage: mapinfo mainId mapId, will show the map detail info in the mainId logic server group");
            Log.Info("chatinfo Usage: chatinfo type(world or nearby), will show the type of chatrooms info");
            Log.Info("zoneinfo Usage: zoneinfo mainId subId, will show the zone detail info in the mainId logic server group");
            Log.Info("allzoneinfo Usage: AllZoneInfo mainId, will show all zone details info in the mainId logic server group");
            Log.Info("battleinfo Usage: battleinfo mainId subId, will show the battle server detail");
            Log.Info("allbattleinfo Usage: just input allbattleinfo, will show all the battle servers detail");
            Log.Info("gateinfo Usage: gateinfo mainId subId. will show the gate detail");
            Log.Info("allgateinfo Usage: just input allgateinfo, will show all the gate servers detail");
            Log.Info("relationinfo Usage: relationinfo mainId, will show the relation detail in the mainId logic server group");
            Log.Info("managerinfo Usage: managerinfo mainId, will show the manager detail in the mainId logic server group");
            Log.Info("barrackinfo Usage: barrackinfo mainId  subId, will show the barrack server detail");
            Log.Info("allbarrackinfo Usage: just input allbarrackinfo, will show the barrack servers detail");
            Log.Info("battlemanagerinfo Usage: just input battlemanagerinfo, will show the battle manager server detail");
            Log.Info("kick Usage: kick mainId uid");
            Log.Info("freeze Usage: freeze mainId uid freezeType hour");
            Log.Info("sendemail Usage: sendemail mainId emailId (saveTime)");
            Log.Info("updatexml Usage: just input updatexml, will make all servers reload xml or updatexml manager/zone/battle/gate/barrack...serverType to make spec type server reload xml");
            Log.Info("updateranklist Usage: just input updateranklist, will reload rank");
            Log.Info("shutdownzone Usage: shutdownzone mainId subId, will close the spec zone through manager");
            Log.Info("shutdownzone1 Usage: shutdownzone1 mainId subId, will close the spec zone directly");
            Log.Info("shutdownbarrack Usage: just input shutdownbarrack, will close barrack server");
            Log.Info("shutdownpay Usage: just input shutdownpay, will close barrack server");
            Log.Info("shutdownrelation Usage: shutdownrelation mainId, will close the spec relation server through its manager");
            Log.Info("shutdownrelation1 Usage: shutdownrelation1 mainId, will close the spec relation server directly");
            Log.Info("shutdowncountry Usage: shutdowncountry mainId, will close the mainId logic server group");
            Log.Info("shutdownbattlemanager Usage: just input shutdownbattlemanager, will close battle maanger server");
            Log.Info("shutdownbattle Usage: shutdownbattle mainId subId, will close the spec battle through battlemanager");
            Log.Info("shutdownbattle1 Usage: shutdownbattle mainId subId, will close the spec battle directly");
            Log.Info("shutdowngate Usage: shutdowngate gateId, will close the spec gate through barrack");
            Log.Info("shutdowngate1 Usage: shutdowngate1 gateId, will close the spec gate directly");
            Log.Info("shutdownchatmanager Usage: just input shutdownchatmanager, will close the chat manager server");
            Log.Info("openwhitelist Usage: just input 'openwhitelist', client which ip in white list can log in");
            Log.Info("closewhitelist Usage: just input 'closewhitelist', client no matter whether ip in white list can log in");
            Log.Info("setfps Usage 1: setfps fps servername mainId subId, will set the server fps");
            Log.Info("setfps Usage 2: setfps fps servername mainId/subId, will set the server fps");
            Log.Info("reloadblacklist Usage: just input 'closewhitelist', barrack will reload black list from db");
            Log.Info("stopfighting Usage: stopfighting 1, will stop all battle and the attacker will win");
            Log.Info("buzyonlinecount Usage: buzyonlinecount num, set ballance online count for every logic server group");
            Log.Info("fullcount Usage: fullcount number, if the number of players in game is greater than fullcount, no one can login");
            Log.Info("waitcount Usage: waitcount number, if the number of players in game is greater than waitcount, new client has to wait");
            Log.Info("waitsec Usage: waitcount number, if the number of players in game is greater than waitcount, client has to wait spec seconds one by one");
            Log.Info("Still Comfused ? Contact To Hansome TrailMa! Tel:15942487320");
            Log.Info("zonetrans Usage: zonetrans mainid isForce fromZones toZOnes, eg:1001 false 1-2-3 4-5-6");
            Log.Info("newserver Usage: newserver serverId1|serverId2|serverId3");
            Log.Info("lineupserver Usage: lineupserver serverId1|serverId2|serverId3");
            Log.Info("recommendserver Usage: recommendserver serverId1|serverId2|serverId3");
            Log.Info("mergeserver Usage: mergeserver serverIdStart|serverIdEnd");
            Log.Info("payinfo Usage: payinfo mainId  subId, will show the barrack server detail");
            Log.Info("allpayinfo Usage: just input allpayinfo, will show the barrack servers detail");
            Log.Info("updatepayglobalinfo Usage: updatepayglobalinfo, update pay server connect global which not connect");
            
            return true;
        }

        private bool SetGateFps(double fps, int mainId, int subId)
        {
            MSG_GGATE_SET_FPS msg = new MSG_GGATE_SET_FPS();
            msg.FPS = fps;

            if (mainId == 0)
            {
                GateServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer gateServer = GateServerManager.GetServer(mainId, subId);
                if (gateServer == null || gateServer.State != ServerState.Started)
                {
                    return false;
                }
                else
                {
                    gateServer.Write(msg);
                }
            }
            return true;
        }

        private bool SetZoneFps(double fps, int mainId, int subId)
        {
            MSG_GM_SET_ZONE_FPS msg = new MSG_GM_SET_ZONE_FPS();
            msg.Fps = fps;
            msg.MainId = mainId;
            msg.SubId = subId;

            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("set zone fps failed: manager server not ready");
                return false;
            }
            else
            {
                mServer.Write(msg);
            }

            return true;
        }

        private bool SetBattleFps(double fps, int mainId, int subId)
        {
            MSG_GBM_SET_BATTLE_FPS msg = new MSG_GBM_SET_BATTLE_FPS();
            msg.Fps = fps;
            msg.MainId = mainId;
            msg.SubId = subId;

            if (BattleManagerServer == null || BattleManagerServer.State != ServerState.Started)
            {
                Log.Warn("setfps battle failed: battle manager server not ready");
                return false;
            }
            else
            {
                BattleManagerServer.Write(msg);
            }
            return true;
        }

        private bool SetManagerFps(double fps, int mainId)
        {
            MSG_GM_SET_FPS msg = new MSG_GM_SET_FPS();
            msg.FPS = fps;

            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("set manager fps failed: manager server not ready");
                return false;
            }
            else
            {
                mServer.Write(msg);
            }
            return true;
        }

        private bool SetBattleManagerFps(double fps)
        {
            MSG_GBM_SET_FPS msg = new MSG_GBM_SET_FPS();
            msg.FPS = fps;

            if (BattleManagerServer == null || BattleManagerServer.State != ServerState.Started)
            {
                Log.Warn("set manager fps failed: manager server not ready");
                return false;
            }
            else
            {
                BattleManagerServer.Write(msg);
            }
            return true;
        }

        private bool SetBarrackFps(double fps, int subId)
        {
            MSG_GB_SET_FPS msg = new MSG_GB_SET_FPS();
            msg.FPS = fps;
            if (subId == 0)
            {
                BarrackServerManager.Broadcast(msg);
            }
            else
            {
                //Barrack 的mainId和global是一致的
                FrontendServer barrackServer = BarrackServerManager.GetServer(MainId, subId);
                if (barrackServer == null)
                {
                    Log.Warn("set barrack fps failed: barrack mainId {0} subId {1} not ready", MainId, SubId);
                    return false;
                }
                else
                {
                    barrackServer.Write(msg);
                }
            }
            return false;
        }

        private bool SetRelationFps(double fps, int mainId)
        {
            MSG_GM_SET_Relation_FPS msg = new MSG_GM_SET_Relation_FPS();
            msg.Fps = fps;
            msg.MainId = mainId;

            if (mainId != 0)
            {
                FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
                if (mServer == null || mServer.State != ServerState.Started)
                {
                    Log.Warn("setfps failed: relation server {0} not exist", mainId);
                    return false;
                }
                mServer.Write(msg);
            }
            else
            {
                ManagerServerManager.Broadcast(msg);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servertype"> 包括, barrack,gate,zone,manager,relation,battle,battlemanager </param>
        /// <param name="fps"></param>
        /// <param name="paramId1"></param>
        /// <param name="paramId2"></param>
        /// <returns></returns>
        private bool SetFps(string servertype, double fps, int paramId1, int paramId2)
        {
            switch (servertype)
            {
                case "zone":
                    SetZoneFps(fps, paramId1, paramId2);
                    break;
                case "battle":
                    SetBattleFps(fps, paramId1, paramId2);
                    break;
                case "gate":
                    SetGateFps(fps, paramId1, paramId2);
                    break;
                case "manager":
                    SetManagerFps(fps, paramId1);
                    break;
                case "battlemanager":
                    SetBattleManagerFps(fps);
                    break;
                case "barrack":
                    SetBarrackFps(fps, paramId1);
                    break;
                case "relation":
                    SetRelationFps(fps, paramId1);
                    break;
            }

            return true;
        }

        private bool SetFpsCommand(string[] cmdArr)
        {
            string help1 = @"SetFps Usage 1: setfps fps servername mainId subId";
            string help2 = @"SetFps Usage 2: setfps fps servername mainId/subId";
            string help = help1 + "\n" + help2;
            string servername = "";

            double fps = 40;

            int paramId1 = 0;
            int paramId2 = 0;

            if (cmdArr.Length < 2)
            {
                Log.Warn(help);
                return false;
            }
            if (double.TryParse(cmdArr[1], out fps))
            {
                if (cmdArr.Length > 2)
                {
                    servername = cmdArr[2];

                    if (cmdArr.Length > 3)
                    {
                        if (!int.TryParse(cmdArr[3], out paramId1))
                        {
                            return false;
                        }
                        if (cmdArr.Length > 4)
                        {
                            if (!int.TryParse(cmdArr[4], out paramId2))
                            {
                                return false;
                            }
                        }
                    }
                    SetFps(servername, fps, paramId1, paramId2);
                }
                else
                {
                    Log.Warn(help);
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }
            return true;
        }


        //private bool RequestChatInfoCommand(string[] cmdArr)
        //{
        //    // 显示该指定main id下 该map有多少线，每个线挂在哪个zone，当前地图有多少人，有多少人在切往该图中
        //    string help = "chatinfo Usage: chatinfo world/nearby";
        //    if (cmdArr.Length != 2)
        //    {
        //        Log.Warn(help);
        //        return false;
        //    }
        //    if (ChatManagerServer == null)
        //    {
        //        return false;
        //    }
        //    switch (cmdArr[1])
        //    {
        //        case "world":
        //            MSG_GCM_WORLD_CHAT_INFO worldMsg = new MSG_GCM_WORLD_CHAT_INFO();
        //            ChatManagerServer.Write(worldMsg);
        //            break;
        //        case "nearby":
        //            MSG_GCM_NEARBY_CHAT_INFO nearbyMsg = new MSG_GCM_NEARBY_CHAT_INFO();
        //            ChatManagerServer.Write(nearbyMsg);
        //            break;
        //        default:
        //            Log.Warn(help);
        //            break;
        //    }
        //    return true;
        //}

        private bool RequestMapInfoCommand(string[] cmdArr)
        {
            // 显示该指定main id下 该map有多少线，每个线挂在哪个zone，当前地图有多少人，有多少人在切往该图中
            string help = "MapInfo Usage: mapinfo mainId mapId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId, mapId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out mapId) == false)
            {
                Log.Warn(help);
                return false;
            }

            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("MapInfo failed: Manager server {0} not ready", mainId);
                return false;
            }
            MSG_GM_MAP_INFO msg = new MSG_GM_MAP_INFO();
            msg.MainId = mainId;
            msg.MapId = mapId;
            mServer.Write(msg);
            return true;
        }

        private bool RequstZoneInfoCommand(string[] cmdArr)
        {
            // 显示该zone有多少图 每个图当前多少人，多少人在切往该图中，该zone CPU信息 帧率 人数统计信息
            string help = "ZoneInfo Usage: zoneinfo mainId subId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId, subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("ZoneInfo failed: Manager server {0} not ready", mainId);
                return false;
            }
            MSG_GM_ZONE_INFO msg = new MSG_GM_ZONE_INFO();
            msg.MainId = mainId;
            msg.SubId = subId;
            mServer.Write(msg);
            return true;
        }

        private bool RequstAllZoneInfoCommand(string[] cmdArr)
        {
            // 显示该main id下所有zone的人数 CPU 帧率 挂载map数 挂载副本数
            string help = "AllZoneInfo Usage: allzoneinfo mainId";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int mainId;
            if (int.TryParse(cmdArr[1], out mainId) == false)
            {
                Log.Warn(help);
                return true;
            }
            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("AllZoneInfo failed: Manager server {0} not ready", mainId);
                return false;
            }
            MSG_GM_ALL_ZONE_INFO msg = new MSG_GM_ALL_ZONE_INFO();
            msg.MainId = mainId;
            mServer.Write(msg);
            return true;
        }

        private bool RequstBattleInfoCommand(string[] cmdArr)
        {
            // battle CPU信息 帧率 
            string help = "BattleInfo Usage: battleinfo mainId subId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId, subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            if (BattleManagerServer == null)
            {
                Log.Warn("BattleInfo failed: battle manager server not ready");
                return false;
            }
            else
            {
                MSG_GBM_BATTLE_INFO msgBattleInfo = new MSG_GBM_BATTLE_INFO();
                msgBattleInfo.MainId = mainId;
                msgBattleInfo.SubId = subId;
                BattleManagerServer.Write(msgBattleInfo);
            }
            return true;
        }

        private bool RequstAllBattleInfoCommand(string[] cmdArr)
        {
            // 显示该main id下所有battle  CPU FPS
            string help = "AllBattleInfo Usage: allbattleinfo";
            if (BattleManagerServer == null)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GBM_ALL_BATTLE_INFO msg = new MSG_GBM_ALL_BATTLE_INFO();
            BattleManagerServer.Write(msg);
            return true;
        }


        private void RequestGateInfoDirectly(int mainId, int subId)
        {
            MSG_GGate_GATE_FPS_INFO msg = new MSG_GGate_GATE_FPS_INFO();
            if (subId == 0)
            {
                GateServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer gateServer = GateServerManager.GetServer(mainId, subId);
                if (gateServer == null || gateServer.State != ServerState.Started)
                {
                    Log.Warn("gateinfo failed: {0} not ready", gateServer.ServerName);
                    return;
                }
                else
                {
                    gateServer.Write(msg);
                }
            }
        }

        private bool RequstGateInfoCommand(string[] cmdArr)
        {
            //gate CPU信息 帧率 
            string help = "GateInfo Usage: gateinfo mainId subId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId, subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            RequestGateInfoDirectly(mainId, subId);
            return true;
        }

        private bool RequstAllGateInfoCommand(string[] cmdArr)
        {
            // 显示该main id下所有battle  CPU FPS
            string help = "AllGateInfo Usage: allgateinfo";
            if (cmdArr.Length != 1)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GB_ALL_GATE_INFO msg = new MSG_GB_ALL_GATE_INFO();
            FrontendServer barrack = BarrackServerManager.GetWatchDogServer();
            if (barrack == null)
            {
                Log.Warn("allgateinfo excute failed: watchdog barrack server not ready");
                return false;
            }
            barrack.Write(msg);
            return true;
        }


        private void RequestBarrackInfo(int mainId, int subId)
        {
            MSG_GB_FPS_INFO msg = new MSG_GB_FPS_INFO();
            if (subId == 0)
            {
                BarrackServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer barrackServer = BarrackServerManager.GetServer(mainId, subId);
                if (barrackServer == null || barrackServer.State != ServerState.Started)
                {
                    Log.Warn("barrackinfo failed: {0} not ready", barrackServer.ServerName);
                    return;
                }
                else
                {
                    barrackServer.Write(msg);
                }
            }
            return;
        }

        private bool RequestBarrackInfoCommand(string[] cmdArr)
        {
            string help = "barrackinfo Usage: barrackinfo mainId subId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId, subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            RequestBarrackInfo(mainId, subId);
            return true;
        }

        private bool RequestAllBarrackInfoCommand(string[] cmdArr)
        {
            // 显示该main id下所有barrack  CPU FPS
            string help = "allbarrackinfo Usage: allbarrackinfo";
            if (cmdArr.Length != 1)
            {
                Log.Warn(help);
                return false;
            }
            RequestBarrackInfo(1001, 0);
            return true;
        }

        private bool RequestPayInfoCommand(string[] cmdArr)
        {
            string help = "payinfo Usage: payinfo mainId subId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId, subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            RequestPayInfo(mainId, subId);
            return true;
        }

        private bool RequestAllPayInfoCommand(string[] cmdArr)
        {
            // 显示该main id下所有barrack  CPU FPS
            string help = "allpayinfo Usage: allpayinfo";
            if (cmdArr.Length != 1)
            {
                Log.Warn(help);
                return false;
            }
            RequestPayInfo(1001, 0);
            return true;
        }

        private void RequestPayInfo(int mainId, int subId)
        {
            MSG_GP_FPS_INFO msg = new MSG_GP_FPS_INFO();
            if (subId == 0)
            {
                PayServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer barrackServer = PayServerManager.GetServer(mainId, subId);
                if (barrackServer == null || barrackServer.State != ServerState.Started)
                {
                    Log.Warn("payinfo failed: {0} not ready", barrackServer.ServerName);
                    return;
                }
                else
                {
                    barrackServer.Write(msg);
                }
            }
            return;
        }

        private bool RequstRelationInfoCommand(string[] cmdArr)
        {
            //relation CPU信息 帧率 
            string help = "Relationinfo Usage: relationinfo mainId";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int relationMainId;
            if (int.TryParse(cmdArr[1], out relationMainId) == false)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GM_RELATION_FPS_INFO msg = new MSG_GM_RELATION_FPS_INFO();
            msg.MainId = relationMainId;
            if (relationMainId != 0)
            {
                FrontendServer mserverForRelation = ManagerServerManager.GetSinglePointServer(relationMainId);
                if (mserverForRelation == null)
                {
                    Log.Warn("relationinfo failed: relation server {0} not exist", relationMainId);
                    return false;
                }
                mserverForRelation.Write(msg);
            }
            else
            {
                ManagerServerManager.Broadcast(msg);
            }
            return true;
        }

        private bool RequstManagerInfoCommand(string[] cmdArr)
        {
            //gate CPU信息 帧率 
            string help = "managerinfo Usage: managerinfo mainId";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int mainId;
            if (int.TryParse(cmdArr[1], out mainId) == false)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GM_FPS_INFO msg = new MSG_GM_FPS_INFO();
            if (mainId != 0)
            {
                FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
                if (mServer == null)
                {
                    Log.Warn("managerinfo failed: manager server {0} not exist", mainId);
                    return false;
                }
                mServer.Write(msg);
            }
            else
            {
                ManagerServerManager.Broadcast(msg);
            }
            return true;
        }

        private bool RequstBattleManagerInfoCommand(string[] cmdArr)
        {
            //gate CPU信息 帧率 
            string help = "battlemanagerinfo Usage: battlemanagerinfo";
            if (BattleManagerServer == null)
            {
                Log.Warn("{0}, battlemanagerinfo failed: battle manager not exist", help);
                return false;
            }
            else
            {
                MSG_GBM_FPS_INFO msg = new MSG_GBM_FPS_INFO();
                BattleManagerServer.Write(msg);
            }
            return true;
        }

        private bool ShutdownPayCommand(string[] cmdArr)
        {
            string help = "ShutdownPay Usage: ShutdownPay subId; if subId == 0 , close all barracks";
            int subId = 0;
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            if (int.TryParse(cmdArr[1], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }

            MSG_GP_SHUTDOWN msgShutdownBarrack = new MSG_GP_SHUTDOWN();
            if (subId == 0)
            {
                PayServerManager.Broadcast(msgShutdownBarrack);
            }
            else
            {
                FrontendServer barrackServer = PayServerManager.GetServer(MainId, subId);
                barrackServer.Write(msgShutdownBarrack);
            }
            return true;
        }

        private bool ShutdownBarrackCommand(string[] cmdArr)
        {
            string help = "ShutDownBarrack Usage: shutdownbarrack subId; if subId == 0 , close all barracks";
            int subId = 0;
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            if (int.TryParse(cmdArr[1], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }

            MSG_GB_SHUTDOWN msgShutdownBarrack = new MSG_GB_SHUTDOWN();
            if (subId == 0)
            {
                BarrackServerManager.Broadcast(msgShutdownBarrack);
            }
            else
            {
                FrontendServer barrackServer = BarrackServerManager.GetServer(MainId, subId);
                barrackServer.Write(msgShutdownBarrack);
            }
            return true;
        }

        private bool ShutdownCountryCommand(string[] cmdArr)
        {
            string help = "shutdowncountry Usage: shutdowncountry mainId; if manId == 0, will close all countrys";
            //int managerMainId = 0;
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            //if (int.TryParse(cmdArr[1], out managerMainId) == false)
            //{
            //    Log.Warn(help);
            //    return false;
            //}

            MSG_GM_SHUTDOWN_MAIN msgShutdownMain = new MSG_GM_SHUTDOWN_MAIN();
            MSG_GGATE_SHUTDOWN_GATE msg = new MSG_GGATE_SHUTDOWN_GATE();
            MSG_GA_SHUTDOWN_GATE gaMsg = new MSG_GA_SHUTDOWN_GATE();

            if (cmdArr[1] == "0")
            {
                ManagerServerManager.Broadcast(msgShutdownMain);

                GateServerManager.Broadcast(msg);

                AnalysisServerManager.Broadcast(gaMsg);
            }
            else
            {
                if (cmdArr[1].Contains("-"))
                {
                    string[] ids = StringSplit.GetArray("-", cmdArr[1]);

                    int startId;
                    if (int.TryParse(ids[0], out startId) == false)
                    {
                        Log.Warn(help);
                        return false;
                    }
                    int endId;
                    if (int.TryParse(ids[1], out endId) == false)
                    {
                        Log.Warn(help);
                        return false;
                    }

                    for (int i = startId; i <= endId; i++)
                    {
                        FrontendServer mServer = ManagerServerManager.GetSinglePointServer(i);
                        if (mServer != null)
                        {
                            mServer.Write(msgShutdownMain);
                        }

                        List<FrontendServer> gServers = GateServerManager.GetAllServer(i);
                        foreach (var gServer in gServers)
                        {
                            gServer.Write(msg);
                        }

                        FrontendServer aServer = AnalysisServerManager.GetSinglePointServer(i);
                        if (aServer != null)
                        {
                            aServer.Write(gaMsg);
                        }
                    }
                }
                else
                {
                    int mainId;
                    if (int.TryParse(cmdArr[1], out mainId) == false)
                    {
                        Log.Warn(help);
                        return false;
                    }


                    if (mainId != 0)
                    {
                        FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
                        if (mServer != null)
                        {
                            mServer.Write(msgShutdownMain);
                        }

                        List<FrontendServer> gServers = GateServerManager.GetAllServer(mainId);
                        foreach (var gServer in gServers)
                        {
                            gServer.Write(msg);
                        }

                        FrontendServer aServer = AnalysisServerManager.GetSinglePointServer(mainId);
                        if (aServer != null)
                        {
                            aServer.Write(gaMsg);
                        }
                    }
                }
            }

            return true;
        }

        private bool ShutdownBattleManagerCommand(string[] cmdArr)
        {
            string help = "shutdownbattlemanager Usage: shutdownbattlemanager;  close battle manager server";
            if (cmdArr.Length != 1)
            {
                Log.Warn(help);
                return false;
            }

            if (BattleManagerServer == null)
            {
                Log.Warn("shutdown failed: battle manager not exist");
                return false;
            }
            else
            {
                MSG_GBM_SHUTDOWN_BATTLEMANAGER msgShutdownBM = new MSG_GBM_SHUTDOWN_BATTLEMANAGER();
                BattleManagerServer.Write(msgShutdownBM);
            }
            return true;
        }


        private bool ShutdownRelationCommand(string[] cmdArr)
        {
            string help = "shutdownrelation Usage: shutdownrelation main; if main == 0, will close all relations";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int relationMainId;
            if (int.TryParse(cmdArr[1], out relationMainId) == false)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GM_SHUTDOWN_RELATION msgShutdownRelation = new MSG_GM_SHUTDOWN_RELATION();
            if (relationMainId != 0)
            {
                FrontendServer mServer = ManagerServerManager.GetSinglePointServer(relationMainId);
                if (mServer == null || mServer.State != ServerState.Started)
                {
                    Log.Warn("shutdown failed: relation server {0} not exist", relationMainId);
                    return false;
                }
                mServer.Write(msgShutdownRelation);
            }
            else
            {
                ManagerServerManager.Broadcast(msgShutdownRelation);
            }
            return true;
        }

        private bool ShutdownRelationDerectlyCommand(string[] cmdArr)
        {
            string help = "shutdownrelation1 Usage: shutdownrelation1 main; if main == 0, will close all relations";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int relationMainId;
            if (int.TryParse(cmdArr[1], out relationMainId) == false)
            {
                Log.Warn(help);
                return false;
            }

            MSG_GR_SHUTDOWN msg = new MSG_GR_SHUTDOWN();
            if (RelationServerManager == null)
            {
                Log.Warn(String.Format("shutdown relation mainId {0} failed: find relation manager failed", relationMainId));
            }

            FrontendServer relationServer = RelationServerManager.GetSinglePointServer(relationMainId);
            if (relationServer == null || relationServer.State != ServerState.Started)
            {
                Log.Warn("shutdown relation failed: relation server not ready");
                return false;
            }
            else
            {
                relationServer.Write(msg);
            }
            return true;
        }


        private bool ShutdownBattleCommand(string[] cmdArr)
        {
            string help = "shutdownbattle Usage: shutdownbattle battleId;if battleId == 0, will close all battles";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int subId;
            if (int.TryParse(cmdArr[1], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            if (BattleManagerServer == null)
            {
                Log.Warn("shutdown failed: battle manager server not ready");
                return false;
            }
            else
            {
                MSG_GBM_SHUTDOWN_BATTLE msgShutdonwBattle = new MSG_GBM_SHUTDOWN_BATTLE();
                msgShutdonwBattle.SubId = subId;
                BattleManagerServer.Write(msgShutdonwBattle);
            }
            return true;
        }

        private bool ShutdownBattleDirectlyCommand(string[] cmdArr)
        {
            string help = "shutdownbattle1 Usage: shutdownbattle battleId; if battleId == 0, will close all battles";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int subId;
            if (int.TryParse(cmdArr[1], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GBattle_SHUTDOWN_Battle msg = new MSG_GBattle_SHUTDOWN_Battle();
            if (BattleServerManager == null)
            {
                Log.Warn(String.Format("shutdown battle subId {1} failed: find battle manager failed", subId));
            }
            if (subId == 0)
            {
                BattleServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer battleServer = BattleServerManager.GetServer(MainId, subId);
                if (battleServer == null || battleServer.State != ServerState.Started)
                {
                    Log.Warn("shutdown battle failed: battle server not ready");
                    return false;
                }
                else
                {
                    battleServer.Write(msg);
                }
            }
            return true;
        }


        private bool ShutdownZoneCommand(string[] cmdArr)
        {
            string help = "shutdownzone Usage: shutdownzone mainId subId; if subId == 0, will close all zone in mainId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId;
            int subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            if (mainId != 0)
            {
                FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
                if (mServer == null || mServer.State != ServerState.Started)
                {
                    Log.Warn("shutdown failed: Manager server {0} not ready", mainId);
                    return false;
                }
                MSG_GM_SHUTDOWN_ZONE msgShutdonwZone = new MSG_GM_SHUTDOWN_ZONE();
                msgShutdonwZone.MainId = mainId;
                msgShutdonwZone.SubId = subId;
                mServer.Write(msgShutdonwZone);
            }
            else
            {
                foreach (var item in ManagerServerManager.ServerList.Values)
                {
                    ManagerServer manager = (ManagerServer)item;
                    MSG_GM_SHUTDOWN_ZONE msgShutdonwZone = new MSG_GM_SHUTDOWN_ZONE();
                    msgShutdonwZone.MainId = manager.MainId;
                    msgShutdonwZone.SubId = subId;
                    manager.Write(msgShutdonwZone);
                }
            }
            return true;
        }

        private bool ShutdownZoneDirectlyCommand(string[] cmdArr)
        {
            string help = "shutdownbattle1 Usage: shutdownbattle mainId subId; ; if subId == 0, will close all battle in mainId";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId;
            int subId;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            MSG_GZ_SHUTDOWN_ZONE msg = new MSG_GZ_SHUTDOWN_ZONE();
            if (ZoneServerManager == null)
            {
                Log.Warn(String.Format("shutdown zone mainId{0} subId{1} failed: find zone manager failed", mainId, subId));
            }
            if (subId == 0)
            {
                ZoneServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer zoneServer = ZoneServerManager.GetServer(mainId, subId);
                if (zoneServer == null || zoneServer.State != ServerState.Started)
                {
                    Log.Warn("shutdown zone failed: zone server not ready");
                    return false;
                }
                else
                {
                    zoneServer.Write(msg);
                }
            }
            return true;
        }


        private void ShutdownGate(int subId)
        {
            //BarrackServerManager.Broadcast(shutdownGate);
            FrontendServer barrack = BarrackServerManager.GetWatchDogServer();
            if (barrack == null)
            {
                Log.Warn("shutdownallgate excute failed: watchdog barrack server not ready");
                return;
            }
            MSG_GB_SHUTDOWN_GATE notify = new MSG_GB_SHUTDOWN_GATE();
            notify.GateId = subId;
            barrack.Write(notify);
        }

        private bool ShutdownGateCommand(string[] cmdArr)
        {
            string help = "shutdowngate Usage: shutdowngate gateId; if gateId == 0, will close all gate in mainId";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int subId;
            if (int.TryParse(cmdArr[1], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }
            ShutdownGate(subId);
            //BarrackServerManager.Broadcast(notify);
            return true;
        }

        private bool ShutdownGateDirectlyCommand(string[] cmdArr)
        {
            string help = "shutdowngate1 Usage: shutdowngate1 gateId; if gateId == 0, will close all gate in mainId";
            if (cmdArr.Length != 2)
            {
                Log.Warn(help);
                return false;
            }
            int subId;
            if (int.TryParse(cmdArr[1], out subId) == false)
            {
                Log.Warn(help);
                return false;
            }

            MSG_GGATE_SHUTDOWN_GATE msg = new MSG_GGATE_SHUTDOWN_GATE();
            if (subId == 0)
            {
                GateServerManager.Broadcast(msg);
            }
            else
            {
                FrontendServer gateServer = GateServerManager.GetServer(mainId, subId);
                if (gateServer == null || gateServer.State != ServerState.Started)
                {
                    return false;
                }
                else
                {
                    gateServer.Write(msg);
                }
            }
            return true;
        }


        //private bool ShutdownChatManagerCommand(string[] cmdArr)
        //{
        //    if (ChatManagerServer == null)
        //    {
        //        return false;
        //    }
        //    MSG_GCM_SHUTDOWN shutdownChatManager = new MSG_GCM_SHUTDOWN();
        //    ChatManagerServer.Write(shutdownChatManager);
        //    return true;
        //}

        private bool ShutdownAllCommand(string[] cmdArr)
        {
            // step 1 gate
            ShutdownGate(0);
            // step 2 barrack
            MSG_GB_SHUTDOWN shutdownBarrack = new MSG_GB_SHUTDOWN();
            BarrackServerManager.Broadcast(shutdownBarrack);
            // step 3 battle
            if (BattleManagerServer != null)
            {
                MSG_GBM_SHUTDOWN_BATTLE shutdonwBattle = new MSG_GBM_SHUTDOWN_BATTLE();
                shutdonwBattle.SubId = 0;
                BattleManagerServer.Write(shutdonwBattle);
                // step 4 battle manager
                MSG_GBM_SHUTDOWN_BATTLEMANAGER shutdownBM = new MSG_GBM_SHUTDOWN_BATTLEMANAGER();
                BattleManagerServer.Write(shutdownBM);
            }
            // step 5 country
            MSG_GM_SHUTDOWN_MAIN shutdownMain = new MSG_GM_SHUTDOWN_MAIN();
            ManagerServerManager.Broadcast(shutdownMain);
            // step 6 chat manager
            //if (ChatManagerServer != null)
            //{
            //    MSG_GCM_SHUTDOWN shutdownChatManager = new MSG_GCM_SHUTDOWN();
            //    ChatManagerServer.Write(shutdownChatManager);
            //}
            return true;
        }


        private bool KickCommand(string[] cmdArr)
        {
            string help = "Kick Usage: Kick mainId playerUid";
            if (cmdArr.Length != 3)
            {
                Log.Warn(help);
                return false;
            }
            int mainId;
            int uid;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out uid) == false)
            {
                Log.Warn(help);
                return false;
            }
            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("kick failed: Manager server {0} not ready", mainId);
                return false;
            }
            MSG_GM_KICK_PLAYER msg = new MSG_GM_KICK_PLAYER();
            msg.MainId = mainId;
            msg.Uid = uid;
            mServer.Write(msg);
            return true;
        }

        private bool FreezeCommand(string[] cmdArr)
        {
            string help = "Freeze Usage: freeze mainId uid freezeType hour";
            if (cmdArr.Length < 4)
            {
                Log.Warn(help);
                return false;
            }
            int mainId;
            int uid;
            int freezeType;
            int hour = 0;
            if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out uid) == false || int.TryParse(cmdArr[3], out freezeType) == false)
            {
                Log.Warn(help);
                return false;
            }
            if (freezeType == (int)FreezeState.Freeze)
            {
                if (cmdArr.Length != 5)
                {
                    Log.Warn(help);
                    return false;
                }
                if (int.TryParse(cmdArr[4], out hour) == false)
                {
                    if (cmdArr.Length != 5)
                    {
                        Log.Warn(help);
                        return false;
                    }
                }
            }
            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("freeze failed: Manager server {0} not ready", mainId);
                return false;
            }

            DateTime freezeTime = DateTime.MinValue;
            if (freezeType == (int)FreezeState.Freeze)
            {
                freezeTime = GlobalServerApi.now.AddHours(hour);
            }

            MSG_GM_FREEZE_PLAYER msg = new MSG_GM_FREEZE_PLAYER();
            msg.MainId = mainId;
            msg.Uid = uid;
            msg.FreezeType = freezeType;
            msg.Hour = hour;
            mServer.Write(msg);
            return true;
        }

        private bool SendEmailCommand(string[] cmdArr)
        {
            string help = "sendemail Usage: sendemail mainId emailId \"where\"sql\" (saveTime);  if omission (saveTime),  will save no delete time";
            int mainId;
            int emailId;
            int saveTime = 0;
            string sqlConditions = string.Empty;
            if (cmdArr.Length == 4)
            {
                if (int.TryParse(cmdArr[1], out mainId) == false ||
                    int.TryParse(cmdArr[2], out emailId) == false)
                {
                    Log.Warn(help);
                    return false;
                }
            }
            else if (cmdArr.Length == 5)
            {
                if (int.TryParse(cmdArr[1], out mainId) == false ||
                    int.TryParse(cmdArr[2], out emailId) == false ||
                    int.TryParse(cmdArr[4], out saveTime) == false)
                {
                    Log.Warn(help);
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }
            sqlConditions = cmdArr[3].Replace("\"", " ");
            if (mainId == 0)
            {
                foreach (var item in ManagerServerManager.ServerList)
                {
                    MSG_GM_SEND_EMAIL msgEmail = new MSG_GM_SEND_EMAIL();
                    msgEmail.EmailId = emailId;
                    msgEmail.SaveTime = saveTime;
                    msgEmail.MainId = item.Value.MainId;
                    msgEmail.SqlConditions = sqlConditions.Trim();
                    item.Value.Write(msgEmail);
                }
            }
            else
            {
                FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
                if (mServer == null || mServer.State != ServerState.Started)
                {
                    Log.Warn("sendemail failed: Manager server {0} not ready", mainId);
                    return false;
                }
                MSG_GM_SEND_EMAIL msgEmail = new MSG_GM_SEND_EMAIL();
                msgEmail.EmailId = emailId;
                msgEmail.SaveTime = saveTime;
                msgEmail.MainId = mainId;
                msgEmail.SqlConditions = sqlConditions.Trim();
                mServer.Write(msgEmail);
            }
            return true;
        }

        private bool AnnouncementCommand(string[] cmdArr)
        {
            //TODO:need to do
            //MSG_GB_REFRESH_ANNOUNCEMENT msgAnnouncement = new MSG_GB_REFRESH_ANNOUNCEMENT();
            //BarrackServerManager.Broadcast(msgAnnouncement);
            return true;
        }

        private bool UpdateXmlCommand(string[] cmdArr)
        {
            string serverType = "";
            string help = @"updatexml Usage: updatexml or updatexml serverType;";
            if (cmdArr.Length == 1)
            {
                serverType = "";
            }
            else if (cmdArr.Length == 2)
            {
                serverType = cmdArr[1];
            }
            else
            {
                Log.Warn(help);
                return false;
            }

            if (serverType == "barrack" || serverType == "")
            {
                MSG_GB_UPDATE_XML msgUpdateGateXml = new MSG_GB_UPDATE_XML();
                BarrackServerManager.Broadcast(msgUpdateGateXml);
            }

            if (serverType == "zone" || serverType == "")
            {
                foreach (var item in ManagerServerManager.ServerList.Values)
                {
                    MSG_GM_UPDATE_XML msgUpdateXml = new MSG_GM_UPDATE_XML();
                    item.Write(msgUpdateXml);
                }
            }

            if (serverType == "gate" || serverType == "")
            {
                MSG_GGate_UPDATE_XML msgUpdateGateXml = new MSG_GGate_UPDATE_XML();
                GateServerManager.Broadcast(msgUpdateGateXml);
            }

            if (serverType == "battle" || serverType == "")
            {
                if (BattleManagerServer != null)
                {
                    MSG_GBM_UPDATE_XML updateXmlMsg = new MSG_GBM_UPDATE_XML();
                    BattleManagerServer.Write(updateXmlMsg);
                }
            }
            if (serverType == "cross" || serverType == "")
            {
                if (CrossServerManager != null)
                {
                    MSG_GCross_UPDATE_XML msgUpdatCrossXml = new MSG_GCross_UPDATE_XML();
                    CrossServerManager.Broadcast(msgUpdatCrossXml);
                }
            }

            if (serverType == "analysis" || serverType == "")
            {
                if (AnalysisServerManager != null)
                {
                    MSG_GA_UPDATE_XML msgUpdatAnalysisXml = new MSG_GA_UPDATE_XML();
                    AnalysisServerManager.Broadcast(msgUpdatAnalysisXml);
                }
            }

            if (serverType == "pay" || serverType == "")
            {
                if (PayServerManager != null)
                {
                    MSG_GP_UPDATE_XML updateXmlMsg = new MSG_GP_UPDATE_XML();
                    PayServerManager.Broadcast(updateXmlMsg);
                }
            }

            InitData();
            UpdateGmHttpServer();
            wlfareMng.UpdateInfo();
            return true;
        }

        private bool UpdateServerXmlCommand(string[] cmdArr)
        {
            int mainId = 0;
            string help = @"updateserverxml Usage: updateserverxml or updateserverxml serverid;";
            if (cmdArr.Length > 1)
            {
                if (int.TryParse(cmdArr[1], out mainId) == false)
                {
                    Log.Warn(help);
                    return false;
                }
            }
     

            if (BarrackServerManager != null)
            {
                MSG_GB_UPDATE_XML msgUpdateBarrackXml = new MSG_GB_UPDATE_XML();
                msgUpdateBarrackXml.Type = 1;
                BarrackServerManager.Broadcast(msgUpdateBarrackXml);
            }

            if (CrossServerManager != null)
            {
                MSG_GCross_UPDATE_XML msgUpdatCrossXml = new MSG_GCross_UPDATE_XML();
                msgUpdatCrossXml.Type = 1;
                CrossServerManager.Broadcast(msgUpdatCrossXml);
            }

            if (ManagerServerManager != null)
            {
                MSG_GM_UPDATE_XML msgUpdateXml = new MSG_GM_UPDATE_XML();
                msgUpdateXml.Type = 1;
                ManagerServerManager.Broadcast(msgUpdateXml, mainId);
            }

            if (GateServerManager != null)
            {
                MSG_GGate_UPDATE_XML msgUpdateGateXml = new MSG_GGate_UPDATE_XML();
                msgUpdateGateXml.Type = 1;
                GateServerManager.Broadcast(msgUpdateGateXml, mainId);
            }

            if (BattleManagerServer != null)
            {
                MSG_GBM_UPDATE_XML updateXmlMsg = new MSG_GBM_UPDATE_XML();
                updateXmlMsg.Type = 1;
                BattleManagerServer.Write(updateXmlMsg);
            }

            if (PayServerManager != null)
            {
                MSG_GP_UPDATE_XML updateXmlMsg = new MSG_GP_UPDATE_XML();
                updateXmlMsg.Type = 1;
                PayServerManager.Broadcast(updateXmlMsg);
            }

            return true;
        }

        private bool UpdateWlfareCommand(string[] cmdArr)
        {
            //string help = @"updatewlfare Usage: updatewlfare ;";

            wlfareMng.UpdateInfo();

            return true;
        }

        private bool UpdateRankListCommand(string[] cmdArr)
        {
            //string help = @"updatranklist Usage: updatranklist";
            foreach (var item in ManagerServerManager.ServerList.Values)
            {
                ManagerServer manager = (ManagerServer)item;
                if (manager.WatchDog)
                {
                    MSG_GM_UPDATE_RANKLIST msgUpdateRank = new MSG_GM_UPDATE_RANKLIST();
                    item.Write(msgUpdateRank);
                }
            }
            return true;
        }


        private bool OpenWhiteListCommand(string[] cmdArr)
        {
            //MSG_GGate_WHITE_LIST msgOpenWhiteList = new MSG_GGate_WHITE_LIST();
            //msgOpenWhiteList.Open = true;
            //GateServerManager.Broadcast(msgOpenWhiteList);
            MSG_GB_WHITE_LIST msgOpenWhiteList = new MSG_GB_WHITE_LIST();
            msgOpenWhiteList.Open = true;
            BarrackServerManager.Broadcast(msgOpenWhiteList);
            return true;
        }

        private bool CloseWhiteListCommand(string[] cmdArr)
        {
            //MSG_GGate_WHITE_LIST msgCloseWhiteList = new MSG_GGate_WHITE_LIST();
            //msgCloseWhiteList.Open = false;
            //GateServerManager.Broadcast(msgCloseWhiteList);

            MSG_GB_WHITE_LIST msgCloseWhiteList = new MSG_GB_WHITE_LIST();
            msgCloseWhiteList.Open = false;
            BarrackServerManager.Broadcast(msgCloseWhiteList);
            return true;
        }

        private bool ReloadBlackListCommand(string[] cmdArr)
        {
            //TODO:need to do
            MSG_GB_BLACKLIST_RELOAD msgReloadBlack = new MSG_GB_BLACKLIST_RELOAD();
            BarrackServerManager.Broadcast(msgReloadBlack);
            return true;
        }


        private bool RechargeTestCommand(string[] cmdArr)
        {
            string help = @"rechargetest Usage: rechargetest pcUid mainId giftId; eg. rechargetest 1001 pcuid 1010";
            int pcUid;
            int giftId;
            int mainId;
            if (cmdArr.Length == 4)
            {
                if (int.TryParse(cmdArr[1], out mainId) == false ||
                    int.TryParse(cmdArr[2], out pcUid) == false ||
                    int.TryParse(cmdArr[3], out giftId) == false)
                {
                    Log.Warn(help);
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }

            FrontendServer mServer = ManagerServerManager.GetOneServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("rechargetest failed: Manager server not ready");
                return false;
            }
            MSG_GM_ECHARGE_TEST msg = new MSG_GM_ECHARGE_TEST();
            msg.Uid = pcUid;
            msg.GiftId = giftId;
            mServer.Write(msg);
            return true;
        }

        private bool ReloadFamilyCommand(string[] cmdArr)
        {
            string help = @"reloadfamily Usage: reloadfamliy mainId familiyId";
            int mainId;
            int familyId;
            if (cmdArr.Length == 3)
            {
                if (int.TryParse(cmdArr[1], out mainId) == false || int.TryParse(cmdArr[2], out familyId) == false)
                {
                    Log.Warn(help);
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }
            FrontendServer mServer = ManagerServerManager.GetSinglePointServer(mainId);
            if (mServer == null || mServer.State != ServerState.Started)
            {
                Log.Warn("reloadfamily failed: Manager server {0} not ready", mainId);
                return false;
            }
            MSG_GM_RELOAD_FAMILY msg = new MSG_GM_RELOAD_FAMILY();
            msg.MainId = mainId;
            msg.FamilyId = familyId;
            mServer.Write(msg);
            return true;
        }


        private bool WaitCountCommand(string[] cmdArr)
        {
            string help = @"waitcount Usage: waitcount number";
            int waitCount;
            if (cmdArr.Length == 2)
            {
                if (int.TryParse(cmdArr[1], out waitCount) == false)
                {
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }
            MSG_GGate_WAIT_COUNT msgWaitCount = new MSG_GGate_WAIT_COUNT();
            msgWaitCount.Count = waitCount;
            GateServerManager.Broadcast(msgWaitCount);
            return true;
        }

        private bool FullCountCommand(string[] cmdArr)
        {
            string help = @"fullcount Usage: fullcount number";

            int fullCount;
            if (cmdArr.Length == 2)
            {
                if (!int.TryParse(cmdArr[1], out fullCount))
                {
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }

            MSG_GGate_FULL_COUNT msg = new MSG_GGate_FULL_COUNT();
            msg.Count = fullCount;
            GateServerManager.Broadcast(msg);
            return true;
        }

        private bool BusyonlineCountCommand(string[] cmdArr)
        {
            if (cmdArr.Length != 2)
            {
                Log.Warn("buzyonlinecount usage: buzyonlinecount count");
                return false;
            }
            int onlineCount = 1;
            int.TryParse(cmdArr[1], out onlineCount);
            MSG_GM_BUZY_ONLINE_COUNT buzyonlineCountMsg = new MSG_GM_BUZY_ONLINE_COUNT();
            buzyonlineCountMsg.Count = onlineCount;
            ManagerServerManager.Broadcast(buzyonlineCountMsg);
            return true;
        }


        private bool WaitsecCommand(string[] cmdArr)
        {
            string help = @"waitsec Usage: waitsec number";
            int waitSec;
            if (cmdArr.Length == 2)
            {
                if (int.TryParse(cmdArr[1], out waitSec) == false)
                {
                    return false;
                }
            }
            else
            {
                Log.Warn(help);
                return false;
            }
            MSG_GGate_WAIT_SEC msg = new MSG_GGate_WAIT_SEC();
            msg.Second = waitSec;
            GateServerManager.Broadcast(msg);
            return true;
        }

        private bool StopFightingCommand(string[] cmdArr)
        {
            int battleResult = 1;
            int.TryParse(cmdArr[1], out battleResult);
            MSG_GBattle_STOP_FIGHTING battleResultMsg = new MSG_GBattle_STOP_FIGHTING();
            battleResultMsg.Result = battleResult;
            BattleServerManager.Broadcast(battleResultMsg);
            return true;
        }

        private bool AddBuffCommand(string[] cmdArr)
        {
            int dstUid = 1;
            int buffId = 2;
            string paramString = "";
            if (cmdArr.Length >= 3)
            {
                if (int.TryParse(cmdArr[1], out dstUid) == false)
                {
                    return false;
                }
                if (int.TryParse(cmdArr[2], out buffId) == false)
                {
                    return false;
                }
                if (cmdArr.Length == 4)
                {
                    paramString = cmdArr[3].ToString();
                }
                MSG_GBattle_ADD_BUFF addBuffMsg = new MSG_GBattle_ADD_BUFF();
                addBuffMsg.Uid = dstUid;
                addBuffMsg.BuffId = buffId;
                addBuffMsg.ParamString = paramString;
                BattleServerManager.Broadcast(addBuffMsg);
                return true;
            }
            else
            {
                Log.Warn("");
                return false;
            }
        }

        private bool AddRewardCommand(string[] cmdArr)
        {
            if (cmdArr.Length != 2)
            {
                return false;
            }
            MSG_GM_ADD_REWARD msg = new MSG_GM_ADD_REWARD();
            msg.Reward = cmdArr[1];
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool EquipHeroCommand(string[] cmdArr)
        {
            if (cmdArr.Length != 3)
            {
                return false;
            }
            MSG_GM_EQUIP_HERO msg = new MSG_GM_EQUIP_HERO();
            msg.HeroId = cmdArr[1].ToInt();
            msg.Equip = cmdArr[2].ToBool();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool AllChatCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 3)
            {
                return false;
            }
            MSG_GM_ALL_CHAT msg = new MSG_GM_ALL_CHAT();
            msg.Channel = cmdArr[1].ToInt();
            msg.Words = cmdArr[2];
            if (cmdArr.Length == 4)
            {
                msg.Param = cmdArr[3].ToInt();
            }
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool AbsorbSoulRingCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 3)
            {
                return false;
            }
            MSG_GM_ABSORB_SOULRING msg = new MSG_GM_ABSORB_SOULRING();
            msg.HeroId = cmdArr[1].ToInt();
            msg.Slot = cmdArr[2].ToInt();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool AbsorbFinishCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 2)
            {
                return false;
            }
            MSG_GM_ABSORB_FINISH msg = new MSG_GM_ABSORB_FINISH();
            msg.HeroId = cmdArr[1].ToInt();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool AddHeroExpCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 2)
            {
                return false;
            }
            MSG_GM_ADD_HEROEXP msg = new MSG_GM_ADD_HEROEXP();
            msg.Exp = cmdArr[1].ToInt();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool HeroAwakenCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 2)
            {
                return false;
            }
            MSG_GM_HERO_AWAKEN msg = new MSG_GM_HERO_AWAKEN();
            msg.HeroId = cmdArr[1].ToInt();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool HeroLevelUpCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 2)
            {
                return false;
            }
            MSG_GM_HERO_LEVELUP msg = new MSG_GM_HERO_LEVELUP();
            msg.HeroId = cmdArr[1].ToInt();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool UpdateHeroPosCommand(string[] cmdArr)
        {
            if (cmdArr.Length < 2)
            {
                return false;
            }
            MSG_GM_UPDATE_HERO_POS msg = new MSG_GM_UPDATE_HERO_POS();
            msg.HeroPos = cmdArr[1];
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool ZoneTransform(string[] cmdArr)
        {
            if (cmdArr.Length < 4)
            {
                return false;
            }
            int serverId = int.Parse(cmdArr[1]);
            bool isForce = bool.Parse(cmdArr[2]);
            List<int> fromZones = cmdArr[3].Split('-').ToList().ConvertAll(x => int.Parse(x));
            List<int> toZones = cmdArr[4].Split('-').ToList().ConvertAll(x => int.Parse(x));

            bool success = fromZones.Count != 0 && toZones.Count != 0;
            foreach (var kv in fromZones)
            {
                FrontendServer zone = ZoneServerManager.GetServer(serverId, kv);
                if (zone == null)
                {
                    success = false;
                    break;
                }
            }

            if (!success)
            {
                return false;
            }

            FrontendServer managerServer = ManagerServerManager.GetSinglePointServer(serverId);
            if (managerServer == null)
            {
                return false;
            }

            //发送当当前server manager
            MSG_GM_ZONE_TRANSFORM requestM = new MSG_GM_ZONE_TRANSFORM() { MainId = serverId, IsForce = (bool)isForce };
            requestM.FromZones.AddRange(fromZones);
            requestM.ToZones.AddRange(toZones);
            managerServer.Write(requestM);

            return true;
        }

        private bool TriggerGiftOpen(string[] cmdArr)
        {
            if (cmdArr.Length < 2)
            {
                return false;
            }
            MSG_GM_GIFT_OPEN msg = new MSG_GM_GIFT_OPEN();
            msg.GiftItemId = cmdArr[1].ToInt();
            ManagerServerManager.Broadcast(msg);
            return true;
        }

        private bool NewServers(string[] cmdArr)
        {
            string help = @"newserver Usage: newupserver serverId1|serverId2|serverId3";
            if (cmdArr.Length < 2)
            {
                Log.Warn(help);
                return false;
            }

            List<int> servers = cmdArr[1].ToList('|');

            MSG_GB_UPDATE_NEW_SERVER msg = new MSG_GB_UPDATE_NEW_SERVER();
            msg.Servers.AddRange(servers);
            BarrackServerManager.Broadcast(msg);
            return true;
        }

        private bool LineupServers(string[] cmdArr)
        {
            string help = @"lineupserver Usage: lineupserver serverId1|serverId2|serverId3";
            if (cmdArr.Length < 2)
            {
                Log.Warn(help);
                return false;
            }

            List<int> servers = cmdArr[1].ToList('|');

            MSG_GB_UPDATE_LINEUP_SERVER msg = new MSG_GB_UPDATE_LINEUP_SERVER();
            msg.Servers.AddRange(servers);
            BarrackServerManager.Broadcast(msg);
            return true;
        }

        private bool RecommendServers(string[] cmdArr)
        {
            string help = @"recommendserver Usage: recommendserver serverId1|serverId2|serverId3";
            if (cmdArr.Length < 2)
            {
                Log.Warn(help);
                return false;
            }

            List<int> servers = cmdArr[1].ToList('|');

            MSG_GB_UPDATE_RECOMMEND_SERVER msg = new MSG_GB_UPDATE_RECOMMEND_SERVER();
            msg.Servers.AddRange(servers);
            BarrackServerManager.Broadcast(msg);
            return true;
        }

        private bool LoginOutInfo(string[] cmdArr)
        {
            MSG_GA_LOGINORLOGOUT msg = new MSG_GA_LOGINORLOGOUT() { Info = new MSG_GA_COMMON_INFO() };
            msg.IsLogin = true;

            msg.Info.ServerId = int.Parse(cmdArr[1]);
            msg.Info.Uid = int.Parse(cmdArr[2]);
            msg.Info.FromTime = Timestamp.GetUnixTimeStampSeconds(DateTime.Now.Date);
            msg.Info.ToTime = Timestamp.GetUnixTimeStampSeconds(DateTime.Now.Date.AddDays(10));
            
            AnalysisServer server = AnalysisServerManager.GetOneServer(msg.Info.ServerId) as AnalysisServer;
            if (server == null)
            {
                return false;
            }
            Log.Info("send to AnalysisServer serverId {0}", msg.Info.ServerId);
            server.Write(msg, 1);
            return true;
        }

        private bool MergeServerReward(string[] cmdArr)
        {
            string help = @"mergeserver Usage: mergeserver serverIdStart|serverIdEnd";
            if (cmdArr.Length < 2)
            {
                Log.Warn(help);
                return false;
            }

            cmdArr = cmdArr[1].Split('|');
            if (cmdArr.Length < 2)
            {
                Log.Warn(help);
                return false;
            }

            List<int> notFindRelation = new List<int>();
            List<RelationServer> relationServers = new List<RelationServer>();

            int start = int.Parse(cmdArr[0]);
            int end = int.Parse(cmdArr[1]);
            for (int i = start; i <= end; i++)
            {
                RelationServer server = RelationServerManager.GetOneServer(i, false) as RelationServer;
                if (server != null)
                {
                    relationServers.Add(server);
                }
                else
                {
                    notFindRelation.Add(i);
                }
            }

            if (notFindRelation.Count > 0)
            {
                Log.Warn($"merge server : not find relation servers {String.Join("|", notFindRelation)}");
            }

            MSG_GR_MERGE_SERVE_REWARD msg = new MSG_GR_MERGE_SERVE_REWARD();
            foreach (var server in relationServers)
            {
                server.Write(msg);
                Log.Warn($"merge server command send to RelationServer serverId {server.MainId}");
            }

            MSG_GCross_MERGE_SERVER_REWARD msgGCross = new MSG_GCross_MERGE_SERVER_REWARD()
            {
                StartServerId = start, 
                EndServerId = end
            };
            CrossServerManager.Broadcast(msgGCross);
            Log.Warn($"merge server command send to CrossServer server");

            return true;
        }

        private bool UpdatePayGlobalInfo(string[] cmdArr)
        {
            MSG_GP_UPDATE_GLOBAL msg = new MSG_GP_UPDATE_GLOBAL();
            PayServerManager.Broadcast(msg);
            return true;
        }
    }
}
