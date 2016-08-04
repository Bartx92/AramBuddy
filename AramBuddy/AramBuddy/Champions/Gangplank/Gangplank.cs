using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Champions.Gangplank
{
    class Gangplank : Base
    {
        private static Spell.Targeted Q { get; }
        private static Spell.Active W { get; }
        private static Spell.Skillshot E { get; }
        private static Spell.Skillshot R { get; }

        static Gangplank()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Circular, 250, int.MaxValue, 350);
            R = new Spell.Skillshot(SpellSlot.R, int.MaxValue, SkillShotType.Circular, 250, int.MaxValue, 600);
            SpellList.Add(Q);
            SpellList.Add(E);

            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            ComboMenu.CreateCheckBox("R", "Use R");
            ComboMenu.CreateSlider("RAOE", "R AOE HIT {0}", 2, 1, 5);

            KillStealMenu.CreateCheckBox("R", "Use R");
            BarrelsManager.Init();
        }

        public override void Active()
        {
            if (user.IsCC() && user.HealthPercent <= 80)
            {
                W.Cast();
            }
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(Q.Slot))
            {
                foreach (var barrel in BarrelsManager.BarrelsList.OrderByDescending(b => b.CountEnemiesInRange(E.Width)).Where(b => b.IsInRange(target, E.Width) && b.Health <= 1))
                {
                    Q.Cast(barrel);
                }

                if (target.IsKillable(Q.Range))
                {
                    Q.Cast(target);
                }
            }

            if (E.IsReady() && Q.IsReady() && target.IsValidTarget(E.Range) && ComboMenu.CheckBoxValue(E.Slot))
            {
                E.Cast(target, HitChance.Low);
            }

            if (R.IsReady() && ComboMenu.CheckBoxValue(R.Slot))
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e.IsKillable(4500)))
                {
                    if (ComboMenu.CompareSlider("RAOE", enemy.CountEnemiesInRange(R.Width)))
                    {
                        R.Cast(enemy);
                    }
                }
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range)) return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                foreach (var barrel in BarrelsManager.BarrelsList.OrderByDescending(b => b.CountEnemiesInRange(E.Width)).Where(b => b.IsInRange(target, E.Width) && b.Health <= 1))
                {
                    Q.Cast(barrel);
                }

                if (target.IsKillable(Q.Range))
                {
                    Q.Cast(target);
                }
            }

            if (E.IsReady() && Q.IsReady() && target.IsValidTarget(E.Range) && HarassMenu.CheckBoxValue(E.Slot) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
            {
                E.Cast(target, HitChance.Low);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(e => e != null && e.IsKillable(Q.Range)))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    foreach (var barrel in BarrelsManager.BarrelsList.OrderByDescending(b => b.CountEnemyMinionsInRange(E.Width)).Where(b => b.IsInRange(target, E.Width) && b.Health <= 1))
                    {
                        Q.Cast(barrel);
                    }

                    if (target.IsKillable(Q.Range))
                    {
                        Q.Cast(target);
                    }
                }

                if (E.IsReady() && Q.IsReady() && target.IsValidTarget(E.Range) && LaneClearMenu.CheckBoxValue(E.Slot) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast(target, HitChance.Low);
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
                if (Q.IsReady() && KillStealMenu.CheckBoxValue(Q.Slot))
                {
                    foreach (var barrel in BarrelsManager.BarrelsList.OrderByDescending(b => b.CountEnemiesInRange(E.Width)).Where(b => b.IsInRange(target, E.Width) && b.Health <= 1 && E.WillKill(target)))
                    {
                        Q.Cast(barrel);
                    }

                    if (target.IsKillable(Q.Range) && Q.WillKill(target))
                    {
                        Q.Cast(target);
                    }
                }

                if (E.IsReady() && Q.IsReady() && target.IsValidTarget(E.Range) && E.WillKill(target) && KillStealMenu.CheckBoxValue(E.Slot))
                {
                    E.Cast(target, HitChance.Low);
                }

                if (R.IsReady() && KillStealMenu.CheckBoxValue(R.Slot) && R.WillKill(target))
                {
                    R.Cast(target);
                }
            }
        }
    }
}
