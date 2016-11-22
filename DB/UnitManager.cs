using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.DB
{
    public class UnitManager
    {
        public static UnitManager instance;
        public UnitManager()
        {
            if (instance != null) return;
            instance = this;
        }
    }

    public class UnitFeatures
    {
        public int id;
        public string name;
        public int health;

        public UnitFeatures(int id, string name, int health)
        {
            this.id = id;
            this.name = name;
            this.health = health;
        }

        public UnitFeatures clone()
        {
            UnitFeatures clonedFeatures = new UnitFeatures(
           this.id, this.name, this.health);
            return clonedFeatures;
        }
    }
}
