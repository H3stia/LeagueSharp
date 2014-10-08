using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


namespace Kennen
{
    class Program
    {
        public const string ChampionName = "Kennen";


        public static readonly List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        public static Obj_AI_Hero Player;
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        public static SpellSlot IgniteSlot;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName) return;

            Game.PrintChat("Hestia Kennen loaded. Enjoy!");

            //Spells
            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            //Menu
            Config = new Menu("Kennen", "Kennen", true);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("qHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2)));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(new StringList(new[] {"Always", "Only Stunnable"}))); //TODO on stunnable
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use smart R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRmulti", "Use R on min X targets").SetValue(new Slider(2, 0, 5))); //0 to deactivate
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            /*
            //Harass Menu
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(new StringList(new string[] { "Always", "Only Stunnable" })));
            _config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "Harass (toggle)!").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            */
            //Killsteal Menu
            Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseWKS", "Use W to KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            //Drawings Menu
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();

            if (Config.Item("Killsteal").GetValue<bool>())
                KillSteal();

            CastRmulti();
            /* TODO
            if ((_config.Item("HarassActive").GetValue<KeyBind>().Active) || (_config.Item("HarassActiveT").GetValue<KeyBind>().Active))
                Harass();
             * */
        }

        public static void CastQ() //Q casting 
        {
            if (!Q.IsReady())
                return;
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var qMode = Config.Item("qHitchance").GetValue<StringList>().SelectedIndex;
            if (qTarget != null && useQ)
            {
                switch (qMode)
                {
                    case 0://low
                        Q.CastIfHitchanceEquals(qTarget, HitChance.Low);
                        break;
                    case 1://medium
                        Q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                        break;
                    case 2://hgih
                        Q.CastIfHitchanceEquals(qTarget, HitChance.High);
                        break;
                    case 3://very high
                        Q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
                        break;
                }
            }
        }

        public static void CastW()
        {
            if (!W.IsReady())
                return;
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var wMode = Config.Item("UseWCombo").GetValue<StringList>().SelectedIndex;
            if (wTarget != null)
                switch (wMode)
                {
                    case 0://always
                        W.Cast();
                        break;
                    /*
                    case 1: //only stunnable 
                        foreach (var buff in wTarget.Buffs)
                        {
                            if (buff.Name == "" && buff.Count == 2) //missing kennen debuff name
                                _w.Cast();
                        }
                        break;
                     */
                }
        }

        //ulti if killable
        public static void CastR()
        {
            if (!R.IsReady())
                return;
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            if (rTarget != null && useR && GetComboDamage() > rTarget.Health)
                R.Cast();
        }

        public static float GetComboDamage()
        {
            var comboDamage = 0d;

            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (Q.IsReady() && qTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.Q);

            if (W.IsReady() && wTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.W);

            if (R.IsReady() && rTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.R);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                comboDamage += Player.GetSummonerSpellDamage(qTarget, Damage.SummonerSpell.Ignite);

            return (float)comboDamage;
        }

        //auto ult for X enemies in range, 0 to deactivate
        public static void CastRmulti()
        {
            if (!R.IsReady())
                return;

            //Thanks to xSalice
            int hit = 0;
            var minHit = Config.Item("UseRmulti").GetValue<Slider>().Value;
            if (minHit == 0)
                return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                if (enemy != null && !enemy.IsDead)
                {
                    if (Player.Distance(enemy) < R.Range)
                        hit++;
                }
            }

            if (hit >= minHit)
                R.Cast();

        }

        public static void Combo()
        {
            CastQ();
            CastW();
            CastR();
        }

        public static void KillSteal()
        {
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (Config.Item("UseQKS").GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && (Player.Distance(target) < Q.Range) && Q.IsReady())
                    Q.Cast(target);

                if (W.IsReady() && Config.Item("UseWKS").GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.W) > target.Health && (Player.Distance(target) < W.Range) && W.IsReady())
                    W.Cast();

                if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Player.Distance(target) < 600 && Config.Item("UseIKS").GetValue<bool>())
                {
                    if (Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }
            }
        }
        /* TODO
        public static void Harass()
        {

        }
        */
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (SpellList == null) return;

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();

                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
    }
}
