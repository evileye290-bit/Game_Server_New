using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Relation.Protocol.RZ;
using Logger;
using System.IO;
using RelationServerLib;
using CommonUtility;
using DataProperty;
using ServerShared;
using EnumerateUtility;
using Message.Zone.Protocol.ZR;
using DBUtility;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        /// <summary>
        /// 获取家族信息
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_FamilyInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_FAMILY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FAMILY_INFO>(stream);
            //Client client = zoneManager.GetClient(msg.Uid);
            //if (client == null ) return;
            //if (client.Family == null)
            //{
            //    // 没有家族 返回家族列表 
            //    PKS_ZC_FAMILY_LIST familyListMsg = new PKS_ZC_FAMILY_LIST();
            //    familyListMsg.uid = client.Uid;
            //    familyListMsg.familyCnt = zoneManager.FamilyMng.GetFamilyList(msg.Page, familyListMsg.familyList, msg.Uid);
            //    Write(familyListMsg);
            //}
            //else
            //{
            //    // 有家族 返回家族成员列表
            //    PKS_ZC_FAMILY_DETAIL_INFO familyMsg = new PKS_ZC_FAMILY_DETAIL_INFO();
            //    client.Family.GetFamilyDetailInfo(msg.Uid, msg.Page, familyMsg, client);
            //    Write(familyMsg);
            //}
        }

        /// <summary>
        /// 查找家族
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_SearchFamily(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_SEARCH_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SEARCH_FAMILY>(stream);
            //Client client = zoneManager.GetClient(msg.Uid);
            //if (client == null) return;
            //int fid = 0;
            //PKS_ZC_SEARCH_FAMILY notify = new PKS_ZC_SEARCH_FAMILY();
            //notify.Uid = msg.Uid;
            //notify.info = new PKS_ZC_FAMILY_SIMPLE_INFO();

            //if (int.TryParse(msg.Name, out fid) == true)
            //{
            //    // 通过 fid 查找家族
            //    Family family = zoneManager.FamilyMng.FindFamily(fid);
            //    if (family != null)
            //    {
            //        family.GetFamilySimpleInfo(notify.info, msg.Uid);
            //    }
            //    else
            //    {
            //        notify.info.fid = 0;
            //    }
            //}
            //else
            //{
            //    // 通过name 查找家族
            //    Family family = zoneManager.FamilyMng.FindFamilyByName(msg.Name);
            //    if (family != null)
            //    {
            //        family.GetFamilySimpleInfo(notify.info, msg.Uid);
            //    }
            //    else
            //    {
            //        notify.info.fid = 0;
            //    }
            //}
            //Write(notify);
        }

        /// <summary>
        /// 加入家族
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_JoinFamily(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_JOIN_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_JOIN_FAMILY>(stream);
            Log.Write("player {0} request to join family {1}", msg.Uid, msg.Fid);
            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null) return;
            MSG_RZ_JOIN_FAMILY notify = new MSG_RZ_JOIN_FAMILY();
            //if (client.Family != null)
            //{
            //    notify.Uid = msg.Uid;
            //    notify.List.Add(msg.Fid);
            //    notify.Result = (int)ErrorCode.NoCamp;
            //    Write(notify);
            //    return;
            //}
            DateTime leaveTime;
            if (ZoneManager.FamilyMng.PcLeaveFamilyList.TryGetValue(msg.Uid, out leaveTime))
            {
                double day = (RelationServerApi.now - leaveTime).TotalHours;
                //if (FamilyLibrary.LeaveFamilyTimeLimit > day)
                //{
                //    notify.Uid = msg.Uid;
                //    notify.List.Add(msg.Fid);
                //    notify.Result = (int)ErrorCode.LeaveFamilyTimeError;
                //    Write(notify);
                //    return;
                //}
            }
            if (msg.Fid == 0)
            {
                // 一键申请
                int count = 0;
                List<Family> familyList = new List<Family>();
                MSG_RZ_JOIN_FAMILY notifyFamilies = new MSG_RZ_JOIN_FAMILY();
                notifyFamilies.Result = (int)ErrorCode.Success;
                notifyFamilies.Uid = msg.Uid;
                foreach (var family in familyList)
                {
                    if (count >= CONST.JOIN_FAMILIES_COUNT)
                    {
                        break;
                    }
                    if (family.ApplyList.ContainsKey(msg.Uid))
                    {
                        continue;
                    }
                    if (family.CanJoinFamily() == ErrorCode.Success)
                    {
                        // 添加申请
                        family.AddNewApplicant(msg.Uid);
                        notifyFamilies.List.Add(family.Uid);
                    }
                }
                Write(notifyFamilies);
            }
            else
            {
                // 申请指定家族
                Family family = null;
                family = ZoneManager.FamilyMng.FindFamily(msg.Fid);
                if (family == null || family.Chief == null)
                {
                    notify.Uid = msg.Uid;
                    notify.List.Add(msg.Fid);
                    //notify.Result = (int)ErrorCode.FamilyNotExist;
                    Write(notify);
                    return;
                }
                if (family.ApplyList.ContainsKey(client.Uid))
                {
                    notify.Uid = msg.Uid;
                    notify.List.Add(msg.Fid);
                    notify.Result = (int)ErrorCode.AlreadyApplyFamily;
                    Write(notify);
                    return;
                }

                ErrorCode result = family.CanJoinFamily();
                if (result != ErrorCode.Success)
                {
                    notify.Uid = msg.Uid;
                    notify.List.Add(msg.Fid);
                    notify.Result = (int)result;
                    Write(notify);
                    return;
                }

                // 验证通过 
                notify.Uid = msg.Uid;
                notify.List.Add(msg.Fid);
                notify.Result = (int)ErrorCode.Success;
                Write(notify);

                family.AddNewApplicant(msg.Uid);
            }
        }

        /// <summary>
        /// 创建家族
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_CreateFamily(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CREATE_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CREATE_FAMILY>(stream);
            Client client = ZoneManager.GetClient(msg.Uid);
            try
            {
                Log.Write("player {0} request create family {1}", msg.Uid, msg.Name);
            }
            catch (Exception)
            {
            }
            //if (client.Family != null)
            //{
            //    MSG_RZ_CREATE_FAMILY notify = new MSG_RZ_CREATE_FAMILY();
            //    notify.Uid = msg.Uid;
            //    notify.Result = (int)ErrorCode.InFamily;
            //    Write(notify);
            //    return;
            //}
            // TODO 检查重名

            MSG_RZ_CREATE_FAMILY notifySuccess = new MSG_RZ_CREATE_FAMILY();
            notifySuccess.Uid = msg.Uid;
            notifySuccess.Result = (int)ErrorCode.Success;
            Write(notifySuccess);

            // 验证通过 创建家族
            Family family = new Family(Api, Api.GetMaxFid(), msg.Name, 0, 0, client, msg.Declaration, "");
            client.JoinFamily(family, FamilyTitle.Chief);
            family.AddMember(client);
            ZoneManager.FamilyMng.AddFamily(family);

            // 同步DB
            //api.GameDBPool.Call(new QueryCreateCuild(family.Uid, family.Name, family.Contribution, msg.Uid, family.Declaration));

            //通知
            //PKS_ZC_FAMILY_DETAIL_INFO familyInfo = new PKS_ZC_FAMILY_DETAIL_INFO();
            //family.GetFamilyDetailInfo(msg.Uid, 0, familyInfo, client);
            //Write(familyInfo);

            ////创建家族发公告
            //MSG_RZ_BROADCAST_ANNOUNCEMENT announcement = new MSG_RZ_BROADCAST_ANNOUNCEMENT();
            //announcement.type = (int)ANNOUNCEMENT_TYPE.CREATE_FAMILY;
            //announcement.list.Add("%M0");
            //announcement.list.Add(client.Name);
            //announcement.list.Add(family.Name);
            //zoneManager.BroadcastAllZones(announcement);
        }

        /// <summary>
        /// 家族申请列表
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_FamilyApplyList(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_FAMILY_APPLY_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FAMILY_APPLY_LIST>(stream);
            //Client client = zoneManager.GetClient(msg.Uid);
            //if (client == null || client.Family == null) return;
            //Family family = client.Family;


            //PKS_ZC_FAMILY_APPLY_LIST notify = new PKS_ZC_FAMILY_APPLY_LIST();
            //notify.Uid = msg.Uid;
            //notify.applyCount = family.ApplyList.Count;
            // 获取所有申请者
            //List<Client> list = new List<Client>();
            //List<int> removeList = new List<int>();
            //foreach (var item in family.ApplyList)
            //{
            //    if (item.Value.AddHours(24) < Api.now)
            //    {
            //        removeList.Add(item.Key);
            //    }
            //    else
            //    {
            //        Client application = zoneManager.GetClient(item.Key);
            //        if (application != null)
            //        {
            //            list.Add(application);
            //        }
            //    }
            //}
            // 清理超过24小时的申请者
            //foreach (var item in removeList)
            //{
            //    family.ApplyList.Remove(item);
            //}

            //list.Sort((left, rigit) =>
            //{
            //    if (left.Power > rigit.Power)
            //    {
            //        return -1;
            //    }
            //    else
            //    {
            //        return 1;
            //    }
            //});

            //int index = msg.Page * CONST.FAMILIY_APPLICATION_COUNT_PER_PAGE;
            //if (index >= list.Count)
            //{
            //    Write(notify);
            //    return;
            //}
            //int endIndex = Math.Min(list.Count, index + CONST.FAMILIY_APPLICATION_COUNT_PER_PAGE);

            //for (int i = index; i < endIndex; i++)
            //{
            //    PKS_ZC_FAMILY_APPLICATION_INFO info = new PKS_ZC_FAMILY_APPLICATION_INFO();
            //    Client application = list[i];
            //    info.uid = application.Uid;
            //    info.name = application.Name;
            //    info.job = application.JobId;
            //    info.power = application.Power;
            //    info.level = application.Level;
            //    info.applyTime = family.ApplyList[info.uid].ToString();
            //    notify.List.Add(info);
            //}
            //Write(notify);
        }

        public void OnResponse_FamilyApplicationAgree(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_FAMILY_APPLICATION_AGREE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FAMILY_APPLICATION_AGREE>(stream);
            //Client client = zoneManager.GetClient(msg.Uid);
            //if (client == null) return;
            //Log.Write("player {0} agree {1} family application {2}", msg.Uid, msg.Agree, msg.applyUid);
            //if (client.Family == null)
            //{
            //    PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //    notify.Uid = msg.Uid;
            //    notify.Result = (int)ErrorCode.NotInFamily;
            //    Write(notify);
            //    return;
            //}
            //if (client.FamilyTitle > FamilyTitle.ViceChief)
            //{
            //    PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //    notify.Uid = msg.Uid;
            //    notify.Result = (int)ErrorCode.FamilyTitleLimit;
            //    Write(notify);
            //    return;
            //}

            //if (msg.applyUid == 0)
            //{
            //    // 批量申请处理
            //    if (msg.Agree == true)
            //    {
            //        PKS_ZC_FAMILY_APPLICATION_AGREE notifySuccess = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //        notifySuccess.Uid = msg.Uid;
            //        notifySuccess.Result = (int)ErrorCode.Success;
            //        List<int> removeList = new List<int>();
            //        foreach (var item in client.Family.ApplyList)
            //        {
            //            if (client.Family.CanJoinFamily() == ErrorCode.Success)
            //            {
            //                Client application = zoneManager.GetClient(item.Key);
            //                if (application == null || application.Family != null)
            //                {
            //                    removeList.Add(item.Key);
            //                }
            //                else
            //                {
            //                    client.Family.AddMember(application);
            //                    application.JoinFamily(client.Family, FamilyTitle.Member);
            //                    notifySuccess.removeApplyList.Add(item.Key);
            //                    PKS_ZC_FAMILY_MEMBER_INFO memberInfo = new PKS_ZC_FAMILY_MEMBER_INFO();
            //                    application.GetFamilyMemberInfo(memberInfo);
            //                    notifySuccess.newMemberList.Add(memberInfo);
            //                    removeList.Add(item.Key);
            //                }
            //            }
            //        }
            //        foreach (var item in removeList)
            //        {
            //            client.Family.ApplyList.Remove(item);
            //        }
            //        Write(notifySuccess);
            //    }
            //    else
            //    {
            //        // 全部残忍拒绝
            //        PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //        foreach (var item in client.Family.ApplyList)
            //        {
            //            notify.removeApplyList.Add(item.Key);
            //        }
            //        notify.Uid = msg.Uid;
            //        notify.Result = (int)ErrorCode.Success;
            //        Write(notify);
            //        client.Family.ApplyList.Clear();
            //        return;
            //    }
            //}
            //else
            //{
            //    if (msg.Agree == true)
            //    {
            //        if (client.Family.ApplyList.ContainsKey(msg.applyUid) == false)
            //        {
            //            PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //            notify.Uid = msg.Uid;
            //            notify.Result = (int)ErrorCode.NotInFamilyApplyList;
            //            Write(notify);
            //            return;
            //        }

            //        ErrorCode result = client.Family.CanJoinFamily();
            //        if (result != ErrorCode.Success)
            //        {
            //            PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //            notify.Uid = msg.Uid;
            //            notify.Result = (int)result;
            //            notify.removeApplyList.Add(msg.applyUid);
            //            Write(notify);
            //            return;
            //        }

            //        Client application = zoneManager.GetClient(msg.applyUid);
            //        if (application == null || application.Family != null)
            //        {
            //            client.Family.ApplyList.Remove(msg.applyUid);
            //            PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //            notify.Uid = msg.Uid;
            //            notify.Result = (int)ErrorCode.InFamily;
            //            notify.removeApplyList.Add(msg.applyUid);
            //            Write(notify);
            //            return;
            //        }

            //        // 通过 加入
            //        client.Family.ApplyList.Remove(msg.applyUid);
            //        client.Family.AddMember(application);
            //        application.JoinFamily(client.Family, FamilyTitle.Member);
            //        PKS_ZC_FAMILY_APPLICATION_AGREE notifySuccess = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //        notifySuccess.Uid = msg.Uid;
            //        notifySuccess.Result = (int)ErrorCode.Success;
            //        notifySuccess.removeApplyList.Add(msg.applyUid);
            //        PKS_ZC_FAMILY_MEMBER_INFO memberInfo = new PKS_ZC_FAMILY_MEMBER_INFO();
            //        application.GetFamilyMemberInfo(memberInfo);
            //        notifySuccess.newMemberList.Add(memberInfo);
            //        Write(notifySuccess);
            //    }
            //    else
            //    {
            //        // 残忍拒绝
            //        PKS_ZC_FAMILY_APPLICATION_AGREE notify = new PKS_ZC_FAMILY_APPLICATION_AGREE();
            //        notify.removeApplyList.Add(msg.applyUid);
            //        notify.Uid = msg.Uid;
            //        notify.Result = (int)ErrorCode.Success;
            //        Write(notify);
            //        client.Family.ApplyList.Remove(msg.applyUid);
            //        return;

            //    }
            //}
        }

        public void OnResponse_AssignFamilyTitle(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ASSIGN_FAMILY_TITLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ASSIGN_FAMILY_TITLE>(stream);
            // 在逗我
            if (msg.Uid == msg.MemberUid) return;
            Client chief = ZoneManager.GetClient(msg.Uid);
            if (chief == null) return;
            Log.Write("player {0} assigin member {1} title {2}", msg.Uid, msg.MemberUid, msg.Title);
            //if (chief.Family == null)
            //{
            //    MSG_RZ_ASSIGN_FAMILY_TITLE notify = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            //    notify.Result = (int)ErrorCode.NotInFamily;
            //    notify.Uid = msg.Uid;
            //    Write(notify);
            //    return;
            //}
            //if (chief.FamilyTitle != FamilyTitle.Chief)
            //{
            //    MSG_RZ_ASSIGN_FAMILY_TITLE notify = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            //    notify.Result = (int)ErrorCode.FamilyTitleLimit;
            //    notify.Uid = msg.Uid;
            //    Write(notify);
            //    return;
            //}
            //if (chief.Family.MemberList.ContainsKey(msg.MemberUid) == false)
            //{
            //    MSG_RZ_ASSIGN_FAMILY_TITLE notify = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            //    notify.Result = (int)ErrorCode.MemberNotExist;
            //    notify.Uid = msg.Uid;
            //    Write(notify);
            //    return;
            //}

            //Client member = zoneManager.GetClient(msg.MemberUid);
            //if (member == null || member.Family == null || member.Family != chief.Family)
            //{
            //    MSG_RZ_ASSIGN_FAMILY_TITLE notify = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            //    notify.Result = (int)ErrorCode.NotInFamily;
            //    notify.Uid = msg.Uid;
            //    Write(notify);
            //    return;
            //}

            MSG_RZ_ASSIGN_FAMILY_TITLE notifySuccess = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            notifySuccess.Uid = msg.Uid;
            //notifySuccess.memberUid = member.Uid;
            //switch ((FamilyTitle)msg.Title)
            //{
            //    case FamilyTitle.Chief:
            //        // 转交族长
            //        chief.Family.Chief = member;
            //        // 同步DB 族长更换
            //        //server.DB.Call(new QueryUpdateFamilyChief(chief.Family.Uid, member.Uid));
            //        chief.UpdateFamilyInfo(chief.Family, FamilyTitle.Member);
            //        member.UpdateFamilyInfo(chief.Family, FamilyTitle.Chief);
            //        notifySuccess.Result = (int)ErrorCode.Success;
            //        notifySuccess.myTitle = (int)FamilyTitle.Member;
            //        notifySuccess.memberTitle = (int)FamilyTitle.Chief;
            //        Write(notifySuccess);
            //        break;
            //    case FamilyTitle.ViceChief:
            //        // 任命副族长
            //        if (chief.Family.CanAssignTitle((FamilyTitle)msg.Title) == false)
            //        {
            //            MSG_RZ_ASSIGN_FAMILY_TITLE notify = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            //            notify.Result = (int)ErrorCode.FamilyTitleFull;
            //            notify.Uid = msg.Uid;
            //            Write(notify);
            //            return;
            //        }
            //        member.UpdateFamilyInfo(chief.Family, FamilyTitle.ViceChief);
            //        notifySuccess.Result = (int)ErrorCode.Success;
            //        notifySuccess.myTitle = (int)FamilyTitle.Chief;
            //        notifySuccess.memberTitle = (int)FamilyTitle.ViceChief;
            //        Write(notifySuccess);
            //        break;
            //    case FamilyTitle.Elite:
            //        // 任命精英
            //        if (chief.Family.CanAssignTitle((FamilyTitle)msg.Title) == false)
            //        {
            //            MSG_RZ_ASSIGN_FAMILY_TITLE notify = new MSG_RZ_ASSIGN_FAMILY_TITLE();
            //            notify.Result = (int)ErrorCode.FamilyTitleFull;
            //            notify.Uid = msg.Uid;
            //            Write(notify);
            //            return;
            //        }
            //        member.UpdateFamilyInfo(chief.Family, FamilyTitle.Elite);
            //        notifySuccess.Result = (int)ErrorCode.Success;
            //        notifySuccess.myTitle = (int)FamilyTitle.Chief;
            //        notifySuccess.memberTitle = (int)FamilyTitle.Elite;
            //        Write(notifySuccess);
            //        break;
            //    case FamilyTitle.Member:
            //        member.UpdateFamilyInfo(chief.Family, FamilyTitle.Member);
            //        notifySuccess.Result = (int)ErrorCode.Success;
            //        notifySuccess.myTitle = (int)FamilyTitle.Chief;
            //        notifySuccess.memberTitle = (int)FamilyTitle.Member;
            //        Write(notifySuccess);
            //        break;
            //    default:
            //        break;
            //}
        }

        /// <summary>
        /// 退出家族
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_QuitFamily(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_QUIT_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_QUIT_FAMILY>(stream);
            Log.Write("player {0} quit family", msg.Uid);
            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null) return;
            //if (client.Family == null)
            //{
            //    MSG_RZ_QUIT_FAMILY notify = new MSG_RZ_QUIT_FAMILY();
            //    notify.Uid = msg.Uid;
            //    notify.Result = (int)ErrorCode.NotInFamily;
            //    Write(notify);
            //    return;
            //}
            //if (client.FamilyTitle == FamilyTitle.Chief && client.Family.MemberList.Count > 1)
            //{
            //    MSG_RZ_QUIT_FAMILY notify = new MSG_RZ_QUIT_FAMILY();
            //    notify.Uid = msg.Uid;
            //    notify.Result = (int)ErrorCode.FamilyChief;
            //    Write(notify);
            //    return;
            //}
            //if (client.Family.MemberList.ContainsKey(msg.Uid) == false)
            //{
            //    MSG_RZ_QUIT_FAMILY notify = new MSG_RZ_QUIT_FAMILY();
            //    notify.Uid = msg.Uid;
            //    notify.Result = (int)ErrorCode.MemberNotExist;
            //    Write(notify);
            //    return;
            //}

            //if (client.Family.Chief == client && client.Family.MemberList.Count == 1 && client.Family.MemberList.ContainsKey(client.Uid))
            //{
            //    // 家族中只有族长一人 退出家族后家族解散
            //    zoneManager.FamilyMng.RemoveFamily(client.Family);
            //    //移除家族首杀
            //    ////RemoveFamilyFirstKillList(client.Family.Uid);
            //}
            //// 成功退出
            //client.Family.RemoveMember(client);

            ////移除家族内立累计伤害
            //RemoveDamageList(client.Family, client.Uid);

            client.QuitFamily(false, false);
            //记录离开家族时间
            ZoneManager.FamilyMng.PcLeaveFamilyList[client.Uid] = RelationServerApi.now;
        }

        /// <summary>
        /// 移除玩家排行榜信息
        /// </summary>
        /// <param name="family"></param>
        /// <param name="PcUid"></param>
        private void RemoveDamageList(Family family, int PcUid)
        {
            //移除累积伤害信息
            foreach (var item in family.AccumulateDamageList)
            {
                int index = -1;
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].PcUid == PcUid)
                    {
                        index = i;
                    }
                }
                if (index >= 0)
                {
                    item.Value.RemoveAt(index);
                }
            }
            ////移除单场最高伤害信息
            //foreach (var item in zoneManager.FamilySingleDamageList)
            //{
            //    int index = -1;
            //    for (int i = 0; i < item.Value.Count; i++)
            //    {
            //        if (item.Value[i][0] == PcUid)
            //        {
            //            index = i;
            //        }
            //    }
            //    if (index >= 0)
            //    {
            //        item.Value.RemoveAt(index);
            //    }
            //}
        }

        /// <summary>
        /// 移除家族成员
        /// </summary>
        /// <param name="stream"></param>
        public void OnResponse_KickFamilyMember(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_KICK_FAMILY_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_KICK_FAMILY_MEMBER>(stream);
            Log.Write("player {0} request family member {1}", msg.Uid, msg.MemberUid);
            Client chief = ZoneManager.GetClient(msg.Uid);
            if (chief == null) return;
            //if (chief.Family == null)
            //{
            //    MSG_RZ_KICK_FAMILY_MEMBER notify = new MSG_RZ_KICK_FAMILY_MEMBER();
            //    notify.Uid = msg.Uid;
            //    notify.memberUid = msg.MemberUid;
            //    notify.Result = (int)ErrorCode.NotInFamily;
            //    Write(notify);
            //    return;
            //}
            //if (chief.FamilyTitle != FamilyTitle.Chief)
            //{
            //    MSG_RZ_KICK_FAMILY_MEMBER notify = new MSG_RZ_KICK_FAMILY_MEMBER();
            //    notify.Uid = msg.Uid;
            //    notify.memberUid = msg.MemberUid;
            //    notify.Result = (int)ErrorCode.FamilyTitleLimit;
            //    Write(notify);
            //    return;
            //}
            //Client member = zoneManager.GetClient(msg.MemberUid);
            //if (member == null)
            //{
            //    MSG_RZ_KICK_FAMILY_MEMBER notify = new MSG_RZ_KICK_FAMILY_MEMBER();
            //    notify.Uid = msg.Uid;
            //    notify.memberUid = msg.MemberUid;
            //    notify.Result = (int)ErrorCode.MemberNotExist;
            //    Write(notify);
            //    return;
            //}
            //if (chief.Family.MemberList.ContainsKey(msg.Uid) == false || member.Family != chief.Family)
            //{
            //    MSG_RZ_KICK_FAMILY_MEMBER notify = new MSG_RZ_KICK_FAMILY_MEMBER();
            //    notify.Uid = msg.Uid;
            //    notify.memberUid = msg.MemberUid;
            //    notify.Result = (int)ErrorCode.MemberNotInFamily;
            //    Write(notify);
            //    return;
            //}

            //// 成功踢出
            //MSG_RZ_KICK_FAMILY_MEMBER notifySuccess = new MSG_RZ_KICK_FAMILY_MEMBER();
            //notifySuccess.Uid = msg.Uid;
            //notifySuccess.memberUid = msg.MemberUid;
            //notifySuccess.Result = (int)ErrorCode.Success;
            //Write(notifySuccess);

            //chief.Family.RemoveMember(member);
            ////移除家族内立累计伤害
            //RemoveDamageList(chief.Family, member.Uid);

            //member.QuitFamily(true, true);
            ////记录离开家族时间
            //zoneManager.FamilyMng.PcLeaveFamilyList[member.Uid] = Api.now;
        }

        /// <summary>
        /// 家族排行榜
        /// </summary>
        /// <param name="stream"></param>
        //public void OnResponse_FamilyRank(MemoryStream stream, int uid = 0)
        //{
        //    MSG_ZR_FAMILY_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FAMILY_RANK>(stream);
        //    Client client = zoneManager.GetClient(msg.Uid);
        //    if (client == null) return;
        //    zoneManager.FamilyMng.CheckSort(msg.camp);
        //    List<Family> list;
        //    switch (msg.camp)
        //    {
        //        case (int)CampType.Freedom:
        //        case 3:
        //            list = zoneManager.FamilyMng.FreedomFamilyList;
        //            break;
        //        case (int)CampType.Royal:
        //        case 4:
        //            list = zoneManager.FamilyMng.RoyalFamilyList;
        //            break;
        //        default:
        //            Log.Warn("player {0} requred family rank type {1}: invalid camp type", msg.Uid, msg.camp);
        //            return;
        //    }
        //    PKS_ZC_FAMILY_RANK notify = new PKS_ZC_FAMILY_RANK();
        //    notify.Uid = msg.Uid;
        //    notify.Type = msg.camp; 
        //    if (client.Family != null)
        //    {
        //        notify.myFamily = new PKS_ZC_FAMILY_SIMPLE_INFO();
        //        client.Family.GetFamilySimpleInfo(notify.myFamily, client.Uid);
        //    }
        //    int count = Math.Min(CONST.FAMILY_COUNT_PER_PAGE, list.Count);
        //    for (int i = 0; i < count; i++)
        //    {
        //        PKS_ZC_FAMILY_SIMPLE_INFO info = new PKS_ZC_FAMILY_SIMPLE_INFO();
        //        list[i].GetFamilySimpleInfo(info, client.Uid);
        //        notify.List.Add(info);
        //    }
        //    Write(notify);
        //}

        public void OnResponse_UpdateFamilyContribution(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_FAMILY_CONTRIBUTION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_FAMILY_CONTRIBUTION>(stream);
            Client client = ZoneManager.GetClient(msg.Uid);
            //if (client != null && client.Family != null)
            //{
            //    if (msg.contribution > 0)
            //    {
            //        client.AddFamilyContributed(msg.contribution);
            //        client.Family.AddContribution(msg.contribution);
            //    }
            //}
        }

        public void OnResponse_FamilyContentEdit(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_FAMILY_CONTENT_EDIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FAMILY_CONTENT_EDIT>(stream);
            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null) return;
            MSG_RZ_FAMILY_CONTENT_EDIT notify = new MSG_RZ_FAMILY_CONTENT_EDIT();
            notify.Type = msg.Type;
            notify.Uid = msg.Uid;

            //if (client.Family == null)
            //{
            //    notify.Result = (int)ErrorCode.NotInFamily;
            //    Write(notify);
            //    return;
            //}
            //if (client.Family.Chief != client || client.FamilyTitle != FamilyTitle.Chief)
            //{
            //    notify.Result = (int)ErrorCode.FamilyTitleLimit;
            //    Write(notify);
            //    return;
            //}
            //// 通过 
            //switch ((FamilyContentType)msg.Type)
            //{
            //    case FamilyContentType.Declaration:
            //        client.Family.Declaration = msg.Content;
            //        //server.DB.Call(new QueryUpdateFamilyDeclaration(client.Family.Uid, msg.Content));
            //        break;
            //    case FamilyContentType.Notice:
            //        client.Family.Notice = msg.Content;
            //        server.DB.Call(new QueryUpdateFamilyNotice(client.Family.Uid, msg.Content));
            //        break;
            //    default:
            //        return;
            //}

            notify.Result = (int)ErrorCode.Success;
            notify.Content = msg.Content;
            Write(notify);
        }

    }
}
