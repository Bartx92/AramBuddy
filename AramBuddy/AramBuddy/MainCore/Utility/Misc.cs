using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace AramBuddy.MainCore.Utility
{
    internal static class Misc
    {
        /// <summary>
        ///     Returns teams totals - used for picking best fights.
        /// </summary>
        public static float TeamTotal(Vector3 Position, bool Enemy = false)
        {
            float enemyteamTotal = 0;
            float allyteamTotal = 0;

            foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => !e.IsDead && e.IsValidTarget() && e.IsInRange(Position, Program.MenuIni["Safe"].Cast<Slider>().CurrentValue)))
            {
                enemyteamTotal += enemy.TotalShieldHealth();
                enemyteamTotal += enemy.Mana;
                enemyteamTotal += enemy.Armor;
                enemyteamTotal += enemy.SpellBlock;
                enemyteamTotal += enemy.TotalMagicalDamage;
                enemyteamTotal += enemy.TotalAttackDamage;
            }

            foreach (var ally in EntityManager.Heroes.Allies.Where(e => !e.IsDead && e.IsValidTarget() && e.IsInRange(Position, Program.MenuIni["Safe"].Cast<Slider>().CurrentValue)))
            {
                allyteamTotal += ally.TotalShieldHealth();
                allyteamTotal += ally.Mana;
                allyteamTotal += ally.Armor;
                allyteamTotal += ally.SpellBlock;
                allyteamTotal += ally.TotalMagicalDamage;
                allyteamTotal += ally.TotalAttackDamage;
            }
            return Enemy ? enemyteamTotal : allyteamTotal;
        }

        /// <summary>
        ///     Returns true if it's safe to dive.
        /// </summary>
        public static bool SafeToDive
        {
            get
            {
                return ObjectsManager.EnemyTurret != null && Player.Instance.HealthPercent > 10 && Core.GameTickCount - Brain.LastTurretAttack > 3000
                       && (ObjectsManager.EnemyTurret.CountMinions() > 2 || ObjectsManager.EnemyTurret.CountAlliesInRange(800) > 1);
            }
        }

        /// <summary>
        ///     Returns true if Vector3 is UnderEnemyTurret.
        /// </summary>
        public static bool UnderEnemyTurret(this Vector3 pos)
        {
            return EntityManager.Turrets.Enemies.Any(t => !t.IsDead && t.IsValidTarget() && t.IsInRange(pos, t.GetAutoAttackRange() + (Player.Instance.BoundingRadius * 2)));
        }

        /// <summary>
        ///     Returns Minions Count.
        /// </summary>
        public static float CountMinions(this Obj_AI_Base target, bool EnemyMinions = false, int range = 800)
        {
            return EnemyMinions
                       ? EntityManager.MinionsAndMonsters.EnemyMinions.Count(m => m.IsValidTarget() && m.IsInRange(target, range))
                       : EntityManager.MinionsAndMonsters.AlliedMinions.Count(m => m.IsValidTarget() && m.IsInRange(target, range));
        }

        /// <summary>
        ///     Randomize Vector3.
        /// </summary>
        public static Vector3 Random(this Vector3 pos)
        {
            var rnd = new Random();
            var X = rnd.Next((int)(pos.X - 200), (int)(pos.X + 200));
            var Y = rnd.Next((int)(pos.Y - 200), (int)(pos.Y + 200));

            return new Vector3(X, Y, pos.Z);
        }
    }
}
