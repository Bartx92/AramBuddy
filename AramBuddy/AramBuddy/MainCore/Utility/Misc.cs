using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace AramBuddy.MainCore.Utility
{
    public static class Misc
    {
        /// <summary>
        ///     Returns true if target Is CC'D.
        /// </summary>
        public static bool IsCC(this Obj_AI_Base target)
        {
            return !target.CanMove || target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Fear)
                   || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Suppression) || target.HasBuffOfType(BuffType.Taunt)
                   || target.HasBuffOfType(BuffType.Sleep);
        }

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
                       && (ObjectsManager.EnemyTurret.CountMinions() > 2 || ObjectsManager.EnemyTurret.CountAlliesInRange(825) > 1);
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
        ///     Returns true if Vector3 is UnderEnemyTurret.
        /// </summary>
        public static bool UnderEnemyTurret(this Vector2 pos)
        {
            return EntityManager.Turrets.Enemies.Any(t => !t.IsDead && t.IsValidTarget() && t.IsInRange(pos, t.GetAutoAttackRange() + (Player.Instance.BoundingRadius * 2)));
        }

        /// <summary>
        ///     Returns true if your team is teamfighting.
        /// </summary>
        public static bool TeamFight
        {
            get
            {
                return EntityManager.Heroes.AllHeroes.Count(a => a.IsAttackPlayer() && a.IsValidTarget()) >= 2;
            }
        }

        /// <summary>
        ///     Class for getting if the figths info.
        /// </summary>
        public class LastAttack
        {
            public AIHeroClient Attacker;
            public AIHeroClient Target;
            public float LastAttackSent;

            public LastAttack(AIHeroClient from, AIHeroClient target)
            {
                this.Attacker = from;
                this.Target = target;
                this.LastAttackSent = 0f;
            }
        }

        /// <summary>
        ///     Returns True if the target is attacking a player.
        /// </summary>
        public static bool IsAttackPlayer(this AIHeroClient target)
        {
            return AutoAttacks.FirstOrDefault(a => a.Attacker.NetworkId.Equals(target.NetworkId) && 500 > Core.GameTickCount - a.LastAttackSent) != null;
        }

        /// <summary>
        ///     Save all Attacks into list.
        /// </summary>
        public static List<LastAttack> AutoAttacks = new List<LastAttack>();

        /// <summary>
        ///     Returns The predicted position for the target.
        /// </summary>
        public static Vector3 PrediectPosition(this Obj_AI_Base target)
        {
            return Prediction.Position.PredictUnitPosition(target, 100 + Game.Ping).To3D();
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
