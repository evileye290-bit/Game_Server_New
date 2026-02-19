using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {

        public void OnResponse_HeroLevelUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_LEVEL_UP>(stream);
            MSG_GateZ_HERO_LEVEL_UP request = new MSG_GateZ_HERO_LEVEL_UP();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }
        public void OnResponse_HeroAwaken(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_AWAKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_AWAKEN>(stream);
            MSG_GateZ_HERO_AWAKEN request = new MSG_GateZ_HERO_AWAKEN();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_HeroTitleUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_TITLE_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_TITLE_UP>(stream);
            MSG_GateZ_HERO_TITLE_UP request = new MSG_GateZ_HERO_TITLE_UP();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }
        
        public void OnResponse_HeroClickTalent(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_CLICK_TALENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_CLICK_TALENT>(stream);
            MSG_GateZ_HERO_CLICK_TALENT request = new MSG_GateZ_HERO_CLICK_TALENT();
            request.HeroId = msg.HeroId;
            request.Strength = msg.Strength;
            request.Physical = msg.Physical;
            request.Agility = msg.Agility;
            request.Outburst = msg.Outburst;
            WriteToZone(request);
        }

        public void OnResponse_HeroResetTalent(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_RESET_TALENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_RESET_TALENT>(stream);
            MSG_GateZ_HERO_RESET_TALENT request = new MSG_GateZ_HERO_RESET_TALENT();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_CallHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CALL_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CALL_HERO>(stream);
            MSG_GateZ_CALL_HERO request = new MSG_GateZ_CALL_HERO();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_RecallHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RECALL_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RECALL_HERO>(stream);
            MSG_GateZ_RECALL_HERO request = new MSG_GateZ_RECALL_HERO();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_ChangeFollower(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_CHANGE_FOLLOWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_CHANGE_FOLLOWER>(stream);
            MSG_GateZ_HERO_CHANGE_FOLLOWER request = new MSG_GateZ_HERO_CHANGE_FOLLOWER();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_ChangeMainHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_MAIN_HERO_CHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_MAIN_HERO_CHANGE>(stream);
            MSG_GateZ_MAIN_HERO_CHANGE request = new MSG_GateZ_MAIN_HERO_CHANGE();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_EquipHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_EQUIP_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIP_HERO>(stream);
            MSG_GateZ_EQUIP_HERO request = new MSG_GateZ_EQUIP_HERO();
            request.HeroId = msg.HeroId;
            request.Equip = msg.Equip;
            WriteToZone(request);
        }

        public void OnResponse_HeroStepsUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_STEPS_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_STEPS_UP>(stream);
            MSG_GateZ_HERO_STEPS_UP request = new MSG_GateZ_HERO_STEPS_UP();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_OnekeyHeroStepsUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ONEKEY_HERO_STEPS_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ONEKEY_HERO_STEPS_UP>(stream);
            MSG_GateZ_ONEKEY_HERO_STEPS_UP request = new MSG_GateZ_ONEKEY_HERO_STEPS_UP();
            request.HeroIds.AddRange(msg.HeroIds);
            WriteToZone(request);
        }

        public void OnResponse_HeroRevert(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_REVERT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_REVERT>(stream);
            MSG_GateZ_HERO_REVERT request = new MSG_GateZ_HERO_REVERT();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_UpdateHeroPos(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_HERO_POS>(stream);
            MSG_GateZ_UPDATE_HERO_POS request = new MSG_GateZ_UPDATE_HERO_POS();
            foreach(var temp in msg.HeroPos)
            {
                request.HeroPos.Add(new MSG_GateZ_HERO_POS()
                {
                    HeroId=temp.HeroId,
                    Delete=temp.Delete,
                    PosId=temp.PosId
                });
            }
            WriteToZone(request);
            
        }

        public void OnResponse_UpdateMainQueueHeroPos(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_MAINQUEUE_HEROPOS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_MAINQUEUE_HEROPOS>(stream);
            MSG_GateZ_UPDATE_MAINQUEUE_HEROPOS request = new MSG_GateZ_UPDATE_MAINQUEUE_HEROPOS();
            request.QueueNum = msg.QueueNum;
            foreach (var temp in msg.HeroPos)
            {
                request.HeroPos.Add(new GateZ_MAINQUEUE_HEROPOS()
                {
                    HeroId = temp.HeroId,
                    PosId = temp.PosId
                });
            }
            WriteToZone(request);
        }

        public void OnResponse_UnlockMainBattleQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UNLOCK_MAINQUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UNLOCK_MAINQUEUE>(stream);
            MSG_GateZ_UNLOCK_MAINQUEUE request = new MSG_GateZ_UNLOCK_MAINQUEUE();
            request.QueueNum = msg.QueueNum;
            WriteToZone(request);
        }

        public void OnResponse_ChangeMainQueueName(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHANGE_MAINQUEUE_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHANGE_MAINQUEUE_NAME>(stream);         
            MSG_GateZ_CHANGE_MAINQUEUE_NAME request = new MSG_GateZ_CHANGE_MAINQUEUE_NAME();
            request.QueueNum = msg.QueueNum;
            request.Name = msg.Name;
            WriteToZone(request);
        }

        public void OnResponse_MainQueueDispatchBattle(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_MAINQUEUE_DISPATCH_BATTLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_MAINQUEUE_DISPATCH_BATTLE>(stream);
            MSG_GateZ_MAINQUEUE_DISPATCH_BATTLE request = new MSG_GateZ_MAINQUEUE_DISPATCH_BATTLE();
            request.QueueNum = msg.QueueNum;          
            WriteToZone(request);
        }

        private void OnResponse_HeroInherit(MemoryStream stream)
        {
            MSG_CG_HERO_INHERIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_INHERIT>(stream);
            MSG_GateZ_HERO_INHERIT request = new MSG_GateZ_HERO_INHERIT();
            request.FromHeroId = msg.FromHeroId;
            request.ToHeroId = msg.ToHeroId;
            WriteToZone(request);
        }

        public void OnResponse_HeroGodStepsUp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_HERO_GOD_STEPS_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HERO_GOD_STEPS_UP>(stream);
            MSG_GateZ_HERO_GOD_STEPS_UP request = new MSG_GateZ_HERO_GOD_STEPS_UP();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }
    }
}
