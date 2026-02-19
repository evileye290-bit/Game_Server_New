using System.Collections.Generic;
using CommonUtility;
using DataProperty;
using Message.Manager.Protocol.MZ;
using System;
using System.Linq;
using Logger;
using EpPathFinding;
using System.IO;
using Message.Relation.Protocol.RZ;
using DBUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using Message.Zone.Protocol.ZM;
using ServerShared.Map;

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap
    {
        private Dictionary<int, Pet> petList = new Dictionary<int, Pet>();
        /// <summary>
        /// 宠物
        /// </summary>
        public IReadOnlyDictionary<int, Pet> PetList
        {
            get { return petList; }
        }

        private List<int> petRemoveList = new List<int>();

        private void UpdatePet(float dt)
        {
            //if (!CheckElapsedUpdateTickTime(dt))
            //{
            //    return;
            //}
            foreach (var pet in petList)
            {
                try
                {
                    pet.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        ///// <summary>
        ///// 控制update帧率时间
        ///// </summary>
        //private double elapsedUpdateTickTime = 0.1;
        //private double updatedTime = 0.0;
        //private bool CheckElapsedUpdateTickTime(double deltaTime)
        //{
        //    updatedTime += (float)deltaTime;
        //    if (updatedTime < elapsedUpdateTickTime)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        updatedTime = 0;
        //        return true;
        //    }
        //}

        private void RemovePet()
        {
            if (petRemoveList.Count > 0)
            {
                foreach (var instanceId in petRemoveList)
                {
                    try
                    {

                        Pet pet = GetPet(instanceId);
                        if (pet != null)
                        {
                            pet.RemoveFromAoi();
                            pet.SetInstanceId(0);//先通知离开然后在设为0
                            pet.SetCurrentMap(null);
                            petList.Remove(instanceId);
                        }
                        RemoveObjectSimpleInfo(instanceId);

                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                petRemoveList.Clear();
            }
        }

        #region 添加
        public void CreatePet(Pet pet)
        {
            if (pet == null) return;

            // 加到地图里
            AddPet(pet);
            if (IsDungeon)
            {
                Vec2 tempPosition = GetPetPostion(pet);
                pet.SetPosition(tempPosition ?? pet.Position);
                pet.InitBaseBattleInfo();
            }
            pet.AddToAoi();
            if (IsDungeon)
            {
                pet.BroadCastHp();
            }
        }

        public void AddPet(Pet pet)
        {
            pet.SetCurrentMap(this);
            pet.SetInstanceId(TokenId);
            petList.Add(pet.InstanceId, pet);
            AddObjectSimpleInfo(pet.InstanceId, TYPE.PET);
        }

        #endregion

        #region 删除

        public void RemovePet(int instance_id)
        {
            petRemoveList.Add(instance_id);
        }

        protected virtual Vec2 GetPetPostion(Pet pet)
        {
            DungeonMap map = this as DungeonMap;
            DungeonModel model = map.DungeonModel;

            int PosIndex = 0;
            if (pet.IsAttacker)
            {
                PosIndex = map.AttackerPosIndex;
            }
            else
            {
                PosIndex = map.DefenderPosIndex;
            }

            Vec2 tempPosition;

            //设置位置，一定在aoi前
            if (pet.OwnerIsRobot)
            {
                //Robot ow = pet.Owner as Robot;
                tempPosition = PetLibrary.PetConfig.GetPetPosition();
            }
            else
            {
                PlayerChar owner = pet.Owner as PlayerChar;

                tempPosition = PetLibrary.PetConfig.GetPetPosition();

                if (map.PetList.Count >= owner.PetManager.GetCallPetCount(model.Id))
                {
                    map.OnePlayerPetDone = true;//此时至少有一个玩家连同其pet加载完了
                }
            }

            if (tempPosition != null)
            {
                tempPosition = model.GetPosition4Count(PosIndex, tempPosition);
            }
            return tempPosition;
        }
        #endregion
    }
}