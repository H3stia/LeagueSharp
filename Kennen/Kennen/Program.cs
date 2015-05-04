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
        public static Obj_AI_Hero Player;

        //Menu
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        //Ignite
        public static SpellDataInst Ignite;

        #region Main

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion

        #region GameLoad

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals("Kennen"))
                return;


            #region Spell Data

            Q = new Spell(SpellSlot.Q, 1050);
            Q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 800);

            R = new Spell(SpellSlot.R, 550);

            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            #endregion


            #region Config Menu

            //Menu
            Config = new Menu("Kennen - The ThunderRat", "Kennen", true);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Combo Menu
            var combo = Config.AddSubMenu(new Menu("Combo Settings", "Combo"));

            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboQ.AddItem(
                new MenuItem("qHitchance", "Q Hitchance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 2)));

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
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 2)));

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

            Notifications.AddNotification("Kennen by Hestia loaded!", 5000);
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        #endregion

        #region HitChance Selector

        //Hitchance
        private static HitChance GetHitChance(string name)
        {
            var hc = Config.Item(name).GetValue<StringList>();
            switch (hc.SList[hc.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "Very High":
                    return HitChance.VeryHigh;
            }
            return HitChance.High;
        }

        #endregion

        #region Combo

        private static void ExecuteCombo()
        {
            if (!Config.Item("ComboActive").GetValue<KeyBind>().Active)
                return;

            if (Target.IsValidTarget(1300))
                Combo();
        }

        //Combo
        private static void Combo()
        {
            var castQ = Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady();
            var castW = Config.Item("UseWCombo").GetValue<bool>() && W.IsReady();
            var castR = Config.Item("UseRCombo").GetValue<bool>() && R.IsReady();

            if (castQ && Q.IsInRange(Target, 1000))
            {
                Q.CastIfHitchanceEquals(Target, GetHitChance("qHitchance"));
            }

            var modeW = Config.Item("UseWmodeC").GetValue<StringList>();
            if (castW && W.IsInRange(Target, W.Range))
            {
                if (modeW.SelectedIndex == 0)
                    foreach (var buff in Target.Buffs)
                    {
                        if (buff.Name == "kennenmarkofstorm")
                            W.Cast();
                    }
                else if (modeW.SelectedIndex == 1)
                {
                    foreach (var buff in Target.Buffs)
                    {
                        if (buff.Name == "kennenmarkofstorm" && buff.Count == 2)
                            W.Cast();
                    }
                }
            }

            if (castR && R.IsInRange(Target, R.Range))
            {
                CastSmartR();
            }
        }

        //Ulti if killable
        private static void CastSmartR()
        {
            if (Target.Health < GetComboDamage(Target) && Player.Distance(Target) < 500)
                R.Cast();
        }

        //Calculate Combo Damage
        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var comboDamage = 0d;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (Ignite.IsReady())
                comboDamage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float) comboDamage;
        }

        #endregion

        #region Harass

        private static void ExecuteHarass()
        {
            if (!Config.Item("HarassActive").GetValue<KeyBind>().Active)
                return;

            if (Target.IsValidTarget(1300))
                Harass();
        }

        //Harass
        private static void Harass()
        {
            var castQ = Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady();
            var castW = Config.Item("UseWHarass").GetValue<bool>() && W.IsReady();

            if (castQ && Q.IsInRange(Target, 1000))
            {
                Q.CastIfHitchanceEquals(Target, GetHitChance("qHitchanceH"));
            }

            var modeW = Config.Item("UseWmodeH").GetValue<StringList>();
            if (castW && W.IsInRange(Target, W.Range))
            {
                if (modeW.SelectedIndex == 0)
                    foreach (var buff in Target.Buffs)
                    {
                        if (buff.Name == "kennenmarkofstorm")
                            W.Cast();
                    }
                else if (modeW.SelectedIndex == 1)
                {
                    foreach (var buff in Target.Buffs)
                    {
                        if (buff.Name == "kennenmarkofstorm" && buff.Count == 2)
                            W.Cast();
                    }
                }
            }
        }

        #endregion

        #region KillSteal

        //KillSteal
        private static void KillSteal()
        {
            if (!Config.Item("Killsteal").GetValue<bool>())
                return;

            Qks();
            Wks();
            Iks();
        }

        //Q Killsteal
        private static void Qks()
        {
            //Q killsteal
            if (!Config.Item("UseQKS").GetValue<bool>())
                return;

            var unit = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault( obj => obj.IsValidTarget(Q.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            if (!unit.IsValidTarget(Q.Range))
                return;

            Q.Cast(unit);
        }

        //W Killsteal
        private static void Wks()
        {
            //W killsteal
            if (!Config.Item("UseWKS").GetValue<bool>())
                return;

            var unit = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(obj => obj.IsValidTarget(W.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.W));

            if (!unit.IsValidTarget(W.Range))
                return;

            W.Cast();
        }

        //Ignite killsteal
        private static void Iks()
        {
            //Ignite
            if (!Config.Item("UseIKS").GetValue<bool>() || Ignite == null || Ignite.Slot == SpellSlot.Unknown ||
                !Ignite.Slot.IsReady())
                return;

            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj =>
                            obj.IsValidTarget(600) &&
                            obj.Health < Player.GetSummonerSpellDamage(obj, Damage.SummonerSpell.Ignite));
            if (unit.IsValidTarget(600))
            {
                Player.Spellbook.CastSpell(Ignite.Slot, unit);
            }
        }

        #endregion

        #region MultiR

        //auto ult for X enemies in range
        private static void CastRmulti()
        {
            if (!Config.Item("ComboActive").GetValue<bool>())
                return;

            var castR = Config.Item("UseRmul").GetValue<bool>() && R.IsReady();
            var minR = Config.Item("UseRmulti").GetValue<Slider>().Value;
            var hits = Player.CountEnemiesInRange(R.Range);
            if (castR && R.IsInRange(Target, R.Range) && hits >= minR)
                R.Cast();
        }

        #endregion

        #region Events

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            KillSteal();

            Target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);

            if (!Target.IsValidTarget(1300))
                return;

            ExecuteCombo();
            ExecuteHarass();
            CastRmulti();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var circle in new List<string> { "Q", "W", "R" }.Select(spell => Config.Item("Draw" + spell).GetValue<Circle>()).Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.ServerPosition, circle.Radius, circle.Color);
            }
        }

        #endregion

    }
}
