using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using Menu = LeagueSharp.SDK.UI.Menu;

namespace BlitzcrankDK
{
    internal class ConfigMenu
    {
        public static Menu Menu;
        public static void InitializeMenu()
        {
            Menu = new Menu("BlitzcrankDK", "BlitzcrankDK", true);
           
            var combo = Menu.Add(new Menu("combo.settings", "Combo Settings"));
            combo.Add(new MenuBool("combo.q", "Use Q", true));
			combo.Add(new MenuSliderButton("combo.q.distance", "Minimum distance to use Q", 0, 0, 950, false)); 
            //combo.Add(new MenuBool("combo.w", "Use W", true));
            combo.Add(new MenuBool("combo.e", "Use E", true));
            //combo.Add(new MenuBool("combo.r", "Use R", true));

            var auto = Menu.Add(new Menu("auto.settings", "Auto Settings"));
            auto.Add(new MenuSeparator("auto.q", "Automatic Q on:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                auto.Add(new MenuBool("auto" + enemy.ChampionName, enemy.ChampionName));
            }

            var blacklist = Menu.Add(new Menu("blacklist.settings", "Blacklist Settings"));
            blacklist.Add(new MenuSeparator("block.q", "Do NOT Q:"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                blacklist.Add(new MenuBool("block" + enemy.ChampionName, enemy.ChampionName));
            }

            var misc = Menu.Add(new Menu("misc.settings", "Misc Settings"));
            misc.Add(new MenuBool("misc.antigap", "Use R on gap-closers"));
            misc.Add(new MenuBool("misc.interrupt", "Use R on to interrupt", true));
            misc.Add(new MenuBool("misc.dash", "Use Q on dashign enemies"));

            var killsteal = Menu.Add(new Menu("killsteal.settings", "Killsteal Settings"));
            killsteal.Add(new MenuBool("killsteal.q", "Use Q to killsteal", true));
            killsteal.Add(new MenuBool("killsteal.r", "Use R to killsteal", true));

            var drawing = Menu.Add(new Menu("drawing.settings", "Drawings Settings"));
            drawing.Add(new MenuBool("drawing.disable", "Disable all drawings", true));
            drawing.Add(new MenuBool("drawing.q", "Draw Q range"));
            drawing.Add(new MenuBool("drawing.e", "Draw E range"));
            drawing.Add(new MenuBool("drawing.r", "Draw R range"));

            Menu.Attach();
        }
    }
}
