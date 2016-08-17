using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.Gangplank
{
    class Gangplank : Base
    {
        private static readonly List<Barrels> BarrelsList = new List<Barrels>();
        private static Spell.Targeted Q { get; }
        private static Spell.Active W { get; }
        private static Spell.Skillshot E { get; }
        private static Spell.Skillshot R { get; }

        static Gangplank()
        {
            BarrelsList.Clear();
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
        }

        public override void Active()
        {
            foreach (var barrel in ObjectManager.Get<Obj_AI_Minion>().Where(o => !o.IsDead && o.HasBuff("GangplankEBarrelActive") && o.GetBuff("GangplankEBarrelActive").Caster.IsMe))
            {
                var b = new Barrels(barrel, Core.GameTickCount);
                if (!BarrelsList.Contains(b))
                {
                    BarrelsList.Add(b);
                }
            }
            BarrelsList.RemoveAll(o => o.barrel.IsDead || o.barrel.Health <= 0);

            if (user.IsCC() && user.HealthPercent <= 80)
            {
                W.Cast();
            }
        }

        public override void Combo()
        {
            if (R.IsReady() && ComboMenu.CheckBoxValue(R.Slot))
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable(4500)))
                {
                    if (ComboMenu.CompareSlider("RAOE", enemy.CountEnemiesInRange(R.Width)))
                    {
                        R.Cast(enemy);
                    }
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(Q.Slot))
            {
                foreach (var barrel in BarrelsList.OrderByDescending(b => b.barrel.CountEnemiesInRange(E.Width)).Where(b => b.barrel.IsInRange(target, E.Width)))
                {
                    var bhealt = Prediction.Health.GetPrediction(barrel.barrel, (int)((Player.Instance.Distance(barrel.barrel) / 2600) * 1000) + Q.CastDelay);
                    if (bhealt <= 1)
                    {
                        Q.Cast(barrel.barrel);
                    }
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
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                foreach (var barrel in BarrelsList.OrderByDescending(b => b.barrel.CountEnemiesInRange(E.Width)).Where(b => b.barrel.IsInRange(target, E.Width) && b.barrel.Health <= 1))
                {
                    Q.Cast(barrel.barrel);
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
                    foreach (var barrel in BarrelsList.OrderByDescending(b => b.barrel.CountEnemyMinionsInRange(E.Width)).Where(b => b.barrel.IsInRange(target, E.Width) && b.barrel.Health <= 1))
                    {
                        Q.Cast(barrel.barrel);
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

        private bool Killable(Obj_AI_Base target)
        {
            if (target.Health < 2)
            {
                return true;
            }
            var barrel = BarrelsList.FirstOrDefault(b => b.barrel.NetworkId == target.NetworkId);
            if (barrel != null)
            {
                var t = Environment.TickCount - barrel.tick - 2 * 1.975f * 1000;
                t = Math.Abs(Math.Min(t, 0));
                if (t - 2600f * 1000 <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal class Barrels
        {
            public Obj_AI_Minion barrel;
            public float tick;

            public Barrels(Obj_AI_Minion Barrel, int Tick)
            {
                barrel = Barrel;
                tick = Tick;
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && KillStealMenu.CheckBoxValue(Q.Slot))
                {
                    foreach (var barrel in BarrelsList.OrderByDescending(b => b.barrel.CountEnemiesInRange(E.Width)).Where(b => b.barrel.IsInRange(target, E.Width) && b.barrel.Health <= 1 && E.WillKill(target)))
                    {
                        Q.Cast(barrel.barrel);
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

                if (R.IsReady() && KillStealMenu.CheckBoxValue(R.Slot) && R.WillKill(target, 2))
                {
                    R.Cast(target);
                }
            }
        }
    }
}
