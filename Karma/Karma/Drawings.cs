using System;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;

namespace Karma
{
    internal class Drawings : Spells
    {
        internal static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ConfigMenu.Menu["KarmaDK"]["drawing.disable"].GetValue<MenuBool>())
            {
                return;
            }

            if (ConfigMenu.Menu["KarmaDK"]["drawing.q"].GetValue<MenuBool>() && q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, q.Range, q.IsReady() ? Color.DodgerBlue : Color.Firebrick);
            }

            if (ConfigMenu.Menu["KarmaDK"]["drawing.w"].GetValue<MenuBool>() && w.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, w.Range, w.IsReady() ? Color.DodgerBlue : Color.Firebrick);
            }

            if (ConfigMenu.Menu["KarmaDK"]["drawing.e"].GetValue<MenuBool>() && e.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, e.Range, e.IsReady() ? Color.DodgerBlue : Color.Firebrick);
            }
        }
    }
}
