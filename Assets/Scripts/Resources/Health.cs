using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using System;

namespace RPG.Resources
{     
    public class Health : MonoBehaviour,ISaveable
    {
         float healthPoints = -1f;
         float MaxHealthPoints;
         bool isDead = false;
         private void Start()
         {
           if(healthPoints<0)
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