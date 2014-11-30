using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;

namespace Activator
{
    class AutoItems
    {
        public enum ItemType
        {
            Offensive = 1,
            Deffensive = 2,
            Utility = 3,
            ClearCC = 4
        }

        public struct ItemStruct
        {
            public string ItemName;
            public ItemId ItemID;
            public ItemType ItemType;

        }
        public static List<ItemStruct> Items = new List<ItemStruct>();
        static AutoItems()
        {
        }

        public static void AddToMenu(Menu menu)
        {
            //Anti CC
            Items.Add(new ItemStruct
            {
                ItemName = "QuicksilverSash",
                ItemID   = (ItemId)3140,
                ItemType = ItemType.ClearCC
            });

            Items.Add(new ItemStruct
            {
                ItemName = "MercurialScimitar",
                ItemID   = (ItemId)3139,
                ItemType = ItemType.ClearCC
            });

            //Offensive
            Items.Add(new ItemStruct
            {
                ItemName = "BladeOfTheRuinedKing",
                ItemID   = (ItemId)3153,
                ItemType = ItemType.Offensive
            });


            //Deffensive
            Items.Add(new ItemStruct
            {
                ItemName = "Zhonyas",
                ItemID   = (ItemId)3157,
                ItemType = ItemType.Deffensive
            });

            //Utility
            Items.Add(new ItemStruct
            {
                ItemName = "TwinShadow",
                ItemID = (ItemId)3023,
                ItemType = ItemType.Utility
            });
        }
    }
}
