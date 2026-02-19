using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
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
        public CampBuildManager CampBuild;

        public void InitCampBuildManager()
        {
            CampBuild = new CampBuildManager(this, server);
        }

        public void LoadCampBuildInfo(DBUtility.QueryLoadCampInfo queryLoadCampBuildInfo)
        {
            CampBuild.Init(queryLoadCampBuildInfo);
        }

        /// <summary>
        //扔色子Go
        /// </summary>
        public void CampBuildGo()
        {
            if (!CampBuild.Check())
            {
                Log.Warn($"player {Uid} camp build go failed: not have camp");
                return;
            }
            //次数效验
            int diceCount = GetCounter(CounterType.CampBuildRefreshDiceCount).Count;
            if (diceCount <= 0)
            {
                MSG_ZGC_CAMPBUILD_GO result = new MSG_ZGC_CAMPBUILD_GO
                {
                    Result = (int)ErrorCode.DiceCountNotEnough,
                };
                Write(result);
                Log.Warn($"player {Uid} camp build go failed: refresh dice count not enough");
                return;
            }
            UpdateCounter(CounterType.CampBuildRefreshDiceCount, -1);

            //投色子步数点
            int stepPoint = CampBuild.RandomStepPoint();
            MSG_ZGC_CAMPBUILD_GO msg = new MSG_ZGC_CAMPBUILD_GO
            {
                Result = (int)ErrorCode.Success,
                StepPoint = stepPoint
            };
            Write(msg);
            
            CampBuild.BuildGo(stepPoint);

            AddPassCardTaskNum(TaskType.CampBuild);
            AddDriftExploreTaskNum(TaskType.CampBuild);
        }
             
        public void ShowCampBuildRankList(int page)
        {
            MSG_ZR_GET_CAMPBATTLE_RANK_LIST req = new MSG_ZR_GET_CAMPBATTLE_RANK_LIST();
            req.Page = page;
            req.Camp = (int)Camp;
            req.RankType = (int)RankType.CampBuildValue;
            server.SendToRelation(req, uid);
        }

        public void OpenCampBuildBox(int boxType)
        {
            if (Camp == CampType.None)
            {
                Log.Warn($"player {Uid} open camp build box failed: not have camp");
                return;
            }

            MSG_ZGC_OPEN_CAMPBUILD_BOX req = new MSG_ZGC_OPEN_CAMPBUILD_BOX();
            req.BoxType = (int)boxType;
            int phaseNum = GetCampBuild().PhaseNum;
            RewardManager rewardManager;
            int param ;
            if (CampBuild.OpenCampBuildBox(boxType, phaseNum,out rewardManager,out param))
            {
                rewardManager.GenerateRewardItemInfo(req.Rewards);
                req.Result = (int)ErrorCode.Success;
                req.Param = param;
            }
            else
            {
                Log.Warn($"player {Uid} open camp build box failed: can not open box");
                req.Result = (int)ErrorCode.Fail;
            }
            Write(req);
            
        }

        internal void GetCampBuildInfo()
        {
            if (Camp == CampType.None)
            {
                return;
            }

            MSG_ZR_CAMPBUILD_INFO msg = new MSG_ZR_CAMPBUILD_INFO();
            msg.Camp = (int)Camp;
            server.RelationServer.Write(msg,uid);
        }

        public void SendCampBuildInfo()
        {
            GetCampBuildInfo();
        }

        public void SendCampBuildInfoMsg()
        {
            CampBuild.SendCampBuildBuildPhaseMsg();
        }
     
        public void SendCampBuildSyncInfoMsg()
        {
            CampBuild.SendCampBuildSyncInfoMsg();
        }

        public CampBuildPhaseInfo GetCampBuild()
        {
            switch (Camp)
            {
                case CampType.None:
                    break;
                case CampType.TianDou:
                    return server.RelationServer.TianDouCampBuild;
                case CampType.XingLuo:
                    return server.RelationServer.XinLuoCampBuild;
                default:
                    break;
            }
            return null;
        }

        public ZMZ_CAMP_BUILD_INFO GetCampBuildTransform()
        {
            return CampBuild.GetCampBuildTransform();
        }

        public void LoadCampBuildTransform(ZMZ_CAMP_BUILD_INFO info)
        {
            CampBuild.LoadCampBuildTransform(info);
        }
    }
}
