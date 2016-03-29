using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Kennen
{
    internal class CommonUtilities : Spells
    {
        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static HitChance GetHitChance(string name)
        {
            var hitChance = ConfigMenu.config.Item(name).GetValue<StringList>();

            switch (hitChance.SList[hitChance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "Very High":
                    return HitChance.VeryHigh;
            }
            return HitChance.VeryHigh;
        }

        public static bool CheckItem()
        {
            return ItemData.Wooglets_Witchcap.GetItem().IsReady() ||
                   ItemData.Zhonyas_Hourglass.GetItem().IsReady();
        }

        public static void UseItem()
        {
            if (!CheckItem())
                return;

            if (ItemData.Wooglets_Witchcap.GetItem().IsOwned(Player))
            {
                ItemData.Wooglets_Witchcap.GetItem().Cast();
            }
            if (ItemData.Zhonyas_Hourglass.GetItem().IsOwned(Player))
            {
                ItemData.Zhonyas_Hourglass.GetItem().Cast();
            }
        }

        public static float GetComboDamage(Obj_AI_Base target)
        {
            var comboDamage = 0d;

            if (q.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (w.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (r.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (ignite.IsReady())
            {
                comboDamage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }
                
            return (float) comboDamage;
        }
    }
}
