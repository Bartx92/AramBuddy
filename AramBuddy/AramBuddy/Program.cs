using System;
using System.Linq;
using AramBuddy.AutoShop;
using AramBuddy.Champions;
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
            if (Game.MapId != GameMapId.HowlingAbyss)
            {
                Console.WriteLine(Game.MapId + " Is Not Supported By AramBuddy !");
                Chat.Print(Game.MapId + " Is Not Supported By AramBuddy !");
                return;
            }

            // Checks for updates
            CheckVersion.Init();

            // Initialize the AutoShop.
            Setup.Init();

            Chat.OnInput += delegate (ChatInputEventArgs msg)
            {
                if (msg.Input.Equals("Load Custom", StringComparison.CurrentCultureIgnoreCase) && !CustomChamp)
                {
                    var Instance = (Base)Activator.CreateInstance(null, "AramBuddy.Champions." + Player.Instance.Hero + "." + Player.Instance.Hero).Unwrap();
                    CustomChamp = true;
                    msg.Process = false;
                }
            };

            Timer = Game.Time;
            Game.OnTick += Game_OnTick;
            Events.OnGameEnd += Events_OnGameEnd;
            Player.OnPostIssueOrder += Player_OnPostIssueOrder;
        }
        
        private static void Player_OnPostIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
                MoveToCommands++;
        }

        private static void Events_OnGameEnd(EventArgs args)
        {
            if (MenuIni["quit"].Cast<CheckBox>().CurrentValue)
                Core.DelayAction(() => Game.QuitGame(), 10250 + Game.Ping);
        }

        private static void Init()
        {
            if (Orbwalker.MovementDelay < 250)
            {
                Orbwalker.MovementDelay = 250 + Game.Ping;
            }

            MenuIni = MainMenu.AddMenu("AramBuddy", "AramBuddy");
            SpellsMenu = MenuIni.AddSubMenu("Spells");
            MenuIni.AddGroupLabel("AramBuddy Settings");
            MenuIni.Add("debug", new CheckBox("Enable Debugging Mode", false));
            MenuIni.Add("DisableSpells", new CheckBox("Disable Built-in Casting Logic", false));
            MenuIni.Add("quit", new CheckBox("Quit On Game End"));
            MenuIni.Add("Safe", new Slider("Safe Slider (Recommended 1250)", 1250, 0, 2500));
            MenuIni.AddLabel("More Value = more defensive playstyle");

            SpellsMenu.AddGroupLabel("SummonerSpells");
            SpellsMenu.Add("Heal", new CheckBox("Use Heal"));
            SpellsMenu.Add("Barrier", new CheckBox("Use Barrier"));
            SpellsMenu.Add("Clarity", new CheckBox("Use Clarity"));
            SpellsMenu.Add("Ghost", new CheckBox("Use Ghost"));
            SpellsMenu.Add("Flash", new CheckBox("Use Flash"));
            SpellsMenu.Add("Cleanse", new CheckBox("Use Cleanse"));
            /*
            SpellsMenu.AddSeparator(0);
            SpellsMenu.AddGroupLabel("Combo - " + Player.Instance.Hero);
            SpellsMenu.Add("Q" + Player.Instance.Hero, new CheckBox("Use Q"));
            SpellsMenu.Add("W" + Player.Instance.Hero, new CheckBox("Use W"));
            SpellsMenu.Add("E" + Player.Instance.Hero, new CheckBox("Use E"));
            SpellsMenu.Add("R" + Player.Instance.Hero, new CheckBox("Use R"));
            SpellsMenu.AddSeparator(0);
            SpellsMenu.AddGroupLabel("Harass - " + Player.Instance.Hero);
            SpellsMenu.Add("QHarass" + Player.Instance.Hero, new CheckBox("Use Q"));
            SpellsMenu.Add("WHarass" + Player.Instance.Hero, new CheckBox("Use W"));
            SpellsMenu.Add("EHarass" + Player.Instance.Hero, new CheckBox("Use E"));
            SpellsMenu.AddSeparator(0);
            SpellsMenu.AddGroupLabel("LaneClear - " + Player.Instance.Hero);
            SpellsMenu.Add("QLaneClear" + Player.Instance.Hero, new CheckBox("Use Q"));
            SpellsMenu.Add("WLaneClear" + Player.Instance.Hero, new CheckBox("Use W"));
            SpellsMenu.Add("ELaneClear" + Player.Instance.Hero, new CheckBox("Use E"));
            */
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
