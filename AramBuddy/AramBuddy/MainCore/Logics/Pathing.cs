using System;
using System.Linq;
using AramBuddy.MainCore.Logics.Casting;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using static AramBuddy.MainCore.Utility.Misc;
using static AramBuddy.Config;

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

            if (TeamFight)
            {
                LastTeamFight = Core.GameTickCount;
            }

            // Hunting Bard chimes kappa.
            if (Player.Instance.Hero == Champion.Bard && ObjectsManager.BardChime != null && ObjectsManager.BardChime.Distance(Player.Instance) <= 600)
            {
                Program.Moveto = "BardChime";
                Position = ObjectsManager.BardChime.Position.Random();
                return;
            }

            // Moves to HealthRelic if the bot needs heal.
            if ((Player.Instance.HealthPercent <= HealthRelicHP || (Player.Instance.ManaPercent <= HealthRelicMP && !Player.Instance.IsNoManaHero())) && ObjectsManager.HealthRelic != null)
            {
                var formana = Player.Instance.ManaPercent < HealthRelicMP && !Player.Instance.IsNoManaHero();
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, ObjectsManager.HealthRelic.Position, 375);
                if (ObjectsManager.EnemyTurret != null)
                {
                    var Circle = new Geometry.Polygon.Circle(ObjectsManager.EnemyTurret.ServerPosition, ObjectsManager.EnemyTurret.GetAutoAttackRange());
                    if ((!Circle.Points.Any(p => rect.IsInside(p)) || Circle.Points.Any(p => rect.IsInside(p)) && SafeToDive) && !EntityManager.Heroes.Enemies.Any(e => rect.IsInside(e.PredictPosition()) && e.IsValidTarget() && !e.IsDead))
                    {
                        if (ObjectsManager.HealthRelic.Name.Contains("Bard"))
                        {
                            if (!formana)
                            {
                                Program.Moveto = "BardShrine";
                                Position = ObjectsManager.HealthRelic.Position;
                                return;
                            }
                        }
                        else
                        {
                            Program.Moveto = "HealthRelic";
                            Position = ObjectsManager.HealthRelic.Position;
                            return;
                        }
                    }
                }
                else
                {
                    if (!EntityManager.Heroes.Enemies.Any(e => rect.IsInside(e.PredictPosition()) && e.IsValidTarget() && !e.IsDead))
                    {
                        if (ObjectsManager.HealthRelic.Name.Contains("Bard"))
                        {
                            if (!formana)
                            {
                                Program.Moveto = "BardShrine";
                                Position = ObjectsManager.HealthRelic.Position;
                                return;
                            }
                        }
                        else
                        {
                            Program.Moveto = "HealthRelic";
                            Position = ObjectsManager.HealthRelic.Position;
                            return;
                        }
                    }
                }
            }

            // Pick Thresh Lantern
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

            // Stays Under tower if the bot health under 10%.
            if ((ModesManager.Flee || (Player.Instance.HealthPercent < 10 && Player.Instance.CountAlliesInRange(2000) < 3)) && EntityManager.Heroes.Enemies.Count(e => !e.IsDead) > 0)
            {
                if (ObjectsManager.SafeAllyTurret != null)
                {
                    Program.Moveto = "SafeAllyTurret";
                    Position = ObjectsManager.SafeAllyTurret.PredictPosition().Random();
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
            if (((Player.Instance.IsUnderEnemyturret() && !SafeToDive) || Core.GameTickCount - Brain.LastTurretAttack < 2000) && ObjectsManager.AllySpawn != null)
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
        public static bool MeleeLogic()
        {
            // if there is a TeamFight follow NearestEnemy.
            if (Core.GameTickCount - LastTeamFight < 1000 && Player.Instance.HealthPercent > 25 && !ModesManager.Flee && ObjectsManager.NearestEnemy != null && Player.Instance.Health > ObjectsManager.NearestEnemy.Health && TeamTotal(ObjectsManager.NearestEnemy.PredictPosition()) >= TeamTotal(ObjectsManager.NearestEnemy.PredictPosition(), true)
                && (ObjectsManager.NearestEnemy.PredictPosition().UnderEnemyTurret() && SafeToDive || !ObjectsManager.NearestEnemy.PredictPosition().UnderEnemyTurret()))
            {
                // if there is a TeamFight move from NearestEnemy to nearestally.
                if (ObjectsManager.NearestAlly != null)
                {
                    Program.Moveto = "NearestEnemyToNearestAlly";
                    Position = ObjectsManager.NearestEnemy.PredictPosition().Extend(ObjectsManager.NearestAlly.PredictPosition(), KiteDistance(ObjectsManager.NearestEnemy)).To3D();
                    return true;
                }
                // if there is a TeamFight move from NearestEnemy to AllySpawn.
                if (ObjectsManager.AllySpawn != null)
                {
                    Program.Moveto = "NearestEnemyToAllySpawn";
                    Position = ObjectsManager.NearestEnemy.PredictPosition().Extend(ObjectsManager.AllySpawn.Position, KiteDistance(ObjectsManager.NearestEnemy)).To3D();
                    return true;
                }
            }

            // if Can AttackObject then start attacking THE DAMN OBJECT FFS.
            if (ObjectsManager.NearestEnemyObject != null && Player.Instance.HealthPercent > 20
                && (TeamTotal(ObjectsManager.NearestEnemyObject.Position) > TeamTotal(ObjectsManager.NearestEnemyObject.Position, true) || ObjectsManager.NearestEnemyObject.CountEnemiesInRange(1250) < 1))
            {
                var extendto = new Vector3();
                if (ObjectsManager.AllySpawn != null)
                {
                    extendto = ObjectsManager.AllySpawn.Position;
                }
                if (ObjectsManager.NearestMinion != null)
                {
                    extendto = ObjectsManager.NearestMinion.Position;
                }
                if (ObjectsManager.NearestAlly != null)
                {
                    extendto = ObjectsManager.NearestAlly.Position;
                }
                var extendtopos = ObjectsManager.NearestEnemyObject.Position.Extend(extendto, KiteDistance(ObjectsManager.NearestEnemyObject)).To3D();
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, ObjectsManager.NearestEnemyObject.Position, 500);
                var Enemy = EntityManager.Heroes.Enemies.Any(a => a != null && a.IsValidTarget() && new Geometry.Polygon.Circle(a.PredictPosition(), a.GetAutoAttackRange()).Points.Any(p => rect.IsInside(p)));
                if (!Enemy)
                {
                    if (ObjectsManager.EnemyTurret != null)
                    {
                        var TurretCircle = new Geometry.Polygon.Circle(ObjectsManager.EnemyTurret.ServerPosition, ObjectsManager.EnemyTurret.GetAutoAttackRange());

                        if (!TurretCircle.Points.Any(p => rect.IsInside(p)))
                        {
                            if (ObjectsManager.NearestEnemyObject is Obj_AI_Turret)
                            {
                                if (SafeToDive)
                                {
                                    Program.Moveto = "NearestEnemyObject";
                                    Position = extendtopos;
                                    return true;
                                }
                            }
                            else
                            {
                                Program.Moveto = "NearestEnemyObject2";
                                Position = extendtopos;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        Program.Moveto = "NearestEnemyObject3";
                        Position = extendtopos;
                        return true;
                    }
                }
            }

            // if SafestAllyToFollow not exsist picks other to follow.
            if (ObjectsManager.SafestAllyToFollow != null)
            {
                // if SafestAllyToFollow exsist follow BestAllyToFollow.
                Program.Moveto = "SafestAllyToFollow";
                Position = ObjectsManager.SafestAllyToFollow.PredictPosition().Random();
                return true;
            }

            // if Minion exsists moves to Minion.
            if (ObjectsManager.Minion != null)
            {
                Program.Moveto = "Minion";
                Position = ObjectsManager.Minion.PredictPosition().Random();
                return true;
            }

            // if FarthestAllyToFollow exsists moves to FarthestAllyToFollow.
            if (ObjectsManager.FarthestAllyToFollow != null)
            {
                Program.Moveto = "FarthestAllyToFollow";
                Position = ObjectsManager.FarthestAllyToFollow.PredictPosition().Random();
                return true;
            }

            // if SecondTurret exsists moves to SecondTurret.
            if (ObjectsManager.SecondTurret != null)
            {
                Program.Moveto = "SecondTurret";
                Position = ObjectsManager.SecondTurret.PredictPosition().Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // if SafeAllyTurret exsists moves to SafeAllyTurret.
            if (ObjectsManager.SafeAllyTurret != null)
            {
                Program.Moveto = "SafeAllyTurret";
                Position = ObjectsManager.SafeAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // if ClosesetAllyTurret exsists moves to ClosesetAllyTurret.
            if (ObjectsManager.ClosesetAllyTurret != null)
            {
                Program.Moveto = "ClosesetAllyTurret";
                Position = ObjectsManager.ClosesetAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // Well if it ends up like this then best thing is to let it end.
            if (ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn";
                Position = ObjectsManager.AllySpawn.Position.Random();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Ranged Champions Logic.
        /// </summary>
        public static bool RangedLogic()
        {
            // TeamFighting Logic.
            if (Core.GameTickCount - LastTeamFight < 1000 && Player.Instance.HealthPercent > 25 && !ModesManager.Flee && ObjectsManager.NearestEnemy != null && TeamTotal(ObjectsManager.NearestEnemy.PredictPosition()) >= TeamTotal(ObjectsManager.NearestEnemy.PredictPosition(), true)
                && (ObjectsManager.NearestEnemy.PredictPosition().UnderEnemyTurret() && SafeToDive || !ObjectsManager.NearestEnemy.PredictPosition().UnderEnemyTurret()))
            {
                // if there is a TeamFight move from NearestEnemy to nearestally.
                if (ObjectsManager.NearestAlly != null)
                {
                    Program.Moveto = "NearestEnemyToNearestAlly";
                    Position = ObjectsManager.NearestEnemy.PredictPosition().Extend(ObjectsManager.NearestAlly.PredictPosition(), KiteDistance(ObjectsManager.NearestEnemy)).To3D();
                    return true;
                }
                // if there is a TeamFight move from NearestEnemy to AllySpawn.
                if (ObjectsManager.AllySpawn != null)
                {
                    Program.Moveto = "NearestEnemyToAllySpawn";
                    Position = ObjectsManager.NearestEnemy.PredictPosition().Extend(ObjectsManager.AllySpawn.Position, KiteDistance(ObjectsManager.NearestEnemy)).To3D();
                    return true;
                }
            }

            // if Can AttackObject then start attacking THE DAMN OBJECT FFS.
            if (ObjectsManager.NearestEnemyObject != null && Player.Instance.HealthPercent > 20
                && (TeamTotal(ObjectsManager.NearestEnemyObject.Position) > TeamTotal(ObjectsManager.NearestEnemyObject.Position, true) || ObjectsManager.NearestEnemyObject.CountEnemiesInRange(1250) < 1))
            {
                var extendto = new Vector3();
                if (ObjectsManager.AllySpawn != null)
                {
                    extendto = ObjectsManager.AllySpawn.Position;
                }
                if (ObjectsManager.NearestMinion != null)
                {
                    extendto = ObjectsManager.NearestMinion.Position;
                }
                if (ObjectsManager.NearestAlly != null)
                {
                    extendto = ObjectsManager.NearestAlly.Position;
                }
                var extendtopos = ObjectsManager.NearestEnemyObject.Position.Extend(extendto, KiteDistance(ObjectsManager.NearestEnemyObject)).To3D();
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, ObjectsManager.NearestEnemyObject.Position, 500);
                var Enemy = EntityManager.Heroes.Enemies.Any(a => a != null && a.IsValidTarget() && new Geometry.Polygon.Circle(a.PredictPosition(), a.GetAutoAttackRange()).Points.Any(p => rect.IsInside(p)));
                if (!Enemy)
                {
                    if (ObjectsManager.EnemyTurret != null)
                    {
                        var TurretCircle = new Geometry.Polygon.Circle(ObjectsManager.EnemyTurret.ServerPosition, ObjectsManager.EnemyTurret.GetAutoAttackRange());

                        if (!TurretCircle.Points.Any(p => rect.IsInside(p)))
                        {
                            if (ObjectsManager.NearestEnemyObject is Obj_AI_Turret)
                            {
                                if (SafeToDive)
                                {
                                    Program.Moveto = "NearestEnemyObject";
                                    Position = extendtopos;
                                    return true;
                                }
                            }
                            else
                            {
                                Program.Moveto = "NearestEnemyObject2";
                                Position = extendtopos;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        Program.Moveto = "NearestEnemyObject3";
                        Position = extendtopos;
                        return true;
                    }
                }
            }

            // if SafestAllyToFollow2 exsists moves to SafestAllyToFollow2.
            if (ObjectsManager.SafestAllyToFollow2 != null)
            {
                Program.Moveto = "SafestAllyToFollow2";
                Position = ObjectsManager.SafestAllyToFollow2.PredictPosition().Random();
                return true;
            }

            // if Minion not exsist picks other to follow.
            if (ObjectsManager.Minion != null)
            {
                Program.Moveto = "Minion";
                Position = ObjectsManager.Minion.PredictPosition().Random();
                return true;
            }

            // if SecondTurret exsists moves to SecondTurret.
            if (ObjectsManager.SecondTurret != null)
            {
                Program.Moveto = "SecondTurret";
                Position = ObjectsManager.SecondTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // if SafeAllyTurret exsists moves to SafeAllyTurret.
            if (ObjectsManager.SafeAllyTurret != null)
            {
                Program.Moveto = "SafeAllyTurret";
                Position = ObjectsManager.SafeAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // if ClosesetAllyTurret exsists moves to ClosesetAllyTurret.
            if (ObjectsManager.ClosesetAllyTurret != null)
            {
                Program.Moveto = "ClosesetAllyTurret";
                Position = ObjectsManager.ClosesetAllyTurret.ServerPosition.Extend(ObjectsManager.AllySpawn, 400).To3D().Random();
                return true;
            }

            // Well if it ends up like this then best thing is to let it end.
            if (ObjectsManager.AllySpawn != null)
            {
                Program.Moveto = "AllySpawn";
                Position = ObjectsManager.AllySpawn.Position.Random();
                return true;
            }
            return false;
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
            var rnddis = new Random().Next(50, 100);
            if (!Player.Instance.Path.LastOrDefault().IsInRange(pos, rnddis) && !Player.Instance.IsInRange(pos, rnddis) && Core.GameTickCount - lastmove >= new Random().Next(300 + Game.Ping, 1000 + Game.Ping))
            {
                // This to prevent diving.
                if (pos.UnderEnemyTurret() && !SafeToDive)
                {
                    return;
                }

                // This to prevent Walking into walls, buildings or traps.
                if (pos.IsWall() || pos.IsBuilding() || ObjectsManager.EnemyTraps.Any(t => t.Trap.IsInRange(pos, t.Trap.BoundingRadius * 2)))
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
