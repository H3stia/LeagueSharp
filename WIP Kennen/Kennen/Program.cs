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
        //Spells
        public static Spell Q, W, E, R;

        //Targets
        public static Obj_AI_Hero Target;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        //Menu
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        //Ignite
        public static SpellDataInst Ignite;

        #region Main

        public static void Main(string[] args)
        {
            Utils.ClearConsole();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion


        public static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals("Kennen"))
            {
                return;
            }


            #region Spell Data

            Q = new Spell(SpellSlot.Q, 1050);
            Q.SetSkillshot(0.65f, 50, 1700, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 800);

            R = new Spell(SpellSlot.R, 550);

            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            #endregion


            #region Config Menu

            //Menu
            Config = new Menu("Kennen", "Kennen", true);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("qHitchance", "Q Hitchance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"}, 2)));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("UseWCombo", "Use W").SetValue(new StringList(new[] {"Always", "Only Stunnable"})));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use smart R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRmul", "Use R for multiple targets").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("UseRmulti", "Use R on min X targets").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass Menu
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("qHitchanceH", "Q Hitchance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"}, 2)));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("UseWHarass", "Use W").SetValue(new StringList(new[] {"Always", "Only Stunnable"})));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassToggle", "Harass (toggle)!").SetValue(new KeyBind("L".ToCharArray()[0],
                        KeyBindType.Toggle)));

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

            #endregion


            Game.PrintChat("Hestia Kennen loaded. Enjoy!");
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

            if ((Config.Item("HarassActive").GetValue<KeyBind>().Active) || (Config.Item("HarassActiveT").GetValue<KeyBind>().Active))
                Harass();
             
        }


        public static void CastW()
        {
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var wMode1 = Config.Item("UseWCombo").GetValue<StringList>().SelectedIndex == 0;
            var wMode2 = Config.Item("UseWCombo").GetValue<StringList>().SelectedIndex == 1;

            if (!W.IsReady() && !wTarget.IsValidTarget(W.Range))
                return;

            if (wTarget != null)
            {
                if (wMode1 && wTarget.HasBuff("kennenmarkofstorm", true))
                    W.Cast();
                
                if (wMode2)
                    foreach (var buff in wTarget.Buffs)
                        {
                            if (buff.Name == "kennenmarkofstorm" && buff.Count == 2)
                                W.Cast();
                        }
            }
        }


        //ulti if killable
        public static void CastR()
        {

        }

        public static float GetComboDamage()
        {
            var comboDamage = 0d;

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && qTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.Q);

            if (W.IsReady() && wTarget != null)
                comboDamage += Player.GetSpellDamage(wTarget, SpellSlot.W);

            if (R.IsReady() && rTarget != null)
                comboDamage += Player.GetSpellDamage(rTarget, SpellSlot.R);

            return (float)comboDamage;
        }

        //auto ult for X enemies in range, 0 to deactivate
        public static void CastRmulti()
        {
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (!R.IsReady() && !rTarget.IsValidTarget(R.Range))
                return;
            if (Config.Item("UseRmult").GetValue<bool>() && Player.CountEnemysInRange(R.Range) >= Config.Item("UseRmulti").GetValue<Slider>().Value)
                R.Cast(rTarget);
        }
        
        public static void Combo()
        {
            CastQ();
            CastW();
            CastR();
        }

        public static void KillSteal()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var iTarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);

            var useQks = Config.Item("UseQKS").GetValue<bool>();
            var useWks = Config.Item("UseWKS").GetValue<bool>();
            var useIks = Config.Item("UseIKS").GetValue<bool>();

            if (Q.IsReady() && useQks && Player.GetSpellDamage(qTarget, SpellSlot.Q) > qTarget.Health)
                Q.Cast(qTarget);

            if (W.IsReady() && useWks && Player.GetSpellDamage(wTarget, SpellSlot.W) > wTarget.Health)
                W.Cast(wTarget);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && useIks)
            {
                if (Player.GetSummonerSpellDamage(iTarget, Damage.SummonerSpell.Ignite) > iTarget.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, iTarget);
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
                Drawing.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.CornflowerBlue : Color.OrangeRed);
            if (Config.Item("WRange").GetValue<bool>() && W.Level > 0)
                Drawing.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.CornflowerBlue : Color.OrangeRed);
            if (Config.Item("RRange").GetValue<bool>() && R.Level > 0)
                Drawing.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.CornflowerBlue : Color.OrangeRed);
            
        }
    }
}
