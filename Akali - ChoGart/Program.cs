#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Akali
{
    internal class Program
    {
        public const string ChampionName = "Akali";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //public static SpellSlot IgniteSlot;
        public static Items.Item HEX;
        public static Items.Item DFG;
        public static Items.Item Cutlass;

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
            Game.PrintChat("Akali by Chogart Loaded");

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

             //IgniteSlot = Player.GetSpellSlot("SummonerDot");
            HEX = new Items.Item(3146, 700);
            DFG = new Items.Item(3128, 750);
            Cutlass = new Items.Item(3144, 450);

            Config = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("HEX", "Use Hextech").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("DFG", "Use Deathfire Grasp").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cutlass", "Use Bilgewater Cutlass").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "Use Q").SetValue(
                        new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseEFarm", "Use E").SetValue(
                        new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "Killsteal R").SetValue(false));

            
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Config.Item("RRange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, R.Range, menuItem.Color);

            var menuItem2 = Config.Item("QRange").GetValue<Circle>();
            if (menuItem2.Active) Utility.DrawCircle(Player.Position, Q.Range, menuItem2.Color);
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttacks(true);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
            if (Config.Item("KillstealR").GetValue<bool>())
            {
                Killsteal();
            }
        }
        private static void Combo()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, 0);
            Orbwalker.SetAttacks(!R.IsReady(0) && !Q.IsReady(0) && !E.IsReady(0) && Geometry.Distance(Player, target) < 800f);
            bool value = Config.Item("HEX").GetValue<bool>();
            bool value2 = Config.Item("DFG").GetValue<bool>();
            bool value3 = Config.Item("Cutlass").GetValue<bool>();
            if (target != null)
            {
                if (Geometry.Distance(Player, target) <= 800f)
                {
                    if (Geometry.Distance(Player, target) >= 630f && R.IsReady(0))
                    {
                        R.CastOnUnit(target, true);
                        if (Q.IsReady(0))
                        {
                            Q.CastOnUnit(target, true);
                        }
                        if (E.IsReady(0))
                        {
                            E.CastOnUnit(target, true);
                        }
                    }
                    else
                    {
                        if (Q.IsReady(0) && Geometry.Distance(Player, target) <= 600f)
                        {
                            Q.CastOnUnit(target, true);
                            if (R.IsReady(0))
                            {
                                R.CastOnUnit(target, true);
                            }
                            if (E.IsReady(0))
                            {
                                E.CastOnUnit(target, true);
                            }
                        }
                        else
                        {
                            if (R.IsReady(0))
                            {
                                R.CastOnUnit(target, true);
                                if (value &&  HEX.IsReady())
                                {
                                    HEX.Cast(target);
                                }
                            }
                            if (E.IsReady(0))
                            {
                                E.CastOnUnit(target, true);
                            }
                            if (value2 && DFG.IsReady())
                            {
                                DFG.Cast(target);
                            }
                            if (value3 && Cutlass.IsReady())
                            {
                                Cutlass.Cast(target);
                            }
                        }
                    }
                }
                else
                {
                    if (Damage.GetSpellDamage(Player, target, SpellSlot.Q, 4) > (double)target.Health)
                    {
                        Q.CastOnUnit(target, true);
                    }
                }
            }


            }
        
        private static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (target != null)
            {
                Q.CastOnUnit(target);
            }
            if (Player.Distance(target) <= 325 && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }
        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance(minion) * 1000 / 1400)) <
                        0.75 * Damage.GetSpellDamage(Player, minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }
            }
            else if (useE && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(E.Range) &&
                        minion.Health < 0.75 * Damage.GetSpellDamage(Player, minion, SpellSlot.E))
                    {
                        E.CastOnUnit(minion);
                        return;
                    }
                }
            }


            if (laneClear)
            {
                foreach (var minion in allMinions)
                {
                    if (useQ)
                        Q.CastOnUnit(minion);

                    if (useE)
                        E.CastOnUnit(minion);
                }
            }
        }
        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                Q.CastOnUnit(mob);
                E.CastOnUnit(mob);
            }
        }
        private static void Killsteal()
        {
            var useR = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();
            if (useR)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
                {
                    if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                        Damage.GetSpellDamage(Player, hero, SpellSlot.R) >= hero.Health)
                        R.CastOnUnit(hero, true);
                }
            }
        }
        
     }
}

