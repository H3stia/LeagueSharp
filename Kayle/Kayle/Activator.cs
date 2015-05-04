using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kayle
{
    internal class BuffList
    {
        public string ChampionName { get; set; }
        public string DisplayName { get; set; }
        public string BuffName { get; set; }
        public bool DefaultValue { get; set; }
        public int Delay { get; set; }
    }

    internal class Activator
    {
        public readonly List<BuffList> BuffList = new List<BuffList>();
        public Activator()
        {

            BuffList.Add(new BuffList
            {
                ChampionName = "Fizz",
                DisplayName = "Fizz (R)",
                BuffName = "FizzChurnTheWatersCling",
                DefaultValue = true,
                Delay = 0
            });

            BuffList.Add(new BuffList
            {
                ChampionName = "Karthus",
                DisplayName = "Karthus (R)",
                BuffName = "KarthusFallenOne",
                DefaultValue = true,
                Delay = 0
            });

            BuffList.Add(new BuffList
            {
                ChampionName = "Zed",
                DisplayName = "Zed (R)",
                BuffName = "ZedUltExecute",
                DefaultValue = true,
                Delay = 3
            });
        }
    }
}
