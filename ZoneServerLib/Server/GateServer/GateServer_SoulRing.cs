using Logger;
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

        private void OnResponse_AbsorbSoulRing(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_ABSORB_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ABSORB_SOULRING>(stream);
            Log.Write("player {0} absorb soulring to hero {1} uid {2} slot {3}", uid, msg.HeroId, msg.SoulRingUid, msg.Slot);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.AbsorbSoulRing(msg.HeroId, msg.SoulRingUid, msg.Slot);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} absorb soulring {1} to hero {2} fail : player is offline.", uid, msg.SoulRingUid,msg.HeroId);
                }
                else
                {
                    Log.WarnLine("player {0} absorb soulbone {1} to hero {2} fail, can not find player.", uid, msg.SoulRingUid, msg.HeroId);
                }
            }
        }


        private void OnResponse_HelpAbsorbSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HELP_ABSORB_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HELP_ABSORB_SOULRING>(stream);
            Log.Write("player {0} help hero {1} absorb soulring", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HelpAbsorbSoulRing(msg.HeroId, msg.PcUids.ToList());
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} help hero {1} absorb soulring fail : player is offline.", uid, msg.HeroId);
                }
                else
                {
                    Log.WarnLine("player {0} help hero {1} absorb soulring fail, can not find player.", uid, msg.HeroId);
                }
            }
        }


        private void OnResponse_GetAbsorbInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_ABSORBINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_ABSORBINFO>(stream);
            Log.Write("player {0} get hero {1} soulring absorb info", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetSoulRingAbsorbInfo(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} get hero {1}  soulring absorb info fail : player is offline.", uid, msg.HeroId);
                }
                else
                {
                    Log.WarnLine("player {0} get hero {1}  soulring absorb info fail, can not find player.", uid, msg.HeroId);
                }
            }
        }

        private void OnResponse_CancelAbsorb(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CANCEL_ABSORB msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CANCEL_ABSORB>(stream);
            Log.Write("player {0} cancel hero {1}  soulring absorb", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CancelSoulRingAbsorb(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} cancel hero {1}  soulring absorb fail : player is offline.", uid, msg.HeroId);
                }
                else
                {
                    Log.WarnLine("player {0} cancel hero {1}  soulring absorb fail, can not find player.", uid, msg.HeroId);
                }
            }
        }

        private void OnResponse_FinishAbsorb(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ABSORB_FINISH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ABSORB_FINISH>(stream);
            Log.Write("player {0} finish hero {1} soulring absorb", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SoulRingAbsorbFinish(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} finish hero {1}  soulring absorb fail : player is offline.", uid, msg.HeroId);
                }
                else
                {
                    Log.WarnLine("player {0} finish hero {1}  soulring absorb fail, can not find player.", uid, msg.HeroId);
                }
            }
        }

        private void OnResponse_GetHelpThanksList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_HELP_THANKS_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HELP_THANKS_LIST>(stream);
            Log.Write("player {0} get soulring absorb help thank list", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetHelpThanksList(msg.Uids);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} get soulring absorb help thank list fail : player is offline.", uid);
                }
                else
                {
                    Log.WarnLine("player {0} get soulring absorb help thank list fail, can not find player.", uid);
                }
            }
        }


        private void OnResponse_ThankFriend(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_THANK_FRIEND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_THANK_FRIEND>(stream);
            Log.Write("player {0} thank friend {1} soulring absorb item {2}", uid, msg.FriendUid, msg.ItemUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ThankFriend(msg.FriendUid,msg.ItemUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} thank friend {1} soulring absorb fail : player is offline.", uid, msg.FriendUid);
                }
                else
                {
                    Log.WarnLine("player {0} thank friend {1} soulring absorb fail, can not find player.", uid, msg.FriendUid);
                }
            }
        }


        private void OnResponse_EnhanceSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENHANCE_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENHANCE_SOULRING>(stream);
            Log.Write("player {0} enhance hero {1} soulring {2}", uid, msg.HeroId, msg.SoulRingUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SoulRingEnhance(msg.HeroId, msg.SoulRingUid, msg.Type);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} enhance hero {1}  soulring {2} fail : player is offline.", uid, msg.HeroId,msg.SoulRingUid);
                }
                else
                {
                    Log.WarnLine("player {0} enhance hero {1}  soulring {2} fail, can not find player.", uid, msg.HeroId,msg.SoulRingUid);
                }
            }
        }

        private void OnResponse_OneKeyEnhanceSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ONEKEY_ENHANCE_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ONEKEY_ENHANCE_SOULRING>(stream);
            Log.Write("player {0} oneKey enhance hero {1} soulring {2}", uid, msg.HeroId, msg.SoulRingUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                //player.SoulRingOneKeyEnhance(msg.HeroId, msg.SoulRingUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} enhance hero {1}  soulring {2} fail : player is offline.", uid, msg.HeroId, msg.SoulRingUid);
                }
                else
                {
                    Log.WarnLine("player {0} enhance hero {1}  soulring {2} fail, can not find player.", uid, msg.HeroId, msg.SoulRingUid);
                }
            }
        }


        private void OnResponse_GetAllAbsorbInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_All_ABSORBINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_All_ABSORBINFO>(stream);
            Log.Write("player {0} get all soulring absorb infos", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetAllAbsorbInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine($"player {uid} get all soulring absorb infos fail : player is offline.");
                }
                else
                {
                    Log.WarnLine($"player {uid} get all soulring absorb infos fail, can not find player.");
                }
            }
        }

        private void OnResponse_GetAbsorbFriendInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_FRIEND_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_FRIEND_INFO>(stream);
            Log.Write("player {0} get hero {1} soulring absorb helpers info", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetAbsorbFriendInfo(msg.FriendUids,msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine($"player {uid} get soulring absorb helpers info not find client fail : player is offline.");
                }
                else
                {
                    Log.WarnLine($"player {uid} get soulring absorb helpers info not find client fail, can not find player.");
                }
            }
        }

        private void OnResponse_ShowHeroSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_HERO_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_HERO_SOULRING>(stream);
            //Log.Write("player {0} show hero soulring", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ShowHeroSoulRing();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine($"player {uid} show hero soulring not find client fail : player is offline.");
                }
                else
                {
                    Log.WarnLine($"player {uid} show hero soulring not find client fail, can not find player.");
                }
            }
        }

        private void OnResponse_ReplaceBetterSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_REPLACE_BETTER_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_REPLACE_BETTER_SOULRING>(stream);
            Log.Write("player {0} replace better soulring to hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ReplaceAllBetterSoulRings(msg.HeroId, msg.SoulRings);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine($"player {uid} replace better soulring not find client fail : player is offline.");
                }
                else
                {
                    Log.WarnLine($"player {uid} replace better soulring not find client fail, can not find player.");
                }
            }
        }
        
        private void OnResponse_SelectSoulRingElement(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SELECT_SOULRING_ELEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SELECT_SOULRING_ELEMENT>(stream);
            Log.Write("player {0} SelectSoulRingElement to hero {1} element Id {2}", uid, msg.HeroId, msg.ElementId);
            
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SelectSoulRingElement(msg.HeroId, msg.Pos, msg.ElementId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine($"player {uid} SelectSoulRingElement not find client fail : player is offline.");
                }
                else
                {
                    Log.WarnLine($"player {uid} SelectSoulRingElement not find client fail, can not find player.");
                }
            }
        }
    }
}
