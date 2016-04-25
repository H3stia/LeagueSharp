using LeagueSharp.SDK.UI;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace Karma
{
    internal class ConfigMenu
    {
        internal static Menu Menu;
        internal static void InitializeMenu()
        {
            Menu = new Menu("KarmaDK", "KarmaDK", true);

            var combo = Menu.Add(new Menu("combo.settings", "Combo Settings"));
            combo.Add(new MenuBool("combo.q", "Use Q", true));
            combo.Add(new MenuBool("combo.w", "Use W", true));
            combo.Add(new MenuBool("combo.r", "Use R", true));

            var harass = Menu.Add(new Menu("harass.settings", "Harass Settings"));
            harass.Add(new MenuBool("harass.q", "Use Q", true));
            harass.Add(new MenuBool("harass.w", "Use W"));
            harass.Add(new MenuBool("harass.r", "Use R"));

            var misc = Menu.Add(new Menu("misc.settings", "Misc Settings"));
            misc.Add(new MenuBool("misc.antigap", "Use E-Q on gap-closers", true));
            misc.Add(new MenuBool("misc.e", "Use E to shield incoming damage", true));

            var drawing = Menu.Add(new Menu("drawing.settings", "Drawings Settings"));
            drawing.Add(new MenuBool("drawing.disable", "Disable all drawings", true));
            drawing.Add(new MenuBool("drawing.q", "Draw Q range"));
            drawing.Add(new MenuBool("drawing.w", "Draw W range"));
            drawing.Add(new MenuBool("drawing.e", "Draw E range"));

            Menu.Attach();
        }
    }
}
