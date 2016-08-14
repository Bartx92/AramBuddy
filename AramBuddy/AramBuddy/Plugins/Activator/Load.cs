using System;
using AramBuddy.MainCore.Utility;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Activator
{
    internal class Load
    {
        internal static Menu MenuIni;
        public static void Init()
        {
            try
            {
                MenuIni = MainMenu.AddMenu("AB Activator", "AB Activator");
                Items.Potions.Init();
                Cleanse.Qss.Init();
                Items.Offence.Init();
                Items.Defence.Init();
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Load Error While Init", ex, Logger.LogLevel.Error);
            }
        }
    }
}
