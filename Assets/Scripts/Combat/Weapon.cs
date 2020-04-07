using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Resources;
using UnityEngine;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
        bool isCollidingWith = false;
        public void OnHit()
        {
           // print("Weapon Hit" + gameObject.name);
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
