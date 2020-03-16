using UnityEngine;
using RPG.Movement;
using RPG.Saving;
using RPG.Resources;
using RPG.Core;
namespace RPG.Combat
{
    public class Fighter : MonoBehaviour,IAction, ISaveable
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] Weapon defaultWeapon = null;
      
        Health target;
        float timeSinceLastAttack = Mathf.Infinity;
        Weapon currentWeapon = null;
        void Start()
        {
            if(currentWeapon == null)
            {
                EquipWeapon(defaultWeapon);
            }
        }
        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon = weapon;
            Animator animator = GetComponent<Animator>();
            weapon.Spawn(rightHandTransform,leftHandTransform,animator);
        }
        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            
            if(target == null) return;
            if(target.IsDead()) return;
            if (!GetisInRange())
            {
                GetComponent<Mover>().MoveTo(target.transform.position,1f);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                AttackBehaviour();                    
               
            }

        }
        public bool CanAttack(GameObject combatTarget)
        {
             if(combatTarget == null) {return false;}
             Health targetToTest = combatTarget.GetComponent<Health>();
             return targetToTest != null && !targetToTest.IsDead();
        }
        private  void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if(timeSinceLastAttack > timeBetweenAttacks)
            {
                //  Trigger the Hit() Event.
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
            if(currentWeapon.HasProjectile())
            {
              currentWeapon.LaunchProjectile(rightHandTransform,leftHandTransform,target);
            }
            target.TakeDamage(currentWeapon.GetWeaponDamage());
        }
        void Shoot()
        {
            Hit();
        }
        private bool GetisInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < currentWeapon.GetWeaponRange();
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

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopattack");
        }
        public object CaptureState()
        {
             return currentWeapon.name;
        }
        public void RestoreState(object state)
        {
             string  weaponName = currentWeapon.name = (string)state;
             Weapon weapon = UnityEngine.Resources.Load<Weapon>(weaponName);
             EquipWeapon(weapon);
        }

    }
    

}