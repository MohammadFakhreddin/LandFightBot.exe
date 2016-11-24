using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LandFightBotReborn.Bot.DataType;
using LandFightBotReborn.DB;
using LandFightBotReborn.Network;
using LandFightBotReborn.Utils;
using LitJson;
using Newtonsoft.Json;

namespace LandFightBotReborn.Bot
{
    public class BotManager
    {
        //private Thread executerBackgroundProcess;
        private int numberOfMapXColumn = 12;
        private int numberOfMapYRow = 6;
        private int maxPower = 3000;//It must be same with server do not change this;
        private int basePowerRegen;//TODO
        private int initialPower;//TODO
        private int deadLanBonusPower;//TODO
        private int turnTime;
        private int maxTurn;

        private GameStatus gameStatus; //instancing the instance
        public User user;
        public HttpManager www;
        public MultiplayerController socket;
        private string enemyName;
        private Queue<String> recievedMessages;
        private int bonusTime = 0;
        /// <summary>
        /// Dirty code for finding attacker
        /// </summary>
        private UnitController lastAttacker;

        private GameNetworkController networkController;

        public BotManager(int gameMode,string username,string password)
        {
            this.user = new User(gameMode,username,password,UnitManager.getAllUnitFeatures());
            initializeNetwork();
            initializeGame();
        }

        public BotManager(int gameMode,User user)
        {
            this.user = user;
            initializeNetwork();
            initializeGame();
        }

        private void initializeNetwork()
        {
            www = new HttpManager(this);
            networkController = new GameNetworkController(this);

        }

        private void onRcCallBack(string message)
        {
            execute(message);
        }

        private class GameNetworkController {
            private bool isConnected = true;
            private long playerMessageNum;
            private long opponentMessageNum;
            private BotManager parent;
            public GameNetworkController(BotManager parent)
            {
                isConnected = false;
                playerMessageNum = 0;
                opponentMessageNum = -1;
                this.parent = parent;
                if (parent.user.getGameMode() == Constants.gameMode.MULTI_PLAYER)
                {
                    parent.socket = new MultiplayerController(parent,onRcCallBack,onDcCallBack,onGameIsReady,onEndGame,
                        null,parent.onReconnectComplete,null,null,false,null);
                }
//                else if(parent.user.getGameMode() == Constants.gameMode.RECONNECT_FOR_START)
//                {
//                    parent.socket = new MultiplayerController(this,onRcCallBack,onDCCallBack,onGameIsReady,onGameFinished,
//                        onEnemyDCAccepted,onReconnectComplete,onReconnectFailed,onWeakConnection,false,onRuleReady);
//                }
            }

//            public  recconnectForExistingGame()
//            {
//                while (MultiplayerController.instance == null )
//                {
//                    yield return new  WaitForEndOfFrame();
//                }
//                MultiplayerController.instance.onLoadForExistingGameComplete(onRcCallBack, onDcCallBack, onGameIsReady, onEndGame, onEnemySuddenDC,GameManager.instance.onReconnectComplete,GameManager.instance.onReconnectFailed,
//                    GameManager.instance.setUpGameRules,GameManager.instance.onWeakConnection);
//                UserManager.instance.user.setGameMode(Strings.gameMode.MULTI_PLAYER);
//                gameMode = Strings.gameMode.MULTI_PLAYER;
//                GameManager.instance.gameMode = Strings.gameMode.MULTI_PLAYER;
//            }
            /// <summary>
            /// onRecieveCallBack
            /// </summary>
            /// <param name="message"></param>
            public void onRcCallBack(string message)
            {
                Console.WriteLine("Recieved new message:"+message);
                if (parent.user.getGameMode() == Constants.gameMode.MULTI_PLAYER)
                {
                    string[] parts = message.Split(Constants.serverMessage.opCodes.ORDER_ID_SEPERATOR);
                    if (parts.Length == 2)//It is from player
                    {
                        long newOrderId = long.Parse(parts[0]);
                        if (newOrderId > opponentMessageNum)//If this is a new order otherwise order would not be executed and it is duplicate
                        {
                            opponentMessageNum = newOrderId;
                            parent.execute(parts[1]);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else//It is from server
                    {
                        parent.execute(message);
                    }
                }
                else
                {
                    parent.execute(message);
                }
            }
            /// <summary>
            /// onDisconnectCallBack
            /// </summary>
            public void onDcCallBack()
            {
                parent.onDCCallBack();
            }

            /// <summary>
            /// onSendCallBack
            /// </summary>
            public void onSCallBack() {
            }

            public void send(string message) {
                Console.WriteLine("Sending:" + message);
                string[] splited = message.Split(Constants.serverMessage.opCodes.SEPERATOR);
                parent.socket.sendOrder(playerMessageNum.ToString() + "" + Constants.serverMessage.opCodes.ORDER_ID_SEPERATOR.ToString() + "" + message);
                playerMessageNum++;
            }

            public void setConnectionState(bool isConnected)
            {
                this.isConnected = isConnected;
            }

            public bool getConnectionState()
            {
                return isConnected;
            }

            private void onGameIsReady(bool isMyTurn)
            {
                if (isMyTurn)
                {
                    parent.execute(Constants.serverMessage.opCodes.START_MULTI_AS_FIRST);
                }
                else
                {
                    parent.execute(Constants.serverMessage.opCodes.START_MULTI_AS_SECOND);
                }
            }

//            public void timeOutHandler()
//            {
//                if (gameMode != Strings.gameMode.MULTI_PLAYER)
//                {
//                    send(Strings.serverMessage.opCodes.LEAVE_GAME);
//                    UXPanelsManager.instance.showNetworkErrorDialog(Errors.TIME_OUT[0]);
//                }
//                else
//                {
//                    UnityEngine.Debug.Log("Sending enemy_dc time out");
//                    MultiplayerController.instance.enemyDC(onEnemyDCAccepted,onPlayerIsDC);
//                }
//            }

//            public void onEnemyDCAccepted(MultiplayerController.Bounty bounty)
//            {
//                UnityEngine.Debug.Log("On enemy dc accepted");
//                UserManager.instance.user.updateUserStatistics(bounty);
//                MultiplayerController.instance.disconnectFromServer();
//            }

//            private void onEnemySuddenDC(MultiplayerController.Bounty bounty)
//            {
//                string message = Strings.serverMessage.opCodes.LEAVE_GAME;
//                GameManager.instance.execute(message);
//                onEnemyDCAccepted(bounty);
//            }

//            public void onPlayerIsDC()
//            {
//                UXPanelsManager.instance.showNetworkErrorDialog(Errors.TIME_OUT[0]);
//                MultiplayerController.instance.disconnectFromServer();
//            }

            private void onEndGame(MultiplayerController.Bounty bounty)
            {
                Console.WriteLine("Game is finished\nBounty is: "+bounty.gold.ToString()+":"+bounty.xp.ToString());
                if (bounty.state == Constants.serverMessage.events.endGameWinLooseCodes.WIN)
                {
                    parent.endGame(true, false, false, false,bounty);
                }
                else if (bounty.state == Constants.serverMessage.events.endGameWinLooseCodes.DRAW)
                {
                    parent.endGame(false, false, true, false,bounty);
                }
                else if (bounty.state == Constants.serverMessage.events.endGameWinLooseCodes.LOOSE)
                {
                    parent.endGame(false, false, false, false,bounty);

                }
                else if (bounty.state == Constants.serverMessage.events.endGameWinLooseCodes.LEAVE)
                {
                   parent.endGame(false, false, false, false,bounty);
                }
                else
                {
                    Console.WriteLine("Invalid state");
                }
//                UserManager.instance.user.deleteTokens();
//                parent.user.updateUserStatistics(bounty);
                parent.socket.disconnectFromServer();
            }

//            public void notifyServerPlayerRecComplete()
//            {
//                MultiplayerController.instance.notifyServerPlayerRecComplete();
//            }
            public void notifyServerPlayerRecComplete()
            {
                parent.socket.notifyServerPlayerRecComplete();
            }
        }

        public void initializeGame()
        {

            gameStatus = new GameStatus();

            gameStatus.gameStarted = false;

            gameStatus.unitMap = new UnitController[numberOfMapXColumn][];
            for (int i = 0; i < gameStatus.unitMap.Length; i++)
            {
                gameStatus.unitMap[i] = new UnitController[numberOfMapYRow];
                for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                {
                    gameStatus.unitMap[i][j] = null;
                }
            }
            gameStatus.myPowerRegen = basePowerRegen;
            gameStatus.enemyPowerRegen = basePowerRegen;
            gameStatus.myPower = initialPower;
            gameStatus.enemyPower = initialPower;
            gameStatus.turn = 0;
//            if (gameMode != Strings.gameMode.SINGLE_PLAYER)
//            {
//                if (user.getGameMode() == Constants.gameMode.MULTI_PLAYER)
//                {
                    setUpGameRules(user.getGameInfo());
//                }
//                else if (user.getGameMode() == Constants.gameMode.RECONNECT_FOR_START)
//                {
//                    enemyName = user.getEnemyName();
//                }
                //networkController = new GameNetworkController(..gameMode);
                //UXPanelsManager.instance.setUserEnemyNames(UserManager.instance.user.username.faConvert(), enemyName.faConvert());
                /**
                 *In multiplayer we need some of the scripts to work all the time till game is working
                 *to send and recive all the things till player is playing
                 */
//            startMultiplayerStartGameAnimPartOne();
//            }
//            else
//            {
//                UXPanelsManager.instance.setMaxPower(maxPower);
//                UXPanelsManager.instance.setPower(gameStatus.myPower);
//                UXPanelsManager.instance.setPowerRegen(gameStatus.myPowerRegen);
//                UXPanelsManager.instance.setUserEnemyNames(UserManager.instance.user.username.faConvert(), "Unknow");
//                startNextTurnAnim();
//            }
            //fillHasAbilityUnits();
            //fillHasInTimeAbilityUnits();
            //}
            //catch (Exception e)
            //{
            //    UnityEngine.Debug.LogError(e);
            //}
        }

//        // Update is called once per frame
//        protected override void Update()
//        {
//            base.Update();
//            if (timer != null && timer.IsRunning && !endTurn)
//            {
//                if (timer.ElapsedMilliseconds + bonusTime >= turnTime)
//                {
//                    if (UXPanelsManager.instance != null) {
//                        UXPanelsManager.instance.timerHandlerScript.resetTimer ();
//                    }
//                    if (myTurn && endTurn == false && gameMode != Strings.gameMode.MULTI_PLAYER)
//                    {
//                        stopTimer();
//                        endGameTurn();
//                    }
//                    else if (gameMode == Strings.gameMode.SINGLE_PLAYER)
//                    {
//                        endGameTurn();
//                    }
//                    else if (gameMode == Strings.gameMode.LOCAL_GAME_CLIENT ||
//                             gameMode == Strings.gameMode.LOCAL_GAME_SERVER ||
//                             gameMode == Strings.gameMode.MULTI_PLAYER)
//                    {
//                        float timeOutTime = timer.ElapsedMilliseconds - turnTime + bonusTime;
//                        if (timeOutTime > Strings.LOCAL_GAME_TIME_OUT_MILSECONDS)
//                        {
//                            //networkController.timeOutHandler();
//                        }
//                    }
//                }
//                else
//                {
//                    double elapsedTime = timer.Elapsed.TotalMilliseconds + bonusTime;
//                    UXPanelsManager.instance.timerHandlerScript.setTimerFill((float)elapsedTime);
//                }
//            }
//        }

        private void setUpGameRules(MultiplayerController.GameInfo info)
        {
            Console.WriteLine("Getting game rules");
            maxPower = info.maxPower;
            initialPower = info.initialPower;
            maxTurn = info.maxTurn;
            turnTime = info.turnTime;
            enemyName = info.enemyName;
            Console.WriteLine("Enemy Name is :  " + enemyName);
            basePowerRegen = info.basePowerRegen;
            deadLanBonusPower = info.landBonus;
            gameStatus.myPower = initialPower;
            gameStatus.enemyPower = initialPower;
            gameStatus.myPowerRegen = basePowerRegen;
            gameStatus.enemyPowerRegen = basePowerRegen;
        }

//        public void onReconnectFailed()//TODO Implment later
//        {
//            Console.WriteLine("Failed to reconnect");
//            //user.deleteTokens();
//            restartGame();
//        }

        /// <summary>
        /// It is just a single animation if you want to finish turn call endGameTurn
        /// If it's current player turn and want to play just endGameTurn you must call this
        /// NOTE:Timer is attached too so you do not need to call it
        /// </summary>
        private void startNextImplemention() {
            List<UnitController> unitList = new List<UnitController>();
            for(int i=0;i<gameStatus.unitMap.Length;i++){
                for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                {
                    if (gameStatus.unitMap[i][j] != null && !unitList.Contains(gameStatus.unitMap[i][j]))
                    {
                        unitList.Add(gameStatus.unitMap[i][j]);
                        if (gameStatus.unitMap[i][j].getIsAly() != gameStatus.myTurn)
                        {
                            gameStatus.unitMap[i][j].notifyEndOfTurn();
                        }
                    }
                }
            }
        }

//        private void multiAnimPartTwoInvoker() {
//            startMultiplayerStartGameAnimPartTwo(gameStatus.firstTurnIsMine);
//        }

        private void startMultiplayerStartGameAnimPartTwo(bool myTurn)//Means connection is ready to start the game//TODO Change this
        {
            gameStatus.myTurn = myTurn;
        }

        private void endGameTurn() {
            resetShotsInTurn ();
            gameStatus.turn++;
            gameStatus.myTurn = !gameStatus.myTurn;
            if (gameStatus.myTurn) {
                //regeneration
                if (gameStatus.enemyPower + gameStatus.enemyPowerRegen <= maxPower) {
                    gameStatus.enemyPower += gameStatus.enemyPowerRegen;
                }
                else {
                    gameStatus.enemyPower = maxPower;
                }
            }
            else {
                if (gameStatus.myPower + gameStatus.myPowerRegen <= maxPower) {
                    gameStatus.myPower += gameStatus.myPowerRegen;
                }
                else {
                    gameStatus.myPower = maxPower;
                }
            }
            bonusTime = 0;
            //changeHealthbarStatus();
//            if (gameMode != Strings.gameMode.MULTI_PLAYER)
//            {
//                if (gameStatus.turn >= maxTurn)
//                {
//                    if (numberOfMapXColumn - 1 - gameStatus.enemyLandStartX
//                        > gameStatus.alyLandEndX)
//                    {
//                        endGame(false, false, false, false,null);
//                        if (gameMode != Strings.gameMode.SINGLE_PLAYER)
//                        {
//                            networkController.send(Strings.serverMessage.opCodes.LOOSE_GAME);
//                        }
//                    }
//                    else if (numberOfMapXColumn - 1 - gameStatus.enemyLandStartX < gameStatus.alyLandEndX)
//                    {
//                        endGame(true, false, false, false,null);
//                        if (gameMode != Strings.gameMode.SINGLE_PLAYER)
//                        {
//                            networkController.send(Strings.serverMessage.opCodes.WIN_GAME);
//                        }
//                    }
//                    else
//                    {
//                        endGame(false, false, true, false,null);
//                        if (gameMode != Strings.gameMode.SINGLE_PLAYER)
//                        {
//                            networkController.send(Strings.serverMessage.opCodes.DRAW_GAME);
//                        }
//                    }
//                }
//            }
            bonusTime = 0;
            startNextImplemention();
        }

        private List<HittedUnits> serverHittedUnits;
        private void attack(UnitController attacker,Vector2 attackPosition,List<HittedUnits> hittedUnits)
        {
            this.serverHittedUnits = hittedUnits;
            int vision = attacker.getFeatures().vision;
            powerManager(gameStatus.myTurn, attacker.getFeatures().powerAttack);
//                    if (networkController != null && myTurn && Constants.gameMode != Strings.gameMode.MULTI_PLAYER)
//                    {
//                        string message = Strings.serverMessage.opCodes.ATTACK_UNIT + Strings.serverMessage.opCodes.SEPERATOR +
//                                         attacker.gameMapPosition.x + Strings.serverMessage.opCodes.SEPERATOR +
//                                         attacker.gameMapPosition.y + Strings.serverMessage.opCodes.SEPERATOR +
//                                         (int)attackPosition.x + Strings.serverMessage.opCodes.SEPERATOR + (int)attackPosition.y;
//                        networkController.send(message);
//                    }
//                    if ((myTurn) || Constants.gameMode == Strings.gameMode.SINGLE_PLAYER)
//                    {
//                        setRedHighlightAttackActive(selectedAlyUnit, attackPosition, attackPosition);
//                    }
//                    if ((!myTurn) || Constants.gameMode == Strings.gameMode.SINGLE_PLAYER)
//                    {
//                        if (attacker.getFeatures().id == Strings.unitIds.BOMBI)
//                        {
//                            redHighlights[(int)attacker.gameMapPosition.x][(int)attacker.gameMapPosition.y].SetActive(false);
//                        }
//                        else
//                        {
//                            redHighlights[(int)attacker.gameMapPosition.x][(int)attacker.gameMapPosition.y].SetActive(true);
//                        }
//                    }
            if (user.getGameMode() == Constants.gameMode.MULTI_PLAYER)
            {
                lastAttacker = attacker;
                attacker.attack(attackPosition, false, null);
            }
//                    else
//                    {
//                        attacker.attack(attackPosition, false, onAttackAnimationFinshed);
//                    }
//                }
        }

        private void hitUnits(List<HittedUnits> hittedUnits,Vector2 targetPos,UnitController doer)
        {
            for (int i = 0; i < gameStatus.unitMap.Length; i++)
            {
                for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                {
                    if (gameStatus.unitMap[i][j] != null)
                    {
                        for (int k = 0; k < hittedUnits.Count; k++)
                        {
                            if (hittedUnits[k].assignedId == gameStatus.unitMap[i][j].getAssignedId())
                            {
                                int damage = hittedUnits[k].damage;
                                gameStatus.unitMap[i][j].hit(damage);
                                gameStatus.unitMap[i][j].addAbility(new UnitController.Ability(doer));
                                hittedUnits.RemoveAt(k);
                                break;
                            }
                        }
                    }
                }
            }
            notifyUnitIsDead(null, false, targetPos, true);
        }

        private bool checkForMapLoss(Vector2 position, bool isInAlyTeretory) {
            bool unitExist = false;
            int endOfLand = gameStatus.alyLandEndX;
            if (!isInAlyTeretory) {
                endOfLand = (int)position.x;
            }
            int startOfLand = (int)position.x;
            if (!isInAlyTeretory) {
                startOfLand = gameStatus.enemyLandStartX;
            }
            for (int i = startOfLand; i <= endOfLand; i++) {
                for (int j = 0; j < gameStatus.unitMap[i].Length; j++) {
                    if (gameStatus.unitMap[i][j] != null && gameStatus.unitMap[i][j].getFeatures().health != 0) {
                        unitExist = true;
                        break;
                    }
                }
                if (unitExist) {
                    break;
                }
            }
            if (!unitExist) {
                if (isInAlyTeretory)
                {
                    //Instantiate(cameraShakeAnimationPlayer);
                    updateAlyLandEndX((int)position.x);
                    if (gameStatus.alyLandEndX <= 0)
                    {
                        return true;
                    }
                }
                else
                {
                    //Instantiate(cameraShakeAnimationPlayer);
                    updateEnemyLandStartX((int)position.x);
                    if (gameStatus.enemyLandStartX >= numberOfMapXColumn - 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// For splash damage checks if it is in game bounds
        /// Or AnyThing else
        /// for checking 2radius!
        /// </summary>
        private bool locationIsValid(int x, int y, bool inEnemyLand) {
            if ((gameStatus.myTurn && inEnemyLand) || (!gameStatus.myTurn && !inEnemyLand)) {
                if (x >= numberOfMapXColumn) return false;
                if (y < 0) return false;
                if (x < numberOfMapXColumn / 2) return false;
                if (y >= numberOfMapYRow) return false;
            }
            else if((!gameStatus.myTurn && inEnemyLand) || (gameStatus.myTurn && !inEnemyLand)) {
                if (x >= numberOfMapXColumn / 2) return false;
                if (y < 0) return false;
                if (x < 0) return false;
                if (y >= numberOfMapYRow) return false;
            }
            return true;
        }

        /// <summary>
        /// For Ability radius check
        /// </summary>
        private bool radiusLocationIsValid(int x, int y, bool isAly) {
            if (!isAly)
            {
                if (x >= numberOfMapXColumn) return false;
                if (y < 0) return false;
                if (x < numberOfMapXColumn / 2) return false;
                if (y >= numberOfMapYRow) return false;
            }
            else
            {
                if (x >= numberOfMapXColumn / 2) return false;
                if (y < 0) return false;
                if (x < 0) return false;
                if (y >= numberOfMapYRow) return false;
            }
            //  UnityEngine.Debug.Log("Checking location isValid for " + x + ":" + y + "suc");
            return true;
        }

        private void createNewUnit(UnitFeatures features,Vector2 gameMapPosition, bool isAly,int level)
        {
            createNewUnit(features,gameMapPosition,isAly,level,-1);
        }

        private void createNewUnit(UnitFeatures features, Vector2 gameMapPosition,
            bool isAly,int level,int assignedId) {
            if (features.width > 1) {
                bool positionChanged = false;
                positionChanged = true;
                if (gameMapPosition.x == 0) {
                    gameMapPosition.x = 1;
                }
                else if (gameMapPosition.x == numberOfMapXColumn - 1) {
                    positionChanged = true;
                    gameMapPosition.x = numberOfMapXColumn - 2;
                }
                if (positionChanged) {
                    float startX = getAlyStartX();
                    if (!isAly) {
                        startX = getEnemyStartX();
                    }
                }
            }
            int x = (int)gameMapPosition.x;
            if (features.width > 1)
            {
                bool positionChanged = false;
                if(gameMapPosition.x==0)
                {
                    positionChanged = true;
                    gameMapPosition.x = 1;
                }else if(gameMapPosition.x == numberOfMapXColumn-1)
                {
                    positionChanged = true;
                    gameMapPosition.x = numberOfMapXColumn - 2;
                }
            }
            if (!checkUnitTail(null, features.width, gameMapPosition, isAly, out x)) {
                return;
            }
//            if (isAly) {
//                if (networkController != null && isUserOrder && Constants.gameMode!=Strings.gameMode.MULTI_PLAYER) {
//                    networkController.send(Strings.serverMessage.opCodes.CREATE_UNIT + Strings.serverMessage.opCodes.SEPERATOR + features.id + Strings.serverMessage.opCodes.SEPERATOR + gameMapPosition.x + Strings.serverMessage.opCodes.SEPERATOR + gameMapPosition.y + Strings.serverMessage.opCodes.SEPERATOR + level);
//                }
//            }
//            else {
//            }
            UnitFeatures newUnitFeatures = features.clone();//This part breaks unit refrences to the same feature
            UnitController controller = new UnitController(newUnitFeatures, gameMapPosition, isAly,
                (int)gameMapPosition.x, (int)gameMapPosition.y,level,assignedId);
            gameStatus.unitMap[(int)gameMapPosition.x][(int)gameMapPosition.y] = controller;
            //checkForAddOrRemoveAbility((int)gameMapPosition.x, (int)gameMapPosition.y, true);
            //if (features.width > 1) {
            //    gameStatus.unitMap[x][(int)gameMapPosition.y] = controller;
            //    if (!isAly)
            //    {
            //        checkForAddOrRemoveAbility((int)gameMapPosition.x + 1, (int)gameMapPosition.y, true);
            //    }
            //    else
            //    {
            //        checkForAddOrRemoveAbility((int)gameMapPosition.x - 1, (int)gameMapPosition.y, true);
            //    }
            //}
            checkLocForAddOrRemoveAbil(controller, true);
            float powerRegen = newUnitFeatures.powerRegen;
            if (gameStatus.myTurn) {
                powerRegen += gameStatus.myPowerRegen;
                gameStatus.myPowerRegen = (int)powerRegen;
            }
            else {
                powerRegen += gameStatus.enemyPowerRegen;
                gameStatus.enemyPowerRegen = (int)powerRegen;
            }
            powerManager(gameStatus.myTurn, features.powerSpawn);
        }

        /// <summary>
        /// It checks if moving is possible or not
        /// </summary>
        /// <param name="unit">Unit which is going to move</param>
        /// <param name="coordinates">Coordiante of move place</param>
        /// <param name="gameMapPosition">The position and x y of units</param>
        /// <param name="neededMovePower">The unit needs for moveing</param>
        /// <param name="onMoveAnimationComplete">The callback will be called when unit moving is complete</param>
        /// <returns>
        /// Move is Possible or not
        /// </returns>
        private bool checkMoveIsPossible(UnitController unit,Vector2 gameMapPosition, int neededMovePower)
        {
            UnitFeatures features = unit.getFeatures();
            if (features.width > 1)
            {
                bool positionChanged = false;
                if (gameMapPosition.x == 0)
                {
                    positionChanged = true;
                    gameMapPosition.x = 1;
                }
                else if (gameMapPosition.x == numberOfMapXColumn - 1)
                {
                    positionChanged = true;
                    gameMapPosition.x = numberOfMapXColumn - 2;
                }
            }
            int x;
            if (!checkUnitTail(unit, unit.getFeatures().width, gameMapPosition, gameStatus.myTurn, out x)) {
                return false;
            }
            return true;
        }

        private void moveUnit(UnitController unit,Vector2 gameMapPosition, int neededMovePower) {
            UnitFeatures features = unit.getFeatures();
            int x;
            if (!checkUnitTail(unit, unit.getFeatures().width, gameMapPosition, myTurn, out x)) {
                return;
            }
            checkLocForAddOrRemoveAbil(unit, false);
            unit.move(gameMapPosition, gameStatus.unitMap);
            checkLocForAddOrRemoveAbil(unit, true);
            powerManager(gameStatus.myTurn, neededMovePower);
        }

        private void endGame(bool isWin, bool byDC, bool isDraw,bool enemyLeave,MultiplayerController.Bounty bounty)//by dc is true when enemy leaves game
        {

        }

//        private void restartGame()
//        {
//            if (LoadingDialogeManager.instance != null)
//            {
//                LoadingDialogeManager.instance.destroy();
//            }
//            SceneManager.LoadScene("LoginScene",LoadSceneMode.Single);
//        }

        public int getNumberOfXColumn() {
            return numberOfMapXColumn;
        }

        public int getNumberOfYRow() {
            return this.numberOfMapYRow;
        }

        /// <summary>
        /// Reduces the power and resets the text
        /// by M.Hooshdar
        /// </summary>
        /// <param name="isAly"></param>
        /// <param name="powerReduced"></param>
        private void powerManager(bool isAly, int powerReduced) {
            if (isAly) {
                gameStatus.myPower -= powerReduced;
            }
            else {
                gameStatus.enemyPower -= powerReduced;
            }
        }
        /// <summary>
        /// Checks if unit is at the current size for new location
        /// </summary>
        /// <returns></returns>
        public bool checkUnitTail(UnitController currentUnit, float width, Vector2 position, bool myTurn, out int x) {
            x = (int)position.x;
            if (gameStatus.unitMap[(int)position.x][(int)position.y] != null)
            {
                return false;
            }
            if (width > 1)
            {
                Console.WriteLine("Checking unit feature");
                if (myTurn) {
                    x -= 1;
                }
                else {
                    x += 1;
                }
                if ((x < 0 && myTurn) || (x > numberOfMapXColumn - 1 && !myTurn) ||
                    (gameStatus.unitMap[x][(int)position.y] != null &&
                     gameStatus.unitMap[x][(int)position.y] != currentUnit)) {
                    return false;
                }
            }
            else {
                if (gameStatus.unitMap[x][(int)position.y] != null && gameStatus.unitMap[x][(int)position.y].getFeatures().health > 0) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// It sends the recieving order from server to queue to be executed on main thread later
        /// </summary>
        /// <param name="message"></param>
        public void execute(string message) {
//            if (recievedMessages == null) {
//                recievedMessages = new Queue<string>();
//            }
//            if (executerBackgroundProcess == null)
//            {
//                executerBackgroundProcess = new Thread(BackgroundWorker);
//                executerBackgroundProcess.Start();
//            }
            Console.WriteLine("execute:" + message);
            recievedMessages.Enqueue(message);
            execute();
        }
        /// <summary>
        /// It executes the orders from main thread
        /// </summary>
        private void execute() {
            if (recievedMessages.Count <= 0) {
                return;
            }
            string message = recievedMessages.Dequeue();
            //opcode format op_id:data:data ...
            //Opcodes are :
            /*
             *startGame => 0
             *gameStatus => 1:Json(gameStatus)
             *turnFinished => 2
             *myWin => 3
             *leaveGame => 4
             *x1,y1,x2,y2 are positions in unitMap
             *move => 5:x1:y1:x2:y2
             *attack => 6:x1:y1:x2:y2   ---->P.N : x1,y1 are unitMap arguments and x2,y2 are localPosition arguments <----
             *newUnit => 7:unit_id:x1:y1
             *
             */
            int x1, x2, y1, y2;
            float startX;
            string[] splited = message.Split(Constants.serverMessage.opCodes.SEPERATOR);
            if (splited[0] == Constants.serverMessage.opCodes.SERVER_READY) {
                if (!gameStatus.gameStarted) {
                    //StartCoroutine(startMultiplayerStartGameAnimPartTwo());
                    gameStatus.firstTurnIsMine = false;
                    startMultiplayerStartGameAnimPartTwo(gameStatus.firstTurnIsMine);
                    gameStatus.gameStarted = true;
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.TURN_FINISHED) {
                Console.WriteLine("Turn is finished");
                if (user.getGameMode() == Constants.gameMode.MULTI_PLAYER)//This part is for ultimate sync with server
                {
                    string purified = splited[1].Replace("\\", "\"");
                    JsonData syncInfo = JsonMapper.ToObject(purified);
                    MultiplayerController.ReGameStatus syncObj = new MultiplayerController.ReGameStatus();
                    syncObj.fillWithRawObj(syncInfo);
                    onReconnectComplete(syncObj);
                }
                else{
                    if (splited.Length > 1)
                    {
                        int rawX = Int32.Parse(splited[1]);
                        if (rawX != -1)
                        {
                            int alyLandEndX = (int)mirrorEnemyPosToAlyPos(new Vector2(rawX, 0)).x;
                            updateAlyLandEndX(alyLandEndX);
                        }
                    }
                    endGameTurn();
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.WIN_GAME) {
                endGame(false, false, false,false,null);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.DRAW_GAME)
            {
                endGame(false, false, true, false,null);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.LOOSE_GAME)
            {
                endGame(true, false, false, false,null);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.LEAVE_GAME)
            {
                endGame(true, false, false, true,null);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.MOVE_UNIT)
            {
                x1 = int.Parse(splited[1]);
                y1 = int.Parse(splited[2]);
                x2 = int.Parse(splited[3]);
                y2 = int.Parse(splited[4]);
                Vector2 mapPos = new Vector2(x2, y2);
//                if (user.getGameMode() != Constants.gameMode.MULTI_PLAYER)
//                {
//                    mapPos = mirrorAlyPosToEnemyPos(new Vector2(x2, y2));
//                }
                bool isAly = false;
                if (mapPos.x < numberOfMapXColumn / 2)
                {
                    isAly = true;
                }
                //Vector2 movingUnitPos = mirrorAlyPosToEnemyPos(new Vector2(x1, y1));
                Vector2 movingUnitPos = new Vector2(x1, y1);
//                if (user.getGameMode() != Constants.gameMode.MULTI_PLAYER)
//                {
//                    movingUnitPos = mirrorAlyPosToEnemyPos(movingUnitPos);
//                }
                UnitController movingUnit = gameStatus.unitMap[(int)movingUnitPos.x][(int)movingUnitPos.y];
//                if (movingUnit != null)
//                {
                    int movePower = movingUnit.callculateDistancePassingPower(mapPos);
                    moveUnit(movingUnit,mapPos, movePower);
//                }
//                else//Means data confilict or it's not created already
//                {
//                    recievedMessages.Enqueue(message);
//                    execute();
//                    executeOnMainThread("execute");
//                    Console.Log(Errors.MAP_SYNC_ISSUE);
//                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.ATTACK_UNIT)
            {
                x1 = int.Parse(splited[1]);
                y1 = int.Parse(splited[2]);
                x2 = int.Parse(splited[3]);
                y2 = int.Parse(splited[4]);
                Vector2 attackerPos = new Vector2(x1,y1);
//                if (Constants.gameMode != Strings.gameMode.MULTI_PLAYER)
//                {
//                    attackerPos = mirrorAlyPosToEnemyPos(new Vector2(x1, y1));
//                }
                UnitController attacker = gameStatus.unitMap[(int)attackerPos.x][(int)attackerPos.y];
                Vector2 attackedPos = new Vector2(x2, y2);
//                if (Constants.gameMode != Strings.gameMode.MULTI_PLAYER)
//                {
//                    attackedPos = mirrorEnemyPosToAlyPos(attackedPos);
//                    attack(attacker, attackedPos,null);
//                }
//                if (Constants.gameMode == Strings.gameMode.MULTI_PLAYER)
//                {
                    List<HittedUnits> hittedUnits = new List<HittedUnits>();
                    string rawHits = splited[5];
                    if (rawHits != "") {
                        string[] parts = rawHits.Split (Constants.serverMessage.opCodes.HITED_UNITS_PARAMS.PART_SEP);
                        for (int i = 0; i < parts.Length; i++) {
                            string[] mems = parts [i].Split (Constants.serverMessage.opCodes.HITED_UNITS_PARAMS.MEM_SEP);
                            int unitId = int.Parse (mems [0]);
                            int damage = int.Parse (mems [1]);
                            hittedUnits.Add (new HittedUnits(unitId,damage));
                        }
                    }
                    attack(attacker, attackedPos, hittedUnits);
//                }
                Console.WriteLine("Attack");
                //redHighlights[(int)attackedPos.x][(int)attackedPos.y].SetActive(true);
                //attacker.attack(attackedPos,false, onAttackAnimationFinshed);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.CREATE_UNIT)
            {
                int unitId = int.Parse(splited[1]);
                x1 = int.Parse(splited[2]);
                y1 = int.Parse(splited[3]);
                int level = int.Parse(splited[4]);
                int assignedId = int.Parse(splited[5]);
                Vector2 mapPos = new Vector2(x1, y1);
                if (user.getGameMode() != Constants.gameMode.MULTI_PLAYER)
                {
                    mapPos = mirrorAlyPosToEnemyPos(mapPos);
                }
                bool isAly = false;
                if (mapPos.x < numberOfMapXColumn / 2)
                {
                    isAly = true;
                }
                createNewUnit((UnitFeatures)availableFeatures[unitId], mapPos, isAly,true, level,assignedId);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.CLIENT_READY)
            {
                if (!gameStatus.gameStarted)
                {
                    gameStatus.firstTurnIsMine = false;
                    //executeOnMainThread("multiAnimPartTwoInvoker");
                    startMultiplayerStartGameAnimPartTwo(gameStatus.firstTurnIsMine);
                    gameStatus.gameStarted = true;
                    //StartCoroutine(startMultiplayerStartGameAnimPartTwo());
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.KILL_UNIT)
            {
                int x = Int32.Parse(splited[1]);
                int y = Int32.Parse(splited[2]);
                Vector2 corpsePos = new Vector2(x, y);
                if (user.getGameMode() != Constants.gameMode.MULTI_PLAYER)
                {
                    corpsePos = mirrorEnemyPosToAlyPos(corpsePos);
                }
                UnitController unit = gameStatus.unitMap[(int)corpsePos.x][(int)corpsePos.y];
                if (unit != null)
                {
                    Console.WriteLine("Some problem \nSolving by force");
                    //checkForAddOrRemoveAbility((int)unit.gameMapPosition.x, (int)unit.gameMapPosition.y, false);
                    //if (unit.getFeatures().width > 1)
                    //{
                    //    if (!unit.getIsAly())
                    //    {
                    //        checkForAddOrRemoveAbility((int)unit.gameMapPosition.x + 1, (int)unit.gameMapPosition.y, false);
                    //    }
                    //    else
                    //    {
                    //        checkForAddOrRemoveAbility((int)unit.gameMapPosition.x - 1, (int)unit.gameMapPosition.y, false);
                    //    }
                    //}
                    checkLocForAddOrRemoveAbil(unit, false);
                    unit.kill();
                }
            }
            else if (splited[0] == Strings.serverMessage.opCodes.CREATE_FAILED)
            {
                int x = Int32.Parse(splited[1]);
                int y = Int32.Parse(splited[2]);
                Vector2 corpsePos = new Vector2(x, y);
                if (Constants.gameMode != Strings.gameMode.MULTI_PLAYER)
                {
                    corpsePos = mirrorEnemyPosToAlyPos(corpsePos);
                }
                UnitController unit = gameStatus.unitMap[(int)corpsePos.x][(int)corpsePos.y];
                if (unit != null)
                {
                    Console.WriteLine("Some problem \nSolving by force");
                    //checkForAddOrRemoveAbility((int)unit.gameMapPosition.x, (int)unit.gameMapPosition.y, false);
                    //if (unit.getFeatures().width > 1)
                    //{
                    //    checkForAddOrRemoveAbility((int)unit.gameMapPosition.x + 1, (int)unit.gameMapPosition.y, false);
                    //    checkForAddOrRemoveAbility((int)unit.gameMapPosition.x - 1, (int)unit.gameMapPosition.y, false);
                    //}
                    checkLocForAddOrRemoveAbil(unit, false);
                    unit.killForGameUpdate();
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.CREATE_ACCEPTED)
            {
                int x = int.Parse(splited[1]);
                int y = int.Parse(splited[2]);
                int assignedId = int.Parse(splited[3]);
                UnitController unit = gameStatus.unitMap[x][y];
                unit.setAssignedId(assignedId);
            }
            else if (splited[0] == Constants.serverMessage.opCodes.START_MULTI_AS_FIRST)
            {
                if (!gameStatus.gameStarted)
                {
                    Console.WriteLine("Start multi as  first");
                    gameStatus.firstTurnIsMine = true;
                    startMultiplayerStartGameAnimPartTwo(gameStatus.firstTurnIsMine);
                    gameStatus.gameStarted = true;
                }
                else
                {
                    Console.WriteLine("Game already is Started");
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.START_MULTI_AS_SECOND)
            {
                if (!gameStatus.gameStarted)
                {
                    Console.WriteLine("Start multi as  second");
                    gameStatus.firstTurnIsMine = false;
                    startMultiplayerStartGameAnimPartTwo(gameStatus.myTurn);
                    gameStatus.gameStarted = true;
                }
                else
                {
                    Console.WriteLine("Game already is Started");
                }
            }
//            else if (splited[0] == Constants.serverMessage.opCodes.USE_ITEM)
//            {
//                int itemId = int.Parse(splited[1]);
//                int doer = int.Parse(splited[2]);
//                if (doer == 0)
//                {
//                    abilityUsed(ItemManager.instance.select(itemId), true);
//                }
//                else
//                {
//                    abilityUsed(ItemManager.instance.select(itemId), false);
//                }
//            }
            else if (splited[0] == Constants.serverMessage.opCodes.FORCE_MOVE)
            {
                int assignedId = int.Parse(splited[1]);
                int x = int.Parse(splited[2]);
                int y = int.Parse(splited[3]);
                for (int i = 0; i < gameStatus.unitMap.Length; i++)
                {
                    bool unitFound = false;
                    for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                    {
                        UnitController unit = gameStatus.unitMap[i][j];
                        if (unit!=null && unit.getAssignedId() == assignedId)
                        {
                            unitFound = true;
                            unit.forceMove(new Vector2(x, y), gameStatus.unitMap);
                            break;
                        }
                    }
                    if (unitFound)
                    {
                        break;
                    }
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.CREATE_FAILED)
            {
                int assignedId = int.Parse(splited[1]);
                for (int i = 0; i < gameStatus.unitMap.Length; i++)
                {
                    bool unitFound = false;
                    for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                    {
                        UnitController unit = gameStatus.unitMap[i][j];
                        if (unit.getAssignedId() == assignedId)
                        {
                            unitFound = true;
                            unit.killForGameUpdate();
                            checkLocForAddOrRemoveAbil(unit, false);
                            //checkForAddOrRemoveAbility((int)unit.gameMapPosition.x, (int)unit.gameMapPosition.y, false);
                            //if (unit.getFeatures().width > 1)
                            //{
                            //    checkForAddOrRemoveAbility((int)unit.gameMapPosition.x + 1, (int)unit.gameMapPosition.y, false);
                            //    checkForAddOrRemoveAbility((int)unit.gameMapPosition.x - 1, (int)unit.gameMapPosition.y, false);
                            //}
                            break;
                        }
                    }
                    if (unitFound)
                    {
                        break;
                    }
                }
            }
            else if (splited[0] == Constants.serverMessage.opCodes.HIT)//It's always multiplayer
            {
                List<HittedUnits> hittedUnits = new List<HittedUnits>();
                int x = int.Parse(splited[1]);
                int y = int.Parse(splited[2]);
                string rawHits = splited[3];
                if (rawHits != "")
                {
                    string[] parts = rawHits.Split(Constants.serverMessage.opCodes.HITED_UNITS_PARAMS.PART_SEP);
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string[] mems = parts[i].Split(Strings.serverMessage.opCodes.HITED_UNITS_PARAMS.MEM_SEP);
                        int unitId = int.Parse(mems[0]);
                        int damage = int.Parse(mems[1]);
                        hittedUnits.Add(new HittedUnits(unitId, damage));
                    }
                }
                hitUnits(hittedUnits,new Vector2(x,y),lastAttacker);
            }
        }

        //private IEnumerator resendData(int number)
        //{
        //    int numberOfSentData = networkController.getNumberOfSentMessages();
        //    for (int i = number; i < numberOfSentData; i++)
        //    {
        //        networkController.resendMessage(i);
        //        yield return new WaitForEndOfFrame();
        //    }
        //    yield return null;
        //}

        /// <summary>
        /// Client and server both are playing at th same side and we mirror it
        /// It mirrors aly pos to enemy pos
        /// </summary>
        /// <param name="recPos">
        /// The position we have recievd
        /// </param>
        /// <returns></returns>
        private Vector2 mirrorAlyPosToEnemyPos(Vector2 alyPos) {
            float newX = (numberOfMapXColumn / 2 - alyPos.x) + numberOfMapXColumn / 2 - 1;
            return new Vector2(newX, alyPos.y);
        }
        /// <summary>
        /// Client and server both are playing at th same side and we mirror it
        /// It mirrors enemy pos to aly pos
        /// </summary>
        /// <param name="recPos">
        /// The position we have recievd
        /// </param>
        /// <returns></returns>
        private Vector2 mirrorEnemyPosToAlyPos(Vector2 enemyPos) {
            int newX = numberOfMapXColumn / 2 - 1 - (enemyPos.x - numberOfMapXColumn / 2);
            return new Vector2(newX, enemyPos.y);
        }

        private void checkLocForAddOrRemoveAbil(UnitController unit,bool isAdd)
        {
            checkForAddOrRemoveAbility((int)unit.gameMapPosition.x, (int)unit.gameMapPosition.y, isAdd);
            if (unit.getFeatures().width > 1)
            {
                if (unit.getIsAly())
                {
                    checkForAddOrRemoveAbility((int)unit.gameMapPosition.x + 1, (int)unit.gameMapPosition.y, isAdd);
                }
                else
                {
                    checkForAddOrRemoveAbility((int)unit.gameMapPosition.x - 1, (int)unit.gameMapPosition.y, isAdd);
                }
            }
        }

        /// <summary>
        /// Whenever an attack is happened this method is called if attack is happened from an empty land
        /// you have to be place features null (Important) and isAly dose not matter and place the position the place which is under attack
        /// </summary>
        /// <param name="features">Died unit faetures</param>
        /// <param name="isAly">Hitted unit is aly or not</param>
        /// <param name="position">Unit position or attack position</param>
        public void notifyUnitIsDead(UnitFeatures features, bool isAly,Vector2 position,bool checkForMapLossIsValid)//When a unit dies it is going to be called
        {
            if (features != null)
            {
                checkForAddOrRemoveAbility((int)position.x, (int)position.y, false);
                if (isAly)
                {
                    if (features.width > 1)
                    {
                        checkForAddOrRemoveAbility((int)position.x-1, (int)position.y, false);
                    }
                    gameStatus.myPowerRegen -= (int)features.powerRegen;

                }
                else
                {
                    if (features.width > 1)
                    {
                        checkForAddOrRemoveAbility((int)position.x + 1, (int)position.y, false);
                    }
                    gameStatus.enemyPowerRegen -= (int)features.powerRegen;
                    gameStatus.myPower += bonusPower;
                }
                removeUnitFromMap(features, isAly, position);
                if (gameStatus.myTurn && (!isAly) && (user.getGameMode() != Constants.gameMode.SINGLE_PLAYER
                 && Constants.gameMode!=Constants.gameMode.MULTI_PLAYER) && checkForMapLossIsValid)
                    //When check for map loss is invalid
                    //we are putting units by hand for update
                {
                    networkController.send(Constants.serverMessage.opCodes.KILL_UNIT.ToString() + Constants.serverMessage.opCodes.SEPERATOR.ToString() +
                                           position.x.ToString() + Constants.serverMessage.opCodes.SEPERATOR.ToString() + position.y.ToString());
                }
            }
            bool isInAlyTeretory = true;
            if (position.x >= numberOfMapXColumn / 2)
            {
                isInAlyTeretory = false;
            }
            bool isEndOfGame = false;
            if (checkForMapLossIsValid)
            {
                isEndOfGame = checkForMapLoss(position, isInAlyTeretory);
            }
            if (isEndOfGame)
            {
                if (user.getGameMode() != Constants.gameMode.MULTI_PLAYER)
                {
                    if (isInAlyTeretory)
                    {
                        Console.WriteLine("Looser");
                        endGame(false, false, false, false,null);
                    }
                    else
                    {
                        Console.WriteLine("Winner");
                        if (networkController != null)
                        {
                            networkController.send(Constants.serverMessage.opCodes.WIN_GAME);
                        }
                        endGame(true, false, false, false,null);
                    }
                }
            }
        }

        private void removeUnitFromMap(UnitFeatures features, bool isAly, Vector2 position)
        {
            //checkForAddOrRemoveAbility((int)position.x, (int)position.y, false);
            checkLocForAddOrRemoveAbil(gameStatus.unitMap[(int)position.x][(int)position.y], false);
            gameStatus.unitMap[(int)position.x][(int)position.y] = null;
            if (features.width > 1)
            {
                int x = (int)position.x - 1;
                if (!isAly)
                {
                    x = (int)position.x + 1;
                }
                //checkForAddOrRemoveAbility((int)position.x + 1, (int)position.y, false);
                //checkForAddOrRemoveAbility((int)position.x - 1, (int)position.y, false);
                gameStatus.unitMap[x][(int)position.y] = null;
            }
        }

        /// <summary>
        /// It is called when player is reconnecting to his/her game  it restores power and units
        /// </summary>
        /// <param name="newStatus">The status we need for rconnecting</param>
        public void onReconnectComplete(MultiplayerController.ReGameStatus newStatus)
        {
            networkController.notifyServerPlayerRecComplete();
            try
            {
                Console.WriteLine("In onReconnect complete method");
                List<MultiplayerController.ReGameStatus.UnitState> newUnits = newStatus.unitList;
                int alyLandEndX = newStatus.alyLandEndX;
                int enemyLandStartX = newStatus.enemyLandStartX;
                //For updating changes in dead land
                updateAlyLandEndX(alyLandEndX);
                updateEnemyLandStartX(enemyLandStartX);
                List<UnitController> checkedUnits = new List<UnitController>();
                //For restoring units I first kill all of them
                for (int i = 0; i < gameStatus.unitMap.Length; i++)
                {
                    for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                    {
                        if (gameStatus.unitMap[i][j] != null && gameStatus.unitMap[i][j].getFeatures().health > 0
                            && !checkedUnits.Contains(gameStatus.unitMap[i][j])
                        )
                        {
                            bool unitExists = false;
                            checkedUnits.Add(gameStatus.unitMap[i][j]);
                            for (int k = 0; k < newUnits.Count; k++)
                            {
                                if (newUnits[k].assignedId == gameStatus.unitMap[i][j].getAssignedId())
                                {
                                    //   gameStatus.
                                    unitExists = true;
                                    gameStatus.unitMap[i][j].updateStatus(newUnits[k].health,
                                        newUnits[k].pos,gameStatus.unitMap,newUnits[k].remainingShots);
                                    newUnits.RemoveAt(k);
                                    break;
                                }
                            }
                            if (!unitExists)
                            {
                                gameStatus.unitMap[i][j].kill();
                            }
                        }
                    }
                }
                //After that I place all of them in game  and set their health
                //nitFeatures temp;
                for (int i = 0; i < newUnits.Count; i++)
                {
                    int unitId = newUnits[i].unitId;
                    Vector2 mapPos = newUnits[i].pos;
                    bool isAly = true;
                    UnitFeatures currentFeature = (UnitFeatures)availableFeatures[unitId];
                    createNewUnit(currentFeature, mapPos, isAly, false, newUnits[i].level, newUnits[i].assignedId);
                    gameStatus.unitMap[(int)mapPos.x][(int)mapPos.y].setUnitHealth(newUnits[i].health);
                    gameStatus.unitMap[(int)mapPos.x][(int)mapPos.y].setAvailableShots(newUnits[i].remainingShots);
                }
                gameStatus.myPower = newStatus.currentPower;
                gameStatus.enemyPower = newStatus.enemyPower;
                gameStatus.myPowerRegen = newStatus.alyPowerRegen;
                gameStatus.enemyPowerRegen = newStatus.enemyPowerRegen;
                gameStatus.firstTurnIsMine = newStatus.firstTurnIsMine;
                gameStatus.turn = newStatus.turnNumber;
                if ((gameStatus.turn % 2 == 0 && gameStatus.firstTurnIsMine) || (gameStatus.turn % 2 == 1 && !gameStatus.firstTurnIsMine))
                {
                    gameStatus.myTurn = true;
                }
                else
                {
                    gameStatus.myTurn = false;
                }
                checkedUnits = new List<UnitController>();
                for (int i = 0; i < gameStatus.unitMap.Length; i++)
                {
                    for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                    {
                        if (gameStatus.unitMap[i][j] != null && !checkedUnits.Contains(gameStatus.unitMap[i][j]))
                        {
                            checkedUnits.Add(gameStatus.unitMap[i][j]);
                            checkForAddOrRemoveAbility(i, j, true);
                        }
                    }
                }
                startNextImplemention();
                //for (int j = 0; j < ItemManager.instance.getAllItemFeatures().Count; j++) {
                //    ItemManager.instance.getAllItemFeatures()[j].isInDeck = false;
                //}
                //for (int i = 0; i < newStatus.itemIds.Count; i++) {
                //    for (int j = 0; j < ItemManager.instance.getAllItemFeatures().Count; j++) {
                //        if (newStatus.itemIds[i] == ItemManager.instance.getAllItemFeatures()[j].id) {
                //            ItemManager.instance.getAllItemFeatures()[j].isInDeck = true;
                //        }
                //    }
                //}
                ////TODO Uncomment
                //itemsPanel.GetComponent<DeckItemsController>().setDatas();
            }
            catch (Exception e)
            {
                Console.WriteLine("Rec error "+e.Message+""+e.StackTrace);
            }
        }
        /// <summary>
        /// It changes enemy and aly map
        /// </summary>
        /// <param name="alyLandEndX">place where aly land end x is placed</param>
        private void updateAlyLandEndX(int alyLandEndX)
        {
            if (alyLandEndX != gameStatus.alyLandEndX)
            {
                Console.WriteLine("aly land is " + alyLandEndX.ToString());
                for (int i = alyLandEndX; i < numberOfMapXColumn / 2; i++)
                {
                    for (int j = 0; j < numberOfMapYRow; j++)
                    {
                        if (alyLandEndX <= i && i < gameStatus.alyLandEndX)
                        {
                        }
                        if (gameStatus.unitMap[i][j] != null)
                        {
                            checkLocForAddOrRemoveAbil(gameStatus.unitMap[i][j], false);
                            gameStatus.unitMap[i][j].kill();
                        }
                    }
                }
                int alyNewDeadLand = (Math.Abs(gameStatus.alyLandEndX - alyLandEndX));
                if(gameStatus.enemyPower + alyNewDeadLand * deadLanBonusPower < maxPower) {
                    gameStatus.enemyPower += alyNewDeadLand * deadLanBonusPower;
                }
                else {
                    gameStatus.enemyPower = maxPower;
                }
                Console.WriteLine("bonus:"+alyNewDeadLand * deadLanBonusPower);
                gameStatus.alyLandEndX = alyLandEndX;
            }
        }

        /// <summary>
        /// It updates enemy land and score automatically
        /// </summary>
        /// <param name="enemyLandStartX">Place where enemy land start</param>
        private void updateEnemyLandStartX(int enemyLandStartX)
        {
            if (enemyLandStartX != gameStatus.enemyLandStartX)
            {
                Console.WriteLine("THis is shitt:  " + enemyLandStartX.ToString());
                for (int i = (numberOfMapXColumn/2); i <= enemyLandStartX; i++)
                {
                    for (int j = 0; j < numberOfMapYRow; j++)
                    {
                        if (j != numberOfMapYRow) {
                            if (gameStatus.unitMap [i] [j] != null) {
                                checkLocForAddOrRemoveAbil(gameStatus.unitMap[i][j],false);
                                gameStatus.unitMap [i] [j].kill ();
                            }
                        }
                    }
                }
                int enemyNewDeadLand = Math.Abs(gameStatus.enemyLandStartX - enemyLandStartX);
                if (gameStatus.myPower + enemyNewDeadLand * deadLanBonusPower < maxPower) {
                    gameStatus.myPower += enemyNewDeadLand * deadLanBonusPower;
                }
                else {
                    gameStatus.myPower = maxPower;
                }
                gameStatus.enemyLandStartX = enemyLandStartX;
            }
        }

        public void onDCCallBack()
        {
            restartGame();
        }

        //public MultiplayerController.ReGameStatus gameStatusGenerator()
        //{
        //    MultiplayerController.ReGameStatus status = new MultiplayerController.ReGameStatus();
        //    status.unMirrorPlayerLandEndX = gameStatus.enemyLandStartX;
        //    status.unMirrorEnemyLandStartX = gameStatus.alyLandEndX;
        //    status.currentPower = gameStatus.enemyPower;
        //    status.enemyPower = gameStatus.myPower;
        //    status.turnNumber = gameStatus.turn;
        //    List<MultiplayerController.ReGameStatus.UnitState> units = new List<MultiplayerController.ReGameStatus.UnitState>();
        //    List<UnitController>addedUnits = new List<UnitController>();
        //    for (int i = 0; i < gameStatus.unitMap.Length; i++)
        //    {
        //        for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
        //        {
        //            if (gameStatus.unitMap[i][j] != null && gameStatus.unitMap[i][j].getFeatures().health > 0
        //                && !addedUnits.Contains(gameStatus.unitMap[i][j]))
        //            {
        //                UnitController currentUnit = gameStatus.unitMap[i][j];
        //                units.Add(currentUnit.getUnitStatus());
        //                addedUnits.Add(currentUnit);
        //            }
        //        }
        //    }
        //    status.unitStateList = units;
        //    //To make sure new ordr can be recieved
        //    networkController.resetEnemyIdCounter();
        //    return status;
        //}

        private void OnDestroy()
        {
//            if (Constants.gameMode != Strings.gameMode.SINGLE_PLAYER)
//            {
//                if (Constants.gameMode == Strings.gameMode.MULTI_PLAYER)
//                {
//                    MultiplayerController.instance.disconnectFromServer();
//                }
//                else
//                {
                    socket.disconnectFromServer();
//                }
//            }
        }
        private void checkForAddOrRemoveAbility(int x, int y, bool adding) {
            if (gameStatus.unitMap[x][y] != null)
            {
                //UnityEngine.Debug.Log("In add or remove ability method");
                bool isSpecial = false;
                if (Constants.unitIds.SPECIAL.needAliveContains(gameStatus.unitMap[x][y].getFeatures().id))
                {
                    isSpecial = true;
                }
                List<UnitController> localSpecials = new List<UnitController>();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if ((i != 1 || j != 1) &&
                            radiusLocationIsValid((int)(i + x - 1), (int)(j + y - 1), gameStatus.unitMap[x][y].getIsAly()))
                        {
                            UnitController targetUnit = gameStatus.unitMap[i + x - 1][j + y - 1];
                            if (targetUnit != null)
                            {
                                if ((gameStatus.unitMap[x][y].getFeatures().width > 1 &&
                                     targetUnit != gameStatus.unitMap[x][y]) || gameStatus.unitMap[x][y].getFeatures().width <= 1)
                                {
                                    if (adding)
                                    {
                                        //if(!Strings.unitIds.SPECIAL.contains(gameStatus.unitMap[x][y].getFeatures().id)) {
                                        if (isSpecial)
                                        {
                                            targetUnit.addAbility(new UnitController.Ability(gameStatus.unitMap[x][y]));
                                            //        UnityEngine.Debug.Log("Abil added");
                                        }
                                        if (Constants.unitIds.SPECIAL.needAliveContains(targetUnit.getFeatures().id))
                                        {
                                            localSpecials.Add(targetUnit);
                                        }
                                        //}
                                    }
                                    else
                                    {
                                        targetUnit.removeAbility(new UnitController.Ability(gameStatus.unitMap[x][y]));
                                        //  UnityEngine.Debug.Log("Abil removed");
                                    }
                                    //if(hasInTimeAbility.Contains(gameStatus.unitMap[x][y].getFeatures().id))
                                    //    targetUnit.setInTimeAbility(gameStatus.unitMap[x][y].getFeatures().id);
                                    //UnityEngine.Debug.Log("abil1");
                                }
                            }
                        }
                    }
                    //}
                }
                gameStatus.unitMap[x][y].removeAllPlaceBasedAbilities();
                for (int i = 0; i < localSpecials.Count; i++)
                {
                    gameStatus.unitMap[x][y].addAbility(new UnitController.Ability(localSpecials[i]));
                }
            }
        }

        private void resetShotsInTurn(){
            for (int i = 0; i < gameStatus.unitMap.Length; i++) {
                for (int j = 0; j < gameStatus.unitMap [i].Length; j++) {
                    if (gameStatus.unitMap [i] [j] != null)
                        gameStatus.unitMap [i] [j].resetShots();
                }
            }
        }

        public bool isMyTurn()
        {
            return gameStatus.myTurn;
        }

        private void onWeakConnection()
        {
            Console.WriteLine("Weak connection detected");
        }
    }
}
