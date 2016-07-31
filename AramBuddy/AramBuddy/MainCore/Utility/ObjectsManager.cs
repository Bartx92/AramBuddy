using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Utility
{
    internal class ObjectsManager
    {
        public static void Init()
        {
            // Clears and adds new HealthRelics and bardhealthshrines.
            HealthRelics.Clear();
            foreach (var hr in ObjectManager.Get<GameObject>().Where(o => o.Name.ToLower().Contains("healthrelic") && o.IsValid).Where(hr => hr != null))
            {
                HealthRelics.Add(hr);
            }
            foreach (var bardshrine in ObjectManager.Get<GameObject>().Where(o => o.Name.ToLower().Contains("bardhealthshrine") && o.IsAlly && o.IsValid).Where(hr => hr != null))
            {
                HealthRelics.Add(bardshrine);
            }

            // Clears and adds new Bard Chimes.
            BardChimes.Clear();
            foreach (var bardchime in ObjectManager.Get<GameObject>().Where(o => o.Name.ToLower().Contains("bardchimeminion") && o.IsAlly && o.IsValid).Where(hr => hr != null))
            {
                BardChimes.Add(bardchime);
            }

            // Clears and adds new EnemyTraps.
            EnemyTraps.Clear();
            foreach (var trap in ObjectManager.Get<Obj_AI_Minion>().Where(trap => trap.IsEnemy && !trap.IsDead))
            {
                if (TrapsNames.Contains(trap.Name))
                {
                    var ttrap = new traps { Trap = trap, IsSpecial = false };
                    EnemyTraps.Add(ttrap);
                }
                if (SpecialTrapsNames.Contains(trap.Name))
                {
                    var ttrap = new traps { Trap = trap, IsSpecial = true };
                    EnemyTraps.Add(ttrap);
                }
            }

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        /// <summary>
        ///     Checks if healthrelic or traps are created and add them to the list.
        /// </summary>
        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var caster = sender as Obj_AI_Base;
            if(caster == null) return;
            if (caster.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Add(caster);
                Logger.Send("Create healthrelic", Logger.LogLevel.Info);
            }

            if (TrapsNames.Contains(sender.Name) && sender.IsEnemy)
            {
                var trap = new traps { Trap = caster, IsSpecial = false };
                EnemyTraps.Add(trap);
                Logger.Send("Create Trap", Logger.LogLevel.Info);
            }

            if (caster.Name.ToLower().Contains("bardhealthshrine") && sender.IsAlly)
            {
                HealthRelics.Remove(caster);
                Logger.Send("Create BardHealthShrine", Logger.LogLevel.Info);
            }

            if (caster.Name.ToLower().Contains("bardchimeminion") && caster.IsAlly)
            {
                HealthRelics.Remove(caster);
                Logger.Send("Create BardcChimeMinion", Logger.LogLevel.Info);
            }

            if (SpecialTrapsNames.Contains(caster.Name) && caster.IsEnemy)
            {
                var trap = new traps { Trap = caster, IsSpecial = true };
                EnemyTraps.Add(trap);
                Logger.Send("Create Special Trap", Logger.LogLevel.Info);
            }
        }

        /// <summary>
        ///     Checks if healthrelic or traps are deleted and remove them from the list.
        /// </summary>
        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var caster = sender as Obj_AI_Base;
            if (caster == null) return;
            var trap = new traps { Trap = caster, IsSpecial = false };
            var Specialtrap = new traps { Trap = caster, IsSpecial = true };
            if (caster.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Remove(caster);
                Logger.Send("Delete healthrelic", Logger.LogLevel.Info);
            }
            if (EnemyTraps.Contains(trap) && caster.IsEnemy)
            {
                EnemyTraps.Remove(trap);
                Logger.Send("Delete Trap", Logger.LogLevel.Info);
            }
            if (caster.Name.ToLower().Contains("bardhealthshrine") && caster.IsAlly)
            {
                HealthRelics.Remove(caster);
                Logger.Send("Delete BardHealthShrine", Logger.LogLevel.Info);
            }
            if (caster.Name.ToLower().Contains("bardchimeminion") && caster.IsAlly)
            {
                HealthRelics.Remove(caster);
                Logger.Send("Delete BardcChimeMinion", Logger.LogLevel.Info);
            }
            if (EnemyTraps.Contains(Specialtrap) && caster.IsEnemy)
            {
                EnemyTraps.Remove(Specialtrap);
                Logger.Send("Delete Special Trap", Logger.LogLevel.Info);
            }
        }

        /// <summary>
        ///     traps struct.
        /// </summary>
        public struct traps
        {
            public Obj_AI_Base Trap;
            public bool IsSpecial;
        }

        /// <summary>
        ///     Traps Names.
        /// </summary>
        public static List<string> TrapsNames = new List<string> { "Cupcake Trap", "Noxious Trap", "Jack In The Box", "Ziggs_Base_E_placedMine.troy" };

        /// <summary>
        ///     Special Traps Names.
        /// </summary>
        public static List<string> SpecialTrapsNames = new List<string> { "Fizz_Base_R_OrbitFish.troy", "Gragas_Base_Q_Enemy", "Lux_Base_E_tar_aoe_red.troy", "Soraka_Base_E_rune.troy", "Ziggs_Base_W_aoe_red.troy", "Viktor_Catalyst_red.troy", "Viktor_base_W_AUG_red.troy", "Barrel" };
        
        /// <summary>
        ///     BardChimes list.
        /// </summary>
        public static List<GameObject> BardChimes = new List<GameObject>();

        /// <summary>
        ///     HealthRelics and BardHealthShrines list.
        /// </summary>
        public static List<GameObject> HealthRelics = new List<GameObject>();

        /// <summary>
        ///     EnemyTraps list.
        /// </summary>
        public static List<traps> EnemyTraps = new List<traps>();

        /// <summary>
        ///     Returns Valid HealthRelic and BardHealthShrine.
        /// </summary>
        public static GameObject HealthRelic
        {
            get
            {
                return HealthRelics.OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault(e => e.IsValid && e.Distance(Player.Instance) < 2000 && e.CountEnemiesInRange(1100) < 1);
            }
        }

        /// <summary>
        ///     Thresh Lantern.
        /// </summary>
        public static Obj_AI_Base ThreshLantern
        {
            get
            {
                return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(l => l.IsValid && !l.IsDead && Player.Instance.Hero != Champion.Thresh && (l.CountEnemiesInRange(1000) > 0 && Player.Instance.Distance(l) < 500 || l.CountEnemiesInRange(1000) < 1) && l.IsAlly && l.Name.Equals("ThreshLantern"));
            }
        }

        /// <summary>
        ///     BardChime.
        /// </summary>
        public static GameObject BardChime
        {
            get
            {
                return BardChimes.OrderBy(c => c.Distance(Player.Instance)).FirstOrDefault(l => l.IsValid && !l.IsDead && Player.Instance.Hero == Champion.Bard && (!l.Position.UnderEnemyTurret() || l.Position.UnderEnemyTurret() && Misc.SafeToDive) && l.IsAlly && (l.CountEnemiesInRange(1000) > 0 && Player.Instance.Distance(l) < 500 || l.CountEnemiesInRange(1000) < 1));
            }
        }

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
