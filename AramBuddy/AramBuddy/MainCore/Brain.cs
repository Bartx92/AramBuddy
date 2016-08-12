using System;
using System.Linq;
using AramBuddy.GenesisSpellDatabase;
using AramBuddy.MainCore.Logics;
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

                ObjectsManager.Init();

                SpecialChamps.Init();
                // Overrides Orbwalker Movements
                Orbwalker.OverrideOrbwalkPosition = OverrideOrbwalkPosition;

                // Initialize AutoLvlup.
                LvlupSpells.Init();

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
        ///     Decisions picking for the bot.
        /// </summary>
        public static void Decisions()
        {
            // Picks best position for the bot.
            Pathing.BestPosition();

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
        ///     Checks Turret Attacks.
        /// </summary>
        public static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender is Obj_AI_Turret && args.Target.IsMe)
            {
                LastTurretAttack = Core.GameTickCount;
            }

            var from = sender as AIHeroClient;
            var target = args.Target as AIHeroClient;
            if (from != null && target != null)
            {
                var lastAttack = new Misc.LastAttack(from, target) { Attacker = from, LastAttackSent = Core.GameTickCount, Target = target };
                Misc.AutoAttacks.Add(lastAttack);
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
