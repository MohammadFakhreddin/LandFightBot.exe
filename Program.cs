using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn
{
    public class Program
    {
        private BotManager botManager ; //for single bot
        private void start()
        {
            //we need the mode 1 for single plager.
            //int mode = 1;
            //mode = Console.Read();
            groupManager(1);
        }

        
        public groupManager(int mode)
        {
            if (mode == 1)
            {
                botManager = new BotManager();
            }
        }

        static void Main(string[] args)
        {
            new Program().start();
        }
    }
}
