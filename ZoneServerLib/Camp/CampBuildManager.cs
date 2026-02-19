using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using Message.Zone.Protocol.ZM;

namespace ZoneServerLib
{
    public class ChessboardItem
    {
        public int IndexId;
        public int XmlId;
        public int ItemId;
        public int ItemType;
        public int ItemCount;
        public bool IsDouble;

        public ChessboardItem(CampBuildItemData data)
        {
            XmlId = data.Id;
            ItemId = data.ItemId;
            ItemType = data.ItemType;
            ItemCount = data.ItemCount;
        }
    }

    public class CampBuildManager
    {
        public PlayerChar owner;
        private ZoneServerApi server;

        public int CurIndex;
        public int FlagCount;
        public int StepCount;
        public int DoubleLeftCount = 30;

        public int ChessboardMaxCount = 36;

        public int DoubleItemCount = 0;
        private int lastIndex;

        CampBuildPhaseData phaseData;

        //主棋盘 
        Dictionary<int, ChessboardItem> chessboardDic = new Dictionary<int, ChessboardItem>();

        //飞行点数
        public int FlyPoint = 9;

        //领过的建设宝箱数
        public int BuildBoxCount = 0;

        public int PhaseNum = 0;       

        public CampBuildManager(PlayerChar owner, ZoneServerApi server)
        {
            this.owner = owner;
            this.server = server;
        }

        internal void Init(QueryLoadCampInfo queryLoadCampBuildInfo)
        {
            if (owner.Camp == CampType.None)
            {
                return;
            }

            //加载上次退出时棋盘数据。
            PhaseNum = queryLoadCampBuildInfo.PhaseNum;
            CurIndex = queryLoadCampBuildInfo.CurGridId;
            FlagCount = queryLoadCampBuildInfo.FlagCount;
            StepCount = queryLoadCampBuildInfo.StepCount;
            DoubleLeftCount = queryLoadCampBuildInfo.DoubleLeftCount;
            DoubleItemCount = queryLoadCampBuildInfo.DoubleItemCount;
            BuildBoxCount = queryLoadCampBuildInfo.BuildBoxCount;         

            UnpackageGridInfoDbString(queryLoadCampBuildInfo.GridInfos);
            SetDouble(CurIndex);

            phaseData = CampBuildLibrary.GetCampBuildPhaseData(PhaseNum);
        }

        internal void SendCampBuildBuildPhaseMsg()
        {
            MSG_ZGC_CAMPBUILD_INFO info = GetCampBuildPhaseMsg();
            owner.Write(info);
        }

        internal void SendCampBuildSyncInfoMsg()
        {
            if (PhaseNum == 0)
            {
                return;
            }
            MSG_ZGC_SYNC_CAMPBUILD_INFO syncInfo = GetCampBuildSyncMsg();
            owner.Write(syncInfo);
        }

        public void RefreshCampBuildAllInfo()
        {
            UpdataCampBuildPhaseNum();
     
            phaseData = CampBuildLibrary.GetCampBuildPhaseData(PhaseNum);

            //初始化数据操作
            CurIndex = 0;
            FlagCount = 0;
            StepCount = 0;

            DoubleLeftCount = phaseData.DoubleLeftStep;
            DoubleItemCount = 0;

            BuildBoxCount = 0;

            //填充整个棋盘
            NewFixChessBoard(DoubleLeftCount, DoubleItemCount, CurIndex);
            UpdataCampBuildInfo2DB();

            SendCampBuildSyncInfoMsg();
            owner.SyncChangeCounterMsg(CounterType.CampBuildRefreshDiceCount);
            SendCampBuildBuildPhaseMsg();
        }

        /// <summary>
        /// 从当前位置开始填充。双倍也一次性填充
        /// </summary>
        /// <param name="doubleLeftStepCount"></param>
        /// <param name="doubleItemCount"></param>
        /// <param name="curIndex"></param>
        private void NewFixChessBoard(int doubleLeftStepCount, int doubleItemCount, int curIndex)
        {
            //不包括旗子
            int count = ChessboardMaxCount - 1;
            //最大下标
            int maxIndex = ChessboardMaxCount - 1;

            Queue<CampBuildItemData> itemDataQueue = ChessboardItemRandom(count);
            int index = curIndex;
            chessboardDic.Clear();

            //或者 重新随机
            while (count > 0)
            {
                bool isDouble = false;
                if (index > maxIndex)
                {
                    index = 0;
                }
                if (index > 0)
                {
                    count--;

                    if (doubleLeftStepCount < 1 && doubleItemCount > 0)
                    {
                        isDouble = true;
                        doubleItemCount--;
                    }

                    CampBuildItemData itemData = itemDataQueue.Dequeue();
                    ChessboardItem item = new ChessboardItem(itemData);
                    item.IsDouble = isDouble;
                    item.IndexId = index;
                    if (itemData == null)
                    {
                        Log.Error("FixChessBoard got an error item");
                        break;
                    }

                    if (chessboardDic.ContainsKey(index))
                    {
                        Log.Error("FixChessBoard got an error index {0}", index);
                        break;
                    }
                    chessboardDic.Add(index, item);
                }

                index++;
            }
        }

        private string PackageGridInfoDbString()
        {
            string str = null;

            foreach (var item in chessboardDic)
            {
                ChessboardItem chessboardItem = item.Value;
                string itemStr = $"{chessboardItem.IndexId}:{chessboardItem.XmlId}";
                if (str == null)
                {
                    str = itemStr;
                }
                else
                {
                    str = $"{str}|{itemStr}";
                }
            }
            return str;
        }

        private void UnpackageGridInfoDbString(string dbStr)
        {
            string[] itemStrArr = dbStr.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var itemStr in itemStrArr)
            {
                string[] itemArr = itemStr.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                int indexId = itemArr[0].ToInt();
                int xmlId = itemArr[1].ToInt();
                CampBuildItemData data = CampBuildLibrary.GetBuildItemData(xmlId);
                if (data == null)
                {
                    Log.Error($"-----UnpackageGridInfoDbString-------wrong---xml id {xmlId}---");
                    return;
                }
                ChessboardItem chessboardItem = new ChessboardItem(data);
                chessboardItem.IndexId = indexId;

                chessboardDic.Add(chessboardItem.IndexId, chessboardItem);
            }


        }

        //判断对应箱子开启条件是否满足
        internal bool OpenCampBuildBox(int boxType, int phase, out RewardManager rewardManager, out int param)
        {
            int oldStepCount = StepCount;
            int oldFlagCount = FlagCount;
            bool isSuccess = false;
            rewardManager = null;
            param = 0;
            switch ((CampBuildBoxType)boxType)
            {
                case CampBuildBoxType.StepBox:
                    if (StepCount >= phaseData.LeftStep)
                    {
                        string rewardString = CampBuildLibrary.GetCampBuildBoxRewardsData(phase, boxType);
                        if (string.IsNullOrEmpty(rewardString))
                        {
                        }
                        else
                        {
                            StepCount = StepCount - phaseData.LeftStep;
                            rewardManager = owner.GetSimpleReward(rewardString, ObtainWay.CampBuild);
                            param = StepCount;
                            isSuccess = true;
                            //获取奖励
                        }
                    }
                    break;
                case CampBuildBoxType.BuildBox:
                    {
                        string rewardString = null;
                        if (CheckBuildBoxGrade(out rewardString))
                        {
                            BuildBoxCount++;
                            //获取奖励
                            rewardManager = owner.GetSimpleReward(rewardString, ObtainWay.CampBuild);
                            param = BuildBoxCount;
                            isSuccess = true;
                        }
                    }
                    break;
                case CampBuildBoxType.FlagBox:

                    if (FlagCount >= phaseData.FlagCount)
                    {
                        string rewardString = CampBuildLibrary.GetCampBuildBoxRewardsData(phase, boxType);
                        if (string.IsNullOrEmpty(rewardString))
                        {
                        }
                        else
                        {

                            rewardManager = owner.GetCalculateReward(2, rewardString, ObtainWay.CampBuild);
                            FlagCount = FlagCount - phaseData.FlagCount;
                            param = FlagCount;
                            isSuccess = true;

                            //获取奖励
                        }

                    }
                    break;
                default:
                    break;
            }
            UpdataCampBuildInfo2DB();
            //komoelog
            if (rewardManager != null)
            {
                List<Dictionary<string, object>> award = owner.ParseRewardInfoToList(rewardManager.RewardList);
                owner.KomoeEventLogCampBuild(((int)owner.Camp).ToString(), owner.Camp.ToString(), 0, 2, 0, oldStepCount, StepCount, oldStepCount - StepCount, oldFlagCount, FlagCount, DoubleLeftCount, null, award);
            }       

            return isSuccess;
        }

        public bool CheckBuildBoxGrade(out string reward)
        {
            reward = null;
            var data = CampBuildLibrary.GetCampBuildBoxData(BuildBoxCount);
            if (data == null)
            {
                return false;
            }
            reward = data.BuildReward;
            if (data.BuildValue < GetCampBuildingValue())
            {
                return true;
            }
            return false;
        }


        public Queue<CampBuildItemData> ChessboardItemRandom(int stepPoint)
        {
            //FIXME:这里由于stepPoint中可能会包含几个特殊点位（飞行点。旗子点）。这里没做筛选。可能比实际需要的多做了几个随机（最坏情况2次）。后续需要优化

            Queue<CampBuildItemData> queueItem = new Queue<CampBuildItemData>();
            for (int i = 0; i < stepPoint; i++)
            {
                CampBuildItemData itemData = CampBuildLibrary.RandomItem(PhaseNum);
                if (itemData != null)
                {
                    queueItem.Enqueue(itemData);
                }
            }
            return queueItem;
        }

        public void BuildGo(int stepPoint)
        {
            lastIndex = CurIndex;
            int realStep = stepPoint;

            CurIndex = lastIndex + realStep;
            if (CurIndex >= ChessboardMaxCount)
            {
                CurIndex = CurIndex - ChessboardMaxCount;
            }
            //计算实际所走步数
            //飞
            if (CurIndex % 9 == 0)
            {
                CurIndex = CurIndex + FlyPoint;
                if (CurIndex >= ChessboardMaxCount)
                {
                    CurIndex = CurIndex - ChessboardMaxCount;
                }
                realStep = realStep + FlyPoint;
            }

            //双倍剩余步数计算
            DoubleLeftCount = DoubleLeftCount - realStep;
            if (DoubleLeftCount < 0)
            {
                DoubleLeftCount = 0;
            }

            //komoelog
            int oldStepCount = StepCount;

            //总步数计算
            StepCount = StepCount + realStep;


            List<int> indexRecord = new List<int>();
            Queue<CampBuildItemData> itemDataQueue = ChessboardItemRandom(realStep);

            int curIndex = lastIndex;

            int doubleStep = RealGo(realStep, indexRecord, itemDataQueue, ref curIndex);       

            SetDouble(indexRecord, curIndex, doubleStep);

            UpdateCampBuildGridInfoList(indexRecord,realStep);
            AddPersonalBuildValue(realStep);

            UpdataCampBuildInfo2DB();

            indexRecord.Clear();

            //日志
            owner.BIRecordCampBuildLog(PhaseNum, realStep, StepCount);

            //komoelog
            owner.KomoeEventLogCampBuild(((int)owner.Camp).ToString(), owner.Camp.ToString(), 0, 1, realStep, oldStepCount, StepCount, StepCount- oldStepCount, curIndex == 0?FlagCount-1: FlagCount, FlagCount, DoubleLeftCount, null, null);
        }

        private int SetDouble(List<int> indexRecord, int curIndex, int doubleStep)
        {

            if (DoubleLeftCount <= 0 )
            {
                int doubleItemCount = DoubleItemCount;
                if (doubleItemCount == 0)
                {
                    if (phaseData != null)
                    {
                        doubleItemCount = phaseData.DoubleItemCount;
                    }
                    DoubleItemCount = doubleItemCount;

                    //双倍
                    while (doubleItemCount > 0)
                    {
                        curIndex++;
                        if (curIndex >= ChessboardMaxCount)
                        {
                            curIndex = 0;
                        }
                        if (curIndex > 0)
                        {
                            ChessboardItemChangeDouble(curIndex);
                            indexRecord.Add(curIndex);
                            doubleItemCount--;
                        }
                    }
                }
                else
                {
                    DoubleItemCount = DoubleItemCount - doubleStep;
                    if (DoubleItemCount == 0)
                    {
                        DoubleLeftCount = 30;
                        if (phaseData != null)
                        {
                            DoubleLeftCount = phaseData.DoubleLeftStep;
                        }
                    }
                }

            }
            

            return curIndex;
        }

        private int SetDouble(int curIndex)
        {
            int doubleItemCount = DoubleItemCount;
            if (DoubleLeftCount <= 0)
            {
                //双倍
                while (doubleItemCount > 0)
                {
                    curIndex++;
                    if (curIndex >= ChessboardMaxCount)
                    {
                        curIndex = 0;
                    }
                    if (curIndex > 0)
                    {
                        ChessboardItemChangeDouble(curIndex);
                        doubleItemCount--;
                    }
                }
            }

            return curIndex;
        }


        private int RealGo(int realStep, List<int> indexRecord, Queue<CampBuildItemData> itemDataQueue, ref int curIndex)
        {
            int doubleStep = 0;
            int step = realStep;
            while (step > 0)
            {
                curIndex++;
                if (curIndex >= ChessboardMaxCount)
                {
                    curIndex = 0;
                }

                if (curIndex == 0)
                {
                    FlagCount++;
                }
                else
                {
                    if (ChessboardItemGetAndFixNew(itemDataQueue, curIndex))
                    {
                        doubleStep++;
                    }
                    indexRecord.Add(curIndex);
                }
                step--;
            }
            return doubleStep;
        }

        private void UpdataCampBuildInfo2DB()
        {
            QueryUpdateCampBuildInfo query = new QueryUpdateCampBuildInfo(owner.Uid, CurIndex, FlagCount, StepCount, DoubleLeftCount, DoubleItemCount, BuildBoxCount, PackageGridInfoDbString(), PhaseNum);
            server.GameDBPool.Call(query);
        }

        public void AddPersonalBuildValue(int value)
        {
            owner.AddCampBattleRankScore(RankType.CampBuild,value);
        }

        public void UpdataCampBuildPhaseNum()
        {
            CampBuildPhaseInfo phaseInfo = null;
            switch (owner.Camp)
            {
                case CampType.None:
                    break;
                case CampType.TianDou:
                    phaseInfo = server.RelationServer.TianDouCampBuild;
                    break;
                case CampType.XingLuo:
                    phaseInfo = server.RelationServer.XinLuoCampBuild;
                    break;
                default:
                    break;
            }
            if (phaseInfo == null)
            {
                return;
            }
            PhaseNum = phaseInfo.PhaseNum;
        }

        private int GetCampBuildingValue()
        {
            int buildingValue = 0;
            switch (owner.Camp)
            {
                case CampType.None:
                    break;
                case CampType.TianDou:
                    buildingValue = server.RelationServer.TianDouCampBuild.BuildingValue;
                    break;
                case CampType.XingLuo:
                    buildingValue = server.RelationServer.XinLuoCampBuild.BuildingValue;
                    break;
                default:
                    break;
            }
            return buildingValue;
        }

        private void ChessboardItemChangeDouble(int index)
        {
            //格子填充
            chessboardDic[index].IsDouble = true;
        }

        /// <summary>
        /// 返回 是否双倍
        /// </summary>
        /// <param name="itemDataQueue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool ChessboardItemGetAndFixNew(Queue<CampBuildItemData> itemDataQueue, int index)
        {
            bool isDouble = false;
            //常规道路
            RewardManager rewards = new RewardManager();
            int itemCount = 0;

            ChessboardItem chessboard;
            if (!chessboardDic.TryGetValue(index,out chessboard))
            {
                Log.Error($"ChessboardItemGetAndFixNew got an error on index {index}");
                return false;
            }
            if (chessboard.IsDouble)
            {
                itemCount = chessboard.ItemCount * 2;
                isDouble = true;
            }
            else
            {
                itemCount = chessboard.ItemCount;
            }
            rewards.AddBreakupReward(chessboard.ItemType, chessboard.ItemId, itemCount);
            //物品获取
            owner.AddRewards(rewards, ObtainWay.CampBuild);
            //格子填充
            var data = itemDataQueue.Dequeue();
            chessboard.ItemId = data.ItemId;
            chessboard.ItemType = data.ItemType;
            chessboard.ItemCount = data.ItemCount;
            chessboard.XmlId = data.Id;
            chessboard.IsDouble = false;
            return isDouble;
        }

        /// <summary>
        /// 更新格子信息
        /// </summary>
        /// <param name="indexList">格子下标List</param>
        internal void UpdateCampBuildGridInfoList(List<int> indexList, int realStep)
        {
            MSG_ZGC_SYNC_CAMPBUILD_INFO msg = new MSG_ZGC_SYNC_CAMPBUILD_INFO();

            if (indexList != null)
            {
                foreach (var index in indexList)
                {
                    CampGrid_Info gInfo = new CampGrid_Info();
                    gInfo.GridNum = chessboardDic[index].IndexId;
                    gInfo.Id = chessboardDic[index].XmlId;
                    gInfo.IsDouble = chessboardDic[index].IsDouble;
                    msg.GridInfoList.Add(gInfo);
                }
            }

            msg.CurGridId = CurIndex;
            msg.DoubleLeftCount = DoubleLeftCount;
            msg.FlagCount = FlagCount;
            msg.StepCount = StepCount;
            msg.BuildBoxCount = BuildBoxCount;
            msg.GoCount = owner.GetCounter(CounterType.CampBuildRefreshDiceCount).Count;

            switch (owner.Camp)
            {
                case EnumerateUtility.CampType.None:
                    break;
                case EnumerateUtility.CampType.TianDou:
                    msg.BuildingValue = server.RelationServer.TianDouCampBuild.BuildingValue+ realStep;
                    break;
                case EnumerateUtility.CampType.XingLuo:
                    msg.BuildingValue = server.RelationServer.XinLuoCampBuild.BuildingValue+ realStep;
                    break;
                default:
                    break;
            }
            owner.Write(msg);

            //TODO：更新存储
        }

        internal MSG_ZGC_CAMPBUILD_INFO GetCampBuildPhaseMsg()
        {
            MSG_ZGC_CAMPBUILD_INFO info = new MSG_ZGC_CAMPBUILD_INFO();
            switch (owner.Camp)
            {
                case EnumerateUtility.CampType.None:
                    break;
                case EnumerateUtility.CampType.TianDou:
                    info.PhaseNum = server.RelationServer.TianDouCampBuild.PhaseNum;
                    info.BeginTime = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.TianDouCampBuild.BeginTime);
                    info.EndTime = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.TianDouCampBuild.EndTime);
                    info.NextBegin = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.TianDouCampBuild.NextBeginTime);
                    break;
                case EnumerateUtility.CampType.XingLuo:
                    info.PhaseNum = server.RelationServer.XinLuoCampBuild.PhaseNum;
                    info.BeginTime = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.XinLuoCampBuild.BeginTime);
                    info.EndTime = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.XinLuoCampBuild.EndTime);
                    info.NextBegin = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.XinLuoCampBuild.NextBeginTime);
                    break;
                default:
                    break;
            }
            return info;

        }

        internal MSG_ZGC_SYNC_CAMPBUILD_INFO GetCampBuildSyncMsg()
        {
            MSG_ZGC_SYNC_CAMPBUILD_INFO msg = new MSG_ZGC_SYNC_CAMPBUILD_INFO();
            msg.CurGridId = CurIndex;
            msg.DoubleLeftCount = DoubleLeftCount;
            msg.FlagCount = FlagCount;
            msg.StepCount = StepCount;
            msg.BuildBoxCount = BuildBoxCount;
            msg.GoCount = owner.GetCounter(CounterType.CampBuildRefreshDiceCount).Count;

            switch (owner.Camp)
            {
                case EnumerateUtility.CampType.None:
                    break;
                case EnumerateUtility.CampType.TianDou:
                    msg.BuildingValue = server.RelationServer.TianDouCampBuild.BuildingValue;
                    break;
                case EnumerateUtility.CampType.XingLuo:
                    msg.BuildingValue = server.RelationServer.XinLuoCampBuild.BuildingValue;
                    break;
                default:
                    break;
            }

            //主棋盘格子updateInfo
            foreach (var item in chessboardDic)
            {
                CampGrid_Info gInfo = new CampGrid_Info();
                gInfo.GridNum = item.Value.IndexId;
                gInfo.Id = item.Value.XmlId;
                gInfo.IsDouble = item.Value.IsDouble;
                msg.GridInfoList.Add(gInfo);
            }
            return msg;
        }

        internal void UpdateCampBuildDiceCount(int count)
        {
            owner.UpdateCounter(CounterType.CampBuildRefreshDiceCount, count);

            MSG_ZGC_BUY_CAMPBUILD_GO_COUNT msg = new MSG_ZGC_BUY_CAMPBUILD_GO_COUNT();
            msg.Result = 1;
            msg.GoCount = owner.GetCounter(CounterType.CampBuildRefreshDiceCount).Count;
            owner.Write(msg);
        }

        public int RandomStepPoint()
        {
            //掷骰子
            int rand = NewRAND.Next(1, 6);
            return rand;
        }

        internal bool Check()
        {
            if (owner.Camp == CampType.None)
            {
                return false;
            }

            return true;
        }

        public ZMZ_CAMP_BUILD_INFO GetCampBuildTransform()
        {
            ZMZ_CAMP_BUILD_INFO msg = new ZMZ_CAMP_BUILD_INFO();
            msg.CurIndex = CurIndex;
            msg.FlagCount = FlagCount;
            msg.StepCount = StepCount;
            msg.DoubleLeftCount = DoubleLeftCount;
            msg.DoubleItemCount = DoubleItemCount;
            msg.LastIndex = lastIndex;
            msg.BuildBoxCount = BuildBoxCount;
            msg.PhaseNum = PhaseNum;       
            foreach (var item in chessboardDic)
            {
                msg.ChessList.Add(GenerateChessBoardItemMsg(item.Value));
            }        
            return msg;
        }

        private ZMZ_CHESS_ITEM GenerateChessBoardItemMsg(ChessboardItem item)
        {
            ZMZ_CHESS_ITEM msg = new ZMZ_CHESS_ITEM();
            msg.IndexId = item.IndexId;
            msg.XmlId = item.XmlId;
            msg.ItemId = item.ItemId;
            msg.ItemType = item.ItemType;
            msg.ItemCount = item.ItemCount;
            msg.IsDouble = item.IsDouble;
            return msg;
        }

        public void LoadCampBuildTransform(ZMZ_CAMP_BUILD_INFO info)
        {
            CurIndex = info.CurIndex;
            FlagCount = info.FlagCount;
            StepCount = info.StepCount;
            DoubleLeftCount = info.DoubleLeftCount;
            DoubleItemCount = info.DoubleItemCount;
            lastIndex = info.LastIndex;
            BuildBoxCount = info.BuildBoxCount;
            PhaseNum = info.PhaseNum;        
            foreach (var chess in info.ChessList)
            {
                AddChessBoardItem(chess);
            }
            SetDouble(CurIndex);
            phaseData = CampBuildLibrary.GetCampBuildPhaseData(PhaseNum);
        }

        private void AddChessBoardItem(ZMZ_CHESS_ITEM chess)
        {
            CampBuildItemData data = CampBuildLibrary.GetBuildItemData(chess.XmlId);
            ChessboardItem item = new ChessboardItem(data);
            item.IndexId = chess.IndexId;
            item.IsDouble = chess.IsDouble;
            chessboardDic.Add(item.IndexId, item);
        }
    }
}
