using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.Bot.DataType
{
    public class GameElements
    {
        public static GameElements instance;
        private Unit[][] unitMap;
        private bool myTurn = true;
        private int myPower;
        private int enemyPower;
        private int myPowerRegen;
        private int enemyPowerRegen;
        private bool gameStarted = false;
        private bool firstTurnIsMine = false;
        private int turn = 0;
        private int numberOfMapXColumn = 10;
        private int numberOfMapYRow = 10;
        private Hashtable availableFeatures;

        public GameElements()
        {
            if (instance != null) return;
            instance = this;
            gameStarted = false;
            unitMap = new Unit[numberOfMapXColumn][];
            for (int i = 0; i < numberOfMapXColumn; i++)
            {
                for (int j = 0; j < numberOfMapYRow; j++)
                {
                    unitMap[i][j] = null;
                }
            }
            UnitManager unitManager = new UnitManager();
            UnitManager.instance.selectAll();
            List<UnitFeatures> cardsFeatures = UnitManager.instance.getAllUnitFeatures();
            for (int i = 0; i < cardsFeatures.Count; i++) availableFeatures.Add(cardsFeatures[i].id, cardsFeatures[i]);
        }

        //////settings
        public bool createNewUnit(Utils.Vector2 position, int unitId, bool isAly, int level, int assignedId)
        {
            if (unitMap[position.x][position.y] != null) return false;
            Unit unit = new Unit();
            UnitFeatures newUnitFeatures = (unitAvailableFeatures[unitId].clone();
            unit.setFeatures(newUnitFeatures, position, isAly, level, assignedId);
            unitMap[position.x][position.y] = unit;
            return true;
            //TODO power manager should be set.
        }

        public bool attack(Utils.Vector2 attackerPos, Utils.Vector2 attackedPos, List<Utils.HittedUnits> hittedUnits)
        {
            if (unitMap[attackedPos.x][attackerPos.y] == null) return false;
            for (int i = 0; i < unitMap.Length; i++)
            {
                for (int j = 0; j < unitMap[i].Length; j++)
                {
                    if (unitMap[i][j] != null)
                    {
                        for (int k = 0; k < hittedUnits.Count; k++)
                        {
                            if (hittedUnits[k].assignedId == unitMap[i][j].getAssignedId())
                            {
                                int damage = hittedUnits[k].damage;
                                unitMap[i][j].hit(damage);
                                unitMap[i][j].addAbility(new Unit.Ability(doer));
                                hittedUnits.RemoveAt(k);
                                break;
                            }
                        }
                    }
                }
            }
            return true;
            //TODO power manager should be set.
        }

        public bool move(Utils.Vector2 movingUnitPos, Utils.Vector2 mapPos)
        {
            Unit movingUnit = unitMap[(int)movingUnitPos.x][(int)movingUnitPos.y];
            if (movingUnit == null) return false;
            Unit temp = unitMap[mapPos.x][mapPos.y];
            unitMap[mapPos.x][mapPos.y] = temp;
            unitMap[mapPos.x][mapPos.y].setLocation(mapPos);
            return true;
            //TODO power manager should be set.
        }

        public bool onReconnectComplete(Utils.ReGameStatus newStatus)
        {
            List<Utils.UnitState> newUnits = newStatus.unitList;
            updateAlyLandEndX(newStatus.alyLandEndX);
            updateEnemyLandStartX(newStatus.enemyLandStartX);
            for (int i = 0; i < unitMap.Length; i++)
            {
                for (int j = 0; j < unitMap[i].Length; j++)
                {
                    if (unitMap[i][j] != null && unitMap[i][j].getFeatures().health > 0)
                    {
                        bool unitExists = false;
                        for (int k = 0; k < newUnits.Count; k++)
                        {
                            if (newUnits[k].assignedId == unitMap[i][j].getAssignedId())
                            {
                                unitExists = true;
                                unitMap[i][j].updateStatus(newUnits[k].health,
                                    new Utils.Vector2(newUnits[k].x, newUnits.y), unitMap, newUnits[k].remainingShots);
                                newUnits.RemoveAt(k);
                                break;
                            }
                        }
                        if (!unitExists)
                        {
                            unitMap[i][j].kill();
                        }
                    }
                }
            }
            for (int i = 0; i < newUnits.Count; i++)
            {
                Utils.Vector2 mapPos = new Utils.Vector2(newUnits[i].x, newUnits[i].y);
                bool isAly = true;
                if (mapPos.x >= numberOfMapXColumn / 2) isAly = false;
                createNewUnit(mapPos, newUnits[i].unitId, isAly, newUnits[i].level, newUnits[i].assignedId);
                unitMap[mapPos.x][mapPos.y].setUnitHealth(newUnits[i].health);
                unitMap[mapPos.x][mapPos.y].setAvailableShots(newUnits[i].remainingShots);
            }

            myPower = newStatus.currentPower;
            enemyPower = newStatus.enemyPower;
            myPowerRegen = newStatus.alyPowerRegen;
            enemyPowerRegen = newStatus.enemyPowerRegen;
            firstTurnIsMine = newStatus.firstTurnIsMine;
            turn = newStatus.turnNumber;
            if ((turn % 2 == 0 && firstTurnIsMine) || (turn % 2 == 1 && !firstTurnIsMine)) myTurn = true;
            else myTurn = false;
            List<Unit> checkedUnits = new List<Unit>();
            for (int i = 0; i < unitMap.Length; i++)
            {
                for (int j = 0; j < unitMap[i].Length; j++)
                {
                    if (unitMap[i][j] != null && !checkedUnits.Contains(unitMap[i][j]))
                    {
                        checkedUnits.Add(unitMap[i][j]);
                        //checkForAddOrRemoveAbility(i, j, true);  ???????
                    }
                }
            }
            //TODO : above question  //TODO update alylandEndx and for enemy;
        }

        /////gettings
        public Unit getUnitMap(int x, int y)
        {
            return null;
        }
    }
}
