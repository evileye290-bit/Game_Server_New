using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //头像框
        public int GetFaceFrame()
        {
            return BagManager.FaceFrameBag.CurFaceFrameId;
        }   

        public void UseFaceFrame(FaceFrameItem item)
        {
            if (item == null) return;

            //把当前头像框 激活状态设为0 表示不再使用
            if (item.Id == BagManager.FaceFrameBag.CurFaceFrameId)
            {
                //重复装备同一件物品,不做更改
                return;
            }

            //更新到客户端
            List<BaseItem> updateList = new List<BaseItem>();

            FaceFrameItem curFaceFrame = BagManager.FaceFrameBag.GetItem(BagManager.FaceFrameBag.CurFaceFrameId) as FaceFrameItem;
            if (curFaceFrame !=null)
            {
                curFaceFrame.ActivateState = 0;
                BagManager.FaceFrameBag.UpdateItem(curFaceFrame);
                updateList.Add(curFaceFrame);
            }
            //设置头像框  激活状态设为1 表示启用
            item.ActivateState = 1;
            BagManager.FaceFrameBag.UpdateItem(item);
            //最后
            BagManager.FaceFrameBag.CurFaceFrameId = item.Id;
 
            updateList.Add(item);
            SyncClientItemsInfo(updateList); 
            //更新到redis
            server.GameRedis.Call(new OperateSetFaceFrame(uid, BagManager.FaceFrameBag.CurFaceFrameId));
        }

    }
}
