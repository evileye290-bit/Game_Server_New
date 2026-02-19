using CommonUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class SoulBoneSuit
    {
        private Dictionary<int, ulong> partAndGuid = new Dictionary<int, ulong>();

        private Dictionary<ulong, SoulBone> soulBones=new Dictionary<ulong, SoulBone>();

        public int EquipedCount { get; private set; }

        public bool Load(SoulBone bone)
        {
            if (partAndGuid.ContainsKey(bone.PartType) || soulBones.ContainsKey(bone.Uid))
            {
                return false;
            }
            partAndGuid.Add(bone.PartType, bone.Uid);
            soulBones.Add(bone.Uid, bone);
            EquipedCount++;
            return true;
        }

        public bool Contain(ulong uid)
        {
            return soulBones.ContainsKey(uid);
        }

        public bool ContainPart(int part)
        {
            return partAndGuid.ContainsKey(part);
        }

        public SoulBone Unload(int type)
        {
            ulong uid = 0;
            SoulBone bone = null;
            if(partAndGuid.TryGetValue(type,out uid) && soulBones.TryGetValue(uid,out bone))
            {
                partAndGuid.Remove(type);
                soulBones.Remove(uid);
                EquipedCount--;
                bone.EquipedHeroId = -1;
            }
            return bone;
        }

        /// <summary>
        /// 返回的是真实用于计算时的魂骨
        /// </summary>
        /// <returns></returns>
        public List<SoulBone> GetSuitAdditions()
        {
            List<SoulBone> bones = new List<SoulBone>();
            //bones = SoulBoneLibrary.GetEnhancedBones2(new List<SoulBone>(soulBones.Values)).Item3;
            foreach(var item in soulBones)
            {
                bones.Add(item.Value.Clone());
            }
            return bones;
        }

        public Dictionary<NatureType,int> GetSuitAddValues()
        {
            return SoulBoneLibrary.GetEnhancedBones4(new List<SoulBone>(soulBones.Values));
        }

        public void ChangeEquipHero(int heroId)
        {
            List<SoulBone> boneList = new List<SoulBone>();
            foreach (var soulBone in soulBones)
            {
                boneList.Add(soulBone.Value);
            }
            for (int i = 0; i < boneList.Count; i++)
            {
                boneList[i].EquipedHeroId = heroId;
            }
            soulBones.Clear();
            foreach (var item in boneList)
            {
                soulBones.Add(item.Uid, item);
            }
        }

        public List<SoulBone> GetSoulBones()
        {
            List<SoulBone> list = new List<SoulBone>();
            foreach (var kv in soulBones)
            {
                list.Add(kv.Value);
            }
            return list;
        }
    }
}
