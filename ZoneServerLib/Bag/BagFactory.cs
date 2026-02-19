using EnumerateUtility;

namespace ZoneServerLib
{
    public static class BagFactory
    {
        public static BaseBag CreatateBag(BagType bagType, BagManager manager)
        {
            switch (bagType)
            {
                case BagType.Normal:
                    return new Bag_Normal(bagType, manager);
                case BagType.SoulBone:
                    return new Bag_SoulBone(manager, bagType);
                case BagType.Equip:
                    return new Bag_Equip(bagType, manager);
                case BagType.SoulRing:
                    return new Bag_SoulRing(bagType, manager);
                case BagType.ChatFrame:
                    return new Bag_ChatFrame(bagType, manager);
                case BagType.FaceFrame:
                    return new Bag_FaceFrame(bagType, manager);
                case BagType.Fashion:
                    return new Bag_Fashion(bagType, manager);
                case BagType.HeroFragment:
                    return new Bag_HeroFragment(bagType, manager);
                case BagType.HiddenWeapon:
                    return new Bag_HiddenWeapon(bagType, manager);
                default:
                    return null;
            }
        }

    }
}
