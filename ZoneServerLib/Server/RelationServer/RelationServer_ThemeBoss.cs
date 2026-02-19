using Logger;
using Message.Relation.Protocol.RZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_OpenNewThemeBoss(MemoryStream stream, int uid = 0)
        {                
            //MSG_RZ_NEW_THEMEBOSS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NEW_THEMEBOSS>(stream);
            //给所有人发
            //foreach (var player in Api.PCManager.PcList)
            //{
            //    player.Value.CheckOpenNewThemeBoss();
            //}
            //foreach (var player in Api.PCManager.PcOfflineList)
            //{
            //    player.Value.CheckOpenNewThemeBoss();
            //}
        }
    }
}
