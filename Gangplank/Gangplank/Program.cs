using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace Gangplank
{
    class Program
    {
        public static Spell Q, W, E, R;

        public static Obj_AI_Hero Target;
        public static Obj_AI_Hero Player;

        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        public static SpellDataInst Ignite;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region GameLoad

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals("GangPlank"))
                return;

            #region Spell Data

            Q = new Spell(SpellSlot.Q, Q.Range);
            W = new Spell(SpellSlot.W, W.Range);
            E = new Spell(SpellSlot.E, E.Range);
            R = new Spell(SpellSlot.R, R.Range);
            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            #endregion

            //TODO
            #region Menu

            Config = new Menu("GangPlank - The Money Maker", "GangPlank", true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            var combo = Config.AddSubMenu(new Menu("Combo Settings", "Combo"));

            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));

            var comboW = combo.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboW.AddItem(
                new MenuItem("UseWmodeC", "W Mode").SetValue(new StringList(new[] { "Always", "Only Stunnable" })));

            var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddItem(new MenuItem("UseRCombo", "Use smart R").SetValue(true));

            combo.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass Menu
            var harass = Config.AddSubMenu(new Menu("Harass Settings", "Harass"));

            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassQ.AddItem(
                new MenuItem("qHitchanceH", "Q Hitchance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var harassW = harass.AddSubMenu(new Menu("W Settings", "W"));
            harassW.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            harassW.AddItem(
                new MenuItem("UseWmodeH", "W Mode").SetValue(new StringList(new[] { "Always", "Only Stunnable" })));
            harass.AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind((byte) 'C', KeyBindType.Press)));

            //Killsteal Menu
            var killsteal = Config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseWKS", "Use W to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            //Misc Menu
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("UseRmul", "Use R for multiple targets").SetValue(true));
            misc.AddItem(new MenuItem("UseRmulti", "Use R on min X targets").SetValue(new Slider(2, 1, 5)));

            //Drawings Menu
            var drawings = Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawings.AddItem(
                new MenuItem("DrawQ", "Q range").SetValue(new Circle(true, Color.CornflowerBlue, Q.Range)));
            drawings.AddItem(
                new MenuItem("DrawW", "W range").SetValue(new Circle(false, Color.CornflowerBlue, W.Range)));
            drawings.AddItem(
                new MenuItem("DrawR", "R range").SetValue(new Circle(false, Color.CornflowerBlue, R.Range)));

            Config.AddToMainMenu();

            #endregion

            Game.PrintChat(
                "<b><font color =\"#990000\">GangPlank - The Money Maker </font><font color=\"#FFFFFF\">by Hestia loaded!</font>");
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        #endregion

        private static void Game_OnUpdate(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }

        

    }
}
