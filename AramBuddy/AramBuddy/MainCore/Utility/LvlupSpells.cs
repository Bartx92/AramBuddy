using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;

namespace AramBuddy.MainCore.Utility
{
    internal class LvlupSpells
    {
        internal static void Init()
        {
            LevelSpells();
            Obj_AI_Base.OnLevelUp += Obj_AI_BaseOnOnLevelUp;
        }

        private static void LevelSpells()
        {
            int[] LevelSet = { };

            if (MaxQChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                LevelSet = MaxQSequence;
            }
            if (MaxWChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                LevelSet = MaxWSequence;
            }
            if (MaxEChampions.Any(s => s.Equals(Player.Instance.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                LevelSet = MaxESequence;
            }
            var qL = Player.Instance.Spellbook.GetSpell(SpellSlot.Q).Level;
            var wL = Player.Instance.Spellbook.GetSpell(SpellSlot.W).Level;
            var eL = Player.Instance.Spellbook.GetSpell(SpellSlot.E).Level;
            var rL = Player.Instance.Spellbook.GetSpell(SpellSlot.R).Level;

            var level = new[] { 0, 0, 0, 0 };
            if (qL + wL + eL + rL < Player.Instance.Level)
            {
                for (var i = 0; i < Player.Instance.Level; i++)
                {
                    if (LevelSet != null)
                    {
                        level[LevelSet[i] - 1] = level[LevelSet[i] - 1] + 1;
                    }
                }

                if (qL < level[0])
                {
                    Player.LevelSpell(SpellSlot.Q);
                }
                if (wL < level[1])
                {
                    Player.LevelSpell(SpellSlot.W);
                }
                if (eL < level[2])
                {
                    Player.LevelSpell(SpellSlot.E);
                }
                if (rL < level[3])
                {
                    Player.LevelSpell(SpellSlot.R);
                }
            }
        }

        /// <summary>
        ///     Levels up spells using Obj_AI_Base.OnLevelUp Event.
        /// </summary>
        public static void Obj_AI_BaseOnOnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            LevelSpells();
        }

        /// <summary>
        ///     Maxing Q Sequence.
        /// </summary>
        public static int[] MaxQSequence
        {
            get
            {
                return new[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 3, 2, 3, 4, 3, 3 };
            }
        }

        /// <summary>
        ///     Maxing W Sequence.
        /// </summary>
        public static int[] MaxWSequence
        {
            get
            {
                return new[] { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 3, 1, 3, 4, 3, 3 };
            }
        }

        /// <summary>
        ///     Maxing E Sequence.
        /// </summary>
        public static int[] MaxESequence
        {
            get
            {
                return new[] { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 2, 1, 2, 4, 2, 2 };
            }
        }

        /// <summary>
        ///     Maxing Q Champions.
        /// </summary>
        private static readonly List<string> MaxQChampions = new List<string>
                                                                 {
                                                                     "Ahri", "Akali", "Alistar", "Amumu", "Annie", "Ashe", "Azir", "Blitzcrank", "Bard", "Braum", "Caitlyn", "Cassiopeia", "ChoGath",
                                                                     "Corki", "Darius", "Diana", "DrMundo", "Draven", "Elise", "Ekko", "Evelynn", "Ezreal", "Fiora", "Fizz", "Galio", "Gangplank", "Gnar",
                                                                     "Gragas", "Hecarim", "Heimerdinger", "Illaoi", "Irelia", "Janna", "JarvanIV", "Jax", "Jayce", "Jhin", "Jinx", "Karma", "Karthus",
                                                                     "Kassadin", "Katarina", "Kennen", "KhaZix", "Kindred", "Leblanc", "LeeSin", "Leona", "Lissandra", "Lucian", "Lulu", "Malphite",
                                                                     "Malzahar", "MasterYi", "MissFortune", "Morgana", "Nami", "Nautilus", "Nidalee", "Nocturne", "Olaf", "Orianna", "Pantheon", "Poppy",
                                                                     "Quinn", "Rammus", "RekSai", "Renekton", "Rengar", "Riven", "Rumble", "Ryze", "Sejuani", "Shen", "Singed", "Sion", "Sivir", "Skarner",
                                                                     "Sona", "Soraka", "Swain", "Syndra", "TahmKench", "Taliyah", "Taric", "Teemo", "Thresh", "Tristana", "Trundle", "Tryndamere",
                                                                     "TwistedFate", "Udyr", "Urgot", "Varus", "Veigar", "Vi", "Vladimir", "VelKoz", "Warwick", "MonkeyKing", "Xerath", "XinZhao", "Yasuo",
                                                                     "Yorick", "Zac", "Zed", "Ziggs", "Zilean", "Zyra"
                                                                 };

        /// <summary>
        ///     Maxing W Champions.
        /// </summary>
        private static readonly List<string> MaxWChampions = new List<string> { "AurelionSol", "Brand", "KogMaw", "Malzahar", "Talon", "Vayne", "Volibear" };

        /// <summary>
        ///     Maxing E Champions.
        /// </summary>
        private static readonly List<string> MaxEChampions = new List<string>
                                                                 {
                                                                     "Aatrox", "Anivia", "Cassiopeia", "Fiddlesticks", "Garen", "Kalista", "Kayle", "Lux", "Maokai", "Mordekaiser", "Nasus", "Nunu",
                                                                     "Shaco", "Shyvana", "Twitch", "Viktor"
                                                                 };
    }
}
