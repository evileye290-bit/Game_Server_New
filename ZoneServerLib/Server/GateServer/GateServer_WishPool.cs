using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_GetWishPoolInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_WISHPOOL_UPDATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_WISHPOOL_UPDATE>(stream);
            Logger.Log.Write("player {0} request get wish pool info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                if (player.CheckWishPoolStageUpdate())
                {
                    player.UpdateWishPool2DB();
                }
                player.SendWishPoolInfo();
            }
            else
            {
                Logger.Log.Warn($"OnResponse_GetWishPoolInfo find no player {uid}");
            }
        }

        private void OnResponse_UseWishPool(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_USINIG_WISHPOOL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_USINIG_WISHPOOL>(stream);
            Logger.Log.Write("player {0} request use wish pool", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UsingWishPool();
            }
            else
            {
                Logger.Log.Warn($"OnResponse_UseWishPool find no player {uid}");
            }
        }
    }
}
