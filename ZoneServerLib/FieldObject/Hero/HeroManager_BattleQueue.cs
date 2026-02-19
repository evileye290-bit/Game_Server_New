using CommonUtility;
using DBUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
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
    public partial class HeroManager
    {
        private Dictionary<int, MainBattleQueueInfo> mainBattleQueue = new Dictionary<int, MainBattleQueueInfo>();
        public Dictionary<int, MainBattleQueueInfo> MainBattleQueue { get { return mainBattleQueue; } }

        public void InitMainBattleQueue(List<MainBattleQueueInfo> infoList)
        {
            foreach (var info in infoList)
            {
                mainBattleQueue.Add(info.QueueNum, info);
                owner.PetManager.CheckSetMainQueuePet(info);
            }
        }      

        public void GenerateMainBattleQueueInfo(MSG_ZGC_MAINQUEUE_INFO response)
        {
            foreach (var kv in mainBattleQueue)
            {
                ZGC_MAINQUEUE_INFO queueInfo = new ZGC_MAINQUEUE_INFO()
                {
                    QueueNum = kv.Value.QueueNum,
                    QueueName = kv.Value.QueueName,
                    BattleState = kv.Value.BattleState,
                    PetUidHigh = kv.Value.PetUid.GetHigh(),
                    PetUidLow = kv.Value.PetUid.GetLow()
                };
                GenerateMainQueueInfo(kv.Value.HeroPosList, queueInfo.HeroPos);
                response.InfoList.Add(queueInfo);
            }         
        }

        private void GenerateMainQueueInfo(Dictionary<int, int> heroPosList, RepeatedField<ZGC_MAINQUEUE_HEROPOS> heroPosMsg)
        {
            foreach (var heroPos in heroPosList)
            {
                ZGC_MAINQUEUE_HEROPOS msg = new ZGC_MAINQUEUE_HEROPOS()
                {
                    HeroId = heroPos.Key,
                    PosId = heroPos.Value
                };
                heroPosMsg.Add(msg);
            }
        }

       public void UpdateMainBattleQueueHeroPos(MainBattleQueueInfo queueInfo, RepeatedField<GateZ_MAINQUEUE_HEROPOS> heroPosMsg, RepeatedField<ZGC_MAINQUEUE_HEROPOS> heroPosList)
        {
            queueInfo.HeroPosList.Clear();

            foreach (var item in heroPosMsg)
            {
                HeroInfo hero = GetHeroInfo(item.HeroId);
                if (hero == null)
                {
                    Log.Warn($"player {owner.Uid} UpdateMainBattleQueueHeroPos queue {queueInfo.QueueNum} failed: heroId param {item.HeroId} error");
                    continue;
                }
                int maxPos = HeroLibrary.GetHeroPosMaxNum();
                if (item.PosId > maxPos)
                {
                    Log.Warn($"player {owner.Uid} UpdateMainBattleQueueHeroPos queue {queueInfo.QueueNum} failed: pos param {item.PosId} error");
                    continue;
                }
                queueInfo.HeroPosList.Add(hero.Id, item.PosId);

                heroPosList.Add(new ZGC_MAINQUEUE_HEROPOS() { HeroId = hero.Id, PosId = item.PosId});
            }

            SyncDbUpdateMainBattleQueueHeroPos(queueInfo);
        }

        public void UpdateOriginalMainHeroPos(MainBattleQueueInfo queueInfo)
        {
            List<int> deleteList = new List<int>();
            foreach (var item in heroPos)
            {
                if (!queueInfo.HeroPosList.ContainsKey(item.Key))
                {
                    deleteList.Add(item.Key);
                }
            }
            foreach (var heroId in deleteList)
            {
                DeleteHeroPos(heroId);
            }

            foreach (var item in queueInfo.HeroPosList)
            {
                UpdateHeroPos(item.Key, item.Value);
            }
        }

        public void UnlockMainBattleQueue(int queueNum, MainBattleQueueModel model, bool isFirstQueue)
        {
            MainBattleQueueInfo info = new MainBattleQueueInfo();
            info.QueueNum = queueNum;
            info.QueueName = model.DefaultName;
            if (isFirstQueue)
            {
                info.BattleState = 1;
                foreach (var item in heroPos)
                {
                    info.HeroPosList.Add(item.Key, item.Value);
                }
            }
            mainBattleQueue.Add(queueNum, info);
            SyncDbInsertMainBattleQueueInfo(info);
        }

        public void UnlockMultiMainBattleQueue(List<MainBattleQueueModel> queueModelList)
        {
            if (queueModelList.Count > 0)
            {              
                MainBattleQueueInfo queueInfo;
                foreach (var item in queueModelList)
                {
                    mainBattleQueue.TryGetValue(item.Id, out queueInfo);
                    if (queueInfo == null && owner.Level >= item.LevelLimit && item.LevelUnlock)
                    {
                        if (item.Id == CharacterInitLibrary.FirstMainQueueNum)
                        {
                            UnlockMainBattleQueue(item.Id, item, true);
                        }
                        else
                        {
                            UnlockMainBattleQueue(item.Id, item, false);
                        }                     
                    }
                }               
                owner.SendMainBattleQueueInfo();
            }
        }

        public void UpdateMainBattleQueueHeroPos()
        {
            MainBattleQueueInfo queueInfo;
            mainBattleQueue.TryGetValue(CharacterInitLibrary.FirstMainQueueNum, out queueInfo);
            if (queueInfo == null)
            {
                return;
            }

            queueInfo.HeroPosList.Clear();

            foreach (var item in heroPos)
            {
                queueInfo.HeroPosList.Add(item.Key, item.Value);
            }

            SyncDbUpdateMainBattleQueueHeroPos(queueInfo);

            owner.SendMainBattleQueueInfo();
        }

        private void SyncDbUpdateMainBattleQueueHeroPos(MainBattleQueueInfo queueInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdateMainBattleQueueHeroPos(owner.Uid, queueInfo.QueueNum, queueInfo.GetHeroPosStringInfo()));
        }

        private void SyncDbInsertMainBattleQueueInfo(MainBattleQueueInfo queueInfo)
        {
            owner.server.GameDBPool.Call(new QueryInsertMainBattleQueueInfo(owner.Uid, queueInfo.QueueNum, queueInfo.BattleState, queueInfo.QueueName, queueInfo.GetHeroPosStringInfo()));
        }

        public MSG_ZMZ_MAINQUEUE_INFO GenerateMainQueueInfoTransformMsg()
        {
            MSG_ZMZ_MAINQUEUE_INFO msg = new MSG_ZMZ_MAINQUEUE_INFO();

            foreach (var kv in mainBattleQueue)
            {
                ZMZ_MAINQUEUE_INFO info = new ZMZ_MAINQUEUE_INFO()
                {
                    QueueNum = kv.Value.QueueNum,
                    QueueName = kv.Value.QueueName,
                    BattleState = kv.Value.BattleState,
                    PetUid = kv.Value.PetUid
                };
                kv.Value.HeroPosList.ForEach(x=> info.HeroPosList.Add(x.Key, x.Value));
                msg.InfoList.Add(kv.Key, info);
            }

            return msg;
        }

        public void LoadMainQueueInfoTransform(MSG_ZMZ_MAINQUEUE_INFO msg)
        {
            foreach (var info in msg.InfoList)
            {
                MainBattleQueueInfo queueInfo = new MainBattleQueueInfo()
                {
                    QueueNum = info.Value.QueueNum,
                    QueueName = info.Value.QueueName,
                    BattleState = info.Value.BattleState,
                    PetUid = info.Value.PetUid
                };
                info.Value.HeroPosList.ForEach(x => queueInfo.HeroPosList.Add(x.Key, x.Value));
                mainBattleQueue.Add(info.Key, queueInfo);
            }
        }
    }
}
