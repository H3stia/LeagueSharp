using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace GangPlank
{
    class Program
    {
        //Champion
        private const string Champion = "Gangplank";

        //Player object
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //Menu and orbwalker declarations
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;

        //Spell declaration
        private static Spell q, w, e, r;
        private static SpellDataInst ignite;

        //Debuffs list for W usage
        private static readonly List<BuffType> DebuffsList = new List<BuffType>();  

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

            q = new Spell(SpellSlot.Q, 625);
            w = new Spell(SpellSlot.W);
            e = new Spell(SpellSlot.E, 1150);
            r = new Spell(SpellSlot.R);
            r.SetSkillshot(0.7f, 200, float.MaxValue, false, SkillshotType.SkillshotCircle);
            ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            //Add Debuffs to debuffsList
            DebuffsList.Add(BuffType.Slow);
            DebuffsList.Add(BuffType.Taunt);
            DebuffsList.Add(BuffType.Stun);
            DebuffsList.Add(BuffType.Polymorph);
            DebuffsList.Add(BuffType.Fear);
            DebuffsList.Add(BuffType.Charm);
            DebuffsList.Add(BuffType.Blind);

            config = new Menu(Player.ChampionName, Player.ChampionName, true);

            var orbwalkerMenu = config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var tsMenu = config.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(tsMenu);

            var comboMenu = config.AddSubMenu(new Menu("Combo", "Combo"));
            var comboMenuQ = comboMenu.AddSubMenu(new Menu("Q Settings", "Q Settings"));
            comboMenuQ.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            var comboMenuW = comboMenu.AddSubMenu(new Menu("W Settings", "W Settings"));
            comboMenuW.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenuW.AddItem(new MenuItem("useWminHP", "Use W if HP%").SetValue(new Slider(50, 1)));
            var comboMenuE = comboMenu.AddSubMenu(new Menu("E Settings", "E Settings"));
            comboMenuE.AddItem(new MenuItem("useE", "Use E").SetValue(true));

            var harassMenu = config.AddSubMenu(new Menu("Harass", "Harass"));
            harassMenu.AddItem(new MenuItem("useQharass", "Use Q in mixed mode"));
            harassMenu.AddItem(new MenuItem("useQharassMana", "Use Q if MP%").SetValue(new Slider(50, 1)));

            var miscMenu = config.AddSubMenu(new Menu("Misc Settings", "Misc Settings"));
            miscMenu.AddItem(new MenuItem("useWcleanse", "Use W to cleanse debuffs").SetValue(true));
            var miscMenuWdebuff = miscMenu.AddSubMenu(new Menu("Debuffs Settings", "Debuffs Settings"));
            miscMenuWdebuff.AddItem(new MenuItem("slow", "Slow").SetValue(true));
            miscMenuWdebuff.AddItem(new MenuItem("taunt", "Taunt").SetValue(true));
            miscMenuWdebuff.AddItem(new MenuItem("stun", "Stun").SetValue(true));
            miscMenuWdebuff.AddItem(new MenuItem("polymorph", "Polymorph").SetValue(true));
            miscMenuWdebuff.AddItem(new MenuItem("fear", "Fear").SetValue(true));
            miscMenuWdebuff.AddItem(new MenuItem("charm", "Charm").SetValue(true));
            miscMenuWdebuff.AddItem(new MenuItem("blind", "Blind").SetValue(true));

            var lastHitMenu = config.AddSubMenu(new Menu("LastHit", "LastHit"));
            lastHitMenu.AddItem(new MenuItem("useQlh", "Use Q to Last Hit minions").SetValue(true));
            lastHitMenu.AddItem(new MenuItem("useQlhMana", "Use Q if MP%").SetValue(new Slider(50, 1)));

            var laneClearMenu = config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            laneClearMenu.AddItem(new MenuItem("useQlc", "Q to LH in lane clear").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("useQlcMana", "Use Q if MP%").SetValue(new Slider(50, 1)));
            laneClearMenu.AddItem(new MenuItem("useElc", "Use E in lane clear").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("useElcMinions", "Use E if at least X minions").SetValue(new Slider(5, 1, 15)));
            laneClearMenu.AddItem(new MenuItem("useElcMana", "Use E if MP%").SetValue(new Slider(50, 1)));

            var killsteal = config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useQks", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useRks", "Use R to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useIks", "Use Ignite to KillSteal").SetValue(true));

            var drawingMenu = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawingMenu.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.DarkOrange, q.Range)));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(new Circle(true, Color.DarkOrange, e.Range)));
            drawingMenu.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));

            config.AddToMainMenu();

            Notifications.AddNotification("GangPlank by Hestia loaded!", 5000);
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnGameUpdate(EventArgs args)
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

                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    ExecuteHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            AutoWdebuffs();
            KillSteal();
        }

        private static void ExecuteCombo()
        {
            var target = TargetSelector.GetTarget(e.Range, TargetSelector.DamageType.Physical); 

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            var castQ = config.Item("useQ").GetValue<bool>() && q.IsReady();
            var castE = config.Item("useE").GetValue<bool>() && e.IsReady();
            var castW = config.Item("useW").GetValue<bool>() && w.IsReady();
            var wHealth = config.Item("useWminHP").GetValue<Slider>().Value;

            var enemyCount = Utility.CountEnemiesInRange(e.Range);
            if ((enemyCount > 0) && castE)
            {
                e.Cast();
            }

            if (castQ && target.IsValidTarget(q.Range))
            {
                q.CastOnUnit(target);
            }

            if (castW && Player.HealthPercent <= wHealth)
            {
                w.Cast();
            }
        }

        private static void ExecuteHarass()
        {
            var target = TargetSelector.GetTarget(q.Range, TargetSelector.DamageType.Physical);

            var castQ = config.Item("useQHarass").GetValue<bool>() && q.IsReady();
            var qMana = config.Item("useQharassMana").GetValue<Slider>().Value;

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            if( castQ && target.IsValidTarget(q.Range) && Player.ManaPercent >= qMana)
            {
                q.CastOnUnit(target);
            }

        }

        private static void LastHit()
        {
            if (!q.IsReady() || !config.Item("useQlh").GetValue<bool>() || Player.ManaPercent <= config.Item("useQlhMana").GetValue<Slider>().Value)
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

        private static void LaneClear()
        {
            if (Player.IsDead)
            {
                return;
            }

            var castQ = config.Item("useQlc").GetValue<bool>();
            var qMana = config.Item("useQlcMana").GetValue<Slider>().Value;
            var castE = config.Item("useElc").GetValue<bool>();
            var eMinions = config.Item("useElcMinions").GetValue<Slider>().Value; 
            var eMana = config.Item("useElcMana").GetValue<Slider>().Value;

            var minionCount = MinionManager.GetMinions(Player.Position, q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minionCount.Count > 0 && castQ && Player.ManaPercent >= qMana)
            {
                foreach (var minion in minionCount.Where(minion => minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    q.CastOnUnit(minion);
                    return;
                }
            }

            if (minionCount.Count >= eMinions && castE && Player.ManaPercent >= eMana)
            {
                e.Cast();
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

                q.CastOnUnit(target);
            }

            if (config.Item("useRks").GetValue<bool>() && r.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget() &&
                                enemy.Health < (Player.GetSpellDamage(enemy, SpellSlot.R)) / 2);

                r.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }

            if (config.Item("useIks").GetValue<bool>() && ignite.Slot.IsReady() && ignite != null && ignite.Slot != SpellSlot.Unknown)
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(600) &&
                                enemy.Health < Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite));

                Player.Spellbook.CastSpell(ignite.Slot, target);
            }
        }

        private static void AutoWdebuffs()
        {
            if (!config.Item("useWcleanse").GetValue<bool>() || !HasDebuff())
            {
                return;   
            }

            if (HasDebuff())
            {
                w.Cast();
            }
        }

        private static bool HasDebuff()
        {
            foreach (var buffType in DebuffsList)
            {
                if (Player.HasBuffOfType(buffType) && config.Item(buffType.ToString().ToLowerInvariant()).GetValue<bool>())
                {
                    return true;
                }
            }
            return false;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (config.Item("disableDraw").GetValue<bool>() || Player.IsDead)
            {
                return;
            }

            var width = config.Item("width").GetValue<Slider>().Value;

            if (config.Item("drawQ").GetValue<Circle>().Active && q.Level > 0)
            {
                var circle = config.Item("drawQ").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }

            if (config.Item("drawE").GetValue<Circle>().Active && e.Level > 0)
            {
                var circle = config.Item("drawE").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }
        }
    }
}
