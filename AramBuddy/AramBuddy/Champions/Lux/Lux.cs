using System;
using System.Linq;
using AramBuddy.KappaEvade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Champions.Lux
{
    internal class Lux : Base
    {
        static Lux()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");
            KappaEvade.KappaEvade.Init();

            Q = new Spell.Skillshot(SpellSlot.Q, 1300, SkillShotType.Linear, 250, 1200, 70)
            {
                AllowedCollisionCount = 1
            };
            W = new Spell.Skillshot(SpellSlot.W, 1075, SkillShotType.Linear, 0, 1400, 85)
            {
                AllowedCollisionCount = int.MaxValue
            };
            E = new Spell.Skillshot(SpellSlot.E, 1100, SkillShotType.Circular, 250, 1400, 335)
            {
                AllowedCollisionCount = int.MaxValue
            };
            R = new Spell.Skillshot(SpellSlot.R, 3300, SkillShotType.Circular, 1000, int.MaxValue, 110)
            {
                AllowedCollisionCount = int.MaxValue
            };

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            AutoMenu.CreateCheckBox("Q", "Flee Q");
            AutoMenu.CreateCheckBox("FleeQ", "Flee Q");
            AutoMenu.CreateCheckBox("FleeW", "Flee W");
            AutoMenu.CreateCheckBox("FleeE", "Flee E");
            AutoMenu.CreateCheckBox("W", "W incoming Dmg self");
            AutoMenu.CreateCheckBox("Wallies", "W incoming Dmg allies");
            AutoMenu.CreateCheckBox("GapQ", "Anti-GapCloser Q");
            AutoMenu.CreateCheckBox("GapE", "Anti-GapCloser E");
            AutoMenu.CreateCheckBox("GapW", "Anti-GapCloser W");
            AutoMenu.CreateCheckBox("IntQ", "Interrupter Q");
            ComboMenu.CreateSlider("RAOE", "R AOE HIT {0}", 3, 1, 5);

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
            Game.OnTick += Lux_SkillshotDetector;
            Game.OnTick += Lux_PopE;
            SpellsDetector.OnTargetedSpellDetected += SpellsDetector_OnTargetedSpellDetected;
        }

        private static Spell.Skillshot Q { get; }
        private static Spell.Skillshot W { get; }
        private static Spell.Skillshot E { get; }
        private static Spell.Skillshot R { get; }

        private static void Lux_PopE(EventArgs args)
        {
            if (Player.Instance.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2 ||
                Player.Instance.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1)
            {
                E.Cast();
            }
        }

        private static void SpellsDetector_OnTargetedSpellDetected(Obj_AI_Base sender, Obj_AI_Base target,
            GameObjectProcessSpellCastEventArgs args, Database.TargetedSpells.TSpell spell)
        {
            if (target.IsMe && spell.DangerLevel >= 3 && AutoMenu.CheckBoxValue("W") && W.IsReady())
            {
                W.Cast();
            }
            if (!AutoMenu.CheckBoxValue("Wallies") || !W.IsReady()) return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(a => !a.IsDead && !a.IsZombie && a.Distance(Player.Instance) <= W.Range).Where(ally => target.NetworkId.Equals(ally.NetworkId)))
            {
                W.Cast(ally);
            }
        }

        private static void Lux_SkillshotDetector(EventArgs args)
        {
            if (AutoMenu.CheckBoxValue("W") && W.IsReady())
            {
                foreach (var spell in Collision.NewSpells.Where(spell => user.IsInDanger(spell)))
                {
                    W.Cast();
                }
            }
            if (!AutoMenu.CheckBoxValue("Wallies") || !W.IsReady()) return;
            {
                foreach (
                    var ally in
                        Collision.NewSpells.Where(spell => user.IsInDanger(spell))
                            .SelectMany(
                                spell =>
                                    EntityManager.Heroes.Allies.Where(
                                        a => a.IsInRange(Player.Instance, W.Range) && a.IsInDanger(spell))))
                {
                    W.Cast(ally);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(Q.Range) || !Q.IsReady() ||
                !AutoMenu.CheckBoxValue("IntQ")) return;
            Q.Cast(sender);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(1000)) return;
            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapQ"))
                Q.Cast(sender, HitChance.Medium);
            if (W.IsReady() && AutoMenu.CheckBoxValue("GapW"))
                W.Cast(sender);
            if (E.IsReady() && AutoMenu.CheckBoxValue("GapE"))
                E.Cast(sender, HitChance.Medium);
        }

        public override void Active()
        {
        }

        public override void Combo()
        {
            foreach (var spell in SpellList.Where(s => s.IsReady() && ComboMenu.CheckBoxValue(s.Slot)))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range)) return;

                if (spell.Slot == SpellSlot.R)
                {
                    R.CastLineAoE(target, HitChance.Medium, ComboMenu.SliderValue("RAOE"));
                    if (R.WillKill(target))
                    {
                        R.Cast();
                    }
                }
                else
                {
                    var skillshot = spell as Spell.Skillshot;
                    {
                        skillshot.Cast(target, HitChance.Medium);
                    }
                }
            }
        }

        public override void Harass()
        {
            foreach (
                var spell in
                    SpellList.Where(
                        s =>
                            s.IsReady() && HarassMenu.CheckBoxValue(s.Slot) &&
                            HarassMenu.CompareSlider(s.Slot + "mana", user.ManaPercent)))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target == null || !target.IsKillable(spell.Range)) return;

                var skillshot = spell as Spell.Skillshot;
                {
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void LaneClear()
        {
            foreach (
                var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                foreach (var skillshot in SpellList.Where(
                    s =>
                        s.IsReady() && s != R && LaneClearMenu.CheckBoxValue(s.Slot) &&
                        LaneClearMenu.CompareSlider(s.Slot + "mana", user.ManaPercent))
                    .Select(spell => spell as Spell.Skillshot))
                {
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(W.Range)) return;
            if (W.IsReady() && AutoMenu.CheckBoxValue("FleeW") && user.ManaPercent >= 65)
            {
                W.Cast(target);
            }
            if (Q.IsReady() && AutoMenu.CheckBoxValue("FleeQ") && user.ManaPercent >= 65)
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget(Q.Range)))
            {
                Q.Cast(enemy, HitChance.Medium);
            }
            if (!E.IsReady() || !AutoMenu.CheckBoxValue("FleeQ") || !(user.ManaPercent >= 65)) return;
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget(E.Range)))
                {
                    E.Cast(enemy, HitChance.Medium);
                }
            }
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                foreach (var skillshot in SpellList.Where(
                    s =>
                        s.WillKill(target) && s.IsReady() && target.IsKillable(s.Range) &&
                        KillStealMenu.CheckBoxValue(s.Slot)).Select(spell => spell as Spell.Skillshot))
                {
                    skillshot.Cast(target, HitChance.Medium);
                }
            }
        }
    }
}