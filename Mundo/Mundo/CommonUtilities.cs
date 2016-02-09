using LeagueSharp;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Mundo
{
    internal class CommonUtilities
    {
        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool CheckItem()
        {
            return ItemData.Tiamat_Melee_Only.GetItem().IsReady() ||
                   ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady() ||
                   ItemData.Titanic_Hydra_Melee_Only.GetItem().IsReady();
        }

        public static void UseItem()
        {
            if (!CheckItem())
                return;

            if (ItemData.Tiamat_Melee_Only.GetItem().IsOwned(Player))
            {
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            }
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsOwned(Player))
            {
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
            if (ItemData.Titanic_Hydra_Melee_Only.GetItem().IsOwned(Player))
            {
                ItemData.Titanic_Hydra_Melee_Only.GetItem().Cast();
            }
        }
    }
}
