using System;
using System.Linq;
using AramBuddy.GenesisSpellDatabase;
using AramBuddy.MainCore.Logics;
using AramBuddy.MainCore.Logics.Casting;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using GenesisSpellLibrary;
using SharpDX;

namespace AramBuddy.MainCore
{
    internal class Brain
    {
        /// <summary>
        ///     Init bot functions.
        /// </summary>
        public static void Init()
        {
            try
            {
                // Initialize Genesis Spell Library.
                SpellManager.Initialize();
                SpellLibrary.Initialize();

                // Initialize ObjectsManager.
                ObjectsManager.Init();

                SpecialChamps.Init();

                // Overrides Orbwalker Movements
                Orbwalker.OverrideOrbwalkPosition += OverrideOrbwalkPosition;

                // Initialize AutoLvlup.
                LvlupSpells.Init();

                // Initialize TeamFights Detector.
                TeamFightsDetection.Init();

                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Gapcloser.OnGapcloser += SpellsCasting.GapcloserOnOnGapcloser;
                Interrupter.OnInterruptableSpell += SpellsCasting.Interrupter_OnInterruptableSpell;
                Obj_AI_Base.OnBasicAttack += SpellsCasting.Obj_AI_Base_OnBasicAttack;
                Obj_AI_Base.OnProcessSpellCast += SpellsCasting.Obj_AI_Base_OnProcessSpellCast;
            }
            catch (Exception ex)
            {
                Logger.Send("There was an Error While Initialize Brain", ex, Logger.LogLevel.Error);
            }
        }

        /// <summary>
        ///     Returns LastUpdate for the bot current postion.
        /// </summary>
        public static float LastUpdate;

        /// <summary>
        ///     Decisions picking for the bot.
        /// </summary>
        public static void Decisions()
        {
            // Picks best position for the bot.
            if (Core.GameTickCount - LastUpdate > 75)
            {
                foreach (var hero in EntityManager.Heroes.AllHeroes.Where(a => a != null && a.IsValidTarget() && !a.Added() && ObjectsManager.HealthRelics.Any(hr => a.Path.LastOrDefault().Distance(hr.Position) <= 1)))
                {
                    hero.Add();
                    Logger.Send("Added: " + hero.BaseSkinName + " - " + hero.NetworkId, Logger.LogLevel.Warn);
                }

                Pathing.BestPosition();
                LastUpdate = Core.GameTickCount;
            }

            // Ticks for the modes manager.
            ModesManager.OnTick();

            // Moves to the Bot selected Position.
            if (Pathing.Position != Vector3.Zero && Pathing.Position.IsValid() && !Pathing.Position.IsZero)
            {
                Pathing.MoveTo(Pathing.Position);
            }
        }

        /// <summary>
        ///     Bool returns true if the bot is alone.
        /// </summary>
        public static bool Alone()
        {
            return Player.Instance.CountAlliesInRange(4500) < 2 || Player.Instance.Path.Any(p => p.IsInRange(Game.CursorPos, 50))
                   || EntityManager.Heroes.Allies.All(a => !a.IsMe && (a.IsInShopRange() || a.IsDead));
        }

        /// <summary>
        ///     Last Turret Attack Time.
        /// </summary>
        public static float LastTurretAttack;

        /// <summary>
        ///     Checks Turret Attacks And saves Heros AutoAttacks.
        /// </summary>
        public static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var turret = sender as Obj_AI_Turret;
            if (turret != null)
            {
                if (args.Target.IsMe)
                {
                    LastTurretAttack = Core.GameTickCount;
                }
                
                var target = args.Target as AIHeroClient;
                if (target != null && target.IsAlly && !target.IsMe)
                {
                    var lastAttack = new Misc.LastAttack(turret, target) { Attacker = turret, LastAttackSent = Core.GameTickCount, Target = target };
                    Misc.AutoAttacks.Add(lastAttack);
                }
            }
        }

        /// <summary>
        ///     Override orbwalker position.
        /// </summary>
        private static Vector3? OverrideOrbwalkPosition()
        {
            return Pathing.Position;
        }
    }
}
