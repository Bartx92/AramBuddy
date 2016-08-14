using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Logics.Casting;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Spells;
using GenesisSpellLibrary;
using GenesisSpellLibrary.Spells;
using static AramBuddy.Config;

namespace AramBuddy.MainCore.Logics
{
    internal class ModesManager
    {
        /// <summary>
        ///     Modes enum.
        /// </summary>
        public enum Modes
        {
            Flee,
            LaneClear,
            Harass,
            Combo,
            None
        }

        /// <summary>
        ///     Bot current active mode.
        /// </summary>
        public static Modes CurrentMode;

        /// <summary>
        ///     Gets the spells from the database.
        /// </summary>
        protected static SpellBase Spell => SpellManager.CurrentSpells;

        /// <summary>
        ///     List contains my hero spells.
        /// </summary>
        public static List<Spell.SpellBase> Spelllist = new List<Spell.SpellBase> { Spell.Q, Spell.W, Spell.E, Spell.R };

        public static void OnTick()
        {
            UpdateSpells();

            Orbwalker.DisableAttacking = Flee || None;

            if (Flee)
            {
                CurrentMode = Modes.Flee;
            }
            else if (LaneClear)
            {
                CurrentMode = Modes.LaneClear;
            }
            else if (Harass)
            {
                CurrentMode = Modes.Harass;
            }
            else if (Combo)
            {
                CurrentMode = Modes.Combo;
            }
            else if (None)
            {
                CurrentMode = Modes.None;
            }

            // Modes Switch
            switch (CurrentMode)
            {
                case Modes.Flee:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Flee;
                    break;
                case Modes.LaneClear:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                    break;
                case Modes.Harass:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Harass;
                    break;
                case Modes.Combo:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo;
                    break;
                case Modes.None:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                    break;
                default:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                    break;
            }

            if (!DisableSpellsCasting && !Program.CustomChamp)
            {
                ModesBase();
            }
        }
        
        /// <summary>
        ///     Update Spell values that needs to be updated.
        /// </summary>
        public static void UpdateSpells()
        {
            if (Player.Instance.Hero == Champion.AurelionSol)
            {
                Spell.E.Range = (uint)(2000 + Spell.E.Level * 1000);
            }
            if (Player.Instance.Hero == Champion.TahmKench)
            {
                Spell.R.Range = (uint)(3500 + Spell.R.Level * 1000);
            }
            if (Player.Instance.Hero == Champion.Ryze)
            {
                Spell.R.Range = (uint)(1500 * Spell.R.Level);
            }
        }

        /// <summary>
        ///     Casts Spells.
        /// </summary>
        public static void ModesBase()
        {
            // Casting the summoner spells
            if (Player.Instance.CountEnemiesInRange(1250) > 0 && Player.Instance.HealthPercent <= 25 && (Combo || Harass || Flee))
            {
                if (SummonerSpells.Heal.IsReady() && Program.SpellsMenu["Heal"].Cast<CheckBox>().CurrentValue && SummonerSpells.Heal.Slot != SpellSlot.Unknown)
                {
                    Logger.Send("Cast Heal HealthPercent " + (int)Player.Instance.HealthPercent, Logger.LogLevel.Info);
                    SummonerSpells.Heal.Cast();
                }
                else
                {
                    if (SummonerSpells.Barrier.IsReady() && Program.SpellsMenu["Barrier"].Cast<CheckBox>().CurrentValue && SummonerSpells.Barrier.Slot != SpellSlot.Unknown)
                    {
                        Logger.Send("Cast Barrier HealthPercent " + (int)Player.Instance.HealthPercent, Logger.LogLevel.Info);
                        SummonerSpells.Barrier.Cast();
                    }
                }
            }

            if (Player.Instance.ManaPercent < 50 && SummonerSpells.Clarity.IsReady() && Program.SpellsMenu["Clarity"].Cast<CheckBox>().CurrentValue && SummonerSpells.Clarity.Slot != SpellSlot.Unknown)
            {
                Logger.Send("Cast Clarity ManaPercent " + (int)Player.Instance.ManaPercent, Logger.LogLevel.Info);
                SummonerSpells.Clarity.Cast();
            }

            if (Flee)
            {
                if (SummonerSpells.Ghost.IsReady() && Program.SpellsMenu["Ghost"].Cast<CheckBox>().CurrentValue && SummonerSpells.Ghost.Slot != SpellSlot.Unknown
                    && Player.Instance.CountEnemiesInRange(800) > 0)
                {
                    Logger.Send("Cast Ghost FleeMode CountEnemiesInRange " + Player.Instance.CountEnemiesInRange(800), Logger.LogLevel.Info);
                    SummonerSpells.Ghost.Cast();
                }
                if (SummonerSpells.Flash.IsReady() && Program.SpellsMenu["Flash"].Cast<CheckBox>().CurrentValue && SummonerSpells.Flash.Slot != SpellSlot.Unknown && Player.Instance.HealthPercent < 20
                    && ObjectsManager.AllySpawn != null)
                {
                    Logger.Send("Cast Flash FleeMode HealthPercent " + (int)Player.Instance.HealthPercent, Logger.LogLevel.Info);
                    SummonerSpells.Flash.Cast(Player.Instance.PredictPosition().Extend(ObjectsManager.AllySpawn, SummonerSpells.Flash.Range).To3D());
                }
            }
            if (SummonerSpells.Cleanse.IsReady() && Program.SpellsMenu["Cleanse"].Cast<CheckBox>().CurrentValue && SummonerSpells.Cleanse.Slot != SpellSlot.Unknown && Player.Instance.IsCC()
                && Player.Instance.CountEnemiesInRange(1250) > 0 && Player.Instance.HealthPercent <= 80)
            {
                Logger.Send("Cast Cleanse FleeMode Player CC'ed HealthPercent " + (int)Player.Instance.HealthPercent + " CountEnemiesInRange " + Player.Instance.CountEnemiesInRange(1250), Logger.LogLevel.Info);
                SummonerSpells.Cleanse.Cast();
            }

            foreach (var spell in Spelllist.Where(s => s != null && s.IsReady() && !s.IsSaver() && !s.IsTP()))
            {
                if (Combo || (Harass && (Player.Instance.ManaPercent > 60 || Player.Instance.ManaPercent.Equals(0))))
                {
                    SpellsCasting.Casting(spell, TargetSelector.GetTarget(spell.Range, DamageType.Mixed));
                }
                if (spell.Slot != SpellSlot.R)
                {
                    if (LaneClear)
                    {
                        var spell1 = spell;
                        foreach (var minion in
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsValidTarget(spell1.Range) && (Player.Instance.ManaPercent > 60 || Player.Instance.ManaPercent.Equals(0))))
                        {
                            SpellsCasting.Casting(spell, minion);
                        }
                    }
                }
                if (Flee && spell.IsCC() && spell.IsReady())
                {
                    SpellsCasting.Casting(spell, TargetSelector.GetTarget(spell.Range, DamageType.Mixed));
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
                return Misc.TeamTotal(Player.Instance.PredictPosition()) > Misc.TeamTotal(Player.Instance.PredictPosition(), true)
                       && Player.Instance.CountAlliesInRange(1000) >= Player.Instance.CountEnemiesInRange(1000) && Player.Instance.CountEnemiesInRange(1000) > 0
                       && ((Player.Instance.PredictPosition().UnderEnemyTurret() && Misc.SafeToDive) || !Player.Instance.IsUnderEnemyturret());
            }
        }

        /// <summary>
        ///     Returns True if Harass needs to be active.
        /// </summary>
        public static bool Harass
        {
            get
            {
                return (Misc.TeamTotal(Player.Instance.PredictPosition()) < Misc.TeamTotal(Player.Instance.PredictPosition(), true) || Player.Instance.IsUnderHisturret())
                       && Player.Instance.CountEnemiesInRange(800) > 0 && ((Player.Instance.PredictPosition().UnderEnemyTurret() && Misc.SafeToDive) || !Player.Instance.IsUnderEnemyturret()) && !Flee;
            }
        }

        /// <summary>
        ///     Returns True if LaneClear needs to be active.
        /// </summary>
        public static bool LaneClear
        {
            get
            {
                return Player.Instance.CountEnemiesInRange(1000) <= 1 && !Combo && !Flee && (Player.Instance.CountAlliesInRange(800) > 1 || Player.Instance.CountMinions() > 0)
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
                       && ((Misc.TeamTotal(Player.Instance.PredictPosition()) < Misc.TeamTotal(Player.Instance.PredictPosition(), true) && Player.Instance.CountAlliesInRange(800) < 2)
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
                return (ObjectsManager.EnemyNexues != null && ObjectsManager.EnemyNexues.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyNexues.BoundingRadius * 3))
                       || (ObjectsManager.EnemyInhb != null && ObjectsManager.EnemyInhb.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyInhb.BoundingRadius * 3))
                       || (ObjectsManager.EnemyTurret != null && ObjectsManager.EnemyTurret.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyTurret.BoundingRadius * 3));
            }
        }
    }
}
