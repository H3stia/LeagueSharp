using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace SimpleTristanaUlt
{
    class Program
    {

        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Menu config;

        private static Spell r;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != "Tristana")
            {
                return;
            }

            r = new Spell(SpellSlot.R, 630);

            config = new Menu("Simple Tristana Ult", "SimpleTristanaUlt", true);

            config.AddItem(
                new MenuItem("ultNearest", "Ult nearest target").SetValue(
                    new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("drawR", "Draw R range").SetValue(new Circle(true, Color.DarkOrange, r.Range)));
            config.AddItem(new MenuItem("width", "Drawings width").SetValue(new Slider(2, 1, 5)));
            config.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (config.Item("ultNearest").GetValue<KeyBind>().Active)
            {
                UltNearestTarget();
            }
        }

        private static void UltNearestTarget()
        {
            var closestEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget(r.Range) && !x.IsDead).MinOrDefault(x => x.Distance(Player));

            r.CastOnUnit(closestEnemy);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (config.Item("drawR").GetValue<Circle>().Active && r.Level > 0)
            {
                var circle = config.Item("drawR").GetValue<Circle>();
                var width = config.Item("width").GetValue<Slider>().Value;
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color, width);
            }
        }
    }
}
