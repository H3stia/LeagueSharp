using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
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
            //Config.SubMenu("Combo").AddItem(new MenuItem("UseRmulti", "Use R on min X targets").SetValue(new Slider(2, 0, 5))); //0 to deactivate
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            
            //Harass Menu
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("qHitchanceH", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2)));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(new StringList(new [] { "Always", "Only Stunnable" })));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "Harass (toggle)!").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            
            //Killsteal Menu
            Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseWKS", "Use W to KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            //Drawings Menu
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "W range").SetValue(false));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(false));
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

            /* TODO
            CastRmulti();
            */

            if ((Config.Item("HarassActive").GetValue<KeyBind>().Active) || (Config.Item("HarassActiveT").GetValue<KeyBind>().Active))
                Harass();
             
        }

        public static void CastQ() //Q casting 
        {
            if (!Q.IsReady())
                return;

            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var useQ = Config.Item("UseQCombo").GetValue<bool>();

            var qLow = Config.Item("qHitchance").GetValue<StringList>().SelectedIndex == 0;
            var qMedium = Config.Item("qHitchance").GetValue<StringList>().SelectedIndex == 1;
            var qHigh = Config.Item("qHitchance").GetValue<StringList>().SelectedIndex == 2;
            var qVeryHigh = Config.Item("qHitchance").GetValue<StringList>().SelectedIndex == 3;

            if (qTarget != null && useQ)
            {
                if (qLow)
                    Q.Cast(qTarget);
                else if (qMedium)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                else if (qHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.High);
                else if (qVeryHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
            }
        }

        public static void CastQharass() //Q casting in harass
        {
            if (!Q.IsReady())
                return;

            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var useQ = Config.Item("UseQHarass").GetValue<bool>();

            var qLow = Config.Item("qHitchanceH").GetValue<StringList>().SelectedIndex == 0;
            var qMedium = Config.Item("qHitchanceH").GetValue<StringList>().SelectedIndex == 1;
            var qHigh = Config.Item("qHitchanceH").GetValue<StringList>().SelectedIndex == 2;
            var qVeryHigh = Config.Item("qHitchanceH").GetValue<StringList>().SelectedIndex == 3;

            if (qTarget != null && useQ)
            {
                if (qLow)
                    Q.Cast(qTarget);
                else if (qMedium)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                else if (qHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.High);
                else if (qVeryHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
            }
        }

        public static void CastW()
        {
            if (!W.IsReady())
                return;
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var wMode1 = Config.Item("UseWCombo").GetValue<StringList>().SelectedIndex == 0;
            //var wMode2 = Config.Item("UseWCombo").GetValue<StringList>().SelectedIndex == 1;

            if (wTarget != null)
            {
                if (wMode1)
                    W.Cast();
                /*
                  if (wMode2)
                        foreach (var buff in wTarget.Buffs)
                        {
                            if (buff.Name == "" && buff.Count == 2) //missing kennen debuff name
                                _w.Cast();
                        }
                       
                     */
            }
        }

        public static void CastWharass()
        {
            if (!W.IsReady())
                return;
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var wMode1 = Config.Item("UseWHarass").GetValue<StringList>().SelectedIndex == 0;
            //var wMode2 = Config.Item("UseWHarass").GetValue<StringList>().SelectedIndex == 1;

            if (wTarget != null)
            {
                if (wMode1)
                    W.Cast();
                /*
                  if (wMode2)
                        foreach (var buff in wTarget.Buffs)
                        {
                            if (buff.Name == "" && buff.Count == 2) //missing kennen debuff name
                                _w.Cast();
                        }
                       
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
            //if (rTarget != null && useR && GetComboDamage() > rTarget.Health)
                //R.Cast();
            if (rTarget != null && useR && Player.GetSpellDamage(rTarget, SpellSlot.R) > rTarget.Health)
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

        /* TODO
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
        */
        public static void Combo()
        {
            CastQ();
            CastW();
            CastR();
        }

        public static void KillSteal()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var iTarget = SimpleTs.GetTarget(600, SimpleTs.DamageType.True);

            var useQks = Config.Item("UseQKS").GetValue<bool>();
            var useWks = Config.Item("UseWKS").GetValue<bool>();
            var useIks = Config.Item("UseIKS").GetValue<bool>();

            if (Q.IsReady() && useQks && Player.GetSpellDamage(qTarget, SpellSlot.Q) > qTarget.Health)
                Q.Cast(qTarget);

            if (W.IsReady() && useWks && Player.GetSpellDamage(wTarget, SpellSlot.W) > wTarget.Health)
                W.Cast(wTarget);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && useIks)
            {
                if (Player.GetSummonerSpellDamage(iTarget, Damage.SummonerSpell.Ignite) > iTarget.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, iTarget);
                }
            }
        }

        public static void Harass()
        {
            CastQharass();
            CastWharass();
        }
       
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) 
                return;
            if (Config.Item("QRange").GetValue<bool>() && Q.Level > 0)
                Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.CornflowerBlue : Color.OrangeRed);
            if (Config.Item("WRange").GetValue<bool>() && W.Level > 0)
                Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.CornflowerBlue : Color.OrangeRed);
            if (Config.Item("RRange").GetValue<bool>() && R.Level > 0)
                Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.CornflowerBlue : Color.OrangeRed);
        }
    }
}
