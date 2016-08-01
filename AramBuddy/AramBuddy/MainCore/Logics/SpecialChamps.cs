using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;

namespace AramBuddy.MainCore.Logics
{
    class SpecialChamps
    {
        public static bool IsCastingImportantSpell;

        public struct ImportantSpells
        {
            public Champion champ;
            public SpellSlot slot;
        }

        public static List<ImportantSpells> Importantspells = new List<ImportantSpells>
        {
            new ImportantSpells {champ = Champion.Jhin, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Xerath, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Katarina, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Velkoz, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Pantheon, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Pantheon, slot = SpellSlot.E},
            new ImportantSpells {champ = Champion.Janna, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.RekSai, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Nunu, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.MissFortune, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Malzahar, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.FiddleSticks, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Caitlyn, slot = SpellSlot.R},
            new ImportantSpells {champ = Champion.Galio, slot = SpellSlot.R}
        };

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnTick += Game_OnTick;
            Player.OnIssueOrder += Player_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && Importantspells.Any(h => h.champ == Player.Instance.Hero && h.slot != args.Slot))
            {
                if (IsCastingImportantSpell)
                {
                    args.Process = false;
                    Logger.Send("Blocked spell - Case Player Channeling important spell " + Player.Instance.Hero, Logger.LogLevel.Info);
                }
            }
        }

        private static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if(!sender.IsMe || !IsCastingImportantSpell) return;

            args.Process = false;
            Logger.Send("Blocked Command - Case Player Channeling important spell " + Player.Instance.Hero, Logger.LogLevel.Info);
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.DisableAttacking = IsCastingImportantSpell;
            Orbwalker.DisableMovement = IsCastingImportantSpell;
            if (!Player.Instance.Spellbook.IsChanneling && !Player.Instance.Spellbook.IsCharging && !Player.Instance.Spellbook.IsCastingSpell && IsCastingImportantSpell)
            {
                IsCastingImportantSpell = false;
                Logger.Send("No Longer Channeling important spell " + Player.Instance.Hero, Logger.LogLevel.Info);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (IsCastingImportantSpell || !sender.IsMe || Importantspells.Any(h => h.champ == Player.Instance.Hero && h.slot != args.Slot)) return;
            IsCastingImportantSpell = true;
            Logger.Send("Player Is Channeling important spell " + Player.Instance.Hero, Logger.LogLevel.Info);
        }
    }
}
