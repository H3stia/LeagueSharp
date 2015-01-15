using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace Nautilus
{
    class Program
    {
        //Spells
        public static Spell Q, W, E, R;

        //Targets
        public static Obj_AI_Hero Target;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        //Menu
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        //Ignite
        public static SpellDataInst Ignite;

        public static void Main(string[] args)
        {
            Utils.ClearConsole();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals("Nautilus"))
                return;


            #region Spell Data

            Q = new Spell(SpellSlot.Q, 1100);
            Q.SetSkillshot(0.250f, 90, 2000, true, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E, 600);

            R = new Spell(SpellSlot.R, 825);

            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            #endregion

            #region Config Menu

            //Menu
            Config = new Menu("Nautilus - Depths Terror", "Nautilus", true);

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
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var comboW = combo.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));

            var comboE = combo.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));

            var comboR = combo.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboR.AddItem(new MenuItem("minRhealth", "Min target Health % for R ")).SetValue(new Slider(60, 1));
            comboR.AddSubMenu(new Menu("Don't use Ult on", "DontUlt"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
            {
                Config.SubMenu("Combo")
                    .SubMenu("R")
                    .SubMenu("DontUlt")
                    .AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            }

            combo.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass Menu
            var harass = Config.AddSubMenu(new Menu("Harass Settings", "Harass"));

            var harassQ = harass.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassQ.AddItem(
                new MenuItem("qHitchanceH", "Q Hitchance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var harassE = harass.AddSubMenu(new Menu("E Settings", "E"));
            harassE.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind((byte)'C', KeyBindType.Press)));

            //Killsteal Menu
            var killsteal = Config.AddSubMenu(new Menu("KillSteal Settings", "KillSteal"));
            killsteal.AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseEKS", "Use E to KillSteal").SetValue(true));
            killsteal.AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            //Misc Menu
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
            misc.AddItem(new MenuItem("wTargetted", "Shield targetted spells").SetValue(true));
            

            //Drawings Menu
            var drawings = Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            drawings.AddItem(
                new MenuItem("DrawQ", "Q range").SetValue(new Circle(true, Color.CornflowerBlue, Q.Range)));
            drawings.AddItem(
                new MenuItem("DrawE", "E range").SetValue(new Circle(false, Color.CornflowerBlue, E.Range)));
            drawings.AddItem(
                new MenuItem("DrawR", "R range").SetValue(new Circle(false, Color.CornflowerBlue, R.Range)));

            Config.AddToMainMenu();

            #endregion


            Game.PrintChat("<b><font color =\"#4980E6\">Nautilus - Depths Terror </font><font color=\"#FFFFFF\">by Hestia loaded!</font>");
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        #region Interrupter

        //Interrup spells with Q and R
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base intTarget, InterruptableSpell args)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>())
                return;

            if (Player.Distance(intTarget) < Q.Range)
            {
                Q.Cast(intTarget);
            }

            if (Player.Distance(intTarget) < R.Range)
            {
                R.CastOnUnit(intTarget);
            }
        }

        #endregion

        #region Shield Targetted

        //W Targetted Spells - Credits @legacy @Kortatu
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid<Obj_AI_Hero>())
            {
                return;
            }

            if (!Config.Item("wTargetted").GetValue<bool>())
            {
                return;
            }

            if (!sender.IsEnemy)
            {
                return;
            }

            if (args.Target == null || !args.Target.IsValid || !args.Target.IsMe)
            {
                return;
            }

            if (args.SData.IsAutoAttack())
            {
                return;
            }

            //Delay the Cast a bit to make it look more human
            Utility.DelayAction.Add(100, () => W.Cast());
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
            }
            return HitChance.Medium;
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
            var castE = Config.Item("UseECombo").GetValue<bool>() && E.IsReady();
            var castR = Config.Item("UseRCombo").GetValue<bool>() && R.IsReady();

            if (castQ && Q.IsInRange(Target, 1050))
            {
                Q.CastIfHitchanceEquals(Target, GetHitChance("qHitchance"));
            }

            if (castW && (Player.Distance(Target) < 250))
            {
                W.Cast();
            }

            if (castE && E.IsInRange(Target, 550))
            {
                E.Cast();
            }

            if (castR && R.IsInRange(Target, R.Range))
            {
                CastR();
            }
        }

        //Ulti
        private static void CastR()
        {
            var useR = (Config.Item("DontUlt" + Target.BaseSkinName) != null && Config.Item("DontUlt" + Target.BaseSkinName).GetValue<bool>() == false);
            var minHp = Config.Item("minRhealth").GetValue<Slider>().Value;
            if (useR && (Player.Distance(Target) < R.Range) && (Target.HealthPercentage() <= minHp))
                R.CastOnUnit(Target);
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
            var castE = Config.Item("UseEHarass").GetValue<bool>() && E.IsReady();

            if (castQ && Q.IsInRange(Target, 1050))
            {
                Q.CastIfHitchanceEquals(Target, GetHitChance("qHitchanceH"));
            }

            if (castE && E.IsInRange(Target, 550))
            {
                E.Cast();
            }

        }

        #endregion

        #region KillSteal

        //KillSteal
        private static void KillSteal()
        {
            if (!Config.Item("Killsteal").GetValue<bool>())
                return;

            Eks();
            Iks();
        }

        //E Killsteal
        private static void Eks()
        {
            //E killsteal
            if (!Config.Item("UseEKS").GetValue<bool>())
                return;

            var unit = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(obj => obj.IsValidTarget(E.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.E));

            if (!unit.IsValidTarget(E.Range))
                return;

            E.Cast();
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

        #region Events

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            KillSteal();

            Target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);

            if (!Target.IsValidTarget(1300))
                return;

            ExecuteCombo();
            ExecuteHarass();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var circle in new List<string> { "Q", "E", "R" }.Select(spell => Config.Item("Draw" + spell).GetValue<Circle>()).Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        #endregion
    }
}
