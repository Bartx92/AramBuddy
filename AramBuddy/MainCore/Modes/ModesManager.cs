namespace AramBuddy.MainCore.Modes
{
    using System.Collections.Generic;
    using System.Linq;

    using AramBuddy.MainCore.Utility;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Spells;

    using GenesisSpellLibrary;
    using GenesisSpellLibrary.Spells;

    internal class ModesManager
    {
        protected static SpellBase Spell => SpellManager.CurrentSpells;

        public static List<Spell.SpellBase> Spelllist = new List<Spell.SpellBase>() { Spell.Q, Spell.W, Spell.E, Spell.R };

        public static void OnTick()
        {
            Orbwalker.DisableAttacking = Flee || None;

            if (!Program.MenuIni["DisableSpells"].Cast<CheckBox>().CurrentValue)
            {
                if (SummonerSpells.Heal.IsReady() && Player.Instance.HealthPercent < 15)
                {
                    SummonerSpells.Heal.Cast();
                }
                ModesBase();
            }
            
            // Activate Flee mode
            if (Flee)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Flee;
                return;
            }

            // Activate LaneClear mode
            if (LaneClear)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                return;
            }

            // Activate Harass mode
            if (Harass)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Harass;
                return;
            }

            // Activate Combo mode
            if (Combo)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo;
                return;
            }

            // Activate None mode
            if (None)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
            }
        }

        /// <summary>
        ///     Casts Spells On Target.
        /// </summary>
        public static void ModesBase()
        {
            foreach (var spell in Spelllist.Where(s => s.IsReady()))
            {
                if (Combo || (Harass && Player.Instance.ManaPercent > 60))
                {
                    Casting(spell, TargetSelector.GetTarget(spell.Range, DamageType.Mixed));
                }
                if (spell.Slot != SpellSlot.R)
                {
                    if (LaneClear)
                    {
                        var spell1 = spell;
                        foreach (
                            var minion in
                                EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                                    m => m.IsValidTarget(spell1.Range) && Player.Instance.ManaPercent > 60))
                        {
                            Casting(spell, minion);
                        }
                    }
                }
            }

            if (Flee)
            {
                if (Spell.Q.IsReady() && Spell.Q.IsCC())
                {
                    Casting(Spell.Q, TargetSelector.GetTarget(Spell.Q.Range, DamageType.Mixed));
                }
                if (Spell.W.IsReady() && Spell.W.IsCC())
                {
                    Casting(Spell.W, TargetSelector.GetTarget(Spell.W.Range, DamageType.Mixed));
                }
                if (Spell.E.IsReady() && Spell.E.IsCC())
                {
                    Casting(Spell.E, TargetSelector.GetTarget(Spell.E.Range, DamageType.Mixed));
                }
                if (Spell.R.IsReady() && Spell.R.IsCC())
                {
                    Casting(Spell.R, TargetSelector.GetTarget(Spell.R.Range, DamageType.Mixed));
                }

                if (SummonerSpells.Ghost.IsReady())
                {
                    SummonerSpells.Ghost.Cast();
                }
                if (SummonerSpells.Flash.IsReady() && Player.Instance.HealthPercent < 20 && ObjectsManager.AllyNexues != null)
                {
                    SummonerSpells.Flash.Cast(Player.Instance.ServerPosition.Extend(ObjectsManager.AllyNexues, SummonerSpells.Flash.Range).To3D());
                }
            }
        }

        /// <summary>
        ///     Casting Logic.
        /// </summary>
        public static void Casting(Spell.SpellBase spellBase, Obj_AI_Base target)
        {
            if(spellBase == null || target == null) return;

            if (spellBase.IsDash())
            {
                spellBase.Cast(target.ServerPosition.Extend(Player.Instance, 200).To3D());
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
                var Chargeable = spellBase as Spell.Chargeable;

                if (!Chargeable.IsCharging)
                {
                    Chargeable.StartCharging();
                    return;
                }
                if (Chargeable.IsInRange(target))
                {
                    Chargeable.Cast(target);
                }
            }
        }

        /// <summary>
        ///     Returns True if combo needs to be active.
        /// </summary>
        public static bool Combo
        {
            get
            {
                return Misc.TeamTotal(Player.Instance.ServerPosition) > Misc.TeamTotal(Player.Instance.ServerPosition, true)
                       && Player.Instance.CountAlliesInRange(800) >= Player.Instance.CountEnemiesInRange(800)
                       && Player.Instance.CountEnemiesInRange(800) > 0 && ((Player.Instance.ServerPosition.UnderEnemyTurret() && Misc.SafeToDive) || !Player.Instance.IsUnderEnemyturret());
            }
        }

        /// <summary>
        ///     Returns True if Harass needs to be active.
        /// </summary>
        public static bool Harass
        {
            get
            {
                return (Misc.TeamTotal(Player.Instance.ServerPosition) < Misc.TeamTotal(Player.Instance.ServerPosition, true)
                        || Player.Instance.IsUnderHisturret()) && Player.Instance.CountEnemiesInRange(800) > 0
                       && ((Player.Instance.ServerPosition.UnderEnemyTurret() && Misc.SafeToDive) || !Player.Instance.IsUnderEnemyturret()) && !Flee;
            }
        }

        /// <summary>
        ///     Returns True if LaneClear needs to be active.
        /// </summary>
        public static bool LaneClear
        {
            get
            {
                return Player.Instance.CountEnemiesInRange(1000) < 1 && !Flee
                       && (Player.Instance.CountAlliesInRange(800) > 1 || Player.Instance.CountMinions() > 0)
                       && (Player.Instance.CountMinions(true) > 0 || AttackObject);
            }
        }

        /// <summary>
        ///     Returns True if Flee needs to be active.
        /// </summary>
        public static bool Flee
        {
            get
            {
                return !Player.Instance.IsUnderHisturret()
                       && ((Misc.TeamTotal(Player.Instance.ServerPosition) < Misc.TeamTotal(Player.Instance.ServerPosition, true)
                            && Player.Instance.CountAlliesInRange(800) < 2) || (Player.Instance.IsUnderEnemyturret() && !Misc.SafeToDive)
                           || (Player.Instance.CountEnemiesInRange(800) > Player.Instance.CountAlliesInRange(800)) || (Player.Instance.HealthPercent < 15 && (Player.Instance.IsUnderEnemyturret() || Player.Instance.CountEnemiesInRange(1000) > 1)));
            }
        }

        /// <summary>
        ///     Returns True if No modes are active.
        /// </summary>
        public static bool None
        {
            get
            {
                return !Combo && !Harass && !LaneClear && !Flee;
            }
        }

        /// <summary>
        ///     Returns True if Can attack objects.
        /// </summary>
        public static bool AttackObject
        {
            get
            {
                return (ObjectsManager.EnemyNexues != null
                        && ObjectsManager.EnemyNexues.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyNexues.BoundingRadius))
                       || (ObjectsManager.EnemyInhb != null
                           && ObjectsManager.EnemyInhb.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + +ObjectsManager.EnemyInhb.BoundingRadius))
                       || (ObjectsManager.EnemyTurret != null
                           && ObjectsManager.EnemyTurret.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() +ObjectsManager.EnemyTurret.BoundingRadius));
            }
        }
    }
}