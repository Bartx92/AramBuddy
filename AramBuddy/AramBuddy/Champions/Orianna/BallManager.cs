using System;
using System.Linq;
using EloBuddy;

namespace AramBuddy.Champions.Orianna
{
    class BallManager
    {
        internal static Obj_AI_Base OriannaBall;

        internal static void Init()
        {
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            OriannaBall = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(o => o != null && (o.HasBuff("OrianaGhostSelf") || o.HasBuff("OrianaGhost") && (o.GetBuff("OrianaGhost").Caster.IsMe || o.GetBuff("OrianaGhostSelf").Caster.IsMe)));
        }
    }
}
