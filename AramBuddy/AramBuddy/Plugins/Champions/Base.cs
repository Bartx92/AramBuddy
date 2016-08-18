using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;

namespace AramBuddy.Plugins.Champions
{
    public abstract class Base
    {
        public static AIHeroClient user = Player.Instance;
        public static string MenuName = "AB " + user.ChampionName;
        public static readonly List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();
        public static Menu MenuIni, AutoMenu, ComboMenu, HarassMenu, LaneClearMenu, KillStealMenu;
        public abstract void Active();
        public abstract void Combo();
        public abstract void Flee();
        public abstract void Harass();
        public abstract void LaneClear();
        public abstract void KillSteal();

        protected Base()
        {
            Game.OnTick += this.Game_OnTick;
        }

        public virtual void Game_OnTick(System.EventArgs args)
        {
            if (user.IsDead)
                return;

            var activemode = Orbwalker.ActiveModesFlags;
            this.Active();
            this.KillSteal();
            switch (activemode)
            {
                case Orbwalker.ActiveModes.Combo:
                    this.Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    this.Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    this.LaneClear();
                    break;
                case Orbwalker.ActiveModes.Flee:
                    this.Flee();
                    break;
            }
        }
    }
}
