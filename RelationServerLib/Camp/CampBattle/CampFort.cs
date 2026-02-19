using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Zone.Protocol.ZR;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class CampFort
    {
        public int Id { get { return XmlData.Id; } }
        public CampFortData XmlData;

        public CampType CampType;

        public DateTime CDTime;
        public double PassTime;

        public FortState State;
        public CampDungeon MainDungeon;
        public Dictionary<int, CampDungeon> SubDungeonDic = new Dictionary<int, CampDungeon>();
        private Dictionary<int, int> addNaturesRatioDic = new Dictionary<int, int>();

        public PLAY_BASE_INFO DefenderPlayerInfo;

        public long MaxProgress;
        public long Progress;

        public bool IsInBattle;

        public CampFort(CampFortData data)
        {
            XmlData = data;
            State = FortState.Monster;
            CDTime = Timestamp.UnixStartTime;
            foreach (var item in XmlData.AddNaturesDic)
            {
                if (!addNaturesRatioDic.ContainsKey(item.Key))
                {
                    addNaturesRatioDic.Add(item.Key, item.Value);
                }
            }
        }

        internal List<int> GetRelationFort()
        {
            return XmlData.RelationForts;
        }

        public int CalcBattleScoreUp()
        {
            int score = 0;
            var config = CampBattleLibrary.GetCampBattleExpend();
            int hold = config.FortDungeonHoldScoreUp;
            int lose = config.FortDungeonLoseScoreUp;

            if (!MainDungeon.IsBeenHold)
            {
                score += hold;
            }
            else
            {
                return 0;
            }
            foreach (var item in SubDungeonDic)
            {
                if (item.Value.IsBeenHold)
                {
                    score += lose;
                }
                else
                {
                    score += hold;
                }
            }
            score += XmlData.ScorePerMin;
            return score;
        }

        public int WindUpBattleScore(double dt)
        {
            if (FortState.Defender != State)
            {
                return 0;
            }
            int score = 0;
            if (dt<1000)
            {
                PassTime = PassTime+1000;
            }
            else
            {
                PassTime = PassTime + dt;
            }

            if (PassTime > 60000)
            {
                PassTime = 0;
                score = CalcBattleScoreUp();
            }
            return score;
        }

        internal void UpdateProgress()
        {
            MaxProgress = 1 + SubDungeonDic.Count;
            Progress = 1;
            foreach (var item in SubDungeonDic)
            {
                if (!item.Value.IsBeenHold)
                {
                    Progress += 1;
                }
            }
        }

        internal void SetDefensiveQueue(Dictionary<int, RepeatedField<HERO_INFO>> queues)
        {
            int subCount = SubDungeonDic.Count ;
            int newSubCount = queues.Count - 1;
            int deltaCount = subCount -newSubCount;

            if (deltaCount >0)
            {
                SubDungeonDic.Clear();
            }

            foreach (var item in queues)
            {
                if (item.Key == 1)
                {
                    MainDungeon.DefenderInfo.UpdateHeroList(item.Value);
                }
                else
                {
                    CampDungeon campDungeon;
                    if (!SubDungeonDic.TryGetValue(item.Key,out campDungeon))
                    {
                        if (item.Key == 0)
                        {
                            continue;
                        }

                        ///替换当前主副本
                        var dungeonData = DungeonLibrary.GetDungeon(XmlData.DefenderDungeonId);
                        if (dungeonData == null)
                        {
                            Log.Error($"init fort {Id} fail: can not find defender dungeon {XmlData.DefenderDungeonId} ,please check xml ");
                        }
                        campDungeon = new CampDungeon(dungeonData, this);
                        Defender defender = new Defender(item.Key, item.Value);
                        campDungeon.InitDefender(defender);

                        SubDungeonDic.Add(item.Key, campDungeon);
                    }
                    else
                    {
                        if (item.Key == 0)
                        {
                            SubDungeonDic.Remove(item.Key);
                            continue;
                        }

                        campDungeon.DefenderInfo.UpdateHeroList(item.Value);
                    }
                }

                UpdateProgress();
            }
        }

        internal void UpdateDefensiveHeroInfo(Dictionary<int, RepeatedField<HERO_INFO>> queues)
        {
            foreach (var item in queues)
            {
                if (item.Key == 1)
                {
                    MainDungeon.DefenderInfo.UpdateHeroList(item.Value);
                }
                else
                {
                    CampDungeon campDungeon;
                    if (!SubDungeonDic.TryGetValue(item.Key, out campDungeon))
                    {
                        if (item.Key == 0)
                        {
                            continue;
                        }

                        ///替换当前主副本
                        var dungeonData = DungeonLibrary.GetDungeon(XmlData.DefenderDungeonId);
                        if (dungeonData == null)
                        {
                            Log.Error($"init fort {Id} fail: can not find defender dungeon {XmlData.DefenderDungeonId} ,please check xml ");
                        }
                        campDungeon = new CampDungeon(dungeonData, this);
                        Defender defender = new Defender(item.Key, item.Value);
                        campDungeon.InitDefender(defender);

                        SubDungeonDic.Add(item.Key, campDungeon);
                    }
                    else
                    {
                        if (item.Key == 0)
                        {
                            SubDungeonDic.Remove(item.Key);
                            continue;
                        }

                        campDungeon.DefenderInfo.UpdateHeroList(item.Value);
                    }
                }
            }
        }

        internal void ResetFortNatureInfo()
        {
            if (DefenderPlayerInfo!=null)
            {
                DefenderPlayerInfo.BuyNatureCount = 0;
            }
            addNaturesRatioDic.Clear();
            foreach (var item in XmlData.AddNaturesDic)
            {
                addNaturesRatioDic.Add(item.Key, item.Value);
            }
        }

        internal void SetFortHold(CampDungeon dungeon, PLAY_BASE_INFO attackerInfo, RepeatedField<HERO_INFO> attackerHeroList)
        {
            State = FortState.Defender;
            if ((int)CampType != attackerInfo.Camp)
            {
                CDTime = RelationServerApi.now;
                CampType = (CampType)attackerInfo.Camp;
            }

            IsInBattle = false;
            DefenderPlayerInfo = attackerInfo;

            Dictionary<int, RepeatedField<HERO_INFO>> queues = new Dictionary<int, RepeatedField<HERO_INFO>>();

            foreach (var item in attackerHeroList)
            {
                RepeatedField<HERO_INFO> list;
                int queueId = item.DefensiveQueueNum;
                if (!queues.TryGetValue(queueId,out list))
                {
                    list = new RepeatedField<HERO_INFO>();
                    queues.Add(queueId, list);
                }
                list.Add(item);
            }
            SubDungeonDic.Clear();

            foreach (var item in queues)
            {
                ///替换当前主副本
                var dungeonData = DungeonLibrary.GetDungeon(XmlData.DefenderDungeonId);
                if (dungeonData == null)
                {
                    Log.Error($"init fort {Id} fail: can not find defender dungeon {XmlData.DefenderDungeonId} ,please check xml ");
                }
                CampDungeon campDungeon = new CampDungeon(dungeonData, this);
                Defender defender = new Defender(item.Key, item.Value);
                campDungeon.InitDefender(defender);
                if (item.Key == 1)
                {
                    MainDungeon = campDungeon;
                }
                else
                {
                    SubDungeonDic.Add(item.Key, campDungeon);
                }
            }

            addNaturesRatioDic.Clear();
            foreach (var item in XmlData.AddNaturesDic)
            {
                addNaturesRatioDic.Add(item.Key, item.Value);
            }
            UpdateProgress();
        }


        internal bool CheckCanAttackMain()
        {
            foreach (var item in SubDungeonDic)
            {
                if (!item.Value.IsBeenHold)
                {
                    return false;
                }
            }
            return true;
        }

        internal CampDungeon GetDungeon(int dungeonId)
        {
            if (dungeonId == MainDungeon.Id)
            {
                return MainDungeon;
            }
            CampDungeon dungeon;
            SubDungeonDic.TryGetValue(dungeonId, out dungeon);
            return dungeon;
        }

        public void LoadData(CAMPFORT fort)
        {
            var dungeonData = DungeonLibrary.GetDungeon(fort.MainDungeon.DungeonId);
            if (dungeonData == null)
            {
                Log.Error($"load redis fort {Id} fail: can not find main dungeon {fort.MainDungeon.DungeonId} ,please check xml ");
            }

            foreach (var item in fort.AddNatures)
            {
                int value;
                if (addNaturesRatioDic.TryGetValue(item.Key,out value))
                {
                    value = value + item.Value;
                }
                addNaturesRatioDic[item.Key] = value;
            }
            MainDungeon = new CampDungeon(dungeonData,this);
            
            MainDungeon.Deserialize(fort.MainDungeon);

            State = (FortState)fort.FortState;
            CampType = (CampType)fort.Camp;

            SubDungeonDic = new Dictionary<int, CampDungeon>();
            foreach (var item in fort.SubDungeons)
            {
                var data = DungeonLibrary.GetDungeon(item.DungeonId);
                if (data == null)
                {
                    Log.Error($"load redis fort {Id} fail: can not find sub dungeon {item.DungeonId} ,please check xml ");
                }
                CampDungeon subDungeon = new CampDungeon(data,this);
                subDungeon.Deserialize(item);
                SubDungeonDic.Add(subDungeon.Id, subDungeon);
                if (subDungeon.IsBeenHold)
                {
                    IsInBattle = true;
                }
            }

            MaxProgress = fort.MaxProgress;
            Progress = fort.Progress;

            CDTime = Timestamp.TimeStampToDateTime(fort.CDTime);

            DefenderPlayerInfo = fort.DefenderPcInfo;
        }

        internal bool CheckInCDTime()
        {
            if(CDTime > DateTime.MinValue && (RelationServerApi.now - CDTime).TotalSeconds - 2 < CampBattleLibrary.GetCampBattleFortGuardTime())
            {
                return true;
            }
            return false;
        }

        public string GetSerialize()
        {
            CAMPFORT fort = new CAMPFORT();
            fort.Id = Id;
            fort.Camp = (int)CampType;
            fort.FortState = (int)State;
            fort.CDTime = Timestamp.GetUnixTimeStampSeconds(CDTime);
            foreach (var item in addNaturesRatioDic)
            {
                fort.AddNatures.Add(item.Key, item.Value);
            }
            fort.MainDungeon = MainDungeon.Serialize();

            fort.MaxProgress = MaxProgress;
            fort.Progress = Progress;

            foreach (var item in SubDungeonDic)
            {
                fort.SubDungeons.Add(item.Value.Serialize());
            }
            fort.DefenderPcInfo = DefenderPlayerInfo;

            return MessagePacker.ProtobufHelper.Serialize2String(fort);
        }

        public void Deserialize(string forStr)
        {
            CAMPFORT fort = MessagePacker.ProtobufHelper.DeserializeFromString<CAMPFORT>(forStr);
            LoadData(fort);
        }


        //internal void UpdateAddNatures(int natureType, int addValue)
        //{
        //    if (addNaturesRatioDic.ContainsKey(natureType))
        //    {
        //        addNaturesRatioDic[natureType] += addValue;
        //    }
        //    else
        //    {
        //        addNaturesRatioDic.Add(natureType, addValue);
        //    }
        //}


        internal Dictionary<int, int> GetAddNatures()
        {
            int oneIntensifyValue = CampBattleLibrary.GetAttributeOneIntensifyValue();
            Dictionary<int, int> finial = new Dictionary<int, int>();
            foreach (var item in addNaturesRatioDic)
            {
                int count = GetBuyNatureCount();
                int value = item.Value + count * oneIntensifyValue;
                finial.Add(item.Key, value);
            }
            return finial;
        }

        internal void InitMonster()
        {

            var dungeonData = DungeonLibrary.GetDungeon(XmlData.BossDungeonId);
            if (dungeonData == null)
            {
                Log.Error($"init fort {Id} fail: can not find boss dungeon {XmlData.BossDungeonId} ,please check xml ");
            }
            MainDungeon = new CampDungeon(dungeonData, this);
        }

        internal bool GiveUp()
        {
            State = FortState.None;
            DefenderPlayerInfo = null;
          
            MainDungeon.GiveUp();
            SubDungeonDic.Clear();
            return true;
        }

        internal int GetDefenderUid()
        {
            if (DefenderPlayerInfo != null)
            {
                return DefenderPlayerInfo.Uid;
            }
            return 0;
        }

        internal string GetDefenderName()
        {
            if (DefenderPlayerInfo != null)
            {
                return DefenderPlayerInfo.Name;
            }
            return "";
        }

        internal int GetIcon()
        {
            if (DefenderPlayerInfo != null)
            {
                return DefenderPlayerInfo.Icon;
            }
            return 0;
        }

        internal void UpdateNatureCount(int newCount)
        {
            if (DefenderPlayerInfo != null)
            {
                DefenderPlayerInfo.BuyNatureCount = newCount;
            }
        }

        internal int GetBuyNatureCount()
        {
            if (DefenderPlayerInfo != null)
            {
                return DefenderPlayerInfo.BuyNatureCount;
            }
            return 0;
        }
    }
}
