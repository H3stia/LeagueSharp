using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using SharpDX;

namespace BlitzcrankDK
{
    internal class Blitzcrank : Spells
    {
        public Blitzcrank()
        {
            if (ObjectManager.Player.ChampionName != "Blitzcrank")
                return;

            InitializeSpells();
            ConfigMenu.InitializeMenu();
            Events.OnGapCloser += EventsOnOnGapCloser;
            Events.OnInterruptableTarget += EventsOnOnInterruptableTarget;
            Game.OnUpdate += GameOnOnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
			Events.OnDash += EventsOnDash;

            Notifications.Add(new Notification("Hestia", "BlitzcrankDK loaded!")
            {
                HeaderTextColor = Color.Chartreuse,
                BodyTextColor = Color.White,
                Icon = NotificationIconType.Check,
            });
        }

		private void EventsOnDash(object sender, Events.DashArgs onDashEventArgs)
		{
		    if (ConfigMenu.Menu["misc.settings"]["misc.dash"].GetValue<MenuBool>() && onDashEventArgs.Unit.IsEnemy && onDashEventArgs.Unit.Distance(ObjectManager.Player) < q.Range)
		    {
                var prediction = q.GetPrediction(onDashEventArgs.Unit);
		        if (prediction.Hitchance == HitChance.Dashing && !prediction.CollisionObjects.Any())
		        {
                    q.Cast(onDashEventArgs.EndPos);
                }
		    }
		}

        private void EventsOnOnGapCloser(object sender, Events.GapCloserEventArgs gapCloserEventArgs)
        {

            if (ConfigMenu.Menu["misc.settings"]["misc.antigap"].GetValue<MenuBool>() && gapCloserEventArgs.IsDirectedToPlayer && gapCloserEventArgs.Sender.Distance(ObjectManager.Player) < 450)
            {
                r.Cast();
            }
        }

        private void EventsOnOnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs interruptableTargetEventArgs)
        {
            var target = interruptableTargetEventArgs.Sender;

            if (ConfigMenu.Menu["misc.settings"]["misc.interrupt"].GetValue<MenuBool>() && target != null)
            {
                if (target.IsEnemy && target.IsValidTarget(r.Range) && r.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) <= r.Range)
                {
                    r.Cast();
                }
                
            }
        }

        private void GameOnOnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            switch (Variables.Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    ExecuteCombo();
                    break;
            }

            AutoQ();
            Killsteal();
        }

        private void ExecuteCombo()
        {
            var qTarget = q.GetTarget(0, true);

			if (qTarget == null) 
			{
				return;
			}

			if (ConfigMenu.Menu["combo.settings"]["combo.q"].GetValue<MenuBool>() && !ConfigMenu.Menu["blacklist.settings"]["block" + qTarget.ChampionName].GetValue<MenuBool>())
            {
                var predictedPos = q.GetPrediction(qTarget);
                if (predictedPos.Hitchance >= HitChance.High && !predictedPos.CollisionObjects.Any())
                {
					if (ConfigMenu.Menu["combo.settings"]["combo.q.distance"].GetValue<MenuSliderButton>() && ObjectManager.Player.Distance(qTarget) >= ConfigMenu.Menu["combo.settings"]["combo.q.distance"].GetValue<MenuSliderButton>().MinValue) 
					{
						q.Cast(predictedPos.UnitPosition);
					} 
					else 
					{
						q.Cast(predictedPos.UnitPosition);
					}
                }
            }

            if (ConfigMenu.Menu["combo.settings"]["combo.e"].GetValue<MenuBool>() && e.IsReady() && qTarget.HasBuff("rocketgrab2"))
            {
                e.Cast();
            }


        }

        private void AutoQ()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(q.Range) && !ConfigMenu.Menu["blacklist.settings"]["block" + enemy.ChampionName].GetValue<MenuBool>() && ConfigMenu.Menu["auto.settings"]["auto" + enemy.ChampionName].GetValue<MenuBool>()))
            {
                var predictedPos = q.GetPrediction(enemy);
                if (predictedPos.Hitchance >= HitChance.High && !predictedPos.CollisionObjects.Any())
                {
                    q.Cast(predictedPos.UnitPosition);
                }
            }
        }
        private void Killsteal()
        {
            if (ConfigMenu.Menu["killsteal.settings"]["killsteal.q"].GetValue<MenuBool>())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(q.Range) && enemy.Health + enemy.MagicalShield < q.GetDamage(enemy) && !enemy.IsZombie))
                {
                    var predictedPos = q.GetPrediction(enemy);
                    if (predictedPos.Hitchance >= HitChance.High && !predictedPos.CollisionObjects.Any())
                    {
                        q.Cast(predictedPos.UnitPosition);
                    }
                }
            }

            if (ConfigMenu.Menu["killsteal.settings"]["killsteal.r"].GetValue<MenuBool>())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(r.Range) && enemy.Health + enemy.MagicalShield < r.GetDamage(enemy) && !enemy.IsZombie))
                {
                    r.Cast(enemy);
                }
            }
        }
    }
}