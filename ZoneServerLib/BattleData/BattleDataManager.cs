using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class BattleDataManager
    {
        private Dictionary<int, Dictionary<int, BattleData>> battleDataList = new Dictionary<int, Dictionary<int, BattleData>>();

        public FieldMap Map { get; private set; }

        public BattleDataManager(FieldMap map)
        {
            Map = map;
        }

        public void Clear()
        {
            battleDataList.Clear();
        }

        public void AddBattleDataHurt(FieldObject caster, FieldObject target, BattleDataType type, long num)
        {
            AddBattleData(caster, BattleDataType.Hurt, num);
            AddBattleData(target, BattleDataType.Damage, num);
        }

        public void AddBattleDataCure(FieldObject caster, FieldObject target, BattleDataType type, long num)
        { 
            AddBattleData(caster, BattleDataType.Cure, num);
        }


        private void AddBattleData(FieldObject field, BattleDataType type, long num)
        {
            int id = GetFieldObjectId(field);
            int ownerInstanceId = GetFieldInstanceId(field) ;

            BattleData data = GetBattleDataWithNew(ownerInstanceId, id, field);
            switch (type)
            {
                case BattleDataType.Hurt:
                    data.AddHurt(num);
                    break;
                case BattleDataType.Damage:
                    data.AddDamage(num);
                    break;
                case BattleDataType.Cure:
                    data.AddCure(num);
                    break;
            }
        }

        private int GetFieldInstanceId(FieldObject field)
        {
            int id = 0;
            switch (field.FieldObjectType)
            {
                case TYPE.PC:
                    id = (field as PlayerChar).Uid;
                    break;
                case TYPE.HERO:
                    id = GetHeroUid(field as Hero);
                    break;
                case TYPE.ROBOT:
                    Robot robot = field as Robot;
                    id = robot.GetOwnerUid();
                    break;
                case TYPE.MONSTER:
                    id = (field as Monster).InstanceId;
                    break;
                case TYPE.PET:
                    id = GetPetUid(field as Pet);
                    break;
            }
            return id;
        }

        private int GetHeroUid(Hero hero)
        {
            int uid;
            if (hero.Owner is Robot)
            {
                Robot robot1 = hero.Owner as Robot;
                uid = robot1.GetOwnerUid();
            }
            else if (hero.Owner is PlayerChar)
            {
                uid = hero.Owner.Uid;
            }
            else
            {
                uid = hero.Uid;
            }
            return uid;
        }

        private int GetPetUid(Pet pet)
        {
            int uid = 0;
            if (pet.Owner is Robot)
            {
                Robot robot1 = pet.Owner as Robot;
                uid = robot1.GetOwnerUid();
            }
            if (pet.Owner is PlayerChar)
            {
                uid = pet.Owner.Uid;
            }
            return uid;
        }

        private int GetFieldObjectId(FieldObject field)
        {
            int id = 0;
            switch (field.FieldObjectType)
            {
                case TYPE.PC:
                    id = (field as PlayerChar).HeroId;
                    break;
                case TYPE.HERO:
                    Hero hero = (field as Hero);
                    id = hero.IsMonsterHero ? hero.HeroDataModel.MonsterHeroId : hero.HeroId;
                    break;
                case TYPE.ROBOT:
                    id = (field as Robot).HeroId;
                    break;
                case TYPE.MONSTER:
                    id = (field as Monster).Generator.Model.MonsterId;
                    break;
                case TYPE.PET:
                    Pet pet = field as Pet;
                    id = pet.PetId + pet.QueueNum;
                    break;
            }
            return id;
        }

        public BattleData GetBattleDataWithNew(int ownerInstanceId, int id, FieldObject field)
        {
            BattleData data = null;
            Dictionary<int, BattleData> datas;
            if (!battleDataList.TryGetValue(ownerInstanceId, out datas))
            {
                datas = new Dictionary<int, BattleData>();
                battleDataList.Add(ownerInstanceId, datas);
            }

            if (!datas.TryGetValue(id, out data))
            {
                data = new BattleData(id, field);
                datas.Add(id, data);
            }

            return data;
        }

        public MSG_ZGC_DUNGEON_BATTLE_DATA GenerateBattleDataMsg(int playerUid)
        {
            MSG_ZGC_DUNGEON_BATTLE_DATA msg = new MSG_ZGC_DUNGEON_BATTLE_DATA();
            msg.IsPvp = Map.PVPType == PvpType.Person;

            FieldObject hero = null;

            foreach (var kv in Map.HeroList)
            {
                int heroOwnerUid = GetHeroUid(kv.Value);
                if (heroOwnerUid == playerUid)
                {
                    hero = kv.Value;
                    break;
                }
            }

            foreach (var kv in battleDataList)
            {
                foreach (var item in kv.Value)
                {
                    if (hero?.IsAlly(item.Value.FieldObject) == true)
                    {
                        MSG_ZGC_BATTLE_DATA dataMsg = item.Value.GenerateMsg();
                        dataMsg.IsTeamMember = kv.Key != playerUid;
                        msg.DataList.Add(dataMsg);
                    }
                    else
                    { 
                        msg.DefDataList.Add(item.Value.GenerateMsg());
                    }
                }
            }

            return msg;
        }

        public BattleData GetBattleDataByInstanceIdAndId(int instanceId, int id)
        {
            Dictionary<int, BattleData> datas;
            battleDataList.TryGetValue(instanceId, out datas);
            BattleData data = null;
            if (datas != null)
            {
                datas.TryGetValue(id, out data);
            }
            return data;
        }
    }
}
