//using Google.Protobuf.Collections;
//using Message.Relation.Protocol.RZ;
//using Message.Zone.Protocol.ZR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RelationServerLib
//{
//    public class Monster
//    {
//        public int MonsterId;

//        public Monster(int monsterId, long hp, long maxHp)
//        {
//            MonsterId = monsterId;
//            Hp = hp;
//            MaxHp = maxHp;
//        }

//        internal void DelHp(long monsterDamage)
//        {
//            if (Hp>monsterDamage)
//            {
//                Hp -= monsterDamage;
//            }
//            else
//            {
//                Hp = 0;
//            }
//        }

//        internal void Clear()
//        {
//            Hp = 0;
//        }
//    }
//}
