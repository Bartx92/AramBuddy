using System;
using System.Linq;
using AramBuddy.KappaEvade;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Garen
{
    internal class Garen : Base
    {
        private static Spell.Active Q { get; }
        private static Spell.Active W { get; }
        private static Spell.Active E { get; }
        private static Spell.Targeted R { get; }

        static Garen()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E, 300);
            R = new Spell.Targeted(SpellSlot.R, 400);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            AutoMenu.CreateCheckBox("Q", "Flee Q");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            AutoMenu.CreateCheckBox("TDmgW", "W against targeted Spells");
            AutoMenu.CreateCheckBox("SDmgW", "W against Skillshots");
            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
            }

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            SpellsDetector.OnTargetedSpellDetected += SpellsDetector_OnTargetedSpellDetected;
            Game.OnTick += Garen_SkillshotDetector;
        }

        private static void Garen_SkillshotDetector(EventArgs args)
        {
            if (!AutoMenu.CheckBoxValue("SDmgW") || !W.IsReady())
                return;
            if (Collision.NewSpells.Any(s => user.IsInDanger(s)))
            {
                W.Cast();
            }
        }

        private static void SpellsDetector_OnTargetedSpellDetected(Obj_AI_Base sender, Obj_AI_Base target, GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spell)
        {
            if (target.IsMe && spell.DangerLevel >= 3 && AutoMenu.CheckBoxValue("TDmgW") && W.IsReady())
            {
                W.Cast();
            }
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            var t = target as AIHeroClient;
            foreach (var spell in
                SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)).Where(spell => t.IsKillable(Player.Instance.GetAutoAttackRange()) && spell.IsReady() && spell.Slot == SpellSlot.Q))
            {
                spell.Cast();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Player.Instance.GetAutoAttackRange()) || !Q.IsReady() || !AutoMenu.CheckBoxValue("IntQ"))
                return;
            {
                Q.Cast();
                Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000))
                return;
            if (AutoMenu.CheckBoxValue("GapW") && W.IsReady() && e.End.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange()))
            {
                W.Cast();
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
                    if (target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    {
                        R.Cast(target);
                    }
                }
                if (spell.Slot == SpellSlot.Q)
                {
                    //
                }
                else
                {
                    var spells = spell as Spell.Active;
                    if (!Player.Instance.HasBuff("GarenE"))
                        spells?.Cast();
                }
            }
        }

        public override void Harass()
        {
            foreach (var spell in
                SpellList.Where(s => s.IsReady() && HarassMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range))
                    return;

                if (spell.Slot == SpellSlot.R)
                {
                    R.Cast(target);
                }
                else
                {
                    var spells = spell as Spell.Active;
                    if (!Player.Instance.HasBuff("GarenE"))
                        spells?.Cast();
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var spell in
                EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget(Player.Instance.GetAutoAttackRange()))
                    .SelectMany(target => SpellList.Where(s => s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot))))
            {
                if (spell.Slot == SpellSlot.R)
                {
                    //
                }
                else
                {
                    var spells = spell as Spell.Active;
                    if (!Player.Instance.HasBuff("GarenE"))
                        spells?.Cast();
                }
            }
        }

        public override void Flee()
        {
            if (Q.IsReady() && AutoMenu.CheckBoxValue("Q"))
            {
                Q.Cast();
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (
                    var spell in
                        SpellList.Where(s => s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) && KillStealMenu.CheckBoxValue(s.Slot)).Where(spell => !Player.Instance.HasBuff("GarenE")))
                {
                    if (spell.Slot == SpellSlot.R)
                    {
                        spell.Cast(target);
                    }
                    else
                    {
                        var spells = spell as Spell.Active;
                        spells?.Cast();
                    }
                }
            }
        }
    }
}
