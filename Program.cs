using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandFightBotReborn.Bot;
using LandFightBotReborn.DB;

namespace LandFightBotReborn
{
    public class Program
    {
        private BotManager botManager ; //for single bot
        private SQLManager sqlManager;
        private UnitManager unitManager;
        private UpgradeRuleManager upgradeRule;

        public Program()
        {
            sqlManager = new SQLManager();
            upgradeRule = new UpgradeRuleManager(sqlManager);
            unitManager = new UnitManager(sqlManager,upgradeRule);
            botManager = new BotManager(Constants.gameMode.MULTI_PLAYER,"glad","1234");//TODO We must define player cards
        }

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
