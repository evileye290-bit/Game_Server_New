using CommonUtility;
using DBUtility;
using Google.Protobuf.Collections;
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
    public class ShovelTreasureManager
    {
        public DateTime StartTime = DateTime.MinValue;
        public int CurPuzzleType { get; private set; }
        public int ZoneTreasureId { get; set; }
        public int PassRewardId { get; private set; }
        //key:puzzleType
        private Dictionary<int, List<int>> lightedPuzzleList = new Dictionary<int, List<int>>();
        //key:puzzleType 
        private Dictionary<int, bool> puzzleRefresh = new Dictionary<int, bool>();
        //key:rewardId
        private List<int> shoveGameRewards = new List<int>();

        public bool IsRevive = false;

        public ulong TreasureMapUid { get; private set; }

        public PlayerChar Owner { get; private set; }

        public ShovelTreasureManager(PlayerChar player)
        {
            this.Owner = player;
        }

        public void BindTreasurePuzzleInfo(DbPuzzleTreasure puzzleItem)
        {
            if (puzzleItem == null)
            {
                return;
            }
            List<int> lightedList = new List<int>();
            string[] info = StringSplit.GetArray("|", puzzleItem.PuzzeInfo);
            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].ToInt() == 1)
                {
                    lightedList.Add(i+1);
                }
            }
            lightedPuzzleList.Add(puzzleItem.Type, lightedList);
            puzzleRefresh.Add(puzzleItem.Type, puzzleItem.NeedRefresh);
            CurPuzzleType = puzzleItem.Type;
        }

        public void RecordShovelGameStartTime(DateTime now)
        {
            StartTime = now;
        }

        public void UpdateGameRewards(List<int> gameRewards, int passRewardId)
        {
            shoveGameRewards.Clear();
            shoveGameRewards.AddRange(gameRewards);
            PassRewardId = passRewardId;
        }

        public List<int> GetGameRewards()
        {
            return shoveGameRewards;
        }

        public bool RandomPuzzleType(bool needRefresh)
        {
            bool change = false;
            int randResult = ShovelTreasureLibrary.RandomPuzzleType(CurPuzzleType, lightedPuzzleList, needRefresh);
            if (randResult != CurPuzzleType)
            {
                CurPuzzleType = randResult;
                change = true;
            }
            if (puzzleRefresh.ContainsKey(CurPuzzleType) && puzzleRefresh[CurPuzzleType])
            {
                change = true;
            }
            puzzleRefresh[CurPuzzleType] = false;
            return change;
        }

        public void RecordTreasureMapUid(NormalItem item)
        {
            TreasureMapUid = item.Uid;
        }

        public List<int> GetLightedPuzzleList()
        {
            List<int> list;
            lightedPuzzleList.TryGetValue(CurPuzzleType, out list);
            return list;
        }

        public bool CheckNeedInsertPuzzle()
        {
            return lightedPuzzleList.Count == 0 && CurPuzzleType == 0;
        }

        public void SetLightedPuzzle(int index)
        {
            List<int> lightedList;
            if (lightedPuzzleList.TryGetValue(CurPuzzleType, out lightedList))
            {
                if (!lightedList.Contains(index))
                {
                    lightedList.Add(index);
                }
            }
            else
            {
                lightedList = new List<int>() { index };               
                lightedPuzzleList.Add(CurPuzzleType, lightedList);
            }
        }

        public bool CheckPuzzleIsLighted(int index)
        {
            List<int> lightedList;
            if (lightedPuzzleList.TryGetValue(CurPuzzleType, out lightedList))
            {
                if (lightedList.Contains(index))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckPuzzleFinished()
        {
            List<int> lightedList;
            lightedPuzzleList.TryGetValue(CurPuzzleType, out lightedList);
            if (lightedList == null)
            {
                return false;
            }
            if (lightedList.Count != ShovelTreasureLibrary.PuzzleFinishCount)
            {
                return false;
            }
            for (int i = 1; i <= ShovelTreasureLibrary.PuzzleFinishCount; ++i)
            {
                if (!lightedList.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }

        public string PuzzleItemToString()
        {
            List<int> lightedList;
            lightedPuzzleList.TryGetValue(CurPuzzleType, out lightedList);
            List<int> items = new List<int>();
            for (int i = 1; i <= ShovelTreasureLibrary.PuzzleFinishCount; ++i)
            {
                if (lightedList == null)
                {
                    items.Add(0);
                }
                else
                {
                    items.Add(lightedList.Contains(i) ? 1 : 0);
                }
            }
            return string.Join("|", items);
        }

        public void SyncDBShovelInsertPuzzleInfo()
        {
            Owner.server.GameDBPool.Call(new QueryInsertTreasurePuzzle(Owner.Uid, CurPuzzleType, GetPuzzleRefreshFlag()));
        }

        public void SyncDBShovelUpdatePuzzleInfo()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateTreasurePuzzle(Owner.Uid, CurPuzzleType, PuzzleItemToString()));
        }

        public void ResetPuzzeItemInfo()
        {
            List<int> list;
            if (lightedPuzzleList.TryGetValue(CurPuzzleType, out list))
            {
                list.Clear();
                List<int> items = new List<int>();
                for (int i = 1; i <= ShovelTreasureLibrary.PuzzleFinishCount; ++i)
                {
                    items.Add(0);
                }
                string puzzleItemInfo = string.Join("|", items);
                Owner.server.GameDBPool.Call(new QueryUpdateTreasurePuzzle(Owner.Uid, CurPuzzleType, puzzleItemInfo));
            }
        }

        public void UpdateRefreshFlag()
        {
            bool refresh;
            if (puzzleRefresh.TryGetValue(CurPuzzleType, out refresh))
            {
                puzzleRefresh[CurPuzzleType] = true;
            }
            SyncDbPuzzleRefershInfo();
        }

        public void SyncDbPuzzleRefershInfo()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateTreasurePuzzleRefreshFlag(Owner.Uid, CurPuzzleType, GetPuzzleRefreshFlag()));
        }

        public bool CheckNeedRandom()
        {
            if (CurPuzzleType == 0)
            {
                puzzleRefresh[CurPuzzleType] = true;
            }           
            return puzzleRefresh[CurPuzzleType];
        }

        private int GetPuzzleRefreshFlag()
        {
            if (puzzleRefresh[CurPuzzleType])
            {
                return 1;
            }
            return 0;
        }

        public int GetRandomPassRewards(NormalItem treasureMap)
        {
            return ShovelTreasureLibrary.GetRandomPassRewards(treasureMap.ItemModel.Quality);
        }

        public List<int> GetRandomShovelRewardsList(NormalItem treasureMap)
        {
            return ShovelTreasureLibrary.GetRandomShovelRewardsList(treasureMap.ItemModel.Quality);
        }

        public string GetCheckPointBasicRewardsById(int checkPointId, NormalItem treasureMap)
        {
            return ShovelTreasureLibrary.GetCheckPointBasicRewardsById(checkPointId, treasureMap.ItemModel.Quality);
        }

        public void ResetTeasureMapRecord()
        {
            TreasureMapUid = 0;
            PassRewardId = 0;
            shoveGameRewards.Clear();
        }

        public ZMZ_SHOVEL_TREASURE_INFO GenerateShovelTreasureInfoTransformMsg()
        {
            ZMZ_SHOVEL_TREASURE_INFO msg = new ZMZ_SHOVEL_TREASURE_INFO();
            msg.StartTime = Timestamp.GetUnixTimeStampSeconds(StartTime);
            msg.CurPuzzleType = CurPuzzleType;
            msg.ZoneTreasureId = ZoneTreasureId;
            msg.PassRewardId = PassRewardId;
            msg.TreasureMapUid = TreasureMapUid;
            
            msg.PuzzleList.AddRange(GenerateLightedPuzzleInfo());
            msg.RefreshList.AddRange(GeneratePuzzleRefreshInfo());
            msg.GameRewards.AddRange(shoveGameRewards);
            return msg;
        }

        private RepeatedField<ZMZ_PUZZLE_INFO> GenerateLightedPuzzleInfo()
        {
            RepeatedField<ZMZ_PUZZLE_INFO> list = new RepeatedField<ZMZ_PUZZLE_INFO>();
            foreach (var kv in lightedPuzzleList)
            {
                ZMZ_PUZZLE_INFO info = new ZMZ_PUZZLE_INFO();
                info.PuzzleType = kv.Key;
                foreach (var id in kv.Value)
                {
                    info.LightedList.Add(id);
                }
                list.Add(info);
            }
            return list;
        }

        private RepeatedField<ZMZ_PUZZLE_REFRESH> GeneratePuzzleRefreshInfo()
        {
            RepeatedField<ZMZ_PUZZLE_REFRESH> list = new RepeatedField<ZMZ_PUZZLE_REFRESH>();
            foreach (var kv in puzzleRefresh)
            {
                ZMZ_PUZZLE_REFRESH info = new ZMZ_PUZZLE_REFRESH() { PuzzleType = kv.Key, Refresh = kv.Value};            
                list.Add(info);
            }
            return list;
        }

        public void LoadShovelTreasureInfoTransform(ZMZ_SHOVEL_TREASURE_INFO info)
        {
            StartTime = Timestamp.TimeStampToDateTime(info.StartTime);
            CurPuzzleType = info.CurPuzzleType;
            ZoneTreasureId = info.ZoneTreasureId;
            PassRewardId = info.PassRewardId;
            TreasureMapUid = info.TreasureMapUid;

            foreach (var puzzle in info.PuzzleList)
            {
                List<int> list = new List<int>();
                foreach (var id in puzzle.LightedList)
                {
                    list.Add(id);
                }
                lightedPuzzleList.Add(puzzle.PuzzleType, list);
            }
            foreach (var item in info.RefreshList)
            {
                puzzleRefresh.Add(item.PuzzleType, item.Refresh);
            }      
            shoveGameRewards.AddRange(info.GameRewards);
        }
    }
}
