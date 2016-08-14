using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.Ashe
{
    internal class Ashe : Base
    {
        private static Spell.Active Q { get; }
        private static Spell.Skillshot W { get; }
        private static Spell.Skillshot R { get; }

        static Ashe()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Active(SpellSlot.Q, 600);
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 0, int.MaxValue, 60);
            R = new Spell.Skillshot(SpellSlot.R, 4500, SkillShotType.Linear, 250, 1600, 100) { AllowedCollisionCount = 0 };
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(R);

            AutoMenu.CreateCheckBox("W", "Flee W");
            AutoMenu.CreateCheckBox("GapR", "Anti-GapCloser R");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");
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
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000) || !R.IsReady() || !AutoMenu.CheckBoxValue("GapR"))
                return;
            R.Cast(sender);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000) || !R.IsReady() || !AutoMenu.CheckBoxValue("IntR"))
                return;
            R.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000) || !R.IsReady() || !AutoMenu.CheckBoxValue("GapR"))
                return;
            R.Cast(sender);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.Q)
                {
                    Q.Cast();
                }
                else
                {
                    var skillshot = spell as Spell.Skillshot;
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && HarassMenu.CheckBoxValue(s.Slot) && HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.Q)
                {
                    Q.Cast();
                }
                else
                {
                    var skillshot = spell as Spell.Skillshot;
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                foreach (var spell in SpellList.Where(s => s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot) && LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        Q.Cast();
                    }
                    else
                    {
                        var skillshot = spell as Spell.Skillshot;
                        skillshot.Cast(target, HitChance.Medium);
                    }
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(W.Range))
                return;
            if (W.IsReady() && AutoMenu.CheckBoxValue("W") && user.ManaPercent >= 65)
            {
                W.Cast(target, HitChance.Medium);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (var spell in SpellList.Where(s => s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) && KillStealMenu.CheckBoxValue(s.Slot)))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        spell.Cast();
                    }
                    else
                    {
                        var skillshot = spell as Spell.Skillshot;
                        skillshot.Cast(target, HitChance.Medium);
                    }
                }
            }
        }
    }
}
