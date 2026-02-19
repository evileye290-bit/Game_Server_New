using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class RobotLibrary
    {

        private static SortedDictionary<int, RobotHeroInfo> heroInfos = new SortedDictionary<int, RobotHeroInfo>();

        private static Dictionary<int, ArenaRobotInfo> arenaRobotInfos = new Dictionary<int, ArenaRobotInfo>();

        private static Dictionary<int, CrossRobotInfo> crossRobotInfos = new Dictionary<int, CrossRobotInfo>();

        private static Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>> crossFinalsRobotInfos = new Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>>();

        private static Dictionary<int, TeamBattleRobotInfo> teamRobotInfos = new Dictionary<int, TeamBattleRobotInfo>();

        private static Dictionary<int, string> robotNames = new Dictionary<int, string>(1000);


        private static Dictionary<int, CrossRobotInfo> crossChallengeRobotInfos = new Dictionary<int, CrossRobotInfo>();

        private static Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>> crossChallengeFinalsRobotInfos = new Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>>();


        public static void Init()
        {

            LoadRobotHeroInfo();

            InitArenaRobotInfos();

            InitCrossRobotInfos();

            InitCrossFinalsRobotInfos();

            InitTeamRobotInfos();

            InitRobotNames();

            InitCrossChallengeRobotInfos();
            InitCrossChallengeFinalsRobotInfos();
        }

        public static void LoadRobotHeroInfo()
        {
            SortedDictionary<int, RobotHeroInfo> heroInfos = new SortedDictionary<int, RobotHeroInfo>();
            //heroInfos.Clear();

            DataList lists = DataListManager.inst.GetDataList("RobotHero");
            foreach (var kv in lists)
            {
                RobotHeroInfo hero = new RobotHeroInfo(kv.Value);
                heroInfos.Add(hero.Id, hero);
            }
            RobotLibrary.heroInfos = heroInfos;
        }

        private static void InitArenaRobotInfos()
        {
            //arenaRobotInfos.Clear();
            Dictionary<int, ArenaRobotInfo> arenaRobotInfos = new Dictionary<int, ArenaRobotInfo>();

            ArenaRobotInfo info;
            DataList dataList = DataListManager.inst.GetDataList("ArenaRobot");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                info = new ArenaRobotInfo();
                info.Id = data.ID;
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");

                info.Name = data.GetString("Name");
                info.Camp = data.GetInt("Camp");
                info.Level = data.GetInt("Level");
                info.LadderLevel = data.GetInt("RankLevel");
                info.BattlePower = data.GetInt("BattlePower");

                info.RobotId = data.GetInt("RobotId");
                info.DefensiveRobot1 = data.GetInt("DefensiveRobot1");
                info.DefensiveRobot2 = data.GetInt("DefensiveRobot2");
                info.DefensiveRobot3 = data.GetInt("DefensiveRobot3");
                info.DefensiveRobot4 = data.GetInt("DefensiveRobot4");
                info.DefPos0 = data.GetInt("DefPos0");
                info.DefPos1 = data.GetInt("DefPos1");
                info.DefPos2 = data.GetInt("DefPos2");
                info.DefPos3 = data.GetInt("DefPos3");
                info.DefPos4 = data.GetInt("DefPos4");
                info.CheckDef();

                RobotHeroInfo robotHero = GetRobotHeroInfo(info.RobotId);
                if (robotHero != null)
                {
                    info.HeroId = robotHero.HeroId;
                }

                if (!arenaRobotInfos.ContainsKey(data.ID))
                {
                    arenaRobotInfos.Add(data.ID, info);
                }
                else
                {
                    Logger.Log.Warn("InitRobotInfos has same id {0}", data.ID);
                }
            }
            RobotLibrary.arenaRobotInfos = arenaRobotInfos;
        }

        private static void InitCrossRobotInfos()
        {
            //crossRobotInfos.Clear();
            Dictionary<int, CrossRobotInfo> crossRobotInfos = new Dictionary<int, CrossRobotInfo>();

            CrossRobotInfo info;
            DataList dataList = DataListManager.inst.GetDataList("CrossRobot");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                info = new CrossRobotInfo();
                info.Id = data.ID;
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");

                info.Name = data.GetString("Name");
                info.Camp = data.GetInt("Camp");
                info.Level = data.GetInt("Level");
                info.CrossLevel = data.GetInt("CrossLevel");
                info.CrossStar = data.GetInt("CrossStar");
                info.BattlePower = data.GetInt("BattlePower");
                info.RobotId = data.GetInt("Queue1Robot1");

                for (int i = 1; i <= 2; i++)
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        int heroRobtId = data.GetInt($"Queue{i}Robot{j}");
                        int posId = data.GetInt($"Queue{i}Pos{j}");
                        RobotHeroInfo robotHero = GetRobotHeroInfo(heroRobtId);
                        if (robotHero != null)
                        {
                            info.HeroId = robotHero.HeroId;
                        }
                        info.AddHero(robotHero, posId, i);
                    }
                }

                if (!crossRobotInfos.ContainsKey(data.ID))
                {
                    crossRobotInfos.Add(data.ID, info);
                }
                else
                {
                    Logger.Log.Warn("InitRobotInfos has same id {0}", data.ID);
                }
            }
            RobotLibrary.crossRobotInfos = crossRobotInfos;
        }

        public static void InitCrossFinalsRobotInfos()
        {
            //crossFinalsRobotInfos.Clear();
            Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>> crossFinalsRobotInfos = new Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>>();

            Dictionary<int, CrossFinalsRobotInfo> dic;
            CrossFinalsRobotInfo info;
            DataList dataList = DataListManager.inst.GetDataList("CrossFinalsRobot");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                info = new CrossFinalsRobotInfo();
                info.Id = data.ID;
                info.Team = data.GetInt("Team");
                info.Index = data.GetInt("Index");

                info.Name = data.GetString("Name");
                info.Camp = data.GetInt("Camp");
                info.Level = data.GetInt("Level");
                info.CrossLevel = data.GetInt("CrossLevel");
                info.CrossStar = data.GetInt("CrossStar");
                info.BattlePower = data.GetInt("BattlePower");
                info.RobotId = data.GetInt("Queue1Robot1");

                for (int i = 1; i <= 2; i++)
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        int heroRobtId = data.GetInt($"Queue{i}Robot{j}");
                        int posId = data.GetInt($"Queue{i}Pos{j}");
                        RobotHeroInfo robotHero = GetRobotHeroInfo(heroRobtId);
                        if (robotHero != null)
                        {
                            info.HeroId = robotHero.HeroId;
                        }
                        info.AddHero(robotHero, posId, i);
                    }
                }

                if (crossFinalsRobotInfos.TryGetValue(info.Team, out dic))
                {
                    dic.Add(info.Index, info);
                }
                else
                {
                    dic = new Dictionary<int, CrossFinalsRobotInfo>();
                    dic.Add(info.Index, info);
                    crossFinalsRobotInfos.Add(info.Team, dic);
                }
            }
            RobotLibrary.crossFinalsRobotInfos = crossFinalsRobotInfos;
        }

        private static void InitTeamRobotInfos()
        {
            //teamRobotInfos.Clear();
            Dictionary<int, TeamBattleRobotInfo> teamRobotInfos = new Dictionary<int, TeamBattleRobotInfo>();

            TeamBattleRobotInfo info;
            DataList dataList = DataListManager.inst.GetDataList("TeamRobot");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                //Uid = this.Uid,
                //Name = this.Name,
                //Sex = this.Sex,
                //Level = this.Level,
                //Icon = this.Icon,
                //IconFrame = this.IconFrame,
                //Job = this.Job,
                //Camp = this.CampId,
                //IsOnline = this.IsOnline,
                //HeroId = this.HeroId,
                info = new TeamBattleRobotInfo();
                info.Id = data.ID;
                info.LevelMin = data.GetInt("LevelMin");
                info.LevelMax = data.GetInt("LevelMax");

                info.Name = data.GetString("Name");
                info.Camp = data.GetInt("Camp");
                info.Level = data.GetInt("Level");
                info.Icon = data.GetInt("Icon");
                info.IconFrame = data.GetInt("IconFrame");
                info.NatureRatio = data.GetFloat("NatureRatio");
                info.BattlePower = data.GetInt("BattlePower");

                info.RobotId = data.GetInt("RobotId");
                info.DefensiveRobot1 = data.GetInt("DefensiveRobot1");
                info.DefensiveRobot2 = data.GetInt("DefensiveRobot2");
                info.DefensiveRobot3 = data.GetInt("DefensiveRobot3");
                info.DefensiveRobot4 = data.GetInt("DefensiveRobot4");
                info.DefPos0 = data.GetInt("DefPos0");
                info.DefPos1 = data.GetInt("DefPos1");
                info.DefPos2 = data.GetInt("DefPos2");
                info.DefPos3 = data.GetInt("DefPos3");
                info.DefPos4 = data.GetInt("DefPos4");
                info.CheckDef();

                RobotHeroInfo robotHero = GetRobotHeroInfo(info.RobotId);
                if (robotHero != null)
                {
                    info.HeroId = robotHero.HeroId;
                }

                if (!teamRobotInfos.ContainsKey(data.ID))
                {
                    teamRobotInfos.Add(data.ID, info);
                }
                else
                {
                    Logger.Log.Warn("InitTeamRobotInfos has same id {0}", data.ID);
                }
            }
            RobotLibrary.teamRobotInfos = teamRobotInfos;
        }

        private static void InitRobotNames()
        {
            Dictionary<int, string> robotNames = new Dictionary<int, string>(1000);
            //robotNames.Clear();
            DataList dataList = DataListManager.inst.GetDataList("TeamRobotName");
            foreach (var item in dataList)
            {
                robotNames.Add(item.Value.ID, item.Value.GetString("Name"));
            }
            RobotLibrary.robotNames = robotNames;
        }

        public static CrossRobotInfo GetCrossRobotInfo(int star)
        {
            foreach (var item in crossRobotInfos)
            {
                if (item.Value.RankMin <= star && star <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static CrossFinalsRobotInfo GetCrossFinalsRobotInfo(int team, int index)
        {
            Dictionary<int, CrossFinalsRobotInfo> dic;
            if (crossFinalsRobotInfos.TryGetValue(team, out dic))
            {
                CrossFinalsRobotInfo info;
                dic.TryGetValue(index, out info);
                return info;
            }
            return null;
        }

        public static ArenaRobotInfo GetArenaRobotInfo(int rank)
        {
            foreach (var item in arenaRobotInfos)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static TeamBattleRobotInfo GetTeamRobotInfo(int teamRobotId)
        {
            //throw new NotImplementedException();
            TeamBattleRobotInfo info = null;
            teamRobotInfos.TryGetValue(teamRobotId, out info);
            return info;
        }

        public static TeamBattleRobotInfo ChooseTeamRobot(int level)
        {
            foreach (var item in teamRobotInfos)
            {
                if (item.Value.LevelMin <= level && level <= item.Value.LevelMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static string GetRandTeamRobotName()
        {
            int rand = RAND.Range(1, robotNames.Count);
            return robotNames[rand];
        }

        //public static TeamBattleRobotInfo GetTeamRobot(int id)
        //{
        //    TeamBattleRobotInfo info = null;
        //    teamRobotInfos.TryGetValue(id, out info);
        //    return info;
        //}

        public static List<RobotHeroInfo> GetRobotHeroList(List<int> infoIds)
        {
            List<RobotHeroInfo> infos = new List<RobotHeroInfo>();
            foreach (var id in infoIds)
            {
                RobotHeroInfo info = GetRobotHeroInfo(id);
                if (info != null)
                {
                    infos.Add(info);
                }
            }
            return infos;
        }

        public static RobotHeroInfo GetRobotHeroInfo(int id)
        {
            RobotHeroInfo info;
            heroInfos.TryGetValue(id, out info);
            return info;
        }

        public static CrossRobotInfo GetCrossChallengeRobotInfo(int star)
        {
            foreach (var item in crossChallengeRobotInfos)
            {
                if (item.Value.RankMin <= star && star <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static CrossFinalsRobotInfo GetCrossChallengeFinalsRobotInfo(int team, int index)
        {
            Dictionary<int, CrossFinalsRobotInfo> dic;
            if (crossChallengeFinalsRobotInfos.TryGetValue(team, out dic))
            {
                CrossFinalsRobotInfo info;
                dic.TryGetValue(index, out info);
                return info;
            }
            return null;
        }


        private static void InitCrossChallengeRobotInfos()
        {
            //crossRobotInfos.Clear();
            Dictionary<int, CrossRobotInfo> crossRobotInfos = new Dictionary<int, CrossRobotInfo>();

            CrossRobotInfo info;
            DataList dataList = DataListManager.inst.GetDataList("CrossChallengeRobot");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                info = new CrossRobotInfo();
                info.Id = data.ID;
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");

                info.Name = data.GetString("Name");
                info.Camp = data.GetInt("Camp");
                info.Level = data.GetInt("Level");
                info.CrossLevel = data.GetInt("CrossLevel");
                info.CrossStar = data.GetInt("CrossStar");
                info.BattlePower = data.GetInt("BattlePower");
                info.RobotId = data.GetInt("Queue1Robot1");

                for (int i = 1; i <= 3; i++)
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        int heroRobtId = data.GetInt($"Queue{i}Robot{j}");
                        int posId = data.GetInt($"Queue{i}Pos{j}");
                        RobotHeroInfo robotHero = GetRobotHeroInfo(heroRobtId);
                        if (robotHero != null)
                        {
                            info.HeroId = robotHero.HeroId;
                        }
                        info.AddHero(robotHero, posId, i);
                    }
                }

                if (!crossRobotInfos.ContainsKey(data.ID))
                {
                    crossRobotInfos.Add(data.ID, info);
                }
                else
                {
                    Logger.Log.Warn("InitRobotInfos has same id {0}", data.ID);
                }
            }
            RobotLibrary.crossChallengeRobotInfos = crossRobotInfos;
        }

        public static void InitCrossChallengeFinalsRobotInfos()
        {
            //crossFinalsRobotInfos.Clear();
            Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>> crossFinalsRobotInfos = new Dictionary<int, Dictionary<int, CrossFinalsRobotInfo>>();

            Dictionary<int, CrossFinalsRobotInfo> dic;
            CrossFinalsRobotInfo info;
            DataList dataList = DataListManager.inst.GetDataList("CrossChallengeFinalsRobot");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                info = new CrossFinalsRobotInfo();
                info.Id = data.ID;
                info.Team = data.GetInt("Team");
                info.Index = data.GetInt("Index");

                info.Name = data.GetString("Name");
                info.Camp = data.GetInt("Camp");
                info.Level = data.GetInt("Level");
                info.CrossLevel = data.GetInt("CrossLevel");
                info.CrossStar = data.GetInt("CrossStar");
                info.BattlePower = data.GetInt("BattlePower");
                info.RobotId = data.GetInt("Queue1Robot1");

                for (int i = 1; i <= 3; i++)
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        int heroRobtId = data.GetInt($"Queue{i}Robot{j}");
                        int posId = data.GetInt($"Queue{i}Pos{j}");
                        RobotHeroInfo robotHero = GetRobotHeroInfo(heroRobtId);
                        if (robotHero != null)
                        {
                            info.HeroId = robotHero.HeroId;
                        }
                        info.AddHero(robotHero, posId, i);
                    }
                }

                if (crossFinalsRobotInfos.TryGetValue(info.Team, out dic))
                {
                    dic.Add(info.Index, info);
                }
                else
                {
                    dic = new Dictionary<int, CrossFinalsRobotInfo>();
                    dic.Add(info.Index, info);
                    crossFinalsRobotInfos.Add(info.Team, dic);
                }
            }
            RobotLibrary.crossChallengeFinalsRobotInfos = crossFinalsRobotInfos;
        }


    }
    //public class RobotLibrary
    //{
    //    private static Dictionary<int, RobotGroup> RobotGroupList = new Dictionary<int, RobotGroup>();

    //    private static Dictionary<int, RobotGroups> BattleLevelRobotList = new Dictionary<int, RobotGroups>();

    //    private static Dictionary<int, RobotInfo> RobotInfoList = new Dictionary<int, RobotInfo>();
    //    public static void BindData()
    //    {
    //        RobotGroupList.Clear();
    //        BattleLevelRobotList.Clear();
    //        RobotInfoList.Clear();

    //        BindBattleLevelRobotData();
    //        BindRobotGroupData();
    //        BindRobotInfoData();
    //    }

    //    private static void BindRobotGroupData()
    //    {
    //        RobotGroup info = new RobotGroup();
    //        RobotMember member = new RobotMember();
    //        DataList gameConfig = DataListManager.inst.GetDataList("RobotGroup");
    //        foreach (var item in gameConfig)
    //        {
    //            info = new RobotGroup();
    //            Data data = item.Value;
    //            info.Id = data.ID;
    //            string[] heros = StringSplit.GetArray("|", data.GetString("heros"));
    //            foreach (var hero in heros)
    //            {
    //                member = new RobotMember();
    //                member.Id = int.Parse(hero);
    //                Data itemData = DataListManager.inst.GetData("HeroCard", member.Id);
    //                if (itemData != null)
    //                {
    //                    member.Quality = itemData.GetString("cardquality");
    //                    info.Heros.Add(member);
    //                }
    //            }

    //            string[] skills = StringSplit.GetArray("|", data.GetString("skills"));
    //            foreach (var skill in skills)
    //            {
    //                member = new RobotMember();
    //                member.Id = int.Parse(skill);
    //                Data itemData = DataListManager.inst.GetData("SkillCard", member.Id);
    //                if (itemData != null)
    //                {
    //                    member.Quality = itemData.GetString("cardquality");
    //                    info.Skills.Add(member);
    //                }
    //            }

    //            string[] backHeros = StringSplit.GetArray("|", data.GetString("backHeros"));
    //            foreach (var hero in backHeros)
    //            {
    //                member = new RobotMember();
    //                member.Id = int.Parse(hero);
    //                Data itemData = DataListManager.inst.GetData("HeroCard", member.Id);
    //                if (itemData != null)
    //                {
    //                    member.Quality = itemData.GetString("cardquality");
    //                    info.BackHeros.Add(member);
    //                }
    //            }
    //            string[] conHeros = StringSplit.GetArray("|", data.GetString("configCallHeros"));
    //            for (int i = 0; i < conHeros.Length; i++)
    //            {
    //                string[] conHeroInfo = StringSplit.GetArray(":", conHeros[i]);
    //                RobotConfigCallHero conCallHero = new RobotConfigCallHero();
    //                conCallHero.Id = int.Parse(conHeroInfo[0]);
    //                conCallHero.PosX = float.Parse(conHeroInfo[1]);
    //                conCallHero.PosY = float.Parse(conHeroInfo[2]);
    //                info.ConfigCallHeros.Add(conCallHero);
    //            }
    //            info.AIPolicy = data.GetString("AI");
    //            RobotGroupList.Add(info.Id, info);
    //        }
    //    }

    //    private static void BindBattleLevelRobotData()
    //    {
    //        RobotGroups robotGroups = new RobotGroups();
    //        DataList gameConfig = DataListManager.inst.GetDataList("RobotLevel");
    //        foreach (var item in gameConfig)
    //        {
    //            robotGroups = new RobotGroups();
    //            Data data = item.Value;

    //            string[] robotGrooups = StringSplit.GetArray("|", data.GetString("HardRobotGroups"));
    //            foreach (var group in robotGrooups)
    //            {
    //                int id = int.Parse(group);
    //                robotGroups.HardGrooupList.Add(id);
    //            }

    //            robotGrooups = StringSplit.GetArray("|", data.GetString("SimpleRobotGroups"));
    //            foreach (var group in robotGrooups)
    //            {
    //                int id = int.Parse(group);
    //                robotGroups.SimpleGrooupList.Add(id);
    //            }

    //            string[] robotInfops = StringSplit.GetArray("|", data.GetString("RobotInfos"));
    //            foreach (var info in robotInfops)
    //            {
    //                int id = int.Parse(info);
    //                robotGroups.InfoList.Add(id);
    //            }

    //            if (robotGroups.HardGrooupList.Count > 0)
    //            {
    //                robotGroups.UseHard = true; 
    //            }
    //            if (robotGroups.SimpleGrooupList.Count > 0)
    //            {
    //                robotGroups.UseSimple = true;
    //            }

    //            BattleLevelRobotList.Add(data.ID, robotGroups);
    //        }
    //    }

    //    private static void BindRobotInfoData()
    //    {
    //        RobotInfo info = new RobotInfo();
    //        DataList gameConfig = DataListManager.inst.GetDataList("RobotInfo");
    //        foreach (var item in gameConfig)
    //        {
    //            info = new RobotInfo();
    //            Data data = item.Value;
    //            info.Id = data.ID;
    //            info.Level = data.GetInt("Level");
    //            info.Name = data.GetString("Name");
    //            info.Uid = data.GetInt("Uid");
    //            info.FaceIcon = data.GetInt("FaceIcon");
    //            info.ShowFaceJpg = data.GetBoolean("ShowFaceJpg");

    //            string[] fashionIds = StringSplit.GetArray("|", data.GetString("FashionIds"));

    //            foreach (var fashionId in fashionIds)
    //            {
    //                int id = int.Parse(fashionId);
    //                Data itemData = DataListManager.inst.GetData("Fashion", id);
    //                if (itemData != null)
    //                {
    //                    FashionType sonType = (FashionType)itemData.GetInt("SonType");
    //                    switch (sonType)
    //                    {
    //                        case FashionType.Head:
    //                            info.Head = id;
    //                            break;
    //                        case FashionType.Clothes:
    //                            info.Body = id;
    //                            break;
    //                        default:
    //                            break;
    //                    }
    //                    info.FashionIds.Add(id);
    //                }
    //            }
    //            RobotInfoList.Add(data.ID, info);

    //        }
    //    }

    //    public static RobotGroups GetBattleLevelRobot(int level)
    //    {
    //        RobotGroups info;
    //        BattleLevelRobotList.TryGetValue(level, out info);
    //        return info;
    //    }

    //    public static RobotGroup GetRobotGroup(int id)
    //    {
    //        RobotGroup info;
    //        RobotGroupList.TryGetValue(id, out info);
    //        return info;
    //    }

    //    public static RobotInfo GetRobotInfo(int id)
    //    {
    //        RobotInfo info;
    //        RobotInfoList.TryGetValue(id, out info);
    //        return info;
    //    }
    //}
}
