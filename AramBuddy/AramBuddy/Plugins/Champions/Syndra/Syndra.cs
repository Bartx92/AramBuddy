using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.Syndra
{
    class Syndra : Base
    {
        internal static List<Obj_AI_Minion> BallsList = new List<Obj_AI_Minion>();

        private static Spell.Skillshot Q { get; }
        private static Spell.Skillshot W { get; }
        private static Spell.Skillshot E { get; }
        private static Spell.Targeted R { get; }

        static Syndra()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 820, SkillShotType.Circular, 600, int.MaxValue, 125);
            W = new Spell.Skillshot(SpellSlot.W, 925, SkillShotType.Circular, 350, 1500, 130);
            E = new Spell.Skillshot(SpellSlot.E, 695, SkillShotType.Cone, 250, 2500, 50);
            R = new Spell.Targeted(SpellSlot.R, 695);
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
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000) || !E.IsReady())
                return;

            if (SelectBall(sender) != null)
            {
                E.Cast(SelectBall(sender));
            }
            else
            {
                if (sender.IsValidTarget(E.Range))
                {
                    E.Cast(sender);
                }
            }
        }

        public override void Active()
        {
            foreach (var ball in ObjectManager.Get<Obj_AI_Minion>().Where(o => o != null && !o.IsDead && o.IsAlly && o.Health > 0 && o.BaseSkinName.Equals("SyndraSphere")))
            {
                if (!BallsList.Contains(ball))
                {
                    BallsList.Add(ball);
                }
            }
            BallsList.RemoveAll(b => b == null || b.IsDead || !b.IsValid || b.Health <= 0);
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && ComboMenu.CheckBoxValue(Q.Slot))
            {
                Q.Cast(target, HitChance.Low);
            }

            if (W.IsReady() && ComboMenu.CheckBoxValue(W.Slot))
            {
                W.Cast(target, HitChance.Medium);
            }

            if (E.IsReady() && ComboMenu.CheckBoxValue(E.Slot))
            {
                if (SelectBall(target) != null)
                {
                    E.Cast(SelectBall(target));
                }
                else
                {
                    if (target.IsKillable(E.Range))
                    {
                        E.Cast(target);
                    }
                }
            }

            if (ComboMenu.CheckBoxValue(R.Slot) && R.IsReady())
            {
                R.Cast(target);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (Q.IsReady() && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(target, HitChance.Low);
            }

            if (W.IsReady() && HarassMenu.CheckBoxValue(W.Slot) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
            {
                W.Cast(target, HitChance.Medium);
            }

            if (E.IsReady() && HarassMenu.CheckBoxValue(E.Slot) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
            {
                if (SelectBall(target) != null)
                {
                    E.Cast(SelectBall(target));
                }
                else
                {
                    if (target.IsKillable(E.Range))
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable(1000)))
            {
                if (Q.IsReady() && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Low);
                }

                if (W.IsReady() && LaneClearMenu.CheckBoxValue(W.Slot) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent))
                {
                    W.Cast(target, HitChance.Medium);
                }

                if (E.IsReady() && LaneClearMenu.CheckBoxValue(E.Slot) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    if (SelectBall(target) != null)
                    {
                        E.Cast(SelectBall(target));
                    }
                    else
                    {
                        if (target.IsKillable(E.Range))
                        {
                            E.Cast(target);
                        }
                    }
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))
                return;

            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target);
            }

            if (SelectBall(target) == null)
            {
                if (Q.IsReady() && E.IsReady())
                {
                    var pos = user.ServerPosition.Extend(target.ServerPosition, 100).To3D();
                    Q.Cast(pos);
                    E.Cast(pos);
                }
            }
            else
            {
                E.Cast(SelectBall(target));
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

                if (W.IsReady() && W.WillKill(target) && KillStealMenu.CheckBoxValue(W.Slot))
                {
                    W.Cast(target, HitChance.Low);
                }

                if (E.IsReady() && E.WillKill(target) && KillStealMenu.CheckBoxValue(E.Slot))
                {
                    if (SelectBall(target) != null)
                    {
                        E.Cast(SelectBall(target));
                    }
                    else
                    {
                        if (target.IsValidTarget(E.Range))
                        {
                            E.Cast(target);
                        }
                    }
                }

                if (R.IsReady() && R.WillKill(target) && KillStealMenu.CheckBoxValue(R.Slot))
                {
                    R.Cast(target);
                }
            }
        }

        private static Obj_AI_Minion SelectBall(Obj_AI_Base target)
        {
            Obj_AI_Minion theball = null;
            foreach (var ball in BallsList.Where(b => b != null && E.IsInRange(b)))
            {
                var rect = new Geometry.Polygon.Rectangle(user.ServerPosition, user.ServerPosition.Extend(ball.ServerPosition, 1000).To3D(), 80);
                if (rect.IsInside(target))
                {
                    theball = ball;
                }
            }
            return theball;
        }
    }
}
