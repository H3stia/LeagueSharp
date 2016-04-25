using System;
using LeagueSharp.SDK;

namespace Karma
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Events.OnLoad += (sender, eventArgs) =>
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
            };
        }
    }
}
