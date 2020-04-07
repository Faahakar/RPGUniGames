using UnityEngine;
using RPG.Movement;
using RPG.Saving;
using RPG.Resources;
using RPG.Core;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System.Collections;
using UnityEngine.AI;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour,IAction, ISaveable, IModifierProvider
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;
        [SerializeField] bool isPlayer  = false;
      
        Health target;
       
        float timeSinceLastAttack = Mathf.Infinity;
        float damage = 0;

        
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;
        int attack = 0;
        bool isAttacking;
        bool canChain, isStunned;
        bool isBlocking = false;
        bool inBlock;
        Animator animator;
        NavMeshAgent navmeshAgent;
        Vector3 velocity;
        private bool isMovingAttacking = false;
        float velocityX, velocityZ;
        private void Awake() 
        {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        private void Start() 
        {       
            animator = GetComponent<Animator>();
            navmeshAgent = GetComponent<NavMeshAgent>();
            currentWeapon.ForceInit();
        }
        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            velocity = navmeshAgent.velocity;
            velocityX = transform.InverseTransformDirection(velocity).x;
            velocityZ = transform.InverseTransformDirection(velocity).z;
            //Update animator with movement values
            if(isPlayer)
            {

                animator.SetFloat("Input X", velocityX);
                animator.SetFloat("Input Z", velocityZ);
         
            }
            else
            {
             animator.SetFloat("ForwardSpeed", velocityZ);

            }
            StartCoroutine(AttackMoveController());              
        }
        private void LateUpdate() {
        }       
        void FixedUpdate()
        {

            UpdateMovement();           
            
        }

        private void UpdateMovement()
        {
            if (isBlocking || isStunned)
            {
                velocity = new Vector3(0, 0, 0);
            }
            else
            {
                /*character is not strafing
				if(!isStrafing){
					newVelocity = motion.normalized * runSpeed;
				}*/
                //character is strafing
                /*else{
					newVelocity = motion.normalized * strafeSpeed;
				}
				if(ledge){
					newVelocity = motion.normalized * ledgeSpeed;
				}
				if(isStealth){
					newVelocity = motion.normalized * stealthSpeed;
				}*/
            }
        }

        IEnumerator AttackMoveController()
        {
           if(isPlayer)
            {
                AbilitiesController();     
            }
            else
            {
                if(!isTargetInvalid())
                {
                    if (!GetisInRangeToAttack())
                    {
                        GetComponent<Mover>().MoveTo(target.transform.position,1f);                     
                    }
                    else
                    {
                        AttackBehaviour();    
                        GetComponent<Mover>().Cancel();
                    }    

                }
    
            }
            yield return null;

                 
        }
        private void AbilitiesController()
        {      
            if((!isStunned || !isBlocking))   
            {
                if(isTargetInvalid())
               {
                    if(velocity.sqrMagnitude > 0)
                    {
                    animator.SetBool("Moving", true);
                    animator.SetBool("Running", true);
                    }
                    else
                    {
                        animator.SetBool("Moving", false);
                        animator.SetBool("Running", false);
                    }
                    
                }
                
                
                else
                {
                    transform.LookAt(target.transform);     
                    //print(attack);
                        if (GetisInRangeToAttack())
                        {  
    
                            GetComponent<Mover>().Cancel();  
                            animator.SetBool("Moving", false);
                            animator.SetBool("Running", false);  
                        if(attack == 0)
                        {
                            if(Input.GetButtonDown("Fire2"))
                            {      
                            // GetComponent<Mover>().MoveTo(target.transform.position,1f); 
                                MoveAttack();
                            }     
                        }    
                        
                            if(Input.GetButtonDown("Fire1") && attack <=3)
                            {  
                                print(attack);
                                AttackChain();                 
                            }
                        }
                        else
                        {
                            animator.SetBool("Moving", true);
                            animator.SetBool("Running", true);    
                            GetComponent<Mover>().MoveTo(target.transform.position,1f); 
                        // print("moving to target");
                        }  
                }        
            }
    
           /* if (Input.GetMouseButtonDown(1))
            {
                if (attack == 0)
                {
                    animator.SetBool("Block", true);
                    isBlocking = true;
                    animator.SetBool("Running", false);
                    animator.SetBool("Moving", false);

                }
            }   */
          else
          {
              velocity = new Vector3(0,0,0);
          }    
        }
        public bool ReachedDestination()
        {
            if (!navmeshAgent.pathPending)
            {
                if (navmeshAgent.remainingDistance <= navmeshAgent.stoppingDistance)
                {
                    if (!navmeshAgent.hasPath || navmeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        return true;             
                    }

                }

            }
            //if(GetComponent<Fighter>().GetTarget() == null)
            return false;
        }
        private bool isTargetInvalid()
        {
            if(target!=null)
            {
                if(!target.IsDead())
                {
                  return false;

                }
            }     
           return true;
        }
        public void MoveAttack()
        {
            if(attack == 0)
            {
                StopAllCoroutines();
                attack = 5;
                isMovingAttacking = true;
                animator.SetTrigger("MoveAttack1Trigger");   
                StartCoroutine(_LockMovementAndAttack(0.9f));
                isMovingAttacking = false;
            }
           
   
	    }
        public void AttackChain()
        {                 
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
                    }
                
            }
            else{

            }
            
	    }
        IEnumerator _Attack1()
        {
            StopAllCoroutines();
            canChain = false;
            animator.SetInteger("Attack", 1);
            attack = 1;
            isAttacking = true;  
            StartCoroutine(_ChainWindow(0.1f, .8f));
            StartCoroutine(_LockMovementAndAttack(0.6f));
            yield return null;
	    }
 
        IEnumerator _Attack2()
        {
            StopAllCoroutines();
            canChain = false;
            animator.SetInteger("Attack", 2); 
            attack = 2;
            StartCoroutine(_ChainWindow(0.4f, 0.9f));
            StartCoroutine(_LockMovementAndAttack(0.5f));
            yield return null;
        }
        IEnumerator _Attack3()
        {
            StopAllCoroutines();
            animator.SetInteger("Attack", 3);     
            attack = 3;
            StartCoroutine(_LockMovementAndAttack(0.8f));
            canChain = false;
            yield return null;
        }
        public IEnumerator _ChainWindow(float timeToWindow, float chainLength)
        {
            yield return new WaitForSeconds(timeToWindow);
            canChain = true;
            animator.SetInteger("Attack", 0);
            yield return new WaitForSeconds(chainLength);
            canChain = false;
        }
        public bool FinishedAttack()
        {
            return isStunned;
        }
        public bool GetIsAttacking()
        {
            return isAttacking;
        }                        
        public IEnumerator _LockMovementAndAttack(float pauseTime)
        {
            isStunned = true;
            animator.applyRootMotion = true;
            velocity = new Vector3(0,0,0);
            animator.SetFloat("Input X", 0);
            animator.SetFloat("Input Z", 0);
            animator.SetBool("Moving", false);
            yield return new WaitForSeconds(pauseTime);
            animator.SetInteger("Attack", 0);
            canChain = false;
            isStunned = false;
            animator.applyRootMotion = false;
            //small pause to let blending finish
            yield return new WaitForSeconds(0.2f);
            isAttacking = false;  
            attack = 0;
        }


   
        public bool CanAttack(GameObject combatTarget)
        {
             if(combatTarget == null) {return false;}
             Health targetToTest = combatTarget.GetComponent<Health>();
             return targetToTest != null && !targetToTest.IsDead();
        }
        public bool GetIsPlayer()
        {
            return isPlayer;
        }
        public  void AttackBehaviour()
        {
                transform.LookAt(target.transform);            
                    if(timeSinceLastAttack > timeBetweenAttacks)
                    {
                        //  Trigger the Hit() Event
                        TriggerAttack();
                        timeSinceLastAttack = 0;
                    }                         
   
        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopattack");
            GetComponent<Animator>().SetTrigger("attack");
                            
        }
      

        // Animation Event
        void Hit()
        {         
            if(target == null) return;
            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            if(currentWeapon.value !=null)
            {
                currentWeapon.value.OnHit();
            }
            if(currentWeaponConfig.HasProjectile())
            {
              currentWeaponConfig.LaunchProjectile(rightHandTransform,leftHandTransform,target,gameObject,damage);
            }
            else
            {
                if(currentWeapon.value.isCollidingWithEnemy())
                target.TakeDamage(gameObject, damage);
            }
            
        }
        void Shoot()
        {
            Hit();
        }
        
	public void FootR(){
	}
	
	public void FootL(){
	}
	
	public void Land(){
	}
	
	public void WeaponSwitch(){
	}
        public bool GetisInRangeToAttack()
        {
            return Vector3.Distance(transform.position, target.transform.position) < currentWeaponConfig.GetWeaponRange();
        }
       /* public bool GetIsInAbilityRange()
        {
             return Vector3.Distance(transform.position, target.transform.position) < currentWeaponConfig.GetAbilityRange();
        }*/

        public void Attack(GameObject combatTarget)
       {
            GetComponent<ActionScheduler>().StartAction(this);
            target =  combatTarget.GetComponent<Health>();
          
       }
        public void Cancel()
        {
            StopAttack();
            target = null;
            GetComponent<Mover>().Cancel();
        }

        public void StopAttack()
        {        
            GetComponent<Animator>().ResetTrigger("attack"); 
            GetComponent<Animator>().SetTrigger("stopattack");   
        }
        public Health GetTarget()
        {
            return target;
        }
        public object CaptureState()
        {
             return currentWeaponConfig.name;
        }
        public void RestoreState(object state)
        {
             string  weaponName = currentWeaponConfig.name = (string)state;
             WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
             EquipWeapon(weapon);
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetWeaponDamage();
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBonus();
            }
        }
    }
    

}
