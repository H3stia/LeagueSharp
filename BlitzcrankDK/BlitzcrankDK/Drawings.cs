using System;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;

namespace BlitzcrankDK
{
    internal class Drawings : Spells
    {
        public static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ConfigMenu.Menu["drawing.settings"]["drawing.disable"].GetValue<MenuBool>())
            {
                return;
            }

            if (ConfigMenu.Menu["drawing.settings"]["drawing.q"].GetValue<MenuBool>() && q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, q.Range, q.IsReady() ? Color.DodgerBlue : Color.Firebrick);
            }

            if (ConfigMenu.Menu["drawing.settings"]["drawing.e"].GetValue<MenuBool>() && e.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, e.Range, e.IsReady() ? Color.DodgerBlue : Color.Firebrick);
            }

            if (ConfigMenu.Menu["drawing.settings"]["drawing.r"].GetValue<MenuBool>() && r.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, r.Range, r.IsReady() ? Color.DodgerBlue : Color.Firebrick);
            }
        }
    }
}
