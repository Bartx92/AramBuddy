using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.TwistedFate
{
    class TwistedFate : Base
    {
        private static bool Selecting;
        private static int lastcasted;
        private static Spell.Skillshot Q { get; }
        private static Spell.Active W { get; }

        static TwistedFate()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            Q = new Spell.Skillshot(SpellSlot.Q, 1450, SkillShotType.Linear, 0, 1000, 40) { AllowedCollisionCount = int.MaxValue };
            W = new Spell.Active(SpellSlot.W, 750);
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
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.Slot != SpellSlot.W)
                return;
            if (args.SData.Name.Equals("PickACard", StringComparison.CurrentCultureIgnoreCase))
            {
                Selecting = true;
            }
            if (args.SData.Name.Equals("GoldCardLock", StringComparison.CurrentCultureIgnoreCase) || args.SData.Name.Equals("RedCardLock", StringComparison.CurrentCultureIgnoreCase)
                || args.SData.Name.Equals("BlueCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                Selecting = false;
            }
        }

        private static void SetectCard(Obj_AI_Base target)
        {
            if (user.CountEnemiesInRange(1000) > 1 && user.ManaPercent > 10)
            {
                StartSelecting("Gold");
            }
            if (target.CountEnemiesInRange(250) > 1 && user.ManaPercent > 10 && user.HealthPercent > 40)
            {
                StartSelecting("Red");
            }
            if (user.CountEnemiesInRange(1000) <= 1 && user.ManaPercent < 30 && user.HealthPercent > 50)
            {
                StartSelecting("Blue");
            }
        }

        private static void StartSelecting(string str)
        {
            if (W.IsReady() && !Selecting && Core.GameTickCount - lastcasted > 500)
            {
                W.Cast();
                lastcasted = Core.GameTickCount;
            }

            if (!Selecting)
                return;

            if (str.Equals("Gold") && W.Name.Equals("GoldCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                W.Cast();
            }
            if (str.Equals("Red") && W.Name.Equals("RedCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                W.Cast();
            }
            if (str.Equals("Blue") && W.Name.Equals("BlueCardLock", StringComparison.CurrentCultureIgnoreCase))
            {
                W.Cast();
            }
        }

        public override void Active()
        {
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
            if (ComboMenu.CheckBoxValue(W.Slot) && target.IsKillable(W.Range))
            {
                SetectCard(target);
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
            if (HarassMenu.CheckBoxValue(W.Slot) && HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent) && target.IsKillable(W.Range))
            {
                SetectCard(target);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && LaneClearMenu.CheckBoxValue(Q.Slot) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (LaneClearMenu.CheckBoxValue(W.Slot) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent) && target.IsKillable(W.Range))
                {
                    SetectCard(target);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(m => m != null))
            {
                if (Q.IsReady() && KillStealMenu.CheckBoxValue(Q.Slot) && target.IsKillable(Q.Range) && Q.WillKill(target))
                {
                    Q.Cast(target, HitChance.Low);
                }
                if (KillStealMenu.CheckBoxValue(W.Slot) && target.IsKillable(W.Range) && W.WillKill(target))
                {
                    SetectCard(target);
                }
            }
        }
    }
}
