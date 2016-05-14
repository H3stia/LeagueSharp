using LeagueSharp;
using LeagueSharp.SDKEx;
using LeagueSharp.SDKEx.Enumerations;
using LeagueSharp.SDKEx.Utils;

namespace BlitzcrankDK
{
    public class Spells
    {
        public static Spell q, w, e, r;

        public void InitializeSpells()
        {
            q = new Spell(SpellSlot.Q, 950f).SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W);
            e = new Spell(SpellSlot.E, ObjectManager.Player.GetRealAutoAttackRange());
            r = new Spell(SpellSlot.R, 550f);
        }
    }
}