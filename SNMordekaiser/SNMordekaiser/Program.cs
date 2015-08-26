using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;


namespace SNMordekaiser
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Spell Q, W, E, R;
        private static Menu m;
        private static Orbwalking.Orbwalker orb;
        private static String champ = "Mordekaiser";
        private static String version = "5.16";
        private static float SlaveDelay = 0;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Mordekaiser")
                return;

            if (!Game.Version.Contains(version))
                Notifications.AddNotification(new Notification("SNMordekaiser isn't updated for " + Game.Version, 1000));
            else
                Game.PrintChat("SNMordekaiser has been loaded.");

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 700); //Check range
            R = new Spell(SpellSlot.R, 650); //Check range

            E.SetSkillshot(0.5f, 15f * 2 * (float)Math.PI / 180, 1500f, false, SkillshotType.SkillshotCone);
            R.SetTargetted(0.5f, 1500f);

            m = new Menu("SNMordekaiser", "Mordekaiser", true);

            InitMenu(m);

            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += OnGameDraw;


        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) // Reduces lag
                return;

            AutoR();

            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
                
               
            }

            else if(orb.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)

            {
                Harass();
            }

            else if(orb.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Laneclear();
            }
        }

        private static void OnGameDraw(EventArgs args)
        {
            if (Player.IsDead) //Don't draw anything if player is dead (reduces lag)
                return;

            if (m.Item("drawE").IsActive())
            Render.Circle.DrawCircle(Player.Position, E.Range, Color.DarkRed);
            if (m.Item("drawR").IsActive())
            Render.Circle.DrawCircle(Player.Position, R.Range, Color.DarkRed);

        }

        private static void InitMenu(Menu m)
        {
            var orbMenu = new Menu("Orbwalker", "orbwalker");
            orb = new Orbwalking.Orbwalker(orbMenu);
            m.AddSubMenu(orbMenu);

            var tsMenu = new Menu("Target Selector", "targetselector");
            TargetSelector.AddToMenu(tsMenu);
            m.AddSubMenu(tsMenu);

            m.AddSubMenu(new Menu("Combo", "combo"));
            m.SubMenu("combo").AddItem(new MenuItem("cUseQ", "Use Q").SetValue(true));
            m.SubMenu("combo").AddItem(new MenuItem("cUseW", "Use W").SetValue(true));
            m.SubMenu("combo").AddItem(new MenuItem("cUseE", "Use E").SetValue(true));
            m.AddSubMenu(new Menu("Harass", "harass"));
            //m.SubMenu("harass").AddItem(new MenuItem("hUseW", "Use W").SetValue(true));
            m.SubMenu("harass").AddItem(new MenuItem("hUseE", "Use E").SetValue(true));
            m.AddSubMenu(new Menu("Laneclear", "laneclear"));
            m.SubMenu("laneclear").AddItem(new MenuItem("lcUseE", "Use E").SetValue(true));
            m.SubMenu("laneclear").AddItem(new MenuItem("lcUseQ", "Use Q").SetValue(true));
            m.AddSubMenu(new Menu("Misc", "misc"));
           // m.SubMenu("misc").AddItem(new MenuItem("wPref", "Use W On").SetValue<string[]>(new string[]{"Least health", "Nearest ally"}));
            m.SubMenu("misc").AddItem(new MenuItem("autoRKillable", "Auto R on Killable").SetValue(true));
           // m.SubMenu("misc").AddItem(new MenuItem("autoRWho", "Don't auto R on").SetValue<string[]>(new string[]{"Enemy1","Enemy2","Enemy3","Enemy4","Enemy5"}));
            m.AddSubMenu(new Menu("Drawings", "drawings"));
            m.SubMenu("drawings").AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
            m.SubMenu("drawings").AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));
           
            m.AddToMainMenu();
        }


        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var aaTarget = TargetSelector.GetTarget(300, TargetSelector.DamageType.Magical);

            if (m.Item("cUseE").IsActive() && E.IsReady() && target.IsValidTarget())
            {
                E.Cast(target);
            }

            if (m.Item("cUseE").IsActive() &&  Q.IsReady() && target.IsValidTarget() && (Player.Distance(aaTarget) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)))
            {
                Q.Cast();
            }

            if(m.Item("cUseW").IsActive() && W.IsReady())
            {
                if (Player.CountEnemiesInRange(W.Range) < 1)
                    return;

                float hp = 0f;
                Obj_AI_Hero ally = Player; //Just to get rid of the "unassigned value" error
                foreach(Obj_AI_Hero a in Player.GetAlliesInRange(2000))
                {
                    if (hp == 0f)
                    {
                        hp = a.Health;
                        continue;
                    }

                    if (a.Health < hp)
                    {
                        hp = a.Health;
                        ally = a;
                    }
                }
                
                if(ally!=Player)
                    if(!Player.HasBuff("mordekaisercreepingdeath"))
                W.Cast(ally);
            }

            var rGhostArea = TargetSelector.GetTarget(1500f, TargetSelector.DamageType.Magical);
            if (MordekaiserHaveSlave && rGhostArea != null && Environment.TickCount >= SlaveDelay && !rGhostArea.UnderTurret())
            {
                R.Cast(rGhostArea);
                SlaveDelay = Environment.TickCount + 1000;
            }else if(MordekaiserHaveSlave && rGhostArea == null && Environment.TickCount >= SlaveDelay)
            {
                R.Cast(Player.Position);
            }

        }

        private static bool MordekaiserHaveSlave
        {
            get { return Player.Spellbook.GetSpell(SpellSlot.R).Name == "mordekaisercotgguide"; }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (m.Item("hUseE").IsActive() && E.IsReady() && target.IsValidTarget())
            {
                E.Cast(target);
            }
        }

        private static void Laneclear()
        {
            if (m.Item("lcUseE").IsActive() && E.IsReady())
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range);
                var minionsE = E.GetCircularFarmLocation(minions, E.Range);
                if (minionsE.MinionsHit < 1 || !E.IsInRange(minionsE.Position.To3D()))
                    return;
                E.Cast(minionsE.Position);
            }
            if(m.Item("lcUseQ").IsActive() && Q.IsReady())
            {
                var minionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.NotAlly);
               
                if(minionsQ != null)
                Q.Cast();
            }
        }

        private static void AutoR()
        {
            if (!m.Item("autoRKillable").IsActive())
                return;
          

            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var ultInitial = 10 + (2.5 * R.Level) + (int)(Player.BaseAbilityDamage / 100) * 2;
            if(R.IsReady() && target.IsValidTarget() && target.HealthPercent <= ultInitial)
            {
                R.Cast(target);
            }
        }
    }
}
