using System;

namespace Ragnarok
{
    [Flags]
    public enum EquipLocation
    {
        LowerHeadgear = 1,
        RightHand = 2,
        Mantle = 4,
        Accessory1 = 8,
        Armor = 16,
        LeftHand = 32,
        Shoes = 64,
        Accessory2 = 128,
        UpperHeadgear = 256,
        MiddleHeadgear = 512,
        Arrow = 32768
    }
}
