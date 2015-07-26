using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;

namespace SNRyze
{
    class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static String Champion = "Ryze";
        private static Spell Q, W, E, R;
        private static Menu menu;
        private static Orbwalking.Orbwalker orbwalker;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void setupMenu()
        {
            menu = new Menu(Player.ChampionName, Player.ChampionName, true);

            Menu ts = new Menu("Target Selector", "ts");
            menu.AddSubMenu(ts);
            TargetSelector.AddToMenu(ts);
            Menu orbwalk = new Menu("Orbwalker", "orbwalk");
            menu.AddSubMenu(orbwalk);
            orbwalker = new Orbwalking.Orbwalker(orbwalk);
            menu.AddSubMenu(new Menu("Drawings", "drawings", false));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawE", "Draw E").SetValue(false));

            menu.AddSubMenu(new Menu("Harass", "harass", false));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseQ", "Use Q").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseW", "Use W").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseE", "Use E").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hActive", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Combo", "combo", false));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseQ", "Use Q").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseW", "Use W").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseE", "Use E").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseRKillable", "Use R When Killable First").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Lane Clear", "laneclear", false));
            menu.SubMenu("laneclear").AddItem(new MenuItem("lcUseQ", "Use Q").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("lcUseW", "Use W").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("lcActive", "Lane Clear").SetValue(new KeyBind('V', KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Last Hit", "lasthit", false));
            menu.SubMenu("lasthit").AddItem(new MenuItem("lhUseQ", "Use Q").SetValue(true));
            menu.SubMenu("lasthit").AddItem(new MenuItem("lhActive", "Last Hit").SetValue(new KeyBind('X', KeyBindType.Press)));

            menu.AddToMainMenu();
        }


        public static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != Champion)
                return;

            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 580f);
            E = new Spell(SpellSlot.E, 580f);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 50f, 1800f, true, SkillshotType.SkillshotLine);
            E.SetTargetted(0.20f, float.MaxValue);

            Game.OnUpdate += OnGameUpdate;

            setupMenu();

        }

        public static void OnGameUpdate(EventArgs args)
        {
            Harass();
            Combo();
        }

        public static void castQ()
        {
            if (!Q.IsReady())
                return;

            var target = TargetSelector.GetTarget(Q.Range-200f, TargetSelector.DamageType.Magical);

            if (target != null && target.IsValidTarget())
            Q.CastIfHitchanceEquals(target, HitChance.Medium);
        }

        public static void castW()
        {
            if (!W.IsReady())
                return;

            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            if (target != null && target.IsValidTarget() && W.IsReady())
                W.Cast(target);

        }

        public static void castE()
        {
            if (!E.IsReady())
                return;

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target != null && target.IsValidTarget() && W.IsReady())
                E.Cast(target);

        }

        public static void Combo()
        {
            if (menu.Item("cActive").IsActive())
            {
                if (menu.Item("cUseQ").IsActive())
                {
                    castQ();
                }

                if (menu.Item("cUseW").IsActive())
                {
                    castW();
                }

                if (menu.Item("cUseE").IsActive())
                {
                    castE();
                }
                

            }
        }

        public static void Harass()
        {
            if (menu.Item("hActive").IsActive())
            {
                if (menu.Item("hUseQ").IsActive())
                {
                    castQ();
                }

                if (menu.Item("hUseW").IsActive())
                {
                    castW();
                }

                if (menu.Item("hUseE").IsActive())
                {
                    castE();
                }
            }
        }

        public static void OnGameDraw(EventArgs args)
        {
            if (menu.Item("drawQ").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);

            }

            if (menu.Item("drawE").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Red);

            }

            if (menu.Item("drawW").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Red);

            }


        }

    }
}
