using CommonUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RobotManager
    {
        public static List<HeroInfo> GetHeroList(List<int> infoIds)
        {
            List<HeroInfo> infos = new List<HeroInfo>();
            foreach (var id in infoIds)
            {
                RobotHeroInfo robot = RobotLibrary.GetRobotHeroInfo(id);
                if (robot != null)
                {
                    HeroInfo info = InitFromRobotInfo(robot);
                    infos.Add(info);
                }
            }
            return infos;
        }

        public static List<HeroInfo> GetHeroList(List<RobotHeroInfo> robotInfos)
        {
            List<HeroInfo> infos = new List<HeroInfo>();
            foreach (var robotInfo in robotInfos)
            {
                HeroInfo info = InitFromRobotInfo(robotInfo);
                infos.Add(info);
            }
            return infos;
        }

        public static List<int> GetDefensiveHeroList(ArenaRobotInfo arenaRInfo)
        {
            List<int> infoIds = new List<int>();
            RobotHeroInfo info = RobotLibrary.GetRobotHeroInfo(arenaRInfo.RobotId);
            if (info != null)
            {
                infoIds.Add(info.HeroId);
            }
            if (arenaRInfo.DefensiveRobot1 > 0)
            {
                info = RobotLibrary.GetRobotHeroInfo(arenaRInfo.DefensiveRobot1);
                if (info != null)
                {
                    infoIds.Add(info.HeroId);
                }
            }
            if (arenaRInfo.DefensiveRobot2 > 0)
            {
                info = RobotLibrary.GetRobotHeroInfo(arenaRInfo.DefensiveRobot2);
                if (info != null)
                {
                    infoIds.Add(info.HeroId);
                }
            }
            if (arenaRInfo.DefensiveRobot3 > 0)
            {
                info = RobotLibrary.GetRobotHeroInfo(arenaRInfo.DefensiveRobot3);
                if (info != null)
                {
                    infoIds.Add(info.HeroId);
                }
            }
            if (arenaRInfo.DefensiveRobot4 > 0)
            {
                info = RobotLibrary.GetRobotHeroInfo(arenaRInfo.DefensiveRobot4);
                if (info != null)
                {
                    infoIds.Add(info.HeroId);
                }
            }
            return infoIds;
        }

        public static List<int> GetHeroRobotIdList(ArenaRobotInfo arenaRInfo)
        {
            List<int> infoIds = new List<int>();
            infoIds.Add(arenaRInfo.RobotId);
            if (arenaRInfo.DefensiveRobot1 > 0)
            {
                infoIds.Add(arenaRInfo.DefensiveRobot1);
            }
            if (arenaRInfo.DefensiveRobot2 > 0)
            {
                infoIds.Add(arenaRInfo.DefensiveRobot2);
            }
            if (arenaRInfo.DefensiveRobot3 > 0)
            {
                infoIds.Add(arenaRInfo.DefensiveRobot3);
            }
            if (arenaRInfo.DefensiveRobot4 > 0)
            {
                infoIds.Add(arenaRInfo.DefensiveRobot4);
            }
            return infoIds;
        }

        public static List<int> GetHeroRobotIdPosesList(ArenaRobotInfo arenaRInfo)
        {
            List<int> infoIds = new List<int>();
            infoIds.Add(arenaRInfo.DefPos0);
            if (arenaRInfo.DefPos1 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos1);
            }
            if (arenaRInfo.DefPos2 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos2);
            }
            if (arenaRInfo.DefPos3 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos3);
            }
            if (arenaRInfo.DefPos4 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos4);
            }
            return infoIds;
        }

        public static List<int> GetHeroRobotIdPosesList(TeamBattleRobotInfo arenaRInfo)
        {
            List<int> infoIds = new List<int>();
            infoIds.Add(arenaRInfo.DefPos0);
            if (arenaRInfo.DefPos1 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos1);
            }
            if (arenaRInfo.DefPos2 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos2);
            }
            if (arenaRInfo.DefPos3 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos3);
            }
            if (arenaRInfo.DefPos4 >= 0)
            {
                infoIds.Add(arenaRInfo.DefPos4);
            }
            return infoIds;
        }

        public static List<int> GetHeroRobotIdList(TeamBattleRobotInfo teamRInfo)
        {
            List<int> infoIds = new List<int>();
            infoIds.Add(teamRInfo.RobotId);
            if (teamRInfo.DefensiveRobot1 > 0)
            {
                infoIds.Add(teamRInfo.DefensiveRobot1);
            }
            if (teamRInfo.DefensiveRobot2 > 0)
            {
                infoIds.Add(teamRInfo.DefensiveRobot2);
            }
            if (teamRInfo.DefensiveRobot3 > 0)
            {
                infoIds.Add(teamRInfo.DefensiveRobot3);
            }
            if (teamRInfo.DefensiveRobot4 > 0)
            {
                infoIds.Add(teamRInfo.DefensiveRobot4);
            }
            return infoIds;
        }

        public static List<HeroInfo> GetTeamHeroListWithRatio(List<int> infoIds, float ratio, int level)
        {
            List<HeroInfo> infos = new List<HeroInfo>();
            foreach (var id in infoIds)
            {
                RobotHeroInfo robot = RobotLibrary.GetRobotHeroInfo(id);
                if (robot != null)
                {
                    HeroInfo info = InitFromRobotInfo(robot, ratio);
                    info.Level = level;
                    infos.Add(info);
                }
            }
            return infos;
        }

        public static List<HeroInfo> GetTeamHeroList(float robotNatureRatio, int teamRobotId, int level)
        {
            List<HeroInfo> ans = new List<HeroInfo>();

            TeamBattleRobotInfo tempRobotInfo = RobotLibrary.GetTeamRobotInfo(teamRobotId);
            List<int> infoIds = GetHeroRobotIdList(tempRobotInfo);
            ans = GetTeamHeroListWithRatio(infoIds, robotNatureRatio, level);
            return ans;
        }

        public static List<int> GetTeamHeroPosList(int teamRobotId)
        {
            List<int> ans = new List<int>();

            TeamBattleRobotInfo tempRobotInfo = RobotLibrary.GetTeamRobotInfo(teamRobotId);
            ans = GetHeroRobotIdPosesList(tempRobotInfo);
            return ans;
        }

        public static HeroInfo InitFromRobotInfo(RobotHeroInfo robot, float ratio = 1)
        {
            HeroInfo info = new HeroInfo();
            info.Id = robot.HeroId;
            info.GodType = robot.GodType;
            info.Level = robot.Level;
            info.AwakenLevel = robot.AwakenLevel;
            info.StepsLevel = robot.StepsLevel;
            info.SoulSkillLevel = robot.SoulSkillLevel;

            foreach (var kv in robot.NatureList)
            {
                info.SetNatureBaseValue(kv.Key, (int)(kv.Value * ratio));
            }

            info.SetNatureBaseValue(NatureType.PRO_HP, robot.PRO_HP <= 0 ? info.GetNatureValue(NatureType.PRO_MAX_HP) : robot.PRO_HP);//有些地方需要外部传入血量，阵营战
            info.TalentMng = new TalentManager(0, 0, 0, 0, 0);
            info.IsRobotHero = true;
            info.RobotInfo = robot;
            info.BindData();

            return info;
        }

        public static PetInfo GetPetInfoFromRobot(RobotPetInfo robtPet)
        {
            PetInfo info = InitPetInfoFromRobot(robtPet);
            return info;
        }

        public static PetInfo InitPetInfoFromRobot(RobotPetInfo robot, float ratio = 1)
        {
            if (robot == null)
            {
                return null;
            }
            PetInfo info = new PetInfo(robot.Id, robot.Aptitude, robot.Level, robot.BreakLevel, robot.Shape);
            foreach (var kv in robot.NatureList)
            {
                info.SetNatureBaseValue(kv.Key, (int)(kv.Value * ratio));
            }
            info.SetNatureBaseValue(NatureType.PRO_HP, robot.PRO_HP <= 0 ? info.GetNatureValue(NatureType.PRO_MAX_HP) : robot.PRO_HP);//有些地方需要外部传入血量，阵营战           
            //info.RobotInfo = robot;

            return info;
        }
    }
}
