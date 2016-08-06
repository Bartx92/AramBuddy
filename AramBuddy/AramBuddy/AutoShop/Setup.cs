#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AramBuddy.AutoShop.Sequences;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;

#endregion

namespace AramBuddy.AutoShop
{
    /// <summary>
    ///     The class where AutoShop is set-up
    /// </summary>
    internal class Setup
    {
        /// <summary>
        ///     Path to the build folder, containing all the champion builds
        /// </summary>
        public static readonly string BuildPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\Builds";

        /// <summary>
        ///     Path to the temporary folder which contains the in-game cache
        /// </summary>
        public static readonly string TempPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\temp";

        /// <summary>
        ///     Path to the temporary file which contains the in-game cache
        /// </summary>
        public static readonly string TempFile = TempPath + "\\buildindex" + Player.Instance.NetworkId + Game.GameId + ".dat";

        /// <summary>
        ///     A Dictionary that contains all the builds detected
        ///     in ChampionName:BuildData format
        /// </summary>
        public static Dictionary<string, string> Builds = new Dictionary<string, string>();

        /// <summary>
        ///     The build detected for the current champion that
        ///     is being played.
        /// </summary>
        public static Build CurrentChampionBuild = new Build();

        /// <summary>
        ///     Initializes the AutoShop system
        /// </summary>
        public static void Init()
        {
            try
            {
                var useDefaultBuild = false;
                // When the game starts
                AramBuddy.Events.OnGameStart += Events_OnGameStart;

                // Item Bought Event, reduce the temp value when we buy the item.
                Events.OnBuyItem += delegate(int price)
                    {
                        Core.DelayAction(
                            () =>
                                {
                                    Buy.TempValue -= price;

                                    // Try to buy more than one item if we can afford it
                                    Buy.BuyNextItem(CurrentChampionBuild);
                                },
                            new Random().Next(1000 + Game.Ping, 3000 + Game.Ping));
                    };

                // Create the build path directory
                Directory.CreateDirectory(BuildPath);

                // Check if the index file exists
                if (!File.Exists(TempFile))
                {
                    // If not, create the index file
                    Buy.CreateIndexFile();
                }

                // Loop through all the builds in the build path directory
                foreach (var build in Directory.GetFiles(BuildPath))
                {
                    // Get the name of the champion from the build
                    var parsed = build.Replace(".json", "").Replace(BuildPath + "\\", "");

                    // Add the build to the Builds dictionary in a ChampionName : BuildData format
                    Builds.Add(parsed, File.ReadAllText(build));
                }

                // Check if there are any builds for our champion
                if (Builds.Keys.All(b => b != Player.Instance.ChampionName))
                {
                    // If not, warn the user
                    Logger.Send("There are no builds for your champion.", Logger.LogLevel.Warn);

                    // and Use Default build
                    if (Builds.Keys.Any(b => b.Equals(Build.BuildName())))
                    {
                        DefaultBuild();
                        Logger.Send("Using default build path!", Logger.LogLevel.Warn);
                    }
                    else
                    {
                        // Creates Default Build for the AutoShop
                        Logger.Send("Creating default build path!", Logger.LogLevel.Warn);
                        Build.Create();
                        useDefaultBuild = true;
                    }
                }

                if (useDefaultBuild)
                    return;

                // Check if the parse of the build for the champion completed successfully and output it to public
                // variable CurrentChampionBuild
                if (Builds.Any(b => b.Key == Player.Instance.ChampionName) && Builds.FirstOrDefault(b => b.Key == Player.Instance.ChampionName).Value.TryParseData(out CurrentChampionBuild))
                {
                    // If the parse is successful, notify the user that the initialization process is finished
                    Logger.Send("AutoShop has been fully and succesfully initialized!", Logger.LogLevel.Info);

                    // and set up event listeners
                    SetUpEventListeners();
                    if (Player.Instance.IsInShopRange())
                    {
                        Buy.BuyNextItem(CurrentChampionBuild);
                    }
                }
            }
            catch (Exception ex)
            {
                // An exception occured somewhere else. Notify the user of the error, and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop:", ex, Logger.LogLevel.Error);

                // Warn the user about the exception
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }

        /// <summary>
        ///     Method that sets up event listeners
        /// </summary>
        /// 
        public static void DefaultBuild()
        {
            try
            {
                // Use Default build
                if (Builds.Keys.Any(b => b.Equals(Build.BuildName())) && Builds.FirstOrDefault(b => b.Key.Equals(Build.BuildName())).Value.TryParseData(out CurrentChampionBuild))
                {
                    Logger.Send(Build.BuildName() + " build Loaded!", Logger.LogLevel.Info);

                    // and set up event listeners
                    SetUpEventListeners();
                    if (Player.Instance.IsInShopRange())
                    {
                        Buy.BuyNextItem(CurrentChampionBuild);
                    }
                }
                else
                {
                    // An error occured during parsing. Catch the error and print it in the console
                    Logger.Send("The selected AutoShop JSON could not be parsed.", Logger.LogLevel.Error);

                    Logger.Send("No build is currently used!", Logger.LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                // An exception occured somewhere else. Notify the user of the error, and print the exception to the console
                Logger.Send("Exception occurred on initialization of AutoShop:", ex, Logger.LogLevel.Error);

                // Warn the user about the exception
                Logger.Send("Exception occurred during AutoShop initialization. AutoShop will most likely NOT work properly!", Logger.LogLevel.Warn);
            }
        }

        /// <summary>
        ///     Method that sets up event listeners
        /// </summary>
        private static void SetUpEventListeners()
        {
            // When we can buy items
            Events.OnBuyAllow += Events_OnBuyAllow;

            // When the user forced a build reset
            Events.OnBuildReset += Events_OnBuildReset;

            // When the game ends
            AramBuddy.Events.OnGameEnd += Events_OnGameEnd;
        }

        /// <summary>
        ///     Fired when the game starts
        /// </summary>
        /// <param name="args">Arguments providing with information about the GameOnLoad</param>
        private static void Events_OnGameStart(EventArgs args)
        {
            // Delete the index file if it exists
            if (File.Exists(TempFile))
            {
                File.Delete(TempFile);
            }
        }

        /// <summary>
        ///     Fired when the game ends
        /// </summary>
        /// <param name="args">Arguments providing with information about the GameEnd</param>
        private static void Events_OnGameEnd(EventArgs args)
        {
            // Delete the index file if it exists
            if (File.Exists(TempFile))
            {
                File.Delete(TempFile);
            }
        }

        /// <summary>
        ///     Fired when a build reset is forced
        /// </summary>
        /// <param name="args">Arguments of the event</param>
        private static void Events_OnBuildReset(EventArgs args)
        {
            // Notify the user that the build has been reset
            Logger.Send("Build has been reset!", Logger.LogLevel.Info);

            // Reset the build index, restarting the build process from the start
            Buy.ResetIndex();
        }

        /// <summary>
        ///     Fired when buying is allowed
        /// </summary>
        /// <param name="args">Arguments of the event</param>
        private static void Events_OnBuyAllow(EventArgs args)
        {
            // Notify the user that we are going to try to buy items now
            Logger.Send("Can buy items", Logger.LogLevel.Info);

            // Attempt to buy as many consecutive items on the build as we can
            Buy.BuyNextItem(CurrentChampionBuild);
        }
    }
}
