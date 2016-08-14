using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions.Kalista
{
    class Kalista : Base
    {
        public static Spell.Skillshot Q { get; }
        public static Spell.Skillshot W { get; }
        public static Spell.Active E { get; }
        public static Spell.Active R { get; }

        static Kalista()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 2100, 80) { AllowedCollisionCount = 0 };
            W = new Spell.Skillshot(SpellSlot.W, 5000, SkillShotType.Circular, 250, 2100, 80);
            E = new Spell.Active(SpellSlot.E, 1000);
            R = new Spell.Active(SpellSlot.R, 1100);
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
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable(R.Range) || !R.IsReady())
                return;

            R.Cast();
        }

        public override void Active()
        {
            var spear = new Item(ItemId.The_Black_Spear);
            var ally = EntityManager.Heroes.Allies.OrderByDescending(a => a.MaxHealth).FirstOrDefault(a => a != null && a.IsValidTarget(600));
            if (ally != null && spear.IsOwned(user) && spear.IsReady())
            {
                spear.Cast(ally);
            }
        }

        public override void Combo()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && ComboMenu.CheckBoxValue(Q.Slot))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (E.IsReady() && EKill(target) && target.IsKillable(E.Range) && ComboMenu.CheckBoxValue(E.Slot))
                {
                    E.Cast();
                }
            }
        }

        public override void Harass()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && HarassMenu.CheckBoxValue(Q.Slot) && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (E.IsReady() && EKill(target) && target.IsKillable(E.Range) && HarassMenu.CheckBoxValue(E.Slot) && HarassMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast();
                }
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(e => e != null))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (E.IsReady() && EKill(target) && target.IsKillable(E.Range) && LaneClearMenu.CheckBoxValue(E.Slot) && LaneClearMenu.CompareSlider(E.Slot + "mana", user.ManaPercent))
                {
                    E.Cast();
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null))
            {
                if (Q.IsReady() && Q.WillKill(target) && target.IsKillable(Q.Range) && KillStealMenu.CheckBoxValue(Q.Slot))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (E.IsReady() && EKill(target) && target.IsKillable(E.Range) && KillStealMenu.CheckBoxValue(E.Slot))
                {
                    E.Cast();
                }
            }
        }

        private static bool EKill(Obj_AI_Base target)
        {
            return EDamage(target) >= Prediction.Health.GetPrediction(target, Game.Ping / 2) && RendCount(target) > 0;
        }

        private static int RendCount(Obj_AI_Base target)
        {
            return target.GetBuffCount("KalistaExpungeMarker");
        }

        private static float RendDamage(int stacks)
        {
            var flatAD = Player.Instance.FlatPhysicalDamageMod;
            var totalAD = Player.Instance.TotalAttackDamage;
            var index = E.Level - 1;
            var Edmg = new float[] { 20, 30, 40, 50, 60 }[index];
            var EdmgPS = new float[] { 10, 14, 19, 25, 32 }[index];
            var EdmgPSM = new[] { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f }[index];
            if (stacks == 0)
            {
                return 0;
            }
            return (((EdmgPS * stacks) + ((EdmgPSM * totalAD) * stacks) + Edmg + (0.6f * flatAD)) + stacks);
        }

        private static float EDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, RendDamage(RendCount(target)));
        }
    }
}
