using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

namespace LeeSinSharp
{
    class LeeSin
    {
        public static Vector2 testSpellCast;
        public static Vector2 testSpellProj;
        private static int _wardJumpRange = 600;
        public static bool loai = false;
        public static Vector3 tg1, tg;
        public static float tx, tz;
        private static readonly int _wardDistance = 300;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static bool da = false;
        public static Spellbook sBook = Player.Spellbook;
        public static float lastwardjump = 0;
        public static float lasttick = 0;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 1100);
        public static Spell W = new Spell(SpellSlot.W, 700);
        public static Spell R = new Spell(SpellSlot.R, 375);

        public static Obj_AI_Hero LockedTarget;

        public static Vector2 harassStart;
        
        public static void checkLock(Obj_AI_Hero target)
        {
            //if (!target.IsValidTarget())
            //    return;
            if ( !LeeSinSharp.Config.Item("ActiveHarass").GetValue<KeyBind>().Active && LockedTarget != null)//Reset all values
            {
                LockedTarget = null;
            }
            else if (LeeSinSharp.Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
                LockedTarget = target;
            else if (target.IsValidTarget() && LockedTarget == null || LeeSinSharp.Config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                LockedTarget = target;
            }
        }

        public static void setSkillShots()
        {
            Q.SetSkillshot(0.4f, 60f, 1800f, true, SkillshotType.SkillshotLine);
        }

        public static void useinsec()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (hero.IsAlly && !hero.IsMe && hero != null && hero.Distance(Player) < 1500)
                            
                        {
                            insec1();
                        }
                        else 
                        {insec();
                        }
                }
                
        }
        public static bool loaidraw()
        {
            foreach (Obj_AI_Hero hero1 in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero1.IsAlly && !hero1.IsMe &&hero1 != null && hero1.Distance(Player) < 1500)
                    return true;
            }
            return false;
        }

        public static void insec()
        {
            if (!R.IsReady())
            {
                da = false;
                return;
            }
            try
            {
                if (da && !W.IsReady())
                {
                    R.Cast(LockedTarget);
                }
                if (Player.Distance(getward(LockedTarget)) > 600 && W.IsReady())
                {
                    castQFirstSmart();
                    castQSecondSmart();
                }
                if (Player.Distance(getward(LockedTarget)) <= 600 && W.IsReady())
                {
                    wardJump(getward(LockedTarget).To2D());
                    da = true;
                }
            }
            catch
            {
               
            }
        }
        public static void insec1()
        {
            if (!R.IsReady())
            {
                da = false;
                return;
            }
            try
            {
                if (da && !W.IsReady())
                {
                    R.Cast(LockedTarget);
                }
                if (Player.Distance(getward2(LockedTarget)) > 600 && W.IsReady())
                {
                    castQFirstSmart();
                    castQSecondSmart();
                }
                if (Player.Distance(getward2(LockedTarget)) < 600 && W.IsReady())
                {
                    wardJump(getward2(LockedTarget).To2D());
                    da = true;
                    
                }
            }
            catch
            {

            }
        }

        public static bool getBackHarass()
        {
            Obj_AI_Turret closest_tower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            Obj_AI_Base jumpOn = ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly && !(ally is Obj_AI_Turret) && !ally.IsMe && ally.Distance(LeeSin.Player.ServerPosition) < 700).OrderBy(tur => tur.Distance(closest_tower.ServerPosition)).First();
            W.Cast(jumpOn);
            return false;
        }

        public static bool targetHasQ(Obj_AI_Hero target)
        {
            foreach (BuffInstance buf in target.Buffs)
            {
                if (buf.Name == "BlindMonkQOne" || buf.Name == "blindmonkqonechaos")
                    return true;
            }
            return false;
        }
        public static bool targetHasUlti(Obj_AI_Hero target)
        {
            foreach (BuffInstance buf in target.Buffs)
            {
                if (buf.Name == "JudicatorIntervention" || buf.Name == "UndyingRage")
                    return false;
            }
            return true;
        }

        public static bool castQFirstSmart()
        {
            if (!Q.IsReady() || Qdata.Name != "BlindMonkQOne" || LockedTarget == null)
                return false;
            {
                Q.CastIfHitchanceEquals(LockedTarget, HitChance.High);
                return true;
            }
        }

        public static bool castQSecondSmart()
        {
            if (Qdata.Name != "blindmonkqtwo" || LockedTarget == null)
                return false;
            if (targetHasQ(LockedTarget) && inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), 1200))
            {
                Q.Cast();
                return true;
            }
            return true;
        }
        public static int getJumpWardId()
        {
            int[] wardIds = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };
            foreach (int id in wardIds)
            {
                if (Items.HasItem(id) && Items.CanUseItem(id))
                    return id;
            }
            return -1;
        }

        public static void moveTo(Vector2 Pos)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Pos.To3D());
        }

        public static void wardJump(Vector2 pos)
        {
            Vector2 posStart = pos;
            if (!W.IsReady())
                return;
            bool wardIs = false;
            if (!inDistance(pos, Player.ServerPosition.To2D(), W.Range+15))
            {
                pos = Player.ServerPosition.To2D() + Vector2.Normalize(pos - Player.ServerPosition.To2D())*600;
            }

            if(!W.IsReady() && W.ChargedSpellName == "")
                return;
            foreach (Obj_AI_Base ally in ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly
                && !(ally is Obj_AI_Turret) && inDistance(pos, ally.ServerPosition.To2D(), 200)))
            {
                    wardIs = true;
                moveTo(pos);
                if (inDistance(Player.ServerPosition.To2D(), ally.ServerPosition.To2D(), W.Range + ally.BoundingRadius))
                {
                    W.Cast(ally);

                }
                return;
            }
            Polygon pol;
            if ((pol = LeeSinSharp.map.getInWhichPolygon(pos)) != null)
            {
                if (inDistance(pol.getProjOnPolygon(pos), Player.ServerPosition.To2D(), W.Range + 15) && !wardIs && inDistance(pol.getProjOnPolygon(pos), pos, 200))
                {
                    if (lastwardjump < Environment.TickCount)
                    {
                        putWard(pos);
                        lastwardjump = Environment.TickCount + 1000;
                    }
                }
            }
            else if(!wardIs)
            {
                if (lastwardjump < Environment.TickCount)
                {
                    putWard(pos);
                    lastwardjump = Environment.TickCount + 1000;
                }
            }

        }

        public static bool putWard(Vector2 pos)
        {
            int wardItem;
            if ((wardItem = getJumpWardId()) != -1)
            {
                foreach (var slot in Player.InventoryItems.Where(slot => slot.Id == (ItemId)wardItem))
                {
                    slot.UseItem(pos.To3D());
                    return true;
                }
            }
            return false;
        }


        public static bool inDistance(Vector2 pos1, Vector2 pos2, float distance)
        {
            float dist2 = Vector2.DistanceSquared(pos1, pos2);
            return (dist2 <= distance * distance) ? true : false;
        }
        public static Vector3 getward(Obj_AI_Hero target)
        {
            Obj_AI_Turret turret = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
              return target.ServerPosition + Vector3.Normalize(turret.ServerPosition - target.ServerPosition) * (-300);
        }
        public static Vector3 getward2(Obj_AI_Hero target)
        {
            Obj_AI_Hero hero = ObjectManager.Get<Obj_AI_Hero>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.ServerPosition + Vector3.Normalize(hero.ServerPosition - target.ServerPosition) * (-300);
        }
        public static Vector3 getward1(Obj_AI_Hero target)
        {
            Obj_AI_Turret turret = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.Position + Vector3.Normalize(turret.Position - target.Position) * (600);
        }
        public static Vector3 getward3(Obj_AI_Hero target)
        {
            Obj_AI_Hero hero = ObjectManager.Get<Obj_AI_Hero>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.Position + Vector3.Normalize(hero.Position - target.Position) * (600);
        }

      }
}
