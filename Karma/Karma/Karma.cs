using System;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;

namespace Karma
{
    internal class Karma : Spells
    {
        public Karma()
        {
            if (ObjectManager.Player.ChampionName != "Karma")
                return;

            InitializeSpells();
            ConfigMenu.InitializeMenu();
            Events.OnGapCloser += EventsOnOnGapCloser;
            Game.OnUpdate += GameOnOnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            switch (Variables.Orbwalker.GetActiveMode())
            {
                case OrbwalkingMode.Combo:
                    ExecuteCombo();
                    break;

                case OrbwalkingMode.Hybrid:
                    ExecuteHarass();
                    break;

                case OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }

            Killsteal();
        }

        private static void Killsteal()
        {
            throw new NotImplementedException();
        }

        private static void JungleClear()
        {
            throw new NotImplementedException();
        }

        private static void LaneClear()
        {
            throw new NotImplementedException();
        }

        private static void ExecuteHarass()
        {
            throw new NotImplementedException();
        }

        private static void ExecuteCombo()
        {
            throw new NotImplementedException();
        }

        private static void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs gapCloserEventArgs)
        {
            if (ConfigMenu.Menu["KarmaDK"]["misc.antigap"].GetValue<MenuBool>() && gapCloserEventArgs.IsDirectedToPlayer && gapCloserEventArgs.Sender.Distance(ObjectManager.Player) < 350)
            {
                e.CastOnUnit(ObjectManager.Player);
                q.Cast(gapCloserEventArgs.Sender.ServerPosition);
            }
        }
    }
}
