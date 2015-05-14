using System;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace Elise
{
    class Program
    {
        //Champion
        private const string Champion = "Elise";

        //Player object
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        //Menu and orbwalker declarations
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;

        //Spell declaration
        private static Spell qHuman, wHuman, eHuman, qSpider, wSpider, eSpider, r;
        private static SpellDataInst ignite;

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

            qHuman = new Spell(SpellSlot.Q, 625);
            wHuman = new Spell(SpellSlot.W, 950);
            wHuman.SetSkillshot(0.3f, 100, 950, true, SkillshotType.SkillshotLine);
            eHuman = new Spell(SpellSlot.E, 1075);
            eHuman.SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);
            qSpider = new Spell(SpellSlot.Q, 475);
            wSpider = new Spell(SpellSlot.W);
            eSpider = new Spell(SpellSlot.E, 700);
            r = new Spell(SpellSlot.R);
            ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            config = new Menu(Player.ChampionName, Player.ChampionName, true);

            var orbwalkerMenu = config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var ts = config.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(ts);

            var combo = config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("useQh", "Use Q human").SetValue(true));
            comboQ.AddItem(new MenuItem("useQs", "Use Q spider").SetValue(true));
            var comboW = combo.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddItem(new MenuItem("useWh", "Use W human").SetValue(true));
            comboW.AddItem(new MenuItem("useWs", "Use W spider").SetValue(true));
            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("useEh", "Use E human").SetValue(true));
            comboE.AddItem(
                new MenuItem("eHitchanceH", "E Hitchance").SetValue(
                    new StringList(
                        new[]
                        {
                            HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                            HitChance.VeryHigh.ToString()
                        }, 2)));
            comboE.AddItem(new MenuItem("useEs", "Use E spider to engage").SetValue(true));
            var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddItem(new MenuItem("useR", "Use R to switch forms").SetValue(true));
            /*
            var harass = config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("useQHarassH", "Use Q human").SetValue(true));
            harassQ.AddItem(new MenuItem("useQHarassS", "Use Q spider").SetValue(true));
            var harassW = harass.AddSubMenu(new Menu("W Settings", "W"));
            harassW.AddItem(new MenuItem("useWHarassH", "Use W human").SetValue(true));
            var harassE = combo.AddSubMenu(new Menu("E Settings", "E"));
            harassE.AddItem(new MenuItem("useEHarassH", "Use E").SetValue(true));
            */
            var killsteal = config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useQksH", "Use Q human to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useQksS", "Use Q spider to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useWksH", "Use W human to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useIks", "Use Ignite to KillSteal").SetValue(true));

            var misc = config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            var miscQ = misc.AddSubMenu(new Menu("Q Settings", "Q"));
            miscQ.AddItem(
                new MenuItem("autoQ", "Auto Q human on enemies").SetValue(
                    new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            miscQ.AddItem(new MenuItem("autoQmp", "Minimum MP% to auto Q").SetValue(new Slider(50, 1)));
            var miscR = misc.AddSubMenu(new Menu("R Settings", "R"));
            miscR.AddItem(new MenuItem("toSpiderBase", "Use R to spider when recalled").SetValue(true));

            var farming = config.AddSubMenu(new Menu("Last Hit Settings", "Last Hit Farming"));
            farming.AddItem(new MenuItem("useQlh", "Use spider Q to last hit minions").SetValue(true));
            farming.AddItem(new MenuItem("useQlc", "Use Q spider to laneclear").SetValue(true));
            farming.AddItem(new MenuItem("useWlcS", "Use W spider in laneclear").SetValue(true));
            farming.AddItem(new MenuItem("useWlcH", "Use W spider in laneclear").SetValue(false));
            farming.AddItem(new MenuItem("useWlcHMP", "Minimum MP% to use W human to laneclear").SetValue(new Slider(60, 1)));
            farming.AddItem(new MenuItem("useWlcMin", "Minimum minion to cast W human").SetValue(new Slider(3, 1, 10)));

            var drawingMenu = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawingMenu.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawCD", "Draw spells CD").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawQh", "Q human range").SetValue(new Circle(true, Color.DarkOrange, qHuman.Range)));
            drawingMenu.AddItem(new MenuItem("drawWh", "W human range").SetValue(new Circle(false, Color.DarkOrange, wHuman.Range)));
            drawingMenu.AddItem(new MenuItem("drawEh", "E human range").SetValue(new Circle(false, Color.DarkOrange, eHuman.Range)));
            drawingMenu.AddItem(new MenuItem("drawQs", "Q spider range").SetValue(new Circle(true, Color.DarkOrange, qSpider.Range)));
            drawingMenu.AddItem(new MenuItem("drawEs", "E spider range").SetValue(new Circle(true, Color.DarkOrange, eSpider.Range)));
            drawingMenu.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));

            config.AddItem(new MenuItem("spacer", ""));
            config.AddItem(new MenuItem("version", "Version: 1.0.0.0"));
            config.AddItem(new MenuItem("author", "Author: Hestia"));

            config.AddToMainMenu();

            Notifications.AddNotification("Elise by Hestia loaded!", 5000);
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;

            if (!SpiderForm())
            {
                return;
            }

            if (t != null && (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) && unit.IsMe)
            {
                var useWs = config.Item("useWs").GetValue<bool>() && wSpider.IsReady();
                var useWsLc = config.Item("useWlcS").GetValue<bool>() && wSpider.IsReady();

                if (useWs || useWsLc)
                {
                    wSpider.Cast();
                }
            }
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

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            ProcessCooldowns();

            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ExecuteCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    LastHit();
                    //ExecuteHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            AutoRbase();
            KillSteal();
            AutoQ();
        }

        private static void ExecuteCombo()
        {
            var target = TargetSelector.GetTarget(eSpider.Range, TargetSelector.DamageType.Magical);

            if (Player.IsDead || target == null || !target.IsValid)
            {
                return;
            }

            var useQh = config.Item("useQh").GetValue<bool>();
            var useQs = config.Item("useQs").GetValue<bool>();
            var useWh = config.Item("useWh").GetValue<bool>();
            //W after attack
            //var useWs = config.Item("useWs").GetValue<bool>();
            var useEh = config.Item("useEh").GetValue<bool>();
            var useEs = config.Item("useEs").GetValue<bool>();
            var useR = config.Item("useR").GetValue<bool>();

            //spider combo
            if (SpiderForm())
            {
                if (useEs && target.IsValidTarget(eSpider.Range) && eSpider.IsReady())
                {
                    eSpider.CastOnUnit(target);
                }

                if (useQs && target.IsValidTarget(qSpider.Range))
                {
                    qSpider.CastOnUnit(target);
                }

                if (r.IsReady() && useR && (eHumanCD == 0 || HumanDamage(target) > target.Health))
                {
                    r.Cast();
                }
            }

            //human combo
            if (!SpiderForm())
            {
                if (useEh && target.IsValidTarget(eHuman.Range) && eHuman.IsReady())
                {
                    eHuman.CastIfHitchanceEquals(target, GetHitChance("eHitchanceH"));
                }

                if (useWh && target.IsValidTarget(wHuman.Range) && wHuman.IsReady())
                {
                    wHuman.Cast(target);
                }

                if (useQh && target.IsValidTarget(qHuman.Range) && qHuman.IsReady())
                {
                    qHuman.CastOnUnit(target);
                }

                if (r.IsReady() && useR && qSpiderCD == 0)
                {
                    r.Cast();
                }
            }
        }

        private static void LastHit()
        {
            var useQs = config.Item("useQlh").GetValue<bool>() && qSpider.IsReady();

            if (Player.IsDead || !Orbwalking.CanMove(40) || !SpiderForm())
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.ServerPosition, qSpider.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minionCount.Count > 0 && useQs)
            {
                foreach (var minion in minionCount)
                {
                    if (minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q, 1))
                    {
                        qSpider.CastOnUnit(minion);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var useQs = config.Item("useQlc").GetValue<bool>() && qSpider.IsReady();
            var useWh = config.Item("useWlcH").GetValue<bool>() && wHuman.IsReady();
            var useWhMP = config.Item("useWlcHMP").GetValue<Slider>().Value;

            if (Player.IsDead || !Orbwalking.CanMove(40))
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Player.ServerPosition, qSpider.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minionCount.Count > 0)
            {
                foreach (var minion in minionCount)
                {
                    if (SpiderForm() && useQs)
                    {
                        qSpider.CastOnUnit(minion);
                    }

                    if (!SpiderForm() && useWh && Player.ManaPercent >= useWhMP && minionCount.Count > config.Item("useWlcMin").GetValue<Slider>().Value)
                    {
                        wHuman.Cast(minion);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            if (Player.IsDead || !config.Item("killsteal").GetValue<bool>())
            {
                return;
            }

            if (config.Item("useQksH").GetValue<bool>() && qHuman.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(qHuman.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.Q));

                if (SpiderForm() && qHumanCD == 0 && Player.Distance(target) < qHuman.Range)
                {
                    r.Cast();
                }

                if (!SpiderForm() && target.IsValidTarget(qHuman.Range))
                {
                    qHuman.CastOnUnit(target);
                }
            }

            if (config.Item("useQksS").GetValue<bool>() && qSpider.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(qSpider.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.Q, 1));

                if (!SpiderForm() && qSpiderCD == 0 && Player.Distance(target) < qSpider.Range)
                {
                    r.Cast();
                }

                if (SpiderForm() && target.IsValidTarget(qSpider.Range))
                {
                    qSpider.CastOnUnit(target);
                }
            }

            if (config.Item("useWksH").GetValue<bool>() && wHuman.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(wHuman.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.W));

                if (SpiderForm() && wHumanCD == 0 && Player.Distance(target) < wHuman.Range)
                {
                    r.Cast();
                }

                if (!SpiderForm() && target.IsValidTarget(wHuman.Range))
                {
                    wHuman.Cast(target);
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

        private static void AutoQ()
        {
            if (Player.IsDead || !config.Item("autoQ").GetValue<KeyBind>().Active || Player.ManaPercent < config.Item("autoQmp").GetValue<Slider>().Value)
            {
                return;
            }

            var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsValidTarget(qHuman.Range));

            if (target.IsValidTarget(qHuman.Range))
            {
                qHuman.CastOnUnit(target);
            }
        }

        private static float HumanDamage(Obj_AI_Base target)
        {
            var comboDamage = 0d;

            if (qHumanCD == 0)
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (wHumanCD == 0)
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);
            }

            return (float) comboDamage;
        }

        private static float SpiderDamage(Obj_AI_Base target)
        {
            var comboDamage = 0d;

            if (qSpiderCD == 0)
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }

            return (float)comboDamage;
        }

        private static void AutoRbase()
        {
            if (Player.IsDead || !r.IsReady())
            {
                return;
            }

            if (Player.InFountain() && !SpiderForm() && config.Item("toSpiderBase").GetValue<bool>())
            {
                r.Cast();
            }
        }

        //Cooldown Tracking. Thanks detuks and Kurisu for the help
        private static readonly float[] qHumanDefaultCD = { 6, 6, 6, 6, 6 };
        private static readonly float[] qSpiderDefaultCD = { 6, 6, 6, 6, 6 };
        private static readonly float[] wHumanDefaultCD = { 12, 12, 12, 12, 12 };
        private static readonly float[] wSpiderDefaultCD = { 12, 12, 12, 12, 12 };
        private static readonly float[] eHumanDefaultCD = { 14, 13, 12, 11, 10 };
        private static readonly float[] eSpiderDefaultCD = { 26, 23, 20, 17, 14 };

        private static float qHumanCD, qSpiderCD, wHumanCD, wSpiderCD, eHumanCD, eSpiderCD;

        private static float qHumanCDremaining,
            qSpiderCDremaining,
            wHumanCDremaining,
            wSpiderCDremaining,
            eHumanCDremaining,
            eSpiderCDremaining;

        public static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs args)
        {
            if (obj.IsMe)
            {
                GetCooldowns(args);
            }
        }

        private static void ProcessCooldowns()
        {
            if (Player.IsDead)
            {
                return;
            }

            qHumanCD = ((qHumanCDremaining - Game.Time) > 0) ? (qHumanCDremaining - Game.Time) : 0;
            wHumanCD = ((wHumanCDremaining - Game.Time) > 0) ? (wHumanCDremaining - Game.Time) : 0;
            eHumanCD = ((eHumanCDremaining - Game.Time) > 0) ? (eHumanCDremaining - Game.Time) : 0;

            qSpiderCD = ((qSpiderCDremaining - Game.Time) > 0) ? (qSpiderCDremaining - Game.Time) : 0;
            wSpiderCD = ((wSpiderCDremaining - Game.Time) > 0) ? (wSpiderCDremaining - Game.Time) : 0;
            eSpiderCD = ((eSpiderCDremaining - Game.Time) > 0) ? (eSpiderCDremaining - Game.Time) : 0;
        }
        
        private static float CalculateSpellCooldown(float time)
        {
            return time + (time * Player.PercentCooldownMod);
        }

        private static void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (SpiderForm())
            {
                if (spell.SData.Name == "EliseSpiderQCast")
                    qSpiderCDremaining = Game.Time + CalculateSpellCooldown(qSpiderDefaultCD[qSpider.Level - 1]);
                
                if (spell.SData.Name == "EliseSpiderW")
                    wSpiderCDremaining = Game.Time + CalculateSpellCooldown(wSpiderDefaultCD[wSpider.Level - 1]);

                if (spell.SData.Name == "EliseSpiderEInitial")
                    eSpiderCDremaining = Game.Time + CalculateSpellCooldown(eSpiderDefaultCD[eSpider.Level - 1]);
            }
            else
            {
                if (spell.SData.Name == "EliseHumanQ")
                    qHumanCDremaining = Game.Time + CalculateSpellCooldown(qHumanDefaultCD[qHuman.Level - 1]);

                if (spell.SData.Name == "EliseHumanW")
                    wHumanCDremaining = Game.Time + CalculateSpellCooldown(wHumanDefaultCD[wHuman.Level - 1]);

                if (spell.SData.Name == "EliseHumanE")
                    eHumanCDremaining = Game.Time + CalculateSpellCooldown(eHumanDefaultCD[eHuman.Level - 1]);
            }
        }
        
        private static bool SpiderForm()
        {
            return Player.Spellbook.GetSpell(SpellSlot.R).Name == "EliseRSpider";
        }

        private static void DrawCooldowns()
        {
            var heroPosition = Drawing.WorldToScreen(Player.Position);

            if (SpiderForm())
            {
                if (qHumanCD == 0)
                {
                    Drawing.DrawText(heroPosition.X - 60, heroPosition.Y - 20, Color.Chartreuse, "Q rdy");
                }
                else
                {
                    Drawing.DrawText(heroPosition.X - 60, heroPosition.Y - 20, Color.OrangeRed, qHumanCD.ToString("0.0"));
                }

                if (wHumanCD == 0)
                {
                    Drawing.DrawText(heroPosition.X, heroPosition.Y - 20, Color.Chartreuse, "W rdy");
                }
                else
                {
                    Drawing.DrawText(heroPosition.X, heroPosition.Y - 20, Color.OrangeRed, wHumanCD.ToString("0.0"));
                }

                if (eHumanCD == 0)
                {
                    Drawing.DrawText(heroPosition.X + 60, heroPosition.Y - 20, Color.Chartreuse, "E rdy");
                }
                else
                {
                    Drawing.DrawText(heroPosition.X + 60, heroPosition.Y - 20, Color.OrangeRed, eHumanCD.ToString("0.0"));
                }
            }
            else
            {
                if (qSpiderCD == 0)
                {
                    Drawing.DrawText(heroPosition.X - 60, heroPosition.Y - 20, Color.Chartreuse, "Q rdy");
                }
                else
                {
                    Drawing.DrawText(heroPosition.X - 60, heroPosition.Y - 20, Color.OrangeRed, qSpiderCD.ToString("0.0"));
                }

                if (wSpiderCD == 0)
                {
                    Drawing.DrawText(heroPosition.X, heroPosition.Y - 20, Color.Chartreuse, "W rdy");
                }
                else
                {
                    Drawing.DrawText(heroPosition.X, heroPosition.Y - 20, Color.OrangeRed, wSpiderCD.ToString("0.0"));
                }

                if (eSpiderCD == 0)
                {
                    Drawing.DrawText(heroPosition.X + 60, heroPosition.Y - 20, Color.Chartreuse, "E rdy");
                }
                else
                {
                    Drawing.DrawText(heroPosition.X + 60, heroPosition.Y - 20, Color.OrangeRed, eSpiderCD.ToString("0.0"));
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var disableDraw = config.Item("disableDraw").GetValue<bool>();
            var drawCD = config.Item("drawCD").GetValue<bool>();
            var drawQhuman = config.Item("drawQh").GetValue<Circle>();
            var drawWhuman = config.Item("drawWh").GetValue<Circle>();
            var drawEhuman = config.Item("drawEh").GetValue<Circle>();
            var drawQspider = config.Item("drawQs").GetValue<Circle>();
            var drawEspider = config.Item("drawEs").GetValue<Circle>();
            var width = config.Item("width").GetValue<Slider>().Value;

            if (disableDraw)
            {
                return;
            }

            if (drawCD)
            {
                DrawCooldowns();
            }

            if (!SpiderForm())
            {
                if (drawQhuman.Active && qHuman.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, drawQhuman.Radius, drawQhuman.Color, width);
                }

                if (drawWhuman.Active && wHuman.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, drawWhuman.Radius, drawWhuman.Color, width);
                }

                if (drawEhuman.Active && eHuman.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, drawEhuman.Radius, drawEhuman.Color, width);
                }
            }

            if (SpiderForm())
            {
                if (drawQspider.Active && qSpider.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, drawQspider.Radius, drawQspider.Color, width);
                }

                if (drawEspider.Active && eSpider.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, drawEspider.Radius, drawEspider.Color, width);
                }
            }
        }
    }
}