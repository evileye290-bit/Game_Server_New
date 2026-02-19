using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //传送
        public TempTaskFlyInfo tempTaskFlyInfo = new TempTaskFlyInfo();

        public ZMZ_TASK_FLY_INFO GetTaskFlyTransfrom()
        {
            ZMZ_TASK_FLY_INFO info = new ZMZ_TASK_FLY_INFO();

            info.ZoneNpcId = tempTaskFlyInfo.zoneNpcId;
            info.StartX = tempTaskFlyInfo.start.X;
            info.StartY = tempTaskFlyInfo.start.Y;
            info.EndX = tempTaskFlyInfo.end.X;
            info.EndY = tempTaskFlyInfo.end.Y;
            info.NeedBlack = tempTaskFlyInfo.needBlack;
            info.NeedFlyAnim = tempTaskFlyInfo.needFlyAnim;
            info.NeedSetPos = tempTaskFlyInfo.needSetPos;
            info.IsUsing = tempTaskFlyInfo.isUsing;
            info.SyncTime = Timestamp.GetUnixTimeStampSeconds(tempTaskFlyInfo.syncTime);
            info.NeedSync = tempTaskFlyInfo.needSync;
            info.RandomLimit = tempTaskFlyInfo.randomLimit;
            info.MapId = tempTaskFlyInfo.MapId;
            info.FishEndFix = tempTaskFlyInfo.FishEndFix;

            return info;
        }

        public void LoadTaskFlyTransform(ZMZ_TASK_FLY_INFO info)
        {
            tempTaskFlyInfo = new TempTaskFlyInfo();

            tempTaskFlyInfo.zoneNpcId = info.ZoneNpcId;
            tempTaskFlyInfo.start = new Vec2();
            tempTaskFlyInfo.end = new Vec2();
            tempTaskFlyInfo.start.X= info.StartX;
            tempTaskFlyInfo.start.Y= info.StartY;
            tempTaskFlyInfo.end.X= info.EndX;
            tempTaskFlyInfo.end.Y= info.EndY;
            tempTaskFlyInfo.needBlack = info.NeedBlack;
            tempTaskFlyInfo.needFlyAnim = info.NeedFlyAnim;
            tempTaskFlyInfo.needSetPos = info.NeedSetPos;
            tempTaskFlyInfo.isUsing = info.IsUsing;
            tempTaskFlyInfo.needSync = info.NeedSync;
            tempTaskFlyInfo.randomLimit = info.RandomLimit;
            tempTaskFlyInfo.MapId = info.MapId;
            tempTaskFlyInfo.FishEndFix = info.FishEndFix;
            tempTaskFlyInfo.syncTime = Timestamp.TimeStampToDateTime(info.SyncTime);

        }

        public void DoTaskFly(int zoneNpcId)
        {
            tempTaskFlyInfo.Clear();

            MSG_ZGC_USE_TASKFLY_ANSWER answer = new MSG_ZGC_USE_TASKFLY_ANSWER();

            tempTaskFlyInfo.zoneNpcId = zoneNpcId;
            this.FsmManager.SetNextFsmStateType(FsmStateType.IDLE, true);
            DataList dataList = DataListManager.inst.GetDataList("PathFindingConfig");
            float pathFindDisLimit = 0f;
            float randomLimit = 0f;
            foreach (var item in dataList)
            {
                Data tempData = item.Value;
                if (pathFindDisLimit < 1f)
                {
                    pathFindDisLimit = tempData.GetFloat("TaskFlyDisLimit");
                }
                randomLimit = tempData.GetFloat("TaskFlyRandomDis");
                tempTaskFlyInfo.randomLimit = randomLimit;
            }

            Data data = DataListManager.inst.GetData("ZoneNPC", zoneNpcId);
            if (data != null)
            {
                int zoneID = data.GetInt("ZoneId");
                int mapId = zoneID;
                Vec2 start = new Vec2();
                start.x = data.GetFloat("FlyPosX");
                start.y = data.GetFloat("FlyPosY");
                tempTaskFlyInfo.start = start;

                if (!CheckTaskFlyNPCStartPos(start))
                {
                    AutoPathFinding(zoneNpcId, (int)FindPathType.Npc);
                    return;
                }

                Vec2 end = new Vec2();
                end.x = data.GetFloat("PosX");
                end.y = data.GetFloat("PosZ");
                tempTaskFlyInfo.end = end;

                if (this.CurrentMap.MapId != mapId)
                {
                    tempTaskFlyInfo.needBlack = false;
                    tempTaskFlyInfo.needFlyAnim = true;
                    tempTaskFlyInfo.needSetPos = false;
                    tempTaskFlyInfo.MapId = mapId;
                }
                else
                {
                    Vec2 tempPos = this.Position;
                    Vec2 minus = new Vec2();
                    Vec2.OperatorMinus(end, tempPos, ref minus);
                    double dis = minus.GetLength();
                    if (dis > pathFindDisLimit)
                    {
                        tempTaskFlyInfo.needBlack = true;
                        tempTaskFlyInfo.needFlyAnim = true;
                        tempTaskFlyInfo.needSetPos = true;
                    }
                    else
                    {
                        tempTaskFlyInfo.needBlack = false;
                        tempTaskFlyInfo.needFlyAnim = false;
                        tempTaskFlyInfo.needSetPos = false;

                        TaskFlyPathFinding();
                    }
                }

                answer.HasAnim = tempTaskFlyInfo.needFlyAnim;
                answer.NeedBlack = tempTaskFlyInfo.needBlack;
                //answer.ErrorCode = false;

                answer.ErrorCode = (int)ErrorCode.Success;

                //回流
                Write(answer);
            }
            else
            {
                Logger.Log.Warn("player {0} DoTaskFly can not find npc {1}", Uid, zoneNpcId);
                answer.ErrorCode = (int)ErrorCode.Fail;
                //回流
                Write(answer);
            }
        }

        public void DoFishFly(int zoneNpcId)
        {

            tempTaskFlyInfo.Clear();

            MSG_ZGC_USE_TASKFLY_ANSWER answer = new MSG_ZGC_USE_TASKFLY_ANSWER();

            tempTaskFlyInfo.zoneNpcId = zoneNpcId;
            this.FsmManager.SetNextFsmStateType(FsmStateType.IDLE, true);
            DataList dataList = DataListManager.inst.GetDataList("PathFindingConfig");
            float pathFindDisLimit = 0f;
            float randomLimit = 0f;
            foreach (var item in dataList)
            {
                Data tempData = item.Value;
                if (pathFindDisLimit < 1f)
                {
                    pathFindDisLimit = tempData.GetFloat("TaskFlyDisLimit");
                }
                randomLimit = tempData.GetFloat("TaskFlyRandomDis");
                tempTaskFlyInfo.randomLimit = randomLimit;
            }

            Data data = DataListManager.inst.GetData("ZoneNPC", zoneNpcId);
            string mapName = data.GetString("zoneName");
            Data mapData = DataListManager.inst.GetData("Zone", mapName);

            int mapId = mapData.ID;
            Vec2 start = new Vec2();
            start.x = data.GetFloat("flyPosX");
            start.y = data.GetFloat("flyPosY");
            tempTaskFlyInfo.start = start;

            if (!CheckTaskFlyNPCStartPos(start))
            {
                AutoPathFinding(zoneNpcId, (int)FindPathType.Npc);
                return;
            }

            Vec2 end = new Vec2();
            end.x = data.GetFloat("genPosX");
            end.y = data.GetFloat("genPosZ");

            DataList fishList = DataListManager.inst.GetDataList("FishConfig");
            string fishPoints = "";
            string[] points =null;
            List<Vec2> endZones = new List<Vec2>();
            foreach (var item in fishList)
            {
                Data tempData = item.Value;
                fishPoints += tempData.GetString("FishingFindPathArea");
                //拿到终点
                //-62.39,45.88|-61.4,44.49|-70.37,39.36|-71.23,40.84
                points = fishPoints.Split('|');
            }

            Vec2 realEnd = new Vec2();
            float yMax = 0f, yMin = 0f, xMax = 0f, xMin = 0f;
            if (points.Count() == 4)
            {
                foreach (var tempString in points)
                {
                    Vec2 vec=new Vec2();
                    string[] xAndY= tempString.Split(',');
                    vec.x = float.Parse(xAndY[0]);
                    vec.y = float.Parse(xAndY[1]);
                    endZones.Add(vec);
                }

                foreach (Vec2 vec in endZones)
                {
                    if (yMax == 0f && xMax==0f)
                    {
                        yMax = yMin = vec.y;
                        xMin = xMax = vec.x;
                        continue;
                    }
                    if(vec.x > xMax)
                    {
                        xMax = vec.x;
                    }
                    if (vec.x < xMin)
                    {
                        xMin = vec.x;
                    }
                    if (vec.y > yMax)
                    {
                        yMax = vec.y;
                    }
                    if (vec.y < yMin)
                    {
                        yMin = vec.y;
                    }

                }

                realEnd = GetRandomFishPoint(xMin, xMax, yMin, yMax);
                int j = 0;
                while (j < 20)
                {
                    if (Vec2.IsPointInPolygon(realEnd, endZones))
                    {
                        end = realEnd;
                        break;
                    }
                    else
                    {
                        realEnd = GetRandomFishPoint(xMin, xMax, yMin, yMax);
                    }
                    
                }
            }
            tempTaskFlyInfo.end = end;
            tempTaskFlyInfo.FishEndFix = true;

            if (this.CurrentMap.MapId != mapId)
            {
                tempTaskFlyInfo.needBlack = false;
                tempTaskFlyInfo.needFlyAnim = true;
                tempTaskFlyInfo.needSetPos = false;
                tempTaskFlyInfo.MapId = mapId;
            }
            else
            {
                Vec2 tempPos = this.Position;
                Vec2 minus = new Vec2();
                Vec2.OperatorMinus(end, tempPos, ref minus);
                double dis = minus.GetLength();
                if (dis > pathFindDisLimit)
                {
                    tempTaskFlyInfo.needBlack = true;
                    tempTaskFlyInfo.needFlyAnim = true;
                    tempTaskFlyInfo.needSetPos = true;
                }
                else
                {
                    tempTaskFlyInfo.needBlack = false;
                    tempTaskFlyInfo.needFlyAnim = false;
                    tempTaskFlyInfo.needSetPos = false;

                    TaskFlyPathFinding();
                }
            }

            answer.HasAnim = tempTaskFlyInfo.needFlyAnim;
            answer.NeedBlack = tempTaskFlyInfo.needBlack;
            //answer.ErrorCode = false;

            answer.ErrorCode = (int)ErrorCode.Success;

            //回流
            Write(answer);
        }

        public bool InFishFly()
        {
            return tempTaskFlyInfo.FishEndFix;
        }

        public bool InFly()
        {
            return tempTaskFlyInfo.FishEndFix || tempTaskFlyInfo.isUsing;
        }

        public void EndFly()
        {
            tempTaskFlyInfo.Clear();
        }

        public void RefreshFishFly()
        {
            tempTaskFlyInfo.FishEndFix = false;
        }

        public Vec2 GetRandomFishPoint(float xMin,float xMax,float yMin,float yMax)
        {
            Vec2 vec = new Vec2();
            Random rand = new Random();
            vec.x = (float)(rand.NextDouble() * (xMax - xMin) + xMin);
            vec.y = (float)(rand.NextDouble() * (yMax - yMin) + yMin);
            return vec;
        }

        public bool CheckTaskFlyNPCStartPos(Vec2 pos)
        {
            if (pos.X == 0f && pos.Y == 0f)
            {
                return false;
            }
            return true;
        }

        public bool CheckPos(Vec2 pos)
        {
            float fX,fY;
            fX = pos.x;
            fY = pos.y;
            return CurrentMap.IsWalkableAt((int)Math.Round(fX), (int)Math.Round(fY), CurrentMap.HighPrecision);
        }

        public void SetFlyPositionOrChangeMap()
        {
            if (!CheckTaskFlyTempInfo())
            {
                return;
            }
            if (tempTaskFlyInfo.needSetPos) //设置位置
            {
                Transmit(tempTaskFlyInfo.start);
            }
            else if (tempTaskFlyInfo.needFlyAnim)  // 切图并设置位置
            {
                AskForEnterMap(tempTaskFlyInfo.MapId, CONST.MAIN_MAP_CHANNEL, tempTaskFlyInfo.start);
            }
            TimeSpan span = new TimeSpan(1);
            tempTaskFlyInfo.syncTime = ZoneServerApi.now + span;
            tempTaskFlyInfo.needSync = true;
            //treasureInfo.syncDelay = 3;
        }

        public void UpdateTaskFly()
        {
            if (tempTaskFlyInfo.needSync && ZoneServerApi.now > tempTaskFlyInfo.syncTime)
            {
                tempTaskFlyInfo.needSync = false;
                MSG_ZGC_TASKFLY_POSITION_SETDONE msg = new MSG_ZGC_TASKFLY_POSITION_SETDONE();
                Write(msg);
            }
        }

        public void TaskFlyPathFinding()
        {
            if (!CheckTaskFlyTempInfo())
            {
                return;
            }
            if (!CheckPos(tempTaskFlyInfo.end))
            {
                Logger.Log.Warn("player {0} TaskFly endPos ({1},{2}) can not reach",Uid,tempTaskFlyInfo.end.X,tempTaskFlyInfo.end.Y);
                return;
            }
            bool needPathFind = false;
            Vec2 tempEnd = new Vec2();
            tempEnd.X = tempTaskFlyInfo.end.X;
            tempEnd.Y = tempTaskFlyInfo.end.Y;
            if (!tempTaskFlyInfo.FishEndFix)
            {
                Vec2.RandomPos(tempEnd, tempTaskFlyInfo.randomLimit);
            }
            if (CheckTaskFlyNPCStartPos(tempTaskFlyInfo.start))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (CheckPos(tempEnd)||tempTaskFlyInfo.FishEndFix)
                    {
                        needPathFind = true;
                        tempTaskFlyInfo.isUsing = true;
                        //tempTaskFlyInfo.FishEndFix = false;
                        break;
                    }
                    else
                    {
                        tempEnd.X = tempTaskFlyInfo.end.X;
                        tempEnd.Y = tempTaskFlyInfo.end.Y;
                        Vec2.RandomPos(tempEnd, tempTaskFlyInfo.randomLimit);
                    }
                }
                if (needPathFind)
                {
                    AutoPathFinding(tempTaskFlyInfo.zoneNpcId, (int)FindPathType.TaskNPC, tempEnd);
                }
            }
        }

        public bool CheckTaskFlyTempInfo()
        {
            if (tempTaskFlyInfo == null||tempTaskFlyInfo.start==null || tempTaskFlyInfo.end==null)
            {
                Logger.Log.Warn("player {0} tempTaskFlyInfo is null",Uid);
                return false;
            }
            return true;
        }
    }
}
