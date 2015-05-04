using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Kayle
{
    class Program
    {

        //Spells
        public static Spell Q, W, E, R;

        //Player
        public static Obj_AI_Hero Player;

        //Menu
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        //Ignite
        public static SpellDataInst Ignite;

        public static Activator Activator;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region GameLoad

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals("Kayle"))
                return;

            #region Spells

            Q = new Spell(SpellSlot.Q, 650, TargetSelector.DamageType.Magical);

            W = new Spell(SpellSlot.W, 900);

            E = new Spell(SpellSlot.E, 525);

            R = new Spell(SpellSlot.R, 900);

            #endregion

            #region Config Menu

            //Menu
            Config = new Menu("Kayle", "Kayle", true);

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

            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));

            combo.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass Menu
            var harass = Config.AddSubMenu(new Menu("Harass Settings", "Harass"));

            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));

            var harassE = harass.AddSubMenu(new Menu("E Settings", "E"));
            harassE.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));

            //Killsteal Menu
            var killsteal = Config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            var laneclear = Config.AddSubMenu(new Menu("LaneClear Settings", "LaneClear"));
            laneclear.AddItem(new MenuItem("Efarm", "Use E to lane clear").SetValue(true));
            laneclear.AddItem(new MenuItem("EfarmCount", "Min number of minions to use E").SetValue(new Slider(3, 1, 10)));

            //Misc Menu
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("QLastHit", "Use Q to last hit minions").SetValue(false));
            
            var miscHeal = misc.AddSubMenu(new Menu("Heal Settings", "Heal"));
            miscHeal.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            miscHeal.AddItem(new MenuItem("minManaW", "Min mana % to use W").SetValue(new Slider(50, 0, 100)));
            foreach (var champion in ObjectManager.Get<Obj_AI_Hero>().Where(champion => champion.IsAlly))
            {
                miscHeal.AddItem(new MenuItem("UseW" + champion.ChampionName, champion.ChampionName).SetValue(false));
                miscHeal.AddItem(new MenuItem("UseWpercent" + champion.ChampionName, champion.ChampionName + "if HP% less than %").SetValue(new Slider(45, 0, 100)));
            }

            var miscR = misc.AddSubMenu(new Menu("Ultimate Settings", "Ultimate"));
            miscR.AddItem(new MenuItem("UseR", "Use R for dangerous spells").SetValue(true));
            foreach (var i in Activator.BuffList)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    if (i.ChampionName == enemy.ChampionName)
                    {
                        miscR.AddItem(new MenuItem(i.BuffName, i.DisplayName).SetValue(i.DefaultValue));
                    }
                }
            }

            //Drawings Menu
            var drawings = Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawings.AddItem(new MenuItem("DrawQ", "Q range").SetValue(new Circle(true, Color.CornflowerBlue, Q.Range)));
            drawings.AddItem(new MenuItem("DrawW", "W range").SetValue(new Circle(false, Color.CornflowerBlue, W.Range)));
            drawings.AddItem(new MenuItem("DrawE", "E range").SetValue(new Circle(false, Color.CornflowerBlue, E.Range)));
            drawings.AddItem(new MenuItem("DrawR", "R range").SetValue(new Circle(true, Color.CornflowerBlue, R.Range)));

            Config.AddToMainMenu();

            #endregion

            Notifications.AddNotification("Kayle by Hestia loaded!", 5000);
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling() || MenuGUI.IsChatOpen)
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

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }

            Heals();
            Ultimate();
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
            var castE = Config.Item("UseECombo").GetValue<bool>() && E.IsReady();

            if (castQ && target.IsValidTarget())
            {
                Q.CastOnUnit(target);
            }

            if (castE && target.IsValidTarget())
            {
                E.Cast();
            }
        }

        #endregion


        private static void ExecuteHarass()
        {
            
        }

        private static void Heals()
        {
            var castW = Config.Item("UseW").GetValue<bool>() && (((int) ((Player.Mana/Player.MaxMana)*100)) >= Config.Item("minManaW").GetValue<Slider>().Value) && W.IsReady();

            if (castW)
            {
                var healTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(target => target.IsValidTarget(W.Range) && target.IsAlly)
                        .OrderBy(healthPercent => (healthPercent.Health / healthPercent.MaxHealth) * 100)
                        .First();

                if (Config.Item("UseW" + healTarget.ChampionName).GetValue<bool>())
                {
                    var hpThreshold = Config.Item("UseWpercent" + healTarget.ChampionName).GetValue<Slider>().Value;

                    if (healTarget.Health <= hpThreshold)
                    {
                        W.CastOnUnit(healTarget);
                    }
                }
            }
        }

        private static void Ultimate()
        {
            if (Player.IsDead || !Config.Item("UseR").GetValue<bool>() || R.Level == 0)
                return;

            foreach (var buff in Player.Buffs)
            {
                foreach (var i in Activator.BuffList)
                {
                    if (buff.Name == i.BuffName && Config.Item(i.BuffName).GetValue<bool>())
                    {
                        if (i.Delay > 0)
                        {
                            Utility.DelayAction.Add((i.Delay*1000)/2, () => R.CastOnUnit(Player));
                        }
                        else
                        {
                            R.CastOnUnit(Player);
                        }
                    }
                }
            }
        }

        #region Farming

        private static void LastHit()
        {
            var lastHitQ = Config.Item("QLastHit").GetValue<bool>() && Q.IsReady();

            var minion =
                MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth)
                    .Cast<Obj_AI_Minion>()
                    .FirstOrDefault(minions => Q.IsKillable(minions));

            if (lastHitQ && minion != null)
            {
                Q.CastOnUnit(minion);
            }
        }

        private static void LaneClear()
        {
            var laneClearE = Config.Item("Efarm").GetValue<bool>() && E.IsReady();
            var minMinionCount = Config.Item("EfarmCount").GetValue<Slider>().Value;

            var minionsCount =
                MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).Count;

            if (laneClearE && minionsCount >= minMinionCount)
            {
                E.Cast();
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
            if (Player.IsDead)
                return;

            foreach (var circle in new List<string> { "Q", "W", "E", "R" }.Select(spell => Config.Item("Draw" + spell).GetValue<Circle>()).Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        #endregion
    }
}
