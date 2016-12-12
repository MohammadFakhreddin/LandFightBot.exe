using LandFightBotReborn.Bot.DataType;
using LandFightBotReborn.DB;
using LandFightBotReborn.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.AI
{
    /// <summary>
    ///It's the parent of all AI classes every one must be extend from this class
    /// </summary>
    public abstract class AIBasic
    {
        public delegate bool Create(int unitId,int x,int y);
        public delegate bool EndTurn();
        public delegate bool Attack(int assignedId,int x,int y);
        public delegate bool Move(int assignedId, int newX, int newY);


        protected Create create;
        protected EndTurn endTurn;
        protected Attack attack;
        protected GameStatus gameStatus;
        protected User user;
        protected Move move;
        protected int mapXColumn;
        protected int mapYRow;

        public AIBasic(GameStatus gameStatus, User user, Create create,EndTurn endTurn,Attack attack,Move move,int mapXColumn,int mapYRow)
        {
            this.gameStatus = gameStatus;
            this.create = create;
            this.endTurn = endTurn;
            this.attack = attack;
            this.move = move;
            this.mapXColumn = mapXColumn;
            this.mapYRow = mapYRow;
            this.user = user;
        }

        public abstract void onAttack(int assignedId,int x,int y,List<HittedUnits> hittedUnits);
        public abstract void onCreate(int assignedId,int x,int y);
        public abstract void onEndTurn(bool myTurn);
        public abstract void onMove(int assignedId, int oldX, int oldY, int newX, int newY);
        public abstract void onEndGame(MultiplayerController.Bounty bounty);

        protected bool locationIsAly(int x)
        {
            if (x < mapXColumn / 2)
            {
                return true;
            }
            return false;
        }
    }
}
