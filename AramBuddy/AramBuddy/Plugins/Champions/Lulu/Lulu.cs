using System;
using System.Linq;
using AramBuddy.KappaEvade;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Lulu
{
    internal class Lulu : Base
    {
        private static readonly Obj_AI_Base PixObject =
            ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(o => o.IsAlly && o.Name == "RobotBuddy");

        static Lulu()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1450, 60);
            {
                Q.AllowedCollisionCount = int.MaxValue;
            }
            Q1 = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1450, 60);
            {
                if (Pix.IsValid && Pix != null)
                    Q1.SourcePosition = Pix.ServerPosition;
            }
            W = new Spell.Targeted(SpellSlot.W, 650);
            E = new Spell.Targeted(SpellSlot.E, 650);
            R = new Spell.Targeted(SpellSlot.R, 900);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);


            AutoMenu.CreateCheckBox("Rsave", "R Saver");
            ComboMenu.CreateSlider("RAOE", "R AOE {0}", 3, 1, 5);
            AutoMenu.CreateCheckBox("DashQ", "Q on dashing Targets");
            AutoMenu.CreateCheckBox("FleeQ", "Flee Q");
            AutoMenu.CreateCheckBox("FleeW", "Flee W");
            AutoMenu.CreateCheckBox("FleeE", "Flee E");
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("GapQ1", "Anti-GapCloser Q Pix");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("GapWally", "Shield Engaging Allys");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntW", "Interrupter W");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");
            AutoMenu.CreateCheckBox("Wself", "Shield self W");
            AutoMenu.CreateCheckBox("Wally", "Shield ally W");
            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
            SpellsDetector.OnTargetedSpellDetected += SpellsDetector_OnTargetedSpellDetected;
            Game.OnTick += Lulu_SkillshotDetector;
        }

        private static Spell.Skillshot Q { get; }
        private static Spell.Skillshot Q1 { get; }
        private static Spell.Targeted W { get; }
        private static Spell.Targeted E { get; }
        private static Spell.Targeted R { get; }

        private static Obj_AI_Base Pix
        {
            get
            {
                if (PixObject != null && PixObject.IsValid)
                {
                    return PixObject;
                }

                return null;
            }
        }

        private static void Lulu_SkillshotDetector(EventArgs args)
        {
            if (AutoMenu.CheckBoxValue("Rsave") && R.IsReady())

                foreach (
                    var ally in
                        from ally in
                            EntityManager.Heroes.Allies.Where(a => a.IsKillable(R.Range) && a.HealthPercent < 10)
                        from spell in Collision.NewSpells.Where(ally.IsInDanger)
                        select ally)
                {
                    R.Cast(ally);
                }

            if (AutoMenu.CheckBoxValue("Wself") && W.IsReady())
            {
                // ReSharper disable once UnusedVariable
                foreach (var spell in Collision.NewSpells.Where(spell => user.IsInDanger(spell)))
                {
                    W.Cast(user);
                }
            }

            if (!AutoMenu.CheckBoxValue("Wally") || !W.IsReady() || user.ManaPercent < 60) return;

            foreach (
                var ally in
                    from ally in EntityManager.Heroes.Allies.Where(a => a.IsKillable(W.Range) && a != user)
                    from spell in Collision.NewSpells.Where(ally.IsInDanger)
                    select ally)
            {
                W.Cast(ally);
            }
        }

        private static void SpellsDetector_OnTargetedSpellDetected(Obj_AI_Base sender, Obj_AI_Base target,
            GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spells)
        {
            if (target.IsMe && spells.DangerLevel >= 3 && AutoMenu.CheckBoxValue("Wself") && W.IsReady())
            {
                W.Cast(user);
            }

            if (!AutoMenu.CheckBoxValue("Wally") || !W.IsReady() || spells.DangerLevel <= 2 ||
                user.ManaPercent < 60) return;

            foreach (
                var ally in
                    EntityManager.Heroes.Allies.Where(a => a.IsKillable(W.Range) && target.NetworkId == a.NetworkId))
            {
                W.Cast(ally);
            }

            if (!AutoMenu.CheckBoxValue("Rsave") || !R.IsReady() || spells.DangerLevel <= 2) return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(a => a.IsKillable(R.Range) && a.HealthPercent < 10))
            {
                R.Cast(ally);
            }
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q1.Range)) return;
            {
                if (AutoMenu.CheckBoxValue("DashQ") && Q.IsReady() &&
                    (e.EndPos.IsInRange(user, Q.Range) || Pix.IsInRange(e.EndPos, Q.Range)))
                {
                    Q.Cast(e.EndPos);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !W.IsReady()) return;

            if (AutoMenu.CheckBoxValue("IntW") && sender.IsKillable(W.Range))
            {
                W.Cast(sender);
            }

            if (!AutoMenu.CheckBoxValue("IntR") || !sender.IsKillable(R.Range)) return;
            {
                foreach (
                    var ally in
                        EntityManager.Heroes.Allies.Where(a => a.IsKillable(R.Range) && a.Distance(sender) <= 300))
                {
                    R.Cast(ally);
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null) return;
            {
                if (sender.IsEnemy && sender.IsKillable(1000))
                {
                    if (AutoMenu.CheckBoxValue("GapQ") && Q.IsReady() && e.End.IsInRange(user, Q.Range))
                    {
                        Q.Cast(e.End);
                    }
                    if (AutoMenu.CheckBoxValue("GapQ1") && Q.IsReady() && e.End.IsInRange(Pix.ServerPosition, Q1.Range))
                    {
                        Q1.Cast(e.End);
                    }
                    if (AutoMenu.CheckBoxValue("GapW") && W.IsReady() && e.End.IsInRange(user, W.Range))
                    {
                        if (sender.IsKillable(W.Range))
                            W.Cast(sender);
                    }
                    if (AutoMenu.CheckBoxValue("GapE") && E.IsReady() && e.End.IsInRange(user, E.Range))
                    {
                        if (sender.IsKillable(E.Range))
                            E.Cast(sender);
                    }
                    if (AutoMenu.CheckBoxValue("GapR") && R.IsReady() && e.End.IsInRange(user, 300) &&
                        user.HealthPercent < 15)
                    {
                        R.Cast(user);
                    }
                }
                if (!sender.IsAlly) return;
                {
                    if (!AutoMenu.CheckBoxValue("GapWally") || !W.IsReady() ||
                        !sender.IsInRange(user, W.Range))
                        return;
                    {
                        if (sender.IsKillable(W.Range))
                            W.Cast(sender);
                    }
                }
            }
        }

        public override void Active()
        {
            if (!R.IsReady()) return;
            {
                foreach (
                    var ally in
                        EntityManager.Heroes.Allies.Where(a => a.IsKillable(R.Range))
                            .Where(
                                ally =>
                                    ally.CountEnemiesInRange(300) >= ComboMenu.SliderValue("RAOE") &&
                                    ComboMenu.CheckBoxValue("R")))
                {
                    R.Cast(ally);
                }
            }
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)
                                                       && s != R))
            {
                var target = Pix != null
                    ? TargetSelector.GetTarget(E.Range + Q.Range, DamageType.Magical)
                    : TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target == null) return;

                var qPredPlayer = Q.GetPrediction(target);
                var qPredPix = Q1.GetPrediction(target);

                if (spell.Slot == SpellSlot.Q)
                {
                    foreach (
                        var enemy in
                            from enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(E.Range + Q.Range))
                            let qPredPlayer1 = Q.GetPrediction(enemy)
                            let qPredPix1 = Q1.GetPrediction(enemy)
                            where qPredPlayer1.HitChance >= HitChance.Medium && qPredPix1.HitChance >= HitChance.Medium
                            select enemy)
                    {
                        Q.Cast(enemy);
                    }

                    if (qPredPlayer.HitChance >= HitChance.Medium || qPredPix.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.W)
                {
                    {
                        if (target.IsKillable(W.Range))
                            W.Cast(target);
                    }
                }
                if (spell.Slot != SpellSlot.E) continue;
                {
                    if (target.IsKillable(E.Range))
                        E.Cast(target);
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && HarassMenu.CheckBoxValue(s.Slot)
                                                       && s != R))
            {
                var target = Pix != null
                    ? TargetSelector.GetTarget(E.Range + Q.Range, DamageType.Magical)
                    : TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target == null) return;

                var qPredPlayer = Q.GetPrediction(target);
                var qPredPix = Q1.GetPrediction(target);

                if (spell.Slot == SpellSlot.Q)
                {
                    foreach (
                        var enemy in
                            from enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(E.Range + Q.Range))
                            let qPredPlayer1 = Q.GetPrediction(enemy)
                            let qPredPix1 = Q1.GetPrediction(enemy)
                            where qPredPlayer1.HitChance >= HitChance.Medium && qPredPix1.HitChance >= HitChance.Medium
                            select enemy)
                    {
                        Q.Cast(enemy);
                    }

                    if (qPredPlayer.HitChance >= HitChance.Medium || qPredPix.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.W)
                {
                    {
                        if (target.IsKillable(W.Range))
                            W.Cast(target);
                    }
                }
                if (spell.Slot != SpellSlot.E) continue;
                {
                    if (target.IsKillable(E.Range))
                        E.Cast(target);
                }
            }
        }

        public override void LaneClear()
        {
            foreach (
                var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                foreach (
                    var spell in
                        SpellList.Where(
                            s =>
                                s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot) &&
                                LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        Q.CastOnBestFarmPosition();
                    }
                    else
                    {
                        var spells = spell as Spell.Targeted;
                        spells?.Cast(target);
                    }
                }
            }
        }

        public override void Flee()
        {
            if (W.IsReady() && AutoMenu.CheckBoxValue("FleeW") && user.ManaPercent >= 65)
            {
                W.Cast(user);
            }
            if (E.IsReady() && AutoMenu.CheckBoxValue("FleeE") && user.ManaPercent >= 65)
            {
                E.Cast(user);
            }
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range)) return;
            if (Q.IsReady() && AutoMenu.CheckBoxValue("FleeQ") && user.ManaPercent >= 65)
            {
                Q.Cast(target, HitChance.Medium);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (
                    var spell in
                        SpellList.Where(
                            s =>
                                s.WillKill(target) && s != R && s.IsReady() && target.IsKillable(s.Range) &&
                                KillStealMenu.CheckBoxValue(s.Slot)))

                    if (spell.Slot == SpellSlot.Q)
                    {
                        if (Q.GetPrediction(target).HitChance >= HitChance.Medium ||
                            Q1.GetPrediction(target).HitChance >= HitChance.Medium)
                            Q.Cast(target);
                    }
                    else
                    {
                        (spell as Spell.Targeted)?.Cast(target);
                    }
            }
        }
    }
}