using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;

namespace AramBuddy.Champions.Gangplank
{
    class BarrelsManager
    {
        internal static List<Obj_AI_Minion> BarrelsList = new List<Obj_AI_Minion>();

        internal static void Init()
        {
            BarrelsList.Clear();
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            foreach (var barrel in ObjectManager.Get<Obj_AI_Minion>().Where(o => !o.IsDead && o.HasBuff("GangplankEBarrelActive") && o.GetBuff("GangplankEBarrelActive").Caster.IsMe))
            {
                if (!BarrelsList.Contains(barrel))
                {
                    BarrelsList.Add(barrel);
                }
            }
            BarrelsList.RemoveAll(o => o.IsDead || o.Health <= 0);
        }
    }
}
