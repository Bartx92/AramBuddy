﻿using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace AramBuddy.MainCore.Logics
{
    internal class Pathing
    {
        /// <summary>
        ///     Returns LastTeamFight Time.
        /// </summary>
        public static float LastTeamFight;

        /// <summary>
        ///     Bot movements position.
        /// </summary>
        public static Vector3 Position;

        /// <summary>
        ///     Picking best Position to move to.
        /// </summary>
        public static void BestPosition()
        {
            if (ObjectsManager.ClosestAlly != null && ObjectsManager.ClosestAlly.Distance(Player.Instance) > 3000)
            {
                Teleport.Cast();
            }

            if (Misc.TeamFight)
            {
                LastTeamFight = Core.GameTickCount;
            }

            // Hunting Bard chimes kappa.
            if (Player.Instance.Hero == Champion.Bard && ObjectsManager.BardChime != null && ObjectsManager.BardChime.Distance(Player.Instance) <= 500)
            {
                Program.Moveto = "BardChime";
                Position = ObjectsManager.BardChime.Position.Random();
                return;
            }

            // Moves to HealthRelic if the bot needs heal.
            if ((Player.Instance.HealthPercent < 75 || (Player.Instance.ManaPercent < 15 && Player.Instance.Mana > 0)) && ObjectsManager.HealthRelic != null)
            {
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, ObjectsManager.HealthRelic.Position, 500);
                if (ObjectsManager.EnemyTurret != null)
                {
                    var Circle = new Geometry.Polygon.Circle(ObjectsManager.EnemyTurret.ServerPosition, ObjectsManager.EnemyTurret.GetAutoAttackRange());
                    if ((!Circle.Points.Any(p => rect.IsInside(p)) || Circle.Points.Any(p => rect.IsInside(p)) && Misc.SafeToDive) && !EntityManager.Heroes.Enemies.Any(e => rect.IsInside(e.PrediectPosition()) && e.IsValidTarget() && !e.IsDead))
                    {
                        Program.Moveto = "HealthRelic";
                        Position = ObjectsManager.HealthRelic.Position.Random();
                        return;
                    }
                }
            }

            if (Player.Instance.HealthPercent <= 50 && ObjectsManager.ThreshLantern != null && ObjectsManager.ThreshLantern.Distance(Player.Instance) <= 800)
            {
                if (Player.Instance.Distance(ObjectsManager.ThreshLantern) > 300)
                {
                    Program.Moveto = "ThreshLantern";
                    Position = ObjectsManager.ThreshLantern.Position.Random();
                }
                else
                {
                    Player.UseObject(ObjectsManager.ThreshLantern);
                }
                return;
            }

            // Stays Under tower if the bot health under 15%.
            if ((ModesManager.Flee || (Player.Instance.HealthPercent < 10 && Player.Instance.CountAlliesInRange(2000) < 3)) && EntityManager.Heroes.Enemies.Count(e => !e.IsDead) > 0)
            {
                if (ObjectsManager.SafeAllyTurret != null)
                {
                    Program.Moveto = "SafeAllyTurret";
                    Position = ObjectsManager.SafeAllyTurret.PrediectPosition().Random();
                    return;
                }
                if (ObjectsManager.AllySpawn != null)
                {
                    Program.Moveto = "AllySpawn";
                    Position = ObjectsManager.AllySpawn.Position.Random();
                    return;
                }
            }

            // Moves to AllySpawn if the bot is diving and it's not safe to dive.
            if (((Player.Instance.IsUnderEnemyturret() && !Misc.SafeToDive) || Core.GameTickCount - Brain.LastTurretAttack < 2000) && ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn";
                Position = ObjectsManager.AllySpawn.Position.Random();
                return;
            }

            if (Player.Instance.IsMelee)
            {
                MeleeLogic();
            }
            else
            {
                RangedLogic();
            }
        }

        /// <summary>
        ///     Melee Champions Logic.
        /// </summary>
        public static void MeleeLogic()
        {
            // if there is a TeamFight follow NearestEnemy.
            if (Core.GameTickCount - LastTeamFight < 750 && Player.Instance.HealthPercent > 20 && ObjectsManager.NearestEnemy != null && Misc.TeamTotal(ObjectsManager.NearestEnemy.PrediectPosition()) >= Misc.TeamTotal(ObjectsManager.NearestEnemy.PrediectPosition(), true)
                && (ObjectsManager.NearestEnemy.PrediectPosition().UnderEnemyTurret() && Misc.SafeToDive || !ObjectsManager.NearestEnemy.PrediectPosition().UnderEnemyTurret()))
            {
                Program.Moveto = "NearestEnemy";
                Position = ObjectsManager.NearestEnemy.PrediectPosition().Random();
                return;
            }

            // if SafestAllyToFollow not exsist picks other to follow.
            if (ObjectsManager.SafestAllyToFollow != null)
            {
                // if SafestAllyToFollow exsist follow BestAllyToFollow.
                Program.Moveto = "SafestAllyToFollow";
                Position = ObjectsManager.SafestAllyToFollow.PrediectPosition().Random();
                return;
            }

            // if Minion exsists moves to Minion.
            if (ObjectsManager.Minion != null)
            {
                Program.Moveto = "Minion";
                Position = ObjectsManager.Minion.PrediectPosition().Random();
                return;
            }

            // if FarthestAllyToFollow exsists moves to FarthestAllyToFollow.
            if (ObjectsManager.FarthestAllyToFollow != null)
            {
                Program.Moveto = "FarthestAllyToFollow";
                Position = ObjectsManager.FarthestAllyToFollow.PrediectPosition().Random();
                return;
            }

            // if SecondTurret exsists moves to SecondTurret.
            if (ObjectsManager.SecondTurret != null)
            {
                Program.Moveto = "SecondTurret";
                Position = ObjectsManager.SecondTurret.PrediectPosition().Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return;
            }

            // if SafeAllyTurret exsists moves to SafeAllyTurret.
            if (ObjectsManager.SafeAllyTurret != null)
            {
                Program.Moveto = "SafeAllyTurret";
                Position = ObjectsManager.SafeAllyTurret.PrediectPosition().Random();
                return;
            }

            // if ClosesetAllyTurret exsists moves to ClosesetAllyTurret.
            if (ObjectsManager.ClosesetAllyTurret != null)
            {
                Program.Moveto = "ClosesetAllyTurret";
                Position = ObjectsManager.ClosesetAllyTurret.PrediectPosition().Random();
                return;
            }

            // Well if it ends up like this then best thing is to let it end.
            if (ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn";
                Position = ObjectsManager.AllySpawn.Position.Random();
            }
        }

        /// <summary>
        ///     Ranged Champions Logic.
        /// </summary>
        public static void RangedLogic()
        {
            // if SafestAllyToFollow2 exsists moves to SafestAllyToFollow2.
            if (ObjectsManager.SafestAllyToFollow2 != null)
            {
                Program.Moveto = "SafestAllyToFollow2";
                Position = ObjectsManager.SafestAllyToFollow2.PrediectPosition().Random();
                return;
            }

            // if Minion not exsist picks other to follow.
            if (ObjectsManager.Minion != null)
            {
                Program.Moveto = "Minion";
                Position = ObjectsManager.Minion.PrediectPosition().Random();
                return;
            }

            // if SecondTurret exsists moves to SecondTurret.
            if (ObjectsManager.SecondTurret != null)
            {
                Program.Moveto = "SecondTurret";
                Position = ObjectsManager.SecondTurret.PrediectPosition().Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return;
            }

            // if SafeAllyTurret exsists moves to SafeAllyTurret.
            if (ObjectsManager.SafeAllyTurret != null)
            {
                Program.Moveto = "SafeAllyTurret";
                Position = ObjectsManager.SafeAllyTurret.PrediectPosition().Random();
                return;
            }

            // if ClosesetAllyTurret exsists moves to ClosesetAllyTurret.
            if (ObjectsManager.ClosesetAllyTurret != null)
            {
                Program.Moveto = "ClosesetAllyTurret";
                Position = ObjectsManager.ClosesetAllyTurret.PrediectPosition().Random();
                return;
            }

            // Well if it ends up like this then best thing is to let it end.
            if (ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn";
                Position = ObjectsManager.AllySpawn.Position.Random();
            }
        }

        /// <summary>
        ///     Returns last move time for the bot.
        /// </summary>
        private static float lastmove;

        /// <summary>
        ///     Sends movement commands.
        /// </summary>
        public static void MoveTo(Vector3 pos)
        {
            // This to prevent the bot from spamming unnecessary movements.
            if (!Player.Instance.Path.LastOrDefault().IsInRange(pos, 75) && !Player.Instance.IsInRange(pos, 75) && Core.GameTickCount - lastmove >= 500)
            {
                // This to prevent diving.
                if (pos.UnderEnemyTurret() && !Misc.SafeToDive)
                {
                    return;
                }

                // This to prevent Walking into walls, buildings or traps.
                if (NavMesh.GetCollisionFlags(pos) == CollisionFlags.Wall || NavMesh.GetCollisionFlags(pos) == CollisionFlags.Building
                    || ObjectsManager.EnemyTraps.Any(t => t.Trap.IsInRange(pos, t.Trap.BoundingRadius * 2)))
                {
                    return;
                }

                // If the bot alone uses IssueOrder.
                if (Orbwalker.DisableMovement)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, pos);
                }
                else
                {
                    Orbwalker.OrbwalkTo(pos);
                }

                // Returns last movement time.
                lastmove = Core.GameTickCount;
            }
        }
    }
}
