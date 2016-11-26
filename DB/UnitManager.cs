using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.SQLite;
using System.IO;
using LandFightBotReborn.Bot.DataType;

namespace LandFightBotReborn.DB
{
    /// <summary>
    /// By M.Hooshdar M.Fakhreddin
    /// Warning : This a file based DB so you have to wait for it to read and write Try using Corotine to manage everything
    /// </summary>
    public class UnitFeatures
    {
        public int id;
        public string name;
        public int powerSpawn;
        public int powerMove;
        public int powerAttack;
        public float powerRegen;//Power we gain for this unit each turn
        public int airDamage;
        public int groundDamage;
        public float armorPricisingDamagePoint;//between 0 to 2
        public float splashDamagePoint;//Between 0 to 1
        public int health;
        public bool isArmored;
        public bool isGroundUnit;
        public bool isLocked;
        public bool isInDeck;
        public float width;
        public float height;
        public int vision;
        public int requiredLevel;
        public int coinCost;
        public int currentLevel;//We calculate next level coin cost by table and requiredLevel
        public int lastModified;
        public int moveSpeed;
        public string description;
        public bool isVehicle;
        public int shotPerTurn;
        public List<UpgradeRule> rules;

        public UnitFeatures(int id, string name, int powerSpawn, int powerMove,
                int powerAttack, float powerRegen, int airDamage,
                int groundDamage, float armorPricisingDamagePoint,
                float splashDamagePoint,int health,
                bool isArmored, bool isGroundUnit, bool isLocked,
                bool isInDeck, float width, float height, int vision,
                int requiredLevel, int coinCost, int currentLevel,
                int lastModified, int moveSpeed,
                string description, bool isVehicle, int shotPerTurn)
        {
            this.id = id;
            this.name = name;
            this.powerSpawn = powerSpawn;
            this.powerMove = powerMove;
            this.powerAttack = powerAttack;
            this.powerRegen = powerRegen;//Power we gain for this unit each turn
            this.airDamage = airDamage;
            this.groundDamage = groundDamage;
            this.armorPricisingDamagePoint = armorPricisingDamagePoint;
            this.splashDamagePoint = splashDamagePoint;
            this.health = health;
            this.isArmored = isArmored;
            this.isGroundUnit = isGroundUnit;
            this.isLocked = isLocked;
            this.isInDeck = isInDeck;
            this.width = width;
            this.height = height;
            this.vision = vision;
            this.requiredLevel = requiredLevel;
            this.coinCost = coinCost;
            this.currentLevel = currentLevel;
            this.lastModified = lastModified;
            this.moveSpeed = moveSpeed;
            this.description = description;
            this.isVehicle = isVehicle;
            this.shotPerTurn = shotPerTurn;
        }

        public UpgradeRule getLevelUpgrade(int level)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].level == level)
                {
                    return rules[i];
                }
            }
            return null;
        }

        public UnitFeatures clone()
        {
            UnitFeatures clonedFeatures = new UnitFeatures(
                this.id, this.name, this.powerSpawn, this.powerMove,
                this.powerAttack, this.powerRegen, this.airDamage,
                this.groundDamage, this.armorPricisingDamagePoint,
                this.splashDamagePoint, this.health, this.isArmored,
                this.isGroundUnit, this.isLocked, this.isInDeck,
                this.width, this.height, this.vision, this.requiredLevel,
                this.coinCost, this.currentLevel,this.lastModified, this.moveSpeed,
                this.description, this.isVehicle, this.shotPerTurn);
            clonedFeatures.rules = rules;
            return clonedFeatures;
        }
    }
    /// <summary>
    /// By M.Hooshdar M.Fakhreddin
    /// </summary>
    public class UnitManager
    {
        private const string VARCHAR = " VARCHAR ";
        private const string INTEGER = " INT ";
        private const string FLOAT = " FLOAT ";
        private const string BOOLEAN = " BOOLEAN ";
        private const string SEPARATOR = " , ";
        private const string COTATION = "'";
        private const string NOT_NULL = " NOT NULL ";
        private const string PRIMARY_KEY = " PRIMARY KEY ";
        private const string EQUAL = " = ";
        private const string WHERE = " WHERE ";

        private const string UNIT_TABLE_NAME = "unitTable";
        private const string UNIT_ID_COL = "id";
        private const string UNIT_NAME_COL = "name";
        private const string UNIT_POWER_SPAWN_COL = "powerSpawn";
        private const string UNIT_POWER_MOVE_COL = "powerMove";
        private const string UNIT_POWER_ATTACK_COL = "powerAttack";
        private const string UNIT_POWER_REGEN_COL = "powerRegen";
        private const string UNIT_AIR_DAMAGE_COL = "airDamage";
        private const string UNIT_GROUND_DAMAGE_COL = "groundDamage";
        private const string UNIT_ARMOR_PRICISING_DAMAGE_POINT = "armorPricisingDamagePoint";
        private const string UNIT_SPLASH_DAMAGE_POINT_COL = "splashDamagePoint";
        private const string UNIT_HEALTH_COL = "health";
        private const string UNIT_IS_ARMORED_COL = "isArmored";
        private const string UNIT_IS_GROUND_COL = "isGround";
        private const string UNIT_IS_LOCKED_COL = "isLocked";
        private const string UNIT_IS_IN_DECK_COL = "isInDeck";
        private const string UNIT_WIDTH_COL = "width";
        private const string UNIT_HEIGHT_COL = "height";
        private const string UNIT_VISION_COL = "vision";
        private const string UNIT_REQUIRED_LEVEL_COL = "requiredLevel";
        private const string UNIT_COIN_COST = "coinCost";
        private const string UNIT_CURRENT_LEVEL_COL = "currentLevel";
        private const string UNIT_LAST_MODIFIED_COL = "lastModified";
        private const string UNIT_MOVE_SPEED_COL = "moveSpeed";
        private const string UNIT_DESCRIPTION_COL = "description";
        private const string UNIT_IS_VEHICLE_COL = "isVehicle";
        private const string UNIT_SHOT_PER_TURN = "shotPerTurn";
        //private const string UNIT_NUMBER_OF_STATE_IMAGES = "numberOfStateImages";
        //private const string UNIT_NUMBER_OF_DIRECTIONS = "numberOfDirections";

        private static List<UnitFeatures> allUnitFeatuers;
        private SQLManager sqlManager;
        private UpgradeRuleManager ruleManager;

        public UnitManager(SQLManager sqlManager,UpgradeRuleManager ruleManager)
        {
            this.sqlManager = sqlManager;
            this.ruleManager = ruleManager;
            createTable();
        }

        public void createTable()
        {
            string query = "CREATE TABLE IF NOT EXISTS " + UNIT_TABLE_NAME + " ( " +
                           UNIT_ID_COL + INTEGER + NOT_NULL + PRIMARY_KEY + SEPARATOR +
                           UNIT_NAME_COL + VARCHAR + NOT_NULL + SEPARATOR +
                           UNIT_POWER_SPAWN_COL + INTEGER + SEPARATOR +
                           UNIT_POWER_MOVE_COL + INTEGER + SEPARATOR +
                           UNIT_POWER_ATTACK_COL + INTEGER + SEPARATOR +
                           UNIT_POWER_REGEN_COL + FLOAT + SEPARATOR +
                           UNIT_AIR_DAMAGE_COL + INTEGER + SEPARATOR +
                           UNIT_GROUND_DAMAGE_COL + INTEGER + SEPARATOR +
                           UNIT_ARMOR_PRICISING_DAMAGE_POINT + FLOAT + SEPARATOR +
                           UNIT_SPLASH_DAMAGE_POINT_COL + INTEGER + SEPARATOR +
                           UNIT_HEALTH_COL + INTEGER + SEPARATOR +
                           UNIT_IS_ARMORED_COL + BOOLEAN + SEPARATOR +
                           UNIT_IS_GROUND_COL + BOOLEAN + SEPARATOR +
                           UNIT_IS_LOCKED_COL + BOOLEAN + SEPARATOR +
                           UNIT_IS_IN_DECK_COL + BOOLEAN + SEPARATOR +
                           UNIT_WIDTH_COL + FLOAT + SEPARATOR +
                           UNIT_HEIGHT_COL + FLOAT + SEPARATOR +
                           UNIT_VISION_COL + INTEGER + SEPARATOR +
                           UNIT_REQUIRED_LEVEL_COL + INTEGER + SEPARATOR +
                           UNIT_COIN_COST + INTEGER + SEPARATOR +
                           UNIT_CURRENT_LEVEL_COL + INTEGER + SEPARATOR +
                           UNIT_LAST_MODIFIED_COL + INTEGER + SEPARATOR +
                           UNIT_MOVE_SPEED_COL + INTEGER + SEPARATOR +
                           UNIT_DESCRIPTION_COL + VARCHAR + SEPARATOR +
                           UNIT_IS_VEHICLE_COL + BOOLEAN + SEPARATOR +
                           UNIT_SHOT_PER_TURN + INTEGER +
                           //SEPARATOR +
                           //UNIT_NUMBER_OF_STATE_IMAGES + INTEGER + SEPARATOR +
                           //UNIT_NUMBER_OF_DIRECTIONS + INTEGER +
                           " ) ";
            sqlManager.execute(query);
        }

        public UnitFeatures select(int inputId) {
            for(int i = 0; i < allUnitFeatuers.Count; i++) {
                if (inputId == allUnitFeatuers[i].id)
                    return allUnitFeatuers[i];
            }
            return null;
        }

        public void selectAll()
        {
            if (allUnitFeatuers == null || allUnitFeatuers.Count == 0)
            {
                ruleManager.selectAll();
                string query = "SELECT * FROM " + UNIT_TABLE_NAME + "";
                IDataReader mReader = sqlManager.executeReader(query);
                List<UnitFeatures> unitList = new List<UnitFeatures>();
                if (mReader.Read())
                {
                    do
                    {
                        int id = mReader.GetInt32(0);
                        string name = mReader.GetString(1);
                        int powerSpawn = mReader.GetInt32(2);
                        int powerMove = mReader.GetInt32(3);
                        int powerAttack = mReader.GetInt32(4);
                        float powerRegen = mReader.GetFloat(5);
                        int airDamage = mReader.GetInt32(6);
                        int groundDamage = mReader.GetInt32(7);
                        float armorPricsingDamagePoint = mReader.GetFloat(8);
                        float splashDamagePoint = mReader.GetFloat(9);
                        int health = mReader.GetInt32(10);
                        bool isArmored = mReader.GetBoolean(11);
                        bool isGrounded = mReader.GetBoolean(12);
                        int width = mReader.GetInt32(15);
                        int height = mReader.GetInt32(16);
                        int vision = mReader.GetInt32(17);
                        int requiredLevel = mReader.GetInt32(18);
                        int coinCost = mReader.GetInt32(19);
                        int lastModified = mReader.GetInt32(21);
                        int moveSpeed = mReader.GetInt32(22);
                        string description = mReader.GetString(23);
                        bool isVehicle = mReader.GetBoolean(24);
                        int shotPerTurn = mReader.GetInt32(25);
                        bool isInDeck;
                        bool isLocked;
                        int currentLevel;
                        isInDeck = false;//mReader.GetBoolean(14);
                        isLocked = false;//mReader.GetBoolean(13);
                        currentLevel = 1;//mReader.GetInt32(20);
                        UnitFeatures unit = new UnitFeatures(id, name, powerSpawn, powerMove, powerAttack, powerRegen,
                            airDamage, groundDamage, armorPricsingDamagePoint, splashDamagePoint, health, isArmored,
                            isGrounded, isLocked, isInDeck, width, height, vision, requiredLevel, coinCost, currentLevel,
                            lastModified, moveSpeed, description, isVehicle, shotPerTurn);//,numberOfStateImages,numberOfDirections);
                        List<UpgradeRule> upgradeRule = ruleManager.selectUnitUpgradeRule(id);
                        unit.rules = upgradeRule;
                        unitList.Add(unit);
                    } while (mReader.NextResult());
                }
                allUnitFeatuers = unitList;
            }
        }

        public static List<UnitFeatures> getAllUnitFeatures() {
            return allUnitFeatuers;
        }

    }

}
