using System;
using System.Linq;
using AramBuddy.KappaEvade;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Events;

namespace AramBuddy
{
    /// <summary>
    ///     A class containing all the globally used events in AutoBuddy
    /// </summary>
    internal static class Events
    {
        /// <summary>
        ///     A handler for the OnGameEnd event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnGameEndHandler(EventArgs args);

        /// <summary>
        ///     A handler for the OnGameStart event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnGameStartHandler(EventArgs args);

        /// <summary>
        ///     A handler for the InComingDamage event
        /// </summary>
        /// <param name="args">The arguments the event provides</param>
        public delegate void OnInComingDamage(InComingDamageEventArgs args);

        public class InComingDamageEventArgs
        {
            public Obj_AI_Base Sender;
            public AIHeroClient Target;
            public float InComingDamage;
            public Type DamageType;
            public enum Type
            {
                TurretAttack, HeroAttack, MinionAttack, SkillShot, TargetedSpell
            }
            public InComingDamageEventArgs(Obj_AI_Base sender, AIHeroClient target, float Damage, Type type)
            {
                Sender = sender;
                Target = target;
                InComingDamage = Damage;
                DamageType = type;
            }
        }

        static Events()
        {
            // Invoke the OnGameEnd event
            #region OnGameEnd

            // Variable used to make sure that the event invoke isn't spammed and is only called once
            var gameEndNotified = false;

            // Every time the game ticks (1ms)
            Game.OnTick += delegate
                {
                    // Make sure we're not repeating the invoke
                    if (gameEndNotified)
                    {
                        return;
                    }

                    // Get the enemy nexus
                    var nexus = ObjectManager.Get<Obj_HQ>();

                    // Check and return if the nexus is null
                    if (nexus == null)
                    {
                        return;
                    }

                    // If the nexus is dead or its health is equal to 0
                    if (nexus.Any(n => n.IsDead || n.Health.Equals(0)))
                    {
                        // Invoke the event
                        OnGameEnd(EventArgs.Empty);

                        // Set gameEndNotified to true, as the event has been completed
                        gameEndNotified = true;

                        Logger.Send("Game ended!", Logger.LogLevel.Info);
                    }
                };

            Game.OnUpdate += delegate
                {
                    // Used to Invoke the Incoming Damage Event When there is SkillShot Incoming
                    foreach (var spell in Collision.NewSpells)
                    {
                        foreach (var ally in EntityManager.Heroes.Allies.Where(a => !a.IsDead && a.IsValidTarget() && a.IsInDanger(spell)))
                        {
                            OnIncomingDamage?.Invoke(new InComingDamageEventArgs(spell.Caster, ally, spell.Caster.GetSpellDamage(ally, spell.spell.slot), InComingDamageEventArgs.Type.SkillShot));
                        }
                    }
                };

            SpellsDetector.OnTargetedSpellDetected += delegate(AIHeroClient sender, AIHeroClient target, GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spell)
            {
                // Used to Invoke the Incoming Damage Event When there is a TargetedSpell Incoming
                if (target.IsAlly)
                        OnIncomingDamage?.Invoke(new InComingDamageEventArgs(sender, target, sender.GetSpellDamage(target, spell.slot), InComingDamageEventArgs.Type.TargetedSpell));
                };

            Obj_AI_Base.OnBasicAttack += delegate(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                // Used to Invoke the Incoming Damage Event When there is an AutoAttack Incoming
                    var target = args.Target as AIHeroClient;
                    var hero = sender as AIHeroClient;
                    var turret = sender as Obj_AI_Turret;
                    var minion = sender as Obj_AI_Minion;

                    if(target == null || !target.IsAlly) return;

                    if(hero != null)
                        OnIncomingDamage?.Invoke(new InComingDamageEventArgs(hero, target, hero.GetAutoAttackDamage(target), InComingDamageEventArgs.Type.HeroAttack));
                    if (turret != null)
                        OnIncomingDamage?.Invoke(new InComingDamageEventArgs(turret, target, turret.GetAutoAttackDamage(target), InComingDamageEventArgs.Type.TurretAttack));
                    if (minion != null)
                        OnIncomingDamage?.Invoke(new InComingDamageEventArgs(minion, target, minion.GetAutoAttackDamage(target), InComingDamageEventArgs.Type.MinionAttack));
                };
            Obj_AI_Base.OnProcessSpellCast += delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                var caster = sender as AIHeroClient;
                var target = args.Target as AIHeroClient;
                if (caster == null || target == null || !caster.IsEnemy || !target.IsAlly || args.IsAutoAttack()) return;
                if (!Database.TargetedSpells.TargetedSpellsList.Any(s => s.hero == caster.Hero && s.slot == args.Slot))
                {
                    OnIncomingDamage?.Invoke(new InComingDamageEventArgs(caster, target, caster.GetSpellDamage(target, args.Slot), InComingDamageEventArgs.Type.TargetedSpell));
                }
            };
            #endregion

            // Invoke the OnGameStart event

            #region OnGameStart

            // When the player object is created
            Loading.OnLoadingComplete += delegate(EventArgs args)
                {
                    if (Player.Instance.IsInShopRange())
                    {
                        //OnGameStart(EventArgs.Empty);

                        Logger.Send("Game started!", Logger.LogLevel.Info);
                    }
                };

            #endregion
        }

        /// <summary>
        ///     Fires when the game has ended
        /// </summary>
        public static event OnGameEndHandler OnGameEnd;

        /// <summary>
        /// Fires when the game has started
        /// </summary>
        public static event OnGameStartHandler OnGameStart;

        /// <summary>
        /// Fires when There is In Coming Damage to an ally
        /// </summary>
        public static event OnInComingDamage OnIncomingDamage;
    }
}
