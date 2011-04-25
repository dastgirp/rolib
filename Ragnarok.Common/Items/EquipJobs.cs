using System;

namespace Ragnarok
{
    [Flags]
    public enum EquipJobs : uint
    {
        Novice = 0x00000001,
        Swordman = 0x00000002,
        Mage = 0x00000004,
        Archer = 0x00000008,
        Acolyte = 0x00000010,
        Merchant = 0x00000020,
        Thief = 0x00000040,
        Knight = 0x00000080,
        Priest = 0x00000100,
        Wizard = 0x00000200,
        Blacksmith = 0x00000400,
        Hunter = 0x00000800,
        Assassin = 0x00001000,
        //Unused = 0x00002000,
        Crusader = 0x00004000,
        Monk = 0x00008000,
        Sage = 0x00010000,
        Rogue = 0x00020000,
        Alchemist = 0x00040000,
        Bard_Dancer = 0x00080000,
        //Unused = 0x00100000,
        Taekwon = 0x00200000,
        StarGladiator = 0x00400000,
        SoulLinker = 0x00800000,
        Gunslinger = 0x01000000,
        Ninja = 0x02000000,
        AllClasses = 0xFFFFFFFF,
        AllExceptNovice = 0xFFFFFFFE
    }
}
