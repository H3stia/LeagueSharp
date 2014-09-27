using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace Jump
{
    internal class AJump
    {
        public static string[] testSpells = { "RelicSmallLantern", "RelicLantern", "SightWard", "wrigglelantern", "ItemGhostWard", "VisionWard",
                                     "BantamTrap", "JackInTheBox","CaitlynYordleTrap", "Bushwhack"};


        

        public const string CharName = "LeeSin";

        public static Menu Config;

        public static Map map;

        public static Obj_AI_Hero target;

        public AJump()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {
            map = new Map();

            Game.PrintChat("Ajumper");

            try
            {

                Config = new Menu("WardJump", "WardJump", true);
                Config.AddItem(new MenuItem("Ward", "Ward Jump")).SetValue(new KeyBind('G', KeyBindType.Press, false));
               
                Config.AddSubMenu(new Menu("Drawings", "Drawings"));
                Config.SubMenu("Drawings").AddItem(new MenuItem("Ward", "Draw Ward")).SetValue(true);


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

            }
            catch
            {
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Ward").GetValue<KeyBind>().Active)
            {
                Jumper.wardJump(Game.CursorPos.To2D());
            }

       }

        private static void onDraw(EventArgs args)
        {
            if(Config.Item("Ward").GetValue<bool>())
                Drawing.DrawCircle(Jumper.Player.Position, 600, Color.Gray); 
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Missile") || sender.Name.Contains("Minion"))
                return;

        }

        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            if (testSpells.ToList().Contains(arg.SData.Name))
            {
                Jumper.testSpellCast = arg.End.To2D();
                Polygon pol;
                if ((pol = map.getInWhichPolygon(arg.End.To2D())) != null)
                {
                    Jumper.testSpellProj = pol.getProjOnPolygon(arg.End.To2D());
                }
            }
        }




    }
}
