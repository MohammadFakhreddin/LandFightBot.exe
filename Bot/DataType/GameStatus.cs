using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandFightBotReborn.DB;
using LandFightBotReborn.Utils;

namespace LandFightBotReborn.Bot.DataType
{
    public class GameStatus
    {
        /// <summary>
        /// All of the game collision is found here Units of the game are hashed in this 2d array that
        /// you can access in the easy and the fastest way
        /// </summary>
        public UnitController[][] unitMap;
        /// <summary>
        /// Aly coordinate start X(It is used to detect touch )
        /// </summary>
        public Vector2 alyTeretoryStart;
        /// <summary>
        /// Aly coordiantes endX(It is used to detect touch )
        /// </summary>
        public Vector2 alyTeretoryEnd;
        /// <summary>
        /// Enemy coordiante start X
        /// </summary>
        public Vector2 enemyTeretoryStart;
        /// <summary>
        /// Enemy coordinate end X
        /// </summary>
        public Vector2 enemyTeretoryEnd;
        /// <summary>
        /// Shows it's your turn or enemy turn
        /// </summary>
        public bool myTurn = true;
        /// <summary>
        /// Shows where aly teretory position ends in real time(in the middle of game)
        /// </summary>
        public int alyLandEndX = 0;
        /// <summary>
        /// Shows where enemy teretory position starts in real time(in the middle of game)
        /// </summary>
        public int enemyLandStartX = 0;
        public int myPower;
        public int enemyPower;
        public int myPowerRegen;
        public int enemyPowerRegen;
        public bool gameStarted = false;
        //public bool enemyHasNewDeadLand = false;
        //public bool iHaveNewDeadLand = false;
        /// <summary>
        /// Requesting to server
        /// </summary>
        public bool firstTurnIsMine=false;
        /// <summary>
        /// Right now turn number
        /// </summary>
        public int turn = 0;
    }
}
