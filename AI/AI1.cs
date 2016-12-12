using LandFightBotReborn.Bot.DataType;
using LandFightBotReborn.DB;
using LandFightBotReborn.Network;
using LandFightBotReborn.Utils;
using System;
using System.Collections.Generic;

namespace LandFightBotReborn.AI
{   
    public class AI1:AIBasic
    {
        int state;
        bool notEnoughPower;
        bool hasAttackedOnce;
        int maxBikhasiatNum = 1;
        int maxEnergiousNum = 2;
        int maxCocholiousNum = 1;
        int maxAttackorNumber = 1;
        int maxBlueNum = 2;
        UnitFeatures bikhasiat;
        UnitFeatures cocholious;
        UnitFeatures blue;
        UnitFeatures energious;
        List<Vector2> readyAttackorPlace = new List<Vector2>();
        List<Vector2> readyCocholiousPlace = new List<Vector2>();
        List<Vector2> readyBluePlace = new List<Vector2>();
        List<Vector2> enemyHiddenPlace = new List<Vector2>();
        List<Vector2> tryedNotEnemyPlace = new List<Vector2>();


        String gamePlay = "";
        int numberOfTurnPlayed = 0;
        public AI1(GameStatus gameStatus , User user , Create create, EndTurn endTurn, Attack attack, Move move, int mapXColumn, int mapYRow)
        :base(gameStatus,user , create,endTurn,attack,move,mapXColumn,mapYRow)
        {
            initializeCards();
        }

        private void initializeCards()
        {
            bikhasiat = user.getAvailableFeatures(Constants.unitIds.BIKHASIAT);
            cocholious = user.getAvailableFeatures(Constants.unitIds.KOLOCHIOUS);
            blue = user.getAvailableFeatures(Constants.unitIds.BLUE);
            energious = user.getAvailableFeatures(Constants.unitIds.ENERGIOUS);
        }

        public override void onEndTurn(bool myTurn)
        {
            Logger.info("------------------------> onEndTurn " + myTurn);
            if (myTurn)
            {
                Console.WriteLine("-------------------------------------------------------------------------");
                Logger.info("endTurnCalled");
                numberOfTurnPlayed++;
                String currentMap = "\n";
                for (int i = 0; i < mapYRow; i++)
                {
                    for (int j = 0; j < mapXColumn; j++)
                    {
                        if (mapXColumn / 2 == j + 1 || mapXColumn / 2 == j) currentMap += "|";
                        else
                        {
                            if (gameStatus.unitMap[j][i] == null) currentMap += "0";
                            else if (gameStatus.unitMap[j][i].getIsAly()) currentMap += "1";
                            else currentMap += "2";
                        }
                    }
                    currentMap += "\n";
                }
                currentMap += "\n";
                Logger.info(currentMap);
                notEnoughPower = false;
                state = checkYourState();
                doThis(state);
                //endTurn();
                Console.WriteLine("-------------------------------------------------------------------------");
            }

        }

        private int checkYourState()
        {
            bool nextStateIsAllowed = true;
            int state = 0;
            int bikhasiatfound = 0;
            for (int i = 0;i<mapYRow; i++)
                if (gameStatus.unitMap[gameStatus.alyLandEndX-1][i] != null &&
                    gameStatus.unitMap[gameStatus.alyLandEndX-1][i].getFeatures().id == Constants.unitIds.BIKHASIAT)
                    bikhasiatfound ++;
            if (bikhasiatfound < maxBikhasiatNum) nextStateIsAllowed = false;
            if (nextStateIsAllowed) state++;
            int energiousFound = 0;
            for (int i = 0; i < mapYRow; i++)
                for (int j = 0; j < gameStatus.alyLandEndX-1; j++)
                    if (gameStatus.unitMap[j][i] != null && 
                        gameStatus.unitMap[j][i].getFeatures().id == Constants.unitIds.ENERGIOUS)
                        energiousFound++;
            if (energiousFound < maxEnergiousNum) nextStateIsAllowed = false;
            if (nextStateIsAllowed) state++;
            int cochooliousFound = 0;
            readyCocholiousPlace.Clear();
            for (int i = 0; i < mapYRow; i++)
                for (int j = 0; j < gameStatus.alyLandEndX-1; j++)
                    if (gameStatus.unitMap[j][i] != null &&
                        gameStatus.unitMap[j][i].getFeatures().id == Constants.unitIds.KOLOCHIOUS &&
                        gameStatus.unitMap[j][i].getAvailableShots()>0)
                    {
                        cochooliousFound++;
                        readyAttackorPlace.Add(new Vector2(j, i));
                    }
            int blueFound = 0;
            readyBluePlace.Clear();
            for (int i = 0; i < mapYRow; i++)
                for (int j = 0; j < gameStatus.alyLandEndX-1; j++)
                    if (gameStatus.unitMap[j][i] != null &&
                        gameStatus.unitMap[j][i].getFeatures().id == Constants.unitIds.BLUE &&
                        gameStatus.unitMap[j][i].getAvailableShots() > 0)
                    {
                        blueFound++;
                        readyAttackorPlace.Add(new Vector2(j, i));
                    }
            if (cochooliousFound+blueFound < maxAttackorNumber) nextStateIsAllowed = false;
            if (nextStateIsAllowed) state++;
            return state;
        }

        public void doThis(int state)
        {
            Logger.info("I am in doThis with state : " + state);
            switch (state)
            {
                case 0:
                    int validRandom = random(mapYRow);
                    while (gameStatus.unitMap[gameStatus.alyLandEndX - 1][validRandom] != null) validRandom = random(mapYRow);
                    notEnoughPower = !create(Constants.unitIds.BIKHASIAT, gameStatus.alyLandEndX - 1,validRandom);
                    break;
                case 1:
                    Vector2 validCoordiante = random( gameStatus.alyLandEndX - 1, mapYRow);
                    while (gameStatus.unitMap[(int)(validCoordiante.x)][(int)(validCoordiante.y)] != null) validCoordiante = random(gameStatus.alyLandEndX - 1, mapYRow);
                    notEnoughPower = !create(Constants.unitIds.ENERGIOUS, (int)validCoordiante.x, (int)validCoordiante.y);
                    break;
                case 2:
                    int randomNumber = random(2);
                    switch (randomNumber)
                    {
                        case (0):
                            validCoordiante = random( gameStatus.alyLandEndX - 1, mapYRow);
                            while (gameStatus.unitMap[(int)(validCoordiante.x)][(int)(validCoordiante.y)] != null) validCoordiante = random(gameStatus.alyLandEndX - 1 , mapYRow);
                            notEnoughPower = !create(Constants.unitIds.BLUE, (int)validCoordiante.x, (int)validCoordiante.y);
                            break;
                        case (1):
                            validCoordiante = random( gameStatus.alyLandEndX - 1, mapYRow);
                            while (gameStatus.unitMap[(int)(validCoordiante.x)][(int)(validCoordiante.y)] != null) validCoordiante = random(gameStatus.alyLandEndX - 1 , mapYRow);
                            notEnoughPower = !create(Constants.unitIds.KOLOCHIOUS, (int)validCoordiante.x, (int)validCoordiante.y);
                            break;
                    }
                    break;
                case 3:
                    bool isEmpty = true;
                    List<Vector2> shouldAttack= new List<Vector2>();
                    for (int i = 0; i < mapYRow; i++)
                        if (gameStatus.unitMap[gameStatus.enemyLandStartX + 1][i] != null )
                        {
                            isEmpty = false;
                            if (gameStatus.unitMap[gameStatus.enemyLandStartX + 1][i].getFeatures().id != Constants.unitIds.BIKHASIAT)
                                shouldAttack.Add(new Vector2(i, gameStatus.enemyLandStartX + 1));
                        }
                    Vector2 firstAttackor = readyAttackorPlace[0];
                    if (isEmpty == true) {
                        int rand = random(mapYRow);
                        Logger.info("attacking to empty place from :" + (firstAttackor.x) + (firstAttackor.y) + " to " + gameStatus.enemyLandStartX + 1 + rand);
                        notEnoughPower = attack(gameStatus.unitMap[(int)(firstAttackor.x)][(int)(firstAttackor.y)].getAssignedId(), 
                            gameStatus.enemyLandStartX + 1  , rand);
                    } else if(enemyHiddenPlace != null && enemyHiddenPlace.Count != 0)
                    {
                        if (enemyHiddenPlace != null && enemyHiddenPlace.Count!= 0)
                        {
                            Logger.info("attacking to enemy hidden place");
                            notEnoughPower = attack(gameStatus.unitMap[(int)(firstAttackor.x)][(int)(firstAttackor.y)].getAssignedId(), 
                                (int) enemyHiddenPlace[0].x,(int) enemyHiddenPlace[0].y);
                        }
                    } else
                    {
                        Logger.info("attacking to random place");
                        int rand = random(mapXColumn - (gameStatus.enemyLandStartX));
                        notEnoughPower = attack(gameStatus.unitMap[(int)(firstAttackor.x)][(int)(firstAttackor.y)].getAssignedId(),
                                rand, random(mapYRow) );
                    }
                    break;
            }
            if (notEnoughPower) endTurn();
        }








        public override void onCreate(int assignedId, int x, int y)
        {
            bool isMyTurn = (x < mapXColumn / 2);
            Logger.info("------------------------> onCreate " + isMyTurn);
            if (isMyTurn)
            {
                state = checkYourState();
                Logger.info(state.ToString());
                doThis(state);
            }
        }

        public override void onMove(int assignedId, int oldX, int oldY, int newX, int newY)
        {
     
        }

        public override void onEndGame(MultiplayerController.Bounty bounty)
        {

        }

        public override void onAttack(int assignedId, int x, int y, List<HittedUnits> hittedUnits)
        {
            bool isMyTurn = (x > mapXColumn / 2);
            Logger.info("------------------------> onAttack " + isMyTurn);
            if (isMyTurn)
            {
                //if (hittedUnits == null && hittedUnits.Count == 0) {
                //    enemyHiddenPlace.Remove(enemyHiddenPlace[0]);
                //}
                state = checkYourState();
                Logger.info(state.ToString());
                doThis(state);
            }
            else
            {
                //Vector2 enemyPlace = findWithAssignedId(assignedId);
                //if (enemyPlace!= null)
                //    enemyHiddenPlace.Add(new Vector2(enemyPlace.x, enemyPlace.y));
            }
        }

        private int random(int size)
        {
            Random rnd = new Random();
            return rnd.Next(0, size);
        }

        private Vector2 random(int sizeX , int sizeY)
        {
            Random rnd = new Random();
            return new Vector2(rnd.Next(0, sizeX), rnd.Next(0, sizeY));
        }

        private Vector2 findWithAssignedId(int assignedId)
        {
            for(int i = 0; i < mapXColumn; i++)
            {
                for(int j = 0; j < mapYRow; j++)
                {
                    if (gameStatus.unitMap[i][j] != null && gameStatus.unitMap[i][j].getAssignedId() == assignedId) return new Vector2(i, j);
                }
            }
            return null;
        }
    }
}
