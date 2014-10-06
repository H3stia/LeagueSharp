using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

/*
 * ToDo:
 * 
 * */


namespace LeeSinSharp
{
    internal class LeeSinSharp
    {
        public static string[] testSpells = { "RelicSmallLantern", "RelicLantern", "SightWard", "wrigglelantern", "ItemGhostWard", "VisionWard",
                                     "BantamTrap", "JackInTheBox","CaitlynYordleTrap", "Bushwhack"};


        

        public const string CharName = "LeeSin";

        public static Menu Config;

        public static Map map;

        public static Obj_AI_Hero target;

        public LeeSinSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName) return;
            map = new Map();

            Game.PrintChat("LeeSin");

            try
            {

                Config = new Menu("LeeSin", "LeeSin", true);
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                SimpleTs.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                LeeSin.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));


                Config.AddSubMenu(new Menu("Insec", "Insec"));
                Config.SubMenu("Insec").AddItem(new MenuItem("ActiveInsec", "Insec!").SetValue((new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false))));


                Config.AddSubMenu(new Menu("WardJump", "WardJump"));
                Config.SubMenu("WardJump").AddItem(new MenuItem("ActiveWard", "WardJump!").SetValue((new KeyBind("G".ToCharArray()[0], KeyBindType.Press, false))));
                
                Config.AddSubMenu(new Menu("Drawings", "Drawings"));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawInsec", "Draw Insec")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
                Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

                LeeSin.setSkillShots();
            }
            catch
            {
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            LeeSin.loaidraw();
            target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);
            LeeSin.checkLock(target);
            LeeSin.orbwalker.SetAttacks(true);
            if (Config.Item("ActiveWard").GetValue<KeyBind>().Active)
            {
                LeeSin.wardJump(Game.CursorPos.To2D());
            }

            if (Config.Item("ActiveInsec").GetValue<KeyBind>().Active)
            {
                LeeSin.useinsec();
            }

            if (LeeSin.orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                
            }
        }

        private static void onDraw(EventArgs args)
        {
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                Utility.DrawCircle(ObjectManager.Player.Position, 1000, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value,
                    Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawW").GetValue<bool>())
            {
                Utility.DrawCircle(ObjectManager.Player.Position, 700, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value,
                    Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawE").GetValue<bool>())
            {
                Utility.DrawCircle(ObjectManager.Player.Position, 350, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value,
                    Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawR").GetValue<bool>())
            {
                Utility.DrawCircle(ObjectManager.Player.Position, 375, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value,
                    Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawInsec").GetValue<bool>() && LeeSin.R.IsReady())
            {
                if (!LeeSin.loaidraw())
                {
                    Vector2 heroPos = Drawing.WorldToScreen(LeeSin.LockedTarget.Position);
                    Vector2 diempos = Drawing.WorldToScreen(LeeSin.getward1(LeeSin.LockedTarget));
                    Drawing.DrawLine(heroPos[0], heroPos[1], diempos[0], diempos[1], 1, System.Drawing.Color.White);
                }
                else
                {
                    Vector2 heroPos = Drawing.WorldToScreen(LeeSin.LockedTarget.Position);
                    Vector2 diempos = Drawing.WorldToScreen(LeeSin.getward3(LeeSin.LockedTarget));
                    Drawing.DrawLine(heroPos[0], heroPos[1], diempos[0], diempos[1], 1, System.Drawing.Color.White);
                }
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Missile") || sender.Name.Contains("Minion"))
                return;
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
          
        }

        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            if (testSpells.ToList().Contains(arg.SData.Name))
            {
                LeeSin.testSpellCast = arg.End.To2D();
                Polygon pol;
                if ((pol = map.getInWhichPolygon(arg.End.To2D())) != null)
                {
                    LeeSin.testSpellProj = pol.getProjOnPolygon(arg.End.To2D());
                }
            }
        }




    }
}
