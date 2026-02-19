using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class DisableRealBodyBuff : BaseBuff
    {
        public DisableRealBodyBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (owner != null && owner.InRealBody)
            {
                if (owner.IsPlayer)
                {
                    PlayerChar player = owner as PlayerChar;
                    player?.Write(RealbodyTimeMsg());
                }
                else if (owner.IsHero)
                {
                    Hero hero = owner as Hero;
                    if (hero.Owner != null)
                    {
                        if (hero.Owner.IsRobot)
                        {
                            Robot playerRobot = hero.Owner as Robot;
                            playerRobot.playerMirror?.Write(RealbodyTimeMsg());
                        }
                        else
                        {
                            PlayerChar player = hero.Owner as PlayerChar;
                            player?.Write(RealbodyTimeMsg());
                        }
                    }
                }
            }
        }

        private MSG_ZGC_REALBODY_TIME RealbodyTimeMsg()
        {
            MSG_ZGC_REALBODY_TIME notify = new MSG_ZGC_REALBODY_TIME();
            notify.DuringTime = s;
            notify.OriginShapeTime = c;
            notify.InstanceId = owner.InstanceId;
            return notify;
        }

        protected override void End()
        {
            owner.DisableRealBody();
        }
    }
}
