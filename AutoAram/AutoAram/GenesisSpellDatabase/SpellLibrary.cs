﻿using System;
using EloBuddy;
using GenesisSpellLibrary.Spells;

namespace GenesisSpellLibrary
{
    internal class SpellLibrary
    {
        public static SpellBase GetSpells(Champion heroChampion)
        {
            var championType = Type.GetType("GenesisSpellLibrary.Spells." + heroChampion);
            if (championType != null)
            {
                return Activator.CreateInstance(championType) as SpellBase;
            }

            Chat.Print("<font color='#FF0000'><b>AutoAram ERROR:</b></font> " + heroChampion + " is not supported.");
            return null;
        }

        public static bool IsOnCooldown(AIHeroClient hero, SpellSlot slot)
        {
            if (!hero.Spellbook.GetSpell(slot).IsLearned)
            {
                return true;
            }

            var cooldown = hero.Spellbook.GetSpell(slot).CooldownExpires - Game.Time;
            return cooldown > 0;
        }

        public static void Initialize()
        {
        }
    }
}