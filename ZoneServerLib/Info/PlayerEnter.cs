using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProperty;
using ServerShared;
using Message.Manager.Protocol.MZ;
using Logger;
using Message.Zone.Protocol.ZM;
using ServerModels;

namespace ZoneServerLib
{
    // 经过Manager分配，等待Enter World的client
    public class PlayerEnter
    {
        private int uid;
        public int Uid
        { get { return uid; } }

        private DateTime readyTime;
        public DateTime ReadyTime
        { get { return readyTime; } }

        private ZoneServerApi server;
        public ZoneServerApi Server
        { get { return server; } }

        private int originSubId;
        public int OriginSubId
        { get { return originSubId; } }

        private EnterMapInfo originMapInfo = new EnterMapInfo();
        public EnterMapInfo OriginMapInfo
        { get { return originMapInfo; } }

        private EnterMapInfo destMapInfo = new EnterMapInfo();
        public EnterMapInfo DestMapInfo
        { get { return destMapInfo; } }
        
        private bool transformDone;
        public bool TransformDone
        {
            get { return transformDone; }
            set { transformDone = value; }
        }

        private PlayerChar player;
        public PlayerChar Player
        { get { return player; } }

        public PlayerEnter(ZoneServerApi server,int uid, int originSubId, EnterMapInfo origin, EnterMapInfo dest)
        {
            this.server = server;
            this.uid = uid;
            this.originSubId = originSubId;
            originMapInfo = origin;
            destMapInfo = dest;
            readyTime = ZoneServerApi.now;
            transformDone = false;
            player = new PlayerChar(server, uid);
            player.OriginMapInfo = origin;
        }

        //// 账号名
        //public string AccountName { get; set; }
        //// 创建账号时间戳
        //public UInt64 CreateTimestamp { get; set; }

        //public void InitLoginInfo(MSG_MZ_CLIENT_ENTER msg)
        //{
        //    AccountName = msg.AccountName;
        //    CreateTimestamp = msg.createTimestamp;
        //    ChannelName = msg.ChannelName;
        //}

        public void SendNeedTransformDataTag(ServerShared.TransformStep step)
        {
            if (player == null)
            {
                return;
            }
            MSG_ZM_NEED_TRANSFORM_DATA_TAG msg = new MSG_ZM_NEED_TRANSFORM_DATA_TAG();
            msg.CharacterUid = player.Uid;
            msg.Tag = (int)step;
            msg.OringinSubId = originSubId;
            if (step == TransformStep.DONE)
            {
                TransformDone = true;
            }
            server.ManagerServer.Write(msg, msg.CharacterUid);
        }
    }
}
