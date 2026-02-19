using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
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
        //采集消耗的行动力
        public int ExpendedAction;

        //采集
        public void CampGather()
        {
            MSG_ZGC_CAMP_GATHER response = new MSG_ZGC_CAMP_GATHER();

            //判断行动力是否够
            int actionCount = GetCounterValue(CounterType.ActionCount);
            if (actionCount <= 0)
            {
                Log.Warn($"player {Uid} camp gather fail, action count not enough");
                response.Result = (int)ErrorCode.ActionCountNotEnough;
                Write(response);
                return;
            }

            int rand = NewRAND.Next(0, 10000);
            CampGatherModel model = GetCampGatherRandomEvent(rand);
            if (model == null)
            {
                Log.Warn($"player {Uid} camp gather fail, camp gather xml error");
                return;
            }

            CampBattleStep step = GetCampBattleStep();
            //扣行动力
            CampBattleExpendModel expendModel = GetCampBattleExpend(step);
            if (expendModel != null)
            {
                UpdateCounter(CounterType.ActionCount, -expendModel.CollectionPoint.Item1);
                ExpendedAction = expendModel.CollectionPoint.Item1;
            }
            //加积分
            CampScoreRuleModel scoreModel = GetBattleStepScore(1);
            if (scoreModel != null)
            {
                //UpdateCampScore(scoreModel.GatherScore);
                AddCampBattleRankScore(RankType.CampBattleScore, scoreModel.GatherScore);
            }
            if (model.Type == (int)GatherRandomType.Normal)
            {
                //发放奖励
                RewardManager manager = GetSimpleReward(model.BasicReward, ObtainWay.CampGather);
                //komoelog
                List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, (int)CounterType.ActionCount, 1);
                KomoeEventLogCampBattle(((int)Camp).ToString(), Camp.ToString(), 0, 2, HeroMng.CalcBattlePower(), 0, 0, "", "", "", 2, consume);
            }

            response.Id = model.Id;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private CampGatherModel GetCampGatherRandomEvent(int rand)
        {
            return CampGatherLibrary.GetCampGatherRandomEvent(rand);
        }

        public void CampGatherSuccessReward(DungeonModel dungeon, RewardManager mng)
        {
            CampGatherModel model = GetCampGatherByDungeonId(dungeon.Id);
            if (model == null)
            {
                Log.Warn($"player {Uid} camp gather enemy dungeon get reward fail, camp gather xml error");
                return;
            }
            //战斗胜利加积分
            //UpdateCampScore(model.SuccessScore);
            AddCampBattleRankScore(RankType.CampBattleScore, model.SuccessScore);

            RewardManager manager = GetCampGatherDungeonReward(mng, model, ObtainWay.CampGather);    
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = dungeon.Id;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);
        }

        public void CampGatherFailReward(DungeonModel dungeon, DungeonResult result)
        {
            CampGatherModel model = GetCampGatherByDungeonId(dungeon.Id);
            if (model == null)
            {
                Log.Warn($"player {Uid} camp gather enemy dungeon get reward fail, camp gather xml error");
                return;
            }
            RewardManager manager = GetSimpleReward(model.FailReward, ObtainWay.CampGather);
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = dungeon.Id;
            rewardMsg.Result = (int)result;
            Write(rewardMsg);
        }

        private CampGatherModel GetCampGatherByDungeonId(int dungeonId)
        {
            return CampGatherLibrary.GetCampGatherByDungeonId(dungeonId);
        }

        private RewardManager GetCampGatherDungeonReward(RewardManager mng, CampGatherModel model, ObtainWay way, int batchCount = 1, string extraParam = "")
        {        
            if (mng == null)
            {
                mng = new RewardManager();
            }
            string rewardStr = model.SuccessReward;
            if (!string.IsNullOrEmpty(rewardStr))
            {
                rewardStr = SoulBoneLibrary.ReplaceSoulBone4AllRewards(rewardStr, HeroMng.GetFirstHeroJob());
                List<ItemBasicInfo> rewards = RewardDropLibrary.GetSimpleRewards(rewardStr, batchCount);
                mng.AddReward(rewards);
            }
            if (!string.IsNullOrEmpty(model.ExtraReward))
            {
                List<ItemBasicInfo> items = GetProbability(model.ExtraReward);
                mng.AddReward(items);
            }

            mng.BreakupRewards();
            AddRewards(mng, way, extraParam);

            return mng;
        }

        public void GatherDialogueComplete(int id, bool refuse)
        {
            MSG_ZGC_GATHER_DIALOGUE_COMPLETE response = new MSG_ZGC_GATHER_DIALOGUE_COMPLETE();

            CampGatherModel model = GetCampGatherById(id);
            if (model == null)
            {
                Log.Warn($"player {Uid} gather dialogue complete fail, not find gather model {id} in xml");
                return;
            }

            if (model.Type != (int)GatherRandomType.Npc && model.Type != (int)GatherRandomType.Dungeon)
            {
                Log.Warn($"player {Uid} gather dialogue complete fail, gather id {id} param error");
                return;
            }

            //发放奖励
            RewardManager manager = new RewardManager();
            if (!refuse)
            {
                string rewardStr = model.BasicReward;
                rewardStr = SoulBoneLibrary.ReplaceSoulBone4AllRewards(rewardStr, HeroMng.GetFirstHeroJob());

                if (!string.IsNullOrEmpty(rewardStr))
                {
                    List<ItemBasicInfo> rewards = RewardDropLibrary.GetSimpleRewards(rewardStr);
                    manager.AddReward(rewards);
                }
                if (!string.IsNullOrEmpty(model.ExtraReward))
                {
                    List<ItemBasicInfo> items = GetProbability(model.ExtraReward);
                    manager.AddReward(items);
                }
            }
            else
            {
                string basicRewardStr = model.BasicReward;
                basicRewardStr = SoulBoneLibrary.ReplaceSoulBone4AllRewards(basicRewardStr, HeroMng.GetFirstHeroJob());

                if (!string.IsNullOrEmpty(basicRewardStr))
                {
                    List<ItemBasicInfo> basicRewards = RewardDropLibrary.GetSimpleRewards(basicRewardStr);
                    manager.AddReward(basicRewards);
                }
            }

            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.CampGather);

            manager.GenerateRewardItemInfo(response.Rewards);

            response.Id = model.Id;
            Write(response);        
        }

        private CampGatherModel GetCampGatherById(int id)
        {
            return CampGatherLibrary.GetCampGatherById(id);
        }

        private CampScoreRuleModel GetBattleStepScore(int result)
        {
            return CampGatherLibrary.GetBattleStepScore(result);
        }

        private CampBattleExpendModel GetCampBattleExpend(CampBattleStep step)
        {
            return CampGatherLibrary.GetCampBattleExpend();
        }

        private bool CheckCampGatherHasDungeon(int dungeonId)
        {
            return CampGatherLibrary.CheckCampGatherHasDungeon(dungeonId);
        }

        private List<ItemBasicInfo> GetProbability(string resourceString)
        {
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            //拆开字符串
            string[] resourceList = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);         
            foreach (string resourceItem in resourceList)
            {
                //取出单个设置
                string[] resource = resourceItem.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);              
                CheckAddItem(getItems, resource);
            }
            return getItems;
        }

        private void CheckAddItem(List<ItemBasicInfo> getItems, string[] resource)
        {          
            int count = RAND.Range(int.Parse(resource[2]), int.Parse(resource[3]));
            getItems.Add(new ItemBasicInfo(int.Parse(resource[1]), int.Parse(resource[0]), count, null));
        }
    }
}
