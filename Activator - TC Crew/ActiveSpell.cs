using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Activator
{
    public enum ActiveSpellType
    {
        Spell,
        SummonerSpell,
        Item,
    }

    [Flags]
    public enum Effects
    {
        Shield,
    }

    class ActiveEventArgs
    {
        public Obj_AI_Base Target;
        public string SpellName;
        public double DamageAmount;
        public Damage.DamageType DamageType;
    }

    class ActiveSpell
    {
        public delegate bool ConditionalCheck(Effects effect, ActiveEventArgs args);
        public string MenuName { get; set; }
        public ActiveSpellType Type { get; set; }
        public Effects Effects { get; set; }
        public ConditionalCheck CheckCondition { get; set; }
        public ConditionalCheck ForceCastCondition { get; set; }
        public string ChampionName { get; set; }
        public int Priority { get; set; }
        public int HealthPercent { get; set; }
        public int Range { get; set; }
        public int ItemId { get; set; }
        public SpellSlot Slot { get; set; }
        public string SummmonerSpellName { get; set; }
        public bool CanTargetAllies { get; set; }
        public bool Cast(Vector2 position)
        {
            return Cast(position, null);
        }
        public bool IsSkillshot { get; set; }
        public bool Cast(Obj_AI_Base target)
        {
            if(IsSkillshot)
            {
                return Cast(target.ServerPosition.To2D(), null);
            }
            else
            {
                return Cast(new Vector2(), target);
            }
        }

        private bool Cast(Vector2 position, Obj_AI_Base target)
        {
            Console.WriteLine("cast equest");
            switch (Type)
            {
                    case ActiveSpellType.Item:
                        if (Items.CanUseItem(ItemId))
                        {
                            if (position.IsValid() && ObjectManager.Player.Distance(position, true) < Range * Range)
                            {
                                Items.UseItem(ItemId, position);
                            }
                            else if (target.IsValidTarget(Range, false))
                            {
                                Items.UseItem(ItemId, target);
                            }

                            return true;
                        }
                        return false;

                    case ActiveSpellType.Spell:
                        if (position.IsValid() && ObjectManager.Player.Distance(position, true) < Range * Range)
                        {
                            return ObjectManager.Player.Spellbook.CastSpell(Slot, position.To3D());
                        }
                        if (target.IsValidTarget(Range, false))
                        {
                            return ObjectManager.Player.Spellbook.CastSpell(Slot, target);
                        }
                        return false;
                    case ActiveSpellType.SummonerSpell:
                        var slot = ObjectManager.Player.GetSpellSlot(SummmonerSpellName);
                        if (slot != SpellSlot.Unknown)
                        {
                            if (position.IsValid() && ObjectManager.Player.Distance(position, true) < Range * Range)
                            {
                                return ObjectManager.Player.SummonerSpellbook.CastSpell(slot, position.To3D());
                            }
                            if (target.IsValidTarget(Range, false))
                            {
                                return ObjectManager.Player.SummonerSpellbook.CastSpell(slot);
                            }
                        }
                        return false;
            }

            return false;
        }

        public ActiveSpell()
        {
            ChampionName = "";
        }
    }

    static class ActiveSpellDatabase
    {
        public static List<ActiveSpell> Spells = new List<ActiveSpell>(); 
        static ActiveSpellDatabase()
        {
            #region Champions shields and heals

            //Janna's E
            Spells.Add(new ActiveSpell
            {
                ChampionName = "Janna",
                MenuName = "Jannas E",
                Type = ActiveSpellType.Spell,
                Slot = SpellSlot.E,
                Effects = Effects.Shield,
                CanTargetAllies = true,
                Range = 800,
                Priority = 1,
                HealthPercent = 7,
            });

            //Morgana's E
            Spells.Add(new ActiveSpell
            {
                ChampionName = "Morgana",
                MenuName = "Morgana E",
                Type = ActiveSpellType.Spell,
                Slot = SpellSlot.E,
                Effects = Effects.Shield,
                CanTargetAllies = true,
                Range = 800,
                Priority = 1,
                HealthPercent = 7,
            });

            #endregion

            Spells.Add(new ActiveSpell
            {
                MenuName = "Barrier",
                Type = ActiveSpellType.SummonerSpell,
                SummmonerSpellName = "SummonerBarrier",
                Effects = Effects.Shield,
                Range = 100,
                Priority = 5,
                HealthPercent = 100,
            });
            
        }
    }
}
