﻿using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using static AramBuddy.Plugins.Activator.Database;

namespace AramBuddy.Plugins.Activator.Items
{
    internal class Offence
    {
        private static List<Item> HPItems = new List<Item>
        {
            Botrk, Cutlass, Hextech_Gunblade
        };

        private static List<Item> DmgItems = new List<Item>
        {
            Hextech_GLP, Hextech_ProtoBelt
        };

        private static List<Item> AAItems = new List<Item>
        {
            Hydra, TitanicHydra, Tiamat, Youmuus
        };

        private static Menu Offen;

        public static void Init()
        {
            try
            {
                Offen = Load.MenuIni.AddSubMenu("Offence Items");
                DmgItems.ForEach(i => Offen.CreateCheckBox(i.Id.ToString(), "Use " + i.Id));
                AAItems.ForEach(i => Offen.CreateCheckBox(i.Id.ToString(), "Use " + i.Id));
                HPItems.ForEach(i => Offen.CreateCheckBox(i.Id.ToString(), "Use " + i.Id));
                HPItems.ForEach(i => Offen.CreateSlider(i.Id + "hp", i.Id + " Use on Health% {0}", 70));
                Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
                Game.OnTick += Game_OnTick;
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Offence Error While Init", ex, Logger.LogLevel.Error);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            try
            {
                foreach (var i in HPItems.Where(a => a.ItemReady(Offen) && Offen.CompareSlider(a.Id + "hp", Player.Instance.HealthPercent)))
                {
                    var target = TargetSelector.GetTarget(i.Range, DamageType.Magical);
                    if (target != null && target.IsKillable(i.Range))
                        i.Cast(target);
                    return;
                }

                foreach (var i in DmgItems.Where(a => a.ItemReady(Offen)))
                {
                    var target = TargetSelector.GetTarget(i.Range, DamageType.Magical);
                    if (target != null && target.IsKillable(i.Range))
                        i.Cast(target);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Offence Error At Game_OnTick", ex, Logger.LogLevel.Error);
            }
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            try
            {
                var hero = target as AIHeroClient;
                if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || hero == null) return;

                foreach (var i in AAItems.Where(a => a.ItemReady(Offen)))
                {
                    i.Cast();
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Send("Activator Offence Error At Orbwalker_OnPostAttack", ex, Logger.LogLevel.Error);
            }
        }
    }
}