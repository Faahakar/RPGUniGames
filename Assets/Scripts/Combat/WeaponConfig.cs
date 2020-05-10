using UnityEngine;
using RPG.Attributes;
using GameDevTV.Inventories;
using RPG.Stats;
using System.Collections.Generic;

namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/MakeNewWeapon", order = 0)]
    public class WeaponConfig : EquipableItem, IModifierProvider
    {
        [SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] Weapon equippedPrefab = null;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] float weapondamage = 5f;
        [SerializeField] float percentageBonus = 0;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] Projectile projectile = null;
        [SerializeField] float staminaGain = 0f;
        [SerializeField] float blockRating = 0f;
        bool isCollidingWith = false;

        [SerializeField] string weaponName = "Weapon";

        public float GetStaminaGain()
        {
            return staminaGain;
        }

        public Weapon Spawn(Transform rightHandTransform,Transform leftHandTransform, Animator animator)
        {
            Weapon weapon = null;
            DestroyOldWeapon(rightHandTransform,leftHandTransform);
            if(equippedPrefab !=null)
            {
                Transform handTransform = GetTransform(rightHandTransform,leftHandTransform);
                weapon = Instantiate(equippedPrefab, handTransform);
                weapon.gameObject.name = weaponName;
                return weapon;
            }
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if(animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
            else if(overrideController != null)
            {                   
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;           
            }
            return weapon;
            
           
        }
        private void DestroyOldWeapon(Transform rightHandTransform, Transform leftHandTransform)
        {
            Transform oldWeapon = rightHandTransform.Find(weaponName);
            if(oldWeapon == null)
            {
                oldWeapon = leftHandTransform.Find(weaponName);
            }
            if(oldWeapon == null) return;
            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);

        }
        private Transform GetTransform(Transform rightHandTransform,Transform leftHandTransform)
        {
             Transform handTransform;
                if(isRightHanded) handTransform = rightHandTransform;
                else handTransform = leftHandTransform;
                return handTransform;
        }
        /*private void OnTriggerEnter(Collider other) {
            if(other.GetComponent<Health>().IsDead()) return; 
            isCollidingWith = true;
        }
        public bool isCollidingWithEnemy()
        {
            return isCollidingWith;
        }*/
        public bool HasProjectile()
        {
            return projectile != null;
        }
        public void LaunchProjectile(Transform rightHandTransform,Transform leftHandTransform, Health target, GameObject instigator, float calculatedDamage)
        {
             Projectile projectileInstance = Instantiate(projectile,GetTransform(rightHandTransform,leftHandTransform).position,Quaternion.identity);
             projectileInstance.SetTarget(target,instigator,calculatedDamage);
        }
        public float GetWeaponRange()
        {
            return weaponRange;
        }
        public float GetPercentageBonus()
        {
            return percentageBonus;
        }
        public float GetWeaponDamage()
        {
            return weapondamage;
        }
        public float GetBlockRating()
        {
            return blockRating;
        }
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return weapondamage;
            }
           /* if(stat == Stat.Block)
            {
                yield return blockRating;
            }*/
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return percentageBonus;
            }
        }
    }
}