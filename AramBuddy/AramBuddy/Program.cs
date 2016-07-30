using System;
using System.Linq;
using AramBuddy.AutoShop;
using AramBuddy.MainCore;
using AramBuddy.MainCore.Logics;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace AramBuddy
{
    internal class Program
    {
        public static bool Loaded;

        public static float Timer;

        public static string Activemode;

        public static string Moveto;

        public static bool Moving;

        public static Menu MenuIni;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Game.MapId != GameMapId.HowlingAbyss)
            {
                Console.WriteLine(Game.MapId + " Is Not Supported By AramBuddy !");
                Chat.Print(Game.MapId + " Is Not Supported By AramBuddy !");
                return;
            }

            CheckVersion.Init();

            // Initialize the AutoShop.
            Setup.Init();

            Timer = Game.Time;
            Game.OnTick += Game_OnTick;
            Events.OnGameEnd += Events_OnGameEnd;
            Obj_AI_Base.OnLevelUp += LvlupSpells.Obj_AI_BaseOnOnLevelUp;
        }

        private static void Events_OnGameEnd(EventArgs args)
        {
            if (MenuIni["quit"].Cast<CheckBox>().CurrentValue)
                Core.DelayAction(() => Game.QuitGame(), 10000);
        }

        private static void Init()
        {
            MenuIni = MainMenu.AddMenu("AramBuddy", "AramBuddy");
            MenuIni.AddGroupLabel("AramBuddy Settings");
            MenuIni.Add("debug", new CheckBox("Enable Debugging Mode", false));
            MenuIni.Add("DisableSpells", new CheckBox("Disable Built-in Casting Logic", false));
            MenuIni.Add("quit", new CheckBox("Quit On Game End"));
            MenuIni.Add("Safe", new Slider("Safe Slider (Recommended 1250)", 1250, 0, 2500));
            MenuIni.AddLabel("More Value = more defensive playstyle");

            // Initialize Bot Functions.
            Brain.Init();

            Drawing.OnEndScene += Drawing_OnEndScene;
            Chat.Print("AramBuddy Loaded !");
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!MenuIni["debug"].Cast<CheckBox>().CurrentValue)
                return;
            Drawing.DrawText(
                Drawing.Width * 0.01f,
                Drawing.Height * 0.025f,
                System.Drawing.Color.White,
                "AllyTeamTotal: " + (int)Misc.TeamTotal(Player.Instance.ServerPosition) + " | EnemyTeamTotal: " + (int)Misc.TeamTotal(Player.Instance.ServerPosition, true) + " | MoveTo: " + Moveto
                + " | ActiveMode: " + Orbwalker.ActiveModesFlags + " | Alone: " + Brain.Alone() + " | AttackObject: " + ModesManager.AttackObject + " | LastTurretAttack: "
                + (Core.GameTickCount - Brain.LastTurretAttack) + " | SafeToDive: " + Misc.SafeToDive);

            Drawing.DrawText(
                Game.CursorPos.WorldToScreen().X + 50,
                Game.CursorPos.WorldToScreen().Y,
                System.Drawing.Color.Goldenrod,
                (Misc.TeamTotal(Game.CursorPos) - Misc.TeamTotal(Game.CursorPos, true)).ToString(),
                5);

            foreach (var hr in ObjectsManager.HealthRelics.Where(h => h.IsValid))
            {
                Circle.Draw(Color.White, hr.BoundingRadius * 2, hr.Position);
            }

            foreach (var trap in ObjectsManager.EnemyTraps)
            {
                Circle.Draw(Color.White, trap.BoundingRadius * 3, trap.Position);
            }

            if (Pathing.Position != null && Pathing.Position != Vector3.Zero)
            {
                Circle.Draw(Color.White, 100, Pathing.Position);
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null))
            {
                Circle.Draw(Color.Chartreuse, spell.Range, Player.Instance);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (!Loaded)
            {
                if (Game.Time - Timer >= 10)
                {
                    Loaded = true;

                    // Initialize The Bot.
                    Init();
                }
            }
            else
            {
                if (Player.Instance.IsDead)
                {
                    return;
                }
                Brain.Decisions();
            }
        }
    }
}
