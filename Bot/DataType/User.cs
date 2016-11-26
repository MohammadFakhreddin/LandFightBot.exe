using System;
using System.Collections.Generic;
using LandFightBotReborn.DB;
using LandFightBotReborn.Network;
using LitJson;

namespace LandFightBotReborn.Bot.DataType
{
    public class User
    {
        private string username;
        private string password;

        private static class SERVER_VALUES
        {
            public static string EMAIL = "email";
            public static string EMAIL_IS_VALID = "emailIsValid";
            public static string GOLD = "gold";
            public static string XP = "xp";
            public static string LEVEL = "level";
            public static string TROPHY = "trophy";
            public static string CAN_ASSIGN_NAME = "canAssignName";
            public static string WINS = "wins";
            public static string THREE_STARS_WIN = "threeStarWins";
            public static string TOTAL_GAME_PLAYED = "totalGamesPlayed";
            public static string CARDS = "cards";
            public static class CARD_VALUES
            {
                public static string CARD_ID = "cardId";
                public static string LEVEL = "level";
                public static string IS_IN_DECK = "isInDeck";
            }
            public static string RESULTS = "gameResults";
            public static class RESULTS_VALUE
            {
                public static string USER_SCORE = "userScore";
                public static string OPP_SCORE = "oppScore";
                public static string OPP_NAME = "oppName";
                public static string OPP_USER_ID = "oppUserId";
            }
        }
        public class UnitInfo
        {
            public int cardId;
            public int level;
            public bool isInDeck;
            public void fillWithRawObj(JsonData rawObj)
            {
                cardId = int.Parse(rawObj[SERVER_VALUES.CARD_VALUES.CARD_ID].ToString());
                level = int.Parse(rawObj[SERVER_VALUES.CARD_VALUES.LEVEL].ToString());
                isInDeck = Boolean.Parse(rawObj[SERVER_VALUES.CARD_VALUES.IS_IN_DECK].ToString());
            }
            public UnitInfo(int id, int level, bool isInDeck){
                cardId = id;
                this.level = level;
                this.isInDeck = isInDeck;
            }
            public UnitInfo(){

            }
        }
        /// <summary>
        /// It indicates user current card if one card is not in list it is not included
        /// </summary>
        private List<UnitInfo> playerCards;
        private int level=1;
        private int gameMode;
        private string accessToken;
        public List<UnitFeatures> features;

        /// <summary>
        /// For retriving game after dc we must fill this part it also have game rules
        /// </summary>
        private MultiplayerController.GameInfo info;

        public User(int gameMode,string username,string password,List<UnitFeatures> features)
        {
            this.gameMode = gameMode;
            this.username = username;
            this.password = password;
            this.features = features;
        }

        public string getAccessToken()
        {
            return accessToken;
        }

        public UnitFeatures getAvailableFeatures(int unitId)
        {
            for (int i = 0; i < features.Count; i++)
            {
                if (features[i].id == unitId)
                {
                    return features[i];
                }
            }
            return null;
        }

        public void setSession(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public void setGameMode(int gameMode)
        {
            this.gameMode = gameMode;
        }

        public void setGameMode(int gameMode, MultiplayerController.GameInfo info)
        {
            this.gameMode = gameMode;
            this.info = info;
        }

        public string getGameToken()
        {
            if (info != null && info.gameId != "")
            {
                return info.gameId;
            }
            return "";
        }

        public string getUserToken()
        {
            if (info != null && info.userId != "")
            {
                return info.userId;
            }
            return "";
        }

        public string getEnemyName()
        {
            if (info != null && info.enemyName != "")
            {
                return info.enemyName;
            }
            return "";
        }

        public string getOpponentToken()
        {
            if (info != null && info.enemyId != "")
            {
                return info.enemyId;
            }
            return "";
        }

        public int getGameMode()
        {
            return gameMode;
        }

        public MultiplayerController.GameInfo getGameInfo()
        {
            return info;
        }

        /// <summary>
        /// When player reconnects
        /// </summary>
        /// <param name="gameToken">Id of game we are playing</param>
        /// <param name="pSocketId">PlayerId</param>
        /// <param name="eSocketId">EnemyId</param>
        public void setUpNewTokens(string gameToken, string pSocketId, string eSocketId)
        {
            info.gameId = gameToken;
            info.userId = pSocketId;
            info.enemyId = eSocketId;
        }

        public void fillWithRawObj(JsonData rawObj)
        {
            level = int.Parse(rawObj[SERVER_VALUES.LEVEL].ToString());
            playerCards = new List<UnitInfo>();
            JsonData rawCards = rawObj[SERVER_VALUES.CARDS];
            Console.WriteLine("MyCards:  " + JsonMapper.ToJson(rawCards).ToString());
            for (int i = 0; i < rawCards.Count; i++)
            {
                UnitInfo info = new UnitInfo();
                info.fillWithRawObj(rawCards[i]);
                playerCards.Add(info);
            }
        }

        public void fillPlayerCards(JsonData rawCards)
        {
            playerCards = new List<UnitInfo>();
            Console.WriteLine("MyCards:  " + JsonMapper.ToJson(rawCards).ToString());
            for (int i = 0; i < rawCards.Count; i++)
            {
                UnitInfo info = new UnitInfo();
                info.fillWithRawObj(rawCards[i]);
                playerCards.Add(info);
            }
            updateAllUnitFeatures();
        }

        public UnitInfo getPlayCard(int id) {
            for(int i = 0; i < playerCards.Count; i++) {
                if(playerCards[i].cardId == id) {
                    return playerCards[i];
                }
            }
            return null;
        }

        /// <summary>
        /// It updates features from playerCards
        /// </summary>
        public void updateAllUnitFeatures()
        {
            for (int i = 0; i < this.features.Count; i++)
            {
                User.UnitInfo unitInfo = getPlayCard(this.features[i].id);
                if (unitInfo == null)
                {
                    features[i].isLocked = true;
                    features[i].isInDeck = false;
                    features[i].currentLevel = 0;
                }
                else
                {
                    features[i].isLocked = false;
                    features[i].isInDeck = unitInfo.isInDeck;
                    features[i].currentLevel = unitInfo.level;
                }
            }
        }
    }
}