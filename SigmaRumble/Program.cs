
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SigmaRumble
{
    class Program
    {

        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Obj_AI_Hero Player;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static float maxRangeR = 2400f;
        public static List<Obj_AI_Base> minions;
        public static Obj_AI_Hero bestChamp;
        public static int maxCount;
        public static float sleepTime;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Drawing.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("castR").GetValue<KeyBind>().Active && R.IsReady())
            {
                var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                CastR(rTarget);
            }
            
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                harass();
            }
            if (Config.Item("FreezeActive").GetValue<KeyBind>().Active)
            {
                freeze();
            }
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                waveClear();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
            if (Config.Item("keepHeat").GetValue<KeyBind>().Active)
            {
                keepHeat();
            }
        }

        public static void keepHeat()
        {
            var useQ = Config.Item("keepHeatQ").GetValue<bool>();
            var useW = Config.Item("keepHeatW").GetValue<bool>();
            if (Player.Mana < 50 && sleepTime + 5f < Game.Time)
            {
                if (Q.IsReady() && sleepTime + 5f < Game.Time && useQ)
                {
                    Q.Cast(Game.CursorPos, true);
                    sleepTime = sleepTime + 1f;
                    return;
                }
                if (W.IsReady() && sleepTime + 5f < Game.Time && useW)
                {
                    W.Cast();
                    sleepTime = sleepTime + 1f;
                    return;
                }
            }
        }

        public static void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var rTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (rTarget != null)
            {
                if (Player.Distance(rTarget) < Orbwalking.GetRealAutoAttackRange(Player) && W.IsReady() && willOverLoad(false) == false && useQ)
                {
                    CastW();
                    return;
                }
                if (Player.Distance(rTarget) < Q.Range && Q.IsReady() && willOverLoad(false) == false && useW)
                {
                    CastQ(rTarget, false);
                    return;
                }
                if (Player.Distance(rTarget) < E.Range && E.IsReady() && willOverLoad(false) == false && useE)
                {
                    CastE(rTarget, false);
                    return;
                }
            }
        }
        public static void waveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (Q.IsReady() && useQ)
                        {
                            CastQ(minion, false);
                            return;
                        }
                        if (E.IsReady() && useE)
                        {
                            CastE(minion, false);
                            return;
                        }
                        
                    }
                }
            
        }
        public static void freeze()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (Q.IsReady() && useQ)
                        {
                            CastQ(minion, false);
                            return;
                        }
                        if (E.IsReady() && useE)
                        {
                            CastE(minion, false);
                            return;
                        }
                    }
                }
            
        }
        public static void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (Q.IsReady() && useQ)
                        {
                            CastQ(minion, false);
                            return;
                        }
                        if (E.IsReady() && useE)
                        {
                            CastE(minion, false);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            CastW();
                            return;
                        }
                       
                    }
                }
            
        }

        public static void Combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var rTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (rTarget != null)
            {
                if (Player.Distance(rTarget) < Orbwalking.GetRealAutoAttackRange(Player) && W.IsReady() && willOverLoad(false) == false && useQ)
                {
                    CastW();
                    return;
                }
                if (Player.Distance(rTarget) < Q.Range && Q.IsReady() && willOverLoad(false) == false && useW)
                {
                    CastQ(rTarget, false);
                    return;
                }
                if (Player.Distance(rTarget) < E.Range && E.IsReady() && willOverLoad(false) == false && useE)
                {
                    CastE(rTarget, false);
                    return;
                }
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Sigma Rumble");
            Player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 850);
            R = new Spell(SpellSlot.R, 1700);

            E.SetSkillshot(0.5f, 90, 1200, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(1700, 120, 1400, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("SigmaRumble", "SigmaRumble", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("keepHeat", "Maintain Heat").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Combo").AddItem(new MenuItem("keepHeatQ", "Use Q to maintain heat").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("keepHeatW", "Use W to maintain heat").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("castR", "Cast R!").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("useQFarm", "Q").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 1)));
            Config.SubMenu("Farm").AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 0)));
            Config.SubMenu("Farm").AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEJung", "Use E").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();
        }

        public static void CastQ(Obj_AI_Base qTarget, bool  ks)
        {
            if (ks)
            {
                if (Player.Distance(qTarget) < Q.Range)
                {
                    Q.Cast(qTarget, true);
                }
            }
            else if (!ks && willOverLoad(false) == false)
            {
                if (Player.Distance(qTarget) < Q.Range)
                {
                    Q.Cast(qTarget, true);
                }
            }
        }

        public static void CastW()
        {
            if (!willOverLoad(false))
            {
                W.Cast();
            }
        }

        public static void CastE(Obj_AI_Base eTarget, bool ks)
        {
            if (ks)
            {
                E.CastIfHitchanceEquals(eTarget, HitChance.Medium, true);
            }

            if (!willOverLoad(true) && ks == false)
            {
                E.CastIfHitchanceEquals(eTarget, HitChance.Medium, true);
            }
        }
        public static void CastR(Obj_AI_Base rTarget)
        {
            var getChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(maxRangeR) && rTarget.Name != champ.Name select champ).ToList();

            maxCount = -1;
            bestChamp = null;

            foreach (var enemy in getChamps)
            {
                var getMoarChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(maxRangeR) select champ).ToList();
                var count = 0;
                foreach (var champs in getMoarChamps)
                {
                    count = (int)SimpleTs.GetPriority(enemy);
                }
                if (maxCount < count || maxCount == -1)
                {
                    maxCount = count;
                    bestChamp = enemy;
                }
            }

            if (bestChamp == null)
            {
                castR2(rTarget.Position, Prediction.GetPrediction(rTarget, 2f).CastPosition);
            }

            if (bestChamp != null)
            {
                Game.PrintChat((maxCount).ToString());
                castR2(R.GetPrediction(rTarget).CastPosition, R.GetPrediction(bestChamp).CastPosition);
            }
            
        }

        public static void castR2(Vector3 point1, Vector3 point2)
        {
            var p1 = point1.To2D();
            var p2 = point2.To2D();

            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, R.Slot, -1, p1.X, p1.Y, p2.X, p2.Y)).Send();
        }

        public static bool willOverLoad(bool isE)
        {
            if (isE && Player.HasBuff("RumbleGrenade"))
            {
                return false;
            }
            if (isE == false || isE && Player.HasBuff("RumbleGrenade") == false)
            {
                if ((Player.Mana + 20) < 100)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
