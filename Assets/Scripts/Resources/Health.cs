using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using System;

namespace RPG.Resources
{     
    public class Health : MonoBehaviour,ISaveable
    {
         [SerializeField] float  regenerationPercentage = 70;
         float healthPoints = -1f;
         bool isDead = false;
         private void Start()
         {
           
            GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
           if(healthPoints < 0)
           {
              healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
           }
         
         } 

        public bool IsDead()
         {
             return isDead;
         }
         public void TakeDamage(GameObject instigator,float damage)
         {
              print(gameObject.name + "took damage: " + damage);
              healthPoints =  Mathf.Max(healthPoints - damage , 0);
               if(healthPoints == 0)
              {
                Die();
                AwardExperience(instigator);
              }
         }

        private void AwardExperience(GameObject instigator)
        {
              
              Experience experience = instigator.GetComponent<Experience>();
              if(experience == null) return;
              experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));

        }
         public float GetHealthPoints()
         {
            return healthPoints;
         }
         public float GetMaxHealth()
         {
             return GetComponent<BaseStats>().GetStat(Stat.Health);
         }
        public float GetHealthPercentage()
         {
            return 100* (healthPoints/GetComponent<BaseStats>().GetStat(Stat.Health));
         }
         private void Die()
         {
            if(isDead) return;
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            transform.Find("Quad").gameObject.SetActive(false);
         }
         private void RegenerateHealth()
        {
          float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage/100);
          healthPoints = Mathf.Max(healthPoints,regenHealthPoints);
        }
        public object CaptureState()
        {
          return  healthPoints;
        }
        public void RestoreState(object state)
        {
           if(healthPoints == 0)
              {
                Die();
              }
           healthPoints = (float)state;
        }
    }
}