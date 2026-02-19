using System;
using ServerShared;
using EnumerateUtility;
using Message.Relation.Protocol.RZ;
using System.Collections.Generic;

namespace RelationServerLib
{
    public partial class Client
    {
        private RelationServerApi server;
        private bool isInDungeon = false;

        public int Uid { get; private set; }

        public int MainId
        { get { return server.MainId; } }

        public bool IsLoaded { get; set; }

        public Boolean IsOnline { get; set; }

        public ZoneServer CurZone { get; set; }

        public CampType Camp { get; set; }

        public int Level { get; set; }

        public int Research { get; set; }

        public int CampRank { get; set; }
        public int ChapterId { get; set; }

        public Family Family { get; set; }

        public Team Team { get; set; }

        //邀请列表
        private Dictionary<int, DateTime> inviteTeamList = new Dictionary<int, DateTime>();
        public Dictionary<int, DateTime> InviteTeamList { get { return this.inviteTeamList; } }


        public Client(int uid, RelationServerApi server, int main_id)
        {
            this.Uid = uid;
            this.server = server;
        }

        public void SetOnline(ZoneServer zone, string lastLoginTime)
        {
            IsOnline = true;
            CurZone = zone;

            if (Team == null)
            {
                // 通知自己退出队伍
                CurZone.Write(new MSG_RZ_QUIT_TEAM { Result = (int)ErrorCode.Success, Uid = Uid });
                return;
            }

            if (Team.IsInBrotherTeam(Uid))
            {
                Team = null;
            }

            if (Team != null)
            {
                Team.SetClient(Uid, this);

                //上线 同步Team信息
                NotifyTeamInfo();
                Team.MemberOnline(Uid);
            }
        }

        public void SetOffline(ZoneServer zone)
        {
            CurZone = null;
            IsOnline = false;

            // 组队数据维护
            if (Team == null)
            {
                return;
            }

            Team.SetClient(this.Uid, null);
            Team.MemberOffline(this.Uid);
            Team = null;

            inviteTeamList.Clear();
        }


        public void SetInDungeon(bool isInDungeon)
        {
            this.isInDungeon = isInDungeon;
        }

        public bool IsInDungeon()
        {
            return isInDungeon || IsTeamInDungeon();
        }

        public bool IsTeamInDungeon()
        {
            return Team == null ? false : Team.InDungeon;
        }


        public void UpdateZone(ZoneServer zone)
        {
            CurZone = zone;

            if (Team == null)
            {
                return;
            }

            // 切换Zone 同步Team信息
            NotifyTeamInfo();
            Team.MemberChangeZone(Uid, CurZone.SubId);
        }


        public void SetCamp(int camp)
        {
            this.Camp = (CampType)camp;
        }

        public void NotifyTeamInfo()
        {
            if (Team == null) return;

            MSG_RZ_CREATE_TEAM notifyTeam = new MSG_RZ_CREATE_TEAM
            {
                Result = (int)ErrorCode.Success,
                Uid = Uid,
                Team = Team.GenerateTeamInfo()
            };
            CurZone.Write(notifyTeam);
        }

        public void Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
            CurZone?.Write(msg, Uid);
        }

        public void JoinFamily(Family family, FamilyTitle title)
        {
            //if (family == null) return;
            //this.family = family;
            //this.FamilyTitle = title;
            //// 通知Zone 更新家族相关数据
            //MSG_RZ_CHAR_FAMILY_INFO notify = new MSG_RZ_CHAR_FAMILY_INFO();
            //notify.Uid = uid;
            //notify.fid = Family.Uid;
            //notify.level = Family.Level;
            //notify.title = (int)FamilyTitle;
            //notify.FamilyName = Family.Name;
            //notify.hasApplication = false;

            //if (curZone != null)
            //{
            //    curZone.Write(notify);
            //}
            //server.DB.Call(new QueryUpdateFamilyMember(Uid, notify.fid, notify.title));

            //删除之前家族BOSS信息
            //server.DB.Call(new QueryDeleteFamilyDungeonMember(Uid));
        }

        public void QuitFamily(bool send_email = false, bool kicked = false)
        {
            //family = null;
            //FamilyTitle = FamilyTitle.Nobody;
            //FamilyContributed = 0;
            //// 同步DB
            ////server.DB.Call(new QueryQuitFamily(Uid));
            //// 在线则通知zone更新缓存
            //if (curZone != null)
            //{
            //    MSG_RZ_QUIT_FAMILY notify = new MSG_RZ_QUIT_FAMILY();
            //    notify.Uid = uid;
            //    notify.Result = (int)ErrorCode.Success;
            //    notify.kicked = kicked;
            //    curZone.Write(notify);
            //}
            //if (send_email)
            //{
            //    // 发邮件
            //    server.SendPersonEmail(mainId, uid, 101);
            //}
        }

    }
}
