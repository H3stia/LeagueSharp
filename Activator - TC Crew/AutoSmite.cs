using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Activator
{
    public static class AutoSmite
    {
        public struct SpellStruct
        {
            public string ChampionName;
            public SpellSlot Slot;
            public float Range;
        }

        public static List<SpellStruct> SpellList = new List<SpellStruct>();
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static SpellSlot SmiteSlot = SpellSlot.Unknown, SemiSmite = SpellSlot.Unknown;
        public static float SpellRange;

        static AutoSmite()
        {
            SmiteSlot = Player.GetSpellSlot("SummonerSmite");

            #region Olaf
            SpellList.Add(new SpellStruct
            {
                ChampionName = "Olaf",
                Slot = SpellSlot.E,
                Range = 325
            });
            #endregion

            #region Nunu
            SpellList.Add(new SpellStruct
            {
                ChampionName = "Nunu",
                Slot = SpellSlot.Q,
                Range = 125
            });
            #endregion

            #region Chogath
            SpellList.Add(new SpellStruct
            {
                ChampionName = "Chogath",
                Slot = SpellSlot.R,
                Range = 175
            });
            #endregion

            #region Kha'Zix 
            SpellList.Add(new SpellStruct
            {
                ChampionName = "Khazix",
                Slot = SpellSlot.Q,
                Range = 325
            });
            #endregion

            #region Warwick
            SpellList.Add(new SpellStruct
            {
                ChampionName = "Warwick",
                Slot = SpellSlot.Q,
                Range = 400
            });
            #endregion

            #region Volibear
            SpellList.Add(new SpellStruct
            {
                ChampionName = "Volibear",
                Slot = SpellSlot.W,
                Range = 400
            });
            #endregion

            foreach (var spell in SpellList.Where(spell => spell.ChampionName == Player.ChampionName))
            {
                SemiSmite = spell.ChampionName == Player.ChampionName ? spell.Slot : SpellSlot.Unknown;
                SpellRange = spell.Range;
            }
                
            if (SemiSmite == SpellSlot.Unknown && SmiteSlot == SpellSlot.Unknown)
                return;

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static void AddToMenu(Menu menu)
        {
            if (SemiSmite == SpellSlot.Unknown && SmiteSlot == SpellSlot.Unknown)
                return;

            var smiteMenu = new Menu("Auto Smite", "AutoSmite");

            smiteMenu.AddItem(new MenuItem("AutoSmiteEnabled", "Toggle Enabled").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle, true)));
            smiteMenu.AddItem(new MenuItem("EnableSmallCamps", "Smite small Camps").SetValue(false));
            smiteMenu.AddItem(new MenuItem("AutoSmiteDrawing", "Enable Drawing").SetValue(true));

            menu.AddSubMenu(smiteMenu);
        }

        //Get Monster
        private static readonly string[] MinionNames =
        {
            "SRU_BaronSpawn", "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith"
        };

        private static readonly string[] SmallMinionNames =
        {
            "Sru_Crab", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "SRU_Gromp"
        };

        private static Obj_AI_Base GetMinion()
        {
            var minionList = MinionManager.GetMinions(Player.ServerPosition, 760, MinionTypes.All, MinionTeam.Neutral);
            var smallCamps = Config.Menu.Item("EnableSmallCamps").GetValue<bool>();
            return smallCamps
                ? minionList.FirstOrDefault(
                    minion => minion.IsValidTarget(760) && MinionNames.Any(name => minion.Name.StartsWith(name)) || SmallMinionNames.Any(smallname => minion.Name.StartsWith(smallname)))
                : minionList.FirstOrDefault(
                    minion => minion.IsValidTarget(760) && MinionNames.Any(name => minion.Name.StartsWith(name)));
        }

        //Calculate damage
        private static float GetDamage(Obj_AI_Base minion)
        {
            var damage = 0d;

            if (Player.SummonerSpellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                damage += Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite);

            if (Player.Spellbook.CanUseSpell(SemiSmite) == SpellState.Ready && Player.Distance(minion.ServerPosition) < SpellRange)
                damage += Player.GetSpellDamage(minion, SemiSmite);

            return (float) damage;
        }

        //Cast spell
        private static void CastSpell(GameObject unit, SpellSlot slot, bool isSummoner)
        {
            if (!isSummoner)
            {
                Player.Spellbook.CastSpell(slot, unit);
            }
            else
            {
                Player.SummonerSpellbook.CastSpell(slot, unit);
            }
        }

        //Kill monster
        private static void KillMinion(Obj_AI_Base minion)
        {
            if (GetDamage(minion) > minion.Health)
            {
                if (Player.Distance(minion) < SpellRange)
                {
                    CastSpell(minion, SemiSmite, false);
                }
                CastSpell(minion, SmiteSlot, true);
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.Menu.Item("AutoSmiteEnabled").GetValue<bool>())
                return;

            var minion = GetMinion();
            if (minion != null)
            {
                KillMinion(minion);
            }
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.Menu.Item("AutoSmiteEnabled").GetValue<bool>() || !Config.Menu.Item("AutoSmiteDrawing").GetValue<bool>())
                return;
       
            Utility.DrawCircle(Player.Position, 760, Color.Coral);
        }
    }
}