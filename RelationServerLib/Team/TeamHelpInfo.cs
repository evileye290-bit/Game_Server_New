using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class TeamHelpInfo
    {
        public int TeamId { get; set; }
        public DateTime FirstSendTime { get; set; }//首次发送时间

        public List<DateTime> SendTimes { get; set; }
        public MSG_ZR_NEED_TEAM_HELP HelpSenderInfo { get; set; }//发送者信息
    }
}
