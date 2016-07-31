using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Spells;
using GenesisSpellLibrary;
using GenesisSpellLibrary.Spells;

namespace AramBuddy.MainCore.Logics
{
    internal class ModesManager
    {
        protected static SpellBase Spell => SpellManager.CurrentSpells;

        public static List<Spell.SpellBase> Spelllist = new List<Spell.SpellBase> { Spell.Q, Spell.W, Spell.E, Spell.R };

        public static void OnTick()
        {
            Orbwalker.DisableAttacking = Flee || None;

            if (!Program.MenuIni["DisableSpells"].Cast<CheckBox>().CurrentValue)
            {
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
            foreach (var spell in Spelllist.Where(s => s != null && s.IsReady() && !s.IsSaver()))
            {
                if (Combo || (Harass && (Player.Instance.ManaPercent > 60 || Player.Instance.ManaPercent.Equals(0))))
                {
                    SpellsCasting.Casting(spell, TargetSelector.GetTarget(spell.Range, DamageType.Mixed), Program.SpellsMenu[spell.Slot.ToString() + Orbwalker.ActiveModesFlags + Player.Instance.Hero].Cast<CheckBox>().CurrentValue);
                }
                if (spell.Slot != SpellSlot.R)
                {
                    if (LaneClear)
                    {
                        var spell1 = spell;
                        foreach (
                            var minion in
                                EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsValidTarget(spell1.Range) && (Player.Instance.ManaPercent > 60 || Player.Instance.ManaPercent.Equals(0))))
                        {
                            SpellsCasting.Casting(spell, minion, Program.SpellsMenu[spell.Slot.ToString() + Orbwalker.ActiveModesFlags + Player.Instance.Hero].Cast<CheckBox>().CurrentValue);
                        }
                    }
                }
                if (Flee && spell.IsCC() && spell.IsReady())
                {
                    SpellsCasting.Casting(spell, TargetSelector.GetTarget(spell.Range, DamageType.Mixed));
                }
            }

            if (Player.Instance.CountEnemiesInRange(750) > 0 && Player.Instance.HealthPercent <= 15)
            {
                if (SummonerSpells.Heal.IsReady() && Program.SpellsMenu["Heal"].Cast<CheckBox>().CurrentValue && SummonerSpells.Heal.Slot != SpellSlot.Unknown)
                {
                    SummonerSpells.Heal.Cast();
                }
                else
                {
                    if (SummonerSpells.Barrier.IsReady() && Program.SpellsMenu["Barrier"].Cast<CheckBox>().CurrentValue && SummonerSpells.Barrier.Slot != SpellSlot.Unknown)
                    {
                        SummonerSpells.Barrier.Cast();
                    }
                }
            }

            if (Player.Instance.ManaPercent < 50 && SummonerSpells.Clarity.IsReady() && Program.SpellsMenu["Clarity"].Cast<CheckBox>().CurrentValue && SummonerSpells.Clarity.Slot != SpellSlot.Unknown)
            {
                SummonerSpells.Clarity.Cast();
            }

            if (Flee)
            {
                if (SummonerSpells.Ghost.IsReady() && Program.SpellsMenu["Ghost"].Cast<CheckBox>().CurrentValue && SummonerSpells.Ghost.Slot != SpellSlot.Unknown && Player.Instance.CountEnemiesInRange(800) > 0)
                {
                    SummonerSpells.Ghost.Cast();
                }
                if (SummonerSpells.Flash.IsReady() && Program.SpellsMenu["Flash"].Cast<CheckBox>().CurrentValue && SummonerSpells.Flash.Slot != SpellSlot.Unknown && Player.Instance.HealthPercent < 20 && ObjectsManager.AllySpawn != null)
                {
                    SummonerSpells.Flash.Cast(Player.Instance.ServerPosition.Extend(ObjectsManager.AllySpawn, SummonerSpells.Flash.Range).To3D());
                }
            }
            if (SummonerSpells.Cleanse.IsReady() && Program.SpellsMenu["Cleanse"].Cast<CheckBox>().CurrentValue && SummonerSpells.Cleanse.Slot != SpellSlot.Unknown && Player.Instance.IsCC() && Player.Instance.CountEnemiesInRange(1250) > 0 && Player.Instance.HealthPercent <= 80)
            {
                SummonerSpells.Cleanse.Cast();
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
                       && Player.Instance.CountAlliesInRange(1000) >= Player.Instance.CountEnemiesInRange(1000) && Player.Instance.CountEnemiesInRange(1000) > 0
                       && ((Player.Instance.ServerPosition.UnderEnemyTurret() && Misc.SafeToDive) || !Player.Instance.IsUnderEnemyturret());
            }
        }

        /// <summary>
        ///     Returns True if Harass needs to be active.
        /// </summary>
        public static bool Harass
        {
            get
            {
                return (Misc.TeamTotal(Player.Instance.ServerPosition) < Misc.TeamTotal(Player.Instance.ServerPosition, true) || Player.Instance.IsUnderHisturret())
                       && Player.Instance.CountEnemiesInRange(800) > 0 && ((Player.Instance.ServerPosition.UnderEnemyTurret() && Misc.SafeToDive) || !Player.Instance.IsUnderEnemyturret()) && !Flee;
            }
        }

        /// <summary>
        ///     Returns True if LaneClear needs to be active.
        /// </summary>
        public static bool LaneClear
        {
            get
            {
                return Player.Instance.CountEnemiesInRange(1000) < 1 && !Flee && (Player.Instance.CountAlliesInRange(800) > 1 || Player.Instance.CountMinions() > 0)
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
                       && ((Misc.TeamTotal(Player.Instance.ServerPosition) < Misc.TeamTotal(Player.Instance.ServerPosition, true) && Player.Instance.CountAlliesInRange(800) < 2)
                           || (Player.Instance.IsUnderEnemyturret() && !Misc.SafeToDive) || (Player.Instance.CountEnemiesInRange(800) > Player.Instance.CountAlliesInRange(800))
                           || (Player.Instance.HealthPercent < 15 && (Player.Instance.IsUnderEnemyturret() || Player.Instance.CountEnemiesInRange(1000) > 1)));
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
                return (ObjectsManager.EnemyNexues != null && ObjectsManager.EnemyNexues.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyNexues.BoundingRadius))
                       || (ObjectsManager.EnemyInhb != null && ObjectsManager.EnemyInhb.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + +ObjectsManager.EnemyInhb.BoundingRadius))
                       || (ObjectsManager.EnemyTurret != null && ObjectsManager.EnemyTurret.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyTurret.BoundingRadius));
            }
        }
    }
}
