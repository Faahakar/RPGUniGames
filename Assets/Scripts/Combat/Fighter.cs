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
        
        public UnityEvent specialAttack,moveAttack;

        public event Action blocked;
        Rigidbody rigidBody;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;

        [SerializeField] WeaponConfig defaultShield = null;
        [SerializeField] bool isPlayer  = false;
      
        Health target;
        Equipment equipment;
       
        float timeSinceLastAttack = Mathf.Infinity;
        float damage = 0, block = 0;
        float rotationSpeed = 15f;
        public float gravity = -9.83f;
        public float runSpeed = 8f;
        public float walkSpeed = 3f;
        public float strafeSpeed = 3f;

        
        WeaponConfig currentWeaponConfig, shieldConfig;
        LazyValue<Weapon> currentWeapon, currentShield;
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
        float journeyLength = 0;
        private float currentTeleportTime;
        private bool acceptInput = true;

        private void Awake()
        {
            shieldConfig = defaultShield;
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
            currentShield = new LazyValue<Weapon>(SetupDefaultShield);
            equipment = GetComponent<Equipment>();
            player =  GameObject.FindWithTag("Player");
            playerFighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();    
            if(equipment)
            {
                if(defaultWeapon !=null)
                {
                    equipment.AddItem(EquipLocation.Weapon,defaultWeapon);

                }
                if(defaultShield !=null)
                {
                    equipment.AddItem(EquipLocation.Shield,defaultShield);

                }
            
                equipment.equipmentUpdated += UpdateWeapon;
            }
            
        }

        private void UpdateWeapon()
        {
            var weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;
            var shield = equipment.GetItemInSlot(EquipLocation.Shield) as WeaponConfig;
            if(weapon == null)
            {
                EquipWeapon(defaultWeapon);
            }
            else
            {
                EquipWeapon(weapon);
            }
            if(shield == null)
            {
                EquipShield(defaultShield);
            }
            else
            {
                EquipShield(shield);
            }

        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }
        private Weapon SetupDefaultShield()
        {
            return AttachWeapon(defaultShield);
        }

        private void Start() 
        {       
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            navmeshAgent = GetComponent<NavMeshAgent>();
            currentWeapon.ForceInit();
            if(defaultShield !=null)
            currentShield.ForceInit();
        }
        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }
        public void EquipShield(WeaponConfig shield)
        {
            shieldConfig = shield;
            currentShield.value = AttachWeapon(shield);
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
            velocity = navmeshAgent.velocity;
            velocityX = transform.InverseTransformDirection(velocity).x;
            velocityZ = transform.InverseTransformDirection(velocity).z;
            
            //Update animator with movement values
            if(isPlayer)
            {

                animator.SetFloat("Input X", velocityX / runSpeed);
                animator.SetFloat("Input Z", velocityZ /runSpeed);
         
            }
            else
            {
             animator.SetFloat("ForwardSpeed", velocityZ);

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
            if (acceptInput == true)
            {
                if (Input.GetMouseButton(1))
                {

                    if (!inBlock && attack == 0)
                    {
                        Cancel();
                        animator.SetBool("Block", true);
                        isBlocking = true;
                        animator.SetBool("Running", false);
                        animator.SetBool("Moving", false);
                    }
                }
                if (Input.GetMouseButtonUp(1))
                {
                    inBlock = false;
                    isBlocking = false;
                    animator.SetBool("Block", false);

                }

                if (Input.GetButtonDown("Fire3"))
                {
                    if (player.GetComponent<Stamina>().GetStaminaPoints() == 100f)
                    {
                        SpecialAttack();
                    }

                }
                if (isTargetInvalid())
                {
                    if (velocity.sqrMagnitude > 0f && 0.1f < velocity.sqrMagnitude)
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
                else if (!isTargetInvalid())
                {
                    transform.LookAt(target.transform);


                    if (!isBlocking)
                    {

                        if (GetisInRangeToAttack(target.transform))
                        {

                            if (Input.GetButtonDown("Fire1"))
                            {
                                AttackChain();
                            }
                            animator.SetBool("Moving", false);
                            animator.SetBool("Running", false);
                        }
                        else if (!GetisInRangeToAttack(target.transform))
                        {
                            if (!isStunned)
                            {
                                if (attack == 0)
                                {
                                    if (Input.GetButtonDown("Fire2") && Vector3.Distance(transform.position, target.transform.position) <= 9f - navmeshAgent.stoppingDistance)
                                    {

                                        MoveAttack();

                                    }

                                    GetComponent<Mover>().MoveTo(target.transform.position, 1f);
                                    animator.SetBool("Moving", true);
                                    animator.SetBool("Running", true);

                                }
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

            moveAttack.Invoke();
                StopAllCoroutines();
                attack = 5;    
                animator.SetTrigger("MoveAttack1Trigger");  
               // StartCoroutine(_ResetTrigger(1.2f));  
                StartCoroutine(_Blockinput(.4f));
                StartCoroutine(_LockMovementAndAttack(0.9f));
                StartCoroutine(_Teleport(1.2f));     
	    }
        public void AttackChain()
        {                 
           // print(attack);
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
        public bool GetAcceptInput()
        {
            return acceptInput;
        }
        IEnumerator _Blockinput(float inputBlockTime)
        {
            GetComponent<Mover>().Cancel();
            acceptInput = false;
            yield return new WaitForSeconds(inputBlockTime);
            acceptInput = true;  

        }
        IEnumerator _ResetTrigger(float resetTriggerTime)
        {
            yield return new WaitForSeconds(resetTriggerTime);
            animator.ResetTrigger("MoveAttack1Trigger");  
            animator.ResetTrigger("SpecialAttack1Trigger");

        }
        IEnumerator _Teleport(float timeToTeleport)
        {
            
            float elapsedTime = 0;
            Vector3 originPosition = player.transform.position;
            Health teleportTarget = target;

            while(elapsedTime < timeToTeleport*2)
            {
                player.transform.position = Vector3.Lerp(originPosition,teleportTarget.transform.position 
                - new Vector3(navmeshAgent.stoppingDistance,0,0),timeToTeleport*2/elapsedTime);
                elapsedTime+= Time.deltaTime;

                yield return null;

            }

            yield return new WaitForSeconds(0.1f);  
         
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
            GetComponent<Mover>().Cancel();
            isStunned = true;
            //animator.applyRootMotion = true;
            animator.SetFloat("Input X", 0);
            animator.SetFloat("Input Z", 0);
            animator.SetBool("Moving", false);
            animator.SetBool("Running", false);
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

        public void SpecialAttack()
        {
          StopAllCoroutines();
          specialAttack.Invoke();
          animator.SetTrigger("SpecialAttack1Trigger");
          attack = 6;
          GetComponent<Mover>().Cancel();
         // StartCoroutine(_ResetTrigger(1.3f));  
          StartCoroutine(_Blockinput(1.3f));
          StartCoroutine(_LockMovementAndAttack(1.1f));
          RaycastHit[] hits =  Physics.SphereCastAll(player.transform.position,7.5f, Vector3.up);
           foreach(RaycastHit hit in hits)
            {
                AIController enemy = hit.collider.GetComponent<AIController>();    
                if(enemy == null) continue;
                enemy.GetComponent<Health>().TakeDamage(player,CalculateDamage()*3);
            }
            player.GetComponent<Stamina>().SetStaminaPoints(0);

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
            StopAllCoroutines();
            StartCoroutine(_LockMovementAndAttack(0.5f));
            target.GetComponent<Animator>().SetTrigger("BlockHitReactTrigger");
            yield return null;
        }

        private void TriggerAttack()
        {
            if(!isPlayer)
            {
                GetComponent<Animator>().ResetTrigger("stopattack");
                GetComponent<Animator>().SetTrigger("attack");


            }

                            
        }

        // Animation Event
        void Hit()
        {
            //1
            float damage = CalculateDamage();
            if (target == null) return;
            if(isBlockingAndInAngle())
            {
                if(target.gameObject.tag == "Player")
                {
                    StartCoroutine(_BlockHitReact());
                }
            }
            Stamina stamina =  this.GetComponent<Stamina>();
            if(stamina != null) 
            stamina.GainStamina(currentWeaponConfig.GetStaminaGain());
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
                target.TakeDamage(gameObject, damage);
            }

        }

        void Shoot()
        {
            Hit();
        }   

        public float CalculateDamage()
        {
            float totalDamage = 0;
           if(isBlockingAndInAngle())
           {
             totalDamage = Mathf.Max(GetComponent<BaseStats>().GetStat(Stat.Damage) - playerFighter.shieldConfig.GetBlockRating(),0);   
              //playerFighter.GetComponent<Stamina>().GainStamina(shieldConfig.GetStaminaGain());
           }
           else
           {
               totalDamage = GetComponent<BaseStats>().GetStat(Stat.Damage);
           }
           return totalDamage;
        }
        public bool isBlockingAndInAngle()
        {
             return playerFighter.isBlocking && IsInPlayerBlockAngle();
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
            if(!isPlayer)
            {
                GetComponent<Animator>().ResetTrigger("attack"); 
                GetComponent<Animator>().SetTrigger("stopattack");   

            }
 
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
