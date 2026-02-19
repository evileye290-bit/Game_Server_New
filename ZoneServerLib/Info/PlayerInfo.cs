using CommonUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;

namespace ZoneServerLib
{
    public static class PlayerInfo
    {
        #region PLAYER_BASE_INFO
        public static PLAYER_BASE_INFO GetPlayerBaseInfo(PlayerChar player)
        {
            PLAYER_BASE_INFO info = new PLAYER_BASE_INFO();
            info.Uid = player.Uid;
            info.Name = player.Name;
            info.Icon = player.Icon;
            info.ShowDIYIcon = player.ShowDIYIcon;
            info.IconFrame = player.GetFaceFrame();
            info.MainId = player.server.MainId;

            info.Level = player.Level;
            info.Sex = player.Sex;
            info.IsOnline = true;
            info.LogOutTime = Timestamp.GetUnixTimeStampSeconds(player.LastLoginTime); //离线时间，(好友列表时使用)
            info.Camp = (int)player.Camp;
            info.HeroId = player.HeroId;

            info.LadderLevel = player.ArenaMng.Level;
            info.BattlePower = player.HeroMng.CalcBattlePower();

            info.GodType = player.GodType;
            info.Research = player.HuntingManager.Research;
            return info;
        }


        public static PLAYER_BASE_INFO GetPlayerBaseInfo(PlayerBaseInfo sinfo)
        {
            PLAYER_BASE_INFO info = new PLAYER_BASE_INFO();
            info.Uid = sinfo.Uid;
            info.Name = sinfo.Name;
            info.Icon = sinfo.Icon;
            info.GodType = sinfo.GodType;
            info.ShowDIYIcon = sinfo.ShowDIYIcon;
            info.IconFrame = sinfo.IconFrame;
            info.MainId = sinfo.MainId;

            info.Level = sinfo.Level;
            info.Sex = sinfo.Sex;
            info.IsOnline = sinfo.IsOnline;
            info.LogOutTime = sinfo.LastLogoutTime;
            info.Camp = sinfo.CampId;
            info.HeroId = sinfo.HeroId;

            info.LadderLevel = sinfo.LadderLevel;  
            info.BattlePower = sinfo.BattlePower;
            info.Research = sinfo.Research;
            return info;
        }

        public static PLAYER_BASE_INFO GetPlayerBaseInfo(ArenaRobotInfo robot, int mainId)
        {
            PLAYER_BASE_INFO info = new PLAYER_BASE_INFO();
            info.Uid = 0;
            info.Name = robot.Name;
            info.Icon = robot.Icon;
            info.ShowDIYIcon = false;
            info.IconFrame = robot.IconFrame;
            info.MainId = mainId;
            info.HeroId = robot.HeroId;

            info.Camp = robot.Camp;
            info.Level = robot.Level;
            info.LadderLevel = robot.LadderLevel;
            info.BattlePower = robot.BattlePower;
            info.IconFrame = 0;

            return info;
        }

        #endregion

        #region CHAT_PLAYER_INFO
        public static PLAYER_INFO GetChatPlayerInfo(PlayerChar player)
        {
            PLAYER_INFO info = new PLAYER_INFO();
            info.PcUid = player.Uid;
            info.Level = player.Level;
            info.Name = player.Name;
            info.Sex = player.Sex;
            info.Title = player.TitleMng.CurTitleId;
            info.Camp = (int)player.Camp;
            info.FaceIcon = player.Icon;
            info.ShowFaceJpg = player.ShowDIYIcon;
            info.FaceFrame = player.GetFaceFrame();
            info.HeroId = player.HeroId;
            info.GodType = player.GodType;
            info.ArenaLevel = player.ArenaMng.Level;
            return info;
        }

        public static SPEAKER_INFO GetChatSpeakerInfo(PlayerChar player)
        {
            SPEAKER_INFO info = new SPEAKER_INFO();
            info.Uid = player.Uid;
            info.Name = player.Name;
            info.Camp = (int)player.Camp;
            info.Level = player.Level;
            info.FaceIcon = player.Icon;
            info.ShowFaceJpg = player.ShowDIYIcon;
            info.FaceFrame = player.GetFaceFrame();
            info.Sex = player.Sex;
            info.Title = player.TitleMng.CurTitleId;
            info.HeroId = player.HeroId;
            info.GodType = player.GodType;
            info.ArenaLevel = player.ArenaMng.Level;
            if (player.Team != null)
            {
                info.TeamId = player.Team.TeamId;
            }
            return info;
        }

        //public static PLAYER_INFO GetChatPlayerInfo(MSG_RZ_CHAT_PLAYER_INFO info)
        //{
        //    PLAYER_INFO pcInfo = new PLAYER_INFO();
        //    pcInfo.PcUid = info.PcUid;
        //    pcInfo.Name = info.Name;
        //    pcInfo.Level = info.Level;
        //    pcInfo.Sex = info.Sex;
        //    pcInfo.Title = info.Title;

        //    pcInfo.FaceIcon = info.FaceIcon;
        //    pcInfo.ShowFaceJpg = info.ShowFaceJpg;
        //    pcInfo.FaceFrame = info.FaceFrame;
         
        //    return pcInfo;
        //}

        //public static MSG_ZR_CHAT_PLAYER_INFO GetZRPersonChatPlayerInfo(PlayerChar player)
        //{
        //    MSG_ZR_CHAT_PLAYER_INFO info = new MSG_ZR_CHAT_PLAYER_INFO();
        //    info.PcUid = player.Uid;
        //    info.Level = player.Level;
        //    info.Name = player.Name;
        //    info.Sex = player.Sex;

        //    info.FaceIcon = player.Icon;
        //    info.ShowFaceJpg = player.ShowDIYIcon;
        //    info.FaceFrame = player.GetFaceFrame();
        //    //info.LadderLevel = player.GetLadderLevel();
        //    info.ChatFrame = player.GetChatFrame();
        //    info.MainId = player.server.MainId;
        //    info.PopScore = player.PopScore;
        //    info.GuildId = player.GetFamilyId();
        //    info.GuildName = player.GetGuidName();
        //    info.GuildIcon = player.GetGuidIcon();
        //    return info;
        //}

        public static PLAYER_INFO GetChatPlayerInfo(MSG_ZR_CHAT info)
        {
            PLAYER_INFO pcInfo = new PLAYER_INFO();

            pcInfo.PcUid = info.SpeakerInfo.Uid;
            pcInfo.Name = info.SpeakerInfo.Name;
            pcInfo.Camp = info.SpeakerInfo.Camp;
            pcInfo.Level = info.SpeakerInfo.Level;
            pcInfo.FaceIcon = info.SpeakerInfo.FaceIcon;
            pcInfo.ShowFaceJpg = info.SpeakerInfo.ShowFaceJpg;
            pcInfo.FaceFrame = info.SpeakerInfo.FaceFrame;
            pcInfo.Sex = info.SpeakerInfo.Sex;
            pcInfo.Title = info.SpeakerInfo.Title;
            pcInfo.TeamId = info.SpeakerInfo.TeamId;
            pcInfo.HeroId = info.SpeakerInfo.HeroId;
            pcInfo.GodType = info.SpeakerInfo.GodType;

            return pcInfo;
        }
        #endregion

        /// <summary>
        /// 获取人物基本信息
        /// </summary>
        /// <returns></returns>
        public static MSG_GC_CHARACTER_INFO GetCharacterInfoMsg(PlayerChar player)
        {
            MSG_GC_CHARACTER_INFO pcInfo = new MSG_GC_CHARACTER_INFO();
            pcInfo.Uid = player.Uid;
            pcInfo.Name = player.Name;

            pcInfo.FaceIcon = player.Icon;
            pcInfo.ShowFaceJpg = player.ShowDIYIcon;

            pcInfo.Sex = player.Sex;
            pcInfo.Level = player.Level;
            pcInfo.MainId = player.MainId;
            pcInfo.MapId = player.CurrentMapId;
            pcInfo.Channel = player.CurrentChannel;
            pcInfo.InstanceId = player.InstanceId;
            pcInfo.PosX = player.Position.X;
            pcInfo.PosY = player.Position.Y;
            //pcInfo.PetId = player.PetId;
            //pcInfo.Model = player.BagManager.FashionBag.GetModel();
            //pcInfo.InitialModel = player.BagManager.FashionBag.GetInitalModel();
            pcInfo.TimeCreated = Timestamp.GetUnixTimeStampSeconds(player.TimeCreated);

            pcInfo.FamilyId = player.FamilyId;
            pcInfo.TaskId = player.MainTaskId;
            pcInfo.BranchTaskIds.AddRange(player.BranchTaskIds);
            pcInfo.MainLineId = player.MainLineId;
            pcInfo.Title = player.TitleMng.CurTitleId;
            //pcInfo.HistoryMaxLadderLevel = player.ladderHistoryMaxLevel;
            pcInfo.Speed = player.MoveHandler.MoveSpeed;
            pcInfo.Job = (int)player.Job;

            pcInfo.BagSpace = player.BagSpace;
            pcInfo.BattlePower = player.HeroMng.CalcBattlePower();
            //pcInfo.HP = player.Nature.PRO_HP;
            //pcInfo.MaxHP = player.Nature.PRO_MAX_HP;
            pcInfo.HP = player.GetHp().ToInt64TypeMsg();
            pcInfo.MaxHP = player.GetMaxHp().ToInt64TypeMsg();
            pcInfo.Camp = (int)player.Camp;
            //pcInfo.HuntingCount = player.HuntingCount;
            pcInfo.FollowerId = player.FollowerId;
            pcInfo.HeroId = player.HeroId;

            pcInfo.AwakenLevel = player.HeroMng?.GetPlayerHeroInfo()?.AwakenLevel??0;
            pcInfo.GuidedId = player.GuideId;
            pcInfo.ResonanceLevel = player.ResonanceLevel;
            pcInfo.GodType = player.GodType;
            return pcInfo;
        }

    }
}
