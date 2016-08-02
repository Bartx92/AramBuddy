using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.GenesisSpellDatabase;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using GenesisSpellLibrary.Spells;

namespace GenesisSpellLibrary
{
    public static class SpellManager
    {
        static SpellManager()
        {
            try
            {
                CurrentSpells = SpellLibrary.GetSpells(Player.Instance.Hero);
                SpellsDictionary = new Dictionary<AIHeroClient, SpellBase>();
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on initialization of Genesis SpellManager.", ex, Logger.LogLevel.Error);
            }
        }

        public static SpellBase CurrentSpells { get; set; }

        public static Dictionary<AIHeroClient, SpellBase> SpellsDictionary { get; set; }

        public static float GetRange(SpellSlot slot, AIHeroClient sender)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault(x => x.Key == sender).Value.Q.Range;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault(x => x.Key == sender).Value.W.Range;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault(x => x.Key == sender).Value.E.Range;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault(x => x.Key == sender).Value.R.Range;
                default:
                    return 0;
            }
        }

        public static bool DontWaste(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QDontWaste;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WDontWaste;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EDontWaste;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RDontWaste;
                default:
                    return false;
            }
        }

        public static bool IsTP(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QisTP;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WisTP;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EisTP;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RisTP;
                default:
                    return false;
            }
        }

        public static bool IsCC(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QisCC;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WisCC;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EisCC;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RisCC;
                default:
                    return false;
            }
        }

        public static bool IsDangerDash(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QisDangerDash;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WisDangerDash;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EisDangerDash;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RisDangerDash;
                default:
                    return false;
            }
        }

        public static bool IsDash(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QisDash;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WisDash;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EisDash;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RisDash;
                default:
                    return false;
            }
        }

        public static bool IsToggle(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QisToggle;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WisToggle;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EisToggle;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RisToggle;
                default:
                    return false;
            }
        }

        public static bool IsSaver(this Spell.SpellBase spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return SpellsDictionary.FirstOrDefault().Value.QisSaver;
                case SpellSlot.W:
                    return SpellsDictionary.FirstOrDefault().Value.WisSaver;
                case SpellSlot.E:
                    return SpellsDictionary.FirstOrDefault().Value.EisSaver;
                case SpellSlot.R:
                    return SpellsDictionary.FirstOrDefault().Value.RisSaver;
                default:
                    return false;
            }
        }

        public static void Initialize()
        {
            try
            {
                PrepareSpells(Player.Instance);
            }
            catch (Exception ex)
            {
                // Exception has been cought; Notify the user of the error and print the exception to the console
                Logger.Send("Exception occurred on PrepareSpells of Genesis SpellManager.", ex, Logger.LogLevel.Error);
            }
        }

        public static void PrepareSpells(AIHeroClient hero)
        {
            var spells = SpellLibrary.GetSpells(hero.Hero);
            //This only needs to be called once per champion, anymore is a memory leak.
            if (spells != null)
            {
                SpellsDictionary.Add(hero, spells);
            }
        }
    }
}
