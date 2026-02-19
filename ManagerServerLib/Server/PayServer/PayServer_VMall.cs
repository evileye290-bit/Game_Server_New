using System.IO;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Barrack.Protocol.BM;
using Message.Manager.Protocol.MP;
using Message.Manager.Protocol.MZ;
using Message.Pay.Protocol.PM;
using ServerModels;
using ServerShared;

namespace ManagerServerLib
{
    public partial class PayServer
    {
        public void OnResponse_GetWebRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_PM_WEB_RECHAEGE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_PM_WEB_RECHAEGE>(stream);
            Log.Info($"player {pks.RoleId} vmall get recharge account {pks.AccountUid} game {pks.GameId} buy {pks.ProductId} ServerId {pks.ServerId} PayMode {pks.PayMode}");
            MSG_MP_WEB_RECHAEGE response = new MSG_MP_WEB_RECHAEGE();
            response.SessionUid = pks.SessionUid;
            response.RoleId = pks.RoleId;

            response.AccountUid = pks.AccountUid;
            response.GameId = pks.GameId;
            response.ProductId = pks.ProductId;
            response.ServerId = pks.ServerId;
            response.PayMode = pks.PayMode;

            response.ErrorCode = 0;

            int itemId = RechargeLibrary.GetRechargeItemId(pks.ProductId);
            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItem(itemId);//rechargeitemid
            if (rechargeItem == null)
            {
                response.ErrorCode = (int)ErrorCode.NotFindRechargeItemId;
                Write(response);
                return;
            }

            int orderId = Api.RechargeMng.GetNewHistoryId();
            Api.RechargeMng.SaveHistoryId(pks.RoleId, itemId, orderId, pks.PayMode);

            response.OrderId = orderId;
            Write(response);

        }

        public void OnResponse_GetRoleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_PM_GET_ROLE_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_PM_GET_ROLE_INFO>(stream);
            MSG_MP_GET_ROLE_INFO response = new MSG_MP_GET_ROLE_INFO();
            response.SessionUid = pks.SessionUid;
            response.RoleId = pks.RoleId;
            response.ErrorCode = 0;

            var query = new QueryRoleInfo(response.RoleId);
            Api.GameDBPool.Call(query, ret =>
            {
                if ((int)ret == 1)
                {
                    if (query.roleInfo.roleId == 0)
                    {
                        response.ErrorCode = (int)ErrorCode.CharNotExist;
                        Write(response);
                        return;
                    }
                    response.RoleName = query.roleInfo.roleName;
                    response.RoleLevel = query.roleInfo.roleLevel;
                    response.Account = query.roleInfo.account;
                    response.RoleId = query.roleInfo.roleId;
                    response.LastLoginTime = query.roleInfo.lastLoginTime;
                    response.RechargeSum = query.roleInfo.rechargeSum;
                    response.PartnerId = query.roleInfo.partnerId;
                    response.RegisterTime = query.roleInfo.registerTime;
                    Write(response);
                }
                else
                {
                    response.ErrorCode = 1;
                    Write(response);
                }
            });
        }


        public void OnResponse_GetRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_PM_RECHAEGE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_PM_RECHAEGE>(stream);
            Log.Info($"player {pks.RoleId} vmall get recharge {pks.OrderNo} buy {pks.ItemId} num {pks.Num} common {pks.Comment}");
            MSG_MP_RECHAEGE response = new MSG_MP_RECHAEGE();
            response.SessionUid = pks.SessionUid;
            response.RoleId = pks.RoleId;
            response.ItemId = pks.ItemId;
            response.Num = pks.Num;
            response.OrderNo = pks.OrderNo;
            response.Comment = pks.Comment;
            response.ErrorCode = 2;

            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItem(pks.ItemId);//rechargeitemid
            if (rechargeItem == null)
            {
                response.ErrorCode = (int)ErrorCode.NotFindRechargeItemId;
                Write(response);
                return;
            }

            RechargePriceModel price = RechargeLibrary.GetRechargePrice(rechargeItem.RechargeId);

            if (price == null)
            {
                response.ErrorCode = (int)ErrorCode.NotFindRechargeItemPrice;
                Write(response);
                return;
            }

            if (rechargeItem.GiftType == RechargeGiftType.VWall)
            {
                float amount = price.Money;
                long orderId = pks.OrderNo;
                Api.GameDBPool.Call(new QueryInsertVMallOrderId(orderId, pks.RoleId, pks.ItemId, amount, (int)RechargeWay.VMall, pks.Num, pks.Comment, Api.Now()), ret =>
                {
                    if ((int)ret == 1)
                    {
                        //MSG_MR_SEND_VWALL_REWARD_EMAI emailMsg = new MSG_MR_SEND_VWALL_REWARD_EMAI();
                        //emailMsg.OrderId = orderId;
                        //emailMsg.Uid = pks.RoleId;
                        //emailMsg.EmailId = rechargeItem.EmailId;

                        //string reward = string.Empty;
                        //if (pks.Num > 1 && !string.IsNullOrEmpty(rechargeItem.Reward))
                        //{
                        //    List<ItemBasicInfo> allRewards = RewardDropLibrary.GetSimpleRewards(rechargeItem.Reward, pks.Num);
                        //    if (allRewards.Count > 0)
                        //    {
                        //        foreach (var item in allRewards)
                        //        {
                        //            reward += "|" + item.ToString();
                        //        }
                        //        reward = reward.Substring(1);
                        //    }

                        //}
                        //else
                        //{
                        //    reward = rechargeItem.Reward;
                        //}

                        //emailMsg.Reward = reward;
                        //Api.RelationServer.Write(emailMsg);

                        //通知Zone发奖
                        MSG_MZ_UPDATE_RECHARGE msg = new MSG_MZ_UPDATE_RECHARGE();
                        msg.OrderId = orderId;
                        msg.Uid = pks.RoleId;
                        msg.RechargeId = pks.ItemId;
                        msg.Money = amount;
                        msg.Way = (int)RechargeWay.VMall;
                        msg.OrderInfo = pks.OrderNo.ToString();
                        msg.Num = pks.Num;
                        Api.ZoneServerManager.Broadcast(msg);


                        response.SessionUid = pks.SessionUid;
                        response.RoleId = pks.RoleId;
                        response.ErrorCode = 0;

                        response.ItemId = pks.ItemId;
                        response.Num = pks.Num;
                        response.OrderNo = pks.OrderNo;
                        response.Comment = pks.Comment;

                        //发货逻辑
                        Write(response);

                        //string payWay = "2";
                        //string moneyType = "CNY";
                        //Api.BILoggerMng.RechargeTaLog(uid, "", "", "", Api.MainId, price.Price, orderId.ToString(), orderId.ToString(), rechargeItem.GiftType.ToString(), moneyType, payWay, pks.ItemId.ToString(), 1, "", "", "", "", "", "", "", "", "");
                    }
                    else
                    {
                        response.SessionUid = pks.SessionUid;
                        response.RoleId = pks.RoleId;
                        response.ErrorCode = (int)ErrorCode.OrderNumberRepeated;

                        response.ItemId = pks.ItemId;
                        response.Num = pks.Num;
                        response.OrderNo = pks.OrderNo;
                        response.Comment = pks.Comment;
                        Write(response);
                        return;
                    }
                });
            }
            else
            {
                Log.WarnLine($"player {pks.RoleId} vmall get recharge {pks.OrderNo} buy {pks.ItemId} type is {rechargeItem.GiftType}");
                response.ErrorCode = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

        }
    }
}