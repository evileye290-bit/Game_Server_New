using CommonUtility;
using CommonUtility.Job;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Client.Protocol.CGate;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace GateServerLib
{
    public partial class ManagerServer
    {
        public void LoadCreateInfo2Redis(CreateCharacterModel createInfo)
        {
            RedisUtility.CharacterInfo info = new RedisUtility.CharacterInfo();

            info.Uid = createInfo.Uid;
            info.IsOnline = false;
            info.MainId = MainId;
            ShowInfo show = new ShowInfo();
            show.Name = createInfo.CharName;
            show.ShowFaceJpg = false;
            show.FaceFrame = CharacterInitLibrary.FaceFrame;

            show.Sex = createInfo.Sex;
            show.Job = createInfo.Job;


            show.Level = CharacterInitLibrary.Level;
            //show.ShowVoice = false;
            //show.Exp = CharacterInitLibrary.Exp;
            //Data nameData = DataListManager.inst.GetData("DefaultName", 1);
            //show.CurQueueName = nameData.GetString("HeroQueueName_0");
            //show.Birthday = GateServerApi.now.ToLongDateString();
            //show.Title = "";
            //show.FamilyName = "";

            //show.HeroCount = CharacterInitLibrary.Heros.Count;
            //show.SkinCount = 0;

            show.FaceIcon = createInfo.FaceIconId;
            show.HeroId = createInfo.HeroId;
            show.GodType = createInfo.GodType;

            show.LadderLevel = 1;
            show.LadderScore = 0;
            //show.LadderHistoryMaxScore = 0;
            //show.LadderTotalWinNum = 0;

            show.CrossLevel = 1;
            show.CrossStar = 0;
            //List<FashionInfo> itemsFashion = GetFashions(MSG, uid);
            //show.FashionCount = itemsFashion.Count;
            //foreach (var item in itemsFashion)
            //{
            //    if (item != null)
            //    {
            //        data = BagLibrary.GetFashionModelData(item.TypeId);
            //        if (data != null)
            //        {
            //            FashionType sontype = (FashionType)data.GetInt("SubType");
            //            switch (sontype)
            //            {
            //                case FashionType.Head:
            //                    show.Head = item.TypeId;
            //                    break;
            //                case FashionType.Weapon:
            //                    show.Weapon = item.TypeId;
            //                    break;
            //                case FashionType.Face:
            //                    show.Face = item.TypeId;
            //                    break;
            //                case FashionType.Clothes:
            //                    show.Clothes = item.TypeId;
            //                    break;
            //                case FashionType.Back:
            //                    show.Back = item.TypeId;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            info.showInfo = show;
            Api.GameRedis.Call(new OperateCreatePlayer(createInfo.Uid, info));
        }

        /// <summary>
        /// 初始时装形象
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private List<FashionInfo> GetFashions(MSG_CG_CREATE_CHARACTER msg, int uid)
        {
            //对客户端传过来的值严格判断
            List<FashionInfo> items = new List<FashionInfo>();

            //int systemHeadId = msg.Head;
            //Data data = DataListManager.inst.GetData("SystemHairStyle", systemHeadId);
            //if (data != null)
            //{
            //    //这里和头部形象head 绑定了一个头像faceicon
            //    int headId = data.GetInt("HeadId");
            //    int faceIcon = data.GetInt("FaceIcon");

            //    data = DataListManager.inst.GetData("Fashion", headId);
            //    if (data != null)
            //    {
            //        MainType type = (MainType)data.GetInt("MainType");
            //        FashionType subtype = (FashionType)data.GetInt("SubType");

            //        if (MainType.Fashion == type && subtype == FashionType.Head)
            //        {
            //            FashionInfo item = new FashionInfo();
            //            item.OwnerUid = uid;
            //            item.Uid = Api.UID.NewIuid(Api.MainId, Api.SubId);
            //            item.TypeId = headId;
            //            item.PileNum = 1;
            //            item.ActivateState = 1;
            //            item.GenerateTime = 0;
            //            items.Add(item);
            //        }
            //    }
            //    else
            //    {
            //        Log.Warn("create player got en error head item id {0}", headId);
            //        return null;
            //    }

            //    data = DataListManager.inst.GetData("RecommendIcon", faceIcon); //防止配表失误，严格判断值的正确性
            //    if (data != null)
            //    {
            //    }
            //    else
            //    {
            //        Log.Warn("create player got en error faceicon id {0}", faceIcon);
            //        return null;
            //    }
            //}
            //else
            //{
            //    Log.Warn("create player got en error systemheadId {0}", systemHeadId);
            //    return null;
            //}
            if (msg.SystemFashionId > 0)
            {
                Data data = DataListManager.inst.GetData("SystemFashion", msg.SystemFashionId);
                if (data != null)
                {
                    //串样式 300001:330001
                    string[] arr = data.GetString("Fashions").Split(':');
                    foreach (var id in arr)
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }

                        data = DataListManager.inst.GetData("Fashion", int.Parse(id));
                        if (data != null)
                        {
                            MainType type = (MainType)data.GetInt("MainType");
                            FashionType sontype = (FashionType)data.GetInt("SubType");
                            if (sontype == FashionType.Head)
                            {
                                continue;
                            }
                            if (MainType.Fashion == type)
                            {
                                FashionInfo item = new FashionInfo();
                                item.OwnerUid = uid;
                                item.Uid = Api.UID.NewIuid(Api.MainId, Api.SubId);
                                item.TypeId = int.Parse(id);
                                item.PileNum = 1;
                                item.ActivateState = 1;
                                item.GenerateTime = 0;
                                items.Add(item);
                            }
                        }
                        else
                        {
                            Log.Warn("create player got en error fashion item id {0}", msg.SystemFashionId);
                            return null;
                        }
                    }
                }
                else
                {
                    Log.Warn("create player got en error systemfashion grop id {0}", msg.SystemFashionId);
                    return null;
                }
            }
          
            return items;
        }

        //private int GetInitFaceIcon(MSG_CG_CREATE_CHARACTER msg)
        //{
        //    int systemHeadId = msg.Head;
        //    Data data = DataListManager.inst.GetData("SystemHairStyle", systemHeadId);
        //    if (data != null)
        //    {
        //        //这里和头部形象head 绑定了一个头像faceicon
        //        return data.GetInt("FaceIcon");
        //    }
        //    return 1;
        //}

        /// <summary>
        /// 初始头像框
        /// </summary>
        /// <returns></returns>
        private FaceFrameInfo GetFaceFrame(int uid)
        {
            Data data = DataListManager.inst.GetData("FaceFrame", CharacterInitLibrary.FaceFrame);
            if (data != null && MainType.FaceFrame == (MainType)data.GetInt("MainType"))
            {
                return new FaceFrameInfo
                {
                    OwnerUid = uid,
                    Uid = Api.UID.NewIuid(Api.MainId, Api.SubId),
                    TypeId = CharacterInitLibrary.FaceFrame,
                    PileNum = 1,
                    ActivateState =1,
                    GenerateTime = 0,
                };
            }
            else
            {
                return null;
            }
        }

        private ChatFrameInfo GetChatFrames(int uid)
        {
            Data data = DataListManager.inst.GetData("ChatBubble", CharacterInitLibrary.ChatFrame);
            if (data != null && MainType.ChatFrame == (MainType)data.GetInt("MainType"))
            {
                return new ChatFrameInfo
                {
                    OwnerUid = uid,
                    Uid = Api.UID.NewIuid(Api.MainId, Api.SubId),
                    TypeId = CharacterInitLibrary.ChatFrame,
                    PileNum = 1,
                    ActivateState = 1,
                    GenerateTime = 0,
                    NewObtain = 0,
                };
            }
            else
            {
                return null;
            }
        }
    }
}