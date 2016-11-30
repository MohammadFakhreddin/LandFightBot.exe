using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace LandFightBotReborn.Utils
{
    public class Logger
    {
        private const string NEW_LOG = ":";
        private const string END_LOG = "\nEND\n";
        public static void debug(string value)
        {
            if (Constants.IS_DEVELOPMENT_BUILD)
            {
                DateTime localDate = DateTime.Now;
                Console.WriteLine(localDate +NEW_LOG+ value+END_LOG);
            }
        }

        public static void info(string value)
        {
            DateTime localDate = DateTime.Now;
            Console.WriteLine(localDate + NEW_LOG + value+END_LOG);
        }
    }
}
