using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator
{
    class AutoPotion
    {
        readonly static Obj_AI_Hero Player = ObjectManager.Player;
        
        private enum PotionType {
            Health = 2003,
            Mana = 2004,
            Biscuit = 2009,
            CrystalFlask = 2041,
        }

        static AutoPotion()
        {
            Game.OnGameUpdate += OnGameUpdate;
        }

        public static void AddToMenu(Menu menu)
        {
            var potionsMenu = new Menu("Auto Potion", "AutoPotion");

            potionsMenu.AddItem(new MenuItem("EnableAutoPotion", "Enabled").SetValue(true));
            potionsMenu.AddItem(new MenuItem("HpPct", "Health %").SetValue(new Slider(40)));
            potionsMenu.AddItem(new MenuItem("MnPct", "Mana %").SetValue(new Slider(20)));

            menu.AddSubMenu(potionsMenu);
        }

        private static bool Disallowed()
        {
            return (Player.HasBuff("Recall") || Utility.InFountain()); 
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (!Config.Menu.Item("EnableAutoPotion").GetValue<bool>())
                return;

            // Default Potions
            var hpPct = Config.Menu.Item("HpPct").GetValue<Slider>().Value / 100f;
            var mnPct = Config.Menu.Item("MnPct").GetValue<Slider>().Value / 100f;

            // Flask
            if (Player.HasBuff("ItemCrystalFlask"))
                return;

            // Health Potion
            if (Player.Health / Player.MaxHealth <= hpPct && !Player.HasBuff("Health Potion") && !Disallowed())
                CastPotion(PotionType.Health);

            // Mana Potion
            if (Player.Mana / Player.MaxMana <= mnPct && !Player.HasBuff("Mana Potion") && !Disallowed())
                CastPotion(PotionType.Mana);
        }

        private static void CastPotion(PotionType type)
        {
            Player.InventoryItems.First(item => item.Id == (type == PotionType.Health ? (ItemId)2003 : (ItemId)2004) || (item.Id == (ItemId)2010) || (item.Id == (ItemId)2041 && item.Charges > 0)).UseItem();
        }
    }
}
