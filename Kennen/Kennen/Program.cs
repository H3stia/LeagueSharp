using System;
using LeagueSharp.Common;

namespace Kennen
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Champion.Kennen();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Could not load assembly: {0}", exception);
                throw;
            }
        }
    }
}
