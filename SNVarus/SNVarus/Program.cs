using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SNVarus
{
    class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Spell q, w, e, r;
        public static Vector2 QCastPos = new Vector2();
        public static Menu menu;
       

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            
            Game.PrintChat("SNVarus 0.3 Loaded");
        }

        private static void Game_OnGameLoad(EventArgs eve)
        {
            if (Player.ChampionName != "Varus")
                return;

            q = new Spell(SpellSlot.Q, 925);
            e = new Spell(SpellSlot.E, 925);

            q.SetSkillshot(0.25f, 70, 1900, false, SkillshotType.SkillshotLine);
            e.SetSkillshot(0.1f, 235, 1500, false, SkillshotType.SkillshotCircle);
            r.SetSkillshot(0.25f, 70, 1450, false, SkillshotType.SkillshotLine);
            q.SetCharged("VarusQ", "VarusQ", 250, 1600, 1.2f);
          
            Game.OnUpdate += Game_OnGameUpdate;

            menu.AddItem(new MenuItem("harass", "Harass", false));
        }

       

        private static void Game_OnGameUpdate(EventArgs eve)
        {

            var target = TargetSelector.GetTarget(e.Range, TargetSelector.DamageType.Physical);

            if (e.IsReady())
                e.CastOnBestTarget();
            


         
        
            

        }
    }
}
