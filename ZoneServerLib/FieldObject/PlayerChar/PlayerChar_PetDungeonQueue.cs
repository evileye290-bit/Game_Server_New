using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
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
        public void InitPetDungeonQueues(Dictionary<DungeonQueueType, List<ulong>> dungeonQueues)
        {
            foreach (var queue in dungeonQueues)
            {
                PetManager.AddPetDungeonQueueInfo(queue.Key, queue.Value);
            }
        }

        public void SendPetDungeonQueuesMsg()
        {
            MSG_ZGC_PET_DUNGEON_QUEUE_LIST msg = new MSG_ZGC_PET_DUNGEON_QUEUE_LIST();
            Dictionary<DungeonQueueType, Dictionary<int, PetInfo>> dungeonQueueList = PetManager.GetPetDungeonQueueList();
            foreach (var kv in dungeonQueueList)
            {
                msg.QueueList.Add(GeneratePetDungeonQueueInfo(kv.Key, kv.Value));
            }
            Write(msg);
        }

        private ZGC_PET_DUNGEON_QUEUE GeneratePetDungeonQueueInfo(DungeonQueueType queueType, Dictionary<int, PetInfo> queuePets)
        {
            ZGC_PET_DUNGEON_QUEUE msg = new ZGC_PET_DUNGEON_QUEUE();
            msg.QueueType = (int)queueType;
            foreach (var kv in queuePets)
            {
                msg.QueuePets.Add(kv.Key, new ZGC_PET_UID() { UidHigh = kv.Value.PetUid.GetHigh(), UidLow = kv.Value.PetUid.GetLow() });
            }
            return msg;
        }

        /// <summary>
        /// 更新宠物副本阵容
        /// </summary>
        /// <param name="queueType">副本阵容类型</param>
        /// <param name="queueNum">第几队</param>
        /// <param name="petUid"></param>
        /// <param name="remove"></param>
        public void UpdatePetDungeonQueue(DungeonQueueType queueType, int queueNum, ulong petUid, bool remove)
        {
            MSG_ZGC_UPDATE_PET_DUNGEON_QUEUE response = new MSG_ZGC_UPDATE_PET_DUNGEON_QUEUE();
            response.QueueType = (int)queueType;
            response.QueueNum = queueNum;
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();
            response.Remove = remove;

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn($"player {Uid} UpdatePetDungeonQueue queueType {(int)queueType} queueNum {queueNum} failed: not find pet {petUid}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (!remove)
            {
                if (!CheckDungeonQueueNumLmit((int)queueType, queueNum))
                {
                    Log.Warn($"player {Uid} UpdatePetDungeonQueue queueType {(int)queueType} queueNum {queueNum} failed: queue num limit");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                bool result = PetManager.UpdateDungeonQueuePetInfo(queueType, queueNum, petInfo);
                if (!result)
                {
                    Log.Warn($"player {Uid} UpdatePetDungeonQueue queueType {(int)queueType} queueNum {queueNum} failed: pet already in queue");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }
            else
            {
                bool result = PetManager.RemoveDungeonQueuePet(queueType, queueNum);
                if (!result)
                {
                    Log.Warn($"player {Uid} UpdatePetDungeonQueue queueType {(int)queueType} queueNum {queueNum} remove pet {petUid} failed: pet not in queue");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }

            DifferentPetDungeonQueueUpdate(queueType, petInfo);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private bool CheckDungeonQueueNumLmit(int queueType, int queueNum)
        {
            if (queueNum <= 0)
            {
                return false;
            }
            int queueNumLimit = PetLibrary.GetPetDungeonQueueNum(queueType);
            if (queueNum <= queueNumLimit)
            {
                return true;
            }
            return false;
        }

        private void DifferentPetDungeonQueueUpdate(DungeonQueueType queueType, PetInfo petInfo)
        {
            switch (queueType)
            {
                case DungeonQueueType.Arena:
                    OnArenaPetQueueUpdate(petInfo);
                    break;
                default:
                    break;
            }
        }

        private void OnArenaPetQueueUpdate(PetInfo petInfo)
        {
            int power = ArenaMng.GetDefensiveBattlePower();
            power += petInfo.GetBattlePower();
            SyncArenaDefensivePetToRelation(power, petInfo);
            //保存Redis
            UpdatePlayerDefensivePetToRedis(petInfo.PetId);
            UpdatePlayerDefensivePowerToRedis(power);
        }

        //private bool CheckDungeonQueueNumLmit(DungeonQueueType queueType, int queueNum)
        //{
        //    if (queueNum <= 0)
        //    {
        //        return false;
        //    }
        //    switch (queueType)
        //    {
        //        case DungeonQueueType.CampDefensive:
        //        case DungeonQueueType.Tower:
        //        case DungeonQueueType.HuntingIntrude:
        //            return queueNum <= 1;
        //        case DungeonQueueType.CrossBattle:
        //            return queueNum <= CrossBattleLibrary.CrossQueueCount;
        //        case DungeonQueueType.ThemeBoss:
        //            return queueNum <= ThemeBossLibrary.ThemeQueueCount;
        //        case DungeonQueueType.CrossBoss:
        //            return queueNum <= CrossBossLibrary.CrossBossQueueCount;
        //        case DungeonQueueType.CarnivalBoss:
        //            return queueNum <= CarnivalBossLibrary.QueueCount;
        //        case DungeonQueueType.IslandChallenge:
        //            return queueNum <= 3;
        //        case DungeonQueueType.CrossChallenge:
        //            return queueNum <= CrossChallengeLibrary.CrossQueueCount;
        //        default:
        //            return false;
        //    }
        //}

        private void SendPetDungeonQueuesTransMsg()
        {
            MSG_ZMZ_PET_DUNGEON_QUEUES msg = new MSG_ZMZ_PET_DUNGEON_QUEUES();

            Dictionary<DungeonQueueType, Dictionary<int, PetInfo>> dungeonQueueList = PetManager.GetPetDungeonQueueList();
            foreach (var kv in dungeonQueueList)
            {
                msg.QueueList.Add((int)kv.Key, GenerateDungeonQueuePetsTransMsg(kv.Value));
            }

            server.ManagerServer.Write(msg, Uid);
        }

        private ZMZ_PET_DUNGEON_QUEUE_INFO GenerateDungeonQueuePetsTransMsg(Dictionary<int, PetInfo> petList)
        {
            ZMZ_PET_DUNGEON_QUEUE_INFO msg = new ZMZ_PET_DUNGEON_QUEUE_INFO();
            foreach (var kv in petList)
            {
                msg.QueuePets.Add(kv.Key, kv.Value.PetUid);
            }
            return msg;
        }

        public void LoadPetDungeonQueuesTransformMsg(MSG_ZMZ_PET_DUNGEON_QUEUES msg)
        {
            foreach (var kv in msg.QueueList)
            {
                List<ulong> pets = new List<ulong>();
                for (int i = 1; i <= kv.Value.QueuePets.Count; i++)
                {
                    ulong petUid;
                    kv.Value.QueuePets.TryGetValue(i, out petUid);
                    pets.Add(petUid);
                }
                PetManager.AddPetDungeonQueueInfo((DungeonQueueType)kv.Key, pets);
            }
        }
    }
}
