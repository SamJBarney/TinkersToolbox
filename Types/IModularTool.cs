using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace TinkersToolbox.Types
{
    interface IModularTool : IModularItem
    {
        float OnBlockBreaking(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter);

        int GetToolTier(IItemStack stack);

        ItemStack GetToolheadStack(IItemStack stack);
    }
}
