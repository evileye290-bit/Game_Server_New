using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetRedioAllAnchorRank(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_RADIO_GET_ALL_ANCHOR_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RADIO_GET_ALL_ANCHOR_RANK>(stream);
            //PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            //if (player == null)
            //{
            //    Log.WarnLine("player {0} GetRedioAllAnchorRank not find pc", pks.PcUid);
            //    return;
            //}

            //player.GetRadioAnchorList();
        }

        public void OnResponse_GetRedioAnchorContributionRank(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_RADIO_GET_ANCHOR_CONTRIBUTION_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RADIO_GET_ANCHOR_CONTRIBUTION_RANK>(stream);
            //PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            //if (player == null)
            //{
            //    Log.WarnLine("player {0} GetRedioAnchorContributionRank not find pc", pks.PcUid);
            //    return;
            //}

            //player.GetRadioAnchorList(pks.AnchorId);
        }

        public void OnResponse_GetRedioAllContributionRank(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_RADIO_GET_ALL_CONTRIBUTION_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RADIO_GET_ALL_CONTRIBUTION_RANK>(stream);
            //PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            //if (player == null)
            //{
            //    Log.WarnLine("player {0} GetRedioAllContributionRank not find pc", pks.PcUid);
            //    return;
            //}

            //player.GetRadioAllContributionList();
        }

        public void OnResponse_GetRedioContributionReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RADIO_GET_CONTRIBUTION_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RADIO_GET_CONTRIBUTION_REWARD>(stream);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.WarnLine("player {0} GetRedioContributionReward not find pc", pks.PcUid);
                return;
            }
        }

        public void OnResponse_RedioGiveGift(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RADIO_GIVE_GIFT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RADIO_GIVE_GIFT>(stream);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.WarnLine("player {0} RedioGiveGift not find pc", pks.PcUid);
                return;
            }
        }
    }

}
