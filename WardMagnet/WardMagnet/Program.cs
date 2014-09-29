using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Toolkit.Diagnostics;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace WardMagnet
{
    static class Menu
    {
        public class MenuItemSettings
        {
            public String Name;
            public dynamic Item;
            public Type Type;
            public bool ForceDisable;
            public LeagueSharp.Common.Menu Menu;
            public List<MenuItemSettings> SubMenus = new List<MenuItemSettings>();
            public List<MenuItem> MenuItems = new List<MenuItem>();

            public MenuItemSettings(Type type, dynamic item)
            {
                Type = type;
                Item = item;
            }

            public MenuItemSettings(dynamic item)
            {
                Item = item;
            }

            public MenuItemSettings(Type type)
            {
                Type = type;
            }

            public MenuItemSettings(String name)
            {
                Name = name;
            }

            public MenuItemSettings()
            {

            }

            public MenuItemSettings AddMenuItemSettings(String displayName, String name)
            {
                SubMenus.Add(new Menu.MenuItemSettings(name));
                MenuItemSettings tempSettings = GetMenuSettings(name);
                if (tempSettings == null)
                {
                    throw new NullReferenceException(name + " not found");
                }
                tempSettings.Menu = Menu.AddSubMenu(new LeagueSharp.Common.Menu(displayName, name));
                return tempSettings;
            }

            public bool GetActive()
            {
                if (Menu == null)
                    return false;
                foreach (var item in Menu.Items)
                {
                    if (item.DisplayName == "Active")
                    {
                        if (item.GetValue<bool>())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }

            public void SetActive(bool active)
            {
                if (Menu == null)
                    return;
                foreach (var item in Menu.Items)
                {
                    if (item.DisplayName == "Active")
                    {
                        item.SetValue(active);
                        return;
                    }
                }
            }

            public MenuItem GetMenuItem(String menuName)
            {
                if (Menu == null)
                    return null;
                foreach (var item in Menu.Items)
                {
                    if (item.Name == menuName)
                    {
                        return item;
                    }
                }
                return null;
            }

            public LeagueSharp.Common.Menu GetSubMenu(String menuName)
            {
                if (Menu == null)
                    return null;
                return Menu.SubMenu(menuName);
            }

            public MenuItemSettings GetMenuSettings(String name)
            {
                foreach (var menu in SubMenus)
                {
                    if (menu.Name.Contains(name))
                        return menu;
                }
                return null;
            }
        }
        public static MenuItemSettings Wards = new MenuItemSettings();
        public static MenuItemSettings WardCorrector = new MenuItemSettings(typeof(WardMagnet.WardCorrector));
    }

    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void CreateMenu()
        {
            {
                LeagueSharp.Common.Menu menu = new LeagueSharp.Common.Menu("Ward Magnet", "WardMagnet", true);


                Menu.Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ward Magnet", "WardMagnet"));
                Menu.WardCorrector.Menu = Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WardCorrector", "WardCorrector"));
                Menu.WardCorrector.MenuItems.Add(Menu.WardCorrector.Menu.AddItem(new LeagueSharp.Common.MenuItem("WardCorrectorActive", "Active").SetValue(true)));

                menu.AddItem(new LeagueSharp.Common.MenuItem("By Screeder", "By Screeder "));
                menu.AddToMainMenu();
            }
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                CreateMenu();
                Game.PrintChat("Ward Magnet loaded!");
                Game.OnGameUpdate += GameOnOnGameUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("Ward Magnet: " + e.ToString());
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            Type classType = typeof(Menu);
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = classType.GetFields(flags);
            foreach (FieldInfo p in fields)
            {
                var item = (Menu.MenuItemSettings)p.GetValue(null);
                if (item.GetActive() == false && item.Item != null)
                {
                    item.Item = null;
                }
                else if (item.GetActive() && item.Item == null && !item.ForceDisable && item.Type != null)
                {
                    try
                    {
                        item.Item = System.Activator.CreateInstance(item.Type);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }
}

