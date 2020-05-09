using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
        bool isCollidingWith = false;
        [SerializeField] UnityEvent onHit;
        public void OnHit()
        {
           onHit.Invoke();
          
        }
        private void OnTriggerEnter(Collider other) {
            if(other.GetComponent<Health>() == null) return;
            if(other.GetComponent<Health>().IsDead()) return;  
           // target.TakeDamage(instigator,damage);

           isCollidingWith = true;

        }

        public bool isCollidingWithEnemy()
        {
            return isCollidingWith;
        }
    }


}
