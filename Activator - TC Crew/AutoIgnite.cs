#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Activator
{
    internal class AutoIgnite
    {
        public const string Ignite = "SummonerDoT";
        public const string Heal = "SummonerHeal";
        public const string Barrier = "SummonerBarrier";
        public static SpellSlot IgniteSlot = SpellSlot.Unknown;
        public static Menu IgniteMenu;

        static AutoIgnite()
        {
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDoT");
            if (IgniteSlot == SpellSlot.Unknown)
            {
                return;
            }

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        public static void AddToMenu(Menu menu)
        {
            if (IgniteSlot == SpellSlot.Unknown)
            {
                return;
            }

            IgniteMenu = new Menu("Auto Ignite", "AutoIgnite");
            IgniteMenu.AddItem(new MenuItem("EnableAutoIgnite", "Enabled").SetValue(true));
            IgniteMenu.AddItem(new MenuItem("Barrier", "Ignite Enemies with Barrier").SetValue(true));
            IgniteMenu.AddItem(new MenuItem("Heal", "IgniteEnemies with Heal").SetValue(true));

            menu.AddSubMenu(IgniteMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var hero in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        hero =>
                            hero.IsValidTarget(600) &&
                            hero.Health < ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) &&
                            (IgniteMenu.Item("Heal").GetValue<bool>() || !HasSummoner(hero, Heal)) &&
                            (IgniteMenu.Item("Barrier").GetValue<bool>() || !HasSummoner(hero, Barrier))))
            {
                CastIgnite(hero);
            }
        }

        private static void CastIgnite(GameObject unit)
        {
            if (CanIgnite())
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, unit);
            }
        }

        private static bool CanIgnite()
        {
            return IgniteSlot != SpellSlot.Unknown &&
                   ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;
        }

        private static bool HasSummoner(Obj_AI_Hero enemy, string name)
        {
            var slot = enemy.GetSpellSlot(name);
            return slot != SpellSlot.Unknown && enemy.SummonerSpellbook.GetSpell(slot).State == SpellState.Ready;
        }
    }
}