using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class ChatFrameItem : BaseItem
    {
        public int ActivateState { get; set; }//激活状态

        private ChatFrameInfo chatFrameInfo;
        public ChatFrameInfo ChatFrameInfo { get { return chatFrameInfo; } }

        private ChatFrameModel model;
        public ChatFrameModel Model { get { return model; } }

        public int NewObtain { get; set; }//是否是新获得的，用于前端红点显示

        public ChatFrameItem(ChatFrameInfo chatFrameInfo) : base(chatFrameInfo)
        {
            this.chatFrameInfo = chatFrameInfo;
            //激活状态
            this.ActivateState = chatFrameInfo.ActivateState;
            this.MainType = MainType.ChatFrame;
            this.BindData(chatFrameInfo.TypeId);
            this.NewObtain = chatFrameInfo.NewObtain;
        }

        public ChatFrameItem(ChatFrameInfo chatFrameInfo, ChatFrameModel model) : base(chatFrameInfo)
        {
            this.MainType = MainType.ChatFrame;
            this.chatFrameInfo = chatFrameInfo;
            this.model = model;
            this.NewObtain = chatFrameInfo.NewObtain;
        }

        public override bool BindData(int id)
        {
            this.model = ChatLibrary.GetChatFrameModel(id);
            if (this.model != null)
            {
                //TODO 绑定差异数据
                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this chatframe model id {id}");
                return false;
            }
        }

        public ITEM GenerateSyncMessage()
        {
            ITEM syncMsg = new ITEM()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = this.ActivateState,
                GenerateTime = this.GenerateTime,
            };
            return syncMsg;
        }

        public ZMZ_ITEM GenerateTransformMessage()
        {
            ZMZ_ITEM syncMsg = new ZMZ_ITEM()
            {
                Uid = this.Uid,
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = this.ActivateState,
                GenerateTime = this.GenerateTime,
                NewObtain = this.NewObtain,
            };
            return syncMsg;
        }

        public void UpdateGenerateTime()
        {
            GenerateTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            chatFrameInfo.GenerateTime = GenerateTime;
        }
    }
}
