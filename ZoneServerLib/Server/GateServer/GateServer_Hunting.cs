using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_HuntingInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_INFO>(stream);
            Log.Write("player {0} request HuntingInfo", msg.Uid);

            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.GetHuntingInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingInfo fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("HuntingInfo fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        public void OnResponse_HuntingSweep(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_SWEEP>(stream);
            Log.Write($"player {uid} request hunting sweep {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingSweep(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingSweep fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingSweep fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_ContinueHunting(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CONTINUE_HUNTING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CONTINUE_HUNTING>(stream);
            Log.Write("player {0} request continue hunting {1}", uid, msg.Continue.ToString());
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ContinueHunting(msg.Continue);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("ContinueHunting fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("ContinueHunting fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_HuntingActivityUnlock(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_ACTICITY_UNLOCK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_ACTICITY_UNLOCK>(stream);
            Log.Write("player {0} request unlock hunting activity {1}", uid, msg.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingActivityUnlock(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingActivityUnlock fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingActivityUnlock fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_HuntingActivitySweep(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_ACTICITY_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_ACTICITY_SWEEP>(stream);
            Log.Write("player {0} request hunting activity sweep {1}", uid, msg.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingActivitySweep(msg.Id, msg.Type);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingActivitySweep fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingActivitySweep fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_HuntingHelp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_HELP>(stream);
            Log.Write("player {0} HuntingHelp", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingHelp(msg.DungeonId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingHelp fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingHelp fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_HuntingHelpAnswer(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_HELP_ANSWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_HELP_ANSWER>(stream);
            Log.Write("player {0} request HuntingHelpAnswer", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingHelpAnswer(msg.AskHelpUid, msg.Agree);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingHelpAnswer fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingHelpAnswer fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_HuntingIntrudeUpdateHeroPos(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_INTRUDE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_INTRUDE_HERO_POS>(stream);
            Log.Write($"player {uid} request HuntingIntrudeUpdateHeroPos");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingIntrudeUpdateHeroPos(msg.HeroPos);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingIntrudeUpdateHeroPos fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingIntrudeUpdateHeroPos fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_HuntingIntrudeChallenge(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HUNTING_INTRUDE_CHALLENGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HUNTING_INTRUDE_CHALLENGE>(stream);
            Log.Write($"player {uid} request HuntingIntrudeChallenge {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HuntingIntrudeChallenge(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HuntingIntrudeChallenge fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HuntingIntrudeChallenge fail, can not find player {0} .", uid);
                }
            }
        }

    }
}
