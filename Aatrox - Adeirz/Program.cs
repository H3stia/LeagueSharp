#region

using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

#endregion

namespace Aatrox
{
    internal class Program
    {
        public const string ChampionName = "Aatrox";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        private static Items.Item HDR, BKR, TMT, BWC, YOU;

        public static bool WHealing;

        public static SpellSlot IgniteSlot;

        public static Menu Config;
        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, Player.AttackRange + 25);
            E = new Spell(SpellSlot.E, 950); //1000?
            R = new Spell(SpellSlot.R, 550); //300?

            Q.SetSkillshot(0.5f, 180f, 1800f, false, SkillshotType.SkillshotCircle); //width tuned
            E.SetSkillshot(0.5f, 150f, 1200f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            HDR = new Items.Item(3074, 175f);
            TMT = new Items.Item(3077, 175f);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);

            Config = new Menu(ChampionName, ChampionName, true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("NoQNear", "Don't use Q near").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("EbeforeQ", "Use E before Q").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            Config.SubMenu("Combo").AddItem(new MenuItem("spacer", "--- Additional ---"));

            Config.SubMenu("Combo").AddItem(new MenuItem("SwitchLife", "Change to Life").SetValue(new Slider(40, 1, 100)));
            Config.SubMenu("Combo").AddItem(new MenuItem("SwitchPower", "Change to Power").SetValue(new Slider(55, 1, 100)));

            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "Combo!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(
                        new KeyBind('T', KeyBindType.Press, false)));

            //Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt spells with Q").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealE", "Killsteal with E").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(false));

            //Drawings menu:
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //Add the events we are going to use:
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Game.PrintChat(ChampionName + " by Ecko loaded!");
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;

            if (Player.Distance(unit) < Q.Range && Q.IsReady())
            {
                Q.Cast(unit, false, true);
            }
        }

        private static float GetComboDamage(Obj_AI_Base qTarget)
        {
            var ComboDamage = 0d;

            if (Q.IsReady())
                ComboDamage += Player.GetSpellDamage(qTarget, SpellSlot.Q);

            if (R.IsReady())
                ComboDamage += Player.GetSpellDamage(qTarget, SpellSlot.R);

            if (E.IsReady())
                ComboDamage += Player.GetSpellDamage(qTarget, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                ComboDamage += Player.GetSummonerSpellDamage(qTarget, Damage.SummonerSpell.Ignite);

            return (float)ComboDamage;
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            bool useQ = Config.Item("UseQCombo").GetValue<bool>();
            bool useW = Config.Item("UseWCombo").GetValue<bool>();
            bool useE = Config.Item("UseECombo").GetValue<bool>();
            bool useR = Config.Item("UseRCombo").GetValue<bool>();

            if (Config.Item("UseItems").GetValue<bool>())
            {
                BKR.Cast(qTarget);
                YOU.Cast();
                BWC.Cast(qTarget);
                if (Player.Distance(qTarget) <= HDR.Range)
                {
                    HDR.Cast(qTarget);
                }
                if (Player.Distance(qTarget) <= TMT.Range)
                {
                    TMT.Cast(qTarget);
                }
            }


            if (Q.IsReady() && E.IsReady() && R.IsReady() && GetComboDamage(qTarget) >= qTarget.Health)
            {

                if (qTarget != null && useQ && Q.IsReady())
                    if ((Config.Item("NoQNear").GetValue<bool>() && Player.Distance(qTarget) < Player.AttackRange + 50)
                        || (Config.Item("EbeforeQ").GetValue<bool>() && E.IsReady()))
                        return; 
                    Q.Cast(qTarget, true, true);

                if (rTarget != null && useR && R.IsReady())
                    R.Cast(rTarget, true);

                if (useW && W.IsReady())
                {
                    if (Player.Health < (Player.MaxHealth * (Config.Item("SwitchLife").GetValue<Slider>().Value) * 0.01) && !WHealing)
                        W.Cast();
                    else
                        if (Player.Health > (Player.MaxHealth * (Config.Item("SwitchPower").GetValue<Slider>().Value) * 0.01) && WHealing)
                            W.Cast();
                }

                if (eTarget != null && useE && E.IsReady())
                    E.Cast(eTarget, true);

            }
            else
            {
                if (qTarget != null && useQ && Q.IsReady())
                    if ((Config.Item("NoQNear").GetValue<bool>() && Player.Distance(qTarget) < Player.AttackRange + 50)
                        || (Config.Item("EbeforeQ").GetValue<bool>() && E.IsReady()))
                        return; 
                    Q.Cast(qTarget, true, true);

                if (useW && W.IsReady())
                {
                    if (Player.Health < (Player.MaxHealth * 0.4) && !WHealing)
                        W.Cast();
                    else
                        if (Player.Health > (Player.MaxHealth * 0.55) && WHealing)
                            W.Cast();
                }

                if (eTarget != null && useE && E.IsReady())
                    E.Cast(eTarget, true);
            }
        }

        private static void Harass()
        {
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            bool useE = Config.Item("UseEHarass").GetValue<bool>();

            if (useE && eTarget != null && E.IsReady())
                E.Cast(eTarget, true);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //foreach (var buff in ObjectManager.Player.Buffs) { Game.PrintChat(buff.DisplayName); }
            CheckWHealing();
            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);

            var useEKS = Config.Item("KillstealE").GetValue<bool>() && E.IsReady();
            var useRKS = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active) 
                Combo();
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                Harass();
            if (useRKS && !Config.Item("ComboActive").GetValue<KeyBind>().Active) 
                KillstealR();
            if (useEKS)
                KillstealE();
        }

        private static void CheckWHealing()
        {
            foreach (var buff in ObjectManager.Player.Buffs) 
            {
                if (buff.DisplayName == "AatroxWLife")
                    WHealing = true;
                else if (buff.DisplayName == "AatroxWPower")
                    WHealing = false;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void KillstealE()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range)))
            {
                if (E.IsReady() && hero.Distance(ObjectManager.Player) <= E.Range &&
                    Damage.GetSpellDamage(Player, hero, SpellSlot.E) >= hero.Health+20)
                    E.CastIfHitchanceEquals(hero, HitChance.High, true);
            }
        }

        private static void KillstealR()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    Damage.GetSpellDamage(Player, hero, SpellSlot.R) >= hero.Health + 20)
                    R.Cast();
            }
        }
    }
}
