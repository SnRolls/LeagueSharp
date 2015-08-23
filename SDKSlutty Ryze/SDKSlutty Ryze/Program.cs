using System;
using LeagueSharp.SDK.Core.Events;

namespace SDKSlutty_Ryze
{

    internal class Program 
    {
        private static void Main(string[] args)
        {
            if (args != null)
            {
                try
                {
                    Load.OnLoad += Ryze.OnLoad;
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex);
                }
            }
        }
    }
}
