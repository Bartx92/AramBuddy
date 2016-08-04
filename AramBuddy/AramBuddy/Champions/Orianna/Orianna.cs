using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Champions.Orianna
{
    class Orianna : Base
    {
        private static Spell.Skillshot Q { get; }
        private static Spell.Active W { get; }
        private static Spell.Targeted E { get; }
        private static Spell.Active R { get; }

        static Orianna()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Linear, 250, 1000, 60);
            W = new Spell.Active(SpellSlot.W, 250);
            E = new Spell.Targeted(SpellSlot.E, 1100);
            R = new Spell.Active(SpellSlot.R, 325);
            SpellList.Add(Q);
            SpellList.Add(W);
            
            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            AutoMenu.CreateCheckBox("W", "Flee W");
            AutoMenu.CreateCheckBox("IntR", "Interrupter R");
            AutoMenu.CreateCheckBox("R", "Use R");
            AutoMenu.CreateSlider("RAOE", "R AOE HIT {0}", 3, 1, 5);

            ComboMenu.CreateCheckBox("R", "Use R");
            ComboMenu.CreateSlider("RAOE", "R AOE HIT {0}", 2, 1, 5);

            KillStealMenu.CreateCheckBox("R", "Use R");
            BallManager.Init();
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !R.IsReady() || !AutoMenu.CheckBoxValue("IntR") || BallManager.OriannaBall == null || !sender.PrediectPosition().IsInRange(BallManager.OriannaBall, R.Range)) return;
            R.Cast(sender);
        }

        public override void Active()
        {
            if (AutoMenu.CheckBoxValue(R.Slot))
                RAOE(AutoMenu.SliderValue("RAOE"));
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(Q.Slot))
            {
                Q.Cast(target, HitChance.Low);
            }
            if (W.IsReady() && BallManager.OriannaBall != null && target.PrediectPosition().IsInRange(BallManager.OriannaBall, W.Range) && ComboMenu.CheckBoxValue(W.Slot))
            {
                W.Cast();
            }
            if (ComboMenu.CheckBoxValue(R.Slot))
                RAOE(ComboMenu.SliderValue("RAOE"));
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(target, HitChance.Low);
            }
            if (W.IsReady() && BallManager.OriannaBall != null && target.PrediectPosition().IsInRange(BallManager.OriannaBall, W.Range) && HarassMenu.CheckBoxValue(W.Slot) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                W.Cast();
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(1000)))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (W.IsReady() && BallManager.OriannaBall != null && target.PrediectPosition().IsInRange(BallManager.OriannaBall, W.Range) && LaneClearMenu.CheckBoxValue(W.Slot) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    W.Cast();
                }
            }
        }

        public override void Flee()
        {
            if(!AutoMenu.CheckBoxValue("W") || BallManager.OriannaBall == null || !W.IsReady()) return;

            if (EntityManager.Heroes.Enemies.Any(e => e != null && e.PrediectPosition().Distance(user) < 400) && user.HealthPercent < 25 && user.ManaPercent > 10)
            {
                if (BallManager.OriannaBall.IsInRange(user, W.Range))
                {
                    W.Cast();
                }
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(m => m != null && m.IsKillable(1000)))
            {
                if (Q.IsReady() && Q.WillKill(target) && target.IsKillable(Q.Range) && KillStealMenu.CheckBoxValue(Q.Slot))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (W.IsReady() && W.WillKill(target) && BallManager.OriannaBall != null && target.PrediectPosition().IsInRange(BallManager.OriannaBall, W.Range) && KillStealMenu.CheckBoxValue(W.Slot))
                {
                    W.Cast();
                }
            }
        }

        private static void RAOE(int hits)
        {
            if (BallManager.OriannaBall != null && R.IsReady())
            {
                if (EntityManager.Heroes.Enemies.Count(e => e != null && e.IsKillable(1100) && e.PrediectPosition().IsInRange(BallManager.OriannaBall, R.Range)) >= hits)
                {
                    R.Cast();
                }
            }
        }
    }
}
