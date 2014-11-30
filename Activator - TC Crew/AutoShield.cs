using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator
{
    static class AutoShield
    {
        public static List<ActiveSpell> Shields = new List<ActiveSpell>();
        static AutoShield()
        {
            Shields = ActiveSpellDatabase.Spells.Where(s => (s.ChampionName == "" || s.ChampionName == ObjectManager.Player.ChampionName) && s.Effects.HasFlag(Effects.Shield) && (s.Type != ActiveSpellType.SummonerSpell || ObjectManager.Player.GetSpellSlot(s.SummmonerSpellName) != SpellSlot.Unknown )).ToList();
        
            Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
        }

        public static void AddToMenu(Menu menu)
        {
            var shieldsMenu = new Menu("Auto Shield", "AutoShield");
           
            //General settings.
            if(Shields.Any(s => s.CanTargetAllies))
            {
                shieldsMenu.AddItem(new MenuItem("DummySpace", ""));
                //Add the allies 
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly))
                {
                    shieldsMenu.AddItem(new MenuItem("shield" + ally.ChampionName, "Shield " + ally.ChampionName).SetValue(true));
                }
            }

            shieldsMenu.AddItem(new MenuItem("DummySpace2", ""));

            shieldsMenu.AddItem(new MenuItem("AutoShieldEnabled", "Enabled").SetValue(true));

            
            //Settings per shields.
            foreach (var shield in Shields)
            {
                var subMenu = new Menu(shield.MenuName, shield.MenuName);
                subMenu.AddItem(new MenuItem("as" + shield.MenuName + "Priority", "Priority").SetValue(new Slider(shield.Priority, 0, 5)));
                subMenu.AddItem(new MenuItem("as" + shield.MenuName + "HealthPercent", "HealthPercent").SetValue(new Slider(shield.HealthPercent)));
                subMenu.AddItem(new MenuItem("as" + shield.MenuName + "Enabled", "Enabled").SetValue(true));
                shieldsMenu.AddSubMenu(subMenu);
            }

            menu.AddSubMenu(shieldsMenu);
        }

        private static T GetShieldConfig<T>(ActiveSpell spell, string field)
        {
            return Config.Menu.SubMenu("AutoShield").Item("as" + spell.MenuName + field).GetValue<T>();
        }

        private static void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Config.Menu.Item("AutoShieldEnabled").GetValue<bool>())
            {
                return;
            }

            if (sender.IsValidTarget() && args.Target is Obj_AI_Hero && args.Target.IsValid && args.Target.IsAlly)
            {
                var hero = (Obj_AI_Hero) args.Target;
                var damageSpell = sender.GetDamageSpell(hero, args.SData.Name);
                if(damageSpell != null)
                {
                    OnReceiveDamage(hero, damageSpell.DamageType, damageSpell.CalculatedDamage, args.SData.Name);
                }
            }
        }

        public static void OnReceiveDamage(Obj_AI_Hero hero, Damage.DamageType type, double amount, string spellName)
        {
            //Setup the variables that the shields conditional checks will use.
            var args = new ActiveEventArgs
            {
                Target = hero, 
                SpellName = spellName,
                DamageAmount = amount,
                DamageType = type,
            };

            if(!Config.Menu.Item("shield" + hero.ChampionName).GetValue<bool>())
            {
                return;
            }

            //Pro-debugging.
            Console.WriteLine(hero.ChampionName + " is receiving " + amount + " damage (" + (amount / hero.Health * 100) + ")");

            for (var i = 1; i <= 5; i++)
            {
                //Avoid using more than 1 shield, maybe in the future check the shield amount to stack them.
                foreach (var shield in Shields)
                {
                    if ( GetShieldConfig<bool>(shield, "Enabled") &&
                         i == GetShieldConfig<Slider>(shield, "Priority").Value &&
                         (shield.CanTargetAllies || hero.IsMe) && 
                         ((amount / hero.Health * 100 >= GetShieldConfig<Slider>(shield, "HealthPercent").Value ) || (shield.ForceCastCondition != null && shield.ForceCastCondition(Effects.Shield, args)) ) &&
                         (shield.CheckCondition == null || shield.CheckCondition(Effects.Shield, args)) &&
                          shield.Cast(hero))
                    {
                        return;
                    }
                }
            }
            
        }
    }
}
