using UnityEngine;
using RPG.Movement;
using RPG.Saving;
using RPG.Attributes;
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
        Rigidbody rigidBody;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;
        [SerializeField] bool isPlayer  = false;
        [SerializeField] float moveAttackTime = 0.9f;
      
        Health target;
       
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
            timeSinceLastAttack += Time.deltaTime;
            StartCoroutine(AttackMoveController());              
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

                animator.SetFloat("Input X", velocityX / runSpeed);
                animator.SetFloat("Input Z", velocityZ / runSpeed);
         
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
       /* void CameraRelativeInput(){
		if(!isStunned){
			float inputHorizontal = Input.GetAxisRaw("Horizontal");
			float inputVertical = Input.GetAxisRaw("Vertical");
			float inputDashHorizontal = Input.GetAxisRaw("DashHorizontal");
			float inputDashVertical = Input.GetAxisRaw("DashVertical");
			//Camera relative movement
			Transform cameraTransform = Camera.main.transform;
			//Forward vector relative to the camera along the x-z plane   
			Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
			forward.y = 0;
			forward = forward.normalized;
			//Right vector relative to the camera always orthogonal to the forward vector
			Vector3 right = new Vector3(forward.z, 0, -forward.x);
			inputVec = inputHorizontal * right + inputVertical * forward;
			dashInputVec = inputDashHorizontal * right + inputDashVertical * forward;
			if(!isBlocking){
				//if there is some input (account for controller deadzone)
				if(inputVertical > 0.1 || inputVertical < -0.1 || inputHorizontal > 0.1 || inputHorizontal < -0.1){
					//set that character is moving
					animator.SetBool("Moving", true);
					animator.SetBool("Running", true);
				}
				else{
					//character is not moving
					animator.SetBool("Moving", false);
					animator.SetBool("Running", false);
				}
			}
		}
	}
*/
        IEnumerator AttackMoveController()
        {
           if(isPlayer)
            {
                StartCoroutine(AbilitiesController());     
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
        IEnumerator AbilitiesController()
        {                         
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
               
                    if (Input.GetMouseButton(1))
                    {
                        if (!inBlock && attack == 0)
                        {
                            animator.SetBool("Block", true);
                            isBlocking = true;
                            animator.SetBool("Running", false);
                            animator.SetBool("Moving", false);
                        }
                    }  
                    if(!Input.GetMouseButton(1))
                    {
                        inBlock = false;
                        isBlocking = false;
                        animator.SetBool("Block", false);
                    }                                
                                   
                    if(!isBlocking)
                    {        
    
                        if (GetisInRangeToAttack())
                        {     
                              
                            print(attack);                        
                            if(Input.GetButtonDown("Fire1"))
                            {  
                              // print(attack);
                                AttackChain();                
                            }
                            // GetComponent<Mover>().Cancel();  
                             yield return new WaitForSeconds(0.15f);
                            //animator.SetBool("Moving", false);
                            //animator.SetBool("Running", false); 
                        }                    
                        else if(!GetisInRangeToAttack())
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
                                if(!isMovingAttacking)
                                {
                                    animator.SetBool("Moving", true);
                                    animator.SetBool("Running", true);
                                    GetComponent<Mover>().MoveTo(target.transform.position,1f);  
                                }
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
                isMovingAttacking = true;                
                animator.SetTrigger("MoveAttack1Trigger");   
                StartCoroutine(_LockMovementAndAttack(moveAttackTime));
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
        public bool GetIsAttacking()
        {
            return isAttacking;
        }                        
        public IEnumerator _LockMovementAndAttack(float pauseTime)
        {
            isStunned = true;
            animator.applyRootMotion = true;
            inputVec = new Vector3(0, 0, 0);
            newVelocity = new Vector3(0, 0, 0);
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
                //if(currentWeapon.value.isCollidingWithEnemy())
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
             string weaponName = currentWeaponConfig.name = (string)state;
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
