using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace Kennen
{
    class Program
    {
        public const string ChampionName = "Kennen";


        public static readonly List<Spell> SpellList = new List<Spell>();
        public static Spell _q, _w, _e, _r;

        public static Obj_AI_Hero Player;
        public static Menu _config;
        public static Orbwalking.Orbwalker _orbwalker;

        public static SpellSlot IgniteSlot;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName) return;

            Game.PrintChat("Hestia Kennen loaded. Enjoy!");

            //Spells
            _q = new Spell(SpellSlot.Q, 1050);
            _w = new Spell(SpellSlot.W, 800);
            _e = new Spell(SpellSlot.E, 0);
            _r = new Spell(SpellSlot.R, 550);

            _q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            //Menu
            _config = new Menu("Kennen", "Kennen", true);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Target Selector
            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(TargetSelectorMenu);
            _config.AddSubMenu(TargetSelectorMenu);

            //Combo Menu
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("qHitchance", "Q Hitchance").SetValue(new StringList(new string[] { "Low", "Medium", "High", "Very High" })));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(new StringList(new string[] {"Always", "Only Stunnable"}))); //TODO on stunnable
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use smart R").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRmulti", "Use R on min X targets").SetValue(new Slider(2, 0, 5))); //0 to deactivate
            _config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            /*
            //Harass Menu
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(new StringList(new string[] { "Always", "Only Stunnable" })));
            _config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "Harass (toggle)!").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            */
            //Killsteal Menu
            _config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("Killsteal", "Activate KillSteal").SetValue(true));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("UseQKS", "Use Q to KillSteal").SetValue(true));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("UseWKS", "Use W to KillSteal").SetValue(true));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("UseIKS", "Use Ignite to KillSteal").SetValue(true));

            //Drawings Menu
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            _config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (_config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();

            if (_config.Item("Killsteal").GetValue<bool>())
                KillSteal();

            CastRmulti();
            /* TODO
            if ((_config.Item("HarassActive").GetValue<KeyBind>().Active) || (_config.Item("HarassActiveT").GetValue<KeyBind>().Active))
                Harass();
             * */
        }

        public static void CastQ() //Q casting 
        {
            if (!_q.IsReady())
                return;
            var qTarget = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            var useQ = _config.Item("UseQCombo").GetValue<bool>();
            var qMode = _config.Item("qHitchance").GetValue<StringList>().SelectedIndex;
            if (qTarget != null && useQ)
            {
                switch (qMode)
                {
                    case 1://low
                        _q.CastIfHitchanceEquals(qTarget, HitChance.Low);
                        break;
                    case 2://medium
                        _q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                        break;
                    case 3://hgih
                        _q.CastIfHitchanceEquals(qTarget, HitChance.High);
                        break;
                    case 4://very high
                        _q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
                        break;
                }
            }
        }

        public static void CastW()
        {
            if (!_w.IsReady())
                return;
            var wTarget = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
            var wMode = _config.Item("UseWCombo").GetValue<StringList>().SelectedIndex;
            if (wTarget != null)
                switch (wMode)
                {
                    case 1://always
                        _w.Cast();
                        break;
                    /*
                    case 2: //only stunnable 
                        foreach (var buff in wTarget.Buffs)
                        {
                            if (buff.Name == "" && buff.Count == 2) //missing kennen debuff name
                                _w.Cast();
                        }
                        break;
                     */
                }
        }

        //ulti if killable
        public static void CastR()
        {
            if (!_r.IsReady())
                return;
            var rTarget = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
            var useR = _config.Item("UseRCombo").GetValue<bool>();
            if (rTarget != null && useR && GetComboDamage() > rTarget.Health)
                _r.Cast();
        }

        public static float GetComboDamage()
        {
            var comboDamage = 0d;

            var qTarget = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);

            if (_q.IsReady() && qTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.Q);

            if (_w.IsReady() && wTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.W);

            if (_r.IsReady() && rTarget != null)
                comboDamage += Player.GetSpellDamage(qTarget, SpellSlot.R);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                comboDamage += Player.GetSummonerSpellDamage(qTarget, Damage.SummonerSpell.Ignite);

            return (float)comboDamage;
        }

        //auto ult for X enemies in range, 0 to deactivate
        public static void CastRmulti()
        {
            if (!_r.IsReady())
                return;

            //Thanks to xSalice
            int hit = 0;
            var minHit = _config.Item("UseRmulti").GetValue<Slider>().Value;
            if (minHit == 0)
                return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                if (enemy != null && !enemy.IsDead)
                {
                    if (Player.Distance(enemy) < _r.Range)
                        hit++;
                }
            }

            if (hit >= minHit)
                _r.Cast();

        }

        public static void Combo()
        {
            CastQ();
            CastW();
            CastR();
        }

        public static void KillSteal()
        {
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (_config.Item("UseQKS").GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && (Player.Distance(target) < _q.Range) && _q.IsReady())
                    _q.Cast(target);

                if (_w.IsReady() && _config.Item("UseWKS").GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.W) > target.Health && (Player.Distance(target) < _w.Range) && _w.IsReady())
                    _w.Cast();

                if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Player.Distance(target) < 600 && _config.Item("UseIKS").GetValue<bool>())
                {
                    if (Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }
            }
        }
        /* TODO
        public static void Harass()
        {

        }
        */
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (SpellList == null) return;

            foreach (var spell in SpellList)
            {
                var menuItem = _config.Item(spell.Slot + "Range").GetValue<Circle>();

                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
    }
}
