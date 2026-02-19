using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 某个Hero所有魂骨达到某品质
    /// model param1 quality
    /// </summary>
    public class GotQualitySoulBoneAction : BaseAction
    {
        public GotQualitySoulBoneAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            SoulBoneItem soulBoneItem = obj as SoulBoneItem;
            if (soulBoneItem == null) return false;

            if (soulBoneItem.Bone.Prefix >= model.Param1)
            {
                if (!CheckActionBySdk(model.Param1))
                {
                    //所有魂环都满足
                    AddNum(model.Param1, true);
                    return true;
                }
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            AddNum(model.Param1, true);
        }
    }
}
