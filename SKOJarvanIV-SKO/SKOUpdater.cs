using System;
using LeagueSharp;
using System.ComponentModel;

namespace SKOJarvanIV
{

   internal class SKOUpdater
    {

        internal static bool isInitialized;

        private static void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Updater updater = new Updater("https://github.com/SKOBoL/LeagueSharp/raw/master/Version/SKOJarvanIV/SKOJarvanIV.version", "https://github.com/SKOBoL/LeagueSharp/raw/master/Build/SKOJarvanIV/SKOJarvanIV.exe", 1);
            if (!updater.NeedUpdate)
            {
                Game.PrintChat("<font color='#1d87f2'> SKOJarvanIV: Most recent version loaded!");
            }
            else
            {
                Game.PrintChat("<font color='#1d87f2'> SKOJarvanIV: Updating ...");
                if (updater.Update())
                {
                    Game.PrintChat("<font color='#1d87f2'> SKOJarvanIV: Update complete, reload please.");
                    return;
                }
            }
        }

        internal static void InitializeSKOUpdate()
        {
            isInitialized = true;
            UpdateCheck();
        }

        private static void UpdateCheck()
        {
            Game.PrintChat("<font color='#1d87f2'> SKOJarvanIV loaded!");
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(bgw_DoWork);
            backgroundWorker.RunWorkerAsync();
        }
    }
}
