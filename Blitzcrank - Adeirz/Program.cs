#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Blitzcrank
{
    internal class Program
    {
        public const string ChampionName = "Blitzcrank";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell E;
        public static Spell R;

        //Menu
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
            Q = new Spell(SpellSlot.Q, 1000);
            E = new Spell(SpellSlot.E, Player.AttackRange+50);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Add the target selector to the menu as submenu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Load the orbwalker and add it to the menu as submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo menu:
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Qhitchance", "Q HitChance")).SetValue(new StringList(new string[] {"Low", "Medium", "High", "Very High"}));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            Config.SubMenu("Combo").AddItem(new MenuItem("spacer", "--- Additional ---"));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoUlt", "Auto Ultimate").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("CountR", "N. Enemy in Range to AutoUlt").SetValue(new Slider(1, 5, 0)));

            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt spells with R").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("APToggle", "Auto Pull on stun").SetValue(true));
            Config.SubMenu("Misc").AddSubMenu(new Menu("Autopull", "AutoPull"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu("Misc").SubMenu("AutoPull").AddItem(new MenuItem("AutoPull" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            //Drawings menu:
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();


            //Add the events we are going to use:
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Game.PrintChat(ChampionName + " Loaded!");
        }


        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && R.IsReady())
            {
                R.Cast();
            }
        }

        private static void Combo()
        {
            Orbwalker.SetAttacks(true);

            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            bool useQ = Config.Item("UseQCombo").GetValue<bool>();
            bool useE = Config.Item("UseECombo").GetValue<bool>();
            bool useR = Config.Item("UseRCombo").GetValue<bool>();

            //Init of the combo. Q Grab.
                var qMode = Config.Item("Qhitchance").GetValue<StringList>().SelectedIndex;
                if (qTarget != null && useQ && Q.IsReady() && Player.Distance(qTarget) < Q.Range)
                {
                    switch (qMode)
                    {
                        case 1://Low
                            Q.Cast(qTarget);
                            break;
                        case 2://Medium
                            Q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                            break;
                        case 3://High
                            Q.CastIfHitchanceEquals(qTarget, HitChance.High);
                            break;
                        case 4://VeryHigh
                            Q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
                            break;
                    }
            }

            //AutoE when you pull the enemy. Q-E Combo.
            if (eTarget != null && useE && E.IsReady())  
            {
                if (eTarget.HasBuff("RocketGrab"))
                    E.Cast();
            }

            //Cast Q if you can't use E and the target is near you.
            //Done to be able to use E even if you didn't land the Q.
            if (eTarget != null && useE && E.IsReady() && !Q.IsReady()) 
                E.Cast();

            //If you can't use the Q, it uses the R.
            //Done to be able to do the Q-E-R combo.
            if (rTarget  != null && !Q.IsReady() && useR && R.IsReady()) 
                R.Cast(rTarget , false, true);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttacks(true);
            Orbwalker.SetMovement(true);


            var useRKS = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active) 
                Combo();
            if (useRKS) 
                Killsteal();
            if (Config.Item("AutoUlt").GetValue<bool>() && Utility.CountEnemysInRange((int)R.Range) >= Config.Item("CountR").GetValue<Slider>().Value && R.IsReady()) 
                R.Cast();
            if (Config.Item("APToggle").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    if (Config.Item("AutoPull" + enemy.BaseSkinName).GetValue<bool>() && Q.IsReady())
                        //foreach (var buff in enemy.Buffs.Where(buff => (buff.Type == (BuffType.Stun) || buff.Type == BuffType.Knockup || buff.Type == BuffType.Snare || buff.Type == BuffType.Suppression)))
                        //    if (buff.EndTime == 0.3 + Q.Delay + (Player.Distance(enemy)/Q.Speed))
                        Q.CastIfHitchanceEquals(enemy, HitChance.Immobile, false);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }
        
        private static void Killsteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    Damage.GetSpellDamage(Player, hero, SpellSlot.R) >= hero.Health)
                    R.Cast();
            }
        }
    }
}
