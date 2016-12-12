using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace LandFightBotReborn.DB
{
    /// <summary>
    /// By M.Fakhreddin
    /// Warning : This a file based DB so you have to wait for it to read and write Try using Corotine to manage everything
    /// </summary>
    public class UpgradeRule
    {
        public int unitFeatureId;//Attention :This is a foreign key
        public int level;
        public int groundAttackPlus;
        public int airAttackPlus;
        public int healthPlus;
        public int price;
        public int minUserLevel;
        public UpgradeRule(int unitFeatureId,int level,int groundAttackPlus,int airAttackPlus,int healthPlus, int price, int minUserLevel)
        {
            this.unitFeatureId = unitFeatureId;
            this.level = level;
            this.groundAttackPlus = groundAttackPlus;
            this.airAttackPlus = airAttackPlus;
            this.healthPlus = healthPlus;
            this.price = price;
            this.minUserLevel = minUserLevel;
        }
    }
    /// <summary>
    /// By M.Hooshdar M.Fakhreddin
    /// </summary>
    public class UpgradeRuleManager
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
        private const string AND = " AND ";

        private const string UPGRADE_TABLE_NAME = "gameRuleUpgradeTable";
        private const string UNIT_ID = "unit_id";
        private const string LEVEL = "level";
        private const string GROUND_ATTACK_PLUS = "gtoundAttackPlus";
        private const string AIR_ATTACK_PLUS = "airAttackPlus";
        private const string HEALTH_PLUS = "healthPlus";
        private const string PRICE = "price";
        private const string MIN_USER_LEVEL = "minUserLevel";

        private static List<UpgradeRule> allUpgradeRules;
        private SQLManager sqlManager;

        public UpgradeRuleManager(SQLManager sqlManager)
        {
            this.sqlManager = sqlManager;
            createTable();
            allUpgradeRules = new List<UpgradeRule>();
        }

        private void createTable()
        {
            string query = "CREATE TABLE IF NOT EXISTS " + UPGRADE_TABLE_NAME + " ( " +
                           UNIT_ID + INTEGER + NOT_NULL + SEPARATOR +
                           LEVEL + INTEGER + NOT_NULL + SEPARATOR +
                           GROUND_ATTACK_PLUS + INTEGER + SEPARATOR +
                           AIR_ATTACK_PLUS + INTEGER + SEPARATOR +
                           HEALTH_PLUS + INTEGER + SEPARATOR +
                           PRICE + INTEGER + SEPARATOR +
                           MIN_USER_LEVEL + INTEGER + SEPARATOR +
                           PRIMARY_KEY + "(" + UNIT_ID + SEPARATOR + LEVEL + ")" +
                           " ) ";
//            while (SQLManager.instance == null)
//            {
//                yield return new WaitForEndOfFrame();
//            }
            sqlManager.execute(query);
        }

        public void selectAll() {
            if (allUpgradeRules == null || allUpgradeRules.Count == 0)
            {
                string query = "SELECT * FROM " + UPGRADE_TABLE_NAME + "";
                IDataReader mReader = sqlManager.executeReader(query);
                List<UpgradeRule> ruleList = new List<UpgradeRule>();
                if (mReader.Read())
                {
                    do
                    {
                        int id = mReader.GetInt32(0);
                        int level = mReader.GetInt32(1);
                        int groundAttackPlus = mReader.GetInt32(2);
                        int airAttackPlus = mReader.GetInt32(3);
                        int healthPlus = mReader.GetInt32(4);
                        int price = mReader.GetInt32(5);
                        int minUserLevel = mReader.GetInt32(6);
                        UpgradeRule unit = new UpgradeRule(id, level,
                            groundAttackPlus, airAttackPlus, healthPlus, price, minUserLevel);
                        ruleList.Add(unit);
                    } while (mReader.NextResult());
                }
                allUpgradeRules = ruleList;
            }
        }

        //If you want a special unit
        //If it return empty it means that there is no upgrade rule attached to them (They have no upgrade)
        public List<UpgradeRule> selectUnitUpgradeRule(int unitID) {
            List<UpgradeRule> ruleList = new List<UpgradeRule>();
            for (int i = 0; i < allUpgradeRules.Count; i++) {
                if (unitID == allUpgradeRules[i].unitFeatureId) {
                    ruleList.Add(allUpgradeRules[i]);
                }
            }
            return ruleList;
        }

        //If you want a special unit with special level
        //If it return empty it means that there is no upgrade rule attached to them (They have no upgrade)
        public UpgradeRule selectUnitLevelUpgradeRule(int unitID, int unitLevel) {
            UpgradeRule rule = null;
            for (int i = 0; i < allUpgradeRules.Count; i++) {
                if (unitID == allUpgradeRules[i].unitFeatureId && unitLevel == allUpgradeRules[i].level) {
                    rule = allUpgradeRules[i];
                    break;
                }
            }
            return rule;
        }

        public static List<UpgradeRule> getAllUpgradeRules() {
            return allUpgradeRules;
        }
    }
}