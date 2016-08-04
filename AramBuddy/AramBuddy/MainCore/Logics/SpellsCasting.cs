using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using GenesisSpellLibrary;

namespace AramBuddy.MainCore.Logics
{
    class SpellsCasting
    {
        /// <summary>
        ///     Casting Logic.
        /// </summary>
        public static void Casting(Spell.SpellBase spellBase, Obj_AI_Base target, bool enabled = true)
        {
            if (spellBase == null || target == null || !enabled)
                return;

            if(spellBase.DontWaste() && ModesManager.LaneClear) return;

            if (spellBase.IsDangerDash() && target.CountAlliesInRange(1000) >= target.CountEnemiesInRange(1000)
                && (target.PrediectPosition().UnderEnemyTurret() && Misc.SafeToDive || !target.PrediectPosition().UnderEnemyTurret()))
            {
                spellBase.Cast(target);
                return;
            }

            if (spellBase.IsDangerDash())
                return;

            if (spellBase.IsDash())
            {
                spellBase.Cast(target.PrediectPosition().Extend(Player.Instance, 200).To3D());
                return;
            }

            if (spellBase.IsToggle())
            {
                if (spellBase is Spell.Active)
                {
                    if (spellBase.Handle.ToggleState != 2 && target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast();
                    }
                    if (spellBase.Handle.ToggleState == 2 && !target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast();
                    }
                }
                else
                {
                    if (spellBase.Handle.ToggleState != 2 && target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast(target);
                    }
                    if (spellBase.Handle.ToggleState == 2 && !target.IsValidTarget(spellBase.Range))
                    {
                        spellBase.Cast(Game.CursorPos);
                    }
                }
            }

            if (spellBase is Spell.Active)
            {
                spellBase.Cast();
                return;
            }

            if ((spellBase is Spell.Skillshot || spellBase is Spell.Targeted || spellBase is Spell.Ranged) && !(spellBase is Spell.Chargeable))
            {
                spellBase.Cast(target);
                return;
            }

            if (spellBase is Spell.Chargeable)
            {
                var chargeable = spellBase as Spell.Chargeable;

                if (!chargeable.IsCharging)
                {
                    chargeable.StartCharging();
                    return;
                }
                if (chargeable.IsInRange(target))
                {
                    chargeable.Cast(target);
                }
            }
        }

        /// <summary>
        ///     Anti-Gapcloser Logic.
        /// </summary>
        public static void GapcloserOnOnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (sender == null || !sender.IsEnemy)
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s.IsCC() && s.IsReady() && s.IsInRange(args.End)))
            {
                Casting(spell, sender);
            }
        }

        /// <summary>
        ///     Interrupter Logic.
        /// </summary>
        public static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s.IsCC() && s.IsReady() && s.IsInRange(sender)))
            {
                Casting(spell, sender);
            }
        }

        /// <summary>
        ///     Obj_AI_Base_OnProcessSpellCast event, used to detect incoming spells.
        /// </summary>
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(args.Target is AIHeroClient) || !sender.IsEnemy)
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsSaver() && s.IsReady()))
            {
                var caster = sender;
                var enemy = sender as AIHeroClient;
                var target = (AIHeroClient)args.Target;
                var hit = EntityManager.Heroes.Allies.FirstOrDefault(a => a.IsInRange(args.End, 100) && a.IsValidTarget(spell.Range));

                if (!(caster is AIHeroClient || caster is Obj_AI_Turret) || !caster.IsEnemy || enemy == null)
                {
                    return;
                }

                if (hit != null)
                {
                    var spelldamage = enemy.GetSpellDamage(hit, args.Slot);
                    var damagepercent = (spelldamage / hit.Health) * 100;
                    var death = damagepercent >= hit.HealthPercent || spelldamage >= hit.Health || caster.GetAutoAttackDamage(hit, true) >= hit.Health;

                    if (death || damagepercent >= 40)
                    {
                        Casting(spell, hit);
                    }
                }

                if (target != null && target.IsValidTarget(spell.Range) && target.IsAlly)
                {
                    var spelldamage = enemy.GetSpellDamage(target, args.Slot);
                    var damagepercent = (spelldamage / target.Health) * 100;
                    var death = damagepercent >= target.HealthPercent || spelldamage >= target.Health || caster.GetAutoAttackDamage(target, true) >= target.Health;

                    if (death || damagepercent >= 10)
                    {
                        Casting(spell, target);
                    }
                }
            }
        }

        /// <summary>
        ///     Obj_AI_Base_OnBasicAttack event, used to detect incoming autoattacks.
        /// </summary>
        public static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(args.Target is AIHeroClient))
            {
                return;
            }

            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsSaver() && s.IsReady()))
            {
                var caster = sender;
                var target = (AIHeroClient)args.Target;

                if (!(caster is AIHeroClient || caster is Obj_AI_Turret) || !caster.IsEnemy || target == null || !target.IsAlly)
                {
                    return;
                }

                var aaprecent = (caster.GetAutoAttackDamage(target, true) / target.Health) * 100;
                var death = caster.GetAutoAttackDamage(target, true) >= target.Health || aaprecent >= target.HealthPercent;

                if ((death || aaprecent >= 10) && target.IsValidTarget(spell.Range))
                {
                    Casting(spell, target);
                }
            }
        }
    }
}
