using LandFightBotReborn.Bot.DataType;
using LandFightBotReborn.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.AI
{
    /// <summary>
    /// It is an example for implementing and AI class
    /// </summary>
    public class AI1:AIBasic
    {
        public AI1(GameStatus gameStatus, Create create, EndTurn endTurn, Attack attack, Move move, int mapXColumn, int mapYRow)
        :base(gameStatus,create,endTurn,attack,move,mapXColumn,mapYRow)//You have the refrence to the varaiables
        {

        }

        public override void onAttack(int assignedId, int x, int y, List<HittedUnits> hittedUnits)
        {
        
        }

        public override void onEndTurn(bool myTurn)
        {

        }

        public override void onCreate(int assignedId, int x, int y)
        {
        
        }

        public override void onMove(int assignedId, int oldX, int oldY, int newX, int newY)
        {
     
        }

        public override void onEndGame(MultiplayerController.Bounty bounty)
        {

        }
    }
}
