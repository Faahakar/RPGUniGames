using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1,99)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;
        int currentLevel = 0;
        private void Start()
        {
            Experience experience =  GetComponent<Experience>();
            currentLevel = CalculateLevel();
        }
        private void Update() 
        {
           int newLevel  = CalculateLevel();
           if(newLevel > currentLevel)
           {
               currentLevel = newLevel;
               print("Levelled Up");
           }
        }
        
        public float GetStat(Stat stat)
        {
            return  progression.GetStat(stat,characterClass,GetLevel());
        }
        public int GetLevel()
        {
            return currentLevel;
        }
        public int CalculateLevel()
        {
            Experience experience =  GetComponent<Experience>();
            if(experience == null) return startingLevel;
            float currentXP = experience.GetExperience();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp,characterClass);
            for(int level = 1; level<penultimateLevel;level++)
            { 
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp,characterClass,level);
                if(XPToLevelUp > currentXP)
                {
                    return level;
                }

            }
            return penultimateLevel + 1;
        }
    }

   
}
