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
        private static Spell q, w, e, r;

        //Player
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //Menu
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;

        //Ignite
        private static SpellDataInst ignite;

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

            q = new Spell(SpellSlot.Q, 1050);
            q.SetSkillshot(0.25f, 60, 2000, true, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W, 325);
            e = new Spell(SpellSlot.E);
            r = new Spell(SpellSlot.R);

            #endregion

            #region Config Menu

            //Menu
            config = new Menu(Player.ChampionName, Player.ChampionName, true);

            var orbwalkerMenu = config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var ts = config.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            var combo = config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboQ.AddItem(new MenuItem("QHealthCombo", "Minimum HP% to use Q").SetValue(new Slider(25, 1)));
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
            comboW.AddItem(new MenuItem("WHealthCombo", "Minimum HP% to use W").SetValue(new Slider(50, 1)));
            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboR.AddItem(new MenuItem("RHealthCombo", "Minimum HP% to use R").SetValue(new Slider(30, 1)));

            var harass = config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassQ.AddItem(new MenuItem("UseQHarassHP", "Minimum HP% to use Q").SetValue(new Slider(50, 1)));
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
            harassW.AddItem(new MenuItem("WHealthHarass", "Minimum HP% to use W").SetValue(new Slider(50, 1)));

            var killsteal = config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            var misc = config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("QLastHit", "Use Q to last hit minions").SetValue(true));
            misc.AddItem(new MenuItem("QLastHitHP", "Minimum HP% to use Q to lasthit").SetValue(new Slider(60, 1)));

            var drawings = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawings.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawings.AddItem(new MenuItem("drawQ", "Q range").SetValue(new Circle(true, Color.CornflowerBlue, q.Range)));
            drawings.AddItem(new MenuItem("drawW", "W range").SetValue(new Circle(false, Color.CornflowerBlue, w.Range)));

            config.AddToMainMenu();

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
            var hc = config.Item(name).GetValue<StringList>();
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
            if (!e.IsReady())
                return;

            var comboMode = orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
            var harassMode = orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed;
            var laneClearMode = orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
            var castE = config.Item("UseECombo").GetValue<bool>() && e.IsReady();

            if ((((comboMode || harassMode) && target is Obj_AI_Hero) || (laneClearMode && target is Obj_AI_Minion)) && castE)
            {
                e.Cast();
            }
        } 
        #endregion

        #region OnUpdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;


            switch (orbwalker.ActiveMode)
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
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            var castQ = config.Item("UseQCombo").GetValue<bool>() && q.IsReady();
            var castW = config.Item("UseWCombo").GetValue<bool>() && w.IsReady();
            var castR = config.Item("UseRCombo").GetValue<bool>() && r.IsReady();

            var qHealth = config.Item("QHealthCombo").GetValue<Slider>().Value;
            if (castQ && Player.HealthPercent >= qHealth && target.IsValidTarget())
            {
                q.CastIfHitchanceEquals(target, GetHitChance("qHitchance"));
            }

            var wHealth = config.Item("WHealthCombo").GetValue<Slider>().Value;
            if (castW && Player.HealthPercent >= wHealth && !Player.HasBuff("BurningAgony") && target.IsValidTarget())
            {
                w.Cast();
            }

            var rHealth = config.Item("RHealthCombo").GetValue<Slider>().Value;
            if (castR && Player.HealthPercent <= rHealth && !Player.InFountain())
            {
                r.Cast();
            }
        }

        #endregion

        #region Harass

        private static void ExecuteHarass()
        {
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var castQ = config.Item("UseQHarass").GetValue<bool>() && q.IsReady();
            var castW = config.Item("UseWHarass").GetValue<bool>() && w.IsReady();

            var qHealth = config.Item("UseQHarassHP").GetValue<Slider>().Value;
            if (castQ && Player.HealthPercent >= qHealth && target.IsValidTarget())
            {
                q.CastIfHitchanceEquals(target, GetHitChance("qHitchanceH"));
            }

            var wHealth = config.Item("WHealthHarass").GetValue<Slider>().Value;
            if (castW && Player.HealthPercent >= wHealth && !Player.HasBuff("BurningAgony") && target.IsValidTarget())
            {
                w.Cast();
            }
        }

        #endregion

        #region LastHit

        private static void LastHit()
        {
            if (!q.IsReady() || !config.Item("QLastHit").GetValue<bool>() || Player.HealthPercent <= config.Item("QLastHitHP").GetValue<Slider>().Value)
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minionCount.Count > 0)
            {
                foreach (var minion in minionCount.Where(minion => minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    q.CastOnUnit(minion);
                    return;
                }
            }
        }

        #endregion


        private static void KillSteal()
        {
            if (!config.Item("Killsteal").GetValue<bool>())
            {
                return;
            }

            Qks();
            Iks();
        }

        private static void Qks()
        {
            //Q killsteal
            if (!config.Item("UseQKS").GetValue<bool>() || !q.IsReady())
            {
                return;
            }

            var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsValidTarget(q.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.Q));

            q.Cast(target);
        }

        private static void Iks()
        {
            //Ignite
            if (!config.Item("UseIKS").GetValue<bool>() || ignite == null || ignite.Slot == SpellSlot.Unknown || !ignite.Slot.IsReady())
            {
                return;
            }

            var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsValidTarget(600) && enemy.Health < Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite));

            Player.Spellbook.CastSpell(ignite.Slot, target);

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || config.Item("disableDraw").GetValue<bool>())
            {
                return;
            }

            if (config.Item("drawQ").GetValue<Circle>().Active && q.Level > 0)
            {
                var circle = config.Item("drawQ").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }

            if (config.Item("drawE").GetValue<Circle>().Active && e.Level > 0)
            {
                var circle = config.Item("drawE").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }

        }
    }
}
