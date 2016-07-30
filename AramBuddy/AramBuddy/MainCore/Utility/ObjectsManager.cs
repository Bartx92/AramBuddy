using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Utility
{
    internal class ObjectsManager
    {
        /// <summary>
        ///     Checks if healthrelic is created and add it to the list.
        /// </summary>
        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Add(sender);
                Chat.Print("create healthrelic");
            }

            if (TrapsNames.Contains(sender.Name) && sender.IsEnemy)
            {
                Chat.Print("create trap");
                EnemyTraps.Add((Obj_AI_Minion)sender);
            }
        }

        /// <summary>
        ///     Checks if healthrelic is deleted and remove it from the list.
        /// </summary>
        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Remove(sender);
                Chat.Print("delete healthrelic");
            }
            if (EnemyTraps.Contains(sender) && sender.IsEnemy)
            {
                EnemyTraps.Remove((Obj_AI_Minion)sender);
                Chat.Print("delete trap");
            }
        }

        /// <summary>
        ///     Traps Names.
        /// </summary>
        public static List<string> TrapsNames = new List<string>() { "Cupcake Trap", "Noxious Trap", "Jack In The Box" };

        /// <summary>
        ///     HealthRelics list.
        /// </summary>
        public static List<GameObject> HealthRelics = new List<GameObject>();

        /// <summary>
        ///     HealthRelics list.
        /// </summary>
        public static List<Obj_AI_Minion> EnemyTraps = new List<Obj_AI_Minion>();

        /// <summary>
        ///     Returns Nearest Enemy.
        /// </summary>
        public static AIHeroClient NearestEnemy
        {
            get
            {
                return EntityManager.Heroes.Enemies.OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault(e => e.IsValidTarget() && !e.IsDead && !e.IsZombie);
            }
        }

        /// <summary>
        ///     Returns Best Allies To Follow.
        /// </summary>
        public static IEnumerable<AIHeroClient> BestAlliesToFollow
        {
            get
            {
                return
                    EntityManager.Heroes.Allies.OrderByDescending(a => Misc.TeamTotal(a.ServerPosition))
                        .ThenByDescending(a => a.Distance(AllyNexues))
                        .Where(
                            a =>
                            a.IsValidTarget() && ((a.IsUnderEnemyturret() && Misc.SafeToDive) || !a.IsUnderEnemyturret()) && a.CountAlliesInRange(1250) > 1 && a.HealthPercent > 15
                            && !a.IsInShopRange() && !a.IsDead && !a.IsZombie && !a.IsMe
                            && (a.Spellbook.IsCharging || a.Spellbook.IsChanneling || a.Spellbook.IsAutoAttacking || a.IsAttackingPlayer || a.Spellbook.IsCastingSpell
                                || a.Path.LastOrDefault().Distance(a) > 50 || EntityManager.Heroes.Enemies.Any(e => e.IsValidTarget() && e.IsInRange(a, Player.Instance.GetAutoAttackRange()))));
            }
        }

        /// <summary>
        ///     Returns Farthest Ally To Follow.
        /// </summary>
        public static AIHeroClient FarthestAllyToFollow
        {
            get
            {
                return BestAlliesToFollow.FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns Best Safest Ally To Follow For Melee.
        /// </summary>
        public static AIHeroClient SafestAllyToFollow
        {
            get
            {
                return BestAlliesToFollow.OrderBy(a => a.Distance(Player.Instance)).FirstOrDefault(a => Misc.TeamTotal(a.ServerPosition) - Misc.TeamTotal(a.ServerPosition, true) > 0);
            }
        }

        /// <summary>
        ///     Returns Best Safest Ally To Follow For Ranged.
        /// </summary>
        public static AIHeroClient SafestAllyToFollow2
        {
            get
            {
                return
                    BestAlliesToFollow.OrderByDescending(a => Misc.TeamTotal(a.ServerPosition) - Misc.TeamTotal(a.ServerPosition, true))
                        .FirstOrDefault(a => a.CountAlliesInRange(1000) > a.CountEnemiesInRange(1000));
            }
        }

        /// <summary>
        ///     Returns Valid HealthRelic.
        /// </summary>
        public static GameObject HealthRelic
        {
            get
            {
                return HealthRelics.OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault(e => e.IsValid && e.Distance(Player.Instance) < 2000 && e.CountEnemiesInRange(1100) < 1);
            }
        }

        /// <summary>
        ///     Returns farthest Ally Minion.
        /// </summary>
        public static Obj_AI_Minion Minion
        {
            get
            {
                return
                    EntityManager.MinionsAndMonsters.AlliedMinions.OrderByDescending(a => a.Distance(AllyNexues))
                        .FirstOrDefault(
                            m =>
                            m.CountAlliesInRange(1250) - m.CountEnemiesInRange(1250) >= 0 && ((m.IsUnderEnemyturret() && Misc.SafeToDive) || !m.IsUnderEnemyturret()) && m.IsValidTarget(2500)
                            && m.IsValid && m.IsHPBarRendered && !m.IsDead && !m.IsZombie && m.HealthPercent > 25 && Misc.TeamTotal(m.ServerPosition) - Misc.TeamTotal(m.ServerPosition, true) >= 0);
            }
        }

        /// <summary>
        ///     Returns Second Tier Turret.
        /// </summary>
        public static Obj_AI_Turret SecondTurret
        {
            get
            {
                return Player.Instance.Team == GameObjectTeam.Order
                           ? EntityManager.Turrets.Allies.FirstOrDefault(t => t.IsValidTarget() && !t.IsDead && t.BaseSkinName.ToLower().Equals("ha_ap_orderturret"))
                           : EntityManager.Turrets.Allies.FirstOrDefault(t => t.IsValidTarget() && !t.IsDead && t.BaseSkinName.ToLower().Equals("ha_ap_chaosturret"));
            }
        }

        /// <summary>
        ///     Returns Closeset Ally Turret.
        /// </summary>
        public static Obj_AI_Turret ClosesetAllyTurret
        {
            get
            {
                return EntityManager.Turrets.Allies.OrderBy(t => t.Distance(Player.Instance)).FirstOrDefault(t => t.IsValidTarget() && !t.IsDead);
            }
        }

        /// <summary>
        ///     Returns Safest Ally Turret.
        /// </summary>
        public static Obj_AI_Turret SafeAllyTurret
        {
            get
            {
                return
                    EntityManager.Turrets.Allies.OrderBy(t => t.Distance(Player.Instance))
                        .FirstOrDefault(t => t.IsValidTarget() && !t.IsDead && t.CountAlliesInRange(t.GetAutoAttackRange()) > t.CountEnemiesInRange(t.GetAutoAttackRange()));
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Turret.
        /// </summary>
        public static Obj_AI_Turret EnemyTurret
        {
            get
            {
                return EntityManager.Turrets.Enemies.OrderBy(t => t.Distance(Player.Instance)).FirstOrDefault(t => t.IsValidTarget() && !t.IsDead);
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Inhbitor.
        /// </summary>
        public static Obj_BarracksDampener EnemyInhb
        {
            get
            {
                return ObjectManager.Get<Obj_BarracksDampener>().FirstOrDefault(i => i.IsEnemy && !i.IsDead);
            }
        }

        /// <summary>
        ///     Returns Enemy Nexues.
        /// </summary>
        public static Obj_HQ EnemyNexues
        {
            get
            {
                return ObjectManager.Get<Obj_HQ>().FirstOrDefault(i => i.IsEnemy);
            }
        }

        /// <summary>
        ///     Returns Ally Nexues.
        /// </summary>
        public static Obj_HQ AllyNexues
        {
            get
            {
                return ObjectManager.Get<Obj_HQ>().FirstOrDefault(i => i.IsAlly);
            }
        }

        /// <summary>
        ///     Returns Ally SpawnPoint.
        /// </summary>
        public static Obj_SpawnPoint AllySpawn
        {
            get
            {
                return ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(i => i.IsAlly);
            }
        }
    }
}
