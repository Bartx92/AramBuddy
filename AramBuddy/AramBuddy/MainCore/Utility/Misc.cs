using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using static AramBuddy.Config;

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
            float enemyteamTotal = EntityManager.Turrets.Enemies.Where(t => !t.IsDead && t.Health > 0 && t.CountEnemiesInRange(t.GetAutoAttackRange()) > 1).Sum(turret => turret.Health);
            float allyteamTotal = EntityManager.Turrets.Allies.Where(t => !t.IsDead && t.Health > 0 && t.CountAlliesInRange(t.GetAutoAttackRange()) > 1 && t.Distance(Player.Instance) <= 1000).Sum(turret => turret.Health);

            enemyteamTotal += EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => !m.IsDead && m.Health > 0 && m.IsValidTarget() && m.CountEnemiesInRange(700) > 1).Sum(minion => (minion.Health * 0.35f) + minion.Armor + minion.SpellBlock + minion.TotalMagicalDamage + minion.TotalAttackDamage);
            allyteamTotal += EntityManager.MinionsAndMonsters.AlliedMinions.Where(m => !m.IsDead && m.Health > 0 && m.IsValidTarget() && m.CountAlliesInRange(700) > 1).Sum(minion => (minion.Health * 0.35f) + minion.Armor + minion.SpellBlock + minion.TotalMagicalDamage + minion.TotalAttackDamage);
            
            enemyteamTotal +=
                EntityManager.Heroes.Enemies.Where(e => !e.IsDead && e.IsValidTarget() && e.IsInRange(Position, SafeValue))
                    .Sum(enemy => enemy.Health + enemy.Mana + enemy.Armor + enemy.SpellBlock + enemy.TotalMagicalDamage + enemy.TotalAttackDamage);
            allyteamTotal +=
                EntityManager.Heroes.Allies.Where(e => !e.IsDead && e.IsValidTarget() && e.IsInRange(Position, SafeValue))
                    .Sum(ally => ally.Health + ally.Mana + ally.Armor + ally.SpellBlock + ally.TotalMagicalDamage + ally.TotalAttackDamage);
            
            enemyteamTotal += TeamDamage(Position, true);
            allyteamTotal += TeamDamage(Position);
            
            return Enemy ? enemyteamTotal : allyteamTotal;
        }

        /// <summary>
        ///     Cache the Damages to prevent lags.
        /// </summary>
        private static float EnemyTeamDamage;
        private static float AllyTeamDamage;

        /// <summary>
        ///     Last Update Done To The Damages.
        /// </summary>
        private static int lastupdate;

        /// <summary>
        ///     Returns Spells Damage for the Whole Team.
        /// </summary>
        public static float TeamDamage(Vector3 Position, bool Enemy = false)
        {
            if (Core.GameTickCount - lastupdate > 1500)
            {
                EnemyTeamDamage = 0;
                AllyTeamDamage = 0;
                var spelllist = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };

                foreach (var slot in spelllist)
                {
                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => !e.IsDead && e.IsValidTarget() && e.IsInRange(Position, SafeValue)
                    && e.Spellbook.GetSpell(slot).IsLearned && e.Spellbook.GetSpell(slot).SData.Mana < e.Mana))
                    {
                        EnemyTeamDamage += enemy.GetSpellDamage(Player.Instance, slot);
                    }
                    foreach (var ally in EntityManager.Heroes.Allies.Where(e => !e.IsDead && e.IsValidTarget() && e.IsInRange(Position, SafeValue)
                    && e.Spellbook.GetSpell(slot).IsLearned && e.Spellbook.GetSpell(slot).SData.Mana < e.Mana))
                    {
                        AllyTeamDamage += ally.GetSpellDamage(Player.Instance, slot);
                    }
                }
                lastupdate = Core.GameTickCount;
            }

            return Enemy ? EnemyTeamDamage : AllyTeamDamage;
        }

        /// <summary>
        ///     Returns true if it's safe to dive.
        /// </summary>
        public static bool SafeToDive
        {
            get
            {
                return ObjectsManager.EnemyTurret != null && Player.Instance.HealthPercent > 10 && Core.GameTickCount - Brain.LastTurretAttack > 3000
                       && (ObjectsManager.EnemyTurret.CountMinions() > 2 || ObjectsManager.EnemyTurret.CountAlliesInRange(825) > 1 || ObjectsManager.EnemyTurret.IsAttackPlayer() && Core.GameTickCount - ObjectsManager.EnemyTurret.LastPlayerAttack() < 1000);
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
        ///     Returns true if Vector2 is UnderEnemyTurret.
        /// </summary>
        public static bool UnderEnemyTurret(this Vector2 pos)
        {
            return EntityManager.Turrets.Enemies.Any(t => !t.IsDead && t.IsValidTarget() && t.IsInRange(pos, t.GetAutoAttackRange() + (Player.Instance.BoundingRadius * 2)));
        }

        /// <summary>
        ///     Returns true if Vector3 is UnderAlliedTurret.
        /// </summary>
        public static bool UnderAlliedTurret(this Vector3 pos)
        {
            return EntityManager.Turrets.Allies.Any(t => !t.IsDead && t.IsInRange(pos, t.GetAutoAttackRange() + (Player.Instance.BoundingRadius * 2)));
        }

        /// <summary>
        ///     Returns true if Vector2 is UnderAlliedTurret.
        /// </summary>
        public static bool UnderAlliedTurret(this Vector2 pos)
        {
            return EntityManager.Turrets.Allies.Any(t => !t.IsDead && t.IsInRange(pos, t.GetAutoAttackRange() + (Player.Instance.BoundingRadius * 2)));
        }

        /// <summary>
        ///     Returns true if your team is teamfighting.
        /// </summary>
        public static bool TeamFight
        {
            get
            {
                return EntityManager.Heroes.Allies.Count(a => a.IsAttackPlayer() && a.CountAlliesInRange(1250) > 1 && a.IsValidTarget()) >= 2;
            }
        }

        /// <summary>
        ///     Returns true if you can deal damage to the target and kill him.
        /// </summary>
        public static bool IsKillable(this Obj_AI_Base target, float range)
        {
            return !target.HasBuff("kindredrnodeathbuff") && !target.Buffs.Any(b => b.Name.ToLower().Contains("fioraw")) && !target.HasBuff("JudicatorIntervention") && !target.IsZombie
                   && !target.HasBuff("ChronoShift") && !target.HasBuff("UndyingRage") && !target.IsInvulnerable && !target.IsZombie && !target.HasBuff("bansheesveil") && !target.IsDead
                   && !target.IsPhysicalImmune && target.Health > 0 && !target.HasBuffOfType(BuffType.Invulnerability) && !target.HasBuffOfType(BuffType.PhysicalImmunity) && target.IsValidTarget(range);
        }

        /// <summary>
        ///     Returns true if you can deal damage to the target and kill him.
        /// </summary>
        public static bool IsKillable(this Obj_AI_Base target)
        {
            return !target.HasBuff("kindredrnodeathbuff") && !target.Buffs.Any(b => b.Name.ToLower().Contains("fioraw")) && !target.HasBuff("JudicatorIntervention") && !target.IsZombie
                   && !target.HasBuff("ChronoShift") && !target.HasBuff("UndyingRage") && !target.IsInvulnerable && !target.IsZombie && !target.HasBuff("bansheesveil") && !target.IsDead
                   && !target.IsPhysicalImmune && target.Health > 0 && !target.HasBuffOfType(BuffType.Invulnerability) && !target.HasBuffOfType(BuffType.PhysicalImmunity) && target.IsValidTarget();
        }

        /// <summary>
        ///     Casts spell with selected hitchance.
        /// </summary>
        public static void Cast(this Spell.Skillshot spell, Obj_AI_Base target, HitChance hitChance)
        {
            if (target != null && spell.IsReady() && target.IsKillable(spell.Range))
            {
                var pred = spell.GetPrediction(target);
                if (pred.HitChance >= hitChance || target.IsCC())
                {
                    spell.Cast(pred.CastPosition);
                }
            }
        }

        /// <summary>
        ///     Distance To Keep from an Object and still be able to attack.
        /// </summary>
        public static float KiteDistance(GameObject target)
        {
            var extra = 0f;
            if (!Player.Instance.IsMelee)
                extra = target.BoundingRadius;

            return (Player.Instance.GetAutoAttackRange() * (Player.Instance.IsMelee ? 0.2f : 0.65f)) + extra;
        }
        
        public static bool Added(this AIHeroClient target)
        {
            var read = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\temp\\temp123.dat");
            return read.Any(a => a.Equals(target.NetworkId.ToString()));
        }

        public static void Add(this AIHeroClient target)
        {
            using (var stream = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EloBuddy\\AramBuddy\\temp\\temp123.dat", true))
            {
                stream.WriteLine(target.NetworkId);
                stream.Close();
            }
        }

        public static List<string> NoManaHeros = new List<string>
        {
            "Akali", "Gnar", "Katarina", "Kennen", "Kled", "LeeSin", "Mordekaiser", "RekSai", "Renekton", "Rengar", "Riven", "Rumble", "Shen", "Shyvana", "Tryndamere", "Vladimir", "Yasuo"
        };

        public static bool IsNoManaHero(this AIHeroClient target)
        {
            return NoManaHeros.Contains(target.BaseSkinName.Trim());
        }

        /// <summary>
        ///     Casts AoE spell with selected hitchance.
        /// </summary>
        public static void CastLineAoE(this Spell.Skillshot spell, Obj_AI_Base target, HitChance hitChance, int hits = 2)
        {
            if (target != null && spell.IsReady() && target.IsKillable(spell.Range))
            {
                var pred = spell.GetPrediction(target);
                var rect = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition, Player.Instance.ServerPosition.Extend(pred.CastPosition, spell.Range).To3D(), spell.Width);
                if (EntityManager.Heroes.Enemies.Count(e => e != null && e.IsKillable(spell.Range) && spell.GetPrediction(e).HitChance >= hitChance && rect.IsInside(spell.GetPrediction(e).CastPosition)) >= hits)
                {
                    spell.Cast(pred.CastPosition);
                }
            }
        }

        /// <summary>
        ///     Creates a checkbox.
        /// </summary>
        public static CheckBox CreateCheckBox(this Menu m, string id, string name, bool defaultvalue = true)
        {
            return m.Add(id, new CheckBox(name, defaultvalue));
        }

        /// <summary>
        ///     Creates a checkbox.
        /// </summary>
        public static CheckBox CreateCheckBox(this Menu m, SpellSlot slot, string name, bool defaultvalue = true)
        {
            return m.Add(slot.ToString(), new CheckBox(name, defaultvalue));
        }

        /// <summary>
        ///     Creates a slider.
        /// </summary>
        public static Slider CreateSlider(this Menu m, string id, string name, int defaultvalue = 0, int MinValue = 0, int MaxValue = 100)
        {
            return m.Add(id, new Slider(name, defaultvalue, MinValue, MaxValue));
        }

        /// <summary>
        ///     Returns CheckBox Value.
        /// </summary>
        public static bool CheckBoxValue(this Menu m, string id)
        {
            return m[id].Cast<CheckBox>().CurrentValue;
        }

        /// <summary>
        ///     Returns CheckBox Value.
        /// </summary>
        public static bool CheckBoxValue(this Menu m, SpellSlot slot)
        {
            return m[slot.ToString()].Cast<CheckBox>().CurrentValue;
        }

        /// <summary>
        ///     Returns Slider Value.
        /// </summary>
        public static int SliderValue(this Menu m, string id)
        {
            return m[id].Cast<Slider>().CurrentValue;
        }

        /// <summary>
        ///     Returns true if the value is >= the slider.
        /// </summary>
        public static bool CompareSlider(this Menu m, string id, float value)
        {
            return value >= m[id].Cast<Slider>().CurrentValue;
        }

        /// <summary>
        ///     Returns true if the spell will kill the target.
        /// </summary>
        public static bool WillKill(this Spell.SpellBase spell, Obj_AI_Base target, int MultiplyDmgBy = 1)
        {
            return Player.Instance.GetSpellDamage(target, spell.Slot) * MultiplyDmgBy >= target.Health;
        }

        /// <summary>
        ///     Class for getting if the figths info.
        /// </summary>
        public class LastAttack
        {
            public Obj_AI_Base Attacker;
            public Obj_AI_Base Target;
            public float LastAttackSent;

            public LastAttack(Obj_AI_Base from, Obj_AI_Base target)
            {
                this.Attacker = from;
                this.Target = target;
                this.LastAttackSent = 0f;
            }
        }

        /// <summary>
        ///     Returns true if the Item IsReady.
        /// </summary>
        public static bool ItemReady(this Item item, Menu menu)
        {
            return item != null && item.IsOwned(Player.Instance) && item.IsReady() && menu.CheckBoxValue(item.Id.ToString());
        }

        /// <summary>
        ///     Returns True if the target is attacking a player.
        /// </summary>
        public static bool IsAttackPlayer(this Obj_AI_Base target)
        {
            return AutoAttacks.FirstOrDefault(a => a.Attacker.NetworkId.Equals(target.NetworkId) && 300 + (a.Attacker.AttackCastDelay * 1000) + (a.Attacker.AttackDelay * 1000) > Core.GameTickCount - a.LastAttackSent) != null;
        }

        /// <summary>
        ///     Returns the last GameTickCount for the Attack.
        /// </summary>
        public static float? LastPlayerAttack(this Obj_AI_Base target)
        {
            return AutoAttacks.FirstOrDefault(a => a.Attacker.NetworkId.Equals(target.NetworkId) && 300 + (a.Attacker.AttackCastDelay * 1000) + (a.Attacker.AttackDelay * 1000) > Core.GameTickCount - a.LastAttackSent)?.LastAttackSent;
        }

        /// <summary>
        ///     Save all Attacks into list.
        /// </summary>
        public static List<LastAttack> AutoAttacks = new List<LastAttack>();

        /// <summary>
        ///     Returns The predicted position for the target.
        /// </summary>
        public static Vector3 PredictPosition(this Obj_AI_Base target)
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
