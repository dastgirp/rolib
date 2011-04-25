namespace Ragnarok
{
    public enum AuthorizationLevel
    {
        User = 0,
        SuperUser = 1,
        SuperUserPlus = 10,
        Mediator = 20,
        SubGameMaster = 40,
        SubGameMasterPlus = 50,
        GameMaster = 60,
        HeadGameMaster = 80,
        Administrator = 99
    }
}
