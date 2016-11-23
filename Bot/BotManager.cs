using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandFightBotReborn.Bot.DataType;
using LandFightBotReborn.DB;
using LandFightBotReborn.Network;
using Newtonsoft.Json;

namespace LandFightBotReborn.Bot
{
    public class BotManager
    {
        public User user;
        public HttpManager www;
        public MultiplayerController socket;

        public BotManager(int gameMode,string username,string password)
        {
            this.user = new User(username,password,UnitManager.getAllUnitFeatures());
            initializeNetwork();
        }

        public BotManager(User user)
        {
            this.user = user;
            initializeNetwork();
        }

        private void initializeNetwork()
        {
            www = new HttpManager(this);
            socket = new MultiplayerController(this,onRcCallBack,onDCCallBack,onGameIsReady,onGameFinished,
                onEnemyDCAccepted,onReconnectComplete,onReconnectFailed,onWeakConnection,false,onRuleReady);
        }

        private void onRuleReady(MultiplayerController.GameInfo info)//TODO must be implemented
        {

        }

        private void onWeakConnection()//TODO must be implemented
        {

        }

        private void onReconnectFailed()//TODO must be implemented
        {

        }

        private void onReconnectComplete(MultiplayerController.ReGameStatus regamestatus)//TODO must be implemented
        {

        }

        private void onEnemyDCAccepted(MultiplayerController.Bounty bounty)//TODO must be implemented
        {

        }

        private void onGameFinished(MultiplayerController.Bounty bounty)//TODO must be implemented
        {

        }

        private void onGameIsReady(bool isready)//TODO must be implemented
        {

        }

        private void onDCCallBack()//TODO must be implemented
        {

        }

        //what to do when oponent did something.
        private int numberOfMapXColumn = 10;
        private int numberOfMapYRow = 10;
        private GameElements gameElements = new GameStatus.GameElements(); //instancing the instance
        GameStatus.GameElements gameStatus = GameStatus.GameElements.instance;

        //FUNCTIONS FOR DOING THINGS

        private void onRcCallBack(string message)
        {
            BotManager.instance.execute(message);
        }

        private void execute(string message)
        {
            string[] splited = message.Split(Strings.serverMessage.opCodes.SEPERATOR);

            //turn finished
            if (splited[0] == Strings.serverMessage.opCodes.TURN_FINISHED)
            {
                string purified = splited[1].Replace("\\", "\"");
                Utils.ReGameStatus syncObj = JsonConvert.DeserializeObject<Utils.ReGameStatus>(purified);
                gameStatus.onReconnectComplete(syncObj);
            }

            //moveUnit
            else if (splited[0] == Strings.serverMessage.opCodes.MOVE_UNIT)
            {
                int x1 = int.Parse(splited[1]);
                int y1 = int.Parse(splited[2]);
                int x2 = int.Parse(splited[3]);
                int y2 = int.Parse(splited[4]);
                Utils.Vector2 mapPos = new Utils.Vector2(x2, y2);
                Utils.Vector2 movingUnitPos = new Utils.Vector2(x1, y1);
                bool isSuccessful = gameStatus.move(movingUnitPos, mapPos);
                //TODO send isSuccessful
            }

            //attackunit
            else if (splited[0] == Strings.serverMessage.opCodes.ATTACK_UNIT)
            {
                int x1 = int.Parse(splited[1]);
                int y1 = int.Parse(splited[2]);
                int x2 = int.Parse(splited[3]);
                int y2 = int.Parse(splited[4]);
                Utils.Vector2 attackerPos = new Utils.Vector2(x1, y1);
                Utils.Vector2 attackedPos = new Utils.Vector2(x2, y2);
                List<Utils.HittedUnits> hittedUnits = new List<Utils.HittedUnits>();
                string rawHits = splited[5];
                if (rawHits != "")
                {
                    string[] parts = rawHits.Split(Strings.serverMessage.opCodes.HITED_UNITS_PARAMS.PART_SEP);
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string[] mems = parts[i].Split(Strings.serverMessage.opCodes.HITED_UNITS_PARAMS.MEM_SEP);
                        int unitId = int.Parse(mems[0]);
                        int damage = int.Parse(mems[1]);
                        hittedUnits.Add(new Utils.HittedUnits(unitId, damage));
                    }
                }

                bool isSuccessful = gameStatus.attack(attackerPos, attackedPos, hittedUnits);
                //TODO sending to logic isSuccessful and attackedPos and HittedUnits
            }

            //create unit
            else if (splited[0] == Strings.serverMessage.opCodes.CREATE_UNIT)
            {
                //TODO just for is aly
                int unitId = int.Parse(splited[1]);
                int x1 = int.Parse(splited[2]);
                int y1 = int.Parse(splited[3]);
                int level = int.Parse(splited[4]);
                int assignedId = int.Parse(splited[5]);
                source.Utils.Vector2 position = new Utils.Vector2(x1, y1);
                bool isAly = false;
                bool isSuccesful = gameStatus.createNewUnit(position, unitId, isAly, level, assignedId);
                //TODO Sending to logic isSuccessful
            }

            //client ready

            //kill unit

            //create failed

            //create accepted

            //start multi as first

            //start multi as second

            //use item

            //force move

            //create failed

            //hit

            //win game

            //draw game

            //loose game

            //leave game

            //server ready

        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //what to do when bot played something.
        private void onSend()
        {

        }

    }
}
