using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;

namespace KarmaDK
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
            if (!ConfigMenu.Menu["killsteal.settings"]["killsteal.q"].GetValue<MenuBool>() && !ConfigMenu.Menu["killsteal.settings"]["killsteal.rq"].GetValue<MenuBool>())
            {
                return;
            }

            if (ConfigMenu.Menu["killsteal.settings"]["killsteal.q"].GetValue<MenuBool>() && q.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(980f) && !target.HasBuffOfType(BuffType.Invulnerability) && target.Health + target.MagicalShield < ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)))
                {
                    q.Cast(target);
                }
            }

            if (ConfigMenu.Menu["killsteal.settings"]["killsteal.rq"].GetValue<MenuBool>() && q.IsReady())
            {
                throw new NotImplementedException();
            }
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
            if (ConfigMenu.Menu["misc.settings"]["misc.antigap"].GetValue<MenuBool>() && gapCloserEventArgs.IsDirectedToPlayer && gapCloserEventArgs.Sender.Distance(ObjectManager.Player) < 350)
            {
                e.CastOnUnit(ObjectManager.Player);
                q.Cast(gapCloserEventArgs.Sender.ServerPosition);
            }
        }
    }
}
