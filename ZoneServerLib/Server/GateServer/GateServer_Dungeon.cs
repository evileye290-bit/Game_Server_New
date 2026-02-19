using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerShared;
using System.IO;

namespace ZoneServerLib
{
    partial class GateServer
    {
        private void OnResponse_CreateDungeon(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_CREATE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CREATE_DUNGEON>(stream);          
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;

            Log.Write($"player {player.Uid} request to create dungeon {msg.DungeonId}");

            player.ManagerCreateDungeon(msg.DungeonId, msg.HuntingHelp);
        }


        private void OnResponse_LeaveDungeon(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_LEAVE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LEAVE_DUNGEON>(stream);         
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;

            Log.Write($"player {player.Uid} request to leave dungeon");
            player.LeaveDungeon();
        }

        private void OnResponse_DungeonStopBattle(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_DUNGEON_STOP_BATTLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DUNGEON_STOP_BATTLE>(stream);           
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;

            bool quitImm = true;
            TeamDungeonMap teamDungeon = player.CurrentMap as TeamDungeonMap;
            //组队副本需要队友确认才能退出
            if (teamDungeon != null && player.Team!= null)
            {
                foreach (var kv in player.Team.MemberList)
                {
                    if (!kv.Value.IsRobot && kv.Key != Uid && kv.Value.IsOnline)
                    {
                        PlayerChar member = Api.PCManager.FindPc(kv.Key);
                        if (member != null)
                        {
                            quitImm = false;
                            member.Write(new MSG_ZGC_REQUEST_QUIT_DUNGEON() { RequestUid = msg.Uid });
                        }
                    }
                }
            }

            if (quitImm)
            {
                player.StopDungeon();
            }
            else
            {
                if (teamDungeon.VerifyQuitTeamTimerId == 0)
                {
                    //延迟退出
                    long timerId = TimerManager.Instance.NewOnceTimer(Timestamp.GetUnixTimeStamp(Api.Now().AddSeconds(TeamLibrary.VerifyQuitTeam)), obj => player.StopDungeon());
                    teamDungeon.VerifyQuitTeamTimerId = timerId;
                }
            }

            Log.Write($"player {player.Uid} request to stop dungeon battle");
        }

        private void OnResponse_DungeonResult(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_DUNGEON_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DUNGEON_RESULT>(stream);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null) return;

            Log.Write($"player {player.Uid} request to dungeon result");
            player.SetDungeonResult(msg.Result);
        }

        private void OnResponse_DungeonBattleData(MemoryStream stream, int Uid = 0)
        {
            //MSG_GateZ_DUNGEON_BATTLE_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DUNGEON_BATTLE_DATA>(stream);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null) return;

            Log.Write($"player {player.Uid} request to dungeon battle data");
            player.Request_BattleDungeonData();
        }

        private void OnResponse_VerifyQuitDungeon(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_VERIFY_QUIT_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_VERIFY_QUIT_DUNGEON>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.RequestUid);
            if (player == null) return;
            Log.Write($"player {player.Uid} request to verify quit dungeon");

            if (msg.Verifyed)
            {
                player.StopDungeon();
            }
            else
            {
                //移除定时器
                TeamDungeonMap teamDungeon = player.CurrentMap as TeamDungeonMap;
                if (teamDungeon != null)
                {
                    TimerManager.Instance.Remove(teamDungeon.VerifyQuitTeamTimerId) ;
                    teamDungeon.VerifyQuitTeamTimerId = 0;
                }

                MSG_ZGC_RSPONSE_VERIFY_QUIT_DUNGEON res = new MSG_ZGC_RSPONSE_VERIFY_QUIT_DUNGEON();
                player.Write(res);
            }
        }

        private void OnResponse_DungeonSpeedUp(MemoryStream stream, int Uid = 0)
        {
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null) return;
            Log.Write($"player {Uid} request to dungeon speed up");

            MSG_ZGC_DUNGEON_SPEED_UP msg = new MSG_ZGC_DUNGEON_SPEED_UP();
            if (player.CurDungeon == null)
            {
                Log.Warn($"player {Uid} request to dungeon speed up failed: not in dungeon");
                msg.Result = (int)ErrorCode.NotInDungeon;
                player.Write(msg);
                return;
            }

            if (!player.CurDungeon.CheckSpeedUp() || !player.CurDungeon.CanSkipBattle())
            {
                Log.Warn($"player {Uid} request to dungeon speed up failed: current dungeon can not skip battle");
                msg.Result = (int)ErrorCode.CurDungeonCanNotSkipBattle;
                player.Write(msg);
                return;
            }

            MapType mapType = player.CurDungeon.GetMapType();
            if (!player.CanSkipBattle(mapType))
            {
                Log.Warn($"player {Uid} request to dungeon speed up failed: not meet map {(int)mapType} skip battle condition");
                msg.Result = (int)ErrorCode.CurDungeonCanNotSkipBattle;
                player.Write(msg);
                return;
            }

            msg.Result = (int)ErrorCode.Success;
            player.Write(msg);

            player.CurDungeon.SetSpeedUp(true);
        }

        private void OnResponse_DungeonSkipBattle(MemoryStream stream, int Uid = 0)
        {
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null) return;
            Log.Write($"player {Uid} request to dungeon skip battle");

            MSG_ZGC_DUNGEON_SKIP_BATTLE msg = new MSG_ZGC_DUNGEON_SKIP_BATTLE();
            if (player.CurDungeon == null)
            {
                Log.Warn($"player {Uid} request to dungeon skip battle failed: not in dungeon");
                msg.Result = (int)ErrorCode.NotInDungeon;
                player.Write(msg);
                return;
            }

            if (!player.CurDungeon.CanSkipBattle())
            {
                Log.Warn($"player {Uid} request to dungeon skip battle failed: current dungeon can not skip battle");
                msg.Result = (int)ErrorCode.CurDungeonCanNotSkipBattle;
                player.Write(msg);
                return;
            }

            //点击快速战斗意味着选择了连续战斗
            if (!player.SecretAreaManager.ContinueFight)
            {
                player.SecretAreaManager.ChangeContinueFightState(true);
            }

            player.CurDungeon.SkipBattle(player);

            msg.Result = (int)ErrorCode.Success;
            player.Write(msg);
        }
    }
}
