using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace TinkersToolbox.Types
{
    interface IToolPart : IModularItem
    {
        bool DamagePart(IWorldAccessor world, Entity byEntity, IItemStack itemstack, int amount);

        float GetMiningSpeed(IItemStack itemstack, Block block, IPlayer forPlayer);
    }
}
