using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator
{
    internal class StealthRecall
    {
        public static Dictionary<string, SupportedChamps> SupportedDictionary =
            new Dictionary<string, SupportedChamps>();

        public static Menu Menu;
        public static SupportedChamps Data;

        static StealthRecall()
        {
            SupportedDictionary.Add("Akali", new SupportedChamps(SpellSlot.W, true));
            SupportedDictionary.Add("Shaco", new SupportedChamps(SpellSlot.Q));
            SupportedDictionary.Add("Twitch", new SupportedChamps(SpellSlot.Q));

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Menu == null || !Menu.Item("Enabled").GetValue<bool>() || !Menu.Item("Key").GetValue<KeyBind>().Active ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Recall).State != SpellState.Ready ||
                ObjectManager.Player.Spellbook.GetSpell(Data.Slot).State != SpellState.Ready)
            {
                return;
            }

            ObjectManager.Player.Spellbook.CastSpell(Data.Slot, ObjectManager.Player.Position);
            ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
        }

        public static void AddToMenu(Menu menu)
        {
            if (!SupportedDictionary.ContainsKey(ObjectManager.Player.ChampionName) ||
                (SupportedDictionary[ObjectManager.Player.ChampionName].RequiresImprovedRecall &&
                 !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Recall).Name.Contains("Improved")))
            {
                return;
            }

            Data = SupportedDictionary[ObjectManager.Player.ChampionName];

            Menu = new Menu("Stealth Recall", "StealthRecall");
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Key", "Key").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press)));
            menu.AddSubMenu(Menu);
        }
    }

    internal class SupportedChamps
    {
        public bool RequiresImprovedRecall;
        public SpellSlot Slot;

        public SupportedChamps(SpellSlot slot, bool requiresImprovedRecall = false)
        {
            Slot = slot;
            RequiresImprovedRecall = requiresImprovedRecall;
        }
    }
}