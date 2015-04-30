using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;

namespace SNWarwick
{
    class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Orbwalking.Orbwalker orbwalker;
        private static String champName = "Warwick";
        private static Spell Q, W, E, R;
        private static Menu menu;
        private static float AARange = 100;

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
            menu.SubMenu("drawings").AddItem(new MenuItem("drawE", "Draw E").SetValue(false));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawR", "Draw R").SetValue(true));

            menu.AddSubMenu(new Menu("Harass", "harass", false));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseQ", "Use Q").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hUseW", "Use W").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("hActive","Harass").SetValue(new KeyBind('C', KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Combo", "combo", false));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseQ", "Use Q").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseW", "Use W").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("cUseR", "Use R").SetValue(true));
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
            if (Player.ChampionName != champName)
                return;

            //Custom message
            //Game.PrintChat("SNWarwick v0.1 Loaded");
            Notifications.AddNotification("SNWarwick v0.1 Loaded", 8000);

            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 1200);
            E = new Spell(SpellSlot.E, 1600);
            R = new Spell(SpellSlot.R, 700);

            R.SetTargetted(0.5f, float.MaxValue);

            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += OnGameDraw;
            //Setup our menu
            setupMenu();

        }

        public static void OnGameUpdate(EventArgs args)
        {
            Harass();
            Combo();
            LaneClear();
            LastHit();
        }

        public static void castQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null && target.IsValidTarget() && Q.IsReady())
                Q.Cast(target);
        }

        public static void castW()
        {
            var target = TargetSelector.GetTarget(AARange, TargetSelector.DamageType.Physical);

            if (target != null && target.IsValidTarget() && W.IsReady())
                W.Cast();
            
        }

        public static void castUlt(Boolean killablePriority)
        {
            if (!killablePriority)
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (target != null && target.IsValidTarget() && R.IsReady())
                    R.Cast(target);
            }
            else
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (target.Health <= Player.GetSpellDamage(target, SpellSlot.R) && target != null && target.IsValidTarget() && R.IsReady())
                    R.Cast(target);
            }
        }

        public static void Combo()
        {
            if (menu.Item("cActive").IsActive())
            {
               if(menu.Item("cUseQ").IsActive())
               {
                   castQ();
               }

                if(menu.Item("cUseW").IsActive())
                {
                    castW();
                }
                if(menu.Item("cUseRKillable").IsActive())
                {
                    castUlt(true);
                }
               if (menu.Item("cUseR").IsActive())
               {
                   castUlt(false);
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

            }
        }


        public static void LaneClear()
        {
            if (!menu.Item("lcActive").IsActive())
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            foreach(var target in minions)
            {
                if (menu.Item("lcUseQ").IsActive() && target.IsValidTarget() && Q.IsKillable(target) && Q.IsReady())
                {
                    Q.Cast(target);
                }

            }

            if (menu.Item("lcUseW").IsActive() && minions.Count > 4 && W.IsReady())
            {
                W.Cast(Player);
            }


        }

        public static void LastHit()
        {
            //LastHit Q Support
            if(menu.Item("lhActive").IsActive())
            {
                var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

                foreach (var target in minions)
                {
                    if (menu.Item("lhUseQ").IsActive() && target.IsValidTarget() && Q.IsKillable(target) && Q.IsReady())
                    {
                        Q.Cast(target);
                    }

                }

            }
        }

        public static void OnGameDraw(EventArgs args)
        {
            if(menu.Item("drawQ").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);

            }

            if (menu.Item("drawE").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Red);

            }

            if (menu.Item("drawR").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Red);

            }


        }
    }
}
