using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Gragas
{
    class Program
    {
        /// <summary>
        ///     The champion name.
        /// </summary>
        private const string Champion = "Gragas";

        /// <summary>
        ///     Gets the player.
        /// </summary>
        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     The target.
        /// </summary>
        private static Obj_AI_Hero target;

        /// <summary>
        ///     The menu.
        /// </summary>
        private static Menu config;

        /// <summary>
        ///     The orbwalker.
        /// </summary>
        private static Orbwalking.Orbwalker orbwalker;

        /// <summary>
        ///     The spells.
        /// </summary>
        private static Spell q, w, e, r;

        /// <summary>
        ///     Ignite.
        /// </summary>
        private static SpellDataInst ignite;

        /// <summary>
        ///     Gragas' Barrel
        /// </summary>
        private static GameObject barrel;

        /// <summary>
        ///     The main entry point.
        /// </summary>
        /// <param name="args">
        ///     The data transferred to the main entry point.
        /// </param>
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        ///     The OnGameLoad event.
        /// </summary>
        /// <param name="args">
        ///     The event data.
        /// </param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != Champion)
            {
                return;
            }

            q = new Spell(SpellSlot.Q, 850);
            q.SetSkillshot(0.3f, 110f, 850f, false, SkillshotType.SkillshotCircle);
            w = new Spell(SpellSlot.W, 0);
            e = new Spell(SpellSlot.E, 600);
            e.SetSkillshot(0.3f, 20f, 1000, true, SkillshotType.SkillshotLine);
            r = new Spell(SpellSlot.R, 1050);
            r.SetSkillshot(0.3f, 120f, 1000, false, SkillshotType.SkillshotCircle);
            ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            config = new Menu(Player.ChampionName, Player.ChampionName, true);

            var orbwalkerMenu = config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var tsMenu = config.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(tsMenu);

            var combo = config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            var comboQ = combo.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));

            var comboW = combo.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddItem(new MenuItem("useW", "Use W").SetValue(true));

            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboE.AddItem(
                new MenuItem("eHitchance", "E Hitchance").SetValue(
                    new StringList(
                        new[]
                        {
                            HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                            HitChance.VeryHigh.ToString()
                        }, 2)));

            //var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            //comboR.AddItem(new MenuItem("useR", "Use R").SetValue(true));

            var harass = config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("useQh", "Use Q").SetValue(true));

            var insec = config.AddSubMenu(new Menu("Insec Settings", "Insec"));
            insec.AddItem(new MenuItem("insec", "Insec Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            insec.AddItem(
                new MenuItem("insecMode", "Insec Mode").SetValue(
                    new StringList(new[] { "To Ally", "To Mouse", "To Turret" })));

            var laneClear = config.AddSubMenu(new Menu("Farm Settings", "Farm"));
            laneClear.AddItem(new MenuItem("useQlc", "Use Q to lane clear").SetValue(true)); 
            laneClear.AddItem(new MenuItem("qLaneClearMP", "Minimum mana % to use Q").SetValue(new Slider(60)));
            laneClear.AddItem(new MenuItem("qLaneClearMinions", "Minimum minions hit to use Q").SetValue(new Slider(3, 1, 10)));
            laneClear.AddItem(new MenuItem("useWlc", "Use W to lane clear").SetValue(true));
            laneClear.AddItem(new MenuItem("useElc", "Use E to lane clear").SetValue(false));
            laneClear.AddItem(new MenuItem("eLaneClearMP", "Minimum mana % to use E").SetValue(new Slider(60)));

            var jungleClear = config.AddSubMenu(new Menu("Jungle Settings", "Jungle"));
            jungleClear.AddItem(new MenuItem("useQj", "Use Q").SetValue(true));
            jungleClear.AddItem(new MenuItem("qJungleMP", "Minimum mana % to use Q").SetValue(new Slider(15)));
            jungleClear.AddItem(new MenuItem("useWj", "Use W").SetValue(true));
            jungleClear.AddItem(new MenuItem("useEj", "Use E").SetValue(false));
            jungleClear.AddItem(new MenuItem("eJungleMP", "Minimum mana % to use E").SetValue(new Slider(15)));
            jungleClear.AddItem(
                new MenuItem("jungleActive", "Jungle Clear!").SetValue(
                    new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var misc = config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("antiGapE", "Use E as anti gapcloser").SetValue(true));
            misc.AddItem(new MenuItem("antiGapR", "Use R as anti gapcloser").SetValue(false));
            misc.AddItem(new MenuItem("interruptE", "Use E to interrupt").SetValue(true));
            misc.AddItem(new MenuItem("interruptR", "Use R to interrupt").SetValue(false));

            var killsteal = config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useQks", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useEks", "Use E to KillSteal").SetValue(false));
            killsteal.AddItem(new MenuItem("useRks", "Use R to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("useIks", "Use Ignite to KillSteal").SetValue(true));

            var drawingMenu = config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawingMenu.AddItem(new MenuItem("disableDraw", "Disable all drawings").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.DarkOrange, q.Range)));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(new Circle(true, Color.DarkOrange, e.Range)));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(new Circle(true, Color.DarkOrange, r.Range)));
            drawingMenu.AddItem(new MenuItem("drawInsec", "Draw Insec").SetValue(true));
            drawingMenu.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));
            drawingMenu.AddItem(new MenuItem("drawDmg", "Draw damage on Healthbar").SetValue(true));

            config.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            AntiGapcloser.OnEnemyGapcloser += OnGapCloser;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
        }

        /// <summary>
        ///     Allows to set the user selected hitchance from the menu.
        /// </summary>
        /// <param name="name">
        ///     The name of the menu item for the hitchance selection.
        /// </param>
        /// <returns>
        ///     The selected hitchance.
        /// </returns>
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

        /// <summary>
        ///     The OnInterruptableTarget event.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The event data.
        /// </param>
        private static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var useE = config.Item("interruptE").GetValue<bool>() && e.IsReady();
            var useR = config.Item("interruptR").GetValue<bool>() && r.IsReady();

            if (useE && sender.IsValidTarget(e.Range))
            {
                e.Cast(sender);
            }

            if (useR && sender.IsValidTarget(r.Range))
            {
                r.Cast(sender);
            }
        }

        /// <summary>
        ///     The OnGapCloser event.
        /// </summary>
        /// <param name="gapcloser">
        ///     The gapcloser.
        /// </param>
        private static void OnGapCloser(ActiveGapcloser gapcloser)
        {
            var useE = config.Item("antiGapE").GetValue<bool>() && e.IsReady();
            var useR = config.Item("antiGapR").GetValue<bool>() && r.IsReady();

            if (useE && gapcloser.Sender.IsValidTarget(e.Range))
            {
                e.Cast(gapcloser.Sender);
            }

            if (useR && gapcloser.Sender.IsValidTarget(r.Range))
            {
                r.Cast(gapcloser.Sender);
            }
        }

        /// <summary>
        ///     The OnUpdate event.
        /// </summary>
        /// <param name="args">
        ///     The event data.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);

            CastSecondQ();

            if (config.Item("insec").GetValue<KeyBind>().Active)
            {
                Insec(target);
            }

            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ExecuteCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
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
        }

        /// <summary>
        ///     Execute the combo.
        /// </summary>
        private static void ExecuteCombo()
        {
            var enemies = Utility.CountEnemiesInRange(800);

            var useQ = config.Item("useQ").GetValue<bool>();
            var useW = config.Item("useW").GetValue<bool>();
            var useE = config.Item("useE").GetValue<bool>();

            if (useW && w.IsReady() && enemies > 0)
            {
                w.Cast();
            }

            if (useQ && target.IsValidTarget(q.Range) && barrel == null)
            {
                q.Cast(target);
            }

            if (useE && target.IsValidTarget(e.Range))
            {
                e.CastIfHitchanceEquals(target, GetHitChance("eHitchance"));
            }
        }

        /// <summary>
        ///     Execute the harass.
        /// </summary>
        private static void ExecuteHarass()
        {
            var useQ = config.Item("useQh").GetValue<bool>();

            if (useQ && target.IsValidTarget(q.Range) && barrel == null)
            {
                q.Cast(target);
            }
        }

        /// <summary>
        ///     Lane Clear
        /// </summary>
        private static void LaneClear()
        {
            if (Player.IsDead)
            {
                return;
            }

            var useQ = config.Item("useQlc").GetValue<bool>();
            var qMana = config.Item("qLaneClearMP").GetValue<Slider>().Value;
            var qMinion = config.Item("qLaneClearMinions").GetValue<Slider>().Value;
            var useW = config.Item("useWlc").GetValue<bool>();
            var useE = config.Item("useElc").GetValue<bool>();
            var eMana = config.Item("eLaneClearMP").GetValue<Slider>().Value;

            var minions = MinionManager.GetMinions(Player.ServerPosition, q.Range);

            var qPos = q.GetCircularFarmLocation(minions, q.Width);
            var ePos = e.GetLineFarmLocation(minions, e.Width);

            if (useQ && Player.ManaPercent >= qMana && qPos.MinionsHit >= qMinion)
            {
                q.Cast(qPos.Position);
                Utility.DelayAction.Add(450, () => q.Cast());
            }

            if (useW && minions.Count >= 1)
            {
                w.Cast();
            }

            if (useE && Player.ManaPercent >= eMana && ePos.MinionsHit >= 2)
            {
                e.Cast(ePos.Position);
            }
        }

        /// <summary>
        ///     Jungle Clear.
        /// </summary>
        private static void JungleClear()
        {
            if (Player.IsDead)
            {
                return;
            }

            var useQ = config.Item("useQj").GetValue<bool>();
            var qMana = config.Item("qJungleMP").GetValue<Slider>().Value;
            var useW = config.Item("useWj").GetValue<bool>();
            var useE = config.Item("useEj").GetValue<bool>();
            var eMana = config.Item("eJungleMP").GetValue<Slider>().Value;

            var minions = MinionManager.GetMinions(Player.ServerPosition, q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var qPos = q.GetCircularFarmLocation(minions, q.Width);
            var ePos = e.GetLineFarmLocation(minions, e.Width);

            if (useQ && Player.ManaPercent >= qMana && qPos.MinionsHit >= 1)
            {
                q.Cast(qPos.Position);
                Utility.DelayAction.Add(450, () => q.Cast());
            }

            if (useW && minions.Count >= 1)
            {
                w.Cast();
            }

            if (useE && Player.ManaPercent >= eMana && ePos.MinionsHit >= 1)
            {
                e.Cast(ePos.Position);
            }
        }

        /// <summary>
        ///     Cast the second Q.
        /// </summary>
        private static void CastSecondQ()
        {
            if (!q.IsReady())
            {
                return;
            }

            if (barrel != null)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Distance(barrel.Position) >= 250 && enemy.Distance(barrel.Position) <= 350 && enemy.IsEnemy))
                {
                    q.Cast(enemy);
                }
            }
        }

        /// <summary>
        ///     Get the insec position.
        /// </summary>
        /// <remarks> 
        ///     Thanks to yol0 for the insec methods.
        /// </remarks> 
        /// <param name="insecTarget">
        ///     The insec target.
        /// </param>
        /// <returns>
        ///     The position.
        /// </returns>
        private static Vector2 GetInsecPosition(Obj_AI_Hero insecTarget)
        {
            if (config.Item("insecMode").GetValue<StringList>().SelectedValue == "To Ally")
            {
                var nearestTurret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(obj => obj.IsAlly)
                        .OrderBy(obj => Player.Distance(obj.Position))
                        .ToList()[0];
                var allies =
                    HeroManager.Allies.Where(hero => hero.Distance(insecTarget.Position) <= 1500)
                        .OrderByDescending(hero => hero.Distance(insecTarget.Position))
                        .ToList();
                if (allies.Any() && !allies[0].IsMe)
                {
                    var directionVector = (insecTarget.Position - allies[0].Position).Normalized().To2D();
                    return insecTarget.Position.To2D() + (directionVector * 250);
                }
                var dirVector = (insecTarget.Position - nearestTurret.Position).Normalized().To2D();
                return insecTarget.Position.To2D() + (dirVector * 250);
            }
            if (config.Item("insecMode").GetValue<StringList>().SelectedValue == "To Mouse")
            {
                var directionVector = (insecTarget.Position - Game.CursorPos).Normalized().To2D();
                return insecTarget.Position.To2D() + (directionVector * 250);
            }
            var nearTurret =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(obj => obj.IsAlly)
                    .OrderBy(obj => Player.Distance(obj.Position))
                    .ToList()[0];
            var dVector = (insecTarget.Position - nearTurret.Position).Normalized().To2D();
            return insecTarget.Position.To2D() + (dVector * 250);
        }

        /// <summary>
        ///     Execute the Insec Combo.
        /// </summary>
        /// <remarks> 
        ///     Thanks to yol0 for the insec methods.
        /// </remarks> 
        /// <param name="insecTarget">
        ///     The insec target.
        /// </param>
        private static void Insec(Obj_AI_Hero insecTarget)
        {
            if (!r.IsReady())
            {
                return;
            }

            if (!insecTarget.IsValidTarget() || insecTarget.IsDead)
            {
                return;
            }

            var ePred = e.GetPrediction(insecTarget);
            var insecPos = GetInsecPosition(insecTarget);
            if (Player.Distance(insecPos) < 900)
            {
                r.Cast(insecPos);
                return;
            }
            
            if (Player.Distance(insecPos) > 900 && e.IsReady())
            {
                if ((Player.Distance(insecPos) - e.Range) < 900 && ePred.Hitchance != HitChance.Collision)
                {
                    e.Cast(insecPos);
                    r.Cast(insecPos);
                }
            }
        }

        /// <summary>
        ///     Kill Steal.
        /// </summary>
        private static void KillSteal()
        {
            if (Player.IsDead || !config.Item("killsteal").GetValue<bool>())
            {
                return;
            }

            if (config.Item("useQks").GetValue<bool>() && q.IsReady())
            {
                var qTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(q.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.Q));
                if (qTarget.IsValidTarget(q.Range))
                {
                    q.CastIfHitchanceEquals(qTarget, HitChance.High);
                }
            }

            if (config.Item("useEks").GetValue<bool>() && e.IsReady())
            {
                var eTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(e.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.E));
                if (eTarget.IsValidTarget(e.Range))
                {
                    e.CastIfHitchanceEquals(eTarget, HitChance.High);
                }
            }

            if (config.Item("useRks").GetValue<bool>() && r.IsReady())
            {
                var rTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(r.Range) && enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.R));
                if (rTarget.IsValidTarget(r.Range))
                {
                    r.CastIfHitchanceEquals(rTarget, HitChance.High);
                }
            }

            if (config.Item("useIks").GetValue<bool>() && ignite.Slot.IsReady() && ignite != null && ignite.Slot != SpellSlot.Unknown)
            {
                var iTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(600) &&
                                enemy.Health < Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite));

                if (iTarget.IsValidTarget(600))
                {
                    Player.Spellbook.CastSpell(ignite.Slot, iTarget);
                }
            }
        }

        /// <summary>
        ///     The OnDraw event.
        /// </summary>
        /// <param name="args">
        ///     The event data.
        /// </param>
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

            if (config.Item("drawE").GetValue<Circle>().Active && e.Level > 0)
            {
                var circle = config.Item("drawE").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }

            if (config.Item("drawR").GetValue<Circle>().Active && r.Level > 0)
            {
                var circle = config.Item("drawR").GetValue<Circle>();
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }

            if (config.Item("drawInsec").GetValue<bool>() && target != null && !target.IsDead &&
                Hud.SelectedUnit != null && target.NetworkId == Hud.SelectedUnit.NetworkId && r.IsReady())
            {
                var insecPos = GetInsecPosition(target);
                Render.Circle.DrawCircle(insecPos.To3D(), 40f, Color.Green, width);
                var dirPos = (target.Position.To2D() - insecPos).Normalized();
                var endPos = target.Position.To2D() + (dirPos * 1200);

                var wts1 = Drawing.WorldToScreen(insecPos.To3D());
                var wts2 = Drawing.WorldToScreen(endPos.To3D());

                Drawing.DrawLine(wts1, wts2, 2, Color.Green);
            }
        }

        /// <summary>
        ///     The OnCreate event.
        /// </summary>
        /// <param name="obj">
        ///     The game object.
        /// </param>
        /// <param name="args">
        ///     The event data.
        /// </param>
        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.Name == "Gragas_Base_Q_Ally.troy")
            {
                barrel = obj;
            }
        }

        /// <summary>
        ///     The OnDelete event.
        /// </summary>
        /// <param name="obj">
        ///     The game object.
        /// </param>
        /// <param name="args">
        ///     The event data.
        /// </param>
        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name == "Gragas_Base_Q_Ally.troy")
            {
                barrel = null;
            }
        }
    }
}
