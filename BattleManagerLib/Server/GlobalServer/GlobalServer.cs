using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using SocketShared;
using ServerShared;
using Logger;
using Message.Global.Protocol.GBM;
using Message.IdGenerator;
using Message.BattleManager.Protocol.BMBattle;
using Message.BattleManager.Protocol.BMG;
using System.Net;
using DBUtility;
using System.Text.RegularExpressions;
using ServerFrame;
using MessagePacker;

namespace BattleManagerServerLib
{
    public class GlobalServer: BaseGlobalServer
    {
        private BattleManagerServerApi Api
        { get { return (BattleManagerServerApi)api; } }
        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GBM_SHUTDOWN_BATTLEMANAGER>.Value, OnResponse_ShutDown_BattleManager);
            AddResponser(Id<MSG_GBM_SHUTDOWN_BATTLE>.Value, OnResponse_ShutDown_Battle);
            AddResponser(Id<MSG_GBM_BATTLE_INFO>.Value, OnResponse_Battle_Info);
            AddResponser(Id<MSG_GBM_ALL_BATTLE_INFO>.Value, OnResponse_AllBattleInfo);
            AddResponser(Id<MSG_GBM_SET_BATTLE_FPS>.Value, OnResponse_SetBattleFPS);
            AddResponser(Id<MSG_GBM_SET_FPS>.Value, OnResponse_SetFPS);
            AddResponser(Id<MSG_GBM_FPS_INFO>.Value, OnResponse_Fps_Info);
            AddResponser(Id<MSG_GBM_UPDATE_XML>.Value, OnResponse_UpdateXml);
            //ResponserEnd
        }

        public void OnResponse_ShutDown_BattleManager(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown battle manager");
            CONST.ALARM_OPEN = false;
            api.StopServer(1);

            MSG_BMBattle_SHUTDOWN_BATTLE msg2battle = new MSG_BMBattle_SHUTDOWN_BATTLE();
            // 关闭所有battle
            Api.BattleServerManager.Broadcast(msg2battle);
        }

        private void OnResponse_ShutDown_Battle(MemoryStream stream, int uid = 0)
        {
            MSG_GBM_SHUTDOWN_BATTLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBM_SHUTDOWN_BATTLE>(stream);
            MSG_BMBattle_SHUTDOWN_BATTLE msg2battle = new MSG_BMBattle_SHUTDOWN_BATTLE();
            if (msg.SubId == 0)
            {
                Log.Warn("global request shutdown all battles");
                // 关闭所有battle
                Api.BattleServerManager.Broadcast(msg2battle);
            }
            else
            {
                Log.Warn("global request shutdown battle sub {0} ", msg.SubId);
                FrontendServer battleServer = Api.BattleServerManager.GetServer(api.ClusterId, msg.SubId);
                if (battleServer !=null)
                {
                    battleServer.Write(msg2battle);
                }
            }
            Api.BattleServerManager.CalcBattleServer();
        }

        private void OnResponse_Battle_Info(MemoryStream stream, int uid = 0)
        {
            MSG_GBM_BATTLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBM_BATTLE_INFO>(stream);
            MSG_BMG_COMMAND_RESULT msg2global = new MSG_BMG_COMMAND_RESULT();

            BattleServer battleServer = (BattleServer)Api.BattleServerManager.GetServer(msg.MainId, msg.SubId);
            if (battleServer ==null)
            {
                msg2global.Success = false;
                msg2global.Info.Add(String.Format("battleInfo main {0} sub {1} failed: find battle failed", msg.MainId, msg.SubId));
                Write(msg2global);
                return;
            }
            else
            {
                msg2global.Success = true;
                msg2global.Info.Add(String.Format("battle main {0} sub {1} sleep time {2} frame {3} memory {4} battle count {5}",
                    battleServer.MainId, battleServer.SubId, battleServer.SleepTime, battleServer.FrameCount,battleServer.Memory,
                    battleServer.Battlegroundcount));
            }
            Write(msg2global);
        }

        private void OnResponse_AllBattleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GBM_ALL_BATTLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBM_ALL_BATTLE_INFO>(stream);
            Log.Write("global request main all battle info");
            BattleServerManager battleManager = Api.BattleServerManager;
            MSG_BMG_COMMAND_RESULT response = new MSG_BMG_COMMAND_RESULT();
            if (battleManager == null)
            {
                response.Success = false;
                response.Info.Add(String.Format("AllBattleInfo main {0} failed: find battle manager failed", msg.MainId));
                Write(response);
                return;
            }

            // 显示该main id下所有battle的人数 CPU 帧率
            response.Success = true;
            int totalCount = 0;
            foreach (var item in battleManager.ServerList)
            {
                BattleServer battle = (BattleServer)item.Value;
             
                response.Info.Add(String.Format("battle sub {0} sleep time {1} frame {2} memory {3} battlecount {4} ",
                    battle.SubId, battle.SleepTime, battle.FrameCount, battle.Memory,battle.Battlegroundcount));
                totalCount += battle.Battlegroundcount;
            }
            response.Info.Add(String.Format("total battlegrouds count {0}", totalCount));
            Write(response);
        }

        private void OnResponse_SetBattleFPS(MemoryStream stream, int uid = 0)
        {
            MSG_GBM_SET_BATTLE_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBM_SET_BATTLE_FPS>(stream);
            MSG_BMG_COMMAND_RESULT response = new MSG_BMG_COMMAND_RESULT();
            if (Api.BattleServerManager ==null)
            {
                response.Success = false;
                response.Info.Add(String.Format("setbattleFps main {0} sub {1} failed: find battle manager failed", msg.MainId, msg.SubId));
                Write(response);
                return;
            }

            MSG_BMBattle_SET_FPS notify = new MSG_BMBattle_SET_FPS();
            notify.FPS = msg.Fps;

            if (msg.SubId == 0)
            {
                //foreach (var zone in zoneManager.ZoneList)
                //{
                //    zone.Value.State = ServerState.Stopping;
                //    zone.Value.Write(notify);
                //}
                Api.BattleServerManager.Broadcast(notify);
            }
            else
            {
                FrontendServer battleServer = Api.BattleServerManager.GetServer(msg.MainId, msg.SubId);
                if (battleServer != null)
                {
                    battleServer.Write(notify);
                }
               else
                {
                    response.Success = false;
                    response.Info.Add(String.Format("setbattleFps main {0} sub {1} failed: find battle failed", msg.MainId, msg.SubId));
                    Write(response);
                    return;
                }
            }
        }

        private void OnResponse_SetFPS(MemoryStream stream, int uid = 0)
        {
            MSG_GBM_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBM_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);

            MSG_BMG_COMMAND_RESULT response = new MSG_BMG_COMMAND_RESULT();
            response.Success = true;
            response.Info.Add(String.Format("setbattleFps main {0} successful",api.MainId));
            Write(response);
        }

        private void OnResponse_Fps_Info(MemoryStream stream, int uid = 0)
        {
            MSG_GBM_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBM_FPS_INFO>(stream);
            FpsAndCpuInfo info =  api.Fps.GetFPSAndCpuInfo();
            MSG_BMG_COMMAND_RESULT msg2global = new MSG_BMG_COMMAND_RESULT();
            if (info == null)
            {
                msg2global.Success = false;
                msg2global.Info.Add(String.Format("battlemanager main {0} getfps fail",
                api.MainId));
            }
            else
            {
                msg2global.Success = true;
                msg2global.Info.Add(String.Format("battlemanager main {0} frame {1} sleep time {2} memory {3}",
                api.MainId, info.fps, info.sleepTime, info.memorySize));
            }
            Write(msg2global);
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            api.UpdateXml();
            MSG_BMBattle_UPDATE_XML msg = new MSG_BMBattle_UPDATE_XML();
            Api.BattleServerManager.Broadcast(msg);
            Log.Write("GM update xml");
        }
    }
}