using VRage.Game.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private long ResolveIdentityId(long playerId)
        {
            return playerIdentityAdapter.ResolveIdentityId(playerId);
        }

        private long ResolveIdentityId(ulong sender)
        {
            return playerIdentityAdapter.ResolveIdentityId(sender);
        }

        private long ResolveIdentityId(IMyCharacter character)
        {
            return playerIdentityAdapter.ResolveIdentityId(character);
        }

        private IMyPlayer FindPlayerByIdentity(long identityId)
        {
            return playerIdentityAdapter.FindPlayerByIdentity(identityId);
        }

        private long ResolveShipToolOperator(IMyCubeBlock toolBlock)
        {
            return playerIdentityAdapter.ResolveShipToolOperator(toolBlock);
        }
    }
}
