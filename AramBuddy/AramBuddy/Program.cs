using System;
using System.IO;
using System.Linq;
using AramBuddy.MainCore;
using AramBuddy.MainCore.Logics;
using AramBuddy.MainCore.Utility;
using AramBuddy.Plugins.Champions;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using static AramBuddy.Config;

namespace AramBuddy
{
    internal class Program
    {
        public static int MoveToCommands;
        public static bool CustomChamp;
        public static bool Loaded;
        public static float Timer;
        public static string Moveto;

        public static Menu MenuIni, SpellsMenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                if (Game.MapId != GameMapId.HowlingAbyss)
                {
                    Logger.Send(Game.MapId + " IS NOT Supported By AramBuddy !", Logger.LogLevel.Warn);
                    Chat.Print(Game.MapId + " IS NOT Supported By AramBuddy !");
                    return;
                }

                // Inits KappaEvade
                KappaEvade.KappaEvade.Init();

                // Checks for updates
                CheckVersion.Init();

                // Initialize the AutoShop.
                AutoShop.Setup.Init();

                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\temp\\temp123.dat"))
                {
                    File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\temp\\temp123.dat");
                }

                Chat.OnInput += delegate (ChatInputEventArgs msg)
                {
                    if (msg.Input.Equals("addcount", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Chat.Print(EntityManager.Heroes.AllHeroes.Count(a => a.Added()));
                        msg.Process = false;
                    }
                    if (msg.Input.Equals("Load Custom", StringComparison.CurrentCultureIgnoreCase) && !CustomChamp)
                    {
                        var Instance = (Base)Activator.CreateInstance(null, "AramBuddy.Plugins.Champions." + Player.Instance.Hero + "." + Player.Instance.Hero).Unwrap();
                        CustomChamp = true;
                        msg.Process = false;
                        Logger.Send("Loaded Custom Champion " + Player.Instance.Hero, Logger.LogLevel.Info);
                    }
                };

                Timer = Game.Time;
                Game.OnTick += Game_OnTick;
                Events.OnGameEnd += Events_OnGameEnd;
                Player.OnPostIssueOrder += Player_OnPostIssueOrder;
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Loading_OnLoadingComplete", ex, Logger.LogLevel.Error);
            }
        }
        
        private static void Player_OnPostIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            try
            {
                if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
                    MoveToCommands++;
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Player_OnPostIssueOrder", ex, Logger.LogLevel.Error);
            }
        }

        private static void Events_OnGameEnd(EventArgs args)
        {
            try
            {
                if (QuitOnGameEnd)
                    Core.DelayAction(() => Game.QuitGame(), new Random().Next(15000 + Game.Ping, 30000 + Game.Ping));
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Events_OnGameEnd", ex, Logger.LogLevel.Error);
            }
        }

        private static void Init()
        {
            try
            {
                if (Orbwalker.MovementDelay < 200)
                {
                    Orbwalker.MovementDelay = 200 + Game.Ping;
                }

                MenuIni = MainMenu.AddMenu("AramBuddy", "AramBuddy");
                SpellsMenu = MenuIni.AddSubMenu("Spells");
                MenuIni.AddGroupLabel("AramBuddy Settings");
                var debug = MenuIni.CreateCheckBox("debug", "Enable Debugging Mode");
                var activator = MenuIni.CreateCheckBox("activator", "Enable Built-In Activator");
                var DisableSpells = MenuIni.CreateCheckBox("DisableSpells", "Disable Built-in Casting Logic", false);
                var quit = MenuIni.CreateCheckBox("quit", "Quit On Game End");
                var stealhr = MenuIni.CreateCheckBox("stealhr", "Dont Steal Health Relics From Allies", false);
                MenuIni.AddSeparator(0);
                var Safe = MenuIni.CreateSlider("Safe", "Safe Slider (Recommended 1250)", 1250, 0, 2500);
                MenuIni.AddLabel("More Safe Value = more defensive playstyle");
                MenuIni.AddSeparator(0);
                var HRHP = MenuIni.CreateSlider("HRHP", "Health Percent To Pick Health Relics (Recommended 75%)", 75);
                var HRMP = MenuIni.CreateSlider("HRMP", "Mana Percent To Pick Health Relics (Recommended 15%)", 15);
                MenuIni.AddSeparator(0);
                var Reset = MenuIni.CreateCheckBox("reset", "Reset All Settings To Default", false);
                Reset.OnValueChange += delegate(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                    {
                        if (args.NewValue)
                        {
                            Reset.CurrentValue = false;
                            debug.CurrentValue = true;
                            activator.CurrentValue = true;
                            DisableSpells.CurrentValue = false;
                            quit.CurrentValue = true;
                            stealhr.CurrentValue = false;
                            Safe.CurrentValue = 1250;
                            HRHP.CurrentValue = 75;
                            HRMP.CurrentValue = 15;
                        }
                    };

                SpellsMenu.AddGroupLabel("SummonerSpells");
                SpellsMenu.Add("Heal", new CheckBox("Use Heal"));
                SpellsMenu.Add("Barrier", new CheckBox("Use Barrier"));
                SpellsMenu.Add("Clarity", new CheckBox("Use Clarity"));
                SpellsMenu.Add("Ghost", new CheckBox("Use Ghost"));
                SpellsMenu.Add("Flash", new CheckBox("Use Flash"));
                SpellsMenu.Add("Cleanse", new CheckBox("Use Cleanse"));

                // Initialize Bot Functions.
                Brain.Init();

                // Inits Activator
                if(EnableActivator)
                    Plugins.Activator.Load.Init();

                Drawing.OnEndScene += Drawing_OnEndScene;
                Chat.Print("AramBuddy Loaded !");
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            try
            {
                if (!EnableDebug) return;

                Drawing.DrawText(
                    Drawing.Width * 0.01f,
                    Drawing.Height * 0.025f,
                    System.Drawing.Color.White,
                    "AllyTeamTotal: " + (int)Misc.TeamTotal(Player.Instance.PredictPosition()) + " | EnemyTeamTotal: " + (int)Misc.TeamTotal(Player.Instance.PredictPosition(), true) + " | MoveTo: "
                    + Moveto + " | ActiveMode: " + Orbwalker.ActiveModesFlags + " | Alone: " + Brain.Alone() + " | AttackObject: " + ModesManager.AttackObject + " | LastTurretAttack: "
                    + (Core.GameTickCount - Brain.LastTurretAttack) + " | SafeToDive: " + Misc.SafeToDive + " | LastTeamFight: " + (int)(Core.GameTickCount - Pathing.LastTeamFight));

                Drawing.DrawText(Drawing.Width * 0.01f, Drawing.Height * 0.050f, System.Drawing.Color.White, "Movement Commands Issued: " + MoveToCommands);

                Drawing.DrawText(
                    Game.CursorPos.WorldToScreen().X + 50,
                    Game.CursorPos.WorldToScreen().Y,
                    System.Drawing.Color.Goldenrod,
                    (Misc.TeamTotal(Game.CursorPos) - Misc.TeamTotal(Game.CursorPos, true)).ToString(),
                    5);

                foreach (var hr in ObjectsManager.HealthRelics.Where(h => h.IsValid && !h.IsDead))
                {
                    Circle.Draw(Color.White, hr.BoundingRadius * 2, hr.Position);
                }

                foreach (var trap in ObjectsManager.EnemyTraps)
                {
                    Circle.Draw(Color.White, trap.Trap.BoundingRadius * 2, trap.Trap.Position);
                }

                if (Pathing.Position != null && Pathing.Position != Vector3.Zero)
                {
                    Circle.Draw(Color.White, 100, Pathing.Position);
                }

                foreach (var spell in ModesManager.Spelllist.Where(s => s != null))
                {
                    Circle.Draw(Color.Chartreuse, spell.Range, Player.Instance);
                }

                foreach (var chime in ObjectsManager.BardChimes.Where(c => Player.Instance.Hero == Champion.Bard && c.IsValid && !c.IsDead))
                {
                    Circle.Draw(Color.White, chime.BoundingRadius * 2, chime.Position);
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Drawing_OnEndScene", ex, Logger.LogLevel.Error);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            try
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
                    if (!Player.Instance.IsDead)
                    {
                        Brain.Decisions();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Program Error At Game_OnTick", ex, Logger.LogLevel.Error);
            }
        }
    }
}
