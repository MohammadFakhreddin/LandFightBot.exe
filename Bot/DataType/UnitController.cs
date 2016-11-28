using System;
using System.Collections.Generic;
using LandFightBotReborn.DB;
using LandFightBotReborn.Utils;

namespace LandFightBotReborn.Bot.DataType
{
    public class UnitController
    {
        private readonly UnitFeatures features;
        private Vector2 gameMapPosition;
        private readonly bool isAly;
        private readonly int maxHealth;
        private int assignedId;
        private bool unitIsAlive;
        private int shotsInTurn;
        private readonly BotManager parent;
        private readonly List<Ability> abilityGain;

        public UnitController(BotManager parent, UnitFeatures features, Vector2 gameMapPosition, bool isAly, int level,
            int assignedId)
        {
            abilityGain = new List<Ability>();
            this.parent = parent;
            this.features = features;
            this.gameMapPosition = gameMapPosition;
            this.isAly = isAly;
            maxHealth = features.health;
            var myRule = features.getLevelUpgrade(level);
            if (myRule != null)
            {
                features.groundDamage += myRule.groundAttackPlus;
                features.airDamage += myRule.airAttackPlus;
                features.health += myRule.healthPlus;
                maxHealth = features.health;
                features.coinCost += myRule.price;
            }
            this.assignedId = assignedId;
            unitIsAlive = true;
        }

        public void hit(int damage)
        {
            if (damage != 0)
            {
                features.health -= damage;
                if (features.health <= 0)
                {
                    features.health = 0;
                    kill();
                }
            }
        }

        public void heal(int number)
        {
            features.health += number;
            if (features.health >= maxHealth)
            {
                if (features.health - maxHealth < number)
                {
                    var temp = maxHealth + number - features.health;
                }
                features.health = maxHealth;
            }
        }

        private bool abilIsValid(Ability ability)
        {
            if (((ability.unit == null) || (Math.Abs(ability.unit.gameMapPosition.x - gameMapPosition.x) > 1) ||
                 (Math.Abs(ability.unit.gameMapPosition.y - gameMapPosition.y) > 1)) &&
                Constants.unitIds.SPECIAL.needAliveContains(ability.unitId))
                return false;
            return true;
        }

        public void notifyEndOfTurn()
        {
            var damage = 0;
            for (var i = abilityGain.Count - 1; i >= 0; i--)
                if (abilIsValid(abilityGain[i]))
                    if (abilityGain[i].unitId == Constants.unitIds.ATISHI)
                    {
                        var damageValue = abilityGain[i].unit.getFeatures().groundDamage;
                        if (!features.isGroundUnit)
                            damageValue = abilityGain[i].unit.getFeatures().airDamage;
                        if (abilityGain[i].tag == null)
                            abilityGain[i].tag = 3;
                        damage += damageValue * (3 - (int) abilityGain[i].tag);
                        abilityGain[i].tag = (int) abilityGain[i].tag - 1;
                        if ((int) abilityGain[i].tag == 0)
                            abilityGain.RemoveAt(i);
                    }
                    else if (abilityGain[i].unitId == Constants.unitIds.DARMANIOS)
                    {
                        var healVal = abilityGain[i].unit.getFeatures().groundDamage;
                        if (!features.isGroundUnit)
                            healVal = abilityGain[i].unit.getFeatures().airDamage;
                        damage -= healVal;
                    }
            if (damage > 0)
                if (features.health - damage > 0)
                    hit(damage);
                else
                    kill();
            else if (damage < 0)
                heal(-1 * damage);

            checkForAbilityChange();
        }

        /// <summary>
        ///   It is called when current unit is hitted
        /// </summary>
        /// <param name="enemyFeatures">
        ///   The enemy features to calculate damage taken
        /// </param>
        /// <param name="isSplash">
        ///   Shows we are hitte by splash or not
        /// </param>
        public void hit(UnitFeatures enemyFeatures, bool isSplash)
        {
            var damagePoint = 1f;
            if (features.isArmored) damagePoint = enemyFeatures.armorPricisingDamagePoint;
            if (isSplash) damagePoint = damagePoint * enemyFeatures.splashDamagePoint;
            float damage = enemyFeatures.airDamage;
            if (features.isGroundUnit) damage = enemyFeatures.groundDamage;
            damage = (int) (damage * damagePoint);
            //commander id = 7!
            //if (abilityGain.Contains(7)) {
            //    damage = damage * nextToCommanderMultiply;
            //}
            damage = (int) (damage * checkForDefenceAbility());
            hit((int) damage);
        }

        private void checkForAbilityChange()
        {
            for (var i = abilityGain.Count - 1; i >= 0; i--)
                if (!abilIsValid(abilityGain[i]))
                    abilityGain.RemoveAt(i);
        }

        public void addAbility(Ability ability)
        {
            //if (!abilityGain.Contains(ability))
            //{
            Console.WriteLine("Add ability method");
            if ((abilityGain.Contains(ability) && Constants.unitIds.SPECIAL.needAliveContains(ability.unitId))
                || Constants.unitIds.SPECIAL.noAbilityContains(features.id, ability.unitId))
            {
                Console.WriteLine("Cannot add this abil");
                return;
            }
            abilityGain.Add(ability);
            //   }
        }

        private float checkForDefenceAbility()
        {
            var value = 1f;
            for (var i = 0; i < abilityGain.Count; i++)
                if (abilIsValid(abilityGain[i]) && Constants.unitIds.SPECIAL.defenceUnitsContains(abilityGain[i].unitId))
                    if (abilityGain[i].unitId == Constants.unitIds.SHEILDER)
                        value *= abilityGain[i].unit.getFeatures().splashDamagePoint;
                    else
                        Console.WriteLine("Unknown unit detected");
            return value;
        }

        public void removeAbility(Ability abil)
        {
            for (var i = abilityGain.Count - 1; i >= 0; i--) //For Deleting only one ability
                if (abilityGain[i].unit == abil.unit)
                    abilityGain.RemoveAt(i);
            checkForAbilityChange();
            //if (abilityGain.Contains(ability))
            //{
            //    abilityGain.Remove(ability);
            //}
        }

        public void removeAllPlaceBasedAbilities()
        {
            //abilityGain.Clear();
            for (var i = abilityGain.Count - 1; i >= 0; i--)
                if (Constants.unitIds.SPECIAL.needAliveContains(abilityGain[i].unitId))
                    abilityGain.RemoveAt(i);
            checkForAbilityChange();
        }

        /// <summary>
        ///   It gives yu the shots which are avaible each turn
        /// </summary>
        /// <returns></returns>
        public int getAvailableShots()
        {
            return features.shotPerTurn - shotsInTurn;
        }

        /// <summary>
        ///   It must be called each turn
        /// </summary>
        public void resetShots()
        {
            shotsInTurn = 0;
        }

        public void updateStatus(int health, Vector2 pos,
            UnitController[][] unitMap, int remainingShots)
        {
            features.health = health;
            setAvailableShots(remainingShots);
            move(pos, unitMap);
        }

        public void kill()
        {
            features.health = 0;
            parent.notifyUnitIsDead(features, isAly, gameMapPosition, false);
            unitIsAlive = false;
        }

        public int getAssignedId()
        {
            return assignedId;
        }

        public void setAssignedId(int assignedId)
        {
            this.assignedId = assignedId;
        }

        public Vector2 getGameMapPosition()
        {
            return gameMapPosition;
        }

        public UnitFeatures getFeatures()
        {
            return features;
        }

        public void setAvailableShots(int remainingShots)
        {
            shotsInTurn = features.shotPerTurn - remainingShots;
        }

        public void setUnitHealth(int health)
        {
            features.health = health;
        }

        public bool getIsAly()
        {
            return isAly;
        }

        public void move(Vector2 gameMapPosition, UnitController[][] unitMap)
        {
            //TODO Check other side
            unitMap[this.gameMapPosition.x][this.gameMapPosition.y] = null;
            unitMap[gameMapPosition.x][gameMapPosition.y] = this;
            var numberOfXColumn = parent.getNumberOfXColumn();
            if (features.width > 1)
            {
                Console.WriteLine("width is more than 1");
                var x = this.gameMapPosition.x;
                if (isAly)
                    x -= 1;
                else
                    x += 1;
                if (((x >= 0) && isAly) || ((x < numberOfXColumn) && !isAly))
                    unitMap[x][this.gameMapPosition.y] = null;
                x = gameMapPosition.x;
                if (isAly)
                    x -= 1;
                else
                    x += 1;
                if (((x >= 0) && isAly) || ((x < numberOfXColumn) && !isAly))
                    unitMap[x][gameMapPosition.y] = this;
            }
            this.gameMapPosition = gameMapPosition;
        }

        public bool attack(Vector2 attackPosition)
        {
            if (shotsInTurn < features.shotPerTurn)
            {
                shotsInTurn++;
                return true;
            }
            return false;
        }

        public int callculateDistancePassingPower(Vector2 newPosition)
        {
            int distance = (Math.Abs((int)(gameMapPosition.x - newPosition.x))) +
                           (Math.Abs((int)(gameMapPosition.y - newPosition.y)));
            int neededMovePower = distance * features.powerMove;
            return neededMovePower;
        }
    }
}
