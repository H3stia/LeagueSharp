using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;

namespace Karma
{
    internal class Spells
    {
        internal static Spell q, w, e, r;
        internal static SpellDataInst ignite;

        internal static void InitializeSpells()
        {
            q = new Spell(SpellSlot.Q, 950).SetSkillshot(0.25f, 60, 1700, true, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W, 675);
            e = new Spell(SpellSlot.E, 800);
            r = new Spell(SpellSlot.R);

            ignite = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("summonerdot"));
        }
    }
}
