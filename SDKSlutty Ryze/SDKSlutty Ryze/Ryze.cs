using System;
using System.Collections.Specialized;
using System.Linq;

using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using System.Drawing;
using System.Windows.Forms;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Utils;
using Color = System.Drawing.Color;
using SharpDX;

using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace SDKSlutty_Ryze
{

    internal class Ryze
    {
        public const string ChampName = "Ryze";
        public const string Menuname = "Slutty Ryze";
        public static Menu Config;
        public static Spell Q, W, E, R, Qn;


        /*
        public static Items.Item HealthPotion = new Items.Item(2003, 0);
        public static Items.Item CrystallineFlask = new Items.Item(2041, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item BiscuitofRejuvenation = new Items.Item(2010, 0);
        public static Items.Item SeraphsEmbrace = new Items.Item(3040, 0);
         */
        // private static SpellSlot Ignite;

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static int[] abilitySequence;
        public static int qOff = 0, wOff = 0, eOff = 0, rOff = 0;

        public static void OnLoad(object sender, EventArgs e)
        {
            if (Player.ChampionName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 865);
            Qn = new Spell(SpellSlot.Q, 865);
            W = new Spell(SpellSlot.W, 585);
            E = new Spell(SpellSlot.E, 585);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.26f, 50f, 1700f, true, SkillshotType.SkillshotLine);
            Qn.SetSkillshot(0.26f, 50f, 1700f, false, SkillshotType.SkillshotLine);

            abilitySequence = new int[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 3, 2, 2, 3, 4, 3, 3};

            Config = new Menu("Slutty Ryze", "Slutty Ryze", true);
            LeagueSharp.SDK.Core.Bootstrap.Init(null); //This will be a loader functionality later on

            var comboMenu = new Menu("combo", "Combo Settings");
            var combodefaultMenu = new Menu("combos", "Spells");
            {
                combodefaultMenu.Add(new MenuBool("useQ", "Use Q", true));
                combodefaultMenu.Add(new MenuBool("useW", "Use W", true));
                combodefaultMenu.Add(new MenuBool("useE", "Use E", true));
                combodefaultMenu.Add(new MenuBool("useR", "Use R"));
                combodefaultMenu.Add(new MenuBool("useRww", "Use R Only when Rooted", true));

                comboMenu.Add(combodefaultMenu);
            }

            var combooptionsMenu = new Menu("combooptions", "Combo Options");
            {
                combooptionsMenu.Add(new MenuBool("AAblock", "Auto Attack Block", true));
                combooptionsMenu.Add(new MenuList<string>("comboooptions", "Combo Mode", new[] { "Stable", "Beta Combo" }));

                comboMenu.Add(combooptionsMenu);
            }


            Config.Add(comboMenu);

            var hybridMenu = new Menu("hybrid", "Hybrid");
            {
                hybridMenu.Add(new MenuBool("nospell", "Don't Proc Passive", true));
                hybridMenu.Add(new MenuBool("useQm", "Use Q", true));
                hybridMenu.Add(new MenuBool("useQml", "Use Q Last Hit", true));
                hybridMenu.Add(new MenuBool("useWm", "Use W", true));
                hybridMenu.Add(new MenuBool("useEm", "Use E", true));
                hybridMenu.Add(new MenuBool("UseQauto", "Auto Q"));
                hybridMenu.Add(new MenuSlider("mMin", "Minimum Mana For Spells", 40));

                Config.Add(hybridMenu);
            }

            var clearMenu = new Menu("laneclear", "Clear");
            var laneMenu = new Menu("laneeclear", "Lane Clear (V)");
            {
                laneMenu.Add(new MenuSlider("minmana", "Minimum Mana For Spells", 40));
                laneMenu.Add(new MenuBool("useq2L", "Use Q", true));
                laneMenu.Add(new MenuBool("usew2L", "Use W", true));
                laneMenu.Add(new MenuBool("usee2L", "Use E", true));
                laneMenu.Add(new MenuBool("user2L", "Use R", true));
                
                laneMenu.Add(new MenuBool("useQlc", "Use Q Last hit", true));
                laneMenu.Add(new MenuBool("useWlc", "Use W Last hit", true));
                laneMenu.Add(new MenuBool("useElc", "Use E Last Hit", true));
                 
               // laneMenu.Add(new MenuSlider("useRl", "Use R When X Minions", 1,3,20));
                laneMenu.Add(new MenuBool("spellblock", "Don't Use Spells when to pop passive"));

                clearMenu.Add(laneMenu);

            }

            var lasthitMenu = new Menu("lasthit", "Last Hit (X)");
            {
                lasthitMenu.Add(new MenuBool("useQ2l", "Use Q Last hit", true));
                lasthitMenu.Add(new MenuBool("useW2l", "Use W Last hit", true));
                lasthitMenu.Add(new MenuBool("useE2l", "Use E Last Hit", true));

                clearMenu.Add(lasthitMenu);
            }

            Config.Add(clearMenu);
            /*
            var potionMenu = new Menu("autoP", "Auto Potions");
            {
                potionMenu.Add(new MenuBool("autoPO", "Auto Health Potion", true));
                potionMenu.Add(new MenuBool("HP", "Auto Health Potion", true));
                potionMenu.Add(new MenuSlider("HPSlider", "Minimum %Health for Potion", 30));

                potionMenu.Add(new MenuBool("Mana", "Auto Mana Potion", true));
                potionMenu.Add(new MenuSlider("MANASlider", "Minimum %Mana for Potion", 30));

                potionMenu.Add(new MenuBool("Biscuit", "Auto Biscuit", true));
                potionMenu.Add(new MenuSlider("bSlider", "Minimum %Health for Biscuit", 30));

                potionMenu.Add(new MenuBool("flask", "Auto Flask", true));
                potionMenu.Add(new MenuSlider("fSlider", "Minimum %Health for flask", 30));
            }
            Config.Add(potionMenu);
            */


            var miscMenu = new Menu("misc", "Miscellaneous");
            {
                miscMenu.Add(new MenuBool("useW2I", "Interrupt With W", true));
                miscMenu.Add(new MenuBool("useQW2D", "Use W/Q On Dashing", true));
                miscMenu.Add(new MenuBool("level", "Auto Skill Level Up", true));
                miscMenu.Add(new MenuBool("autow", "Auto W enemy under turre", true));

                Config.Add(miscMenu);
            }

            /*
            var passiveMenu = new Menu("passive", "Passive Stack");
            {
                passiveMenu.Add(new MenuBool("autoPassive", "Passive Stack"))
            }
             */

            var drawMenu = new Menu("draw", "Drawings");
            {
                drawMenu.Add(new MenuBool("qDraw", "Draw Q", true));
                drawMenu.Add(new MenuBool("wDraw", "Draw W", true));
                drawMenu.Add(new MenuBool("eDraw", "Draw E", true));

                Config.Add(drawMenu);
            }

            Config.Attach();
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
           InterruptableSpell.OnInterruptableTarget += Interruptable;
            // Dash.OnDash += Unit_Ondash;
        }
        /*
        private static void Unit_Ondash(Obj_AI_Base sender, Dash.DashArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var qSpell = Config.Item("useQW2D").GetValue<bool>();

            if (!sender.IsEnemy)
            {
                return;
            }

            if (sender.NetworkId == target.NetworkId)
            {
                if (qSpell)
                {

                    if (Q.IsReady()
                        && args.EndPos.Distance(target) < Q.Range)
                    {
                        var delay = (int)(args.EndTick - Game.Time - Q.Delay - 0.1f);
                        if (delay > 0)
                        {
                            DelayAction.Add(delay * 1000, () => Q.Cast(args.EndPos));
                        }
                        else
                        {
                            Q.Cast(args.EndPos);
                        }
                        if (Q.IsReady()
                            && args.EndPos.Distance(new Vector2 Player.Position, < Q.Range)
                        {
                            if (delay > 0)
                            {
                                DelayAction.Add(delay * 1000, () => Q.Cast(args.EndPos));
                            }
                            else
                            {
                                W.CastOnUnit(target);
                            }
                        }
                    }
                }
            }
        }
         */

        private static double eDamage(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 50, 66, 82, 98, 114 }[E.Level - 1] + 0.3 * Player.FlatMagicDamageMod + 0.02 * Player.Mana);
        }
        private static double qDamage(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 60, 95, 130, 165, 200 }[Q.Level - 1] + 0.55 * Player.FlatMagicDamageMod);
        }

        private static double wDamage(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 80, 100, 120, 140, 160 }[E.Level - 1] + 0.4 * Player.FlatMagicDamageMod);
        }


        private static void Interruptable(object sender, InterruptableSpell.InterruptableTargetEventArgs e)
        {
            var enable = Config["misc"]["useW2I"].GetValue<MenuBool>().Value;
            
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (enable)
            {
                W.CastOnUnit(target);
            }

        }

        private static void LevelUpSpells()
        {
            var enable = Config["misc"]["level"].GetValue<MenuBool>().Value;
            int qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            int wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            int eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            int rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (!enable)
                return;

            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = { 0, 0, 0, 0 };
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[abilitySequence[i] - 1] = level[abilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);

            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            var qDraw = Config["draw"]["qDraw"].GetValue<MenuBool>().Value;
            var wDraw = Config["draw"]["wDraw"].GetValue<MenuBool>().Value;
            var eDraw = Config["draw"]["eDraw"].GetValue<MenuBool>().Value;


            if (qDraw
                && Q.Level > 0)
            {
                Drawing.DrawCircle(Player.Position, Q.Range, Color.Green);
            }
            if (eDraw
                && E.Level > 0)
            {
                Drawing.DrawCircle(Player.Position, E.Range, Color.Gold);
            }
            if (wDraw 
                && W.Level > 0)
            {
                Drawing.DrawCircle(Player.Position, W.Range, Color.Red);
            }
            /*
            var tears = Config.Item("tearS").GetValue<KeyBind>().Active;
            var passive = Config.Item("autoPassive").GetValue<KeyBind>().Active;
            var laneclear = Config.Item("disablelane").GetValue<KeyBind>().Active;

            if (Config.Item("notdraw").GetValue<bool>())
            {
                if (tears)
                {
                    var heroPosition = Drawing.WorldToScreen(Player.Position);
                    var textDimension = Drawing.GetTextExtent("Stunnable!");
                    Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height,
                        Color.DarkGreen, "Tear Stack: On");
                }

                if (passive)
                {
                    var heroPosition = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(heroPosition.X - 150, heroPosition.Y - 30,
                        Color.DarkGreen, "Passive Stack: On");
                }

                if (laneclear)
                {
                    var heroPosition = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(heroPosition.X + 20, heroPosition.Y - 30,
                        Color.DarkGreen, "Lane Clear: On");
                }

                if (!tears)
                {
                    var heroPosition = Drawing.WorldToScreen(Player.Position);
                    var textDimension = Drawing.GetTextExtent("Stunnable!");
                    Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height,
                        Color.Red, "Tear Stack: Off");
                }

                if (!passive)
                {
                    var heroPosition = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(heroPosition.X - 150, heroPosition.Y - 30,
                        Color.Red, "Passive Stack: Off");
                }

                if (!laneclear)
                {
                    var heroPosition = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(heroPosition.X + 20, heroPosition.Y - 30,
                        Color.Red, "Lane Clear: Off");
                }
        }
             */

    

}

        private static void OnUpdate(EventArgs args)
        {
            Orbwalker.Attack = true;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    if (((Player.Distance(target) > 440)
                         || (Q.IsReady() || E.IsReady() || W.IsReady()))
                        && target.Health > (Player.GetAutoAttackDamage(target) * 3))
                    {
                        Orbwalker.Attack = false;
                    }
                    else
                    {
                        Orbwalker.Attack = true;
                    }
                    Combo();
                    break;

                case OrbwalkerMode.LastHit:
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;

                case OrbwalkerMode.Hybrid:
                    Hybrid();
                    break;
            }
            LevelUpSpells();
           // AutoPassive();
            // Potion();
        }

        private static int GetPassiveBuff
        {
            get
            {
                var data = ObjectManager.Player.Buffs.FirstOrDefault(b => b.DisplayName == "RyzePassiveStack");
                return data != null ? data.Count : 0;
            }
        }

        private static void Combo()
        {

            // Ignite = Player.GetSpellSlot("summonerdot");
            var qSpell = Config["combo"]["combos"]["useQ"].GetValue<MenuBool>().Value;
            var eSpell = Config["combo"]["combos"]["useE"].GetValue<MenuBool>().Value;
            var wSpell = Config["combo"]["combos"]["useW"].GetValue<MenuBool>().Value;
            var rSpell = Config["combo"]["combos"]["useR"].GetValue<MenuBool>().Value;
            var rwwSpell = Config["combo"]["combos"]["useRww"].GetValue<MenuBool>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            var combooptions = Config["combo"]["combooptions"]["comboooptions"].GetValue<MenuList>();

            if (!target.IsValidTarget(Q.Range))
            {
                return;
            }


            switch (combooptions.Index)
            {
                case 1:
                    if (R.IsReady())
                    {
                        if (GetPassiveBuff == 1
                            || !Player.HasBuff("RyzePassiveStack"))
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if ((Player.Distance(target) < W.Range)  && (wSpell) && 
                                W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }


                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (rSpell)
                            {
                                if (target.IsValidTarget(W.Range) &&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 2)
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                   )
                                {
                                    if (target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 3)
                        {
                            if (Q.IsReady()
                                && target.IsValidTarget(Q.Range))
                            {
                                {
                                    Qn.Cast(target);
                                }
                            }
                            if (E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                {
                                    W.CastOnUnit(target);
                                }
                            }
                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range) &&
                                     target.Health > qDamage(target) + eDamage(target))
                               {
                                   if (rwwSpell && target.HasBuff("RyzeW")
                                       && (Q.IsReady() || W.IsReady() || E.IsReady()))
                                   {
                                       R.Cast();
                                   }
                                   if (!rwwSpell
                                       && (Q.IsReady() || W.IsReady() || E.IsReady()))
                                   {
                                       R.Cast();
                                   }
                               }
                           }
                       }

                       if (GetPassiveBuff == 4)
                       {
                           if (target.IsValidTarget(W.Range)
                               && wSpell
                               && W.IsReady())
                           {
                               W.CastOnUnit(target);
                           }
                           if (target.IsValidTarget(Qn.Range)
                               && Q.IsReady()
                               && qSpell)
                           {
                               Qn.Cast(target);
                           }
                           if (target.IsValidTarget(E.Range)
                               && E.IsReady()
                               && eSpell)
                           {
                               E.CastOnUnit(target);
                           }

                           if (R.IsReady()
                               && rSpell)
                           {
                               if (target.IsValidTarget(W.Range) &&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                    if (!Q.IsReady() && !W.IsReady() && !E.IsReady())
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (Player.HasBuff("ryzepassivecharged"))
                        {
                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (wSpell
                                && W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                W.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (eSpell
                                && E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range) &&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                    if (!E.IsReady() && !Q.IsReady() && !W.IsReady())
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }
                    }

                    if (!R.IsReady())
                    {
                        if (GetPassiveBuff == 1
                            || !Player.HasBuff("RyzePassiveStack"))
                        {
                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }

                        if (GetPassiveBuff == 2)
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (rSpell)
                            {
                                if (target.IsValidTarget(W.Range) &&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 3)
                        {
                            if (Q.IsReady()
                                && target.IsValidTarget(Q.Range))
                            {
                                {
                                    Qn.Cast(target);
                                }
                            }
                            if (E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                {
                                    W.CastOnUnit(target);
                                }
                            }
                        }

                        if (GetPassiveBuff == 4)
                        {
                            if (target.IsValidTarget(E.Range)
                                && E.IsReady()
                                && eSpell)
                            {
                                E.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (target.IsValidTarget(Qn.Range)
                                && Q.IsReady()
                                && qSpell)
                            {
                                Qn.Cast(target);
                            }
                        }

                        if (Player.HasBuff("ryzepassivecharged"))
                        {
                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (wSpell
                                && W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                W.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (eSpell
                                && E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }
                        }
                    }
                    break;



                case 0:

                    if (target.IsValidTarget(Q.Range))
                    {
                        if (GetPassiveBuff <= 2
                            || !Player.HasBuff("RyzePassiveStack"))
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range) &&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }


                        if (GetPassiveBuff == 3)
                        {
                            if (Q.IsReady()
                                && target.IsValidTarget(Q.Range))
                            {
                                {
                                    Qn.Cast(target);
                                }
                            }
                            if (E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                {
                                    W.CastOnUnit(target);
                                }
                            }
                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)&&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }

                        }

                        if (GetPassiveBuff == 4)
                        {
                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (target.IsValidTarget(Qn.Range)
                                && Q.IsReady()
                                && qSpell)
                            {
                                Qn.Cast(target);
                            }
                            if (target.IsValidTarget(E.Range)
                                && E.IsReady()
                                && eSpell)
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)&&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (Player.HasBuff("ryzepassivecharged"))
                        {
                            if (wSpell
                                && W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                W.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (eSpell
                                && E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)&&
                                     target.Health > qDamage(target) + eDamage(target))
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                    if (!E.IsReady() && !Q.IsReady() && !W.IsReady())
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (wSpell
                            && W.IsReady()
                            && target.IsValidTarget(W.Range))
                        {
                            W.CastOnUnit(target);
                        }

                        if (qSpell
                            && Qn.IsReady()
                            && target.IsValidTarget(Qn.Range))
                        {
                            Qn.Cast(target);
                        }

                        if (eSpell
                            && E.IsReady()
                            && target.IsValidTarget(E.Range))
                        {
                            E.CastOnUnit(target);
                        }

                    }
                    break;
            }

            if (R.IsReady()
            && GetPassiveBuff == 4
            && rSpell)
            {
                if (!Q.IsReady()
                && !W.IsReady()
                && !E.IsReady())
                {
                    R.Cast();
                }
            }
        }

        private static void LaneClear()
        {
            var minions =
                GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Q.Range).ToList();

            if (GetPassiveBuff == 4
                && !Player.HasBuff("RyzeR")
                && Config["laneclear"]["laneeclear"]["spellsblock"].GetValue<MenuBool>().Value)
                return;

            var q2LSpell = Config["laneclear"]["laneeclear"]["useq2L"].GetValue<MenuBool>().Value;
            var e2LSpell = Config["laneclear"]["laneeclear"]["usee2L"].GetValue<MenuBool>().Value;
            var w2LSpell = Config["laneclear"]["laneeclear"]["usew2L"].GetValue<MenuBool>().Value;
            var r2LSpell = Config["laneclear"]["laneeclear"]["user2L"].GetValue<MenuBool>().Value;
            
            var qlchSpell = Config["laneclear"]["laneeclear"]["useQlc"].GetValue<MenuBool>().Value;
            var elchSpell = Config["laneclear"]["laneeclear"]["useElc"].GetValue<MenuBool>().Value;
            var wlchSpell = Config["laneclear"]["laneeclear"]["useWlc"].GetValue<MenuBool>().Value;
             

        //   var rSpell = Config["laneclear"]["laneeclear"]["useRl"].GetValue<MenuSlider>().Value;
            var minMana = Config["laneclear"]["laneeclear"]["minmana"].GetValue<MenuSlider>().Value;


            if (Player.ManaPercent <= minMana)
                return;

            if (!minions.Any())
                return;

            foreach (var minion in minions)
            {
                
                if (qlchSpell
                    && Q.IsReady()
                    && minion.IsValidTarget(Q.Range)
                    && minion.Health <= qDamage(minion))
                {
                    Q.Cast(minion);
                }

                if (wlchSpell
                    && W.IsReady()
                    && minion.IsValidTarget(W.Range)
                    && minion.Health <= wDamage(minion))
                {
                    W.CastOnUnit(minion);
                }

                if (elchSpell
                    && E.IsReady()
                    && minion.IsValidTarget(E.Range)
                    && minion.Health <= eDamage(minion))
                {
                    E.CastOnUnit(minion);
                }
                

                if (q2LSpell
                    && Q.IsReady()
                    && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion);
                }
                if (w2LSpell
                    && W.IsReady()
                    && minion.IsValidTarget(W.Range))
                {
                    W.CastOnUnit(minion);
                }

                if (e2LSpell
                    && E.IsReady()
                    && minion.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(minion);
                }

                if (r2LSpell
                    && R.IsReady()
                    && minion.IsValidTarget(W.Range))
                   // && minions.Count < rSpell)
                {
                    R.Cast();
                }
            }
        }
        /*
        private static void Potion()
        {

            var autoPotion = Config["autoP"]["autoPO"].GetValue<MenuBool>().Value;
            var hPotion = Config["autoP"]["HP"].GetValue<MenuBool>().Value;
            var mPotion = Config["autoP"]["MANA"].GetValue<MenuBool>().Value;
            var bPotion = Config["autoP"]["Biscuit"].GetValue<MenuBool>().Value;
            var fPotion = Config["autoP"]["flask"].GetValue<MenuBool>().Value;
            var pSlider = Config["autoP"]["HPSlider"].GetValue<MenuSlider>().Value;
            var mSlider = Config["autoP"]["MANASlider"].GetValue<MenuSlider>().Value;
            var bSlider = Config["autoP"]["bSlider"].GetValue<MenuSlider>().Value;
            var fSlider = Config["autoP"]["fSlider"].GetValue<MenuSlider>().Value;

            if (Player.IsRecalling() || Player.InFountain())
            {
                return;
            }
            if (!autoPotion)
            {
                return;
            }

            if (hPotion
                && Player.HealthPercent <= pSlider
                && HealthPotion.IsReady
                && !Player.HasBuff("FlaskOfCrystalWater")
                && !Player.HasBuff("ItemCrystalFlask")
                && !Player.HasBuff("RegenerationPotion"))
            {
                HealthPotion.Cast();
            }

            if (mPotion
                && Player.ManaPercent <= mSlider
                && ManaPotion.IsReady
                && !Player.HasBuff("RegenerationPotion")
                && !Player.HasBuff("FlaskOfCrystalWater"))
            {
                ManaPotion.Cast();
            }

            if (bPotion
                && Player.HealthPercent <= bSlider
                && BiscuitofRejuvenation.IsReady
                && !Player.HasBuff("ItemMiniRegenPotion"))
            {
                BiscuitofRejuvenation.Cast();
            }

            if (fPotion
                && Player.HealthPercent <= fSlider
                && CrystallineFlask.IsReady
                && !Player.HasBuff("ItemMiniRegenPotion")
                && !Player.HasBuff("ItemCrystalFlask")
                && !Player.HasBuff("RegenerationPotion")
                && !Player.HasBuff("FlaskOfCrystalWater"))
            {
                CrystallineFlask.Cast();
            }
        }
         */

        private static void Hybrid()
        {
            var qSpell = Config["hybrid"]["useQm"].GetValue<MenuBool>().Value;
            var qlSpell = Config["hybrid"]["useQml"].GetValue<MenuBool>().Value;
            var wSpell = Config["hybrid"]["useWm"].GetValue<MenuBool>().Value;
            var eSpell = Config["hybrid"]["useEm"].GetValue<MenuBool>().Value;

            if (Player.ManaPercent < Config["hybrid"]["mMin"].GetValue<MenuSlider>().Value)
                return;

            Obj_AI_Hero target = TargetSelector.GetTarget(900, DamageType.Magical);

            if (GetPassiveBuff == 4
    && !Player.HasBuff("RyzeR")
    && Config["laneclear"]["laneeclear"]["nospell"].GetValue<MenuBool>().Value)
                return;

            if (qSpell
                && Q.IsReady()
                && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }

            if (wSpell
                && W.IsReady()
                && target.IsValidTarget(W.Range))
            {
                W.CastOnUnit(target);
            }

            if (eSpell
                && E.IsReady()
                && target.IsValidTarget(E.Range))
            {
                E.CastOnUnit(target);
            }

            var minions =
    GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Q.Range).ToList();
            {

                foreach (var minion in minions)
                {
                    if (qlSpell
                        && Q.IsReady())
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        /*
        private static void AutoPassive()
        {

            var minions = MinionManager.GetMinions(
ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy,
MinionOrderTypes.MaxHealth);
            if (Player.Mana < Config.Item("ManapSlider").GetValue<Slider>().Value)
                return;

            if (Player.IsRecalling()
                || minions.Count >= 1)
                return;

            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                return;
            }

            var stackSliders = Config.Item("stackSlider").GetValue<Slider>().Value;
            if (Player.IsRecalling() || Player.InFountain())
            {
                return;
            }

            if (GetPassiveBuff >= stackSliders)
            {
                return;
            }
            if (Environment.TickCount - Q.LastCastAttemptT >= 9000
                && Q.IsReady())
            {
                Q.Cast(Game.CursorPos);
            }

        }       
         */
    }
}
