using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn
{
    public class Constants
    {
        public static string GAME_NAME = "landFight";
        //public static string GAME_URL = "http://landfight.ddns.net/";
        //public static string GAME_URL = "http://192.99.103.114/";
        public static string GAME_URL = "http://192.168.137.1/";
        //public static string GAME_URL = "http://192.168.1.6/";
        public static class HEADERS
        {
            public static string SET_COOKIE = "SET-COOKIE";
            public static string COOKIE = "cookie";
            public static string SESSION = "RadxSession=";
        }

        public static string REGISTER_URL = "users/register";
        public static class REGISTER_FIELDS
        {
            //TODO We need device id to login
            //public static string[] BODY_PARAMS = new string[]{
            //    "UUID"  
            //};
            public static class RESPONCE
            {
                public static string USERNAME = "username";
                public static string PASSWORD = "password";
                //public static string TOKEN = "landfighttoken";
                public static string USER = "user";
                public static string SUCCESS = "success";
            }
        };
        public static string LOGIN_URL = "users/login";
        public static class LOGIN_FIELDS
        {
            public static string[] BODY_PARAMS = new string[]{
            "username",
            "password"
        };
            public static class RESPONCE
            {
                public static string STATUS = "status";
                //public static string TOKEN = "landfighttoken";
                public static string ERR = "err";
                public static string SUCCESS = "success";
                public static string USER = "user";
            }
        };


        public static class CHANGE_PASSWORD
        {
            public static string CHANGE_PASSWD_URL = "users/changePassword";
            public static string[] PARAMS = new string[] {"username", "password", "newPassword" };
            public static class RESPONSE
            {
                public static string SUCCESS = "success";
                public static string STATUS = "status";
                public static string NEW_PASSWORD = "password";
            }
        }

        public static string SOCKET_URL = "ws://192.99.103.114:80/socket.io/?EIO=4&transport=websocket";
        public static class SOCKET_FIELDS
        {
            public static string[] PARAMS = new string[]
            {
                "landfighttoken"
            };
        }

        public static class GAME_RESULT
        {
            public static string[] PARAMS = new string[] { /*"username", "password", "landfighttoken"*/ };
            public static string GAME_RESULT_URL = "users/gameResults";
            public static class RESPONSE
            {
                public static string STATUS = "status";
                public static string SUCCESS = "success";
                public static string ERR = "err";
                public static string RESULTS = "gameResults";
            }
            public static class RESULTS_VALUE
            {
                public static string USER_SCORE = "userScore";
                public static string OPP_SCORE = "oppScore";
                public static string OPP_NAME = "oppName";
                public static string OPP_USER_ID = "oppUserId";
            }
        }

        public static class LeaderBoard
        {
            public static string[] PARAMS = new string[] { /*"username", "password", "landfighttoken" */};
            public static string TOP_LEADER_BOARD_URL = "users/bestLeaderBoard";
            public static string LOCAL_LEADER_BOARD_URL = "users/myLeaderBoard";
            public class TopLeaderBoardResponse
            {
                public static string STATUS = "status";
                public static string ERR = "err";
                public static string SUCCESS = "success";
                public static string TOP_USERS = "topUsers";
            }
            public class LocalLeaderBoardResponse
            {
                public static string STATUS = "status";
                public static string ERR = "err";
                public static string SUCCESS = "success";
                public static string LOCAL_USERS = "localUsers";
            }

            public static class UserInfo
            {
                public static string USER_ID = "userId";
                public static string TROPHY = "trophy";
                public static string USER_NAME = "username";
            }

            public static class UserBio
            {
                public static string USER_BIO_URL = "users/getUserShortBio";
                public static string[] PARAMS = new string[] {/*"landfighttoken",*/"userId" };
                public static class RESPONSE
                {
                    public static string BIO = "bio";
                    public static string STATUS = "status";
                    public static string SUCCESS = "success";
                    public static string ERR = "err";
                    public static string USERNAME = "username";
                    public static string GOLD = "gold";
                    public static string LEVEL = "level";
                    public static string XP = "xp";
                    public static string TROPHY = "trophy";
                    public static string WINS = "wins";
                    public static string THREE_STARS_WINS = "threeStarWins";
                    public static string TOTAL_GAMES_PLAYED = "totalGamesPlayed";
                }
            }
        }
        public static class gameInfo
        {
            public static int GAME_VERSION = 2;//Each time we update game we must change this
            public static string GAME_TOKEN = "gameToken";
            public static string USER_TOKEN = "pSocketId";
            public static string ENEMY_TOKEN = "eSocketId";
            public static string ENEMY_NAME = "enemyName";
            public static string DB_VERSION = "dbVersion";
            public static string SESSION_TOKEN = "sessionToken";
        }
        public static class gameMode
        {
            public static int SINGLE_PLAYER = 0;
            public static int LOCAL_GAME_SERVER = 1;
            public static int LOCAL_GAME_CLIENT = 2;
            public static int MULTI_PLAYER = 3;
            public static int RECONNECT_FOR_START = 4;
        }
        public static class serverMessage
        {
            public static class opCodes
            {
                public static char SEPERATOR = '@';
                public static char ORDER_ID_SEPERATOR = '#';
                public static string SERVER_READY = "0";
                public static string GAME_STATUS = "1";
                public static string TURN_FINISHED = "2";//xLand,enemyxLand,power,power,powerRegen,powerRegen
                public static string WIN_GAME = "3";
                public static string LEAVE_GAME = "4";
                public static string MOVE_UNIT = "5";//UnitId,x,y,power
                public static string ATTACK_UNIT = "6";//x,y,x,y,hittedUnits
                public static string HIT = "16";//hittedUnits=unitId,damage
                public static class HITED_UNITS_PARAMS
                {
                    public static char PART_SEP = '%';
                    public static char MEM_SEP = ',';
                }
                public static string CREATE_UNIT = "7";//x,y,unitId
                public static string CLIENT_READY = "8";
                public static string DRAW_GAME = "10";
                public static string LOOSE_GAME = "11";
                public static string KILL_UNIT = "12";
                public static string START_MULTI_AS_FIRST = "13";
                public static string START_MULTI_AS_SECOND = "14";
                public static string USE_ITEM = "15";
                public static string FORCE_MOVE = "17";//UnitId,x,y
                public static string CREATE_FAILED = "18";//UnitId
                public static string CREATE_ACCEPTED = "19";//UnitId
                public static char END_MESSAGE = '#';
            }
            /*
             * Start game is in handsahke mode server says i'm ready then client says that and after these two things 
             * we start the game
             */
            public static class events
            {
                public static string CONNECT = "connect";//It is a reserved value do not change it
                public static string DISCONNECT = "disconnect";//It is reserverd value            
                public static string REQUEST_NEW_GAME = "request_new_game";
                public static string REQUEST_NEW_GAME_SUC = "request_new_game_successful";
                public static class REQUEST_NEW_GAME_PARAMS
                {
                    public static string TOKEN = "session";
                }
                public static string CANCEL_FIND_GAME = "cancel_find_game";
                public static string CANCEL_FIND_GAME_ACCEPTED = "cancel_find_game_accepted";
                public static string ENEMY_TIMEOUT = "enemy_timeout";
                public static string NOTIFY_FOUND_GAME = "notify_found_game";
                public static string ACCEPT_GAME = "accept_game";
                public static string ACCEPT_RECIEVED = "accept_received";
                public static string START_GAME = "start_game";
                public static string LOOSE_GAME = "loose_game";
                public static string ENEMY_LEAVE = "enemy_leave";
                public static string ENEMY_DC = "enemy_dc";
                public static string ENEMY_DC_ACCEPTED = "enemy_dc_accepted";
                public static string END_GAME = "end_game";
                public static class endGameWinLooseCodes
                {
                    public static int WIN = 1;
                    public static int LOOSE = 2;
                    public static int DRAW = 3;
                    public static int LEAVE = 4;
                }
                public static string USER_ORDER = "user_order";//We send every player move by this order
                public static string SERVER_ORDER = "server_order";//Server sends us what really happens in game
                public static string RECONNECT = "reconnect";
                public static string RECONNECT_REQ_ARRIVE = "reconnect_req_arrive";
                public static string ON_GAME_STATUS_READY = "on_game_status_ready";
                public static string ON_RECONNECT_FAILED = "on_reconnect_failed";
                public static string REQUEST_GAME_RULE = "request_game_rule";
                public static string GAME_RULE_READY = "game_rule_ready";
                public static string NEW_TOKENS = "new_tokens";
                public static string ON_PLAYER_RECONNECT_COMPLETE = "on_player_reconnect_complete";
                public static string CORRECTION = "correction";
                public static string PING = "ping";
                public static string PING_SUC = "ping_suc";
            }
        }
    }
}
