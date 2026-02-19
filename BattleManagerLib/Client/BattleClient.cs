using BattleManagerServerLib;
using CommonUtility;
using Logger;
using ServerModels;
using ServerModels.Server;
using ServerShared;
using System;
using System.Collections.Generic;

namespace BattleServerLib.Client
{
    public class BattleClient : BaseClient
    {
        public bool Attacker = true;
        public bool Ready = false;
        public bool IsTeam = false;

        private ZoneServer curZone;
        public ZoneServer CurZone
        {
            get { return curZone; }
        }

        //private BattleServer curBattle;
        //public BattleServer CurBattle
        //{
        //    get { return curBattle; }
        //}
        public string Name { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public int FaceIcon { get; set; }
        /// <summary>
        /// 是否使用自定义头像
        /// </summary>
        public bool ShowFaceJpg { get; set; }
        /// <summary>
        /// 公会ID
        /// </summary>
        public int GuildId { get; set; }
        /// <summary>
        /// 公会名
        /// </summary>
        public string GuildName { get; set; }
        /// <summary>
        /// 公会图标
        /// </summary>
        public int GuildIcon { get; set; }
        /// <summary>
        /// 头像框
        /// </summary>
        public int FaceFrame { get; set; }
        /// <summary>
        /// 是否邀请公会
        /// </summary>
        public bool InviteGuild { get; set; }

        public List<int> InviteUidLsit = new List<int>();
        #region 外显
        /// <summary>
        /// 头部模型
        /// </summary>
        public int Head;
        /// <summary>
        /// 身体模型
        /// </summary>
        public int Body;
        /// <summary>
        /// 主角时装
        /// </summary>
        public List<int> FashionIds = new List<int>();
        #endregion

        public bool IsRobot { get; set; }

        public bool MustUseSimpleRobot { get; set; }
        public bool MustUseHardRobot { get; set; }
        public bool IsUseSimpleRobot { get; set; }
        public bool IsUseHardRobot { get; set; }
        public bool MatchingValue2Robot { get; set; }

        /// <summary>
        /// 当前积分
        /// </summary>
        public int RankingValue { get; set; }
        /// <summary>
        /// 当前竞技场等级
        /// </summary>
        public int LadderLevel { get; set; }

        public int PVELevel { get; set; }

        /// <summary>
        /// 根据当前积分计算的一个内部等级
        /// </summary>
        public int RankingLevel { get; set; }
        /// <summary>
        /// 匹配简单机器人的补偿值
        /// </summary>
        public int MatchingValue1 { get; set; }
        /// <summary>
        /// 匹配困难机器人的补偿值
        /// </summary>
        public int MatchingValue2 { get; set; }
        /// <summary>
        /// 加入匹配队列时间
        /// </summary>
        public DateTime JoinTime { get; set; }

        /// <summary>
        /// 当前等级机器人信息
        /// </summary>
        public RobotGroups RobotGroups { get; set; }

        public int RobotGroupId { get; set; }
        public int GameLevelId { get; set; }
        public int RValue { get; set; }
        public int TValue { get; set; }
        public int YValue { get; set; }
        /// <summary>
        /// 临时变量的积分值
        /// </summary>
        public int TempRankingValue { get; set; }
        /// <summary>
        /// 临时差值
        /// </summary>
        public int TempValue { get; set; }
        /// <summary>
        /// 匹配到机器后一个时间随机值
        /// </summary>
        public int MatchingRobotTime { get; set; }

        public string AIPolicy { get; set; }

        public int BossId { get; set; }
        public List<RobotConfigCallHero> ConfigCallHeros = new List<RobotConfigCallHero>();
        public void SetRankingInfo(int rankingValue, int matchingValue1, int matchingValue2, int ladderLevel)
        {
            //开始匹配时间
            JoinTime = BattleManagerServerApi.now;
            //连败补偿值
            MatchingValue1 = matchingValue1;
            //大R补偿值
            MatchingValue2 = matchingValue2;
            //当前匹配值
            RankingValue = rankingValue;
            //临时使用值
            TempRankingValue = rankingValue;
            //竞技场等级
            LadderLevel = ladderLevel;

        
            //临时匹配等级
            RankingLevel = TempRankingValue / CommonConst.BATTLE_LEVEL_BASE;

        }

        public void CheckMatchingTimeOutRobot()
        {
        
        }

        private double GetWriteTime()
        {
            return (BattleManagerServerApi.now - JoinTime).TotalSeconds;
        }


        public List<int> GetLevelList(int outtime)
        {
            List<int> list = new List<int>();

            double time = GetWriteTime();
            if (time <= outtime)
            {
                TempValue = GetLevelValue(time);

                if (TempValue != -1)
                {
                    int miniVaule = Math.Max(CommonConst.BATTLE_RANKING_VALUE_MINI, TempRankingValue - TempValue);
                    int minLevel = miniVaule / CommonConst.BATTLE_LEVEL_BASE;
                    int maxLevel = (TempRankingValue + TempValue) / CommonConst.BATTLE_LEVEL_BASE;

                    for (int i = minLevel; i <= maxLevel; i++)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }

        public bool CheckBossMatchingTimeOut(int outtime)
        {
            double time = GetWriteTime();
            if (time <= outtime)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取差值
        /// </summary>
        /// <param name="time">经过的时间</param>
        /// <returns>差值</returns>
        public int GetLevelValue(double time)
        {
            return 0;
        }

        public bool CheckMatchingRobot()
        {
            return true;
        }

        public bool CheckMatchingRobotStart()
        {
            double time = GetWriteTime();
            if (time <= MatchingRobotTime)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void UpdateZone(ZoneServer zone)
        {
            curZone = zone;
        }

        public void Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (curZone != null)
            {
                curZone.Write(msg);
            }
        }
    
    }

}

