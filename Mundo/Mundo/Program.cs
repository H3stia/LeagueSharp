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

        //Player object
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //Menu and orbwalker declarations
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;

        //Spell declaration
        private static Spell q, w, e, r;
        public static Spell Ignite = new Spell(SpellSlot.Unknown, 600);

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != Champion)
                return;

            q = new Spell(SpellSlot.Q, 1050);
            q.SetSkillshot(0.25f, 60, 2000, true, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W, 325);
            e = new Spell(SpellSlot.E);
            r = new Spell(SpellSlot.R);
            var ignite = Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "summonerdot");
            if (ignite != null)
            {
                Ignite.Slot = ignite.Slot;
            }
                

            //Menu
            config = new Menu(Player.ChampionName, Player.ChampionName, true);

            var orbwalkerMenu = config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var ts = config.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            var combo = config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
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
            comboW.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboW.AddItem(new MenuItem("WHealthCombo", "Minimum HP% to use W").SetValue(new Slider(50, 1)));
            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("useE", "Use E").SetValue(true));

            var harass = config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassQ.AddItem(new MenuItem("useQHarassHP", "Minimum HP% to use Q").SetValue(new Slider(50, 1)));
            harassQ.AddItem(
                new MenuItem("qHitchanceH", "Q Hitchance").SetValue(
                    new StringList(
                        new[]
                        {
                            HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                            HitChance.VeryHigh.ToString()
                        }, 2)));
            var harassW = harass.AddSubMenu(new Menu("W Settings", "W"));
            harassW.AddItem(new MenuItem("useWHarass", "Use W").SetValue(true));
            harassW.AddItem(new MenuItem("WHealthHarass", "Minimum HP% to use W").SetValue(new Slider(50, 1)));
            var harassE = combo.AddSubMenu(new Menu("E Settings", "E"));
            harassE.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));

            var killsteal = config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useQks", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useIks", "Use Ignite to KillSteal").SetValue(true));

            var misc = config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("useQlh", "Use Q to last hit minions").SetValue(true));
            misc.AddItem(new MenuItem("useQlhHP", "Minimum HP% to use Q to lasthit").SetValue(new Slider(60, 1)));
            misc.AddItem(new MenuItem("useQlc", "Use Q to last hit in laneclear").SetValue(true));
            misc.AddItem(new MenuItem("useQlcHP", "Minimum HP% to use Q to laneclear").SetValue(new Slider(60, 1)));
            misc.AddItem(new MenuItem("useWlc", "Use W in laneclear").SetValue(true));
            misc.AddItem(new MenuItem("useWlcHP", "Minimum HP% to use W to laneclear").SetValue(new Slider(60, 1)));
            var miscR = misc.AddSubMenu(new Menu("R Settings", "R"));
            miscR.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            miscR.AddItem(new MenuItem("RHealth", "Minimum HP% to use R").SetValue(new Slider(30, 1)));
            miscR.AddItem(new MenuItem("RHealthEnemies", "If enemies nearby").SetValue(true));

            var drawingMenu = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawingMenu.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawQ", "Q range").SetValue(new Circle(true, Color.DarkOrange, q.Range)));
            drawingMenu.AddItem(new MenuItem("drawW", "W range").SetValue(new Circle(false, Color.DarkOrange, w.Range)));
            drawingMenu.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));

            config.AddToMainMenu();

            Notifications.AddNotification("Dr.Mundo by Hestia loaded!", 5000);
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.OnAttack += OrbwalkingOnAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

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

        private static void OrbwalkingOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t != null && (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && unit.IsMe)
            {
                var castE = config.Item("useE").GetValue<bool>() && e.IsReady();

                if (castE)
                {
                    e.Cast();
                }
            }
        } 

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

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

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            AutoR();
            BurningDisabler();
            KillSteal();
        }

        private static void ExecuteCombo()
        {
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            var castQ = config.Item("useQ").GetValue<bool>() && q.IsReady();
            var castW = config.Item("useW").GetValue<bool>() && w.IsReady();
            var castE = config.Item("useE").GetValue<bool>() && w.IsReady();
            var qHealth = config.Item("QHealthCombo").GetValue<Slider>().Value;
            var wHealth = config.Item("WHealthCombo").GetValue<Slider>().Value;

            if (castQ && Player.HealthPercent >= qHealth && target.IsValidTarget(q.Range))
            {
                q.CastIfHitchanceEquals(target, GetHitChance("qHitchance"));
            }

            if (castW && Player.HealthPercent >= wHealth && !IsBurning() && target.IsValidTarget(w.Range) && Player.Distance(target) < w.Range)
            {
                w.Cast();
            }

            if (castE && target.IsValidTarget(w.Range) && Player.Distance(target) < w.Range)
            {
                e.Cast();
            }
        }

        private static void ExecuteHarass()
        {
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);

            var castQ = config.Item("useQHarass").GetValue<bool>() && q.IsReady();
            var castW = config.Item("useWHarass").GetValue<bool>() && w.IsReady();
            var castE = config.Item("useEHarass").GetValue<bool>() && e.IsReady();
            var qHealth = config.Item("useQHarassHP").GetValue<Slider>().Value;
            var wHealth = config.Item("WHealthHarass").GetValue<Slider>().Value;

            if (Player.IsDead || target == null || !target.IsValid)
                return;

            if (castQ && Player.HealthPercent >= qHealth && target.IsValidTarget(q.Range))
            {
                q.CastIfHitchanceEquals(target, GetHitChance("qHitchanceH"));
            }

            if (castW && Player.HealthPercent >= wHealth && !IsBurning() && target.IsValidTarget(w.Range))
            {
                w.Cast();
            }

            if (castE && target.IsValidTarget(w.Range) && Player.Distance(target) < w.Range)
            {
                e.Cast();
            }
        }

        private static void LastHit()
        {
            var castQ = config.Item("useQlh").GetValue<bool>() && q.IsReady();
            var qHealth = config.Item("useQlhHP").GetValue<Slider>().Value;

            if (Player.IsDead || !Orbwalking.CanMove(40))
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.ServerPosition, q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minionCount.Count > 0 && castQ && Player.HealthPercent >= qHealth)
            {
                foreach (var minion in minionCount)
                {
                    if (q.IsKillable(minion))
                    {
                        q.Cast(minion);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var castQ = config.Item("useQlc").GetValue<bool>() && q.IsReady();
            var qHealth = config.Item("useQlcHP").GetValue<Slider>().Value;
            var castW = config.Item("useWlc").GetValue<bool>() && w.IsReady();
            var wHealth = config.Item("useWlcHP").GetValue<Slider>().Value;

            if (Player.IsDead)
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minionCount.Count > 0)
            {
                if (castQ && Player.HealthPercent >= qHealth)
                {
                    foreach (var minion in minionCount.Where(minion => minion.Health <= q.GetDamage(minion)))
                    {
                        q.Cast(minion);
                        return;
                    }
                }

                if (castW && Player.HealthPercent >= wHealth && !IsBurning())
                {
                    w.Cast();
                }
            }
        }

        private static void KillSteal()
        {
            if (Player.IsDead || !config.Item("killsteal").GetValue<bool>())
            {
                return;
            }

            if (config.Item("useQks").GetValue<bool>() && q.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(q.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.Q));
                if (target.IsValidTarget(q.Range))
                {
                    q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }

            if (config.Item("useIks").GetValue<bool>() && Ignite.Slot.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(Ignite.Range) &&
                                enemy.Health < Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite) && Ignite.Slot != SpellSlot.Unknown);

                if (target.IsValidTarget(Ignite.Range))
                {
                    Ignite.Cast(target);
                }
            }
        }

        private static bool IsBurning()
        {
            if (Player.HasBuff("BurningAgony"))
            {
                return true;
            }
            return false;
        }

        private static void BurningDisabler()
        {
            var enemyCount = Utility.CountEnemiesInRange(w.Range * 2);

            if (IsBurning() && enemyCount == 0 && orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                w.Cast();
            }
        }

        private static void AutoR()
        {
            var castR = config.Item("useR").GetValue<bool>() && r.IsReady();
            var rHealth = config.Item("RHealth").GetValue<Slider>().Value;
            var rEnemies = config.Item("RHealthEnemies").GetValue<bool>();
            var enemyCount = Utility.CountEnemiesInRange(q.Range * 2);

            if (rEnemies && castR && Player.HealthPercent <= rHealth && !Player.InFountain())
            {
                if (enemyCount > 0)
                {
                    r.Cast();
                }
            }
            else if (!rEnemies && castR && Player.HealthPercent <= rHealth && !Player.InFountain())
            {
                r.Cast();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || config.Item("disableDraw").GetValue<bool>())
            {
                return;
            }

            var width = config.Item("width").GetValue<Slider>().Value;

            if (config.Item("drawQ").GetValue<Circle>().Active && q.Level > 0)
            {
                var circle = config.Item("drawQ").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }

            if (config.Item("drawW").GetValue<Circle>().Active && w.Level > 0)
            {
                var circle = config.Item("drawW").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }
        }
    }
}
