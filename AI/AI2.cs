using LandFightBotReborn.Bot.DataType;
using LandFightBotReborn.DB;
using LandFightBotReborn.Network;
using LandFightBotReborn.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LandFightBotReborn.AI
{
    //By M.Fakhreddin
    public class AI2 : AIBasic
    {
        private UnitFeatures abiFeatures;
        private UnitFeatures bombieFeatures;
        private UnitFeatures maliousFeatures;
        private UnitFeatures enerjiousFeatures;
        private Dictionary<int, int> enemyCategoryCount;
        private Dictionary<int, List<UnitController>> myUnits;
        private Dictionary<int, UnitController> enemyKnownUnits;
        private const int DECK_SIZE = 4;
        private const int enerjousMax = 5;
        private const int maliosMax = 2;
        private const int bombiMax = 1;
        private const int abiMax = 2;
        private bool isFirstTurn = true;

        public AI2(GameStatus gameStatus, User user, Create create, EndTurn endTurn,
            Attack attack, Move move, int mapXColumn, int mapYRow)
        : base(gameStatus, user, create, endTurn, attack, move, mapXColumn, mapYRow)
        {
            abiFeatures = this.user.getAvailableFeatures(Constants.unitIds.BLUE);
            bombieFeatures = this.user.getAvailableFeatures(Constants.unitIds.BOMBI);
            maliousFeatures = this.user.getAvailableFeatures(Constants.unitIds.BOMBI);
            enerjiousFeatures = this.user.getAvailableFeatures(Constants.unitIds.ENERGIOUS);
            enemyCategoryCount = new Dictionary<int, int>();
            myUnits = new Dictionary<int, List<UnitController>>();
            myUnits[Constants.unitIds.BLUE] = new List<UnitController>();
            myUnits[Constants.unitIds.BOMBI] = new List<UnitController>();
            myUnits[Constants.unitIds.SHEILDER] = new List<UnitController>();
            myUnits[Constants.unitIds.ENERGIOUS] = new List<UnitController>();
            enemyKnownUnits = new Dictionary<int, UnitController>();
            isFirstTurn = true;
        }

        private Thread decisionThread;
        public override void onEndTurn(bool myTurn)
        {
            if (myTurn)
            {
                if (decisionThread != null)
                {
                    if (decisionThread.IsAlive)
                    {
                        decisionThread.Abort();
                    }
                    decisionThread = null;
                }
                decisionThread = new Thread(() =>
                {
                    while (myTurn && gameStatus.myPower > 300)
                    {
                        int chance = random(100);
                        bool changed = false;
                        if (myUnits[Constants.unitIds.BLUE].Count == 0 ||
                            (myUnits[Constants.unitIds.BLUE].Count < abiMax && chance > 75))
                        {
                            int xPos = 0;
                            int yPos = 0;
                            int tries = 0;
                            do
                            {
                                yPos = random(mapYRow);
                                xPos = gameStatus.alyLandEndX - 1;
                                tries++;
                            } while (gameStatus.unitMap[xPos][yPos] == null && tries < 10);
                            create(Constants.unitIds.BLUE, xPos, yPos);
                            changed = true;
                        }
                        else if ((enemyKnownUnits.Count > 0 && chance > 50) &&
                           (myUnits[Constants.unitIds.BOMBI].Count > 0 || myUnits[Constants.unitIds.BLUE].Count > 0))
                        {
                            int xPos = 0;
                            int yPos = 0;
                            int tries = 0;
                            do
                            {
                                yPos = random(mapYRow);
                                xPos = random(gameStatus.alyLandEndX - 1);
                                tries++;
                            } while (gameStatus.unitMap[xPos][yPos] == null && tries < 10);
                            create(Constants.unitIds.BOMBI, xPos, yPos);
                            changed = true;
                        }
                        else if ((myUnits[Constants.unitIds.ENERGIOUS].Count < enerjousMax && chance > 80) ||
                           (myUnits[Constants.unitIds.ENERGIOUS].Count < enerjousMax / 2 && chance > 40) || gameStatus.myPowerRegen < 400)
                        {
                            int xPos = 0;
                            int yPos = 0;
                            int tries = 0;
                            do
                            {
                                yPos = random(mapYRow);
                                xPos = random(gameStatus.alyLandEndX - 1);
                                tries++;
                            } while (gameStatus.unitMap[xPos][yPos] == null && tries < 10);
                            create(Constants.unitIds.ENERGIOUS, xPos, yPos);
                            changed = true;
                        }
                        //TOOD Malious
                        else if (!isFirstTurn && (myUnits[Constants.unitIds.BOMBI].Count > 0 || myUnits[Constants.unitIds.BLUE].Count > 0))
                        {
                            UnitController nearsetUnit = null;
                            try
                            {
                                foreach (UnitController unit in enemyKnownUnits.Values)
                                {
                                    if (unit.getFeatures().health <= 0)
                                    {
                                        enemyKnownUnits.Remove(unit.getAssignedId());
                                    }
                                    if (unit != null && (nearsetUnit == null ||
                                    nearsetUnit.getGameMapPosition().x > unit.getGameMapPosition().x))
                                    {
                                        nearsetUnit = unit;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                            UnitController attacker = null;
                            if (myUnits[Constants.unitIds.BOMBI].Count > 0)
                            {
                                attacker = myUnits[Constants.unitIds.BOMBI][0];
                                myUnits[Constants.unitIds.BOMBI].RemoveAt(0);
                            }
                            else if (attacker == null && myUnits[Constants.unitIds.BLUE].Count > 0)
                            {
                                attacker = myUnits[Constants.unitIds.BLUE][0];
                            }
                            if (attacker == null)
                            {
                                throw new System.NullReferenceException();
                            }
                            if (nearsetUnit == null)
                            {
                                int randomX = gameStatus.enemyLandStartX + 1 + random(mapXColumn - gameStatus.enemyLandStartX - 1);
                                int randomY = random(mapYRow);
                                attack(attacker.getAssignedId(), randomX
                               , randomY);
                            }
                            else
                            {
                                attack(attacker.getAssignedId(), (int)nearsetUnit.getGameMapPosition().x
                                    , (int)nearsetUnit.getGameMapPosition().y);
                            }
                            changed = true;
                        }
                        if (changed && gameStatus.myPower > 300)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    if (myTurn)
                    {
                        endTurn();
                    }
                }
                );
                decisionThread.Start();
                isFirstTurn = false;
            }
        }

        public override void onCreate(int assignedId, int x, int y)
        {
            Logger.debug("In on create");
            UnitController createdUnit = findWithAssignedId(assignedId);
            if (createdUnit == null)
            {
                throw new System.ArgumentException("CreatedUnit not found");
            }
            if (!createdUnit.getIsAly())
            {
                Logger.debug("is enemy is true");
                if (enemyCategoryCount.ContainsKey(createdUnit.getFeatures().id))
                {
                    enemyCategoryCount[createdUnit.getFeatures().id] += 1;
                }
                else
                {
                    enemyCategoryCount.Add(createdUnit.getFeatures().id, 1);
                }
                if (createdUnit.getGameMapPosition().x == gameStatus.enemyLandStartX + 1)
                {
                    enemyKnownUnits.Add(createdUnit.getAssignedId(), createdUnit);
                    Logger.debug("New enemy detected");
                }
            }
            else
            {
                if (myUnits.ContainsKey(createdUnit.getFeatures().id))
                {
                    myUnits[createdUnit.getFeatures().id].Add(createdUnit);
                }
                else
                {
                    myUnits.Add(createdUnit.getFeatures().id, new List<UnitController>());
                    myUnits[createdUnit.getFeatures().id].Add(createdUnit);
                }
            }
        }

        public override void onMove(int assignedId, int oldX, int oldY, int newX, int newY)
        {
        }

        public override void onEndGame(MultiplayerController.Bounty bounty)
        {
        }

        public override void onAttack(int assignedId, int attackX, int attackY, List<HittedUnits> hittedUnits)
        {
            UnitController attacker = findWithAssignedId(assignedId);
            if (attacker == null)
            {
                throw new System.NullReferenceException("Assigned id not found");
            }
            if (attacker.getIsAly())
            {
                for (int i = 0; i < hittedUnits.Count; i++)
                {
                    UnitController unit = findWithAssignedId(hittedUnits[i].assignedId);
                    if (unit != null)
                    {
                        if (unit.getFeatures().health > 0)
                        {
                            if (!enemyKnownUnits.ContainsKey(hittedUnits[i].assignedId))
                            {
                                enemyKnownUnits.Add(hittedUnits[i].assignedId, unit);
                            }
                        }
                        else
                        {
                            if (enemyKnownUnits.ContainsKey(hittedUnits[i].assignedId) && unit.getFeatures().health < 0)
                            {
                                enemyKnownUnits.Remove(hittedUnits[i].assignedId);
                            }
                        }
                    }
                    else
                    {
                        //throw new System.NullReferenceException("unit not found");
                    }
                }
            }
            else
            {
                enemyKnownUnits.Add(attacker.getAssignedId(), attacker);
                for (int i = 0; i < hittedUnits.Count; i++)
                {
                    UnitController unit = findWithAssignedId(hittedUnits[i].assignedId);
                    if (unit == null)
                    {
                        //throw new System.NullReferenceException("unit not found");
                    }
                    if (unit.getFeatures().health <= 0)
                    {
                        myUnits[hittedUnits[i].assignedId].Remove(unit);
                    }
                }
            }
        }

        private int random(int size)
        {
            Random rnd = new Random();
            return rnd.Next(0, size);
        }

        //private Vector2 random(int sizeX, int sizeY)
        //{
        //    Random rnd = new Random();
        //    return new Vector2(rnd.Next(0, sizeX), rnd.Next(0, sizeY));
        //}

        private UnitController findWithAssignedId(int assignedId)
        {
            for (int i = 0; i < gameStatus.unitMap.Length; i++)
            {
                for (int j = 0; j < gameStatus.unitMap[i].Length; j++)
                {
                    if (gameStatus.unitMap[i][j] != null && gameStatus.unitMap[i][j].getAssignedId() == assignedId) return gameStatus.unitMap[i][j];
                }
            }
            return null;
        }
    }
}
