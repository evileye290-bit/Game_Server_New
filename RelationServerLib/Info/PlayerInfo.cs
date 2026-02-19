using Message.Relation.Protocol.RR;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;

namespace RelationServerLib
{
    public static class PlayerInfo
    {
        #region PLAYER_BASE_INFO
        public static MSG_RZ_PLAYER_BASE_INFO GetRZPlayerBaseInfo(MSG_RR_PLAYER_BASE_INFO sinfo)
        {
            MSG_RZ_PLAYER_BASE_INFO info = new MSG_RZ_PLAYER_BASE_INFO();
            info.Uid = sinfo.Uid;
            info.Name = sinfo.Name;
            info.FaceIcon = sinfo.FaceIcon;
            info.ShowFaceJpg = sinfo.ShowFaceJpg;
            info.FaceFrame = sinfo.FaceFrame;
            info.Level = sinfo.Level;
            info.Sex = sinfo.Sex;
            info.LadderScore = sinfo.LadderScore;
            info.LadderLevel = sinfo.LadderLevel;
            info.IsOnline = sinfo.IsOnline;
            info.LogOutTime = sinfo.LogOutTime;
            info.PopScore = sinfo.PopScore;
            info.Rank = sinfo.Rank;
            info.GuildId = sinfo.GuildId;
            info.GuildName = sinfo.GuildName;
            info.GuildIcon = sinfo.GuildIcon;
            return info;
        }

        public static MSG_RZ_PLAYER_BASE_INFO GetRZPlayerBaseInfo(MSG_ZR_PLAYER_BASE_INFO sinfo)
        {
            MSG_RZ_PLAYER_BASE_INFO info = new MSG_RZ_PLAYER_BASE_INFO();
            info.Uid = sinfo.Uid;
            info.Name = sinfo.Name;
            info.FaceIcon = sinfo.FaceIcon;
            info.ShowFaceJpg = sinfo.ShowFaceJpg;
            info.FaceFrame = sinfo.FaceFrame;
            info.Level = sinfo.Level;
            info.Sex = sinfo.Sex;
            info.LadderScore = sinfo.LadderScore;
            info.LadderLevel = sinfo.LadderLevel;
            info.IsOnline = sinfo.IsOnline;
            info.LogOutTime = sinfo.LogOutTime;
            info.PopScore = sinfo.PopScore;
            info.Rank = sinfo.Rank;
            info.GuildId = sinfo.GuildId;
            info.GuildName = sinfo.GuildName;
            info.GuildIcon = sinfo.GuildIcon;
            return info;
        }

        public static MSG_RR_PLAYER_BASE_INFO GetRRPlayerBaseInfo(MSG_ZR_PLAYER_BASE_INFO sinfo)
        {
            MSG_RR_PLAYER_BASE_INFO info = new MSG_RR_PLAYER_BASE_INFO();
            info.Uid = sinfo.Uid;
            info.Name = sinfo.Name;
            info.FaceIcon = sinfo.FaceIcon;
            info.ShowFaceJpg = sinfo.ShowFaceJpg;
            info.FaceFrame = sinfo.FaceFrame;
            info.Level = sinfo.Level;
            info.Sex = sinfo.Sex;
            info.LadderScore = sinfo.LadderScore;
            info.LadderLevel = sinfo.LadderLevel;
            info.IsOnline = sinfo.IsOnline;
            info.LogOutTime = sinfo.LogOutTime;
            info.PopScore = sinfo.PopScore;
            info.Rank = sinfo.Rank;
            info.GuildId = sinfo.GuildId;
            info.GuildName = sinfo.GuildName;
            info.GuildIcon = sinfo.GuildIcon;
            return info;
        }
        #endregion

        #region CHAT_PLAYER_INFO
        public static MSG_RZ_CHAT_PLAYER_INFO GetRZChatInfo(MSG_RR_CHAT_PLAYER_INFO info)
        {
            MSG_RZ_CHAT_PLAYER_INFO pcInfo = new MSG_RZ_CHAT_PLAYER_INFO();
            pcInfo.PcUid = info.PcUid;
            pcInfo.Name = info.Name;
            pcInfo.Level = info.Level;
            pcInfo.Sex = info.Sex;
            pcInfo.Title = info.Title;
            pcInfo.Grade = info.Grade;

            pcInfo.FaceIcon = info.FaceIcon;
            pcInfo.ShowFaceJpg = info.ShowFaceJpg;
            pcInfo.FaceFrame = info.FaceFrame;
            pcInfo.LadderLevel = info.LadderLevel;
            pcInfo.ChatFrame = info.ChatFrame;
            pcInfo.PopScore = info.PopScore;
            pcInfo.GuildId = info.GuildId;
            pcInfo.GuildName = info.GuildName;
            pcInfo.GuildIcon = info.GuildIcon;
            return pcInfo;
        }

        public static MSG_RZ_CHAT_PLAYER_INFO GetRZChatInfo(MSG_ZR_CHAT_PLAYER_INFO info)
        {
            MSG_RZ_CHAT_PLAYER_INFO pcInfo = new MSG_RZ_CHAT_PLAYER_INFO();
            pcInfo.PcUid = info.PcUid;
            pcInfo.Name = info.Name;
            pcInfo.Level = info.Level;
            pcInfo.Sex = info.Sex;
            pcInfo.Title = info.Title;
            pcInfo.Grade = info.Grade;

            pcInfo.FaceIcon = info.FaceIcon;
            pcInfo.ShowFaceJpg = info.ShowFaceJpg;
            pcInfo.FaceFrame = info.FaceFrame;
            pcInfo.LadderLevel = info.LadderLevel;
            pcInfo.ChatFrame = info.ChatFrame;
            pcInfo.MainId = info.MainId;
            pcInfo.PopScore = info.PopScore;
            pcInfo.GuildId = info.GuildId;
            pcInfo.GuildName = info.GuildName;
            pcInfo.GuildIcon = info.GuildIcon;
            return pcInfo;
        }

        public static MSG_RR_CHAT_PLAYER_INFO GetRRChatInfo(MSG_ZR_CHAT_PLAYER_INFO info)
        {
            MSG_RR_CHAT_PLAYER_INFO pcInfo = new MSG_RR_CHAT_PLAYER_INFO();
            pcInfo.PcUid = info.PcUid;
            pcInfo.Name = info.Name;
            pcInfo.Level = info.Level;
            pcInfo.Sex = info.Sex;
            pcInfo.Title = info.Title;
            pcInfo.Grade = info.Grade;

            pcInfo.FaceIcon = info.FaceIcon;
            pcInfo.ShowFaceJpg = info.ShowFaceJpg;
            pcInfo.FaceFrame = info.FaceFrame;
            pcInfo.LadderLevel = info.LadderLevel;
            pcInfo.ChatFrame = info.ChatFrame;
            pcInfo.MainId = info.MainId;
            pcInfo.PopScore = info.PopScore;
            pcInfo.GuildId = info.GuildId;
            pcInfo.GuildName = info.GuildName;
            pcInfo.GuildIcon = info.GuildIcon;
            return pcInfo;
        }
        #endregion





    }
}
