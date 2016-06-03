using System;
using LeagueSharp.SDK;

namespace KarmaDK
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrap.Init();
            Events.OnLoad += EventsOnOnLoad;
        }

        private static void EventsOnOnLoad(object sender, EventArgs eventArgs)
        {
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Karma();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Could not load the assembly - {0}", exception);
                throw;
            }
        }
    }
}
