using LeagueSharp;
using LeagueSharp.Common;

namespace Nautilus
{
    internal class CommonUtilities
    {
        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }


        public static HitChance GetHitChance(string name)
        {
            var hitChance = ConfigMenu.config.Item(name).GetValue<StringList>();

            switch (hitChance.SList[hitChance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "Very High":
                    return HitChance.VeryHigh;
            }
            return HitChance.VeryHigh;
        }
    }
}
