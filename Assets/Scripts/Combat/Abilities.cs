using System.Collections;
using System.Collections.Generic;
using RPG.Movement;
using RPG.Resources;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Combat
{
    public class Abilities : MonoBehaviour
    {
        int attack = 0;
        bool canChain, isStunned, shouldDamage;

        Animator animator;
        Health target = null;
        GameObject instigator = null;

        NavMeshAgent navMeshAgent;
        float damage = 0;
        private void Awake()
        {
          navMeshAgent = GetComponent<NavMeshAgent>(); 
        }
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
        }
        public void MoveAttack()
        {
           
            StopAllCoroutines();
            attack = 5;
            animator.SetTrigger("MoveAttack1Trigger");    
            StartCoroutine(_LockMovementAndAttack(1.4f));
 	
	    }
        public void AttackChain()
        {
            if(Input.GetMouseButton(0) && attack <=3)
            {            
                //if charater is not in air, do regular attack
                if(attack == 0)
                {
                    
                    StartCoroutine(_Attack1());
                }
                //if within chain time
                else if(canChain){
                        if(attack == 1){
                        
                            StartCoroutine(_Attack2());
                        }
                        else if(attack == 2){
                            StartCoroutine(_Attack3());
                        }
                        else{
                            //do nothing
                        }
                    
                }
                else{
                    //do nothing
                }
            }
	    }
        IEnumerator _Attack1()
        {
            StopAllCoroutines();
            canChain = false;
            GetComponent<Animator>().SetInteger("Attack", 1);
            attack = 1;
            StartCoroutine(_ChainWindow(0.1f, .8f));
            StartCoroutine(_LockMovementAndAttack(1f));

            yield return null;
	    }
 
        IEnumerator _Attack2()
        {
            StopAllCoroutines();
            canChain = false;
            GetComponent<Animator>().SetInteger("Attack", 2); 
            attack = 2;
            StartCoroutine(_ChainWindow(0.4f, 0.9f));
            StartCoroutine(_LockMovementAndAttack(0.5f));
            yield return null;
        }
        IEnumerator _Attack3()
        {
            StopAllCoroutines();
            GetComponent<Animator>().SetInteger("Attack", 3);     
            attack = 3;
            StartCoroutine(_LockMovementAndAttack(0.8f));
            canChain = false;
            yield return null;
        }
        public IEnumerator _ChainWindow(float timeToWindow, float chainLength)
        {
            yield return new WaitForSeconds(timeToWindow);
            canChain = true;
            GetComponent<Animator>().SetInteger("Attack", 0);
            yield return new WaitForSeconds(chainLength);
            canChain = false;
        }
        public bool FinishedAttack()
        {
            return isStunned;
        }
        
        public IEnumerator _LockMovementAndAttack(float pauseTime)
        {
            isStunned = true;
            GetComponent<Animator>().applyRootMotion = true;
            GetComponent<Mover>().StopCharacter();
            yield return new WaitForSeconds(pauseTime);
            GetComponent<Animator>().SetInteger("Attack", 0);
            canChain = false;
            isStunned = false;
            GetComponent<Animator>().applyRootMotion = false;
            //small pause to let blending finish
            yield return new WaitForSeconds(0.2f);
            attack = 0;
        }




    }


}
