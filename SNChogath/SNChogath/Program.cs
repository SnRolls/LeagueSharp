using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SNChogath
{
    class Program
    {

        private static Obj_AI_Hero myPlayer { get { return ObjectManager.Player; } }
        private static Spell Q, W, E, R, FlashR;
        private static Orbwalking.Orbwalker orb;
        private static Menu menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += onGameLoad;
        }

        private static void onGameLoad(EventArgs args)
        {
            if (myPlayer.ChampionName != "Chogath")
                return;

            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 175);
            FlashR = new Spell(SpellSlot.R, 555);

            Q.SetSkillshot(500f, 175f, 750f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Speed, false, SkillshotType.SkillshotCone);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotLine);
            R.SetTargetted(0.5f, int.MaxValue);

            menu = new Menu(myPlayer.ChampionName, myPlayer.ChampionName, true);

            Menu ts = new Menu("Target Selector", "ts");
            menu.AddSubMenu(ts);

            TargetSelector.AddToMenu(ts);
            Menu orbwalker = new Menu("Orbwalker", "orbwalk");

            menu.AddSubMenu(orbwalker);
            orb = new Orbwalking.Orbwalker(orbwalker);

            menu.AddSubMenu(new Menu("Drawings", "drawings", false));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawR", "Draw R").SetValue(true));

            menu.AddSubMenu(new Menu("Harass", "harass", false));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseQ", "Use Q").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseW", "Use W").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseE", "Use E").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseR", "Use R").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hActive", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Combo", "combo", false));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseQ", "Use Q").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseW", "Use W").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseE", "Use E").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseR", "Use R").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Lane Clear", "laneclear", false));
            menu.SubMenu("laneclear").AddItem(new MenuItem("lcUseQ", "Use Q").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("lcUseW", "Use W").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("lcActive", "Lane Clear").SetValue(new KeyBind('V', KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Last Hit", "lasthit", false));
            menu.SubMenu("lasthit").AddItem(new MenuItem("lhUseQ", "Use Q").SetValue(true));
            menu.SubMenu("lasthit").AddItem(new MenuItem("lhDisableAA", "Disable AA").SetValue(false));
            menu.SubMenu("lasthit").AddItem(new MenuItem("lhActive", "Last Hit").SetValue(new KeyBind('X', KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Misc", "misc", false));

            menu.AddToMainMenu();

            Game.OnUpdate += onGameUpdate;
            Drawing.OnDraw += onDraw;
        }

        private static void onGameUpdate(EventArgs args)
        {
            if (myPlayer.IsDead)
                return;

            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {            
                Combo();

            }

            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
            }

            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                LastHit();
            }




        }

        public static void Combo()
        {
            if (menu.Item("cUseQ").GetValue<bool>())
                CastQ();

            if (menu.Item("cUseW").GetValue<bool>())
                CastW();

        }

        public static void Harass()
        {


        }

        public static void CastQ()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if(target.IsValidTarget())
            {
                Q.CastIfHitchanceEquals(target, HitChance.Medium);
            }

        }

        public static void CastW()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            if (target.IsValidTarget())
            {
                W.Cast(target);
            }

        }

        public static void CastE()
        {


        }

        public static void CastR()
        {


        }

        public static void LaneClear()
        {


        }

        public static void LastHit()
        {

        }

        public static void onDraw(EventArgs args)
        {
            if (menu.Item("drawQ").GetValue<bool>())
                Render.Circle.DrawCircle(myPlayer.Position, Q.Range, System.Drawing.Color.Purple);
            if (menu.Item("drawW").GetValue<bool>())
                Render.Circle.DrawCircle(myPlayer.Position, W.Range, System.Drawing.Color.Purple);
            if (menu.Item("drawE").GetValue<bool>())
                Render.Circle.DrawCircle(myPlayer.Position, E.Range, System.Drawing.Color.Purple);
            if (menu.Item("drawR").GetValue<bool>())
                Render.Circle.DrawCircle(myPlayer.Position, R.Range, System.Drawing.Color.Purple);
        }
    }
}
