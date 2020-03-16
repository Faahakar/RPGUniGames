﻿using UnityEngine;
using RPG.Resources;
namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        
        [SerializeField] float speedModifier = 5f;
        [SerializeField] bool isHoming = true;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float maxLifeTime = 10f;
        [SerializeField] GameObject[] destroyOnHit = null;
        [SerializeField] float lifeAfterImpact = 2f;
        Health target = null;
         float damage = 0;
        // Start is called before the first frame update
        void Start()
        {     
             transform.LookAt(GetAimLocation());    
        }

        // Update is called once per frame
        void Update()
        {
            if(target == null) return;  
            if(isHoming && !target.IsDead())
            {
                transform.LookAt(GetAimLocation());
            }         
            transform.Translate(Vector3.forward * speedModifier * Time.deltaTime) ;
        }

        public void SetTarget(Health target, float damage )
        {
          this.target = target;
          this.damage = damage;
          Destroy(gameObject,maxLifeTime);
        }
        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
            if(targetCapsule == null)
            {
                return target.transform.position;
            }
            return target.transform.position + Vector3.up * targetCapsule.height/2;
        }
        private void OnTriggerEnter(Collider other) {
            if(other.GetComponent<Health>() != target) return;
            if(target.IsDead()) return;  
            target.TakeDamage(damage);
            speedModifier = 0;
            if(hitEffect !=null)
            {
               GameObject hiteffectInstance = Instantiate(hitEffect,GetAimLocation(),transform.rotation);
            }  
          
            foreach(GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }
            Destroy(gameObject,lifeAfterImpact);
        }

    }

}
