using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public enum BattleDataType
    {
        /// <summary>
        /// 造成伤害
        /// </summary>
        Hurt = 1,
        /// <summary>
        /// 承受伤害
        /// </summary>
        Damage = 2,
        /// <summary>
        /// 治疗
        /// </summary>
        Cure = 3,
    }


    public class BattleData
    {
        public int Id { get; private set; }
        public long Hurt { get; private set; }
        public long Damage { get; private set; }
        public long Cure { get; private set; }

        public FieldObject FieldObject { get; private set; }

        public BattleData(int id, FieldObject field)
        {
            Id = id;
            FieldObject = field;
        }

        public bool IsAlly(FieldObject fieldObject)
        {
            return fieldObject.IsAlly(FieldObject);
        }

        public void AddHurt(long num)
        {
            Hurt += num;
        }

        public void AddDamage(long num)
        {
            Damage += num;
        }

        public void AddCure(long num)
        {
            Cure += num;
        }

        public MSG_ZGC_BATTLE_DATA GenerateMsg()
        {
            MSG_ZGC_BATTLE_DATA msg = new MSG_ZGC_BATTLE_DATA
            {
                Id = Id,
                Hurt = Hurt.ToString(),
                Damage = Damage.ToString(),
                Cure = Cure.ToString()
            };

            if (FieldObject is Hero)
            {
                Hero hero = (FieldObject as Hero);
                msg.GodType = hero.HeroInfo.GodType;
                msg.IsMonsterHero = hero.IsMonsterHero;
            }
            else if (FieldObject is Pet)
            {
                msg.IsPet = true;
                Pet pet = FieldObject as Pet;
                msg.Id = Id - pet.QueueNum;
            }

            return msg;
        }
    }
}
