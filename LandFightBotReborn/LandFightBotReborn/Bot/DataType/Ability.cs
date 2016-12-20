namespace LandFightBotReborn.Bot.DataType
{
    public class Ability
    {
        public int unitId;
        public UnitController unit;
        public object tag;
        public Ability(UnitController unit)
        {
            unitId = unit.getFeatures().id;
            this.unit = unit;
            if (unitId == Constants.unitIds.ATISHI)
            {
                tag = 3;
            }
        }
    }
}