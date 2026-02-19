using System.IO;
using Logger;
using Message.BattleManager.Protocol.BMZ;
using EnumerateUtility;
using ServerFrame;
using Message.IdGenerator;

namespace ZoneServerLib
{
    public partial class BattleManagerServer : BackendServer
    {
        private ZoneServerApi Api
        { get { return (ZoneServerApi)api; } }

        public BattleManagerServer(BaseApi api)
            : base(api)
        {
        }


        protected override void BindResponser()
        {
            base.BindResponser();
            //battle
            AddResponser(Id<MSG_BMZ_ERROR_CODE>.Value, OnResponse_ErrorCode);
            //AddResponser(Id<MSG_BMZ_FIND_PLAYER_INFO>.Value, OnResponse_FindPlayerInfo);
            //AddResponser(Id<MSG_BMZ_TEAM_FIND_PLAYER_INFO>.Value, OnResponse_TeamFindPlayerInfo);
            //AddResponser(Id<MSG_BMZ_TEAM_ROOM>.Value, OnResponse_SendTeamRoomInfo);
            //AddResponser(Id<MSG_BMZ_LEAVE_MATCHING>.Value, OnResponse_LeaveMatching);
            //AddResponser(Id<MSG_BMZ_LEAVE_TEAM>.Value, OnResponse_LeaveTeam);
            //AddResponser(Id<MSG_BMZ_CHALLENGE_PLAYER_RESPONSE>.Value, OnResponse_ChallengePlayerResponse);
            //AddResponser(Id<MSG_BMZ_MATCHING_BATTLE_SERVER>.Value, OnResponse_MatchingBattleServer);
            //AddResponser(Id<MSG_BMZ_ENTER_GL_MAP>.Value, OnResponse_EnterGameLevelMap);
            //ResponserEnd
        }
  

        public void OnResponse_ErrorCode(MemoryStream stream, int uid = 0)
        {
            MSG_BMZ_ERROR_CODE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_BMZ_ERROR_CODE>(stream);
            int pcUid = pks.PcUid;
            int errorCode = pks.ErrorCode;

            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.SendErrorCodeMsg(errorCode);
                switch (errorCode)
                {
                    case (int)ErrorCode.NotFindBattleServer:
                    case (int)ErrorCode.CasterNotFind:
                    case (int)ErrorCode.TargetNotFind:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Log.WarnLine("player {0} error code not find player.", pcUid);
            }
        }
     
    }
}