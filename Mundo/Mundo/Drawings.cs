using System;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mundo
{
    internal class Drawings : Spells
    {
        public static void OnDraw(EventArgs args)
        {
            if (CommonUtilities.Player.IsDead || ConfigMenu.config.Item("disableDraw").GetValue<bool>())
                return;

            var heroPosition = Drawing.WorldToScreen(CommonUtilities.Player.Position);
            var textDimension = Drawing.GetTextExtent("AutoQ: ON");
            var drawQstatus = ConfigMenu.config.Item("drawAutoQ").GetValue<bool>();
            var autoQ = ConfigMenu.config.Item("autoQ").GetValue<KeyBind>().Active;

            if (drawQstatus && autoQ)
            {
                Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height, Color.DarkOrange, "AutoQ: ON");
            }

            var width = ConfigMenu.config.Item("width").GetValue<Slider>().Value;

            if (ConfigMenu.config.Item("drawQ").GetValue<Circle>().Active && q.Level > 0)
            {
                var circle = ConfigMenu.config.Item("drawQ").GetValue<Circle>();
                Render.Circle.DrawCircle(CommonUtilities.Player.Position, circle.Radius, circle.Color, width);
            }

            if (ConfigMenu.config.Item("drawW").GetValue<Circle>().Active && w.Level > 0)
            {
                var circle = ConfigMenu.config.Item("drawW").GetValue<Circle>();
                Render.Circle.DrawCircle(CommonUtilities.Player.Position, circle.Radius, circle.Color, width);
            }
        }
    }
}
