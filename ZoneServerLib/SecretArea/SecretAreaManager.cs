using CommonUtility;
using DBUtility;
using RedisUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShared;
using EnumerateUtility;
using ServerModels;
using Message.Zone.Protocol.ZM;

namespace ZoneServerLib
{
    public class SecretAreaManager
    {
        public PlayerChar Owner { get; private set; }
        public int Id { get; private set; }//当前挑战胜利的关卡
        public int Rank { get; private set; }
        public SecretAreaState State { get; private set; }
        public float PassTime { get; private set; }

        public bool ContinueFight { get; private set; }//连续战斗

        public SecretAreaManager(PlayerChar player)
        {
            this.Owner = player;
            //LoadRank();
        }

        public void BindSecretAreaInfo(int id, int state, float passTime)
        {
            this.Id = id;
            this.State = (SecretAreaState)state;
            this.PassTime = passTime;
        }

        public int GetTire()
        {
            //超时不算通关
            return State == SecretAreaState.TimeOut ? (Id - 1) / 10 + 1 : Id / 10 + 1;
        }

        public void UpdateSecretAreaInfo(int id, SecretAreaState state, int passTime)
        {
            if (CheckNeedUpdate(id, state, passTime))
            {
                BindSecretAreaInfo(id, (int)state, passTime);

                //更新数据
                QueryUpdateSecretArea query = new QueryUpdateSecretArea(Owner.Uid, id, (int)state, passTime);
                Owner.server.GameDBPool.Call(query);

                //更新排行榜
                UpdateSecretAreaRank(id, passTime);

                //秘境层数
                Owner.AddTaskNumForType(TaskType.SecretAreaStage, id, false);
                Owner.AddSchoolTaskNum(TaskType.SecretAreaStage, id, true);

                Owner.SerndUpdateRankValue(RankType.SecretArea, id);

                //玩家行为
                Owner.RecordAction(ActionType.SecretArea, id);
            }
        }

        public bool CheckNeedUpdate(int id, SecretAreaState state, float passTime)
        {
            if (id > this.Id) return true;

            if (state > this.State) return true;

            if (passTime < this.PassTime) return true;

            return false;
        }

        public bool IsFirstPassReward(int passId, SecretAreaState newState)
        {
            return passId > this.Id;
        }

        public bool CheckSweep(int id)
        {
            if (id > this.Id)
            {
                //超过了当前关卡，无法扫荡
                return false;
            }
            else if (id == this.Id)
            {
                //当前管卡没有正常通关
                return State == SecretAreaState.Passed;
            }
            return true;
        }

        public ErrorCode CheckCreateDungeon(int dungeonId)
        {
            if (Owner.Team != null)
            {
                return ErrorCode.InTeam;
            }

            //2 秘境币达到上限
            if (Owner.CheckCoinUpperLimit(CurrenciesType.secretAreaCoin))
            {
                return ErrorCode.SecretAreaCoinUpperLimit;
            }

            SecretAreaModel model = SecretAreaLibrary.GetModelByDungeonId(dungeonId);
            if (model == null)
            {
                return ErrorCode.Fail;
            }

            //第一关特殊处理, 无需过多检测
            if (model.Id == SecretAreaLibrary.FirstSecretArea)
            {
                return ErrorCode.Success;
            }

            if (model.Id == Id )
            {
                //当前关卡已通关，不能挑战，只能够扫荡
                if (State == SecretAreaState.Passed)
                {
                    return ErrorCode.SecretAreaHadPassed;
                }
                return ErrorCode.Success;
            }

            //跳关, 则需要通过上一关
            if (model.Id != Id + 1)
            {
                return ErrorCode.SecretAreaNeedPassLast;
            }
            return ErrorCode.Success;
        }

        private void UpdateSecretAreaRank(int id, float passTime)
        {
            //int score = ServerShared.SecretAreaManager.BuildSecretAreaJobRankScore(id, (int)passTime);
            //OperateSecretAreaJobRankUpdate opera = new OperateSecretAreaJobRankUpdate(Owner.MainId, Owner.Uid, score);
            //Owner.server.Redis.Call(opera);
            Owner.server.GameRedis.Call(new OperateUpdateRankScore(RankType.SecretArea, Owner.server.MainId, Owner.Uid, Id, Owner.server.Now()));
        }

        //private void LoadRank()
        //{
        //    OperateGetSecretRank opera = new OperateGetSecretRank(Owner.MainId, Owner.Uid);
        //    Owner.server.Redis.Call(opera, result =>
        //    {
        //        if ((int)result == 1)
        //        {
        //            this.Rank = opera.Rank;
        //        }
        //    });
        //}

        public void ChangeContinueFightState(bool continueFight)
        {
            ContinueFight = continueFight;
        }

        public MSG_ZMZ_SECRETAREA_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_SECRETAREA_INFO msg = new MSG_ZMZ_SECRETAREA_INFO();
            msg.Id = Id;
            msg.Rank = Rank;
            msg.State = (int)State;
            msg.PassTime = PassTime;
            msg.ContinueFight = ContinueFight;
            return msg;
        }

        public void LoadTransform(MSG_ZMZ_SECRETAREA_INFO info)
        {
            Id = info.Id;
            Rank = info.Rank;
            State = (SecretAreaState)info.State;
            PassTime = info.PassTime;
            ContinueFight = info.ContinueFight;
        }
    }
}
