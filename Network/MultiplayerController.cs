using System;
using System.Collections.Generic;
using LitJson;
using LandFightBotReborn.Utils;
using System.Threading;
using LandFightBotReborn.Bot;
using LandFightBotReborn.SocketIO.JSONObjects;
using LandFightBotReborn.SocketIO;
namespace LandFightBotReborn.Network
{
    public class MultiplayerController
    {
        public delegate void OnConnect();
        public delegate void OnDisconnect();
        public delegate void OnGameFound(GameInfo info);
        public delegate void OnFindGameReqSendComplete();
        public delegate void OnGameIsReady(bool isReady);
        public delegate void OnReciveCallBack(string data);
        public delegate void OnEnemyDCAccepted(Bounty bounty);
        public delegate void OnPlayerIsDC();
        public delegate void OnGameFinished(Bounty bounty);
        public delegate void OnReconnectComplete(ReGameStatus reGameStatus);
        public delegate void OnReconnectFailed();
        public delegate void GameStatusGenerator(OnStatusGenrateComplete onStatusGen);
        public delegate void OnStatusGenrateComplete(ReGameStatus status);
        public delegate void OnRuleReady(GameInfo info);
        public delegate void OnCancelFindGameAccepted();
        public delegate void OnCancelFindGameFailed();
        public delegate void OnWeakConectionDetected();
        private OnDisconnect onDCCallBack;
        private OnWeakConectionDetected onWeakConnection;
        public class GameInfo
        {
            public string gameId;
            public string userId;
            public string enemyId;
            public int turnTime;
            public int landBonus;
            public int initialPower;
            public int basePowerRegen;
            public int maxTurn;
            public int maxPower;
            public List<int> itemIds;
            public string enemyName;
            public void fillWithRawObj(SocketIOEvent obj)
            {
                userId = obj.data.GetField("userToken").ToString();
                gameId = obj.data.GetField("gameToken").ToString();
                enemyId = obj.data.GetField("oppToken").ToString();
                turnTime = int.Parse(obj.data.GetField("turnTime").ToString());
                landBonus = int.Parse(obj.data.GetField("landBonus").ToString());
                initialPower = int.Parse(obj.data.GetField("initialPower").ToString());
                basePowerRegen = int.Parse(obj.data.GetField("basePowerRegen").ToString());
                maxTurn = int.Parse(obj.data.GetField("maxTurn").ToString());
                maxPower = int.Parse(obj.data.GetField("maxPower").ToString());
                JsonData rawHints = JsonMapper.ToObject(obj.data.GetField("itemIds").ToString());
                itemIds = new List<int>();
                for (int i = 0; i < rawHints.Count; i++)
                {
                    string hint = (rawHints[i].ToString());
                    itemIds.Add(int.Parse(hint));
                }
                enemyName = obj.data.GetField("enemyName").ToString().Replace("\"", "");
            }
            //When we are continuing current game
            public void fillWithCommonData(SocketIOEvent obj, string userId, string gameId, string enemyId, string enemyName)
            {
                this.userId = userId;
                this.gameId = gameId;
                this.enemyId = enemyId;
                this.enemyName = enemyName;
                turnTime = int.Parse(obj.data.GetField("turnTime").ToString());
                landBonus = int.Parse(obj.data.GetField("landBonus").ToString());
                initialPower = int.Parse(obj.data.GetField("initialPower").ToString());
                basePowerRegen = int.Parse(obj.data.GetField("basePowerRegen").ToString());
                maxTurn = int.Parse(obj.data.GetField("maxTurn").ToString());
                maxPower = int.Parse(obj.data.GetField("maxPower").ToString());
                //JsonData rawHints = JsonMapper.ToObject(obj.data.GetField("itemIds").ToString());
                //itemIds = new List<int>();
                //for (int i = 0; i < rawHints.Count; i++)
                //{
                //    Debug.Log("here");
                //    string hint = (rawHints[i].ToString());
                //    itemIds.Add(int.Parse(hint));
                //}
            }
        }
        public class Bounty
        {
            public int gold;
            public int xp;
            public int trophy;
            public int state;
            public int stars;
            public void fillWithRawData(JSONObject rawObj)
            {
                gold = int.Parse(rawObj.GetField("gold").ToString());
                xp = int.Parse(rawObj.GetField("xp").ToString());
                trophy = int.Parse(rawObj.GetField("trophy").ToString());
                state = int.Parse(rawObj.GetField("state").ToString());
                stars = int.Parse(rawObj.GetField("stars").ToString());
            }
        }
        public class ReGameStatus
        {
            public class UnitState
            {
                public int unitId;
                public int health;
                public Vector2 pos;
                public int level;
                public int remainingShots;
                public int assignedId;
                public void fillWithRawObj(JSONObject rawObj)
                {
                    unitId = int.Parse(rawObj.GetField("unitId").ToString());
                    level = int.Parse(rawObj.GetField("level").ToString());
                    health = (int)float.Parse(rawObj.GetField("health").ToString());//Jsut for more safty
                    int x = int.Parse(rawObj.GetField("x").ToString());
                    int y = int.Parse(rawObj.GetField("y").ToString());
                    pos = new Vector2(x, y);
                    remainingShots = int.Parse(rawObj.GetField("remainingShots").ToString());
                    assignedId = int.Parse(rawObj.GetField("assignedId").ToString());
                }

                public void fillWithRawObj(JsonData rawObj)
                {
                    unitId = int.Parse(rawObj["unitId"].ToString());
                    level = int.Parse(rawObj["level"].ToString());
                    health = int.Parse(rawObj["health"].ToString());
                    int x = int.Parse(rawObj["x"].ToString());
                    int y = int.Parse(rawObj["y"].ToString());
                    pos = new Vector2(x, y);
                    remainingShots = int.Parse(rawObj["remainingShots"].ToString());
                    assignedId = int.Parse(rawObj["assignedId"].ToString());
                }

            }
            /// <summary>
            /// List of game current units
            /// Note:JsonUtitly does not support string
            /// Note2:Data must be less than 128 kb or data will be sent in seprate parts and you hava to handle parts seperatly
            /// </summary>
            public List<UnitState> unitList;
            public int alyLandEndX;
            public int alyPowerRegen;
            public int enemyLandStartX;
            public int enemyPower;
            public int enemyPowerRegen;
            public int currentPower;
            public int turnNumber;
            public int time;
            public bool firstTurnIsMine;
            public List<int> itemIds;
            public void fillWithRawObj(JSONObject obj)
            {
                alyLandEndX = int.Parse(obj.GetField("alyLandEndX").ToString());
                enemyLandStartX = int.Parse(obj.GetField("enemyLandStartX").ToString());
                currentPower = int.Parse(obj.GetField("currentPower").ToString());
                alyPowerRegen = int.Parse(obj.GetField("alyPowerRegen").ToString());
                enemyPower = int.Parse(obj.GetField("enemyPower").ToString());
                enemyPowerRegen = int.Parse(obj.GetField("enemyPowerRegen").ToString());
                turnNumber = int.Parse(obj.GetField("turnNumber").ToString());
                time = int.Parse(obj.GetField("time").ToString());
                firstTurnIsMine = bool.Parse(obj.GetField("isFirst").ToString());
                unitList = new List<UnitState>();
                //Logger.debug("Re game status before units");
                JSONObject rawUnits = obj.GetField("unitList");
                for (int i = 0; i < rawUnits.Count; i++)
                {
                    UnitState newUnit = new UnitState();
                    newUnit.fillWithRawObj(rawUnits[i]);
                    unitList.Add(newUnit);
                }
                //ogger.debug("Re game status parse complete");
            }

            public void fillWithRawObj(JsonData syncInfo)
            {
                alyLandEndX = int.Parse(syncInfo["alyLandEndX"].ToString());
                enemyLandStartX = int.Parse(syncInfo["enemyLandStartX"].ToString());
                currentPower = int.Parse(syncInfo["currentPower"].ToString());
                enemyPower = int.Parse(syncInfo["enemyPower"].ToString());
                turnNumber = int.Parse(syncInfo["turnNumber"].ToString());
                alyPowerRegen = int.Parse(syncInfo["alyPowerRegen"].ToString());
                enemyPowerRegen = int.Parse(syncInfo["enemyPowerRegen"].ToString());
                time = int.Parse(syncInfo["time"].ToString());
                firstTurnIsMine = bool.Parse(syncInfo["isFirst"].ToString());
                unitList = new List<UnitState>();
                //Logger.debug("Re game status before units");
                JsonData rawUnits = syncInfo["unitList"];
                for (int i = 0; i < rawUnits.Count; i++)
                {
                    UnitState newUnit = new UnitState();
                    newUnit.fillWithRawObj(rawUnits[i]);
                    unitList.Add(newUnit);
                }
                //Logger.debug("Re game status parse complete");
            }
        }
        /// <summary>
        /// It handles the recieved messages from server events
        /// Note:When ever you initilize an event you have to use functions instead of socketIOEvent beacuase it creates new function
        /// So it's duplicates orders each time
        /// </summary>
        private class EventHandler
        {
            private SocketIOComponent io;
            private OnReciveCallBack onRcCallBack;
            private OnDisconnect onDCCallBack;
            private OnGameIsReady onGameIsReady;
            private OnGameFinished onGameFinished;
            private OnEnemyDCAccepted onEnemyDCAccepted;
            private OnReconnectComplete onReconnectComplete;
            private OnReconnectFailed onReconnectFailed;
            private int correctionCode = -1;
            private MultiplayerController parent;
            public void initializeForFirstTime(MultiplayerController parent, SocketIOComponent io, OnReciveCallBack onRcCallBack, OnDisconnect onDCCallBack, OnGameIsReady onGameIsReady, OnGameFinished onGameFinished, OnEnemyDCAccepted onEnemyDCAccepted, OnReconnectComplete onReconnectComplete, OnReconnectFailed onReconnectFailed)
            {
                this.parent = parent;
                this.io = io;
                this.onRcCallBack = onRcCallBack;
                this.onDCCallBack = onDCCallBack;
                this.onGameIsReady = onGameIsReady;
                this.onGameFinished = onGameFinished;
                this.onEnemyDCAccepted = onEnemyDCAccepted;
                this.onReconnectComplete = onReconnectComplete;
                this.onReconnectFailed = onReconnectFailed;
                io.On(Constants.serverMessage.events.START_GAME, startGame);
                io.On(Constants.serverMessage.events.END_GAME, endGame);
                io.On(Constants.serverMessage.events.ENEMY_DC_ACCEPTED, enemyDCAccepted);
                io.On(Constants.serverMessage.events.ON_GAME_STATUS_READY, gameStatusRecieve);
                io.On(Constants.serverMessage.events.ON_RECONNECT_FAILED, onRecFailed);
                io.On(Constants.serverMessage.events.NEW_TOKENS, onNewTokenRecieved);
                io.On(Constants.serverMessage.events.SERVER_ORDER, onServerOrder);
                io.On(Constants.serverMessage.events.CORRECTION, onCorrection);
                io.On(Constants.serverMessage.events.PING_SUC, onPingSuc);
                //io.On(Strings.serverMessage.events.ENABLE_USER, onEnableUser);
            }

            private void onServerOrder(SocketIOEvent obj)
            {
                //Logger.debug("On server order method");
                string message = obj.data.GetField("message").ToString().Replace("\"", string.Empty);
                //Logger.debug("Recieved new order:" + message);
                parent.lastDataTime = 0;
                onRcCallBack(message);
            }

            private void startGame(SocketIOEvent obj)
            {
                JsonData data = JsonMapper.ToJson(obj);
                bool isFirstTurnMine = bool.Parse(data["isFirst"].ToString());
                // Debug.Log("Starting game First turn is me:"+bool.Parse(obj.data.ToString()));
                onGameIsReady(isFirstTurnMine);
            }

            private void endGame(SocketIOEvent obj)
            {
                Bounty bounty = new Bounty();
                bounty.fillWithRawData(obj.data);
                onGameFinished(bounty);
            }

            private void enemyDCAccepted(SocketIOEvent obj)
            {
                Console.WriteLine("Enemy is DC");
                Bounty bounty = new Bounty();
                bounty.fillWithRawData(obj.data);
                onEnemyDCAccepted(bounty);
            }

            private void gameStatusRecieve(SocketIOEvent obj)
            {
                if (parent.reconnectMode)
                {
                    int newCorrectionCode = int.Parse(obj.data.GetField("correctionNum").ToString());
                    if (newCorrectionCode > correctionCode)
                    {
                        correctionCode = newCorrectionCode;
                    }
                    else
                    {
                        Logger.debug("Rconnect data is outdated");
                        return;
                    }
                    //Logger.debug("Recieved game status:" + obj.data.ToString());
                    ReGameStatus gameStatus = new ReGameStatus();
                    gameStatus.fillWithRawObj(obj.data);
                    Logger.debug("Filled with raw obj");
                    onReconnectComplete(gameStatus);
                    parent.reconnectMode = false;
                }
            }

            private void onCorrection(SocketIOEvent obj)
            {
                JsonData data = JsonMapper.ToJson(obj);
                parent.lastDataTime = 0;
                int newCorrectionCode = int.Parse(data["correctionNum"].ToString());
                if (newCorrectionCode > correctionCode)
                {
                    correctionCode = newCorrectionCode;
                }
                else
                {
                    Console.WriteLine("Rconnect data is outdated");
                    return;
                }
                Console.WriteLine("Recieved game status:" + data.ToString());
                ReGameStatus gameStatus = new ReGameStatus();
                gameStatus.fillWithRawObj(obj.data);
                onReconnectComplete(gameStatus);
                Console.WriteLine("Recievd game status");
            }

            private void onRecFailed(SocketIOEvent obj)
            {
                if (parent.reconnectMode)
                {
                    parent.reconnectMode = false;
                    onReconnectFailed();
                    this.onDCCallBack = null;
                    io.Close();
                }
            }

            public void reInitialize()
            {
                io.Close();
                io = new SocketIOComponent(Constants.SOCKET_URL);
                initializeForFirstTime(parent, io, onRcCallBack, onDCCallBack, onGameIsReady,
                    onGameFinished, onEnemyDCAccepted, onReconnectComplete, onReconnectFailed);
            }

            private void onNewTokenRecieved(SocketIOEvent obj)
            {
                string gameToken = obj.data.GetField(Constants.gameInfo.GAME_TOKEN).ToString();
                string pSocketId = obj.data.GetField(Constants.gameInfo.USER_TOKEN).ToString();
                string eSocketId = obj.data.GetField(Constants.gameInfo.ENEMY_TOKEN).ToString();
                parent.player.user.setUpNewTokens(gameToken, pSocketId, eSocketId);
            }

            private void onPingSuc(SocketIOEvent obj)
            {
                parent.lastDataTime = 0;
                //MultiplayerController.instance.sentPingTime = 0;
            }
            //private void onEnableUser(SocketIOEvent obj) {
            //    TouchManager.instance.setEnable(true);
            //}
        }
        /**By M.Fakhreddin
         *For starting socket io you have to call connect() and I have to mention that whenever you call this you have to
         *redefine all the events again for removing and reseting the socket you have to call close to remove this socket
         *from connected sockets to server
         */
        private SocketIOComponent io;
        private bool reconnectMode;
        private bool gameHasStarted;
        private EventHandler eventHandler;
        private double lastDataTime;
        private BotManager player;
        public MultiplayerController(BotManager player, OnReciveCallBack onRcCallBack, OnDisconnect onDCCallBack,
            OnGameIsReady onGameIsReady, OnGameFinished onGameFinished,
            OnEnemyDCAccepted onEnemyDCAccepted, OnReconnectComplete onReconnectComplete,
            OnReconnectFailed onReconnectFailed, OnWeakConectionDetected onWeakConnection,
            bool isForExistingGame, OnRuleReady onRuleReady)
        {
            this.player = player;
            io = new SocketIOComponent(Constants.SOCKET_URL);
            reconnectMode = false;
            eventHandler = new EventHandler();
            lastDataTime = 0f;
            this.onDCCallBack = onDCCallBack;
            this.onWeakConnection = onWeakConnection;
            if (!isForExistingGame)
            {
                eventHandler.initializeForFirstTime(this, io, onRcCallBack, onDCCallBack, onGameIsReady,
                    onGameFinished, onEnemyDCAccepted, onReconnectComplete,
                    onReconnectFailed);
                //Thread acceptGameThread = new Thread(new ThreadStart(acceptGame));
                //acceptGameThread.Start();
            }
            else
            {
                (new Thread(() =>
                {
                    requestGameRule(onRcCallBack, onDCCallBack, onGameIsReady,
                        onGameFinished, onEnemyDCAccepted, onReconnectComplete, onReconnectFailed, onRuleReady);
                })).Start();
            }

        }

        private OnFindGameReqSendComplete onSendComplete;
        private OnGameFound onGameFound;

        public void connectToServer(OnFindGameReqSendComplete onSendComplete, OnGameFound onGameFound)
        {
            this.onSendComplete = onSendComplete;
            this.onGameFound = onGameFound;
            Thread reqGameThread = new Thread(new ThreadStart(connectToServer));
            reqGameThread.Start();
        }

        private void connectToServer()
        {
            if (!io.IsConnected)
            {
                io.Connect();
            }
            while (!io.IsConnected)
            {
                Thread.Sleep(100);
                Logger.debug("Trying to connect");
            }
            Logger.debug("Connection complete");
            gameHasStarted = false;
            //io.On(Strings.serverMessage.events.CONNECT, (SocketIOEvent obj) =>
            //{
            //    //Dictionary<string,string> info = new Dictionary<string,string>();
            //    //info["username"] = UserManager.instance.user.username;
            //    //info["password"] = UserManager.instance.user.password;
            //    //JSONObject jObj = new JSONObject(info);
            //    //io.Emit(Strings.serverMessage.events.REQUEST_NEW_GAME,jObj);
            //}
            //);
            io.On(Constants.serverMessage.events.DISCONNECT, onDisconnect);
            io.On(Constants.serverMessage.events.NOTIFY_FOUND_GAME, (SocketIOEvent obj) =>
            {
                Logger.debug("A game found");
                GameInfo info = new GameInfo();
                info.fillWithRawObj(obj);
                gameHasStarted = true;
                onGameFound(info);
            });
            Dictionary<string, string> data = new Dictionary<string, string>();
            //data[Strings.serverMessage.events.REQUEST_NEW_GAME_PARAMS.USERNAME] = UserManager.instance.user.username;
            //data[Strings.serverMessage.events.REQUEST_NEW_GAME_PARAMS.PASSWORD] = UserManager.instance.user.password;
            data[Constants.serverMessage.events.REQUEST_NEW_GAME_PARAMS.TOKEN] = Constants.HEADERS.SESSION + player.user.getAccessToken();
            JSONObject jObj = new JSONObject(data);
            bool reqComplete = false;
            io.On(Constants.serverMessage.events.REQUEST_NEW_GAME_SUC, (SocketIOEvent obj) =>
            {
                //Logger.debug("Requesting new game is successful");
                reqComplete = true;
            });
            while (!reqComplete)
            {
                io.Emit(Constants.serverMessage.events.REQUEST_NEW_GAME, jObj);
                Thread.Sleep(100);
            }
            onSendComplete();
        }

        private void onDisconnect(SocketIOEvent obj)
        {
            if (onDCCallBack != null)
            {
                Console.WriteLine("Disconnect call back");
                onDCCallBack();
            }
        }

        public void disconnectFromServer()
        {
            onDCCallBack = null;//For avoiding onDcCallback
            io.Close();
        }

        public void readyToAccept()
        {
            Thread acceptGameThread = new Thread(new ThreadStart(acceptGame));
            acceptGameThread.Start();
        }

        private void acceptGame()
        {
            if (!io.IsConnected)
            {
                io.Connect();
            }
            while (!io.IsConnected)
            {
                Thread.Sleep(100);
            }
            bool reqSuc = false;
            OnDisconnect temp = onDCCallBack;
            onDCCallBack = null;
            io.On(Constants.serverMessage.events.ACCEPT_RECIEVED, (SocketIOEvent obj) =>
            {
                onDCCallBack = temp;
                reqSuc = true;
            }
            );
            while (!reqSuc)
            {
                io.Emit(Constants.serverMessage.events.ACCEPT_GAME);
                Thread.Sleep(100);
            }
        }

        public void onLoadForExistingGameComplete(MultiplayerController parent, OnReciveCallBack onRcCallBack, OnDisconnect onDCCallBack,
            OnGameIsReady onGameIsReady, OnGameFinished onGameFinished, OnEnemyDCAccepted onEnemyDCAccepted,
            OnReconnectComplete onReconnectComplete, OnReconnectFailed onReconnectFailed,
            OnRuleReady onRuleReady, OnWeakConectionDetected onWeakConnection, bool isExistingGame)
        {
            this.onDCCallBack = onDCCallBack;
            this.onWeakConnection = onWeakConnection;
            (new Thread(() =>
            {
                requestGameRule(onRcCallBack, onDCCallBack, onGameIsReady,
                    onGameFinished, onEnemyDCAccepted, onReconnectComplete, onReconnectFailed, onRuleReady);
            })).Start();
        }

        private void requestGameRule(OnReciveCallBack onRcCallBack, OnDisconnect onDCCallBack, OnGameIsReady onGameIsReady,
            OnGameFinished onGameFinished, OnEnemyDCAccepted onEnemyDCAccepted,
            OnReconnectComplete onReconnectComplete, OnReconnectFailed onReconnectFailed, OnRuleReady onRuleReady)
        {
            io.Connect();
            while (!io.IsConnected)
            {
                io.On(Constants.serverMessage.events.CONNECT, (SocketIOEvent obj) =>
                {
                }
                );
                io.On(Constants.serverMessage.events.DISCONNECT, onDisconnect);
                Thread.Sleep(100);
            }
            eventHandler.initializeForFirstTime(this, io, onRcCallBack, onDCCallBack, onGameIsReady,
                    onGameFinished, onEnemyDCAccepted, onReconnectComplete, onReconnectFailed);
            bool ruleIsReady = false;
            io.On(Constants.serverMessage.events.GAME_RULE_READY, (obj) =>
            {
                ruleIsReady = true;
                GameInfo info = new GameInfo();
                string gameToken = player.user.getGameToken();
                string userToken = player.user.getUserToken();
                string enemyToken = player.user.getOpponentToken();
                string enemyName = player.user.getEnemyName();
                info.fillWithCommonData(obj, userToken, gameToken, enemyToken, enemyName);
                onRuleReady(info);
                player.user.setGameMode(Constants.gameMode.MULTI_PLAYER, info);
                Thread t = new Thread(new ThreadStart(retryConnection));
                t.Start();
            }
            );
            while (!ruleIsReady)
            {
                io.Emit(Constants.serverMessage.events.REQUEST_GAME_RULE);
                Thread.Sleep(100);
            }
        }

        private void onReady(Object obj)
        {
        }

        public void sendOrder(string message)
        {
            Console.WriteLine("Sending to server :\n" + message);
            JSONObject data = new JSONObject();
            data.AddField("message", message);
            io.Emit(Constants.serverMessage.events.USER_ORDER,data);
        }

        public void enemyDC(OnEnemyDCAccepted onEnemyDcAccepted, OnPlayerIsDC onPlayerIsDC)
        {
            io.Emit(Constants.serverMessage.events.ENEMY_DC);
            (new Thread(() =>
            {
                waitForPlayerToConnect(onEnemyDcAccepted, onPlayerIsDC);
            })).Start();
        }

        private void waitForPlayerToConnect(OnEnemyDCAccepted onEnemyDcAccepted, OnPlayerIsDC onPlayerIsDc)
        {
            bool responceRecieved = false;
            io.On(Constants.serverMessage.events.ENEMY_DC_ACCEPTED, (obj) =>
            {
                responceRecieved = true;
                Bounty bounty = new Bounty();
                bounty.fillWithRawData(obj.data);
                onEnemyDcAccepted(bounty);
            }
            );
            Thread.Sleep(2000);
            if (responceRecieved == false)
            {
                onPlayerIsDc();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus == true)
            {
                //Unfortunatly socket io cannot send data when application is suspended so I handle that on server
                //io.Emit(Strings.serverMessage.opCodes.TURN_FINISHED);
                io.Close();
                if (onDCCallBack != null)
                {
                    onDCCallBack();
                }
            }
            else if (pauseStatus == false && gameHasStarted)
            {
                new Thread(new ThreadStart(retryConnection)).Start();
            }
        }

        private void retryConnection()
        {
            Console.WriteLine("retryConnection methd");
            if (!io.IsConnected)
            {
                io.Connect();
            }
            while (!io.IsConnected)
            {
                Console.WriteLine("Attempting to reset connection");
                io.On(Constants.serverMessage.events.CONNECT, (obj) =>
                {
                    Console.WriteLine("reconnectng complete");
                }
                );
                io.On(Constants.serverMessage.events.DISCONNECT, onDisconnect);
                Thread.Sleep(1000);
            }
            eventHandler.reInitialize();
            string gameToken = player.user.getGameToken();
            string userToken = player.user.getUserToken();
            string opponentToken = player.user.getOpponentToken();
            if (gameToken != "" && userToken != "" && opponentToken != "")
            {
                //Debug.Log("Requsting gameInfo with"+gameToken+":"+userToken);
                JSONObject data = new JSONObject();
                data.AddField(Constants.gameInfo.GAME_TOKEN, gameToken.ToString().Replace("\"", string.Empty));
                data.AddField(Constants.gameInfo.GAME_TOKEN,gameToken.ToString().Replace("\"", string.Empty));
                data.AddField(Constants.gameInfo.USER_TOKEN,userToken.ToString().Replace("\"", string.Empty));
                data.AddField(Constants.gameInfo.ENEMY_TOKEN,opponentToken.ToString().Replace("\"", string.Empty));
                reconnectMode = true;
                bool requestArrived = false;
                io.On(Constants.serverMessage.events.RECONNECT_REQ_ARRIVE, (obj) =>
                {
                    requestArrived = true;
                }
                );
                while (!requestArrived)
                {
                    Console.WriteLine("Requesting gameInfo " + data.ToString());
                    io.Emit(Constants.serverMessage.events.RECONNECT, data);
                    Thread.Sleep(100);
                }
            }
        }

        public void cancelSearch(OnCancelFindGameAccepted onCancelAccepted)
        {
            if (io.IsConnected)
            {
                io.Emit(Constants.serverMessage.events.CANCEL_FIND_GAME);
                io.On(Constants.serverMessage.events.CANCEL_FIND_GAME_ACCEPTED, (obj) =>
                {
                    if (onCancelAccepted != null)
                    {
                        onCancelAccepted();
                    }
                }
                );
            }
            else
            {
                if (onCancelAccepted != null)
                {
                    onCancelAccepted();
                }
            }
        }

        public void pauseEmulator(bool pause)
        {
            OnApplicationPause(pause);
        }

        public void notifyServerPlayerRecComplete()
        {
            Console.WriteLine("Reconnecy complete");
            io.Emit(Constants.serverMessage.events.ON_PLAYER_RECONNECT_COMPLETE);
        }
    }
}
