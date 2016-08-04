using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;

namespace AramBuddy.Champions.Syndra
{
    class BallsManager
    {
        internal static List<Obj_AI_Minion> BallsList = new List<Obj_AI_Minion>();

        internal static void Init()
        {
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            foreach (var ball in ObjectManager.Get<Obj_AI_Minion>().Where(o => o != null && !o.IsDead && o.IsAlly && o.Health > 0 && o.BaseSkinName.Equals("SyndraSphere")))
            {
                if (!BallsList.Contains(ball))
                {
                    BallsList.Add(ball);
                }
            }
            BallsList.RemoveAll(b => b == null || b.IsDead || !b.IsValid || b.Health <= 0);
        }
    }
}
