using CommonUtility;
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
using Message.Global.Protocol.GP;

namespace GlobalServerLib
{
    partial class GlobalServerApi
    {
        public void GMExcuteCommand(AHttpSession session)
        {
            //核心处理并且回消息
            string help = "";
            string[] args = session.Args;
            string answer = "OK";
            Log.Write("someone httpCmd with  cmd={0} args={1}", session.Cmd, session.Args);
            switch (session.Cmd)
            {
                case "openwhitelist":
                    {
                        MSG_GB_WHITE_LIST msgOpenWhiteList = new MSG_GB_WHITE_LIST();
                        msgOpenWhiteList.Open = true;
                        BarrackServerManager.Broadcast(msgOpenWhiteList);
                    }
                    break;
                case "closewhitelist":
                    {
                        MSG_GB_WHITE_LIST msgCloseWhiteList = new MSG_GB_WHITE_LIST();
                        msgCloseWhiteList.Open = false;
                        BarrackServerManager.Broadcast(msgCloseWhiteList);
                    }
                    break;
                case "shutdowncountry":
                    {
                        help = "shutdowncountry Usage: shutdowncountry mainId; if manId == 0, will close all countrys";
                        if (args.Length != 1)
                        {
                            //Log.Warn(help);
                            answer = help;
                            session.AnswerHttpCmd(answer);
                            return;
                        }

                        MSG_GM_SHUTDOWN_MAIN msgShutdownMain = new MSG_GM_SHUTDOWN_MAIN();
                        MSG_GGATE_SHUTDOWN_GATE msg = new MSG_GGATE_SHUTDOWN_GATE();
                        MSG_GA_SHUTDOWN_GATE gaMsg = new MSG_GA_SHUTDOWN_GATE();

                        if (args[0] == "0")
                        {
                            ManagerServerManager.Broadcast(msgShutdownMain);

                            GateServerManager.Broadcast(msg);

                            AnalysisServerManager.Broadcast(gaMsg);
                        }
                        else
                        {
                            if (args[0].Contains("-"))
                            {
                                string[] ids = StringSplit.GetArray("-", args[0]);

                                int startId;
                                if (int.TryParse(ids[0], out startId) == false)
                                {
                                    //Log.Warn(help);
                                    answer = help;
                                    session.AnswerHttpCmd(answer);
                                    return;
                                }
                                int endId;
                                if (int.TryParse(ids[1], out endId) == false)
                                {
                                    //Log.Warn(help);
                                    answer = help;
                                    session.AnswerHttpCmd(answer);
                                    return;
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
                                if (int.TryParse(args[0], out mainId) == false)
                                {
                                    //Log.Warn(help);
                                    answer = help;
                                    session.AnswerHttpCmd(answer);
                                    return;
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
                    }
                    break;
                case "updatexml":
                    {
                        help = @"updatexml Usage: updatexml mainId;";
                        int mainId;
                        if (args.Length == 1)
                        {
                            if (int.TryParse(args[0], out mainId) == false)
                            {
                                //Log.Warn(help);
                                answer = help;
                                session.AnswerHttpCmd(answer);
                                return;
                            }
                        }
                        else
                        {
                            //Log.Warn(help);
                            answer = help;
                            session.AnswerHttpCmd(answer);
                            return;
                        }

                        if (CrossServerManager != null)
                        {
                            MSG_GCross_UPDATE_XML msgUpdatCrossXml = new MSG_GCross_UPDATE_XML();
                            CrossServerManager.Broadcast(msgUpdatCrossXml);
                        }

                        if (mainId == 0)
                        {
                            foreach (var item in ManagerServerManager.ServerList)
                            {
                                MSG_GM_UPDATE_XML msgUpdateXml = new MSG_GM_UPDATE_XML();
                                item.Value.Write(msgUpdateXml);
                            }
                        }
                        else
                        {
                            ManagerServer mServer = null;
                            mServer = (ManagerServer)ManagerServerManager.GetSinglePointServer(mainId);
                            if (mServer == null || mServer.State != ServerState.Started)
                            {
                                //Log.Warn("updatexml failed: Manager server {0} not ready", mainId);
                                answer = String.Format("updatexml failed: Manager server {0} not ready", mainId);
                                session.AnswerHttpCmd(answer);
                                return;
                            }
                            MSG_GM_UPDATE_XML msgUpdateXml = new MSG_GM_UPDATE_XML();
                            mServer.Write(msgUpdateXml);
                        }
                    }
                    break;
                case "updatebarrack":
                    {
                        help = @"updatebarrack Usage: updatebarrack;";
                        MSG_GB_UPDATE_XML msgUpdateGateXml = new MSG_GB_UPDATE_XML();
                        BarrackServerManager.Broadcast(msgUpdateGateXml);
                    }
                    break;
                case "updatepay":
                    {
                        help = @"updatepay Usage: updatepay;";
                        MSG_GP_UPDATE_XML msgUpdateXml = new MSG_GP_UPDATE_XML();
                        PayServerManager.Broadcast(msgUpdateXml);
                    }
                    break;
                case "updateserverxml":
                    {
                        help = @"updateserverxml Usage: updateserverxml or updateserverxml serverid;";
                        int mainId = 0;
                        if (args.Length > 1)
                        {
                            if (int.TryParse(args[1], out mainId) == false)
                            {
                                Log.Warn(help);
                                return;
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

                        wlfareMng.UpdateInfo();
                    }
                    break;
                case "shutdown":
                    {
                        help = "shutdown Usage: shutdown mainId; if mainId == 0, will close all zone ";
                        if (args.Length != 1)
                        {
                            //Log.Warn(help);
                            answer = help;
                            session.AnswerHttpCmd(answer);
                            return;
                        }
                        int mainId;
                        if (int.TryParse(args[0], out mainId) == false)
                        {
                            //Log.Warn(help);
                            answer = help;
                            session.AnswerHttpCmd(answer);
                            return;
                        }
                        MSG_GM_SHUTDOWN_MAIN msgShutdonwZone = new MSG_GM_SHUTDOWN_MAIN();
                        if (mainId != 0)
                        {
                            ManagerServer mServer = null;
                            mServer = (ManagerServer)ManagerServerManager.GetSinglePointServer(mainId);
                            if (mServer == null || mServer.State != ServerState.Started)
                            {
                                //Log.Warn("shutdown failed: Manager server {0} not ready", mainId);
                                answer = String.Format("shutdown failed: Manager server {0} not ready", mainId);
                                session.AnswerHttpCmd(answer);
                                return;
                            }

                            mServer.Write(msgShutdonwZone);
                        }
                        else
                        {
                            foreach (var item in ManagerServerManager.ServerList)
                            {
                                item.Value.Write(msgShutdonwZone);
                            }
                            MSG_GGATE_SHUTDOWN_GATE msg = new MSG_GGATE_SHUTDOWN_GATE();
                            GateServerManager.Broadcast(msg);

                            MSG_GA_SHUTDOWN_GATE gaMsg = new MSG_GA_SHUTDOWN_GATE();
                            AnalysisServerManager.Broadcast(gaMsg);
                        }
                    }
                    break;
                case "shutdownmanager":
                    help = "shutdownmanager Usage: shutdownmanager mainId; if mainId == 0, will close all manager";
                    if (args.Length != 1)
                    {
                        //Log.Warn(help);
                        answer = help;
                        session.AnswerHttpCmd(answer);
                        return;
                    }
                    int managerMainId;
                    if (int.TryParse(args[0], out managerMainId) == false)
                    {
                        //Log.Warn(help);
                        answer = help;
                        session.AnswerHttpCmd(answer);
                        return;
                    }
                    MSG_GM_SHUTDOWN_MANAGER msgShutdownManager = new MSG_GM_SHUTDOWN_MANAGER();
                    if (managerMainId != 0)
                    {
                        ManagerServer mServer = null;
                        mServer = (ManagerServer)ManagerServerManager.GetSinglePointServer(managerMainId);
                        if (mServer == null || mServer.State != ServerState.Started)
                        {
                            //Log.Warn("shutdown failed: Manager server {0} not exist", managerMainId);
                            answer = String.Format("shutdown failed: Manager server {0} not exist", managerMainId);
                            session.AnswerHttpCmd(answer);
                            return;
                        }
                        mServer.Write(msgShutdownManager);
                    }
                    else
                    {
                        foreach (var item in ManagerServerManager.ServerList)
                        {
                            item.Value.Write(msgShutdownManager);
                        }
                    }
                    break;
                case "shutdownbarrack":
                    help = "shutdownbarrack Usage: shutdownbarrack country; if country == 0, will close all barracks";
                    if (args.Length != 1)
                    {
                        //Log.Warn(help);
                        answer = help;
                        session.AnswerHttpCmd(answer);
                        return;
                    }
                    int barrackId;
                    if (int.TryParse(args[0], out barrackId) == false)
                    {
                        //Log.Warn(help);
                        answer = help;
                        session.AnswerHttpCmd(answer);
                        return;
                    }
                    MSG_GB_SHUTDOWN msgShutdownBarrack = new MSG_GB_SHUTDOWN();
                    if (barrackId != 0)
                    {
                        BarrackServer barrackServer = null;
                        barrackServer = (BarrackServer)BarrackServerManager.GetSinglePointServer(barrackId);
                        if (barrackServer == null)
                        {
                            //Log.Warn("shutdown failed: barrack server {0} not exist", barrackId);
                            answer = String.Format("shutdown failed: barrack server {0} not exist", barrackId);
                            session.AnswerHttpCmd(answer);
                            return;
                        }
                        barrackServer.Write(msgShutdownBarrack);
                    }
                    else
                    {
                        foreach (var item in BarrackServerManager.ServerList)
                        {
                            item.Value.Write(msgShutdownBarrack);
                        }
                    }
                    break;
                case "shutdownrelation":
                    help = "shutdownrelation Usage: shutdownrelation country; if country == 0, will close all relations";
                    if (args.Length != 1)
                    {
                        //Log.Warn(help);
                        answer = help;
                        session.AnswerHttpCmd(answer);
                        return;
                    }
                    int relationCountryId;
                    if (int.TryParse(args[0], out relationCountryId) == false)
                    {
                        //Log.Warn(help);
                        answer = help;
                        session.AnswerHttpCmd(answer);
                        return;
                    }
                    MSG_GM_SHUTDOWN_RELATION msgShutdownRelation = new MSG_GM_SHUTDOWN_RELATION();
                    if (relationCountryId != 0)
                    {
                        ManagerServer mserverForRelation = null;
                        mserverForRelation = (ManagerServer)ManagerServerManager.GetSinglePointServer(relationCountryId);
                        if (mserverForRelation == null)
                        {
                            //Log.Warn("shutdown failed: relation server {0} not exist", relationCountryId);
                            answer = String.Format("shutdown failed: relation server {0} not exist", relationCountryId);
                            session.AnswerHttpCmd(answer);
                            return;
                        }
                        mserverForRelation.Write(msgShutdownRelation);
                    }
                    else
                    {
                        foreach (var item in ManagerServerManager.ServerList)
                        {
                            item.Value.Write(msgShutdownRelation);
                        }
                    }
                    break;

                case "help":
                    {
                        //Log.Write("shutdown Usage: ");
                        answer = @"updatexml Usage: updatexml mainId;
shutdown Usage: shutdown mainId subId; if subId == 0, will close all zone in mainId 
shutdownmanager Usage: shutdownmanager mainId; if mainId == 0, will close all manager
shutdownbarrack Usage: shutdownbarrack country; if country == 0, will close all barracks
shutdownrelation Usage: shutdownrelation country; if country == 0, will close all relations";
                        session.AnswerHttpCmd(answer);
                    }
                    break;
                default:
                    //Log.Warn("command {0} not support, try command 'help' for more infomation", session.Cmd);
                    answer = String.Format("command {0} not support, try command 'help' for more infomation", session.Cmd);
                    session.AnswerHttpCmd(answer);
                    break;
            }
            session.AnswerHttpCmd(answer);
        }

    }
}
