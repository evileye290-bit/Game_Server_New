using CommonUtility;
using EnumerateUtility;
using Message.Relation.Protocol.RZ;
using ServerModels;
using ServerShared;

namespace RelationServerLib
{
    public class TeamMember
    {
        public Client Client { get; private set; }

        public int Uid { get; set; }//{ get { return memberInfo.Uid; } }
        public string Name { get; set; }//{ get { return memberInfo.Name; } }
        public int Sex { get; set; }//{ get { return memberInfo.Sex; } }
        public int Level { get; set; }//{ get { return memberInfo.Level; } }
        public int Icon { get; set; }//{ get { return memberInfo.Icon; } }
        public int IconFrame { get; set; }// { get { return memberInfo.IconFrame; } }
        public int GodType { get; set; }// { get { return memberInfo.IconFrame; } }
        public int Job { get; set; }//{ get { return memberInfo.Job; } }
        public int CampId { get; set; }//{ get { return memberInfo.CampId; } }
        public bool IsOnline { get; set; }//{ get { return Client != null && Client.IsOnline || this is Robot; } }//在线状态需要及时刷新
        public int HeroId { get; set; }//{ get { return memberInfo.HeroId; } }
        public int BattlePower { get; set; }
        public int HeroMaxLevel { get; set; }
        public int Research { get; set; }

        public bool IsRobot => this is Robot;

        public bool IsAllowOffline { get; internal set; }

        public TeamMember(Client client)
        {
            this.Client = client;
        }

        public TeamMember(Client client, RedisPlayerInfo info)
        {
            this.Client = client;

            Level = info.GetIntValue(HFPlayerInfo.Level);
            Uid = info.GetIntValue(HFPlayerInfo.Uid);
            Name = info.GetStringValue(HFPlayerInfo.Name);
            Sex = info.GetIntValue(HFPlayerInfo.Sex);
            Icon = info.GetIntValue(HFPlayerInfo.Icon);
            IconFrame = info.GetIntValue(HFPlayerInfo.IconFrame);
            Job = info.GetIntValue(HFPlayerInfo.Job);
            CampId = info.GetIntValue(HFPlayerInfo.CampId);
            IsOnline = info.GetBoolValue(HFPlayerInfo.IsOnline);
            HeroId = info.GetIntValue(HFPlayerInfo.HeroId);
            BattlePower = info.GetIntValue(HFPlayerInfo.BattlePower);
            GodType = info.GetIntValue(HFPlayerInfo.GodType);
            Research = info.GetIntValue(HFPlayerInfo.Research);
        }

        public void SetClient(Client client)
        {
            this.Client = client;
        }

        public virtual bool CheckOnline()
        {
            return Client?.CurZone != null;
        }

        public MSG_RZ_TEAM_MEMBER GenerateMemberInfo()
        {
            MSG_RZ_TEAM_MEMBER info = new MSG_RZ_TEAM_MEMBER()
            {
                Uid = this.Uid,
                Name = this.Name,
                Sex = this.Sex,
                Level = this.Level,
                Icon = this.Icon,
                IconFrame = this.IconFrame,
                Job = this.Job,
                Camp = this.CampId,
                IsOnline = this.Client != null && Client.IsOnline || this is Robot,
                HeroId = this.HeroId,
                BattlePower = this.BattlePower,
                Chapter = Client?.ChapterId ?? 0,
                IsAllowOffline = this.IsAllowOffline,
                GodType = this.GodType,
                Research = Client?.Research ?? Research
            };

            info.IsRobot = this is Robot || IsAllowOffline; 

            return info;
        }
    }

    public class Robot : TeamMember
    {
        TeamBattleRobotInfo robotInfo = null;
        public Robot(int uid, TeamMember cap, TeamBattleRobotInfo robotInfo) : base(null)
        {
            this.robotInfo = robotInfo;

            HeroModel model = HeroLibrary.GetHeroModel(robotInfo.HeroId);

            Uid = uid;
            Name = RobotLibrary.GetRandTeamRobotName();
            Sex = RAND.Happened(5000) ? 1 : 2;
            Icon = robotInfo.Icon;
            //ShowDIYIcon = false;
            IconFrame = robotInfo.IconFrame;
            Level = cap.Level;
            HeroId = robotInfo.HeroId;
            if (model != null)
            {
                Job = model.Job;
            }
            CampId = cap.CampId;
            BattlePower = (int)(robotInfo.BattlePower * robotInfo.NatureRatio);
            HeroMaxLevel = cap.HeroMaxLevel;
            GodType = 0;
        }

        public float GetRatio()
        {
            if (robotInfo == null)
            {
                return 0;
            }

            return robotInfo.NatureRatio;
        }

        public override bool CheckOnline()
        {
            return true;
        }

        public int GetRobotId()
        {
            if (robotInfo == null)
            {
                return 0;
            }
            return robotInfo.Id;
        }
    }
}
