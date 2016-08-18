using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.Amumu
{
    class Amumu : Base
    {
        private static Spell.Skillshot Q { get; }
        private static Spell.Active W { get; }
        private static Spell.Active E { get; }
        private static Spell.Active R { get; }

        static Amumu()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 2000, 80) { AllowedCollisionCount = 0 };
            W = new Spell.Active(SpellSlot.W, 300);
            E = new Spell.Active(SpellSlot.E, 350);
            R = new Spell.Active(SpellSlot.R, 550);
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
            AutoMenu.CreateSlider("RAOE", "R AOE hit cunt {0}", 3, 1, 5);
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ") && (sender.IsKillable(Q.Range) || e.EndPos.IsInRange(user, Q.Range)))
            {
                Q.Cast(sender, HitChance.Low);
                return;
            }
            if (R.IsReady() && AutoMenu.CheckBoxValue("GapR") && (sender.IsKillable(R.Range) || e.EndPos.IsInRange(user, R.Range)))
            {
                R.Cast();
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("IntQ") && sender.IsKillable(Q.Range))
            {
                Q.Cast(sender, HitChance.Low);
                return;
            }
            if (R.IsReady() && AutoMenu.CheckBoxValue("IntR") && sender.IsKillable(R.Range))
            {
                R.Cast();
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ") && (sender.IsKillable(Q.Range) || e.End.IsInRange(user, Q.Range)))
            {
                Q.Cast(sender, HitChance.Low);
                return;
            }
            if (R.IsReady() && AutoMenu.CheckBoxValue("GapR") && (sender.IsKillable(R.Range) || e.End.IsInRange(user, R.Range)))
            {
                R.Cast();
            }
        }

        public override void Active()
        {
            RAOE(AutoMenu.SliderValue("RAOE"));
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

        }

        public override void Harass()
        {
        }

        public override void LaneClear()
        {
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
        }

        private static void RAOE(int HitCount)
        {
            if (EntityManager.Heroes.Enemies.Count(e => e.IsKillable() && e.PredictPosition().IsInRange(user, R.Range)) >= HitCount && R.IsReady())
            {
                R.Cast();
            }
        }
    }
}
