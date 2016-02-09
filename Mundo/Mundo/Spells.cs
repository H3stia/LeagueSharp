using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mundo
{
    internal class Spells
    {
        public static Spell q, w, e, r;

        public static SpellDataInst ignite;

        public static void InitializeSpells()
        {
            try
            {
                q = new Spell(SpellSlot.Q, 1050);
                w = new Spell(SpellSlot.W, 325);
                e = new Spell(SpellSlot.E, Orbwalking.GetRealAutoAttackRange(CommonUtilities.Player));
                r = new Spell(SpellSlot.R);

                q.SetSkillshot(0.25f, 60, 2000, true, SkillshotType.SkillshotLine);

                ignite = CommonUtilities.Player.Spellbook.GetSpell(CommonUtilities.Player.GetSpellSlot("summonerdot"));
            }
            catch (Exception exception)
            {
                Console.WriteLine("Could not load the spells - {0}", exception);
            }
        }

    }
}
