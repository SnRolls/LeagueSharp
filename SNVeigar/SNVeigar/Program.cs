using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SNVeigar
{
    class Program
    {
        private static Obj_AI_Hero myPlayer { get { return ObjectManager.Player; } }
        private static Spell Q, W, E, R;
        private static Orbwalking.Orbwalker orb;
        private static Menu menu;

        private static int[] QMana = new []{60, 60, 65, 70, 75, 80};
        private static int[] WMana = new []{70, 70, 80, 90, 100, 110};
        private static int[] EMana = new []{80, 80, 90, 100, 110, 120};
        private static int[] RMana = new[] { 125, 125, 175, 225 };
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += onGameLoad;
        }


        private static void onGameLoad(EventArgs args)
        {
            if (myPlayer.ChampionName != "Veigar")
                return;

            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 650);

            Q.SetSkillshot(250, 70, 2000, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(1350, 225, int.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(500, 80, int.MaxValue, false, SkillshotType.SkillshotCircle);

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
            menu.SubMenu("misc").AddItem(new MenuItem("useEW", "Use W after E").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("useWE", "Use E after W").SetValue(false));
            menu.SubMenu("misc").AddItem(new MenuItem("overkill", "Don't Overkill").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("manaCombo", "Don't use combo if you don't have enough mana").SetValue(true));
            menu.AddToMainMenu();

            Game.OnUpdate += onGameUpdate;
            Drawing.OnDraw += onDraw;
        }

        private static void onGameUpdate(EventArgs args)
        {
            if (myPlayer.IsDead)
                return;

            if(orb.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (menu.Item("manaCombo").GetValue<bool>())
                {
                    if (haveManaCombo())
                        Combo();
                }
                else
                {

                    Combo();

                }
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
          
            if (menu.Item("useWE").GetValue<bool>())
            {
                if (menu.Item("cUseW").GetValue<bool>())
                    CastW();

                if (menu.Item("cUseE").GetValue<bool>())
                    CastE();
            }

            //Default is EW combo
            if (menu.Item("cUseE").GetValue<bool>())
                CastE();

            if (menu.Item("cUseW").GetValue<bool>())
                CastW();

            if (menu.Item("cUseR").GetValue<bool>())
                CastR();

            if (menu.Item("cUseQ").GetValue<bool>())
                CastQ();

        }

        public static void Harass()
        {

            if (menu.Item("useWE").GetValue<bool>())
            {
                if (menu.Item("cUseW").GetValue<bool>())
                    CastW();

                if (menu.Item("cUseE").GetValue<bool>())
                    CastE();
            }

            //Default is EW
            if (menu.Item("cUseE").GetValue<bool>())
                CastE();

            if (menu.Item("cUseW").GetValue<bool>())
                CastW();

            if (menu.Item("hUseQ").GetValue<bool>())
                CastQ();
        }

        public static void LaneClear()
        {
            Vector2[] castPos = new Vector2[1];

            foreach(var target in MinionManager.GetMinions(Q.Range))
            {
                if (Q.IsKillable(target))
                    castPos[0] = target.ServerPosition.To2D();
                
            }

        }

        public static void LastHit()
        {
           foreach(var target in MinionManager.GetMinions(Q.Range))
            {
                if (target.Health <= Q.GetDamage(target))
                {

                    Q.Cast(target);
                }

            }

        }

        public static void CastQ()
        {
            if (!Q.IsReady())
                return;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if(target.IsValidTarget() && Q.IsInRange(target.Position))
            {
                Q.Cast(target);
            }

        }

        public static void CastW()
        {
           if (!W.IsReady())
                return;

            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (E.IsReady())
                    W.Cast(target.Position);
                else
                if(target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                W.Cast(target.Position);
                   
                    //Check if dashing, and if is dashing, cast on dashing end position
                else if(target.IsDashing())
                {
                    W.Cast(target.GetDashInfo().EndPos);
                }
                
            }

        }

     

        public static void CastE()
        {
            if (!E.IsReady())
                return;

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target.IsValidTarget())
            {
                PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
                Vector2 cast = pred.UnitPosition.To2D() -
                                  Vector2.Normalize(pred.UnitPosition.To2D() - myPlayer.Position.To2D()) * E.Width;

                if (pred.Hitchance >= HitChance.High)
                {
                    E.Cast(cast);
                }

            }
        }

        public static void CastR()
        {
            if (!R.IsReady())
                return;

            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if(menu.Item("overkill").GetValue<bool>())
            if (Q.IsReady() && Q.GetDamage(target) >= target.Health) 
                return;

            if (target.IsValidTarget())
            {
                R.Cast(target);
            }
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

        public static bool haveManaCombo()
        {
            var mana = myPlayer.Mana;
            if ((int)mana >= (QMana[Q.Level] + WMana[W.Level] + EMana[E.Level] + RMana[R.Level]))
                return true;

            return false;

        }

    }
}
