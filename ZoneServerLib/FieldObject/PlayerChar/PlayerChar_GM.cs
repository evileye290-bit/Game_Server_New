using CommonUtility;
using EnumerateUtility;
using EpPathFinding;
using JumpPointSearch;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //GM 指令
        public bool CheckGMCommand(string content)
        {
            if(!content.StartsWith("//"))
            {
                return false;
            }
            content = content.ToLower();
            string[] cmdList = content.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if(cmdList.Length < 2 )
            {
                return true;
            }
            DungeonMap dungeon;
            switch (cmdList[1])
            {
                case "speed":
                    if(cmdList.Length < 3)
                    {
                        return true;
                    }
                    float speed;
                    if (float.TryParse(cmdList[2], out speed))
                    {
                        SetNatureBaseValue(CommonUtility.NatureType.PRO_SPD, (int)(speed * 10000));
                        BroadcastSimpleInfo();
                    }
                    break;
                case "arena":
                    if (cmdList.Length < 3)
                    {
                        return true;
                    }
                    if (float.TryParse(cmdList[2], out speed))
                    {
                        EnterArenaMap(1);
                    }
                    break;
                case "arenauid":
                    if (cmdList.Length < 3)
                    {
                        return true;
                    }
                    int uid;
                    if (int.TryParse(cmdList[2], out uid))
                    {
                        EnterVersusMapByUid(uid);
                    }
                    break;
                case "createrdungeon":
                    if (cmdList.Length < 3)
                    {
                        return true;
                    }
                    int robotDungeon;
                    if (int.TryParse(cmdList[2], out robotDungeon))
                    {
                        CreateRobotDungeon(robotDungeon);
                    }
                    break;
                case "createdungeon":
                    if (cmdList.Length < 3)
                    {
                        return true;
                    }
                    int dungeonId;
                    if(int.TryParse(cmdList[2], out dungeonId))
                    {
                        DungeonMap map =  CreateDungeon(dungeonId);
                        if (map?.CanSkipBattle() == true)
                        {
                            map.SetSpeedUp(true);
                        }
                    }
                    break;

                case "rdungeon":
                    if (cmdList.Length < 4)
                    {
                        return true;
                    }
                    int rank;
                    if (int.TryParse(cmdList[2], out dungeonId) && int.TryParse(cmdList[3],out rank))
                    {
                        CreateRobotRankDungeon(dungeonId,rank);
                    }
                    break;
                case "leavedungeon":
                    dungeon = currentMap as DungeonMap;
                    LeaveDungeon();
                    break;
                case "gc":
                    {
                        GC.Collect();
                    }
                    break;
                case "reward":
                    if (cmdList.Length < 3)
                    {
                        return true;
                    }
                    string reward = cmdList[2];
                    RewardManager rewards = new RewardManager();
                    rewards.InitSimpleReward(reward);
                    AddRewards(rewards, ObtainWay.GM);
                    break;
                case "map":
                    if(cmdList.Length < 4)
                    {
                        return true;
                    }
                    int mapId;
                    int channel;
                    if(int.TryParse(cmdList[2], out mapId) && int.TryParse(cmdList[3], out channel))
                    {
                        MapModel mapModel = MapLibrary.GetMap(mapId);
                        if(mapModel == null)
                        {
                            return true;
                        }
                        AskForEnterMap(mapId, channel, mapModel.BeginPos, true);
                        return true;
                    }
                    break;
                case "closedungeon":
                    if(CurDungeon == null)
                    {
                        break;
                    }
                    //LeaveDungeon();
                    CurDungeon.Close();
                    break;
                case "mainhero":
                    if (cmdList.Length < 4)
                    {
                        ChangeMainHero(int.Parse(cmdList[2]));
                        return false;
                    }
                    break;
                case "addbuff":
                    if(cmdList.Length < 3 || !CurrentMap.IsDungeon)
                    {
                        return false;
                    }
                    AddBuff(this, int.Parse(cmdList[2]), 1);
                    break;
                case "run":
                    // run x y
                    if(cmdList.Length < 4)
                    {
                        return false;
                    }
                    Vec2 dest = new Vec2(float.Parse(cmdList[2]), float.Parse(cmdList[3]));
                    MoveHandler.NeedFindPath = true;
                    MoveHandler.UseNewJps = false;
                    //Log.Debug("from {0} {1}", pc.MoveHandler.CurPosition.x, pc.MoveHandler.CurPosition.Y);
                    //Log.Debug("to   {0} {1}", msg.x, msg.y);
                    //Log.Debug("duration {0}", pc.MoveHandler.GetDuration(new Vec2(msg.x, msg.y), pc.MoveHandler.CurPosition));

                    SetDestination(dest);
                    FsmManager.SetNextFsmStateType(FsmStateType.RUN, true);
                    break;

                case "run2":
                    // run2 x y
                    if (cmdList.Length < 4)
                    {
                        return false;
                    }
                    Vec2 dest2 = new Vec2(float.Parse(cmdList[2]), float.Parse(cmdList[3]));

                    int startX = (int)Math.Round(Position.x);
                    int startY = (int)Math.Round(Position.y);
                    int endX = (int)Math.Round(dest2.x);
                    int endY = (int)Math.Round(dest2.y);

                    //long oldJps = currentMap.Model.TestJps(startX, startY, endX, endY, 1000, "old jps");
                    //Log.Warn($"old jps takes mil seconds {oldJps}");

                    //long oldJpsBig = currentMap.Model.TestJps(startX, startY, endX, endY, 1000, "old jps big");
                    //Log.Warn($"old jps big takes mil seconds {oldJpsBig}");

                    //long newJpsMilSec = currentMap.Model.TestJps(startX, startY, endX, endY, 1000, "new jps");
                    //Log.Warn($"new jps takes mil seconds {newJpsMilSec} in 1000");

                    //long newJpsMilSec2 = currentMap.Model.TestJps(startX, startY, endX, endY, 1000, "new jps big");
                    //Log.Warn($"new jps big takes mil seconds {newJpsMilSec2} in 1000");

                    //long newJpsWithRayCastMilSec = currentMap.Model.TestJps(startX, startY, endX, endY, 1000, "jps raycast");
                    //Log.Warn($" jps rayCast takes mil seconds {newJpsWithRayCastMilSec} in 1000");

                    //long newJpsWithRayCastMilSec2 = currentMap.Model.TestJps(startX, startY, endX, endY, 1000, "jps big raycast");
                    //Log.Warn($"jps rayCast big takes mil seconds {newJpsWithRayCastMilSec2} in 1000");

                    CurrentMap.EnableDynamicGrid(true);
                    MoveHandler.NeedFindPath = true;
                    MoveHandler.UseNewJps = true;
                    SetDestination(dest2);
                    FsmManager.SetNextFsmStateType(FsmStateType.RUN, true);
                    break;
                case "crosschallenge":
                    GetCrossChallengePreliminaryChallenger();
                    break;
                case "ccfinal":
                    ShowCrossChallengeFinalsInfo(1);
                    break;
                case "1":
                {
                    var dicHero = HeroLibrary.GetHeroAllModel();
                    foreach (var hero in dicHero)
                    {
                        CheckGMCommand("// reward " + hero.Key + ":7:200");
                    }
                    break;
                }
                case "testrefresh":
                {
                    SpaceTimeRefreshCardPool(false);
                    break;
                }
                case "teststshop":
                {
                    if (cmdList.Length < 3)
                    {
                        break;
                    }
                    
                    int iProductId = 0;
                    int.TryParse(cmdList[2], out iProductId);
                    object[] arrParam = new object[cmdList.Length - 3];
                    for (int i = 3; i < cmdList.Length; i++)
                    {
                        int iParam = 0;
                        int.TryParse(cmdList[i], out iParam);
                        arrParam[i - 3] = iParam;
                    }

                    OptProduct(iProductId, arrParam);
                    break;
                }
                case "randomproduct":
                {
                    SpaceTimeTowerMng.RandomProduct(1);
                    break;
                }
                case "teststonewall":
                {
                    if (cmdList.Length > 3)
                    {
                        int.TryParse(cmdList[2], out int iType);
                        int.TryParse(cmdList[3], out int iTbId);
                        //HandleGetStoneWallAllReward(iType, iTbId, true);
                    }
                    break;
                }
                default:
                    break;
            }
            return true;
        }
    }
}
