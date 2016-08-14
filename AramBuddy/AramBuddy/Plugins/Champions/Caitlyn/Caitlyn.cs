using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace AramBuddy.Plugins.Champions.Caitlyn
{
    internal class Caitlyn : Base
    {
        static Caitlyn()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, 250, 2000, 60);
            {
                Q.AllowedCollisionCount = int.MaxValue;
            }
            W = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, int.MaxValue, 80);
            {
                W.AllowedCollisionCount = int.MaxValue;
            }
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, 250, 1600, 80);
            R = new Spell.Targeted(SpellSlot.R, 2000);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            AutoMenu.CreateCheckBox("E", "Flee E");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("IntW", "Interrupter W");
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

        private static Spell.Skillshot Q { get; }
        private static Spell.Skillshot W { get; }
        private static Spell.Skillshot E { get; }
        private static Spell.Targeted R { get; }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000))
                return;
            {
                if (AutoMenu.CheckBoxValue("DashW") && W.IsReady() && e.EndPos.IsInRange(Player.Instance, W.Range))
                {
                    W.Cast(e.EndPos);
                }
                if (!Player.HasBuff("caitlynheadshot") && !Player.HasBuff("caitlynheadshotrangecheck") && AutoMenu.CheckBoxValue("DashE") && E.IsReady() && e.EndPos.IsInRange(Player.Instance, E.Range))
                {
                    E.Cast(sender as AIHeroClient, HitChance.Medium);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(W.Range) || !W.IsReady() || !AutoMenu.CheckBoxValue("IntW"))
                return;
            W.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000))
                return;
            if (AutoMenu.CheckBoxValue("GapE") && E.IsReady() && e.End.IsInRange(Player.Instance, E.Range))
            {
                E.Cast((Vector3)Player.Instance.ServerPosition.Extend(e.End, E.Range));
            }
            if (AutoMenu.CheckBoxValue("GapW") && W.IsReady() && e.End.IsInRange(Player.Instance, W.Range))
            {
                W.Cast(e.End);
            }
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    R.Cast(target);
                }
                if (spell.Slot == SpellSlot.E)
                {
                    if (!Player.HasBuff("caitlynheadshot") && !Player.HasBuff("caitlynheadshotrangecheck"))
                    {
                        E.Cast(target, HitChance.Medium);
                    }
                }
                if (spell.Slot == SpellSlot.Q)
                {
                    if (Player.Instance.GetAutoAttackDamage(target) < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target, HitChance.Medium);
                    }
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
            foreach (var spell in
                SpellList.Where(s => s.IsReady() && HarassMenu.CheckBoxValue(s.Slot) && HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    if (target.CountEnemiesInRange(300) == 0)
                    {
                        R.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.E)
                {
                    if (!Player.HasBuff("caitlynheadshot") && !Player.HasBuff("caitlynheadshotrangecheck"))
                    {
                        E.Cast(target, HitChance.Medium);
                    }
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
            foreach (var target in
                from target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget())
                from spell in SpellList.Where(s => s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot) && LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent))
                where spell.Slot == SpellSlot.Q
                select target)
            {
                Q.Cast(target);
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(E.Range))
                return;
            if (E.IsReady() && AutoMenu.CheckBoxValue("E") && user.ManaPercent >= 65)
            {
                E.Cast(target, HitChance.Medium);
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (var spell in
                    SpellList.Where(s => s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) && KillStealMenu.CheckBoxValue(s.Slot)))
                {
                    if (spell.Slot == SpellSlot.R)
                    {
                        if (target.CountEnemiesInRange(300) == 0)
                            spell.Cast(target);
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
