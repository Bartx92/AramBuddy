using AramBuddy.MainCore.Utility;
using static AramBuddy.Program;

namespace AramBuddy
{
    internal class Config
    {
        public static bool EnableActivator => MenuIni.CheckBoxValue("activator");
        public static bool EnableDebug => MenuIni.CheckBoxValue("debug");
        public static bool DisableSpellsCasting => MenuIni.CheckBoxValue("DisableSpells");
        public static bool QuitOnGameEnd => MenuIni.CheckBoxValue("quit");
        public static bool DontStealHR => MenuIni.CheckBoxValue("stealhr");
        public static int SafeValue => MenuIni.SliderValue("Safe");
        public static int HealthRelicHP => MenuIni.SliderValue("HRHP");
        public static int HealthRelicMP => MenuIni.SliderValue("HRMP");
    }
}
