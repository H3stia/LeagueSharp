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
        private static SpellDataInst ignite;

        private static GameObject akaliShroud;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != Champion)
            {
                return;
            }

            q = new Spell(SpellSlot.Q, 1050);
            q.SetSkillshot(0.25f, 75, 1500, true, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W, 325);
            e = new Spell(SpellSlot.E);
            r = new Spell(SpellSlot.R);
            ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));
            
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
            var miscQ = misc.AddSubMenu(new Menu("Q Settings", "Q"));
            miscQ.AddItem(
                new MenuItem("autoQ", "Auto Q on enemies").SetValue(
                    new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            miscQ.AddItem(
                new MenuItem("qHitchanceAuto", "Q Hitchance").SetValue(
                    new StringList(
                        new[]
                        {
                            HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                            HitChance.VeryHigh.ToString()
                        }, 2)));
            miscQ.AddItem(new MenuItem("autoQhp", "Minimum HP% to auto Q").SetValue(new Slider(50, 1)));
            var miscW = misc.AddSubMenu(new Menu("W Settings", "W"));
            miscW.AddItem(new MenuItem("wHandler", "Use automatic W handler").SetValue(
                    new KeyBind("W".ToCharArray()[0], KeyBindType.Toggle)));
            miscW.AddItem(
                new MenuItem("wDisableRange", "Disable W if no enemies in range:").SetValue(new Slider(700, 250, 2500)));
            var miscR = misc.AddSubMenu(new Menu("R Settings", "R"));
            miscR.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            miscR.AddItem(new MenuItem("RHealth", "Minimum HP% to use R").SetValue(new Slider(30, 1)));
            miscR.AddItem(new MenuItem("RHealthEnemies", "If enemies nearby").SetValue(true));

            var farming = config.AddSubMenu(new Menu("Farming Settings", "Farming"));
            farming.AddItem(new MenuItem("useQlh", "Use Q to last hit minions").SetValue(true));
            farming.AddItem(new MenuItem("useQlhHP", "Minimum HP% to use Q to lasthit").SetValue(new Slider(60, 1)));
            farming.AddItem(new MenuItem("qRange", "Only use Q if far from minions").SetValue(true));
            farming.AddItem(new MenuItem("useQlc", "Use Q to last hit in laneclear").SetValue(true));
            farming.AddItem(new MenuItem("useQlcHP", "Minimum HP% to use Q to laneclear").SetValue(new Slider(60, 1)));
            farming.AddItem(new MenuItem("useWlc", "Use W in laneclear").SetValue(true));
            farming.AddItem(new MenuItem("useWlcHP", "Minimum HP% to use W to laneclear").SetValue(new Slider(60, 1)));

            var jungleFarming = config.AddSubMenu(new Menu("Jungle Settings", "Jungle"));
            jungleFarming.AddItem(new MenuItem("useQj", "Use Q to jungle").SetValue(true));
            jungleFarming.AddItem(new MenuItem("useQjHP", "Minimum HP% to use Q in jungle").SetValue(new Slider(20, 1)));
            jungleFarming.AddItem(new MenuItem("useWj", "Use W to jungle").SetValue(true));
            jungleFarming.AddItem(new MenuItem("useEj", "Use E to jungle").SetValue(true));
            jungleFarming.AddItem(
                new MenuItem("jungleActive", "Jungle Clear!").SetValue(
                    new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var drawingMenu = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawingMenu.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawQ", "Q range").SetValue(new Circle(true, Color.DarkOrange, q.Range)));
            drawingMenu.AddItem(new MenuItem("drawW", "W range").SetValue(new Circle(false, Color.DarkOrange, w.Range)));
            drawingMenu.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));
            drawingMenu.AddItem(new MenuItem("drawAutoQ", "Draw AutoQ status").SetValue(true));

            config.AddItem(new MenuItem("spacer", ""));
            config.AddItem(new MenuItem("version", "Version: 1.0.0.3"));
            config.AddItem(new MenuItem("author", "Author: Hestia"));

            config.AddToMainMenu();

            Notifications.AddNotification("Dr.Mundo by Hestia loaded!", 5000);
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.OnAttack += OrbwalkingOnAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
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
                    LastHit();
                    ExecuteHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            if (config.Item("jungleActive").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }

            AutoR();
            AutoQ();
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
            var castE = config.Item("useE").GetValue<bool>() && e.IsReady();
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
                    if (config.Item("qRange").GetValue<bool>())
                    {
                        if (
                        HealthPrediction.GetHealthPrediction(
                            minion, (int)(q.Delay + (minion.Distance(Player.Position) / q.Speed))) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) && Player.Distance(minion) > Player.AttackRange * 2)
                        {
                            q.Cast(minion);
                        }
                    }
                    else
                    {
                        if (
                        HealthPrediction.GetHealthPrediction(
                            minion, (int)(q.Delay + (minion.Distance(Player.Position) / q.Speed))) <
                        Player.GetSpellDamage(minion, SpellSlot.Q))
                        {
                            q.Cast(minion);
                        }
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

            if (Player.IsDead || !Orbwalking.CanMove(40))
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range);

            if (minionCount.Count > 0)
            {
                if (castQ && Player.HealthPercent >= qHealth)
                {
                    foreach (var minion in minionCount)
                    {
                        if (
                            HealthPrediction.GetHealthPrediction(
                                minion, (int)(q.Delay + (minion.Distance(Player.Position) / q.Speed))) <
                            Player.GetSpellDamage(minion, SpellSlot.Q))
                        {
                            q.Cast(minion);
                        }
                    }
                }

                if (castW && Player.HealthPercent >= wHealth && !IsBurning())
                {
                    w.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            var castQ = config.Item("useQj").GetValue<bool>() && q.IsReady();
            var qHealth = config.Item("useQjHP").GetValue<Slider>().Value;
            var castW = config.Item("useWj").GetValue<bool>() && w.IsReady();
            var castE = config.Item("useEj").GetValue<bool>() && e.IsReady();

            if (Player.IsDead || !Orbwalking.CanMove(40))
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (minionCount.Count > 0)
            {
                var minion = minionCount[0];

                if (castQ && Player.HealthPercent >= qHealth)
                {
                    q.Cast(minion);
                }

                if (castE && Player.Distance(minion) < (q.Range / 2))
                {
                    e.Cast();
                }

                if (castW  && !IsBurning())
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

            if (config.Item("useIks").GetValue<bool>() && ignite.Slot.IsReady() && ignite != null && ignite.Slot != SpellSlot.Unknown)
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(600) &&
                                enemy.Health < Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite));

                if (target.IsValidTarget(600))
                {
                    Player.Spellbook.CastSpell(ignite.Slot, target);
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
            if (!config.Item("wHandler").GetValue<KeyBind>().Active)
            {
                return;
            }

            var enemyCount = Utility.CountEnemiesInRange(config.Item("wDisableRange").GetValue<Slider>().Value);
            var minionCount = MinionManager.GetMinions(Player.Position, w.Range * 2, MinionTypes.All, MinionTeam.Neutral);

            if (IsBurning() && akaliShroud !=null && Player.Distance(akaliShroud.Position) < w.Range * 3)
            {
                return;
            }

            if (IsBurning() && minionCount.Count > 0)
            {
                return;
            }

            if (IsBurning() && enemyCount == 0 && orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                w.Cast();
            }
        }

        private static void AutoQ()
        {
            var autoQ = config.Item("autoQ").GetValue<KeyBind>().Active;
            var qHealth = config.Item("autoQhp").GetValue<Slider>().Value;

            if (Player.IsDead)
            {
                return;
            }

            if (autoQ && q.IsReady() && Player.HealthPercent >= qHealth)
            {
                var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsValidTarget(q.Range));

                if (target.IsValidTarget(q.Range))
                {
                    q.CastIfHitchanceEquals(target, GetHitChance("qHitchanceAuto"));
                }
            }
        }

        private static void AutoR()
        {
            var castR = config.Item("useR").GetValue<bool>() && r.IsReady();
            var rHealth = config.Item("RHealth").GetValue<Slider>().Value;
            var rEnemies = config.Item("RHealthEnemies").GetValue<bool>();
            var enemyCount = Utility.CountEnemiesInRange(q.Range * 2);

            if (Player.IsDead)
            {
                return;
            }

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

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.Name == "akali_smoke_bomb_tar_team_red.troy")
            {
                akaliShroud = obj;
            }
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name == "akali_smoke_bomb_tar_team_red.troy")
            {
                akaliShroud = null;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || config.Item("disableDraw").GetValue<bool>())
            {
                return;
            }

            var heroPosition = Drawing.WorldToScreen(Player.Position);
            var textDimension = Drawing.GetTextExtent("AutoQ: ON");
            var drawQstatus = config.Item("drawAutoQ").GetValue<bool>();
            var autoQ = config.Item("autoQ").GetValue<KeyBind>().Active;

            if (drawQstatus && autoQ)
            {
                Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height, Color.DarkOrange, "AutoQ: ON");
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
