using TinkersToolbox.Client.Mesh;
using Vintagestory.Client.NoObf;

namespace TinkersToolbox.Client.Systems
{
    class SystemObjectLifetime : ClientSystem
    {
        public override string Name => "SystemObjectLifetime";

        private ClientMain game;

        public SystemObjectLifetime(ClientMain game) : base(game)
        {
            this.game = game;
            game.RegisterGameTickListener(new Vintagestory.API.Common.Action<float>(this.ClearLifetimeObjects), 60000);
        }

        public override EnumClientSystemType GetSystemType()
        {
            return EnumClientSystemType.Misc;
        }

        public void ClearLifetimeObjects(float dt)
        {
            var deletedCount = MeshManager.ClearOldEntries(game, 300);

            game.Logger.Debug("[SystemObjectLifetime] Cleared {0} model entries.", deletedCount);
        }
    }
}
