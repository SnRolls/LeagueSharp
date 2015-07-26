using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = SharpDX.Color;

namespace SNTwitch
{
    
    class Program
    {
        private static Obj_AI_Hero p { get { return ObjectManager.Player; } }
        private static Spell Q, W, E, R, useYumu;
        private static Orbwalking.Orbwalker orb;
        private static Menu m;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        public static void OnLoad(EventArgs args)
        {
            if(p.ChampionName != "Twitch")
                return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 1200, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R);
            useYumu = new Spell(SpellSlot.Q, 1500);

            W.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);

            m = new Menu("SNTwitch by SnRolls", "Twitch", true);

            orb = new Orbwalking.Orbwalker(m.SubMenu("Orbwalker"));
            TargetSelector.AddToMenu(m.SubMenu("Target Selector"));

            m.AddSubMenu(new Menu("Drawings", "drawings", false));
            m.SubMenu("drawings").AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            m.SubMenu("drawings").AddItem(new MenuItem("drawE", "Draw E").SetValue(true));

            m.AddSubMenu(new Menu("Harass", "harass", false));
            m.SubMenu("harass").AddItem(new MenuItem("hUseE", "Use E on 6 stacks").SetValue(true));
            m.SubMenu("harass").AddItem(new MenuItem("hUseW", "Use W on 6 stacks").SetValue(true));
            m.SubMenu("harass").AddItem(new MenuItem("hActive", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));

            m.AddSubMenu(new Menu("Combo", "combo", false));
            m.SubMenu("combo").AddItem(new MenuItem("cUseQ", "Use Q").SetValue(true));
            m.SubMenu("combo").AddItem(new MenuItem("cUseW", "Use W").SetValue(true));
            m.SubMenu("combo").AddItem(new MenuItem("cUseR", "Use R").SetValue(true));
            m.SubMenu("combo").AddItem(new MenuItem("cActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            m.AddSubMenu(new Menu("Lane Clear", "laneclear", false));
            m.SubMenu("laneclear").AddItem(new MenuItem("lcUseW", "Use W").SetValue(true));
            m.SubMenu("laneclear").AddItem(new MenuItem("lcActive", "Lane Clear").SetValue(new KeyBind('V', KeyBindType.Press)));

            m.AddSubMenu(new Menu("Last Hit", "lasthit", false));
            m.SubMenu("lasthit").AddItem(new MenuItem("lhActive", "Last Hit").SetValue(new KeyBind('X', KeyBindType.Press)));

            m.AddSubMenu(new Menu("Misc", "misc", false));
                m.SubMenu("misc").AddItem(new MenuItem("Emobs", "Kill mobs with E").SetValue(new StringList(new [] { "Baron + Dragon + Siege Minion", "Baron + Dragon", "None" })));
            m.SubMenu("misc").AddItem(new MenuItem("au", "Auto Ult").SetValue(true));
            m.SubMenu("misc").AddItem(new MenuItem("eKS", "E Auto-KillSteal").SetValue(true));
            m.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public static void OnUpdate(EventArgs args)
        {
            if (p.IsDead) return;

            if(m.Item("eKS").IsActive())
            CastEKS();
            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget(2500, TargetSelector.DamageType.Physical);
                if(target.IsValidTarget(2500))
                {
                    Q.Cast();
                }

                if (target.IsValidTarget(800))
                {
                    Items.UseItem(3142);
                    if (m.Item("au").IsActive())
                        R.Cast();
                }


                    
                    if (target != null && target.Type == p.Type &&
                    target.ServerPosition.Distance(p.ServerPosition) < 450)
                    {
                        var hasCutGlass = Items.HasItem(3144);
                        var hasBotrk = Items.HasItem(3153);

                        if (hasBotrk || hasCutGlass)
                        {
                            var itemId = hasCutGlass ? 3144 : 3153;
                            var damage = p.GetItemDamage(target, Damage.DamageItems.Botrk);
                            if (hasCutGlass || p.Health + damage < p.MaxHealth)
                                Items.UseItem(itemId, target);
                        }
                    }

                    


                    //if (W.IsReady())
                    //{
                    //    if (W.IsInRange(target))
                     //   {
                    //        CastW();
                    //    }
                   // }

                    foreach (
                        var enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(enemy => enemy.IsValidTarget(E.Range) && E.IsKillable(enemy))
                        )
                    {
                        E.Cast();
                    }
                }
            
            if (orb.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
               if(m.Item("hUseW").IsActive())
               {
                   foreach (
                      var enemy in
                         ObjectManager.Get<Obj_AI_Hero>()
                         .Where(enemy => enemy.IsValidTarget(W.Range) && enemy.GetBuffCount("twitchdeadlyvenom") > 5)
                             )
                   {
                       if((enemy.GetBuff("twitchdeadlyvenom").EndTime-1 < Game.ClockTime))
                       W.Cast(enemy);
                       
                   }
               }

                if(m.Item("hUseE").IsActive())
               {
                   foreach (
                      var enemy in
                         ObjectManager.Get<Obj_AI_Hero>()
                         .Where(enemy => enemy.IsValidTarget(E.Range) && enemy.GetBuffCount("twitchdeadlyvenom") > 5)
                             )
                   {
                       E.Cast();
                       
                   }
               }
            }

            if (orb.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                {
                    var minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly);
                    foreach (var mi in minions)
                    {
                        switch (m.Item("Emobs").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if ((mi.BaseSkinName.Contains("MinionSiege") || mi.BaseSkinName.Contains("Dragon") || mi.BaseSkinName.Contains("Baron")) && E.IsKillable(mi))
                                {
                                    E.Cast();
                                }
                                break;

                            case 1:
                                if ((mi.BaseSkinName.Contains("Dragon") || mi.BaseSkinName.Contains("Baron")) && E.IsKillable(mi))
                                {
                                    E.Cast();
                                }
                                break;

                            case 2:
                                return;
                                break;
                        }
                    }
                }
            
            

        }

        public static void OnDraw(EventArgs args)
        {
            if (!p.IsDead && W.Level > 0 && W.IsReady() && m.Item("drawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(p.Position, W.Range, System.Drawing.Color.DarkGreen);
            }

            if (!p.IsDead && E.Level > 0 && E.IsReady() && m.Item("drawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(p.Position, E.Range, System.Drawing.Color.DarkGreen);
            }

        }

        public static void CastQ()
        {
            Q.Cast();
        }

        public static void CastEKS()
        {
            foreach (
                       var enemy in
                           ObjectManager.Get<Obj_AI_Hero>()
                               .Where(enemy => enemy.IsValidTarget(E.Range) && E.IsKillable(enemy))
                       )
            {
                E.Cast();
            }

        }
        public static void CastW()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.True);

            if(target.IsStunned || target.IsRooted || target.IsCharmed)
            {
                W.Cast(target);
            }
            else
            {
                W.CastIfHitchanceEquals(target, HitChance.High);
            }
        }
    }
}
