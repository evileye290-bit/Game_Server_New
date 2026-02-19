using DBUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CampBattleManager
    {
        private PlayerChar owner;

        /// <summary>
        /// 阵营积分
        /// </summary>
        public int CampScore = 0;
        public int HistoricalMaxCampScore = 0;
        
        /// <summary>
        /// 出战次数
        /// </summary>
        public int CampFight = 0;

        /// <summary>
        /// 个人粮草
        /// </summary>
        public int CampCollection = 0;

        /// <summary>
        /// 建设值
        /// </summary>
        public int CampBuildValue = 0;

        public bool IsInBattleRank = false;

        public CampBattleManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(QueryLoadCampInfo queryLoadCampBattleInfo)
        {
            if (owner.Camp == CampType.None)
            {
                return;
            }

            //FIXME:此处这一堆数据的维护堪称灾难。在这里先这样处理。
            OperateGetCampScore operateGetCampScore = new OperateGetCampScore(owner.server.MainId, (int)owner.Camp, RankType.CampBattleScore, owner.Uid);
            owner.server.GameRedis.Call(operateGetCampScore, ret =>
            {
                if ((int)ret == 1)
                {
                    int score = operateGetCampScore.Score;
              
                    CampScore = queryLoadCampBattleInfo.CampScore;
                    CampScore = score;

                    CampFight = queryLoadCampBattleInfo.CampFight;
                    CampCollection = queryLoadCampBattleInfo.CampCollection;
                    HistoricalMaxCampScore = queryLoadCampBattleInfo.HistoricalMaxCampScore;
                    if (CampScore > HistoricalMaxCampScore)
                    {
                        HistoricalMaxCampScore = CampScore;
                        OperateRecordHistoricalMaxCampScore oper = new OperateRecordHistoricalMaxCampScore(owner.server.MainId, owner.Uid, HistoricalMaxCampScore);
                        owner.server.GameRedis.Call(oper);
                    }

                    MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE msg = new MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE();
                    msg.Uid = owner.Uid;
                    msg.Score = HistoricalMaxCampScore;
                    owner.server.RelationServer.Write(msg);
                }
            });

        }

        public void LoadCampScoreFromRedis()
        {
            OperateGetCampScore operateGetCampScore = new OperateGetCampScore(owner.server.MainId, (int)owner.Camp, RankType.CampBattleScore, owner.Uid);
            owner.server.GameRedis.Call(operateGetCampScore, ret =>
            {
                if ((int)ret == 1)
                {
                    int score = operateGetCampScore.Score;
                    CampScore = score;
                    if (CampScore > HistoricalMaxCampScore)
                    {
                        HistoricalMaxCampScore = CampScore;
                    }
                }
            });
        }


        public ZMZ_CAMP_BATTLE_INFO GetCampBattleTransform()
        {
            ZMZ_CAMP_BATTLE_INFO msg = new ZMZ_CAMP_BATTLE_INFO();
            msg.CampScore = CampScore;
            msg.HistoricalMaxCampScore = HistoricalMaxCampScore;
            msg.CampFight = CampFight;
            msg.CampCollection = CampCollection;
            msg.IsInBattleRank = IsInBattleRank;
            return msg;
        }

        public void LoadCampBattleTransform(ZMZ_CAMP_BATTLE_INFO info)
        {
            CampScore = info.CampScore;
            CampFight = info.CampFight;
            CampCollection = info.CampCollection;
            HistoricalMaxCampScore = info.HistoricalMaxCampScore;
            IsInBattleRank = info.IsInBattleRank;
        }
    }
}
