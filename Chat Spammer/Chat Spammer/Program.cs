using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace Chat_Spammer
{
    static class Program
    {
        public static Menu m;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CustomEvents.Game.OnGameLoad += onLoad;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }

        public static void onLoad(EventArgs args)
        {
            m = new Menu("Chat Spammer", "Chat Spammer", true);

            m.AddSubMenu(new Menu("Chat spam", "cs", false));
            m.SubMenu("cs").AddItem(new MenuItem("key", "Spam Key").SetValue(new KeyBind('T', KeyBindType.Press)));
            m.SubMenu("cs").AddItem(new MenuItem("who", "Spam Who?").SetValue(new StringList(new [] {"Your Team", "Everyone"})));
            m.SubMenu("cs").AddItem(new MenuItem("howmuch", "How much times?").SetValue(new Slider(10, 0, 100)));
            Game.OnUpdate += onUpdate;
            m.AddToMainMenu();
        }

        public static void onUpdate(EventArgs args)
        {
            if(m.Item("key").IsActive())
            {
                switch(m.Item("who").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        for (int i = 0; i < m.Item("howmuch").GetValue<Slider>().Value; i++)
                        {
                            Game.Say("" + Form1.text);
                        }
                        break;

                    case 1:
                        for (int i = 0; i < m.Item("howmuch").GetValue<Slider>().Value; i++)
                        {
                            Game.Say("/all " + Form1.text);
                        }
                        break;
                }
            }
        }
    }
}
