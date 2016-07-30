using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;

namespace AramBuddy
{
    /// <summary>
    ///     A class containing all the globally used events in AutoBuddy
    /// </summary>
    internal static class Events
    {
        /// <summary>
        ///     A handler for the OnGameEnd event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnGameEndHandler(EventArgs args);

        /// <summary>
        ///     A handler for the OnGameStart event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnGameStartHandler(EventArgs args);

        static Events()
        {
            // Invoke the OnGameEnd event

            #region OnGameEnd

            // Variable used to make sure that the event invoke isn't spammed and is only called once
            var gameEndNotified = false;

            // Every time the game ticks (1ms)
            Game.OnTick += delegate
                {
                    // Make sure we're not repeating the invoke
                    if (gameEndNotified)
                    {
                        return;
                    }

                    // Get the enemy nexus
                    var nexus = ObjectManager.Get<Obj_HQ>();

                    // Check and return if the nexus is null
                    if (nexus == null)
                    {
                        return;
                    }

                    // If the nexus is dead or its health is equal to 0
                    if (nexus.Any(n => n.IsDead) || nexus.Any(n => n.Health == 0.0f))
                    {
                        // Invoke the event
                        OnGameEnd(EventArgs.Empty);

                        // Set gameEndNotified to true, as the event has been completed
                        gameEndNotified = true;

                        Console.WriteLine("Game ended!");
                    }
                };

            #endregion

            // Invoke the OnGameStart event

            #region OnGameStart

            // When the player object is created
            Loading.OnLoadingComplete += delegate(EventArgs args)
                {
                    if (Player.Instance.IsInShopRange())
                    {
                        //OnGameStart(EventArgs.Empty);

                        Console.WriteLine("Game started!");
                    }
                };

            #endregion
        }

        /// <summary>
        ///     Fires when the game has ended
        /// </summary>
        public static event OnGameEndHandler OnGameEnd;

        /// <summary>
        /// Fires when the game has started
        /// </summary>
        public static event OnGameStartHandler OnGameStart;
    }
}
