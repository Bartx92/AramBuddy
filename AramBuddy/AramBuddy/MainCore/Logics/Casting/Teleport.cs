using System.Linq;
using AramBuddy.MainCore.Utility;
using EloBuddy;
using EloBuddy.SDK;
using GenesisSpellLibrary;

namespace AramBuddy.MainCore.Logics.Casting
{
    class Teleport
    {
        public static void Cast()
        {
            foreach (var spell in ModesManager.Spelllist.Where(s => s != null && s.IsReady() && s.IsTP()))
            {
                if (spell is Spell.Skillshot && ObjectsManager.ClosestAlly != null && ObjectsManager.ClosestAlly.IsValidTarget(spell.Range) && ObjectsManager.AllySpawn != null && Player.Instance.Distance(ObjectsManager.AllySpawn) < 5000 && ObjectsManager.ClosestAlly.Distance(Player.Instance) > spell.Range - 500 && ObjectsManager.ClosestAlly.Distance(ObjectsManager.AllySpawn) > Player.Instance.Distance(ObjectsManager.AllySpawn))
                {
                    spell.Cast(ObjectsManager.ClosestAlly.PredictPosition().Random());
                }
            }
        }
    }
}
