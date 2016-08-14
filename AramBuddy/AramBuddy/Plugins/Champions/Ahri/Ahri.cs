using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.Ahri
{
    class Ahri : Base
    {
        private static Spell.Skillshot Q { get; }
        private static Spell.Active W { get; }
        private static Spell.Skillshot E { get; }
        private static Spell.Skillshot R { get; }

        static Ahri()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1750, 100) { AllowedCollisionCount = int.MaxValue };
            W = new Spell.Active(SpellSlot.W, 750);
            E = new Spell.Skillshot(SpellSlot.E, 950, SkillShotType.Linear, 250, 1550, 60) { AllowedCollisionCount = 0 };
            R = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Linear, 250, 1550, 60) { AllowedCollisionCount = int.MaxValue };
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("IntE", "Interrupter E");
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(E.Range) || !E.IsReady() || !AutoMenu.CheckBoxValue("GapE"))
                return;
            E.Cast(sender, HitChance.Low);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(E.Range) || !E.IsReady() || !AutoMenu.CheckBoxValue("IntE"))
                return;
            E.Cast(sender, HitChance.Low);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.Q))
            {
                Q.Cast(target, HitChance.Medium);
            }
            if (W.IsReady() && target.IsKillable(W.Range) && ComboMenu.CheckBoxValue(SpellSlot.W))
            {
                W.Cast();
            }
            if (E.IsReady() && target.IsKillable(E.Range) && ComboMenu.CheckBoxValue(SpellSlot.E))
            {
                E.Cast(target, HitChance.Medium);
            }
            if (R.IsReady() && ComboMenu.CheckBoxValue(SpellSlot.R))
            {
                R.Cast(target.PredictPosition().Extend(user.PredictPosition(), 300).To3D());
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(SpellSlot.Q) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(target, HitChance.Medium);
            }
            if (W.IsReady() && target.IsKillable(W.Range) && HarassMenu.CheckBoxValue(SpellSlot.W) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                W.Cast();
            }
            if (E.IsReady() && target.IsKillable(E.Range) && HarassMenu.CheckBoxValue(SpellSlot.E) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
            {
                E.Cast(target, HitChance.Medium);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(Q.Range)))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Medium);
                }
                if (W.IsReady() && target.IsKillable(W.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    W.Cast();
                }
                if (E.IsReady() && target.IsKillable(E.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.E) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable(Q.Range)))
            {
                if (Q.IsReady() && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(target, HitChance.Medium);
                }
                if (W.IsReady() && W.WillKill(target) && target.IsKillable(W.Range) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    W.Cast();
                }
                if (E.IsReady() && E.WillKill(target) && target.IsKillable(E.Range) && KillStealMenu.CheckBoxValue(SpellSlot.E))
                {
                    E.Cast(target, HitChance.Medium);
                }
                if (R.IsReady() && R.WillKill(target) && target.IsKillable(R.Range) && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    R.Cast(target, HitChance.Low);
                }
            }
        }
    }
}
