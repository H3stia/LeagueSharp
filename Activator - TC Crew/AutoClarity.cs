using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator
{
    internal class AutoClarity
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static SpellSlot ClaritySlot = SpellSlot.Unknown;
        public static Menu Menu;

        static AutoClarity()
        {
            ClaritySlot = ObjectManager.Player.GetSpellSlot("SummonerClarity");

            if (ClaritySlot == SpellSlot.Unknown)
            {
                return;
            }

            Game.OnGameUpdate += OnGameUpdate;
        }

        public static void AddToMenu(Menu menu)
        {
            if (ClaritySlot == SpellSlot.Unknown)
            {
                return;
            }

            Menu = new Menu("Auto Clarity", "AutoClarity");

            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("MnPct", "Mana %").SetValue(new Slider(20)));

            menu.AddSubMenu(Menu);
        }

        private static bool Disallowed()
        {
            return (!Menu.Item("Enabled").GetValue<bool>() || Player.HasBuff("Recall") || Utility.InFountain() ||
                    Player.Spellbook.GetSpell(ClaritySlot).State != SpellState.Ready);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (!Disallowed() && Player.Mana/Player.MaxMana <= Menu.Item("MnPct").GetValue<Slider>().Value/100f)
            {
                Player.Spellbook.CastSpell(ClaritySlot);
            }
        }
    }
}