using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mundo
{
    class Program
    {

        //Champion
        private const string Champion = "DrMundo";

        //Spells
        private static Spell Q, W, E, R;

        //Player
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //Menu
        private static Menu Config;
        private static Orbwalking.Orbwalker Orbwalker;

        //Ignite
        private static SpellDataInst Ignite;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region GameLoad

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != Champion)
                return;

            #region Spells

            Q = new Spell(SpellSlot.Q, 1050);
            Q.SetSkillshot(0.25f, 60, 2000, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 325);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R);

            #endregion

            #region Config Menu

            //Menu
            Config = new Menu(Player.ChampionName, Player.ChampionName, true);

            //Orbwalker
            var orbwalkerMenu = Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            //Target Selector
            var ts = Config.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);


            //Combo Menu
            var combo = new Menu("Combo Settings", "Combo");

            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboQ.AddItem(new MenuItem("QHealthCombo", "Minimum HP% to use Q").SetValue(new Slider(25, 1, 100)));
            comboQ.AddItem(
                new MenuItem("qHitchance", "Q Hitchance").SetValue(
                    new StringList(
                        new[]
                        {
                            HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                            HitChance.VeryHigh.ToString()
                        }, 2)));

            var comboW = combo.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboW.AddItem(new MenuItem("WHealthCombo", "Minimum HP% to use W").SetValue(new Slider(50, 1, 100)));

            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));

            var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboR.AddItem(new MenuItem("RHealthCombo", "Minimum HP% to use R").SetValue(new Slider(30, 1, 100)));

            combo.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.AddSubMenu(combo);

            //Harass Menu
            var harass = new Menu("Harass Settings", "Harass");

            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassQ.AddItem(new MenuItem("UseQHarassHP", "Minimum HP% to use Q").SetValue(new Slider(50, 1, 100)));
            harassQ.AddItem(
                new MenuItem("qHitchanceH", "Q Hitchance").SetValue(
                    new StringList(
                        new[]
                        {
                            HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                            HitChance.VeryHigh.ToString()
                        }, 2)));

            var harassW = harass.AddSubMenu(new Menu("W Settings", "W"));
            harassW.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            harassW.AddItem(new MenuItem("WHealthHarass", "Minimum HP% to use W").SetValue(new Slider(50, 1, 100)));
            Config.AddSubMenu(harass);

            //Killsteal Menu
            var killsteal = new Menu("KillSteal Settings", "KillSteal");
            killsteal.AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));
            Config.AddSubMenu(killsteal);

            //Misc Menu
            var misc = new Menu("Misc Settings", "Misc");
            misc.AddItem(new MenuItem("QLastHit", "Use Q to last hit minions").SetValue(true));
            misc.AddItem(new MenuItem("QLastHitHP", "Minimum HP% to use Q to lasthit").SetValue(new Slider(60, 1, 100)));
            Config.AddSubMenu(misc);

            //Drawings Menu
            var drawings = new Menu("Drawings", "Drawings");
            drawings.AddItem(new MenuItem("DrawQ", "Q range").SetValue(new Circle(true, Color.CornflowerBlue, Q.Range)));
            drawings.AddItem(new MenuItem("DrawW", "W range").SetValue(new Circle(false, Color.CornflowerBlue, W.Range)));
            Config.AddSubMenu(drawings);

            Config.AddToMainMenu();

            #endregion

            Notifications.AddNotification("Dr.Mundo by Hestia loaded!", 5000);
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Drawing.OnDraw += Drawing_OnDraw;
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

        #region OnAttack

        private static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!E.IsReady())
                return;

            var comboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
            var harassMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed;
            var laneClearMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
            var castE = Config.Item("UseECombo").GetValue<bool>() && E.IsReady();

            if ((((comboMode || harassMode) && target is Obj_AI_Hero) || (laneClearMode && target is Obj_AI_Minion)) && castE)
            {
                E.Cast();
            }
        } 
        #endregion

        #region OnUpdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;


            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ExecuteCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    ExecuteHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
            }

            KillSteal();
        }

        #endregion

        #region Combo

        private static void ExecuteCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var castQ = Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady();
            var castW = Config.Item("UseWCombo").GetValue<bool>() && W.IsReady();
            var castR = Config.Item("UseRCombo").GetValue<bool>() && R.IsReady();

            var qHealth = Config.Item("QHealthCombo").GetValue<Slider>().Value;
            if (castQ && Player.HealthPercent >= qHealth && target.IsValidTarget())
            {
                Q.CastIfHitchanceEquals(target, GetHitChance("qHitchance"));
            }

            var wHealth = Config.Item("WHealthCombo").GetValue<Slider>().Value;
            if (castW && Player.HealthPercent >= wHealth && !Player.HasBuff("BurningAgony") && target.IsValidTarget())
            {
                W.Cast();
            }

            var rHealth = Config.Item("RHealthCombo").GetValue<Slider>().Value;
            if (castR && Player.HealthPercent <= rHealth && !Player.InFountain())
            {
                R.Cast();
            }
        }

        #endregion

        #region Harass

        private static void ExecuteHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var castQ = Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady();
            var castW = Config.Item("UseWHarass").GetValue<bool>() && W.IsReady();

            var qHealth = Config.Item("UseQHarassHP").GetValue<Slider>().Value;
            if (castQ && Player.HealthPercent >= qHealth && target.IsValidTarget())
            {
                Q.CastIfHitchanceEquals(target, GetHitChance("qHitchanceH"));
            }

            var wHealth = Config.Item("WHealthHarass").GetValue<Slider>().Value;
            if (castW && Player.HealthPercent >= wHealth && !Player.HasBuff("BurningAgony") && target.IsValidTarget())
            {
                W.Cast();
            }
        }

        #endregion

        #region LastHit

        private static void LastHit()
        {
            var lastHitQ = Config.Item("QLastHit").GetValue<bool>() && Q.IsReady();
            var qHealth = Config.Item("QLastHitHP").GetValue<Slider>().Value;

            var minion =
                MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth)
                    .Cast<Obj_AI_Minion>()
                    .FirstOrDefault(minions => Q.IsKillable(minions));

            if (lastHitQ && Player.HealthPercent >= qHealth && minion != null)
            {
                Q.Cast(minion);
            }
        }

        #endregion

        #region KillSteal

        private static void KillSteal()
        {
            if (!Config.Item("Killsteal").GetValue<bool>())
                return;

            Qks();
            Iks();
        }

        private static void Qks()
        {
            //Q killsteal
            if (!Config.Item("UseQKS").GetValue<bool>())
                return;

            var target =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(Q.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            if (!target.IsValidTarget())
                return;

            Q.Cast(target);
        }

        private static void Iks()
        {
            //Ignite
            if (!Config.Item("UseIKS").GetValue<bool>() || Ignite == null || Ignite.Slot == SpellSlot.Unknown ||
                !Ignite.Slot.IsReady())
                return;

            var target =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj =>
                            obj.IsValidTarget(600) &&
                            obj.Health < Player.GetSummonerSpellDamage(obj, Damage.SummonerSpell.Ignite));

            if (target.IsValidTarget(600))
            {
                Player.Spellbook.CastSpell(Ignite.Slot, target);
            }
        }

        #endregion

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (
                var circle in
                    new List<string> { "Q", "W" }.Select(spell => Config.Item("Draw" + spell).GetValue<Circle>())
                        .Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        #endregion

    }
}
