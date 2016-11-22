using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.Bot.DataType
{
    public class HittedUnits
    {
        public int assignedId;
        public int damage;
        public HittedUnits(int assignedId, int damage)
        {
            this.assignedId = assignedId;
            this.damage = damage;
        }
    }

    public class ReGameStatus
    {
        public int alyLandEndX { get; set; }
        public int enemyLandStartX { get; set; }
        public int currentPower { get; set; }
        public int enemyPower { get; set; }
        public int turnNumber { get; set; }
        public int alyPowerRegen { get; set; }
        public int enemyPowerRegen { get; set; }
        public int time { get; set; }
        public bool firstTurnIsMine { get; set; }
        public List<UnitState> unitList { get; set; }
    }

    public class UnitState
    {
        public int unitId { get; set; }
        public int level { get; set; }
        public int health { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int remainingShots { get; set; }
        public int assignedId { get; set; }
    }
}
