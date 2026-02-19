using System.IO;
using Message.IdGenerator;
using ServerShared;
using Logger;
using Message.Manager.Protocol.MR;
using Message.Global.Protocol.GM;
using Message.Relation.Protocol.RM;
using Message.Manager.Protocol.MG;
using ServerFrame;
using DBUtility;

namespace ManagerServerLib
{
    public class RelationServer : FrontendServer
    {
        private ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }

        private int sleepTime = 0;
        public int SleepTime
        { get { return sleepTime; } }

        private int frameCount = 0;
        public int FrameCount
        { get { return frameCount; } }

        private long memory = 0;
        public long Memory
        { get { return memory; } }


        public RelationServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_RM_DB_EXCEPTION>.Value, OnResponse_DbException);
            AddResponser(Id<MSG_RM_GM_FAMLIY_INFO>.Value, OnResponse_GMFamliyInfo);
            AddResponser(Id<MSG_RM_CHANGE_FAMLIY_NAME>.Value, OnResponse_ChangeFamilyName);
            AddResponser(Id<MSG_RM_CPU_INFO>.Value, OnResponse_CpuInfo);
            //ResponserEnd
        }

        public void OnResponse_DbException(MemoryStream stream, int uid = 0)
        {
            MSG_RM_DB_EXCEPTION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RM_DB_EXCEPTION>(stream);
            Api.GameDBPool.Call(new QueryAlarm((int)AlarmType.DB, MainId, 0, ManagerServerApi.now.ToString(), msg.Content));
        }

        public void OnResponse_GMFamliyInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RM_GM_FAMLIY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RM_GM_FAMLIY_INFO>(stream);
            MSG_MG_FAMILY_INFO response = new MSG_MG_FAMILY_INFO();
            response.CustomUid = msg.CustomUid;
            response.FamilyName = msg.FamilyName;
            response.Rank = msg.Rank;
            response.Level = msg.Level;
            response.Contribution = msg.Contribution;
            foreach (var item in msg.MemberList)
            {
                MSG_MG_FAMILY_INFO.Types.FAMILY_MEMBER memberInfo = new MSG_MG_FAMILY_INFO.Types.FAMILY_MEMBER();
                memberInfo.Uid = item.Uid;
                memberInfo.Name = item.Name;
                memberInfo.Title = item.Title;
                response.MemberList.Add(memberInfo);
            }
            foreach (var item in msg.DungeonList)
            {
                MSG_MG_FAMILY_INFO.Types.FAMILY_DUNGEON dungeonInfo = new MSG_MG_FAMILY_INFO.Types.FAMILY_DUNGEON();
                dungeonInfo.DungeonId = item.DungeonId;
                dungeonInfo.CurHp = item.CurHp;
                dungeonInfo.MaxHp = item.MaxHp;
                response.DungeonList.Add(dungeonInfo);
            }
            if (Api.GlobalServer != null)
            {
                Api.GlobalServer.Write(response);
            }
        }

        public void OnResponse_ChangeFamilyName(MemoryStream stream, int uid = 0)
        {
            MSG_RM_CHANGE_FAMLIY_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RM_CHANGE_FAMLIY_NAME>(stream);
            MSG_MG_CHANGE_FAMILY_NAME response = new MSG_MG_CHANGE_FAMILY_NAME();
            response.CustomUid = msg.CustomUid;
            response.Result = msg.Result;
            response.OldFamilyName = msg.OldFamilyName;
            response.NewFamilyName = msg.NewFamilyName;
            if (Api.GlobalServer != null)
            {
                Api.GlobalServer.Write(response);
            }
        }

        public void OnResponse_CpuInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RM_CPU_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RM_CPU_INFO>(stream);
            sleepTime = msg.SleepTime;
            frameCount = msg.FrameCount;
            memory = msg.Memory;
            //Log.WriteLine("mainId {0} subId {1} frame count {2} sleep time {3} memory {4}MB", mainId, subId, frameCount, sleepTime, memory);
        }

    }
}
