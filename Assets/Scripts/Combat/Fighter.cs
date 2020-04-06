using UnityEngine;
using RPG.Movement;
using RPG.Saving;
using RPG.Resources;
using RPG.Core;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System.Collections;

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
            currentWeapon.ForceInit();
        }
        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if(target == null) return;
            if(target.IsDead()) return; 
            AttackMoveController();
                                             
        }
        private void AttackMoveController()
        {
            if(isPlayer)
            {
                 PlayerAttackController(); 
            }
            else
            {
                if (!GetisInRange())
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
        public bool CanAttack(GameObject combatTarget)
        {
             if(combatTarget == null) {return false;}
             Health targetToTest = combatTarget.GetComponent<Health>();
             return targetToTest != null && !targetToTest.IsDead();
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
        private void PlayerAttackController()
        {
             //  Trigger the Hit() Event
            transform.LookAt(target.transform);   
            if(Input.GetButtonDown("Fire1") && Vector3.Distance(transform.position, target.transform.position) < 8f)
            {  
               GetComponent<Abilities>().MoveAttack();

            }
            
            if (!GetisInRange())
            {
               GetComponent<Mover>().MoveTo(target.transform.position,1f);                     
            }
            else
            {    
                GetComponent<Mover>().Cancel();    
                GetComponent<Abilities>().AttackChain();            

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
        public bool GetisInRange()
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
