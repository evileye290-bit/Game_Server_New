using System;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class DungeonMap : FieldMap, IDisposable
    {
        protected int battleStage = 0;
        //是否是手动结束的
        protected bool isQuitDungeon = false;
        public virtual void Open(int firstPlayer = 0)
        {
            server.MapManager.AddMap(this);
            MSG_ZM_NEW_MAP notify = new MSG_ZM_NEW_MAP();
            notify.MapId = MapId;
            notify.Channel = Channel;
            notify.Owner = firstPlayer;
            server.ManagerServer.Write(notify);

            Log.Write($"open dungeon {MapId} success mainId {server.MainId} subId {server.SubId}");
        }

        /// <summary>
        /// 副本开打
        /// </summary>
        protected virtual void Start()
        {
            //设置副本开始，结束时间
            SetStartTime(BaseApi.now);
            InitTriggers();
            NotifyDungeonBattleStart();

            //战斗回放
            BattleFpsManager.StartRecordVedio();

            State = DungeonState.Started;

            foreach(var hero in HeroList)
            {
                try
                {
                    hero.Value.StartFighting();
                }
                catch(Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach (var pet in PetList)
            {
                try
                {
                    pet.Value.StartFighting();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach(var monster in MonsterList)
            {
                try
                {
                    monster.Value.StartFighting();
                }
                catch(Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }

            foreach (var hero in HeroList)
            {
                hero.Value.DispatchHeroStartFightMsg(hero.Value.HeroId);
            }
        }

        public virtual void OnStopFighting()
        {
            foreach (var player in PcList)
            {
                try
                {
                    player.Value.StopFighting();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach (var robot in RobotList)
            {
                try
                {
                    robot.Value.StopFighting();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach (var hero in HeroList)
            {
                try
                {
                    hero.Value.StopFighting();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach (var monster in MonsterList)
            {
                try
                {
                    monster.Value.StopFighting();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach (var pet in PetList)
            {
                try
                {
                    pet.Value.StopFighting();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }


        public virtual void Stop(DungeonResult result)
        {
            // 已经有胜负结果，不再更新（防止临界状态下下，有可能又赢又输）
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            //副本结束取消所有trigger
            DungeonResult = result;
            State = DungeonState.Stopped;

            SaveHeroAndMonsterInfo();

            OnStopFighting();

            switch (result)
            {
                case DungeonResult.Success:
                    Success();
                    break;
                case DungeonResult.Failed:
                    Failed();
                    break;
                case DungeonResult.Tie:
                    Tie();
                    break;
                default:
                    break;
            }
        }

        //玩家点击退出按钮停止战斗，结算奖励
        public virtual void OnStopBattle(PlayerChar player)
        {
            isQuitDungeon = true;
            Stop(DungeonResult.Failed);
            //player.LeaveDungeon();
        }

        // 不同玩法的副本，应重载副本结算相关方法
        protected virtual void Success()
        {
            DoReward();
            PlayerChar player = null;
            foreach (var kv in PcList)
            {
                try
                {
                    player = kv.Value;;
                    if (player == null)
                    {
                        continue;
                    }

                    RewardManager mng = GetFinalReward(player.Uid);
                    mng.BreakupRewards();
                    player.AddRewards(mng, ObtainWay.DungeonFinalReward, DungeonModel.Type.ToString());

                    //通知前端奖励
                    MSG_ZGC_DUNGEON_REWARD rewardMsg = player.GetRewardSyncMsg(mng);
                    rewardMsg.DungeonId = DungeonModel.Id;
                    rewardMsg.Result = (int)DungeonResult;
                    player.Write(rewardMsg);

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }

            ResetReward();
        }

        protected virtual void SaveHeroAndMonsterInfo() { }

        public virtual void Tie()
        {
        }

        protected virtual void Failed()
        {
            foreach (var kv in PcList)
            {
                try
                {
                    //通知前端奖励
                    MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                    rewardMsg.DungeonId = DungeonModel.Id;
                    rewardMsg.Result = (int)DungeonResult;

                    kv.Value.CheckCacheRewardMsg(rewardMsg);
                }
                catch (Exception ex)
                {
                    Logger.Log.Alert(ex);
                }
            }
        }

        protected virtual void OnBattleStageChange()
        {
        }

        /// <summary>
        /// 副本关闭
        /// </summary>
        public void Close()
        {
            Log.Write("dungeon {0} channel {1} closed", MapId, Channel);
            // 防止策划时间配错，Close在Stop完成踢人前先发生
            KickPlayer();

            State = DungeonState.Closed;
            server.MapManager.RemoveMap(this);
            MSG_ZM_DELETE_MAP notify = new MSG_ZM_DELETE_MAP();
            notify.MapId = MapId;
            notify.Channel = Channel;
            server.ManagerServer.Write(notify);

            this.Dispose();
        }

        public void AddBattleStage()
        {
            this.battleStage += 1;
            OnBattleStageChange();
        }

        public void SetBattleStage(int stage)
        {
            this.battleStage = Math.Max(this.battleStage, stage);
            OnBattleStageChange();
        }

        protected virtual bool HadPassedHuntingDungeon()
        {
            return false;
        }

        protected virtual bool CheckHuntingPeriodBuffEffectCondition()
        {
            return false;
        }

        public void HuntingPeriodBuffEffect(Monster monster)
        {
            if (!CheckHuntingPeriodBuffEffectCondition()) return;

            HuntingBuffSuitModel model = HuntingLibrary.GetHuntingBuffSuit(HuntingLibrary.GetWeekIndex(BaseApi.now), DungeonModel.Id);
            if (model == null) return;

            model.BuffList.ForEach(id => monster.AddBuff(monster, id, 1));
        }

        public void Dispose()
        {
            ReleaseResource();
            GC.SuppressFinalize(this);

            Log.Debug($"Dispose dungeonmap {this.DungeonModel.Id} type {this.DungeonModel.Type}");
        }

        //副本结束,释放资源，便于GC回收map
        public void ReleaseResource()
        {
            ReleaseMonsterResource();
        }
    }
}
