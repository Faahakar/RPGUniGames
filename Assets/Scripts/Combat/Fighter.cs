using UnityEngine;
using RPG.Movement;
using GameDevTV.Saving;
using RPG.Attributes;
using RPG.Core;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System.Collections;
using UnityEngine.AI;
using System;
using GameDevTV.Inventories;
using RPG.Control;
using UnityEngine.Events;
using RPG.Inventories;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour,IAction, ISaveable
    {
        Rigidbody rigidBody;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;
        [SerializeField] bool isPlayer  = false;
        [SerializeField] float moveAttackTime = 0.9f;
      
        Health target;
        Equipment equipment;
       
        float timeSinceLastAttack = Mathf.Infinity;
        float damage = 0;
        float rotationSpeed = 15f;
        public float gravity = -9.83f;
        public float runSpeed = 8f;
        public float walkSpeed = 3f;
        public float strafeSpeed = 3f;

        
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
        private Vector3 newVelocity;
        private Vector3 inputVec;
        private Vector3 dashInputVec;
        GameObject player;
        Fighter playerFighter;
       [SerializeField]  OnTookHit onTookHit;
        public class OnTookHit: UnityEvent<float>
        {

        }

        private void Awake() 
        {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
            equipment = GetComponent<Equipment>();
            player =  GameObject.FindWithTag("Player");
            playerFighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();    
            if(equipment)
            {
                if(defaultWeapon !=null)
                {
                    equipment.AddItem(EquipLocation.Weapon,defaultWeapon);

                }
            
                equipment.equipmentUpdated += UpdateWeapon;
            }
            
        }

        private void UpdateWeapon()
        {
            var weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;
            if(weapon == null)
            {
                EquipWeapon(defaultWeapon);
            }
            else
            {
                EquipWeapon(weapon);
            }

        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        private void Start() 
        {       
            rigidBody = GetComponent<Rigidbody>();
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
            IsInPlayerBlockAngle();
            timeSinceLastAttack += Time.deltaTime;
            StartCoroutine(PlayerEnemyBranching());           
        }

        private void LateUpdate() {
            rigidBody.velocity = navmeshAgent.velocity;
            velocity = rigidBody.velocity;
            velocityX = transform.InverseTransformDirection(velocity).x;
            velocityZ = transform.InverseTransformDirection(velocity).z;
            //rgVelocityX = transform.InverseTransformDirection(rigidBody.velocity).x;
            //rgVelocityZ = transform.InverseTransformDirection(rigidBody.velocity).x;
            
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
        }       
        void FixedUpdate()
        {
            rigidBody.AddForce(0, gravity, 0, ForceMode.Acceleration);
            UpdateMovement();           
            
        }

        private void UpdateMovement()
        {
           	Vector3 motion = inputVec;
			if(!isBlocking && !isStunned){
				//character is not strafing
					newVelocity = motion.normalized * runSpeed;
				
            }
			
			//no input, character not moving
			else{
				newVelocity = new Vector3(0,0,0);
				inputVec = new Vector3(0,0,0);
			}
        }
        IEnumerator PlayerEnemyBranching()
        {
           if(isPlayer)
            {
                StartCoroutine(AbilitiesController());  
                
                   
            }
            else
            {
                
                if(!isTargetInvalid())
                {
                    if (!GetisInRangeToAttack(target.transform))
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
        IEnumerator AbilitiesController()
        {      
          
            if (Input.GetMouseButtonDown(1))
            {
                
                if (!inBlock && attack == 0)
                {
                    
                    animator.SetBool("Block", true);
                    isBlocking = true;        
                    animator.SetBool("Running", false);
                    animator.SetBool("Moving", false);
                }
            }  
            if(Input.GetMouseButtonUp(1))
            {
                inBlock = false;
                isBlocking = false;       
                animator.SetBool("Block", false);
            }                            
               if(!isStunned && isTargetInvalid())
               {
                    if(velocity.sqrMagnitude > 0f && 0.1f < velocity.sqrMagnitude)
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

                else if(!isTargetInvalid())
                {
                    transform.LookAt(target.transform);   
                                      
                                   
                    if(!isBlocking)
                    {        
    
                        if (GetisInRangeToAttack(target.transform))
                        {     
                              
                            //print(attack);                        
                            if(Input.GetButtonDown("Fire1"))
                            {  
                              // print(attack);
                                AttackChain();                
                            }
                            // GetComponent<Mover>().Cancel();  
                            // yield return new WaitForSeconds(0.15f);
                            animator.SetBool("Moving", false);
                            animator.SetBool("Running", false); 
                        }                    
                        else if(!GetisInRangeToAttack(target.transform))
                        {               
                            if(attack == 0)
                            {
                                if(Input.GetButtonDown("Fire2") && Vector3.Distance(transform.position,target.transform.position) <= 9f - navmeshAgent.stoppingDistance)
                                {
                                    MoveAttack();  
                                    animator.SetBool("Moving", false);
                                    animator.SetBool("Running", false);                                
                                    yield return new WaitForSeconds(moveAttackTime);                                
                                    animator.SetBool("Moving", true);
                                    animator.SetBool("Running", true);  

                                }        
                                    GetComponent<Mover>().MoveTo(target.transform.position,1f);  
                            } 
      
        
                        }  
                      }
                    }
                
                
                yield return null;          
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
                animator.SetTrigger("MoveAttack1Trigger");   
                StartCoroutine(_LockMovementAndAttack(moveAttackTime));
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
        public IEnumerator _LockMovementAndAttack(float pauseTime)
        {
            isStunned = true;
            //animator.applyRootMotion = true;
            inputVec = new Vector3(0, 0, 0);
            newVelocity = new Vector3(0, 0, 0);
            animator.SetFloat("Input X", 0);
            animator.SetFloat("Input Z", 0);
            animator.SetBool("Moving", false);
            yield return new WaitForSeconds(pauseTime);
            animator.SetInteger("Attack", 0);
            canChain = false;
            isStunned = false;
            //animator.applyRootMotion = false;
            //small pause to let blending finish
            yield return new WaitForSeconds(0.2f);
            isAttacking = false;  
            attack = 0;
        }


   
        public bool CanAttack(GameObject combatTarget)
        {
             if(combatTarget == null) {return false;}
             if(!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) && !GetisInRangeToAttack(combatTarget.transform))
             {

                 return false;

             } 
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
        public IEnumerator _BlockHitReact(){
            StartCoroutine(_LockMovementAndAttack(0.5f));
            animator.SetTrigger("BlockHitReactTrigger");
            yield return null;
        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopattack");
            GetComponent<Animator>().SetTrigger("attack");
                            
        }

        // Animation Event
        void Hit()
        {
            BlockAngle();
            if (target == null) return;
            if (currentWeapon.value != null)
            {
                currentWeapon.value.OnHit();
            }
            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else
            {
                
                //if(currentWeapon.value.isCollidingWithEnemy())
                target.TakeDamage(gameObject, damage);
            }

        }
        void Shoot()
        {
            Hit();
        }
        public void BlockAngle()
        {           
          
            if (playerFighter.isBlocking && IsInPlayerBlockAngle())
            {
                
                damage = GetComponent<BaseStats>().GetStat(Stat.Damage) * 0.35f;
            }
            else
            {
                damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            }  
            
        }
        private bool IsInPlayerBlockAngle()
        {          
            float viewAngle = 90f;
            RaycastHit[] hits =  Physics.SphereCastAll(player.transform.position,15f, Vector3.up);
            // Loop
            foreach(RaycastHit hit in hits)
            {
                AIController enemy = hit.collider.GetComponent<AIController>();    
                if(enemy == null) continue;
                Vector3 enemyPos = enemy.transform.position;
                if (Vector3.Distance(enemyPos, player.transform.position) <= 15f)
                {
                    Vector3 directionToPlayer = (enemyPos - player.transform.position);
                    float angleBetweenGuardAndPlayer = Vector3.Angle(player.transform.forward, directionToPlayer);
                    if (angleBetweenGuardAndPlayer <= viewAngle)
                    {   
                        return true;            
                    }
                }
            }                    
            return false;
        }

        public void FootR(){
	}
	
	public void FootL(){
	}
	
	public void Land(){
	}
	
	public void WeaponSwitch(){
	}
        public bool GetisInRangeToAttack(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetWeaponRange();
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
             string weaponName = currentWeaponConfig.name = (string)state;
             WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
             EquipWeapon(weapon);
        }
    }
    

}
