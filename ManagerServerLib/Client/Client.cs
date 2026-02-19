using System;

namespace ManagerServerLib
{
    public class Client
    {
        private int characterUid;
        public int CharacterUid
        { get { return characterUid; } }

        private string accountId;//这里为完整accountRealName 即带channelName的
        public string AccountId
        { get { return accountId; } }

        private ZoneServer zone;
        public ZoneServer Zone
        {
            get { return zone; }
            set {
                // Logger.Log.Warn("Client Uid {0} changed", characterUid);
                zone = value; 
            }
        }

        // 所在map
        private Map map;
        public Map CurrentMap
        { get { return map; } }

        private bool isTransforming = false;
        public bool IsTransforming
        {
            get { return isTransforming; }
            set { isTransforming = value; }
        }

        private ZoneServer destZone;
        public ZoneServer DestZone
        {
            get { return destZone; }
            set { destZone = value; }
        }

        private DateTime enterMapTime;
        public DateTime EnterMapTime
        {
            get { return enterMapTime; }
            set { enterMapTime = value; }
        }

        private DateTime leaveMapTime;
        public DateTime LeaveWorldTime
        {
            get { return leaveMapTime; }
            set { leaveMapTime = value; }
        }

        private bool enteredMap = false;
        public bool EnteredMap
        { get { return enteredMap; } }

        public string ChannelName { get; set; }
        public Client(int char_uid, ZoneServer zone,string accountId)
        {
            characterUid = char_uid;
            //this.camp = camp;
            this.Zone = zone;
            this.accountId = accountId;

            string[] arry = accountId.Split('$');
            if (arry.Length > 1)
            {
                ChannelName = arry[1];
            }
            if (string.IsNullOrWhiteSpace(ChannelName))
            {
                ChannelName = "default";
            }
        }

        public void EnterMap(Map map)
        {
            this.map = map;
            enteredMap = true;
            enterMapTime = ManagerServerApi.now;
        }

        public void LeaveMap()
        {
            this.map = null;
            enteredMap = false;
            leaveMapTime = ManagerServerApi.now;
        }

        public void EnterZone(ZoneServer zone)
        {
            this.Zone = zone;
        }

        public void LeaveZone()
        {
            this.Zone = null;
        }

        public void TransformingToZone(ZoneServer server)
        {
            destZone = server;
            isTransforming = true;
        }

        public void FinishTransforming()
        {
            destZone = null;
            isTransforming = false;
        }
    }
}
