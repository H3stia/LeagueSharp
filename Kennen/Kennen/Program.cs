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
        //Champion
        private const string Champion = "Kennen";

        //Player object
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //Menu and orbwalker declarations
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;

        //Spell declaration
        private static Spell q, w, e, r;
        private static SpellDataInst ignite;

        public static void Main(string[] args)
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
            q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W, 800);
            r = new Spell(SpellSlot.R, 550);
            ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            config = new Menu(Player.ChampionName, Player.ChampionName, true);

            var orbwalkerMenu = config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var tsMenu = config.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(tsMenu);

            var combo = config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboQ.AddItem(
                new MenuItem("qHitchance", "Q Hitchance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 2)));
            var comboW = combo.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboW.AddItem(
                new MenuItem("useWmodeCombo", "W Mode").SetValue(new StringList(new[] { "Always", "Only Stunnable" })));
            var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddItem(new MenuItem("useR", "Use smart R").SetValue(true));

            var harass = config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassQ.AddItem(
                new MenuItem("qHitchanceH", "Q Hitchance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 2)));
            var harassW = harass.AddSubMenu(new Menu("W Settings", "W"));
            harassW.AddItem(new MenuItem("useWHarass", "Use W").SetValue(true));
            harassW.AddItem(
                new MenuItem("useWmodeHarass", "W Mode").SetValue(new StringList(new[] { "Always", "Only Stunnable" })));

            var misc = config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("useRmul", "Use R for multiple targets").SetValue(true));
            misc.AddItem(new MenuItem("useRmulti", "Use R on min X targets").SetValue(new Slider(2, 1, 5)));

            var lastHitMenu = config.AddSubMenu(new Menu("LastHit", "LastHit"));
            lastHitMenu.AddItem(new MenuItem("useQlh", "Use Q to Last Hit minions").SetValue(true));

            var laneClearMenu = config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            laneClearMenu.AddItem(new MenuItem("useQlc", "Q to LH in lane clear").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("useQlcH", "Q to harass in lane clear").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("useWlc", "Use W to harass in lane clear").SetValue(false));

            var jungleClearMenu = config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            jungleClearMenu.AddItem(new MenuItem("useQj", "Q in jungle clear").SetValue(true));
            jungleClearMenu.AddItem(new MenuItem("useWj", "Use W in jungle clear").SetValue(true));
            jungleClearMenu.AddItem(
                new MenuItem("jungleActive", "Jungle Clear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var killsteal = config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("killsteal", "Activate Killsteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useQks", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useWks", "Use W to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useIks", "Use Ignite to KillSteal").SetValue(true));

            var drawingMenu = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawingMenu.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.DarkOrange, q.Range)));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(new Circle(true, Color.DarkOrange, w.Range)));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(new Circle(true, Color.DarkOrange, r.Range)));
            drawingMenu.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));
            drawingMenu.AddItem(new MenuItem("drawDmg", "Draw damage on Healthbar").SetValue(true));

            config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;

            Notifications.AddNotification("Kennen by Hestia loaded!", 5000);
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            Utility.HpBarDamageIndicator.Enabled = config.Item("drawDmg").GetValue<bool>();

            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ExecuteCombo();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    LastHit();
                    ExecuteHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            if (config.Item("jungleActive").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }

            KillSteal();
            CastRmulti();
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

        private static void ExecuteCombo()
        {
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            var castQ = config.Item("useQ").GetValue<bool>() && q.IsReady();
            var castW = config.Item("useW").GetValue<bool>() && w.IsReady();
            var castR = config.Item("useR").GetValue<bool>() && r.IsReady();
            var modeW = config.Item("useWmodeCombo").GetValue<StringList>();

            if (castQ && target.IsValidTarget(q.Range))
            {
                q.CastIfHitchanceEquals(target, GetHitChance("qHitchance"));
            }

            if (castW && target.IsValidTarget(w.Range))
            {
                switch (modeW.SelectedIndex)
                {
                    case 0:
                        if (target.HasBuff("kennenmarkofstorm"))
                        {
                            w.Cast();
                        }
                        break;

                    case 1:
                        foreach (var buff in target.Buffs)
                        {
                            if (buff.Name == "kennenmarkofstorm" && buff.Count == 2)
                            {
                                w.Cast();
                            }
                        }
                        break;
                }
            }

            if (castR && target.IsValidTarget(r.Range))
            {
                if (target.Health < GetComboDamage(target))
                {
                    r.Cast();
                }
            }
        }

        private static void ExecuteHarass()
        {
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            var castQ = config.Item("useQHarass").GetValue<bool>() && q.IsReady();
            var castW = config.Item("useWHarass").GetValue<bool>() && w.IsReady();
            var modeW = config.Item("useWmodeHarass").GetValue<StringList>();

            if (castQ && target.IsValidTarget(q.Range))
            {
                q.CastIfHitchanceEquals(target, GetHitChance("qHitchanceH"));
            }

            if (castW && target.IsValidTarget(w.Range))
            {
                switch (modeW.SelectedIndex)
                {
                    case 0:
                        if (target.HasBuff("kennenmarkofstorm"))
                        {
                            w.Cast();
                        }
                        break;

                    case 1:
                        foreach (var buff in target.Buffs)
                        {
                            if (buff.Name == "kennenmarkofstorm" && buff.Count == 2)
                            {
                                w.Cast();
                            }
                        }
                        break;
                }
            }
        }

        private static void LastHit()
        {
            var castQ = config.Item("useQlh").GetValue<bool>() && q.IsReady();

            if (!q.IsReady() || !Orbwalking.CanMove(40))
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (minionCount.Count > 0 && castQ)
            {
                foreach (var minion in minionCount)
                {
                    if (Player.Distance(minion) <= Player.AttackRange)
                    {
                        return;
                    }   
                    if (HealthPrediction.GetHealthPrediction(
                            minion, (int)(q.Delay + (minion.Distance(Player.Position) / q.Speed))) <
                        Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        q.Cast(minion);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            if (Player.IsDead || !Orbwalking.CanMove(40))
            {
                return;
            }

            var castQ = config.Item("useQlc").GetValue<bool>();
            var castW = config.Item("useWlc").GetValue<bool>();
            var castQh = config.Item("useQlcH").GetValue<bool>();

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            var qEnemies = Utility.CountEnemiesInRange(q.Range);
            var qTarget = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Magical);
            var qPred = q.GetPrediction(qTarget);

            if (minionCount.Count > 0 && castQ && qEnemies == 0)
            {
                foreach (var minion in minionCount)
                {
                    if (Player.Distance(minion) <= Player.AttackRange)
                    {
                        return;
                    }
                    if (HealthPrediction.GetHealthPrediction(
                            minion, (int)(q.Delay + (minion.Distance(Player.Position) / q.Speed))) <
                        Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        q.Cast(minion);
                    }
                }
            }

            if (minionCount.Count > 0 && castQ && castQh && qEnemies > 0 && qPred.Hitchance == HitChance.Collision)
            {
                foreach (var minion in minionCount)
                {
                    if (Player.Distance(minion) <= Player.AttackRange)
                    {
                        return;
                    }
                    if (HealthPrediction.GetHealthPrediction(
                            minion, (int)(q.Delay + (minion.Distance(Player.Position) / q.Speed))) <
                        Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        q.Cast(minion);
                    }
                }
            }

            if (castQh && qEnemies > 0 && qPred.Hitchance == HitChance.VeryHigh)
            {
                q.Cast(qTarget);
            }

            if (castW)
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(enemy => enemy.IsValidTarget(w.Range) && enemy.HasBuff("kennenmarkofstorm"));

                if (target.IsValidTarget(w.Range))
                {
                    w.Cast(target);
                }
            }
        }

        private static void JungleClear()
        {
            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var castQ = config.Item("useQj").GetValue<bool>();
            var castW = config.Item("useWj").GetValue<bool>();

            if (minionCount.Count > 0)
            {
                var minion = minionCount[0];

                if (castQ)
                {
                    q.Cast(minion);
                }

                if (castW && minion.HasBuff("kennenmarkofstorm"))
                {
                    w.Cast(minion);
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

            if (config.Item("useWks").GetValue<bool>() && w.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(w.Range) &&
                                enemy.Health < (Player.GetSpellDamage(enemy, SpellSlot.W)) && enemy.HasBuff("kennenmarkofstorm"));

                if (target.IsValidTarget(w.Range))
                {
                    w.Cast();
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

        private static void CastRmulti()
        {
            var castR = config.Item("useRmul").GetValue<bool>() && r.IsReady();
            var minR = config.Item("useRmulti").GetValue<Slider>().Value;
            var enemiesCount = Player.CountEnemiesInRange(r.Range);

            if (Player.IsDead)
            {
                return;
            }

            if (castR && enemiesCount >= minR)
            {
                r.Cast();
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var comboDamage = 0d;

            if (q.IsReady())
                comboDamage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (w.IsReady())
                comboDamage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (r.IsReady())
                comboDamage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (ignite.IsReady() && enemy.Health < Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite) + comboDamage)
                comboDamage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)comboDamage;
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

            if (config.Item("drawR").GetValue<Circle>().Active && r.Level > 0)
            {
                var circle = config.Item("drawR").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }
        }
    }
}
