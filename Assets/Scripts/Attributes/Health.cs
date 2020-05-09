using UnityEngine;
using GameDevTV.Saving;
using RPG.Stats;
using RPG.Core;
using System;
using GameDevTV.Utils;
using UnityEngine.Events;

namespace RPG.Attributes
{     
    public class Health : MonoBehaviour,ISaveable
    {
        [SerializeField] float  regenerationPercentage = 70;
        [SerializeField] TakeDamageEvent takeDamage;

        [SerializeField] HealDamageEvent healDamage;
        [SerializeField] UnityEvent OnDie, OnTookHit;
        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float>
        {

        }

        public class HealDamageEvent : UnityEvent<float>
        {

        }
       
        
         LazyValue<float> healthPoints;
         bool isDead = false;
         private void Awake()
         {
            healthPoints  = new LazyValue<float>(GetInitialHealth);
         }
         private float GetInitialHealth()
         {
             return GetComponent<BaseStats>().GetStat(Stat.Health);
         }
         private void Start()
         {
           healthPoints.ForceInit();
         } 

         private void OnEnable() 
         {
             GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
         }
         private void OnDisable() 
         {
             GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
         }

        public bool IsDead()
         {
             return isDead;
         }
         public void TakeDamage(GameObject instigator,float damage)
         {
           SetHealthPoints(Mathf.Max(healthPoints.value - damage , 0));
              if(healthPoints.value == 0)
              {
                Die();
                OnDie.Invoke();               
                AwardExperience(instigator);
              }
              else
              {             
                
                takeDamage.Invoke(damage);
              }
         }
         public void HealDamage(float value,GameObject user)
         {
            Health target = user.GetComponent<Health>();
            target.SetHealthPoints(Mathf.Min(target.healthPoints.value + value, target.GetMaxHealth()));
           /* if(healthPoints.value > 0)            
            healDamage.Invoke(value);*/
         }

        private void AwardExperience(GameObject instigator)
        {
              
              Experience experience = instigator.GetComponent<Experience>();
              if(experience == null) return;
              experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));

        }
        public void SetHealthPoints(float value)
        {
           healthPoints.value = value;
        }
         public float GetHealthPoints()
         {
            return healthPoints.value;
         }
         public float GetMaxHealth()
         {
             return GetComponent<BaseStats>().GetStat(Stat.Health);
         }
        public float GetHealthPercentage()
         {
            return 100* (GetHealthFraction());
         }
         public float GetHealthFraction()
         {
              return healthPoints.value/GetComponent<BaseStats>().GetStat(Stat.Health);
         }
         private void Die()
         {
            if(isDead) return;
            isDead = true;        
            GetComponent<Animator>().SetTrigger("DeathTrigger");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            transform.Find("Quad").gameObject.SetActive(false);
         }

         private void RegenerateHealth()
        {
          float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage/100);
          SetHealthPoints(Mathf.Max(healthPoints.value,regenHealthPoints));
        }
        public object CaptureState()
        {
          return  healthPoints.value;
        }
        public void RestoreState(object state)
        {
           if(healthPoints.value == 0)
              {
                Die();
              }
           healthPoints.value = (float)state;
        }
    }
}