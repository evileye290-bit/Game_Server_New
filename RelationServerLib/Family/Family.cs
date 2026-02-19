using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using ServerShared;
using CommonUtility;
using Message.Relation.Protocol.RZ;
using DataProperty;
using EnumerateUtility;
using DataUtility;

namespace RelationServerLib
{
    public class Family
    {
        public CampType Camp;
        public int Uid; // 家族id
        public string Name; // 家族名
        public int Contribution; // 家族贡献
        public int WeekContribution; // 家族周贡献
        public Client Chief; //族长
        public Dictionary<int, Client> MemberList = new Dictionary<int,Client>(); // 成员列表 包含族长
        public List<Client> MemberSortedList = new List<Client>();
        public string Declaration; // 宣言
        public string Notice; // 公告
        public int Level = 0;
        public Dictionary<int, DateTime> ApplyList = new Dictionary<int, DateTime>();
        public int Rank = 0; // 排名
        
        public List<int> FamilyWarMembers = new List<int>();
        public List<int> FamilyWarSorts = new List<int>();

        //家族副本家族累积伤害排行榜
        private Dictionary<int, List<FamilyAccumulateDamageModel>> accumulateDamageList = new Dictionary<int, List<FamilyAccumulateDamageModel>>();

        public Dictionary<int, List<FamilyAccumulateDamageModel>> AccumulateDamageList
        {
            get { return accumulateDamageList; }
            set { accumulateDamageList = value; }
        }

        private Dictionary<int, int[]> bossStates = new Dictionary<int, int[]>();

        public Dictionary<int, int[]> BossStates
        {
            get { return bossStates; }
            set { bossStates = value; }
        }


        private RelationServerApi server;
        public Family(RelationServerApi server, int id, string name, int contribution, int weekContribution, Client chief, string declaration, string notice)
        {
            this.server = server;
            Uid = id;
            Name = name;
            if (chief != null)
            {
                Chief = chief;
            }
            Declaration = declaration;
            Notice = notice;
            Contribution = contribution;
            WeekContribution = weekContribution;
            CalcLevel();
        }

        private void CalcLevel()
        { 
            Level = Calc.GetFamilyLevel(Contribution);
        }

        public void AddContribution(int value)
        {
            //增加周贡献

            Contribution += value;
            int newLevel = Calc.GetFamilyLevel(Contribution);
            if (newLevel > Level)
            {
                // 升级 
                Level = newLevel;
                foreach (var member in MemberList)
                {
                    if (member.Value.IsOnline && member.Value.CurZone != null)
                    {
                        MSG_RZ_FAMILY_LEVELUP notify = new MSG_RZ_FAMILY_LEVELUP();
                        notify.Uid = member.Value.Uid;
                        notify.Fid = Uid;
                        notify.FamilyLevel = Level;
                        member.Value.CurZone.Write(notify);
                    }
                }
            }
            //server.DB.Call(new QueryUpdateFamilyContribution(Uid, Contribution, WeekContribution));
        }


        public void SetChief(Client chief)
        {
            Chief = chief;
        }

        public void AddMember(Client client)
        {
            if (client == null) return;
            try
            {
                MemberList.Add(client.Uid, client);
                MemberSortedList.Add(client);
            }
            catch (Exception e)
            {
                Log.Alert("family {0} add member failed: {1}", Uid, e.ToString());
            }
        }

        public void RemoveMember(Client member)
        {
            MemberList.Remove(member.Uid);
            MemberSortedList.Remove(member);
            FamilyWarMembers.Remove(member.Uid);
            //server.DB.Call(new QueryUpdateFamilyWarMembers(FamilyWarMembers, Uid));
            //server.DB.Call(new QueryRemoveFamilyMember(member.Uid));
        }

        //public void GetFamilySimpleInfo(PKS_ZC_FAMILY_SIMPLE_INFO info, int uid)
        public void GetFamilySimpleInfo(object info, int uid)
        {
            if (info == null) return;
            //info.fid = Uid;
            //info.familyName = Name;
            //if(Chief != null)
            //{
            //    info.chiefName = Chief.Name;
            //    info.chiefJob = Chief.JobId;
            //    info.chiefUid = Chief.Uid;
            //}
            //info.rank = Rank;
            //info.mamberCount = MemberList.Count;
            //info.level = Level;
            //info.declaration = Declaration;
            //info.contribution = Contribution;
            //info.weekContribution = WeekContribution;
            //info.applied = ApplyList.ContainsKey(uid);
        }

        //public void GetFamilyDetailInfo(int uid, int page, PKS_ZC_FAMILY_DETAIL_INFO msg, Client me)
        public void GetFamilyDetailInfo(int uid, int page, object msg, Client me)
        {
            //msg.Uid = uid;
            //msg.Fid = this.Uid;
            //msg.familyLevel = Level;
            //msg.memberCnt = MemberList.Count;
            //if (Chief != null)
            //{
            //    msg.chiefName = Chief.Name;
            //}
            //msg.contribution = Contribution;
            //msg.declaration = Declaration;
            //msg.notice = Notice;
            //msg.FamilyName = Name;

            //// 获取在线成员
            //List<Client> onlineList = new List<Client>();
            //List<Client> offlineList = new List<Client>();
            //foreach (var member in MemberList)
            //{
            //    if (member.Value.IsOnline)
            //    {
            //        onlineList.Add(member.Value);
            //    }
            //    else
            //    {
            //        offlineList.Add(member.Value);
            //    }
            //}
            //// 在线成员 先排title 后排contributed
            //onlineList.Sort((left, right) =>
            //{
            //    if (left.FamilyTitle < right.FamilyTitle)
            //    {
            //        return -1;
            //    }
            //    else if (left.FamilyTitle > right.FamilyTitle)
            //    {
            //        return 1;
            //    }
            //    else
            //    {
            //        // 职位相等 判断贡献
            //        if (left.FamilyContributed > right.FamilyContributed)
            //        {
            //            return -1;
            //        }
            //        else if (left.FamilyContributed < right.FamilyContributed)
            //        {
            //            return 1;
            //        }
            //        else
            //        {
            //            if (left.Uid < right.Uid)
            //            {
            //                return -1;
            //            }
            //            else
            //            {
            //                return 1;
            //            }
            //        }
            //    }
            //});
            //// 离线玩家 只排contributed
            //offlineList.Sort((left, right) =>
            //{
            //    if (left.FamilyContributed > right.FamilyContributed)
            //    {
            //        return -1;
            //    }
            //    else if (left.FamilyContributed < right.FamilyContributed)
            //    {
            //        return 1;
            //    }
            //    else
            //    {
            //        if (left.Uid < right.Uid)
            //        {
            //            return -1;
            //        }
            //        else
            //        {
            //            return 1;
            //        }
            //    }
            //});
            //msg.onlineCnt = onlineList.Count;

            //// 合并成一个list 
            //onlineList.AddRange(offlineList);
            //// 重命名 去除语义歧义
            //List<Client> list = onlineList;
            //int index = page * CONST.FAMILY_MEMBER_COUNT_PER_PAGE;
            //if(index >= list.Count)
            //{
            //    return;
            //}
            //int endIndex = Math.Min(list.Count, index + CONST.FAMILY_MEMBER_COUNT_PER_PAGE);

            //bool includeMe = false;
            //for (int i = index; i < endIndex; i++)
            //{
            //    PKS_ZC_FAMILY_MEMBER_INFO info = new PKS_ZC_FAMILY_MEMBER_INFO();
            //    list[i].GetFamilyMemberInfo(info);
            //    msg.memberList.Add(info);
            //    if (info.uid == me.Uid)
            //    {
            //        includeMe = true;
            //    }
            //}
            //if (includeMe == false)
            //{
            //    PKS_ZC_FAMILY_MEMBER_INFO info = new PKS_ZC_FAMILY_MEMBER_INFO();
            //    me.GetFamilyMemberInfo(info);
            //    msg.memberList.Add(info);
            //}
        }


        public ErrorCode CanJoinFamily()
        {
            Data limitData = DataListManager.inst.GetData("FamilyLevel", Level);
            if (limitData == null)
            {
                Log.Warn("family {0} level {1} get level data failed", Uid, Level);
                //return ErrorCode.FamilyMemberCount;
            }
            if (MemberList.Count >= limitData.GetInt("MemberCount"))
            {
                //return ErrorCode.FamilyMemberCount;
            }
            return ErrorCode.Success;
        }

        public void AddNewApplicant(int uid)
        {
            ApplyList.Add(uid, RelationServerApi.now);
            // 通知族长 副族长红点
            //foreach (var member in MemberList)
            //{
            //    if (member.Value.FamilyTitle <= FamilyTitle.ViceChief && member.Value.IsOnline == true && member.Value.CurZone != null)
            //    {
            //        MSG_RZ_NEW_FAMILY_APPLICANT newApplicant = new MSG_RZ_NEW_FAMILY_APPLICANT();
            //        newApplicant.uid = member.Key;
            //        newApplicant.applyUid = uid;
            //        member.Value.CurZone.Write(newApplicant);
            //    }
            //}
        }

        public void UpdateChief(Client newChief)
        {
            if (newChief == null) return;
            Chief = newChief;
        }

        public bool CanAssignTitle(FamilyTitle title)
        {
            Data data = DataListManager.inst.GetData("FamilyLevel", Level);
            if (data == null)
            {
                return false;
            }
            switch (title)
            { 
                case FamilyTitle.Chief:
                case FamilyTitle.Member:
                    return true;
                case FamilyTitle.Elite:
                    //int eliteCount = 0;
                    //int eliteMaxCount = data.GetInt("EliteCount");
                    //foreach (var item in MemberList)
                    //{
                        //if (item.Value.FamilyTitle == FamilyTitle.Elite)
                        //{
                        //    eliteCount++;
                        //    if (eliteCount >= eliteMaxCount)
                        //    {
                        //        return false;
                        //    }
                        //}
                    //}
                    return true;
                case FamilyTitle.ViceChief:
                    //int viceCount = 0;
                    //int viceMaxCount = data.GetInt("ViceChiefCount");
                    //foreach (var item in MemberList)
                    //{
                        //if (item.Value.FamilyTitle == FamilyTitle.ViceChief)
                        //{
                        //    viceCount++;
                        //    if (viceCount>= viceMaxCount)
                        //    {
                        //        return false;
                        //    }
                        //}
                    //}
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 添加家族BOSS血量信息
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="hp"></param>
        /// <param name="maxHp"></param>
        /// <param name="playerNum"></param>
        public void AddBossHp(int stage, int hp, int maxHp, int playerNum, int date)
        {
            int[] list = new int[] { hp, maxHp, playerNum, date };
            bossStates[stage] = list;
            //int[] list;
            //if (bossStates.TryGetValue(stage, out list))
            //{
            //    list = new int[] { hp, maxHp, playerNum };
            //}
            //else
            //{
            //    list = new int[] { hp, maxHp, playerNum };
            //    bossStates.Add(stage, list);
            //}
        }
        /// <summary>
        /// 获取家族BOSS的血量
        /// </summary>
        /// <param name="stage">副本ID</param>
        /// <returns>0：当前血量，1：最大血量，2玩家人数</returns>
        public int[] GetBossHp(int stage)
        {
            int[] hp;
            bossStates.TryGetValue(stage, out hp);
            return hp;
        }

        public void AddAccumulateDamage(int pcUid, int stage, ulong damage, int familyTitle)
        {
            FamilyAccumulateDamageModel info = new FamilyAccumulateDamageModel();
            info.PcUid = pcUid;
            info.Damage = damage;
            info.FamilyTitle = familyTitle;
            List<FamilyAccumulateDamageModel> list;
            if (accumulateDamageList.TryGetValue(stage, out list))
            {
                list.Add(info);
            }
            else
            {
                list = new List<FamilyAccumulateDamageModel>();
                list.Add(info);
                accumulateDamageList.Add(stage, list);
            }
        }

        public List<FamilyAccumulateDamageModel> GetAccumulateDamage(int stage)
        {
            List<FamilyAccumulateDamageModel> list;
            accumulateDamageList.TryGetValue(stage, out list);
            return list;
        }
        /// <summary>
        /// 累积伤害排序
        /// </summary>
        public void AccumulateDamageListSort()
        {
            List<int> keys = accumulateDamageList.Keys.ToList();
            foreach (var key in keys)
            {
                List<FamilyAccumulateDamageModel> list = accumulateDamageList[key];
                accumulateDamageList[key] = (from tempInfo in list orderby tempInfo.Damage descending select tempInfo).ToList();
            }
        }

        public void SetFamilyWarMembers()
        {
            FamilyWarMembers.Clear();

            List<Client> tempList = new List<Client>();
            foreach (var member in MemberList)
            {
                //if (member.Value.Level >= FamilyLibrary.WarMemberLimitLevel
                //    && member.Value.FamilyContributed >= FamilyLibrary.WarMemberLimitContribution)
                //{
                //    tempList.Add(member.Value);
                //}
            }

            //List<Client> memberList = (from tempInfo in tempList orderby tempInfo.Power descending select tempInfo).ToList();
            //int i = 0;
            //foreach (var member in memberList)
            //{
            //    FamilyWarMembers.Add(member.Uid);
            //    i++;
            //    if (i == FamilyLibrary.WarLimitCount)
            //    {
            //        break;
            //    }
            //}
        }
    }

    public class FamilyManager
    {
        public FamilyManager(RelationServerApi server)
        {
            this.server = server;
        }
        private RelationServerApi server;
        // key familyUid, value Family
        public Dictionary<int, Family> FamilyList = new Dictionary<int,Family>();
        public Dictionary<string, Family> FamilyNameList = new Dictionary<string,Family>();

        // 家族列表 按照阵营区分 定期排序 
        public List<Family> FreedomFamilyList = new List<Family>();
        public List<Family> RoyalFamilyList = new List<Family>();

        public Dictionary<int, Family> FamilyWarList = new Dictionary<int, Family>();
        public List<int> FamilyWarSortList = new List<int>();
        public Dictionary<int, DateTime> PcLeaveFamilyList = new Dictionary<int, DateTime>();
        public void AddFamily(Family family)
        {
            if (family == null) return;
            switch (family.Camp)
            { 
                case CampType.TianDou:
                    FreedomFamilyList.Add(family);
                    family.Rank = FreedomFamilyList.Count;
                    break;
                case CampType.XingLuo:
                    RoyalFamilyList.Add(family);
                    family.Rank = RoyalFamilyList.Count;
                    break;
                default:
                    Log.Warn("add family {0} camp {1} invalid", family.Uid, family.Camp);
                    return;
            }
            FamilyList.Add(family.Uid, family);
            FamilyNameList.Add(family.Name, family);
        }

        //public void Sort()
        //{
        //    FreedomFamilyList.Sort((left, right) =>
        //    {
        //        if (left.Contribution > right.Contribution)
        //        {
        //            return -1;
        //        }
        //        else
        //        {
        //            return 1;
        //        }
        //    });
        //    for (int i = 0; i < FreedomFamilyList.Count; i++)
        //    {
        //        FreedomFamilyList[i].Rank = i + 1;
        //    }

        //    RoyalFamilyList.Sort((left, right) =>
        //    {
        //        if (left.Contribution > right.Contribution)
        //        {
        //            return -1;
        //        }
        //        else
        //        {
        //            return 1;
        //        }
        //    });
        //    for (int i = 0; i < RoyalFamilyList.Count; i++)
        //    {
        //        RoyalFamilyList[i].Rank = i + 1;
        //    }
        //}

        public void CheckSort(int type)
        {
            //Sort();
            List<Family> list = new List<Family>();
            switch (type)
            {
                case (int)CampType.TianDou:
                case 3:
                    list = (from tempInfo in FreedomFamilyList orderby tempInfo.Contribution descending select tempInfo).ToList();
                    break;
                case (int)CampType.XingLuo:
                case 4:
                    list = (from tempInfo in RoyalFamilyList orderby tempInfo.Contribution descending select tempInfo).ToList();
                    break;
                default:
                    break;
            }

            switch (type)
            {
                case (int)CampType.TianDou:
                case (int)CampType.XingLuo:
                    list.Sort((left, right) =>
                        {
                            if (left.Contribution > right.Contribution)
                            {
                                return -1;
                            }
                            else
                            {
                                return 1;
                            }
                        });
                    break;
                case 3:
                case 4:
                    list.Sort((left, right) =>
                      {
                          if (left.WeekContribution > right.WeekContribution)
                          {
                              return -1;
                          }
                          else
                          {
                              return 1;
                          }
                      });
                    break;
                default:
                    break;
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Rank = i + 1;
            }

            switch (type)
            {
                case (int)CampType.TianDou:
                case 3:
                    FreedomFamilyList = list;
                    break;
                case (int)CampType.XingLuo:
                case 4:
                    RoyalFamilyList = list;
                    break;
                default:
                    break;
            }
        }

        //public int GetFamilyList(int page, List<PKS_ZC_FAMILY_SIMPLE_INFO> familyList, int uid)
        public int GetFamilyList(int page, List<object> familyList, int uid)
        {
            List<Family> list = null;
            int index = page * CONST.FAMILY_COUNT_PER_PAGE;
            if(index >= list.Count)
            {
                return list.Count;
            }
            int endIndex = Math.Min(list.Count, index + CONST.FAMILY_COUNT_PER_PAGE);

            //for (int i = index; i < endIndex; i++)
            //{
            //    PKS_ZC_FAMILY_SIMPLE_INFO info = new PKS_ZC_FAMILY_SIMPLE_INFO();
            //    list[i].GetFamilySimpleInfo(info, uid);
            //    familyList.Add(info);
            //}
            return list.Count;
        }

        public void RemoveFamily(Family family)
        {
            if (family == null) return;
            FamilyList.Remove(family.Uid);
            FamilyNameList.Remove(family.Name);

            //if (FamilyWarList.ContainsKey(family.Uid))
            //{
            //    FamilyWarList.Remove(family.Uid);
            //    server.DB.Call(new QueryRemoveFamilyWar(family.Uid)); 
            //}
               
            switch (family.Camp)
            { 
                case CampType.TianDou:
                    FreedomFamilyList.Remove(family);
                    break;
                case CampType.XingLuo:
                    RoyalFamilyList.Remove(family);
                    break;
                default:
                    break;
            }
            //server.DB.Call(new QueryRemoveFamily(family.Uid));
        }

        public Family FindFamily(int fid)
        {
            Family family = null;
            FamilyList.TryGetValue(fid, out family);
            return family;
        }

        public Family FindFamilyByName(string name)
        {
            Family family = null;
            FamilyNameList.TryGetValue(name, out family);
            return family;
        }


    }
}
