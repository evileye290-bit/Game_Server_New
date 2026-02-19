using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //时装
        public void UseFashion(FashionItem item)
        {
            if (item == null) return;

            int oldNum = item.PileNum;
            //激活状态设为0 表示卸下
            int curFashionId;
            FashionItem curFashion;
            //更新到客户端
            List<BaseItem> updateList = new List<BaseItem>();
            //if (!BagManager.FashionBag.IsInitFashion(item))
            //{
            //    if (item.DurationDay > 0 && item.GenerateTime == 0)
            //    {
            //        item.GenerateTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            //    }
            //}
            //else
            {
                item.GenerateTime = 0;
            }
            
            if (BagManager.FashionBag.CurFashion.TryGetValue(item.SonType,out curFashionId))
            {
               curFashion = BagManager.FashionBag.GetItem(curFashionId) as FashionItem;
               curFashion.ActivateState = 0;
                BagManager.FashionBag.UpdateItem(curFashion);
               updateList.Add(curFashion);
            }
            else
            {
            }

            //激活状态设为1 表示装备
            item.ActivateState = 1;
            BagManager.FashionBag.UpdateItem(item);

            BagManager.FashionBag.CurFashion[item.SonType] = item.Id;
         
            updateList.Add(item);
            SyncClientItemsInfo(updateList);
            //记录时装使用及其期限
            //RecordConsumeLog(ConsumeWay.ItemUse, RewardType.NormalItem, item.Id, 0, 1,"duration:"+item.DurationDay);
            //消耗埋点
            BIRecordConsumeItem(RewardType.NormalItem, ConsumeWay.ItemUse, item.Id, 1, 1, item);
            //BI 消耗物品
            KomoeEventLogItemFlow("reduce", "", item.Id, item.MainType.ToString(), 1, oldNum, item.PileNum, (int)ConsumeWay.ItemUse, 0, 0, 0, 0);
            ////更新到redis
            //server.Redis.Call(new OperateSetFashion(uid, item.Id,true));

            //更新到数据库
            //string tableName = "character";
            server.GameDBPool.Call(new QuerySetFashion(uid, item.Id));

            MSG_GC_CHAR_SIMPLE_INFO simpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
            GetSimpleInfo(simpleInfo);
            BroadCast(simpleInfo);
        }

        /// <summary>
        /// 时装过期需要发送邮件
        /// </summary>
        public void SendFashionPastDataEmail()
        {
            if (BagManager.FashionBag.fasionPastDataLst.Count>0)
            {
                EmailInfo info = EmailLibrary.GetEmailInfo(EmailLibrary.ItemOverTimeEmail);
                if (info == null)
                {
                    Log.Warn("player {0} SendFashionPastDataEmail not find email id:{1}", Uid, EmailLibrary.ItemOverTimeEmail);
                }

                foreach (var rm in BagManager.FashionBag.fasionPastDataLst)
                {
                    //string Text = info.Body.Replace(CommonConst.EMAIL_ITEM_ID, rm.Key.ToString());
                    if (!info.Body.Contains(CommonConst.EMAIL_ITEM_ID))
                    {
                        continue;
                    }
                    string param = CommonConst.EMAIL_ITEM_ID + ":" + rm.Key;
                    SendPersonEmail(info, info.Body, "", 0, param);
                }

                BagManager.FashionBag.fasionPastDataLst.Clear();
            }
        }
    }
}
