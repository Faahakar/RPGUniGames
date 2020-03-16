using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Resources
{     
    public class Health : MonoBehaviour,ISaveable
    {
         [SerializeField] float healthPoints = 100f;
         bool isDead = false;
         private void Start()
         {
           healthPoints = GetComponent<BaseStats>().GetHealth();
         }
         public bool IsDead()
         {
             return isDead;
         }
         public void TakeDamage(float damage)
         {
              healthPoints =  Mathf.Max(healthPoints - damage , 0);
               if(healthPoints == 0)
              {
                Die();
              }
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