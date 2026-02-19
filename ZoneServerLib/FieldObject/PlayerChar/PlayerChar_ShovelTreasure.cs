using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
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
        public ShovelTreasureManager ShovelTreasureMng { get; private set; }     
        public TreasureFlyInfo treasureFlyInfo = new TreasureFlyInfo();
        public void InitShovelTreasureManager()
        {
            ShovelTreasureMng = new ShovelTreasureManager(this);
        }

        public void InitTreasurePuzzle(DbPuzzleTreasure puzzleItem)
        {
            ShovelTreasureMng.BindTreasurePuzzleInfo(puzzleItem);
        }

        public void RandomTreasureAndGoToDestination(NormalItem item)
        {
            ShovelTreasureMng.ZoneTreasureId = ShovelTreasureLibrary.RandomTreasureId();
            DoTreasureFly(ShovelTreasureMng.ZoneTreasureId);
            ShovelTreasureMng.RecordTreasureMapUid(item);
        }

        public void GetShovelGameRewards(int checkPointId, int collideCount, int blood, bool pass)
        {
            MSG_ZGC_SHOVEL_GAME_REWARDS response = new MSG_ZGC_SHOVEL_GAME_REWARDS();
            //检查是否有藏宝图
            BaseItem item = BagManager.GetItem(ShovelTreasureMng.TreasureMapUid);
            if (item == null)
            {
                Log.Warn($"player {Uid} get shovel game rewards failed, not use treasureMap yet");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            NormalItem treasureMap = item as NormalItem;
            if (treasureMap == null)
            {
                Log.Warn($"player {Uid} get shovel game rewards failed, treasureMap not exists");
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }
            //检查是否为正常请求
            List<int> randRewardList = ShovelTreasureMng.GetGameRewards();
            if (ShovelTreasureMng.PassRewardId == 0 || randRewardList.Count == 0)
            {
                Log.Warn($"player {Uid} get shovel game rewards failed, not right request stream");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            string rewards = string.Empty;
            if (collideCount > ShovelTreasureLibrary.GameRewardMaxCount || collideCount > randRewardList.Count)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get shovel game rewards failed: collideCount {1} error", Uid, collideCount);
                Write(response);
                return;
            }
            if (collideCount == ShovelTreasureLibrary.GameRewardMaxCount && (ZoneServerApi.now - ShovelTreasureMng.StartTime).TotalSeconds < ShovelTreasureLibrary.TreasureGameTime)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get shovel game rewards failed: game consume time {1} error", Uid, (ZoneServerApi.now - ShovelTreasureMng.StartTime).TotalSeconds);
                Write(response);
                return;
            }
            //检查关卡和藏宝图品质是否匹配
            if (!CheckPointMatchTreasureMapQuality(checkPointId, treasureMap))
            {
                Log.Warn($"player {Uid} get shovel game rewards failed, checkPoint not match treaureMap quality");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            string basicRewards = ShovelTreasureMng.GetCheckPointBasicRewardsById(checkPointId, treasureMap);
            if (!string.IsNullOrEmpty(basicRewards))
            {
                rewards = string.Format("{0}|{1}|", rewards, basicRewards);
            }
         
            List<int> realRewardList = randRewardList.GetRange(0, collideCount);
            foreach (var rewardId in realRewardList)
            {
                string unitRewards = ShovelTreasureLibrary.GetShovelRewardsById(rewardId);
                if (!string.IsNullOrEmpty(unitRewards))
                {
                    rewards = string.Format("{0}|{1}|", rewards, unitRewards);
                }
            }
            if (pass && (ZoneServerApi.now - ShovelTreasureMng.StartTime).TotalSeconds >= ShovelTreasureLibrary.TreasureGameTime)
            {
                string passRewards = ShovelTreasureLibrary.GetCheckPointPassRewardsById(ShovelTreasureMng.PassRewardId);
                if (!string.IsNullOrEmpty(passRewards))
                {
                    rewards = string.Format("{0}|{1}|", rewards, passRewards);
                }
            }
            //藏宝图消耗
            UseTreaureMap(treasureMap);
            //防止封包获取高级藏宝图奖励
            ShovelTreasureMng.ResetTeasureMapRecord();

            RewardManager manager = GetSimpleReward(rewards, ObtainWay.ShovelTreasure);
            manager.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.Pass = pass;
            Write(response);

            BIRecordTreasureMap(blood, pass, manager);
            KomoeLogRecordTreasureMap(ShovelTreasureMng.IsRevive, manager.RewardList, treasureMap);
            ShovelTreasureMng.IsRevive = false;

            //通行证挖宝任务
            AddPassCardTaskNum(TaskType.ShovelTreasureNum);

            //累计完成寻宝次数发称号卡
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.ShovelTreasureCount);
        }

        public void RecordShovelGameStartTime()
        {
            ShovelTreasureMng.RecordShovelGameStartTime(ZoneServerApi.now);          
        }

        public void SendShovelRewards()
        {
            MSG_ZGC_SHOVEL_GAME_START response = new MSG_ZGC_SHOVEL_GAME_START();
            BaseItem item = BagManager.GetItem(ShovelTreasureMng.TreasureMapUid);
            if (item == null)
            {
                return;
            }
            NormalItem treasureMap = item as NormalItem;
            if (treasureMap == null)
            {
                return;
            }
            List<int> gameRewards = ShovelTreasureMng.GetRandomShovelRewardsList(treasureMap);
            int passRewardId = ShovelTreasureMng.GetRandomPassRewards(treasureMap);
            ShovelTreasureMng.UpdateGameRewards(gameRewards, passRewardId);
            response.RewardIdList.AddRange(gameRewards);
            response.PassRewardId = passRewardId;
            Write(response);
        }

        public void RandomPuzzleType()
        {
            bool needInsert = ShovelTreasureMng.CheckNeedInsertPuzzle();
            if (ShovelTreasureMng.CheckPuzzleFinished())
            {
                ShovelTreasureMng.ResetPuzzeItemInfo();
            }
            bool needRefresh = ShovelTreasureMng.CheckNeedRandom();

            if (needRefresh)
            {
                bool change = ShovelTreasureMng.RandomPuzzleType(needRefresh);
                if (change)
                {
                    ShovelTreasureMng.SyncDbPuzzleRefershInfo();
                }
            }
            if (needInsert)
            {
                ShovelTreasureMng.SyncDBShovelInsertPuzzleInfo();
            }
            NotifyPuzzleType();
           
        }

        public void LightTreasurePuzzle(int index)
        {
            MSG_ZGC_LIGHT_TREASURE_PUZZLE msg = new MSG_ZGC_LIGHT_TREASURE_PUZZLE();

            List<int> lightedList = ShovelTreasureMng.GetLightedPuzzleList();
            Dictionary<int, int> puzzleList = ShovelTreasureLibrary.GetPuzzlePiecesList(ShovelTreasureMng.CurPuzzleType);
           
            if (lightedList == null)
            {              
                lightedList = new List<int>();
            }

            if (lightedList.Count == puzzleList.Count)
            {
                Log.Warn($"player {Uid} light treasure puzzle error: finished");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (index <= 0 || index > puzzleList.Count)
            {
                Log.Warn($"player {Uid} light treasure puzzle error: index {index} error");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (lightedList.Contains(index))
            {
                Log.Warn($"player {Uid} light treasure puzzle error: puzzle {index} had lighted");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            BaseItem item = bagManager.NormalBag.GetItem(ShovelTreasureLibrary.PuzzleItemId);
            if (item == null || item.PileNum < ShovelTreasureLibrary.PuzzleCost)
            {
                Log.Warn($"player {Uid} light treasure puzzle error: item {ShovelTreasureLibrary.PuzzleItemId} not enough");
                msg.Result = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }

            item = DelItem2Bag(item, RewardType.NormalItem, ShovelTreasureLibrary.PuzzleCost, ConsumeWay.TreasurePuzzle);
            if (item != null)
            {
                SyncClientItemInfo(item);
            }

            List<int> lighted = new List<int>() { index };
            ShovelTreasureMng.SetLightedPuzzle(index);

            //判断是否有额外点亮的
            int extraLight;
            puzzleList.TryGetValue(index, out extraLight);
            if (extraLight > 0 && !ShovelTreasureMng.CheckPuzzleIsLighted(extraLight))
            {
                lighted.Add(extraLight);
                ShovelTreasureMng.SetLightedPuzzle(extraLight);
            }
            msg.CurLighted = string.Join("|", lighted);

            ShovelTreasureMng.SyncDBShovelUpdatePuzzleInfo();
            
            msg.Finished = ShovelTreasureMng.CheckPuzzleFinished();
            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            if (msg.Finished)
            {
                SendTreasurePuzzleReward(ShovelTreasureMng.CurPuzzleType);
                ShovelTreasureMng.UpdateRefreshFlag();
            }
        }   

        public void UseTreaureMap(NormalItem treasureMap)
        {
            BaseItem baseItem = DelItem2Bag(treasureMap, RewardType.NormalItem, 1, ConsumeWay.ItemUse);

            if (baseItem != null)
            {
                SyncClientItemInfo(treasureMap);
                //使用消耗品
                AddTaskNumForType(TaskType.UseConsumable, 1, true, treasureMap.SubType);
            }
        }    

        private void NotifyPuzzleType()
        {
            MSG_ZGC_RANDOM_PUZZLE response = new MSG_ZGC_RANDOM_PUZZLE();
            response.Type = ShovelTreasureMng.CurPuzzleType;
            response.PuzzleState = ShovelTreasureMng.CheckPuzzleFinished() ? 1 : 0;
            response.PuzzleInfo = ShovelTreasureMng.PuzzleItemToString();
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private static int RandomGoZoneId(Dictionary<int, int> mapNeighbors)
        {
            List<int> neighbors = new List<int>();
            neighbors.AddRange(mapNeighbors.Keys);
            Random rand = new Random();
            int result = rand.Next(0, neighbors.Count);
            return neighbors[result];
        }

        private void SendTreasurePuzzleReward(int type)
        {
            MSG_ZGC_TREASURE_PUZZLE_REWARD response = new MSG_ZGC_TREASURE_PUZZLE_REWARD();
            string reward = ShovelTreasureLibrary.GetTreasurePuzzleReward(type);
            RewardManager manager = GetSimpleReward(reward, ObtainWay.TreasurePuzzle);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }
        
        /// <summary>
        /// 查看拼图信息
        /// </summary>
        public void LookCurPuzzleInfo()
        {
            MSG_ZGC_RANDOM_PUZZLE response = new MSG_ZGC_RANDOM_PUZZLE();
            if (ShovelTreasureMng.CurPuzzleType == 0)
            {   
                Log.Warn($"player {Uid} light treasure puzzle error: finished");
                response.Result = (int)ErrorCode.NotHavePuzzleInfo;
                Write(response);
                return;
            }

            response.Type = ShovelTreasureMng.CurPuzzleType;
            response.PuzzleState = ShovelTreasureMng.CheckPuzzleFinished() ? 1 : 0;
            response.PuzzleInfo = ShovelTreasureMng.PuzzleItemToString();
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private bool CheckPointMatchTreasureMapQuality(int checkPointId, NormalItem treasureMap)
        {
            return ShovelTreasureLibrary.CheckPointMatchTreasureMapQuality(checkPointId, treasureMap.ItemModel.Quality);
        }

        public void BIRecordTreasureMap(int blood, bool pass, RewardManager rewards)
        {
            Dictionary<int, int> normalRewards;
            rewards.RewardList.TryGetValue(RewardType.NormalItem, out normalRewards);
            int num = 0;
            if (normalRewards != null)
            {
                normalRewards.TryGetValue(ShovelTreasureLibrary.PuzzleItemId, out num);
            }
            if (pass)
            {
                BIRecordTreasureMapLog(1, blood, num);
            }
            else if (blood == 0)
            {
                BIRecordTreasureMapLog(2, blood, num);
            }
            else if (!pass && blood > 0 && (ZoneServerApi.now - ShovelTreasureMng.StartTime).TotalSeconds >= ShovelTreasureLibrary.TreasureGameTime)
            {
                BIRecordTreasureMapLog(3, blood, num);
            }
        }       

        //小游戏复活
        public void ShovelGameRevive()
        {
            MSG_ZGC_SHOVEL_GAME_REVIVE response = new MSG_ZGC_SHOVEL_GAME_REVIVE();

            int diamondCost = ShovelTreasureLibrary.GameReviveDiamond;

            BaseItem curTreasureMap = BagManager.GetItem(ShovelTreasureMng.TreasureMapUid);
            if (curTreasureMap.Id == ShovelTreasureLibrary.HighTrerasureMap)
            {
                diamondCost = ShovelTreasureLibrary.GameReviveDiamondDiff;
            }

            if (!CheckCoins(CurrenciesType.diamond, diamondCost))
            {
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Log.Warn($"player {Uid} shovel game revive failed: diamond not enough");
                Write(response);
                return;
            }

            //扣钱
            DelCoins(CurrenciesType.diamond, diamondCost, ConsumeWay.ShovelGameRevive, "");

            //复活标记用于KommoeLog
            ShovelTreasureMng.IsRevive = true;

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void KomoeLogRecordTreasureMap(bool isRevive, Dictionary<RewardType, Dictionary<int, int>> rewardList, NormalItem treasureMap)
        {
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, treasureMap.Id, 1);

            List<Dictionary<string, object>> award = ParseRewardInfoToList(rewardList);

            KomoeEventLogTreasureMap("success", isRevive.ToString(), consume, award);
        }

        #region
        public void DoTreasureFly(int zoneTreasureId)
        {
            treasureFlyInfo.Clear();

            MSG_ZGC_SHOVEL_TREASURE_FLY answer = new MSG_ZGC_SHOVEL_TREASURE_FLY();

            treasureFlyInfo.zoneTreasureId = zoneTreasureId;
            this.FsmManager.SetNextFsmStateType(FsmStateType.IDLE, true);
            DataList dataList = DataListManager.inst.GetDataList("PathFindingConfig");          
            float randomLimit = 0f;
            foreach (var item in dataList)
            {
                Data tempData = item.Value;
                randomLimit = tempData.GetFloat("TaskFlyRandomDis");
                treasureFlyInfo.randomLimit = randomLimit;
            }

            Data data = DataListManager.inst.GetData("ZoneShovelTreasure", zoneTreasureId);
            int zoneID = data.GetInt("ZoneId");
            int mapId = zoneID;
            Vec2 start = new Vec2();
            start.x = data.GetFloat("FlyPosX");
            start.y = data.GetFloat("FlyPosY");
            treasureFlyInfo.start = start;

            if (!CheckTreasureFlyStartPos(start))
            {
                AutoPathFinding(zoneTreasureId, (int)FindPathType.Treasure);
                return;
            }

            Vec2 end = new Vec2();
            end.x = data.GetFloat("PosX");
            end.y = data.GetFloat("PosZ");
            treasureFlyInfo.end = end;

            if (this.CurrentMap.MapId != mapId)
            {
                treasureFlyInfo.needBlack = false;
                treasureFlyInfo.needFlyAnim = true;
                treasureFlyInfo.needSetPos = false;
                treasureFlyInfo.MapId = mapId;
            }
            else
            {
                treasureFlyInfo.needBlack = false;
                treasureFlyInfo.needFlyAnim = false;
                treasureFlyInfo.needSetPos = false;
                treasureFlyInfo.MapId = mapId;

                TreaureFlyPathFinding();
            }

            answer.HasAnim = treasureFlyInfo.needFlyAnim;
            answer.NeedBlack = treasureFlyInfo.needBlack;
            answer.ErrorCode = (int)ErrorCode.Success;

            //回流
            Write(answer);
        }

        public bool CheckTreasureFlyStartPos(Vec2 pos)
        {
            if (pos.X == 0f && pos.Y == 0f)
            {
                return false;
            }
            return true;
        }

        public void TreaureFlyPathFinding()
        {
            if (!CheckTreasureFlyInfo())
            {
                return;
            }
            if (!CheckPosition(treasureFlyInfo.end) || treasureFlyInfo.MapId != CurrentMap.MapId)
            {
                Log.Warn("player {0} TreasureFly destMap {1} endPos ({2},{3}) can not reach", Uid, treasureFlyInfo.MapId, treasureFlyInfo.end.X, treasureFlyInfo.end.Y);
                return;
            }
            bool needPathFind = false;
            Vec2 tempEnd = new Vec2();
            tempEnd.X = treasureFlyInfo.end.X;
            tempEnd.Y = treasureFlyInfo.end.Y;
            if (!treasureFlyInfo.FishEndFix)
            {
                Vec2.RandomPos(tempEnd, treasureFlyInfo.randomLimit);
            }
            if (CheckTreasureFlyStartPos(treasureFlyInfo.start))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (CheckPosition(tempEnd) || treasureFlyInfo.FishEndFix)
                    {
                        needPathFind = true;
                        treasureFlyInfo.isUsing = true;
                        //tempTaskFlyInfo.FishEndFix = false;
                        break;
                    }
                    else
                    {
                        tempEnd.X = treasureFlyInfo.end.X;
                        tempEnd.Y = treasureFlyInfo.end.Y;
                        Vec2.RandomPos(tempEnd, treasureFlyInfo.randomLimit);
                    }
                }
                if (needPathFind)
                {
                    AutoPathFinding(treasureFlyInfo.zoneTreasureId, (int)FindPathType.Treasure, tempEnd);
                }
            }
        }

        public bool CheckTreasureFlyInfo()
        {
            if (treasureFlyInfo == null || treasureFlyInfo.start == null || treasureFlyInfo.end == null)
            {
                Log.Warn("player {0} treasureFlyInfo is null", Uid);
                return false;
            }
            return true;
        }

        public bool CheckPosition(Vec2 pos)
        {
            float fX, fY;
            fX = pos.x;
            fY = pos.y;
            return CurrentMap.IsWalkableAt((int)Math.Round(fX), (int)Math.Round(fY), CurrentMap.HighPrecision);
        }

        public bool InTreasureFly()
        {
            return treasureFlyInfo.FishEndFix || treasureFlyInfo.isUsing;
        }

        public void EndTreausureFly()
        {
            treasureFlyInfo.Clear();
        }

        public void SetTreasureFlyPositionOrChangeMap()
        {
            if (!CheckTreasureFlyInfo())
            {
                return;
            }
            if (treasureFlyInfo.needSetPos) //设置位置
            {
                Transmit(treasureFlyInfo.start);
            }
            else if (treasureFlyInfo.needFlyAnim)  // 切图并设置位置
            {
                AskForEnterMap(treasureFlyInfo.MapId, CONST.MAIN_MAP_CHANNEL, treasureFlyInfo.start);
            }
            TimeSpan span = new TimeSpan(1);
            treasureFlyInfo.syncTime = ZoneServerApi.now + span;
            treasureFlyInfo.needSync = true;
        }

        public void UpdateTreasureFly()
        {
            if (treasureFlyInfo.needSync && ZoneServerApi.now > treasureFlyInfo.syncTime)
            {
                treasureFlyInfo.needSync = false; 
            }
        }

        public ZMZ_TREASURE_FLY_INFO GetTreasureFlyTransfrom()
        {
            ZMZ_TREASURE_FLY_INFO info = new ZMZ_TREASURE_FLY_INFO();

            info.ZoneTreasureId = treasureFlyInfo.zoneTreasureId;
            info.StartX = treasureFlyInfo.start.X;
            info.StartY = treasureFlyInfo.start.Y;
            info.EndX = treasureFlyInfo.end.X;
            info.EndY = treasureFlyInfo.end.Y;
            info.NeedBlack = treasureFlyInfo.needBlack;
            info.NeedFlyAnim = treasureFlyInfo.needFlyAnim;
            info.NeedSetPos = treasureFlyInfo.needSetPos;
            info.IsUsing = treasureFlyInfo.isUsing;
            info.SyncTime = Timestamp.GetUnixTimeStampSeconds(treasureFlyInfo.syncTime);
            info.NeedSync = treasureFlyInfo.needSync;
            info.RandomLimit = treasureFlyInfo.randomLimit;
            info.MapId = treasureFlyInfo.MapId;
            info.FishEndFix = treasureFlyInfo.FishEndFix;

            return info;
        }

        public void LoadTreasureFlyTransform(ZMZ_TREASURE_FLY_INFO info)
        {
            treasureFlyInfo = new TreasureFlyInfo();

            treasureFlyInfo.zoneTreasureId = info.ZoneTreasureId;
            treasureFlyInfo.start = new Vec2();
            treasureFlyInfo.end = new Vec2();
            treasureFlyInfo.start.X = info.StartX;
            treasureFlyInfo.start.Y = info.StartY;
            treasureFlyInfo.end.X = info.EndX;
            treasureFlyInfo.end.Y = info.EndY;
            treasureFlyInfo.needBlack = info.NeedBlack;
            treasureFlyInfo.needFlyAnim = info.NeedFlyAnim;
            treasureFlyInfo.needSetPos = info.NeedSetPos;
            treasureFlyInfo.isUsing = info.IsUsing;
            treasureFlyInfo.needSync = info.NeedSync;
            treasureFlyInfo.randomLimit = info.RandomLimit;
            treasureFlyInfo.MapId = info.MapId;
            treasureFlyInfo.FishEndFix = info.FishEndFix;
            treasureFlyInfo.syncTime = Timestamp.TimeStampToDateTime(info.SyncTime);
        }
        #endregion
    }
}
