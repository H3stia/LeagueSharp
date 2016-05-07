using System;
using LeagueSharp.SDK;

namespace BlitzcrankDK
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Events.OnLoad += EventsOnOnLoad;
        }

        private static void EventsOnOnLoad(object sender, EventArgs eventArgs)
        {
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Blitzcrank();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Could not load the assembly - {0}", exception);
                throw;
            }
        }
    }
}
